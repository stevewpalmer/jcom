// JCalc
// Window management
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

using System.Drawing;
using JComLib;

namespace JCalc;

public class Window {
    private Rectangle _viewportBounds;

    /// <summary>
    /// Create an empty window
    /// </summary>
    public Window() {
        Sheet = new Sheet(string.Empty);
    }

    /// <summary>
    /// Create a window for the specified sheet
    /// </summary>
    /// <param name="sheet">Associated sheet</param>
    public Window(Sheet sheet) {
        Sheet = sheet;
    }

    /// <summary>
    /// Sheet associated with window
    /// </summary>
    public Sheet Sheet { get; }

    /// <summary>
    /// Set the viewport for the window.
    /// </summary>
    /// <param name="x">Left edge, 0 based</param>
    /// <param name="y">Top edge, 0 based</param>
    /// <param name="width">Width of window</param>
    /// <param name="height">Height of window</param>
    public void SetViewportBounds(int x, int y, int width, int height) {
        _viewportBounds = new Rectangle(x, y, width, height);
    }

    /// <summary>
    /// Refresh this window with a full redraw on screen.
    /// </summary>
    public void Refresh() {
        RenderFrame();
    }

    /// <summary>
    /// Draw the window frame
    /// </summary>
    private void RenderFrame() {
        Rectangle frameRect = _viewportBounds;
        frameRect.Inflate(1, 1);

        RenderTitle();

        Terminal.ForegroundColour = Screen.Colours.ForegroundColour;

        for (int c = frameRect.Top + 1; c < frameRect.Height - 1; c++) {
            Terminal.SetCursor(frameRect.Left, c);
            Terminal.Write($"\u2502{new string(' ', frameRect.Width - 2)}\u2502");
        }

        Terminal.SetCursor(frameRect.Left, frameRect.Height - 1);
        Terminal.Write($"\u2558{new string('═', frameRect.Width - 2)}\u255b");
    }

    /// <summary>
    /// Render the sheet filename at the top of the window. If the window
    /// is narrower than the title then we truncate the title to fit.
    /// </summary>
    private void RenderTitle() {
        string title = Sheet.Name;
        Rectangle frameRect = _viewportBounds;
        frameRect.Inflate(1, 1);
        Point savedCursor = Terminal.GetCursor();

        Terminal.SetCursor(frameRect.Left, frameRect.Top);
        Terminal.ForegroundColour = Screen.Colours.ForegroundColour;
        Terminal.BackgroundColour = Screen.Colours.BackgroundColour;
        Terminal.Write($"\u2552{new string('═', frameRect.Width - 2)}\u2555");

        int realLength = Math.Min(title.Length, frameRect.Width - 4);
        Terminal.SetCursor((frameRect.Width - realLength - 2) / 2, 0);
        Terminal.ForegroundColour = Screen.Colours.SelectedTitleColour;
        Terminal.Write($@" {title[..realLength]} ");

        Terminal.SetCursor(savedCursor);
    }
}