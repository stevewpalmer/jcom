// JShell
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

using JComLib;

namespace JShell;

internal static class Program {

    private record CommandDefinition {
        public Func<Parser, bool> Function;
        public string Description;
    }

    // Command table
    private static readonly Dictionary<string, CommandDefinition> CommandMap = new() {
        { "comal", new CommandDefinition { Function = Commands.CmdComal, Description = "Run the Comal compiler/interpreter" } },
        { "fortran", new CommandDefinition { Function = Commands.CmdFortran, Description = "Run the Fortran compiler" } },
        { "edit", new CommandDefinition { Function = Commands.CmdEdit, Description = "Create or edit a file" } },
        { "dir", new CommandDefinition { Function = Commands.CmdDir, Description = "Display a list of files" } },
        { "type", new CommandDefinition { Function = Commands.CmdType, Description = "Display the content of a file" } },
        { "help", new CommandDefinition { Function = CmdHelp, Description = "Display this help" } }
    };

    private static void Main() {

        Console.WriteLine($"{AssemblySupport.AssemblyDescription} {AssemblySupport.AssemblyVersion}");
        Console.WriteLine($"{AssemblySupport.AssemblyCopyright}");
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
            Parser cmdLine = new(inputLine);
            string command = cmdLine.NextWord();
            while (command != null) {

                if (CommandMap.TryGetValue(command.ToLower(), out CommandDefinition commandFunc)) {
                    commandFunc.Function(cmdLine);
                } else {
                    Console.WriteLine($"Unknown command {command}");
                    break;
                }
                command = cmdLine.NextWord();
            }
        }
    }

    // HELP command.
    // Display a list of all commands.
    private static bool CmdHelp(Parser cmdLine) {

        int maxLength = CommandMap.Keys.Max(k => k.Length);
        foreach (string key in CommandMap.Keys.OrderBy(k => k)) {
            Console.WriteLine(key.PadRight(maxLength) + " ... " + CommandMap[key].Description);
        }
        return true;
    }
}
