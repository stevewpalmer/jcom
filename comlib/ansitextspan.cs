// JComLib
// ANSI text span
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

using System.Text;

namespace JComLib;

public class AnsiTextSpan(string text) {

    // Control Sequence Introducer
    private const string CSI = @"[";

    /// <summary>
    /// Copy constructor
    /// </summary>
    /// <param name="other">AnsiTextSpan to copy</param>
    public AnsiTextSpan(AnsiTextSpan other) : this(other.Text) {
        ForegroundColour = other.ForegroundColour;
        BackgroundColour = other.BackgroundColour;
        Bold = other.Bold;
        Italic = other.Italic;
        Underline = other.Underline;
    }

    /// <summary>
    /// Control sequence
    /// </summary>
    public string CS {
        get {
            StringBuilder cs = new();
            if (ForegroundColour.HasValue && BackgroundColour.HasValue) {
                cs.Append($"{CSI}{ForegroundColour};{BackgroundColour+10}m");
            }
            if (Bold) {
                cs.Append($"{CSI}1m");
            }
            if (Italic) {
                cs.Append($"{CSI}3m");
            }
            if (Underline) {
                cs.Append($"{CSI}4m");
            }
            return cs.ToString();
        }
    }

    /// <summary>
    /// Foreground colour
    /// </summary>
    public int? ForegroundColour { get; init; } = AnsiColour.BrightWhite;

    /// <summary>
    /// Background colour
    /// </summary>
    public int? BackgroundColour { get; init; } = AnsiColour.Black;

    /// <summary>
    /// Specifies boldface text
    /// </summary>
    public bool Bold { get; init; }

    /// <summary>
    /// Specifies italic text
    /// </summary>
    public bool Italic { get; init; }

    /// <summary>
    /// Specifies Underlined text
    /// </summary>
    public bool Underline { get; init; }

    /// <summary>
    /// Raw text string
    /// </summary>
    public string Text { get; init; } = text;

    /// <summary>
    /// Returns the text span with all escape sequences applied.
    /// </summary>
    /// <returns>Escaped string</returns>
    public string EscapedString() {
        return $"{CS}{Text}{CSI}0m";
    }
}