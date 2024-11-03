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
using System.Globalization;
using JComLib;

namespace JCalc;

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
    ACCEPT_UP,

    /// <summary>
    /// Accept the input and move left
    /// </summary>
    ACCEPT_LEFT,

    /// <summary>
    /// Accept the input and move right
    /// </summary>
    ACCEPT_RIGHT
}

public enum FormFieldType {

    /// <summary>
    /// Number required
    /// </summary>
    NUMBER,

    /// <summary>
    /// Any text required
    /// </summary>
    TEXT
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
    public FormFieldType Type { get; init; }

    /// <summary>
    /// Initial value and output
    /// </summary>
    public Variant Value { get; init; } = new(0);

    /// <summary>
    /// Input field width (also define the maximum input)
    /// </summary>
    public int Width { get; init; } = 5;

    /// <summary>
    /// Save the input buffer to the Value
    /// </summary>
    /// <param name="inputBuffer">Input buffer</param>
    public void Save(List<char> inputBuffer) {
        string inputValue = string.Join("", inputBuffer);
        switch (Type) {
            case FormFieldType.NUMBER:
                Value.Set(int.TryParse(inputValue, out int _result) ? _result : 0);
                break;
            default:
                Value.Set(inputValue);
                break;
        }
    }
}

public class CommandBar {
    private readonly int _cellStatusRow;
    private readonly int _promptRow;
    private readonly int _messageRow;
    private ConsoleColor _bgColour;
    private ConsoleColor _fgColour;
    private readonly int _displayWidth;
    private readonly int _cursorPositionWidth;
    private readonly int _cellContentPosition;
    private readonly int _cellContentWidth;
    private string _cursorPosition;
    private CellValue _cellValue;
    private ConsoleKeyInfo? _pushedKey;

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
        new() { Key = ConsoleKey.PageDown, CommandId = KeyCommand.KC_PAGEDOWN },
        new() { Key = ConsoleKey.F2, CommandId = KeyCommand.KC_EDIT },
        new() { Key = ConsoleKey.F5, CommandId = KeyCommand.KC_GOTO }
    ];

    /// <summary>
    /// Command bar height
    /// </summary>
    public const int Height = 3;

    /// <summary>
    /// Construct a command bar object
    /// </summary>
    public CommandBar() {
        _cellStatusRow = 0;
        _promptRow = _cellStatusRow + 1;
        _messageRow = _cellStatusRow + 2;
        _displayWidth = Terminal.Width;
        _cursorPositionWidth = 10;
        _cursorPosition = string.Empty;
        _cellValue = new CellValue();
        _cellContentPosition = _cursorPositionWidth;
        _cellContentWidth = _displayWidth - _cursorPositionWidth;
    }

    /// <summary>
    /// Update the active cell location and contents on the command bar.
    /// </summary>
    /// <param name="cell">Cell</param>
    public void UpdateCellStatus(Cell cell) {
        _cursorPosition = cell.Position;
        _cellValue = cell.Value;
        RenderCursorPosition();
        RenderCellContents();
    }

    /// <summary>
    /// Refresh the command bar, for example, when the screen
    /// colours change.
    /// </summary>
    public void Refresh() {
        _fgColour = Screen.Colours.NormalMessageColour;
        _bgColour = Screen.Colours.BackgroundColour;
        RenderCursorPosition();
    }

    /// <summary>
    /// Read a key
    /// </summary>
    /// <returns>ConsoleKeyInfo for the input key</returns>
    public ConsoleKeyInfo ReadKey() {
        if (_pushedKey != null) {
            ConsoleKeyInfo key = _pushedKey.Value;
            _pushedKey = null;
            return key;
        }
        return Console.ReadKey(true);
    }

    /// <summary>
    /// Map a keystroke to a command
    /// </summary>
    public KeyCommand MapKeyToCommand(ConsoleKeyInfo keyIn) {
        if (keyIn.KeyChar == '/') {
            return KeyCommand.KC_COMMAND_BAR;
        }
        foreach (KeyMap command in KeyTable) {
            if (command.Key == keyIn.Key) {
                return command.CommandId;
            }
        }
        if (!char.IsControl(keyIn.KeyChar)) {
            PushKey(keyIn);
            return KeyCommand.KC_VALUE;
        }
        return KeyCommand.KC_NONE;
    }

    /// <summary>
    /// Display a prompt on the command bar for input using a series of form fields. The form fields
    /// should specify the prompt for each input, type of each input, a default value and width. On
    /// completion, the default value will be replaced with the actual value entered.
    /// </summary>
    /// <param name="fields">List of input fields</param>
    /// <returns>True if input was provided, false if the user hit Esc to cancel</returns>
    public bool PromptForInput(FormField [] fields) {
        Point cursorPosition = Terminal.GetCursor();
        int column = 0;
        int row = _promptRow;
        List<Point> fieldPositions = [];
        foreach (FormField field in fields) {
            string value = string.Empty;
            int width = field.Width;
            int labelWidth = field.Text.Length + 2;
            switch (field.Type) {
                case FormFieldType.NUMBER:
                    value = $"{field.Value:-field.Width}";
                    break;

                case FormFieldType.TEXT:
                    value = field.Value.StringValue;
                    break;

                default:
                    Debug.Assert(false, $"{field.Type} is not supported");
                    break;
            }
            if (column + width + labelWidth > _displayWidth) {
                column = 7;
                ++row;
            }
            if (!string.IsNullOrEmpty(field.Text)) {
                Terminal.WriteText(column, row, _displayWidth, $"{field.Text}:", _fgColour, _bgColour);
                column += labelWidth;
            }
            fieldPositions.Add(new Point(column, row));
            Terminal.WriteText(column, row, width, value, _fgColour, _bgColour);
            column += width + 1;
        }
        ClearRow(_messageRow);
        ConsoleKeyInfo input;
        int fieldIndex = 0;
        int index = 0;
        bool initialiseField = true;
        List<char> inputBuffer = [];
        do {
            if (initialiseField) {
                inputBuffer = fields[fieldIndex].Value.StringValue.ToList();
                index = inputBuffer.Count;
                initialiseField = false;
            }

            string text = string.Join("", inputBuffer);
            Terminal.WriteText(fieldPositions[fieldIndex].X, fieldPositions[fieldIndex].Y, fields[fieldIndex].Width, text, _bgColour, _fgColour);
            Terminal.SetCursor(fieldPositions[fieldIndex].X + index, fieldPositions[fieldIndex].Y);

            input = Console.ReadKey(true);
            switch (input.Key) {
                case ConsoleKey.Tab: {
                    fields[fieldIndex].Save(inputBuffer);
                    Terminal.WriteText(fieldPositions[fieldIndex].X, fieldPositions[fieldIndex].Y, fields[fieldIndex].Width, text, _fgColour, _bgColour);
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

                case ConsoleKey.Backspace when index > 0:
                    inputBuffer.RemoveAt(--index);
                    break;

                default:
                    if (fields[fieldIndex].Type == FormFieldType.NUMBER && !char.IsDigit(input.KeyChar)) {
                        continue;
                    }
                    if (!char.IsControl(input.KeyChar) && inputBuffer.Count < fields[fieldIndex].Width) {
                        inputBuffer.Insert(index++, input.KeyChar);
                    }
                    break;
            }
        } while (input.Key != ConsoleKey.Enter && input.Key != ConsoleKey.Escape);

        fields[fieldIndex].Save(inputBuffer);

        ClearRow(_promptRow);
        Terminal.SetCursor(cursorPosition);
        return input.Key == ConsoleKey.Enter;
    }

    /// <summary>
    /// Prompt for a cell input value.
    /// </summary>
    /// <param name="cellValue">Output value</param>
    /// <returns>A CellInputResponse indicating how the input was completed</returns>
    public CellInputResponse PromptForCellInput(ref CellValue cellValue) {
        Point cursorPosition = Terminal.GetCursor();

        List<char> inputBuffer = [];
        CellInputResponse response;
        if (cellValue.Type != CellType.NONE) {
            inputBuffer = [..cellValue.StringValue.ToCharArray()];
        }
        int column = inputBuffer.Count;
        do {
            string text = string.Join("", inputBuffer);
            Terminal.WriteText(0, _promptRow, _displayWidth, text, _fgColour, _bgColour);
            Terminal.SetCursor(column, _promptRow);

            ConsoleKeyInfo inputKey = ReadKey();
            response = inputKey.Key switch {
                ConsoleKey.Escape => CellInputResponse.CANCEL,
                ConsoleKey.Enter => CellInputResponse.ACCEPT,
                ConsoleKey.DownArrow => CellInputResponse.ACCEPT_DOWN,
                ConsoleKey.UpArrow => CellInputResponse.ACCEPT_UP,
                ConsoleKey.LeftArrow when column == 0 => CellInputResponse.ACCEPT_LEFT,
                ConsoleKey.RightArrow when column == inputBuffer.Count => CellInputResponse.ACCEPT_RIGHT,
                _ => CellInputResponse.NONE
            };
            if (response != CellInputResponse.NONE) {
                break;
            }
            switch (inputKey.Key) {
                case ConsoleKey.LeftArrow when column > 0:
                    --column;
                    break;

                case ConsoleKey.RightArrow when column < inputBuffer.Count:
                    ++column;
                    break;

                case ConsoleKey.Backspace when column > 0:
                   inputBuffer.RemoveAt(--column);
                   break;
           }
           if (!char.IsControl(inputKey.KeyChar) && inputBuffer.Count < _displayWidth) {
               inputBuffer.Insert(column++, inputKey.KeyChar);
           }
        } while (true);

        string input = string.Join("", inputBuffer);
        if (double.TryParse(input, out double _value)) {
            cellValue.StringValue = _value.ToString(CultureInfo.InvariantCulture);
            cellValue.Type = CellType.NUMBER;
        } else {
            cellValue.StringValue = input;
            cellValue.Type = CellType.TEXT;
        }

        ClearRow(_promptRow);
        Terminal.SetCursor(cursorPosition);
        return response;
    }

    /// <summary>
    /// Display a prompt on the status bar and prompt for a keystroke input.
    /// </summary>
    /// <returns>True if a value was input, false if empty or cancelled</returns>
    public bool Prompt(string prompt, char[] validInput, out char inputValue) {

        Point cursorPosition = Terminal.GetCursor();
        prompt = prompt.Replace("@@", $"[{string.Join("", validInput)}]");
        Terminal.WriteText(0, _promptRow, _displayWidth, prompt, _fgColour, _bgColour);
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
        ClearRow(_promptRow);
        return input.Key != ConsoleKey.Escape;
    }

    /// <summary>
    /// Render the list of commands
    /// </summary>
    public RenderHint PromptForCommand(CommandMapID commandMapID) {
        Stack<CommandMapID> commandMapStack = new Stack<CommandMapID>();
        int selectedCommand = 0;
        bool redraw = true;
        RenderHint flags = RenderHint.NONE;
        do {
            CommandMap commandMap = Commands.CommandMapForID(commandMapID);
            CommandMapEntry current = commandMap.Commands[selectedCommand];
            if (redraw) {
                RenderCommandMap(commandMapID, _promptRow, selectedCommand);
                if (!string.IsNullOrEmpty(current.Description)) {
                    Message(current.Description);
                } else {
                    RenderCommandMap(current.SubCommandId, _messageRow, -1);
                }
                redraw = false;
            }
            ConsoleKeyInfo input = Console.ReadKey(true);
            bool actionCommand = false;
            if (char.IsLetter(input.KeyChar)) {
                char inputKey = char.ToUpper(input.KeyChar);
                CommandMapEntry? commandId = commandMap.Commands.FirstOrDefault(c => c.Name[0] == inputKey);
                if (commandId == null) {
                    continue;
                }
                current = commandId;
                actionCommand = true;
            }
            if (input.Key == ConsoleKey.Enter) {
                actionCommand = true;
            }
            if (actionCommand) {
                if (current.SubCommandId != CommandMapID.NONE) {
                    commandMapStack.Push(commandMapID);
                    commandMapID = current.SubCommandId;
                    selectedCommand = 0;
                    redraw = true;
                    continue;
                }
                ClearRow(_messageRow);
                flags = Screen.HandleCommand(current.CommandId);
                if (flags != RenderHint.CANCEL) {
                    break;
                }
                redraw = true;
                continue;
            }
            if (input.Key == ConsoleKey.Escape) {
                if (commandMapStack.TryPop(out commandMapID)) {
                    selectedCommand = 0;
                    redraw = true;
                    continue;
                }
                break;
            }
            switch (input.Key) {
                case ConsoleKey.RightArrow:
                    if (++selectedCommand == commandMap.Commands.Length) {
                        selectedCommand = 0;
                    }
                    redraw = true;
                    break;

                case ConsoleKey.LeftArrow:
                    if (--selectedCommand < 0) {
                        selectedCommand = commandMap.Commands.Length - 1;
                    }
                    redraw = true;
                    break;
            }
        } while (true);
        ClearRow(_promptRow);
        ClearRow(_messageRow);
        return flags;
    }

    /// <summary>
    /// Render a command map.
    /// </summary>
    /// <param name="commandMapID">ID of command map to render</param>
    /// <param name="row">Row to which command map should be rendered</param>
    /// <param name="selectedCommand">Selected command</param>
    private void RenderCommandMap(CommandMapID commandMapID, int row, int selectedCommand) {
        CommandMap commandMap = Commands.CommandMapForID(commandMapID);
        ClearRow(row);
        int column = 0;
        for (int i = 0; i < commandMap.Commands.Length; i++) {
            CommandMapEntry command = commandMap.Commands[i];
            ConsoleColor fgColour = i == selectedCommand ? _bgColour : _fgColour;
            ConsoleColor bgColour = i == selectedCommand ? _fgColour : _bgColour;
            Terminal.WriteText(column, row, command.Name.Length, command.Name, fgColour, bgColour);
            column += command.Name.Length + 1;
        }
    }

    /// <summary>
    /// Show the current selected row and column position
    /// </summary>
    private void RenderCursorPosition() {
        Terminal.WriteText(0, _cellStatusRow, _cursorPositionWidth, _cursorPosition, _fgColour, _bgColour);
    }

    /// <summary>
    /// Show the active cell contents
    /// </summary>
    private void RenderCellContents() {
        string cellValue = string.Empty;
        if (_cellValue.Type != CellType.NONE) {
            cellValue = _cellValue.Type == CellType.TEXT ? $"\"{_cellValue.StringValue}\"" : _cellValue.StringValue;
        }
        Terminal.WriteText(_cellContentPosition, _cellStatusRow, _cellContentWidth, cellValue, _fgColour, _bgColour);
    }

    /// <summary>
    /// Display a message on the prompt bar
    /// </summary>
    /// <param name="text">Message to display</param>
    private void Message(string text) {
        Terminal.WriteText(0, _messageRow, _displayWidth, text, _fgColour, _bgColour);
    }

    /// <summary>
    /// Clear a row on the command bar
    /// </summary>
    /// <param name="row">Row to clear</param>
    private void ClearRow(int row) {
        Terminal.WriteText(0, row, _displayWidth, string.Empty, _fgColour, _bgColour);
    }

    /// <summary>
    /// Push a keystroke back to the input queue.
    /// </summary>
    /// <param name="keyToPush">Key to push</param>
    private void PushKey(ConsoleKeyInfo keyToPush) {
        _pushedKey = keyToPush;
    }
}