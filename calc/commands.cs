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
    COLOURS,
    COLUMN_WIDTH,
    DATES,
    DEFAULT_ALIGNMENT,
    DEFAULT_DATES,
    DEFAULT_FORMAT,
    DELETE,
    FILE,
    FORMAT,
    INSERT,
    MAIN,
    RANGE,
    SETTINGS,
    STYLE,
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
                new CommandMapEntry { Name = Calc.Copy, CommandId = KeyCommand.KC_COPY, Description = Calc.CopyDescription},
                new CommandMapEntry { Name = Calc.Move, CommandId = KeyCommand.KC_CUT, Description = Calc.CutDescription},
                new CommandMapEntry { Name = Calc.Paste, CommandId = KeyCommand.KC_PASTE, Description = Calc.PasteDescription},
                new CommandMapEntry { Name = Calc.File, SubCommandId = CommandMapID.FILE },
                new CommandMapEntry { Name = Calc.Settings, SubCommandId = CommandMapID.SETTINGS },
                new CommandMapEntry { Name = Calc.Quit, CommandId = KeyCommand.KC_QUIT, Description = Calc.QuitCalc}
            ]
        },
        new() {
            ID = CommandMapID.WORKSHEET,
            Commands = [
                new CommandMapEntry { Name = Calc.WorksheetInsert, SubCommandId = CommandMapID.INSERT, Description = Calc.WorksheetInsertDescription},
                new CommandMapEntry { Name = Calc.WorksheetDelete, SubCommandId = CommandMapID.DELETE, Description = Calc.WorksheetDeleteDescription },
                new CommandMapEntry { Name = Calc.WorksheetColumnWidth, SubCommandId = CommandMapID.COLUMN_WIDTH, Description = Calc.WorksheetColumnWidthDescription}
            ]
        },
        new() {
            ID = CommandMapID.INSERT,
            Commands = [
                new CommandMapEntry { Name = Calc.Column, CommandId = KeyCommand.KC_INSERT_COLUMN, Description = Calc.InsertColumnDescription },
                new CommandMapEntry { Name = Calc.Row, CommandId = KeyCommand.KC_INSERT_ROW, Description = Calc.InsertRowDescription },
                new CommandMapEntry { Name = Calc.Worksheet, CommandId = KeyCommand.KC_NEW, Description = Calc.InsertWorksheetDescription}
            ]
        },
        new() {
            ID = CommandMapID.DELETE,
            Commands = [
                new CommandMapEntry { Name = Calc.Column, CommandId = KeyCommand.KC_DELETE_COLUMN, Description = Calc.DeleteColumnDescription },
                new CommandMapEntry { Name = Calc.Row, CommandId = KeyCommand.KC_DELETE_ROW, Description = Calc.DeleteRowDescription },
                new CommandMapEntry { Name = Calc.Worksheet, CommandId = KeyCommand.KC_DELETE_WORKSHEET, Description = Calc.DeleteWorksheetDescription}
            ]
        },
        new() {
            ID = CommandMapID.RANGE,
            Commands = [
                new CommandMapEntry { Name = Calc.RangeFormat, SubCommandId = CommandMapID.FORMAT, Description = Calc.RangeFormatDescription },
                new CommandMapEntry { Name = Calc.RangeAlign, SubCommandId = CommandMapID.ALIGN, Description = Calc.RangeAlignDescription },
                new CommandMapEntry { Name = Calc.RangeText, SubCommandId = CommandMapID.STYLE, Description = Calc.RangeTextDescription },
                new CommandMapEntry { Name = Calc.RangeExport, CommandId = KeyCommand.KC_RANGE_EXPORT, Description = Calc.RangeExportDescription },
                new CommandMapEntry { Name = Calc.RangeSort, CommandId = KeyCommand.KC_RANGE_SORT, Description = Calc.RangeSortDescription }
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
                new CommandMapEntry { Name = Calc.FormatPercent, CommandId = KeyCommand.KC_FORMAT_PERCENT, Description = Calc.FormatPercentDescription },
                new CommandMapEntry { Name = Calc.FormatDate, SubCommandId = CommandMapID.DATES, Description = Calc.FormatDatesDescription },
                new CommandMapEntry { Name = Calc.FormatText, CommandId = KeyCommand.KC_FORMAT_TEXT, Description = Calc.FormatTextDescription },
                new CommandMapEntry { Name = Calc.FormatReset, CommandId = KeyCommand.KC_FORMAT_RESET, Description = Calc.FormatResetDescription }
            ]
        },
        new() {
            ID = CommandMapID.STYLE,
            Commands = [
                new CommandMapEntry { Name = Calc.StyleColours, SubCommandId = CommandMapID.COLOURS, Description = Calc.StyleForegroundDescription },
                new CommandMapEntry { Name = Calc.StyleBold, CommandId = KeyCommand.KC_STYLE_BOLD, Description = Calc.StyleBoldDescription },
                new CommandMapEntry { Name = Calc.StyleItalic, CommandId = KeyCommand.KC_STYLE_ITALIC, Description = Calc.StyleItalicDescription },
                new CommandMapEntry { Name = Calc.StyleUnderline, CommandId = KeyCommand.KC_STYLE_UNDERLINE, Description = Calc.StyleUnderlineDescription }
            ]
        },
        new() {
            ID = CommandMapID.COLOURS,
            Commands = [
                new CommandMapEntry { Name = Calc.StyleForeground, CommandId = KeyCommand.KC_STYLE_FG, Description = Calc.StyleForegroundDescription },
                new CommandMapEntry { Name = Calc.StyleBackground, CommandId = KeyCommand.KC_STYLE_BG, Description = Calc.StyleBackgroundDescription }
            ]
        },
        new () {
            ID = CommandMapID.ALIGN,
            Commands = [
                new CommandMapEntry { Name = Calc.AlignLeft, CommandId = KeyCommand.KC_ALIGN_LEFT, Description = Calc.AlignLeftDescription },
                new CommandMapEntry { Name = Calc.AlignRight, CommandId = KeyCommand.KC_ALIGN_RIGHT, Description = Calc.AlignRightDescription },
                new CommandMapEntry { Name = Calc.AlignCentre, CommandId = KeyCommand.KC_ALIGN_CENTRE, Description = Calc.AlignCentreDescription },
                new CommandMapEntry { Name = Calc.AlignGeneral, CommandId = KeyCommand.KC_ALIGN_GENERAL, Description = Calc.AlignGeneralDescription }
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
                new CommandMapEntry { Name = Calc.FileOpen, CommandId = KeyCommand.KC_FILE_RETRIEVE, Description = Calc.FileOpenDescription },
                new CommandMapEntry { Name = Calc.FileSave, CommandId = KeyCommand.KC_FILE_SAVE, Description = Calc.FileSaveDescription},
                new CommandMapEntry { Name = Calc.FileImport, CommandId = KeyCommand.KC_FILE_IMPORT, Description = Calc.Calc_FileImportDescription},
                new CommandMapEntry { Name = Calc.FileExport, CommandId = KeyCommand.KC_FILE_EXPORT, Description = Calc.FileExportDescription}
            ]
        },
        new() {
            ID = CommandMapID.SETTINGS,
            Commands = [
                new CommandMapEntry { Name = Calc.SettingsColours, CommandId = KeyCommand.KC_SETTINGS_COLOURS, Description = Calc.SettingsColourDescription},
                new CommandMapEntry { Name = Calc.SettingsFormat, SubCommandId = CommandMapID.DEFAULT_FORMAT, Description = Calc.SettingsFormatDescription },
                new CommandMapEntry { Name = Calc.SettingsAlignment, SubCommandId = CommandMapID.DEFAULT_ALIGNMENT, Description = Calc.SettingsAlignmentDescription},
                new CommandMapEntry { Name = Calc.SettingsDecimalPoints, CommandId = KeyCommand.KC_SETTINGS_DECIMAL_POINTS, Description = Calc.SettingsDecimalPointDescription },
                new CommandMapEntry { Name = Calc.SettingsBackups, CommandId = KeyCommand.KC_SETTINGS_BACKUPS, Description = Calc.SettingsBackupDescription }
            ]
        },
        new () {
            ID = CommandMapID.COLUMN_WIDTH,
            Commands = [
                new CommandMapEntry { Name = Calc.ColumnWidthSet, CommandId = KeyCommand.KC_SET_COLUMN_WIDTH, Description = Calc.SetColumnWidthDescription },
                new CommandMapEntry { Name = Calc.ColumnWidthReset, CommandId = KeyCommand.KC_RESET_COLUMN_WIDTH, Description = Calc.ResetColumnWidthDescription }
            ]
        },
        new () {
            ID = CommandMapID.DEFAULT_FORMAT,
            Commands = [
                new CommandMapEntry { Name = Calc.FormatFixed, CommandId = KeyCommand.KC_DEFAULT_FORMAT_FIXED, Description = Calc.FormatFixedDescription },
                new CommandMapEntry { Name = Calc.FormatScientific, CommandId = KeyCommand.KC_DEFAULT_FORMAT_SCI, Description = Calc.FormatScientificDescription },
                new CommandMapEntry { Name = Calc.FormatCurrency, CommandId = KeyCommand.KC_DEFAULT_FORMAT_CURRENCY, Description = Calc.FormatCurrencyDescription },
                new CommandMapEntry { Name = Calc.FormatCommas, CommandId = KeyCommand.KC_DEFAULT_FORMAT_COMMAS, Description = Calc.FormatCommasDescription },
                new CommandMapEntry { Name = Calc.FormatGeneral, CommandId = KeyCommand.KC_DEFAULT_FORMAT_GENERAL, Description = Calc.FormatGeneralDescription },
                new CommandMapEntry { Name = Calc.FormatPercent, CommandId = KeyCommand.KC_DEFAULT_FORMAT_PERCENT, Description = Calc.FormatPercentDescription },
                new CommandMapEntry { Name = Calc.FormatDate, SubCommandId = CommandMapID.DEFAULT_DATES, Description = Calc.FormatDatesDescription },
                new CommandMapEntry { Name = Calc.FormatText, CommandId = KeyCommand.KC_DEFAULT_FORMAT_TEXT, Description = Calc.FormatTextDescription }
            ]
        },
        new () {
            ID = CommandMapID.DEFAULT_DATES,
            Commands = [
                new CommandMapEntry { Name = Calc.DatesDMY, CommandId = KeyCommand.KC_DEFAULT_DATE_DMY, Description = Calc.DatesDMYDescription },
                new CommandMapEntry { Name = Calc.DatesDM, CommandId = KeyCommand.KC_DEFAULT_DATE_DM, Description = Calc.DatesDMDescription },
                new CommandMapEntry { Name = Calc.DatesMY, CommandId = KeyCommand.KC_DEFAULT_DATE_MY, Description = Calc.DatesMYDescription }
            ]
        },
        new () {
            ID = CommandMapID.DEFAULT_ALIGNMENT,
            Commands = [
                new CommandMapEntry { Name = Calc.AlignLeft, CommandId = KeyCommand.KC_DEFAULT_ALIGN_LEFT, Description = Calc.AlignLeftDescription },
                new CommandMapEntry { Name = Calc.AlignRight, CommandId = KeyCommand.KC_DEFAULT_ALIGN_RIGHT, Description = Calc.AlignRightDescription },
                new CommandMapEntry { Name = Calc.AlignCentre, CommandId = KeyCommand.KC_DEFAULT_ALIGN_CENTRE, Description = Calc.AlignCentreDescription },
                new CommandMapEntry { Name = Calc.AlignGeneral, CommandId = KeyCommand.KC_DEFAULT_ALIGN_GENERAL, Description = Calc.AlignGeneralDescription }
            ]
        }
    ];
}
