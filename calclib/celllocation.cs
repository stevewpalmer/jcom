// JCalcLib
// A cell location
//
// Authors:
//  Steve
//
// Copyright (C) 2024 Steve
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
using System.Drawing;
using System.Text.Json.Serialization;

namespace JCalcLib;

public struct CellLocation {
    private int _row = 1;
    private int _column = 1;

    public CellLocation() { }

    /// <summary>
    /// 1-based row
    /// </summary>
    public int Row {
        get => _row;
        set {
            Debug.Assert(value >= 1);
            _row = value;
        }
    }

    /// <summary>
    /// 1-based column
    /// </summary>
    public int Column {
        get => _column;
        set {
            Debug.Assert(value >= 1);
            _column = value;
        }
    }

    /// <summary>
    /// Returns the location as a Point
    /// </summary>
    [JsonIgnore]
    public Point Point => new(Column, Row);
}