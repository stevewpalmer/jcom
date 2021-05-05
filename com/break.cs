// JCom Compiler Toolkit
// Break/exit parse node
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

namespace CCompiler {

    /// <summary>
    /// Specifies a parse node that defines a break statement.
    /// </summary>
    public sealed class BreakParseNode : ParseNode {

        /// <summary>
        /// Optional break expression
        /// </summary>
        public ParseNode BreakExpression { get; set; }

        /// <summary>
        /// Enclosing body parse node
        /// </summary>
        public LoopParseNode ScopeParseNode { get; set; }

        /// <summary>
        /// Creates a break parse node.
        /// </summary>
        public BreakParseNode() : base(ParseID.BREAK) { }

        /// <summary>
        /// Dumps the contents of this parse node to the ParseNode XML
        /// output under the specified parent node.
        /// </summary>
        /// <param name="root">The parent XML node</param>
        public override void Dump(ParseNodeXml root) {
            ParseNodeXml blockNode = root.Node("Break");
            if (BreakExpression != null) {
                BreakExpression.Dump(blockNode);
            }
        }

        /// <summary>
        /// Emit the code to generate a break statement. If no condition
        /// is specified, this does an immediate jump to the exit label on
        /// the scope. If a condition is specified, the condition is evaluated
        /// and a break to the exit label on the scope occurs if the condition
        /// is true.
        /// </summary>
        /// <param name="cg">A code generator object</param>
        public override void Generate(CodeGenerator cg) {
            if (cg == null) {
                throw new ArgumentNullException(nameof(cg));
            }

            if (IsUnconditionalBreak) {
                cg.Emitter.Branch(ScopeParseNode.ExitLabel);
            } else {
                cg.GenerateExpression(SymType.BOOLEAN, BreakExpression);
                cg.Emitter.BranchIfTrue(ScopeParseNode.ExitLabel);
            }
        }

        /// <summary>
        /// Test whether this is an unconditional break
        /// </summary>
        public bool IsUnconditionalBreak => BreakExpression == null || (BreakExpression.IsConstant && BreakExpression.Value.BoolValue);
    }
}
