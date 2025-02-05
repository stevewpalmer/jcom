// JCalcLib
// Workbook management
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

using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JCalcLib;

public class Book {
    private readonly List<Sheet> _sheets = [];
    private readonly CellGraph _cellGraph = new();
    private FileInfo? _fileInfo;
    private bool _modified;
    private int _sheetNumber = 1;

    /// <summary>
    /// Backup file extension
    /// </summary>
    private const string BackupExtension = ".bak";

    /// <summary>
    /// Default file extension
    /// </summary>
    public const string DefaultExtension = ".clc";

    /// <summary>
    /// Default filename (for an empty sheet)
    /// </summary>
    private const string DefaultFilename = "temp" + DefaultExtension;

    /// <summary>
    /// List of all sheets in this book
    /// </summary>
    public ReadOnlyCollection<Sheet> Sheets => new(_sheets);

    /// <summary>
    /// Has this workbook been modified?
    /// </summary>
    public bool Modified {
        get => _modified || _sheets.Any(s => s.Modified);
        private set => _modified = value;
    }

    /// <summary>
    /// Create an empty workbook with just one sheet.
    /// </summary>
    public Book() {
        AddSheet();
    }

    /// <summary>
    /// Reset this book back to new. Any existing worksheets will
    /// be lost unless they are saved.
    /// </summary>
    public void New() {
        _fileInfo = null;
        _sheets.Clear();
        _cellGraph.Clear();
        AddSheet();
    }

    /// <summary>
    /// Opens a workbook from the specified file.
    /// </summary>
    /// <param name="path">The workbook file to be opened</param>
    /// <exception cref="FileNotFoundException">The specified file cannot be found</exception>
    /// <exception cref="FileLoadException">The specified file cannot be loaded</exception>
    public void Open(string path) {
        if (!File.Exists(path)) {
            FileInfo info = new(path);
            throw new FileNotFoundException(null, info.FullName);
        }
        try {
            using FileStream stream = File.OpenRead(path);
            Sheet[]? inputSheets = JsonSerializer.Deserialize<Sheet[]>(stream);
            if (inputSheets != null) {
                _sheets.Clear();
                _cellGraph.Clear();
                _sheetNumber = 1;
                foreach (Sheet inputSheet in inputSheets) {

                    string sheetName = inputSheet.Name;
                    if (string.IsNullOrEmpty(sheetName)) {
                        sheetName = $"Sheet{_sheetNumber++}";
                    }
                    Sheet sheet = new(this, sheetName) {
                        Location = inputSheet.Location
                    };
                    sheet.ColumnList = inputSheet.ColumnList.Select(cellList => new CellList {
                        Index = cellList.Index,
                        Size = cellList.Size,
                        Cells = cellList.Cells.Select(sourceCell => new Cell(sheet, sourceCell)).ToList()
                    }).ToList();

                    sheet.Modified = false;
                    _sheets.Add(sheet);
                }

                // At this point, none of the cell dependencies have been computed so all
                // sheets will require a full recalculate to build those dependencies and
                // calculate all the initial values.
                foreach (Sheet sheet in _sheets) {
                    sheet.Ready = true;
                    sheet.FullRecalculate();
                }
            }
        }
        catch (Exception) {
            _sheets.Add(new Sheet(this, 1){ Ready = true });
            FileInfo info = new(path);
            throw new FileLoadException(null, info.FullName);
        }
        Filename = path;
        Modified = false;
    }

    /// <summary>
    /// Write the workbook back to disk, optionally saving any existing copy
    /// to a backup file.
    /// </summary>
    /// <param name="backup">True if we generate a backup, false if no backup is generated</param>
    /// <exception cref="IOException">An I/O error occurred while saving the file</exception>
    public void Write(bool backup) {
        if (backup && File.Exists(Filename)) {
            string backupFile = Filename + BackupExtension;
            File.Delete(backupFile);
            File.Copy(Filename, backupFile);
        }
        using FileStream stream = File.Create(Filename);
        JsonSerializer.Serialize(stream, Sheets, new JsonSerializerOptions {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            WriteIndented = true
        });
        foreach (Sheet sheet in _sheets) {
            sheet.Modified = false;
        }
        Modified = false;
    }

    /// <summary>
    /// Create a new worksheet and add it to the workbook.
    /// </summary>
    /// <returns>The new worksheet</returns>
    public Sheet AddSheet() {
        Sheet newSheet = new(this, _sheetNumber++) { Ready = true };
        _sheets.Add(newSheet);
        Modified = true;
        return newSheet;
    }

    /// <summary>
    /// Remove the specified worksheet from the workbook.
    /// </summary>
    /// <param name="sheet">The sheet to be removed</param>
    /// <exception cref="ArgumentException">The specified sheet does not exist in the workbook</exception>
    public void RemoveSheet(Sheet sheet) {
        if (!_sheets.Contains(sheet)) {
            throw new ArgumentException("Sheet does not exist");
        }
        _sheets.Remove(sheet);
        Modified = true;
    }

    /// <summary>
    /// Return the name of the sheet. This is the base part of the filename if
    /// there is one, or the default filename otherwise.
    /// </summary>
    public string Name => _fileInfo == null ? DefaultFilename : _fileInfo.Name;

    /// <summary>
    /// Debugger flag.
    /// </summary>
    public bool Debug { get; set; }

    /// <summary>
    /// Return the fully qualified file name with path.
    /// </summary>
    public string Filename {
        get => _fileInfo == null ? DefaultFilename : _fileInfo.FullName;
        set => _fileInfo = string.IsNullOrEmpty(value) ? null : new FileInfo(value);
    }

    /// <summary>
    /// Return a worksheet given its name.
    /// </summary>
    /// <param name="sheetName">Name of sheet to locate</param>
    /// <returns>The sheet whose name matches the given name, or null if not found</returns>
    public Sheet? Sheet(string sheetName) {
        return _sheets.FirstOrDefault(s => s.Name == sheetName);
    }

    /// <summary>
    /// Set the dependencies for the specified cell location.
    /// </summary>
    /// <param name="location">Absolute cell location</param>
    /// <param name="dependents">List of absolute dependencies</param>
    public void SetDependencies(CellLocation location, IEnumerable<CellLocation> dependents) {
        foreach (CellLocation dependent in dependents) {
            _cellGraph.AddEdge(location, dependent);
        }
    }

    /// <summary>
    /// Remove all dependencies from the specified location.
    /// </summary>
    /// <param name="location">Location to remove</param>
    public void RemoveDependencies(CellLocation location) {
        _cellGraph.DeleteEdges(location);
    }

    /// <summary>
    /// Return the dependencies for the cell at the specified location.
    /// </summary>
    /// <param name="location">Fully qualified location of cell</param>
    /// <returns>A collection of dependent cell locations</returns>
    public IEnumerable<CellLocation> Dependents(CellLocation location) => _cellGraph.GetDependents(location);

    /// <summary>
    /// Return the precedents for the cell at the specified location.
    /// </summary>
    /// <param name="location">Fully qualified location of cell</param>
    /// <returns>A collection of precedent cell locations</returns>
    public IEnumerable<CellLocation> Precedents(CellLocation location) => _cellGraph.GetPrecedents(location);
}