// JOs
// Main program
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

using CCompiler;
using JComLib;

namespace JShell;

static class Program {

    // Command table
    private static readonly Dictionary<string, Func<CommandLine, bool>> commandMap = new() {
        { "comal", Commands.CmdComal },
        { "fortran", Commands.CmdFortran },
        { "dir", Commands.CmdDir },
        { "type", Commands.CmdType },
    };

    static void Main(string[] args) {

        Console.WriteLine($"{Options.AssemblyDescription} {Options.AssemblyVersion}");
        Console.WriteLine($"{Options.AssemblyCopyright}");
        Console.WriteLine();

        // Ensure we have a home folder and set it as the default.
        string homeFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string josRoot = $"{homeFolder}/jos/home";
        if (!Directory.Exists(josRoot)) {
            Directory.CreateDirectory(josRoot);
        }
        Directory.SetCurrentDirectory(josRoot);

        while (true) {
            Console.Write("$ ");
            ReadLine readLine = new() {
                AllowHistory = true
            };
            string inputLine = readLine.Read(string.Empty);
            if (inputLine == null) {
                continue;
            }
            CommandLine cmdLine = new(inputLine);
            string command = cmdLine.NextWord();
            while (command != null) {

                if (commandMap.TryGetValue(command.ToLower(), out var commandFunc)) {
                    commandFunc(cmdLine);
                } else {
                    Console.WriteLine($"Unknown command {command}");
                    break;
                }
                command = cmdLine.NextWord();
            }
        }
    }
}