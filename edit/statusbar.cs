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
        Display.WriteTo(0, _statusRow, _displayWidth, AppTitle);
        RenderTime();
    }

    /// <summary>
    /// Display a prompt on the status bar and prompt for a keystroke input.
    /// </summary>
    /// <param name="prompt">Prompt string</param>
    /// <returns>The console key input</returns>
    public ConsoleKey Prompt(string prompt) {
        Point cursorPosition = Display.WriteToNc(0, _statusRow, _displayWidth, prompt);
        Display.SetCursor(new Point(prompt.Length, _statusRow));
        ConsoleKey input = Console.ReadKey().Key;
        Display.WriteTo(0, _statusRow, _displayWidth, _cachedText);
        Display.SetCursor(cursorPosition);
        return input;
    }

    /// <summary>
    /// Update the cursor position indicator on the status bar
    /// </summary>
    public void UpdateCursorPosition(int line, int column) {
        string text = $"Line:{line} Col:{column}";
        Display.WriteTo(_cursorPositionPosition, _statusRow, _cursorPositionWidth, text);
    }

    /// <summary>
    /// Return the application title string
    /// </summary>
    private static string AppTitle => $"{AssemblySupport.AssemblyDescription} v{AssemblySupport.AssemblyVersion} - {AssemblySupport.AssemblyCopyright}";

    /// <summary>
    /// Render the time field of the status bar
    /// </summary>
    private static void RenderTime() {
        Timer timer = new((e) => {
            string timeString = DateTime.Now.ToString("h:mm tt");
            Display.WriteTo(_timePosition, _statusRow, _timeWidth, timeString);
        }, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
    }
}