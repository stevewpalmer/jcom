// JCom Compiler Toolkit
// Method Builder
//
// Authors:
//  Steve Palmer
//
// Copyright (C) 2021 Steve Palmer
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
/// Defines a method within a type
/// </summary>
public class JMethod {

    /// <summary>
    /// Owning type
    /// </summary>
    public JType OwningType { get; set; }

    /// <summary>
    /// Method builder
    /// </summary>
    public MethodBuilder Builder { get; }

    /// <summary>
    /// Constructor builder
    /// </summary>
    public ConstructorBuilder CBuilder { get; set; }

    /// <summary>
    /// Code emitter for this method
    /// </summary>
    public Emitter Emitter { get; }

    /// <summary>
    /// Constructs a method that belongs to the given type
    /// </summary>
    /// <param name="owningType">Method owner</param>
    /// <param name="metd">Method builder</param>
    public JMethod(JType owningType, MethodBuilder metd) {
        OwningType = owningType;
        Builder = metd;
        Emitter = new Emitter(metd) {
            IsDebuggable = owningType.Debuggable
        };
    }

    /// <summary>
    /// Constructs a method that belongs to the given type
    /// </summary>
    /// <param name="owningType">Method owner</param>
    /// <param name="cntb">Constructor builder</param>
    public JMethod(JType owningType, ConstructorBuilder cntb) {
        OwningType = owningType;
        CBuilder = cntb;
        Emitter = new Emitter(cntb) {
            IsDebuggable = owningType.Debuggable
        };
    }
}