// JCom Compiler Toolkit
// Static Parse Node
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

using System.Reflection;

namespace CCompiler; 

/// <summary>
/// Specifies a parse node that stores the index of a local identifier.
/// This parse node is used during optimisation and array initialisation
/// and is not intended to be used during compilation.
/// </summary>
public class StaticParseNode : ParseNode {
    private readonly FieldInfo _static;

    /// <summary>
    /// Creates a local parse node with the specified local index.
    /// </summary>
    /// <param name="local">The local identifier index</param>
    public StaticParseNode(FieldInfo staticField) {
        _static = staticField;
    }

    /// <summary>
    /// Dumps the contents of this parse node to the ParseNode XML
    /// output under the specified parent node.
    /// </summary>
    /// <param name="root">The parent XML node</param>
    public override void Dump(ParseNodeXml root) {
        ParseNodeXml subNode = root.Node("Static");
        subNode.Attribute("Info", _static.Name);
    }

    /// <summary>
    /// Implements the base code generator for the node to invoke a
    /// function implementation with a symbol type.
    /// </summary>
    /// <param name="emitter">The emitter</param>
    /// <param name="cg">The code generator object</param>
    /// <param name="returnType">The expected type of the return value</param>
    /// <returns>The computed type</returns>
    public override SymType Generate(Emitter emitter, ProgramParseNode cg, SymType returnType) {
        if (emitter == null) {
            throw new ArgumentNullException(nameof(emitter));
        }
        emitter.LoadStatic(_static);
        return Symbol.SystemTypeToSymbolType(_static.FieldType);
    }
}
