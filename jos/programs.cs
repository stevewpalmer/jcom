// JOs
// Program commands
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

namespace jOS {

	public partial class Commands {

        // Run a program.
        static internal bool RunProgram(string programName, CommandLine cmdLine) {

            string homeFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string josBinRoot = $"{homeFolder}/jos/bin";

            Process process = new();
            process.StartInfo.FileName = $"{josBinRoot}/{programName}";
            process.StartInfo.Arguments = string.Join(' ', cmdLine.RestOfLine());
            process.Start();
            process.WaitForExit();
            return true;
        }

        // Run the Fortran compiler.
        static public bool CmdFortran(CommandLine cmdLine) {
            return RunProgram("for", cmdLine);
        }

        // Run the Comal interpreter/compiler.
        static public bool CmdComal(CommandLine cmdLine) {
            return RunProgram("comal", cmdLine);
        }
    }
}

