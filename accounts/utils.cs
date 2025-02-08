// Accounts
// Utility functions
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

public static class Utils {

    public const ConsoleColor ReverseColour = ConsoleColor.Yellow;
    public const ConsoleColor BackgroundColour = ConsoleColor.Black;
    public const ConsoleColor ForegroundColour = ConsoleColor.White;
    public const ConsoleColor TitleColour = ConsoleColor.Cyan;

    /// <summary>
    /// Default name of the data folder in the home directory.
    /// </summary>
    /// <value>File name</value>
    public static string DataFolder => "data";

    /// <summary>
    /// Return the name of the fixed expenditure data file.
    /// </summary>
    /// <value>File name</value>
    public static string FixedDataFile => $"{DataFolder}/fixed";

    /// <summary>
    /// Show the program title, version and section name at the top of the
    /// screen.
    /// </summary>
    /// <param name="sectionName">Name to be shown in the title</param>
    public static void ShowTitle(string sectionName) {
        ScreenClear(0, 1, Terminal.Height - 1, Terminal.Width - 1);
        PrintBar(0, $"Accounts {AssemblySupport.AssemblyVersion}     {sectionName.ToUpper()}");
    }

    /// <summary>
    /// Display the specified text in the footer bar.
    /// </summary>
    /// <param name="text">Text to display</param>
    public static void ShowFooter(string text) {
        int windowBottom = Terminal.Height - 1;
        PrintBar(windowBottom, text);
    }

    /// <summary>
    /// Display a prompt asking whether or not to save changes.
    /// </summary>
    /// <returns>True if we exit, false otherwise</returns>
    public static bool AskExit() {
        int windowBottom = Terminal.Height - 1;
        PrintBar(windowBottom, "Exit without saving? Are you sure? (Y/N)");
        return char.ToUpper(Console.ReadKey(true).KeyChar) == 'Y';
    }

    /// <summary>
    /// Clear an area of the screen.
    /// </summary>
    /// <param name="topRow">Zero based top row of area</param>
    /// <param name="leftColumn">Zero based left column of area</param>
    /// <param name="bottomRow">Zero based bottom row of area</param>
    /// <param name="rightColumn">Zero based right column of area</param>
    public static void ScreenClear(int topRow, int leftColumn, int bottomRow, int rightColumn) {
        int width = rightColumn - leftColumn + 1;
        for (int index = topRow; index <= bottomRow; index++) {
            Terminal.Write(leftColumn, index, width, BackgroundColour, ForegroundColour, " ");
        }
    }

    /// <summary>
    /// Draw a bar on the terminal in the reverse colour with the
    /// specified text.
    /// </summary>
    /// <param name="y">Row at which bar is to be drawn</param>
    /// <param name="barText">Text to be displayed</param>
    private static void PrintBar(int y, string barText) {
        Terminal.Write(0, y, Terminal.Width, ReverseColour, BackgroundColour, $" {barText}");
    }
}