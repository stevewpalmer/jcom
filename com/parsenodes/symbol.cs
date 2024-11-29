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

namespace CCompiler;

/// <summary>
/// Specifies a Symbol parse node that stores a simple symbol table
/// reference. (Note that this may be merged into IdentifierParseNode
/// at a later date.)
/// </summary>
public class SymbolParseNode : ParseNode {

    /// <summary>
    /// Creates a symbol parse node for the specified symbol.
    /// </summary>
    /// <param name="sym">A symbol entry</param>
    public SymbolParseNode(Symbol sym) : base(ParseID.LABEL) {
        Symbol = sym;
        Type = sym.Type;
    }

    /// <summary>
    /// Emit this code to load the value to the stack.
    /// </summary>
    /// <param name="emitter">The emitter</param>
    /// <param name="cg">Code generator</param>
    /// <param name="returnType">The type required by the caller</param>
    /// <returns>The symbol type of the value generated</returns>
    public override SymType Generate(Emitter emitter, ProgramParseNode cg, SymType returnType) {
        ArgumentNullException.ThrowIfNull(emitter);
        return emitter.LoadSymbol(Symbol);
    }

    /// <summary>
    /// Dumps the contents of this parse node to the ParseNode XML
    /// output under the specified parent node.
    /// </summary>
    /// <param name="root">The parent XML node</param>
    public override void Dump(ParseNodeXml root) {
        ParseNodeXml subNode = root.Node("Symbol");
        subNode.Attribute("Name", Symbol.Name);
    }

    /// <summary>
    /// Returns the symbol table entry corresponding to this node.
    /// </summary>
    public Symbol Symbol { get; }
}