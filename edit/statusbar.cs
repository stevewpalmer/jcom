// JEdit
// Status bar
//
// Authors:
//  Steve Palmer
//
// Copyright (C) 2023 Steve Palmer
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
using System.Runtime.InteropServices;
using JComLib;
using JEdit.Resources;

namespace JEdit;

public class StatusBar {
    private static readonly object LockObj = new();
    private static int _statusRow;
    private static int _timeWidth;
    private static int _timePosition;
    private readonly string _cachedText;
    private readonly int _modeWidth;
    private readonly int _cursorPositionWidth;
    private int _displayWidth;
    private int _modePosition;
    private int _cursorPositionPosition;
    private string _cachedTextInput;
    private KeystrokesMode _keystrokesMode;
    private string _currentMessage;
    private ConsoleColor _currentColour;
    private bool _timerStarted;
    private int _cursorRow;
    private int _cursorColumn;
    private ConsoleColor _bgColour;
    private ConsoleColor _fgColour;
    private ConsoleColor _errColour;
    private bool _showClock;
    private Timer? _clockTimer;

    /// <summary>
    /// Construct a status bar object.
    /// </summary>
    public StatusBar() {
        _statusRow = Terminal.Height - 1;
        _cachedText = string.Empty;
        _cachedTextInput = string.Empty;
        _clockTimer = null;
        _modeWidth = 4;
        _timeWidth = 8;
        _showClock = false;
        _cursorPositionWidth = 18;
        _timePosition = Terminal.Width - _timeWidth;
        _modePosition = Terminal.Width - _modeWidth;
        _cursorPositionPosition = _modePosition - _cursorPositionWidth;
        _displayWidth = Terminal.Width - (_cursorPositionWidth + _modeWidth);
        _currentMessage = string.Empty;
        _currentColour = _fgColour;
        _keystrokesMode = KeystrokesMode.NONE;
        _cursorRow = 1;
        _cursorColumn = 1;
    }

    /// <summary>
    /// Refresh the status bar.
    /// </summary>
    public void Refresh() {
        if (!_timerStarted && _showClock) {
            StartTimer();
            _timerStarted = true;
        }
        _fgColour = Screen.Colours.NormalMessageColour;
        _bgColour = Screen.Colours.BackgroundColour;
        _errColour = Screen.Colours.ErrorMessageColour;
        RenderMessage();
        RenderCursorPosition();
        RenderModeIndicator();
    }

    /// <summary>
    /// Specifies the keystroke recording/playback mode indicator
    /// </summary>
    public KeystrokesMode KeystrokesMode {
        get => _keystrokesMode;
        set {
            _keystrokesMode = value;
            RenderModeIndicator();
        }
    }

    /// <summary>
    /// Whether or not we show the clock on the status bar.
    /// </summary>
    public bool ShowClock {
        get => _showClock;
        set {
            if (_showClock != value) {
                _showClock = value;
                int timeWidth = _showClock ? _timeWidth : 0;
                _modePosition = Terminal.Width - timeWidth - _modeWidth;
                _cursorPositionPosition = _modePosition - _cursorPositionWidth;
                _displayWidth = Terminal.Width - (timeWidth + _cursorPositionWidth + _modeWidth);
                if (!_showClock && _clockTimer != null) {
                    _clockTimer.Change(Timeout.Infinite, Timeout.Infinite);
                }
                _timerStarted = false;
                Refresh();
            }
        }
    }

    /// <summary>
    /// Display a message on the status bar.
    /// </summary>
    /// <param name="message">Message string</param>
    public void Message(string message) {
        _currentMessage = message;
        _currentColour = _fgColour;
        RenderMessage();
    }

    /// <summary>
    /// Display an error message on the status bar.
    /// </summary>
    /// <param name="message">Message string</param>
    public void Error(string message) {
        _currentMessage = message;
        _currentColour = _errColour;
        RenderText(0, _statusRow, _displayWidth, _currentMessage, _errColour);
    }

    /// <summary>
    /// Display a prompt on the status bar and prompt for a keystroke input.
    /// </summary>
    /// <returns>True if a value was input, false if empty or cancelled</returns>
    public bool Prompt(string prompt, char[] validInput, char defaultChar, out char inputValue) {

        Point cursorPosition = Terminal.GetCursor();
        prompt = prompt.Replace("@@", $"[{string.Join("",validInput)}]");
        Message(prompt);
        Terminal.SetCursor(prompt.Length, _statusRow);
        ConsoleKeyInfo input = Console.ReadKey(true);
        while (!validInput.Contains(char.ToLower(input.KeyChar))) {
            if (input.Key is ConsoleKey.Enter or ConsoleKey.Escape) {
                break;
            }
            input = Console.ReadKey(true);
        }
        Message(input.Key == ConsoleKey.Escape ? Edit.CommandCancelled : _cachedText);
        Terminal.SetCursor(cursorPosition);
        inputValue = input.Key == ConsoleKey.Enter ? defaultChar : char.ToLower(input.KeyChar);
        return input.Key != ConsoleKey.Escape;
    }

    /// <summary>
    /// Display a prompt to repeat a command a given number of times.
    /// </summary>
    /// <param name="repeatCount">Set to the repeat count</param>
    /// <param name="command">Initialised with the editing command</param>
    /// <returns>True if a repeat count and command were entered, false if cancelled</returns>
    public bool PromptForRepeat(out int repeatCount, out Command command) {
        repeatCount = 1;
        command = new Command { Id = KeyCommand.KC_NONE };

        Point cursorPosition = Terminal.GetCursor();
        List<char> inputBuffer = new();

        while (true) {
            string prompt = string.Format(Edit.RepeatCount, repeatCount);
            Message(prompt);
            Terminal.SetCursor(prompt.Length, _statusRow);
            ConsoleKeyInfo input = Console.ReadKey(true);
            if (input.Key == ConsoleKey.Escape) {
                repeatCount = 0;
                break;
            }
            if (KeyMap.MapKeyToCommand(input).Id == KeyCommand.KC_REPEAT) {
                repeatCount *= 4;
                continue;
            }
            if (char.IsDigit(input.KeyChar) && inputBuffer.Count < 3) {
                inputBuffer.Add(input.KeyChar);
                repeatCount = Convert.ToInt32(string.Join("", inputBuffer));
                continue;
            }
            command = KeyMap.MapKeyToCommand(input);
            if (command.Id != KeyCommand.KC_NONE) {
                break;
            }
        }
        Message(repeatCount == 0 ? Edit.CommandCancelled : _cachedText);
        Terminal.SetCursor(cursorPosition);
        return repeatCount > 0;
    }

    /// <summary>
    /// Display a prompt on the status bar and prompt for a numeric value.
    /// </summary>
    /// <param name="prompt">Prompt string</param>
    /// <param name="inputValue">The input value</param>
    /// <returns>True if a value was input, false if empty or cancelled</returns>
    public bool PromptForNumber(string prompt, out int inputValue) {
        Point cursorPosition = Terminal.GetCursor();
        Message(prompt);
        Terminal.SetCursor(prompt.Length, _statusRow);

        History history = History.Get(prompt);
        List<char> inputBuffer = new();

        ConsoleKeyInfo input = Console.ReadKey(true);
        while (input.Key != ConsoleKey.Enter) {
            if (input.Key == ConsoleKey.Escape) {
                inputBuffer.Clear();
                break;
            }
            string readyText = input.Key switch {
                ConsoleKey.UpArrow => history.Next(),
                ConsoleKey.DownArrow => history.Previous(),
                _ => string.Empty
            };
            if (input.Key == ConsoleKey.Backspace && inputBuffer.Count > 0) {
                inputBuffer.RemoveAt(inputBuffer.Count - 1);
                Terminal.Write(@" ");
            }
            else if (char.IsDigit(input.KeyChar) && inputBuffer.Count < 6) {
                inputBuffer.Add(input.KeyChar);
                Terminal.Write(_bgColour, _fgColour, input.KeyChar);
            }
            if (readyText != string.Empty) {
                inputBuffer = new List<char>(readyText.ToCharArray());
                Message(prompt + readyText);
                Terminal.SetCursor(prompt.Length + readyText.Length, _statusRow);
            }
            input = Console.ReadKey(true);
        }
        inputValue =  inputBuffer.Count > 0 ? Convert.ToInt32(string.Join("", inputBuffer)) : 0;
        if (inputBuffer.Count > 1) {
            history.Add(inputBuffer);
        }
        Message(input.Key == ConsoleKey.Escape ? Edit.CommandCancelled : _cachedText);
        Terminal.SetCursor(cursorPosition);
        return inputBuffer.Count > 0;
    }

    /// <summary>
    /// Display a prompt on the status bar and input text.
    /// </summary>
    /// <param name="prompt">Prompt string</param>
    /// <param name="inputValue">The input value</param>
    /// <param name="allowFilenameCompletion">True to allow filename completion</param>
    /// <returns>True if a value was input, false if empty or cancelled</returns>
    public bool PromptForInput(string prompt, ref string inputValue, bool allowFilenameCompletion) {
        Point cursorPosition = Terminal.GetCursor();
        Message(prompt);
        Terminal.SetCursor(prompt.Length, _statusRow);

        List<char> inputBuffer = new();
        History history = History.Get(prompt);
        string[]? allfiles = null;
        int allfilesIndex = 0;
        string readyText = inputValue;
        bool selection = false;
        int index = 0;
        ConsoleKeyInfo input;

        do {
            if (readyText != string.Empty) {
                int totalWidth = prompt.Length + readyText.Length;
                inputBuffer = new List<char>(readyText.ToCharArray());
                Selected(prompt.Length, _statusRow, _displayWidth - totalWidth, readyText);
                Terminal.SetCursor(totalWidth, _statusRow);
                index = readyText.Length;
                selection = true;
            }

            input = Console.ReadKey(true);
            if (input.Key == ConsoleKey.Enter) {
                break;
            }
            if (input.Key == ConsoleKey.Escape) {
                inputBuffer.Clear();
                break;
            }
            readyText = input.Key switch {
                ConsoleKey.UpArrow => history.Next(),
                ConsoleKey.DownArrow => history.Previous(),
                _ => string.Empty
            };
            if (input.KeyChar == 172) {
                readyText = _cachedTextInput;
            }
            else {
                switch (input.Key) {
                    case ConsoleKey.Backspace when inputBuffer.Count > 0:
                        int count = selection ? inputBuffer.Count : 1;
                        index -= count;
                        while (count-- > 0) {
                            inputBuffer.RemoveAt(inputBuffer.Count - 1);
                        }
                        break;

                    case ConsoleKey.RightArrow when index < inputBuffer.Count:
                        ++index;
                        break;

                    case ConsoleKey.LeftArrow when index > 0:
                        --index;
                        break;

                    case ConsoleKey.Tab: {
                        if (!allowFilenameCompletion) {
                            break;
                        }
                        if (allfiles == null) {
                            string partialName = string.Join("", inputBuffer) + "*";
                            allfiles = Directory.GetFiles(".", partialName, SearchOption.TopDirectoryOnly);
                            allfilesIndex = 0;
                        }
                        if (allfiles.Length > 0) {
                            string completedName = new FileInfo(allfiles[allfilesIndex++]).Name;
                            if (allfilesIndex == allfiles.Length) {
                                allfilesIndex = 0;
                            }
                            readyText = completedName;
                        }
                        break;
                    }

                    default: {
                        if (!char.IsControl(input.KeyChar) && inputBuffer.Count < 80) {
                            if (selection) {
                                inputBuffer.Clear();
                                index = 0;
                            }
                            inputBuffer.Insert(index++, input.KeyChar);
                            allfiles = null;
                        }
                        break;
                    }
                }

                string text = string.Join("", inputBuffer);
                RenderText(prompt.Length, _statusRow, _displayWidth - prompt.Length, text, _fgColour);
                Terminal.SetCursor(prompt.Length + index, _statusRow);
                selection = false;
            }
        } while (true);
        inputValue =  inputBuffer.Count > 0 ? string.Join("", inputBuffer) : string.Empty;
        if (inputValue.Length > 1) {
            _cachedTextInput = inputValue;
            history.Add(inputBuffer);
        }
        Message(input.Key == ConsoleKey.Escape ? Edit.CommandCancelled : _cachedText);
        Terminal.SetCursor(cursorPosition);
        return inputBuffer.Count > 0;
    }

    /// <summary>
    /// Update the cursor position indicator on the status bar
    /// </summary>
    public void UpdateCursorPosition(int line, int column) {
        _cursorRow = line;
        _cursorColumn = column;
        RenderCursorPosition();
    }

    /// <summary>
    /// Start the background timer that updates the time on the status bar.
    /// </summary>
    private void StartTimer() {
        _clockTimer = new Timer(_ => {
            RenderTime();
        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
    }

    /// <summary>
    /// Display the current status bar message.
    /// </summary>
    private void RenderMessage() {
        RenderText(0, _statusRow, _displayWidth, _currentMessage, _currentColour);
    }

    /// <summary>
    /// Display the current cursor position.
    /// </summary>
    private void RenderCursorPosition() {
        string text = $"Line: {_cursorRow,-3} Col: {_cursorColumn,-3}";
        RenderText(_cursorPositionPosition, _statusRow, _cursorPositionWidth, text, _fgColour);
    }

    /// <summary>
    /// Render the mode indicator field
    /// </summary>
    private void RenderModeIndicator() {
        string modeField;
        if (KeystrokesMode != KeystrokesMode.NONE) {
            modeField = Utilities.GetEnumDescription(KeystrokesMode);
        }
        else {
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            char numLockEnabled = isWindows && Console.NumberLock ? '#' : ' ';
            char capsLockEnabled = isWindows && Console.CapsLock ? '\u2191' : ' ';
            char insertMode = Screen.Config.InsertMode ? ' ' : 'O';
            modeField = $"{numLockEnabled}{capsLockEnabled}{insertMode}";
        }
        RenderText(_modePosition, _statusRow, _modeWidth, modeField, _fgColour);
    }

    /// <summary>
    /// Render the time field of the status bar
    /// </summary>
    private void RenderTime() {
        if (_showClock) {
            char separatorChar = DateTime.Now.Second % 2 == 0 ? ' ' : ':';
            string timeString = DateTime.Now.ToString($"h{separatorChar}mm tt");
            RenderText(_timePosition, _statusRow, _timeWidth, timeString, _fgColour);
        }
    }

    /// <summary>
    /// Write text to the status bar in the normal text colour.
    /// </summary>
    private void RenderText(int x, int y, int w, string text, ConsoleColor fgColor) {
        lock (LockObj) {
            (int savedLeft, int savedTop) = Console.GetCursorPosition();
            Terminal.Write(x, y, w, _bgColour, fgColor, Utilities.SpanBound(text, 0, w));
            Console.SetCursorPosition(savedLeft, savedTop);
        }
    }

    /// <summary>
    /// Write text to the status bar in selected colours where the foreground colour
    /// is used for the background and the background colour for the text.
    /// </summary>
    private void Selected(int x, int y, int w, string text) {
        Terminal.Write(x, y, text.Length, _fgColour, _bgColour, text);
        Terminal.Write(x + text.Length, y, w - text.Length, _bgColour, _fgColour, " ");
    }
}