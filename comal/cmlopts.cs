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

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using CCompiler;

namespace JComal {

    /// <summary>
    /// Class that extends the <c>Options</c> class with additional
    /// options used by the interpreter and compiler.
    /// </summary>
    public class ComalOptions : Options {

        /// <summary>
        /// Initialises an instance of the <c>ComalOptions</c> class.
        /// </summary>
        public ComalOptions() {
            Messages = new MessageCollection(this);
        }

        /// <value>
        /// Return or set the list of compiler messages.
        /// </value>
        public MessageCollection Messages { get; set; }

        /// <summary>
        /// Sets and returns whether the compiler is operating in interactive
        /// mode or as an interpreter.
        /// </summary>
        public bool Interactive { get; set; }

        /// <summary>
        /// Sets and returns whether the strict mode is enforced.
        /// </summary>
        public bool Strict { get; set; }

        /// <summary>
        /// Parse the specified command line array passed to the application.
        /// Any errors are added to the error list. Also note that the first
        /// argument in the list is assumed to be the application name.
        /// </summary>
        /// <param name="arguments">Array of string arguments</param>
        /// <returns>True if the arguments are valid, false otherwise</returns>
        public bool Parse(string[] arguments) {
            bool success = true;
            bool stopParse = false;

            foreach (string optstring in arguments) {
                if (optstring.StartsWith("-")) {
                    string [] opts = optstring.Substring(1).ToLower().Split(':');
                    if (opts[0] == "help" || opts[0] == "?") {
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
                    } else if (opts[0] == "strict") {
                        Strict = true;
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
                    SourceFiles.Add(optstring);
                }
                if (stopParse) {
                    return false;
                }
            }
            if (SourceFiles.Count == 0) {
                Interactive = true;
            }
            return success;
        }

        // Return the assembly copyright from the assemblyinfo module
        private string AssemblyCopyright() {
            object[] attributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            Debug.Assert(attributes.Length > 0);
            return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
        }

        // Return the assembly description from the assemblyinfo module
        private string AssemblyDescription() {
            object[] attributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
            Debug.Assert(attributes.Length > 0);
            return ((AssemblyDescriptionAttribute)attributes[0]).Description;
        }

        // Return this executable filename.
        private string ExecutableFilename() {
            return Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location);
        }

        // Display the current version number
        private void DisplayVersion() {
            Version ver = Assembly.GetEntryAssembly().GetName().Version;
            Messages.Info($"{ver.Major}.{ver.Minor}.{ver.Build}");
        }

        // Display the compiler Help page
        private void DisplayHelp() {
            Messages.Info(AssemblyDescription() + ", " + AssemblyCopyright()  + "\n" +
                           ExecutableFilename() + " [options] source-files\n" +
                           "   -debug              Generate debugging information\n" +
                           "   -help               Lists all compiler options (short: -?)\n" +
                           "   -noinline           Don't inline intrinsic calls\n" +
                           "   -out:FILE           Specifies output executable name\n" +
                           "   -run                Run the executable if no errors\n" +
                           "   -strict             Enable strict mode\n" +
                           "   -version            Display compiler version (short: -v)\n" +
                           "   -warn:0-4           Sets warning level, the default is 4 (short -w:)\n" +
                           "   -warnaserror        Treats all warnings as errors\n");
        }
    }
}