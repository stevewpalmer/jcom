// JCalcLib
// A single worksheet cell
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
using System.Text.Json.Serialization;
using JComLib;

namespace JCalcLib;

public class Cell {

    /// <summary>
    /// Cell value
    /// </summary>
    public CellValue CellValue { get; set; } = new();

    /// <summary>
    /// Cell alignment
    /// </summary>
    public CellAlignment Alignment { get; set; }

    /// <summary>
    /// Cell location
    /// </summary>
    public CellLocation Location { get; set; } = new();

    /// <summary>
    /// Return the current cell location as a string that
    /// represents the cell address on the screen.
    /// </summary>
    [JsonIgnore]
    public string Address => $"{ColumnToAddress(Location.Column)}{Location.Row}";

    /// <summary>
    /// Return the format description of the current cell.
    /// </summary>
    [JsonIgnore]
    public string FormatDescription {
        get {
            string text = Format switch {
                CellFormat.BAR => "B",
                CellFormat.FIXED => $"F{DecimalPlaces}",
                CellFormat.SCIENTIFIC => $"S{DecimalPlaces}",
                CellFormat.TEXT => "T",
                CellFormat.GENERAL => "G",
                CellFormat.PERCENT => $"P{DecimalPlaces}",
                CellFormat.COMMAS => $",{DecimalPlaces}",
                CellFormat.CURRENCY => $"C{DecimalPlaces}",
                CellFormat.DATE_DM => "D2",
                CellFormat.DATE_MY => "D3",
                CellFormat.DATE_DMY => "D1",
                _ => "?"
            };
            return $"({text})";
        }
    }

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
        ArgumentNullException.ThrowIfNull(pattern);
        if (value < -657435.0) {
            return CellValue.Value;
        }
        if (value > 2958465.99999999) {
            return CellValue.Value;
        }
        DateTime dateTime = DateTime.FromOADate(value);
        return dateTime.ToString(pattern);
    }

    /// <summary>
    /// Parse a cell address and return the cell location that
    /// corresponds to that address, or (0,0) if the address
    /// cannot be parsed.
    /// </summary>
    /// <param name="address">Address string</param>
    /// <returns>CellLocation contain the cell column and row</returns>
    public static CellLocation LocationFromAddress(string address) {
        ArgumentNullException.ThrowIfNull(address);
        int newColumn = 0;
        int newRow = 0;
        int index = 0;
        while (index < address.Length && char.IsLetter(address[index])) {
            newColumn = newColumn * 26 + char.ToUpper(address[index]) - 'A' + 1;
            index++;
        }
        while (index < address.Length && char.IsDigit(address[index])) {
            newRow = newRow * 10 + address[index] - '0';
            index++;
        }
        return new CellLocation { Column = newColumn, Row = newRow };
    }

    /// <summary>
    /// Convert a column offset to its address (A, B, etc).
    /// </summary>
    /// <param name="column">Column offset, 1-based</param>
    /// <returns>Column address</returns>
    public static string ColumnToAddress(int column) {
        ArgumentOutOfRangeException.ThrowIfLessThan(column, 1);
        string columnNumber = "";
        while (--column >= 0) {
            columnNumber = (char)(column % 26 + 'A') + columnNumber;
            column /= 26;
        }
        return columnNumber;
    }

    /// <summary>
    /// Convert a column address to its column offset.
    /// </summary>
    /// <param name="address">Column address</param>
    /// <returns>Column offset</returns>
    public static int AddressToColumn(string address) {
        ArgumentNullException.ThrowIfNull(address);
        int newColumn = 0;
        int index = 0;
        while (index < address.Length && char.IsLetter(address[index])) {
            newColumn = newColumn * 26 + char.ToUpper(address[index]) - 'A' + 1;
            index++;
        }
        return newColumn;
    }

    /// <summary>
    /// Return the string value of the cell for display.
    /// </summary>
    /// <param name="width">Column width to use</param>
    /// <returns>String value of cell</returns>
    public string ToString(int width) {
        Debug.Assert(width >= 0);
        string cellValue = CellValue.Value;
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
                CellFormat.DATE_DM => ToDateTime("d-MMM", doubleValue),
                CellFormat.DATE_MY => ToDateTime("MMM-yyyy", doubleValue),
                CellFormat.DATE_DMY => ToDateTime("d-MMM-yyyy", doubleValue),
                CellFormat.GENERAL => cellValue,
                CellFormat.TEXT => CellValue.ToText(),
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
                CellAlignment.GENERAL => !isNumber ? cellValue.PadRight(width) : cellValue.PadLeft(width),
                _ => throw new ArgumentException($"Unknown Cell Alignment: {Alignment}")
            };
        }
        return cellValue;
    }

    /// <summary>
    /// Exchange the contents of this cell with another cell.
    /// </summary>
    /// <param name="other">Cell to swap</param>
    public void Swap(Cell other) {
        (CellValue, other.CellValue) = (other.CellValue, CellValue);
        (Format, other.Format) = (other.Format, Format);
        (Alignment, other.Alignment) = (other.Alignment, Alignment);
        (DecimalPlaces, other.DecimalPlaces) = (other.DecimalPlaces, DecimalPlaces);
    }
}