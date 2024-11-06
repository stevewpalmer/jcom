// JCalc
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
using System.Text.Json;
using System.Text.Json.Serialization;
using JCalc.Resources;
using JCalcLib;
using JComLib;

namespace JCalc;

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
                    Row = inputSheet.Row;
                    Column = inputSheet.Column;
                    Cells = inputSheet.Cells;
                    ColumnWidths = inputSheet.ColumnWidths;
                }
            }
            catch (Exception e) {
                Screen.Command.Error(string.Format(Calc.CannotOpenFile, filename, e.Message));
            }
        }
        Filename = filename;
        SheetNumber = number;
    }

    /// <summary>
    /// The current selected row, 1 offset
    /// </summary>
    [JsonInclude]
    public int Row { get; set; } = 1;

    /// <summary>
    /// The current selected column, 1 offset
    /// </summary>
    [JsonInclude]
    public int Column { get; set; } = 1;

    /// <summary>
    /// Sheet number
    /// </summary>
    [JsonIgnore]
    public int SheetNumber { get; }

    /// <summary>
    /// Cells
    /// </summary>
    [JsonInclude]
    public Dictionary<int, Cell> Cells { get; set; } = new();

    /// <summary>
    /// Column widths
    /// </summary>
    [JsonInclude]
    public Dictionary<int, int> ColumnWidths { get; set; } = new();

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
    /// Return the height of the specified row
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
    /// <param name="columnNumber">Column number, 1-based</param>
    /// <returns>Column width</returns>
    public int ColumnWidth(int columnNumber) {
        Debug.Assert(columnNumber is >= 1 and <= Consts.MaxColumns);
        return ColumnWidths.GetValueOrDefault(columnNumber, Consts.DefaultColumnWidth);
    }

    /// <summary>
    /// Set the new width of the specified column.
    /// </summary>
    /// <param name="columnNumber">Column number, 1-based</param>
    /// <param name="width">New width</param>
    public bool SetColumnWidth(int columnNumber, int width) {
        Debug.Assert(columnNumber is >= 1 and <= Consts.MaxColumns);
        Debug.Assert(width is >= 0 and <= 100);
        bool success = false;
        if (ColumnWidth(columnNumber) != width) {
            if (width == Consts.DefaultColumnWidth) {
                ColumnWidths.Remove(columnNumber);
            }
            else {
                ColumnWidths[columnNumber] = width;
            }
            Modified = true;
            success = true;
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
        Terminal.Write(cell.ToString(ColumnWidth(Column)));
    }

    /// <summary>
    /// Return the cell at the given column and row.
    /// </summary>
    /// <param name="sheetColumn">Column</param>
    /// <param name="sheetRow">Row</param>
    /// <param name="createIfEmpty">Create the cell if it is empty</param>
    /// <returns>The cell at the row</returns>
    public Cell Cell(int sheetColumn, int sheetRow, bool createIfEmpty) {
        int cellHash = sheetRow * Consts.MaxRows + sheetColumn;
        if (!Cells.TryGetValue(cellHash, out Cell? _cell)) {
            _cell = new Cell {
                Column = sheetColumn,
                Row = sheetRow,
                Alignment = Screen.Config.DefaultCellAlignment,
                Format = Screen.Config.DefaultCellFormat,
                DecimalPlaces = Screen.Config.DefaultDecimals
            };
            if (createIfEmpty) {
                Cells.Add(cellHash, _cell);
                Modified = true;
            }
        }
        return _cell;
    }

    /// <summary>
    /// Insert a new column at the specified column position.
    /// </summary>
    /// <param name="column">Insertion column</param>
    public void InsertColumn(int column) {
        foreach (Cell cell in Cells.Values.Where(cell => cell.Column >= column)) {
            ++cell.Column;
        }
        Modified = true;
    }

    /// <summary>
    /// Insert a new row at the specified row position.
    /// </summary>
    /// <param name="row">Insertion row</param>
    public void InsertRow(int row) {
        foreach (Cell cell in Cells.Values.Where(cell => cell.Row >= row)) {
            ++cell.Row;
        }
        Modified = true;
    }

    /// <summary>
    /// Delete the specified column.
    /// </summary>
    /// <param name="column">Deletion column</param>
    public void DeleteColumn(int column) {
        foreach (Cell cell in Cells.Values.Where(cell => cell.Column >= column)) {
            if (cell.Column == column) {
                DeleteCell(cell);
            } else {
                --cell.Column;
            }
        }
        Modified = true;
    }

    /// <summary>
    /// Delete the specified row.
    /// </summary>
    /// <param name="row">Deletion row</param>
    public void DeleteRow(int row) {
        foreach (Cell cell in Cells.Values.Where(cell => cell.Row >= row)) {
            if (cell.Row == row) {
                DeleteCell(cell);
            } else {
                --cell.Row;
            }
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
    private void DeleteCell(Cell cell) {
        int cellHash = cell.Row * Consts.MaxRows + cell.Column;
        Cells.Remove(cellHash);
    }
}