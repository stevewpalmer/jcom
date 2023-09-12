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
using JEdit.Resources;

namespace JEdit;

public class Window {
    private Rectangle _viewportBounds;
    private Point _viewportOffset;

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
    public Buffer Buffer { get; }

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
        RenderFrame();
        RenderTitle();
        Render();
    }

    /// <summary>
    /// Handle a keyboard command.
    /// </summary>
    /// <param name="parser">Macro parser</param>
    /// <param name="commandId">Command ID</param>
    /// <returns>Rendering hint</returns>
    public RenderHint Handle(Macro parser, KeyCommand commandId) {
        RenderHint flags = commandId switch {
            KeyCommand.KC_CDOWN => CursorDown(),
            KeyCommand.KC_CUP => CursorUp(),
            KeyCommand.KC_CLEFT => CursorLeft(),
            KeyCommand.KC_CRIGHT => CursorRight(),
            KeyCommand.KC_CLINESTART => StartOfCurrentLine(),
            KeyCommand.KC_CLINEEND => EndOfCurrentLine(),
            KeyCommand.KC_CPAGEDOWN => PageDown(),
            KeyCommand.KC_CFILESTART => FileStart(),
            KeyCommand.KC_CFILEEND => FileEnd(),
            KeyCommand.KC_CWINDOWTOP => WindowTop(),
            KeyCommand.KC_CWINDOWBOTTOM => WindowBottom(),
            KeyCommand.KC_CPAGEUP => PageUp(),
            KeyCommand.KC_CWORDRIGHT => WordRight(),
            KeyCommand.KC_CWORDLEFT => WordLeft(),
            KeyCommand.KC_GOTO => GoToLine(parser),
            KeyCommand.KC_SCREENDOWN => ScreenDown(),
            KeyCommand.KC_SCREENUP => ScreenUp(),
            KeyCommand.KC_WRITEBUFFER => WriteBuffer(),
            _ => RenderHint.NONE
        };
        return ApplyRenderHint(flags);
    }

    /// <summary>
    /// Apply rendering hints to the current window. For those that
    /// are applied, we clear the flag before returning.
    /// </summary>
    /// <param name="flags">Render flags to apply</param>
    /// <returns>Render flags that were not applied</returns>
    public RenderHint ApplyRenderHint(RenderHint flags) {
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
        if (flags.HasFlag(RenderHint.TITLE)) {
            RenderTitle();
            flags &= ~RenderHint.TITLE;
        }
        return flags;
    }

    /// <summary>
    /// Go to input line.
    /// </summary>
    private RenderHint GoToLine(Macro parser) {
        RenderHint flags = RenderHint.NONE;
        if (parser.GetNumber(Edit.GoToLine, out int inputLine)) {
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
    /// Scroll the screen down one line, keeping the cursor in the same
    /// column.
    /// </summary>
    private RenderHint ScreenDown() {
        RenderHint flags = RenderHint.NONE;
        if (_viewportOffset.Y > 0) {
            --_viewportOffset.Y;
            flags = RenderHint.REDRAW;
        }
        return flags | CursorFromOffset();
    }

    /// <summary>
    /// Scroll the screen up one line, keeping the cursor in the same
    /// column.
    /// </summary>
    private RenderHint ScreenUp() {
        RenderHint flags = RenderHint.NONE;
        if (_viewportOffset.Y < Buffer.Length - 2) {
            ++_viewportOffset.Y;
            flags = RenderHint.REDRAW;
        }
        return flags | CursorFromOffset();
    }

    /// <summary>
    /// Write the current buffer to disk.
    /// </summary>
    /// <returns></returns>
    private RenderHint WriteBuffer() {
        Buffer.Write();
        return RenderHint.NONE;
    }

    /// <summary>
    /// Draw the window frame
    /// </summary>
    private void RenderFrame() {
        Rectangle frameRect = _viewportBounds;
        frameRect.Inflate(1, 1);
        Console.SetCursorPosition(frameRect.Left, frameRect.Top);
        Console.Write('╒');
        Console.Write(new string('═', frameRect.Width - 2));
        Console.Write('╕');

        for (int c = frameRect.Top + 1; c < frameRect.Height - 1; c++) {
            Console.SetCursorPosition(frameRect.Left, c);
            Console.Write('│');
            Console.SetCursorPosition(frameRect.Width - 1, c);
            Console.Write('│');
        }

        Console.SetCursorPosition(frameRect.Left, frameRect.Height - 1);
        Console.Write('╘');
        Console.Write(new string('═', frameRect.Width - 2));
        Console.Write('╛');
    }

    /// <summary>
    /// Render the buffer filename at the top of the window.
    /// </summary>
    private void RenderTitle() {
        string title = Buffer.BaseFilename;
        Rectangle frameRect = _viewportBounds;
        frameRect.Inflate(1, 1);
        (int savedLeft, int savedTop) = Console.GetCursorPosition();
        Console.SetCursorPosition(frameRect.Left, frameRect.Top);
        Console.Write('╒');
        Console.Write(new string('═', frameRect.Width - 2));
        Console.Write('╕');
        Console.SetCursorPosition((frameRect.Width - title.Length - 2) / 2, 0);
        Console.ForegroundColor = Screen.Colours.SelectedTitleColour;
        Console.Write($" {title} ");
        Console.ForegroundColor = Screen.Colours.ForegroundColour;
        Console.SetCursorPosition(savedLeft, savedTop);
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
            int left = Math.Min(_viewportOffset.X, line.Length);
            int length = Math.Min(w, line.Length - left);
            Display.WriteToNc(_viewportBounds.Left, y++, w, line.Substring(left, length));
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
            if (CursorRowInViewport < _viewportBounds.Height) {
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
            if (CursorRowInViewport >= 0) {
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
            if (CursorRowInViewport >= _viewportBounds.Height - 1) {
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
            if (CursorRowInViewport < 0) {
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
    /// Move to the start of the buffer.
    /// </summary>
    private RenderHint FileStart() {
        RenderHint flags = RenderHint.NONE;
        if (Buffer.LineIndex > 0 || Buffer.Offset > 0) {
            Buffer.LineIndex = 0;
            Buffer.Offset = 0;
            if (_viewportOffset.Y > 0 || _viewportOffset.X > 0) {
                _viewportOffset.X = 0;
                _viewportOffset.Y = 0;
                flags |= RenderHint.REDRAW;
            } else {
                flags |= RenderHint.CURSOR;
            }
        }
        return flags;
    }
    
    /// <summary>
    /// Move to the end of the buffer.
    /// </summary>
    private RenderHint FileEnd() {
        Buffer.LineIndex = Buffer.Length - 1;
        Buffer.Offset = Buffer.GetLine(Buffer.LineIndex).Length;
        return CursorFromLineIndex();
    }

    /// <summary>
    /// Move to the top of the window
    /// </summary>
    private RenderHint WindowTop() {
        Buffer.LineIndex -= CursorRowInViewport;
        return CursorFromLineIndex();
    }

    /// <summary>
    /// Move to the bottom of the window
    /// </summary>
    private RenderHint WindowBottom() {
        Buffer.LineIndex += _viewportBounds.Height - 1;
        if (Buffer.LineIndex >= Buffer.Length) {
            Buffer.LineIndex = Buffer.Length - 1;
        }
        return CursorFromLineIndex();
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
        if (Buffer.LineIndex < _viewportOffset.Y) {
            _viewportOffset.Y = Buffer.LineIndex;
            flags |= RenderHint.REDRAW;
        }
        else if (Buffer.LineIndex >= _viewportBounds.Height) {
            _viewportOffset.Y = Buffer.LineIndex - (_viewportBounds.Height - 1);
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
    private RenderHint CursorFromOffset() {
        RenderHint flags = RenderHint.NONE;
        if (Buffer.Offset < _viewportOffset.X) {
            _viewportOffset.X = Buffer.Offset;
            flags |= RenderHint.REDRAW;
        }
        else if (Buffer.Offset >= _viewportOffset.X + _viewportBounds.Width - 1) {
            _viewportOffset.X = Buffer.Offset - (_viewportBounds.Width - 1);
            flags |= RenderHint.REDRAW;
        } else {
            flags |= RenderHint.CURSOR;
        }
        return flags;        
    }

    /// <summary>
    /// Return the cursor row position within the viewport (where 0 is the
    /// top row).
    /// </summary>
    private int CursorRowInViewport => Buffer.LineIndex - _viewportOffset.Y;

    /// <summary>
    /// Return the cursor column position within the viewport (where 0 is the
    /// left-most column)
    /// </summary>
    private int CursorColumnInViewport => Buffer.Offset - _viewportOffset.X;

    /// <summary>
    /// Place the cursor on screen if it is visible.
    /// </summary>
    private void PlaceCursor() {
        int column = CursorColumnInViewport + _viewportBounds.Left;
        int row = CursorRowInViewport + _viewportBounds.Top;
        if (_viewportBounds.Contains(column, row)) {
            Console.SetCursorPosition(column, row);
        }
    }
}

