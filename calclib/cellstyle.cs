// JCalcLib
// Cell styles
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

using System.Text.Json.Serialization;

namespace JCalcLib;

public class CellStyle(Sheet? sheet) {

    /// <summary>
    /// Empty constructor
    /// </summary>
    public CellStyle() : this(null) { }

    /// <summary>
    /// Construct a CellStyle from another CellStyle
    /// </summary>
    /// <param name="sheet1">Worksheet associated with cell style</param>
    /// <param name="other">CellStyle to initialise from</param>
    public CellStyle(Sheet sheet1, CellStyle other) : this(sheet1) {
        Colour = other.Colour;
        Background = other.Background;
        Bold = other.Bold;
        Italic = other.Italic;
        Underline = other.Underline;
    }

    /// <summary>
    /// Text colour
    /// </summary>
    [JsonInclude]
    public int? Colour { get; private set; }

    /// <summary>
    /// Text colour
    /// </summary>
    [JsonIgnore]
    public int TextColour {
        get => Colour.GetValueOrDefault(CellFactory.TextColour);
        set {
            Colour = value;
            if (sheet != null) {
                sheet.Modified = true;
            }
        }
    }

    /// <summary>
    /// Background colour
    /// </summary>
    [JsonInclude]
    public int? Background { get; private set; }

    /// <summary>
    /// Background colour
    /// </summary>
    [JsonIgnore]
    public int BackgroundColour {
        get => Background.GetValueOrDefault(CellFactory.BackgroundColour);
        set {
            Background = value;
            if (sheet != null) {
                sheet.Modified = true;
            }
        }
    }

    /// <summary>
    /// Specifies boldface text
    /// </summary>
    [JsonInclude]
    public bool Bold { get; private set; }

    /// <summary>
    /// Sets or gets the cell text bold style.
    /// </summary>
    [JsonIgnore]
    public bool IsBold {
        get => Bold;
        set {
            Bold = value;
            if (sheet != null) {
                sheet.Modified = true;
            }
        }
    }

    /// <summary>
    /// Specifies italic text
    /// </summary>
    [JsonInclude]
    public bool Italic { get; private set; }

    /// <summary>
    /// Sets or gets the cell text italic style.
    /// </summary>
    [JsonIgnore]
    public bool IsItalic {
        get => Italic;
        set {
            Italic = value;
            if (sheet != null) {
                sheet.Modified = true;
            }
        }
    }

    /// <summary>
    /// Specifies Underlined text
    /// </summary>
    [JsonInclude]
    public bool Underline { get; private set; }

    /// <summary>
    /// Sets or gets the cell text underline style.
    /// </summary>
    [JsonIgnore]
    public bool IsUnderlined {
        get => Underline;
        set {
            Underline = value;
            if (sheet != null) {
                sheet.Modified = true;
            }
        }
    }

}