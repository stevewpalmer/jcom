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
    private string? _customFormatString;
    private string _content = string.Empty;

    /// <summary>
    /// Empty constructor
    /// </summary>
    public Cell() : this(null) { }

    /// <summary>
    /// Creates a Cell that copies another Cell
    /// </summary>
    public Cell(Sheet sheet, Cell other) : this(sheet) {
        Location = other.Location;
        Format = other.Format;
        CustomFormat = other.CustomFormat;
        Align = other.Align;
        Decimal = other.Decimal;
        Style = other.Style;
        Content = other.Content;
    }

    /// <summary>
    /// Retrieve or set the cell's value. The cell's value is always the value
    /// used in calculation and is separate from its Content which is used
    /// to determine the cell's value and may be a formula.
    /// </summary>
    [JsonIgnore]
    public Variant Value {
        get => ComputedValue;
        set {
            FormulaTree = null;
            if (sheet != null) {
                sheet.Modified = true;
            }
            if (TryParseDate(value.StringValue, out Variant _dateValue)) {
                if (!Format.HasValue) {
                    CellFormat = CellFormat.DATE_DMY;
                }
                ComputedValue = _dateValue;
                return;
            }
            if (TryParseTime(value.StringValue, out Variant _timeValue)) {
                if (!Format.HasValue) {
                    CellFormat = CellFormat.TIME;
                }
                ComputedValue = _timeValue;
                return;
            }
            if (TryParseFormula(value.StringValue, Location, out CellNode? formulaTree)) {
                FormulaTree = formulaTree;
                ComputedValue = new Variant(0);
                return;
            }
            if (double.TryParse(value.StringValue, out double doubleValue)) {
                ComputedValue = new Variant(doubleValue);
                return;
            }
            ComputedValue = value;
        }
    }

    /// <summary>
    /// The computed value of the cell. This is set by the calculation as
    /// well as by the Value property once it has translates the input
    /// value into a discrete cell value.
    /// </summary>
    [JsonIgnore]
    internal Variant ComputedValue { get; set; } = new();

    /// <summary>
    /// Cell content. The cell content is the value assigned to the cell
    /// by the user and may be a numeric or string constant, or a formula.
    /// If the cell contains a formula, then Content returns the formula
    /// with any cell references corrected for adjustments to the cell
    /// location on the sheet and thus will not necessarily be the exact
    /// same formula as originally entered.
    /// </summary>
    [JsonInclude]
    public string Content {
        get => FormulaTree != null ? $"={FormulaTree}" : _content;
        set {
            _content = value;
            FormulaTree = null;
            Value = new Variant(value);
        }
    }

    /// <summary>
    /// The root of the evaluated formula tree assigned to this
    /// cell, if any. If the cell has no formula, this is null.
    /// </summary>
    [JsonIgnore]
    public CellNode? FormulaTree { get; private set; }

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
            string text = Utilities.GetEnumDescription(CellFormat);
            text = CellFormat switch {
                CellFormat.FIXED => $"{text}{DecimalPlaces}{thousands}",
                CellFormat.SCIENTIFIC or CellFormat.PERCENT or CellFormat.CURRENCY => $"{text}{DecimalPlaces}",
                CellFormat.CUSTOM => $"{CustomFormatString}",
                _ => text
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
    /// Is this a blank cell?
    /// </summary>
    [JsonIgnore]
    public bool IsEmptyCell => !Value.HasValue;

    /// <summary>
    /// Returns whether or not this cell has a formula assigned to it.
    /// </summary>
    [JsonIgnore]
    public bool HasFormula => FormulaTree != null;

    /// <summary>
    /// Copy properties from another cell
    /// </summary>
    public void CopyFrom(Cell other) {
        CellFormat = other.CellFormat;
        CustomFormat = other.CustomFormat;
        Alignment = other.Alignment;
        DecimalPlaces = other.DecimalPlaces;
        Style = other.Style;
        Content = other.FormulaTree != null ? $"={other.FormulaTree.ToRawString()}" : other.Content;
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
        string cellValue;
        CultureInfo culture = CultureInfo.CurrentCulture;
        if (!Value.IsNumber) {
            cellValue = Value.StringValue ?? string.Empty;
        } else {
            cellValue = CellFormat switch {
                CellFormat.FIXED or CellFormat.PERCENT or CellFormat.CURRENCY or CellFormat.SCIENTIFIC
                    or CellFormat.DATE_DM or CellFormat.DATE_MY or CellFormat.DATE_DMY or CellFormat.TIME
                    or CellFormat.GENERAL => NumberFormats.GetFormat(CellFormat, UseThousandsSeparator, DecimalPlaces)
                        .Format(Value.DoubleValue, culture),
                CellFormat.CUSTOM => CustomFormat != null
                    ? CustomFormat.Format(Value.DoubleValue, CultureInfo.CurrentCulture)
                    : Value.DoubleValue.ToString(CultureInfo.CurrentCulture),
                CellFormat.TEXT => Content,
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
            CellAlignment.GENERAL => !Value.IsNumber ? cellValue.PadRight(width) : cellValue.PadLeft(width),
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
        return FormulaTree?.FixupAddress(Location, column, row, offset) ?? false;
    }

    /// <summary>
    /// Validate that a string parses to a valid formula. It only checks the
    /// formula syntax and does not attempt to evaluate it.
    /// </summary>
    /// <param name="formula">String to verify</param>
    /// <param name="location">Location of cell containing formula</param>
    /// <param name="formulaTree">Set to the generated formula tree, if any</param>
    /// <returns>True if formula is valid, false otherwise</returns>
    private bool TryParseFormula(string formula, CellLocation location, out CellNode? formulaTree) {
        if (sheet == null || formula.Length == 0 || formula[0] != '=') {
            formulaTree = null;
            return false;
        }
        try {
            FormulaParser parser = new FormulaParser(formula[1..], location);
            formulaTree = parser.Parse();
            return true;
        }
        catch (FormatException) {
            formulaTree = null;
            return false;
        }
    }

    /// <summary>
    /// Try to parse the value as a date and, if we succeed, sets dateValue to the OADate
    /// value and returns true. Otherwise, it returns the value unchanged and returns false.
    /// </summary>
    /// <param name="value">Value to parse</param>
    /// <param name="dateValue">Set to the date serial number if the value parses successfully
    /// as a date, or is set to the input value otherwise</param>
    /// <returns>True if the value is successfully parsed as a date, false otherwise</returns>
    private static bool TryParseDate(string value, out Variant dateValue) {
        CultureInfo culture = CultureInfo.CurrentCulture;
        string compactValue = value.Replace(" ", "");
        if (DateTime.TryParseExact(compactValue, "d-MMM", culture, DateTimeStyles.None, out DateTime _date)) {
            dateValue = new Variant(_date.ToOADate());
            return true;
        }
        if (DateTime.TryParseExact(compactValue, "MMM-yyyy", culture, DateTimeStyles.None, out _date)) {
            dateValue = new Variant(_date.ToOADate());
            return true;
        }
        if (DateTime.TryParseExact(compactValue, "d-MMM-yyyy", culture, DateTimeStyles.None, out _date)) {
            dateValue = new Variant(_date.ToOADate());
            return true;
        }
        dateValue = new Variant(value);
        return false;
    }

    /// <summary>
    /// Try to parse the value as a time and, if we succeed, sets timeValue to the OADate
    /// value and returns true. Otherwise, it returns the value unchanged and returns false.
    /// </summary>
    /// <param name="value">Value to parse</param>
    /// <param name="timeValue">Set to the time serial number if the value parses successfully
    /// as a time, or is set to the input value otherwise</param>
    /// <returns>True if the value is successfully parsed as a time, false otherwise</returns>
    private static bool TryParseTime(string value, out Variant timeValue) {
        CultureInfo culture = CultureInfo.CurrentCulture;
        string compactValue = value.Replace(" ", "");
        if (DateTime.TryParseExact(compactValue, "t", culture, DateTimeStyles.None, out DateTime _date)) {
            timeValue = new Variant(_date.ToOADate());
            return true;
        }
        if (DateTime.TryParseExact(compactValue, "T", culture, DateTimeStyles.None, out _date)) {
            timeValue = new Variant(_date.ToOADate());
            return true;
        }
        if (DateTime.TryParseExact(value, "h:mm:ss tt", culture, DateTimeStyles.None, out _date)) {
            timeValue = new Variant(_date.ToOADate());
            return true;
        }
        timeValue = new Variant(value);
        return false;
    }
}