// JEdit
// Window management
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

namespace JEdit;

public class Window {
    private Rectangle _viewportBounds;
    private Point _viewportOffset;
    private Point _cursor;
    
    /// <summary>
    /// Create an empty window
    /// </summary>
    public Window() {
        Buffer = new Buffer(null);
    }

    /// <summary>
    /// Create a window for the specified buffer
    /// </summary>
    /// <param name="buffer">Associated buffer</param>
    public Window(Buffer buffer) {
        Buffer = buffer;
    }

    /// <summary>
    /// Buffer associated with window
    /// </summary>
    public Buffer Buffer { get; set; }

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
    /// Set the viewport offset
    /// </summary>
    /// <param name="x">Left offset</param>
    /// <param name="y">Top offset</param>
    public void SetViewportOffsets(int x, int y) {
        _viewportOffset = new Point(x, y);
    }

    /// <summary>
    /// Handle a keyboard command.
    /// </summary>
    /// <param name="commandId">Command ID</param>
    /// <returns>Rendering hint</returns>
    public RenderHint Handle(KeyCommand commandId) {
        RenderHint flags = RenderHint.NONE;
        switch (commandId) {
            case KeyCommand.KC_CDOWN:
                flags = CursorDown();
                break;

            case KeyCommand.KC_CUP:
                flags = CursorUp();
                break;

            case KeyCommand.KC_CLEFT:
                flags = CursorLeft();
                break;

            case KeyCommand.KC_CRIGHT:
                flags = CursorRight();
                break;

            case KeyCommand.KC_CLINESTART:
                flags = StartOfCurrentLine();
                break;

            case KeyCommand.KC_CLINEEND:
                flags = EndOfCurrentLine();
                break;
            
            case KeyCommand.KC_CPAGEDOWN:
                flags = PageDown();
                break;
            
            case KeyCommand.KC_CPAGEUP:
                flags = PageUp();
                break;
        }
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
    /// Update this window
    /// </summary>
    public void Render() {

        int i = _viewportOffset.Y;
        int y = _viewportBounds.Top;
        int w = _viewportBounds.Width;

        Point savedCursor = Display.GetCursor();
        string line = Buffer.GetLine(i);
        while (line != null && y < _viewportBounds.Bottom) {
            Display.WriteToNc(_viewportBounds.Left, y++, w, line.Substring(_viewportOffset.X, Math.Min(w, line.Length)));
            line = Buffer.GetLine(++i);
        }
        while (y < _viewportBounds.Bottom) {
            Display.WriteToNc(_viewportBounds.Left, y++, w, string.Empty);
        }
        Display.SetCursor(savedCursor);
        PlaceCursor();
    }

    /// <summary>
    /// Move the cursor down if possible.
    /// </summary>
    private RenderHint CursorDown() {
        RenderHint flags = RenderHint.NONE;
        if (Buffer.LineIndex < Buffer.Length - 1) {
            ++Buffer.LineIndex;
            if (Buffer.Offset > Buffer.GetLine(Buffer.LineIndex).Length) {
                flags |= EndOfCurrentLine();
            }
            if (_cursor.Y < _viewportBounds.Height - 1) {
                ++_cursor.Y;
                flags |= RenderHint.CURSOR;
            } else {
                ++_viewportOffset.Y;
                flags |= RenderHint.REDRAW;
            }
        }
        return flags;
    }

    /// <summary>
    /// Move the cursor up if possible.
    /// </summary>
    private RenderHint CursorUp() {
        RenderHint flags = RenderHint.NONE;
        if (Buffer.LineIndex > 0) {
            --Buffer.LineIndex;
            if (Buffer.Offset > Buffer.GetLine(Buffer.LineIndex).Length) {
                flags |= EndOfCurrentLine();
            }
            if (_cursor.Y > 0) {
                --_cursor.Y;
                flags |= RenderHint.CURSOR;
            } else {
                --_viewportOffset.Y;
                flags |= RenderHint.REDRAW;
            }
        }
        return flags;
    }

    /// <summary>
    /// Move the cursor left.
    /// </summary>
    private RenderHint CursorLeft() {
        if (Buffer.Offset == 0) {
            return EndOfPreviousLine();
        }
        --Buffer.Offset;
        if (_cursor.X > 0) {
            --_cursor.X;
            return RenderHint.CURSOR;
        }
        --_viewportOffset.X;
        return RenderHint.REDRAW;
    }

    /// <summary>
    /// Move the cursor right.
    /// </summary>
    private RenderHint CursorRight() {
        if (Buffer.Offset == Buffer.GetLine(Buffer.LineIndex).Length) {
            return StartOfNextLine();
        }
        ++Buffer.Offset;
        if (_cursor.X < _viewportBounds.Right - 1) {
            ++_cursor.X;
            return RenderHint.CURSOR;
        }
        ++_viewportOffset.X;
        return RenderHint.REDRAW;
    }

    /// <summary>
    /// Move the cursor to the start of the next line if possible.
    /// The cursor is placed at the beginning of the line and we
    /// scroll the viewport to bring it into focus if necessary.
    /// </summary>
    private RenderHint StartOfNextLine() {
        RenderHint flags = RenderHint.NONE;
        if (Buffer.LineIndex < Buffer.Length - 1) {
            ++Buffer.LineIndex;
            if (_cursor.Y < _viewportBounds.Height - 1) {
                ++_cursor.Y;
            } else {
                ++_viewportOffset.Y;
                flags |= RenderHint.REDRAW;
            }
            flags |= StartOfCurrentLine();
            if (flags == RenderHint.NONE) {
                flags |= RenderHint.CURSOR;
            }
        }
        return flags;
    }

    /// <summary>
    /// Move the cursor to the end of the previous line if possible.
    /// The cursor is placed after the last character and if the line
    /// extends beyond the viewport then scroll the viewport to bring
    /// it into view.
    /// </summary>
    private RenderHint EndOfPreviousLine() {
        RenderHint flags = RenderHint.NONE;
        if (Buffer.LineIndex > 0) {
            --Buffer.LineIndex;
            if (_cursor.Y > 0) {
                --_cursor.Y;
            } else {
                --_viewportOffset.Y;
                flags |= RenderHint.REDRAW;
            }
            flags |= EndOfCurrentLine();
            if (flags == RenderHint.NONE) {
                flags |= RenderHint.CURSOR;
            }
        }
        return flags;
    }

    /// <summary>
    /// Move the cursor to the start of the current line.
    /// </summary>
    private RenderHint StartOfCurrentLine() {
        RenderHint flags = RenderHint.NONE;
        Buffer.Offset = 0;
        _cursor.X = Buffer.Offset;
        if (_cursor.X < _viewportBounds.Left) {
            _viewportOffset.X = 0;
            _cursor.X = _viewportBounds.Left;
            flags |= RenderHint.REDRAW;
        } else {
            flags |= RenderHint.CURSOR;
        }
        return flags;
    }

    /// <summary>
    /// Move the cursor to the end of the current line.
    /// </summary>
    private RenderHint EndOfCurrentLine() {
        RenderHint flags = RenderHint.NONE;
        Buffer.Offset = Buffer.GetLine(Buffer.LineIndex).Length;
        _cursor.X = Buffer.Offset;
        if (_cursor.X > _viewportBounds.Width) {
            _viewportOffset.X = _viewportBounds.Width - _cursor.X;
            _cursor.X = _viewportBounds.Width - 1;
            flags |= RenderHint.REDRAW;
        } else {
            flags |= RenderHint.CURSOR;
        }
        return flags;
    }

    /// <summary>
    /// Move the cursor down one page.
    /// </summary>
    private RenderHint PageDown() {
        RenderHint flags = RenderHint.NONE;
        int previousLineIndex = Buffer.LineIndex;
        Buffer.LineIndex = Math.Min(Buffer.LineIndex + _viewportBounds.Height, Buffer.Length - 1);
        if (Buffer.LineIndex != previousLineIndex) {
            if (Buffer.Offset > Buffer.GetLine(Buffer.LineIndex).Length) {
                flags |= EndOfCurrentLine();
            }
            _viewportOffset.Y += Buffer.LineIndex - previousLineIndex;
            flags |= RenderHint.REDRAW;
        }
        return flags;
    }

    /// <summary>
    /// Move the cursor up one page.
    /// </summary>
    private RenderHint PageUp() {
        RenderHint flags = RenderHint.NONE;
        int previousLineIndex = Buffer.LineIndex;
        Buffer.LineIndex = Math.Max(Buffer.LineIndex - _viewportBounds.Height, 0);
        if (Buffer.LineIndex != previousLineIndex) {
            if (Buffer.Offset > Buffer.GetLine(Buffer.LineIndex).Length) {
                flags |= EndOfCurrentLine();
            }
            _viewportOffset.Y -= previousLineIndex - Buffer.LineIndex;
            if (_viewportOffset.Y < 0) {
                _viewportOffset.Y = 0;
            }
            flags |= RenderHint.REDRAW;
        }
        return flags;
    }

    /// <summary>
    /// Place the cursor on screen if it is visible.
    /// </summary>
    private void PlaceCursor() {
        int column = _cursor.X + _viewportBounds.Left;
        int row = _cursor.Y + _viewportBounds.Top;
        if (_viewportBounds.Contains(column, row)) {
            Console.SetCursorPosition(column, row);
        }
    }
}

