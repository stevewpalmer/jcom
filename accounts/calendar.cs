// Accounts
// Calendar view
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

using JComLib;

namespace JAccounts;

public static class Calendar {

    /// <summary>
    /// Show a calendar view of the specified account.
    /// </summary>
    /// <param name="account">Account to view</param>
    public static void Show(TAccount account) {
        TDisplayFormResult result;
        int selectedItem = 0;
        do {
            Utils.ShowTitle("Calendar");

            TForm form = new(TForm.Simple);
            int rowIndex = 4;

            // Display instructions line.
            form.AddLabel(rowIndex, 4, "_Choose a year to see the summary for that year. Choose a month to");
            form.AddLabel(rowIndex + 1, 4, "_view and edit the accounts for that month.");
            rowIndex += 3;

            int[] years = TAccount.ListYears();

            foreach (int year in years) {
                int columnIndex = 5;
                object yearData = new TDate(year, 0, 0);
                form.AddOption(rowIndex, columnIndex, year.ToString(), yearData);

                for (int monthIndex = 1; monthIndex <= 12; monthIndex++) {
                    string title = TDate.MonthName(monthIndex);
                    int screenWidth = Terminal.Width;

                    // Handle wrapping
                    if (columnIndex + title.Length + 2 > screenWidth) {
                        columnIndex = 5;
                        ++rowIndex;
                    }
                    object monthData = new TDate(year, monthIndex, 0);
                    form.AddOption(rowIndex + 2, columnIndex, title, monthData);
                    columnIndex += title.Length + 2;
                }
                rowIndex += 4;
            }
            form.SelectedItem = selectedItem;
            result = form.DisplayForm();
            if (result == TDisplayFormResult.Pick) {
                selectedItem = form.SelectedItem;
                if (form.Fields(selectedItem).Data is TDate date) {
                    if (date.Month != 0) {
                        Month.Show(account, date.Year, date.Month);
                    }
                    else {
                        Year.ShowYear(account, date.Year);
                    }
                }
            }
        } while (result != TDisplayFormResult.Cancel);
    }
}