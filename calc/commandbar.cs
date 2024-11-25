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
using JCalcLib;
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

    /// <summary>
    /// Modifier key, if applicable
    /// </summary>
    public ConsoleModifiers Modifier { get; init; } = ConsoleModifiers.None;
}

/// <summary>
/// Response from the PromptCellInput method indication what action
/// should be taken after.
/// </summary>
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

/// <summary>
/// Type of data input in a form field.
/// </summary>
public enum FormFieldType {

    /// <summary>
    /// Number required
    /// </summary>
    NUMBER,

    /// <summary>
    /// Any text required
    /// </summary>
    TEXT,

    /// <summary>
    /// Boolean flag
    /// </summary>
    BOOLEAN,

    /// <summary>
    /// Pick a range of values
    /// </summary>
    PICKER
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
    /// Allow filename completion in the input field. This is only
    /// valid if there is a single input field in the form.
    /// </summary>
    public bool AllowFilenameCompletion { get; init; }

    /// <summary>
    /// Filter used to limit filename completion.
    /// </summary>
    public string FilenameCompletionFilter { get; init; } = "*";

    /// <summary>
    /// Minimum allowed value, for numeric input
    /// </summary>
    public int MinimumValue { get; init; }

    /// <summary>
    /// Maximum allowed value, for numeric input
    /// </summary>
    public int MaximumValue { get; init; }

    /// <summary>
    /// Pick list values
    /// </summary>
    public string[] List { get; init; } = [];
}

internal class FormFieldInternal(FormField inner) {

    /// <summary>
    /// Encapsulated form field
    /// </summary>
    public FormField Inner { get; } = inner;

    /// <summary>
    /// X position of form field on screen.
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// Y position of form field on screen.
    /// </summary>
    public int Y { get; set; }

    /// <summary>
    /// Returns whether there is a valid minimum and maximum range.
    /// </summary>
    private bool HasMinMaxRange => Inner.MinimumValue + Inner.MaximumValue > 0;

    /// <summary>
    /// Maximum width of input field
    /// </summary>
    public int MaxWidth { get; set; }

    /// <summary>
    /// True if input cannot be edited directly (e.g. boolean toggle)
    /// </summary>
    public bool IsReadOnly { get; set; }

    /// <summary>
    /// Save the input buffer to the Value. String values have leading and trailing
    /// spaces removed. Numeric values are validated against the minimum and maximum
    /// value permitted.
    /// </summary>
    /// <param name="inputBuffer">Input buffer</param>
    /// <returns>True if value was saved, false if it failed validation</returns>
    public bool Save(List<char> inputBuffer) {
        string inputValue = string.Join("", inputBuffer);
        switch (Inner.Type) {
            case FormFieldType.NUMBER:
                int value = int.TryParse(inputValue, out int _result) ? _result : 0;
                if (HasMinMaxRange && (value < Inner.MinimumValue || value > Inner.MaximumValue)) {
                    return false;
                }
                Inner.Value.Set(value);
                break;
            case FormFieldType.BOOLEAN:
                Inner.Value.Set(bool.TryParse(inputValue, out bool _boolresult) && _boolresult);
                break;
            default:
                Inner.Value.Set(inputValue.Trim());
                break;
        }
        return true;
    }
}

public class CommandBar {

    /// <summary>
    /// Command bar height
    /// </summary>
    public const int Height = 3;

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
        new() { Key = ConsoleKey.F5, CommandId = KeyCommand.KC_GOTO },
        new() { Key = ConsoleKey.Delete, CommandId = KeyCommand.KC_DELETE },
        new() { Key = ConsoleKey.F6, CommandId = KeyCommand.KC_NEXT_WINDOW },
        new() { Key = ConsoleKey.C, Modifier = ConsoleModifiers.Control, CommandId = KeyCommand.KC_COPY },
        new() { Key = ConsoleKey.X, Modifier = ConsoleModifiers.Control, CommandId = KeyCommand.KC_CUT },
        new() { Key = ConsoleKey.V, Modifier = ConsoleModifiers.Control, CommandId = KeyCommand.KC_PASTE }
    ];

    private readonly int _cellStatusRow;
    private readonly int _displayWidth;
    private readonly int _messageRow;
    private readonly int _promptRow;
    private int _bgColour;
    private Sheet? _currentSheet;
    private int _fgColour;
    private ConsoleKeyInfo? _pushedKey;
    private int _selColour;

    /// <summary>
    /// Construct a command bar object
    /// </summary>
    public CommandBar() {
        _cellStatusRow = 0;
        _currentSheet = null;
        _promptRow = _cellStatusRow + 1;
        _messageRow = _cellStatusRow + 2;
        _displayWidth = Terminal.Width;
    }

    /// <summary>
    /// Update the active sheet details on the command bar.
    /// </summary>
    /// <param name="sheet">Cell</param>
    public void UpdateCellStatus(Sheet sheet) {
        _currentSheet = sheet;
        RenderCellStatus();
    }

    /// <summary>
    /// Refresh the command bar, for example, when the screen
    /// colours change.
    /// </summary>
    public void Refresh() {
        _fgColour = Screen.Colours.NormalMessageColour;
        _bgColour = Screen.Colours.BackgroundColour;
        _selColour = Screen.Colours.SelectionColour;
        ClearRow(_cellStatusRow);
        ClearRow(_promptRow);
        ClearRow(_messageRow);
        RenderCellStatus();
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
        if (keyIn.KeyChar == 181) {
            return KeyCommand.KC_MARK;
        }
        foreach (KeyMap command in KeyTable) {
            if (command.Key == keyIn.Key && command.Modifier == keyIn.Modifiers) {
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
    /// <param name="userfields">List of input fields</param>
    /// <returns>True if input was provided, false if the user hit Esc to cancel</returns>
    public bool PromptForInput(FormField[] userfields) {
        int column = 0;
        int row = _promptRow;

        List<FormFieldInternal> fields = [];
        foreach (FormField userfield in userfields) {
            FormFieldInternal field = new FormFieldInternal(userfield);
            fields.Add(field);
            string value = string.Empty;
            int width = userfield.Width;
            string prompt = userfield.Text;
            switch (userfield.Type) {
                case FormFieldType.NUMBER:
                    value = $"{userfield.Value:-field.Width}";
                    prompt = prompt.Replace("@@", $"({userfield.MinimumValue}..{userfield.MaximumValue})");
                    break;

                case FormFieldType.BOOLEAN:
                    value = $"{(userfield.Value.BoolValue ? bool.TrueString : bool.FalseString)}";
                    width = Math.Max(bool.TrueString.Length, bool.FalseString.Length);
                    break;

                case FormFieldType.PICKER:
                    value = userfield.Value.StringValue;
                    width = userfield.List.Max(p => p.Length);
                    break;

                case FormFieldType.TEXT:
                    value = userfield.Value.StringValue;
                    break;

                default:
                    Debug.Assert(false, $"{userfield.Type} is not supported");
                    break;
            }
            int labelWidth = prompt.Length + 2;
            if (column + width + labelWidth > _displayWidth) {
                column = 7;
                ++row;
            }
            if (!string.IsNullOrEmpty(prompt)) {
                Terminal.Write(column, row, _displayWidth, _fgColour, _bgColour, $"{prompt}:");
                column += labelWidth;
            }
            field.X = column;
            field.Y = row;
            field.MaxWidth = width;
            field.IsReadOnly = userfield.Type == FormFieldType.BOOLEAN;
            Terminal.Write(column, row, width, _fgColour, _bgColour, value);
            column += width + 2;
        }

        ClearRow(_messageRow);

        int fieldIndex = 0;
        int index = 0;
        bool initialiseField = true;
        string[]? allfiles = null;
        int allfilesIndex = 0;
        List<char> inputBuffer = [];
        bool selection = false;
        FormFieldInternal currentField = fields[0];

        do {
            if (initialiseField) {
                inputBuffer = currentField.Inner.Value.StringValue.ToList();
                if (currentField.Inner.Type == FormFieldType.PICKER) {
                    string pickList = string.Join(" ", currentField.Inner.List);
                    Terminal.Write(0, row + 1, _displayWidth, _fgColour, _bgColour, pickList);
                    Terminal.ShowCursor(false);
                }
                else {
                    ClearRow(row + 1);
                    Terminal.ShowCursor(true);
                }
                index = inputBuffer.Count;
                initialiseField = false;
                selection = true;
            }

            string text = string.Join("", inputBuffer);
            int fg = selection ? _bgColour : _fgColour;
            int bg = selection ? _selColour : _bgColour;
            Terminal.Write(currentField.X, currentField.Y, currentField.MaxWidth, fg, bg, text);
            Terminal.SetCursor(currentField.X + index, currentField.Y);

            ConsoleKeyInfo input = Console.ReadKey(true);
            switch (input.Key) {
                case ConsoleKey.Tab when fields.Count > 1: {
                    if (currentField.Save(inputBuffer)) {
                        Terminal.Write(currentField.X, currentField.Y, currentField.Inner.Width, _fgColour, _bgColour, text);
                        int direction = input.Modifiers.HasFlag(ConsoleModifiers.Shift) ? -1 : 1;
                        fieldIndex = Utilities.ConstrainAndWrap(fieldIndex + direction, 0, fields.Count);
                        currentField = fields[fieldIndex];
                        initialiseField = true;
                    }
                    break;
                }

                case ConsoleKey.Tab when fields.Count == 1 && currentField.Inner.AllowFilenameCompletion: {
                    if (allfiles == null) {
                        string partialName = string.Join("", inputBuffer) + currentField.Inner.FilenameCompletionFilter;
                        allfiles = Directory.GetFiles(".", partialName, SearchOption.TopDirectoryOnly);
                        allfilesIndex = 0;
                    }
                    if (allfiles.Length > 0) {
                        string completedName = new FileInfo(allfiles[allfilesIndex++]).Name;
                        if (allfilesIndex == allfiles.Length) {
                            allfilesIndex = 0;
                        }
                        currentField.Save(completedName.ToList());
                        initialiseField = true;
                    }
                    break;
                }

                case ConsoleKey.RightArrow when index < inputBuffer.Count && !currentField.IsReadOnly:
                    ++index;
                    break;

                case ConsoleKey.LeftArrow when index > 0 && !currentField.IsReadOnly:
                    --index;
                    break;

                case ConsoleKey.Delete when index < inputBuffer.Count && !currentField.IsReadOnly:
                    inputBuffer.RemoveAt(index);
                    break;

                case ConsoleKey.Backspace when index > 0 && selection && !currentField.IsReadOnly:
                    inputBuffer.Clear();
                    index = 0;
                    break;

                case ConsoleKey.Backspace when index > 0 && !selection && !currentField.IsReadOnly:
                    inputBuffer.RemoveAt(--index);
                    break;

                case ConsoleKey.Enter:
                    if (currentField.Save(inputBuffer)) {
                        ClearRow(row);
                        ClearRow(row + 1);
                        Terminal.ShowCursor(false);
                        return !string.IsNullOrEmpty(currentField.Inner.Value.StringValue);
                    }
                    break;

                case ConsoleKey.Escape:
                    ClearRow(row);
                    ClearRow(row + 1);
                    Terminal.ShowCursor(false);
                    return false;

                default:
                    if (currentField.Inner.Type == FormFieldType.BOOLEAN) {
                        inputBuffer = (char.ToUpper(input.KeyChar) == 'Y' ? bool.TrueString : bool.FalseString).ToList();
                        continue;
                    }
                    if (currentField.Inner.Type == FormFieldType.NUMBER && !char.IsDigit(input.KeyChar)) {
                        continue;
                    }
                    if (currentField.Inner.Type == FormFieldType.PICKER) {
                        string? pickIndex = currentField.Inner.List.FirstOrDefault(p => p.StartsWith(char.ToUpper(input.KeyChar)));
                        if (pickIndex != null) {
                            inputBuffer = pickIndex.ToList();
                            index = inputBuffer.Count;
                        }
                        continue;
                    }
                    if (!char.IsControl(input.KeyChar)) {
                        if (selection) {
                            inputBuffer.Clear();
                            index = 0;
                        }
                        if (inputBuffer.Count < currentField.Inner.Width) {
                            inputBuffer.Insert(index++, input.KeyChar);
                            allfiles = null;
                        }
                    }
                    break;
            }
            selection = false;
        } while (true);
    }

    /// <summary>
    /// Edit a cell content.
    /// </summary>
    /// <param name="cellValue">Reference to value being edited</param>
    /// <returns>A CellInputResponse indicating how the input was completed</returns>
    public CellInputResponse PromptForCellInput(ref string cellValue) {

        List<char> inputBuffer = [];
        CellInputResponse response;
        if (!string.IsNullOrEmpty(cellValue)) {
            inputBuffer = [..cellValue.ToCharArray()];
        }
        int column = inputBuffer.Count;
        Terminal.ShowCursor(true);
        do {
            string text = string.Join("", inputBuffer);
            Terminal.Write(0, _promptRow, _displayWidth, _fgColour, _bgColour, text);
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

                case ConsoleKey.Delete when column < inputBuffer.Count:
                    inputBuffer.RemoveAt(column);
                    break;

                case ConsoleKey.Backspace when column > 0:
                    inputBuffer.RemoveAt(--column);
                    break;
            }
            if (!char.IsControl(inputKey.KeyChar) && inputBuffer.Count < _displayWidth) {
                inputBuffer.Insert(column++, inputKey.KeyChar);
            }
        } while (true);

        cellValue = string.Join("", inputBuffer);
        ClearRow(_promptRow);
        Terminal.ShowCursor(false);
        return response;
    }

    /// <summary>
    /// Display a prompt on the status bar and prompt for a keystroke input.
    /// </summary>
    /// <returns>True if a value was input, false if empty or cancelled</returns>
    public bool Prompt(string prompt, char[] validInput, out char inputValue) {

        prompt = prompt.Replace("@@", $"[{string.Join("", validInput)}]");
        Terminal.Write(0, _promptRow, _displayWidth, _fgColour, _bgColour, prompt);
        Terminal.SetCursor(prompt.Length + 1, _promptRow);
        Terminal.ShowCursor(true);
        ConsoleKeyInfo input = Console.ReadKey(true);
        while (!validInput.Contains(char.ToLower(input.KeyChar))) {
            if (input.Key is ConsoleKey.Escape) {
                break;
            }
            input = Console.ReadKey(true);
        }
        Terminal.ShowCursor(false);
        inputValue = char.ToLower(input.KeyChar);
        ClearRow(_promptRow);
        return input.Key != ConsoleKey.Escape;
    }

    /// <summary>
    /// Render the list of commands
    /// </summary>
    public RenderHint PromptForCommand(CommandMapID commandMapID) {
        Stack<(CommandMapID, int)> commandMapStack = new Stack<(CommandMapID, int)>();
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
                }
                else {
                    RenderCommandMap(current.SubCommandId, _messageRow, -1);
                }
                redraw = false;
            }
            ConsoleKeyInfo input = Console.ReadKey(true);
            bool actionCommand = false;
            if (char.IsLetterOrDigit(input.KeyChar)) {
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
                    commandMapStack.Push((commandMapID, selectedCommand));
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
                if (commandMapStack.TryPop(out (CommandMapID, int) tuple)) {
                    commandMapID = tuple.Item1;
                    selectedCommand = tuple.Item2;
                    redraw = true;
                    continue;
                }
                ClearRow(_messageRow);
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
            int fgColour = i == selectedCommand ? _bgColour : _fgColour;
            int bgColour = i == selectedCommand ? _selColour : _bgColour;
            Terminal.Write(column, row, command.Name.Length, fgColour, bgColour, command.Name);
            column += command.Name.Length + 1;
        }
    }

    /// <summary>
    /// Show the current selected row and column position
    /// </summary>
    private void RenderCellStatus() {
        if (_currentSheet != null) {
            string text = $"{(char)(_currentSheet.SheetNumber - 1 + 'A')}:";
            Cell activeCell = _currentSheet.ActiveCell;
            text += $"{activeCell.Address}: {activeCell.FormatDescription} ";
            if (!activeCell.IsEmptyCell) {
                text += activeCell.Content;
            }
            Terminal.Write(0, _cellStatusRow, _displayWidth, _fgColour, _bgColour, text);
        }
    }

    /// <summary>
    /// Display a message on the prompt bar
    /// </summary>
    /// <param name="text">Message to display</param>
    private void Message(string text) {
        Terminal.Write(0, _messageRow, _displayWidth, _fgColour, _bgColour, text);
    }

    /// <summary>
    /// Clear a row on the command bar
    /// </summary>
    /// <param name="row">Row to clear</param>
    private void ClearRow(int row) {
        Terminal.Write(0, row, _displayWidth, _fgColour, _bgColour, string.Empty);
    }

    /// <summary>
    /// Push a keystroke back to the input queue.
    /// </summary>
    /// <param name="keyToPush">Key to push</param>
    private void PushKey(ConsoleKeyInfo keyToPush) {
        _pushedKey = keyToPush;
    }
}