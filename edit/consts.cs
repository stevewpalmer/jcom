// JEdit
// Global constants
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

namespace JEdit;

public static class Consts {

    /// <summary>
    /// Maximum number of keystrokes that can be recorded.
    /// </summary>
    public const int MaxKeystrokes = 350;

    /// <summary>
    /// File extension for macro files.
    /// </summary>
    public const string MacroExtension = ".km";

    /// <summary>
    /// Configuration filename.
    /// </summary>
    public const string ConfigurationFilename = "edit.json";

    /// <summary>
    /// Maximum number of commands remembered in history
    /// </summary>
    public const int MaxCommandHistory = 10;

    /// <summary>
    /// Internal end-of-line character.
    /// </summary>
    public const char EndOfLine = '\n';

    /// <summary>
    /// Backup file extension
    /// </summary>
    public const string BackupExtension = ".bak";
}