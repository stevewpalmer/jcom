// JCom Compiler Toolkit
// Type Builder
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

using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

namespace CCompiler; 

/// <summary>
/// JType construction flags
/// </summary>
[Flags]
public enum JTypeAttributes {

    /// <summary>
    /// Type is public
    /// </summary>
    Public = 1,

    /// <summary>
    /// Type is sealed and cannot be inherited
    /// </summary>
    Sealed = 2,

    /// <summary>
    /// Type is debuggable
    /// </summary>
    Debuggable = 4
}

/// <summary>
/// Defines a single assembly type.
/// </summary>
public class JType {

    private static int _staticIndex;
    private Type _createdType;
    private JMethod _defaultConstructor;

    /// <summary>
    /// Type builder
    /// </summary>
    public TypeBuilder Builder { get; set; }

    /// <summary>
    /// True if type is debuggable
    /// </summary>
    public bool Debuggable { get; set; }

    /// <summary>
    /// Creates the specified type in the module.
    /// </summary>
    /// <param name="mb">Module builder</param>
    /// <param name="typeName">Type name</param>
    /// <param name="attributes">Type attributes</param>
    public JType(ModuleBuilder mb, string typeName, JTypeAttributes attributes) {
        TypeAttributes attr = TypeAttributes.BeforeFieldInit;
        if (attributes.HasFlag(JTypeAttributes.Public)) {
            attr |= TypeAttributes.Public;
        }
        if (attributes.HasFlag(JTypeAttributes.Sealed)) {
            attr |= TypeAttributes.Sealed;
        }
        if (attributes.HasFlag(JTypeAttributes.Debuggable)) {
            Debuggable = true;
        }
        Builder = mb.DefineType(typeName, attr);
    }

    /// <summary>
    /// Create an instance of this type
    /// </summary>
    public Type CreateType {
        get {
            if (_defaultConstructor != null) {
                _defaultConstructor.Emitter.Emit0(OpCodes.Ret);
                _defaultConstructor.Emitter.Save();
            }
            if (_createdType == null) {
                _createdType = Builder.CreateTypeInfo();
            }
            return _createdType;
        }
    }

    /// <summary>
    /// The default constructor for this type
    /// </summary>
    public JMethod DefaultConstructor {
        get {
            if (_defaultConstructor == null) {
                ConstructorBuilder cntb = Builder.DefineConstructor(MethodAttributes.Static,
                        CallingConventions.Standard,
                        Array.Empty<Type>());
                _defaultConstructor = new JMethod(this, cntb);
            }
            return _defaultConstructor;
        }
    }

    /// <summary>
    /// Creates a field.
    /// </summary>
    /// <param name="sym">The symbol</param>
    /// <returns>The A FieldInfo representing the new field</returns>
    public void CreateField(Symbol sym) {
        if (sym == null) {
            throw new ArgumentNullException(nameof(sym));
        }
        string name = $"S{_staticIndex++}_{sym.Name}";
        sym.Info = Builder.DefineField(name, sym.SystemType, FieldAttributes.Static);
    }

    /// <summary>
    /// Creates a temporary field of the specified type.
    /// </summary>
    /// <param name="type">The symbol system type</param>
    /// <returns>The A FieldInfo representing the new temporary field</returns>
    public FieldInfo TemporaryField(Type type) {
        string name = $"S{_staticIndex++}_Temp";
        return Builder.DefineField(name, type, FieldAttributes.Static);
    }

    /// <summary>
    /// Creates a method within this type.
    /// </summary>
    /// <param name="sym">Symbol representing the method</param>
    /// <param name="attributes">Method attributes</param>
    /// <param name="paramTypes">Parameter types</param>
    /// <returns></returns>
    public JMethod CreateMethod(Symbol sym, MethodAttributes attributes) {

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
        metb = Builder.DefineMethod(sym.Name, attributes, returnType, paramTypes);

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
                metb.DefineParameter(paramIndex++, ParameterAttributes.None, param.Name);
            }
        }

        return new JMethod(this, metb);
    }
}
