// JCalcLib
// Cell factory
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

using JComLib;

namespace JCalcLib;

/// <summary>
/// A CellFactory encapsulates default values for the properties new cells.
/// Changing CellFactory values only affects cells that are created after
/// the change. Existing cells are not affected. Changes are not persisted
/// but must be set each time the program is run.
/// </summary>
public static class CellFactory {

    /// <summary>
    /// Default number of decimal places
    /// </summary>
    public static int DecimalPlaces { get; set; } = 2;

    /// <summary>
    /// Default cell background colour
    /// </summary>
    public static int BackgroundColour { get; set; } = AnsiColour.Black;

    /// <summary>
    /// Default cell text colour
    /// </summary>
    public static int TextColour { get; set; } = AnsiColour.BrightWhite;

    /// <summary>
    /// Default cell alignment
    /// </summary>
    public static CellAlignment Alignment { get; set; } = CellAlignment.GENERAL;

    /// <summary>
    /// Default cell format
    /// </summary>
    public static CellFormat Format { get; set; } = CellFormat.GENERAL;
}