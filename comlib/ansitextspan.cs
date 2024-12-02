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
using JCalcLib;

namespace JComLib;

public class AnsiTextSpan(string text) {
    private const string CSI = @"[";
    private int _width;

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
        Width = other.Width;
        Alignment = other.Alignment;
    }

    /// <summary>
    /// Return the control sequence for any explicit colours set on
    /// the span, or an empty string.
    /// </summary>
    public string CSColours {
        get {
            if (ForegroundColour.HasValue && BackgroundColour.HasValue) {
                return $"{CSI}{ForegroundColour};{BackgroundColour + 10}m";
            }
            return string.Empty;
        }
    }

    /// <summary>
    /// Control sequence
    /// </summary>
    public string CS {
        get {
            StringBuilder cs = new();
            if (ForegroundColour.HasValue && BackgroundColour.HasValue) {
                cs.Append($"{CSI}{ForegroundColour};{BackgroundColour + 10}m");
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
    /// Width of the span. By default this is the length of the text
    /// but a wider width can be explicitly set where the text needs
    /// to be padded out or aligned. If Width is set to 0 then it is
    /// disregarded, as is any alignment.
    /// </summary>
    public int Width {
        get => _width > 0 ? _width : Text.Length;
        set => _width = value;
    }

    /// <summary>
    /// Alignment. The width must be set as otherwise no
    /// alignment will applied.
    /// </summary>
    public AnsiAlignment Alignment { get; init; } = AnsiAlignment.LEFT;

    /// <summary>
    /// Allocate an empty string of the given length.
    /// </summary>
    /// <param name="length">Length of string to be allocated</param>
    /// <returns>A string filled with spaces up to the given length</returns>
    private string EmptyString(int length) => new(' ', length);

    /// <summary>
    /// Returns the text span with all escape sequences applied.
    /// </summary>
    /// <returns>Escaped string</returns>
    public string EscapedString() {
        string leftPadding = string.Empty;
        string rightPadding = string.Empty;
        string text = Text;
        if (Width > 0) {
            int spacing;
            switch (Alignment) {
                case AnsiAlignment.NONE:
                    spacing = Width - text.Length;
                    rightPadding = $"{CSColours}{EmptyString(spacing)}{CSI}0m";
                    break;

                case AnsiAlignment.LEFT:
                    text = text.Trim();
                    spacing = Width - text.Length;
                    rightPadding = $"{CSColours}{EmptyString(spacing)}{CSI}0m";
                    break;

                case AnsiAlignment.CENTRE:
                    text = text.Trim();
                    spacing = Width - text.Length;
                    int leftSize = spacing / 2;
                    int rightSize = spacing - leftSize;
                    rightPadding = $"{CSColours}{EmptyString(leftSize)}{CSI}0m";
                    leftPadding = $"{CSColours}{EmptyString(rightSize)}{CSI}0m";
                    break;

                case AnsiAlignment.RIGHT:
                    text = text.Trim();
                    spacing = Width - text.Length;
                    leftPadding = $"{CSColours}{EmptyString(spacing)}{CSI}0m";
                    break;
            }
        }
        return $"{leftPadding}{CS}{text}{CSI}0m{rightPadding}";
    }
}