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

namespace CCompiler {

    /// <summary>
    /// Defines an instruction class that constructs an opcode that takes
    /// an integer parameter.
    /// </summary>
    public class InstructionInt : Instruction {

        /// <summary>
        /// Create an InstructionInt object with the given opcode
        /// and integer value.
        /// </summary>
        /// <param name="op">Opcode</param>
        /// <param name="value">An integer value</param>
        public InstructionInt(OpCode op, int value) : base(op) {
            Value = value;
        }

        /// <summary>
        /// Gets the integer parameter.
        /// </summary>
        /// <value>The integer parameter assigned to this opcode</value>
        public int Value { get; private set; }

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
                case "ldc.i4":
                    switch (Value) {
                        case -1: il.Emit(OpCodes.Ldc_I4_M1); return;
                        case 0: il.Emit(OpCodes.Ldc_I4_0); return;
                        case 1: il.Emit(OpCodes.Ldc_I4_1); return;
                        case 2: il.Emit(OpCodes.Ldc_I4_2); return;
                        case 3: il.Emit(OpCodes.Ldc_I4_3); return;
                        case 4: il.Emit(OpCodes.Ldc_I4_4); return;
                        case 5: il.Emit(OpCodes.Ldc_I4_5); return;
                        case 6: il.Emit(OpCodes.Ldc_I4_6); return;
                        case 7: il.Emit(OpCodes.Ldc_I4_7); return;
                        case 8: il.Emit(OpCodes.Ldc_I4_8); return;
                    }
                    if (Value >= -128 && Value <= 127) {
                        il.Emit(OpCodes.Ldc_I4_S, (byte)Value);
                        return;
                    }
                    il.Emit(Code, Value);
                    break;

                default:
                    il.Emit(Code, Value);
                    break;
            }
        }
    }
}
