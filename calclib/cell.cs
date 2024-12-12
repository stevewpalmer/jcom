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
        CustomFormatString = other.CustomFormatString;
        Align = other.Align;
        Decimal = other.Decimal;
        Style = new CellStyle(sheet, other.Style);
        ContentString = other.ContentString;
        Error = other.Error;
        FormulaTree = other.FormulaTree;
        Value = new Variant(other.ContentString);
    }

    /// <summary>
    /// Raw content string.
    /// </summary>
    [JsonIgnore]
    private string ContentString { get; set; } = string.Empty;

    /// <summary>
    /// Retrieve or set the cell's value. The cell's value is always the value
    /// used in calculation and is separate from its Content which is used
    /// to determine the cell's value and may be a formula.
    /// </summary>
    /// <exception cref="FormatException">An error was found when setting a formula</exception>
    [JsonIgnore]
    public Variant Value {
        get => ComputedValue;
        set {
            FormulaTree = null;
            Error = false;
            if (sheet != null) {
                sheet.Modified = true;
            }
            if (value.StringValue == "") {
                ComputedValue = new Variant();
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
            if (TryParseDate(value.StringValue, out Variant _dateValue, out string _format)) {
                if (!Format.HasValue) {
                    CellFormat = CellFormat.CUSTOM;
                    CustomFormatString = _format;
                }
                ComputedValue = _dateValue;
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
    /// <exception cref="FormatException">An error was found when setting a formula</exception>
    [JsonInclude]
    public string Content {
        get {
            if (FormulaTree != null) {
                return $"={FormulaTree}";
            }
            if (IsEmptyCell) {
                return string.Empty;
            }
            if (Format is CellFormat.DATE_DM or CellFormat.DATE_DMY or CellFormat.DATE_MY) {
                return DateTime.FromOADate(ComputedValue.DoubleValue).ToShortDateString();
            }
            if (Format is CellFormat.TIME_HM or CellFormat.TIME_HMZ or CellFormat.TIME_HMS or CellFormat.TIME_HMSZ) {
                return DateTime.FromOADate(ComputedValue.DoubleValue).ToShortTimeString();
            }
            return ContentString;
        }
        set {
            ContentString = value;
            FormulaTree = null;
            Value = new Variant(value);
        }
    }

    /// <summary>
    /// The root of the evaluated formula tree assigned to this
    /// cell, if any. If the cell has no formula, this is null.
    /// </summary>
    [JsonIgnore]
    internal CellNode? FormulaTree { get; private set; }

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
    /// Return the AnsiAlignment that maps to the cell alignment.
    /// </summary>
    internal AnsiAlignment AnsiAlignment =>
        Alignment switch {
            CellAlignment.LEFT => AnsiAlignment.LEFT,
            CellAlignment.RIGHT => AnsiAlignment.RIGHT,
            CellAlignment.CENTRE => AnsiAlignment.CENTRE,
            CellAlignment.GENERAL => Value.IsNumber ? AnsiAlignment.RIGHT : AnsiAlignment.LEFT,
            _ => throw new ArgumentOutOfRangeException()
        };

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
    /// Cell has an error in the formula
    /// </summary>
    [JsonIgnore]
    public bool Error { get; set; }

    /// <summary>
    /// Copy properties from another cell
    /// </summary>
    public void CopyFrom(Cell other) {
        StyleFrom(other);
        Content = other.FormulaTree != null ? $"={other.FormulaTree.ToRawString()}" : other.Content;
    }

    /// <summary>
    /// Copy style from another cell
    /// </summary>
    public void StyleFrom(Cell other) {
        CellFormat = other.CellFormat;
        CustomFormat = other.CustomFormat;
        CustomFormatString = other.CustomFormatString;
        Alignment = other.Alignment;
        DecimalPlaces = other.DecimalPlaces;
        Style = other.Style;
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
    /// Return the formatted contents of the cell for display. If includeSpilled
    /// is True and this cell is empty then the contents include any spilled cells
    /// to the left.
    /// </summary>
    /// <param name="width">Column width to use</param>
    /// <param name="includeSpilled">True if we include spilled cells</param>
    /// <returns>AnsiTextSpan for cell</returns>
    public AnsiTextSpan AnsiTextForWidth(int width, bool includeSpilled) {
        string cellText = TextForWidth(width);
        Cell thisCell = this;
        AnsiAlignment alignment = AnsiAlignment;
        if (includeSpilled && IsEmptyCell && sheet != null) {
            int column = Location.Column;
            Cell? leftCell = null;
            while (column >= 1) {
                leftCell = sheet.GetCell(column, Location.Row, false);
                if (!leftCell.IsEmptyCell) {
                    break;
                }
                column--;
            }
            if (column > 0) {
                int leftCellWidth = sheet.ColumnWidth(column);
                if (leftCell is { Value.IsNumber: false } && leftCell.Value.StringValue.Length > leftCellWidth) {
                    int leftCellLength = leftCell.Value.StringValue.Length;
                    string leftCellText = leftCell.TextForWidth(leftCellLength);
                    int index = leftCellWidth;
                    while (++column < Location.Column) {
                        index += sheet.ColumnWidth(column);
                    }
                    if (index < leftCellLength) {
                        cellText = Utilities.SpanBound(leftCellText, index, sheet.ColumnWidth(column));
                        alignment = AnsiAlignment.NONE;
                        thisCell = leftCell;
                    }
                }
            }
        }
        return new AnsiTextSpan(cellText) {
            ForegroundColour = thisCell.Style.TextColour,
            BackgroundColour = thisCell.Style.BackgroundColour,
            Alignment = alignment,
            Width = width,
            Bold = thisCell.Style.Bold,
            Italic = thisCell.Style.Italic,
            Underline = thisCell.Style.Underline
        };
    }

    /// <summary>
    /// Return the nominal width of this cell.
    /// </summary>
    [JsonIgnore]
    public int Width => Text.Length;

    /// <summary>
    /// Return the string value of the cell with formatting
    /// applied.
    /// </summary>
    /// <returns>String contents of cell</returns>
    public string Text {
        get {
            string cellValue;
            if (Error) {
                return "!ERR";
            }
            if (!Value.IsNumber) {
                cellValue = Value.StringValue ?? string.Empty;
            }
            else {
                CultureInfo culture = CultureInfo.CurrentCulture;
                switch (CellFormat) {
                    case CellFormat.FIXED or
                        CellFormat.PERCENT or
                        CellFormat.CURRENCY or
                        CellFormat.SCIENTIFIC or
                        CellFormat.DATE_DM or
                        CellFormat.DATE_MY or
                        CellFormat.DATE_DMY or
                        CellFormat.TIME_HMSZ or
                        CellFormat.TIME_HMS or
                        CellFormat.TIME_HMZ or
                        CellFormat.TIME_HM or
                        CellFormat.GENERAL:
                        cellValue = NumberFormats.GetFormat(CellFormat, UseThousandsSeparator, DecimalPlaces).Format(Value.DoubleValue, culture);
                        break;
                    case CellFormat.CUSTOM:
                        cellValue = CustomFormat != null ? CustomFormat.Format(Value.DoubleValue, culture) : Value.DoubleValue.ToString(CultureInfo.CurrentCulture);
                        break;
                    case CellFormat.TEXT:
                        cellValue = Content;
                        break;
                    default:
                        throw new ArgumentException($"Unknown Cell Format: {CellFormat}");
                }
            }
            return cellValue;
        }
    }

    /// <summary>
    /// Return the string value of the cell for display, formatted for
    /// the specified column width. Numeric values that exceed the width
    /// are replaced by a string of asterisks of the given width. Text
    /// values are truncated to the width. The final output is aligned.
    /// </summary>
    /// <param name="width">Column width to use</param>
    /// <returns>String contents of cell</returns>
    public string TextForWidth(int width) {
        Debug.Assert(width >= 0);
        string cellValue = Text;
        if (Error) {
            return Utilities.CentreString(cellValue, width);
        }
        if (cellValue.Length > width) {
            cellValue = Value.IsNumber ? new string('*', width) :  cellValue[..width];
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
    public AnsiTextSpan AnsiTextSpan(int width) {
        return new AnsiTextSpan(TextForWidth(width)) {
            ForegroundColour = Style.TextColour,
            BackgroundColour = Style.BackgroundColour,
            Bold = Style.Bold,
            Italic = Style.Italic,
            Underline = Style.Underline,
            Alignment = AnsiAlignment,
            Width = width
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
    /// <exception cref="FormatException">An error was found in the formula</exception>
    private bool TryParseFormula(string formula, CellLocation location, out CellNode? formulaTree) {
        if (sheet == null || formula.Length < 2 || formula[0] != '=') {
            formulaTree = null;
            return false;
        }
        FormulaParser parser = new(formula[1..], location);
        formulaTree = parser.Parse();
        return true;
    }

    /// <summary>
    /// Try to parse the value as a date and, if we succeed, sets dateValue to the OADate
    /// value and returns true. Otherwise, it returns the value unchanged and returns false.
    /// </summary>
    /// <param name="value">Value to parse</param>
    /// <param name="dateValue">
    /// Set to the date serial number if the value parses successfully
    /// as a date, or is set to the input value otherwise
    /// </param>
    /// <param name="customFormat">Custom format to be applied to the cell if successful</param>
    /// <returns>True if the value is successfully parsed as a date, false otherwise</returns>
    private static bool TryParseDate(string value, out Variant dateValue, out string customFormat) {
        (string, string)[] formats = [
            ("yyyy-MM-ddTHH:mm:ssZ", "d/m/yyyy h:mm:ss"),
            ("d/M", "d-m"),
            ("d/MMM", "d-mmm"),
            ("d/MMMM", "d-mmmm"),
            ("d MMM", "d-mmm"),
            ("d MMMM", "d-mmmm"),
            ("d MMMM y", "d-mmmm-y"),
            ("d MMMM yyyy", "d-mmmm-yyy"),
            ("d/M/y", "d-m-y"),
            ("d/M/yyyy", "d-m-yyy"),
            ("MMM/y", "mmm-y"),
            ("MMMM/y", "mmmm-y"),
            ("MMM/yyyy", "mmm-yyy"),
            ("MMMM/yyyy", "mmmm-yyy"),
            ("MMM y", "mmm-y"),
            ("MMMM y", "mmmm-y"),
            ("MMM yyyy", "mmm-yyy"),
            ("MMMM yyyy", "mmmm-yyy"),
            ("h:m tt", "h:mm am/pm"),
            ("H:m", "h:mm"),
            ("h tt", "h am/pm"),
            ("H:m", "h:mm"),
            ("h:m:s tt", "h:mm:ss am/pm"),
            ("H:m:s", "h:mm:ss")
        ];
        foreach ((string, string) format in formats) {
            if (DateTime.TryParseExact(value, format.Item1, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, out DateTime dateTime)) {
                dateValue = new Variant(dateTime.ToOADate());
                customFormat = format.Item2;
                return true;
            }
        }
        dateValue = new Variant(value);
        customFormat = "";
        return false;
    }
}