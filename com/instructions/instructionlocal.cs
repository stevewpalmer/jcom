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
/// Defines an instruction class that constructs an opcode that takes
/// an local variable index.
/// </summary>
public class InstructionLocal : Instruction {

    /// <summary>
    /// Create an InstructionInt object with the given opcode
    /// and integer value.
    /// </summary>
    /// <param name="op">Opcode</param>
    /// <param name="value">An integer value</param>
    public InstructionLocal(OpCode op, LocalDescriptor value) : base(op) {
        if (value == null) {
            throw new ArgumentNullException(nameof(value));
        }
        Value = value;
    }

    /// <summary>
    /// Gets the local variable reference.
    /// </summary>
    /// <value>The integer parameter assigned to this opcode</value>
    public LocalDescriptor Value { get; private set; }

    /// <summary>
    /// Generate MSIL code to emit a opcode that takes an integer
    /// parameter.
    /// </summary>
    /// <param name="il">ILGenerator object</param>
    public override void Generate(ILGenerator il) {
        if (il == null) {
            throw new ArgumentNullException(nameof(il));
        }
        if (Deleted) {
            return;
        }
        switch (Code.Name) {
            case "stloc":
                switch (Value.Index) {
                    case 0: il.Emit(OpCodes.Stloc_0); return;
                    case 1: il.Emit(OpCodes.Stloc_1); return;
                    case 2: il.Emit(OpCodes.Stloc_2); return;
                    case 3: il.Emit(OpCodes.Stloc_3); return;
                }
                if (Value.Index < 256) {
                    il.Emit(OpCodes.Stloc_S, (byte)Value.Index);
                } else {
                    il.Emit(OpCodes.Stloc, Value.Index);
                }
                break;

            case "ldloc":
                switch (Value.Index) {
                    case 0: il.Emit(OpCodes.Ldloc_0); return;
                    case 1: il.Emit(OpCodes.Ldloc_1); return;
                    case 2: il.Emit(OpCodes.Ldloc_2); return;
                    case 3: il.Emit(OpCodes.Ldloc_3); return;
                }
                if (Value.Index < 256) {
                    il.Emit(OpCodes.Ldloc_S, (byte)Value.Index);
                } else {
                    il.Emit(OpCodes.Ldloc, Value.Index);
                }
                break;

            case "ldloca":
                if (Value.Index < 256) {
                    il.Emit(OpCodes.Ldloca_S, (byte)Value.Index);
                } else {
                    il.Emit(OpCodes.Ldloca, Value.Index);
                }
                break;

            default:
                il.Emit(Code, Value.Index);
                break;
        }
    }
}
