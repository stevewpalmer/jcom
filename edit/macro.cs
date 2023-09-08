// JEdit
// Macro command parsing
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

public class Macro : Parser {

    /// <summary>
    /// Initialise an empty macro command line
    /// </summary>
    public Macro() : base("") { }

    /// <summary>
    /// Initalise a macro parser with the specified input
    /// </summary>
    /// <param name="input">Command line to parse</param>
    public Macro(string input) : base(input) { }

    /// <summary>
    /// Read a number from the command line. If none present then prompt
    /// for the input.
    /// </summary>
    /// <param name="prompt">Prompt to display if input is required</param>
    /// <param name="inputValue">Input value</param>
    /// <returns>True if string retrieved, false if the input was cancelled</returns>
    public bool GetNumber(string prompt, out int inputValue) {
        string nextWord = NextWord();
        if (string.IsNullOrEmpty(nextWord) || !int.TryParse(nextWord, out inputValue)) {
            if (!Screen.StatusBar.PromptForNumber(prompt, out inputValue)) {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Read a filename from the command line. If none present then prompt
    /// for the input.
    /// </summary>
    /// <param name="prompt">Prompt to display if input is required</param>
    /// <param name="inputValue">Input value</param>
    /// <returns>True if string retrieved, false if the input was cancelled</returns>
    public bool GetFilename(string prompt, out string inputValue) {
        inputValue = NextWord();
        if (string.IsNullOrEmpty(inputValue)) {
            if (!Screen.StatusBar.PromptForInput(prompt, out inputValue, true)) {
                return false;
            }
        }
        return true;
    }
}