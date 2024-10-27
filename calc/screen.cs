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

using JComLib;

namespace JCalc;

public static class Screen {
    private static readonly List<Window> _windowList = new();
    private static Window? _activeWindow;

    /// <summary>
    /// Configured colours
    /// </summary>
    public static Colours Colours { get; private set; } = new();

    /// <summary>
    /// Open the main window.
    /// </summary>
    public static void Open() {
        Terminal.Open();
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
    }

    /// <summary>
    /// Prompt for the initial sheet to edit, replacing the one in the
    /// existing window.
    /// </summary>
    /// <returns>True if file retrieved, false if the user cancelled the prompt</returns>
    public static bool GetInitialFile() {
        string inputValue = string.Empty;
        return inputValue != string.Empty;
    }

    /// <summary>
    /// Add a window to the window list. This will not make the window
    /// active.
    /// </summary>
    public static void AddWindow(Window theWindow) {
        if (theWindow.Sheet.NewFile) {
            string message = theWindow.Sheet.Filename == string.Empty ? "New File" : string.Format("Calc.NewFileWarning", theWindow.Sheet.Name);
        }
        _windowList.Add(theWindow);
        theWindow.SetViewportBounds(1, 1, Terminal.Width - 2, Terminal.Height - 3);
    }

    /// <summary>
    /// Activate a window by its index
    /// </summary>
    /// <param name="index">Index of the window to be activated</param>
    public static void ActivateWindow(int index) {
        _activeWindow = _windowList[index];
        _activeWindow.Refresh();
    }
}