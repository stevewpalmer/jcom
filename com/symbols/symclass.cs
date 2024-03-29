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

namespace CCompiler;

/// <summary>
/// Defines the symbol storage class.
/// </summary>
public enum SymClass {

    /// <summary>
    /// Program
    /// </summary>
    PROGRAM,

    /// <summary>
    /// Variable (includes parameters and constants)
    /// </summary>
    VAR,

    /// <summary>
    /// Function
    /// </summary>
    FUNCTION,

    /// <summary>
    /// Subroutine (or void function)
    /// </summary>
    SUBROUTINE,

    /// <summary>
    /// Label
    /// </summary>
    LABEL,

    /// <summary>
    /// Built-in intrinsic
    /// </summary>
    INTRINSIC,

    /// <summary>
    /// Inlined code (e.g. Fortran statement functions).
    /// </summary>
    INLINE
}