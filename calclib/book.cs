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
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JCalcLib;

public class Book {
    private readonly List<Sheet> _sheets = [];
    private FileInfo? _fileInfo;
    private bool _modified;

    /// <summary>
    /// Backup file extension
    /// </summary>
    public const string BackupExtension = ".bak";

    /// <summary>
    /// Default file extension
    /// </summary>
    public const string DefaultExtension = ".clc";

    /// <summary>
    /// Default filename (for an empty sheet)
    /// </summary>
    public const string DefaultFilename = "temp" + DefaultExtension;

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
        _sheets.Add(new Sheet(1));
    }

    /// <summary>
    /// Opens a workbook from the specified file.
    /// </summary>
    /// <param name="filename">Name of file</param>
    public bool Open(string filename) {

        if (!File.Exists(filename)) {
            FileInfo info = new FileInfo(filename);
            throw new Exception(string.Format(CalcLib.FileNotFound, info.Name));
        }
        try {
            using FileStream stream = File.OpenRead(filename);
            Sheet[]? inputSheets = JsonSerializer.Deserialize<Sheet[]>(stream);
            if (inputSheets != null) {
                int sheetNumber = 1;
                _sheets.Clear();
                foreach (Sheet inputSheet in inputSheets) {
                    Sheet sheet = new Sheet(sheetNumber) {
                        Location = inputSheet.Location,
                        ColumnList = inputSheet.ColumnList
                    };
                    _sheets.Add(sheet);
                    Calculate calc = new Calculate(sheet);
                    calc.Update();
                    sheetNumber++;
                }
            }
        }
        catch (JsonException) {
            FileInfo info = new FileInfo(filename);
            throw new Exception(string.Format(CalcLib.ErrorReadingFile, info.Name));
        }
        Filename = filename;
        Modified = false;
        return true;
    }

    /// <summary>
    /// Write the workbook back to disk.
    /// </summary>
    /// <param name="backup"></param>
    public void Write(bool backup) {
        try {
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
        }
        catch (Exception e) {
            throw new Exception(string.Format(CalcLib.CannotSaveFile, Filename, e.Message));
        }
        Modified = false;
    }

    /// <summary>
    /// Create a new worksheet and add it to the workbook.
    /// </summary>
    /// <returns>The new worksheet</returns>
    public Sheet AddSheet() {
        Sheet newSheet = new Sheet(NextSheetNumber());
        _sheets.Add(newSheet);
        Modified = true;
        return newSheet;
    }

    /// <summary>
    /// Remove the specified worksheet from the workbook.
    /// </summary>
    /// <param name="sheet">The sheet to be removed</param>
    public void RemoveSheet(Sheet sheet) {
        Debug.Assert(_sheets.Contains(sheet));
        _sheets.Remove(sheet);
        Modified = true;
    }

    /// <summary>
    /// Return the name of the sheet. This is the base part of the filename if
    /// there is one, or the default filename otherwise.
    /// </summary>
    public string Name => _fileInfo == null ? DefaultFilename : _fileInfo.Name;

    /// <summary>
    /// Return the fully qualified file name with path.
    /// </summary>
    public string Filename {
        get => _fileInfo == null ? DefaultFilename : _fileInfo.FullName;
        set => _fileInfo = string.IsNullOrEmpty(value) ? null : new FileInfo(value);
    }

    /// <summary>
    /// Return the next available sheet number.
    /// </summary>
    /// <returns>Sheet number</returns>
    private int NextSheetNumber() {
        int sheetNumber = 1;
        foreach (Sheet _ in _sheets.TakeWhile(s => s.SheetNumber == sheetNumber)) {
            ++sheetNumber;
        }
        return sheetNumber;
    }
}