// JCom Compiler Toolkit
// Parse tree node classes
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
    /// Specifies a Symbol Table parse node that stores a reference to a
    /// symbol collection.
    /// </summary>
    public class SymbolTableParseNode : ParseNode {

        /// <summary>
        /// Creates a symbol table parse node with the specified symbol
        /// collection.
        /// </summary>
        /// <param name="symbols">A symbol collection</param>
        public SymbolTableParseNode(SymbolCollection symbols) {
            Symbols = symbols;
        }

        /// <summary>
        /// Returns the symbol collection.
        /// </summary>
        public SymbolCollection Symbols { get; private set; }

        /// <summary>
        /// Dumps the contents of this parse node to the ParseNode XML
        /// output under the specified parent node.
        /// </summary>
        /// <param name="root">The parent XML node</param>
        public override void Dump(ParseNodeXml root) {
            ParseNodeXml blockNode = root.Node("Symbols");
            foreach (Symbol symbol in Symbols) {
                symbol.Dump(blockNode);
            }
        }
    }
}
