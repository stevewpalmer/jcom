﻿// JCom Compiler Toolkit
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

namespace CCompiler {

    /// <summary>
    /// Types of Emit exception handlers.
    /// </summary>
    public enum EmitExceptionHandlerType {

        /// <summary>
        /// Try part of the exception handler
        /// </summary>
        Try,

        /// <summary>
        /// Catch part of the exception handler
        /// </summary>
        Catch,

        /// <summary>
        /// Default catch block
        /// </summary>
        DefaultCatch,

        /// <summary>
        /// End of the Try/Catch block
        /// </summary>
        EndCatch
    }
}