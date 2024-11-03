// JCalc
// Cell format types
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

public enum CellFormat {

    /// <summary>
    /// General - contents are formatted according to type
    /// </summary>
    GENERAL,

    /// <summary>
    /// Exponential number
    /// </summary>
    SCIENTIFIC,

    /// <summary>
    /// Fixed format
    /// </summary>
    FIXED,

    /// <summary>
    /// Commas inserted
    /// </summary>
    COMMAS,

    /// <summary>
    /// Currency
    /// </summary>
    CURRENCY,

    /// <summary>
    /// Percentage
    /// </summary>
    PERCENT,

    /// <summary>
    /// Horizontal bar
    /// </summary>
    BAR,

    /// <summary>
    /// Text format
    /// </summary>
    TEXT,

    /// <summary>
    /// Date in Day-Month-Year format
    /// </summary>
    DATE_DMY,

    /// <summary>
    /// Date in Month-Year format
    /// </summary>
    DATE_MY,

    /// <summary>
    /// Date in Day-Month format
    /// </summary>
    DATE_DM,
}