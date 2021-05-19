// Project title
// File title
//
// Authors:
//  Steven Palmer
//
// Copyright (C) 2021 Steven Palmer
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
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

namespace CCompiler {

    /// <summary>
    /// Defines a single assembly type.
    /// </summary>
    public class JType {

        private static int _staticIndex = 0;

        /// <summary>
        /// Type builder
        /// </summary>
        public TypeBuilder Builder { get; set; }

        /// <summary>
        /// Creates the specified type in the module.
        /// </summary>
        /// <param name="mb">Module builder</param>
        /// <param name="typeName">Type name</param>
        /// <param name="attributes">Type attributes</param>
        public JType(ModuleBuilder mb, string typeName, TypeAttributes attributes) {
            Builder = mb.DefineType(typeName, attributes);
        }

        /// <summary>
        /// Creates a field.
        /// </summary>
        /// <param name="sym">The symbol</param>
        /// <returns>The A FieldInfo representing the new field</returns>
        public FieldInfo CreateField(Symbol sym) {
            if (sym == null) {
                throw new ArgumentNullException(nameof(sym));
            }
            string name = $"S{_staticIndex++}_{sym.Name}";
            return Builder.DefineField(name, sym.SystemType, FieldAttributes.Static);
        }


        /// <summary>
        /// Creates a constructor for the type
        /// </summary>
        /// <param name="attributes">Constructor attributes</param>
        /// <param name="paramTypes">Parameter types</param>
        /// <returns></returns>
        public ConstructorBuilder CreateConstructor(MethodAttributes attributes, Type[] paramTypes) {
            return Builder.DefineConstructor(attributes, CallingConventions.Standard, paramTypes);
        }

        /// <summary>
        /// Creates a method within this type.
        /// </summary>
        /// <param name="sym">Symbol representing the method</param>
        /// <param name="atributes">Method attributes</param>
        /// <param name="paramTypes">Parameter types</param>
        /// <returns></returns>
        public MethodBuilder CreateMethod(Symbol sym, MethodAttributes atributes) {

            bool isFunction = sym.RetVal != null || sym.Class == SymClass.FUNCTION;
            Type returnType;

            if (isFunction) {
                returnType = Symbol.SymTypeToSystemType(sym.Type);
            } else {
                returnType = typeof(void);
            }

            int paramCount = (sym.Parameters != null) ? sym.Parameters.Count : 0;

            Type[] paramTypes = new Type[paramCount];

            for (int c = 0; c < paramCount; ++c) {
                Symbol param = sym.Parameters[c];
                if (param == null) {
                    throw new NullReferenceException("Parameters");
                }
                Debug.Assert(param.IsParameter);
                Type thisType = param.SystemType;
                if (param.Linkage == SymLinkage.BYREF) {
                    thisType = thisType.MakeByRefType();
                }
                paramTypes[c] = thisType;
                param.ParameterIndex = c;
            }

            MethodBuilder metb;
            metb = Builder.DefineMethod(sym.Name, atributes, returnType, paramTypes);

            int paramIndex = 0;
            if (isFunction) {
                metb.DefineParameter(paramIndex++, ParameterAttributes.Retval, returnType.Name);
            }

            // For each parameter, set the actual name and type.
            for (int c = 0; c < paramCount; ++c) {
                Symbol param = sym.Parameters[c];
                if (param == null) {
                    throw new NullReferenceException("Parameters");
                }
                if (param.Linkage == SymLinkage.BYREF) {
                    metb.DefineParameter(paramIndex++, ParameterAttributes.In | ParameterAttributes.Out, param.Name);
                } else {
                    metb.DefineParameter(paramIndex++, ParameterAttributes.In, param.Name);
                }
            }

            return metb;
        }
    }
}
