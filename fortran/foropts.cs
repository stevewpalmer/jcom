// JFortran Compiler
// JFor command line options
//
// Authors:
//  Steve Palmer
//
// Copyright (C) 2013 Steve Palmer
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

namespace JFortran;

/// <summary>
/// Class that extends the <c>Options</c> class with additional
/// options used by the Fortran compiler.
/// </summary>
public class FortranOptions : Options {

    /// <summary>
    /// Initialises an instance of the <c>FortranOptions</c> class.
    /// </summary>
    public FortranOptions() {
        Backslash = false;
        F90 = false;
    }

    /// <value>
    /// Sets and returns whether backslashes are normal characters rather
    /// than an escape sequence delimiter.
    /// </value>
    [OptionField("-backslash", Help = "Permit C style escapes in strings")]
    public bool Backslash { get; set; }

    /// <value>
    /// Sets and returns whether the source code should be compiled using
    /// the Fortran 90 compiler. This is not a settable option but is
    /// determined by the file extension.
    /// </value>
    public bool F90 { get; private set; }

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

        if (SourceFiles.Count == 0) {
            Messages.Error(MessageCode.MISSINGSOURCEFILE, "Missing input source file");
            return false;
        }

        // If file extension is .f90 then compile in Fortran 90 mode.
        foreach (string sourceFile in SourceFiles) {
            string ext = Path.GetExtension(sourceFile);
            string extLower = ext.ToLower();
            if (extLower != ".f" && extLower != ".for" && extLower != ".f90") {
                Messages.Error(MessageCode.BADEXTENSION, $"Source file {sourceFile} must have .f extension");
                return false;
            }
            if (extLower == ".f90") {
                F90 = true;
            }
        }
        return true;
    }
}