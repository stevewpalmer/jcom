// JCalcLib
// Formula tree nodes
//
// Authors:
//  Steve Palmer
//
// Copyright (C) 2024 Steve Palmer
//
// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
//
// # http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.

using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.CompilerServices;
using JComLib;

[assembly: InternalsVisibleTo("tests")]

namespace JCalcLib;

/// <summary>
/// Basic cell node
/// </summary>
/// <param name="tokenID">Node token ID</param>
internal class CellNode(TokenID tokenID) {

    /// <summary>
    /// List of built-in functions
    /// </summary>
    public static readonly Dictionary<TokenID, string> Functions = new() {
        { TokenID.KSUM, "SUM" },
        { TokenID.KNOW, "NOW" },
        { TokenID.KTODAY, "TODAY" },
        { TokenID.KYEAR, "YEAR" },
        { TokenID.KMONTH, "MONTH" },
        { TokenID.KTIME, "TIME" },
        { TokenID.KCONCATENATE, "CONCATENATE" }
    };

    /// <summary>
    /// Operator
    /// </summary>
    public TokenID Op { get; } = tokenID;

    /// <summary>
    /// Convert a token ID to its string representation.
    /// </summary>
    /// <param name="tokenId">Token ID</param>
    /// <returns>String</returns>
    public static string TokenToString(TokenID tokenId) =>
        tokenId switch {
            TokenID.PLUS => "+",
            TokenID.EXP => "^",
            TokenID.MINUS => "-",
            TokenID.MULTIPLY => "*",
            TokenID.DIVIDE => "/",
            TokenID.KLE => "<=",
            TokenID.KLT => "<",
            TokenID.KGE => ">=",
            TokenID.KGT => ">",
            TokenID.KEQ => "=",
            TokenID.KNE => "<>",
            TokenID.COLON => ":",
            TokenID.CONCAT => "&",
            TokenID.COMMA => ",",
            _ => Functions.GetValueOrDefault(tokenId, "?")
        };

    /// <summary>
    /// Convert this node to its raw string.
    /// </summary>
    /// <returns>String</returns>
    public virtual string ToRawString() => TokenToString(Op);

    /// <summary>
    /// Convert this node to its string.
    /// </summary>
    /// <returns>String</returns>
    public override string ToString() => TokenToString(Op);

    /// <summary>
    /// Fix up any address references on the node.
    /// </summary>
    /// <param name="location"></param>
    /// <param name="column">Column to fix</param>
    /// <param name="row">Row to fix</param>
    /// <param name="offset">Offset to be applied to the column and/or row</param>
    public virtual bool FixupAddress(CellLocation location, int column, int row, int offset) {
        return false;
    }

    /// <summary>
    /// Invoke the function through reflection.
    /// </summary>
    /// <param name="context">Source cell containing the original formula</param>
    /// <returns>The variant result from the function</returns>
    public virtual Variant Evaluate(CalculationContext context) {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Node for a function call.
/// </summary>
/// <param name="tokenID">Node token ID</param>
internal class FunctionNode(TokenID tokenID, CellNode[] parameters) : CellNode(tokenID) {

    /// <summary>
    /// Function parameter list
    /// </summary>
    public CellNode[] Parameters { get; } = parameters;

    /// <summary>
    /// Convert this node to its raw string.
    /// </summary>
    /// <returns>String</returns>
    public override string ToRawString() => $"{TokenToString(Op)}({string.Join(",", Parameters.Select(p => p.ToRawString()))})";

    /// <summary>
    /// Convert this node to its string.
    /// </summary>
    /// <returns>String</returns>
    public override string ToString() => $"{TokenToString(Op)}({string.Join(",", Parameters.Select(p => p.ToString()))})";

    /// <summary>
    /// Fix up any address references on the node.
    /// </summary>
    /// <param name="location"></param>
    /// <param name="column">Column to fix</param>
    /// <param name="row">Row to fix</param>
    /// <param name="offset">Offset to be applied to the column and/or row</param>
    public override bool FixupAddress(CellLocation location, int column, int row, int offset) {
        bool fixup = false;
        foreach (CellNode parameter in Parameters) {
            if (parameter.FixupAddress(location, column, row, offset)) {
                fixup = true;
            }
        }
        return fixup;
    }

    /// <summary>
    /// Evaluate this node by invoking the function through reflection.
    /// </summary>
    /// <param name="context">The calculation context</param>
    /// <returns>The variant result from the function</returns>
    public override Variant Evaluate(CalculationContext context) {
        string name = TokenToString(Op);
        Debug.Assert(name != null);
        MethodInfo? method = typeof(Functions).GetMethod(name, BindingFlags.Static | BindingFlags.Public, [typeof(IEnumerable<Variant>)]);
        Debug.Assert(method != null);
        Variant? result = method.Invoke(null, [Arguments(context)]) as Variant;
        if (result == null) {
            throw new ApplicationException("Null result");
        }
        if (Op == TokenID.KNOW) {
            CellLocation sourceLocation = context.ReferenceList.First();
            Cell cell = context.Sheet.GetCell(sourceLocation, false);
            if (cell.Format is null or CellFormat.GENERAL) {
                cell.CellFormat = CellFormat.CUSTOM;
                cell.CustomFormatString = "dd/mm/yyyy h:mm";
            }
        }
        return result;
    }

    /// <summary>
    /// Iterator that returns the Variant value of all cells specified by the function
    /// parameter list.
    /// </summary>
    /// <param name="context">Calculation context</param>
    /// <returns>The next Variant from the referenced cells</returns>
    private IEnumerable<Variant> Arguments(CalculationContext context) {
        CellLocation sourceLocation = context.ReferenceList.First();
        foreach (CellNode parameter in Parameters) {
            if (parameter is RangeNode rangeNode) {
                foreach (CellLocation location in rangeNode.RangeIterator(sourceLocation)) {
                    Variant result = EvaluateLocation(context, location);
                    if (result.HasValue) {
                        yield return result;
                    }
                }
            }
            else {
                Variant result = parameter.Evaluate(context);
                if (result.HasValue) {
                    yield return result;
                }
            }
        }
    }

    /// <summary>
    /// Evaluate the cell at the specified location.
    /// </summary>
    /// <param name="context">Calculation context</param>
    /// <param name="location">Location of cell</param>
    /// <returns>The variant value of the cell</returns>
    internal static Variant EvaluateLocation(CalculationContext context, CellLocation location) {
        Cell cell = context.Sheet.GetCell(location, false);
        if (cell.IsEmptyCell) {
            return new Variant();
        }
        if (cell.HasFormula) {
            Debug.Assert(cell.FormulaTree != null);
            if (context.ReferenceList.Last() == location) {
                throw new Exception("Circular reference");
            }
            // We have already calculated the formula on this cell
            // so just pick up the original result.
            if (context.UpdateList.Contains(cell) && !cell.Error) {
                return cell.Value;
            }
            context.ReferenceList.Push(cell.Location);
            Variant result = cell.FormulaTree.Evaluate(context);
            context.ReferenceList.Pop();
            return result;
        }
        return cell.Value;
    }
}

/// <summary>
/// Represents a node that holds a binary operation.
/// </summary>
/// <param name="tokenID">Node token ID</param>
/// <param name="left">Left part of expression</param>
/// <param name="right">Right part of expression</param>
internal class BinaryOpNode(TokenID tokenID, CellNode left, CellNode right) : CellNode(tokenID) {

    /// <summary>
    /// Left child node
    /// </summary>
    public CellNode Left { get; } = left;

    /// <summary>
    /// Right child node
    /// </summary>
    public CellNode Right { get; } = right;

    /// <summary>
    /// Fix up any address references on the node.
    /// </summary>
    /// <param name="location"></param>
    /// <param name="column">Column to fix</param>
    /// <param name="row">Row to fix</param>
    /// <param name="offset">Offset to be applied to the column and/or row</param>
    public override bool FixupAddress(CellLocation location, int column, int row, int offset) {
        bool left = Left.FixupAddress(location, column, row, offset);
        bool right = Right.FixupAddress(location, column, row, offset);
        return left || right;
    }

    /// <summary>
    /// Convert this node to its raw string.
    /// </summary>
    /// <returns>String</returns>
    public override string ToRawString() {
        string left = Left.ToRawString();
        string right = Right.ToRawString();
        return FormatToString(left, right);
    }

    /// <summary>
    /// Convert this node to its string. For binary operations we
    /// add the appropriate parenthesis if the precedence of either side
    /// of the expression is less than this one.
    /// </summary>
    /// <returns>String</returns>
    public override string ToString() {
        string left = Left.ToString();
        string right = Right.ToString();
        return FormatToString(left, right);
    }

    /// <summary>
    /// Format a binary expression to a string representing the original expression.
    /// Parenthesis are applied to the expression to ensure precedence.
    /// </summary>
    /// <param name="left">Left operand of the operation</param>
    /// <param name="right">Right operand of the operation</param>
    /// <returns>A string representing the binary expression</returns>
    private string FormatToString(string left, string right) {
        if (FormulaParser.Precedence(Op) > FormulaParser.Precedence(Left.Op)) {
            left = $"({left})";
        }
        if (FormulaParser.Precedence(Op) > FormulaParser.Precedence(Right.Op)) {
            right = $"({right})";
        }
        return left + TokenToString(Op) + right;
    }

    /// <summary>
    /// Invoke the function through reflection.
    /// </summary>
    /// <param name="context">Source cell containing the original formula</param>
    /// <returns>The variant result from the function</returns>
    public override Variant Evaluate(CalculationContext context) {
        Variant left = Left.Evaluate(context);
        Variant right = Right.Evaluate(context);
        return Op switch {
            TokenID.PLUS => new Variant(left + right),
            TokenID.MINUS => new Variant(left - right),
            TokenID.MULTIPLY => new Variant(left * right),
            TokenID.DIVIDE => new Variant(left / right),
            TokenID.EXP => new Variant(Math.Pow(left.DoubleValue, right.DoubleValue)),
            TokenID.KEQ => new Variant(left == right),
            TokenID.KNE => new Variant(left != right),
            TokenID.KGE => new Variant(left >= right),
            TokenID.KGT => new Variant(left > right),
            TokenID.KLE => new Variant(left <= right),
            TokenID.KLT => new Variant(left < right),
            TokenID.CONCAT => new Variant(left.StringValue + right.StringValue),
            _ => throw new NotImplementedException("Unknown binary operator")
        };
    }
}

/// <summary>
/// Represents a node that holds a numeric value.
/// </summary>
/// <param name="value">Double value</param>
internal class NumberNode(double value) : CellNode(TokenID.NUMBER) {

    /// <summary>
    /// Value of node
    /// </summary>
    public Variant Value { get; } = new(value);

    /// <summary>
    /// Convert this node to its raw string.
    /// </summary>
    /// <returns>String</returns>
    public override string ToRawString() => ToString();

    /// <summary>
    /// Convert this node to its string.
    /// </summary>
    /// <returns>String</returns>
    public override string ToString() {
        return Value.StringValue;
    }

    /// <summary>
    /// Evaluate this node and return the variant value.
    /// </summary>
    /// <param name="context">Source cell containing the original formula</param>
    /// <returns>The variant value</returns>
    public override Variant Evaluate(CalculationContext context) => Value;
}

/// <summary>
/// Represents a node that holds a string value.
/// </summary>
/// <param name="value">String value</param>
internal class TextNode(string value) : CellNode(TokenID.TEXT) {

    /// <summary>
    /// Value of node
    /// </summary>
    private string Value { get; } = value;

    /// <summary>
    /// Convert this node to its raw string.
    /// </summary>
    /// <returns>String</returns>
    public override string ToRawString() => ToString();

    /// <summary>
    /// Convert this node to its string.
    /// </summary>
    /// <returns>String</returns>
    public override string ToString() {
        return $"\"{Value}\"";
    }

    /// <summary>
    /// Evaluate this node and return the variant value.
    /// </summary>
    /// <param name="context">Source cell containing the original formula</param>
    /// <returns>The variant value</returns>
    public override Variant Evaluate(CalculationContext context) => new(Value);
}

/// <summary>
/// Represents a node that holds a relative cell location.
/// </summary>
/// <param name="absoluteLocation">Absolute location</param>
/// <param name="relativeLocation">Relative location</param>
internal class LocationNode(CellLocation absoluteLocation, Point relativeLocation) : CellNode(TokenID.ADDRESS) {

    /// <summary>
    /// Absolute location
    /// </summary>
    public CellLocation AbsoluteLocation { get; private set; } = absoluteLocation;

    /// <summary>
    /// Absolute location
    /// </summary>
    public Point RelativeLocation { get; private set; } = relativeLocation;

    /// <summary>
    /// True if the cell location now contains an error.
    /// </summary>
    public bool Error { get; private set; }

    /// <summary>
    /// Fix up any address references on the node.
    /// </summary>
    /// <param name="location">Location of this cell</param>
    /// <param name="column">Column to fix</param>
    /// <param name="row">Row to fix</param>
    /// <param name="offset">Offset to be applied to the column and/or row</param>
    public override bool FixupAddress(CellLocation location, int column, int row, int offset) {
        bool needRecalculate = false;
        if (column > 0) {
            if (AbsoluteLocation.Column + offset < 1) {
                Error = true;
            }
            else {
                if (AbsoluteLocation.Column >= column) {
                    AbsoluteLocation = AbsoluteLocation with { Column = AbsoluteLocation.Column + offset };
                }
                RelativeLocation = RelativeLocation with { X = AbsoluteLocation.Column - location.Column };
            }
            needRecalculate = true;
        }
        if (row > 0) {
            if (AbsoluteLocation.Row + offset < 1) {
                Error = true;
            }
            else {
                if (AbsoluteLocation.Row >= row) {
                    AbsoluteLocation = AbsoluteLocation with { Row = AbsoluteLocation.Row + offset };
                }
                RelativeLocation = RelativeLocation with { Y = AbsoluteLocation.Row - location.Row };
            }
            needRecalculate = true;
        }
        return needRecalculate;
    }

    /// <summary>
    /// Convert this node to its raw string. The raw string is the internal
    /// representation used for copying cells in a location independent way.
    /// </summary>
    /// <returns>String</returns>
    public override string ToRawString() {
        return Cell.LocationToAddress(RelativeLocation);
    }

    /// <summary>
    /// Convert this node to its string.
    /// </summary>
    /// <returns>String</returns>
    public override string ToString() {
        return AbsoluteLocation.Address;
    }

    /// <summary>
    /// Convert the relative address to an absolute cell reference
    /// </summary>
    /// <returns>CellLocation</returns>
    public CellLocation ToAbsolute(CellLocation sourceCell) {
        return new CellLocation {
            Column = sourceCell.Column + RelativeLocation.X,
            Row = sourceCell.Row + RelativeLocation.Y
        };
    }

    /// <summary>
    /// Evaluate a cell location reference and return the value of the cell.
    /// The cell may either be a constant value or a formula. If the former
    /// then we return the value. Otherwise, we evaluate the formula and return
    /// the result of the evaluation.
    /// </summary>
    /// <param name="context">The calculation context</param>
    /// <returns>A variant</returns>
    public override Variant Evaluate(CalculationContext context) {
        CellLocation sourceLocation = context.ReferenceList.Peek();
        CellLocation absoluteLocation = ToAbsolute(sourceLocation);
        return FunctionNode.EvaluateLocation(context, absoluteLocation);
    }
}

/// <summary>
/// A cell range of the format Start:End where Start and End are each
/// a location node.
/// </summary>
/// <param name="start">Start of range</param>
/// <param name="end">End of range</param>
internal class RangeNode(LocationNode start, LocationNode end) : CellNode(TokenID.RANGE) {

    /// <summary>
    /// Start of range
    /// </summary>
    public LocationNode RangeStart { get; } = start;

    /// <summary>
    /// End of range
    /// </summary>
    public LocationNode RangeEnd { get; } = end;

    /// <summary>
    /// Fix up any address references on the node.
    /// </summary>
    /// <param name="location"></param>
    /// <param name="column">Column to fix</param>
    /// <param name="row">Row to fix</param>
    /// <param name="offset">Offset to be applied to the column and/or row</param>
    public override bool FixupAddress(CellLocation location, int column, int row, int offset) {
        bool start = RangeStart.FixupAddress(location, column, row, offset);
        bool end = RangeEnd.FixupAddress(location, column, row, offset);
        return start || end;
    }

    /// <summary>
    /// Convert this node to its raw string. The raw string is the internal
    /// representation used for copying cells in a location independent way.
    /// </summary>
    /// <returns>String</returns>
    public override string ToRawString() {
        return $"{RangeStart.ToRawString()}:{RangeEnd.ToRawString()}";
    }

    /// <summary>
    /// Convert this node to its string.
    /// </summary>
    /// <returns>String</returns>
    public override string ToString() {
        return $"{RangeStart}:{RangeEnd}";
    }

    /// <summary>
    /// Return an iterator over the range defined by the start and end.
    /// </summary>
    /// <returns>The next cell location in the iterator, or null</returns>
    public IEnumerable<CellLocation> RangeIterator(CellLocation sourceCell) {
        CellLocation rangeStart = RangeStart.ToAbsolute(sourceCell);
        CellLocation rangeEnd = RangeEnd.ToAbsolute(sourceCell);
        int startColumn = Math.Min(rangeStart.Column, rangeEnd.Column);
        int endColumn = Math.Max(rangeStart.Column, rangeEnd.Column);
        int startRow = Math.Min(rangeStart.Row, rangeEnd.Row);
        int endRow = Math.Max(rangeStart.Row, rangeEnd.Row);

        for (int column = startColumn; column <= endColumn; column++) {
            for (int row = startRow; row <= endRow; row++) {
                yield return new CellLocation { Column = column, Row = row };
            }
        }
    }
}