// JCalcLib
// Cell fill routines
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
using System.Globalization;
using ExcelNumberFormat;
using JComLib;

namespace JCalcLib;

public abstract class ICellTypeFiller {

    /// <summary>
    /// Create a cell type filler based on the specified cell.
    /// </summary>
    /// <param name="cell">Cell to use as the basis</param>
    /// <returns>A ICellTypeFiller class</returns>
    public static ICellTypeFiller Create(Cell cell) {
        if (cell.Format is CellFormat.DATE_DM or CellFormat.DATE_DMY or CellFormat.DATE_MY) {
            return new CellDateVariantFiller();
        }
        if (cell.Format is CellFormat.CUSTOM && cell.CustomFormat != null && cell.CustomFormat.IsDateTimeFormat) {
            return new CellDateVariantFiller();
        }
        return new CellVariantFiller();
    }

    /// <summary>
    /// Update the cell at the specified index using the computed sequence.
    /// </summary>
    /// <param name="index">1-based index of cell to fill</param>
    /// <param name="cell">Cell to fill</param>
    public abstract void Update(int index, Cell cell);
}

/// <summary>
/// Cell type filler for variant values
/// </summary>
public class CellVariantFiller : ICellTypeFiller {
    private Variant cellValue = new(0);
    private Variant deltaValue = new(0);
    private Cell? firstCell;

    /// <summary>
    /// Update the variant cell at the index. If index is 1, use the
    /// value of the cell to determine the seed. If index is 2 and the
    /// second cell has a value then it determines the delta between
    /// successive cells. If the second cell is empty then the seed value
    /// is replicated into all cells.
    /// </summary>
    /// <param name="index">1-based index of cell to fill</param>
    /// <param name="cell">Cell to fill</param>
    public override void Update(int index, Cell cell) {
        if (index == 1) {
            firstCell = cell;
            cellValue = cell.Value;
            return;
        }
        if (index == 2 && cell is { IsEmptyCell: false, Value.HasValue: true }) {
            deltaValue = cell.Value - cellValue;
            cellValue = cell.Value;
            Debug.Assert(firstCell != null);
            cell.StyleFrom(firstCell);
            return;
        }
        if (cellValue.HasValue) {
            Debug.Assert(firstCell != null);
            cell.StyleFrom(firstCell);
            cell.Content = (cellValue + deltaValue).ToString();
            cellValue += deltaValue;
        }
    }
}

/// <summary>
/// Cell type filler for dates
/// </summary>
public class CellDateVariantFiller : ICellTypeFiller {
    private DateTime cellDateTime;
    private int dayDelta;
    private int monthDelta;
    private Cell? firstCell;

    /// <summary>
    /// Update the date in the cell at the index. If index is 1, use the
    /// date value of the cell to determine the seed. If index is 2 and the
    /// second cell has a value then it determines the delta between
    /// successive cells. This varies depending on the date format in the
    /// initial cell.
    /// DATE_DMY or DATE_DM - the delta is the day difference.
    /// DATE_MY - the delta is the month difference.
    /// If the second cell is empty then the seed value is replicated into
    /// all cells.
    /// </summary>
    /// <param name="index">1-based index of cell to fill</param>
    /// <param name="cell">Cell to fill</param>
    public override void Update(int index, Cell cell) {
        if (index == 1) {
            cellDateTime = DateTime.FromOADate(cell.Value.DoubleValue);
            firstCell = cell;
            return;
        }
        if (index == 2 && cell is { IsEmptyCell: false, Value.HasValue: true }) {
            DateTime deltaDateTime = DateTime.FromOADate(cell.Value.DoubleValue);
            switch (cell.CellFormat) {
                case CellFormat.DATE_DMY:
                case CellFormat.DATE_DM:
                    dayDelta = (deltaDateTime - cellDateTime).Days;
                    break;
                case CellFormat.DATE_MY:
                    monthDelta = (deltaDateTime.Year - cellDateTime.Year) * 12 + deltaDateTime.Month - cellDateTime.Month;
                    break;
            }
            Debug.Assert(firstCell != null);
            cell.StyleFrom(firstCell);
            cellDateTime = deltaDateTime;
            return;
        }
        cellDateTime = cellDateTime.AddDays(dayDelta).AddMonths(monthDelta);
        Debug.Assert(firstCell != null);
        cell.StyleFrom(firstCell);
        cell.Content = cellDateTime.ToOADate().ToString(CultureInfo.CurrentCulture);
    }
}

/// <summary>
/// The CellFiller class implements a filler across a range of cells on
/// a sheet based on the specified enumeration.
/// </summary>
/// <param name="sheet">Worksheet</param>
/// <param name="cells">An enumerable list of cells to fill</param>
public class CellFiller(Sheet sheet, IEnumerable<CellLocation> cells) {

    /// <summary>
    /// Process a fill on the selected cells
    /// </summary>
    public void Process() {
        int index = 1;
        ICellTypeFiller? cellTypeFiller = null;

        foreach (CellLocation location in cells) {
            Cell cell = sheet.GetCell(location, true);
            if (index == 1) {
                cellTypeFiller = ICellTypeFiller.Create(cell);
            }
            Debug.Assert(cellTypeFiller != null);
            cellTypeFiller.Update(index, cell);
            index++;
        }
        sheet.NeedRecalculate = true;
    }
}