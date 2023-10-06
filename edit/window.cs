﻿// JEdit
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
using System.Text;
using JComLib;
using JEdit.Resources;
using JEdit.Resources;

namespace JEdit;

/// <summary>
/// Block actions
/// </summary>
[Flags]
public enum BlockAction {

    /// <summary>
    /// Copy the block to the scrap buffer
    /// </summary>
    COPY = 1,

    /// <summary>
    /// Delete the block
    /// </summary>
    DELETE = 2,

    /// <summary>
    /// Copy the block and then delete
    /// </summary>
    COPY_AND_DELETE = COPY | DELETE
}

public class Window {
    private Rectangle _viewportBounds;
    private Point _viewportOffset;
    private MarkMode _markMode;
    private Point _markAnchor;
    private Point _lastMarkPoint;

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
    /// Refresh this window with a full redraw on screen.
    /// </summary>
    public void Refresh() {
        RenderFrame();
        Render(RenderHint.REDRAW);
    }

    /// <summary>
    /// Handle a keyboard command.
    /// </summary>
    /// <param name="parser">Macro parser</param>
    /// <param name="commandId">Command ID</param>
    /// <returns>Rendering hint</returns>
    public RenderHint HandleCommand(Macro parser, KeyCommand commandId) {
        RenderHint flags = commandId switch {
            KeyCommand.KC_CDOWN => CursorDown(),
            KeyCommand.KC_CENTRE => CenterWindow(),
            KeyCommand.KC_CFILEEND => FileEnd(),
            KeyCommand.KC_CFILESTART => FileStart(),
            KeyCommand.KC_CLEFT => CursorLeft(),
            KeyCommand.KC_CLINEEND => EndOfCurrentLine(),
            KeyCommand.KC_CLINESTART => StartOfCurrentLine(),
            KeyCommand.KC_COPY => HandleBlock(BlockAction.COPY),
            KeyCommand.KC_CPAGEDOWN => PageDown(),
            KeyCommand.KC_CPAGEUP => PageUp(),
            KeyCommand.KC_CRIGHT => CursorRight(),
            KeyCommand.KC_CTOBOTTOM => LineToBottom(),
            KeyCommand.KC_CTOTOP => LineToTop(),
            KeyCommand.KC_CUP => CursorUp(),
            KeyCommand.KC_CUT => HandleBlock(BlockAction.COPY_AND_DELETE),
            KeyCommand.KC_CWINDOWBOTTOM => WindowBottom(),
            KeyCommand.KC_CWINDOWTOP => WindowTop(),
            KeyCommand.KC_CWORDLEFT => WordLeft(),
            KeyCommand.KC_CWORDRIGHT => WordRight(),
            KeyCommand.KC_DELETELINE => DeleteLine(),
            KeyCommand.KC_DELETETOEND => DeleteToEndOfLine(),
            KeyCommand.KC_DELETETOSTART => DeleteToStartOfLine(),
            KeyCommand.KC_GOTO => GoToLine(parser),
            KeyCommand.KC_MARK => Mark(MarkMode.CHARACTER),
            KeyCommand.KC_MARKCOLUMN => Mark(MarkMode.COLUMN),
            KeyCommand.KC_MARKLINE => Mark(MarkMode.LINE),
            KeyCommand.KC_SCREENDOWN => ScreenDown(),
            KeyCommand.KC_SCREENUP => ScreenUp(),
            KeyCommand.KC_WRITEBUFFER => WriteBuffer(),
            _ => RenderHint.NONE
        };
        return ApplyRenderHint(flags);
    }

    /// <summary>
    /// Handle an editing action at the screen level. The keys here are not
    /// associated with any command and thus are treated as primitives. Other
    /// editing actions are handled in HandleCommand.
    /// </summary>
    /// <param name="keyInfo">Console key info</param>
    /// <returns>The rendering hint</returns>
    public RenderHint HandleEditing(ConsoleKeyInfo keyInfo) {
        RenderHint flags = RenderHint.BLOCK;
        if (!char.IsControl(keyInfo.KeyChar)) {
            Buffer.Insert(keyInfo.KeyChar);
        }
        else {
            flags = keyInfo.Key switch {
                ConsoleKey.Enter => Newline(),
                ConsoleKey.Delete => DeleteChar(),
                ConsoleKey.Backspace => Backspace(),
                _ => RenderHint.NONE
            };
        }
        return ApplyRenderHint(flags);
    }

    /// <summary>
    /// Apply the render hint flags to the current window. On completion,
    /// return just the flags that were not applied.
    /// </summary>
    public RenderHint ApplyRenderHint(RenderHint flags) {
        if (flags.HasFlag(RenderHint.REDRAW)) {
            Render(RenderHint.REDRAW);
            flags &= ~RenderHint.REDRAW|RenderHint.BLOCK;
            flags |= RenderHint.CURSOR_STATUS;
        }
        if (flags.HasFlag(RenderHint.BLOCK)) {
            Render(RenderHint.BLOCK);
            flags &= ~RenderHint.BLOCK;
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
    /// Perform a search for the text specified in searchData and update the
    /// cursor to the first match.
    /// </summary>
    /// <param name="searchData">A search object</param>
    public RenderHint Search(Search searchData) {
        RenderHint flags = RenderHint.NONE;
        if (searchData.Next()) {
            Buffer.LineIndex = searchData.Row;
            Buffer.Offset = searchData.Column;
            flags |= CursorFromLineIndex();
        }
        return ApplyRenderHint(flags);
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
    /// Render the buffer filename at the top of the window. If the window
    /// is narrower than the title then we truncate the title to fit.
    /// </summary>
    private void RenderTitle() {
        string title = Buffer.BaseFilename;
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

    /// <summary>
    /// Update this window
    /// </summary>
    private void Render(RenderHint flags) {

        Point savedCursor = Terminal.GetCursor();
        ConsoleColor bg = Screen.Colours.BackgroundColour;
        ConsoleColor fg = Screen.Colours.ForegroundColour;

        // By default, the extent being updated is the entire window. This would be
        // the case if the window was scrolled or the text attributes changed.
        Extent renderExtent = new Extent()
            .Add(new Point(0, _viewportOffset.Y))
            .Add(new Point(0, _viewportOffset.Y + _viewportBounds.Height - 1));

        Extent markExtent = new Extent()
            .Add(_markAnchor)
            .Add(Buffer.Cursor);

        // For block updates, we're scoping the area being rendered down to just those
        // lines that are affected. For changes to the block mark, this would be the
        // area being marked plus any area where the mark was removed. The other area
        // is the buffer invalidate extent which is the extent of the buffer that was
        // modified by the most recent edit action. The resulting extent to be updated
        // is the superset of the two, limited to the area of the visible window.
        if (flags.HasFlag(RenderHint.BLOCK)) {
            Extent blockExtent = new Extent();
            if (_markMode != MarkMode.NONE) {
                blockExtent
                    .Add(_markAnchor)
                    .Add(Buffer.Cursor)
                    .Add(_lastMarkPoint);
            }
            if (Buffer.InvalidateExtent.Valid) {
                blockExtent
                    .Add(Buffer.InvalidateExtent.Start)
                    .Add(Buffer.InvalidateExtent.End);
            }
            renderExtent.Subtract(blockExtent.Start, blockExtent.End);
        }

        int i = renderExtent.Start.Y;
        string line = Buffer.GetLine(i);
        while (line != null && i <= renderExtent.End.Y) {

            int y = _viewportBounds.Top + (i - _viewportOffset.Y);
            int x = _viewportBounds.Left;
            int w = _viewportBounds.Width;
            int left = Math.Min(_viewportOffset.X, line.Length);
            int length = Math.Min(w, line.Length - left);

            switch (_markMode) {
                case MarkMode.LINE:
                    if (i >= markExtent.Start.Y && i <= markExtent.End.Y) {
                        bg = Screen.Colours.ForegroundColour;
                        fg = Screen.Colours.BackgroundColour;
                    }
                    break;

                case MarkMode.COLUMN:
                    if (i >= markExtent.Start.Y && i <= markExtent.End.Y) {
                        if (markExtent.Start.X > 0 && markExtent.Start.X > _viewportOffset.X) {
                            int diff = markExtent.Start.X - _viewportOffset.X;
                            Terminal.Write(x, y, bg, fg, Utilities.SpanBound(line, left, diff));
                            x += diff;
                            w -= diff;
                            left += diff;
                            length -= diff;
                        }
                        if (markExtent.End.X > _viewportOffset.X) {
                            int diff = Math.Min(markExtent.End.X - markExtent.Start.X + 1, line.Length - left);
                            int diff2 = markExtent.End.X - markExtent.Start.X + 1;
                            bg = Screen.Colours.ForegroundColour;
                            fg = Screen.Colours.BackgroundColour;
                            Terminal.WriteLine(x, y, w, bg, fg, Utilities.SpanBound(line, left, diff));
                            x += diff2;
                            w -= diff2;
                            left += diff;
                            length -= diff;
                        }
                        bg = Screen.Colours.BackgroundColour;
                        fg = Screen.Colours.ForegroundColour;
                    }
                    break;

                case MarkMode.CHARACTER:
                    if (i == markExtent.Start.Y) {
                        if (markExtent.Start.X > 0 && markExtent.Start.X > _viewportOffset.X) {
                            int diff = markExtent.Start.X - _viewportOffset.X;
                            Terminal.Write(x, y, bg, fg, Utilities.SpanBound(line, left, diff));
                            x += diff;
                            w -= diff;
                            left += diff;
                            length -= diff;
                        }
                        bg = Screen.Colours.ForegroundColour;
                        fg = Screen.Colours.BackgroundColour;
                    }
                    if (i > markExtent.Start.Y && i < markExtent.End.Y) {
                        bg = Screen.Colours.ForegroundColour;
                        fg = Screen.Colours.BackgroundColour;
                    }
                    if (i == markExtent.End.Y) {
                        int diff = Math.Min(markExtent.End.X - left + 1, line.Length - left);
                        if (diff > 0) {
                            bg = Screen.Colours.ForegroundColour;
                            fg = Screen.Colours.BackgroundColour;
                            Terminal.Write(x, y, bg, fg, Utilities.SpanBound(line, left, diff));
                            x += diff;
                            w -= diff;
                            left = markExtent.End.X + 1;
                            length = Math.Min(w, line.Length - left);
                        }
                        bg = Screen.Colours.BackgroundColour;
                        fg = Screen.Colours.ForegroundColour;
                    }
                    break;
            }

            Terminal.WriteLine(x, y, w, bg, fg, Utilities.SpanBound(line, left, length));
            line = Buffer.GetLine(++i);

            bg = Screen.Colours.BackgroundColour;
            fg = Screen.Colours.ForegroundColour;
        }
        while (i <= renderExtent.End.Y) {
            int y = _viewportBounds.Top + (i - _viewportOffset.Y);
            Terminal.Write(_viewportBounds.Left, y, _viewportBounds.Width, bg, fg, string.Empty);
            i++;
        }

        // Indicate that all buffer modifications have been rendered.
        Buffer.InvalidateExtent.Clear();

        Terminal.SetCursor(savedCursor);
        PlaceCursor();
    }

    /// <summary>
    /// Go to input line.
    /// </summary>
    private RenderHint GoToLine(Macro parser) {
        RenderHint flags = RenderHint.NONE;
        if (_markMode != MarkMode.NONE) {
            flags |= RenderHint.BLOCK;
        }
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
    /// If we are in mark mode, deletes the marked block
    /// otherwise delete the character at the cursor
    /// </summary>
    private RenderHint DeleteChar() {
        RenderHint flags = RenderHint.NONE;
        if (_markMode == MarkMode.NONE) {
            flags |= RenderHint.BLOCK;
            Buffer.Delete(1);
        }
        else {
            flags |= HandleBlock(BlockAction.DELETE);
        }
        return flags | CursorFromOffset();
    }

    /// <summary>
    /// Backspace over the previous character if we're not at the start
    /// of the buffer.
    /// </summary>
    private RenderHint Backspace() {
        RenderHint flags = RenderHint.NONE;
        if (!Buffer.AtStartOfBuffer) {
            flags = CursorLeft();
            Buffer.Delete(1);
            flags |= RenderHint.BLOCK;
        }
        return flags;
    }

    /// <summary>
    /// Insert a newline character at the cursor.
    /// </summary>
    private RenderHint Newline() {
        Buffer.Break();
        return RenderHint.BLOCK;
    }

    /// <summary>
    /// Move the line containing the cursor to the bottom of the
    /// current window.
    /// </summary>
    private RenderHint LineToBottom() {
        return ShiftInWindow(_viewportBounds.Height - 1);
    }

    /// <summary>
    /// Move the line containing the cursor to the top of the
    /// current window.
    /// </summary>
    private RenderHint LineToTop() {
        return ShiftInWindow(0);
    }

    /// <summary>
    /// Center the cursor in the window
    /// </summary>
    private RenderHint CenterWindow() {
        return ShiftInWindow(_viewportBounds.Height / 2);
    }

    /// <summary>
    /// Shift the cursor in the current window by the specified offset
    /// by scrolling the window up or down as required.
    /// </summary>
    private RenderHint ShiftInWindow(int offset) {
        RenderHint flags = RenderHint.NONE;
        int diff = offset - CursorRowInViewport;
        int newOffset = Math.Max(0, _viewportOffset.Y - diff);
        if (newOffset != _viewportOffset.Y) {
            _viewportOffset.Y = newOffset;
            flags = RenderHint.REDRAW;
        }
        return flags;
    }

    /// <summary>
    /// Delete the current line.
    /// </summary>
    private RenderHint DeleteLine() {
        int length = Buffer.GetLine(Buffer.LineIndex).Length;
        Buffer.Offset = 0;
        Buffer.Delete(length);
        return RenderHint.BLOCK | CursorFromOffset();
    }

    /// <summary>
    /// Delete to the end of the current line.
    /// </summary>
    private RenderHint DeleteToEndOfLine() {
        int length = Buffer.GetLine(Buffer.LineIndex).Length - Buffer.Offset - 1;
        Buffer.Delete(length);
        return RenderHint.BLOCK | CursorFromOffset();
    }

    /// <summary>
    /// Delete to the start of the current line.
    /// </summary>
    private RenderHint DeleteToStartOfLine() {
        int length = Buffer.Offset;
        Buffer.Offset = 0;
        Buffer.Delete(length);
        return RenderHint.BLOCK | CursorFromOffset();
    }

    /// <summary>
    /// Start or end a block mark.
    /// </summary>
    private RenderHint Mark(MarkMode markMode) {
        if (_markMode == markMode) {
            Buffer.InvalidateExtent
                .Add(_markAnchor)
                .Add(Buffer.Cursor);
            _markMode = MarkMode.NONE;
        }
        else {
            if (_markMode == MarkMode.NONE) {
                _markAnchor = Buffer.Cursor;
                _lastMarkPoint = _markAnchor;
            }
            _markMode = markMode;
        }
        return RenderHint.BLOCK;
    }

    /// <summary>
    /// Perform the specified actions on a marked block.
    /// </summary>
    private RenderHint HandleBlock(BlockAction action) {

        (Point markStart, Point markEnd) = GetOrderedMarkRange();

        // Delete ranges are a collection of ranges to be deleted, indicated by
        // a cursor position and a count. For LINE and COLUMN blocks, these are
        // simply the start of the mark range and the entire extent. For COLUMN,
        // these are a collection for each line in the column.
        List<(Point, int)> deleteRanges = new();
        (Point, int Count) currentRange = new(markStart, 0);

        StringBuilder copyText = new();
        for (int l = markStart.Y; l <= markEnd.Y; l++) {
            string line = Buffer.GetLine(l);
            int startIndex = 0;
            int length = line.Length;
            switch (_markMode) {
                case MarkMode.COLUMN:
                    if (currentRange.Count > 0) {
                        deleteRanges.Add(currentRange);
                    }
                    --length;
                    startIndex = Math.Min(length, markStart.X);
                    length = Math.Min(length - startIndex, markEnd.X - startIndex + 1);
                    currentRange = new(new Point(startIndex, l), 0);
                    break;

                case MarkMode.CHARACTER:
                    if (l == markStart.Y) {
                        startIndex = markStart.X;
                        length -= startIndex;
                    }
                    if (l == markEnd.Y) {
                        length = markEnd.X - startIndex + 1;
                    }
                    break;
            }
            if (action.HasFlag(BlockAction.COPY)) {
                copyText.Append(Utilities.SpanBound(line, startIndex, length));
                if (copyText.Length > 0 && copyText[^1] != Consts.EndOfLine) {
                    copyText.Append(Consts.EndOfLine);
                }
            }
            if (action.HasFlag(BlockAction.DELETE)) {
                currentRange.Count += length;
            }
        }

        if (action.HasFlag(BlockAction.COPY)) {
            Screen.ScrapBuffer.Content = copyText.ToString();
            Buffer.InvalidateExtent
                .Add(markStart)
                .Add(markEnd);
        }
        if (action.HasFlag(BlockAction.DELETE)) {
            deleteRanges.Add(currentRange);
            foreach ((Point point, int count) in deleteRanges) {
                if (count > 0) {
                    Buffer.Offset = point.X;
                    Buffer.LineIndex = point.Y;
                    Buffer.Delete(count);
                }
            }
        }

        Screen.StatusBar.Message(action switch {
            BlockAction.COPY =>  Edit.CopiedToScrap,
            BlockAction.DELETE => Edit.BlockDeleted,
            BlockAction.COPY_AND_DELETE => Edit.DeletedToScrap,
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
        });

        Buffer.Offset = markStart.X;
        Buffer.LineIndex = markStart.Y;

        _markMode = MarkMode.NONE;
        return RenderHint.BLOCK;
    }

    /// <summary>
    /// Return the mark range as two Point tuples where the first tuple is guaranteed
    /// to be earlier in the range than the second.
    /// </summary>
    private (Point, Point) GetOrderedMarkRange() {
        Extent markExtent = new Extent().Add(Buffer.Cursor);
        if (_markMode != MarkMode.NONE) {
            markExtent.Add(_markAnchor);
        }
        return (markExtent.Start, markExtent.End);
    }

    /// <summary>
    /// Scroll the screen down one line, keeping the cursor in the same
    /// column.
    /// </summary>
    private RenderHint ScreenDown() {
        RenderHint flags = RenderHint.NONE;
        if (CursorRowInViewport == _viewportBounds.Bottom - 1) {
            flags |= CursorUp();
        }
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
        if (CursorRowInViewport == 0) {
            flags |= CursorDown();
        }
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
    /// If we're marking, save the current cursor position before we update
    /// it so that we maintain a last mark point to compute the extent of the
    /// area to render when we update the window.
    /// </summary>
    private RenderHint SaveLastMarkPoint() {
        if (_markMode != MarkMode.NONE) {
            _lastMarkPoint = Buffer.Cursor;
            return RenderHint.BLOCK;
        }
        return RenderHint.NONE;
    }

    /// <summary>
    /// Move the cursor down if possible.
    /// </summary>
    private RenderHint CursorDown() {
        RenderHint flags = SaveLastMarkPoint();
        if (Buffer.LineIndex < Buffer.Length - 1) {
            ++Buffer.LineIndex;
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
        RenderHint flags = SaveLastMarkPoint();
        if (Buffer.LineIndex > 0) {
            --Buffer.LineIndex;
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
        RenderHint flags = SaveLastMarkPoint();
        --Buffer.Offset;
        return flags | CursorFromOffset();
    }

    /// <summary>
    /// Move the cursor right.
    /// </summary>
    private RenderHint CursorRight() {
        RenderHint flags = SaveLastMarkPoint();
        ++Buffer.Offset;
        return flags | CursorFromOffset();
    }

    /// <summary>
    /// Move the cursor to the start of the next line if possible.
    /// The cursor is placed at the beginning of the line and we
    /// scroll the viewport to bring it into focus if necessary.
    /// </summary>
    private RenderHint StartOfNextLine() {
        RenderHint flags = SaveLastMarkPoint();
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
        RenderHint flags = SaveLastMarkPoint();
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
        RenderHint flags = SaveLastMarkPoint();
        Buffer.Offset = 0;
        return flags | CursorFromOffset();
    }

    /// <summary>
    /// Move the cursor to the end of the current line.
    /// </summary>
    private RenderHint EndOfCurrentLine() {
        RenderHint flags = SaveLastMarkPoint();
        Buffer.Offset = Buffer.GetLine(Buffer.LineIndex).Length - 1;
        return flags | CursorFromOffset();
    }

    /// <summary>
    /// Move the cursor down one page.
    /// </summary>
    private RenderHint PageDown() {
        RenderHint flags = SaveLastMarkPoint();
        int previousLineIndex = Buffer.LineIndex;
        Buffer.LineIndex = Math.Min(Buffer.LineIndex + _viewportBounds.Height, Buffer.Length - 1);
        if (Buffer.LineIndex != previousLineIndex) {
            _viewportOffset.Y += Buffer.LineIndex - previousLineIndex;
            flags |= RenderHint.REDRAW;
        }
        return flags;
    }

    /// <summary>
    /// Move the cursor up one page.
    /// </summary>
    private RenderHint PageUp() {
        RenderHint flags = SaveLastMarkPoint();
        int previousLineIndex = Buffer.LineIndex;
        Buffer.LineIndex = Math.Max(Buffer.LineIndex - _viewportBounds.Height, 0);
        if (Buffer.LineIndex != previousLineIndex) {
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
        RenderHint flags = SaveLastMarkPoint();
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
        RenderHint flags = SaveLastMarkPoint();
        Buffer.LineIndex = Buffer.Length - 1;
        Buffer.Offset = Buffer.GetLine(Buffer.LineIndex).Length - 1;
        return flags | CursorFromLineIndex();
    }

    /// <summary>
    /// Move to the top of the window
    /// </summary>
    private RenderHint WindowTop() {
        RenderHint flags = SaveLastMarkPoint();
        Buffer.LineIndex -= CursorRowInViewport;
        return flags | CursorFromLineIndex();
    }

    /// <summary>
    /// Move to the bottom of the window
    /// </summary>
    private RenderHint WindowBottom() {
        RenderHint flags = SaveLastMarkPoint();
        Buffer.LineIndex += _viewportBounds.Height - 1;
        if (Buffer.LineIndex >= Buffer.Length) {
            Buffer.LineIndex = Buffer.Length - 1;
        }
        return flags | CursorFromLineIndex();
    }

    /// <summary>
    /// Move to the next word to the right of the cursor. If the cursor is
    /// within a word, it moves over the remainder of the word and subsequent
    /// spaces to the start of the next word. If it is within spaces, it moves
    /// to the start of the next word. If it is on the last word on a line, it
    /// moves to the start of the first word on the next line.
    /// </summary>
    private RenderHint WordRight() {
        RenderHint flags = SaveLastMarkPoint();
        if (Buffer.Offset >= Buffer.GetLine(Buffer.LineIndex).Length) {
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
        RenderHint flags = SaveLastMarkPoint();
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
            if (Buffer.Offset > Buffer.GetLine(Buffer.LineIndex).Length) {
                Terminal.SetVirtualCursor();
            } else {
                Terminal.SetDefaultCursor();
            }
            Terminal.SetCursor(column, row);
        }
    }
}

