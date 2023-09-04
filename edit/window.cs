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
    /// Make this window active
    /// </summary>
    public void SetActive() {
        Screen.RenderTitle(Buffer.BaseFilename);
        Render();
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
            
            case KeyCommand.KC_CFILESTART:
                flags = FileStart();
                break;
            
            case KeyCommand.KC_CFILEEND:
                flags = FileEnd();
                break;
            
            case KeyCommand.KC_CPAGEUP:
                flags = PageUp();
                break;
            
            case KeyCommand.KC_CWORDRIGHT:
                flags = WordRight();
                break;
            
            case KeyCommand.KC_CWORDLEFT:
                flags = WordLeft();
                break;
            
            case KeyCommand.KC_GOTO:
                flags = GoToLine();
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
    /// Go to input line.
    /// </summary>
    private RenderHint GoToLine() {
        RenderHint flags = RenderHint.NONE;
        if (Screen.StatusBar.PromptForNumber("Go to line: ", out int inputLine)) {
            if (inputLine > Buffer.Length) {
                inputLine = Buffer.Length;
            }
            if (inputLine < 1) {
                inputLine = 1;
            }
            Buffer.LineIndex = inputLine - 1;
            flags |= CursorFromLineIndex();
        }
        return flags;
    }

    /// <summary>
    /// Update this window
    /// </summary>
    private void Render() {

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
        return CursorFromOffset();
    }

    /// <summary>
    /// Move the cursor right.
    /// </summary>
    private RenderHint CursorRight() {
        if (Buffer.Offset == Buffer.GetLine(Buffer.LineIndex).Length) {
            return StartOfNextLine();
        }
        ++Buffer.Offset;
        return CursorFromOffset();
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
        Buffer.Offset = 0;
        return CursorFromOffset();
    }

    /// <summary>
    /// Move the cursor to the end of the current line.
    /// </summary>
    private RenderHint EndOfCurrentLine() {
        Buffer.Offset = Buffer.GetLine(Buffer.LineIndex).Length;
        return CursorFromOffset();
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
    /// Move to the start of the file.
    /// </summary>
    private RenderHint FileStart() {
        Buffer.LineIndex = 0;
        Buffer.Offset = 0;
        return CursorFromOffset();
    }
    
    /// <summary>
    /// Move to the end of the file.
    /// </summary>
    private RenderHint FileEnd() {
        Buffer.LineIndex = Buffer.Length - 1;
        Buffer.Offset = Buffer.GetLine(Buffer.LineIndex).Length - 1;
        return CursorFromOffset();
    }

    /// <summary>
    /// Move to the next word to the right of the cursor. If the cursor is
    /// within a word, it moves over the remainder of the word and subsequent
    /// spaces to the start of the next word. If it is within spaces, it moves
    /// to the start of the next word. If it is on the last word on a line, it
    /// moves to the start of the first word on the next line.
    /// </summary>
    private RenderHint WordRight() {
        RenderHint flags = RenderHint.NONE;
        if (Buffer.Offset == Buffer.GetLine(Buffer.LineIndex).Length) {
            flags = StartOfNextLine();
        }
        char[] text = Buffer.GetLine(Buffer.LineIndex).ToCharArray();
        int offset = Buffer.Offset;
        while (offset < text.Length && !char.IsWhiteSpace(text[offset])) {
            ++offset;
        }
        while (offset < text.Length && char.IsWhiteSpace(text[offset])) {
            ++offset;
        }
        Buffer.Offset = offset;
        return flags | CursorFromOffset();
    }

    /// <summary>
    /// Move to the previous word to the left of the cursor. If the cursor is
    /// within a word, it moves to the start of the word. If it is within spaces,
    /// it moves past the spaces to the end of the previous word. If it is on
    /// the first word on a line, it moves to the end of the first word on the
    /// previous line.
    /// </summary>
    private RenderHint WordLeft() {
        RenderHint flags = RenderHint.NONE;
        if (Buffer.Offset == 0) {
            flags = EndOfPreviousLine();
        }
        char[] text = Buffer.GetLine(Buffer.LineIndex).ToCharArray();
        int offset = Buffer.Offset;
        while (offset > 0 && offset < text.Length && !char.IsWhiteSpace(text[offset])) {
            --offset;
        }
        while (offset > 0 && offset < text.Length && char.IsWhiteSpace(text[offset])) {
            --offset;
        }
        Buffer.Offset = offset;
        return flags | CursorFromOffset();
    }

    /// <summary>
    /// Update the physical cursor position in the current viewport
    /// based on the buffer line index.
    /// </summary>
    private RenderHint CursorFromLineIndex() {
        RenderHint flags = RenderHint.NONE;
        if (Buffer.Offset > Buffer.GetLine(Buffer.LineIndex).Length) {
            flags |= EndOfCurrentLine();
        }
        _cursor.Y = Buffer.LineIndex;
        if (_cursor.Y < _viewportOffset.Y) {
            _viewportOffset.Y = _cursor.Y;
            _cursor.Y = 0;
            flags |= RenderHint.REDRAW;
        }
        else if (_cursor.Y >= _viewportBounds.Height) {
            _viewportOffset.Y = _cursor.Y - (_viewportBounds.Height - 1);
            _cursor.Y = _viewportBounds.Height - 1;
            flags |= RenderHint.REDRAW;
        } else {
            flags |= RenderHint.CURSOR;
        }
        return flags;
    }

    /// <summary>
    /// Update the physical cursor position in the current viewport
    /// based on the buffer offset.
    /// </summary>
    /// <returns></returns>
    private RenderHint CursorFromOffset() {
        RenderHint flags = RenderHint.NONE;
        _cursor.X = Buffer.Offset;
        if (_cursor.X > _viewportBounds.Width) {
            _viewportOffset.X = _viewportBounds.Width - _cursor.X;
            _cursor.X = _viewportBounds.Width - 1;
            flags |= RenderHint.REDRAW;
        }
        else if (_cursor.X < _viewportBounds.Left) {
            _viewportOffset.X = 0;
            _cursor.X = 0;
            flags |= RenderHint.REDRAW;
        } else {
            flags |= RenderHint.CURSOR;
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

