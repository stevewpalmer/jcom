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

namespace JCalc;

public class Sheet {
    private FileInfo? _fileInfo;
    private readonly Dictionary<int, Cell> _cells = new();

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
        }
        else {
            NewFile = true;
        }
        Filename = filename;
        SheetNumber = number;
    }

    /// <summary>
    /// Maximum number of columns
    /// </summary>
    public static int MaxColumns => 255;

    /// <summary>
    /// Maximum number of rows
    /// </summary>
    public static int MaxRows => 4096;

    /// <summary>
    /// The current selected row, 1 offset
    /// </summary>
    public int Row { get; set; } = 1;

    /// <summary>
    /// The current selected column, 1 offset
    /// </summary>
    public int Column { get; set; } = 1;

    /// <summary>
    /// Sheet number
    /// </summary>
    public int SheetNumber { get; init; }

    /// <summary>
    /// Return the name of the sheet. This is the base part of the filename if
    /// there is one, or the New File string otherwise.
    /// </summary>
    public string Name => _fileInfo == null ? "New File" : _fileInfo.Name;

    /// <summary>
    /// Return the fully qualified file name with path.
    /// </summary>
    public string Filename {
        get => _fileInfo == null ? string.Empty : _fileInfo.FullName;
        set => _fileInfo = string.IsNullOrEmpty(value) ? null : new FileInfo(value);
    }

    /// <summary>
    /// Whether the sheet has been modified since it was last
    /// changed.
    /// </summary>
    public bool Modified { get; private set; }

    /// <summary>
    /// Is this a new file (i.e. the file has a name but does not
    /// currently exist on disk).
    /// </summary>
    public bool NewFile { get; private set; }

    /// <summary>
    /// Write the sheet back to disk.
    /// </summary>
    public void Write() {
        Modified = false;
        NewFile = false;
    }

    /// <summary>
    /// Return the width of the specified column.
    /// </summary>
    /// <param name="columnNumber">Column number, 1-based</param>
    /// <returns>Column width</returns>
    public int ColumnWidth(int columnNumber) {
        return 10;
    }

    /// <summary>
    /// Return the height of the specified row
    /// </summary>
    /// <param name="rowNumber">Row number, 1-based</param>
    /// <returns>Row height</returns>
    public int RowHeight(int rowNumber) {
        return 1;
    }

    /// <summary>
    /// Return the cell at the given column and row.
    /// </summary>
    /// <param name="sheetColumn">Column</param>
    /// <param name="sheetRow">Row</param>
    /// <param name="createIfEmpty">Create the cell if it is empty</param>
    /// <returns>The cell at the row</returns>
    public Cell Cell(int sheetColumn, int sheetRow, bool createIfEmpty) {
        int cellHash = sheetRow * MaxRows + sheetColumn;
        if (!_cells.TryGetValue(cellHash, out Cell? _cell)) {
            _cell = new Cell {
                Column = sheetColumn,
                Row = sheetRow
            };
            if (createIfEmpty) {
                _cells.Add(cellHash, _cell);
                Modified = true;
            }
        }
        return _cell;
    }
}