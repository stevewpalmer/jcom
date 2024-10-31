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
    /// Open the main window.
    /// </summary>
    public static void Open() {
        Terminal.Open();

        Config = Config.Load();
        Colours = new Colours(Config);

        Command.Refresh();
    }

    /// <summary>
    /// Close the main screen when calc is closed.
    /// </summary>
    public static void Close() {
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
    private static RenderHint HandleCommand(KeyCommand command) {
        if (_activeWindow == null) {
            throw new InvalidOperationException();
        }
        RenderHint flags = command switch {
            KeyCommand.KC_QUIT => Exit(true),
            KeyCommand.KC_GOTO => GotoCommand(),
            KeyCommand.KC_FORMAT => FormatCommand(),
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
    /// Handle the Goto command
    /// </summary>
    /// <returns>Render hint</returns>
    private static RenderHint GotoCommand() {
        Command.SetActiveCommandMap(CommandMapID.GOTO);
        return RenderHint.NONE;
    }

    /// <summary>
    /// Handle the Format command
    /// </summary>
    /// <returns>Render hint</returns>
    private static RenderHint FormatCommand() {
        Command.SetActiveCommandMap(CommandMapID.FORMAT);
        return RenderHint.NONE;
    }

    /// <summary>
    /// Exit the editor, saving any buffers if required. If prompt is
    /// TRUE, we prompt whether to save or exit without saving. If prompt
    /// is FALSE, we just save all modified buffers and exit.
    /// </summary>
    /// <param name="prompt">True to display a confirmation prompt, false to just save and exit</param>
    /// <returns>Render hint</returns>
    private static RenderHint Exit(bool prompt) {
        RenderHint flags = RenderHint.EXIT;
        bool writeSheets = !prompt;
        Sheet[] modifiedSheets = _windowList.Where(w => w.Sheet.Modified).Select(b => b.Sheet).ToArray();
        if (prompt) {
            if (modifiedSheets.Length != 0) {
                char[] validInput = ['y', 'n'];
                if (!Command.Prompt(Calc.Quit, Calc.QuitPrompt, validInput, out char inputChar)) {
                    flags = RenderHint.NONE;
                }
                else {
                    writeSheets = inputChar switch {
                        'y' => true,
                        _ => writeSheets
                    };
                }
            }
        }
        if (writeSheets) {
            foreach (Sheet buffer in modifiedSheets) {
                buffer.Write();
            }
        }
        return flags;
    }
}