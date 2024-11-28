// Accounts
// Monthly record of expenses
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

public static class Month {

    /// <summary>
    /// Show the statement for the current month
    /// </summary>
    /// <param name="account">Account to view</param>
    public static void CurrentMonth(TAccount account) {
        Show(account, DateTime.Now.Year, DateTime.Now.Month);
    }

    /// <summary>
    /// Display statement for the specified year and month.
    /// </summary>
    /// <param name="account">Account to view</param>
    /// <param name="thisYear">Year to show</param>
    /// <param name="thisMonth">Month to show</param>
    public static void Show(TAccount account, int thisYear, int thisMonth) {
        TForm form = new TForm(TForm.CanPrint);
        int rowIndex = 4;
        TDisplayFormResult result;
        int selectedItem = -1;
        int insertDay = DateTime.Now.Day;
        const string sectionName = "Fixed";

        Utils.ShowTitle($"Statement for {TDate.MonthName(thisMonth)} {thisYear}");

        // Get the statement for this year and month
        TStatement? statement = account.Get(thisYear, thisMonth);
        if (statement == null) {
            TForm errorForm = new TForm(TForm.Simple);
            errorForm.AddLabel(rowIndex, 4, $"Unable to find a statement for {TDate.MonthName(thisMonth)} {thisYear}.");
            errorForm.DisplayForm();
            return;
        }

        // Display instructions line
        form.AddLabel(rowIndex, 4, "Here you can add and remove entries for this month. Use negative values for outgoings.");
        rowIndex += 2;

        // For prior months, insertDay is the end of the month
        if (thisYear < DateTime.Now.Year || (thisYear == DateTime.Now.Year && thisMonth < DateTime.Now.Month)) {
            insertDay = DateTime.DaysInMonth(thisYear, thisMonth);
        }

        // Show header
        form.AddLabel(rowIndex, 4, "_Date");
        form.AddLabel(rowIndex, 12, "_Description");
        form.AddLabel(rowIndex, 36, 10, TAlign.Right, "_Amount");
        rowIndex += 2;

        // Show entry balance
        form.AddLabel(rowIndex, 12, "Entry Balance");
        form.AddLabel(rowIndex, 36, 10, TAlign.Right, statement.EntryBalance.ToString("F2"));
        rowIndex += 2;

        form.BeginSection(sectionName);
        foreach (TRecord record in statement.Records) {
            if (record.Date.Day > insertDay && selectedItem == -1) {
                form.AddNumeric(rowIndex, 4, 6, DateTime.Now.Day, "{0}-" + TDate.ShortMonthName(thisMonth));
                form.AddText(rowIndex, 12, 20, "");
                form.AddCurrency(rowIndex, 36, 0);
                selectedItem = form.Count - 2;
                rowIndex++;
            }
            form.AddNumeric(rowIndex, 4, 6, record.Date.Day, "{0}-" + TDate.ShortMonthName(thisMonth));
            form.AddText(rowIndex, 12, 20, record.Name);
            form.AddCurrency(rowIndex, 36, record.Value);
            rowIndex++;
        }

        // Set selection to the text field
        if (selectedItem == -1) {
            form.AddNumeric(rowIndex, 4, 6, insertDay, "{0}-" + TDate.ShortMonthName(thisMonth));
            form.AddText(rowIndex, 12, 20, "");
            form.AddCurrency(rowIndex, 36, 0);
            selectedItem = form.Count - 2;
        }
        form.EndSection(sectionName);
        form.SelectedItem = selectedItem;

        // Show overspend balance
        rowIndex += 2;
        form.AddLabel(rowIndex, 12, "Overspend");
        form.AddLabel(rowIndex, 36, 10, TAlign.Right, "");
        rowIndex += 2;

        // Show exit balance at the end
        form.AddLabel(rowIndex, 12, "Exit Balance");
        form.AddLabel(rowIndex, 36, 10, TAlign.Right, "");

        // Start the loop
        do {

            // Calculate the current total
            double total = statement.EntryBalance;
            int totalIndex = form.Count - 1;
            int overspendIndex = totalIndex - 2;

            for (int index = 0; index < form.Count; index++) {
                if (form.Fields(index).FieldType == TFieldType.Currency) {
                    double value = Convert.ToDouble(form.Fields(index).Value);
                    total += value;
                }
            }
            form.Fields(totalIndex).Value = total.ToString("F2");

            // Show overspend
            double overspend = Math.Abs(Math.Max(0, statement.EntryBalance - total));
            form.Fields(overspendIndex).Value = overspend.ToString("F2");

            // Activate the picker
            result = form.DisplayForm();

            // Insert a row?
            if (result == TDisplayFormResult.Insert) {
                int insertIndex = form.SelectedItem;
                while (form.Fields(insertIndex).FieldType != TFieldType.Currency) {
                    insertIndex++;
                }

                form.SelectedItem = insertIndex + 1;
                rowIndex = form.Fields(insertIndex).Row + 1;
                form.InsertNumeric(insertIndex + 1, rowIndex, 4, 6, DateTime.Now.Day, "{0}-" + TDate.ShortMonthName(thisMonth));
                form.InsertText(insertIndex + 2, rowIndex, 12, 20, "");
                form.InsertCurrency(insertIndex + 3, rowIndex, 36, 0);
                insertIndex += 4;

                // Adjust row positions of rest of form
                while (insertIndex < form.Count) {
                    ++form.Fields(insertIndex).Row;
                    insertIndex++;
                }
            }

            // Delete current row?
            if (result == TDisplayFormResult.Deleted) {
                int deleteIndex = form.SelectedItem;

                // Find the start of the current row
                while (form.Fields(deleteIndex).FieldType != TFieldType.Numeric) {
                    --deleteIndex;
                }

                // Don't delete the last row
                if (!(form.Fields(deleteIndex - 1).IsSection && form.Fields(deleteIndex + 3).IsSection)) {
                    if (form.Fields(deleteIndex + 3).IsSection) {
                        form.SelectedItem = deleteIndex - 3;
                    }
                    form.DeleteField(deleteIndex);
                    form.DeleteField(deleteIndex);
                    form.DeleteField(deleteIndex);

                    // Adjust row positions of rest of form
                    while (deleteIndex < form.Count) {
                        --form.Fields(deleteIndex).Row;
                        deleteIndex++;
                    }
                }
            }

            // Cancel
            if (result == TDisplayFormResult.Cancel) {
                if (!form.IsModified || Utils.AskExit()) {
                    break;
                }
            }
        } while (result != TDisplayFormResult.Save);

        // Save the results
        if (result == TDisplayFormResult.Save) {
            statement.Records = [];
            int index = form.FindSection(sectionName);
            do {
                int theDay = Convert.ToInt32(form.Fields(index).Value);
                string theName = form.Fields(index + 1).Value;
                double theValue = Convert.ToDouble(form.Fields(index + 2).Value);
                TDate theDate = new TDate(thisYear, thisMonth, theDay);

                if (!string.IsNullOrEmpty(theName)) {
                    statement.Records.Add(new TRecord(theName, theValue, theDate));
                }
                index += 3;

            } while (!form.Fields(index).IsSection);
            statement.Save();

            // Recalculate entry balance so changes here flow to the next months.
            account.UpdateEntryBalances();
        }
    }
}