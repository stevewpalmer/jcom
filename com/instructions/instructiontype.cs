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
    /// a Type parameter.
    /// </summary>
    public class InstructionType : Instruction {
        private readonly Type _type;

        /// <summary>
        /// Create an InstructionType object with the given opcode
        /// and Type parameter.
        /// </summary>
        /// <param name="op">Opcode</param>
        /// <param name="type">Type object</param>
        public InstructionType(OpCode op, Type type) : base(op) {
            _type = type;
        }

        /// <summary>
        /// Generate MSIL code to emit a opcode that takes a Type parameter.
        /// </summary>
        /// <param name="il">ILGenerator object</param>
        public override void Generate(ILGenerator il) {
            if (il == null) {
                throw new ArgumentNullException(nameof(il));
            }
            if (Deleted) {
                return;
            }
            il.Emit(Code, _type);
        }
    }
}
