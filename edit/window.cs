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

using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using JComLib;
using JEdit.Resources;

namespace JEdit;

public partial class Window {
    private Rectangle _viewportBounds;
    private Point _viewportOffset;
    private MarkMode _markMode;
    private Point _markAnchor;
    private Point _lastMarkPoint;
    private Extent _searchExtent = new();

    /// <summary>
    /// Create an empty window
    /// </summary>
    public Window() {
        Buffer = new Buffer(string.Empty);
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
    /// Active list of errors from last compile
    /// </summary>
    private CircularList ErrorList { get; set; } = new();

    /// <summary>
    /// Compiler error string compile-time regex.
    /// </summary>
    [GeneratedRegex(@"^.*\((\d+)\).*?:\s*(.*)$")]
    private static partial Regex ErrorRegex();

    /// <summary>
    /// Set the viewport for the window.
    /// </summary>
    /// <param name="x">Left edge, 0 based</param>
    /// <param name="y">Top edge, 0 based</param>
    /// <param name="width">Width of window</param>
    /// <param name="height">Height of window</param>
    public void SetViewportBounds(int x, int y, int width, int height) {
        _viewportBounds = new Rectangle(x, y, width, height);
        if (Screen.Config.HideBorders) {
            _viewportBounds.Inflate(1, 1);
        }
    }

    /// <summary>
    /// Refresh this window with a full redraw on screen.
    /// </summary>
    public void Refresh() {
        RenderFrame();
        Render(RenderHint.REDRAW);
    }

    /// <summary>
    /// Handle actions before a top-level command. If there is a buffer
    /// invalidation pending then apply that first.
    /// </summary>
    public void PreCommand() {
        if (Buffer.InvalidateExtent.Valid) {
            ApplyRenderHint(RenderHint.BLOCK);
        }
    }

    /// <summary>
    /// Handle a keyboard command.
    /// </summary>
    /// <param name="command">Editing command</param>
    /// <returns>Render hint</returns>
    public RenderHint HandleCommand(Command command) {
        RenderHint flags = command.Id switch {
            KeyCommand.KC_BACKTAB => BackTab(),
            KeyCommand.KC_BACKSPACE => Backspace(),
            KeyCommand.KC_CDOWN => CursorDown(),
            KeyCommand.KC_CENTRE => CentreLine(),
            KeyCommand.KC_CFILEEND => FileEnd(),
            KeyCommand.KC_CFILESTART => FileStart(),
            KeyCommand.KC_CLEFT => CursorLeft(),
            KeyCommand.KC_CLINEEND => EndOfCurrentLine(),
            KeyCommand.KC_CLINESTART => StartOfCurrentLine(),
            KeyCommand.KC_COMPILE => CompileAndRun(),
            KeyCommand.KC_COPY => HandleBlock(command, BlockAction.COPY),
            KeyCommand.KC_CPAGEDOWN => PageDown(),
            KeyCommand.KC_CPAGEUP => PageUp(),
            KeyCommand.KC_CRIGHT => CursorRight(),
            KeyCommand.KC_CTOBOTTOM => LineToBottom(),
            KeyCommand.KC_CTOTOP => LineToTop(),
            KeyCommand.KC_CUP => CursorUp(),
            KeyCommand.KC_CUT => HandleBlock(command, BlockAction.CUT),
            KeyCommand.KC_CWINDOWBOTTOM => WindowBottom(),
            KeyCommand.KC_CWINDOWCENTRE => CenterWindow(),
            KeyCommand.KC_CWINDOWTOP => WindowTop(),
            KeyCommand.KC_CWORDLEFT => WordLeft(),
            KeyCommand.KC_CWORDRIGHT => WordRight(),
            KeyCommand.KC_DELETECHAR => DeleteChar(command),
            KeyCommand.KC_DELETELINE => DeleteLine(),
            KeyCommand.KC_DELETEPREVWORD => DeleteWord(false),
            KeyCommand.KC_DELETETOEND => DeleteToEndOfLine(),
            KeyCommand.KC_DELETETOSTART => DeleteToStartOfLine(),
            KeyCommand.KC_DELETEWORD => DeleteWord(true),
            KeyCommand.KC_GOTO => GoToLine(command),
            KeyCommand.KC_LOWERCASE => HandleBlock(command, BlockAction.LOWER),
            KeyCommand.KC_MARK => Mark(MarkMode.CHARACTER),
            KeyCommand.KC_MARKCOLUMN => Mark(MarkMode.COLUMN),
            KeyCommand.KC_MARKLINE => Mark(MarkMode.LINE),
            KeyCommand.KC_NEXTERROR => NextError(),
            KeyCommand.KC_OPENLINE => OpenLine(),
            KeyCommand.KC_PASTE => Paste(),
            KeyCommand.KC_REFORM => ReformParagraph(),
            KeyCommand.KC_SELFINSERT => SelfInsert(command),
            KeyCommand.KC_SCREENDOWN => ScreenDown(),
            KeyCommand.KC_SCREENUP => ScreenUp(),
            KeyCommand.KC_UPPERCASE => HandleBlock(command, BlockAction.UPPER),
            KeyCommand.KC_WRITEBUFFER => WriteBuffer(command),
            _ => RenderHint.NONE
        };
        return ApplyRenderHint(flags);
    }

    /// <summary>
    /// Apply the render hint flags to the current window. On completion,
    /// return just the flags that were not applied.
    /// </summary>
    /// <returns>Unapplied render hint</returns>
    public RenderHint ApplyRenderHint(RenderHint flags) {
        if (flags.HasFlag(RenderHint.REDRAW)) {
            Render(RenderHint.REDRAW);
            flags &= ~(RenderHint.REDRAW | RenderHint.BLOCK);
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
    /// <returns>Render hint</returns>
    public RenderHint Search(Search searchData) {
        if (searchData.Next()) {
            MarkSearch(searchData);
        }
        return RenderHint.CURSOR_STATUS;
    }

    /// <summary>
    /// Perform a search for the text specified in searchData and update the
    /// cursor to the first match. Then prompt the user whether to replace
    /// the text or skip, or end the search.
    /// </summary>
    /// <param name="searchData">A search object</param>
    /// <returns>Render hint</returns>
    public RenderHint Translate(Search searchData) {
        RenderHint flags = RenderHint.NONE;
        bool prompt = true;
        bool finish = false;
        while (!finish && searchData.Next()) {
            MarkSearch(searchData);
            if (prompt) {
                char[] validChars = ['y', 'n', 'g', 'o'];
                if (!Screen.StatusBar.Prompt(Edit.TranslatePrompt, validChars, 'n', out char inputChar)) {
                    break;
                }
                if (inputChar == 'g') {
                    prompt = false;
                }
                if (inputChar == 'n') {
                    continue;
                }
                if (inputChar == 'o') {
                    finish = true;
                }
            }
            Buffer.Delete(searchData.MatchLength);
            Buffer.Insert(searchData.ReplacementString);
            searchData.Advance(searchData.ReplacementString.Length);
            flags |= RenderHint.BLOCK | CursorFromLineIndex();
            flags = ApplyRenderHint(flags);
            ++searchData.TranslateCount;
        }
        return ApplyRenderHint(flags);
    }

    /// <summary>
    /// Compile and run the code in the active window.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint CompileAndRun() {
        RenderHint flags = RenderHint.NONE;
        Compiler? compiler = Compiler.CompilerForExtension(Path.GetExtension(Buffer.Filename));
        if (compiler != null) {
            Buffer.Write();
            Terminal.Close();

            Process process = new();
            process.StartInfo.FileName = Path.Combine(Compiler.BinaryPath, compiler.ProgramName);
            process.StartInfo.Arguments = string.Format(compiler.CommandLine, Buffer.Filename);
            process.StartInfo.RedirectStandardError = true;
            process.Start();
            process.WaitForExit();

            Terminal.Write(Edit.PressAnyKey);
            Console.ReadKey(true);
            Terminal.Write("\n");

            Terminal.Open();
            if (process.ExitCode == 0) {
                Screen.StatusBar.Message(string.Empty);
            }
            else {
                ErrorList = new CircularList();
                StreamReader myStreamReader = process.StandardError;
                string[] lines = myStreamReader.ReadToEnd().Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in lines) {
                    ErrorList.Append(line);
                }
                flags |= NextError();
            }
            flags |= RenderHint.REFRESH;
        }
        return flags;
    }

    /// <summary>
    /// Display the next error in the error list.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint NextError() {
        string line = ErrorList.Next();
        Match match = ErrorRegex().Match(line);
        if (match is { Success: true, Groups.Count: 3 }) {
            if (int.TryParse(match.Groups[1].ToString(), out int lineNumber)) {
                if (lineNumber >= 1 && lineNumber <= Buffer.Length) {
                    Buffer.LineIndex = lineNumber - 1;
                }
            }
            Screen.StatusBar.Error(match.Groups[2].ToString());
        }
        return CursorFromLineIndex();
    }

    /// <summary>
    /// Highlight the matched search string
    /// </summary>
    /// <param name="searchData">Search string</param>
    private void MarkSearch(Search searchData) {
        RenderHint flags = RenderHint.BLOCK;
        _lastMarkPoint = Buffer.Cursor;
        Buffer.LineIndex = searchData.Row;
        Buffer.Offset = searchData.Column;
        _markMode = MarkMode.SEARCH;
        _searchExtent = new Extent()
            .Add(Buffer.Cursor)
            .Add(new Point(Buffer.Offset + searchData.MatchLength - 1, Buffer.LineIndex));
        flags |= CursorFromOffset() | CursorFromLineIndex();
        ApplyRenderHint(flags);
    }

    /// <summary>
    /// Draw the window frame
    /// </summary>
    private void RenderFrame() {
        if (!Screen.Config.HideBorders) {
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
    }

    /// <summary>
    /// Render the buffer filename at the top of the window. If the window
    /// is narrower than the title then we truncate the title to fit.
    /// </summary>
    private void RenderTitle() {
        if (!Screen.Config.HideBorders) {
            string title = Buffer.Name;
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

        Extent markExtent = new();
        if (_markMode == MarkMode.SEARCH) {
            markExtent = new Extent()
                .Add(_searchExtent.Start)
                .Add(_searchExtent.End);
        }
        else if (_markMode != MarkMode.NONE) {
            markExtent = new Extent()
                .Add(_markAnchor)
                .Add(Buffer.Cursor);
        }

        // For block updates, we're scoping the area being rendered down to just those
        // lines that are affected. For changes to the block mark, this would be the
        // area being marked plus any area where the mark was removed. The other area
        // is the buffer invalidate extent which is the extent of the buffer that was
        // modified by the most recent edit action. The resulting extent to be updated
        // is the superset of the two, limited to the area of the visible window.
        if (flags.HasFlag(RenderHint.BLOCK)) {
            Extent blockExtent = new();
            blockExtent
                .Add(markExtent.Start)
                .Add(markExtent.End)
                .Add(_lastMarkPoint);
            if (Buffer.InvalidateExtent.Valid) {
                blockExtent
                    .Add(Buffer.InvalidateExtent.Start)
                    .Add(Buffer.InvalidateExtent.End);
            }
            if (blockExtent.Valid) {
                renderExtent.Subtract(blockExtent.Start, blockExtent.End);
            }
        }

        int i = renderExtent.Start.Y;
        string line = GetPreRenderLine(i);
        while (i <= renderExtent.End.Y) {

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
                            Terminal.Write(x, y, 0, bg, fg, Utilities.SpanBound(line, left, diff));
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
                            Terminal.Write(x, y, w, bg, fg, Utilities.SpanBound(line, left, diff));
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
                case MarkMode.SEARCH:
                    if (i == markExtent.Start.Y) {
                        if (markExtent.Start.X > 0 && markExtent.Start.X > _viewportOffset.X) {
                            int diff = markExtent.Start.X - _viewportOffset.X;
                            Terminal.Write(x, y, 0, bg, fg, Utilities.SpanBound(line, left, diff));
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
                            Terminal.Write(x, y, 0, bg, fg, Utilities.SpanBound(line, left, diff));
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

            Terminal.Write(x, y, w, bg, fg, Utilities.SpanBound(line, left, length));
            line = GetPreRenderLine(++i);

            bg = Screen.Colours.BackgroundColour;
            fg = Screen.Colours.ForegroundColour;
        }

        // Indicate that all buffer modifications have been rendered.
        Buffer.InvalidateExtent.Clear();

        // Search highlighting is temporary and needs to be removed in response
        // to the next keystroke.
        if (_markMode == MarkMode.SEARCH) {
            Buffer.InvalidateExtent
                .Add(_searchExtent.Start)
                .Add(_searchExtent.End);
            _searchExtent.Clear();
            _markMode = MarkMode.NONE;
        }

        Terminal.SetCursor(savedCursor);
        PlaceCursor();
    }

    /// <summary>
    /// Retrieve and pre-render a line by expanding tabs and replacing line
    /// terminators.
    /// </summary>
    /// <param name="lineIndex">Index of line to retrieve</param>
    /// <returns>Pre-rendered line</returns>
    private string GetPreRenderLine(int lineIndex) {
        StringBuilder line = new(Buffer.GetLine(lineIndex));
        int index = 0;
        while (index < line.Length) {
            if (line[index] == Consts.EndOfLine) {
                line[index] = ' ';
            }
            else if (line[index] == '\t') {
                int spacesToTab = SpacesToPad(index);
                line[index] = ' ';
                while (--spacesToTab > 0) {
                    line.Insert(index++, ' ');
                }
            }
            index++;
        }
        return line.ToString();
    }

    /// <summary>
    /// Go to input line.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint GoToLine(Command command) {
        RenderHint flags = RenderHint.NONE;
        if (_markMode != MarkMode.NONE) {
            flags |= RenderHint.BLOCK;
        }
        if (command.GetNumber(Edit.GoToLine, out int inputLine)) {
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
    /// Handle the self-insert command.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint SelfInsert(Command command) {
        string nextWord = command.Args.NextWord();
        int inputValue;
        if (string.IsNullOrEmpty(nextWord)) {
            inputValue = 10;
        }
        else if (!int.TryParse(nextWord, out inputValue)) {
            Screen.StatusBar.Error("Invalid number");
            return RenderHint.NONE;
        }
        RenderHint flags = inputValue switch {
            8 => Backspace(),
            9 => TabChar(),
            10 => Newline(),
            _ => AddChar((char)inputValue)
        };
        return ApplyRenderHint(flags);
    }

    /// <summary>
    /// If we are in mark mode, deletes the marked block
    /// otherwise delete the character at the cursor
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint DeleteChar(Command command) {
        RenderHint flags = RenderHint.NONE;
        if (_markMode == MarkMode.NONE) {
            flags |= RenderHint.BLOCK;
            Buffer.Delete(1);
        }
        else {
            flags |= HandleBlock(command, BlockAction.DELETE);
        }
        return flags | CursorFromOffset();
    }

    /// <summary>
    /// Backspace over the previous character if we're not at the start
    /// of the buffer.
    /// </summary>
    /// <returns>Render hint</returns>
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
    /// Insert a tab character or add spaces up to the next
    /// tab stop if UseTabChar is set.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint TabChar() {
        if (_markMode != MarkMode.NONE) {
            return HandleBlock(new Command(), BlockAction.INDENT);
        }
        if (!Screen.Config.InsertMode) {
            Buffer.Offset += SpacesToPad(Buffer.Offset);
            return CursorFromOffset();
        }
        if (!Screen.Config.UseTabChar) {
            Buffer.Insert('\t');
        }
        else {
            int spacesToAdd = SpacesToPad(Buffer.Offset);
            while (spacesToAdd-- > 0) {
                Buffer.Insert(' ');
            }
        }
        return RenderHint.BLOCK | CursorFromOffset();
    }

    /// <summary>
    /// Handle the backward-tab key.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint BackTab() {
        RenderHint flags = RenderHint.NONE;
        if (_markMode != MarkMode.NONE) {
            flags = HandleBlock(new Command(), BlockAction.OUTDENT);
        }
        else {
            if (Buffer.Offset == 0) {
                if (Buffer.LineIndex == 0) {
                    return flags;
                }
                flags |= EndOfPreviousLine();
            }
            int previousStop = 1;
            foreach (int tabStop in TabStops()) {
                if (tabStop >= Buffer.Offset + 1) {
                    break;
                }
                previousStop = tabStop;
            }
            Buffer.Offset = previousStop - 1;
            flags |= CursorFromOffset();
        }
        return flags;
    }

    /// <summary>
    /// Add the specified character to the buffer. In insert
    /// mode, the character is inserted at the current cursor
    /// position. In overstrike mode it replaces the current
    /// character at the cursor position.
    /// </summary>
    /// <param name="ch">Character to add</param>
    private RenderHint AddChar(char ch) {
        if (!char.IsControl(ch)) {
            if (Screen.Config.InsertMode) {
                Buffer.Insert(ch);
            }
            else {
                Buffer.Replace(ch);
            }
        }
        return RenderHint.BLOCK;
    }

    /// <summary>
    /// Given a 0-based line offset, compute the number of spaces
    /// from that offset to the next tab stop.
    /// </summary>
    /// <param name="offset"></param>
    /// <returns>Number of spaces to next tab offset</returns>
    private static int SpacesToPad(int offset) {
        int tabWidth = 7;
        int previousStop = 1;
        foreach (int tabStop in TabStops()) {
            tabWidth = tabStop - previousStop;
            if (tabStop > offset + 1) {
                break;
            }
            previousStop = tabStop;
        }
        return tabWidth - offset % tabWidth;
    }

    /// <summary>
    /// Iterator that returns each defined tabstop. If no tab
    /// stops are configured then the iterator returns tab stops
    /// set 8 units apart.
    /// </summary>
    /// <returns>Next tabstop</returns>
    private static IEnumerable<int> TabStops() {
        int tabWidth = 7;
        int previousStop = 1;
        foreach (int tabStop in Screen.Config.TabStops) {
            tabWidth = tabStop - previousStop;
            yield return tabStop;
            previousStop = tabStop;
        }
        while (previousStop < 100) {
            int tabStop = previousStop + tabWidth;
            yield return tabStop;
            previousStop = tabStop;
        }
    }

    /// <summary>
    /// Insert a newline character at the cursor.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint Newline() {
        if (!Screen.Config.InsertMode) {
            return StartOfNextLine();
        }
        Buffer.Insert(Consts.EndOfLine);
        return RenderHint.BLOCK;
    }

    /// <summary>
    /// Open a line below the current line.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint OpenLine() {
        EndOfCurrentLine();
        Buffer.Insert(Consts.EndOfLine);
        return RenderHint.BLOCK;
    }

    /// <summary>
    /// Move the line containing the cursor to the bottom of the
    /// current window.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint LineToBottom() {
        return ShiftInWindow(_viewportBounds.Height - 1);
    }

    /// <summary>
    /// Move the line containing the cursor to the top of the
    /// current window.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint LineToTop() {
        return ShiftInWindow(0);
    }

    /// <summary>
    /// Center the cursor in the window
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint CenterWindow() {
        return ShiftInWindow(_viewportBounds.Height / 2);
    }

    /// <summary>
    /// Shift the cursor in the current window by the specified offset
    /// by scrolling the window up or down as required.
    /// </summary>
    /// <returns>Render hint</returns>
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
    /// <returns>Render hint</returns>
    private RenderHint DeleteLine() {
        int length = Buffer.GetLine(Buffer.LineIndex).Length;
        Buffer.Offset = 0;
        Buffer.Delete(length);
        return RenderHint.BLOCK | CursorFromOffset();
    }

    /// <summary>
    /// Delete to the end of the current line.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint DeleteToEndOfLine() {
        int length = Buffer.GetLine(Buffer.LineIndex).Length - Buffer.Offset - 1;
        Buffer.Delete(length);
        return RenderHint.BLOCK | CursorFromOffset();
    }

    /// <summary>
    /// Delete to the start of the current line.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint DeleteToStartOfLine() {
        int length = Buffer.Offset;
        Buffer.Offset = 0;
        Buffer.Delete(length);
        return RenderHint.BLOCK | CursorFromOffset();
    }

    /// <summary>
    /// Delete the next or previous word.
    /// </summary>
    /// <param name="next">True if we delete the next word, false if we delete the previous word</param>
    /// <returns>Render hint</returns>
    private RenderHint DeleteWord(bool next) {
        char[] text = Buffer.GetLine(Buffer.LineIndex).ToCharArray();
        int offset = Buffer.Offset;
        if (next) {
            while (offset < text.Length && !char.IsWhiteSpace(text[offset])) {
                ++offset;
            }
            while (offset < text.Length && char.IsWhiteSpace(text[offset])) {
                ++offset;
            }
            Buffer.Delete(offset - Buffer.Offset);
        }
        else {
            if (Buffer.Offset == 0) {
                EndOfPreviousLine();
                offset = Buffer.Offset;
            }
            int lastOffset = Buffer.Offset;
            while (offset > 0 && offset < text.Length && char.IsWhiteSpace(text[offset - 1])) {
                --offset;
            }
            while (offset > 0 && offset < text.Length && !char.IsWhiteSpace(text[offset - 1])) {
                --offset;
            }
            Buffer.Offset = offset;
            Buffer.Delete(lastOffset - offset);
        }
        return RenderHint.BLOCK | CursorFromOffset();
    }

    /// <summary>
    /// Reformat a paragraph.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint ReformParagraph() {
        int paraStartIndex = Buffer.LineIndex;
        while (paraStartIndex > 0) {
            string line = Buffer.GetLine(paraStartIndex - 1);
            if (string.IsNullOrWhiteSpace(line)) {
                break;
            }
            --paraStartIndex;
        }
        int paraEndIndex = Buffer.LineIndex;
        while (paraEndIndex < Buffer.Length - 1) {
            string line = Buffer.GetLine(paraEndIndex + 1);
            if (string.IsNullOrWhiteSpace(line)) {
                break;
            }
            ++paraEndIndex;
        }
        int paragraphLength = 0;
        StringBuilder formattedText = new();
        StringBuilder formattedLine = new();
        for (int index = paraStartIndex; index <= paraEndIndex; index++) {
            string line = Buffer.GetLine(index);
            paragraphLength += line.Length;
            string[] words = line.Split(' ',
                StringSplitOptions.TrimEntries |
                StringSplitOptions.RemoveEmptyEntries);
            foreach (string word in words) {
                if (formattedLine.Length + word.Length + 1 > Screen.Config.Margin) {
                    formattedLine.Append(Consts.EndOfLine);
                    formattedText.Append(formattedLine);
                    formattedLine.Clear();
                }
                if (formattedLine.Length > 0) {
                    formattedLine.Append(' ');
                }
                formattedLine.Append(word);
            }
        }
        if (formattedLine.Length > 0) {
            formattedLine.Append(Consts.EndOfLine);
            formattedText.Append(formattedLine);
        }
        Buffer.LineIndex = paraStartIndex;
        Buffer.Offset = 0;
        Buffer.Delete(paragraphLength);
        Buffer.Insert(formattedText.ToString());
        return RenderHint.BLOCK;
    }

    /// <summary>
    /// Start or end a block mark.
    /// </summary>
    /// <returns>Render hint</returns>
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
    /// Paste the contents of the scrap buffer at the current cursor
    /// position.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint Paste() {
        Buffer.Insert(Screen.ScrapBuffer.Content);
        return RenderHint.BLOCK;
    }

    /// <summary>
    /// Perform the specified actions on a marked block.
    /// </summary>
    /// <param name="command">Editing command</param>
    /// <param name="action">Block action</param>
    /// <returns>Render hint</returns>
    private RenderHint HandleBlock(Command command, BlockAction action) {

        Point savedCursor = Buffer.Cursor;
        (Point markStart, Point markEnd) = GetOrderedMarkRange();

        // Ranges are a collection of ranges to be copied or modified, indicated by
        // a cursor position and a count. For LINE and COLUMN blocks, these are
        // simply the start of the mark range and the entire extent. For COLUMN,
        // these are a collection for each line in the column.
        List<(Point, int)> blockRanges = [];
        (Point, int Count) currentRange = new(markStart, 0);

        bool indenting = action is BlockAction.INDENT or BlockAction.OUTDENT;

        for (int l = markStart.Y; l <= markEnd.Y; l++) {
            string line = Buffer.GetLine(l);
            int startIndex = 0;
            int length = line.Length;
            switch (_markMode) {
                case MarkMode.COLUMN:
                    if (currentRange.Count > 0) {
                        blockRanges.Add(currentRange);
                    }
                    --length;
                    startIndex = Math.Min(length, markStart.X);
                    length = Math.Min(length - startIndex, markEnd.X - startIndex + 1);
                    currentRange = new ValueTuple<Point, int>(new Point(startIndex, l), 0);
                    break;

                case MarkMode.LINE:
                    currentRange.Item1.X = 0;
                    if (indenting) {
                        if (currentRange.Count > 0) {
                            blockRanges.Add(currentRange);
                        }
                        currentRange = new ValueTuple<Point, int>(new Point(startIndex, l), 0);
                    }
                    break;

                case MarkMode.CHARACTER:
                    if (l == markStart.Y) {
                        startIndex = markStart.X;
                        length -= startIndex;
                    }
                    if (l == markEnd.Y) {
                        length = markEnd.X - startIndex + 1;
                    }
                    if (indenting) {
                        if (currentRange.Count > 0) {
                            blockRanges.Add(currentRange);
                        }
                        currentRange = new ValueTuple<Point, int>(new Point(startIndex, l), 0);
                    }
                    break;
            }
            currentRange.Count += length;
        }

        blockRanges.Add(currentRange);
        StringBuilder copyText = new();
        string separator = string.Empty;
        foreach ((Point point, int count) in blockRanges) {
            if (count > 0) {
                Buffer.Offset = point.X;
                Buffer.LineIndex = point.Y;
                string text = Buffer.GetText(count);
                if (action.HasFlag(BlockAction.UPPER)) {
                    Buffer.Delete(count);
                    Buffer.Insert(text.ToUpper());
                }
                if (action.HasFlag(BlockAction.LOWER)) {
                    Buffer.Delete(count);
                    Buffer.Insert(text.ToLower());
                }
                if (action.HasFlag(BlockAction.INDENT)) {
                    if (!Screen.Config.UseTabChar) {
                        Buffer.Insert('\t');
                    }
                    else {
                        int spacesToAdd = SpacesToPad(Buffer.Offset);
                        while (spacesToAdd-- > 0) {
                            Buffer.Insert(' ');
                        }
                    }
                }
                if (action.HasFlag(BlockAction.OUTDENT)) {
                    if (text.Length > 0 && text[0] == '\t') {
                        text = text[1..];
                    }
                    else {
                        int firstTabStop = TabStops().First();
                        int index = 0;
                        while (index < firstTabStop && index < text.Length) {
                            if (text[index] != ' ') {
                                break;
                            }
                            ++index;
                        }
                        text = text[index..];
                    }
                    Buffer.Delete(count);
                    Buffer.Insert(text);
                }
                if (action.HasFlag(BlockAction.GET)) {
                    copyText.Append($"{separator}{text}");
                    separator = Consts.EndOfLine.ToString();
                }
                if (action.HasFlag(BlockAction.DELETE)) {
                    Buffer.Delete(count);
                }
            }
        }

        if (action.HasFlag(BlockAction.COPY)) {
            Screen.ScrapBuffer.Content = copyText.ToString();
        }

        if (action.HasFlag(BlockAction.WRITE)) {
            string outputFileName = string.Empty;
            if (command.GetFilename(Edit.WriteBlockAs, ref outputFileName)) {
                Buffer writeBuffer = new(outputFileName) {
                    Content = copyText.ToString()
                };
                writeBuffer.Write();
            }
        }

        // Any action which is non-destructive requires an explicit invalidate of
        // the extent to ensure the block mark is removed from the screen.
        if (action.HasFlag(BlockAction.GET) || indenting) {
            Buffer.InvalidateExtent
                .Add(markStart)
                .Add(markEnd);
        }

        Screen.StatusBar.Message(action switch {
            BlockAction.COPY => Edit.CopiedToScrap,
            BlockAction.DELETE => Edit.BlockDeleted,
            BlockAction.CUT => Edit.DeletedToScrap,
            BlockAction.LOWER => Edit.Lowercasing,
            BlockAction.UPPER => Edit.Uppercasing,
            BlockAction.WRITE => Edit.WriteSuccess,
            _ => string.Empty
        });

        if (!indenting) {
            Buffer.Offset = markStart.X;
            Buffer.LineIndex = markStart.Y;
            _markMode = MarkMode.NONE;
        }
        else {
            Buffer.Offset = savedCursor.X;
            Buffer.LineIndex = savedCursor.Y;
        }
        return RenderHint.BLOCK;
    }

    /// <summary>
    /// Return the mark range as two Point tuples where the first tuple is guaranteed
    /// to be earlier in the range than the second.
    /// </summary>
    /// <returns>Tuple with ordered mark range</returns>
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
    /// <returns>Render hint</returns>
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
    /// <returns>Render hint</returns>
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
    /// <returns>Render hint</returns>
    private RenderHint WriteBuffer(Command command) {
        RenderHint flags = RenderHint.NONE;
        if (_markMode != MarkMode.NONE) {
            flags = HandleBlock(command, BlockAction.WRITE);
        }
        else {
            Buffer.Write();
            Screen.StatusBar.Message(Edit.WriteSuccess);
        }
        return flags;
    }

    /// <summary>
    /// If we're marking, save the current cursor position before we update
    /// it so that we maintain a last mark point to compute the extent of the
    /// area to render when we update the window.
    /// </summary>
    /// <returns>Render hint</returns>
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
    /// <returns>Render hint</returns>
    private RenderHint CursorDown() {
        RenderHint flags = SaveLastMarkPoint();
        if (Buffer.LineIndex < Buffer.Length - 1) {
            ++Buffer.LineIndex;
            if (CursorRowInViewport < _viewportBounds.Height) {
                flags |= RenderHint.CURSOR;
            }
            else {
                ++_viewportOffset.Y;
                flags |= RenderHint.REDRAW;
            }
        }
        return flags;
    }

    /// <summary>
    /// Move the cursor up if possible.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint CursorUp() {
        RenderHint flags = SaveLastMarkPoint();
        if (Buffer.LineIndex > 0) {
            --Buffer.LineIndex;
            if (CursorRowInViewport >= 0) {
                flags |= RenderHint.CURSOR;
            }
            else {
                --_viewportOffset.Y;
                flags |= RenderHint.REDRAW;
            }
        }
        return flags;
    }

    /// <summary>
    /// Move the cursor left.
    /// </summary>
    /// <returns>Render hint</returns>
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
    /// <returns>Render hint</returns>
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
    /// <returns>Render hint</returns>
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
    /// <returns>Render hint</returns>
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
    /// <returns>Render hint</returns>
    private RenderHint StartOfCurrentLine() {
        RenderHint flags = SaveLastMarkPoint();
        Buffer.Offset = 0;
        return flags | CursorFromOffset();
    }

    /// <summary>
    /// Move the cursor to the end of the current line.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint EndOfCurrentLine() {
        RenderHint flags = SaveLastMarkPoint();
        string line = Buffer.GetLine(Buffer.LineIndex);
        int offset = line.Length;
        if (offset > 0 && line[offset - 1] == Consts.EndOfLine) {
            --offset;
        }
        Buffer.Offset = offset;
        return flags | CursorFromOffset();
    }

    /// <summary>
    /// Move the cursor down one page.
    /// </summary>
    /// <returns>Render hint</returns>
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
    /// <returns>Render hint</returns>
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
    /// <returns>Render hint</returns>
    private RenderHint FileStart() {
        RenderHint flags = SaveLastMarkPoint();
        if (Buffer.LineIndex > 0 || Buffer.Offset > 0) {
            Buffer.LineIndex = 0;
            Buffer.Offset = 0;
            if (_viewportOffset.Y > 0 || _viewportOffset.X > 0) {
                _viewportOffset.X = 0;
                _viewportOffset.Y = 0;
                flags |= RenderHint.REDRAW;
            }
            else {
                flags |= RenderHint.CURSOR;
            }
        }
        return flags;
    }

    /// <summary>
    /// Move to the end of the buffer.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint FileEnd() {
        RenderHint flags = SaveLastMarkPoint();
        Buffer.LineIndex = Buffer.Length - 1;
        Buffer.Offset = Buffer.GetLine(Buffer.LineIndex).Length - 1;
        return flags | CursorFromLineIndex();
    }

    /// <summary>
    /// Move to the top of the window
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint WindowTop() {
        RenderHint flags = SaveLastMarkPoint();
        Buffer.LineIndex -= CursorRowInViewport;
        return flags | CursorFromLineIndex();
    }

    /// <summary>
    /// Move to the bottom of the window
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint WindowBottom() {
        RenderHint flags = SaveLastMarkPoint();
        Buffer.LineIndex += _viewportBounds.Height - 1;
        if (Buffer.LineIndex >= Buffer.Length) {
            Buffer.LineIndex = Buffer.Length - 1;
        }
        return flags | CursorFromLineIndex();
    }

    /// <summary>
    /// Center the current line horizontally in the window between
    /// the current margin.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint CentreLine() {
        string line = Buffer.GetLine(Buffer.LineIndex).TrimEnd(Consts.EndOfLine);
        int savedOffset = Buffer.Offset;
        Buffer.Offset = 0;
        Buffer.Delete(line.Length);
        line = line.PadLeft((Screen.Config.Margin - line.Length) / 2 + line.Length).PadRight(Screen.Config.Margin);
        Buffer.Insert(line);
        Buffer.Offset = savedOffset;
        return RenderHint.BLOCK | CursorFromOffset();
    }

    /// <summary>
    /// Move to the next word to the right of the cursor. If the cursor is
    /// within a word, it moves over the remainder of the word and subsequent
    /// spaces to the start of the next word. If it is within spaces, it moves
    /// to the start of the next word. If it is on the last word on a line, it
    /// moves to the start of the first word on the next line.
    /// </summary>
    /// <returns>Render hint</returns>
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
    /// <returns>Render hint</returns>
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
    /// <returns>Render hint</returns>
    private RenderHint CursorFromLineIndex() {
        RenderHint flags = RenderHint.NONE;
        if (Buffer.LineIndex < _viewportOffset.Y) {
            _viewportOffset.Y = Buffer.LineIndex;
            flags |= RenderHint.REDRAW;
        }
        else if (Buffer.LineIndex >= _viewportBounds.Height) {
            _viewportOffset.Y = Buffer.LineIndex - (_viewportBounds.Height - 1);
            flags |= RenderHint.REDRAW;
        }
        else {
            flags |= RenderHint.CURSOR;
        }
        return flags;
    }

    /// <summary>
    /// Update the physical cursor position in the current viewport,
    /// adjusting the horizontal scroll if needed.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint CursorFromOffset() {
        RenderHint flags = RenderHint.NONE;
        int cursorColumn = CursorColumn();
        if (cursorColumn < _viewportOffset.X) {
            _viewportOffset.X = cursorColumn;
            flags |= RenderHint.REDRAW;
        }
        else if (cursorColumn >= _viewportOffset.X + _viewportBounds.Width - 1) {
            _viewportOffset.X = cursorColumn - (_viewportBounds.Width - 1);
            flags |= RenderHint.REDRAW;
        }
        else {
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
    private int CursorColumnInViewport => CursorColumn() - _viewportOffset.X;

    /// <summary>
    /// Calculate the actual cursor column from the start of the line, taking
    /// hard tabs into account.
    /// </summary>
    /// <returns>Cursor column</returns>
    private int CursorColumn() {
        int column = 0;
        string line = Buffer.GetLine(Buffer.LineIndex);
        for (int offset = 0; offset < Buffer.Offset; offset++) {
            column += offset < line.Length && line[offset] == '\t' ? SpacesToPad(column) : 1;
        }
        return column;
    }

    /// <summary>
    /// Place the cursor on screen if it is visible.
    /// </summary>
    private void PlaceCursor() {
        int column = CursorColumnInViewport + _viewportBounds.Left;
        int row = CursorRowInViewport + _viewportBounds.Top;
        if (_viewportBounds.Contains(column, row)) {
            if (Buffer.Offset > Buffer.GetLine(Buffer.LineIndex).Length) {
                Terminal.SetVirtualCursor();
            }
            else {
                Terminal.SetDefaultCursor();
            }
            Terminal.SetCursor(column, row);
        }
    }
}