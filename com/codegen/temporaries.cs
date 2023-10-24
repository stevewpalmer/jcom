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

using System.Collections.ObjectModel;

namespace CCompiler;

/// <summary>
/// Defines a class that stores a collection of temporaries local to
/// a scope that can be released when the scope exits.
/// </summary>
public sealed class Temporaries {
    private readonly Collection<LocalDescriptor> _locals;
    private readonly Emitter _em;

    /// <summary>
    /// Initializes a new instance of the <see cref="Temporaries"/> class
    /// using the given emitter.
    /// </summary>
    /// <param name="em">A code generator Emitter</param>
    public Temporaries(Emitter em) {
        _em = em;
        _locals = new Collection<LocalDescriptor>();
    }

    /// <summary>
    /// Obtains a new temporary store of the given symbol type.
    /// </summary>
    /// <param name="type">The symbol type requested.</param>
    /// <returns>A LocalDescriptor object for the type</returns>
    public LocalDescriptor New(SymType type) {
        return New(Symbol.SymTypeToSystemType(type));
    }

    /// <summary>
    /// Obtains a new temporary store of the given system type.
    /// </summary>
    /// <param name="type">The symbol type requested.</param>
    /// <returns>A LocalDescriptor object for the type</returns>
    public LocalDescriptor New(Type type) {
        LocalDescriptor local = _em.GetTemporary(type);
        _locals.Add(local);
        return local;
    }

    /// <summary>
    /// Frees up all temporaries allocated.
    /// </summary>
    public void Free() {
        foreach (LocalDescriptor local in _locals) {
            Emitter.ReleaseTemporary(local);
        }
        _locals.Clear();
    }
}