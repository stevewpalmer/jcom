// JCalc
// Colour configuration
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

namespace JCalc;

public class Colours {

    private readonly Config? _config;

    private readonly ConsoleColor[] _colours = [
        ConsoleColor.Black,
        ConsoleColor.Blue,
        ConsoleColor.Green,
        ConsoleColor.Cyan,
        ConsoleColor.Red,
        ConsoleColor.Magenta,
        ConsoleColor.DarkMagenta,
        ConsoleColor.White,
        ConsoleColor.DarkGray,
        ConsoleColor.DarkBlue,
        ConsoleColor.DarkGreen,
        ConsoleColor.DarkCyan,
        ConsoleColor.DarkRed,
        ConsoleColor.DarkMagenta,
        ConsoleColor.Yellow,
        ConsoleColor.DarkYellow
    ];

    /// <summary>
    /// Initialise a Colors object
    /// </summary>
    public Colours() : this(null) { }

    /// <summary>
    /// Initialise a Colors object with the specified configuration
    /// </summary>
    /// <param name="config">Configuration file</param>
    public Colours(Config? config) {
        _config = config;
    }

    /// <summary>
    /// Retrieve the background colour
    /// </summary>
    public ConsoleColor BackgroundColour =>
        GetColour(_config?.BackgroundColour, 0);

    /// <summary>
    /// Retrieve the foreground colour
    /// </summary>
    public ConsoleColor ForegroundColour =>
        GetColour(_config?.ForegroundColour, 7);

    /// <summary>
    /// Retrieve the normal message colour
    /// </summary>
    public ConsoleColor NormalMessageColour =>
        GetColour(_config?.NormalMessageColour, 7);

    /// <summary>
    /// Parse a colour value from the configuration file. Colours are specified as
    /// an index into the _colours array. Out of bounds values are treated as if
    /// they were absent and the default index is used instead.
    /// </summary>
    /// <param name="value">Configuration value</param>
    /// <param name="defaultIndex">Default index if config not present</param>
    /// <returns>Colour</returns>
    private ConsoleColor GetColour(string? value, int defaultIndex) {
        if (value == null || !int.TryParse(value, out int index) || index < 0 || index >= _colours.Length) {
            index = defaultIndex;
        }
        return _colours[index];
    }
}