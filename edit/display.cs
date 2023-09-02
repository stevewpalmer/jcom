// JEdit
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

namespace JEdit; 

public static class Display {
    
    /// <summary>
    /// Write to the console at the specified position, padding out to the width
    /// with spaces if required. The previous cursor position is saved and then
    /// restored at the end.
    /// </summary>
    /// <param name="x">Zero based column of output</param>
    /// <param name="y">Zero based line of output</param>
    /// <param name="width">Width of area to write to</param>
    /// <param name="str">String to output</param>
    public static void WriteTo(int x, int y, int width, string str) {
        SetCursor(WriteToNc(x, y, width, str));
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
    public static Point WriteToNc(int x, int y, int width, string str) {
        (int savedLeft, int savedTop) = Console.GetCursorPosition();
        Console.SetCursorPosition(x, y);
        Console.Write(str.PadRight(width));
        return new Point(savedLeft, savedTop);
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