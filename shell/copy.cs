// JShell
// COPY command
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

public static partial class Commands {

    /// <summary>
    /// COPY command: copy a file.
    /// </summary>
    /// <param name="cmdLine">Command line</param>
    /// <returns>True if the copy succeeds, false otherwise</returns>
    public static bool CmdCopy(Parser cmdLine) {

        string sourcefile = cmdLine.NextWord();
        string targetfile = cmdLine.NextWord();
        if (string.IsNullOrEmpty(sourcefile) || string.IsNullOrEmpty(targetfile)) {
            Console.WriteLine(Shell.SyntaxError);
            return false;
        }
        if (!File.Exists(sourcefile)) {
            Console.WriteLine(Shell.SourceFileDoesNotExist, sourcefile);
            return false;
        }
        File.Copy(sourcefile, targetfile);
        return true;
    }
}