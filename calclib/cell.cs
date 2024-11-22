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
using ExcelNumberFormat;
using JComLib;

namespace JCalcLib;

public class Cell(Sheet? sheet) {
    private CellParseNode? _cellParseNode;
    private string _content = string.Empty;
    private string? _customFormatString;

    /// <summary>
    /// Empty constructor
    /// </summary>
    public Cell() : this(null) { }

    /// <summary>
    /// Copy from another cell
    /// </summary>
    public void CopyFrom(Cell other) {
        CellFormat = other.CellFormat;
        CustomFormat = other.CustomFormat;
        Alignment = other.Alignment;
        CellValue = other.CellValue;
        DecimalPlaces = other.DecimalPlaces;
        Content = other.Content;
        Style = other.Style;
    }

    /// <summary>
    /// Cell value
    /// </summary>
    [JsonIgnore]
    public CellValue CellValue { get; set; } = new();

    /// <summary>
    /// Cell alignment
    /// </summary>
    [JsonInclude]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CellAlignment? Align { get; private set; }

    /// <summary>
    /// Cell alignment
    /// </summary>
    [JsonIgnore]
    public CellAlignment Alignment {
        get => Align.GetValueOrDefault(CellFactory.Alignment);
        set {
            Align = value;
            if (sheet != null) {
                sheet.Modified = true;
            }
        }
    }

    /// <summary>
    /// Cell text style
    /// </summary>
    public CellStyle Style { get; set; } = new(sheet);

    /// <summary>
    /// Cell location
    /// </summary>
    public CellLocation Location { get; set; } = new();

    /// <summary>
    /// Return the current cell location as a string that
    /// represents the cell address on the screen.
    /// </summary>
    [JsonIgnore]
    public string Address => Location.Address;

    /// <summary>
    /// Return the format description of the current cell.
    /// </summary>
    [JsonIgnore]
    public string FormatDescription {
        get {
            string thousands = UseThousandsSeparator ? "C" : "";
            string text = CellFormat switch {
                CellFormat.FIXED => $"F{DecimalPlaces}{thousands}",
                CellFormat.SCIENTIFIC => $"S{DecimalPlaces}",
                CellFormat.TEXT => "T",
                CellFormat.GENERAL => "G",
                CellFormat.PERCENT => $"P{DecimalPlaces}",
                CellFormat.CURRENCY => $"C{DecimalPlaces}",
                CellFormat.DATE_DM => "D2",
                CellFormat.DATE_MY => "D3",
                CellFormat.DATE_DMY => "D1",
                CellFormat.TIME => "TM",
                CellFormat.CUSTOM => $"{CustomFormatString}",
                _ => "?"
            };
            return $"({text})";
        }
    }

    /// <summary>
    /// Cell format
    /// </summary>
    [JsonInclude]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CellFormat? Format { get; private set; }

    /// <summary>
    /// Cell format
    /// </summary>
    [JsonIgnore]
    public CellFormat CellFormat {
        get => Format.GetValueOrDefault(CellFactory.Format);
        set {
            Format = value;
            if (sheet != null) {
                sheet.Modified = true;
            }
        }
    }

    /// <summary>
    /// Pre-compiled custom number format
    /// </summary>
    [JsonIgnore]
    private NumberFormat? CustomFormat { get; set; }

    /// <summary>
    /// Custom cell format when format is set to CUSTOM
    /// </summary>
    [JsonInclude]
    public string? CustomFormatString {
        get => _customFormatString;
        set {
            _customFormatString = value;
            CustomFormat = new NumberFormat(_customFormatString);
        }
    }

    /// <summary>
    /// General number format
    /// </summary>
    [JsonIgnore]
    private readonly NumberFormat GeneralFormat = new("General");

    /// <summary>
    /// Number of decimal places
    /// </summary>
    [JsonInclude]
    public int? Decimal { get; private set; }

    /// <summary>
    /// Number of decimal places
    /// </summary>
    [JsonIgnore]
    public int DecimalPlaces {
        get => Decimal.GetValueOrDefault(CellFactory.DecimalPlaces);
        set {
            Decimal = value;
            if (sheet != null) {
                sheet.Modified = true;
            }
        }
    }

    /// <summary>
    /// Whether or not numbers are formatted with the thousand separator
    /// </summary>
    [JsonInclude]
    public bool? UseThousands { get; private set; }

    /// <summary>
    /// Whether or not numbers are formatted with the thousand separator
    /// </summary>
    [JsonIgnore]
    public bool UseThousandsSeparator {
        get => UseThousands.GetValueOrDefault(false);
        set {
            UseThousands = value;
            if (sheet != null) {
                sheet.Modified = true;
            }
        }
    }

    /// <summary>
    /// Fixed number format
    /// </summary>
    [JsonIgnore]
    private NumberFormat FixedFormat => NumberFormats.GetFormat("F", UseThousandsSeparator, DecimalPlaces);

    /// <summary>
    /// Scientific number format
    /// </summary>
    [JsonIgnore]
    private NumberFormat ScientificFormat => NumberFormats.GetFormat("S", false, DecimalPlaces);

    /// <summary>
    /// Scientific number format
    /// </summary>
    [JsonIgnore]
    private NumberFormat CurrencyFormat => NumberFormats.GetFormat("C", true, DecimalPlaces);

    /// <summary>
    /// Scientific number format
    /// </summary>
    [JsonIgnore]
    private NumberFormat PercentFormat => NumberFormats.GetFormat("P", true, DecimalPlaces);

    /// <summary>
    /// Date1 format
    /// </summary>
    [JsonIgnore]
    private static NumberFormat DateDMYFormat => NumberFormats.GetFormat("D1");

    /// <summary>
    /// Date2 format
    /// </summary>
    [JsonIgnore]
    private static NumberFormat DateDMFormat => NumberFormats.GetFormat("D2");

    /// <summary>
    /// Date3 format
    /// </summary>
    [JsonIgnore]
    private static NumberFormat DateMYFormat => NumberFormats.GetFormat("D3");

    /// <summary>
    /// Time format
    /// </summary>
    [JsonIgnore]
    private static NumberFormat TimeFormat => NumberFormats.GetFormat("TM");

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
                _content = TryParseTime(_content);
                CellValue.Value = _content;
                CellValue.Type = double.TryParse(_content, out double _) ? CellType.NUMBER : CellType.TEXT;
            }
            if (sheet != null) {
                sheet.Modified = true;
            }
        }
    }

    /// <summary>
    /// Convert a CellLocation to its relative address in the format
    /// R(y)C(x) where y and x are row and column offsets respectively.
    /// </summary>
    /// <param name="location">A CellLocation</param>
    /// <returns>Cell location to a relative address</returns>
    public static string LocationToAddress(Point location) =>
        $"R({location.Y})C({location.X})";

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
    public string FormattedText(int width) {
        Debug.Assert(width >= 0);
        string cellValue = CellValue.Value;
        bool isNumber = double.TryParse(cellValue, out double doubleValue);
        if (isNumber) {
            cellValue = CellFormat switch {
                CellFormat.FIXED => FixedFormat.Format(doubleValue, CultureInfo.CurrentCulture),
                CellFormat.PERCENT => PercentFormat.Format(doubleValue, CultureInfo.CurrentCulture),
                CellFormat.CURRENCY => CurrencyFormat.Format(doubleValue, CultureInfo.CurrentCulture),
                CellFormat.SCIENTIFIC => ScientificFormat.Format(doubleValue, CultureInfo.CurrentCulture),
                CellFormat.DATE_DM => DateDMFormat.Format(doubleValue, CultureInfo.CurrentCulture),
                CellFormat.DATE_MY => DateMYFormat.Format(doubleValue, CultureInfo.CurrentCulture),
                CellFormat.DATE_DMY => DateDMYFormat.Format(doubleValue, CultureInfo.CurrentCulture),
                CellFormat.CUSTOM => CustomFormat != null ? CustomFormat.Format(doubleValue, CultureInfo.CurrentCulture) : doubleValue.ToString(CultureInfo.CurrentCulture),
                CellFormat.GENERAL => GeneralFormat.Format(doubleValue, CultureInfo.CurrentCulture),
                CellFormat.TIME => TimeFormat.Format(doubleValue, CultureInfo.CurrentCulture),
                CellFormat.TEXT => UIContent,
                _ => throw new ArgumentException($"Unknown Cell Format: {CellFormat}")
            };
            if (cellValue.Length > width) {
                cellValue = new string('*', width);
            }
        }
        if (cellValue.Length > width) {
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
        return new AnsiText.AnsiTextSpan(FormattedText(width)) {
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
        Cell temp = new();
        temp.CopyFrom(other);
        other.CopyFrom(this);
        CopyFrom(temp);
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
        }
        catch (FormatException) {
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

    /// <summary>
    /// Try to parse the value as a time and, if we succeed, return the OADate
    /// value as a string. Otherwise. return the original value.
    /// </summary>
    /// <param name="value">Value to parse</param>
    /// <returns>OADate value of time, or the original value</returns>
    private static string TryParseTime(string value) {
        CultureInfo culture = CultureInfo.CurrentCulture;
        string compactValue = value.Replace(" ", "");
        if (DateTime.TryParseExact(compactValue, "t", culture, DateTimeStyles.None, out DateTime _date)) {
            return _date.ToOADate().ToString(culture);
        }
        if (DateTime.TryParseExact(compactValue, "T", culture, DateTimeStyles.None, out _date)) {
            return _date.ToOADate().ToString(culture);
        }
        if (DateTime.TryParseExact(compactValue, "h:mm:ss tt zz", culture, DateTimeStyles.None, out _date)) {
            return _date.ToOADate().ToString(culture);
        }
        return value;
    }
}