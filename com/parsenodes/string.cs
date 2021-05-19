// JCom Compiler Toolkit
// String parse node
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
using JComLib;

namespace CCompiler {

    /// <summary>
    /// Specifies a string parse node that stores a simple string
    /// value.
    /// </summary>
    public sealed class StringParseNode : ParseNode {

        /// <summary>
        /// Creates a string parse node with the specified string.
        /// </summary>
        /// <param name="value">A string</param>
        public StringParseNode(string value) : base(ParseID.STRING) {
            Value = new Variant(value);
            Type = SymType.CHAR;
        }

        /// <summary>
        /// Creates a string parse node with the specified variant.
        /// </summary>
        /// <param name="value">A variant</param>
        public StringParseNode(Variant value) : base(ParseID.STRING) {
            Value = value;
            Type = SymType.CHAR;
        }

        /// <summary>
        /// Returns whether this parse node represents a constant.
        /// </summary>
        /// <value><c>true</c> if this instance is a constant; otherwise, <c>false</c>.</value>
        public override bool IsConstant => true;

        /// <summary>
        /// Emit this code to load the value to the stack.
        /// </summary>
        /// <param name="cg">A CodeGenerator object</param>
        /// <param name="returnType">The type required by the caller</param>
        /// <returns>The symbol type of the value generated</returns>
        public override SymType Generate(ProgramParseNode cg, SymType returnType) {
            if (cg == null) {
                throw new ArgumentNullException(nameof(cg));
            }
            cg.Emitter.LoadString(Value.StringValue);
            return SymType.CHAR;
        }

        /// <summary>
        /// Dumps the contents of this parse node to the ParseNode XML
        /// output under the specified parent node.
        /// </summary>
        /// <param name="root">The parent XML node</param>
        public override void Dump(ParseNodeXml root) {
            ParseNodeXml subNode = root.Node("String");
            subNode.Attribute("Value", Value.ToString());
        }
        
        /// <summary>
        /// Returns the string value of this node.
        /// </summary>
        public override Variant Value { get; set; }
    }
}
