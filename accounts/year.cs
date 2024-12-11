// Accounts
// Year summary of expenses
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

namespace JAccounts;

public static class Year {

    /// <summary>
    /// Show the summary for the current year.
    /// </summary>
    /// <param name="account">Account to view</param>
    public static void CurrentYear(TAccount account) {
        ShowYear(account, DateTime.Now.Year);
    }

    /// <summary>
    /// Show the summary for the given year.
    /// </summary>
    /// <param name="account">Account to view</param>
    /// <param name="thisYear">Year to show</param>
    public static void ShowYear(TAccount account, int thisYear) {

        Utils.ShowTitle($"Summary for {thisYear}");
        IEnumerable<TCategory> list = account.Categories(thisYear);

        TForm form = new(TForm.Simple + TForm.CanPrint);
        int rowIndex = 4;

        // Show explanation if the summary is for the current or future years
        if (thisYear >= DateTime.Now.Year) {
            form.AddLabel(rowIndex, 4, "The summary displayed is the projected expenditure for this year based");
            form.AddLabel(rowIndex + 1, 4, "on current fixed outgoings and incomes and statements to date.");
            rowIndex += 3;
        }

        // Entry and exit balance for year
        TStatement? statement = account.Get(thisYear, 1);
        if (statement != null) {
            form.AddLabel(rowIndex, 4, $"Entry Balance for {thisYear}");
            form.AddLabel(rowIndex, 30, 10, TAlign.Right, statement.EntryBalance.ToString("F2"));
            form.AddLabel(rowIndex + 1, 4, $"Exit Balance for {thisYear}");
            form.AddLabel(rowIndex + 1, 30, 10, TAlign.Right, statement.ExitBalance.ToString("F2"));
            rowIndex += 3;
        }

        // Headers
        form.AddLabel(rowIndex, 4, "_Category");
        form.AddLabel(rowIndex, 30, 10, TAlign.Right, "_Total");
        rowIndex += 2;

        // Summary of expenses
        foreach (TCategory category in list) {
            form.AddLabel(rowIndex, 4, category.Name);
            form.AddLabel(rowIndex, 30, 10, TAlign.Right, category.Value.ToString("F2"));
            rowIndex++;
        }

        // Display the expenses.
        TDisplayFormResult result;
        do {
            result = form.DisplayForm();
        } while (result != TDisplayFormResult.Cancel);
    }
}