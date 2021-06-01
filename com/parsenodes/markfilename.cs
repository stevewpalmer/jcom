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

namespace CCompiler {

    /// <summary>
    /// Specifies a parse node that defines a filename marker in
    /// the parse tree. A filename marker specifies the name of the
    /// original source file that produced the subsequent nodes in
    /// the parse tree up until the end or until a new filename
    /// marker with a different filename.
    /// </summary>
    public sealed class MarkFilenameParseNode : ParseNode {

        /// <summary>
        /// Creates a filename marker parse node.
        /// </summary>
        public MarkFilenameParseNode() : base(ParseID.FILENAME) {}

        /// <summary>
        /// Gets or sets the filename marker.
        /// </summary>
        /// <value>The filename string</value>
        public string Filename { get; set; }

        /// <summary>
        /// Emit the code to mark the file name at the current position
        /// in the generated code.
        /// </summary>
        /// <param name="cg">A code generator object</param>
        public override void Generate(ProgramParseNode cg) {
            if (cg == null) {
                throw new ArgumentNullException(nameof(cg));
            }
            cg.MarkFile(Filename);
        }

        /// <summary>
        /// Dumps the contents of this parse node to the ParseNode XML
        /// output under the specified parent node.
        /// </summary>
        /// <param name="root">The parent XML node</param>
        public override void Dump(ParseNodeXml root) {
            ParseNodeXml subNode = root.Node("Filename");
            subNode.Attribute("Name", Filename);
        }
    }
}