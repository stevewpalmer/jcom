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
    /// Render the current workbook.
    /// </summary>
    private static void RenderBook() {
        _windowList.Clear();
        foreach (Sheet sheet in _activeBook.Sheets) {
            AddWindow(new Window(sheet));
        }
        ActivateWindow(0);
    }

    /// <summary>
    /// Initialise the workbook from the specified file.
    /// </summary>
    /// <param name="filename">Workbook filename</param>
    /// <param name="debug">Optional flag to enable debug mode</param>
    public static void OpenBook(string filename, bool debug = false) {
        _activeBook.Debug = debug;
        if (!string.IsNullOrEmpty(filename)) {
            RenderBook();
            Status.Message($"Loading {filename}...");
            try {
                _activeBook.Open(filename);
                Status.ClearMessage();
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
        RenderBook();
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
            KeyCommand.KC_FILE_NEW => NewWorkbook(),
            KeyCommand.KC_FILE_RETRIEVE => OpenWorkbook(),
            KeyCommand.KC_FILE_IMPORT => ImportFile(),
            KeyCommand.KC_FILE_SAVE => SaveFile(),
            KeyCommand.KC_SHEET_RENAME => RenameWorksheet(),
            KeyCommand.KC_SETTINGS_FGCOLOUR => SetForegroundColour(),
            KeyCommand.KC_SETTINGS_BGCOLOUR => SetBackgroundColour(),
            KeyCommand.KC_SETTINGS_MESSAGES => SetMessageColour(),
            KeyCommand.KC_SETTINGS_SELECTION => SetSelectionColour(),
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
        theWindow.SetViewportBounds(0, CommandBar.Height, Terminal.Width, Terminal.Height - (CommandBar.Height + StatusBar.Height));
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
        CellFactory.TextColour = Colours.ForegroundColour;
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
        Window newWindow = new(sheet);
        AddWindow(newWindow);
        _activeWindow = newWindow;
        _activeWindow.Refresh(RenderHint.REFRESH);
        return RenderHint.NONE;
    }

    /// <summary>
    /// Rename the current worksheet
    /// </summary>
    /// <returns>Render hint</returns>
    private static RenderHint RenameWorksheet() {
        Debug.Assert(_activeWindow != null);
        FormField[] formFields = [
            new() {
                Text = Calc.EnterWorksheetName,
                Type = FormFieldType.TEXT,
                Width = 0,
                Value = new Variant(_activeWindow.Sheet.Name)
            }
        ];
        if (Command.PromptForInput(formFields)) {
            _activeWindow.Sheet.Name = formFields[0].Value.ToString();
            Command.Refresh();
        }
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
        _windowList.Remove(_activeWindow);
        _activeBook.RemoveSheet(sheet);
        SelectWindow(1);
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
                Width = 0,
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
    /// Create a new blank workbook.
    /// </summary>
    /// <returns>Render hint</returns>
    private static RenderHint NewWorkbook() {
        if (CloseWorkbook() != RenderHint.CANCEL) {
            _activeBook.New();
            OpenBook(string.Empty);
        }
        return RenderHint.NONE;
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
                    Width = 0,
                    AllowFilenameCompletion = true,
                    FilenameCompletionFilter = $"*{Book.DefaultExtension}",
                    Value = new Variant(string.Empty)
                }
            ];
            if (Command.PromptForInput(formFields)) {
                string inputValue = formFields[0].Value.StringValue;
                Debug.Assert(!string.IsNullOrEmpty(inputValue));

                inputValue = Utilities.AddExtensionIfMissing(inputValue, Book.DefaultExtension);
                FileInfo fileInfo = new(inputValue);
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
                Width = 0,
                AllowFilenameCompletion = true,
                FilenameCompletionFilter = $"*{Consts.CSVExtension}",
                Value = new Variant(string.Empty)
            }
        ];
        if (Command.PromptForInput(formFields)) {
            string inputValue = formFields[0].Value.StringValue;
            Debug.Assert(!string.IsNullOrEmpty(inputValue));

            inputValue = Utilities.AddExtensionIfMissing(inputValue, Consts.CSVExtension);
            FileInfo fileInfo = new(inputValue);
            inputValue = fileInfo.FullName;

            Sheet sheet = _activeBook.AddSheet();
            bool truncated = false;
            try {
                using StreamReader stream = new(inputValue);
                using CsvParser parser = new(stream, CultureInfo.InvariantCulture);

                int row = 1;
                long fileSize = stream.BaseStream.Length;
                int lastPercent = -1;
                while (parser.Read()) {
                    if (row > Sheet.MaxRows) {
                        truncated = true;
                        break;
                    }
                    string[]? fields = parser.Record;
                    if (fields != null) {
                        if (fields.Length > Sheet.MaxColumns) {
                            truncated = true;
                        }
                        int maxColumn = Math.Min(Sheet.MaxColumns, fields.Length);
                        for (int column = 1; column <= maxColumn; column++) {
                            Cell cell = sheet.GetCell(column, row, true);
                            cell.Content = fields[column - 1];
                        }
                    }
                    int percent = Convert.ToInt32(parser.CharCount / (double)fileSize * 100.0d);
                    if (percent != lastPercent) {
                        if (percent % 5 == 0) {
                            Status.Message(string.Format(Calc.ImportProgress, percent));
                        }
                        lastPercent = percent;
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
            Status.ClearMessage();
            if (truncated) {
                Status.Message(Calc.TruncatedCSVImport);
            }
            Window newWindow = new(sheet);
            AddWindow(newWindow);
            _activeWindow = newWindow;
            _activeWindow.Refresh(RenderHint.REFRESH);
            flags = RenderHint.NONE;
        }
        return flags;
    }

    /// <summary>
    /// Display the colour picker for the screen foreground colour.
    /// </summary>
    /// <returns>Render hint</returns>
    private static RenderHint SetForegroundColour() {
        int cellColour = Colours.ForegroundColour;
        if (!GetColourInput(Calc.ScreenTextColour, ref cellColour)) {
            return RenderHint.NONE;
        }
        Config.ForegroundColour = cellColour;
        Config.Save();
        SetCellFactory();
        return RenderHint.REFRESH;
    }

    /// <summary>
    /// Display the colour picker for the screen background colour.
    /// </summary>
    /// <returns>Render hint</returns>
    private static RenderHint SetBackgroundColour() {
        int cellColour = Colours.BackgroundColour;
        if (!GetColourInput(Calc.ScreenBackgroundColour, ref cellColour)) {
            return RenderHint.NONE;
        }
        Config.BackgroundColour = cellColour;
        Config.Save();
        SetCellFactory();
        return RenderHint.REFRESH;
    }

    /// <summary>
    /// Display the colour picker for the messages colour.
    /// </summary>
    /// <returns>Render hint</returns>
    private static RenderHint SetMessageColour() {
        int cellColour = Colours.NormalMessageColour;
        if (!GetColourInput(Calc.MessageColour, ref cellColour)) {
            return RenderHint.NONE;
        }
        Config.NormalMessageColour = cellColour;
        Config.Save();
        SetCellFactory();
        return RenderHint.REFRESH;
    }

    /// <summary>
    /// Display the colour picker for the selection colour.
    /// </summary>
    /// <returns>Render hint</returns>
    private static RenderHint SetSelectionColour() {
        int cellColour = Colours.SelectionColour;
        if (!GetColourInput(Calc.SelectionColour, ref cellColour)) {
            return RenderHint.NONE;
        }
        Config.SelectionColour = cellColour;
        Config.Save();
        SetCellFactory();
        return RenderHint.REFRESH;
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
                MaximumValue = Sheet.MaxColumns,
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
    /// Input a colour index as part of the colour command.
    /// </summary>
    /// <param name="prompt">Prompt to display</param>
    /// <param name="colourValue">Output value</param>
    /// <returns>True if the output value is valid, false otherwise.</returns>
    public static bool GetColourInput(string prompt, ref int colourValue) {
        FormField[] formFields = [
            new() {
                Text = prompt,
                Type = FormFieldType.COLOURPICKER,
                Value = new Variant(colourValue)
            }
        ];
        if (!Command.PromptForInput(formFields)) {
            colourValue = 0;
            return false;
        }
        colourValue = formFields[0].Value.IntValue;
        Debug.Assert(colourValue >= 0);
        return true;
    }

    /// <summary>
    /// Close the existing workbook and offer to save any changes. Returns
    /// RenderHint EXIT if the workbook can be closed, or RenderHint CANCEL
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