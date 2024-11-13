// JCalc
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
using JCalcLib;
using JComLib;

namespace JCalc;

public class Calculate(Sheet sheet) {

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
            try {
                List<CellLocation> referenceList = [
                    cell.Location
                ];
                CellParseNode parseNode = cell.CellValue.ParseNode;
                cell.CellValue.Value = EvaluateNode(parseNode, referenceList).StringValue;
                CellsToUpdate.Add(cell);
            }
            catch (Exception) {
                cell.CellValue.Value = "!ERR";
            }
        }
    }

    /// <summary>
    /// Evaluate one node of the parse tree and return the literal result of
    /// the evaluation.
    /// </summary>
    /// <param name="node">Node to evaluate</param>
    /// <param name="referenceList">List of references</param>
    /// <returns>Value of node</returns>
    private Variant EvaluateNode(CellParseNode node, List<CellLocation> referenceList) {
        switch (node.Op) {
            case TokenID.NUMBER:
                NumberParseNode numberNode = (NumberParseNode)node;
                return numberNode.Value;

            case TokenID.TEXT:
                TextParseNode textNode = (TextParseNode)node;
                return new Variant(textNode.Value);

            case TokenID.EXP: {
                BinaryOpParseNode binaryNode = (BinaryOpParseNode)node;
                double left = EvaluateNode(binaryNode.Left, referenceList).DoubleValue;
                double right = EvaluateNode(binaryNode.Right, referenceList).DoubleValue;
                return new Variant(Math.Pow(left, right));
            }

            case TokenID.PLUS: {
                BinaryOpParseNode binaryNode = (BinaryOpParseNode)node;
                double left = EvaluateNode(binaryNode.Left, referenceList).DoubleValue;
                double right = EvaluateNode(binaryNode.Right, referenceList).DoubleValue;
                return new Variant(left + right);
            }

            case TokenID.MINUS: {
                BinaryOpParseNode binaryNode = (BinaryOpParseNode)node;
                double left = EvaluateNode(binaryNode.Left, referenceList).DoubleValue;
                double right = EvaluateNode(binaryNode.Right, referenceList).DoubleValue;
                return new Variant(left - right);
            }

            case TokenID.MULTIPLY: {
                BinaryOpParseNode binaryNode = (BinaryOpParseNode)node;
                double left = EvaluateNode(binaryNode.Left, referenceList).DoubleValue;
                double right = EvaluateNode(binaryNode.Right, referenceList).DoubleValue;
                return new Variant(left * right);
            }

            case TokenID.DIVIDE: {
                BinaryOpParseNode binaryNode = (BinaryOpParseNode)node;
                double left = EvaluateNode(binaryNode.Left, referenceList).DoubleValue;
                double right = EvaluateNode(binaryNode.Right, referenceList).DoubleValue;
                return new Variant(left / right);
            }

            case TokenID.KEQ: {
                BinaryOpParseNode binaryNode = (BinaryOpParseNode)node;
                double left = EvaluateNode(binaryNode.Left, referenceList).DoubleValue;
                double right = EvaluateNode(binaryNode.Right, referenceList).DoubleValue;
                return new Variant(Math.Abs(left - right) < 0.01);
            }

            case TokenID.KNE: {
                BinaryOpParseNode binaryNode = (BinaryOpParseNode)node;
                double left = EvaluateNode(binaryNode.Left, referenceList).DoubleValue;
                double right = EvaluateNode(binaryNode.Right, referenceList).DoubleValue;
                return new Variant(Math.Abs(left - right) > 0.01);
            }

            case TokenID.KGT: {
                BinaryOpParseNode binaryNode = (BinaryOpParseNode)node;
                double left = EvaluateNode(binaryNode.Left, referenceList).DoubleValue;
                double right = EvaluateNode(binaryNode.Right, referenceList).DoubleValue;
                return new Variant(left > right);
            }

            case TokenID.KGE: {
                BinaryOpParseNode binaryNode = (BinaryOpParseNode)node;
                double left = EvaluateNode(binaryNode.Left, referenceList).DoubleValue;
                double right = EvaluateNode(binaryNode.Right, referenceList).DoubleValue;
                return new Variant(left >= right);
            }

            case TokenID.KLT: {
                BinaryOpParseNode binaryNode = (BinaryOpParseNode)node;
                double left = EvaluateNode(binaryNode.Left, referenceList).DoubleValue;
                double right = EvaluateNode(binaryNode.Right, referenceList).DoubleValue;
                return new Variant(left < right);
            }

            case TokenID.KLE: {
                BinaryOpParseNode binaryNode = (BinaryOpParseNode)node;
                double left = EvaluateNode(binaryNode.Left, referenceList).DoubleValue;
                double right = EvaluateNode(binaryNode.Right, referenceList).DoubleValue;
                return new Variant(left <= right);
            }

            case TokenID.ADDRESS: {
                LocationParseNode addressNode = (LocationParseNode)node;
                if (referenceList.Contains(addressNode.Value)) {
                    throw new Exception("Circular reference");
                }
                if (addressNode.Error) {
                    throw new Exception("Error in address");
                }
                Cell cell = sheet.GetCell(addressNode.Value, false);
                if (cell.CellValue.Type == CellType.NONE) {
                    return new Variant(0);
                }
                CellParseNode parseNode = cell.CellValue.ParseNode;
                return EvaluateNode(parseNode, referenceList);
            }

            default:
                Debug.Assert(false, "Unhandled Token");
                return null;
        }
    }
}