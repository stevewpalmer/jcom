// JCalcLib
// Number format dictionary
//
// Authors:
//  Steven Palmer
//
// Copyright (C) 2024 Steven Palmer
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
using ExcelNumberFormat;

namespace JCalcLib;

public static class NumberFormats {
    private static readonly Dictionary<string, NumberFormat> formats = new();

    /// <summary>
    /// Get a numeric format from the formats cache. Since number formats can be computationally
    /// expensive to construct when they are shared with multiple cells, they are cached and
    /// </summary>
    /// <param name="format">Cell format</param>
    /// <param name="thousands">True if comma separators are required</param>
    /// <param name="decimalPlaces">Number of decimal places required</param>
    /// <returns></returns>
    public static NumberFormat GetFormat(CellFormat format, bool thousands = false, int decimalPlaces = 2) {
        string key = $"{JComLib.Utilities.GetEnumDescription(format)}{(thousands ? "C" : "N")}{decimalPlaces}";
        if (!formats.TryGetValue(key, out NumberFormat? _format)) {
            string main = format switch {
                CellFormat.GENERAL => "General",
                CellFormat.FIXED => (thousands ? "#,##0." : "0.") + new string('0', decimalPlaces),
                CellFormat.PERCENT => $"0.{new string('0', decimalPlaces)}%",
                CellFormat.SCIENTIFIC => $"0.{new string('0', decimalPlaces)}E+00",
                CellFormat.CURRENCY => $"{NumberFormatInfo.CurrentInfo.CurrencySymbol}#,##0.{new string('0', decimalPlaces)}",
                CellFormat.DATE_DMY => "dd-mmm-yyyy",
                CellFormat.DATE_DM => "dd-mmm",
                CellFormat.DATE_MY => "mmm-yyyy",
                CellFormat.TIME => "h:mm:ss AM/PM",
                _ => throw new ArgumentException($"Unhandled number format {format}")
            };
            _format = new NumberFormat(main);
            formats.Add(key, _format);
        }
        return _format;
    }
}