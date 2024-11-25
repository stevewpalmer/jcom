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
                Debug.Assert(cell.ParseNode != null);
                cell.Value = EvaluateNode(cell, cell.ParseNode);
            }
            catch (Exception) {
                cell.Value = new Variant("!ERR");
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
    private Variant EvaluateNode(Cell sourceCell, CellParseNode node) {
        switch (node.Op) {
            case TokenID.NUMBER:
                NumberParseNode numberNode = (NumberParseNode)node;
                return numberNode.Value;

            case TokenID.TEXT:
                TextParseNode textNode = (TextParseNode)node;
                return new Variant(textNode.Value);

            case TokenID.KSUM: return KSum(sourceCell, node);
            case TokenID.KNOW: return KNow(sourceCell);
            case TokenID.KTODAY: return KToday(sourceCell);
            case TokenID.KYEAR: return KYear(sourceCell, node);
            case TokenID.KMONTH: return KMonth(sourceCell, node);

            case TokenID.EXP: {
                BinaryOpParseNode binaryNode = (BinaryOpParseNode)node;
                double left = EvaluateNode(sourceCell, binaryNode.Left).DoubleValue;
                double right = EvaluateNode(sourceCell, binaryNode.Right).DoubleValue;
                return new Variant(Math.Pow(left, right));
            }

            case TokenID.PLUS: {
                BinaryOpParseNode binaryNode = (BinaryOpParseNode)node;
                double left = EvaluateNode(sourceCell, binaryNode.Left).DoubleValue;
                double right = EvaluateNode(sourceCell, binaryNode.Right).DoubleValue;
                return new Variant(left + right);
            }

            case TokenID.MINUS: {
                BinaryOpParseNode binaryNode = (BinaryOpParseNode)node;
                double left = EvaluateNode(sourceCell, binaryNode.Left).DoubleValue;
                double right = EvaluateNode(sourceCell, binaryNode.Right).DoubleValue;
                return new Variant(left - right);
            }

            case TokenID.MULTIPLY: {
                BinaryOpParseNode binaryNode = (BinaryOpParseNode)node;
                double left = EvaluateNode(sourceCell, binaryNode.Left).DoubleValue;
                double right = EvaluateNode(sourceCell, binaryNode.Right).DoubleValue;
                return new Variant(left * right);
            }

            case TokenID.DIVIDE: {
                BinaryOpParseNode binaryNode = (BinaryOpParseNode)node;
                double left = EvaluateNode(sourceCell, binaryNode.Left).DoubleValue;
                double right = EvaluateNode(sourceCell, binaryNode.Right).DoubleValue;
                return new Variant(left / right);
            }

            case TokenID.KEQ: {
                BinaryOpParseNode binaryNode = (BinaryOpParseNode)node;
                double left = EvaluateNode(sourceCell, binaryNode.Left).DoubleValue;
                double right = EvaluateNode(sourceCell, binaryNode.Right).DoubleValue;
                return new Variant(Math.Abs(left - right) < 0.01);
            }

            case TokenID.KNE: {
                BinaryOpParseNode binaryNode = (BinaryOpParseNode)node;
                double left = EvaluateNode(sourceCell, binaryNode.Left).DoubleValue;
                double right = EvaluateNode(sourceCell, binaryNode.Right).DoubleValue;
                return new Variant(Math.Abs(left - right) > 0.01);
            }

            case TokenID.KGT: {
                BinaryOpParseNode binaryNode = (BinaryOpParseNode)node;
                double left = EvaluateNode(sourceCell, binaryNode.Left).DoubleValue;
                double right = EvaluateNode(sourceCell, binaryNode.Right).DoubleValue;
                return new Variant(left > right);
            }

            case TokenID.KGE: {
                BinaryOpParseNode binaryNode = (BinaryOpParseNode)node;
                double left = EvaluateNode(sourceCell, binaryNode.Left).DoubleValue;
                double right = EvaluateNode(sourceCell, binaryNode.Right).DoubleValue;
                return new Variant(left >= right);
            }

            case TokenID.KLT: {
                BinaryOpParseNode binaryNode = (BinaryOpParseNode)node;
                double left = EvaluateNode(sourceCell, binaryNode.Left).DoubleValue;
                double right = EvaluateNode(sourceCell, binaryNode.Right).DoubleValue;
                return new Variant(left < right);
            }

            case TokenID.KLE: {
                BinaryOpParseNode binaryNode = (BinaryOpParseNode)node;
                double left = EvaluateNode(sourceCell, binaryNode.Left).DoubleValue;
                double right = EvaluateNode(sourceCell, binaryNode.Right).DoubleValue;
                return new Variant(left <= right);
            }

            case TokenID.ADDRESS: {
                LocationParseNode addressNode = (LocationParseNode)node;
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
                Variant result = cell.ParseNode != null ? EvaluateNode(cell, cell.ParseNode) : cell.Value;
                referenceList.Pop();
                return result;
            }

            default:
                Debug.Assert(false, "Unhandled Token");
                return null;
        }
    }

    /// <summary>
    /// Calculate the result of the SUM function
    /// </summary>
    /// <param name="sourceCell">Source cell</param>
    /// <param name="node">SUM function parse node</param>
    /// <returns>The result of the function as a Variant</returns>
    private Variant KSum(Cell sourceCell, CellParseNode node) {
        Debug.Assert(node is FunctionParseNode);
        Variant sumTotal = new Variant(0);
        FunctionParseNode functionNode = (FunctionParseNode)node;
        foreach (CellParseNode parameter in functionNode.Parameters) {
            Debug.Assert(parameter is RangeParseNode);
            RangeParseNode rangeNode = (RangeParseNode)parameter;
            foreach (CellLocation location in rangeNode.RangeIterator(sourceCell.Location)) {
                Variant result;
                Cell cell = sheet.GetCell(location, false);
                if (cell.IsEmptyCell) {
                    result = new Variant(0);
                }
                else {
                    referenceList.Push(cell.Location);
                    result = cell.ParseNode != null ? EvaluateNode(cell, cell.ParseNode) : cell.Value;
                    referenceList.Pop();
                }
                sumTotal += result;
            }
        }
        return sumTotal;
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
    private Variant KYear(Cell cell, CellParseNode node) {
        Debug.Assert(node is FunctionParseNode);
        FunctionParseNode functionNode = (FunctionParseNode)node;
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
    private Variant KMonth(Cell cell, CellParseNode node) {
        Debug.Assert(node is FunctionParseNode);
        FunctionParseNode functionNode = (FunctionParseNode)node;
        Variant result = EvaluateNode(cell, functionNode.Parameters[0]);
        try {
            DateTime date = DateTime.FromOADate(result.DoubleValue);
            return new Variant(date.Month);
        }
        catch {
            return new Variant(0);
        }
    }
}