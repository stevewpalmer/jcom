// JCalcLib
// Cell format types
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

using System.Globalization;
using System.Text.Json.Serialization;

namespace JCalcLib;

public class CellValue {
    private string _value = string.Empty;

    /// <summary>
    /// Cell type
    /// </summary>
    [JsonIgnore]
    public CellType Type { get; private set; } = CellType.NONE;

    /// <summary>
    /// String representation of content
    /// </summary>
    public string Value {
        get => _value;
        set {
            _value = TryParseDate(value);
            Type = double.TryParse(_value, out double _) ? CellType.NUMBER : CellType.TEXT;
        }
    }

    /// <summary>
    /// Return the cell value as a string for display.
    /// </summary>
    public new string ToString() {
        return Type == CellType.NUMBER ? Value : $"\"{Value}\"";
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
        if (DateTime.TryParseExact(compactValue, "dd-MMM", culture, DateTimeStyles.None, out DateTime _date)) {
            return _date.ToOADate().ToString(culture);
        }
        if (DateTime.TryParseExact(compactValue, "MMM-yyyy", culture, DateTimeStyles.None, out _date)) {
            return _date.ToOADate().ToString(culture);
        }
        if (DateTime.TryParseExact(compactValue, "dd-MMM-yyyy", culture, DateTimeStyles.None, out _date)) {
            return _date.ToOADate().ToString(culture);
        }
        return value;
    }
}