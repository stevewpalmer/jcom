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

using System;
using System.Collections.ObjectModel;
using System.Reflection.Emit;
using JComLib;

namespace CCompiler {

    /// <summary>
    /// Specifies a parse node that defines an arithmetic based conditional
    /// block such as a FORTRAN arithmetic IF.
    /// </summary>
    public sealed class ArithmeticConditionalParseNode : CollectionParseNode {

        /// <summary>
        /// Gets or sets the expression that is evaluated to determine
        /// the condition.
        /// </summary>
        /// <value>The value expression.</value>
        public ParseNode ValueExpression { get; set; }

        /// <summary>
        /// Emit the code to generate an arithmetic conditional block.
        /// </summary>
        /// <param name="cg">A code generator object</param>
        public override void Generate(CodeGenerator cg) {
            if (cg == null) {
                throw new ArgumentNullException(nameof(cg));
            }
            SymType exprType = cg.GenerateExpression(SymType.NONE, ValueExpression);
            Symbol label1 = cg.GetLabel(Nodes[0]);
            Symbol label2 = cg.GetLabel(Nodes[1]);
            Symbol label3 = cg.GetLabel(Nodes[2]);
            LocalDescriptor tempIndex = cg.Emitter.GetTemporary(Symbol.SymTypeToSystemType(exprType));
            cg.Emitter.StoreLocal(tempIndex);
            cg.Emitter.LoadLocal(tempIndex);
            cg.Emitter.LoadValue(exprType, new Variant(0));
            cg.Emitter.BranchLess((Label)label1.Info);
            cg.Emitter.LoadLocal(tempIndex);
            cg.Emitter.LoadValue(exprType, new Variant(0));
            cg.Emitter.BranchEqual((Label)label2.Info);
            cg.Emitter.Branch((Label)label3.Info);
            cg.Emitter.ReleaseTemporary(tempIndex);
        }
    }

    /// <summary>
    /// Specifies a parse node that defines a conditional block.
    /// </summary>
    public sealed class ConditionalParseNode : ParseNode {

        private readonly Collection<ParseNode> _exprList = new();

        /// <summary>
        /// Creates a conditional parse node.
        /// </summary>
        public ConditionalParseNode() : base(ParseID.COND) {}

        /// <summary>
        /// Return the list of body tokens for each conditional block.
        /// </summary>
        /// <value>The body list.</value>
        public Collection<CollectionParseNode> BodyList { get; } = new();

        /// <summary>
        /// Adds a conditional and body.
        /// </summary>
        /// <param name="expr">Conditional expression node</param>
        /// <param name="body">Statements to be executed</param>
        public void Add(ParseNode expr, CollectionParseNode body) {
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
                if (expr != null) {
                    expr.Dump(blockNode);
                }
                ParseNode body = BodyList[c];
                if (body != null) {
                    body.Dump(blockNode);
                }
            }
        }

        /// <summary>
        /// Emit the code to generate a conditional block. Optimisation
        /// is performed on the tests so that any conditional which is
        /// a constant true causes the block to be executed and the whole
        /// loop code generation terminated after that block. A constant
        /// false causes the block to be ignored.
        /// </summary>
        /// <param name="cg">A code generator object</param>
        public override void Generate(CodeGenerator cg) {
            if (cg == null) {
                throw new ArgumentNullException(nameof(cg));
            }
            int index = 0;

            Label labFalse = cg.Emitter.CreateLabel(); // Destination of false condition
            Label labExit = cg.Emitter.CreateLabel();  // Exit from entire IF statement

            while (index < _exprList.Count) {
                bool isLastBlock = index == _exprList.Count - 1;
                bool skipBlock = false;

                ParseNode expr = _exprList[index];
                if (expr != null) {
                    if (expr.IsConstant) {
                        if (expr.Value.BoolValue) {
                            isLastBlock = true;
                        } else {
                            skipBlock = true;
                        }
                    } else {
                        cg.GenerateExpression(SymType.BOOLEAN, _exprList[index]);
                        cg.Emitter.BranchIfFalse(isLastBlock ? labExit : labFalse);
                    }
                }
                if (!skipBlock) {
                    CollectionParseNode body = BodyList[index];
                    foreach (ParseNode node in body.Nodes) {
                        node.Generate(cg);
                    }
                }
                if (!isLastBlock) {
                    if (!skipBlock) {
                        cg.Emitter.Branch(labExit);
                        cg.Emitter.MarkLabel(labFalse);
                        labFalse = cg.Emitter.CreateLabel();
                    }
                } else {
                    break;
                }
                ++index;
            }
            cg.Emitter.MarkLabel(labExit);
        }
    }
}
