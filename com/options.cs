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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace CCompiler {

    /// <summary>
    /// Used to mark Option attributes
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class OptionField : Attribute {

        // ReSharper disable UnusedMember.Global
        public OptionField(string name) {
            Name = name;
        }
        // ReSharper restore UnusedMember.Global

        public OptionField() {
            Name = string.Empty;
        }

        /// <summary>
        /// Option input name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Option input short name
        /// </summary>
        public string ShortName { get; set; }

        /// <summary>
        /// For options that take an argument, the name of the argument
        /// </summary>
        public string ArgName { get; set; }

        /// <summary>
        /// Help text
        /// </summary>
        public string Help { get; set; }

        /// <summary>
        /// Minimum range of argument
        /// </summary>
        public int MinRange { get; set; }

        /// <summary>
        /// Maximum range of argument
        /// </summary>
        public int MaxRange { get; set; }
    }

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
        [OptionField("debug", Help= "Generate debugging information")]
        public bool GenerateDebug { get; set; }

        /// <value>
        /// Sets and returns the compiler warning level where 0 means no warnings
        /// and 4 equates to all warnings.
        /// </value>
        [OptionField("warn", ShortName="w", ArgName="NUM", MinRange=0, MaxRange = 4, Help = "Sets warning level, the default is 4")]
        public int WarnLevel { get; set; }

        /// <value>
        /// Sets and returns whether warnings should be treated as errors.
        /// </value>
        [OptionField("warnaserror", Help = "Treats all warnings as errors")]
        public bool WarnAsError { get; set; }

        /// <value>
        /// Sets and returns whether we dump compiler debugging information
        /// </value>
        [OptionField("dump", Help = "Output compiler debugging information to a file")]
        public bool Dump { get; set; }

        /// <value>
        /// Sets and returns whether some intrinsic calls are inlined.
        /// </value>
        [OptionField("noinline", Help = "Don't inline intrinsic calls")]
        public bool Inline { get; set; }

        /// <value>
        /// Sets and returns whether the generated program is to run after complication.
        /// </value>
        [OptionField("run", Help = "Run the executable if no errors")]
        public bool Run { get; set; }

        /// <value>
        /// Sets and returns whether the compiler is operating in development mode (and
        /// thus doing diagnostic stuff to aid debugging).
        /// </value>
        [OptionField("dev")]
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
        [OptionField("out", ArgName="FILE", Help = "Specifies output executable name")]
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

        /// <summary>
        /// Parse the specified command line array passed to the application.
        /// Any errors are added to the error list. Also note that the first
        /// argument in the list is assumed to be the application name.
        /// </summary>
        /// <param name="arguments">Array of string arguments</param>
        /// <returns>True if the arguments are valid, false otherwise</returns>
        public virtual bool Parse(string[] arguments) {
            PropertyInfo[] props = GetType().GetProperties();

            foreach (string optstring in arguments) {
                if (optstring.StartsWith("-")) {
                    string[] opts = optstring.Substring(1).ToLower().Split(':');
                    if (opts[0] == "help" || opts[0] == "?") {

                        StringBuilder help = new();
                        help.AppendLine(AssemblyDescription + " " + AssemblyVersion + " " + AssemblyCopyright);
                        help.AppendLine(ExecutableFilename() + " [options] [source-files]");

                        help.AppendLine("   -help               Lists all compiler options (short: -?)");
                        help.AppendLine("   -version            Display compiler version (short: -v)");

                        foreach (PropertyInfo prop in props) {

                            if (Attribute.IsDefined(prop, typeof(OptionField))) {
                                if (Attribute.GetCustomAttribute(prop, typeof(OptionField)) is OptionField da) {

                                    string optionDescription = da.Help;
                                    if (!string.IsNullOrEmpty(da.Help)) {
                                        string name = da.Name;
                                        string helptext = da.Help;
                                        if (da.ArgName != null) {
                                            name += ":" + da.ArgName;
                                        }
                                        if (da.ShortName != null) {
                                            helptext += $" (short: -{da.ShortName})";
                                        }
                                        help.AppendLine(string.Format("   -{0,-18} {1}", name, helptext));
                                    }
                                }
                            }
                        }
                        Messages.Info(help.ToString());
                        return false;
                    }
                    if (opts[0] == "version" || opts[0] == "v") {
                        DisplayVersion();
                        return false;
                    }

                    bool validOption = false;
                    foreach (PropertyInfo prop in props) {
                        if (Attribute.IsDefined(prop, typeof(OptionField))) {
                            if (Attribute.GetCustomAttribute(prop, typeof(OptionField)) is OptionField da) {
                                if (opts[0] == da.Name || opts[0] == da.ShortName) {

                                    if (prop.PropertyType == typeof(bool)) {
                                        prop.SetValue(this, true);
                                    }
                                    if (prop.PropertyType == typeof(int)) {
                                        if (opts.Length < 2 || !int.TryParse(opts[1], out int value) || value < da.MinRange || value > da.MaxRange) {
                                            Messages.Error(MessageCode.BADARGUMENTRANGE, $"Value for option {opts[0]} out of range {da.MinRange}-{da.MaxRange}");
                                            return false;
                                        }
                                        prop.SetValue(this, value);
                                    }
                                    if (prop.PropertyType == typeof(string)) {
                                        if (opts.Length < 2 || string.IsNullOrEmpty(opts[1])) {
                                            Messages.Error(MessageCode.MISSINGOPTIONVALUE, $"Missing value for option {opts[0]}");
                                            return false;
                                        }
                                        prop.SetValue(this, opts[1]);
                                    }
                                    validOption = true;
                                }
                            }
                        }
                    }
                    if (!validOption) {
                        Messages.Error(MessageCode.BADOPTION, $"Invalid option -{opts[0]}");
                        return false;
                    }
                } else {
                    SourceFiles.Add(optstring);
                }
            }
            return true;
        }
    }
}