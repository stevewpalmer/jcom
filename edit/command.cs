// JEdit
// Command object
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
/// A single editing command with optional
/// arguments.
/// </summary>
public class Command {

    /// <summary>
    /// Default constructor.
    /// </summary>
    public Command() { }

    /// <summary>
    /// Create a clone of an existing Command
    /// </summary>
    /// <param name="copy">Command to copy</param>
    public Command(Command copy) {
        Id = copy.Id;
        Args = new Parser(copy.Args);
    }

    /// <summary>
    /// Command ID
    /// </summary>
    public KeyCommand Id { get; init; }

    /// <summary>
    /// Command arguments
    /// </summary>
    public Parser Args { get; init; }

    /// <summary>
    /// Read a number from the command line. If none present then prompt
    /// for the input.
    /// </summary>
    /// <param name="prompt">Prompt to display if input is required</param>
    /// <param name="inputValue">Input value</param>
    /// <returns>True if string retrieved, false if the input was cancelled</returns>
    public bool GetNumber(string prompt, out int inputValue) {
        string nextWord = Args.NextWord();
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
        inputValue = Args.NextWord();
        if (string.IsNullOrEmpty(inputValue)) {
            if (!Screen.StatusBar.PromptForInput(prompt, ref inputValue, true)) {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Read input from the command line. If none present then prompt
    /// for the input.
    /// </summary>
    /// <param name="prompt">Prompt to display if input is required</param>
    /// <param name="inputValue">Input value</param>
    /// <returns>True if string retrieved, false if the input was cancelled</returns>
    public bool GetInput(string prompt, ref string inputValue) {
        string nextWord = Args.NextWord();
        if (string.IsNullOrEmpty(nextWord)) {
            if (!Screen.StatusBar.PromptForInput(prompt, ref inputValue, false)) {
                return false;
            }
        }
        else {
            inputValue = nextWord;
        }
        return true;
    }
}