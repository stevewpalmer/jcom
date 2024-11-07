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

using System.Drawing;
using JComLib;

namespace JCalcLib;

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
    public int Row { get; set; }

    /// <summary>
    /// Cell column
    /// </summary>
    public int Column { get; set; }

    /// <summary>
    /// Return the cell row and column as a 1-based location
    /// </summary>
    public Point Location => new(Column, Row);

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
    /// <param name="value">Value</param>
    /// <returns>The date and time as a string</returns>
    private string ToDateTime(string pattern, double value) {
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
        ArgumentNullException.ThrowIfNull(position);
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
    /// Return the string value of the cell for display.
    /// </summary>
    /// <param name="width">Column width to use</param>
    /// <returns>String value of cell</returns>
    public string ToString(int width) {
        string cellValue = Value.StringValue;
        bool isNumber = double.TryParse(cellValue, out double doubleValue);
        if (isNumber) {
            int maxBar = Math.Min(width + 1, (int)Math.Abs(doubleValue));
            cellValue = Format switch {
                CellFormat.FIXED => doubleValue.ToString($"F{DecimalPlaces}"),
                CellFormat.PERCENT => $"{(doubleValue * 100).ToString($"F{DecimalPlaces}")}%",
                CellFormat.CURRENCY => doubleValue < 0 ?
                        $"(\u00a3{(-doubleValue).ToString($"N{DecimalPlaces}")})" :
                        $"\u00a3{doubleValue.ToString($"N{DecimalPlaces}")}",
                CellFormat.COMMAS => doubleValue < 0 ?
                        $"({(-doubleValue).ToString($"N{DecimalPlaces}")})" :
                        $"{doubleValue.ToString($"N{DecimalPlaces}")}",
                CellFormat.BAR => new string(doubleValue < 0 ? '-' : '+', maxBar),
                CellFormat.SCIENTIFIC => doubleValue.ToString($"0.{new string('#', DecimalPlaces)}E+00"),
                CellFormat.DATE_DM => ToDateTime("dd-MMM", doubleValue),
                CellFormat.DATE_MY => ToDateTime("MMM-yyyy", doubleValue),
                CellFormat.DATE_DMY => ToDateTime("dd-MMM-yyyy", doubleValue),
                CellFormat.GENERAL => cellValue,
                CellFormat.TEXT => cellValue,
                _ => throw new ArgumentException($"Unknown Cell Format: {Format}")
            };
        }
        if (cellValue.Length > width) {
            cellValue = new string('*', width);
        }
        else if (Format == CellFormat.BAR) {
            cellValue = cellValue.PadRight(width);
        }
        else {
            cellValue = Alignment switch {
                CellAlignment.LEFT => cellValue.PadRight(width),
                CellAlignment.RIGHT => cellValue.PadLeft(width),
                CellAlignment.CENTRE => Utilities.CentreString(cellValue, width),
                CellAlignment.GENERAL => Value.Type switch {
                    CellType.TEXT => cellValue.PadRight(width),
                    CellType.NUMBER => cellValue.PadLeft(width),
                    _ => "".PadRight(width)
                },
                _ => throw new ArgumentException($"Unknown Cell Alignment: {Alignment}")
            };
        }
        return cellValue;
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