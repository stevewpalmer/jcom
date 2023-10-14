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

using JComLib;

namespace JEdit;

/// <summary>
/// List of command IDs
/// </summary>
public enum KeyCommand {
    KC_NONE,
    KC_ASSIGNTOKEY,
    KC_BACKSPACE,
    KC_BACKUPFILE,
    KC_BORDERS,
    KC_CD,
    KC_CDOWN,
    KC_CENTRE,
    KC_CFILEEND,
    KC_CFILESTART,
    KC_CLEFT,
    KC_CLINEEND,
    KC_CLINESTART,
    KC_CLOCK,
    KC_CLOSE,
    KC_COLOUR,
    KC_COMMAND,
    KC_COPY,
    KC_CPAGEDOWN,
    KC_CPAGEUP,
    KC_CRIGHT,
    KC_CTOBOTTOM,
    KC_CTOTOP,
    KC_CUP,
    KC_CUT,
    KC_CWINDOWBOTTOM,
    KC_CWINDOWCENTRE,
    KC_CWINDOWTOP,
    KC_CWORDLEFT,
    KC_CWORDRIGHT,
    KC_DELETECHAR,
    KC_DELETELINE,
    KC_DELETEPREVWORD,
    KC_DELETETOEND,
    KC_DELETETOSTART,
    KC_DELETEWORD,
    KC_DELFILE,
    KC_DETAILS,
    KC_EDIT,
    KC_EXIT,
    KC_GOTO,
    KC_INSERTMODE,
    KC_LOADKEYSTROKES,
    KC_LOWERCASE,
    KC_MARGIN,
    KC_MARK,
    KC_MARKCOLUMN,
    KC_MARKLINE,
    KC_NEXTBUFFER,
    KC_OPENLINE,
    KC_OUTPUTFILE,
    KC_PASTE,
    KC_PLAYBACK,
    KC_PREVBUFFER,
    KC_REGEXP,
    KC_REMEMBER,
    KC_REPEAT,
    KC_SAVEKEYSTROKES,
    KC_SCREENDOWN,
    KC_SCREENUP,
    KC_SEARCHAGAIN,
    KC_SEARCHBACK,
    KC_SEARCHCASE,
    KC_SEARCHFORWARD,
    KC_SELFINSERT,
    KC_TABS,
    KC_TRANSLATE,
    KC_TRANSLATEAGAIN,
    KC_TRANSLATEBACK,
    KC_UPPERCASE,
    KC_USETABCHAR,
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
    public string CommandName { get; init; } = "";

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
        new() { CommandName = "assign_to_key", CommandId = KeyCommand.KC_ASSIGNTOKEY },
        new() { CommandName = "backspace", CommandId = KeyCommand.KC_BACKSPACE },
        new() { CommandName = "beginning_of_line", CommandId = KeyCommand.KC_CLINESTART },
        new() { CommandName = "borders", CommandId = KeyCommand.KC_BORDERS },
        new() { CommandName = "cd", CommandId = KeyCommand.KC_CD },
        new() { CommandName = "center", CommandId = KeyCommand.KC_CENTRE },
        new() { CommandName = "centre", CommandId = KeyCommand.KC_CENTRE },
        new() { CommandName = "center_line", CommandId = KeyCommand.KC_CWINDOWCENTRE },
        new() { CommandName = "centre_line", CommandId = KeyCommand.KC_CWINDOWCENTRE },
        new() { CommandName = "colour", CommandId = KeyCommand.KC_COLOUR },
        new() { CommandName = "color", CommandId = KeyCommand.KC_COLOUR },
        new() { CommandName = "copy", CommandId = KeyCommand.KC_COPY },
        new() { CommandName = "cut", CommandId = KeyCommand.KC_CUT },
        new() { CommandName = "del", CommandId = KeyCommand.KC_DELFILE },
        new() { CommandName = "delete_char", CommandId = KeyCommand.KC_DELETECHAR },
        new() { CommandName = "delete_curr_buffer", CommandId = KeyCommand.KC_CLOSE },
        new() { CommandName = "delete_line", CommandId = KeyCommand.KC_DELETELINE },
        new() { CommandName = "delete_next_word", CommandId = KeyCommand.KC_DELETEWORD },
        new() { CommandName = "delete_previous_word", CommandId = KeyCommand.KC_DELETEPREVWORD },
        new() { CommandName = "delete_to_bol", CommandId = KeyCommand.KC_DELETETOSTART },
        new() { CommandName = "delete_to_eol", CommandId = KeyCommand.KC_DELETETOEND },
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
        new() { CommandName = "insert_mode", CommandId = KeyCommand.KC_INSERTMODE },
        new() { CommandName = "goto_line", CommandId = KeyCommand.KC_GOTO },
        new() { CommandName = "load_keystroke_macro", CommandId = KeyCommand.KC_LOADKEYSTROKES },
        new() { CommandName = "margin", CommandId = KeyCommand.KC_MARGIN },
        new() { CommandName = "mark", CommandId = KeyCommand.KC_MARK },
        new() { CommandName = "mark_col", CommandId = KeyCommand.KC_MARKCOLUMN },
        new() { CommandName = "mark_line", CommandId = KeyCommand.KC_MARKLINE },
        new() { CommandName = "next_char", CommandId = KeyCommand.KC_CRIGHT },
        new() { CommandName = "next_word", CommandId = KeyCommand.KC_CWORDRIGHT },
        new() { CommandName = "open_line", CommandId = KeyCommand.KC_OPENLINE },
        new() { CommandName = "output_file", CommandId = KeyCommand.KC_OUTPUTFILE },
        new() { CommandName = "page_down", CommandId = KeyCommand.KC_CPAGEDOWN },
        new() { CommandName = "page_up", CommandId = KeyCommand.KC_CPAGEUP },
        new() { CommandName = "paste", CommandId = KeyCommand.KC_PASTE },
        new() { CommandName = "playback", CommandId = KeyCommand.KC_PLAYBACK },
        new() { CommandName = "prev_char", CommandId = KeyCommand.KC_CLEFT },
        new() { CommandName = "previous_word", CommandId = KeyCommand.KC_CWORDLEFT },
        new() { CommandName = "remember", CommandId = KeyCommand.KC_REMEMBER },
        new() { CommandName = "repeat", CommandId = KeyCommand.KC_REPEAT },
        new() { CommandName = "save_keystroke_macro", CommandId = KeyCommand.KC_SAVEKEYSTROKES },
        new() { CommandName = "screen_down", CommandId = KeyCommand.KC_SCREENDOWN },
        new() { CommandName = "screen_up", CommandId = KeyCommand.KC_SCREENUP },
        new() { CommandName = "search_again", CommandId = KeyCommand.KC_SEARCHAGAIN },
        new() { CommandName = "search_back", CommandId = KeyCommand.KC_SEARCHBACK },
        new() { CommandName = "search_case", CommandId = KeyCommand.KC_SEARCHCASE },
        new() { CommandName = "search_fwd", CommandId = KeyCommand.KC_SEARCHFORWARD },
        new() { CommandName = "self_insert", CommandId = KeyCommand.KC_SELFINSERT },
        new() { CommandName = "set_backup", CommandId = KeyCommand.KC_BACKUPFILE },
        new() { CommandName = "show_clock", CommandId = KeyCommand.KC_CLOCK },
        new() { CommandName = "tabs", CommandId = KeyCommand.KC_TABS },
        new() { CommandName = "toggle_re", CommandId = KeyCommand.KC_REGEXP },
        new() { CommandName = "tolower", CommandId = KeyCommand.KC_LOWERCASE },
        new() { CommandName = "toupper", CommandId = KeyCommand.KC_UPPERCASE },
        new() { CommandName = "to_bottom", CommandId = KeyCommand.KC_CTOBOTTOM },
        new() { CommandName = "to_top", CommandId = KeyCommand.KC_CTOTOP },
        new() { CommandName = "top_of_buffer", CommandId = KeyCommand.KC_CFILESTART },
        new() { CommandName = "top_of_window", CommandId = KeyCommand.KC_CWINDOWTOP },
        new() { CommandName = "translate", CommandId = KeyCommand.KC_TRANSLATE },
        new() { CommandName = "translate_again", CommandId = KeyCommand.KC_TRANSLATEAGAIN },
        new() { CommandName = "translate_back", CommandId = KeyCommand.KC_TRANSLATEBACK },
        new() { CommandName = "up", CommandId = KeyCommand.KC_CUP },
        new() { CommandName = "use_tab_char", CommandId = KeyCommand.KC_USETABCHAR },
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
    private ConsoleModifiers Modifiers { get; set; }

    /// <summary>
    /// The key code
    /// </summary>
    private ConsoleKey Key { get; set; }

    /// <summary>
    /// The key code
    /// </summary>
    private int KeyChar { get; set; }

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
    /// Compare this keymap with another.
    /// </summary>
    /// <param name="other">KeyMap to compare</param>
    /// <returns>True if match, false otherwise</returns>
    private bool Compare(KeyMap other) {
        return Modifiers == other.Modifiers &&
               ((Key != 0 && Key == other.Key) || (KeyChar != 0 && KeyChar == other.KeyChar));
    }

    /// <summary>
    /// Parse a string that defines a keystroke and creates a KeyMap instance
    /// for that key with the command ID left empty.
    /// </summary>
    private static KeyMap? Parse(string keydef) {
        KeyMap newKeymap = new KeyMap();
        bool validKeymap = false;
        foreach (string part in keydef.Split('-', '+').Select(s => s.Trim().ToLower())) {
            switch (part) {
                case "ctrl":
                case "ctl":
                    newKeymap.Modifiers |= ConsoleModifiers.Control;
                    break;
                case "shift":
                    newKeymap.Modifiers |= ConsoleModifiers.Shift;
                    break;
                case "alt":
                case "opt":
                case "option":
                    newKeymap.Modifiers |= ConsoleModifiers.Alt;
                    break;
                default:
                    if (part.StartsWith('#') && int.TryParse(part[1..], out int keycharCode)) {
                        newKeymap.KeyChar = keycharCode;
                        validKeymap = true;
                        continue;
                    }
                    if (Enum.TryParse(part.ToUpper(), out ConsoleKey key)) {
                        newKeymap.Key = key;
                        validKeymap = true;
                    }
                    break;
            }
        }
        return validKeymap ? newKeymap : null;
    }

    /// <summary>
    /// Keyboard map
    /// </summary>
    private static readonly KeyMap[] KeyMaps = {
        new() { KeyCommand = KeyCommand.KC_BACKSPACE, Key = ConsoleKey.Backspace },
        new() { KeyCommand = KeyCommand.KC_BACKUPFILE, Modifiers = ConsoleModifiers.Control, Key = ConsoleKey.W },
        new() { KeyCommand = KeyCommand.KC_BORDERS, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.F1 },
        new() { KeyCommand = KeyCommand.KC_CDOWN, Key = ConsoleKey.DownArrow },
        new() { KeyCommand = KeyCommand.KC_CWINDOWCENTRE, Modifiers = ConsoleModifiers.Control, Key = ConsoleKey.W },
        new() { KeyCommand = KeyCommand.KC_CFILEEND, Modifiers = ConsoleModifiers.Control, Key = ConsoleKey.U },
        new() { KeyCommand = KeyCommand.KC_CFILEEND, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.End },
        new() { KeyCommand = KeyCommand.KC_CFILESTART, Modifiers = ConsoleModifiers.Control, Key = ConsoleKey.L },
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
        new() { KeyCommand = KeyCommand.KC_COPY, Modifiers = ConsoleModifiers.Control, Key = ConsoleKey.OemPlus },
        new() { KeyCommand = KeyCommand.KC_CPAGEDOWN, Key = ConsoleKey.PageDown },
        new() { KeyCommand = KeyCommand.KC_CPAGEUP, Key = ConsoleKey.PageUp },
        new() { KeyCommand = KeyCommand.KC_CRIGHT, Key = ConsoleKey.RightArrow },
        new() { KeyCommand = KeyCommand.KC_CTOBOTTOM, Modifiers = ConsoleModifiers.Control, Key = ConsoleKey.B },
        new() { KeyCommand = KeyCommand.KC_CTOTOP, Modifiers = ConsoleModifiers.Control, Key = ConsoleKey.T },
        new() { KeyCommand = KeyCommand.KC_CUP, Key = ConsoleKey.UpArrow },
        new() { KeyCommand = KeyCommand.KC_CUT, Modifiers = ConsoleModifiers.Control, Key = ConsoleKey.X },
        new() { KeyCommand = KeyCommand.KC_CUT, Modifiers = ConsoleModifiers.Control, Key = ConsoleKey.OemMinus },
        new() { KeyCommand = KeyCommand.KC_CWINDOWBOTTOM, KeyChar = 8747 },
        new() { KeyCommand = KeyCommand.KC_CWINDOWBOTTOM, Modifiers = ConsoleModifiers.Control, Key = ConsoleKey.End },
        new() { KeyCommand = KeyCommand.KC_CWINDOWTOP, KeyChar = 8224 },
        new() { KeyCommand = KeyCommand.KC_CWINDOWTOP, Modifiers = ConsoleModifiers.Control, Key = ConsoleKey.Home },
        new() { KeyCommand = KeyCommand.KC_CWORDLEFT, Modifiers = ConsoleModifiers.Control, Key = ConsoleKey.LeftArrow },
        new() { KeyCommand = KeyCommand.KC_CWORDRIGHT, Modifiers = ConsoleModifiers.Control, Key = ConsoleKey.RightArrow },
        new() { KeyCommand = KeyCommand.KC_DELETECHAR, Key = ConsoleKey.Delete },
        new() { KeyCommand = KeyCommand.KC_DELETELINE, KeyChar = 8706},
        new() { KeyCommand = KeyCommand.KC_DELETELINE, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.D },
        new() { KeyCommand = KeyCommand.KC_DELETEPREVWORD, KeyChar = 180 },
        new() { KeyCommand = KeyCommand.KC_DELETEPREVWORD, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.E },
        new() { KeyCommand = KeyCommand.KC_DELETETOSTART, Modifiers = ConsoleModifiers.Control, Key = ConsoleKey.K },
        new() { KeyCommand = KeyCommand.KC_DELETETOEND, KeyChar = 730},
        new() { KeyCommand = KeyCommand.KC_DELETETOEND, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.K },
        new() { KeyCommand = KeyCommand.KC_DELETEWORD, KeyChar = 729 },
        new() { KeyCommand = KeyCommand.KC_DELETEWORD, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.Backspace },
        new() { KeyCommand = KeyCommand.KC_DETAILS, KeyChar = 402 },
        new() { KeyCommand = KeyCommand.KC_DETAILS, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.F },
        new() { KeyCommand = KeyCommand.KC_EDIT, Modifiers = ConsoleModifiers.Control, Key = ConsoleKey.O },
        new() { KeyCommand = KeyCommand.KC_EDIT, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.E },
        new() { KeyCommand = KeyCommand.KC_EXIT, KeyChar = 8776 },
        new() { KeyCommand = KeyCommand.KC_EXIT, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.X },
        new() { KeyCommand = KeyCommand.KC_GOTO, Modifiers = ConsoleModifiers.Control, Key = ConsoleKey.G },
        new() { KeyCommand = KeyCommand.KC_GOTO, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.G },
        new() { KeyCommand = KeyCommand.KC_INSERTMODE, Key = ConsoleKey.Insert },
        new() { KeyCommand = KeyCommand.KC_LOADKEYSTROKES, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.F7 },
        new() { KeyCommand = KeyCommand.KC_LOADKEYSTROKES, Key = ConsoleKey.F12 },
        new() { KeyCommand = KeyCommand.KC_MARK, KeyChar = 181 },
        new() { KeyCommand = KeyCommand.KC_MARK, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.M },
        new() { KeyCommand = KeyCommand.KC_MARKCOLUMN, KeyChar = 231 },
        new() { KeyCommand = KeyCommand.KC_MARKCOLUMN, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.C },
        new() { KeyCommand = KeyCommand.KC_MARKLINE, KeyChar = 172 },
        new() { KeyCommand = KeyCommand.KC_MARKLINE, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.L },
        new() { KeyCommand = KeyCommand.KC_NEXTBUFFER, Modifiers = ConsoleModifiers.Control, Key = ConsoleKey.N },
        new() { KeyCommand = KeyCommand.KC_NEXTBUFFER, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.N },
        new() { KeyCommand = KeyCommand.KC_OUTPUTFILE, KeyChar = 248 },
        new() { KeyCommand = KeyCommand.KC_OUTPUTFILE, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.O },
        new() { KeyCommand = KeyCommand.KC_PASTE, Modifiers = ConsoleModifiers.Control, Key = ConsoleKey.V },
        new() { KeyCommand = KeyCommand.KC_PASTE, Key = ConsoleKey.Insert },
        new() { KeyCommand = KeyCommand.KC_PLAYBACK, Key = ConsoleKey.F8 },
        new() { KeyCommand = KeyCommand.KC_NEXTBUFFER, Modifiers = ConsoleModifiers.Control, Key = ConsoleKey.P },
        new() { KeyCommand = KeyCommand.KC_PREVBUFFER, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.OemMinus },
        new() { KeyCommand = KeyCommand.KC_REGEXP, Modifiers = ConsoleModifiers.Control, Key = ConsoleKey.F },
        new() { KeyCommand = KeyCommand.KC_REMEMBER, Key = ConsoleKey.F7 },
        new() { KeyCommand = KeyCommand.KC_REPEAT, Modifiers = ConsoleModifiers.Control, Key = ConsoleKey.R },
        new() { KeyCommand = KeyCommand.KC_SAVEKEYSTROKES, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.F8 },
        new() { KeyCommand = KeyCommand.KC_SAVEKEYSTROKES, Key = ConsoleKey.F13 },
        new() { KeyCommand = KeyCommand.KC_SELFINSERT, Key = ConsoleKey.Enter },
        new() { KeyCommand = KeyCommand.KC_SCREENDOWN, Modifiers = ConsoleModifiers.Control, Key = ConsoleKey.D },
        new() { KeyCommand = KeyCommand.KC_SCREENUP, Modifiers = ConsoleModifiers.Control, Key = ConsoleKey.E },
        new() { KeyCommand = KeyCommand.KC_SEARCHAGAIN, Modifiers = ConsoleModifiers.Control, Key = ConsoleKey.A },
        new() { KeyCommand = KeyCommand.KC_SEARCHAGAIN, Modifiers = ConsoleModifiers.Shift, Key = ConsoleKey.F5 },
        new() { KeyCommand = KeyCommand.KC_SEARCHBACK, Modifiers = ConsoleModifiers.Control, Key = ConsoleKey.U },
        new() { KeyCommand = KeyCommand.KC_SEARCHBACK, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.F5 },
        new() { KeyCommand = KeyCommand.KC_SEARCHCASE, Modifiers = ConsoleModifiers.Control, Key = ConsoleKey.S },
        new() { KeyCommand = KeyCommand.KC_SEARCHFORWARD, KeyChar = 223 },
        new() { KeyCommand = KeyCommand.KC_SEARCHFORWARD, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.S },
        new() { KeyCommand = KeyCommand.KC_SEARCHFORWARD, Key = ConsoleKey.F5 },
        new() { KeyCommand = KeyCommand.KC_TRANSLATE, Key = ConsoleKey.F6 },
        new() { KeyCommand = KeyCommand.KC_TRANSLATE, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.T },
        new() { KeyCommand = KeyCommand.KC_TRANSLATEAGAIN, KeyChar = 229},
        new() { KeyCommand = KeyCommand.KC_TRANSLATEAGAIN, Modifiers = ConsoleModifiers.Shift, Key = ConsoleKey.F6 },
        new() { KeyCommand = KeyCommand.KC_TRANSLATEBACK, KeyChar = 168 },
        new() { KeyCommand = KeyCommand.KC_TRANSLATEBACK, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.F6 },
        new() { KeyCommand = KeyCommand.KC_VERSION, KeyChar = 8730 },
        new() { KeyCommand = KeyCommand.KC_VERSION, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.V },
        new() { KeyCommand = KeyCommand.KC_WRITEANDEXIT, Modifiers = ConsoleModifiers.Control, Key = ConsoleKey.X },
        new() { KeyCommand = KeyCommand.KC_WRITEBUFFER, KeyChar = 8721 },
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
               .FirstOrDefault(KeyCommand.KC_NONE);
    }

    /// <summary>
    /// Map a keyboard input to its corresponding command.
    /// </summary>
    /// <param name="keyIn">Keyboard input</param>
    /// <returns>KeyCommand equivalent, or KC_NONE if there's no mapping</returns>
    public static Command MapKeyToCommand(ConsoleKeyInfo keyIn) {

        KeyMap? match = KeyMaps.FirstOrDefault(km => km.Match(keyIn));
        KeyCommand commandId = match?.KeyCommand ?? KeyCommand.KC_NONE;
        Parser commandArgs = new Parser(string.Empty);
        if (commandId == KeyCommand.KC_NONE) {
            commandId = KeyCommand.KC_SELFINSERT;
            commandArgs = new Parser(((int)keyIn.KeyChar).ToString());
        }
        return new Command { Id = commandId, Args = commandArgs };
    }

    /// <summary>
    /// Remap a keystroke to the specified command. The keystroke must be a
    /// valid string of the format:
    ///
    /// [modifier]+[keyname]
    ///
    /// where modifier can be Ctrl,Shift or Alt, and may be combined as
    /// needed. Keyname must be a letter or function key name.
    /// </summary>
    public static bool RemapKeyToCommand(string keystroke, string command) {
        KeyCommand commandId = MapCommandNameToCommand(command);
        KeyMap? keyMap = Parse(keystroke);
        bool success = false;
        if (commandId != KeyCommand.KC_NONE && keyMap != null) {
            foreach (KeyMap km in KeyMaps) {
                if (km.Compare(keyMap)) {
                    km.Key = 0;
                    km.Modifiers = 0;
                    km.KeyChar = 0;
                }
                if (km.KeyCommand == commandId) {
                    km.Key = keyMap.Key;
                    km.Modifiers = keyMap.Modifiers;
                    km.KeyChar = keyMap.KeyChar;
                    success = true;
                }
            }
        }
        return success;
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
            KeyCommand.KC_NONE => false,
            KeyCommand.KC_REPEAT => false,
            KeyCommand.KC_REMEMBER => false,
            KeyCommand.KC_COMMAND => false,
            KeyCommand.KC_SAVEKEYSTROKES => false,
            _ => true
        };
    }
}

