// JComal
// Command line options
//
// Authors:
//  Steve Palmer
//
// Copyright (C) 2021 Steve Palmer
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

using CCompiler;
using JComLib;

namespace JComal;

/// <summary>
/// Class that extends the <c>Options</c> class with additional
/// options used by the interpreter and compiler.
/// </summary>
public class ComalOptions : Options {

    /// <summary>
    /// Sets and returns whether the compiler is operating in interactive
    /// mode or as an interpreter.
    /// </summary>
    public bool Interactive { get; set; }

    /// <summary>
    /// Sets and returns whether the strict mode is enforced.
    /// </summary>
    [OptionField("-strict", Help = "Enable strict mode")]
    public bool Strict { get; init; }

    /// <summary>
    /// Sets and returns whether the compiler is running in IDE mode. In
    /// IDE mode, line numbers in the error and warning messages relate
    /// to the physical line position, not the COMAL line numbers.
    /// </summary>
    [OptionField("-ide", Help = "Enable IDE mode")]
    public bool IDE { get; init; }

    /// <summary>
    /// Parse the specified command line array passed to the application.
    /// Any errors are added to the error list. Also note that the first
    /// argument in the list is assumed to be the application name.
    /// </summary>
    /// <param name="arguments">Array of string arguments</param>
    /// <returns>True if the arguments are valid, false otherwise</returns>
    public override bool Parse(string[] arguments) {
        if (!base.Parse(arguments)) {
            return false;
        }

        for (int c = 0; c < SourceFiles.Count; c++) {
            SourceFiles[c] = Utilities.AddExtensionIfMissing(SourceFiles[c], Consts.SourceFileExtension);
        }
        if (SourceFiles.Count == 0) {
            Interactive = true;
        }
        return true;
    }
}