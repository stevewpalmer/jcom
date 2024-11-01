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
using JCalc.Resources;
using JComLib;

namespace JCalc;

public class Window {
    private Rectangle _viewportBounds;
    private Rectangle _sheetBounds;
    private Point _scrollOffset = Point.Empty;
    private int _numberOfColumns;

    /// <summary>
    /// Create an empty window
    /// </summary>
    public Window() {
        Sheet = new Sheet(1);
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
    /// Return the cell at the cursor.
    /// </summary>
    public Cell ActiveCell => Sheet.Cell(Sheet.Column, Sheet.Row, false);

    /// <summary>
    /// Set the viewport and sheet bounds for the window.
    /// </summary>
    /// <param name="x">Left edge, 0 based</param>
    /// <param name="y">Top edge, 0 based</param>
    /// <param name="width">Width of window</param>
    /// <param name="height">Height of window</param>
    public void SetViewportBounds(int x, int y, int width, int height) {
        _viewportBounds = new Rectangle(x, y, width, height);
        _sheetBounds = new Rectangle(5, 1, width - 5, height - 5);
    }

    /// <summary>
    /// Refresh this window with a full redraw on screen. This method should
    /// be used when row or column titles have changed, including scrolling
    /// and column width.
    /// </summary>
    public void Refresh() {
        RenderFrame();
        Render();
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
            KeyCommand.KC_UP => CursorUp(),
            KeyCommand.KC_DOWN => CursorDown(),
            KeyCommand.KC_HOME => CursorHome(),
            KeyCommand.KC_PAGEUP => CursorPageUp(),
            KeyCommand.KC_PAGEDOWN => CursorPageDown(),
            KeyCommand.KC_GOTO_ROWCOL => GotoRowColumn(),
            KeyCommand.KC_ALPHA => InputAlpha(),
            KeyCommand.KC_VALUE => InputValue(),
            KeyCommand.KC_FORMAT_WIDTH => FormatWidth(),
            KeyCommand.KC_FORMAT_DEFAULT => FormatDefaults(),
            _ => RenderHint.NONE
        };
        return ApplyRenderHint(flags);
    }

    /// <summary>
    /// Apply the render hint flags to the current window. On completion,
    /// return just the flags that were not applied.
    /// </summary>
    /// <returns>Unapplied render hint</returns>
    private RenderHint ApplyRenderHint(RenderHint flags) {
        if (flags.HasFlag(RenderHint.CONTENTS)) {
            Render();
            flags &= ~RenderHint.CONTENTS;
            flags |= RenderHint.CURSOR_STATUS;
        }
        if (flags.HasFlag(RenderHint.CURSOR)) {
            PlaceCursor();
            flags &= ~RenderHint.CURSOR;
            flags |= RenderHint.CURSOR_STATUS;
        }
        if (flags.HasFlag(RenderHint.CURSOR_STATUS)) {
            Screen.UpdateCursorPosition();
            flags &= ~RenderHint.CURSOR_STATUS;
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
        int x = _sheetBounds.X;
        _numberOfColumns = 0;
        while (x < frameRect.Right && columnNumber <= Consts.MaxColumns) {
            int width = Sheet.ColumnWidth(columnNumber);
            int space = Math.Min(width, frameRect.Width - x);
            if (space == width) {
                ++_numberOfColumns;
            }
            Terminal.SetCursor(x, frameRect.Top);
            Terminal.Write(Utilities.CentreString(columnNumber.ToString(), width)[..space]);
            x += width;
            columnNumber++;
        }

        // Row numbers
        int rowNumber = 1 + _scrollOffset.Y;
        int y = _sheetBounds.Y;
        while (y < frameRect.Bottom - CommandBar.Height && rowNumber <= Consts.MaxRows) {
            Terminal.SetCursor(frameRect.Left, y);
            Terminal.Write(rowNumber.ToString().PadLeft(4));
            y += 1;
            rowNumber++;
        }
    }

    /// <summary>
    /// Draw the sheet in its entirety. Only the sheet contents are changed, not
    /// the row and column titles, so this method is only applicable where the
    /// changes are to the cell values. If the rows or column titles need to be
    /// updated, perhaps due to scrolling or column width changes, call the Refresh
    /// method instead.
    /// </summary>
    private void Render() {
        Point cursorPosition = Terminal.GetCursor();
        for (int y = _sheetBounds.Top; y < _sheetBounds.Bottom; ++y) {
            Terminal.SetCursor(_sheetBounds.Left, y);
            Terminal.Write(new string(' ', _sheetBounds.Width));
        }
        foreach (Cell cell in Sheet.Cells.Values) {
            if (cell.Row >= _scrollOffset.Y && cell.Column >= _scrollOffset.X) {
                cell.Draw(Sheet, GetXPositionOfCell(cell.Column), GetYPositionOfCell(cell.Row));
            }
        }
        PlaceCursor();
        Terminal.SetCursor(cursorPosition.X, cursorPosition.Y);
    }

    /// <summary>
    /// Draw the cursor
    /// </summary>
    private void PlaceCursor() {
        ShowCell(Sheet.Column, Sheet.Row, Screen.Colours.BackgroundColour, Screen.Colours.ForegroundColour);
    }

    /// <summary>
    /// Clear the cursor.
    /// </summary>
    private void ResetCursor() {
        ShowCell(Sheet.Column, Sheet.Row, Screen.Colours.ForegroundColour, Screen.Colours.BackgroundColour);
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
        Terminal.ForegroundColour = fgColour;
        Terminal.BackgroundColour = bgColour;
        Cell cell = Sheet.Cell(column, row, false);
        cell.Draw(Sheet, GetXPositionOfCell(column), GetYPositionOfCell(row));
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
    /// Return the Y position of the specified row
    /// </summary>
    /// <param name="row">1-based row index</param>
    /// <returns>Y position of row</returns>
    private int GetYPositionOfCell(int row) {
        int y = _sheetBounds.Top;
        for (int d = _scrollOffset.Y; d < row - 1; d++) {
            y += Sheet.RowHeight(d + 1);
        }
        return y;
    }

    /// <summary>
    /// Handle the format width command to set column widths
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint FormatWidth() {
        RenderHint flags = RenderHint.NONE;
        FormField[] formFields = [
            new() {
                Type = FormFieldType.NUMBER_OR_DEFAULT,
                Width = 2,
                Default = Consts.DefaultColumnWidth,
                Value = new Variant("d")
            },
            new() {
                Text = Calc.FormatWidthColumn,
                Type = FormFieldType.NUMBER,
                Width = 3,
                Value = new Variant(Sheet.Column)
            },
            new() {
                Text = Calc.FormatWidthThrough,
                Type = FormFieldType.NUMBER,
                Width = 3,
                Value = new Variant(Sheet.Column)
            }
        ];
        if (Screen.Command.PromptForInput(Calc.FormatWidthPrompt, formFields)) {
            int newWidth = formFields[0].Value.IntValue;
            int startColumn = formFields[1].Value.IntValue;
            int endColumn = formFields[2].Value.IntValue;
            while (startColumn <= endColumn) {
                Sheet.SetColumnWidth(startColumn++, newWidth);
            }
            flags = RenderHint.REFRESH;
        }
        return flags;
    }

    /// <summary>
    /// Set the default cell formatting.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint FormatDefaults() {
        RenderHint flags = RenderHint.NONE;
        FormField[] formFields = [
            new() {
                Text = Calc.FormatAlignment,
                Type = FormFieldType.PICKER,
                PickerList = new Dictionary<int, string> {
                    { (int)CellAlignment.CENTRE, "Ctr" },
                    { (int)CellAlignment.GENERAL, "Gen" },
                    { (int)CellAlignment.LEFT, "Left" },
                    { (int)CellAlignment.RIGHT, "Right" }
                },
                Value = new Variant((int)Screen.Config.DefaultCellAlignment)
            },
            new() {
                Text = Calc.FormatCode,
                Type = FormFieldType.PICKER,
                PickerList = new Dictionary<int, string> {
                    { (int)CellFormat.CONTINUOUS, "Cont" },
                    { (int)CellFormat.EXPONENTIAL, "Exp" },
                    { (int)CellFormat.FIXED, "Fix" },
                    { (int)CellFormat.GENERAL, "Gen" },
                    { (int)CellFormat.INTEGER, "Int" },
                    { (int)CellFormat.CURRENCY, "$" },
                    { (int)CellFormat.BAR, "*" },
                    { (int)CellFormat.PERCENT, "%" }
                },
                Value = new Variant((int)Screen.Config.DefaultCellFormat)
            },
            new() {
                Text = Calc.FormatDecimals,
                Type = FormFieldType.NUMBER,
                Width = 1,
                Value = new Variant(Screen.Config.DefaultDecimals)
            }
        ];
        if (Screen.Command.PromptForInput(Calc.FormatDefaultCells, formFields)) {
            CellAlignment alignment = (CellAlignment)formFields[0].Value.IntValue;
            CellFormat format = (CellFormat)formFields[1].Value.IntValue;
            int decimals = formFields[2].Value.IntValue;
            Screen.Config.DefaultCellAlignment = alignment;
            Screen.Config.DefaultDecimals = decimals;
            Screen.Config.DefaultCellFormat = format;
            flags = RenderHint.REFRESH;
        }
        return flags;
    }

    /// <summary>
    /// Input alpha text into a cell
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint InputAlpha() {
        return InputAlphaOrValue(CellInputFlags.ALPHA);
    }

    /// <summary>
    /// Input value text into a cell
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint InputValue() {
        return InputAlphaOrValue(CellInputFlags.VALUE);
    }

    /// <summary>
    /// Input alpha or value text into a cell depending on parameter.
    /// </summary>
    /// <param name="flags">Input flags</param>
    /// <returns>Render hint</returns>
    private RenderHint InputAlphaOrValue(CellInputFlags flags) {
        RenderHint hint = RenderHint.NONE;
        do {
            Cell cell = Sheet.Cell(Sheet.Column, Sheet.Row, true);
            CellValue value = cell.Value;
            CellInputResponse result = Screen.Command.PromptForCellInput(flags, ref value);
            if (result == CellInputResponse.CANCEL) {
                break;
            }
            cell.Value = value;
            cell.Draw(Sheet, GetXPositionOfCell(cell.Column), GetYPositionOfCell(cell.Row));
            if (result == CellInputResponse.ACCEPT) {
                hint = RenderHint.CURSOR_STATUS;
                break;
            }
            switch (result) {
                case CellInputResponse.ACCEPT_UP:
                    ApplyRenderHint(CursorUp());
                    break;
                case CellInputResponse.ACCEPT_DOWN:
                    ApplyRenderHint(CursorDown());
                    break;
                case CellInputResponse.ACCEPT_LEFT:
                    ApplyRenderHint(CursorLeft());
                    break;
                case CellInputResponse.ACCEPT_RIGHT:
                    ApplyRenderHint(CursorRight());
                    break;
            }
            flags = CellInputFlags.ALPHAVALUE;
        } while (true);
        return hint;
    }

    /// <summary>
    /// Move the cursor to the home position.
    /// </summary>
    /// <returns></returns>
    private RenderHint CursorHome() {
        RenderHint flags = RenderHint.NONE;
        if (Sheet is { Column: > 1, Row: > 1 }) {
            ResetCursor();
            flags = RenderHint.CURSOR;
            if (_scrollOffset.X > 0 || _scrollOffset.Y > 0) {
                flags |= RenderHint.REFRESH;
                _scrollOffset = new Point(0, 0);
            }
            Sheet.Column = 1;
            Sheet.Row = 1;
        }
        return flags;
    }

    /// <summary>
    /// Move the cell selector left one cell
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint CursorLeft() {
        RenderHint flags = RenderHint.NONE;
        if (Sheet.Column > 1) {
            ResetCursor();
            --Sheet.Column;
            if (Sheet.Column <= _scrollOffset.X) {
                --_scrollOffset.X;
                flags |= RenderHint.REFRESH;
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
        if (Sheet.Column < Consts.MaxColumns) {
            ResetCursor();
            ++Sheet.Column;
            if (GetXPositionOfCell(Sheet.Column) + Sheet.ColumnWidth(Sheet.Column) >= _sheetBounds.Right) {
                ++_scrollOffset.X;
                flags |= RenderHint.REFRESH;
            }
            else {
                flags |= RenderHint.CURSOR;
            }
        }
        return flags;
    }

    /// <summary>
    /// Move the cell selector up one row
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint CursorUp() {
        RenderHint flags = RenderHint.NONE;
        if (Sheet.Row > 1) {
            ResetCursor();
            --Sheet.Row;
            if (Sheet.Row <= _scrollOffset.Y) {
                --_scrollOffset.Y;
                flags |= RenderHint.REFRESH;
            }
            else {
                flags |= RenderHint.CURSOR;
            }
        }
        return flags;
    }

    /// <summary>
    /// Move the cell selector up one page
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint CursorPageUp() {
        RenderHint flags = RenderHint.NONE;
        int pageSize = Math.Min(_sheetBounds.Height, Sheet.Row - 1);
        if (Sheet.Row > 1) {
            ResetCursor();
            Sheet.Row -= pageSize;
            if (Sheet.Row < _scrollOffset.Y) {
                _scrollOffset.Y -= pageSize;
                flags |= RenderHint.REFRESH;
            }
            else {
                flags |= RenderHint.CURSOR;
            }
        }
        return flags;
    }

    /// <summary>
    /// Move the cell selector down one row
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint CursorDown() {
        RenderHint flags = RenderHint.NONE;
        if (Sheet.Row < Consts.MaxRows) {
            ResetCursor();
            ++Sheet.Row;
            if (GetYPositionOfCell(Sheet.Row) + Sheet.RowHeight(Sheet.Row) > _sheetBounds.Bottom) {
                ++_scrollOffset.Y;
                flags |= RenderHint.REFRESH;
            }
            else {
                flags |= RenderHint.CURSOR;
            }
        }
        return flags;
    }

    /// <summary>
    /// Move the cell selector down one page
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint CursorPageDown() {
        RenderHint flags = RenderHint.NONE;
        int pageSize = _sheetBounds.Height;
        if (Sheet.Row + pageSize < Consts.MaxRows) {
            ResetCursor();
            Sheet.Row += pageSize;
            if (GetYPositionOfCell(Sheet.Row) + pageSize * Sheet.RowHeight(Sheet.Row) > _sheetBounds.Bottom) {
                _scrollOffset.Y += pageSize;
                flags |= RenderHint.REFRESH;
            }
            else {
                flags |= RenderHint.CURSOR;
            }
        }
        return flags;
    }

    /// <summary>
    /// Sync the Sheet.Row and Sheet.Column to ensure they are visible on the window by
    /// adjusting the scroll offsets as appropriate.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint SyncRowColumnToSheet() {
        RenderHint flags = RenderHint.CURSOR;
        if (Sheet.Column <= _scrollOffset.X) {
            _scrollOffset.X = Sheet.Column - 1;
            flags |= RenderHint.REFRESH;
        }
        if (Sheet.Column > _numberOfColumns + _scrollOffset.X) {
            _scrollOffset.X =  Sheet.Column - _numberOfColumns;
            flags |= RenderHint.REFRESH;
        }
        if (Sheet.Row <= _scrollOffset.Y) {
            _scrollOffset.Y = Sheet.Row - 1;
            flags |= RenderHint.REFRESH;
        }
        if (Sheet.Row > _sheetBounds.Height + _scrollOffset.Y) {
            _scrollOffset.Y = Sheet.Row - _sheetBounds.Height;
            flags |= RenderHint.REFRESH;
        }
        return flags;
    }

    /// <summary>
    /// Prompt for a row and column to move the cursor
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint GotoRowColumn() {
        RenderHint flags = RenderHint.NONE;
        FormField[] formFields = [
            new() {
                Text = Calc.GotoRowPrompt,
                Type = FormFieldType.NUMBER,
                Width = 4,
                Value = new Variant(Sheet.Row)
            },
            new() {
                Text = Calc.GotoColumnPrompt,
                Type = FormFieldType.NUMBER,
                Width = 3,
                Value = new Variant(Sheet.Column)
            }
        ];
        if (Screen.Command.PromptForInput(Calc.GotoPrompt, formFields)) {
            int newRow = formFields[0].Value.IntValue;
            int newColumn = formFields[1].Value.IntValue;
            if (newRow is >= 1 and <= Consts.MaxRows && newColumn is >= 1 and <= Consts.MaxColumns) {
                ResetCursor();
                Sheet.Row = newRow;
                Sheet.Column = newColumn;
                flags = SyncRowColumnToSheet();
            }
        }
        return flags;
    }
}