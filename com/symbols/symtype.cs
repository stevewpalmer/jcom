﻿// JCom Compiler Toolkit
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

namespace CCompiler {

    /// <summary>
    /// Defines the symbol type.
    /// </summary>
    public enum SymType {

        /// <summary>
        /// None
        /// </summary>
        NONE,

        /// <summary>
        /// 32-bit integer
        /// </summary>
        INTEGER,

        /// <summary>
        /// Single-precision floating point
        /// </summary>
        FLOAT,

        /// <summary>
        /// Double-precision floating point
        /// </summary>
        DOUBLE,

        /// <summary>
        /// Boolean
        /// </summary>
        BOOLEAN,

        /// <summary>
        /// Source code label, such as the target of a GOTO
        /// or a Fortran FORMAT statement.
        /// </summary>
        LABEL,

        /// <summary>
        /// String
        /// </summary>
        CHAR,

        /// <summary>
        /// Fixed length string (implemented in JComLib)
        /// </summary>
        FIXEDCHAR,

        /// <summary>
        /// Reference type
        /// </summary>
        REF,

        /// <summary>
        /// Variable argument list
        /// </summary>
        VARARG,

        /// <summary>
        /// Top-level program
        /// </summary>
        PROGRAM,

        /// <summary>
        /// Fortran COMMON block
        /// </summary>
        COMMON,

        /// <summary>
        /// Generic type
        /// </summary>
        GENERIC,

        /// <summary>
        /// Complex number
        /// </summary>
        COMPLEX
    }
}