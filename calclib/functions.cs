// JCalcLib
// Implementation of functions
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

using JComLib;

namespace JCalcLib;

// ReSharper disable UnusedMember.Global

public static class Functions {

    /// <summary>
    /// Calculate the result of the SUM function
    /// </summary>
    /// <param name="_">Source cell</param>
    /// <param name="arguments">Function parameters</param>
    /// <returns>The result of the function as a Variant</returns>
    public static Variant SUM(Cell _, IEnumerable<Variant> arguments) {
        Variant sumTotal = new Variant(0);
        return arguments.Aggregate(sumTotal, (current, value) => current + value);
    }

    /// <summary>
    /// Insert the current date and time. If the cell has no existing
    /// format, we apply a default date-time format.
    /// </summary>
    /// <param name="cell">Source cell</param>
    /// <param name="_">Function parameters</param>
    /// <returns>Value to be applied to the cell</returns>
    public static Variant NOW(Cell cell, IEnumerable<Variant> _) {
        if (cell.Format == null) {
            cell.CellFormat = CellFormat.CUSTOM;
            cell.CustomFormatString = "dd/mm/yyyy h:mm";
        }
        return new Variant(DateTime.Now.ToOADate());
    }

    /// <summary>
    /// Insert the current date. If the cell has no existing
    /// format, we apply a default date-time format.
    /// </summary>
    /// <param name="cell">Source cell</param>
    /// <param name="_">Function parameters</param>
    /// <returns>Value to be applied to the cell</returns>
    public static Variant TODAY(Cell cell, IEnumerable<Variant> _) {
        if (cell.Format == null) {
            cell.CellFormat = CellFormat.DATE_DMY;
        }
        return new Variant(DateTime.Now.ToOADate());
    }

    /// <summary>
    /// Extract and insert the year part of a date.
    /// </summary>
    /// <param name="_">Source cell</param>
    /// <param name="arguments">Function arguments</param>
    /// <returns>Value to be applied to the cell</returns>
    public static Variant YEAR(Cell _, IEnumerable<Variant> arguments) {
        Variant result = arguments.First();
        try {
            DateTime date = DateTime.FromOADate(result.DoubleValue);
            return new Variant(date.Year);
        }
        catch {
            return new Variant(0);
        }
    }

    /// <summary>
    /// Extract and insert the month part of a date.
    /// </summary>
    /// <param name="_">Source cell</param>
    /// <param name="arguments">Function arguments</param>
    /// <returns>Value to be applied to the cell</returns>
    public static Variant MONTH(Cell _, IEnumerable<Variant> arguments) {
        Variant result = arguments.First();
        try {
            DateTime date = DateTime.FromOADate(result.DoubleValue);
            return new Variant(date.Month);
        }
        catch {
            return new Variant(0);
        }
    }

    /// <summary>
    /// Concatenate the result of all arguments into a single text string.
    /// </summary>
    /// <param name="_">Source cell</param>
    /// <param name="arguments">Function arguments</param>
    /// <returns>A variant containing the result of the concatenation</returns>
    public static Variant CONCATENATE(Cell _, IEnumerable<Variant> arguments) {
        return new Variant(string.Concat(arguments));
    }
}