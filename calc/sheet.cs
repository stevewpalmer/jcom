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

namespace JCalc;

public class Sheet {
    private FileInfo? _fileInfo;

    public Sheet() {}

    /// <summary>
    /// Create a new empty sheet not associated with any file.
    /// </summary>
    /// <param name="number">Sheet number</param>
    public Sheet(int number) {
        NewFile = true;
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
            catch (Exception) { }
        }
        else {
            NewFile = true;
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
    public int SheetNumber { get; init; }

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
    private string Filename {
        get => _fileInfo == null ? string.Empty : _fileInfo.FullName;
        set => _fileInfo = string.IsNullOrEmpty(value) ? null : new FileInfo(value);
    }

    /// <summary>
    /// Whether the sheet has been modified since it was last
    /// changed.
    /// </summary>
    [JsonIgnore]
    public bool Modified { get; set; }

    /// <summary>
    /// Is this a new file (i.e. the file has a name but does not
    /// currently exist on disk).
    /// </summary>
    [JsonIgnore]
    public bool NewFile { get; private set; }

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
            Screen.Command.Error($"Cannot save {Filename} - {e.Message}");
        }

        Modified = false;
        NewFile = false;
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
    /// Return the height of the specified row
    /// </summary>
    /// <returns>Row height</returns>
    public static int RowHeight => 1;

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
                Alignment = Screen.Config.DefaultCellAlignment
            };
            if (createIfEmpty) {
                Cells.Add(cellHash, _cell);
                Modified = true;
            }
        }
        return _cell;
    }
}