// JEdit
// Block action values
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

/// <summary>
/// Block actions
/// </summary>
[Flags]
public enum BlockAction {

    /// <summary>
    /// Get the block
    /// </summary>
    GET = 1,

    /// <summary>
    /// Delete the block
    /// </summary>
    DELETE = 2,

    /// <summary>
    /// Copy the block to the scrap buffer
    /// </summary>
    COPY = GET | 4,

    /// <summary>
    /// Copy the block to the scrap buffer and then delete
    /// </summary>
    CUT = COPY | DELETE,

    /// <summary>
    /// Mark block lower case
    /// </summary>
    LOWER = 8,

    /// <summary>
    /// Mark block upper case
    /// </summary>
    UPPER = 16,

    /// <summary>
    /// Write the block to a file
    /// </summary>
    WRITE = GET | 32,

    /// <summary>
    /// Block indent
    /// </summary>
    INDENT = 64,

    /// <summary>
    /// Block outdent
    /// </summary>
    OUTDENT = 128
}