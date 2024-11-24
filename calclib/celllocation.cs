// JCalcLib
// A cell location
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
using System.Text.Json.Serialization;

namespace JCalcLib;

/// <summary>
/// A CellLocation represents a 1-based index of a cell on a sheet. A CellLocation
/// has two public elements: the cell row and the cell column. It also exposes
/// equality semantics allowing two CellLocation objects to be compared for equality.
/// </summary>
public struct CellLocation : IEquatable<CellLocation> {
    private int _row = 1;
    private int _column = 1;

    /// <summary>
    /// Constructs an empty CellLocation
    /// </summary>
    public CellLocation() { }

    /// <summary>
    /// Initialise a CellLocation with a given column and
    /// row. Both column and row should be 1-based and less
    /// than the column and row maximums.
    /// </summary>
    /// <param name="column">Column</param>
    /// <param name="row">Row</param>
    public CellLocation(int column, int row) {
        Column = column;
        Row = row;
    }

    /// <summary>
    /// Initialise a CellLocation with a given address or throws an exception if the address
    /// is not a valid cell location.
    /// </summary>
    /// <param name="address">A string containing a cell address</param>
    /// <exception cref="ArgumentException">The address is not a valid cell location</exception>
    public CellLocation(string address) {
        if (!TryParseAddress(address, out CellLocation location)) {
            throw new FormatException();
        }
        Column = location.Column;
        Row = location.Row;
    }

    /// <summary>
    /// Converts the string representation of a cell location to its CellLocation equivalent.
    /// A return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="s">A string containing an address to parse</param>
    /// <param name="result">
    /// When this method returns, contains CellLocation equivalent
    /// of the address contained in s, if the conversion succeeded, or (1, 1) if the conversion failed.
    /// The conversion fails if the s parameter is null or Empty, is not of the correct format,
    /// or represents an address that is outside the range supported by Sheet. This parameter is
    /// passed uninitialized; any value originally supplied in result will be overwritten.
    /// </param>
    /// <returns>True if the address is parsed as a cell location, false otherwise</returns>
    public static bool TryParseAddress(string s, out CellLocation result) {
        if (!string.IsNullOrEmpty(s)) {
            int newColumn = 0;
            int newRow = 0;
            int index = 0;
            while (index < s.Length && char.IsLetter(s[index])) {
                newColumn = newColumn * 26 + char.ToUpper(s[index]) - 'A' + 1;
                index++;
            }
            while (index < s.Length && char.IsDigit(s[index])) {
                newRow = newRow * 10 + s[index] - '0';
                index++;
            }
            if (newColumn is >= 1 and <= Sheet.MaxColumns && newRow is >= 1 and <= Sheet.MaxRows) {
                result = new CellLocation(newColumn, newRow);
                return true;
            }
        }
        result = new CellLocation(1, 1);
        return false;
    }

    /// <summary>
    /// Retrieve or set the 1-based row of this cell location. If the new value
    /// is outside the valid row range for the sheet then an argument range
    /// exception is thrown.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Value is out of range</exception>
    public int Row {
        get => _row;
        set {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value, Sheet.MaxRows);
            _row = value;
        }
    }

    /// <summary>
    /// Retrieve or set the 1-based column of this cell location. If the new value
    /// is outside the valid column range for the sheet then an argument range
    /// exception is thrown.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Value is out of range</exception>
    public int Column {
        get => _column;
        set {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value, Sheet.MaxColumns);
            _column = value;
        }
    }

    /// <summary>
    /// Returns this cell location as a Point.
    /// </summary>
    [JsonIgnore]
    public Point Point => new(Column, Row);

    /// <summary>
    /// Convert this CellLocation to its absolute address.
    /// </summary>
    /// <returns>A string containing the absolute cell location</returns>
    [JsonIgnore]
    public string Address => $"{Cell.ColumnToAddress(Column)}{Row}";

    /// <summary>
    /// Check two cell locations for equality
    /// </summary>
    /// <param name="other">CellLocation to compare</param>
    /// <returns>True if the locations are equal</returns>
    public bool Equals(CellLocation other) {
        return _row == other._row && _column == other._column;
    }

    /// <summary>
    /// Check two cell locations for equality
    /// </summary>
    /// <param name="obj">A CellLocation object or null</param>
    /// <returns>True if the locations are equal</returns>
    public override bool Equals(object? obj) {
        return obj is CellLocation other && Equals(other);
    }

    /// <summary>
    /// Computes the hash code for the cell location.
    /// </summary>
    /// <returns>Hash code value</returns>
    public override int GetHashCode() {
        return HashCode.Combine(_row, _column);
    }

    /// <summary>
    /// Equality operator
    /// </summary>
    public static bool operator ==(CellLocation left, CellLocation right) =>
        left.Equals(right);

    /// <summary>
    /// Not equality operator.
    /// </summary>
    public static bool operator !=(CellLocation left, CellLocation right) =>
        !(left == right);
}