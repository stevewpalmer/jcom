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

using System.Text.Json.Serialization;

namespace JCalcLib;

public class CellValue : IComparable<CellValue> {

    /// <summary>
    /// Cell type
    /// </summary>
    [JsonIgnore]
    public CellType Type { get; internal set; } = CellType.NONE;

    /// <summary>
    /// The computed or literal value of the cell.
    /// </summary>
    [JsonIgnore]
    public string Value { get; set; } = string.Empty;

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
}