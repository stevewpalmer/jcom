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

    /// <summary>
    /// Status bar height
    /// </summary>
    public const int Height = 1;

    private readonly int _statusBarRow;
    private readonly int _statusBarWidth;
    private int _bgColour;
    private int _fgColour;
    private string _filename;
    private int _maxFilenameWidth;
    private string _message;

    /// <summary>
    /// Set the status bar position and width
    /// </summary>
    public StatusBar() {
        _statusBarRow = Terminal.Height - Height;
        _statusBarWidth = Terminal.Width;
        _filename = string.Empty;
        _message = string.Empty;
    }

    /// <summary>
    /// Render the status bar
    /// </summary>
    public void Refresh() {
        _fgColour = Screen.Colours.NormalMessageColour;
        _bgColour = Screen.Colours.BackgroundColour;
        ClearRow();
        RenderMessage();
        RenderFilename();
    }

    /// <summary>
    /// Clear the message on the status bar.
    /// </summary>
    public void ClearMessage() {
        _message = string.Empty;
        RenderMessage();
    }

    /// <summary>
    /// Write a message to the status bar
    /// </summary>
    /// <param name="newMessage">Message to display</param>
    public void Message(string newMessage) {
        _message = newMessage;
        RenderMessage();
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
    /// Clear the status bar row.
    /// </summary>
    private void ClearRow() {
        Terminal.Write(0, _statusBarRow, _statusBarWidth, _fgColour, _bgColour, string.Empty);
    }

    /// <summary>
    /// Display the message on the status bar.
    /// </summary>
    private void RenderMessage() {
        int width = _statusBarWidth - _maxFilenameWidth;
        Terminal.Write(0, _statusBarRow, width, _fgColour, _bgColour, _message);
    }

    /// <summary>
    /// Display the filename on the status bar
    /// </summary>
    private void RenderFilename() {
        _maxFilenameWidth = Math.Max(_maxFilenameWidth, _filename.Length);
        if (_maxFilenameWidth > 0) {
            int pos = _statusBarWidth - _maxFilenameWidth;
            Terminal.Write(pos, _statusBarRow, _maxFilenameWidth, _fgColour, _bgColour, _filename.PadLeft(_maxFilenameWidth));
        }
    }
}