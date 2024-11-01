// JCalc
// Global constants
//
// Authors:
//  Steve Palmer
//
// Copyright (C) 2024 Steve Palmer
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

namespace JCalc;

public static class Consts {

    /// <summary>
    /// Configuration filename.
    /// </summary>
    public const string ConfigurationFilename = "calc.json";

    /// <summary>
    /// Backup file extension
    /// </summary>
    public const string BackupExtension = ".bak";

    /// <summary>
    /// Default filename (for an empty sheet)
    /// </summary>
    public const string DefaultFilename = "temp";

    /// <summary>
    /// Maximum number of columns
    /// </summary>
    public const int MaxColumns = 255;

    /// <summary>
    /// Maximum number of rows
    /// </summary>
    public const int MaxRows = 4096;

    /// <summary>
    /// Default column width
    /// </summary>
    public const int DefaultColumnWidth = 10;
}