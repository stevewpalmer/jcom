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
    /// a string parameter.
    /// </summary>
    public class InstructionString : Instruction {
        private readonly string _str;

        /// <summary>
        /// Create an InstructionString object with the given opcode
        /// and string value.
        /// </summary>
        /// <param name="op">Opcode</param>
        /// <param name="str">A string value</param>
        public InstructionString(OpCode op, string str) : base(op) {
            if (str == null) {
                throw new ArgumentNullException(nameof(str));
            }
            _str = str;
        }

        /// <summary>
        /// Generate MSIL code to emit a opcode that takes a string
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
                case "ldstr":
                    if (_str.Length == 0) {
                        il.Emit(OpCodes.Ldsfld, typeof(string).GetField("Empty"));
                    } else {
                        il.Emit(OpCodes.Ldstr, _str);
                    }
                    break;

                default:
                    il.Emit(Code, _str);
                    break;
            }
        }
    }
}
