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

using System.Diagnostics;
using JComLib;

namespace JShell;

internal static class Program {

    private record CommandDefinition(Func<Parser, bool> Function, string Description) {
        public readonly Func<Parser, bool> Function = Function;
        public readonly string Description = Description;
    }

    // Command table
    private static readonly Dictionary<string, CommandDefinition> CommandMap = new() {
        { "accounts", new CommandDefinition(Commands.CmdAccounts, "Run the Accounts program") },
        { "comal", new CommandDefinition(Commands.CmdComal, "Run the Comal compiler/interpreter") },
        { "copy", new CommandDefinition(Commands.CmdCopy, "Copy a file") },
        { "del", new CommandDefinition(Commands.CmdDel, "Delete files") },
        { "dir", new CommandDefinition(Commands.CmdDir, "Display a list of files") },
        { "edit", new CommandDefinition(Commands.CmdEdit, "Create or edit a file") },
        { "for", new CommandDefinition(Commands.CmdFortran, "Run the Fortran compiler") },
        { "help", new CommandDefinition(CmdHelp, "Display this help") },
        { "rename", new CommandDefinition(Commands.CmdRename, "Rename a file") },
        { "type", new CommandDefinition(Commands.CmdType, "Display the content of a file") }
    };

    /// <summary>
    /// Shell home folder, where user files are stored
    /// </summary>
    public static string HomeFolder { get; private set; } = "";

    /// <summary>
    /// Shell binary folder, where executables are stored
    /// </summary>
    public static string BinaryFolder { get; private set; } = "";

    private static void Main() {

        Console.WriteLine($@"{AssemblySupport.AssemblyDescription} {AssemblySupport.AssemblyVersion}");
        Console.WriteLine($@"{AssemblySupport.AssemblyCopyright}");
        Console.WriteLine();

        // Ensure we have a home folder and set it as the default.
        // Get the full name path of the executable file
        ProcessModule? mainModule = Process.GetCurrentProcess().MainModule;
        if (mainModule == null) {
            return;
        }
        BinaryFolder = Path.GetDirectoryName(mainModule.FileName) ?? string.Empty;
        if (BinaryFolder == string.Empty) {
            return;
        }
        HomeFolder = $"{Directory.GetParent(BinaryFolder)?.FullName}/home";
        if (!Directory.Exists(HomeFolder)) {
            Directory.CreateDirectory(HomeFolder);
        }
        Directory.SetCurrentDirectory(HomeFolder);

        while (true) {
            Console.Write(@"$ ");
            ReadLine readLine = new() {
                AllowHistory = true,
                AllowFilenameCompletion = true
            };
            string inputLine = readLine.Read(string.Empty);
            if (inputLine == null) {
                continue;
            }
            Parser cmdLine = new(inputLine);
            string command = cmdLine.NextWord();
            while (command != null) {

                if (CommandMap.TryGetValue(command.ToLower(), out CommandDefinition? commandFunc)) {
                    try {
                        commandFunc.Function(cmdLine);
                    }
                    catch (Exception e) {
                        Console.WriteLine(e.Message);
                        break;
                    }
                }
                else {
                    Console.WriteLine(Shell.UnknownCommand, command);
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
            Console.WriteLine(key.PadRight(maxLength) + @" ... " + CommandMap[key].Description);
        }
        return true;
    }
}