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

using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using CsvHelper;
using JCalc.Resources;
using JCalcLib;
using JComLib;

namespace JCalc;

public class Window {
    private Rectangle _viewportBounds;
    private Rectangle _sheetBounds;
    private Point _scrollOffset = Point.Empty;
    private Point _markAnchor;
    private Point _lastMarkPoint;
    private bool _isMarkMode;
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
    /// Set the viewport and sheet bounds for the window.
    /// </summary>
    /// <param name="x">Left edge, 0 based</param>
    /// <param name="y">Top edge, 0 based</param>
    /// <param name="width">Width of window</param>
    /// <param name="height">Height of window</param>
    public void SetViewportBounds(int x, int y, int width, int height) {
        _viewportBounds = new Rectangle(x, y, width, height);
        _sheetBounds = new Rectangle(x + 5, y + CommandBar.Height + 1, width - 5, height - (CommandBar.Height + 1 + StatusBar.Height));
        _markAnchor = Point.Empty;
        _isMarkMode = false;
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
            KeyCommand.KC_MARK => ToggleMarkMode(),
            KeyCommand.KC_GOTO => GotoRowColumn(),
            KeyCommand.KC_VALUE => InputValue(false),
            KeyCommand.KC_EDIT => InputValue(true),
            KeyCommand.KC_FILE_EXPORT => ExportFile(),
            KeyCommand.KC_SET_COLUMN_WIDTH => SetColumnWidth(),
            KeyCommand.KC_RESET_COLUMN_WIDTH => ResetColumnWidth(),
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
            KeyCommand.KC_FILE_SAVE => SaveSheet(),
            KeyCommand.KC_INSERT_COLUMN => InsertColumn(),
            KeyCommand.KC_INSERT_ROW => InsertRow(),
            KeyCommand.KC_DELETE_COLUMN => DeleteColumn(),
            KeyCommand.KC_DELETE_ROW => DeleteRow(),
            KeyCommand.KC_DELETE => DeleteCells(),
            KeyCommand.KC_RANGE_EXPORT => ExportRange(),
            KeyCommand.KC_RANGE_SORT => SortRange(),
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
            flags &= ~RenderHint.CONTENTS | RenderHint.BLOCK;
            flags |= RenderHint.CURSOR_STATUS;
        }
        if (flags.HasFlag(RenderHint.BLOCK)) {
            RenderBlock();
            flags &= ~RenderHint.BLOCK;
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
        Terminal.BackgroundColour = Screen.Colours.SelectionColour;

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
            Terminal.Write(Utilities.CentreString(Cell.ColumnToAddress(columnNumber), width)[..space]);
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
        ConsoleColor bg = Screen.Colours.BackgroundColour;
        ConsoleColor fg = Screen.Colours.ForegroundColour;

        RExtent renderExtent = new RExtent()
            .Add(new Point(1, _scrollOffset.Y + 1))
            .Add(new Point(_sheetBounds.Width, _scrollOffset.Y + _sheetBounds.Height));
        RExtent markExtent = new RExtent();
        if (_isMarkMode) {
            markExtent
                .Add(_markAnchor)
                .Add(Sheet.Location.Point);
        }

        int i = renderExtent.Start.Y;
        int y = _sheetBounds.Top;
        while (i <= renderExtent.End.Y) {
            string line = Sheet.GetRow(_scrollOffset.X + 1, i);
            int x = _sheetBounds.Left;
            int w = _sheetBounds.Width;
            int left = 0;
            int length = Math.Min(w, line.Length - left);

            if (i >= markExtent.Start.Y && i <= markExtent.End.Y) {
                int extentStart = GetXPositionOfCell(markExtent.Start.X);
                int extentEnd = GetXPositionOfCell(markExtent.End.X + 1);
                if (extentStart > x) {
                    int diff = extentStart - x;
                    Terminal.Write(x, y, diff, bg, fg, Utilities.SpanBound(line, left, diff));
                    x += diff;
                    w -= diff;
                    left += diff;
                    length -= diff;
                }
                if (extentEnd > _scrollOffset.X) {
                    int extentWidth = extentEnd - extentStart;
                    bg = Screen.Colours.SelectionColour;
                    fg = Screen.Colours.BackgroundColour;
                    Terminal.Write(x, y, extentWidth, bg, fg, Utilities.SpanBound(line, left, extentWidth));
                    x += extentWidth;
                    w -= extentWidth;
                    left += extentWidth;
                    length -= extentWidth;
                }
                bg = Screen.Colours.BackgroundColour;
                fg = Screen.Colours.ForegroundColour;
            }
            Terminal.Write(x, y, w, bg, fg, Utilities.SpanBound(line, left, length));

            bg = Screen.Colours.BackgroundColour;
            fg = Screen.Colours.ForegroundColour;
            ++i;
            ++y;
        }
        PlaceCursor();
        Terminal.SetCursor(cursorPosition.X, cursorPosition.Y);
    }

    /// <summary>
    /// Return the mark extent.
    /// </summary>
    /// <returns>An RExtent with the mark extent</returns>
    private RExtent GetMarkExtent() {
        RExtent markExtent = new RExtent()
            .Add(Sheet.Location.Point);
        if (_isMarkMode) {
            markExtent.Add(_markAnchor);
        }
        return markExtent;
    }

    /// <summary>
    /// Clear the marked block from the window by rendering the
    /// original marked block area.
    /// </summary>
    private void ClearBlock() {
        if (_isMarkMode) {
            RExtent extent = GetMarkExtent();
            _isMarkMode = false;
            RenderExtent(extent);
        }
    }

    /// <summary>
    /// Render the area covered by the mark including any area
    /// uncovered as indicated by the _lastMarkPoint anchor.
    /// </summary>
    private void RenderBlock() {
        RExtent extent = new RExtent()
            .Add(_markAnchor)
            .Add(Sheet.Location.Point)
            .Add(_lastMarkPoint);
        RenderExtent(extent);
    }

    /// <summary>
    /// Render the area of the window specified by the extent
    /// which indicates the top left and bottom right portion
    /// of the sheet to be rendered.
    /// </summary>
    /// <param name="extent">Extent to be rendered</param>
    private void RenderExtent(RExtent extent) {
        RExtent markExtent = new RExtent();
        if (_isMarkMode) {
            markExtent
                .Add(_markAnchor)
                .Add(Sheet.Location.Point);
        }
        for (int row = extent.Start.Y; row <= extent.End.Y; row++) {
            for (int column = extent.Start.X; column <= extent.End.X; column++) {
                bool inMarked = markExtent.Contains(new Point(column, row));
                ConsoleColor fg = inMarked ? Screen.Colours.BackgroundColour : Screen.Colours.ForegroundColour;
                ConsoleColor bg = inMarked ? Screen.Colours.SelectionColour : Screen.Colours.BackgroundColour;
                ShowCell(new CellLocation { Column = column, Row = row}, fg, bg);
            }
        }
    }

    /// <summary>
    /// Draw the cursor
    /// </summary>
    private void PlaceCursor() {
        ShowCell(Sheet.Location, Screen.Colours.BackgroundColour, Screen.Colours.SelectionColour);
    }

    /// <summary>
    /// Clear the cursor.
    /// </summary>
    private void ResetCursor() {
        ShowCell(Sheet.Location, Screen.Colours.ForegroundColour, Screen.Colours.BackgroundColour);
    }

    /// <summary>
    /// Return an iterator over the marked block extent, or just the current
    /// cursor position if no block is marked.
    /// </summary>
    /// <returns>The next cell location in the iterator, or null</returns>
    private IEnumerable<CellLocation> RangeIterator() {
        RExtent markExtent = GetMarkExtent();
        for (int row = markExtent.Start.Y; row <= markExtent.End.Y; row++) {
            for (int column = markExtent.Start.X; column <= markExtent.End.X; column++) {
                yield return new CellLocation { Column = column, Row = row };
            }
        }
    }

    /// <summary>
    /// Draw the cell at the specified column and row in the given foreground and
    /// background colours
    /// </summary>
    /// <param name="location">Cell location</param>
    /// <param name="fgColour">Foreground colour</param>
    /// <param name="bgColour">Background colour</param>
    private void ShowCell(CellLocation location, ConsoleColor fgColour, ConsoleColor bgColour) {
        Terminal.ForegroundColour = fgColour;
        Terminal.BackgroundColour = bgColour;
        Cell cell = Sheet.GetCell(location, false);
        Sheet.DrawCell(cell, GetXPositionOfCell(location.Column), GetYPositionOfCell(location.Row));
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
        RenderHint flags = _isMarkMode ? RenderHint.CONTENTS : RenderHint.CURSOR;
        foreach (CellLocation location in RangeIterator()) {
            Sheet.SetCellAlignment(Sheet.GetCell(location, true), alignment);
        }
        ClearBlock();
        return flags;
    }

    /// <summary>
    /// Delete a range of cells
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint DeleteCells() {
        foreach (CellLocation location in RangeIterator()) {
            Sheet.DeleteCell(Sheet.GetCell(location, false));
        }
        ClearBlock();
        return RenderHint.CONTENTS;
    }

    /// <summary>
    /// Format a range of cells
    /// </summary>
    /// <param name="format">Requested format</param>
    /// <returns>Render hint</returns>
    private RenderHint FormatCells(CellFormat format) {
        RenderHint flags = _isMarkMode ? RenderHint.CONTENTS : RenderHint.CURSOR;
        Cell cell = Sheet.ActiveCell;
        int decimalPlaces = 0;
        if (format is CellFormat.FIXED or CellFormat.SCIENTIFIC or CellFormat.CURRENCY or CellFormat.PERCENT) {
            FormField[] formFields = [
                new() {
                    Text = Calc.EnterDecimalPlaces,
                    Type = FormFieldType.NUMBER,
                    Width = 2,
                    MinimumValue = 0,
                    MaximumValue = 15,
                    Value = new Variant(cell.DecimalPlaces)
                }
            ];
            if (!Screen.Command.PromptForInput(formFields)) {
                return RenderHint.CANCEL;
            }
            decimalPlaces = formFields[0].Value.IntValue;
            Debug.Assert(decimalPlaces >= formFields[0].MinimumValue && decimalPlaces <= formFields[0].MaximumValue);
        }
        foreach (CellLocation location in RangeIterator()) {
            Sheet.SetCellFormat(Sheet.GetCell(location, true), format, decimalPlaces);
        }
        ClearBlock();
        return flags;
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
                MinimumValue = 1,
                MaximumValue = 72,
                Value = new Variant(Sheet.ColumnWidth(Sheet.Location.Column))
            }
        ];
        if (Screen.Command.PromptForInput(formFields)) {
            int newWidth = formFields[0].Value.IntValue;
            Debug.Assert(newWidth >= formFields[0].MinimumValue && newWidth <= formFields[0].MaximumValue);
            flags = Sheet.SetColumnWidth(Sheet.Location.Column, newWidth) ? RenderHint.REFRESH : RenderHint.NONE;
        }
        return flags;
    }

    /// <summary>
    /// Reset the current column width to the global default.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint ResetColumnWidth() {
        RenderHint flags = RenderHint.NONE;
        if (Sheet.SetColumnWidth(Sheet.Location.Column, Consts.DefaultColumnWidth)) {
            flags = RenderHint.REFRESH;
        }
        return flags;
    }

    /// <summary>
    /// Insert a column to the left of the cursor position
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint InsertColumn() {
        Sheet.InsertColumn(Sheet.Location.Column);
        return RenderHint.CONTENTS;
    }

    /// <summary>
    /// Insert a row above the cursor position
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint InsertRow() {
        Sheet.InsertRow(Sheet.Location.Row);
        return RenderHint.CONTENTS;
    }

    /// <summary>
    /// Delete a column at the cursor position
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint DeleteColumn() {
        Sheet.DeleteColumn(Sheet.Location.Column);
        return RenderHint.CONTENTS;
    }

    /// <summary>
    /// Delete a row at the cursor position
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint DeleteRow() {
        Sheet.DeleteRow(Sheet.Location.Row);
        return RenderHint.CONTENTS;
    }

    /// <summary>
    /// Export the entire worksheet to a CSV file.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint ExportFile() {
        return ExportExtent(Sheet.GetCellExtent());
    }

    /// <summary>
    /// Export the selected range of cells to a CSV file.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint ExportRange() {
        return ExportExtent(GetMarkExtent());
    }

    /// <summary>
    /// Export the specified extent to a CSV file
    /// </summary>
    /// <param name="extent">Extent to export</param>
    /// <returns>Render hints</returns>
    private RenderHint ExportExtent(RExtent extent) {
        RenderHint flags = RenderHint.CANCEL;
        FormField[] formFields = [
            new() {
                Text = Calc.ExportFilename,
                Type = FormFieldType.TEXT,
                Width = 50,
                AllowFilenameCompletion = true,
                FilenameCompletionFilter = $"*{Consts.CSVExtension}",
                Value = new Variant(string.Empty)
            }
        ];
        if (Screen.Command.PromptForInput(formFields)) {
            string inputValue = formFields[0].Value.StringValue;
            Debug.Assert(!string.IsNullOrEmpty(inputValue));

            inputValue = Utilities.AddExtensionIfMissing(inputValue, Consts.CSVExtension);

            try {
                using FileStream stream = File.Create(inputValue);
                using StreamWriter textStream = new(stream);
                using CsvWriter csvWriter = new CsvWriter(textStream, CultureInfo.InvariantCulture);

                if (extent.Valid) {
                    for (int row = extent.Start.Y; row <= extent.End.Y; row++) {
                        for (int column = extent.Start.X; column <= extent.End.X; column++) {
                            Cell cell = Sheet.GetCell(new CellLocation { Column = column, Row = row }, false);
                            csvWriter.WriteField(cell.CellValue.Value);
                        }
                        csvWriter.NextRecord();
                    }
                }
                csvWriter.Flush();
            }
            catch (Exception e) {
                Screen.Command.Error(string.Format(Calc.ErrorExportingFile, inputValue, e.Message));
            }
            flags = RenderHint.NONE;
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
                AllowFilenameCompletion = true,
                FilenameCompletionFilter = $"*{Consts.DefaultExtension}",
                Value = new Variant(Sheet.Name)
            }
        ];
        if (Screen.Command.PromptForInput(formFields)) {
            string inputValue = formFields[0].Value.StringValue;
            Debug.Assert(!string.IsNullOrEmpty(inputValue));
            inputValue = Utilities.AddExtensionIfMissing(inputValue, Consts.DefaultExtension);
            Sheet.Filename = inputValue;
            Sheet.Write();
            Screen.Status.UpdateFilename(Sheet.Name);
            flags = RenderHint.NONE;
        }
        return flags;
    }

    /// <summary>
    /// Sort a marked range of cells
    /// </summary>
    /// <returns></returns>
    private RenderHint SortRange() {
        RenderHint flags = RenderHint.CANCEL;
        FormField[] formFields = [
            new() {
                Text = Calc.EnterSortColumn,
                Type = FormFieldType.TEXT,
                Width = 2,
                Value = new Variant(Cell.ColumnToAddress(Sheet.Location.Column))
            },
            new() {
                Text = Calc.EnterSortOrder,
                Type = FormFieldType.PICKER,
                List = ["ASC", "DESC"],
                Value = new Variant("ASC")
            }
        ];
        if (Screen.Command.PromptForInput(formFields)) {
            RExtent markExtent = GetMarkExtent();
            int sortColumn = Cell.AddressToColumn(formFields[0].Value.StringValue);
            bool descending = formFields[1].Value.StringValue == "DESC";
            if (sortColumn >= markExtent.Start.X && sortColumn <= markExtent.End.X) {
                Sheet.SortCells(sortColumn, descending, markExtent);
            }
            flags = RenderHint.CURSOR;
        }
        ClearBlock();
        return flags;
    }

    /// <summary>
    /// Input value text into a cell or edit the existing value in the cell.
    /// </summary>
    /// <param name='editValue'>True if we're editing the existing cell, false otherwise</param>
    /// <returns>Render hint</returns>
    private RenderHint InputValue(bool editValue) {
        RenderHint hint = RenderHint.NONE;
        Cell cell = Sheet.GetCell(Sheet.Location, true);
        CellValue value = editValue ? cell.CellValue : new CellValue();
        CellInputResponse result = Screen.Command.PromptForCellInput(ref value);
        if (result != CellInputResponse.CANCEL) {

            Sheet.SetCellValue(cell, value);

            Calculate calc = new Calculate(Sheet);
            calc.Update();
            UpdateCells([cell]);
            UpdateCells(calc.CellsToUpdate);

            hint = result switch {
                CellInputResponse.ACCEPT => RenderHint.CURSOR,
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
    /// Render a collection of cells
    /// </summary>
    /// <param name="cells">List of cells to update</param>
    private void UpdateCells(List<Cell> cells) {
        Terminal.ForegroundColour = Screen.Colours.ForegroundColour;
        Terminal.BackgroundColour = Screen.Colours.BackgroundColour;
        foreach (Cell cell in cells) {
            int x = GetXPositionOfCell(cell.Location.Column);
            int y = GetYPositionOfCell(cell.Location.Row);
            if (_sheetBounds.Contains(new Point(x, y))) {
                Sheet.DrawCell(cell, x, y);
            }
        }
    }

    /// <summary>
    /// Toggle whether we are in mark mode when moving the
    /// cursor over the screen.
    /// </summary>
    /// <returns></returns>
    private RenderHint ToggleMarkMode() {
        RenderHint flags;
        if (_isMarkMode) {
            ClearBlock();
            flags = RenderHint.CURSOR;
        }
        else {
            _markAnchor = Sheet.Location.Point;
            _lastMarkPoint = _markAnchor;
            _isMarkMode = true;
            flags = RenderHint.BLOCK;
        }
        return flags;
    }

    /// <summary>
    /// Save the last mark point at the start or end of marking a
    /// range. If the Shift key is down, we either drop the mark
    /// anchor if we are not currently in mark mode or update the
    /// mark point to where the cursor currently is before it gets
    /// moved by the caller.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint SaveLastMarkPoint() {
        RenderHint flags = RenderHint.NONE;
        if (_isMarkMode) {
           _lastMarkPoint = Sheet.Location.Point;
            flags = RenderHint.BLOCK;
        }
        if (!_isMarkMode) {
            ResetCursor();
        }
        return flags;
    }

    /// <summary>
    /// Move the cursor to the home position.
    /// </summary>
    /// <returns></returns>
    private RenderHint CursorHome() {
        RenderHint flags = RenderHint.NONE;
        if (Sheet is { Location: { Column: > 1, Row: > 1 } }) {
            flags = SaveLastMarkPoint();
            if (_scrollOffset.X > 0 || _scrollOffset.Y > 0) {
                flags |= RenderHint.REFRESH;
                _scrollOffset = new Point(0, 0);
            }
            Sheet.Location = new CellLocation { Column = 1, Row = 1 };
        }
        return flags;
    }

    /// <summary>
    /// Move the cell selector left one cell
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint CursorLeft() {
        RenderHint flags = RenderHint.NONE;
        CellLocation sheetLocation = Sheet.Location;
        if (sheetLocation.Column > 1) {
            flags = SaveLastMarkPoint();
            --sheetLocation.Column;
            Sheet.Location = sheetLocation;
            if (sheetLocation.Column <= _scrollOffset.X) {
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
        CellLocation sheetLocation = Sheet.Location;
        if (sheetLocation.Column < Consts.MaxColumns) {
            flags = SaveLastMarkPoint();
            ++sheetLocation.Column;
            Sheet.Location = sheetLocation;
            if (GetXPositionOfCell(sheetLocation.Column) + Sheet.ColumnWidth(sheetLocation.Column) >= _sheetBounds.Right) {
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
        CellLocation sheetLocation = Sheet.Location;
        if (sheetLocation.Row > 1) {
            flags = SaveLastMarkPoint();
            --sheetLocation.Row;
            Sheet.Location = sheetLocation;
            if (sheetLocation.Row <= _scrollOffset.Y) {
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
        RenderHint flags = SaveLastMarkPoint();
        CellLocation sheetLocation = Sheet.Location;
        int previousRow = sheetLocation.Row;
        sheetLocation.Row = Math.Max(sheetLocation.Row - _sheetBounds.Height, 1);
        Sheet.Location = sheetLocation;
        if (sheetLocation.Row == previousRow) {
            PlaceCursor();
        } else {
            _scrollOffset.Y -= previousRow - sheetLocation.Row;
            if (_scrollOffset.Y < 0) {
                _scrollOffset.Y = 0;
            }
            flags |= RenderHint.REFRESH;
        }
        return flags;
    }

    /// <summary>
    /// Move the cell selector down one row
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint CursorDown() {
        RenderHint flags = RenderHint.NONE;
        CellLocation sheetLocation = Sheet.Location;
        if (sheetLocation.Row < Consts.MaxRows) {
            flags = SaveLastMarkPoint();
            ++sheetLocation.Row;
            Sheet.Location = sheetLocation;
            if (GetYPositionOfCell(sheetLocation.Row) + Sheet.RowHeight > _sheetBounds.Bottom) {
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
        RenderHint flags = SaveLastMarkPoint();
        CellLocation sheetLocation = Sheet.Location;
        int previousRow = sheetLocation.Row;
        sheetLocation.Row = Math.Min(sheetLocation.Row + _sheetBounds.Height, Consts.MaxRows);
        Sheet.Location = sheetLocation;
        if (sheetLocation.Row == previousRow) {
            PlaceCursor();
        } else {
            _scrollOffset.Y += sheetLocation.Row - previousRow;
            flags |= RenderHint.REFRESH;
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
        if (Sheet.Location.Column <= _scrollOffset.X) {
            _scrollOffset.X = Sheet.Location.Column - 1;
            flags |= RenderHint.REFRESH;
        }
        if (Sheet.Location.Column > _numberOfColumns + _scrollOffset.X) {
            _scrollOffset.X =  Sheet.Location.Column - _numberOfColumns;
            flags |= RenderHint.REFRESH;
        }
        if (Sheet.Location.Row <= _scrollOffset.Y) {
            _scrollOffset.Y = Sheet.Location.Row - 1;
            flags |= RenderHint.REFRESH;
        }
        if (Sheet.Location.Row > _sheetBounds.Height + _scrollOffset.Y) {
            _scrollOffset.Y = Sheet.Location.Row - _sheetBounds.Height;
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
                Value = new Variant(Sheet.ActiveCell.Address)
            }
        ];
        if (Screen.Command.PromptForInput(formFields)) {
            string newAddress = formFields[0].Value.StringValue;
            CellLocation location = Cell.LocationFromAddress(newAddress);
            if (location.Row is >= 1 and <= Consts.MaxRows && location.Column is >= 1 and <= Consts.MaxColumns) {
                ResetCursor();
                Sheet.Location = location;
                flags = SyncRowColumnToSheet();
            }
            else {
                flags |= RenderHint.NONE;
            }
        }
        return flags;
    }
}