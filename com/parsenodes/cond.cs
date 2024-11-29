// JCom Compiler Toolkit
// Conditional parse node
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
using System.Reflection.Emit;

namespace CCompiler;

/// <summary>
/// Specifies a parse node that defines a conditional block.
/// </summary>
public sealed class ConditionalParseNode : ParseNode {

    private readonly Collection<ParseNode> _exprList = [];

    /// <summary>
    /// Creates a conditional parse node.
    /// </summary>
    public ConditionalParseNode() : base(ParseID.COND) { }

    /// <summary>
    /// Return the list of block nodes for each conditional block.
    /// </summary>
    /// <value>The body list.</value>
    public Collection<BlockParseNode> BodyList { get; } = [];

    /// <summary>
    /// Adds a conditional and body.
    /// </summary>
    /// <param name="expr">Conditional expression node</param>
    /// <param name="body">Statements to be executed</param>
    public void Add(ParseNode expr, BlockParseNode body) {
        _exprList.Add(expr);
        BodyList.Add(body);
    }

    /// <summary>
    /// Dumps the contents of this parse node to the ParseNode XML
    /// output under the specified parent node.
    /// </summary>
    /// <param name="root">The parent XML node</param>
    public override void Dump(ParseNodeXml root) {
        ParseNodeXml blockNode = root.Node("Conditional");
        for (int c = 0; c < _exprList.Count; ++c) {
            ParseNode expr = _exprList[c];
            expr?.Dump(blockNode);
            BlockParseNode body = BodyList[c];
            body?.Dump(blockNode);
        }
    }

    /// <summary>
    /// Emit the code to generate a conditional block. Optimisation
    /// is performed on the tests so that any conditional which is
    /// a constant true causes the block to be executed and the whole
    /// loop code generation terminated after that block. A constant
    /// false causes the block to be ignored.
    /// </summary>
    /// <param name="emitter">Code emitter</param>
    /// <param name="cg">A code generator object</param>
    public override void Generate(Emitter emitter, ProgramParseNode cg) {
        ArgumentNullException.ThrowIfNull(cg);
        int index = 0;

        Label labFalse = emitter.CreateLabel(); // Destination of false condition
        Label labExit = emitter.CreateLabel(); // Exit from entire IF statement

        while (index < _exprList.Count) {
            bool isLastBlock = index == _exprList.Count - 1;
            bool skipBlock = false;

            ParseNode expr = _exprList[index];
            if (expr != null) {
                if (expr.IsConstant) {
                    if (expr.Value.BoolValue) {
                        isLastBlock = true;
                    }
                    else {
                        skipBlock = true;
                    }
                }
                else {
                    cg.GenerateExpression(emitter, SymType.BOOLEAN, _exprList[index]);
                    emitter.BranchIfFalse(isLastBlock ? labExit : labFalse);
                }
            }
            if (!skipBlock) {
                BodyList[index].Generate(emitter, cg);
            }
            if (!isLastBlock) {
                if (!skipBlock) {
                    emitter.Branch(labExit);
                    emitter.MarkLabel(labFalse);
                    labFalse = emitter.CreateLabel();
                }
            }
            else {
                break;
            }
            ++index;
        }
        emitter.MarkLabel(labExit);
    }
}