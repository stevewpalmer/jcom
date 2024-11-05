// JCalc
// Status bar
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

public class StatusBar {

    private readonly int _statusBarRow;
    private readonly int _statusBarWidth;
    private string _filename;
    private ConsoleColor _bgColour;
    private ConsoleColor _fgColour;

    /// <summary>
    /// Set the status bar position and width
    /// </summary>
    public StatusBar() {
        _statusBarRow = Terminal.Height - 1;
        _statusBarWidth = Terminal.Width;
        _filename = string.Empty;
    }

    /// <summary>
    /// Status bar height
    /// </summary>
    public const int Height = 1;

    /// <summary>
    /// Render the status bar
    /// </summary>
    public void Refresh() {
        _fgColour = Screen.Colours.NormalMessageColour;
        _bgColour = Screen.Colours.BackgroundColour;
        RenderFilename();
    }

    /// <summary>
    /// Update the filename shown on the status bar
    /// </summary>
    /// <param name="newFilename">Filename to display</param>
    public void UpdateFilename(string newFilename) {
        _filename = newFilename;
        RenderFilename();
    }

    /// <summary>
    /// Display the filename on the status bar
    /// </summary>
    private void RenderFilename() {
        Terminal.WriteText(0, _statusBarRow, _statusBarWidth, $"Calc: {_filename}", _fgColour, _bgColour);
    }
}