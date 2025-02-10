// Accounts
// Statement searching
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

using System.Globalization;

namespace JAccounts;

public static class Search {

    /// <summary>
    /// Search all statements
    /// </summary>
    /// <param name="account">Account to search</param>
    public static void SearchStatements(TAccount account) {
        TForm form = new(TForm.Simple);
        TDisplayFormResult result;

        Utils.ShowTitle("Search Statements");

        string searchText = "";
        do {
            int rowIndex = TForm.FirstRow;

            form.Clear();
            form.AddLabel(rowIndex, 4, "Enter payment or amount to search for:");
            form.AddText(rowIndex + 2, 4, 50, "");
            int searchFieldIndex = form.Count - 1;

            if (!string.IsNullOrEmpty(searchText)) {
                bool hasHeader = false;
                double total = 0;
                rowIndex += 4;

                for (int index = 0; index < account.Count; index++) {
                    TStatement statement = account.Get(index);
                    foreach (TRecord record in statement.Records) {
                        string theName = record.Name;
                        double theValue = record.Value;
                        TDate theDate = record.Date;

                        if (theName.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                            theValue.ToString(CultureInfo.InvariantCulture).Contains(searchText)) {
                            if (!hasHeader) {
                                form.AddLabel(rowIndex, 4, 6, TAlign.Left, "_Date");
                                form.AddLabel(rowIndex, 12, 20, TAlign.Left, "_Description");
                                form.AddLabel(rowIndex, 36, 10, TAlign.Right, "_Amount");
                                rowIndex += 2;
                                hasHeader = true;
                            }
                            form.AddLabel(rowIndex, 4, 6, TAlign.Left, $"{theDate.Day}-{TDate.ShortMonthName(statement.Month)}");
                            form.AddLabel(rowIndex, 12, 20, TAlign.Left, theName);
                            form.AddLabel(rowIndex, 36, 10, TAlign.Right, theValue.ToString("F2"));
                            rowIndex += 1;

                            total += theValue;
                        }
                    }
                }

                // Show total if we found any results
                if (hasHeader) {
                    form.AddLabel(rowIndex + 1, 4, "_Total");
                    form.AddLabel(rowIndex + 1, 36, 10, TAlign.Right, total.ToString("F2"));
                    ++rowIndex;
                }
                else {
                    form.AddLabel(rowIndex, 4, $"No results found for {searchText}");
                }
                form.Fields(searchFieldIndex).Value = "";
            }

            form.SelectedItem = searchFieldIndex;
            result = form.DisplayForm();
            if (result == TDisplayFormResult.Pick) {
                searchText = form.Fields(searchFieldIndex).Value.Trim();
            }

        } while (result != TDisplayFormResult.Cancel);
    }
}