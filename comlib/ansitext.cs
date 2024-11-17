// Jcom Runtime Libary
// ANSI text support
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
    public const int Gray = 90;
    public const int BrightRed = 91;
    public const int BrightGreen = 92;
    public const int BrightYellow = 93;
    public const int BrightBlue = 94;
    public const int BrightMagenta = 95;
    public const int BrightCyan = 96;
    public const int BrightWhite = 97;
}

public static class AnsiExtensions {

    /// <summary>
    /// Return a string to be rendered to an Ansi console with the specified
    /// colours.
    /// </summary>
    /// <param name="str">Input string</param>
    /// <param name="fg">Ansi foreground colour</param>
    /// <param name="bg">Ansi background colour</param>
    /// <returns>String with Ansi escape sequences</returns>
    public static string AnsiColour(this string str, int fg, int bg) =>
        new AnsiText.AnsiTextSpan(str) {
            ForegroundColour = fg,
            BackgroundColour = bg
        }.EscapedString();
}

public class AnsiText {

    /// <summary>
    /// Empty AnsiText string
    /// </summary>
    public AnsiText(IEnumerable<AnsiTextSpan> spans) {
        Spans = spans.ToList();
    }

    /// <summary>
    /// List of spans
    /// </summary>
    public List<AnsiTextSpan> Spans { get; private set; }

    /// <summary>
    /// Length of the rendered text
    /// </summary>
    public int Length => Spans.Sum(span => span.Text.Length);

    /// <summary>
    /// Return the raw Ansi text
    /// </summary>
    public string Text => string.Join("", Spans.Select(span => span.Text));

    /// <summary>
    /// Return a substring of an AnsiText string starting from start
    /// and for the length number of characters. If start is outside
    /// the string length, an empty string is returned. If start plus
    /// length is longer than the string length, the string is
    /// truncated.
    /// </summary>
    /// <param name="start">Zero based start index</param>
    /// <param name="length">Length required</param>
    /// <returns></returns>
    public AnsiText Substring(int start, int length) {
        int spanIndex = 0;
        List<AnsiTextSpan> spans = [];
        while (spanIndex < Spans.Count && Spans[spanIndex].Text.Length <= start) {
            start -= Spans[spanIndex++].Text.Length;
        }
        while (spanIndex < Spans.Count && length > 0) {
            int textLength = Spans[spanIndex].Text.Length;
            int spanWidth = Math.Min(Math.Min(textLength, length), textLength - start);
            AnsiTextSpan co = new(Spans[spanIndex]) {
                Text = Spans[spanIndex].Text.Substring(start, spanWidth)
            };
            spans.Add(co);
            length -= spanWidth;
            start = 0;
            spanIndex++;
        }
        if (spans.Count == 0) {
            spans.Add(new AnsiTextSpan(string.Empty));
        }
        return new AnsiText(spans);
    }

    /// <summary>
    /// Style a portion of the AnsiText.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="length"></param>
    /// <param name="fg"></param>
    /// <param name="bg"></param>
    public void Style(int start, int length, int fg, int bg) {
        AnsiText preStyle = Substring(0, start);
        AnsiText newStyle = Substring(start, length);
        AnsiText postStyle = Substring(start + length, Length);

        List<AnsiTextSpan> newSpans = [];
        if (preStyle.Length > 0) {
            newSpans.AddRange(preStyle.Spans);
        }
        if (newStyle.Length > 0) {
            newSpans.Add(new AnsiTextSpan(newStyle.Text) {
                ForegroundColour = fg,
                BackgroundColour = bg
            });
        }
        if (postStyle.Length > 0) {
            newSpans.AddRange(postStyle.Spans);
        }
        Spans = newSpans;
    }

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
}