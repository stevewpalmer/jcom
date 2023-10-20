// Accounts
// Statement class
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

using System.Text.Json;

namespace JAccounts;

public class TStatement {

    /// <summary>
    /// Statement constructor with a specified year, month and set
    /// of records.
    /// </summary>
    /// <param name="theYear">Year</param>
    /// <param name="theMonth">Month</param>
    /// <param name="theRecords">List of records</param>
    public TStatement(int theYear, int theMonth, List<TRecord> theRecords) {
        Year = theYear;
        Month = theMonth;
        Records = theRecords;
    }

    /// <summary>
    /// Get or set the entry balance of this statement
    /// </summary>
    public double EntryBalance { get; set; }

    /// <summary>
    /// Get or set the year covered by this statement.
    /// </summary>
    public int Year { get; }

    /// <summary>
    /// Get or set the month covered by this statement.
    /// </summary>
    public int Month { get; }

    /// <summary>
    /// Return the list of records in this statement.
    /// </summary>
    public List<TRecord> Records { get; set; }

    /// <summary>
    /// Calculate and return the exit balance of this statement
    /// </summary>
    /// <returns>Exit balance value</returns>
    public double ExitBalance => EntryBalance + Records.Sum(record => record.Value);

    /// <summary>
    /// Is this a future statement?
    /// </summary>
    public bool IsFuture =>  Year > DateTime.Now.Year || Year == DateTime.Now.Year && Month > DateTime.Now.Month;

    /// <summary>
    /// Save this statement
    /// </summary>
    public void Save() {
        string fileName = $"{Utils.DataFolder}/{Year}/{Month}";
        string backupFile = $"{fileName}.bak";

        if (File.Exists(fileName)) {
            if (File.Exists(backupFile)) {
                File.Delete(backupFile);
            }
            File.Copy(fileName, backupFile);
        }

        using FileStream stream = File.Create(fileName);
        JsonSerializer.Serialize(stream, Records, new JsonSerializerOptions {
            WriteIndented = true
        });
    }
}