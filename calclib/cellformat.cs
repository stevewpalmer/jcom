// JCalcLib
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

using System.ComponentModel;

namespace JCalcLib;

public enum CellFormat {

    /// <summary>
    /// General - contents are formatted according to type
    /// </summary>
    [Description("G")]
    GENERAL,

    /// <summary>
    /// Exponential number
    /// </summary>
    [Description("S")]
    SCIENTIFIC,

    /// <summary>
    /// Fixed format
    /// </summary>
    [Description("F")]
    FIXED,

    /// <summary>
    /// Currency
    /// </summary>
    [Description("C")]
    CURRENCY,

    /// <summary>
    /// Percentage
    /// </summary>
    [Description("P")]
    PERCENT,

    /// <summary>
    /// Text format
    /// </summary>
    [Description("R")]
    TEXT,

    /// <summary>
    /// Date in Day-Month-Year format
    /// </summary>
    [Description("D1")]
    DATE_DMY,

    /// <summary>
    /// Date in Month-Year format
    /// </summary>
    [Description("D3")]
    DATE_MY,

    /// <summary>
    /// Date in Day-Month format
    /// </summary>
    [Description("D2")]
    DATE_DM,

    /// <summary>
    /// Custom format
    /// </summary>
    [Description("Z")]
    CUSTOM,

    /// <summary>
    /// Time in Hours-Minutes-Seconds 12-hour format
    /// </summary>
    [Description("T1")]
    TIME_HMSZ,

    /// <summary>
    /// Time in Hours-Minutes 24-hour format
    /// </summary>
    [Description("T2")]
    TIME_HM,

    /// <summary>
    /// Time in Hours-Minutes-Seconds 24-hour format
    /// </summary>
    [Description("T3")]
    TIME_HMS,

    /// <summary>
    /// Time in Hours-Minutes 12-hour format
    /// </summary>
    [Description("T4")]
    TIME_HMZ
}