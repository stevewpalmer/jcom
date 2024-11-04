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
        _sheetBounds = new Rectangle(x + 5, y + CommandBar.Height + 1, width - 5, height - (CommandBar.Height + 1 + StatusBar.Height));
    }

    /// <summary>
    /// Refresh this window with a full redraw on screen. This method should
    /// be used when row or column titles have changed, including scrolling
    /// and column width.
    /// </summary>
    public void Refresh() {
        Screen.UpdateCursorPosition();
        Screen.Status.UpdateFilename(Sheet.Name);
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
            KeyCommand.KC_GOTO => GotoRowColumn(),
            KeyCommand.KC_VALUE => InputValue(false),
            KeyCommand.KC_SET_COLUMN_WIDTH => SetColumnWidth(),
            KeyCommand.KC_RESET_COLUMN_WIDTH => ResetColumnWidth(),
            KeyCommand.KC_EDIT => InputValue(true),
            KeyCommand.KC_ALIGN_LEFT => AlignCells(CellAlignment.LEFT),
            KeyCommand.KC_ALIGN_RIGHT => AlignCells(CellAlignment.RIGHT),
            KeyCommand.KC_ALIGN_CENTRE => AlignCells(CellAlignment.CENTRE),
            KeyCommand.KC_FORMAT_FIXED => FormatCells(CellFormat.FIXED),
            KeyCommand.KC_FORMAT_PERCENT => FormatCells(CellFormat.PERCENT),
            KeyCommand.KC_FORMAT_CURRENCY => FormatCells(CellFormat.CURRENCY),
            KeyCommand.KC_FORMAT_COMMAS => FormatCells(CellFormat.COMMAS),
            KeyCommand.KC_FORMAT_BAR => FormatCells(CellFormat.BAR),
            KeyCommand.KC_FORMAT_SCI => FormatCells(CellFormat.SCIENTIFIC),
            KeyCommand.KC_FORMAT_GENERAL => FormatCells(CellFormat.GENERAL),
            KeyCommand.KC_FORMAT_TEXT => FormatCells(CellFormat.TEXT),
            KeyCommand.KC_FORMAT_RESET => FormatCells(Screen.Config.DefaultCellFormat),
            KeyCommand.KC_DATE_DMY => FormatCells(CellFormat.DATE_DMY),
            KeyCommand.KC_DATE_DM => FormatCells(CellFormat.DATE_DM),
            KeyCommand.KC_DATE_MY => FormatCells(CellFormat.DATE_MY),
            KeyCommand.KC_SAVE => SaveSheet(),
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

        Terminal.ForegroundColour = Screen.Colours.BackgroundColour;
        Terminal.BackgroundColour = Screen.Colours.ForegroundColour;

        // Sheet number
        int x = _sheetBounds.X;
        int y = frameRect.Top + CommandBar.Height;
        Terminal.SetCursor(0, y);
        Terminal.Write($@"  {(char)(Sheet.SheetNumber + 'A' - 1)}  ");

        // Column numbers
        int columnNumber = 1 + _scrollOffset.X;
        _numberOfColumns = 0;
        while (x < frameRect.Right && columnNumber <= Consts.MaxColumns) {
            int width = Sheet.ColumnWidth(columnNumber);
            int space = Math.Min(width, frameRect.Width - x);
            if (space == width) {
                ++_numberOfColumns;
            }
            Terminal.SetCursor(x, y);
            Terminal.Write(Utilities.CentreString(Cell.ColumnNumber(columnNumber), width)[..space]);
            x += width;
            columnNumber++;
        }

        // Row numbers
        int rowNumber = 1 + _scrollOffset.Y;
        y = frameRect.Top + CommandBar.Height + 1;
        while (y < _sheetBounds.Bottom && rowNumber <= Consts.MaxRows) {
            Terminal.SetCursor(frameRect.Left, y);
            Terminal.Write($" {rowNumber.ToString(),-3} ");
            y += 1;
            rowNumber++;
        }
        Terminal.ForegroundColour = Screen.Colours.ForegroundColour;
        Terminal.BackgroundColour = Screen.Colours.BackgroundColour;
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
        foreach (Cell cell in Sheet.Cells.Values.Where(cell => cell.Row >= _scrollOffset.Y && cell.Column >= _scrollOffset.X)) {
            cell.Draw(Sheet, GetXPositionOfCell(cell.Column), GetYPositionOfCell(cell.Row));
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
            y += Sheet.RowHeight;
        }
        return y;
    }

    /// <summary>
    /// Align a range of cells
    /// </summary>
    /// <param name="alignment">Requested alignment</param>
    /// <returns>Render hint</returns>
    private RenderHint AlignCells(CellAlignment alignment) {
        ActiveCell.Alignment = alignment;
        Sheet.Modified = true;
        PlaceCursor();
        return RenderHint.NONE;
    }

    /// <summary>
    /// Format a range of cells
    /// </summary>
    /// <param name="format">Requested format</param>
    /// <returns>Render hint</returns>
    private RenderHint FormatCells(CellFormat format) {
        ActiveCell.Format = format;
        Sheet.Modified = true;
        PlaceCursor();
        return RenderHint.NONE;
    }

    /// <summary>
    /// Handle the format width command to set column widths
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint SetColumnWidth() {
        RenderHint flags = RenderHint.CANCEL;
        FormField[] formFields = [
            new() {
                Text = Calc.EnterColumnWidth,
                Type = FormFieldType.NUMBER,
                Width = 2,
                Value = new Variant(Sheet.ColumnWidth(Sheet.Column))
            }
        ];
        if (Screen.Command.PromptForInput(formFields)) {
            int newWidth = formFields[0].Value.IntValue;
            flags = Sheet.SetColumnWidth(Sheet.Column, newWidth) ? RenderHint.REFRESH : RenderHint.NONE;
        }
        return flags;
    }

    /// <summary>
    /// Reset the current column width to the global default.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint ResetColumnWidth() {
        RenderHint flags = RenderHint.NONE;
        if (Sheet.SetColumnWidth(Sheet.Column, Consts.DefaultColumnWidth)) {
            flags = RenderHint.REFRESH;
        }
        return flags;
    }

    /// <summary>
    /// Save the changes to this sheet.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint SaveSheet() {
        RenderHint flags = RenderHint.CANCEL;
        FormField[] formFields = [
            new() {
                Text = Calc.EnterSaveFilename,
                Type = FormFieldType.TEXT,
                Width = 50,
                Value = new Variant(Sheet.Name)
            }
        ];
        if (Screen.Command.PromptForInput(formFields)) {
            Sheet.Filename = formFields[0].Value.StringValue;
            Sheet.Write();
            Screen.Status.UpdateFilename(Sheet.Name);
            flags = RenderHint.NONE;
        }
        return flags;
    }

    /// <summary>
    /// Input value text into a cell
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint InputValue(bool editValue) {
        RenderHint hint = RenderHint.NONE;
        Cell cell = Sheet.Cell(Sheet.Column, Sheet.Row, true);
        CellValue value = editValue ? cell.Value : new CellValue();
        CellInputResponse result = Screen.Command.PromptForCellInput(ref value);
        if (result != CellInputResponse.CANCEL) {
            cell.Value = value;
            cell.Draw(Sheet, GetXPositionOfCell(cell.Column), GetYPositionOfCell(cell.Row));
            hint = result switch {
                CellInputResponse.ACCEPT => RenderHint.CURSOR_STATUS,
                CellInputResponse.ACCEPT_UP => CursorUp(),
                CellInputResponse.ACCEPT_DOWN => CursorDown(),
                CellInputResponse.ACCEPT_LEFT => CursorLeft(),
                CellInputResponse.ACCEPT_RIGHT => CursorRight(),
                _ => hint
            };
        }
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
            if (GetYPositionOfCell(Sheet.Row) + Sheet.RowHeight > _sheetBounds.Bottom) {
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
            if (GetYPositionOfCell(Sheet.Row) + pageSize * Sheet.RowHeight > _sheetBounds.Bottom) {
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
        RenderHint flags = RenderHint.CANCEL;
        FormField[] formFields = [
            new() {
                Text = Calc.GotoRowPrompt,
                Type = FormFieldType.TEXT,
                Width = 7,
                Value = new Variant(ActiveCell.Position)
            }
        ];
        if (Screen.Command.PromptForInput(formFields)) {
            string newAddress = formFields[0].Value.StringValue;
            (int newColumn, int newRow) = Cell.ColumnAndRowFromPosition(newAddress);
            if (newRow is >= 1 and <= Consts.MaxRows && newColumn is >= 1 and <= Consts.MaxColumns) {
                ResetCursor();
                Sheet.Row = newRow;
                Sheet.Column = newColumn;
                flags = SyncRowColumnToSheet();
            }
            else {
                flags |= RenderHint.NONE;
            }
        }
        return flags;
    }
}