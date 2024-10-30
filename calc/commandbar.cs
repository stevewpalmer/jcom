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

using System.Diagnostics;
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
    KC_DOWN,
    KC_HOME,
    KC_PAGEUP,
    KC_PAGEDOWN,
    KC_GOTO_MACRO,
    KC_GOTO_WINDOW,
    KC_GOTO_ROWCOL,
    KC_GOTO_NAME
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

public enum CellInputFlags {

    /// <summary>
    /// Alpha input
    /// </summary>
    ALPHA = 1,

    /// <summary>
    /// Value input
    /// </summary>
    VALUE = 2,

    /// <summary>
    /// Both alpha and value input
    /// </summary>
    ALPHAVALUE = ALPHA + VALUE
}

public enum CellInputResponse {

    /// <summary>
    /// None
    /// </summary>
    NONE,

    /// <summary>
    /// Cancel the input
    /// </summary>
    CANCEL,

    /// <summary>
    /// Accept the input and remain on the current cell
    /// </summary>
    ACCEPT,

    /// <summary>
    /// Accept the input and move down
    /// </summary>
    ACCEPT_DOWN,

    /// <summary>
    /// Accept the input and move up
    /// </summary>
    ACCEPT_UP
}

/// <summary>
/// An input form field
/// </summary>
public class FormField {

    /// <summary>
    /// Form label
    /// </summary>
    public string Text { get; init; } = string.Empty;

    /// <summary>
    /// Type of input required
    /// </summary>
    public VariantType Type { get; init; } = VariantType.NONE;

    /// <summary>
    /// Initial value and output
    /// </summary>
    public Variant Value { get; init; } = new(0);

    /// <summary>
    /// Input field width (also define the maximum input)
    /// </summary>
    public int Width { get; init; } = 5;
}

public class CommandBar {
    private readonly int _commandBarRow;
    private readonly int _statusRow;
    private readonly int _promptRow;
    private ConsoleColor _bgColour;
    private ConsoleColor _fgColour;
    private readonly int _displayWidth;
    private readonly int _cursorPositionWidth;
    private readonly int _cellContentPosition;
    private readonly int _cellContentWidth;
    private int _cursorRow = 1;
    private int _cursorColumn = 1;
    private string _filename;
    private int _selectedCommand;
    private CommandMapID _currentCommandMapID;
    private CommandMap? _commandMap;
    private Variant _cellValue;

    /// <summary>
    /// Non-command (navigation) key map
    /// </summary>
    private static readonly KeyMap[] KeyTable = [
        new() { Key = ConsoleKey.LeftArrow, CommandId = KeyCommand.KC_LEFT },
        new() { Key = ConsoleKey.RightArrow, CommandId = KeyCommand.KC_RIGHT },
        new() { Key = ConsoleKey.UpArrow, CommandId = KeyCommand.KC_UP },
        new() { Key = ConsoleKey.DownArrow, CommandId = KeyCommand.KC_DOWN },
        new() { Key = ConsoleKey.Home, CommandId = KeyCommand.KC_HOME },
        new() { Key = ConsoleKey.PageUp, CommandId = KeyCommand.KC_PAGEUP },
        new() { Key = ConsoleKey.PageDown, CommandId = KeyCommand.KC_PAGEDOWN }
    ];

    /// <summary>
    /// Command bar height
    /// </summary>
    public const int Height = 4;

    /// <summary>
    /// Construct a command bar object
    /// </summary>
    public CommandBar() {
        _commandBarRow = Terminal.Height - Height;
        _displayWidth = Terminal.Width;
        _statusRow = Terminal.Height - 1;
        _promptRow = _commandBarRow + 2;
        _cursorPositionWidth = 9;
        _filename = string.Empty;
        _currentCommandMapID = CommandMapID.MAIN;
        _commandMap = Commands.CommandMapForID(_currentCommandMapID);
        _selectedCommand = 0;
        _cellValue = new Variant();
        _cellContentPosition = _cursorPositionWidth + 1;
        _cellContentWidth = _displayWidth - _cursorPositionWidth - 10;
    }

    /// <summary>
    /// Set the active command map for the command bar.
    /// </summary>
    public void SetActiveCommandMap(CommandMapID newMap) {
        _commandMap = Commands.CommandMapForID(newMap);
        _currentCommandMapID = newMap;
        _selectedCommand = 0;
        RenderCommandList(true);
    }

    /// <summary>
    /// Update the cursor position indicator on the command bar
    /// </summary>
    public void UpdateCursorPosition(int line, int column) {
        _cursorRow = line;
        _cursorColumn = column;
        RenderCursorPosition();
    }

    /// <summary>
    /// Update the active cell contents on the command bar.
    /// </summary>
    /// <param name="cellValue">Cell value</param>
    public void UpdateCellContents(Variant cellValue) {
        _cellValue = cellValue;
        RenderCellContents();
    }

    /// <summary>
    /// Update the filename on the command bar
    /// </summary>
    public void UpdateFilename(string newFilename) {
        _filename = newFilename;
        RenderSheetFilename();
    }

    /// <summary>
    /// Refresh the command bar, for example, when the screen
    /// colours change.
    /// </summary>
    public void Refresh() {
        _fgColour = Screen.Colours.NormalMessageColour;
        _bgColour = Screen.Colours.BackgroundColour;
        RenderCommandList(true);
        RenderCursorPosition();
        RenderSheetFilename();
    }

    /// <summary>
    /// Map a keystroke to a command
    /// </summary>
    public KeyCommand MapKeyToCommand(ConsoleKeyInfo keyIn) {
        if (keyIn.Key == ConsoleKey.Escape) {
            if (_currentCommandMapID != CommandMapID.MAIN) {
                SetActiveCommandMap(CommandMapID.MAIN);
            }
            Message(Calc.SelectOptionPrompt);
            return KeyCommand.KC_NONE;
        }
        if (keyIn.Key == ConsoleKey.Spacebar) {
            MoveSelectedCommand(1);
            return KeyCommand.KC_NONE;
        }
        if (keyIn.Key == ConsoleKey.Backspace) {
            MoveSelectedCommand(-1);
            return KeyCommand.KC_NONE;
        }
        if (keyIn.Key == ConsoleKey.Enter) {
            return ActivateSelectedCommand();
        }
        foreach (KeyMap command in KeyTable) {
            if (command.Key == keyIn.Key) {
                return command.CommandId;
            }
        }
        if (char.IsLetter(keyIn.KeyChar)) {
            Debug.Assert(_commandMap != null);
            foreach (CommandMapEntry entry in _commandMap.Commands) {
                if (entry.Name[0] == char.ToUpper(keyIn.KeyChar)) {
                    return entry.CommandId;
                }
            }
            Message(Calc.NotAValidOption);
        }
        return KeyCommand.KC_NONE;
    }

    /// <summary>
    /// Display a prompt on the command bar for input using a series of form fields. The form fields
    /// should specify the prompt for each input, type of each input, a default value and width. On
    /// completion, the default value will be replaced with the actual value entered.
    /// </summary>
    /// <param name="prompt">Prompt</param>
    /// <param name="fields">List of input fields</param>
    /// <returns>True if input was provided, false if the user hit Esc to cancel</returns>
    public bool PromptForInput(string prompt, FormField [] fields) {
        Point cursorPosition = Terminal.GetCursor();
        RenderText(0, _commandBarRow, _displayWidth, prompt, _fgColour, _bgColour);
        RenderText(0, _commandBarRow + 1, _displayWidth, "", _fgColour, _bgColour);
        int column = prompt.Length + 1;
        List<Point> fieldPositions = [];
        foreach (FormField field in fields) {
            if (!string.IsNullOrEmpty(field.Text)) {
                RenderText(column, _commandBarRow, _displayWidth, $"{field.Text}:", _fgColour, _bgColour);
                column += field.Text.Length + 2;
            }
            string value = string.Empty;
            switch (field.Type) {
                case VariantType.INTEGER:
                    value = $"{field.Value:-field.Width}";
                    break;

                case VariantType.STRING:
                    value = field.Value.StringValue;
                    break;

                default:
                    Debug.Assert(false, $"{field.Type} is not supported");
                    break;
            }
            fieldPositions.Add(new Point(column, _commandBarRow));
            RenderText(column, _commandBarRow, field.Width, value, _bgColour, _fgColour);
            column += field.Width + 1;
        }
        ConsoleKeyInfo input;
        int fieldIndex = 0;
        int index = 0;
        bool initialiseField = true;
        List<char> inputBuffer = [];
        do {
            if (initialiseField) {
                RenderText(0, _promptRow, _displayWidth, Calc.EnterNumber, _fgColour, _bgColour);
                inputBuffer = fields[fieldIndex].Value.StringValue.ToList();
                index = inputBuffer.Count;
                Terminal.SetCursor(fieldPositions[fieldIndex].X + index, fieldPositions[fieldIndex].Y);
                initialiseField = false;
            }
            input = Console.ReadKey(true);
            switch (input.Key) {
                case ConsoleKey.Tab: {
                    fields[fieldIndex].Value.Set(string.Join("", inputBuffer));
                    int direction = input.Modifiers.HasFlag(ConsoleModifiers.Shift) ? -1 : 1;
                    fieldIndex = Utilities.ConstrainAndWrap(fieldIndex + direction, 0, fields.Length);
                    initialiseField = true;
                    continue;
                }

                case ConsoleKey.RightArrow when index < inputBuffer.Count:
                    ++index;
                    break;

                case ConsoleKey.LeftArrow when index > 0:
                    --index;
                    break;

                case ConsoleKey.Backspace when index > 0: {
                    inputBuffer.RemoveAt(--index);
                    break;
                }
            }
            if (fields[fieldIndex].Type == VariantType.INTEGER && !char.IsDigit(input.KeyChar)) {
                continue;
            }
            if (!char.IsControl(input.KeyChar) && inputBuffer.Count < fields[fieldIndex].Width) {
                inputBuffer.Insert(index++, input.KeyChar);
            }

            string text = string.Join("", inputBuffer);
            RenderText(fieldPositions[fieldIndex].X, fieldPositions[fieldIndex].Y, fields[fieldIndex].Width, text, _bgColour, _fgColour);
            Terminal.SetCursor(fieldPositions[fieldIndex].X + index, fieldPositions[fieldIndex].Y);

        } while (input.Key != ConsoleKey.Enter && input.Key != ConsoleKey.Escape);

        fields[fieldIndex].Value.Set(string.Join("", inputBuffer));

        Terminal.SetCursor(cursorPosition);
        RenderCommandList(true);
        return input.Key == ConsoleKey.Enter;
    }

    /// <summary>
    /// Prompt for a cell input value.
    /// </summary>
    /// <param name="flags">Type of input permitted</param>
    /// <param name="variant">Output value</param>
    /// <returns>A CellInputResponse indicating how the input was completed</returns>
    public CellInputResponse PromptForCellInput(CellInputFlags flags, ref Variant variant) {

        Point cursorPosition = Terminal.GetCursor();

        int index = 0;
        List<char> inputBuffer = [];
        int column = 0;
        int maxWidth = 0;
        bool initalise = true;
        CellInputResponse response;
        if (variant.Type != VariantType.NONE) {
            inputBuffer = [..variant.ToString().ToCharArray()];
        }
        do {
            if (initalise) {
                string prompt = flags switch {
                    CellInputFlags.ALPHA => Calc.Alpha,
                    CellInputFlags.VALUE => Calc.Value,
                    CellInputFlags.ALPHAVALUE => Calc.AlphaValue,
                    _ => string.Empty
                };
                RenderText(0, _commandBarRow, _displayWidth, prompt, _fgColour, _bgColour);
                RenderText(0, _commandBarRow + 1, _displayWidth, string.Empty, _fgColour, _bgColour);
                column = prompt.Length + 1;
                maxWidth = _displayWidth - column - 1;

                prompt = flags switch {
                    CellInputFlags.ALPHA => Calc.EnterText,
                    CellInputFlags.VALUE => Calc.EnterNumber,
                    CellInputFlags.ALPHAVALUE => Calc.EnterTextOrValue,
                    _ => string.Empty
                };
                RenderText(0, _promptRow, _displayWidth, prompt, _fgColour, _bgColour);
                initalise = false;
            }

            string text = string.Join("", inputBuffer);
            RenderText(column, _commandBarRow, maxWidth, text, _fgColour, _bgColour);
            Terminal.SetCursor(column + index, _commandBarRow);

            ConsoleKeyInfo input = Console.ReadKey(true);
            response = input.Key switch {
                ConsoleKey.Escape => CellInputResponse.CANCEL,
                ConsoleKey.Enter => CellInputResponse.ACCEPT,
                ConsoleKey.DownArrow => CellInputResponse.ACCEPT_DOWN,
                ConsoleKey.UpArrow => CellInputResponse.ACCEPT_UP,
                _ => 0
            };
            if (response != 0) {
                break;
            }
            switch (input.Key) {
               case ConsoleKey.LeftArrow when index > 0:
                   --index;
                   break;

               case ConsoleKey.RightArrow when index < inputBuffer.Count:
                   ++index;
                   break;

               case ConsoleKey.Backspace when index > 0:
                   inputBuffer.RemoveAt(--index);
                   break;
           }
           if (flags == CellInputFlags.ALPHAVALUE) {
               flags = IsValueChar(input.KeyChar) ? CellInputFlags.VALUE : CellInputFlags.ALPHA;
               initalise = true;
           }
           else if (flags == CellInputFlags.VALUE && !IsValueChar(input.KeyChar)) {
               continue;
           }
           if (!char.IsControl(input.KeyChar) && inputBuffer.Count < maxWidth) {
               inputBuffer.Insert(index++, input.KeyChar);
           }
        } while (true);

        if (response != CellInputResponse.CANCEL) {
            string input = string.Join("", inputBuffer);
            switch (flags) {
                case CellInputFlags.ALPHA:
                    variant.Set(input);
                    break;

                case CellInputFlags.VALUE:
                    variant.Set(double.TryParse(input, out double _value) ? _value : 0);
                    break;
            }
        }

        Terminal.SetCursor(cursorPosition);
        if (response is CellInputResponse.CANCEL or CellInputResponse.ACCEPT) {
            RenderCommandList(true);
        }
        return response;
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
    /// Activate the highlighted command in the command map.
    /// </summary>
    /// <returns>Command ID</returns>
    private KeyCommand ActivateSelectedCommand() {
        Debug.Assert(_commandMap != null);
        CommandMapEntry command = _commandMap.Commands[_selectedCommand];
        return command.CommandId;
    }

    /// <summary>
    /// Render the list of commands
    /// </summary>
    /// <param name="redraw">True if we do a full redraw, false if we just update</param>
    private void RenderCommandList(bool redraw) {
        Debug.Assert(_commandMap != null);
        string commandPrompt = $"{Utilities.GetEnumDescription(_commandMap.ID)}: ";
        int row = _commandBarRow;
        int column = commandPrompt.Length;
        if (redraw) {
            RenderText(0, row, _displayWidth, commandPrompt, _fgColour, _bgColour);
        }
        for (int i = 0; i < _commandMap.Commands.Length; i++) {
            CommandMapEntry command = _commandMap.Commands[i];
            if (column + command.Name.Length >= _displayWidth) {
                row++;
                column = commandPrompt.Length;
            }
            ConsoleColor fgColour = i == _selectedCommand ? _bgColour : _fgColour;
            ConsoleColor bgColour = i == _selectedCommand ? _fgColour : _bgColour;
            RenderText(column, row, command.Name.Length, command.Name, fgColour, bgColour);
            column += command.Name.Length + 1;
        }
        if (redraw) {
            if (++row < _promptRow) {
                RenderText(0, row, _displayWidth, string.Empty, _fgColour, _bgColour);
            }
            Message(Calc.SelectOptionPrompt);
        }
    }

    /// <summary>
    /// Show the current selected row and column position
    /// </summary>
    private void RenderCursorPosition() {
        string text = $"R{_cursorRow}C{_cursorColumn}";
        RenderText(0, _statusRow, _cursorPositionWidth, text, _fgColour, _bgColour);
    }

    /// <summary>
    /// Show the current sheet filename in the bottom left corner
    /// </summary>
    private void RenderSheetFilename() {
        string text = $@"Calc: {_filename}";
        RenderText(_displayWidth - text.Length - 4, _statusRow, text.Length, text, _fgColour, _bgColour);
    }

    /// <summary>
    /// Show the active cell contents
    /// </summary>
    private void RenderCellContents() {
        string cellValue = string.Empty;
        if (_cellValue.HasValue) {
            cellValue = _cellValue.Type == VariantType.STRING ? $"\"{_cellValue.StringValue}\"" : _cellValue.StringValue;
        }
        RenderText(_cellContentPosition, _statusRow, _cellContentWidth, cellValue, _fgColour, _bgColour);
    }

    /// <summary>
    /// Update the selected command index.
    /// </summary>
    private void MoveSelectedCommand(int direction) {
        Debug.Assert(_commandMap != null);
        _selectedCommand = Utilities.ConstrainAndWrap(_selectedCommand + direction, 0, _commandMap.Commands.Length);
        RenderCommandList(false);
    }

    /// <summary>
    /// Is the specified character one which is valid in a Value field?
    /// </summary>
    /// <param name="ch">Character to test</param>
    /// <returns>True if the character is a valid value character, false otherwise</returns>
    private static bool IsValueChar(char ch) => char.IsDigit(ch) || ch == '(' || ch == '.' || ch == '=' || ch == '+' || ch == '-' || ch == '\"';

    /// <summary>
    /// Display a message on the prompt bar
    /// </summary>
    /// <param name="text">Message to display</param>
    private void Message(string text) {
        RenderText(0, _promptRow, _displayWidth, text, _fgColour, _bgColour);
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