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

using System.Reflection.Emit;

namespace CCompiler;

/// <summary>
/// Defines an instruction class that constructs an opcode that takes
/// an array of labels as its parameter.
/// </summary>
public class InstructionLabelArray : Instruction {
    private readonly Label[] _labels;

    /// <summary>
    /// Create an InstructionLabelArray object with the given opcode
    /// and an array of Label objects.
    /// </summary>
    /// <param name="op">Opcode</param>
    /// <param name="labels">Array of Label objects</param>
    public InstructionLabelArray(OpCode op, Label[] labels) : base(op) {
        ArgumentNullException.ThrowIfNull(labels);
        _labels = labels;
    }

    /// <summary>
    /// Generate MSIL code to emit a opcode that takes an array of
    /// labels as a parameter.
    /// </summary>
    /// <param name="il">ILGenerator object</param>
    public override void Generate(ILGenerator il) {
        ArgumentNullException.ThrowIfNull(il);
        if (Deleted) {
            return;
        }
        il.Emit(Code, _labels);
    }
}