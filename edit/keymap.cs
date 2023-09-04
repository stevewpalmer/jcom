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
    KC_CPAGEUP,
    KC_CPAGEDOWN,
    KC_CWORDLEFT,
    KC_CWORDRIGHT,
    KC_NEXTBUFFER,
    KC_PREVBUFFER,
    KC_VERSION,
    KC_CLOSE,
    KC_DETAILS
};

public class KeyMap {

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
        new() { KeyCommand = KeyCommand.KC_NEXTBUFFER, KeyChar = 710 },
        new() { KeyCommand = KeyCommand.KC_NEXTBUFFER, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.N },
        new() { KeyCommand = KeyCommand.KC_PREVBUFFER, KeyChar = 305 },
        new() { KeyCommand = KeyCommand.KC_PREVBUFFER, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.OemMinus },
        new() { KeyCommand = KeyCommand.KC_CLOSE, Modifiers = ConsoleModifiers.Control, Key = ConsoleKey.D7 },
        new() { KeyCommand = KeyCommand.KC_CLOSE, Modifiers = ConsoleModifiers.Control, Key = ConsoleKey.OemMinus },
        new() { KeyCommand = KeyCommand.KC_DETAILS, KeyChar = 402 },
        new() { KeyCommand = KeyCommand.KC_DETAILS, Modifiers = ConsoleModifiers.Alt, Key = ConsoleKey.F }
    };

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

