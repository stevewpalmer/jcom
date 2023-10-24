// Accounts
// Main program
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

internal static class Program {

    // Main menu displayed on program startup.
    private static readonly TMenuItem[] mainMenu = {
        new('C', "Current Month", "Display statement for the current Month"),
        new('A', "Annual", "Show the annual summary"),
        new('F', "Fixed", "Edit fixed income and outgoings"),
        new('L', "Calendar", "Show the statements calendar"),
        new('S', "Search", "Search statements for payments"),
        new('E', "Exit", "Exit the Accounts program")
    };

    private static void Main() {
        TAccount account = new();
        TAccount.Init();
        account.ReadAccounts();

        int selectedItem = 0;
        bool exit = false;

        Terminal.Open();
        while (!exit) {
            TForm form = new TForm(TForm.Simple);

            Utils.ShowTitle("Main Menu");

            form.SelectedItem = selectedItem;

            for (int index = 0; index < mainMenu.Length; index++) {
                int screenWidth = Terminal.Width;

                string itemString = (" " + char.ToUpper(mainMenu[index].ShortcutKey) + Utils.Space(6))[..6];
                itemString += (" " + mainMenu[index].Name.ToUpper() + Utils.Space(16))[..16];
                itemString += (" -  " + mainMenu[index].Title + Utils.Space(50))[..50];

                int rowIndex = 4 + index * 2;
                int columnIndex = (screenWidth - itemString.Length) / 2;

                form.AddOption(rowIndex, columnIndex, itemString, mainMenu[index].ShortcutKey);
            }

            TDisplayFormResult result = form.DisplayForm();
            selectedItem = form.SelectedItem;

            if (result == TDisplayFormResult.Cancel) {
                exit = true;
            }
            else if (result == TDisplayFormResult.Pick) {
                switch (form.Fields(form.SelectedItem).Ch) {
                    case 'C':
                        Month.CurrentMonth(account);
                        break;
                    case 'E':
                        exit = true;
                        break;
                    case 'A':
                        Year.CurrentYear(account);
                        break;
                    case 'F':
                        Fixed.Show(account);
                        break;
                    case 'L':
                        Calendar.Show(account);
                        break;
                    case 'S':
                        Search.SearchStatements(account);
                        break;
                }
            }
        }
        Terminal.Close();
    }
}