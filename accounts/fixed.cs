// Accounts
// Fixed expenses management
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

public static class Fixed {

    /// <summary>
    /// Display the list of fixed expenses and allow them to be
    /// edited in the form.
    /// </summary>
    /// <param name="account">Account to view</param>
    public static void Show(TAccount account) {
        const string sectionName = "Fixed";

        Utils.ShowTitle("Fixed Incomings and Outgoings");

        TForm form = new(TForm.CanPrint);
        int rowIndex = TForm.FirstRow;

        // Display instructions line
        form.AddLabel(rowIndex, 4, "Enter name and amount of incoming and outgoings. Use negative values for outgoings.");
        form.AddLabel(rowIndex + 1, 4, "For the Type, 0 = specific day, 1 = repeats every week on a day, 2 = nearest weekday.");
        form.AddLabel(rowIndex + 2, 4, "For Type 0 or 2, enter the day of the month when the amount is debited or credited.");
        form.AddLabel(rowIndex + 3, 4, "For Type 1, set Day to 0 for Sunday through to 6 for Saturday.");
        form.AddLabel(rowIndex + 4, 4, "Note: changes here only affect future months.");
        rowIndex += 6;

        // Show header
        form.AddLabel(rowIndex, 4, "_Description");
        form.AddLabel(rowIndex, 28, 10, TAlign.Right, "_Amount");
        form.AddLabel(rowIndex, 44, "_Type");
        form.AddLabel(rowIndex, 54, "_Day");
        rowIndex += 2;

        // Show current fixed records
        //List<TStanding> fixedRecords = account.Fixed;
        form.BeginSection(sectionName);

        foreach (TStanding record in account.Fixed) {
            form.AddText(rowIndex, 4, 20, record.Name);
            form.AddCurrency(rowIndex, 28, record.Value);
            form.AddNumeric(rowIndex, 44, 2, (int)record.Frequency, "");
            form.AddNumeric(rowIndex, 54, 2, record.Day, "");
            rowIndex++;
        }

        // Add a blank row for new entries
        form.AddText(rowIndex, 4, 20, "");
        form.AddCurrency(rowIndex, 28, 0);
        form.AddNumeric(rowIndex, 44, 1, 1, "");
        form.AddNumeric(rowIndex, 54, 2, 1, "");
        form.EndSection(sectionName);
        rowIndex += 2;

        // Add a total row
        form.AddLabel(rowIndex, 4, "Total");
        form.AddLabel(rowIndex, 28, 10, TAlign.Right, "");

        // Start the editor
        TDisplayFormResult result;
        do {

            // Calculate the current total
            double total = 0;
            int totalIndex = form.Count - 1;
            for (int index = 0; index < form.Count; index++) {
                if (form.Fields(index).FieldType == TFieldType.Currency) {
                    double value = Convert.ToDouble(form.Fields(index).Value);
                    total += value;
                }
            }
            form.Fields(totalIndex).Value = total.ToString("F2");

            // Activate the picker
            result = form.DisplayForm();

            // Insert a row?
            if (result == TDisplayFormResult.Insert) {
                int insertIndex = form.SelectedItem;

                // Find the end of the current row
                while (form.Fields(insertIndex).FieldType != TFieldType.Numeric) {
                    ++insertIndex;
                }

                form.SelectedItem = insertIndex + 1;
                rowIndex = form.Fields(insertIndex).Row + 1;
                form.InsertText(insertIndex + 1, rowIndex, 4, 20, "");
                form.InsertCurrency(insertIndex + 2, rowIndex, 28, 0);
                form.InsertNumeric(insertIndex + 3, rowIndex, 44, 1, 1, "");
                form.InsertNumeric(insertIndex + 4, rowIndex, 54, 2, 1, "");
                insertIndex += 5;

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
                while (form.Fields(deleteIndex).FieldType != TFieldType.Text) {
                    --deleteIndex;
                }

                // Don't delete the last row
                if (!(form.Fields(deleteIndex - 1).IsSection && form.Fields(deleteIndex + 4).IsSection)) {
                    if (form.Fields(deleteIndex + 4).IsSection) {
                        form.SelectedItem = deleteIndex - 4;
                    }
                    form.Delete(deleteIndex);
                    form.Delete(deleteIndex);
                    form.Delete(deleteIndex);
                    form.Delete(deleteIndex);

                    // Adjust row positions of rest of form
                    while (deleteIndex < form.Count) {
                        --form.Fields(deleteIndex).Row;
                        deleteIndex++;
                    }
                }
            }

            // Cancel?
            if (result == TDisplayFormResult.Cancel) {
                if (!form.IsModified || Utils.AskExit()) {
                    break;
                }
            }
        } while (result != TDisplayFormResult.Save);

        // Save the results
        if (result == TDisplayFormResult.Save) {
            List<TStanding> fixedRecords = [];
            int index = form.FindSection(sectionName);
            do {
                string theName = form.Fields(index).Value;
                double theValue = Convert.ToDouble(form.Fields(index + 1).Value);
                TFrequency frequency = (TFrequency)Convert.ToInt32(form.Fields(index + 2).Value);
                int day = Convert.ToInt32(form.Fields(index + 3).Value);

                if (!string.IsNullOrEmpty(theName)) {
                    fixedRecords.Add(new TStanding(theName, theValue, frequency, day));
                }

                index += 4;
            } while (!form.Fields(index).IsSection);
            account.SaveFixed(fixedRecords);

            // Recalculate entry balance so changes here flow to the next months.
            account.UpdateEntryBalances();
        }
    }
}