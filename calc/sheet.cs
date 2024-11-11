﻿// JCalc
// Sheet management
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
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using JCalc.Resources;
using JCalcLib;
using JComLib;

namespace JCalc;

public class CellList {

    /// <summary>
    /// Row or column 1-based index of cells
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// List of cells in the row or column
    /// </summary>
    [JsonInclude]
    public List<Cell> Cells { get; set; } = [];

    /// <summary>
    /// Size of the row or column
    /// </summary>
    public int Size { get; set; }
}

public class Sheet {
    private FileInfo? _fileInfo;

    public Sheet() {}

    /// <summary>
    /// Create a new empty sheet not associated with any file.
    /// </summary>
    /// <param name="number">Sheet number</param>
    public Sheet(int number) {
        SheetNumber = number;
    }

    /// <summary>
    /// Creates a sheet with the specified file.
    /// </summary>
    /// <param name="number">Sheet number</param>
    /// <param name="filename">Name of file</param>
    public Sheet(int number, string filename) {

        if (!string.IsNullOrEmpty(filename) && File.Exists(filename)) {
            try {
                using FileStream stream = File.OpenRead(filename);
                Sheet? inputSheet = JsonSerializer.Deserialize<Sheet>(stream);
                if (inputSheet != null) {
                    Location = inputSheet.Location;
                    ColumnList = inputSheet.ColumnList;
                }
            }
            catch (JsonException) {
                FileInfo info = new FileInfo(filename);
                Screen.Command.Error(string.Format(Calc.ErrorReadingFile, info.Name));
            }
        }
        Filename = filename;
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
    /// Return the name of the sheet. This is the base part of the filename if
    /// there is one, or the New File string otherwise.
    /// </summary>
    [JsonIgnore]
    public string Name => _fileInfo == null ? "New File" : _fileInfo.Name;

    /// <summary>
    /// Return the fully qualified file name with path.
    /// </summary>
    [JsonIgnore]
    public string Filename {
        get => _fileInfo == null ? Consts.DefaultFilename : _fileInfo.FullName;
        set => _fileInfo = string.IsNullOrEmpty(value) ? null : new FileInfo(value);
    }

    /// <summary>
    /// Whether the sheet has been modified since it was last
    /// changed.
    /// </summary>
    [JsonIgnore]
    public bool Modified { get; private set; }

    /// <summary>
    /// Returns the active cell
    /// </summary>
    [JsonIgnore]
    public Cell ActiveCell => Cell(Location, false);

    /// <summary>
    /// Return the height of a row
    /// </summary>
    /// <returns>Row height</returns>
    public static int RowHeight => 1;

    /// <summary>
    /// Write the sheet back to disk.
    /// </summary>
    public void Write() {
        try {
            if (Screen.Config.BackupFile && File.Exists(Filename)) {
                string backupFile = Filename + Consts.BackupExtension;
                File.Delete(backupFile);
                File.Copy(Filename, backupFile);
            }
            using FileStream stream = File.Create(Filename);
            JsonSerializer.Serialize(stream, this, new JsonSerializerOptions {
                WriteIndented = true
            });
        }
        catch (Exception e) {
            Screen.Command.Error(string.Format(Calc.CannotSaveFile, Filename, e.Message));
        }
        Modified = false;
    }

    /// <summary>
    /// Return the width of the specified column.
    /// </summary>
    /// <param name="column">Column number, 1-based</param>
    /// <returns>Column width</returns>
    public int ColumnWidth(int column) {
        Debug.Assert(column is >= 1 and <= Consts.MaxColumns);
        CellList? cellList = ColumnList.Find(c => c.Index == column);
        return cellList?.Size ?? Consts.DefaultColumnWidth;
    }

    /// <summary>
    /// Set the new width of the specified column.
    /// </summary>
    /// <param name="column">Column number, 1-based</param>
    /// <param name="width">New width</param>
    public bool SetColumnWidth(int column, int width) {
        Debug.Assert(column is >= 1 and <= Consts.MaxColumns);
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
        return success;
    }

    /// <summary>
    /// Draw this cell at the given cell position (1-based row and column) at
    /// the given physical screen offset where (0,0) is the top left corner.
    /// </summary>
    /// <param name="cell">Cell to draw</param>
    /// <param name="x">X position of cell</param>
    /// <param name="y">Y position of cell</param>
    public void DrawCell(Cell cell, int x, int y) {
        Terminal.SetCursor(x, y);
        Terminal.Write(cell.ToString(ColumnWidth(cell.Location.Column)));
    }

    /// <summary>
    /// Return the cell at the given column and row.
    /// </summary>
    /// <param name="location">Location of cell</param>
    /// <param name="createIfEmpty">Create the cell if it is empty</param>
    /// <returns>The cell at the row</returns>
    public Cell Cell(CellLocation location, bool createIfEmpty) {
        CellList? cellList = CellListForColumn(location.Column, createIfEmpty);
        Cell cell = new Cell {
            Alignment = Screen.Config.DefaultCellAlignment,
            Format = Screen.Config.DefaultCellFormat,
            DecimalPlaces = Screen.Config.DefaultDecimals,
            Location = location,
        };
        if (cellList != null) {
            int c = 0;
            while (c < cellList.Cells.Count) {
                if (cellList.Cells[c].Location.Row == location.Row) {
                    return cellList.Cells[c];
                }
                if (cellList.Cells[c].Location.Row > location.Row) {
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
    /// Insert a new column at the specified column position.
    /// </summary>
    /// <param name="column">Insertion column</param>
    public void InsertColumn(int column) {
        Debug.Assert(column is >= 1 and <= Consts.MaxColumns);
        int c = 0;
        while (c < ColumnList.Count) {
            if (ColumnList[c].Index >= column) {
                break;
            }
            c++;
        }
        while (++c < ColumnList.Count) {
            ++ColumnList[c].Index;
            foreach (Cell cell in ColumnList[c].Cells) {
                CellLocation cellLocation = cell.Location;
                ++cellLocation.Column;
                cell.Location = cellLocation;
            }
        }
        Modified = true;
    }

    /// <summary>
    /// Insert a new row at the specified row position.
    /// </summary>
    /// <param name="row">Insertion row</param>
    public void InsertRow(int row) {
        Debug.Assert(row is >= 1 and <= Consts.MaxRows);
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
        Modified = true;
    }

    /// <summary>
    /// Delete the specified column.
    /// </summary>
    /// <param name="column">Deletion column</param>
    public void DeleteColumn(int column) {
        Debug.Assert(column is >= 1 and <= Consts.MaxColumns);
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
        Modified = true;
    }

    /// <summary>
    /// Delete the specified row.
    /// </summary>
    /// <param name="row">Deletion row</param>
    public void DeleteRow(int row) {
        Debug.Assert(row is >= 1 and <= Consts.MaxRows);
        int c = ColumnList.Count - 1;
        while (c >= 0) {
            int r = ColumnList[c].Index - 1;
            while (r >= 0) {
                Cell cell = ColumnList[c].Cells[r];
                if (cell.Location.Row == row) {
                    ColumnList[c].Cells.RemoveAt(c);
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
        Modified = true;
    }

    /// <summary>
    /// Set the new cell format and number of decimal places.
    /// </summary>
    /// <param name="cell">Cell to format</param>
    /// <param name="format">New format</param>
    /// <param name="decimalPlaces">Number of decimal places</param>
    public void SetCellFormat(Cell cell, CellFormat format, int decimalPlaces) {
        cell.Format = format;
        cell.DecimalPlaces = decimalPlaces;
        Modified = true;
    }

    /// <summary>
    /// Set the cell alignment.
    /// </summary>
    /// <param name="cell">Cell to align</param>
    /// <param name="alignment">New alignment</param>
    public void SetCellAlignment(Cell cell, CellAlignment alignment) {
        cell.Alignment = alignment;
        Modified = true;
    }

    /// <summary>
    /// Delete the specified cell
    /// </summary>
    /// <param name="cell">Cell to delete</param>
    public void DeleteCell(Cell cell) {
        Modified = true;
    }

    /// <summary>
    /// Render a row of cells using the specified data
    /// </summary>
    /// <param name="column">Start column, 1-based</param>
    /// <param name="row">Row index to return</param>
    /// <returns>String representation of row</returns>
    public string GetRow(int column, int row) {
        Debug.Assert(column is >= 1 and <= Consts.MaxColumns);
        StringBuilder line = new();
        int columnIndex = column;
        Cell emptyCell = new();
        int c = 0;
        while (c < ColumnList.Count && ColumnList[c].Index < column) {
            c++;
        }
        while (c < ColumnList.Count) {
            while (ColumnList[c].Index > columnIndex) {
                line.Append(emptyCell.ToString(Consts.DefaultColumnWidth));
                columnIndex++;
            }
            int width = ColumnList[c].Size;
            Cell? cell = ColumnList[c].Cells.Find(c => c.Location.Row == row);
            line.Append(cell == null ? emptyCell.ToString(width) : cell.ToString(width));
            columnIndex++;
            c++;
        }
        return line.ToString();
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
                Cell cell1 = Cell(new CellLocation { Row = r, Column = sortColumn}, false);
                Cell cell2 = Cell(new CellLocation { Row = r + 1, Column = sortColumn}, false);
                if (cell1.CellValue.CompareTo(cell2.CellValue) * ordering > 0) {
                    for (int c = swapExtent.Start.X; c <= swapExtent.End.X; c++) {
                        cell1 = Cell(new CellLocation { Row = r, Column = c}, false);
                        cell2 = Cell(new CellLocation { Row = r + 1, Column = c}, false);
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
        foreach (Cell cell in ColumnList.SelectMany(cellList => cellList.Cells)) {
            extent.Add(cell.Location.Point);
        }
        return extent;
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
        ColumnList.Insert(c, new CellList { Index = column, Size = Consts.DefaultColumnWidth });
        return ColumnList[c];
    }
}