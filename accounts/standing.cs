// Accounts
// A standing order
//
// Authors:
//  Steve Palmer
//
// Copyright (C) 2023 Steve Palmer
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

namespace JAccounts;

public enum TFrequency {

    /// <summary>
    /// Scheduled for a specific day of the month
    /// </summary>
    DAY,

    /// <summary>
    /// Scheduled every week on a given day.
    /// </summary>
    WEEKLY,

    /// <summary>
    /// Scheduled for a specific Monday to Friday of the month
    /// </summary>
    WORKING_DAY
}

public class TStanding {

    /// <summary>
    /// Default constructor for JSON deserialization.
    /// </summary>
    [JsonConstructor]
    public TStanding() { }

    /// <summary>
    /// Construct a TStanding with the given properties.
    /// </summary>
    /// <param name="theName">Order name</param>
    /// <param name="theValue">Order value</param>
    /// <param name="frequency">The frequency of the order</param>
    /// <param name="day">Day when the order occurs</param>
    public TStanding(string theName, double theValue, TFrequency frequency, int day) {
        Name = theName;
        Value = theValue;
        Frequency = frequency;
        Day = day;
    }

    /// <summary>
    /// Order name
    /// </summary>
    [JsonInclude]
    public string Name { get; set; } = "";

    /// <summary>
    /// Order value
    /// </summary>
    [JsonInclude]
    public double Value { get; set; }

    /// <summary>
    /// Frequency of the order
    /// </summary>
    [JsonInclude]
    public TFrequency Frequency { get; set; }

    /// <summary>
    /// Day of the order
    /// </summary>
    [JsonInclude]
    public int Day { get; set; } = 1;

    /// <summary>
    /// Given a year and month, return all standing orders in that time
    /// period.
    /// </summary>
    /// <param name="year">Year</param>
    /// <param name="month">Month</param>
    /// <returns>An array of records</returns>
    public TRecord[] Records(int year, int month) {
        List<TRecord> records = [];
        int lastDay = DateTime.DaysInMonth(year, month);
        if (Frequency == TFrequency.DAY) {
            int day = Day <= lastDay ? Day : lastDay;
            records.Add(new TRecord(Name, Value, new TDate(year, month, day)));
        }
        if (Frequency == TFrequency.WORKING_DAY) {
            int day = NearestWeekday(year, month, Day);
            records.Add(new TRecord(Name, Value, new TDate(year, month, day)));
        }
        if (Frequency == TFrequency.WEEKLY) {
            for (int day = 1; day <= lastDay; day++) {
                DateTime start = new(year, month, day);
                if (start.DayOfWeek == (DayOfWeek)Day) {
                    records.Add(new TRecord(Name, Value, new TDate(year, month, day)));
                }
            }
        }
        return records.ToArray();
    }

    /// <summary>
    /// Returns the nearest week day for the given day in a month. If the day falls
    /// on a Saturday or Sunday, the next Monday is returned unless that falls in
    /// the next month in which case the previous Friday is returned. Otherwise,
    /// the day is returned unchanged unless it falls outside the number of days in
    /// the month in which case it is set to the last weekday in that month.
    /// </summary>
    /// <param name="year">Year</param>
    /// <param name="month">Month</param>
    /// <param name="day">Day</param>
    /// <returns>The nearest week day to the given day</returns>
    private int NearestWeekday(int year, int month, int day) {
        int lastDay = DateTime.DaysInMonth(year, month);
        if (day > lastDay) {
            day = lastDay;
        }
        DateTime start = new(year, month, day);
        if (start.DayOfWeek == DayOfWeek.Saturday) {
            return day > 1 && day + 2 > lastDay ? day - 1 : day + 2;
        }
        if (start.DayOfWeek == DayOfWeek.Sunday) {
            return day > 2 && day + 1 > lastDay ? day - 2 : day + 1;
        }
        return day;
    }
}