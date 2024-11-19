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
using System.Text.Json;
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
    public void Refresh(RenderHint flags) {
        if (flags.HasFlag(RenderHint.REFRESH)) {
            Screen.UpdateCursorPosition();
            RenderFrame();
            flags |= RenderHint.CONTENTS;
        }
        if (flags.HasFlag(RenderHint.CONTENTS)) {
            Render();
        }
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
            KeyCommand.KC_ALIGN_GENERAL => AlignCells(CellAlignment.GENERAL),
            KeyCommand.KC_FORMAT_FIXED => FormatCells(CellFormat.FIXED),
            KeyCommand.KC_FORMAT_PERCENT => FormatCells(CellFormat.PERCENT),
            KeyCommand.KC_FORMAT_CURRENCY => FormatCells(CellFormat.CURRENCY),
            KeyCommand.KC_FORMAT_COMMAS => FormatCells(CellFormat.COMMAS),
            KeyCommand.KC_FORMAT_SCI => FormatCells(CellFormat.SCIENTIFIC),
            KeyCommand.KC_FORMAT_GENERAL => FormatCells(CellFormat.GENERAL),
            KeyCommand.KC_FORMAT_TEXT => FormatCells(CellFormat.TEXT),
            KeyCommand.KC_FORMAT_RESET => FormatCells(Screen.Config.DefaultCellFormat),
            KeyCommand.KC_DATE_DMY => FormatCells(CellFormat.DATE_DMY),
            KeyCommand.KC_DATE_DM => FormatCells(CellFormat.DATE_DM),
            KeyCommand.KC_DATE_MY => FormatCells(CellFormat.DATE_MY),
            KeyCommand.KC_INSERT_COLUMN => InsertColumn(),
            KeyCommand.KC_INSERT_ROW => InsertRow(),
            KeyCommand.KC_DELETE_COLUMN => DeleteColumn(),
            KeyCommand.KC_DELETE_ROW => DeleteRow(),
            KeyCommand.KC_DELETE => PerformBlockAction(BlockAction.DELETE),
            KeyCommand.KC_RANGE_EXPORT => ExportRange(),
            KeyCommand.KC_RANGE_SORT => SortRange(),
            KeyCommand.KC_STYLE_FG => SetCellForegroundColour(),
            KeyCommand.KC_STYLE_BG => SetCellBackgroundColour(),
            KeyCommand.KC_STYLE_BOLD => SetCellBold(),
            KeyCommand.KC_STYLE_ITALIC => SetCellItalic(),
            KeyCommand.KC_STYLE_UNDERLINE => SetCellUnderline(),
            KeyCommand.KC_COPY => PerformBlockAction(BlockAction.COPY),
            KeyCommand.KC_CUT => PerformBlockAction(BlockAction.CUT),
            KeyCommand.KC_PASTE => Paste(),
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
        if (flags.HasFlag(RenderHint.RECALCULATE)) {
            Calculate calc = new Calculate(Sheet);
            calc.Update();
            if (!flags.HasFlag(RenderHint.CONTENTS)) {
                UpdateCells(calc.CellsToUpdate);
            }
            Sheet.NeedRecalculate = false;
            flags &= ~RenderHint.RECALCULATE;
        }
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

        int fg = Screen.Colours.BackgroundColour;
        int bg = Screen.Colours.SelectionColour;

        // Sheet number
        int x = _sheetBounds.X;
        int y = frameRect.Top + CommandBar.Height;
        Terminal.Write(0, y, 5, fg, bg, $@"  {(char)(Sheet.SheetNumber + 'A' - 1)}  ");

        // Column numbers
        int columnNumber = 1 + _scrollOffset.X;
        _numberOfColumns = 0;
        while (x < frameRect.Right && columnNumber <= Sheet.MaxColumns) {
            int width = Sheet.ColumnWidth(columnNumber);
            int space = Math.Min(width, frameRect.Width - x);
            if (space == width) {
                ++_numberOfColumns;
            }
            Terminal.Write(x, y, width, fg, bg, Utilities.CentreString(Cell.ColumnToAddress(columnNumber), width)[..space]);
            x += width;
            columnNumber++;
        }

        // Row numbers
        int rowNumber = 1 + _scrollOffset.Y;
        y = frameRect.Top + CommandBar.Height + 1;
        while (y < _sheetBounds.Bottom && rowNumber <= Sheet.MaxRows) {
            Terminal.Write(frameRect.Left, y, 5, fg, bg, $" {rowNumber.ToString(),-3} ");
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
            AnsiText line = Sheet.GetRow(_scrollOffset.X + 1, i, _sheetBounds.Width);
            int x = _sheetBounds.Left;
            int w = _sheetBounds.Width;
            int length = Math.Min(w, line.Length);

            if (i >= markExtent.Start.Y && i <= markExtent.End.Y) {
                int extentStart = GetXPositionOfCell(markExtent.Start.X);
                int extentEnd = GetXPositionOfCell(markExtent.End.X + 1);
                int left = 0;
                if (extentStart > x) {
                    left = extentStart - x;
                }
                if (extentEnd > _scrollOffset.X) {
                    int extentWidth = extentEnd - extentStart;
                    line.Style(left, extentWidth, Screen.Colours.BackgroundColour, Screen.Colours.SelectionColour);
                }
            }
            Terminal.Write(x, y, _sheetBounds.Width, line.Substring(0, length));
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
                CellLocation location = new CellLocation { Column = column, Row = row };
                Cell cell = Sheet.GetCell(location, false);
                int fg = inMarked ? Screen.Colours.BackgroundColour : cell.Style.ForegroundColour;
                int bg = inMarked ? Screen.Colours.SelectionColour : cell.Style.BackgroundColour;
                DrawCell(cell, GetXPositionOfCell(location.Column), GetYPositionOfCell(location.Row), fg, bg);
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
        ShowCell(Sheet.Location, Sheet.ActiveCell.Style.ForegroundColour, Sheet.ActiveCell.Style.BackgroundColour);
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
    /// <param name="fg">Foreground colour</param>
    /// <param name="bg">Background colour</param>
    private void ShowCell(CellLocation location, int fg, int bg) {
        Cell cell = Sheet.GetCell(location, false);
        DrawCell(cell, GetXPositionOfCell(location.Column), GetYPositionOfCell(location.Row), fg, bg);
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
    /// Carry out a block action on the marked range of cells.
    /// </summary>
    /// <param name="blockAction">Block action to perform</param>
    /// <returns>Render hint</returns>
    private RenderHint PerformBlockAction(BlockAction blockAction) {
        if (blockAction.HasFlag(BlockAction.COPY)) {
            IEnumerable<Cell> cells = RangeIterator().Select(location => Sheet.GetCell(location, false)).Where(cell => !cell.IsEmptyCell);
            Clipboard.Data = JsonSerializer.Serialize(cells);
        }
        if (blockAction.HasFlag(BlockAction.DELETE)) {
            foreach (CellLocation location in RangeIterator()) {
                Sheet.DeleteCell(Sheet.GetCell(location, false));
            }
        }
        ClearBlock();
        return RenderHint.CURSOR;
    }

    /// <summary>
    /// Paste cells from the clipboard to the current location.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint Paste() {
        RenderHint flags = RenderHint.NONE;
        List<Cell>? cellsToPaste = JsonSerializer.Deserialize<List<Cell>>(Clipboard.Data);
        if (cellsToPaste?.Count > 0) {
            CellLocation current = Sheet.Location;

            foreach (Cell cellToPaste in cellsToPaste) {
                Cell cell = Sheet.GetCell(current, true);
                cell.Format = cellToPaste.Format;
                cell.Align = cellToPaste.Align;
                cell.CellValue = cellToPaste.CellValue;
                cell.Decimal = cellToPaste.Decimal;
                cell.Content = cellToPaste.Content;
                cell.Style = cellToPaste.Style;
                ++current.Row;
            }
            Sheet.Modified = true;
            flags = RenderHint.RECALCULATE | RenderHint.CONTENTS;
        }
        return flags;
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
        if (Sheet.SetColumnWidth(Sheet.Location.Column, Sheet.DefaultColumnWidth)) {
            flags = RenderHint.REFRESH;
        }
        return flags;
    }

    /// <summary>
    /// Insert a column to the left of the cursor position
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint InsertColumn() {
        RenderHint flags = RenderHint.REFRESH;
        Sheet.InsertColumn(Sheet.Location.Column);
        if (Sheet.NeedRecalculate) {
            flags |= RenderHint.RECALCULATE;
        }
        return flags;
    }

    /// <summary>
    /// Insert a row above the cursor position
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint InsertRow() {
        RenderHint flags = RenderHint.REFRESH;
        Sheet.InsertRow(Sheet.Location.Row);
        if (Sheet.NeedRecalculate) {
            flags |= RenderHint.RECALCULATE;
        }
        return flags;
    }

    /// <summary>
    /// Delete a column at the cursor position
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint DeleteColumn() {
        RenderHint flags = RenderHint.REFRESH;
        Sheet.DeleteColumn(Sheet.Location.Column);
        if (Sheet.NeedRecalculate) {
            flags |= RenderHint.RECALCULATE;
        }
        return flags;
    }

    /// <summary>
    /// Delete a row at the cursor position
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint DeleteRow() {
        RenderHint flags = RenderHint.REFRESH;
        Sheet.DeleteRow(Sheet.Location.Row);
        if (Sheet.NeedRecalculate) {
            flags |= RenderHint.RECALCULATE;
        }
        return flags;
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
                Screen.Status.Message(string.Format(Calc.ErrorExportingFile, inputValue, e.Message));
            }
            flags = RenderHint.NONE;
        }
        return flags;
    }

    /// <summary>
    /// Prompt for a colour value for a range of cells and set
    /// those cell foreground colour.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint SetCellForegroundColour() {
        int cellColour = Sheet.ActiveCell.Style.ForegroundColour;
        if (!Screen.GetColourInput(Calc.EnterCellColour, ref cellColour)) {
            return RenderHint.NONE;
        }
        foreach (CellLocation location in RangeIterator()) {
            Cell cell = Sheet.GetCell(location, true);
            cell.Style.Foreground = cellColour;
        }
        Sheet.Modified = true;
        return RenderHint.CONTENTS;
    }

    /// <summary>
    /// Prompt for a colour value for a range of cells and set
    /// those cell background colour.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint SetCellBackgroundColour() {
        int cellColour = Sheet.ActiveCell.Style.BackgroundColour;
        if (!Screen.GetColourInput(Calc.EnterCellColour, ref cellColour)) {
            return RenderHint.NONE;
        }
        foreach (CellLocation location in RangeIterator()) {
            Cell cell = Sheet.GetCell(location, true);
            cell.Style.Background = cellColour;
        }
        Sheet.Modified = true;
        return RenderHint.CONTENTS;
    }

    /// <summary>
    /// Toggle bold style in the active cell.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint SetCellBold() {
        RenderHint flags = _isMarkMode ? RenderHint.CONTENTS : RenderHint.CURSOR;
        foreach (CellLocation location in RangeIterator()) {
            Cell cell = Sheet.GetCell(location, true);
            cell.Style.Bold = !cell.Style.Bold;
        }
        Sheet.Modified = true;
        ClearBlock();
        return flags;
    }

    /// <summary>
    /// Toggle italic style in the active cell.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint SetCellItalic() {
        RenderHint flags = _isMarkMode ? RenderHint.CONTENTS : RenderHint.CURSOR;
        foreach (CellLocation location in RangeIterator()) {
            Cell cell = Sheet.GetCell(location, true);
            cell.Style.Italic = !cell.Style.Italic;
        }
        Sheet.Modified = true;
        ClearBlock();
        return flags;
    }

    /// <summary>
    /// Toggle underline style in the active cell.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint SetCellUnderline() {
        RenderHint flags = _isMarkMode ? RenderHint.CONTENTS : RenderHint.CURSOR;
        foreach (CellLocation location in RangeIterator()) {
            Cell cell = Sheet.GetCell(location, true);
            cell.Style.Underline = !cell.Style.Underline;
        }
        Sheet.Modified = true;
        ClearBlock();
        return flags;
    }

    /// <summary>
    /// Sort a marked range of cells
    /// </summary>
    /// <returns>Render hint</returns>
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
        string cellValue = editValue ? cell.UIContent : string.Empty;
        CellInputResponse result = Screen.Command.PromptForCellInput(ref cellValue);
        if (result != CellInputResponse.CANCEL) {

            cell.UIContent = cellValue;
            Sheet.Modified = true;

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
        foreach (Cell cell in cells) {
            int x = GetXPositionOfCell(cell.Location.Column);
            int y = GetYPositionOfCell(cell.Location.Row);
            if (_sheetBounds.Contains(new Point(x, y))) {
                DrawCell(cell, x, y, cell.Style.ForegroundColour, cell.Style.BackgroundColour);
            }
        }
    }

    /// <summary>
    /// Draw this cell at the given cell position (1-based row and column) at
    /// the given physical screen offset where (0,0) is the top left corner.
    /// </summary>
    /// <param name="cell">Cell to draw</param>
    /// <param name="x">X position of cell</param>
    /// <param name="y">Y position of cell</param>
    /// <param name="fg">Foreground colour</param>
    /// <param name="bg">Background colour</param>
    private void DrawCell(Cell cell, int x, int y, int fg, int bg) {
        int width = Sheet.ColumnWidth(cell.Location.Column);
        if (x + width > _sheetBounds.Right) {
            width = _sheetBounds.Right - x;
            if (width <= 0) {
                return;
            }
        }
        string cellText = cell.ToString(width)[..width];
        Terminal.SetCursor(x, y);
        Terminal.Write(new AnsiText.AnsiTextSpan(cellText) {
            ForegroundColour = fg,
            BackgroundColour = bg,
            Bold = cell.Style.Bold,
            Italic = cell.Style.Italic,
            Underline = cell.Style.Underline
        }.EscapedString());
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
        if (sheetLocation.Column < Sheet.MaxColumns) {
            flags = SaveLastMarkPoint();
            ++sheetLocation.Column;
            Sheet.Location = sheetLocation;
            flags |= RenderHint.CURSOR;
            while (GetXPositionOfCell(sheetLocation.Column) + Sheet.ColumnWidth(sheetLocation.Column) >= _sheetBounds.Right) {
                ++_scrollOffset.X;
                flags |= RenderHint.REFRESH;
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
        if (sheetLocation.Row < Sheet.MaxRows) {
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
        sheetLocation.Row = Math.Min(sheetLocation.Row + _sheetBounds.Height, Sheet.MaxRows);
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
            if (location.Row is >= 1 and <= Sheet.MaxRows && location.Column is >= 1 and <= Sheet.MaxColumns) {
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