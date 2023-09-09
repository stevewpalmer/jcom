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

namespace JEdit;

public class StatusBar {
    private static int _statusRow;
    private string _cachedText;
    private static int _timeWidth;
    private int _cursorPositionWidth;
    private int _displayWidth;
    private static int _timePosition;
    private int _cursorPositionPosition;
    private string _cachedTextInput;

    /// <summary>
    /// Default render of status bar. Show the application title and start
    /// the thread that displays the time.
    /// </summary>
    public void InitialRender() {
        _statusRow = Console.WindowHeight - 1;
        _cachedText = string.Empty;
        _timeWidth = 11;
        _cursorPositionWidth = 18;
        _displayWidth = Console.WindowWidth - (_timeWidth + _cursorPositionWidth);
        _timePosition = Console.WindowWidth - _timeWidth;
        _cursorPositionPosition = _timePosition - _cursorPositionWidth;
        RenderVersion();
        RenderTime();
    }

    /// <summary>
    /// Display a message on the status bar.
    /// </summary>
    /// <param name="message">Message string</param>
    public void Message(string message) {
        Display.WriteTo(0, _statusRow, _displayWidth, message);
    }

    /// <summary>
    /// Display a prompt on the status bar and prompt for a keystroke input.
    /// </summary>
    /// <returns>True if a value was input, false if empty or cancelled</returns>
    public bool Prompt(string prompt, char[] validInput, char defaultChar, out char inputValue) {

        prompt = prompt.Replace("@@", $"[{string.Join("",validInput)}]");
        Point cursorPosition = Display.WriteToNc(0, _statusRow, _displayWidth, prompt);
        Display.SetCursor(new Point(prompt.Length, _statusRow));
        ConsoleKeyInfo input = Console.ReadKey(true);
        while (!validInput.Contains(char.ToLower(input.KeyChar))) {
            if (input.Key is ConsoleKey.Enter or ConsoleKey.Escape) {
                break;
            }
            input = Console.ReadKey(true);
        }
        Display.WriteTo(0, _statusRow, _displayWidth, input.Key == ConsoleKey.Escape ? "Command cancelled." : _cachedText);
        Display.SetCursor(cursorPosition);
        inputValue = input.Key == ConsoleKey.Enter ? defaultChar : char.ToLower(input.KeyChar);
        return input.Key != ConsoleKey.Escape;
    }

    /// <summary>
    /// Display a prompt to repeat a command a given number of times.
    /// </summary>
    /// <param name="repeatCount">Set to the repeat count</param>
    /// <param name="commandId">Set to the command Id</param>
    /// <returns>True if a repeat count and command were entered, false if cancelled</returns>
    public bool PromptForRepeat(out int repeatCount, out KeyCommand commandId) {
        repeatCount = 1;
        commandId = KeyCommand.KC_NONE;

        Point cursorPosition;
        List<char> inputBuffer = new();

        while (true) {
            string prompt = $"Repeat count = {repeatCount}: type count or command.";
            cursorPosition = Display.WriteToNc(0, _statusRow, _displayWidth, prompt);
            Display.SetCursor(new Point(prompt.Length, _statusRow));
            ConsoleKeyInfo input = Console.ReadKey(true);
            if (input.Key == ConsoleKey.Escape) {
                repeatCount = 0;
                break;
            }
            if (input.Key == ConsoleKey.R && input.Modifiers.HasFlag(ConsoleModifiers.Control)) {
                repeatCount *= 4;
                continue;
            }
            if (char.IsDigit(input.KeyChar) && inputBuffer.Count < 3) {
                inputBuffer.Add(input.KeyChar);
                repeatCount = Convert.ToInt32(string.Join("", inputBuffer));
                continue;
            }
            commandId = KeyMap.MapKeyToCommand(input);
            if (commandId != KeyCommand.KC_NONE) {
                break;
            }
        }
        Display.WriteTo(0, _statusRow, _displayWidth, repeatCount == 0 ? "Command cancelled." : _cachedText);
        Display.SetCursor(cursorPosition);
        return repeatCount > 0;
    }

    /// <summary>
    /// Display a prompt on the status bar and prompt for a numeric value.
    /// </summary>
    /// <param name="prompt">Prompt string</param>
    /// <param name="inputValue">The input value</param>
    /// <returns>True if a value was input, false if empty or cancelled</returns>
    public bool PromptForNumber(string prompt, out int inputValue) {
        Point cursorPosition = Display.WriteToNc(0, _statusRow, _displayWidth, prompt);
        Display.SetCursor(new Point(prompt.Length, _statusRow));

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
                _ => null
            };
            if (input.Key == ConsoleKey.Backspace && inputBuffer.Count > 0) {
                inputBuffer.RemoveAt(inputBuffer.Count - 1);
                Console.Write("\b \b");
            }
            else if (char.IsDigit(input.KeyChar) && inputBuffer.Count < 6) {
                inputBuffer.Add(input.KeyChar);
                Console.Write(input.KeyChar);
            }
            if (readyText != null) {
                inputBuffer = new List<char>(readyText.ToCharArray());
                Display.SetCursor(new Point(prompt.Length, _statusRow));
                Display.WriteToNc(prompt.Length, _statusRow, _displayWidth - prompt.Length, readyText);
                Display.SetCursor(new Point(prompt.Length + readyText.Length, _statusRow));
            }
            input = Console.ReadKey(true);
        }
        inputValue =  inputBuffer.Count > 0 ? Convert.ToInt32(string.Join("", inputBuffer)) : 0;
        if (inputBuffer.Count > 1) {
            history.Add(inputBuffer);
        }
        Display.WriteTo(0, _statusRow, _displayWidth, input.Key == ConsoleKey.Escape ? "Command cancelled." : _cachedText);
        Display.SetCursor(cursorPosition);
        return inputBuffer.Count > 0;
    }

    /// <summary>
    /// Display a prompt on the status bar and input text.
    /// </summary>
    /// <param name="prompt">Prompt string</param>
    /// <param name="inputValue">The input value</param>
    /// <param name="allowFilenameCompletion">True to allow filename completion</param>
    /// <returns>True if a value was input, false if empty or cancelled</returns>
    public bool PromptForInput(string prompt, out string inputValue, bool allowFilenameCompletion) {
        Point cursorPosition = Display.WriteToNc(0, _statusRow, _displayWidth, prompt);
        Display.SetCursor(new Point(prompt.Length, _statusRow));

        List<char> inputBuffer = new();
        History history = History.Get(prompt);
        string[] allfiles = null;
        int allfilesIndex = 0;

        ConsoleKeyInfo input = Console.ReadKey(true);
        while (input.Key != ConsoleKey.Enter) {
            if (input.Key == ConsoleKey.Escape) {
                inputBuffer.Clear();
                break;
            }
            string readyText = input.Key switch {
                ConsoleKey.UpArrow => history.Next(),
                ConsoleKey.DownArrow => history.Previous(),
                _ => null
            };
            if (input.KeyChar == 172) {
                readyText = _cachedTextInput;
            }
            else switch (input.Key) {
                case ConsoleKey.Backspace when inputBuffer.Count > 0:
                    inputBuffer.RemoveAt(inputBuffer.Count - 1);
                    Console.Write("\b \b");
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
                        string completedName = Buffer.GetBaseFilename(allfiles[allfilesIndex++]);
                        if (allfilesIndex == allfiles.Length) {
                            allfilesIndex = 0;
                        }
                        readyText = completedName;
                    }
                    break;
                }
                default: {
                    if (!char.IsControl(input.KeyChar) && inputBuffer.Count < 80) {
                        inputBuffer.Add(input.KeyChar);
                        Console.Write(input.KeyChar);
                        allfiles = null;
                    }
                    break;
                }
            }
            if (readyText != null) {
                inputBuffer = new List<char>(readyText.ToCharArray());
                Display.SetCursor(new Point(prompt.Length, _statusRow));
                Display.WriteToNc(prompt.Length, _statusRow, _displayWidth - prompt.Length, readyText);
                Display.SetCursor(new Point(prompt.Length + readyText.Length, _statusRow));
            }
            input = Console.ReadKey(true);
        }
        inputValue =  inputBuffer.Count > 0 ? string.Join("", inputBuffer) : string.Empty;
        if (inputValue.Length > 1) {
            _cachedTextInput = inputValue;
            history.Add(inputBuffer);
        }
        Display.WriteTo(0, _statusRow, _displayWidth, input.Key == ConsoleKey.Escape ? "Command cancelled." : _cachedText);
        Display.SetCursor(cursorPosition);
        return inputBuffer.Count > 0;
    }

    /// <summary>
    /// Update the cursor position indicator on the status bar
    /// </summary>
    public void UpdateCursorPosition(int line, int column) {
        string text = $"Line: {line,-3} Col: {column,-3}";
        Display.WriteTo(_cursorPositionPosition, _statusRow, _cursorPositionWidth, text);
    }

    /// <summary>
    /// Display the program version.
    /// </summary>
    public void RenderVersion() {
        Display.WriteTo(0, _statusRow, _displayWidth, AppTitle);
    }

    /// <summary>
    /// Return the application title string
    /// </summary>
    private static string AppTitle => $"{AssemblySupport.AssemblyDescription} v{AssemblySupport.AssemblyVersion} - {AssemblySupport.AssemblyCopyright}";

    /// <summary>
    /// Render the time field of the status bar
    /// </summary>
    private static void RenderTime() {
        Timer _ = new(_ => {
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            char numLockEnabled = isWindows && Console.NumberLock ? '#' : ' ';
            char capsLockEnabled = isWindows && Console.CapsLock ? '\u2191' : ' ';
            char separatorChar = DateTime.Now.Second % 2 == 0 ? ' ' : ':';
            string timeString = DateTime.Now.ToString($"{capsLockEnabled}{numLockEnabled} h{separatorChar}mm tt");
            Display.WriteTo(_timePosition, _statusRow, _timeWidth, timeString);
        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
    }
}