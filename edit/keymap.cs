// JEdit
// Keyboard and command mapping
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

namespace JEdit;

/// <summary>
/// List of command IDs
/// </summary>
public enum KeyCommand {
    KC_NONE,
    KC_EXIT,
    KC_CDOWN,
    KC_CUP,
    KC_CLEFT,
    KC_CRIGHT,
    KC_CLINESTART,
    KC_CLINEEND,
    KC_CFILESTART,
    KC_CFILEEND,
    KC_CWINDOWTOP,
    KC_CWINDOWBOTTOM,
    KC_CPAGEUP,
    KC_CPAGEDOWN,
    KC_CWORDLEFT,
    KC_CWORDRIGHT,
    KC_NEXTBUFFER,
    KC_PREVBUFFER,
    KC_VERSION,
    KC_EDIT,
    KC_CLOSE,
    KC_DETAILS,
    KC_GOTO,
    KC_COMMAND
}

/// <summary>
/// Mapping of command names and their IDs
/// </summary>
public class KeyCommands {

    /// <summary>
    /// Command name
    /// </summary>
    public string CommandName { get; init; }

    /// <summary>
    /// Associated command ID
    /// </summary>
    public KeyCommand CommandId { get; init; }
}

public class KeyMap {

    /// <summary>
    /// Table of commands and their default keystrokes
    /// </summary>
    private static readonly KeyCommands[] CommandTable = {
        new() { CommandName = "exit", CommandId = KeyCommand.KC_EXIT },
        new() { CommandName = "down", CommandId = KeyCommand.KC_CDOWN },
        new() { CommandName = "up", CommandId = KeyCommand.KC_CUP },
        new() { CommandName = "prev_char", CommandId = KeyCommand.KC_CLEFT },
        new() { CommandName = "next_char", CommandId = KeyCommand.KC_CRIGHT },
        new() { CommandName = "beginning_of_line", CommandId = KeyCommand.KC_CLINESTART },
        new() { CommandName = "end_of_line", CommandId = KeyCommand.KC_CLINEEND },
        new() { CommandName = "top_of_buffer", CommandId = KeyCommand.KC_CFILESTART },
        new() { CommandName = "end_of_buffer", CommandId = KeyCommand.KC_CFILEEND },
        new() { CommandName = "top_of_window", CommandId = KeyCommand.KC_CWINDOWTOP },
        new() { CommandName = "end_of_window", CommandId = KeyCommand.KC_CWINDOWBOTTOM },
        new() { CommandName = "page_up", CommandId = KeyCommand.KC_CPAGEUP },
        new() { CommandName = "page_down", CommandId = KeyCommand.KC_CPAGEDOWN },
        new() { CommandName = "previous_word", CommandId = KeyCommand.KC_CWORDLEFT },
        new() { CommandName = "next_word", CommandId = KeyCommand.KC_CWORDRIGHT },
        new() { CommandName = "edit_next_buffer", CommandId = KeyCommand.KC_NEXTBUFFER },
        new() { CommandName = "edit_prev_buffer", CommandId = KeyCommand.KC_PREVBUFFER },
        new() { CommandName = "version", CommandId = KeyCommand.KC_VERSION },
        new() { CommandName = "edit_file", CommandId = KeyCommand.KC_EDIT },
        new() { CommandName = "delete_curr_buffer", CommandId = KeyCommand.KC_CLOSE },
        new() { CommandName = "display_file_name", CommandId = KeyCommand.KC_DETAILS },
        new() { CommandName = "goto_line", CommandId = KeyCommand.KC_GOTO },
        new() { CommandName = "execute_macro", CommandId = KeyCommand.KC_COMMAND }
    };

    /// <summary>
    /// Key command
    /// </summary>
    private KeyCommand KeyCommand { get; init; }

    /// <summary>
    /// Modifier keys
    /// </summary>
    private ConsoleModifiers Modifiers { get; init; }

    /// <summary>
    /// The key code
    /// </summary>
    private ConsoleKey Key { get; init; }

    /// <summary>
    /// The key code
    /// </summary>
    private int KeyChar { get; init; }

    /// <summary>
    /// Match the ConsoleKeyInfo with this KeyMap.
    /// </summary>
    /// <param name="keyIn">ConsoleKeyInfo input</param>
    /// <returns>True if match, false otherwise</returns>
    private bool Match(ConsoleKeyInfo keyIn) {
        return Modifiers == keyIn.Modifiers &&
               ((Key != 0 && Key == keyIn.Key) || (KeyChar != 0 && KeyChar == keyIn.KeyChar));
    }

    /// <summary>
    /// Keyboard map
    /// </summary>
    private static readonly KeyMap[] KeyMaps = {
        new() { KeyCommand = KeyCommand.KC_EXIT, KeyChar = 8776 },
        new() { KeyCommand = KeyCommand.KC_EXIT, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.X },
        new() { KeyCommand = KeyCommand.KC_VERSION, KeyChar = 8730 },
        new() { KeyCommand = KeyCommand.KC_VERSION, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.V },
        new() { KeyCommand = KeyCommand.KC_CDOWN, Key = ConsoleKey.DownArrow },
        new() { KeyCommand = KeyCommand.KC_CUP, Key = ConsoleKey.UpArrow },
        new() { KeyCommand = KeyCommand.KC_CLEFT, Key = ConsoleKey.LeftArrow },
        new() { KeyCommand = KeyCommand.KC_CRIGHT, Key = ConsoleKey.RightArrow },
        new() { KeyCommand = KeyCommand.KC_CLINESTART, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.B },
        new() { KeyCommand = KeyCommand.KC_CLINESTART, Key = ConsoleKey.Home },
        new() { KeyCommand = KeyCommand.KC_CLINEEND, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.F },
        new() { KeyCommand = KeyCommand.KC_CLINEEND, Key = ConsoleKey.End },
        new() { KeyCommand = KeyCommand.KC_CFILESTART, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.Home },
        new() { KeyCommand = KeyCommand.KC_CFILEEND, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.End },
        new() { KeyCommand = KeyCommand.KC_CPAGEUP, Key = ConsoleKey.PageUp },
        new() { KeyCommand = KeyCommand.KC_CPAGEDOWN, Key = ConsoleKey.PageDown },
        new() { KeyCommand = KeyCommand.KC_CWORDRIGHT, Modifiers = ConsoleModifiers.Control, Key = ConsoleKey.RightArrow },
        new() { KeyCommand = KeyCommand.KC_CWORDLEFT, Modifiers = ConsoleModifiers.Control, Key = ConsoleKey.LeftArrow },
        new() { KeyCommand = KeyCommand.KC_CWINDOWTOP, Modifiers = ConsoleModifiers.Control, Key = ConsoleKey.Home },
        new() { KeyCommand = KeyCommand.KC_CWINDOWBOTTOM, Modifiers = ConsoleModifiers.Control, Key = ConsoleKey.End },
        new() { KeyCommand = KeyCommand.KC_NEXTBUFFER, KeyChar = 710 },
        new() { KeyCommand = KeyCommand.KC_NEXTBUFFER, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.N },
        new() { KeyCommand = KeyCommand.KC_PREVBUFFER, KeyChar = 305 },
        new() { KeyCommand = KeyCommand.KC_PREVBUFFER, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.OemMinus },
        new() { KeyCommand = KeyCommand.KC_CLOSE, Modifiers = ConsoleModifiers.Control, Key = ConsoleKey.D7 },
        new() { KeyCommand = KeyCommand.KC_CLOSE, Modifiers = ConsoleModifiers.Control, Key = ConsoleKey.OemMinus },
        new() { KeyCommand = KeyCommand.KC_DETAILS, KeyChar = 207 },
        new() { KeyCommand = KeyCommand.KC_DETAILS, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.F },
        new() { KeyCommand = KeyCommand.KC_GOTO, KeyChar = 204 },
        new() { KeyCommand = KeyCommand.KC_GOTO, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.G },
        new() { KeyCommand = KeyCommand.KC_EDIT, KeyChar = 8240 },
        new() { KeyCommand = KeyCommand.KC_EDIT, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.E },
        new() { KeyCommand = KeyCommand.KC_COMMAND, Key = ConsoleKey.F10 },
    };

    /// <summary>
    /// Map a command name to its corresponding command.
    /// </summary>
    /// <param name="input">Command name</param>
    /// <returns>KeyCommand equivalent, or KC_NONE if there's no mapping</returns>
    public static KeyCommand MapCommandNameToCommand(string input) {
        return CommandTable.Where(ct => ct.CommandName == input)
               .Select(ct => ct.CommandId)
               .FirstOrDefault();
    }

    /// <summary>
    /// Map a keyboard input to its corresponding command.
    /// </summary>
    /// <param name="keyIn">Keyboard input</param>
    /// <returns>KeyCommand equivalent, or KC_NONE if there's no mapping</returns>
    public static KeyCommand MapKeyToCommand(ConsoleKeyInfo keyIn) {

        KeyMap match = KeyMaps.FirstOrDefault(km => km.Match(keyIn));
        return match?.KeyCommand ?? KeyCommand.KC_NONE;
    }
}

