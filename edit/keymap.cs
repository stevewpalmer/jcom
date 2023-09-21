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
    KC_CDOWN,
    KC_CFILEEND,
    KC_CFILESTART,
    KC_CLEFT,
    KC_CLINEEND,
    KC_CLINESTART,
    KC_CLOSE,
    KC_COLOUR,
    KC_COMMAND,
    KC_COPY,
    KC_CPAGEDOWN,
    KC_CPAGEUP,
    KC_CRIGHT,
    KC_CUP,
    KC_CWINDOWBOTTOM,
    KC_CWINDOWTOP,
    KC_CWORDLEFT,
    KC_CWORDRIGHT,
    KC_DETAILS,
    KC_EDIT,
    KC_EXIT,
    KC_GOTO,
    KC_LOADKEYSTROKES,
    KC_MARK,
    KC_MARKCOLUMN,
    KC_MARKLINE,
    KC_NEXTBUFFER,
    KC_OUTPUTFILE,
    KC_PLAYBACK,
    KC_PREVBUFFER,
    KC_REMEMBER,
    KC_REPEAT,
    KC_SAVEKEYSTROKES,
    KC_SCREENDOWN,
    KC_SCREENUP,
    KC_VERSION,
    KC_WRITEANDEXIT,
    KC_WRITEBUFFER
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
        new() { CommandName = "beginning_of_line", CommandId = KeyCommand.KC_CLINESTART },
        new() { CommandName = "colour", CommandId = KeyCommand.KC_COLOUR },
        new() { CommandName = "color", CommandId = KeyCommand.KC_COLOUR },
        new() { CommandName = "copy", CommandId = KeyCommand.KC_COPY },
        new() { CommandName = "delete_curr_buffer", CommandId = KeyCommand.KC_CLOSE },
        new() { CommandName = "display_file_name", CommandId = KeyCommand.KC_DETAILS },
        new() { CommandName = "down", CommandId = KeyCommand.KC_CDOWN },
        new() { CommandName = "edit_file", CommandId = KeyCommand.KC_EDIT },
        new() { CommandName = "edit_next_buffer", CommandId = KeyCommand.KC_NEXTBUFFER },
        new() { CommandName = "edit_prev_buffer", CommandId = KeyCommand.KC_PREVBUFFER },
        new() { CommandName = "end_of_buffer", CommandId = KeyCommand.KC_CFILEEND },
        new() { CommandName = "end_of_line", CommandId = KeyCommand.KC_CLINEEND },
        new() { CommandName = "end_of_window", CommandId = KeyCommand.KC_CWINDOWBOTTOM },
        new() { CommandName = "execute_macro", CommandId = KeyCommand.KC_COMMAND },
        new() { CommandName = "exit", CommandId = KeyCommand.KC_EXIT },
        new() { CommandName = "goto_line", CommandId = KeyCommand.KC_GOTO },
        new() { CommandName = "load_keystroke_macro", CommandId = KeyCommand.KC_LOADKEYSTROKES },
        new() { CommandName = "mark", CommandId = KeyCommand.KC_MARK },
        new() { CommandName = "next_char", CommandId = KeyCommand.KC_CRIGHT },
        new() { CommandName = "next_word", CommandId = KeyCommand.KC_CWORDRIGHT },
        new() { CommandName = "output_file", CommandId = KeyCommand.KC_OUTPUTFILE },
        new() { CommandName = "page_down", CommandId = KeyCommand.KC_CPAGEDOWN },
        new() { CommandName = "page_up", CommandId = KeyCommand.KC_CPAGEUP },
        new() { CommandName = "playback", CommandId = KeyCommand.KC_PLAYBACK },
        new() { CommandName = "prev_char", CommandId = KeyCommand.KC_CLEFT },
        new() { CommandName = "previous_word", CommandId = KeyCommand.KC_CWORDLEFT },
        new() { CommandName = "remember", CommandId = KeyCommand.KC_REMEMBER },
        new() { CommandName = "repeat", CommandId = KeyCommand.KC_REPEAT },
        new() { CommandName = "save_keystroke_macro", CommandId = KeyCommand.KC_SAVEKEYSTROKES },
        new() { CommandName = "screen_down", CommandId = KeyCommand.KC_SCREENDOWN },
        new() { CommandName = "screen_up", CommandId = KeyCommand.KC_SCREENUP },
        new() { CommandName = "top_of_buffer", CommandId = KeyCommand.KC_CFILESTART },
        new() { CommandName = "top_of_window", CommandId = KeyCommand.KC_CWINDOWTOP },
        new() { CommandName = "up", CommandId = KeyCommand.KC_CUP },
        new() { CommandName = "version", CommandId = KeyCommand.KC_VERSION },
        new() { CommandName = "write_and_exit", CommandId = KeyCommand.KC_WRITEANDEXIT },
        new() { CommandName = "write_buffer", CommandId = KeyCommand.KC_WRITEBUFFER }
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
        new() { KeyCommand = KeyCommand.KC_CDOWN, Key = ConsoleKey.DownArrow },
        new() { KeyCommand = KeyCommand.KC_CFILEEND, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.End },
        new() { KeyCommand = KeyCommand.KC_CFILESTART, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.Home },
        new() { KeyCommand = KeyCommand.KC_CLEFT, Key = ConsoleKey.LeftArrow },
        new() { KeyCommand = KeyCommand.KC_CLINEEND, Key = ConsoleKey.End },
        new() { KeyCommand = KeyCommand.KC_CLINEEND, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.F },
        new() { KeyCommand = KeyCommand.KC_CLINESTART, Key = ConsoleKey.Home },
        new() { KeyCommand = KeyCommand.KC_CLINESTART, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.B },
        new() { KeyCommand = KeyCommand.KC_CLOSE, Modifiers = ConsoleModifiers.Control, Key = ConsoleKey.D7 },
        new() { KeyCommand = KeyCommand.KC_CLOSE, Modifiers = ConsoleModifiers.Control, Key = ConsoleKey.OemMinus },
        new() { KeyCommand = KeyCommand.KC_COMMAND, Key = ConsoleKey.F10 },
        new() { KeyCommand = KeyCommand.KC_COPY, Modifiers = ConsoleModifiers.Control, Key = ConsoleKey.C },
        new() { KeyCommand = KeyCommand.KC_CPAGEDOWN, Key = ConsoleKey.PageDown },
        new() { KeyCommand = KeyCommand.KC_CPAGEUP, Key = ConsoleKey.PageUp },
        new() { KeyCommand = KeyCommand.KC_CRIGHT, Key = ConsoleKey.RightArrow },
        new() { KeyCommand = KeyCommand.KC_CUP, Key = ConsoleKey.UpArrow },
        new() { KeyCommand = KeyCommand.KC_CWINDOWBOTTOM, Modifiers = ConsoleModifiers.Control, Key = ConsoleKey.End },
        new() { KeyCommand = KeyCommand.KC_CWINDOWTOP, Modifiers = ConsoleModifiers.Control, Key = ConsoleKey.Home },
        new() { KeyCommand = KeyCommand.KC_CWORDLEFT, Modifiers = ConsoleModifiers.Control, Key = ConsoleKey.LeftArrow },
        new() { KeyCommand = KeyCommand.KC_CWORDRIGHT, Modifiers = ConsoleModifiers.Control, Key = ConsoleKey.RightArrow },
        new() { KeyCommand = KeyCommand.KC_DETAILS, KeyChar = 207 },
        new() { KeyCommand = KeyCommand.KC_DETAILS, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.F },
        new() { KeyCommand = KeyCommand.KC_EDIT, KeyChar = 8240 },
        new() { KeyCommand = KeyCommand.KC_EDIT, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.E },
        new() { KeyCommand = KeyCommand.KC_EXIT, KeyChar = 8776 },
        new() { KeyCommand = KeyCommand.KC_EXIT, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.X },
        new() { KeyCommand = KeyCommand.KC_GOTO, KeyChar = 204 },
        new() { KeyCommand = KeyCommand.KC_GOTO, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.G },
        new() { KeyCommand = KeyCommand.KC_LOADKEYSTROKES, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.F7 },
        new() { KeyCommand = KeyCommand.KC_LOADKEYSTROKES, Key = ConsoleKey.F12 },
        new() { KeyCommand = KeyCommand.KC_MARK, KeyChar = 181 },
        new() { KeyCommand = KeyCommand.KC_MARK, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.M },
        new() { KeyCommand = KeyCommand.KC_MARKCOLUMN, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.C },
        new() { KeyCommand = KeyCommand.KC_MARKLINE, Modifiers = ConsoleModifiers.Control, Key = ConsoleKey.L },
        new() { KeyCommand = KeyCommand.KC_NEXTBUFFER, KeyChar = 710 },
        new() { KeyCommand = KeyCommand.KC_NEXTBUFFER, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.N },
        new() { KeyCommand = KeyCommand.KC_OUTPUTFILE, KeyChar = 248 },
        new() { KeyCommand = KeyCommand.KC_OUTPUTFILE, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.O },
        new() { KeyCommand = KeyCommand.KC_PLAYBACK, Key = ConsoleKey.F8 },
        new() { KeyCommand = KeyCommand.KC_PREVBUFFER, KeyChar = 305 },
        new() { KeyCommand = KeyCommand.KC_PREVBUFFER, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.OemMinus },
        new() { KeyCommand = KeyCommand.KC_REMEMBER, Key = ConsoleKey.F7 },
        new() { KeyCommand = KeyCommand.KC_REPEAT, Modifiers = ConsoleModifiers.Control, Key = ConsoleKey.R },
        new() { KeyCommand = KeyCommand.KC_SAVEKEYSTROKES, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.F8 },
        new() { KeyCommand = KeyCommand.KC_SAVEKEYSTROKES, Key = ConsoleKey.F13 },
        new() { KeyCommand = KeyCommand.KC_SCREENDOWN, Modifiers = ConsoleModifiers.Control, Key = ConsoleKey.D },
        new() { KeyCommand = KeyCommand.KC_SCREENUP, Modifiers = ConsoleModifiers.Control, Key = ConsoleKey.E },
        new() { KeyCommand = KeyCommand.KC_VERSION, KeyChar = 8730 },
        new() { KeyCommand = KeyCommand.KC_VERSION, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.V },
        new() { KeyCommand = KeyCommand.KC_WRITEANDEXIT, Modifiers = ConsoleModifiers.Control, Key = ConsoleKey.X },
        new() { KeyCommand = KeyCommand.KC_WRITEBUFFER, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.W }
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

    /// <summary>
    /// Map a command to its corresponding command name.
    /// </summary>
    /// <param name="commandId">Command ID</param>
    /// <returns>Command name equivalent</returns>
    public static string MapCommandToCommandName(KeyCommand commandId) {
        return CommandTable.Where(ct => ct.CommandId == commandId)
            .Select(ct => ct.CommandName).First();
    }

    /// <summary>
    /// Return whether the given command can be added to the keystroke buffer.
    /// </summary>
    /// <param name="commandId">Command ID</param>
    /// <returns>True if command can be recorded. False otherwise</returns>
    public static bool IsRecordable(KeyCommand commandId) {
        return commandId switch {
            KeyCommand.KC_REPEAT => false,
            KeyCommand.KC_REMEMBER => false,
            KeyCommand.KC_SAVEKEYSTROKES => false,
            _ => true
        };
    }
}

