// JCom Compiler Toolkit
// Write parse node
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

using System.Reflection.Emit;
using JFortranLib;

namespace CCompiler; 

/// <summary>
/// Specifies a parse node that defines a single write item.
/// </summary>
public sealed class WriteItemParseNode : ParseNode {

    private const string _libraryName = "JFortranLib.IO,jforlib";
    private const string _name = "WRITE";

    /// <summary>
    /// Gets or sets the local index to store the return value.
    /// </summary>
    /// <value>The local index of the return.</value>
    public LocalDescriptor ReturnIndex { get; set; }

    /// <summary>
    /// Gets or sets the local index of the WriteManager instance
    /// to use for this item.
    /// </summary>
    /// <value>The local index of the WriteManager instance.</value>
    public LocalDescriptor WriteManagerIndex { get; set; }

    /// <summary>
    /// Gets or sets the list of parameters for the write function.
    /// </summary>
    /// <value>The write parameters node</value>
    public ParametersParseNode WriteParamsNode { get; set; }

    /// <summary>
    /// Gets or sets the symbol representing the optional error label.
    /// This may be NULL if no ERR was specified in the control list.
    /// </summary>
    /// <value>The error label symbol parse node</value>
    public SymbolParseNode ErrLabel { get; set; }

    /// <summary>
    /// Dumps the contents of this parse node to the ParseNode XML
    /// output under the specified parent node.
    /// </summary>
    /// <param name="root">The parent XML node</param>
    public override void Dump(ParseNodeXml root) {
        ParseNodeXml blockNode = root.Node("WriteItem");
        WriteParamsNode.Dump(blockNode);
    }

    /// <summary>
    /// Emit the code to generate a call to the write library function. A
    /// parse node must be specified which evaluates to the value to be
    /// written.
    /// </summary>
    /// <param name="emitter">Code emitter</param>
    /// <param name="cg">A CodeGenerator object</param>
    /// <param name="node">A parse node for the WRITE identifier</param>
    public override void Generate(Emitter emitter, ProgramParseNode cg, ParseNode node) {
        if (cg == null) {
            throw new ArgumentNullException(nameof(cg));
        }
        if (node is LoopParseNode loopNode) {
            loopNode.Callback = this;
            loopNode.Generate(emitter, cg);
        } else {
            Type writeManagerType = typeof(WriteManager);
            List<Type> writeParamTypes = new();

            emitter.LoadLocal(WriteManagerIndex);
            writeParamTypes.Add(writeManagerType);

            if (WriteParamsNode != null) {
                writeParamTypes.AddRange(WriteParamsNode.Generate(emitter, cg));
            }

            if (node != null) {
                ParameterParseNode exprParam = new(node);
                writeParamTypes.Add(exprParam.Generate(emitter, cg));
            }

            emitter.Call(cg.GetMethodForType(_libraryName, _name, writeParamTypes.ToArray()));
            emitter.StoreLocal(ReturnIndex);

            if (ErrLabel != null) {
                emitter.LoadLocal(ReturnIndex);
                emitter.LoadInteger(-1);
                emitter.BranchEqual((Label)ErrLabel.Symbol.Info);
            }
        }
    }
}
