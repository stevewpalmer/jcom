// JFortran Compiler
// Main program class
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
using System.Diagnostics;
using System.IO;
using CCompiler;

namespace JFortran {

    /// <summary>
    /// This is the main driver file for the compiler and the only part that
    /// interacts with the user directly. It constructs the compiler object
    /// using options specified, compiles the files given and generates an
    /// output file if no errors were found.
    /// 
    /// All output is controlled by a central MessageCollection created
    /// here and donated to the Options and Compiler class.
    /// 
    /// Keep this module short and compact. It should really do as little
    /// as possible.
    /// </summary>
    internal static class Program {

        private static void Main(string[] args) {

            FortranOptions opts = new();
            MessageCollection messages = new(opts);

            opts.Messages = messages;
            if (opts.Parse(args)) {
                Compiler comp = new(opts) {
                    Messages = messages
                };

                try {
                    foreach (string srcfile in opts.SourceFiles) {
                        if (!File.Exists(srcfile)) {
                            messages.Error(MessageCode.SOURCEFILENOTFOUND, $"File '{srcfile}' not found");
                            break;
                        }
                        comp.Compile(srcfile);
                    }
                    if (messages.ErrorCount == 0) {
                        comp.Save();
                        if (opts.Run && messages.ErrorCount == 0) {
                            comp.Execute();
                        }
                    }
                }
                catch (Exception) { }
            }
            foreach (Message msg in messages) {
                if (msg.Level == MessageLevel.Error) {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                Console.WriteLine(msg);
                Console.ResetColor();
            }
            if (messages.ErrorCount > 0) {
                Console.WriteLine("*** {0} errors found. Compilation stopped.", messages.ErrorCount);
            }
        }
    }
}
