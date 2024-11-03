// JCalc
// Commands
//
// Authors:
//  Steve Palmer
//
// Copyright (C) 2024 Steve Palmer
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

using JCalc.Resources;

namespace JCalc;

/// <summary>
/// List of command IDs
/// </summary>
public enum CommandMapID {
    NONE,
    MAIN,
    WORKSHEET,
    RANGE,
    FILE,
    DATA,
    COLUMN_WIDTH,
    DATES,
    FORMAT,
    LABELPREFIX
}

/// <summary>
/// A single entry in a command map
/// </summary>
public class CommandMapEntry {

    /// <summary>
    /// Command name
    /// </summary>
    public string Name { get; init; } = "";

    /// <summary>
    /// Associated command ID
    /// </summary>
    public KeyCommand CommandId { get; init; }

    /// <summary>
    /// ID of sub-command map
    /// </summary>
    public CommandMapID SubCommandId { get; init; } = CommandMapID.NONE;

    /// <summary>
    /// Description of command
    /// </summary>
    public string Description { get; init; } = "";
}

/// <summary>
/// Mapping of command names and their IDs
/// </summary>
public class CommandMap {

    /// <summary>
    /// Command map ID
    /// </summary>
    public CommandMapID ID { get; init; }

    /// <summary>
    /// List of commands in this map
    /// </summary>
    public CommandMapEntry[] Commands { get; init; } = [];
}

public static class Commands {

    /// <summary>
    /// Retrieve the command map by its ID
    /// </summary>
    /// <param name="id">ID of command map required</param>
    /// <returns>Command map for that ID</returns>
    public static CommandMap CommandMapForID(CommandMapID id) => Maps.First(m => m.ID == id);

    /// <summary>
    /// All command maps
    /// </summary>
    private static readonly CommandMap[] Maps = [
        new() {
            ID = CommandMapID.MAIN,
            Commands = [
                new CommandMapEntry { Name = Calc.Worksheet, SubCommandId = CommandMapID.WORKSHEET },
                new CommandMapEntry { Name = Calc.Range, SubCommandId = CommandMapID.RANGE },
                new CommandMapEntry { Name = Calc.Copy, CommandId = KeyCommand.KC_COPY, Description = "Copy a range of cells"},
                new CommandMapEntry { Name = Calc.Move, CommandId = KeyCommand.KC_MOVE, Description = "Move a cell or range of cells"},
                new CommandMapEntry { Name = Calc.File, CommandId = KeyCommand.KC_FILE, SubCommandId = CommandMapID.FILE },
                new CommandMapEntry { Name = Calc.Print, CommandId = KeyCommand.KC_PRINT, Description = "Output a range to the printer or print file" },
                new CommandMapEntry { Name = Calc.Graph, CommandId = KeyCommand.KC_GRAPH, Description = "Create a graph"},
                new CommandMapEntry { Name = Calc.Data, SubCommandId = CommandMapID.DATA },
                new CommandMapEntry { Name = Calc.Quit, CommandId = KeyCommand.KC_QUIT, Description = "Quit Calc"}
            ]
        },
        new() {
            ID = CommandMapID.WORKSHEET,
            Commands = [
                new CommandMapEntry { Name = Calc.WorksheetGlobal, CommandId = KeyCommand.KC_GLOBAL, Description = "Set worksheet settings" },
                new CommandMapEntry { Name = Calc.WorksheetInsert, CommandId = KeyCommand.KC_INSERT, Description = "Insert blank column(s) or row(s)"},
                new CommandMapEntry { Name = Calc.WorksheetDelete, CommandId = KeyCommand.KC_DELETE, Description = "Delete entire column(s) or row(s)" },
                new CommandMapEntry { Name = Calc.WorksheetColumnWidth, SubCommandId = CommandMapID.COLUMN_WIDTH, Description = "Set display width of the current column"},
                new CommandMapEntry { Name = Calc.WorksheetErase, CommandId = KeyCommand.KC_ERASE, Description = "Erase the entire worksheet" },
                new CommandMapEntry { Name = Calc.WorksheetTitles, CommandId = KeyCommand.KC_TITLES, Description = "Set horizontal or vertical titles" },
                new CommandMapEntry { Name = Calc.WorksheetWindow, CommandId = KeyCommand.KC_WINDOW, Description = "Set split-screen and synchronised scrolling" },
                new CommandMapEntry { Name = Calc.WorksheetStatus, CommandId = KeyCommand.KC_STATUS, Description = "Display worksheet settings" }
            ]
        },
        new() {
            ID = CommandMapID.RANGE,
            Commands = [
                new CommandMapEntry { Name = Calc.RangeFormat, SubCommandId = CommandMapID.FORMAT, Description = "Format a cell or range of cells" },
                new CommandMapEntry { Name = Calc.RangeLabelPrefix, SubCommandId = CommandMapID.LABELPREFIX, Description = "Align a label or range of labels (Left, Right or Centre)" },
                new CommandMapEntry { Name = Calc.RangeErase, CommandId = KeyCommand.KC_ERASE },
                new CommandMapEntry { Name = Calc.RangeName, CommandId = KeyCommand.KC_NAME },
                new CommandMapEntry { Name = Calc.RangeJustify, CommandId = KeyCommand.KC_JUSTIFY },
                new CommandMapEntry { Name = Calc.RangeProtect, CommandId = KeyCommand.KC_PROTECT },
                new CommandMapEntry { Name = Calc.RangeUnprotect, CommandId = KeyCommand.KC_UNPROTECT },
                new CommandMapEntry { Name = Calc.RangeInput, CommandId = KeyCommand.KC_INPUT }
            ]
        },
        new () {
            ID = CommandMapID.FORMAT,
            Commands = [
                new CommandMapEntry { Name = Calc.FormatFixed, CommandId = KeyCommand.KC_FORMAT_FIXED, Description = "Fixed number of decimal places (x.xx)" },
                new CommandMapEntry { Name = Calc.FormatScientific, CommandId = KeyCommand.KC_FORMAT_SCI, Description = "Exponential format (x.xxE+xx)" },
                new CommandMapEntry { Name = Calc.FormatCurrency, CommandId = KeyCommand.KC_FORMAT_CURRENCY, Description = "Currency format (£x,xxx.xx)" },
                new CommandMapEntry { Name = Calc.FormatCommas, CommandId = KeyCommand.KC_FORMAT_COMMAS, Description = "Commas inserted; negative values parenthised (x,xxx.xx)" },
                new CommandMapEntry { Name = Calc.FormatGeneral, CommandId = KeyCommand.KC_FORMAT_GENERAL, Description = "Standard format (x.xx or x.xxExx)" },
                new CommandMapEntry { Name = Calc.FormatBar, CommandId = KeyCommand.KC_FORMAT_BAR, Description = "Horizontal bar graph format (+++ or ---)" },
                new CommandMapEntry { Name = Calc.FormatPercent, CommandId = KeyCommand.KC_FORMAT_PERCENT, Description = "Percent format (x.xx%)" },
                new CommandMapEntry { Name = Calc.FormatDate, SubCommandId = CommandMapID.DATES, Description = "Date formats" },
                new CommandMapEntry { Name = Calc.FormatText, CommandId = KeyCommand.KC_FORMAT_TEXT, Description = "Display formula instead of value" },
                new CommandMapEntry { Name = Calc.FormatReset, CommandId = KeyCommand.KC_FORMAT_RESET, Description = "Use global format" }
            ]
        },
        new () {
            ID = CommandMapID.LABELPREFIX,
            Commands = [
                new CommandMapEntry { Name = Calc.LabelPrefixLeft, CommandId = KeyCommand.KC_ALIGN_LEFT, Description = "Align labels with left edges of cells" },
                new CommandMapEntry { Name = Calc.LabelPrefixRight, CommandId = KeyCommand.KC_ALIGN_RIGHT, Description = "Align labels with right edges of cells" },
                new CommandMapEntry { Name = Calc.LabelPrefixCentre, CommandId = KeyCommand.KC_ALIGN_CENTRE, Description = "Centre labels in cells" }
            ]
        },
        new () {
            ID = CommandMapID.DATES,
            Commands = [
                new CommandMapEntry { Name = Calc.DatesDMY, CommandId = KeyCommand.KC_DATE_DMY, Description = "Day-Month-Year" },
                new CommandMapEntry { Name = Calc.DatesDM, CommandId = KeyCommand.KC_DATE_DM, Description = "Day-Month" },
                new CommandMapEntry { Name = Calc.DatesMY, CommandId = KeyCommand.KC_DATE_MY, Description = "Month-Year" }
            ]
        },
        new() {
            ID = CommandMapID.FILE,
            Commands = [
                new CommandMapEntry { Name = Calc.FileRetrieve, CommandId = KeyCommand.KC_RETRIEVE },
                new CommandMapEntry { Name = Calc.FileSave, CommandId = KeyCommand.KC_SAVE },
                new CommandMapEntry { Name = Calc.FileCombine, CommandId = KeyCommand.KC_COMBINE },
                new CommandMapEntry { Name = Calc.FileXtract, CommandId = KeyCommand.KC_XTRACT },
                new CommandMapEntry { Name = Calc.FileErase, CommandId = KeyCommand.KC_ERASE },
                new CommandMapEntry { Name = Calc.FileList, CommandId = KeyCommand.KC_LIST },
                new CommandMapEntry { Name = Calc.Fileimport, CommandId = KeyCommand.KC_IMPORT },
                new CommandMapEntry { Name = Calc.FileDirectory, CommandId = KeyCommand.KC_DIRECTORY }
            ]
        },
        new() {
            ID = CommandMapID.DATA,
            Commands = [
                new CommandMapEntry { Name = Calc.DataFill, CommandId = KeyCommand.KC_FILL },
                new CommandMapEntry { Name = Calc.DataTable, CommandId = KeyCommand.KC_TABLE },
                new CommandMapEntry { Name = Calc.DataSort, CommandId = KeyCommand.KC_SORT },
                new CommandMapEntry { Name = Calc.DataQuery, CommandId = KeyCommand.KC_QUERY },
                new CommandMapEntry { Name = Calc.DataDistribution, CommandId = KeyCommand.KC_DISTRIBUTION }
            ]
        },
        new () {
            ID = CommandMapID.COLUMN_WIDTH,
            Commands = [
                new CommandMapEntry { Name = Calc.ColumnWidthSet, CommandId = KeyCommand.KC_SET_COLUMN_WIDTH, Description = "Set width of current column" },
                new CommandMapEntry { Name = Calc.ColumnWidthReset, CommandId = KeyCommand.KC_RESET_COLUMN_WIDTH, Description = "Use global column width" }
            ]
        }
    ];
}
