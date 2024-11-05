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
    ALIGN,
    COLUMN_WIDTH,
    DATA,
    DATES,
    DELETE,
    FILE,
    FORMAT,
    INSERT,
    MAIN,
    RANGE,
    WORKSHEET
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
                new CommandMapEntry { Name = Calc.File, SubCommandId = CommandMapID.FILE },
                new CommandMapEntry { Name = Calc.Print, CommandId = KeyCommand.KC_PRINT, Description = "Output a range to the printer or print file" },
                new CommandMapEntry { Name = Calc.Graph, CommandId = KeyCommand.KC_GRAPH, Description = "Create a graph"},
                new CommandMapEntry { Name = Calc.Data, SubCommandId = CommandMapID.DATA },
                new CommandMapEntry { Name = Calc.Quit, CommandId = KeyCommand.KC_QUIT, Description = Calc.QuitCalc}
            ]
        },
        new() {
            ID = CommandMapID.WORKSHEET,
            Commands = [
                new CommandMapEntry { Name = Calc.WorksheetGlobal, CommandId = KeyCommand.KC_WORKSHEET_GLOBAL, Description = "Set worksheet settings" },
                new CommandMapEntry { Name = Calc.WorksheetInsert, SubCommandId = CommandMapID.INSERT, Description = Calc.SheetInsertDescription},
                new CommandMapEntry { Name = Calc.WorksheetDelete, SubCommandId = CommandMapID.DELETE, Description = Calc.SheetDeleteDescription },
                new CommandMapEntry { Name = Calc.WorksheetColumnWidth, SubCommandId = CommandMapID.COLUMN_WIDTH, Description = Calc.SheetColumnWidthDescription},
                new CommandMapEntry { Name = Calc.WorksheetErase, CommandId = KeyCommand.KC_WORKSHEET_ERASE, Description = "Erase the entire worksheet" },
                new CommandMapEntry { Name = Calc.WorksheetTitles, CommandId = KeyCommand.KC_WORKSHEET_TITLES, Description = "Set horizontal or vertical titles" },
                new CommandMapEntry { Name = Calc.WorksheetWindow, CommandId = KeyCommand.KC_WORKSHEET_WINDOW, Description = "Set split-screen and synchronised scrolling" },
                new CommandMapEntry { Name = Calc.WorksheetStatus, CommandId = KeyCommand.KC_WORKSHEET_STATUS, Description = "Display worksheet settings" }
            ]
        },
        new() {
            ID = CommandMapID.INSERT,
            Commands = [
                new CommandMapEntry { Name = Calc.Column, CommandId = KeyCommand.KC_INSERT_COLUMN, Description = Calc.InsertColumnDescription },
                new CommandMapEntry { Name = Calc.Row, CommandId = KeyCommand.KC_INSERT_ROW, Description = Calc.InsertRowDescription }
            ]
        },
        new() {
            ID = CommandMapID.DELETE,
            Commands = [
                new CommandMapEntry { Name = Calc.Column, CommandId = KeyCommand.KC_DELETE_COLUMN, Description = "Delete column at cursor" },
                new CommandMapEntry { Name = Calc.Row, CommandId = KeyCommand.KC_DELETE_ROW, Description = "Delete row at cursor" }
            ]
        },
        new() {
            ID = CommandMapID.RANGE,
            Commands = [
                new CommandMapEntry { Name = Calc.RangeFormat, SubCommandId = CommandMapID.FORMAT, Description = Calc.RangeFormatDescription },
                new CommandMapEntry { Name = Calc.RangeAlign, SubCommandId = CommandMapID.ALIGN, Description = Calc.RangeAlignDescription },
                new CommandMapEntry { Name = Calc.RangeErase, CommandId = KeyCommand.KC_RANGE_ERASE },
                new CommandMapEntry { Name = Calc.RangeName, CommandId = KeyCommand.KC_RANGE_NAME },
                new CommandMapEntry { Name = Calc.RangeJustify, CommandId = KeyCommand.KC_RANGE_JUSTIFY },
                new CommandMapEntry { Name = Calc.RangeProtect, CommandId = KeyCommand.KC_RANGE_PROTECT },
                new CommandMapEntry { Name = Calc.RangeUnprotect, CommandId = KeyCommand.KC_RANGE_UNPROTECT },
                new CommandMapEntry { Name = Calc.RangeInput, CommandId = KeyCommand.KC_RANGE_INPUT }
            ]
        },
        new () {
            ID = CommandMapID.FORMAT,
            Commands = [
                new CommandMapEntry { Name = Calc.FormatFixed, CommandId = KeyCommand.KC_FORMAT_FIXED, Description = Calc.FormatFixedDescription },
                new CommandMapEntry { Name = Calc.FormatScientific, CommandId = KeyCommand.KC_FORMAT_SCI, Description = Calc.FormatScientificDescription },
                new CommandMapEntry { Name = Calc.FormatCurrency, CommandId = KeyCommand.KC_FORMAT_CURRENCY, Description = Calc.FormatCurrencyDescription },
                new CommandMapEntry { Name = Calc.FormatCommas, CommandId = KeyCommand.KC_FORMAT_COMMAS, Description = Calc.FormatCommasDescription },
                new CommandMapEntry { Name = Calc.FormatGeneral, CommandId = KeyCommand.KC_FORMAT_GENERAL, Description = Calc.FormatGeneralDescription },
                new CommandMapEntry { Name = Calc.FormatBar, CommandId = KeyCommand.KC_FORMAT_BAR, Description = Calc.FormatBarDescription },
                new CommandMapEntry { Name = Calc.FormatPercent, CommandId = KeyCommand.KC_FORMAT_PERCENT, Description = Calc.FormatPercentDescription },
                new CommandMapEntry { Name = Calc.FormatDate, SubCommandId = CommandMapID.DATES, Description = Calc.FormatDatesDescription },
                new CommandMapEntry { Name = Calc.FormatText, CommandId = KeyCommand.KC_FORMAT_TEXT, Description = "Display formula instead of value" },
                new CommandMapEntry { Name = Calc.FormatReset, CommandId = KeyCommand.KC_FORMAT_RESET, Description = Calc.FormatResetDescription }
            ]
        },
        new () {
            ID = CommandMapID.ALIGN,
            Commands = [
                new CommandMapEntry { Name = Calc.LabelPrefixLeft, CommandId = KeyCommand.KC_ALIGN_LEFT, Description = Calc.AlignLeftDescription },
                new CommandMapEntry { Name = Calc.LabelPrefixRight, CommandId = KeyCommand.KC_ALIGN_RIGHT, Description = Calc.AlignRightDescription },
                new CommandMapEntry { Name = Calc.LabelPrefixCentre, CommandId = KeyCommand.KC_ALIGN_CENTRE, Description = Calc.AlignCentreDescription }
            ]
        },
        new () {
            ID = CommandMapID.DATES,
            Commands = [
                new CommandMapEntry { Name = Calc.DatesDMY, CommandId = KeyCommand.KC_DATE_DMY, Description = Calc.DatesDMYDescription },
                new CommandMapEntry { Name = Calc.DatesDM, CommandId = KeyCommand.KC_DATE_DM, Description = Calc.DatesDMDescription },
                new CommandMapEntry { Name = Calc.DatesMY, CommandId = KeyCommand.KC_DATE_MY, Description = Calc.DatesMYDescription }
            ]
        },
        new() {
            ID = CommandMapID.FILE,
            Commands = [
                new CommandMapEntry { Name = Calc.FileRetrieve, CommandId = KeyCommand.KC_FILE_RETRIEVE, Description = Calc.FileEditDescription },
                new CommandMapEntry { Name = Calc.FileSave, CommandId = KeyCommand.KC_FILE_SAVE, Description = Calc.FileSaveDescription},
                new CommandMapEntry { Name = Calc.FileCombine, CommandId = KeyCommand.KC_FILE_COMBINE, Description = "Incorporate all or part of a worksheet file into the worksheet" },
                new CommandMapEntry { Name = Calc.FileXtract, CommandId = KeyCommand.KC_FILE_XTRACT, Description = "Store a cell range in a worksheet file"},
                new CommandMapEntry { Name = Calc.FileList, CommandId = KeyCommand.KC_FILE_LIST, Description = "Display names of calc files in the current directory" },
                new CommandMapEntry { Name = Calc.Fileimport, CommandId = KeyCommand.KC_FILE_IMPORT, Description = "Read text or numbers from a print file into the worksheet" },
                new CommandMapEntry { Name = Calc.FileDirectory, CommandId = KeyCommand.KC_FILE_DIRECTORY, Description = "Display and/or set the current directory"}
            ]
        },
        new() {
            ID = CommandMapID.DATA,
            Commands = [
                new CommandMapEntry { Name = Calc.DataFill, CommandId = KeyCommand.KC_DATA_FILL },
                new CommandMapEntry { Name = Calc.DataTable, CommandId = KeyCommand.KC_DATA_TABLE },
                new CommandMapEntry { Name = Calc.DataSort, CommandId = KeyCommand.KC_DATA_SORT },
                new CommandMapEntry { Name = Calc.DataQuery, CommandId = KeyCommand.KC_DATA_QUERY },
                new CommandMapEntry { Name = Calc.DataDistribution, CommandId = KeyCommand.KC_DATA_DISTRIBUTION }
            ]
        },
        new () {
            ID = CommandMapID.COLUMN_WIDTH,
            Commands = [
                new CommandMapEntry { Name = Calc.ColumnWidthSet, CommandId = KeyCommand.KC_SET_COLUMN_WIDTH, Description = Calc.SetColumnWidthDescription },
                new CommandMapEntry { Name = Calc.ColumnWidthReset, CommandId = KeyCommand.KC_RESET_COLUMN_WIDTH, Description = Calc.ResetColumnWidthDescription }
            ]
        }
    ];
}
