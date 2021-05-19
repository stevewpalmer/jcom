// JCom Compiler Toolkit
// Source code markers parse node
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
using System.Reflection.Emit;

namespace CCompiler {

    /// <summary>
    /// Specifies a parse node that marks a label at the current
    /// point in the parse tree.
    /// </summary>
    public sealed class MarkLabelParseNode : ParseNode {

        /// <summary>
        /// Gets or sets the symbol representing the label.
        /// </summary>
        /// <value>Symbol table entry for the label</value>
        public Symbol Label { get; set; }

        /// <summary>
        /// Emit the code to mark the label at the current position
        /// in the generated code.
        /// </summary>
        /// <param name="cg">A code generator object</param>
        public override void Generate(ProgramParseNode cg) {
            if (cg == null) {
                throw new ArgumentNullException(nameof(cg));
            }
            if (Label.Type == SymType.LABEL && Label.IsReferenced) {
                cg.Emitter.MarkLabel((Label)Label.Info);
            }
        }

        /// <summary>
        /// Dumps the contents of this parse node to the ParseNode XML
        /// output under the specified parent node.
        /// </summary>
        /// <param name="root">The parent XML node</param>
        public override void Dump(ParseNodeXml root) {
            ParseNodeXml subNode = root.Node("Label");
            subNode.Attribute("Name", Label.Name);
        }
    }
}