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
    private Rectangle _sheetBounds;
    private Point _scrollOffset = Point.Empty;

    /// <summary>
    /// Create an empty window
    /// </summary>
    public Window() {
        Sheet = new Sheet(1, string.Empty);
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
    /// Set the viewport and sheet bounds for the window.
    /// </summary>
    /// <param name="x">Left edge, 0 based</param>
    /// <param name="y">Top edge, 0 based</param>
    /// <param name="width">Width of window</param>
    /// <param name="height">Height of window</param>
    public void SetViewportBounds(int x, int y, int width, int height) {
        _viewportBounds = new Rectangle(x, y, width, height);
        _sheetBounds = new Rectangle(3, 1, width - 3, height - 5);
    }

    /// <summary>
    /// Refresh this window with a full redraw on screen.
    /// </summary>
    public void Refresh() {
        RenderFrame();
        Render();
    }

    /// <summary>
    /// Apply the render hint flags to the current window. On completion,
    /// return just the flags that were not applied.
    /// </summary>
    /// <returns>Unapplied render hint</returns>
    private RenderHint ApplyRenderHint(RenderHint flags) {
        if (flags.HasFlag(RenderHint.REDRAW)) {
            Render();
            flags &= ~RenderHint.REDRAW;
            flags |= RenderHint.CURSOR_STATUS;
        }
        if (flags.HasFlag(RenderHint.CURSOR)) {
            PlaceCursor();
            flags &= ~RenderHint.CURSOR;
            flags |= RenderHint.CURSOR_STATUS;
        }
        return flags;
    }

    /// <summary>
    /// Draw the window content showing the row and column headings at their
    /// scroll offset.
    /// </summary>
    private void RenderFrame() {

        Rectangle frameRect = _viewportBounds;

        // Sheet number
        Terminal.SetCursor(frameRect.Left, frameRect.Top);
        Terminal.ForegroundColour = Screen.Colours.BackgroundColour;
        Terminal.BackgroundColour = Screen.Colours.ForegroundColour;
        Terminal.Write($@"#{Sheet.SheetNumber}");
        Terminal.ForegroundColour = Screen.Colours.ForegroundColour;
        Terminal.BackgroundColour = Screen.Colours.BackgroundColour;

        // Column numbers
        int columnNumber = 1 + _scrollOffset.X;
        int x = frameRect.Left + 3;
        while (x < frameRect.Right && columnNumber <= Sheet.MaxColumns) {
            int width = Sheet.ColumnWidth(columnNumber);
            int space = Math.Min(width, frameRect.Width - x);
            Terminal.SetCursor(x, frameRect.Top);
            Terminal.Write(Utilities.CentreString(columnNumber.ToString(), width)[..space]);
            x += width;
            columnNumber++;
        }

        // Row numbers
        int y = 1;
        int rowNumber = 1 + _scrollOffset.Y;
        while (y < frameRect.Bottom - CommandBar.Height && rowNumber <= Sheet.MaxRows) {
            Terminal.SetCursor(frameRect.Left, y);
            Terminal.Write(rowNumber.ToString().PadLeft(3));
            y += 1;
            rowNumber++;
        }
    }

    /// <summary>
    /// Draw the sheet in its entirety
    /// </summary>
    private void Render() {
        RenderFrame();
        PlaceCursor();
    }

    /// <summary>
    /// Draw the cursor
    /// </summary>
    private void PlaceCursor() {
        ShowCell(Sheet.Column, Sheet.Row, Screen.Colours.BackgroundColour, Screen.Colours.ForegroundColour);
    }

    /// <summary>
    /// Draw the cell at the specified column and row in the given foreground and
    /// background colours
    /// </summary>
    /// <param name="column">1-based column offset</param>
    /// <param name="row">1-based row offset</param>
    /// <param name="fgColour">Foreground colour</param>
    /// <param name="bgColour">Background colour</param>
    private void ShowCell(int column, int row, ConsoleColor fgColour, ConsoleColor bgColour) {
        int y = _sheetBounds.Top;
        for (int d = _scrollOffset.Y; d < row - 1; d++) {
            y += Sheet.RowHeight(d + 1);
        }
        Terminal.ForegroundColour = fgColour;
        Terminal.BackgroundColour = bgColour;
        Sheet.DrawCell(Sheet.Column, Sheet.Row, GetXPositionOfCell(column), y);
    }

    /// <summary>
    /// Return the X position of the specified column
    /// </summary>
    /// <param name="column">1-based column index</param>
    /// <returns>X position of column</returns>
    private int GetXPositionOfCell(int column) {
        int x = _sheetBounds.Left;
        for (int c = _scrollOffset.X; c < column - 1; c++) {
            x += Sheet.ColumnWidth(c + 1);
        }
        return x;
    }

    /// <summary>
    /// Handle a keyboard command.
    /// </summary>
    /// <param name="command">Editing command</param>
    /// <returns>Render hint</returns>
    public RenderHint HandleCommand(KeyCommand command) {
        RenderHint flags = command switch {
            KeyCommand.KC_RIGHT => CursorRight(),
            KeyCommand.KC_LEFT => CursorLeft(),
            _ => RenderHint.NONE
        };
        return ApplyRenderHint(flags);
    }

    /// <summary>
    /// Move the cell selector left one cell
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint CursorLeft() {
        RenderHint flags = RenderHint.NONE;
        if (Sheet.Column > 1) {
            ShowCell(Sheet.Column, Sheet.Row, Screen.Colours.ForegroundColour, Screen.Colours.BackgroundColour);
            --Sheet.Column;
            if (Sheet.Column <= _scrollOffset.X) {
                --_scrollOffset.X;
                flags |= RenderHint.REDRAW;
            }
            else {
                flags |= RenderHint.CURSOR;
            }
        }
        return flags;
    }

    /// <summary>
    /// Move the cell selector right one cell
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint CursorRight() {
        RenderHint flags = RenderHint.NONE;
        if (Sheet.Column < Sheet.MaxColumns) {
            ShowCell(Sheet.Column, Sheet.Row, Screen.Colours.ForegroundColour, Screen.Colours.BackgroundColour);
            ++Sheet.Column;
            if (GetXPositionOfCell(Sheet.Column) + Sheet.ColumnWidth(Sheet.Column) >= _sheetBounds.Right) {
                ++_scrollOffset.X;
                flags |= RenderHint.REDRAW;
            }
            else {
                flags |= RenderHint.CURSOR;
            }
        }
        return flags;
    }
}