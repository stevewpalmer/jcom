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

using System.ComponentModel;
using JCalc.Resources;

namespace JCalc;

/// <summary>
/// List of command IDs
/// </summary>
public enum CommandMapID {

    [Description("COMMAND")]
    MAIN,

    [Description("GOTO")]
    GOTO
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
                new() { Name = "Alpha", CommandId = KeyCommand.KC_ALPHA },
                new() { Name = "Blank", CommandId = KeyCommand.KC_BLANK },
                new() { Name = "Copy", CommandId = KeyCommand.KC_COPY },
                new() { Name = "Delete", CommandId = KeyCommand.KC_DELETE },
                new() { Name = "Edit", CommandId = KeyCommand.KC_EDIT },
                new() { Name = "Format", CommandId = KeyCommand.KC_FORMAT },
                new() { Name = "Goto", CommandId = KeyCommand.KC_GOTO },
                new() { Name = "Help", CommandId = KeyCommand.KC_HELP },
                new() { Name = "Insert", CommandId = KeyCommand.KC_INSERT },
                new() { Name = "Lock", CommandId = KeyCommand.KC_LOCK },
                new() { Name = "Move", CommandId = KeyCommand.KC_MOVE },
                new() { Name = "Name", CommandId = KeyCommand.KC_NAME },
                new() { Name = "Options", CommandId = KeyCommand.KC_OPTIONS },
                new() { Name = "Print", CommandId = KeyCommand.KC_PRINT },
                new() { Name = "Quit", CommandId = KeyCommand.KC_QUIT },
                new() { Name = "Sort", CommandId = KeyCommand.KC_SORT },
                new() { Name = "Transfer", CommandId = KeyCommand.KC_TRANSFER },
                new() { Name = "Value", CommandId = KeyCommand.KC_VALUE },
                new() { Name = "Window", CommandId = KeyCommand.KC_WINDOW },
                new() { Name = "Xternal", CommandId = KeyCommand.KC_XTERNAL }
            ]
        },
        new() {
            ID = CommandMapID.GOTO,
            Commands = [
                new() { Name = Calc.GotoMacro, CommandId = KeyCommand.KC_GOTO_MACRO },
                new() { Name = Calc.GotoName, CommandId = KeyCommand.KC_GOTO_NAME },
                new() { Name = Calc.GotoRowCol, CommandId = KeyCommand.KC_GOTO_ROWCOL },
                new() { Name = Calc.GotoWindow, CommandId = KeyCommand.KC_GOTO_WINDOW }
            ]
        }
    ];
}
