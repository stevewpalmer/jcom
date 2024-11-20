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
using System.Drawing;
using System.Globalization;
using System.Text.Json.Serialization;
using JComLib;

namespace JCalcLib;

public class Cell(Sheet? sheet) {
    private CellParseNode? _cellParseNode;
    private string _content = string.Empty;
    private Sheet? _sheet = sheet;

    /// <summary>
    /// Empty constructor
    /// </summary>
    public Cell() : this(null) { }

    /// <summary>
    /// Cell value
    /// </summary>
    [JsonIgnore]
    public CellValue CellValue { get; set; } = new();

    /// <summary>
    /// Cell alignment
    /// </summary>
    public CellAlignment? Align { get; set; }

    /// <summary>
    /// Cell alignment
    /// </summary>
    [JsonIgnore]
    public CellAlignment Alignment => Align.GetValueOrDefault(CellFactory.Alignment);

    /// <summary>
    /// Cell text style
    /// </summary>
    public CellStyle Style { get; set; } = new();

    /// <summary>
    /// Cell location
    /// </summary>
    public CellLocation Location { get; set; } = new();

    /// <summary>
    /// Return the current cell location as a string that
    /// represents the cell address on the screen.
    /// </summary>
    [JsonIgnore]
    public string Address => LocationToAddress(Location);

    /// <summary>
    /// Return the format description of the current cell.
    /// </summary>
    [JsonIgnore]
    public string FormatDescription {
        get {
            string text = CellFormat switch {
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
    public CellFormat? Format { get; set; }

    /// <summary>
    /// Cell format
    /// </summary>
    [JsonIgnore]
    public CellFormat CellFormat => Format.GetValueOrDefault(CellFactory.Format);

    /// <summary>
    /// Number of decimal places
    /// </summary>
    public int? Decimal { get; set; }

    /// <summary>
    /// Number of decimal places
    /// </summary>
    [JsonIgnore]
    public int DecimalPlaces => Decimal.GetValueOrDefault(CellFactory.DecimalPlaces);

    /// <summary>
    /// Is this a blank cell?
    /// </summary>
    [JsonIgnore]
    public bool IsEmptyCell => CellValue.Type == CellType.NONE;

    /// <summary>
    /// Returns the cell parse node that represents this cell value. If
    /// the cell is a simple literal text or number, it returns a node
    /// representing that number. If it is a formula, it parses the formula
    /// and returns the root parse node of the expression tree generated
    /// from the formula.
    /// </summary>
    [JsonIgnore]
    public CellParseNode ParseNode {
        get {
            if (_cellParseNode == null) {
                if (_cellParseNode == null) {
                    switch (CellValue.Type) {
                        case CellType.TEXT:
                            _cellParseNode = new TextParseNode(_content);
                            break;
                        case CellType.NUMBER:
                            _cellParseNode = new NumberParseNode(double.Parse(_content));
                            break;
                        case CellType.FORMULA: {
                            TryParseFormula(_content, Location, out _cellParseNode);
                            break;
                        }
                    }
                }
            }
            Debug.Assert(_cellParseNode != null);
            return _cellParseNode;
        }
    }

    /// <summary>
    /// Contents of the cell as set from a data file.
    /// </summary>
    [JsonInclude]
    public string Content {
        get => CellValue.Type == CellType.FORMULA ? $"={ParseNode.ToRawString()}" : CellValue.Value;
        set {
            _cellParseNode = null;
            _content = value;
            if (TryParseFormula(value, Location, out CellParseNode? _)) {
                CellValue.Value = "0";
                CellValue.Type = CellType.FORMULA;
            }
            else {
                CellValue.Value = _content;
                CellValue.Type = double.TryParse(_content, out double _) ? CellType.NUMBER : CellType.TEXT;
            }
        }
    }

    /// <summary>
    /// Retrieve or set the cell's value. Retrieving the value always returns
    /// the calculated value. The behaviour on setting the value depends on
    /// whether the new value is a formula or a constant. If it is a formula
    /// then it assigns the formula to the cell and evaluates it. If it is a
    /// constant then it becomes the new value of the cell.
    /// </summary>
    [JsonIgnore]
    public string Value {
        get => CellValue.Value;
        set => UIContent = value;
    }

    /// <summary>
    /// The UI view of the content.
    /// </summary>
    [JsonIgnore]
    public string UIContent {
        get => CellValue.Type == CellType.FORMULA ? $"={ParseNode}" : CellValue.Value;
        set {
            _cellParseNode = null;
            if (TryParseFormula(value, Location, out CellParseNode? _)) {
                _content = value;
                CellValue.Value = "0";
                CellValue.Type = CellType.FORMULA;
            }
            else {
                _content = TryParseDate(value);
                CellValue.Value = _content;
                CellValue.Type = double.TryParse(_content, out double _) ? CellType.NUMBER : CellType.TEXT;
            }
            if (_sheet != null) {
                _sheet.Modified = true;
            }
        }
    }

    /// <summary>
    /// Convert a CellLocation to its absolute address.
    /// </summary>
    /// <param name="location">A CellLocation</param>
    /// <returns>String containing the absolute location</returns>
    public static string LocationToAddress(CellLocation location) =>
        $"{ColumnToAddress(location.Column)}{location.Row}";

    /// <summary>
    /// Convert a CellLocation to its relative address in the format
    /// R(y)C(x) where y and x are row and column offsets respectively.
    /// </summary>
    /// <param name="location">A CellLocation</param>
    /// <returns>Cell location to a relative address</returns>
    public static string LocationToAddress(Point location) =>
        $"R({location.Y})C({location.X})";

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
    /// Parse a cell relative address of the format R(y)C(x) and return
    /// a Point that contains (x,y).
    /// </summary>
    /// <param name="address">Address string</param>
    /// <returns>Point contain a relative row and column</returns>
    public static Point PointFromRelativeAddress(string address) {
        ArgumentNullException.ThrowIfNull(address);
        string exceptionString = $"Invalid relative address: {address}";
        int newColumn = 0;
        int newRow = 0;
        int index = 0;
        int factor = 1;
        if (index < address.Length - 2 && address[index++] != 'R' || address[index++] != '(') {
            throw new ArgumentException(exceptionString);
        }
        if (index < address.Length && address[index] == '-') {
            factor = -1;
            index++;
        }
        while (index < address.Length && char.IsDigit(address[index])) {
            newRow = newRow * 10 + address[index] - '0';
            index++;
        }
        newRow *= factor;
        if (index < address.Length - 3 && address[index++] != ')' || address[index++] != 'C' || address[index++] != '(') {
            throw new ArgumentException(exceptionString);
        }
        factor = 1;
        if (index < address.Length && address[index] == '-') {
            factor = -1;
            index++;
        }
        while (index < address.Length && char.IsDigit(address[index])) {
            newColumn = newColumn * 10 + address[index] - '0';
            index++;
        }
        newColumn *= factor;
        if (index >= address.Length || address[index] != ')') {
            throw new ArgumentException(exceptionString);
        }
        return new Point { X = newColumn, Y = newRow };
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
    public string Text(int width) {
        Debug.Assert(width >= 0);
        string cellValue = CellValue.Value;
        bool isNumber = double.TryParse(cellValue, out double doubleValue);
        if (isNumber) {
            cellValue = CellFormat switch {
                CellFormat.FIXED => doubleValue.ToString($"F{DecimalPlaces}"),
                CellFormat.PERCENT => $"{(doubleValue * 100).ToString($"F{DecimalPlaces}")}%",
                CellFormat.CURRENCY => doubleValue < 0 ?
                        $"(\u00a3{(-doubleValue).ToString($"N{DecimalPlaces}")})" :
                        $"\u00a3{doubleValue.ToString($"N{DecimalPlaces}")}",
                CellFormat.COMMAS => doubleValue < 0 ?
                        $"({(-doubleValue).ToString($"N{DecimalPlaces}")})" :
                        $"{doubleValue.ToString($"N{DecimalPlaces}")}",
                CellFormat.SCIENTIFIC => doubleValue.ToString($"0.{new string('#', DecimalPlaces)}E+00"),
                CellFormat.DATE_DM => ToDateTime("d-MMM", doubleValue),
                CellFormat.DATE_MY => ToDateTime("MMM-yyyy", doubleValue),
                CellFormat.DATE_DMY => ToDateTime("d-MMM-yyyy", doubleValue),
                CellFormat.GENERAL => cellValue,
                CellFormat.TEXT => UIContent,
                _ => throw new ArgumentException($"Unknown Cell Format: {CellFormat}")
            };
            if (cellValue.Length > width) {
                cellValue = new string('*', width);
            }
        }
        else if (cellValue.Length > width) {
            cellValue = cellValue[..width];
        }
        cellValue = Alignment switch {
            CellAlignment.LEFT => cellValue.PadRight(width),
            CellAlignment.RIGHT => cellValue.PadLeft(width),
            CellAlignment.CENTRE => Utilities.CentreString(cellValue, width),
            CellAlignment.GENERAL => !isNumber ? cellValue.PadRight(width) : cellValue.PadLeft(width),
            _ => throw new ArgumentException($"Unknown Cell Alignment: {Alignment}")
        };
        return cellValue;
    }

    /// <summary>
    /// Return an AnsiTextSpan for the current cell.
    /// </summary>
    /// <param name="width">Column width to use</param>
    /// <returns>AnsiTextSpan</returns>
    public AnsiText.AnsiTextSpan AnsiTextSpan(int width) {
        return new AnsiText.AnsiTextSpan(Text(width)) {
            ForegroundColour = Style.ForegroundColour,
            BackgroundColour = Style.BackgroundColour,
            Bold = Style.Bold,
            Italic = Style.Italic,
            Underline = Style.Underline
        };
    }

    /// <summary>
    /// Exchange the contents of this cell with another cell.
    /// </summary>
    /// <param name="other">Cell to swap</param>
    public void Swap(Cell other) {
        (CellValue, other.CellValue) = (other.CellValue, CellValue);
        (Format, other.Format) = (other.Format, Format);
        (Align, other.Align) = (other.Align, Align);
        (Decimal, other.Decimal) = (other.Decimal, Decimal);
        (Style, other.Style) = (other.Style, Style);
    }

    /// <summary>
    /// If this is a formula cell, this fixes up the address references in the
    /// formula that refer to the specified row or column by the given offset.
    /// This method would typically be called when columns or rows are added.
    /// If either column or row are set to 0, no change is made to that part
    /// of the location.
    /// </summary>
    /// <param name="column">Column to fix</param>
    /// <param name="row">Row to fix</param>
    /// <param name="offset">Offset to be applied to the column and/or row</param>
    public bool FixupFormula(int column, int row, int offset) {
        Debug.Assert(CellValue.Type == CellType.FORMULA);
        return ParseNode.FixupAddress(Location, column, row, offset);
    }

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
    /// Validate that a string parses to a valid formula. It only checks the
    /// formula syntax and does not attempt to evaluate it.
    /// </summary>
    /// <param name="formula">String to verify</param>
    /// <param name="location">Location of cell containing formula</param>
    /// <param name="cellParseNode">Set to the generated parse tree root</param>
    /// <returns>True if formula is valid, false otherwise</returns>
    private static bool TryParseFormula(string formula, CellLocation location, out CellParseNode? cellParseNode) {
        if (formula.Length == 0 || formula[0] != '=') {
            cellParseNode = null;
            return false;
        }
        try {
            FormulaParser parser = new FormulaParser(formula[1..], location);
            cellParseNode = parser.Parse();
            return true;
        } catch (FormatException) {
            cellParseNode = null;
            return false;
        }
    }

    /// <summary>
    /// Try to parse the value as a date and, if we succeed, return the OADate
    /// value as a string. Otherwise. return the original value.
    /// </summary>
    /// <param name="value">Value to parse</param>
    /// <returns>OADate value of date, or the original value</returns>
    private static string TryParseDate(string value) {
        CultureInfo culture = CultureInfo.CurrentCulture;
        string compactValue = value.Replace(" ", "");
        if (DateTime.TryParseExact(compactValue, "d-MMM", culture, DateTimeStyles.None, out DateTime _date)) {
            return _date.ToOADate().ToString(culture);
        }
        if (DateTime.TryParseExact(compactValue, "MMM-yyyy", culture, DateTimeStyles.None, out _date)) {
            return _date.ToOADate().ToString(culture);
        }
        if (DateTime.TryParseExact(compactValue, "d-MMM-yyyy", culture, DateTimeStyles.None, out _date)) {
            return _date.ToOADate().ToString(culture);
        }
        return value;
    }
}