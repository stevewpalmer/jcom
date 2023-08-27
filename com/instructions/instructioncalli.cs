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

using System.Reflection;
using System.Reflection.Emit;

namespace CCompiler; 

/// <summary>
/// Defines an instruction class that constructs an indirect all opcode with
/// specified calling convention, return type and parameter types.
/// </summary>
public class InstructionCalli : Instruction {
    private readonly CallingConventions _conv;
    private readonly Type _returnType;
    private readonly Type[] _parameterTypes;

    /// <summary>
    /// Create an InstructionCalli object with the given opcode,
    /// and function parameter definitions.
    /// </summary>
    /// <param name="op">Opcode</param>
    /// <param name="conv">The function calling convention</param>
    /// <param name="returnType">The return type</param>
    /// <param name="parameterTypes">An array of types for each parameter</param>
    public InstructionCalli(OpCode op, CallingConventions conv, Type returnType, Type[] parameterTypes) : base(op) {
        _conv = conv;
        _returnType = returnType;
        _parameterTypes = parameterTypes;
    }

    /// <summary>
    /// Generate MSIL code to emit a Calli indirect call with the given
    /// calling convention, return type and array of parameter types.
    /// </summary>
    /// <param name="il">ILGenerator object</param>
    public override void Generate(ILGenerator il) {
        if (il == null) {
            throw new ArgumentNullException(nameof(il));
        }
        if (Deleted) {
            return;
        }
        il.EmitCalli(OpCodes.Calli, _conv, _returnType, _parameterTypes, null);
    }
}
