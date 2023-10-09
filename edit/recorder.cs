// JEdit
// Keystroke recorder
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

using System.ComponentModel;
using JComLib;

namespace JEdit;

public enum KeystrokesMode {

    /// <summary>
    /// No recording
    /// </summary>
    [Description("  ")]
    NONE,

    /// <summary>
    /// We're recording keystrokes
    /// </summary>
    [Description("RE")]
    RECORDING,

    /// <summary>
    /// We're playing back keystrokes
    /// </summary>
    [Description("PL")]
    PLAYBACK
}

public class Recorder {
    private List<string> _keystrokeBuffer = new();

    /// <summary>
    /// Add the specified command and arguments to the keystroke buffer
    /// list if room.
    /// </summary>
    /// <param name="command">Editing command</param>
    /// <returns>True if the keystroke was saved, false if the buffer was full.</returns>
    public bool RememberKeystroke(Command command) {
        bool success = false;
        if (_keystrokeBuffer.Count < Consts.MaxKeystrokes) {
            string line = KeyMap.MapCommandToCommandName(command.Id);
            Parser args = new Parser(command.Args);
            string[] argStrings = args.RestOfLine();
            if (argStrings.Length > 0) {
                line += $" {string.Join(", ", argStrings)}";
            }
            _keystrokeBuffer.Add(line);
            success = true;
        }
        return success;
    }

    /// <summary>
    /// Return the keystroke buffer as an array.
    /// </summary>
    public IEnumerable<string> Keystrokes => _keystrokeBuffer.ToArray();

    /// <summary>
    /// Return whether there is an existing keystroke macro.
    /// </summary>
    public bool HasKeystrokeMacro => _keystrokeBuffer.Count > 0;

    /// <summary>
    /// Load keystrokes from the specified file.
    /// </summary>
    /// <param name="filename">Keystroke macro filename</param>
    /// <returns>True if the file was successfully loaded, false otherwise</returns>
    public bool LoadKeystrokes(string filename) {
        bool success = false;
        if (File.Exists(filename)) {
            filename = Utilities.AddExtensionIfMissing(filename, Consts.MacroExtension);
            _keystrokeBuffer = File.ReadAllLines(filename).ToList();
            success = true;
        }
        return success;
    }

    /// <summary>
    /// Save keystrokes to the specified file.
    /// </summary>
    /// <param name="filename">Keystroke macro filename</param>
    public void SaveKeystrokes(string filename) {
        filename = Utilities.AddExtensionIfMissing(filename, Consts.MacroExtension);
        File.WriteAllLines(filename, _keystrokeBuffer);
    }
}