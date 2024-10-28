// JCalc
// Render hint flags
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

[Flags]
public enum RenderHint {

    /// <summary>
    /// No changes needed
    /// </summary>
    NONE = 0,

    /// <summary>
    /// Update the current window
    /// </summary>
    REDRAW = 1,

    /// <summary>
    /// Cursor position update needed
    /// </summary>
    CURSOR = 2,

    /// <summary>
    /// Cursor position on the command bar needs updating
    /// </summary>
    CURSOR_STATUS = 4,

    /// <summary>
    /// Exit the program
    /// </summary>
    EXIT = 8,

    /// <summary>
    /// Update the filename on the command bar
    /// </summary>
    TITLE = 16,

    /// <summary>
    /// Refresh the entire screen
    /// </summary>
    REFRESH = 32
}