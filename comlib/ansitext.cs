// Jcom Runtime Libary
// ANSI text parser
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

namespace JComLib;

public class AnsiText {
    private readonly string _text;
    private int _index;

    public class AnsiTextSpan {
        public ConsoleColor Foreground { get; init; } = ConsoleColor.White;
        public ConsoleColor Background { get; init; } = ConsoleColor.Black;
        public string Text { get; init; } = "";
    }

    /// <summary>
    /// List of spans
    /// </summary>
    public List<AnsiTextSpan> Spans { get; } = [];

    /// <summary>
    /// Parse the specified text into one or more AnsiTextSpan elements
    /// with their distinct colours. Throws a FormatException if the ANSI
    /// escape sequences are malformed.
    /// </summary>
    /// <param name="text">Text to parse</param>
    /// <exception cref="FormatException"></exception>
    public AnsiText(string text) {
        _text = text;
        _index = _text.IndexOf('\u001b');
        int startIndex = 0;
        int foregroundColourIndex = 37;
        int backgroundColourIndex = 30;
        do {
            if (_index < 0) {
                Spans.Add(new AnsiTextSpan {
                    Text = text[startIndex..],
                    Background = MapIndexToConsoleColour(backgroundColourIndex),
                    Foreground = MapIndexToConsoleColour(foregroundColourIndex)
                });
                break;
            }
            if (_index > 0) {
                Spans.Add(new AnsiTextSpan {
                    Text = text[startIndex.._index],
                    Background = MapIndexToConsoleColour(backgroundColourIndex),
                    Foreground = MapIndexToConsoleColour(foregroundColourIndex)
                });
            }
            _ = NextChar();
            if (NextChar() != '[') {
                throw new FormatException("Unexpected escape sequence");
            }
            foregroundColourIndex = NextNumber();
            char ch;
            if (foregroundColourIndex == 0) {
                foregroundColourIndex = 37;
                backgroundColourIndex = 30;
                ch = NextChar();
            }
            else {
                ch = NextChar();
                if (ch == ';') {
                    backgroundColourIndex = NextNumber();
                    ch = NextChar();
                }
            }
            if (ch != 'm') {
                throw new FormatException("Unexpected escape sequence");
            }
            startIndex = _index;
            _index= _text.IndexOf('\u001b', _index);
        } while (true);
    }

    /// <summary>
    /// Map an ANSI colour number to its ConsoleColor equivalent.
    /// </summary>
    /// <param name="ansiColourNumber">ANSI colour number</param>
    /// <returns>ConsoleColor representing that colour number</returns>
    private static ConsoleColor MapIndexToConsoleColour(int ansiColourNumber) =>
        ansiColourNumber switch {
            30 => ConsoleColor.Black,
            31 => ConsoleColor.Red,
            32 => ConsoleColor.Green,
            33 => ConsoleColor.Yellow,
            34 => ConsoleColor.Blue,
            35 => ConsoleColor.Magenta,
            36 => ConsoleColor.Cyan,
            37 => ConsoleColor.White,
            _ => ConsoleColor.Gray
        };

    /// <summary>
    /// Retrieve the next character from the text string. A FormatException
    /// is thrown if we were at the end of the string.
    /// </summary>
    /// <returns>Character read at the index position</returns>
    /// <exception cref="FormatException"></exception>
    private char NextChar() {
        if (_index == _text.Length) {
            throw new FormatException("Unexpected end of text");
        }
        return _text[_index++];
    }

    /// <summary>
    /// Retrieve the next number from the text string.
    /// </summary>
    /// <returns>Character read at the index position</returns>
    private int NextNumber() {
        int value = 0;
        while (_index < _text.Length && char.IsDigit(_text[_index])) {
            value = value * 10 + (_text[_index++] - '0');
        }
        return value;
    }
}