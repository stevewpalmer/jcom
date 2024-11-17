// calclib
// Window management
//
// Authors:
//  Steve
//
// Copyright (C) 2024 Steve
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

using System.Text.Json.Serialization;
using JComLib;

namespace JCalcLib;

public class CellStyle {

    /// <summary>
    /// Foreground colour
    /// </summary>
    public int? Foreground { get; set; }

    /// <summary>
    /// Foreground colour
    /// </summary>
    [JsonIgnore]
    public int ForegroundColour => Foreground.GetValueOrDefault(DefaultForegroundColour);

    /// <summary>
    /// Background colour
    /// </summary>
    public int? Background { get; set; }

    /// <summary>
    /// Background colour
    /// </summary>
    [JsonIgnore]
    public int BackgroundColour => Background.GetValueOrDefault(DefaultBackgroundColour);

    /// <summary>
    /// Specifies boldface text
    /// </summary>
    public bool Bold { get; set; }

    /// <summary>
    /// Specifies italic text
    /// </summary>
    public bool Italic { get; set; }

    /// <summary>
    /// Specifies Underlined text
    /// </summary>
    public bool Underline { get; set; }

    /// <summary>
    /// Default cell background colour
    /// </summary>
    [JsonIgnore]
    public static int DefaultBackgroundColour = AnsiColour.Black;

    /// <summary>
    /// Default cell background colour
    /// </summary>
    [JsonIgnore]
    public static int DefaultForegroundColour = AnsiColour.BrightWhite;
}