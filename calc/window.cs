﻿// JCalc
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
    private bool _isMarkMode;
    private Point _lastMarkPoint;
    private Point _markAnchor;
    private Point _scrollOffset = Point.Empty;
    private Rectangle _sheetBounds;
    private Rectangle _viewportBounds;

    /// <summary>
    /// Width of the row label frame
    /// </summary>
    private const int RowLabelWidth = 5;

    /// <summary>
    /// Height of the column label frame
    /// </summary>
    private const int ColumnLabelHeight = 1;

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
    /// Extent of area invalidated.
    /// </summary>
    private RExtent InvalidateExtent { get; } = new();

    /// <summary>
    /// Set the viewport and sheet bounds for the window.
    /// </summary>
    /// <param name="x">Left edge, 0 based</param>
    /// <param name="y">Top edge, 0 based</param>
    /// <param name="width">Width of window</param>
    /// <param name="height">Height of window</param>
    public void SetViewportBounds(int x, int y, int width, int height) {
        _viewportBounds = new Rectangle(x, y, width, height);
        _sheetBounds = new Rectangle(x + RowLabelWidth, y + ColumnLabelHeight, width - RowLabelWidth, height - ColumnLabelHeight);
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
            SyncRowColumnToSheet();
            RenderFrame();
            flags |= RenderHint.CONTENTS;
        }
        if (flags.HasFlag(RenderHint.CONTENTS)) {
            InvalidateExtent.Clear();
            if (Sheet.NeedRecalculate) {
                Sheet.Calculate();
            }
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
            KeyCommand.KC_END => CursorEnd(),
            KeyCommand.KC_PAGEUP => CursorPageUp(),
            KeyCommand.KC_PAGEDOWN => CursorPageDown(),
            KeyCommand.KC_MARK => ToggleMarkMode(),
            KeyCommand.KC_GOTO => GotoRowColumn(),
            KeyCommand.KC_VALUE => InputValue(false),
            KeyCommand.KC_EDIT => InputValue(true),
            KeyCommand.KC_FILE_EXPORT => ExportFile(),
            KeyCommand.KC_SET_COLUMN_WIDTH => SetColumnWidth(),
            KeyCommand.KC_RESET_COLUMN_WIDTH => ResetColumnWidth(),
            KeyCommand.KC_AUTOSIZE_COLUMN_WIDTH => AutoSizeColumnWidth(),
            KeyCommand.KC_ALIGN_LEFT => AlignCells(CellAlignment.LEFT),
            KeyCommand.KC_ALIGN_RIGHT => AlignCells(CellAlignment.RIGHT),
            KeyCommand.KC_ALIGN_CENTRE => AlignCells(CellAlignment.CENTRE),
            KeyCommand.KC_ALIGN_GENERAL => AlignCells(CellAlignment.GENERAL),
            KeyCommand.KC_FORMAT_FIXED => FormatCells(CellFormat.FIXED),
            KeyCommand.KC_FORMAT_PERCENT => FormatCells(CellFormat.PERCENT),
            KeyCommand.KC_FORMAT_CURRENCY => FormatCells(CellFormat.CURRENCY),
            KeyCommand.KC_FORMAT_SCI => FormatCells(CellFormat.SCIENTIFIC),
            KeyCommand.KC_FORMAT_GENERAL => FormatCells(CellFormat.GENERAL),
            KeyCommand.KC_FORMAT_TEXT => FormatCells(CellFormat.TEXT),
            KeyCommand.KC_FORMAT_RESET => FormatCells(Screen.Config.DefaultCellFormat),
            KeyCommand.KC_DATE_DMY => FormatCells(CellFormat.DATE_DMY),
            KeyCommand.KC_DATE_DM => FormatCells(CellFormat.DATE_DM),
            KeyCommand.KC_DATE_MY => FormatCells(CellFormat.DATE_MY),
            KeyCommand.KC_TIME_HMSZ => FormatCells(CellFormat.TIME_HMSZ),
            KeyCommand.KC_TIME_HMS => FormatCells(CellFormat.TIME_HMS),
            KeyCommand.KC_TIME_HM => FormatCells(CellFormat.TIME_HM),
            KeyCommand.KC_TIME_HMZ => FormatCells(CellFormat.TIME_HMZ),
            KeyCommand.KC_INSERT_COLUMN => InsertColumn(),
            KeyCommand.KC_INSERT_ROW => InsertRow(),
            KeyCommand.KC_DELETE_COLUMN => DeleteColumn(),
            KeyCommand.KC_DELETE_ROW => DeleteRow(),
            KeyCommand.KC_DELETE => PerformBlockAction(BlockAction.DELETE),
            KeyCommand.KC_RANGE_EXPORT => ExportRange(),
            KeyCommand.KC_RANGE_SORT => SortRange(),
            KeyCommand.KC_DATAFILL => FillRange(),
            KeyCommand.KC_STYLE_FG => SetCellTextColour(),
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
        if (Sheet.NeedRecalculate) {
            IEnumerable<Cell> cellsToUpdate = Sheet.Calculate();
            InvalidateCells(cellsToUpdate);
            flags |= RenderHint.CONTENTS;
        }
        if (flags.HasFlag(RenderHint.BLOCK)) {
            if (_isMarkMode) {
                InvalidateExtent
                    .Clear()
                    .Add(_markAnchor)
                    .Add(Sheet.Location.Point)
                    .Add(_lastMarkPoint);
                flags |= RenderHint.CONTENTS;
            }
            flags &= ~RenderHint.BLOCK;
        }
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
        int fg = Screen.Colours.BackgroundColour;
        int bg = Screen.Colours.SelectionColour;
        int x = _sheetBounds.X;
        int y = _viewportBounds.Top;

        // Corner
        Terminal.Write(_viewportBounds.Left, y, RowLabelWidth, fg, bg, Utilities.EmptyString(RowLabelWidth));

        // Column numbers
        int columnNumber = 1 + _scrollOffset.X;
        while (x < _viewportBounds.Right) {
            int width;
            string label;
            if (columnNumber <= Sheet.MaxColumns) {
                width = Sheet.ColumnWidth(columnNumber);
                label = Utilities.CentreString(Cell.ColumnToAddress(columnNumber), width);
            }
            else {
                width = _viewportBounds.Width - x;
                label = Utilities.EmptyString(width);
            }
            int space = Math.Min(width, _viewportBounds.Width - x);
            Terminal.Write(x, y, width, fg, bg, label[..space]);
            x += width;
            columnNumber++;
        }

        // Row numbers
        int rowNumber = 1 + _scrollOffset.Y;
        y = _viewportBounds.Top + ColumnLabelHeight;
        while (y < _viewportBounds.Bottom && rowNumber <= Sheet.MaxRows) {
            Terminal.Write(_viewportBounds.Left, y, RowLabelWidth, fg, bg, $" {rowNumber.ToString(),-3} ");
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
        RExtent renderExtent = new RExtent()
            .Add(new Point(1, _scrollOffset.Y + 1))
            .Add(new Point(_sheetBounds.Width, _scrollOffset.Y + _sheetBounds.Height));
        if (InvalidateExtent.Valid) {
            renderExtent.Subtract(InvalidateExtent.Start, InvalidateExtent.End);
        }

        RExtent markExtent = new();
        if (_isMarkMode) {
            markExtent
                .Add(_markAnchor)
                .Add(Sheet.Location.Point);
        }

        int i = renderExtent.Start.Y;
        int y = GetYPositionOfCell(i);
        while (i <= renderExtent.End.Y) {
            AnsiText line = Sheet.GetRow(_scrollOffset.X + 1, i, _sheetBounds.Width);
            int x = _sheetBounds.Left;
            Debug.Assert(line.Length == _sheetBounds.Width);

            if (i >= markExtent.Start.Y && i <= markExtent.End.Y) {
                int extentStart = GetXPositionOfCell(markExtent.Start.X);
                int extentEnd = GetXPositionOfCell(markExtent.End.X + 1);
                int left = 0;
                if (extentStart > x) {
                    left = extentStart - x;
                }
                if (extentStart < _scrollOffset.X) {
                    extentStart = _sheetBounds.Left;
                }
                if (extentEnd > _scrollOffset.X) {
                    int extentWidth = extentEnd - extentStart;
                    line.Style(left, extentWidth, Screen.Colours.BackgroundColour, Screen.Colours.SelectionColour);
                }
            }
            Terminal.Write(x, y, _sheetBounds.Width, line);
            ++i;
            ++y;
        }
        InvalidateExtent.Clear();
        PlaceCursor();
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
        InvalidateExtent.Add(GetMarkExtent());
        _isMarkMode = false;
    }

    /// <summary>
    /// Draw the cursor
    /// </summary>
    private void PlaceCursor() {
        UpdateActiveCell(Screen.Colours.BackgroundColour, Screen.Colours.SelectionColour);
    }

    /// <summary>
    /// Clear the cursor.
    /// </summary>
    private void ResetCursor() {
        UpdateActiveCell(Sheet.ActiveCell.Style.TextColour, Sheet.ActiveCell.Style.BackgroundColour);
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
    /// Return the X position of the specified column
    /// </summary>
    /// <param name="column">1-based column index</param>
    /// <returns>X position of column</returns>
    private int GetXPositionOfCell(int column) {
        if (column - 1 < _scrollOffset.X) {
            return -1;
        }
        int x = _sheetBounds.Left;
        for (int c = _scrollOffset.X; c < column - 1; c++) {
            if (x >= _sheetBounds.Right) {
                return -1;
            }
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
        if (row - 1 < _scrollOffset.Y) {
            return -1;
        }
        int y = _sheetBounds.Top;
        for (int d = _scrollOffset.Y; d < row - 1; d++) {
            if (y >= _sheetBounds.Bottom) {
                return -1;
            }
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
            Cell cell = Sheet.GetCell(location, true);
            cell.Alignment = alignment;
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
            Clipboard.Data = RangeIterator().Select(location => new Cell(Sheet, Sheet.GetCell(location, false))).ToArray();
        }
        if (blockAction.HasFlag(BlockAction.DELETE)) {
            foreach (CellLocation location in RangeIterator()) {
                Sheet.DeleteCell(Sheet.GetCell(location, false));
            }
        }
        ClearBlock();
        return RenderHint.CONTENTS;
    }

    /// <summary>
    /// Paste cells from the clipboard to the current location while
    /// respecting the geometry of the original cells.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint Paste() {
        Cell[] cellsToPaste = Clipboard.Data;
        List<Cell> cellsToUpdate = [];
        if (cellsToPaste.Length > 0) {
            Point start = GetMarkExtent().Start;
            CellLocation current = new() { Column = start.X, Row = start.Y };

            Point startPoint = cellsToPaste[0].Location.Point;
            foreach (Cell cellToPaste in cellsToPaste) {
                Point cellPoint = cellToPaste.Location.Point;
                Cell cell = Sheet.GetCell(new CellLocation {
                    Column = current.Column + (cellPoint.X - startPoint.X),
                    Row = current.Row + (cellPoint.Y - startPoint.Y)
                }, true);
                cell.CopyFrom(cellToPaste);
                cellsToUpdate.Add(cell);
            }
            InvalidateCells(cellsToUpdate);
        }
        ClearBlock();
        return RenderHint.NONE;
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
        bool useThousands = false;
        if (format is CellFormat.FIXED) {
            FormField[] formFields = [
                new() {
                    Text = Calc.EnterDecimalPlaces,
                    Type = FormFieldType.NUMBER,
                    Width = 2,
                    MinimumValue = 0,
                    MaximumValue = Sheet.MaxDecimalPlaces,
                    Value = new Variant(cell.DecimalPlaces)
                },
                new() {
                    Text = Calc.UseThousandsSeparator,
                    Type = FormFieldType.BOOLEAN,
                    Value = new Variant(cell.UseThousandsSeparator)
                }
            ];
            if (!Screen.Command.PromptForInput(formFields)) {
                return RenderHint.CANCEL;
            }
            decimalPlaces = formFields[0].Value.IntValue;
            useThousands = formFields[1].Value.BoolValue;
            Debug.Assert(decimalPlaces >= formFields[0].MinimumValue && decimalPlaces <= formFields[0].MaximumValue);
        }
        if (format is CellFormat.SCIENTIFIC or CellFormat.CURRENCY or CellFormat.PERCENT) {
            FormField[] formFields = [
                new() {
                    Text = Calc.EnterDecimalPlaces,
                    Type = FormFieldType.NUMBER,
                    Width = 2,
                    MinimumValue = 0,
                    MaximumValue = Sheet.MaxDecimalPlaces,
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
            Cell cell2 = Sheet.GetCell(location, true);
            cell2.CellFormat = format;
            cell2.DecimalPlaces = decimalPlaces;
            cell2.UseThousandsSeparator = useThousands;
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
                MinimumValue = Sheet.MinColumnWidth,
                MaximumValue = Sheet.MaxColumnWidth,
                Value = new Variant(Sheet.ColumnWidth(Sheet.Location.Column))
            }
        ];
        if (Screen.Command.PromptForInput(formFields)) {
            int newWidth = formFields[0].Value.IntValue;
            Debug.Assert(newWidth >= formFields[0].MinimumValue && newWidth <= formFields[0].MaximumValue);
            flags = RenderHint.NONE;

            foreach (CellLocation location in RangeIterator()) {
                if (Sheet.SetColumnWidth(location.Column, newWidth)) {
                    flags = RenderHint.REFRESH;
                }
            }
        }
        ClearBlock();
        return flags;
    }

    /// <summary>
    /// Reset the current column width to the global default.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint ResetColumnWidth() {
        RenderHint flags = RenderHint.NONE;
        foreach (CellLocation location in RangeIterator()) {
            if (Sheet.SetColumnWidth(location.Column, Sheet.DefaultColumnWidth)) {
                flags = RenderHint.REFRESH;
            }
        }
        ClearBlock();
        return flags;
    }

    /// <summary>
    /// Automatically size the selected column widths.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint AutoSizeColumnWidth() {
        RenderHint flags = RenderHint.NONE;
        foreach (CellLocation location in RangeIterator()) {
            if (Sheet.SetColumnWidth(location.Column, 0)) {
                flags = RenderHint.REFRESH;
            }
        }
        ClearBlock();
        return flags;
    }

    /// <summary>
    /// Insert a column to the left of the cursor position
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint InsertColumn() {
        Sheet.InsertColumn(Sheet.Location.Column);
        return RenderHint.REFRESH;
    }

    /// <summary>
    /// Insert a row above the cursor position
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint InsertRow() {
        Sheet.InsertRow(Sheet.Location.Row);
        return RenderHint.REFRESH;
    }

    /// <summary>
    /// Delete a column at the cursor position
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint DeleteColumn() {
        Sheet.DeleteColumn(Sheet.Location.Column);
        return RenderHint.REFRESH;
    }

    /// <summary>
    /// Delete a row at the cursor position
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint DeleteRow() {
        Sheet.DeleteRow(Sheet.Location.Row);
        return RenderHint.REFRESH;
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
        RenderHint flags = ExportExtent(GetMarkExtent());
        if (flags != RenderHint.CANCEL) {
            ClearBlock();
            flags = RenderHint.REFRESH;
        }
        return flags;
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
                Width = 0,
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
                using CsvWriter csvWriter = new(textStream, CultureInfo.InvariantCulture);

                if (extent.Valid) {
                    long fileSize = extent.End.Y - extent.Start.Y;
                    int lastPercent = -1;
                    for (int row = extent.Start.Y; row <= extent.End.Y; row++) {
                        for (int column = extent.Start.X; column <= extent.End.X; column++) {
                            Cell cell = Sheet.GetCell(column, row, false);
                            csvWriter.WriteField(cell.Value);
                        }
                        int percent = Convert.ToInt32((row - extent.Start.Y) / (double)fileSize * 100.0d);
                        if (percent != lastPercent) {
                            if (percent % 5 == 0) {
                                Screen.Status.Message(string.Format(Calc.ExportProgress, percent));
                            }
                            lastPercent = percent;
                        }
                        csvWriter.NextRecord();
                    }
                }
                csvWriter.Flush();
            }
            catch (Exception e) {
                Screen.Status.Message(string.Format(Calc.ErrorExportingFile, inputValue, e.Message));
                return RenderHint.NONE;
            }
            Screen.Status.ClearMessage();
            flags = RenderHint.NONE;
        }
        return flags;
    }

    /// <summary>
    /// Prompt for a colour value for a range of cells and set
    /// those cell text colour.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint SetCellTextColour() {
        int cellColour = Sheet.ActiveCell.Style.TextColour;
        if (!Screen.GetColourInput(Calc.SelectCellTextColour, ref cellColour)) {
            return RenderHint.NONE;
        }
        foreach (CellLocation location in RangeIterator()) {
            Cell cell = Sheet.GetCell(location, true);
            cell.Style.TextColour = cellColour;
        }
        return RenderHint.CONTENTS;
    }

    /// <summary>
    /// Prompt for a colour value for a range of cells and set
    /// those cell background colour.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint SetCellBackgroundColour() {
        int cellColour = Sheet.ActiveCell.Style.BackgroundColour;
        if (!Screen.GetColourInput(Calc.SelectCellBackgroundColour, ref cellColour)) {
            return RenderHint.NONE;
        }
        foreach (CellLocation location in RangeIterator()) {
            Cell cell = Sheet.GetCell(location, true);
            cell.Style.BackgroundColour = cellColour;
        }
        return RenderHint.CONTENTS;
    }

    /// <summary>
    /// Toggle bold style in the active cell.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint SetCellBold() {
        foreach (CellLocation location in RangeIterator()) {
            Cell cell = Sheet.GetCell(location, true);
            cell.Style.IsBold = !cell.Style.IsBold;
            InvalidateCells([cell]);
        }
        ClearBlock();
        InvalidateRow();
        return RenderHint.CONTENTS;
    }

    /// <summary>
    /// Toggle italic style in the active cell.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint SetCellItalic() {
        foreach (CellLocation location in RangeIterator()) {
            Cell cell = Sheet.GetCell(location, true);
            cell.Style.IsItalic = !cell.Style.IsItalic;
            InvalidateCells([cell]);
        }
        ClearBlock();
        InvalidateRow();
        return RenderHint.CONTENTS;
    }

    /// <summary>
    /// Toggle underline style in the active cell.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint SetCellUnderline() {
        foreach (CellLocation location in RangeIterator()) {
            Cell cell = Sheet.GetCell(location, true);
            cell.Style.IsUnderlined = !cell.Style.IsUnderlined;
            InvalidateCells([cell]);
        }
        ClearBlock();
        InvalidateRow();
        return RenderHint.CONTENTS;
    }

    /// <summary>
    /// Fill the selected cells. The algorithm applied depends on the number of
    /// cells at the start of the range that contain values. If only one cell
    /// has a value, that value is replicated in the range. If two cells have
    /// values then the difference is applied to subsequent cells. However, if
    /// the two cells are dates then it uses the formatting on the cells to
    /// determine the date distribution.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint FillRange() {
        CellFiller filler = new(Sheet, RangeIterator());
        filler.Process();
        ClearBlock();
        return RenderHint.CONTENTS;
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
            flags = RenderHint.REFRESH;
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
        string cellValue = editValue ? Sheet.ActiveCell.Content : string.Empty;
        CellInputResponse result = Screen.Command.PromptForCellInput(ref cellValue);
        if (result != CellInputResponse.CANCEL) {
            try {
                Cell cell = Sheet.GetCell(Sheet.Location, true);
                cell.Content = cellValue;
                InvalidateCells([cell]);

                hint = result switch {
                    CellInputResponse.ACCEPT => RenderHint.CURSOR,
                    CellInputResponse.ACCEPT_UP => CursorUp(),
                    CellInputResponse.ACCEPT_DOWN => CursorDown(),
                    CellInputResponse.ACCEPT_LEFT => CursorLeft(),
                    CellInputResponse.ACCEPT_RIGHT => CursorRight(),
                    _ => hint
                };
            }
            catch (FormatException e) {
                Screen.Status.Message(e.Message);
            }
        }
        return hint;
    }

    /// <summary>
    /// Invalidate the current row
    /// </summary>
    private void InvalidateRow() {
        InvalidateExtent
            .Add(new Point(1, Sheet.Location.Row))
            .Add(new Point(Sheet.MaxColumns, Sheet.Location.Row));
    }

    /// <summary>
    /// Render a collection of cells
    /// </summary>
    /// <param name="cells">List of cells to update</param>
    private void InvalidateCells(IEnumerable<Cell> cells) {
        foreach (Cell cell in cells) {
            InvalidateExtent.Add(cell.Location.Point);
        }
    }

    /// <summary>
    /// Update the current cell.
    /// </summary>
    /// <param name="fg">Foreground colour</param>
    /// <param name="bg">Background colour</param>
    private void UpdateActiveCell(int fg, int bg) {
        Cell cell = Sheet.ActiveCell;
        int x = GetXPositionOfCell(cell.Location.Column);
        int y = GetYPositionOfCell(cell.Location.Row);
        if (_sheetBounds.Contains(x, y)) {
            int width = Sheet.ColumnWidth(cell.Location.Column);
            if (x + width > _sheetBounds.Right) {
                width = _sheetBounds.Right - x;
                if (width <= 0) {
                    return;
                }
            }
            AnsiTextSpan cellText = cell.AnsiTextForWidth(width, true);
            Terminal.SetCursor(x, y);
            Terminal.Write(new AnsiTextSpan(cellText) {
                ForegroundColour = fg,
                BackgroundColour = bg
            }.EscapedText);
        }
    }

    /// <summary>
    /// Toggle whether we are in mark mode when moving the
    /// cursor over the screen.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint ToggleMarkMode() {
        RenderHint flags;
        if (_isMarkMode) {
            ClearBlock();
            flags = RenderHint.CONTENTS;
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
    /// Save the last mark point if we are currently marking a block so
    /// we know the extent of the area to be invalidated when we update
    /// the window.
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
    /// <returns>Render hint</returns>
    private RenderHint CursorHome() {
        RenderHint flags = SaveLastMarkPoint();
        Sheet.Location = new CellLocation(1, 1);
        return flags | SyncRowColumnToSheet();
    }

    /// <summary>
    /// Move the cursor to the home position.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint CursorEnd() {
        RenderHint flags = SaveLastMarkPoint();
        RExtent sheetExtent = Sheet.GetCellExtent();
        Sheet.Location = new CellLocation(sheetExtent.End.X, sheetExtent.End.Y);
        return flags | SyncRowColumnToSheet();
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
        sheetLocation.Row = Math.Max(sheetLocation.Row - _sheetBounds.Height, 1);
        Sheet.Location = sheetLocation;
        return flags | SyncRowColumnToSheet(true);
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
        sheetLocation.Row = Math.Min(sheetLocation.Row + _sheetBounds.Height, Sheet.MaxRows);
        Sheet.Location = sheetLocation;
        return flags | SyncRowColumnToSheet(true);
    }

    /// <summary>
    /// Compute the number of full columns visible in this sheet.
    /// </summary>
    /// <returns>Number of fully visible columns</returns>
    private int TotalColumnsOnSheet() {
        int x = _sheetBounds.X;
        int columnNumber = 1 + _scrollOffset.X;
        int numberOfColumns = 0;
        while (x < _viewportBounds.Right && columnNumber <= Sheet.MaxColumns) {
            int width = Sheet.ColumnWidth(columnNumber);
            int space = Math.Min(width, _viewportBounds.Width - x);
            if (space == width) {
                ++numberOfColumns;
            }
            x += width;
            columnNumber++;
        }
        return numberOfColumns;
    }

    /// <summary>
    /// Sync the Sheet.Row and Sheet.Column to ensure they are visible on the window by
    /// adjusting the scroll offsets as appropriate.
    /// </summary>
    /// <returns>Render hint</returns>
    private RenderHint SyncRowColumnToSheet(bool tryCenter = false) {
        RenderHint flags = RenderHint.CURSOR;
        int numberOfColumns = TotalColumnsOnSheet();

        if (Sheet.Location.Column <= _scrollOffset.X) {
            int centreColumn = Sheet.Location.Column - _scrollOffset.X + numberOfColumns / 2;
            _scrollOffset.X = tryCenter ? Math.Max(0, centreColumn) : Sheet.Location.Column - 1;
            flags |= RenderHint.REFRESH;
        }
        if (Sheet.Location.Column > numberOfColumns + _scrollOffset.X) {
            int centreColumn = Sheet.Location.Column - _scrollOffset.X + numberOfColumns / 2;
            _scrollOffset.X = tryCenter ? centreColumn : Sheet.Location.Column - numberOfColumns;
            flags |= RenderHint.REFRESH;
        }
        if (Sheet.Location.Row <= _scrollOffset.Y) {
            int centreRow = _sheetBounds.Height / 2 - (Sheet.Location.Row - _scrollOffset.Y);
            _scrollOffset.Y = tryCenter ? Math.Max(0, _scrollOffset.Y - centreRow) : Sheet.Location.Row - 1;
            flags |= RenderHint.REFRESH;
        }
        if (Sheet.Location.Row > _sheetBounds.Height + _scrollOffset.Y) {
            int centreRow = _sheetBounds.Height / 2 - (Sheet.Location.Row - _scrollOffset.Y);
            _scrollOffset.Y = tryCenter ? Math.Max(0, _scrollOffset.Y - centreRow) :Sheet.Location.Row - _sheetBounds.Height;
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
        int inputWidth = new CellLocation(Sheet.MaxColumns, Sheet.MaxRows).Address.Length;
        FormField[] formFields = [
            new() {
                Text = Calc.GotoRowPrompt,
                Type = FormFieldType.TEXT,
                Width = inputWidth,
                Value = new Variant(Sheet.ActiveCell.Address)
            }
        ];
        if (Screen.Command.PromptForInput(formFields)) {
            string newAddress = formFields[0].Value.StringValue;
            try {
                CellLocation location = new(newAddress);
                ResetCursor();
                Sheet.Location = location;
                flags = SyncRowColumnToSheet();
            }
            catch (FormatException) {
                Screen.Status.Message(Calc.InvalidAddress);
            }
        }
        return flags;
    }
}