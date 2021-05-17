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

using System.IO;
using CCompiler;

namespace JFortran {

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
        }

        /// <value>
        /// Sets and returns whether backslashes are normal characters rather
        /// than an escape sequence delimiter.
        /// </value>
        public bool Backslash { get; set; }
        
        /// <value>
        /// Sets and returns whether the source code should be compiled using
        /// the FORTRAN 77 compiler.
        /// </value>
        public bool F77 { get; set; }
        
        /// <value>
        /// Sets and returns whether the source code should be compiled using
        /// the Fortran 90 compiler.
        /// </value>
        public bool F90 { get; set; }

        /// <summary>
        /// Parse the specified command line array passed to the application.
        /// Any errors are added to the error list. Also note that the first
        /// argument in the list is assumed to be the application name.
        /// </summary>
        /// <param name="arguments">Array of string arguments</param>
        /// <returns>True if the arguments are valid, false otherwise</returns>
        public bool Parse(string[] arguments) {
            bool success = true;
            bool possibleF90 = false;
            bool stopParse = false;

            foreach (string optstring in arguments) {
                if (optstring.StartsWith("-")) {
                    string [] opts = optstring.Substring(1).ToLower().Split(':');
                    if (opts[0] == "backslash") {
                        Backslash = true;
                    } else if (opts[0] == "help" || opts[0] == "?") {
                        DisplayHelp();
                        stopParse = true;
                    } else if (opts[0] == "debug") {
                        GenerateDebug = true;
                    } else if (opts[0] == "warnaserror") {
                        WarnAsError = true;
                    } else if (opts[0] == "warn" || opts[0] == "w") {
                        if (opts.Length < 2 || !int.TryParse(opts[1], out int level) || level < 0 || level > 4) {
                            Messages.Error(MessageCode.BADWARNLEVEL, "Missing or out of range warning level");
                        } else {
                            WarnLevel = level;
                        }
                    } else if (opts[0] == "dump") {
                        Dump = true;
                    } else if (opts[0] == "f77") {
                        if (F90) {
                            Messages.Error(MessageCode.BADCOMPILEROPT, "Cannot specify both -f77 and -f90");
                        }
                        F77 = true;
                        F90 = false;
                    } else if (opts[0] == "f90") {
                        if (F77) {
                            Messages.Error(MessageCode.BADCOMPILEROPT, "Cannot specify both -f77 and -f90");
                        }
                        F77 = false;
                        F90 = true;
                    } else if (opts[0] == "dev") {
                        DevMode = (opts.Length < 2) || opts[1] == "1" || opts[1] == "yes";
                    } else if (opts[0] == "run") {
                        Run = true;
                    } else if (opts[0] == "version" || opts[0] == "v") {
                        DisplayVersion();
                        stopParse = true;
                    } else if (opts[0] == "noinline") {
                        Inline = false;
                    } else if (opts[0] == "out") {
                        if (opts.Length < 2 || string.IsNullOrEmpty(opts[1])) {
                            Messages.Error(MessageCode.NOOUTPUTFILE, "Missing output file name");
                            success = false;
                        } else {
                            OutputFile = opts[1];
                        }
                    } else {
                        Messages.Error(MessageCode.BADOPTION, $"Unknown option: {optstring}");
                        success = false;
                    }
                } else {
                    // It's a source file
                    string ext = Path.GetExtension(optstring);
                    string extLower = ext.ToLower();
                    if (extLower != ".f" && extLower != ".for" && extLower != ".f90") {
                        Messages.Error(MessageCode.BADEXTENSION, $"Source file {optstring} must have .f extension");
                        success = false;
                    }
                    if (extLower == ".f90") {
                        possibleF90 = true;
                    }
                    SourceFiles.Add(optstring);
                }
                if (stopParse) {
                    return false;
                }
            }
            if (SourceFiles.Count == 0) {
                Messages.Error(MessageCode.MISSINGSOURCEFILE, "Missing input source file");
                success = false;
            }
            if (!F77 && !F90) {
                if (possibleF90) {
                    F90 = true;
                } else {
                    F77 = true;
                }
            }
            return success;
        }

        // Display the compiler Help page
        private void DisplayHelp() {
            Messages.Info(AssemblyDescription() + ", " + AssemblyCopyright()  + "\n" +
                           ExecutableFilename() + " [options] source-files\n" +
                           "   -backslash          Permit C style escapes in strings\n" +
                           "   -debug              Generate debugging information\n" +
                           "   -f77                Compile FORTRAN 77 source (default)\n" +
                           "   -f90                Compile Fortran 90 source\n" +
                           "   -help               Lists all compiler options (short: -?)\n" +
                           "   -noinline           Don't inline intrinsic calls\n" +
                           "   -out:FILE           Specifies output executable name\n" +
                           "   -run                Run the executable if no errors\n" +
                           "   -version            Display compiler version (short: -v)\n" +
                           "   -warn:0-4           Sets warning level, the default is 4 (short -w:)\n" +
                           "   -warnaserror        Treats all warnings as errors\n");
        }
    }
}