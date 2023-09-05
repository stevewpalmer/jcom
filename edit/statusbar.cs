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

    /// <summary>
    /// Default render of status bar. Show the application title and start
    /// the thread that displays the time.
    /// </summary>
    public void InitialRender() {
        _statusRow = Console.WindowHeight - 1;
        _cachedText = string.Empty;
        _timeWidth = 8;
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
    /// Display a prompt on the status bar and prompt for a numeric value.
    /// </summary>
    /// <param name="prompt">Prompt string</param>
    /// <param name="inputValue">The input value</param>
    /// <returns>True if a value was input, false if empty or cancelled</returns>
    public bool PromptForNumber(string prompt, out int inputValue) {
        Point cursorPosition = Display.WriteToNc(0, _statusRow, _displayWidth, prompt);
        Display.SetCursor(new Point(prompt.Length, _statusRow));
        ConsoleKeyInfo input = Console.ReadKey(true);
        List<char> inputBuffer = new();
        while (input.Key != ConsoleKey.Enter) {
            if (input.Key == ConsoleKey.Escape) {
                inputBuffer.Clear();
                break;
            }
            if (input.Key == ConsoleKey.Backspace && inputBuffer.Count > 0) {
                inputBuffer.RemoveAt(inputBuffer.Count - 1);
                Console.Write("\b \b");
            }
            else if (char.IsDigit(input.KeyChar) && inputBuffer.Count < 6) {
                inputBuffer.Add(input.KeyChar);
                Console.Write(input.KeyChar);
            }
            input = Console.ReadKey(true);
        }
        inputValue =  inputBuffer.Count > 0 ? Convert.ToInt32(string.Join("", inputBuffer)) : 0;
        Display.WriteTo(0, _statusRow, _displayWidth, input.Key == ConsoleKey.Escape ? "Command cancelled." : _cachedText);
        Display.SetCursor(cursorPosition);
        return inputBuffer.Count > 0;
    }

    /// <summary>
    /// Update the cursor position indicator on the status bar
    /// </summary>
    public void UpdateCursorPosition(int line, int column) {
        string text = $"Line:{line} Col:{column}";
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
            string timeString = DateTime.Now.ToString("h:mm tt");
            Display.WriteTo(_timePosition, _statusRow, _timeWidth, timeString);
        }, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
    }
}