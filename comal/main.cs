// JComal
// Main program
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

namespace JComal; 

public static class Program {

    public static void Main(string[] args) {
        ComalOptions opts = new();
        MessageCollection messages = new(opts);
        int exitCode = 0;

        opts.Messages = messages;
        if (opts.Parse(args)) {

            if (opts.Interactive) {
                Interpreter interpreter = new();
                interpreter.Run(opts);
            } else {
                Compiler comp = new(opts) {
                    Messages = messages
                };

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
        }
        foreach (Message msg in messages) {
            if (msg.Level == MessageLevel.Error) {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            Console.Error.WriteLine(msg);
            Console.ResetColor();
        }
        if (messages.ErrorCount > 0) {
            Console.WriteLine("*** {0} errors found. Compilation stopped.", messages.ErrorCount);
            exitCode = 1;
        }
        Environment.ExitCode = exitCode;
    }
}
