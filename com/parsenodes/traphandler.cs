// JCom Compiler Toolkit
// Trap handler parse node
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

namespace CCompiler; 

/// <summary>
/// Specifies a parse node that includes statements wrapped in
/// an exception trapper and the exception handling code.
/// </summary>
public sealed class TrappableParseNode : ParseNode {

    /// <summary>
    /// Creates an exception handler parse node.
    /// </summary>
    public TrappableParseNode() { }

    /// <summary>
    /// Gets or sets the statements within the trapper.
    /// </summary>
    /// <value>The BlockParseNode parse node</value>
    public BlockParseNode Body { get; set; }

    /// <summary>
    /// Gets or sets the statements within the handler.
    /// </summary>
    /// <value>The BlockParseNode parse node</value>
    public BlockParseNode Handler { get; set; }

    /// <summary>
    /// Gets or sets the symbol that is set to the runtime error value.
    /// </summary>
    /// <value>The run-time error value symbol</value>
    public Symbol Err { get; set; }

    /// <summary>
    /// Gets or sets the symbol that is set to the runtime error string.
    /// </summary>
    /// <value>The run-time error string symbol</value>
    public Symbol Message { get; set; }

    /// <summary>
    /// Dumps the contents of this parse node to the ParseNode XML
    /// output under the specified parent node.
    /// </summary>
    /// <param name="root">The parent XML node</param>
    public override void Dump(ParseNodeXml root) {
        ParseNodeXml blockNode = root.Node("Trap");
        Err.Dump(blockNode);
        Message.Dump(blockNode);
        Body.Dump(blockNode);
        Handler.Dump(blockNode);
    }

    /// <summary>
    /// Emit the code to create a try and catch handler. This consists of two
    /// blocks: the code within the try and the code within the handler that
    /// runs when a trappable error is raised from the try block.
    /// </summary>
    /// <param name="emitter">Code emitter</param>
    /// <param name="cg">A code generator object</param>
    public override void Generate(Emitter emitter, ProgramParseNode cg) {
        emitter.SetupTryCatchBlock();
        Body.Generate(emitter, cg);
        emitter.AddTryCatchHandlerBlock(Err, Message);
        Handler.Generate(emitter, cg);
        emitter.CloseTryCatchBlock();
    }
}
