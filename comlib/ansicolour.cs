// JComLib
// ANSI text colours
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

using System.Diagnostics;
using JCalcLib;

namespace JComLib;

/// <summary>
/// Ansi colour codes. Add 10 for the background code.
/// </summary>
public static class AnsiColour {
    public const int Black = 30;
    public const int Red = 31;
    public const int Green = 32;
    public const int Yellow = 33;
    public const int Blue = 34;
    public const int Magenta = 35;
    public const int Cyan = 36;
    public const int White = 37;
    public const int Grey = 90;
    public const int BrightRed = 91;
    public const int BrightGreen = 92;
    public const int BrightYellow = 93;
    public const int BrightBlue = 94;
    public const int BrightMagenta = 95;
    public const int BrightCyan = 96;
    public const int BrightWhite = 97;

    private static readonly Dictionary<int, string> colors = new() {
        { Black, "Black" },
        { Red, "Red" },
        { Green, "Green" },
        { Yellow, "Yellow" },
        { Blue, "Blue" },
        { Magenta, "Magenta" },
        { Cyan, "Cyan" },
        { White, "White" },
        { Grey, "Grey" },
        { BrightRed, "Bright Red" },
        { BrightGreen, "Bright Green" },
        { BrightYellow, "Bright Yellow" },
        { BrightBlue, "Bright Blue" },
        { BrightMagenta, "Bright Magenta" },
        { BrightCyan, "Bright Cyan" },
        { BrightWhite, "Bright White" }
    };

    /// <summary>
    /// The names of all supported colours
    /// </summary>
    public static string[] ColourNames => colors.Values.ToArray();

    /// <summary>
    /// The all supported colour values
    /// </summary>
    public static int[] ColourValues => colors.Keys.ToArray();

    /// <summary>
    /// Return a label for use by the command bar colour picker for the
    /// specified colour.
    /// </summary>
    /// <param name="colourValue">Colour value</param>
    /// <returns>String label</returns>
    public static string LabelForColour(int colourValue) {
        int colourIndex = Array.FindIndex(ColourValues, c => c == colourValue);
        Debug.Assert(colourIndex >= 0 && colourIndex < colors.Count);
        int fgColour = colourIndex switch {
            0 or 1 or 4 or 5 or 8 or 12 => White,
            _ => Black
        };
        return new AnsiTextSpan(colourIndex.ToString("X")) {
            Width = 3,
            Alignment = AnsiAlignment.CENTRE,
            ForegroundColour = fgColour,
            BackgroundColour = ColourValues[colourIndex]
        }.EscapedText;
    }

    /// <summary>
    /// Retrieve the colour value for the specified display name.
    /// </summary>
    /// <param name="name">Colour name</param>
    /// <returns>Colour value, or -1</returns>
    public static int ColourFromName(string name) {
        foreach (KeyValuePair<int, string> color in colors.Where(color => color.Value == name)) {
            return color.Key;
        }
        return -1;
    }

    /// <summary>
    /// Retrieve the display name for the specified colour value.
    /// </summary>
    /// <param name="colourValue">Colour value</param>
    /// <returns>Display name for colour value, or null</returns>
    public static string NameFromColour(int colourValue) => colors.GetValueOrDefault(colourValue);
}