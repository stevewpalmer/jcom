// JCalcLib
// Worksheet management
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
using System.Text.Json.Serialization;
using JComLib;

namespace JCalcLib;

public class Sheet {

    /// <summary>
    /// Maximum number of columns
    /// </summary>
    public const int MaxColumns = 255;

    /// <summary>
    /// Maximum number of rows
    /// </summary>
    public const int MaxRows = 4096;

    /// <summary>
    /// Default column width
    /// </summary>
    public const int DefaultColumnWidth = 10;

    /// <summary>
    /// Empty constructor with no sheet number
    /// </summary>
    public Sheet() { }

    /// <summary>
    /// Create a new empty sheet with the specified number.
    /// </summary>
    /// <param name="number">Sheet number</param>
    public Sheet(int number) {
        SheetNumber = number;
    }

    /// <summary>
    /// Column list
    /// </summary>
    [JsonInclude]
    public List<CellList> ColumnList { get; set; } = [];

    /// <summary>
    /// The current selected cell location, 1 offset
    /// </summary>
    [JsonInclude]
    public CellLocation Location { get; set; } = new();

    /// <summary>
    /// Sheet number
    /// </summary>
    [JsonIgnore]
    public int SheetNumber { get; }

    /// <summary>
    /// Do changes to the sheet require a recalculation?
    /// </summary>
    [JsonIgnore]
    public bool NeedRecalculate { get; set; }

    /// <summary>
    /// Whether the sheet has been modified since it was last
    /// changed.
    /// </summary>
    [JsonIgnore]
    public bool Modified { get; internal set; }

    /// <summary>
    /// Returns the active cell
    /// </summary>
    [JsonIgnore]
    public Cell ActiveCell => GetCell(Location, false);

    /// <summary>
    /// Return the height of a row
    /// </summary>
    /// <returns>Row height</returns>
    public static int RowHeight => 1;

    /// <summary>
    /// Return the width of the specified column.
    /// </summary>
    /// <param name="column">Column number, 1-based</param>
    /// <returns>Column width</returns>
    public int ColumnWidth(int column) {
        Debug.Assert(column is >= 1 and <= MaxColumns);
        CellList? cellList = CellListForColumn(column, false);
        return cellList?.Size ?? DefaultColumnWidth;
    }

    /// <summary>
    /// Set the new width of the specified column.
    /// </summary>
    /// <param name="column">Column number, 1-based</param>
    /// <param name="width">New width</param>
    public bool SetColumnWidth(int column, int width) {
        Debug.Assert(column is >= 1 and <= MaxColumns);
        Debug.Assert(width is >= 0 and <= 100);
        int c = 0;
        bool success = false;
        while (c < ColumnList.Count) {
            if (ColumnList[c].Index == column) {
                success = ColumnList[c].Size != width;
                ColumnList[c].Size = width;
                break;
            }
            if (ColumnList[c].Index > column) {
                ColumnList.Insert(c, new CellList { Index = column, Size = width });
                success = true;
                break;
            }
            c++;
        }
        if (success) {
            Modified = true;
        }
        return success;
    }

    /// <summary>
    /// Return the cell at the given column and row.
    /// </summary>
    /// <param name="location">Location of cell</param>
    /// <param name="createIfEmpty">Create the cell if it is empty</param>
    /// <returns>The cell at the row</returns>
    public Cell GetCell(CellLocation location, bool createIfEmpty) =>
        GetCell(location.Column, location.Row, createIfEmpty);

    /// <summary>
    /// Return the cell at the given column and row.
    /// </summary>
    /// <param name="column">Column location of cell</param>
    /// <param name="row">Row location of cell</param>
    /// <param name="createIfEmpty">Create the cell if it is empty</param>
    /// <returns>The cell at the row</returns>
    public Cell GetCell(int column, int row, bool createIfEmpty) {
        CellList? cellList = CellListForColumn(column, createIfEmpty);
        Cell cell = new Cell(this) {
            Location = new CellLocation(column, row)
        };
        if (cellList != null) {
            int c = 0;
            while (c < cellList.Cells.Count) {
                if (cellList.Cells[c].Location.Row == row) {
                    return cellList.Cells[c];
                }
                if (cellList.Cells[c].Location.Row > row) {
                    break;
                }
                c++;
            }
            if (createIfEmpty) {
                cellList.Cells.Insert(c, cell);
            }
        }
        return cell;
    }

    /// <summary>
    /// Recalculate all formulas on the sheet and update the values
    /// on the formula cells.
    /// </summary>
    public IEnumerable<Cell> Calculate() {
        List<Cell> formulaCells = [];
        foreach (CellList cellList in ColumnList) {
            formulaCells.AddRange(cellList.FormulaCells);
        }
        List<Cell> cellsToUpdate = [];
        foreach (Cell cell in formulaCells) {
            try {
                CalculationContext context = new CalculationContext {
                    ReferenceList = new Stack<CellLocation>(),
                    UpdateList = cellsToUpdate.ToArray(),
                    Sheet = this
                };
                context.ReferenceList.Push(cell.Location);
                Debug.Assert(cell.FormulaTree != null);
                cell.ComputedValue = cell.FormulaTree.Evaluate(context);
                cellsToUpdate.Add(cell);
            }
            catch (Exception) {
                cell.Error = true;
            }
        }
        return cellsToUpdate;
    }

    /// <summary>
    /// Insert a new column at the specified column position.
    /// </summary>
    /// <param name="column">Insertion column</param>
    public void InsertColumn(int column) {
        Debug.Assert(column is >= 1 and <= MaxColumns);
        int c = 0;
        while (c < ColumnList.Count && ColumnList[c].Index < column) {
            c++;
        }
        while (c < ColumnList.Count) {
            ++ColumnList[c].Index;
            foreach (Cell cell in ColumnList[c].Cells) {
                CellLocation cellLocation = cell.Location;
                ++cellLocation.Column;
                cell.Location = cellLocation;
            }
            c++;
        }
        FixupFormulaCells(column, 0, 1);
        Modified = true;
    }

    /// <summary>
    /// Insert a new row at the specified row position.
    /// </summary>
    /// <param name="row">Insertion row</param>
    public void InsertRow(int row) {
        Debug.Assert(row is >= 1 and <= MaxRows);
        int c = 0;
        while (c < ColumnList.Count) {
            foreach (Cell cell in ColumnList[c].Cells) {
                if (cell.Location.Row >= row) {
                    CellLocation cellLocation = cell.Location;
                    ++cellLocation.Row;
                    cell.Location = cellLocation;
                }
            }
            c++;
        }
        FixupFormulaCells(0, row, 1);
        Modified = true;
    }

    /// <summary>
    /// Delete the specified column.
    /// </summary>
    /// <param name="column">Deletion column</param>
    public void DeleteColumn(int column) {
        Debug.Assert(column is >= 1 and <= MaxColumns);
        int c = ColumnList.Count - 1;
        while (c >= 0) {
            if (ColumnList[c].Index == column) {
                ColumnList.RemoveAt(c);
                break;
            }
            if (ColumnList[c].Index < column) {
                break;
            }
            --ColumnList[c].Index;
            foreach (Cell cell in ColumnList[c].Cells) {
                CellLocation cellLocation = cell.Location;
                --cellLocation.Column;
                cell.Location = cellLocation;
            }
            c--;
        }
        FixupFormulaCells(column, 0, -1);
        Modified = true;
    }

    /// <summary>
    /// Delete the specified row.
    /// </summary>
    /// <param name="row">Deletion row</param>
    public void DeleteRow(int row) {
        Debug.Assert(row is >= 1 and <= MaxRows);
        int c = ColumnList.Count - 1;
        while (c >= 0) {
            int r = ColumnList[c].Cells.Count - 1;
            while (r >= 0) {
                Cell cell = ColumnList[c].Cells[r];
                if (cell.Location.Row == row) {
                    ColumnList[c].Cells.RemoveAt(r);
                    break;
                }
                if (cell.Location.Row > row) {
                    CellLocation cellLocation = cell.Location;
                    --cellLocation.Row;
                    cell.Location = cellLocation;
                }
                r--;
            }
            c--;
        }
        FixupFormulaCells(0, row, -1);
        Modified = true;
    }

    /// <summary>
    /// Delete the specified cell
    /// </summary>
    /// <param name="cell">Cell to delete</param>
    public void DeleteCell(Cell cell) {
        CellList? cellList = CellListForColumn(cell.Location.Column, false);
        if (cellList != null) {
            cellList.Cells.Remove(cell);
            Modified = true;
        }
    }

    /// <summary>
    /// Render a row of cells using the specified data
    /// </summary>
    /// <param name="column">Start column, 1-based</param>
    /// <param name="row">Row index to return</param>
    /// <param name="width">Line width</param>
    /// <returns>String representation of row</returns>
    public AnsiText GetRow(int column, int row, int width) {
        Debug.Assert(column is >= 1 and <= MaxColumns);
        List<AnsiTextSpan> spans = [];
        int columnIndex = 1;
        int totalWidth = 0;
        while (totalWidth < width) {
            int size = ColumnWidth(columnIndex);
            Cell cell = GetCell(columnIndex, row, false);
            if (cell is { Value.IsNumber: false } && cell.Value.StringValue?.Length > size) {
                int labelLength = cell.Value.StringValue.Length;
                int fwdColumnIndex = columnIndex;
                List<int> widths = [size];
                do {
                    Cell nextCell = GetCell(fwdColumnIndex + 1, row, false);
                    if (nextCell is not { IsEmptyCell: true }) {
                        break;
                    }
                    int columnWidth = ColumnWidth(++fwdColumnIndex);
                    widths.Add(columnWidth);
                    size += columnWidth;
                } while (size < labelLength);
                string labelCellText = cell.Text(labelLength);
                int index = 0;
                foreach (int columnWidth in widths) {
                    if (columnIndex >= column) {
                        spans.Add(new AnsiTextSpan(Utilities.SpanBound(labelCellText, index, columnWidth)) {
                            ForegroundColour = cell.Style.ForegroundColour,
                            BackgroundColour = cell.Style.BackgroundColour,
                            Bold = cell.Style.Bold,
                            Italic = cell.Style.Italic,
                            Underline = cell.Style.Underline,
                            Alignment = AnsiAlignment.NONE
                        });
                    }
                    index += columnWidth;
                    columnIndex++;
                }
            }
            else {
                if (columnIndex >= column) {
                    spans.Add(cell.Text(size, false));
                }
                columnIndex++;
            }
            totalWidth += size;
        }
        return new AnsiText(spans);
    }

    /// <summary>
    /// Sort a range of cells using the specified sort column and order. The sort
    /// column must lie within the swapExtent column range otherwise an assertion is
    /// thrown.
    /// </summary>
    /// <param name="sortColumn">1-based index of sort column</param>
    /// <param name="descending">True if we sort descending</param>
    /// <param name="swapExtent">Range of cells to sort</param>
    public void SortCells(int sortColumn, bool descending, RExtent swapExtent) {
        Debug.Assert(sortColumn >= swapExtent.Start.X && sortColumn <= swapExtent.End.X);
        int ordering = descending ? -1 : 1;
        bool sorted;
        do {
            sorted = true;
            for (int r = swapExtent.Start.Y; r < swapExtent.End.Y; r++) {
                Cell cell1 = GetCell(sortColumn, r , false);
                Cell cell2 = GetCell(sortColumn, r + 1, false);
                if (cell1.Value.CompareTo(cell2.Value) * ordering > 0) {
                    for (int c = swapExtent.Start.X; c <= swapExtent.End.X; c++) {
                        cell1 = GetCell(c, r, false);
                        cell2 = GetCell(c, r + 1, false);
                        cell1.Swap(cell2);
                    }
                    sorted = false;
                    Modified = true;
                }
            }
        } while (!sorted);
    }

    /// <summary>
    /// Get the extent of all cells on the sheet.
    /// </summary>
    /// <returns>Extent that covers all cells on the sheet</returns>
    public RExtent GetCellExtent() {
        RExtent extent = new RExtent();
        Point? topLeft = ColumnList.FirstOrDefault()?.Cells.FirstOrDefault()?.Location.Point;
        Point? bottomRight = ColumnList.LastOrDefault()?.Cells.LastOrDefault()?.Location.Point;
        if (topLeft != null) {
            extent.Add(topLeft.Value);
            Debug.Assert(bottomRight.HasValue);
            extent.Add(bottomRight.Value);
        }
        return extent;
    }

    /// <summary>
    /// Iterate over all formula cells and fix up any address references
    /// to the specified column or row by the given offset.
    /// </summary>
    /// <param name="column">Column to fix</param>
    /// <param name="row">Row to fix</param>
    /// <param name="offset">Offset to be applied to the column and/or row</param>
    private void FixupFormulaCells(int column, int row, int offset) {
        NeedRecalculate = false;
        foreach (Cell cell in ColumnList.SelectMany(cellList => cellList.FormulaCells)) {
            if (cell.FixupFormula(column, row, offset)) {
                NeedRecalculate = true;
            }
        }
    }

    /// <summary>
    /// Return the CellList for the specified column, creating it in
    /// column order if needed.
    /// </summary>
    /// <param name="column">Column required</param>
    /// <param name="createIfEmpty">True if we create the cell list</param>
    /// <returns>CellList for the column</returns>
    private CellList? CellListForColumn(int column, bool createIfEmpty) {
        int c = 0;
        while (c < ColumnList.Count) {
            if (ColumnList[c].Index == column) {
                return ColumnList[c];
            }
            if (ColumnList[c].Index > column) {
                break;
            }
            c++;
        }
        if (!createIfEmpty) {
            return null;
        }
        ColumnList.Insert(c, new CellList { Index = column, Size = DefaultColumnWidth });
        return ColumnList[c];
    }
}