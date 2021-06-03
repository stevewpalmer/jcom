// JCom Compiler Toolkit
// Options class
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

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace CCompiler {

    /// <summary>
    /// Class that encapsulates options used by all compilers. This class should
    /// always be inherited with additional options specific to that compiler.
    /// </summary>
    public class Options {
        private string _outputFile;

        /// <summary>
        /// Initialises an instance of the <c>Options</c> class.
        /// </summary>
        public Options() {
            GenerateDebug = false;
            WarnLevel = 2;
            WarnAsError = false;
            Dump = false;
            Inline = true;
            Run = false;
            DevMode = false;
            VersionString = "1.0.0.0";
            Messages = new MessageCollection(this);
        }

        /// <value>
        /// Return or set the list of compiler messages.
        /// </value>
        public MessageCollection Messages { get; set; }

        /// <value>
        /// Sets and returns whether debuggable code is enabled.
        /// </value>
        public bool GenerateDebug { get; set; }
        
        /// <value>
        /// Sets and returns the compiler warning level where 0 means no warnings
        /// and 4 equates to all warnings.
        /// </value>
        public int WarnLevel { get; set; }

        /// <value>
        /// Sets and returns whether warnings should be treated as errors.
        /// </value>
        public bool WarnAsError { get; set; }

        /// <value>
        /// Sets and returns whether we dump compiler debugging information
        /// </value>
        public bool Dump { get; set; }

        /// <value>
        /// Sets and returns whether some intrinsic calls are inlined.
        /// </value>
        public bool Inline { get; set; }

        /// <value>
        /// Sets and returns whether the generated program is to run after complication.
        /// </value>
        public bool Run { get; set; }

        /// <value>
        /// Sets and returns whether the compiler is operating in development mode (and
        /// thus doing diagnostic stuff to aid debugging).
        /// </value>
        public bool DevMode { get; set; }

        /// <value>
        /// Sets and returns the version string to be embedded in the program.
        /// </value>
        public string VersionString { get; set; }

        /// <value>
        /// Return an array of all source files detected on the command line
        /// </value>
        public Collection<string> SourceFiles { get; } = new();

        /// <value>
        /// Gets or sets the output file name.
        /// </value>
        public string OutputFile {
            get {
                if (_outputFile != null) {
                    return _outputFile;
                }
                if (SourceFiles.Count > 0) {
                    return Path.ChangeExtension(SourceFiles[0], null);
                }
                return string.Empty;
            }
            set => _outputFile = value;
        }

        /// <summary>
        /// Return the assembly copyright
        /// </summary>
        public static string AssemblyCopyright {
            get {
                object[] attributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                Debug.Assert(attributes.Length > 0);
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        /// <summary>
        /// Return the assembly description
        /// </summary>
        public static string AssemblyDescription {
            get {
                object[] attributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                Debug.Assert(attributes.Length > 0);
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        /// <summary>
        /// Return the assembly version.
        /// </summary>
        public static string AssemblyVersion {
            get {
                Version ver = Assembly.GetEntryAssembly().GetName().Version;
                return $"{ver.Major}.{ver.Minor}.{ver.Build}";
            }
        }

        /// <summary>
        /// Return this executable filename.
        /// </summary>
        /// <returns>Executable filename string</returns>
        public static string ExecutableFilename() => Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location);

        /// <summary>
        /// Display the current version number
        /// </summary>
        public void DisplayVersion() {
            Version ver = Assembly.GetEntryAssembly().GetName().Version;
            Messages.Info($"{ver.Major}.{ver.Minor}.{ver.Build}");
        }
    }
}