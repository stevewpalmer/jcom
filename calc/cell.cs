// JCalc
// A single spreadsheet cell
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

using JComLib;

namespace JCalc;

public class Cell {

    /// <summary>
    /// Cell value
    /// </summary>
    public CellValue Value { get; set; } = new();

    /// <summary>
    /// Cell alignment
    /// </summary>
    public CellAlignment Alignment { get; set; }

    /// <summary>
    /// Cell row
    /// </summary>
    public int Row { get; init; }

    /// <summary>
    /// Cell column
    /// </summary>
    public int Column { get; init; }

    /// <summary>
    /// Return the current cell location as a string.
    /// </summary>
    public string Position => $"{ColumnNumber(Column)}{Row}";

    /// <summary>
    /// Cell format
    /// </summary>
    public CellFormat Format { get; set; }

    /// <summary>
    /// Number of decimal places
    /// </summary>
    public int DecimalPlaces { get; set; }

    /// <summary>
    /// Try and convert a value to a date and time string.
    /// </summary>
    /// <param name="pattern">The date/time pattern to use</param>
    /// <returns>The date and time as a string</returns>
    private string ToDateTime(string pattern) {
        if (!double.TryParse(Value.StringValue, out double value)) {
            return Value.StringValue;
        }
        if (value < -657435.0) {
            return Value.StringValue;
        }
        if (value > 2958465.99999999) {
            return Value.StringValue;
        }
        DateTime dateTime = DateTime.FromOADate(value);
        return dateTime.ToString(pattern);
    }

    /// <summary>
    /// Parse a position and return the column and row that
    /// corresponds to that position, or (0,0) if the position
    /// cannot be parsed.
    /// </summary>
    /// <param name="position">Position string</param>
    /// <returns>Tuple containing column and row</returns>
    public static (int, int) ColumnAndRowFromPosition(string position) {
        int newColumn = 0;
        int newRow = 0;
        int index = 0;
        while (index < position.Length && char.IsLetter(position[index])) {
            newColumn = newColumn * 26 + char.ToUpper(position[index]) - 'A' + 1;
            index++;
        }
        while (index < position.Length && char.IsDigit(position[index])) {
            newRow = newRow * 10 + position[index] - '0';
            index++;
        }
        return (newColumn, newRow);
    }

    /// <summary>
    /// Draw this cell at the given cell position (1-based row and column) at
    /// the given physical screen offset where (0,0) is the top left corner.
    /// </summary>
    /// <param name="sheet">Sheet to which cell belongs</param>
    /// <param name="x">X position of cell</param>
    /// <param name="y">Y position of cell</param>
    public void Draw(Sheet sheet, int x, int y) {
        Terminal.SetCursor(x, y);
        int width = sheet.ColumnWidth(Column);
        string cellValue = Value.StringValue;
        bool isNumber = double.TryParse(cellValue, out double doubleValue);
        if (isNumber) {
            int maxBar = Math.Min(width + 1, (int)Math.Abs(doubleValue));
            cellValue = Format switch {
                CellFormat.FIXED => doubleValue.ToString($"F{DecimalPlaces}"),
                CellFormat.PERCENT => $"{(doubleValue * 100).ToString($"F{DecimalPlaces}")}%",
                CellFormat.CURRENCY => $"\u00a3{doubleValue.ToString($"N{DecimalPlaces}")}",
                CellFormat.COMMAS => doubleValue < 0 ? $"({(-doubleValue).ToString($"N{DecimalPlaces}")})" : $"{doubleValue.ToString($"N{DecimalPlaces}")}",
                CellFormat.BAR => new string(doubleValue < 0 ? '-' : '+', maxBar),
                CellFormat.SCIENTIFIC => doubleValue.ToString("E" + DecimalPlaces),
                CellFormat.DATE_DM => ToDateTime("dd-MMM"),
                CellFormat.DATE_MY => ToDateTime("MMM-yyyy"),
                CellFormat.DATE_DMY => ToDateTime("dd-MMM-yyyy"),
                _ => cellValue
            };
        }
        if (cellValue.Length > width) {
            cellValue = new string('*', width);
        }
        cellValue = Alignment switch {
            CellAlignment.LEFT => cellValue.PadRight(width),
            CellAlignment.RIGHT => cellValue.PadLeft(width),
            CellAlignment.CENTRE => Utilities.CentreString(cellValue, width),
            CellAlignment.GENERAL => Value.Type switch {
                CellType.TEXT => cellValue.PadRight(width),
                CellType.NUMBER => cellValue.PadLeft(width),
                _ => "".PadRight(width)
            },
            _ => cellValue
        };
        Terminal.Write(cellValue);
    }

    /// <summary>
    /// Convert a column offset to its location.
    /// </summary>
    /// <param name="column">Column offset, 1-based</param>
    /// <returns>Column location</returns>
    public static string ColumnNumber(int column) {
        string columnNumber = "";
        while (--column >= 0) {
            columnNumber = (char)(column % 26 + 'A') + columnNumber;
            column /= 26;
        }
        return columnNumber;
    }
}