// JCalcLib
// Calculate cell formulae
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

namespace JCalcLib;

/// <summary>
/// A CalculationContext carries the context information needed by
/// the evaluation methods on each node.
/// </summary>
public class CalculationContext {

    /// <summary>
    /// The sheet on which this calculation is running.
    /// </summary>
    public required Sheet Sheet { get; init; }

    /// <summary>
    /// List of cells updated by the calculation.
    /// </summary>
    public Cell[] UpdateList { get; init; } = [];

    /// <summary>
    /// A reference list of visited formula nodes. This is used to
    /// catch potential circular computations.
    /// </summary>
    public Stack<CellLocation> ReferenceList = new();
}
