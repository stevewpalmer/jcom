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
    public static Variant AVG(params Variant[] arguments) {
        Variant totalValues = new(arguments.Length);
        Variant sumTotal = arguments.Aggregate(new Variant(0), (current, value) => current + value);
        return totalValues.IntValue > 0 ? new Variant(sumTotal / totalValues) : new Variant(0);
    }

    /// <summary>
    /// Calculate the sum of all cells and constants in the argument list.
    /// </summary>
    /// <param name="arguments">Function parameters</param>
    /// <returns>A variant containing the sum of the arguments</returns>
    public static Variant SUM(params Variant[] arguments) {
        return arguments.Aggregate(new Variant(0), (current, value) => current + value);
    }

    /// <summary>
    /// Returns the length of a string.
    /// </summary>
    /// <param name="str">A variant specifying the string</param>
    /// <returns>A variant containing the length of the input string</returns>
    public static Variant LEN(Variant str) {
        return new Variant(str.StringValue.Length);
    }

    /// <summary>
    /// Returns the current date and time as a serial number.
    /// </summary>
    /// <returns>A variant containing the serial number of the current date and time</returns>
    public static Variant NOW() {
        return new Variant(DateTime.Now.ToOADate());
    }

    /// <summary>
    /// Returns the current date as a serial number.
    /// </summary>
    /// <returns>A variant containing the serial number of the current date</returns>
    public static Variant TODAY() {
        return new Variant(DateTime.Now.ToOADate());
    }

    /// <summary>
    /// Returns a serial number representing the specified time on the current date.
    /// </summary>
    /// <param name="hour">Variant specifying the hour element</param>
    /// <param name="minute">Variant specifying the minute element</param>
    /// <param name="second">Variant specifying the second element</param>
    /// <returns>A variant containing the serial number of the specified time</returns>
    public static Variant TIME(Variant hour, Variant minute, Variant second) {
        DateTime oaBaseDate = DateTime.FromOADate(0);
        TimeSpan ts = new(hour.IntValue, minute.IntValue, second.IntValue);
        return new Variant(oaBaseDate.Add(ts).ToOADate());
    }

    /// <summary>
    /// Returns a serial number representing the specified date.
    /// </summary>
    /// <param name="day">Variant specifying the hour element</param>
    /// <param name="month">Variant specifying the hour element</param>
    /// <param name="year">Variant specifying the hour element</param>
    /// <returns>A variant containing the serial number of the specified date</returns>
    public static Variant DATE(Variant day, Variant month, Variant year) {
        DateTime ts = new(day.IntValue, month.IntValue, year.IntValue);
        return new Variant(ts.ToOADate());
    }

    /// <summary>
    /// Returns the serial number that represents the date that is the indicated number
    /// of months before or after a specified date (the start_date).
    /// </summary>
    /// <param name="initialDate">Variant specifying the initial date</param>
    /// <param name="months">Variant specifying the month</param>
    /// <returns>A variant containing the serial number of the computed date</returns>
    public static Variant EDATE(Variant initialDate, Variant months) {
        try {
            DateTime date = DateTime.FromOADate(initialDate.DoubleValue);
            date = date.AddMonths(months.IntValue);
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
    /// <param name="date1">Variant specifying the first date</param>
    /// <param name="date2">Variant specifying the second date</param>
    /// <returns>
    /// A variant containing the number of days between the two days according to
    /// a 360-day year.
    /// </returns>
    public static Variant DAYS360(Variant date1, Variant date2) {
        try {
            DateTime startDate = DateTime.FromOADate(date1.DoubleValue);
            DateTime endDate = DateTime.FromOADate(date2.DoubleValue);
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
    /// <param name="inputDate">Variant specifying the input date</param>
    /// <returns>A vatiant containing the year part of a date</returns>
    public static Variant YEAR(Variant inputDate) {
        try {
            DateTime date = DateTime.FromOADate(inputDate.DoubleValue);
            return new Variant(date.Year);
        }
        catch {
            throw new Exception("Number out of range");
        }
    }

    /// <summary>
    /// Extract and return the month part of a date.
    /// </summary>
    /// <param name="inputDate">Variant specifying the input date</param>
    /// <returns>A variant containing the month value of a date</returns>
    public static Variant MONTH(Variant inputDate) {
        try {
            DateTime date = DateTime.FromOADate(inputDate.DoubleValue);
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
    public static Variant CONCATENATE(params Variant[] arguments) {
        return new Variant(string.Concat(arguments.ToList()));
    }

    /// <summary>
    /// Format a value by applying formatting to it with format codes. The
    /// value is specified in the first parameter and the format in the second
    /// parameter.
    /// </summary>
    /// <param name="value">Variant specifying the value to format</param>
    /// <param name="formatString">Variant specifying the format string</param>
    /// <returns>
    /// A variant containing the result of the value parameter formatted using the
    /// format specified in the format parameter. If the format parameter does not specify a
    /// valid number or string format then the text is returned unchanged.
    /// </returns>
    public static Variant TEXT(Variant value, Variant formatString) {
        NumberFormat customFormat = new(formatString.StringValue);
        CultureInfo culture = CultureInfo.CurrentCulture;
        return new Variant(customFormat.Format(value.DoubleValue, culture));
    }
}