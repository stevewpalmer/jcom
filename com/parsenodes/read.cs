﻿// JCom Compiler Toolkit
// Read parse node
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

using JFortranLib;

namespace CCompiler;

/// <summary>
/// Specifies a parse node that defines a READ statement.
/// </summary>
public sealed class ReadParseNode : ParseNode {

    /// <summary>
    /// Gets or sets the list of parameters to initialise the ReadManager
    /// object.
    /// </summary>
    /// <value>The read manager parameters node</value>
    public ParametersParseNode ReadManagerParamsNode { get; set; }

    /// <summary>
    /// Gets or sets the list of parameters for the READ function.
    /// </summary>
    /// <value>The read parameters node</value>
    public ParametersParseNode ReadParamsNode { get; set; }

    /// <summary>
    /// Gets or sets the list of arguments for the READ.
    /// </summary>
    /// <value>The argument list</value>
    public CollectionParseNode ArgList { get; set; }

    /// <summary>
    /// Gets or sets the symbol representing the optional end label.
    /// This may be NULL if no END was specified in the control list.
    /// </summary>
    /// <value>The end label symbol parse node</value>
    public SymbolParseNode EndLabel { get; set; }

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
        ParseNodeXml blockNode = root.Node("Read");
        ReadManagerParamsNode.Dump(blockNode.Node("ReadManagerParams"));
        ReadParamsNode.Dump(blockNode.Node("ReadParams"));
        ArgList.Dump(blockNode);
    }

    /// <summary>
    /// Emit the code to generate a call to the READ library function.
    /// </summary>
    /// <param name="emitter">Code emitter</param>
    /// <param name="cg">A CodeGenerator object</param>
    public override void Generate(Emitter emitter, ProgramParseNode cg) {
        ArgumentNullException.ThrowIfNull(cg);

        Type readManagerType = typeof(ReadManager);
        Type[] paramTypes = ReadManagerParamsNode.Generate(emitter, cg);

        emitter.CreateObject(readManagerType, paramTypes);
        LocalDescriptor objIndex = emitter.GetTemporary(readManagerType);
        emitter.StoreLocal(objIndex);

        if (EndLabel != null) {
            emitter.LoadLocal(objIndex);
            emitter.LoadInteger(1);
            emitter.Call(readManagerType.GetMethod("set_HasEnd", [typeof(bool)]));
        }

        if (ErrLabel != null) {
            emitter.LoadLocal(objIndex);
            emitter.LoadInteger(1);
            emitter.Call(readManagerType.GetMethod("set_HasErr", [typeof(bool)]));
        }

        LocalDescriptor index = emitter.GetTemporary(typeof(int));

        // Construct a parsenode that will be called for each item in the loop
        // including any implied DO loops.
        ReadItemParseNode itemNode = new() {
            ReturnIndex = index,
            ErrLabel = ErrLabel,
            EndLabel = EndLabel,
            ReadParamsNode = ReadParamsNode,
            ReadManagerIndex = objIndex
        };

        if (ArgList != null && ArgList.Nodes.Count > 0) {
            int countOfArgs = ArgList.Nodes.Count;

            for (int c = 0; c < countOfArgs; ++c) {
                itemNode.Generate(emitter, cg, ArgList.Nodes[c]);
                ReadParamsNode.FreeLocalDescriptors();
            }
        }
        else {
            itemNode.Generate(emitter, cg, null);
        }

        // Issue an EndRecord to complete this record read
        emitter.LoadLocal(objIndex);
        emitter.Call(cg.GetMethodForType(readManagerType, "EndRecord", System.Type.EmptyTypes));

        Emitter.ReleaseTemporary(index);
        Emitter.ReleaseTemporary(objIndex);
    }
}