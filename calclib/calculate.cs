// JCalcLib
// Calculate cell formulae
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
using JComLib;

namespace JCalcLib;

public class Calculate(Sheet sheet) {
    private Stack<CellLocation> referenceList = new();

    /// <summary>
    /// List of affected cells
    /// </summary>
    public List<Cell> CellsToUpdate { get; } = [];

    /// <summary>
    /// Recalculate all formulas on the sheet and update the values
    /// on the formula cells.
    /// </summary>
    public void Update() {
        List<Cell> formulaCells = [];
        foreach (CellList cellList in sheet.ColumnList) {
            formulaCells.AddRange(cellList.FormulaCells);
        }
        foreach (Cell cell in formulaCells) {
            CellsToUpdate.Add(cell);
            try {
                referenceList = new Stack<CellLocation>();
                referenceList.Push(cell.Location);
                Debug.Assert(cell.FormulaTree != null);
                cell.ComputedValue = EvaluateNode(cell, cell.FormulaTree);
            }
            catch (Exception) {
                cell.Error = true;
            }
        }
    }

    /// <summary>
    /// Evaluate one node of the parse tree and return the literal result of
    /// the evaluation.
    /// </summary>
    /// <param name="sourceCell">Source cell</param>
    /// <param name="node">Node to evaluate</param>
    /// <returns>Value of node</returns>
    private Variant EvaluateNode(Cell sourceCell, CellNode node) {
        switch (node.Op) {
            case TokenID.NUMBER:
                NumberNode numberNode = (NumberNode)node;
                return numberNode.Value;

            case TokenID.TEXT:
                TextNode textNode = (TextNode)node;
                return new Variant(textNode.Value);

            case TokenID.KSUM: return KSum(sourceCell, node);
            case TokenID.KNOW: return KNow(sourceCell);
            case TokenID.KTODAY: return KToday(sourceCell);
            case TokenID.KYEAR: return KYear(sourceCell, node);
            case TokenID.KMONTH: return KMonth(sourceCell, node);
            case TokenID.KCONCATENATE: return KConcatenate(sourceCell, node);

            case TokenID.EXP: {
                BinaryOpNode binaryNode = (BinaryOpNode)node;
                double left = EvaluateNode(sourceCell, binaryNode.Left).DoubleValue;
                double right = EvaluateNode(sourceCell, binaryNode.Right).DoubleValue;
                return new Variant(Math.Pow(left, right));
            }

            case TokenID.PLUS: {
                BinaryOpNode binaryNode = (BinaryOpNode)node;
                double left = EvaluateNode(sourceCell, binaryNode.Left).DoubleValue;
                double right = EvaluateNode(sourceCell, binaryNode.Right).DoubleValue;
                return new Variant(left + right);
            }

            case TokenID.MINUS: {
                BinaryOpNode binaryNode = (BinaryOpNode)node;
                double left = EvaluateNode(sourceCell, binaryNode.Left).DoubleValue;
                double right = EvaluateNode(sourceCell, binaryNode.Right).DoubleValue;
                return new Variant(left - right);
            }

            case TokenID.MULTIPLY: {
                BinaryOpNode binaryNode = (BinaryOpNode)node;
                double left = EvaluateNode(sourceCell, binaryNode.Left).DoubleValue;
                double right = EvaluateNode(sourceCell, binaryNode.Right).DoubleValue;
                return new Variant(left * right);
            }

            case TokenID.DIVIDE: {
                BinaryOpNode binaryNode = (BinaryOpNode)node;
                double left = EvaluateNode(sourceCell, binaryNode.Left).DoubleValue;
                double right = EvaluateNode(sourceCell, binaryNode.Right).DoubleValue;
                return new Variant(left / right);
            }

            case TokenID.KEQ: {
                BinaryOpNode binaryNode = (BinaryOpNode)node;
                double left = EvaluateNode(sourceCell, binaryNode.Left).DoubleValue;
                double right = EvaluateNode(sourceCell, binaryNode.Right).DoubleValue;
                return new Variant(Math.Abs(left - right) < 0.01);
            }

            case TokenID.KNE: {
                BinaryOpNode binaryNode = (BinaryOpNode)node;
                double left = EvaluateNode(sourceCell, binaryNode.Left).DoubleValue;
                double right = EvaluateNode(sourceCell, binaryNode.Right).DoubleValue;
                return new Variant(Math.Abs(left - right) > 0.01);
            }

            case TokenID.KGT: {
                BinaryOpNode binaryNode = (BinaryOpNode)node;
                double left = EvaluateNode(sourceCell, binaryNode.Left).DoubleValue;
                double right = EvaluateNode(sourceCell, binaryNode.Right).DoubleValue;
                return new Variant(left > right);
            }

            case TokenID.KGE: {
                BinaryOpNode binaryNode = (BinaryOpNode)node;
                double left = EvaluateNode(sourceCell, binaryNode.Left).DoubleValue;
                double right = EvaluateNode(sourceCell, binaryNode.Right).DoubleValue;
                return new Variant(left >= right);
            }

            case TokenID.KLT: {
                BinaryOpNode binaryNode = (BinaryOpNode)node;
                double left = EvaluateNode(sourceCell, binaryNode.Left).DoubleValue;
                double right = EvaluateNode(sourceCell, binaryNode.Right).DoubleValue;
                return new Variant(left < right);
            }

            case TokenID.KLE: {
                BinaryOpNode binaryNode = (BinaryOpNode)node;
                double left = EvaluateNode(sourceCell, binaryNode.Left).DoubleValue;
                double right = EvaluateNode(sourceCell, binaryNode.Right).DoubleValue;
                return new Variant(left <= right);
            }

            case TokenID.CONCAT: {
                BinaryOpNode binaryNode = (BinaryOpNode)node;
                string left = EvaluateNode(sourceCell, binaryNode.Left).StringValue;
                string right = EvaluateNode(sourceCell, binaryNode.Right).StringValue;
                return new Variant(left + right);
            }

            case TokenID.ADDRESS: {
                LocationNode addressNode = (LocationNode)node;
                if (referenceList.Last() == addressNode.AbsoluteLocation) {
                    throw new Exception("Circular reference");
                }
                if (addressNode.Error) {
                    throw new Exception("Error in address");
                }
                CellLocation absoluteLocation = addressNode.ToAbsolute(sourceCell.Location);
                Cell cell = sheet.GetCell(absoluteLocation, false);
                if (cell.IsEmptyCell) {
                    return new Variant(0);
                }
                referenceList.Push(cell.Location);
                Variant result = cell.FormulaTree != null ? EvaluateNode(cell, cell.FormulaTree) : cell.Value;
                referenceList.Pop();
                return result;
            }

            default:
                Debug.Assert(false, "Unhandled Token");
                return null;
        }
    }

    /// <summary>
    /// Iterator that returns the Variant value of all cells specified by the function
    /// parameter list.
    /// </summary>
    /// <param name="sourceCell">Source cell</param>
    /// <param name="node">A FunctionParseNode</param>
    /// <returns>The next Variant from the referenced cells</returns>
    private IEnumerable<Variant> Arguments(Cell sourceCell, FunctionNode node) {
        foreach (CellNode parameter in node.Parameters) {
            if (parameter is RangeNode rangeNode) {
                foreach (CellLocation location in rangeNode.RangeIterator(sourceCell.Location)) {
                    Variant result = EvaluateLocation(location);
                    if (result.HasValue) {
                        yield return result;
                    }
                }
            }
            else {
                Variant result = EvaluateNode(sourceCell, parameter);
                if (result.HasValue) {
                    yield return result;
                }
            }
        }
    }

    /// <summary>
    /// Evaluate the cell at the specified location.
    /// </summary>
    /// <param name="location">CellLocation of cell to evaluate</param>
    /// <returns>The variant value of the cell</returns>
    private Variant EvaluateLocation(CellLocation location) {
        Variant result;
        Cell cell = sheet.GetCell(location, false);
        if (cell.IsEmptyCell) {
            result = new Variant();
        }
        else if (cell.HasFormula) {
            Debug.Assert(cell.FormulaTree != null);
            if (referenceList.Last() == location) {
                throw new Exception("Circular reference");
            }
            referenceList.Push(cell.Location);
            result = EvaluateNode(cell, cell.FormulaTree);
            referenceList.Pop();
        }
        else {
            result = cell.Value;
        }
        return result;
    }

    /// <summary>
    /// Calculate the result of the SUM function
    /// </summary>
    /// <param name="sourceCell">Source cell</param>
    /// <param name="node">SUM function parse node</param>
    /// <returns>The result of the function as a Variant</returns>
    private Variant KSum(Cell sourceCell, CellNode node) {
        Debug.Assert(node is FunctionNode);
        Variant sumTotal = new Variant(0);
        FunctionNode functionNode = (FunctionNode)node;
        return Arguments(sourceCell, functionNode).Aggregate(sumTotal, (current, value) => current + value);
    }

    /// <summary>
    /// Insert the current date and time. If the cell has no existing
    /// format, we apply a default date-time format.
    /// </summary>
    /// <param name="cell">Source cell</param>
    /// <returns>Value to be applied to the cell</returns>
    private static Variant KNow(Cell cell) {
        if (cell.Format == null) {
            cell.CellFormat = CellFormat.CUSTOM;
            cell.CustomFormatString = "dd/mm/yyyy h:mm";
        }
        return new Variant(DateTime.Now.ToOADate());
    }

    /// <summary>
    /// Insert the current date. If the cell has no existing
    /// format, we apply a default date-time format.
    /// </summary>
    /// <param name="cell">Source cell</param>
    /// <returns>Value to be applied to the cell</returns>
    private static Variant KToday(Cell cell) {
        if (cell.Format == null) {
            cell.CellFormat = CellFormat.DATE_DMY;
        }
        return new Variant(DateTime.Now.ToOADate());
    }

    /// <summary>
    /// Extract and insert the year part of a date.
    /// </summary>
    /// <param name="cell">Source cell</param>
    /// <param name="node">Function node</param>
    /// <returns>Value to be applied to the cell</returns>
    private Variant KYear(Cell cell, CellNode node) {
        Debug.Assert(node is FunctionNode);
        FunctionNode functionNode = (FunctionNode)node;
        Variant result = EvaluateNode(cell, functionNode.Parameters[0]);
        try {
            DateTime date = DateTime.FromOADate(result.DoubleValue);
            return new Variant(date.Year);
        }
        catch {
            return new Variant(0);
        }
    }

    /// <summary>
    /// Extract and insert the month part of a date.
    /// </summary>
    /// <param name="cell">Source cell</param>
    /// <param name="node">Function node</param>
    /// <returns>Value to be applied to the cell</returns>
    private Variant KMonth(Cell cell, CellNode node) {
        Debug.Assert(node is FunctionNode);
        FunctionNode functionNode = (FunctionNode)node;
        Variant result = EvaluateNode(cell, functionNode.Parameters[0]);
        try {
            DateTime date = DateTime.FromOADate(result.DoubleValue);
            return new Variant(date.Month);
        }
        catch {
            return new Variant(0);
        }
    }

    /// <summary>
    /// Concatenate the result of all arguments into a single text string.
    /// </summary>
    /// <param name="sourceCell">Source cell</param>
    /// <param name="node">Function node</param>
    /// <returns>A variant containing the result of the concatenation</returns>
    private Variant KConcatenate(Cell sourceCell, CellNode node) {
        FunctionNode functionNode = (FunctionNode)node;
        return new Variant(string.Concat(Arguments(sourceCell, functionNode)));
    }
}