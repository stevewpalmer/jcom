// JCalcLib
// A cell location
//
// Authors:
//  Steve
//
// Copyright (C) 2024 Steve
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
using System.Text.Json.Serialization;

namespace JCalcLib;

public struct CellLocation : IEquatable<CellLocation> {
    private int _row = 1;
    private int _column = 1;

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
    /// Initialise a CellLocation with a given address.
    /// Both column and row should be 1-based and less
    /// than the column and row maximums.
    /// </summary>
    /// <param name="address">Address string</param>
    public CellLocation(string address) {
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
        Column = newColumn;
        Row = newRow;
    }

    /// <summary>
    /// 1-based row
    /// </summary>
    public int Row {
        get => _row;
        set {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value, Sheet.MaxRows);
            _row = value;
        }
    }

    /// <summary>
    /// 1-based column
    /// </summary>
    public int Column {
        get => _column;
        set {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value, Sheet.MaxColumns);
            _column = value;
        }
    }

    /// <summary>
    /// Returns the location as a Point
    /// </summary>
    [JsonIgnore]
    public Point Point => new(Column, Row);

    /// <summary>
    /// Convert a CellLocation to its absolute address.
    /// </summary>
    /// <returns>String containing the absolute location</returns>
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