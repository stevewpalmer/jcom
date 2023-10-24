// Accounts
// Top-level account class
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

public class TAccount {
    private List<TRecord> _fixed = new();
    private readonly List<TStatement> _statements = new();
    private const double _startBalance = 0.0;
    private bool hasFixedRecords;

    /// <summary>
    /// Initialise this account.
    /// </summary>
    public static void Init() {
        string dataFolderPath = Utils.DataFolder;
        string thisYear = DateTime.Now.Year.ToString();
        string yearFolderPath = $"{dataFolderPath}/{thisYear}";

        if (!Directory.Exists(dataFolderPath)) {
            Directory.CreateDirectory(dataFolderPath);
        }

        // Make sure there's a folder for the current year
        if (!Directory.Exists(yearFolderPath)) {
            Directory.CreateDirectory(yearFolderPath);
        }
    }

    /// <summary>
    /// Retrieve a statement given an index.
    /// </summary>
    /// <param name="index">Index of statement</param>
    /// <returns>Statement</returns>
    public TStatement Get(int index) {

        TStatement statement = _statements[index];
        if (statement.Records.Count == 0 && statement.IsFuture) {
            statement.Records = ReadFixed();
        }

        // Recalculate the entry balance
        UpdateEntryBalances();
        return statement;
    }

    /// <summary>
    /// Return the number of statements
    /// </summary>
    public int Count => _statements.Count;

    /// <summary>
    /// Retrieve the statement for the specified year and month.
    /// </summary>
    /// <param name="year">Requested year</param>
    /// <param name="month">Requested month</param>
    /// <returns>The statement for the given year and month, or null if none exist</returns>
    public TStatement? Get(int year, int month) {
        return _statements.FirstOrDefault(statement => statement.Year == year && statement.Month == month);
    }

    /// <summary>
    /// Read all statements and compute the entry and exit balances for
    /// each month.
    /// </summary>
    public void ReadAccounts() {
        double entryBalance = _startBalance;
        int currentYear = DateTime.Now.Year;
        int theYear = currentYear;
        int startMonth = 13;
        int[] years = ListYears();

        if (years.Length > 0) {
            if (years[0] < theYear) {
                theYear = years[0];
            }
        }
        while (theYear <= currentYear) {
            int[] months = ListMonths(theYear);
            if (months.Length > 0) {
                if (months[0] < startMonth) {
                    startMonth = months[0];
                }
            }
            if (startMonth == 13) {
                startMonth = 1;
            }
            for (int theMonth = startMonth; theMonth <= 12; theMonth++) {
                List<TRecord> records = ReadMonth(theYear, theMonth);
                TStatement oneMonth = new TStatement(theYear, theMonth, records) {
                    EntryBalance = entryBalance
                };
                if (oneMonth.Records.Count == 0 && oneMonth.IsFuture) {
                    oneMonth.Records = ReadFixed();
                }
                entryBalance = oneMonth.ExitBalance;
                _statements.Add(oneMonth);
            }
            theYear++;
        }
    }

    /// <summary>
    /// Return an array of categorised expenditures
    /// </summary>
    public IEnumerable<TCategory> Categories(int theYear) {

        List<TCategory> list = new();
        foreach (TStatement statement in _statements.Where(statement => statement.Year == theYear)) {
            if (statement.Records.Count == 0 && statement.IsFuture) {
                statement.Records = ReadFixed();
            }
            foreach (TRecord record in statement.Records) {
                string itemName = record.Name;
                double itemValue = record.Value;
                bool found = false;

                foreach (TCategory item in list.Where(item => item.Name == itemName)) {
                    item.Value += Math.Abs(itemValue);
                    found = true;
                    break;
                }
                if (!found) {
                    list.Add(new TCategory {
                        Name = itemName,
                        Value = Math.Abs(itemValue)
                    });
                }
            }
        }
        return list.OrderBy(c => c.Name);
    }

    /// <summary>
    /// Read the fixed incomings and outgoings entries record
    /// </summary>
    /// <returns></returns>
    public List<TRecord> ReadFixed() {
        if (!hasFixedRecords) {
            _fixed = ReadDataFile(Utils.FixedDataFile);
            hasFixedRecords = true;
        }
        return _fixed;
    }

    /// <summary>
    /// Save the fixed expenditure data file.
    /// </summary>
    /// <param name="records">Fixed expenditure records</param>
    public static void SaveFixed(List<TRecord> records) {
        string fileName = Utils.FixedDataFile;
        string backupFile = $"{fileName}.bak";

        if (File.Exists(fileName)) {
            if (File.Exists(backupFile)) {
                File.Delete(backupFile);
            }
            File.Copy(fileName, backupFile);
        }
        using FileStream stream = File.Create(fileName);
        JsonSerializer.Serialize(stream, records, new JsonSerializerOptions {
            WriteIndented = true
        });
    }

    /// <summary>
    /// Return an integer array of all the saved years
    /// in the account data folder
    /// </summary>
    /// <returns>Array of all saved years</returns>
    public static int[] ListYears() {
        string[] dirName = Directory.GetDirectories(Utils.DataFolder, "*", SearchOption.TopDirectoryOnly);
        List<int> years = new List<int>();
        foreach (string name in dirName) {
            DirectoryInfo directory = new DirectoryInfo(name);
            if (int.TryParse(directory.Name, out int year)) {
                years.Add(year);
            }
        }
        return years.OrderBy(y => y).ToArray();
    }

    /// <summary>
    /// Update the entry balance across all statements.
    /// </summary>
    public void UpdateEntryBalances() {
        double entryBalance = _startBalance;

        foreach (TStatement statement in _statements) {
            statement.EntryBalance = entryBalance;
            if (statement.Records.Count == 0 && statement.IsFuture) {
                statement.Records = ReadFixed();
            }
            entryBalance = statement.ExitBalance;
        }
    }

    /// <summary>
    /// Read the records for the specified year and month
    /// </summary>
    /// <param name="theYear"></param>
    /// <param name="theMonth"></param>
    /// <returns>A list of records for the given year and month</returns>
    private static List<TRecord> ReadMonth(int theYear, int theMonth) {
        string fileName = $"{Utils.DataFolder}/{theYear}/{theMonth}";
        return ReadDataFile(fileName);
    }

    /// <summary>
    /// Open the specified data file and retrieve the records. If the file
    /// does not exist, and empty record list is returned.
    /// </summary>
    /// <param name="fileName">Name of file to be read</param>
    /// <returns>A list of records</returns>
    private static List<TRecord> ReadDataFile(string fileName) {
        List<TRecord> records = new List<TRecord>();
        if (File.Exists(fileName)) {
            using FileStream inputFile = File.Open(fileName, FileMode.Open);
            records = JsonSerializer.Deserialize<List<TRecord>>(inputFile) ?? new List<TRecord>();
        }
        return records.OrderBy(r => r.Date).ToList();
    }

    /// <summary>
    /// Return an integer array of all the saved months in the account data folder
    /// for the specified year.
    /// </summary>
    /// <param name="theYear">Year to retrieve</param>
    /// <returns>Array of all saved years</returns>
    private static int[] ListMonths(int theYear) {
        string[] dirName = Directory.GetDirectories($"{Utils.DataFolder}/{theYear}", "*", SearchOption.TopDirectoryOnly);
        List<int> months = new List<int>();
        foreach (string name in dirName) {
            if (int.TryParse(name, out int year)) {
                months.Add(year);
            }
        }
        return months.OrderBy(m => m).ToArray();
    }
}