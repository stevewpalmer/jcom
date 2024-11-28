// JCom Compiler Toolkit
// ParametersParseNode class
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
/// Specifies a parameters parse node.
/// </summary>
public class ParametersParseNode : ParseNode {
    private Temporaries _locals;

    /// <summary>
    /// Creates a subroutine or function parameters parse node.
    /// </summary>
    public ParametersParseNode() {
        Nodes = [];
    }

    /// <summary>
    /// Adds the given parameter to this set of parameters.
    /// </summary>
    /// <param name="node">The Parsenode to add</param>
    public void Add(ParseNode node) {
        ArgumentNullException.ThrowIfNull(node);
        Add(node, false);
    }

    /// <summary>
    /// Adds the given parameter to this set of parameters and specify how the
    /// parameter should be passed to the function.
    /// </summary>
    /// <param name="node">The Parsenode to add</param>
    /// <param name="useByRef">Whether the parameter should be passed by value or reference</param>
    public void Add(ParseNode node, bool useByRef) {
        ArgumentNullException.ThrowIfNull(node);
        ParameterParseNode paramNode = new(node) {
            Type = node.Type,
            IsByRef = useByRef
        };
        Nodes.Add(paramNode);
    }

    /// <summary>
    /// Adds the given parameter to this set of parameters.
    /// </summary>
    /// <param name="node">The Parsenode to add</param>
    /// <param name="symbol">The symbol associated with the parameter</param>
    public void Add(ParseNode node, Symbol symbol) {
        ArgumentNullException.ThrowIfNull(node);
        ParameterParseNode paramNode = new(node, symbol) {
            Type = node.Type
        };
        Nodes.Add(paramNode);
    }

    /// <summary>
    /// A collection of ParameterParseNode elements for this list.
    /// </summary>
    public Collection<ParameterParseNode> Nodes { get; private set; }

    /// <summary>
    /// Frees the local descriptors allocated during code generation.
    /// </summary>
    public void FreeLocalDescriptors() {
        if (_locals != null) {
            _locals.Free();
        }
    }

    /// <summary>
    /// Dumps the contents of this parse node to the ParseNode XML
    /// output under the specified parent node.
    /// </summary>
    /// <param name="root">The parent XML node</param>
    public override void Dump(ParseNodeXml root) {
        ParseNodeXml blockNode = root.Node("Parameters");
        foreach (ParameterParseNode node in Nodes) {
            node.Dump(blockNode);
        }
    }

    /// <summary>
    /// Generate the code to push the specified parameters onto the caller
    /// stack. Unless the called function or subroutine is being called
    /// indirectly (and thus we may not have knowledge of its parameter
    /// count or types), the number of parameters in the caller and callee
    /// must agree.
    /// </summary>
    /// <param name="emitter">The emitter</param>
    /// <param name="cg">A CodeGenerator object</param>
    /// <returns>A list of system types corresponding to the computed parameters.</returns>
    public new Type[] Generate(Emitter emitter, ProgramParseNode cg) {
        ArgumentNullException.ThrowIfNull(emitter);
        ArgumentNullException.ThrowIfNull(cg);

        _locals = new Temporaries(emitter);

        Type[] paramTypes = new Type[Nodes.Count];
        for (int c = 0; c < Nodes.Count; ++c) {
            ParameterParseNode paramNode = Nodes[c];
            paramTypes[c] = paramNode.Generate(emitter, cg, _locals);
        }
        return paramTypes;
    }

    /// <summary>
    /// Generate the code to push the specified parameters onto the caller
    /// stack. Unless the called function or subroutine is being called
    /// indirectly (and thus we may not have knowledge of its parameter
    /// count or types), the number of parameters in the caller and callee
    /// must agree.
    /// </summary>
    /// <param name="emitter">Emitter</param>
    /// <param name="cg">A CodeGenerator object</param>
    /// <param name="sym">Symbol entry for the called function</param>
    /// <returns>A list of system types corresponding to the computed parameters.</returns>
    public Type[] Generate(Emitter emitter, ProgramParseNode cg, Symbol sym) {

        ArgumentNullException.ThrowIfNull(cg);
        ArgumentNullException.ThrowIfNull(sym);

        int callerParameterCount = Nodes.Count;
        int calleeParameterCount = sym.Parameters != null ? sym.Parameters.Count : 0;

        if (!sym.IsParameter && callerParameterCount != calleeParameterCount) {
            cg.Error($"Parameter count mismatch for {sym.Name}");
        }

        _locals = new Temporaries(emitter);

        Type[] paramTypes = new Type[callerParameterCount];
        for (int c = 0; c < Nodes.Count; ++c) {
            ParameterParseNode paramNode = Nodes[c];
            Symbol symParam = sym.Parameters?[c];
            paramTypes[c] = paramNode.Generate(emitter, cg, symParam, _locals);
        }
        return paramTypes;
    }
}