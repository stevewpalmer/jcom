// JCom Compiler Toolkit
// Symbol Table stack
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

using System.Diagnostics;

namespace CCompiler {

    /// <summary>
    /// Defines a stack of symbol collections
    /// </summary>
    public class SymbolStack {

        private readonly List<SymbolCollection> _stack = new();

        /// <summary>
        /// Return the symbol table at the top of the stack. 
        /// </summary>
        public SymbolCollection Top => _stack[0];

        /// <summary>
        /// Return all symbols ordered from top downwards.
        /// </summary>
        public SymbolCollection[] All => _stack.ToArray();

        /// <summary>
        /// Add a new symbol table to the top of the stack
        /// </summary>
        public void Push(SymbolCollection symbols) {
            _stack.Insert(0, symbols);
        }

        /// <summary>
        /// Removes the symbol table at the top of the stack. You cannot
        /// remove the global symbol table.
        /// </summary>
        public void Pop() {
            Debug.Assert(_stack.Count > 1);
            _stack.RemoveAt(0);
        }
    }
}
