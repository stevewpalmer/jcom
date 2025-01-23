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

using System.Diagnostics;
using System.Globalization;
using ExcelNumberFormat;
using JComLib;

namespace JCalcLib;

// ReSharper disable UnusedMember.Global
public static class Functions {

    /// <summary>
    /// Calculate the average of all cells and constants in the argument list.
    /// </summary>
    /// <param name="arguments">Function parameters</param>
    /// <returns>A variant containing the average of the arguments</returns>
    public static Variant AVG(IEnumerable<Variant> arguments) {
        Variant sumTotal = new(0);
        Variant totalValues = new(arguments.Count());
        sumTotal = arguments.Aggregate(sumTotal, (current, value) => current + value);
        return new Variant(sumTotal / totalValues);
    }

    /// <summary>
    /// Calculate the sum of all cells and constants in the argument list.
    /// </summary>
    /// <param name="arguments">Function parameters</param>
    /// <returns>A variant containing the sum of the arguments</returns>
    public static Variant SUM(IEnumerable<Variant> arguments) {
        Variant sumTotal = new(0);
        return arguments.Aggregate(sumTotal, (current, value) => current + value);
    }

    /// <summary>
    /// Returns the current date and time as a serial number.
    /// </summary>
    /// <param name="_">Function parameters</param>
    /// <returns>A variant containing the serial number of the current date and time</returns>
    public static Variant NOW(IEnumerable<Variant> _) {
        return new Variant(DateTime.Now.ToOADate());
    }

    /// <summary>
    /// Returns the current date as a serial number.
    /// </summary>
    /// <param name="_">Function parameters</param>
    /// <returns>A variant containing the serial number of the current date</returns>
    public static Variant TODAY(IEnumerable<Variant> _) {
        return new Variant(DateTime.Now.ToOADate());
    }

    /// <summary>
    /// Returns a serial number representing the specified time on the current date.
    /// </summary>
    /// <param name="arguments">Function parameters</param>
    /// <returns>A variant containing the serial number of the specified time</returns>
    public static Variant TIME(IEnumerable<Variant> arguments) {
        Variant[] parts = arguments.ToArray();
        Debug.Assert(parts.Length == 3);
        DateTime oaBaseDate = DateTime.FromOADate(0);
        TimeSpan ts = new(parts[0].IntValue, parts[1].IntValue, parts[2].IntValue);
        return new Variant(oaBaseDate.Add(ts).ToOADate());
    }

    /// <summary>
    /// Returns a serial number representing the specified date.
    /// </summary>
    /// <param name="arguments">Function parameters</param>
    /// <returns>A variant containing the serial number of the specified date</returns>
    public static Variant DATE(IEnumerable<Variant> arguments) {
        Variant[] parts = arguments.ToArray();
        Debug.Assert(parts.Length == 3);
        DateTime ts = new(parts[0].IntValue, parts[1].IntValue, parts[2].IntValue);
        return new Variant(ts.ToOADate());
    }

    /// <summary>
    /// Returns the serial number that represents the date that is the indicated number
    /// of months before or after a specified date (the start_date).
    /// </summary>
    /// <param name="arguments">Function parameters</param>
    /// <returns>A variant containing the serial number of the computed date</returns>
    public static Variant EDATE(IEnumerable<Variant> arguments) {
        Variant[] parts = arguments.ToArray();
        Debug.Assert(parts.Length == 2);
        try {
            DateTime date = DateTime.FromOADate(parts[0].DoubleValue);
            date = date.AddMonths(parts[1].IntValue);
            return new Variant(date.ToOADate());
        }
        catch {
            throw new Exception("Number out of range");
        }
    }

    /// <summary>
    /// Returns the number of days between two dates based on a 360-day year (twelve 30-day months),
    /// which is used in some accounting calculations. The method applied is the European method and
    /// will require updates to support the NASD method.
    /// </summary>
    /// <param name="arguments">Function parameters</param>
    /// <returns>
    /// A variant containing the number of days between the two days according to
    /// a 360-day year.
    /// </returns>
    public static Variant DAYS360(IEnumerable<Variant> arguments) {
        Variant[] parts = arguments.ToArray();
        Debug.Assert(parts.Length == 2);
        try {
            DateTime startDate = DateTime.FromOADate(parts[0].DoubleValue);
            DateTime endDate = DateTime.FromOADate(parts[1].DoubleValue);
            if (endDate < startDate) {
                (endDate, startDate) = (startDate, endDate);
            }
            int lastFebruary = new DateTime(startDate.Year, 3, 1).AddDays(-1).Day;
            int startDays = startDate.Day;
            int endDays = endDate.Day;
            if (startDays == 31 || (startDate.Month == 2 && startDays == lastFebruary)) {
                startDays = 30;
            }
            if (endDays == 31 && startDays == 30) {
                endDays = 30;
            }
            int duration = (endDate.Year - startDate.Year) * 360 + (endDate.Month - startDate.Month) * 30 + (endDays - startDays);
            return new Variant(duration);
        }
        catch {
            throw new Exception("Number out of range");
        }
    }

    /// <summary>
    /// Extract and return the year part of a date.
    /// </summary>
    /// <param name="arguments">Function parameters</param>
    /// <returns>A vatiant containing the year part of a date</returns>
    public static Variant YEAR(IEnumerable<Variant> arguments) {
        Variant result = arguments.First();
        try {
            DateTime date = DateTime.FromOADate(result.DoubleValue);
            return new Variant(date.Year);
        }
        catch {
            throw new Exception("Number out of range");
        }
    }

    /// <summary>
    /// Extract and return the month part of a date.
    /// </summary>
    /// <param name="arguments">Function parameters</param>
    /// <returns>A variant containing the month value of a date</returns>
    public static Variant MONTH(IEnumerable<Variant> arguments) {
        Variant result = arguments.First();
        try {
            DateTime date = DateTime.FromOADate(result.DoubleValue);
            return new Variant(date.Month);
        }
        catch {
            throw new Exception("Number out of range");
        }
    }

    /// <summary>
    /// Concatenate the result of all arguments into a single text string.
    /// </summary>
    /// <param name="arguments">Function parameters</param>
    /// <returns>A variant containing the result of the concatenation</returns>
    public static Variant CONCATENATE(IEnumerable<Variant> arguments) {
        return new Variant(string.Concat(arguments));
    }

    /// <summary>
    /// Format a value by applying formatting to it with format codes. The
    /// value is specified in the first parameter and the format in the second
    /// parameter.
    /// </summary>
    /// <param name="arguments">Function parameters</param>
    /// <returns>
    /// A variant containing the result of the value parameter formatted using the
    /// format specified in the format parameter. If the format parameter does not specify a
    /// valid number or string format then the text is returned unchanged.
    /// </returns>
    public static Variant TEXT(IEnumerable<Variant> arguments) {
        Variant[] parts = arguments.ToArray();
        Debug.Assert(parts.Length == 2);
        NumberFormat customFormat = new(parts[1].StringValue);
        CultureInfo culture = CultureInfo.CurrentCulture;
        return new Variant(customFormat.Format(parts[0].DoubleValue, culture));
    }
}