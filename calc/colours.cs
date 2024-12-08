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

using JComLib;

namespace JCalc;

public class Colours {

    private readonly Config? _config;

    /// <summary>
    /// Initialise a Colours object
    /// </summary>
    public Colours() : this(null) { }

    /// <summary>
    /// Initialise a Colours object with the specified configuration
    /// </summary>
    /// <param name="config">Configuration file</param>
    public Colours(Config? config) {
        _config = config;
    }

    /// <summary>
    /// Retrieve the background colour
    /// </summary>
    public int BackgroundColour => _config?.BackgroundColour ?? AnsiColour.Black;

    /// <summary>
    /// Retrieve the foreground colour
    /// </summary>
    public int ForegroundColour => _config?.ForegroundColour ?? AnsiColour.BrightWhite;

    /// <summary>
    /// Retrieve the normal message colour
    /// </summary>
    public int NormalMessageColour => _config?.NormalMessageColour ?? AnsiColour.BrightWhite;

    /// <summary>
    /// Retrieve the selection colour
    /// </summary>
    public int SelectionColour => _config?.SelectionColour ?? AnsiColour.BrightCyan;
}