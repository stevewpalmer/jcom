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

using JCalc.Resources;
using JCalcLib;
using JComLib;

namespace JCalc;

public static class Screen {
    private static readonly List<Window> _windowList = [];
    private static Window? _activeWindow;

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

        Config = Config.Load();
        Colours = new Colours(Config);

        Command.Refresh();
        Status.Refresh();
    }

    /// <summary>
    /// Close the main screen when calc is closed.
    /// </summary>
    public static void Close() {
        Config.Save();
        Terminal.Close();
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
            Cell cell = _activeWindow.ActiveCell;
            Command.UpdateCellStatus(cell);
        }
    }

    /// <summary>
    /// Add a window to the window list. This will not make the window
    /// active.
    /// </summary>
    public static void AddWindow(Window theWindow) {
        _windowList.Add(theWindow);
        theWindow.SetViewportBounds(0, 0, Terminal.Width, Terminal.Height);
    }

    /// <summary>
    /// Activate a window by its index
    /// </summary>
    /// <param name="index">Index of the window to be activated</param>
    public static void ActivateWindow(int index) {
        _activeWindow = _windowList[index];
        _activeWindow.Refresh();
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
        RenderHint flags = command switch {
            KeyCommand.KC_COMMAND_BAR => HandleCommandBar(),
            KeyCommand.KC_FILE_RETRIEVE => RetrieveFile(),
            KeyCommand.KC_SETTINGS_COLOURS => ConfigureColours(),
            KeyCommand.KC_DEFAULT_DATE_DM => SetDefaultFormat(CellFormat.DATE_DM),
            KeyCommand.KC_DEFAULT_DATE_DMY => SetDefaultFormat(CellFormat.DATE_DMY),
            KeyCommand.KC_DEFAULT_DATE_MY => SetDefaultFormat(CellFormat.DATE_MY),
            KeyCommand.KC_DEFAULT_FORMAT_BAR => SetDefaultFormat(CellFormat.BAR),
            KeyCommand.KC_DEFAULT_FORMAT_COMMAS => SetDefaultFormat(CellFormat.COMMAS),
            KeyCommand.KC_DEFAULT_FORMAT_CURRENCY => SetDefaultFormat(CellFormat.CURRENCY),
            KeyCommand.KC_DEFAULT_FORMAT_FIXED => SetDefaultFormat(CellFormat.FIXED),
            KeyCommand.KC_DEFAULT_FORMAT_GENERAL => SetDefaultFormat(CellFormat.GENERAL),
            KeyCommand.KC_DEFAULT_FORMAT_PERCENT => SetDefaultFormat(CellFormat.PERCENT),
            KeyCommand.KC_DEFAULT_FORMAT_SCI => SetDefaultFormat(CellFormat.SCIENTIFIC),
            KeyCommand.KC_DEFAULT_FORMAT_TEXT => SetDefaultFormat(CellFormat.TEXT),
            KeyCommand.KC_DEFAULT_ALIGN_LEFT => SetDefaultAlignment(CellAlignment.LEFT),
            KeyCommand.KC_DEFAULT_ALIGN_RIGHT => SetDefaultAlignment(CellAlignment.RIGHT),
            KeyCommand.KC_DEFAULT_ALIGN_CENTRE => SetDefaultAlignment(CellAlignment.CENTRE),
            KeyCommand.KC_SETTINGS_DECIMAL_POINTS => SetDefaultDecimalPoints(),
            KeyCommand.KC_SETTINGS_BACKUPS => ConfigureBackups(),
            KeyCommand.KC_QUIT => Exit(),
            _ => _activeWindow.HandleCommand(command)
        };
        if (flags.HasFlag(RenderHint.CURSOR_STATUS)) {
            UpdateCursorPosition();
            flags &= ~RenderHint.CURSOR_STATUS;
        }
        if (flags.HasFlag(RenderHint.REFRESH)) {
            Command.Refresh();
            _activeWindow.Refresh();
            flags &= ~RenderHint.REFRESH;
        }
        return flags;
    }

    /// <summary>
    /// Show the command bar.
    /// </summary>
    /// <returns>Render hint</returns>
    private static RenderHint HandleCommandBar() {
        return Command.PromptForCommand(CommandMapID.MAIN);
    }

    /// <summary>
    /// Retrieve a file and add it to the sheet list.
    /// </summary>
    /// <returns>Render hint</returns>
    private static RenderHint RetrieveFile() {
        RenderHint flags = RenderHint.CANCEL;
        FormField[] formFields = [
            new() {
                Text = Calc.EnterEditFilename,
                Type = FormFieldType.TEXT,
                Width = 50,
                AllowFilenameCompletion = true,
                FilenameCompletionFilter = $"*{Consts.DefaultExtension}",
                Value = new Variant(string.Empty)
            }
        ];
        if (Command.PromptForInput(formFields)) {
            string inputValue = formFields[0].Value.StringValue;

            inputValue = Utilities.AddExtensionIfMissing(inputValue, Consts.DefaultExtension);
            FileInfo fileInfo = new FileInfo(inputValue);
            inputValue = fileInfo.FullName;

            Window? newWindow = _windowList.FirstOrDefault(window => window.Sheet.Filename == inputValue);
            if (newWindow == null) {
                int sheetNumber = 1;
                foreach (Window _ in _windowList.TakeWhile(window => window.Sheet.SheetNumber == sheetNumber)) {
                    ++sheetNumber;
                }
                Sheet sheet = new Sheet(sheetNumber, inputValue);
                newWindow = new Window(sheet);
                AddWindow(newWindow);
            }
            _activeWindow = newWindow;
            _activeWindow.Refresh();
            flags = RenderHint.NONE;
        }
        return flags;
    }

    /// <summary>
    /// Set the default cell format.
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    private static RenderHint SetDefaultFormat(CellFormat format) {
        Config.DefaultCellFormat = format;
        Config.Save();
        return RenderHint.NONE;
    }


    /// <summary>
    /// Set the default cell alignment.
    /// </summary>
    /// <param name="alignment"></param>
    /// <returns></returns>
    private static RenderHint SetDefaultAlignment(CellAlignment alignment) {
        Config.DefaultCellAlignment = alignment;
        Config.Save();
        return RenderHint.NONE;
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
                Value = new Variant(Config.DefaultDecimals)
            }
        ];
        if (!Command.PromptForInput(formFields)) {
            return RenderHint.CANCEL;
        }
        Config.DefaultDecimals = Math.Min(formFields[0].Value.IntValue, 15);
        return RenderHint.NONE;
    }

    /// <summary>
    /// Configure whether or not to create a backup file when a sheet is saved.
    /// </summary>
    /// <returns>Render hint</returns>
    private static RenderHint ConfigureBackups() {
        char[] validInput = ['y', 'n'];
        if (!Command.Prompt("Create backup files when saving? @@", validInput, out char inputChar)) {
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
        int backgroundColour = int.TryParse(Config.BackgroundColour, out int _bgColour) ? _bgColour : 0;
        int foregroundColour = int.TryParse(Config.ForegroundColour, out int _fgColour) ? _fgColour : 7;
        int normalMessageColour = int.TryParse(Config.NormalMessageColour, out int _nmColour) ? _nmColour : 3;
        if (!GetColourInput("Enter background colour number", ref backgroundColour)) {
            return RenderHint.NONE;
        }
        if (!GetColourInput("Enter foreground colour number", ref foregroundColour)) {
            return RenderHint.NONE;
        }
        if (foregroundColour == backgroundColour) {
            Command.Error("Background colour is already set to foreground colour.");
            return RenderHint.NONE;
        }
        if (!GetColourInput("Enter message colour number", ref normalMessageColour)) {
            return RenderHint.NONE;
        }
        Config.BackgroundColour = backgroundColour.ToString();
        Config.ForegroundColour = foregroundColour.ToString();
        Config.NormalMessageColour = normalMessageColour.ToString();
        Config.Save();
        return RenderHint.REFRESH;
    }

    /// <summary>
    /// Input a colour index as part of the colour command.
    /// </summary>
    /// <param name="prompt">Prompt to display</param>
    /// <param name="colourValue">Output value</param>
    /// <returns>True if the output value is valid, false otherwise.</returns>
    private static bool GetColourInput(string prompt, ref int colourValue) {
        FormField [] formFields = [
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
        if (colourValue < 0 || colourValue > Colours.MaxColourIndex) {
            Command.Error(string.Format(Calc.InvalidColourIndex, Colours.MaxColourIndex));
            return false;
        }
        return true;
    }

    /// <summary>
    /// Exit the editor, saving any buffers if required. If prompt is
    /// TRUE, we prompt whether to save or exit without saving. If prompt
    /// is FALSE, we just save all modified buffers and exit.
    /// </summary>
    /// <returns>Render hint</returns>
    private static RenderHint Exit() {
        RenderHint flags = RenderHint.EXIT;
        Sheet[] modifiedSheets = _windowList.Where(w => w.Sheet.Modified).Select(b => b.Sheet).ToArray();
        if (modifiedSheets.Length != 0) {
            char[] validInput = ['y', 'n'];
            if (!Command.Prompt(Calc.QuitPrompt, validInput, out char inputChar)) {
                flags = RenderHint.CANCEL;
            }
            else if (inputChar == 'y') {
                foreach (Sheet sheet in modifiedSheets) {
                    sheet.Write();
                }
            }
        }
        return flags;
    }
}