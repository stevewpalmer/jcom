// JCom Runtime Library
// Console I/O
//
// Authors:
//  Steve Palmer
//
// Copyright (C) 2023 Steve Palmer
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

using System.Drawing;
using System.Runtime.InteropServices;

namespace JComLib;

public static class Terminal {
    private static readonly object LockObj = new();
    private static ConsoleColor _savedBackgroundColour;
    private static ConsoleColor _savedForegroundColour;
    private static bool _isWindows;

    /// <summary>
    /// Initialise the display.
    /// </summary>
    public static void Open() {
        _savedBackgroundColour = Console.BackgroundColor;
        _savedForegroundColour = Console.ForegroundColor;
        _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        Console.TreatControlCAsInput = true;
        if (!_isWindows) {
            Console.Write(@"7[?47h");
        }
        Console.Clear();
        SetDefaultCursor();
    }

    /// <summary>
    /// Close down the display.
    /// </summary>
    public static void Close() {
        Console.TreatControlCAsInput = false;
        Console.BackgroundColor = _savedBackgroundColour;
        Console.ForegroundColor = _savedForegroundColour;
        Console.Clear();
        ShowCursor(true);
        if (!_isWindows) {
            Console.Write(@"[2J[?47l8");
        }
    }

    /// <summary>
    /// Return the display width
    /// </summary>
    public static int Width => Console.WindowWidth;

    /// <summary>
    /// Return the display width
    /// </summary>
    public static int Height => Console.WindowHeight;

    /// <summary>
    /// Get and set the current foreground colour
    /// </summary>
    public static ConsoleColor ForegroundColour {
        set => Console.ForegroundColor = value;
    }

    /// <summary>
    /// Get and set the current background colour
    /// </summary>
    public static ConsoleColor BackgroundColour {
        set => Console.BackgroundColor = value;
    }

    /// <summary>
    /// Show or hide the cursor
    /// </summary>
    /// <param name="show">True to show the cursor, false to hide it</param>
    public static void ShowCursor(bool show) {
        Console.Write(show ? @"[?25h" : @"[?25l");
    }

    /// <summary>
    /// Set the default cursor.
    /// </summary>
    public static void SetDefaultCursor() {
        if (_isWindows) {
#pragma warning disable CA1416
            Console.CursorSize = 10;
#pragma warning restore CA1416
        }
    }

    /// <summary>
    /// Set the virtual cursor which indicates that the user
    /// is within the virtual editing space.
    /// </summary>
    public static void SetVirtualCursor() {
        if (_isWindows) {
#pragma warning disable CA1416
            Console.CursorSize = 50;
#pragma warning restore CA1416
        }
    }

    /// <summary>
    /// Write to the console at the specified position, padding out to the width
    /// with spaces if required. The cursor position is left at the end of the
    /// string.
    /// </summary>
    /// <param name="x">Zero based column of output</param>
    /// <param name="y">Zero based line of output</param>
    /// <param name="width">Width of area to write to</param>
    /// <param name="str">String to output</param>
    public static void Write(int x, int y, int width, AnsiText str) {
        lock (LockObj) {
            Console.SetCursorPosition(x, y);
            foreach (AnsiTextSpan span in str.Spans) {
                Console.Write(span.EscapedString());
                width -= span.Text.Length;
            }
            if (width > 0) {
                Console.Write(string.Empty.PadRight(width));
            }
        }
    }

    /// <summary>
    /// Write to the console at the specified position, padding out to the width
    /// with spaces if required. The cursor position is left at the end of the
    /// string.
    /// </summary>
    /// <param name="x">Zero based column of output</param>
    /// <param name="y">Zero based line of output</param>
    /// <param name="width">Width of area to write to</param>
    /// <param name="fg">Foreground colour</param>
    /// <param name="bg">Background colour</param>
    /// <param name="str">String to output</param>
    public static void Write(int x, int y, int width, int fg, int bg, string str) {
        lock (LockObj) {
            Console.SetCursorPosition(x, y);
            Console.Write(str.PadRight(width).AnsiColour(fg, bg));
        }
    }

    /// <summary>
    /// Write to the console at the specified position, padding out to the width
    /// with spaces if required. The cursor position is left at the end of the
    /// string.
    /// </summary>
    /// <param name="x">Zero based column of output</param>
    /// <param name="y">Zero based line of output</param>
    /// <param name="width">Width of area to write to</param>
    /// <param name="bg">Background colour</param>
    /// <param name="fg">Foreground colour</param>
    /// <param name="str">String to output</param>
    public static void Write(int x, int y, int width, ConsoleColor bg, ConsoleColor fg, string str) {
        lock (LockObj) {
            Console.SetCursorPosition(x, y);
            ConsoleColor fgSaved = Console.ForegroundColor;
            ConsoleColor bgSaved = Console.BackgroundColor;
            Console.ForegroundColor = fg;
            Console.BackgroundColor = bg;
            Console.Write(str.PadRight(width));
            Console.ForegroundColor = fgSaved;
            Console.BackgroundColor = bgSaved;
        }
    }

    /// <summary>
    /// Write the specified text at the current cursor position
    /// </summary>
    /// <param name="str">String to output</param>
    public static void Write(string str) {
        Console.Write(str);
    }

    /// <summary>
    /// Write the specified character at the current cursor position
    /// </summary>
    /// <param name="bg">Background colour</param>
    /// <param name="fg">Foreground colour</param>
    /// <param name="ch">Character to output</param>
    public static void Write(ConsoleColor bg, ConsoleColor fg, char ch) {
        lock (LockObj) {
            ConsoleColor fgSaved = Console.ForegroundColor;
            ConsoleColor bgSaved = Console.BackgroundColor;
            Console.ForegroundColor = fg;
            Console.BackgroundColor = bg;
            Console.Write(ch);
            Console.ForegroundColor = fgSaved;
            Console.BackgroundColor = bgSaved;
        }
    }

    /// <summary>
    /// Set the cursor position. Positions are zero based offsets with
    /// row 0 at the top of the console screen.
    /// </summary>
    /// <param name="x">Cursor column</param>
    /// <param name="y">Cursor row</param>
    public static void SetCursor(int x, int y) {
        Console.SetCursorPosition(x, y);
    }

    /// <summary>
    /// Set the cursor position. Positions are zero based offsets with
    /// row 0 at the top of the console screen.
    /// </summary>
    /// <param name="position">Point with cursor position</param>
    public static void SetCursor(Point position) {
        Console.SetCursorPosition(position.X, position.Y);
    }

    /// <summary>
    /// Get the cursor position. Positions are zero based offsets with
    /// /// row 0 at the top of the console screen.
    /// </summary>
    /// <returns>Cursor position</returns>
    public static Point GetCursor() {
        (int savedLeft, int savedTop) = Console.GetCursorPosition();
        return new Point(savedLeft, savedTop);
    }
}