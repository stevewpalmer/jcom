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

using System;
using System.Reflection;
using System.Reflection.Emit;
using JComLib;

namespace CCompiler {
    /// <summary>
    /// Defines a class that emits static opcodes and operands
    /// </summary>
    public static class StaticEmitter {
        private static int _staticIndex = 0;

        /// <summary>
        /// Creates a static local variable.
        /// </summary>
        /// <param name="tb">Typebuilder for the current type</param>
        /// <param name="sym">The symbol</param>
        /// <returns>The A FieldInfo representing the static variable</returns>
        public static FieldInfo CreateStatic(TypeBuilder tb, Symbol sym) {
            if (tb == null) {
                throw new ArgumentNullException(nameof(tb));
            }
            if (sym == null) {
                throw new ArgumentNullException(nameof(sym));
            }
            string name = $"S{_staticIndex++}_{sym.Name}";
            FieldBuilder fieldInfo = tb.DefineField(name, sym.SystemType, FieldAttributes.Static);
            return fieldInfo;
        }

        /// <summary>
        /// Creates a fixed value static local variable that is initialised with
        /// the symbol's value.
        /// </summary>
        /// <param name="tb">Typebuilder for the current type</param>
        /// <param name="sym">The symbol</param>
        /// <returns>The A FieldInfo representing the static variable</returns>
        public static FieldInfo CreateFixedStatic(TypeBuilder tb, Symbol sym) {
            if (tb == null) {
                throw new ArgumentNullException(nameof(tb));
            }
            if (sym == null) {
                throw new ArgumentNullException(nameof(sym));
            }
            string name = $"S{_staticIndex++}_{sym.Name}";
            FieldAttributes flags = FieldAttributes.Private | FieldAttributes.Static;
            if (sym.Value.Type != VariantType.STRING) {
                flags |= FieldAttributes.Literal;
            }
            FieldBuilder fieldInfo = tb.DefineField(name, sym.SystemType, flags);
            switch (sym.Value.Type) {
                case VariantType.DOUBLE:
                    fieldInfo.SetConstant(sym.Value.DoubleValue);
                    break;
                case VariantType.FLOAT:
                    fieldInfo.SetConstant(sym.Value.RealValue);
                    break;
                case VariantType.INTEGER:
                    fieldInfo.SetConstant(sym.Value.IntValue);
                    break;
                case VariantType.BOOLEAN:
                    fieldInfo.SetConstant(sym.Value.BoolValue);
                    break;
                case VariantType.COMPLEX:
                    fieldInfo.SetConstant(sym.Value.ComplexValue);
                    break;
            }
            return fieldInfo;
        }
    }
}
