// JCom Compiler Toolkit
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
/// Defines a single Instruction base class that constructs an opcode that
/// takes no parameters.
/// </summary>
public class Instruction {

    /// <summary>
    /// Create an Instruction object with the given opcode.
    /// </summary>
    /// <param name="op">An OpCode</param>
    public Instruction(OpCode op) {
        Code = op;
    }

    /// <summary>
    /// Create an Instruction object that takes no opcode.
    /// </summary>
    public Instruction() { }

    /// <summary>
    /// Generate MSIL code to emit a opcode that takes no parameters.
    /// </summary>
    /// <param name="il">ILGenerator object</param>
    public virtual void Generate(ILGenerator il) {
        if (il == null) {
            throw new ArgumentNullException(nameof(il));
        }
        if (Deleted) {
            return;
        }
        il.Emit(Code);
    }

    /// <summary>
    /// Get or set the instruction opcode.
    /// </summary>
    public OpCode Code { get; set; }

    /// <summary>
    /// Gets or sets a flag which indicates whether or not this
    /// instruction should be omitted.
    /// </summary>
    public bool Deleted { get; set; }
}