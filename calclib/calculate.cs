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

namespace JCalcLib;

/// <summary>
/// A CalculationContext carries the context information needed by
/// the evaluation methods on each node.
/// </summary>
public class CalculationContext {

    /// <summary>
    /// The sheet on which this calculation is running.
    /// </summary>
    public required Sheet Sheet { get; init; }

    /// <summary>
    /// The cell containing the formula that triggered the
    /// calculation.
    /// </summary>
    public required Cell SourceCell { get; init; }

    /// <summary>
    /// A reference list of visited formula nodes. This is used to
    /// catch potential circular computations.
    /// </summary>
    public Stack<CellLocation> ReferenceList = new();
}

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
            CellsToUpdate.Add(cell);
            try {
                CalculationContext context = new CalculationContext {
                    ReferenceList = new Stack<CellLocation>(),
                    SourceCell = cell,
                    Sheet = sheet
                };
                context.ReferenceList.Push(cell.Location);
                Debug.Assert(cell.FormulaTree != null);
                cell.ComputedValue = cell.FormulaTree.Evaluate(context);
            }
            catch (Exception) {
                cell.Error = true;
            }
        }
    }
}