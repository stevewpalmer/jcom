// JEdit
// Built-in and custom compiler commands
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

public class Compiler {

    /// <summary>
    /// Name of the executable for the compiler.
    /// </summary>
    public string ProgramName { get; private init; } = "";

    /// <summary>
    /// Command-line for the compiler with placeholders.
    /// </summary>
    public string CommandLine { get; private set; } = "";

    /// <summary>
    /// Extensions associated with this compiler
    /// </summary>
    private string[] Extensions { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Built-in compilers.
    /// </summary>
    private static readonly Compiler[] BuiltIn = {
        new() { Extensions = new []{ "f", "f90" }, ProgramName = "for", CommandLine = "--run {0}"},
        new() { Extensions = new [] { "cml", "lst" }, ProgramName = "comal", CommandLine = "--run {0}"},
    };

    /// <summary>
    /// Return the Compiler object associated with the given extension, or null if
    /// there is none.
    /// </summary>
    /// <param name="extension">Filename extension</param>
    /// <returns>Compiler object, or null</returns>
    public static Compiler? CompilerForExtension(string extension) {
        if (extension.StartsWith('.')) {
            extension = extension[1..];
        }
        return BuiltIn.FirstOrDefault(comp => comp.Extensions.Contains(extension));
    }
}