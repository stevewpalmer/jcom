// JCom Compiler Toolkit
// Symbol Table management
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

namespace CCompiler; 

/// <summary>
/// Defines a set of modifiers applied to the symbol.
/// </summary>
[Flags]
public enum SymModifier {

    /// <summary>
    /// Hidden symbol, generally one created by the compiler
    /// to support internal functionality. These are generally
    /// invisible to the end user, hence the name HIDDEN
    /// </summary>
    HIDDEN = 1,

    /// <summary>
    /// Symbol references an external function, procedure or
    /// variable.
    /// </summary>
    EXTERNAL = 2,

    /// <summary>
    /// Symbol is a static
    /// </summary>
    STATIC = 4,

    /// <summary>
    /// Symbol holds the return value for a function for languages
    /// where values are returned by assigning to the function name
    /// </summary>
    RETVAL = 8,

    /// <summary>
    /// Symbol is a constructor
    /// </summary>
    CONSTRUCTOR = 16,

    /// <summary>
    /// Symbol is a flat array. Flat arrays implement multi-dimensional arrays
    /// as flat 1-D internally and uses arithmetic operations to calculate the
    /// offset of the requested indexes. This is used by languages for which the
    /// type has no explicit multi-dimensional array support.
    /// </summary>
    FLATARRAY = 32,

    /// <summary>
    /// The symbol references the program entry point e.g. Main()
    /// </summary>
    ENTRYPOINT = 64,

    /// <summary>
    /// The symbol is exported and thus is made public with a standard calling
    /// convention.
    /// </summary>
    EXPORTED = 128
}
