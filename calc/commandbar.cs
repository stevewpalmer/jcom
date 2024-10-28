// JCalc
// Command bar
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

using System.Drawing;
using JCalc.Resources;
using JComLib;

namespace JCalc;

/// <summary>
/// List of command IDs
/// </summary>
public enum KeyCommand {
    KC_NONE,
    KC_ALPHA,
    KC_BLANK,
    KC_COPY,
    KC_DELETE,
    KC_EDIT,
    KC_FORMAT,
    KC_GOTO,
    KC_HELP,
    KC_INSERT,
    KC_LOCK,
    KC_MOVE,
    KC_NAME,
    KC_OPTIONS,
    KC_PRINT,
    KC_QUIT,
    KC_SORT,
    KC_TRANSFER,
    KC_VALUE,
    KC_WINDOW,
    KC_XTERNAL,
    KC_LEFT,
    KC_RIGHT,
    KC_UP,
    KC_DOWN
}

/// <summary>
/// Mapping of command names and their IDs
/// </summary>
public class KeyCommands {

    /// <summary>
    /// Command name
    /// </summary>
    public string Name { get; init; } = "";

    /// <summary>
    /// Associated command ID
    /// </summary>
    public KeyCommand CommandId { get; init; }
}

/// <summary>
/// Map a console key to a command
/// </summary>
public class KeyMap {

    /// <summary>
    /// Console keystroke
    /// </summary>
    public ConsoleKey Key { get; init; }

    /// <summary>
    /// Associated command ID
    /// </summary>
    public KeyCommand CommandId { get; init; }
}

public class CommandBar {
    private readonly int _commandBarRow;
    private readonly int _statusRow;
    private readonly int _promptRow;
    private ConsoleColor _bgColour;
    private ConsoleColor _fgColour;
    private readonly int _displayWidth;
    private readonly int _cursorPositionWidth;
    private int _cursorRow = 1;
    private int _cursorColumn = 1;
    private string _filename;
    private int _selectedCommand = 0;

    private static readonly KeyMap[] KeyMap = [
        new() { Key = ConsoleKey.LeftArrow, CommandId = KeyCommand.KC_LEFT },
        new() { Key = ConsoleKey.RightArrow, CommandId = KeyCommand.KC_RIGHT },
        new() { Key = ConsoleKey.UpArrow, CommandId = KeyCommand.KC_UP },
        new() { Key = ConsoleKey.DownArrow, CommandId = KeyCommand.KC_DOWN },
        new() { Key = ConsoleKey.A, CommandId = KeyCommand.KC_ALPHA },
        new() { Key = ConsoleKey.B, CommandId = KeyCommand.KC_BLANK },
        new() { Key = ConsoleKey.C, CommandId = KeyCommand.KC_COPY },
        new() { Key = ConsoleKey.D, CommandId = KeyCommand.KC_DELETE },
        new() { Key = ConsoleKey.E, CommandId = KeyCommand.KC_EDIT },
        new() { Key = ConsoleKey.F, CommandId = KeyCommand.KC_FORMAT },
        new() { Key = ConsoleKey.G, CommandId = KeyCommand.KC_GOTO },
        new() { Key = ConsoleKey.H, CommandId = KeyCommand.KC_HELP },
        new() { Key = ConsoleKey.I, CommandId = KeyCommand.KC_INSERT },
        new() { Key = ConsoleKey.L, CommandId = KeyCommand.KC_LOCK },
        new() { Key = ConsoleKey.M, CommandId = KeyCommand.KC_MOVE },
        new() { Key = ConsoleKey.N, CommandId = KeyCommand.KC_NAME },
        new() { Key = ConsoleKey.O, CommandId = KeyCommand.KC_OPTIONS },
        new() { Key = ConsoleKey.P, CommandId = KeyCommand.KC_PRINT },
        new() { Key = ConsoleKey.Q, CommandId = KeyCommand.KC_QUIT },
        new() { Key = ConsoleKey.S, CommandId = KeyCommand.KC_SORT },
        new() { Key = ConsoleKey.T, CommandId = KeyCommand.KC_TRANSFER },
        new() { Key = ConsoleKey.V, CommandId = KeyCommand.KC_VALUE },
        new() { Key = ConsoleKey.W, CommandId = KeyCommand.KC_WINDOW },
        new() { Key = ConsoleKey.X, CommandId = KeyCommand.KC_XTERNAL }
    ];

    private static readonly KeyCommands[] CommandTable = [
        new() { Name = "Alpha", CommandId = KeyCommand.KC_ALPHA },
        new() { Name = "Blank", CommandId = KeyCommand.KC_BLANK },
        new() { Name = "Copy", CommandId = KeyCommand.KC_COPY },
        new() { Name = "Delete", CommandId = KeyCommand.KC_DELETE },
        new() { Name = "Edit", CommandId = KeyCommand.KC_EDIT },
        new() { Name = "Format", CommandId = KeyCommand.KC_FORMAT },
        new() { Name = "Goto", CommandId = KeyCommand.KC_GOTO },
        new() { Name = "Help", CommandId = KeyCommand.KC_HELP },
        new() { Name = "Insert", CommandId = KeyCommand.KC_INSERT },
        new() { Name = "Lock", CommandId = KeyCommand.KC_LOCK },
        new() { Name = "Move", CommandId = KeyCommand.KC_MOVE },
        new() { Name = "Name", CommandId = KeyCommand.KC_NAME },
        new() { Name = "Options", CommandId = KeyCommand.KC_OPTIONS },
        new() { Name = "Print", CommandId = KeyCommand.KC_PRINT },
        new() { Name = "Quit", CommandId = KeyCommand.KC_QUIT },
        new() { Name = "Sort", CommandId = KeyCommand.KC_SORT },
        new() { Name = "Transfer", CommandId = KeyCommand.KC_TRANSFER },
        new() { Name = "Value", CommandId = KeyCommand.KC_VALUE },
        new() { Name = "Window", CommandId = KeyCommand.KC_WINDOW },
        new() { Name = "Xternal", CommandId = KeyCommand.KC_XTERNAL }
    ];

    /// <summary>
    /// Construct a command bar object
    /// </summary>
    public CommandBar() {
        _commandBarRow = Terminal.Height - Height;
        _displayWidth = Terminal.Width;
        _statusRow = Terminal.Height - 1;
        _promptRow = _commandBarRow + 2;
        _cursorPositionWidth = 5;
        _filename = "TEMP";
    }

    /// <summary>
    /// Command bar height
    /// </summary>
    public const int Height = 4;

    /// <summary>
    /// Update the cursor position indicator on the status bar
    /// </summary>
    public void UpdateCursorPosition(int line, int column) {
        _cursorRow = line;
        _cursorColumn = column;
        RenderCursorPosition();
    }

    /// <summary>
    /// Update the cursor position indicator on the status bar
    /// </summary>
    public void UpdateFilename(string newFilename) {
        _filename = newFilename;
        RenderSheetFilename();
    }

    /// <summary>
    /// Refresh the status line
    /// </summary>
    public void Refresh() {
        _fgColour = Screen.Colours.NormalMessageColour;
        _bgColour = Screen.Colours.BackgroundColour;
        RenderCommandList();
        RenderCursorPosition();
        RenderSheetFilename();
    }

    /// <summary>
    /// Render the list of commands
    /// </summary>
    private void RenderCommandList() {
        string commandPrompt = Calc.CommandPrompt;
        int commandPromptLength = commandPrompt.Length;
        int row = _commandBarRow;
        int column = commandPrompt.Length;
        RenderText(0, row, commandPromptLength, commandPrompt, _fgColour, _bgColour);
        for (int i = 0; i < CommandTable.Length; i++) {
            KeyCommands command = CommandTable[i];
            if (column + command.Name.Length >= _displayWidth) {
                row++;
                column = commandPrompt.Length;
            }
            ConsoleColor fgColour = i == _selectedCommand ? _bgColour : _fgColour;
            ConsoleColor bgColour = i == _selectedCommand ? _fgColour : _bgColour;
            RenderText(column, row, command.Name.Length, command.Name, fgColour, bgColour);
            column += command.Name.Length + 1;
        }
        RenderText(0, _promptRow, _displayWidth, Calc.SelectOptionPrompt, _fgColour, _bgColour);
    }

    /// <summary>
    /// Show the current selected row and column position
    /// </summary>
    private void RenderCursorPosition() {
        string text = $"R{_cursorRow}C{_cursorColumn}";
        RenderText(0, _statusRow, _cursorPositionWidth, text, _fgColour, _bgColour);
    }

    private void RenderSheetFilename() {
        string text = $@"Calc: {_filename}";
        RenderText(_displayWidth - text.Length - 4, _statusRow, text.Length, text, _fgColour, _bgColour);
    }

    /// <summary>
    /// Map a keystroke to a command
    /// </summary>
    public static KeyCommand MapKeyToCommand(ConsoleKeyInfo keyIn) {
        foreach (KeyMap command in KeyMap) {
            if (command.Key == keyIn.Key) {
                return command.CommandId;
            }
        }
        return KeyCommand.KC_NONE;
    }

    /// <summary>
    /// Display a prompt on the status bar and prompt for a keystroke input.
    /// </summary>
    /// <returns>True if a value was input, false if empty or cancelled</returns>
    public bool Prompt(string prompt, char[] validInput, out char inputValue) {

        Point cursorPosition = Terminal.GetCursor();
        prompt = prompt.Replace("@@", $"[{string.Join("", validInput)}]");
        RenderText(0, _promptRow, _displayWidth, prompt, _fgColour, _bgColour);
        Terminal.SetCursor(prompt.Length, _promptRow);
        ConsoleKeyInfo input = Console.ReadKey(true);
        while (!validInput.Contains(char.ToLower(input.KeyChar))) {
            if (input.Key is ConsoleKey.Escape) {
                break;
            }
            input = Console.ReadKey(true);
        }
        Terminal.SetCursor(cursorPosition);
        inputValue = char.ToLower(input.KeyChar);
        return input.Key != ConsoleKey.Escape;
    }

    /// <summary>
    /// Write text to the command bar in the specified colour.
    /// </summary>
    private static void RenderText(int x, int y, int w, string text, ConsoleColor fgColor, ConsoleColor bgColour) {
        Point saved = Terminal.GetCursor();
        Terminal.Write(x, y, w, bgColour, fgColor, Utilities.SpanBound(text, 0, w));
        Terminal.SetCursor(saved);
    }
}