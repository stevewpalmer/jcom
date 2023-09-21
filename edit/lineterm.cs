// JEdit
// Line terminator types
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

public enum LineTerminator {

    /// <summary>
    /// No line terminator determined.
    /// </summary>
    NONE,

    /// <summary>
    /// Carriage return terminator
    /// (Typically Mac System text files)
    /// </summary>
    CR,

    /// <summary>
    /// Linefeed terminator
    /// (Typically Unix/Linux text files)
    /// </summary>
    LF,

    /// <summary>
    /// Carriage return and linefeed terminator.
    /// (Typically Windows text files)
    /// </summary>
    CRLF
}