// JCalcLib
// Cell value
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
using System.Globalization;
using System.Text.Json.Serialization;

namespace JCalcLib;

public class CellValue : IComparable<CellValue> {
    private CellParseNode? _cellParseNode;
    private string _content = string.Empty;

    /// <summary>
    /// Cell type
    /// </summary>
    [JsonIgnore]
    public CellType Type { get; private set; } = CellType.NONE;

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
                switch (Type) {
                    case CellType.TEXT:
                        _cellParseNode = new TextParseNode(_content);
                        break;
                    case CellType.NUMBER:
                        _cellParseNode = new NumberParseNode(double.Parse(_content));
                        break;
                    case CellType.FORMULA: {
                        FormulaParser parser = new FormulaParser(_content[1..]);
                        _cellParseNode = parser.Parse();
                        break;
                    }
                }
            }
            Debug.Assert(_cellParseNode != null);
            return _cellParseNode;
        }
    }

    /// <summary>
    /// The computed or literal value of the cell.
    /// </summary>
    [JsonIgnore]
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Contents of the cell. This differs from the value in that it holds
    /// the raw cell content as entered by the user. If the content is a
    /// formula, the value is evaluated from the content. For other types,
    /// the content and value are identical.
    /// </summary>
    [JsonInclude]
    public string Content {
        get => Type == CellType.FORMULA ? $"={ParseNode}" : ParseNode.ToString() ?? "";
        set {
            _cellParseNode = null;
            if (value.Length > 0 && value[0] == '=') {
                _content = value;
                Value = "0";
                Type = CellType.FORMULA;
            }
            else {
                _content = TryParseDate(value);
                Value = _content;
                Type = double.TryParse(_content, out double _) ? CellType.NUMBER : CellType.TEXT;
            }
        }
    }

    /// <summary>
    /// Compare this cell value with another for use when sorting.
    /// </summary>
    /// <param name="other">Other cell value to compare</param>
    /// <returns>Sort relationship</returns>
    public int CompareTo(CellValue? other) {
        if (other != null) {
            switch (Type) {
                case CellType.NUMBER: {
                    double value1 = double.Parse(Value);
                    double value2 = double.TryParse(other.Value, out double value) ? value : 0;
                    return value1.CompareTo(value2);
                }
                case CellType.TEXT:
                    return string.Compare(Value, other.Value, StringComparison.Ordinal);
            }
        }
        return 1;
    }

    /// <summary>
    /// Implement greater than operator
    /// </summary>
    /// <param name="operand1">First cell value</param>
    /// <param name="operand2">Second cell value</param>
    /// <returns>True if operand1 is greater than operand2, false otherwise</returns>
    public static bool operator > (CellValue operand1, CellValue operand2) {
        return operand1.CompareTo(operand2) > 0;
    }

    /// <summary>
    /// Implement less than operator
    /// </summary>
    /// <param name="operand1">First cell value</param>
    /// <param name="operand2">Second cell value</param>
    /// <returns>True if operand1 is less than operand2, false otherwise</returns>
    public static bool operator < (CellValue operand1, CellValue operand2) {
        return operand1.CompareTo(operand2) < 0;
    }

    /// <summary>
    /// Return the cell value as a string for display.
    /// </summary>
    public new string ToString() {
        return Type == CellType.TEXT ? $"\"{Value}\"" : Value;
    }

    /// <summary>
    /// Return the raw cell contents.
    /// </summary>
    public string ToText() {
        return Type == CellType.FORMULA ? Content : ToString();
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