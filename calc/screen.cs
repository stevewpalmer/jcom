// JCalc
// Screen management
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
using CsvHelper;
using JCalc.Resources;
using JCalcLib;
using JComLib;

namespace JCalc;

public static class Screen {
    private static readonly List<Window> _windowList = [];
    private static Window? _activeWindow;
    private static readonly Book _activeBook = new();

    /// <summary>
    /// Configured colours
    /// </summary>
    public static Colours Colours { get; private set; } = new();

    /// <summary>
    /// Configuration
    /// </summary>
    public static Config Config { get; private set; } = new();

    /// <summary>
    /// The command bar
    /// </summary>
    public static CommandBar Command { get; } = new();

    /// <summary>
    /// The status bar
    /// </summary>
    public static StatusBar Status { get; } = new();

    /// <summary>
    /// Open the main window.
    /// </summary>
    public static void Open() {
        Terminal.Open();
        Terminal.ShowCursor(false);

        Config = Config.Load();
        Colours = new Colours(Config);

        SetCellFactory();
        Command.Refresh();
        Status.Refresh();
        Version();
    }

    /// <summary>
    /// Close the main screen when calc is closed.
    /// </summary>
    public static void Close() {
        Config.Save();
        Terminal.Close();
    }

    /// <summary>
    /// Initialise the workbook from the specified file.
    /// </summary>
    /// <param name="filename">Workbook filename</param>
    public static void OpenBook(string filename) {
        if (!string.IsNullOrEmpty(filename)) {
            try {
                _activeBook.Open(filename);
                _windowList.Clear();
            }
            catch (FileNotFoundException e) {
                Status.Message(string.Format(Calc.FileNotFound, e.FileName));
            }
            catch (FileLoadException e) {
                Status.Message(string.Format(Calc.ErrorReadingFile, e.FileName));
            }
            catch (Exception e) {
                Status.Message(e.Message);
            }
        }
        foreach (Sheet sheet in _activeBook.Sheets) {
            AddWindow(new Window(sheet));
        }
        ActivateWindow(0);
        Status.UpdateFilename(_activeBook.Name);
    }

    /// <summary>
    /// Start the keyboard loop and exit when the user issues the
    /// exit command.
    /// </summary>
    public static void StartKeyboardLoop() {
        RenderHint flags;
        do {
            ConsoleKeyInfo keyIn = Command.ReadKey();
            flags = HandleCommand(Command.MapKeyToCommand(keyIn));
        } while (flags != RenderHint.EXIT);
    }

    /// <summary>
    /// Render the current cursor position on the status bar.
    /// </summary>
    public static void UpdateCursorPosition() {
        if (_activeWindow != null) {
            Command.UpdateCellStatus(_activeWindow.Sheet);
        }
    }

    /// <summary>
    /// Handle commands at the screen level and pass on any unhandled
    /// ones to the active window.
    /// </summary>
    /// <param name="command">Editing command</param>
    /// <returns>Render hint</returns>
    public static RenderHint HandleCommand(KeyCommand command) {
        if (_activeWindow == null) {
            throw new InvalidOperationException();
        }
        Status.ClearMessage();
        RenderHint flags = command switch {
            KeyCommand.KC_COMMAND_BAR => HandleCommandBar(),
            KeyCommand.KC_NEW => NewWorksheet(),
            KeyCommand.KC_DELETE_WORKSHEET => DeleteWorksheet(),
            KeyCommand.KC_FILE_RETRIEVE => OpenWorkbook(),
            KeyCommand.KC_FILE_IMPORT => ImportFile(),
            KeyCommand.KC_FILE_SAVE => SaveFile(),
            KeyCommand.KC_SETTINGS_COLOURS => ConfigureColours(),
            KeyCommand.KC_DEFAULT_DATE_DM => SetDefaultFormat(CellFormat.DATE_DM),
            KeyCommand.KC_DEFAULT_DATE_DMY => SetDefaultFormat(CellFormat.DATE_DMY),
            KeyCommand.KC_DEFAULT_DATE_MY => SetDefaultFormat(CellFormat.DATE_MY),
            KeyCommand.KC_DEFAULT_FORMAT_CURRENCY => SetDefaultFormat(CellFormat.CURRENCY),
            KeyCommand.KC_DEFAULT_FORMAT_FIXED => SetDefaultFormat(CellFormat.FIXED),
            KeyCommand.KC_DEFAULT_FORMAT_GENERAL => SetDefaultFormat(CellFormat.GENERAL),
            KeyCommand.KC_DEFAULT_FORMAT_PERCENT => SetDefaultFormat(CellFormat.PERCENT),
            KeyCommand.KC_DEFAULT_FORMAT_SCI => SetDefaultFormat(CellFormat.SCIENTIFIC),
            KeyCommand.KC_DEFAULT_FORMAT_TEXT => SetDefaultFormat(CellFormat.TEXT),
            KeyCommand.KC_DEFAULT_ALIGN_LEFT => SetDefaultAlignment(CellAlignment.LEFT),
            KeyCommand.KC_DEFAULT_ALIGN_RIGHT => SetDefaultAlignment(CellAlignment.RIGHT),
            KeyCommand.KC_DEFAULT_ALIGN_CENTRE => SetDefaultAlignment(CellAlignment.CENTRE),
            KeyCommand.KC_DEFAULT_ALIGN_GENERAL => SetDefaultAlignment(CellAlignment.GENERAL),
            KeyCommand.KC_SETTINGS_DECIMAL_POINTS => SetDefaultDecimalPoints(),
            KeyCommand.KC_SETTINGS_BACKUPS => ConfigureBackups(),
            KeyCommand.KC_NEXT_WINDOW => SelectWindow(1),
            KeyCommand.KC_QUIT => CloseWorkbook(),
            _ => _activeWindow.HandleCommand(command)
        };
        if (flags.HasFlag(RenderHint.CURSOR_STATUS)) {
            UpdateCursorPosition();
            flags &= ~RenderHint.CURSOR_STATUS;
        }
        if (flags.HasFlag(RenderHint.CONTENTS)) {
            _activeWindow.Refresh(flags);
            flags &= ~RenderHint.CONTENTS;
        }
        if (flags.HasFlag(RenderHint.REFRESH)) {
            Command.Refresh();
            Status.Refresh();
            _activeWindow.Refresh(flags);
            flags &= ~RenderHint.REFRESH;
        }
        return flags;
    }

    /// <summary>
    /// Show the editor version on the status bar.
    /// </summary>
    private static void Version() {
        Status.Message($"{AssemblySupport.AssemblyDescription} v{AssemblySupport.AssemblyVersion} - {AssemblySupport.AssemblyCopyright}");
    }

    /// <summary>
    /// Add a window to the window list. This will not make the window
    /// active.
    /// </summary>
    /// <param name="theWindow">Window to be added</param>
    private static void AddWindow(Window theWindow) {
        _windowList.Add(theWindow);
        theWindow.SetViewportBounds(0, 0, Terminal.Width, Terminal.Height);
    }

    /// <summary>
    /// Activate a window by its index
    /// </summary>
    /// <param name="index">Index of the window to be activated</param>
    private static void ActivateWindow(int index) {
        _activeWindow = _windowList[index];
        _activeWindow.Refresh(RenderHint.REFRESH);
    }

    /// <summary>
    /// Set or update the cell factory with the current cell defaults
    /// from the configuration.
    /// </summary>
    private static void SetCellFactory() {
        CellFactory.BackgroundColour = Colours.BackgroundColour;
        CellFactory.ForegroundColour = Colours.ForegroundColour;
        CellFactory.DecimalPlaces = Config.DefaultDecimals;
        CellFactory.Alignment = Config.DefaultCellAlignment;
        CellFactory.Format = Config.DefaultCellFormat;
    }

    /// <summary>
    /// Select the next window in the specified direction in the window list.
    /// </summary>
    /// <param name="direction">Direction</param>
    /// <returns>Render hint</returns>
    private static RenderHint SelectWindow(int direction) {
        if (_windowList.Count == 1 || _activeWindow == null) {
            return RenderHint.NONE;
        }
        int currentBufferIndex = _windowList.IndexOf(_activeWindow) + direction;
        if (currentBufferIndex == _windowList.Count) {
            currentBufferIndex = 0;
        }
        if (currentBufferIndex < 0) {
            currentBufferIndex = _windowList.Count - 1;
        }
        ActivateWindow(currentBufferIndex);
        return RenderHint.CURSOR_STATUS;
    }

    /// <summary>
    /// Show the command bar.
    /// </summary>
    /// <returns>Render hint</returns>
    private static RenderHint HandleCommandBar() {
        return Command.PromptForCommand(CommandMapID.MAIN);
    }

    /// <summary>
    /// Insert a new, blank, worksheet.
    /// </summary>
    /// <returns>Render hint</returns>
    private static RenderHint NewWorksheet() {
        Sheet sheet = _activeBook.AddSheet();
        Window newWindow = new Window(sheet);
        AddWindow(newWindow);
        _activeWindow = newWindow;
        _activeWindow.Refresh(RenderHint.REFRESH);
        return RenderHint.NONE;
    }

    /// <summary>
    /// Delete the current worksheet
    /// </summary>
    /// <returns>Render hint</returns>
    private static RenderHint DeleteWorksheet() {
        Debug.Assert(_activeWindow != null);
        if (_activeBook.Sheets.Count == 1) {
            Status.Message(Calc.DeleteWorksheetError);
            return RenderHint.NONE;
        }
        Sheet sheet = _activeWindow.Sheet;
        SelectWindow(1);
        _activeBook.RemoveSheet(sheet);
        _windowList.Remove(_activeWindow);
        return RenderHint.NONE;
    }

    /// <summary>
    /// Save the active workbook.
    /// </summary>
    /// <returns>Render hint</returns>
    private static RenderHint SaveFile() {
        RenderHint flags = RenderHint.CANCEL;
        FormField[] formFields = [
            new() {
                Text = Calc.EnterSaveFilename,
                Type = FormFieldType.TEXT,
                Width = 50,
                AllowFilenameCompletion = true,
                FilenameCompletionFilter = $"*{Book.DefaultExtension}",
                Value = new Variant(_activeBook.Name)
            }
        ];
        if (Command.PromptForInput(formFields)) {
            string inputValue = formFields[0].Value.StringValue;
            Debug.Assert(!string.IsNullOrEmpty(inputValue));
            inputValue = Utilities.AddExtensionIfMissing(inputValue, Book.DefaultExtension);
            _activeBook.Filename = inputValue;
            try {
                _activeBook.Write(Config.BackupFile);
            }
            catch (Exception e) {
                Status.Message(e.Message);
            }
            Status.UpdateFilename(_activeBook.Name);
            flags = RenderHint.NONE;
        }
        return flags;
    }

    /// <summary>
    /// Open a workbook.
    /// </summary>
    /// <returns>Render hint</returns>
    private static RenderHint OpenWorkbook() {
        RenderHint flags = RenderHint.CANCEL;
        if (CloseWorkbook() != RenderHint.CANCEL) {
            FormField[] formFields = [
                new() {
                    Text = Calc.EnterEditFilename,
                    Type = FormFieldType.TEXT,
                    Width = 50,
                    AllowFilenameCompletion = true,
                    FilenameCompletionFilter = $"*{Book.DefaultExtension}",
                    Value = new Variant(string.Empty)
                }
            ];
            if (Command.PromptForInput(formFields)) {
                string inputValue = formFields[0].Value.StringValue;
                Debug.Assert(!string.IsNullOrEmpty(inputValue));

                inputValue = Utilities.AddExtensionIfMissing(inputValue, Book.DefaultExtension);
                FileInfo fileInfo = new FileInfo(inputValue);
                inputValue = fileInfo.FullName;

                OpenBook(inputValue);
                flags = RenderHint.NONE;
            }
        }
        return flags;
    }

    /// <summary>
    /// Import a CSV file into a new sheet and add it to the sheet list.
    /// </summary>
    /// <returns>Render hint</returns>
    private static RenderHint ImportFile() {
        RenderHint flags = RenderHint.CANCEL;
        FormField[] formFields = [
            new() {
                Text = Calc.EnterImportFilename,
                Type = FormFieldType.TEXT,
                Width = 50,
                AllowFilenameCompletion = true,
                FilenameCompletionFilter = $"*{Consts.CSVExtension}",
                Value = new Variant(string.Empty)
            }
        ];
        if (Command.PromptForInput(formFields)) {
            string inputValue = formFields[0].Value.StringValue;
            Debug.Assert(!string.IsNullOrEmpty(inputValue));

            inputValue = Utilities.AddExtensionIfMissing(inputValue, Consts.CSVExtension);
            FileInfo fileInfo = new FileInfo(inputValue);
            inputValue = fileInfo.FullName;

            Sheet sheet = _activeBook.AddSheet();
            try {
                using TextReader stream = new StreamReader(inputValue);
                using CsvParser parser = new CsvParser(stream, CultureInfo.InvariantCulture);

                int row = 1;
                while (parser.Read()) {
                    string[]? fields = parser.Record;
                    if (fields != null) {
                        for (int column = 1; column <= fields.Length; column++) {
                            Cell cell = sheet.GetCell(new CellLocation { Column = column, Row = row }, true);
                            cell.Content = fields[column - 1];
                        }
                    }
                    row++;
                }
            }
            catch (CsvHelperException) {
                Status.Message(string.Format(Calc.BadCSVImportFile, fileInfo.Name));
                return RenderHint.NONE;
            }
            catch (FileNotFoundException) {
                Status.Message(string.Format(Calc.CannotOpenFile, fileInfo.Name));
                return RenderHint.NONE;
            }
            Window newWindow = new Window(sheet);
            AddWindow(newWindow);
            _activeWindow = newWindow;
            _activeWindow.Refresh(RenderHint.REFRESH);
            flags = RenderHint.NONE;
        }
        return flags;
    }

    /// <summary>
    /// Set the default cell format.
    /// </summary>
    /// <param name="format">Format to be set as default</param>
    /// <returns>Render hint</returns>
    private static RenderHint SetDefaultFormat(CellFormat format) {
        Config.DefaultCellFormat = format;
        Config.Save();
        SetCellFactory();
        return RenderHint.CONTENTS;
    }

    /// <summary>
    /// Set the default cell alignment.
    /// </summary>
    /// <param name="alignment">Alignment to be set as default</param>
    /// <returns>Render hint</returns>
    private static RenderHint SetDefaultAlignment(CellAlignment alignment) {
        Config.DefaultCellAlignment = alignment;
        Config.Save();
        SetCellFactory();
        return RenderHint.CONTENTS;
    }

    /// <summary>
    /// Set the default number of decimal points.
    /// </summary>
    /// <returns>Render hint</returns>
    private static RenderHint SetDefaultDecimalPoints() {
        FormField[] formFields = [
            new() {
                Text = Calc.EnterDecimalPlaces,
                Type = FormFieldType.NUMBER,
                Width = 2,
                MinimumValue = 0,
                MaximumValue = 15,
                Value = new Variant(Config.DefaultDecimals)
            }
        ];
        if (!Command.PromptForInput(formFields)) {
            return RenderHint.CANCEL;
        }
        int decimalPlaces = formFields[0].Value.IntValue;
        Debug.Assert(decimalPlaces >= formFields[0].MinimumValue && decimalPlaces <= formFields[0].MaximumValue);
        Config.DefaultDecimals = decimalPlaces;
        SetCellFactory();
        return RenderHint.CONTENTS;
    }

    /// <summary>
    /// Configure whether or not to create a backup file when a sheet is saved.
    /// </summary>
    /// <returns>Render hint</returns>
    private static RenderHint ConfigureBackups() {
        char[] validInput = ['y', 'n'];
        if (!Command.Prompt(Calc.CreateBackupFilePrompt, validInput, out char inputChar)) {
            return RenderHint.CANCEL;
        }
        Config.BackupFile = inputChar == 'y';
        Config.Save();
        return RenderHint.NONE;
    }

    /// <summary>
    /// Allow configuring the calc screen colours
    /// </summary>
    /// <returns>Render hint</returns>
    private static RenderHint ConfigureColours() {
        int backgroundColour = Colours.BackgroundColour;
        int foregroundColour = Colours.ForegroundColour;
        int normalMessageColour = Colours.NormalMessageColour;
        int selectionMessageColour = Colours.SelectionColour;
        if (!GetColourInput(Calc.EnterBackgroundColour, ref backgroundColour)) {
            return RenderHint.NONE;
        }
        if (!GetColourInput(Calc.EnterForegroundColour, ref foregroundColour)) {
            return RenderHint.NONE;
        }
        if (foregroundColour == backgroundColour) {
            Status.Message(Calc.BackgroundColourError);
            return RenderHint.NONE;
        }
        if (!GetColourInput(Calc.EnterMessageColour, ref normalMessageColour)) {
            return RenderHint.NONE;
        }
        if (!GetColourInput(Calc.EnterSelectionColour, ref selectionMessageColour)) {
            return RenderHint.NONE;
        }
        Config.BackgroundColour = backgroundColour;
        Config.ForegroundColour = foregroundColour;
        Config.NormalMessageColour = normalMessageColour;
        Config.SelectionColour = selectionMessageColour;
        Config.Save();
        SetCellFactory();
        return RenderHint.REFRESH;
    }

    /// <summary>
    /// Input a colour index as part of the colour command.
    /// </summary>
    /// <param name="prompt">Prompt to display</param>
    /// <param name="colourValue">Output value</param>
    /// <returns>True if the output value is valid, false otherwise.</returns>
    public static bool GetColourInput(string prompt, ref int colourValue) {
        FormField[] formFields = [
            new() {
                Text = prompt,
                Value = new Variant(colourValue),
                Width = 2
            }
        ];
        if (!Command.PromptForInput(formFields)) {
            colourValue = 0;
            return false;
        }
        colourValue = formFields[0].Value.IntValue;
        if (colourValue is (< 30 or > 37) and (< 90 or > 97)) {
            Status.Message(string.Format(Calc.InvalidColourIndex));
            return false;
        }
        return true;
    }

    /// <summary>
    /// Close the existing workbook and offer to save any changes. Returns
    /// RenderHint.EXIT if the workbook can be closed, or RenderHint.CANCEL
    /// if the user choose to cancel the operation.
    /// </summary>
    /// <returns>Render hint</returns>
    private static RenderHint CloseWorkbook() {
        RenderHint flags = RenderHint.EXIT;
        if (_activeBook.Modified) {
            char[] validInput = ['y', 'n'];
            if (!Command.Prompt(Calc.QuitPrompt, validInput, out char inputChar)) {
                flags = RenderHint.CANCEL;
            }
            else if (inputChar == 'y') {
                try {
                    _activeBook.Write(Config.BackupFile);
                }
                catch (Exception e) {
                    Status.Message(e.Message);
                    return RenderHint.CANCEL;
                }
            }
        }
        return flags;
    }
}