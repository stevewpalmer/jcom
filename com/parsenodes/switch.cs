// JCom Compiler Toolkit
// Switch statement parse node
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
using System.Collections.Generic;
using System.Reflection.Emit;

namespace CCompiler {

    /// <summary>
    /// Specifies a parse node that defines a switch/select statement.
    /// </summary>
    public sealed class SwitchParseNode : ParseNode {

        private readonly List<ParseNode> _caseList = new();
        private readonly List<ParseNode> _labelList = new();

        /// <summary>
        /// Gets or sets the compare expression.
        /// </summary>
        /// <value>The compare expression.</value>
        public ParseNode CompareExpression { get; set; }

        /// <summary>
        /// Adds a case statement and target label.
        /// </summary>
        /// <param name="expr">Case expression node</param>
        /// <param name="label">Parsenode for the target label</param>
        public void Add(ParseNode expr, ParseNode label) {
            _caseList.Add(expr);
            _labelList.Add(label);
        }

        /// <summary>
        /// Dumps the contents of this parse node to the ParseNode XML
        /// output under the specified parent node.
        /// </summary>
        /// <param name="root">The parent XML node</param>
        public override void Dump(ParseNodeXml root) {
            ParseNodeXml blockNode = root.Node("Switch");
            CompareExpression.Dump(blockNode);
            for (int c = 0; c < _caseList.Count; ++c) {
                ParseNode node = _caseList[c];
                if (node != null) {
                    node.Dump(blockNode);
                }
                node = _labelList[c];
                if (node != null) {
                    node.Dump(blockNode);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cg">A code generator object</param>
        public override void Generate(ProgramParseNode cg) {
            if (cg == null) {
                throw new ArgumentNullException(nameof(cg));
            }
            int switchCount = _caseList.Count;

            SymType exprType = cg.GenerateExpression(CompareExpression.Type, CompareExpression);
            if (switchCount == 1) {
                cg.GenerateExpression(exprType, _caseList[0]);
                Symbol sym = cg.GetLabel(_labelList[0]);
                cg.Emitter.BranchEqual((Label)sym.Info);
            } else {
                LocalDescriptor index = cg.Emitter.GetTemporary(Symbol.SymTypeToSystemType(exprType));
                cg.Emitter.StoreLocal(index);
                for (int switchIndex = 0; switchIndex < switchCount; ++switchIndex) {
                    cg.Emitter.LoadLocal(index);
                    cg.GenerateExpression(exprType, _caseList[switchIndex]);
                    Symbol sym = cg.GetLabel(_labelList[switchIndex]);
                    cg.Emitter.BranchEqual((Label)sym.Info);
                }
                Emitter.ReleaseTemporary(index);
            }
        }
    }
}