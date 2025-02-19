﻿// JCom Compiler Toolkit
// Emitter for MSIL
//
// Authors:
//  Steve Palmer
//
// Copyright (C) 2013 Steve Palmer
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

using System.Diagnostics.SymbolStore;
using System.Reflection.Emit;

namespace CCompiler;

/// <summary>
/// Defines an instruction class that constructs a source code marker.
/// </summary>
public class InstructionMarker : Instruction {
    private readonly ISymbolDocumentWriter _doc;
    private readonly int _linenumber;

    /// <summary>
    /// Create an InstructionMarker object to represent the source code
    /// file and line number of the current point in the sequence.
    /// </summary>
    /// <param name="doc">An ISymbolDocumentWriter object</param>
    /// <param name="linenumber">An integer line number</param>
    public InstructionMarker(ISymbolDocumentWriter doc, int linenumber) {
        _doc = doc;
        _linenumber = linenumber;
    }

    /// <summary>
    /// Generate MSIL code to emit an instruction marker at the current
    /// sequence in the output.
    /// </summary>
    /// <param name="il">ILGenerator object</param>
    public override void Generate(ILGenerator il) {
        ArgumentNullException.ThrowIfNull(il);
        if (Deleted) { }
        il.MarkSequencePoint(_doc, _linenumber, 1, _linenumber, 100);
    }
}