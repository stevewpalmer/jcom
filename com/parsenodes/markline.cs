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

namespace CCompiler;

/// <summary>
/// Specifies a parse node that defines a line number marker.
/// The line number marker specifies the line number of the original
/// source file that corresponds to the subsequent parse nodes up
/// until the end of the file or the next line number marker with
/// a different line number.
/// </summary>
public sealed class MarkLineParseNode : ParseNode {

    /// <summary>
    /// Creates a line number marker parse node.
    /// </summary>
    public MarkLineParseNode() : base(ParseID.LINENUMBER) { }

    /// <summary>
    /// Gets or sets the line number marker.
    /// </summary>
    /// <value>An integer line number</value>
    public int LineNumber { get; set; }

    /// <summary>
    /// Get the displayable line number for languages such as BASIC or COMAL
    /// </summary>
    public int DisplayableLineNumber { get; set; }

    /// <summary>
    /// Emit the code to mark the line number at the current position
    /// in the generated code.
    /// </summary>
    /// <param name="cg">A code generator object</param>
    public override void Generate(ProgramParseNode cg) {
        if (cg == null) {
            throw new ArgumentNullException(nameof(cg));
        }
        cg.MarkLine(null, LineNumber);
    }

    /// <summary>
    /// Emit the code to mark the line number at the current position
    /// in the generated code.
    /// </summary>
    /// <param name="emitter">Code emitter</param>
    /// <param name="cg">A code generator object</param>
    public override void Generate(Emitter emitter, ProgramParseNode cg) {
        if (cg == null) {
            throw new ArgumentNullException(nameof(cg));
        }
        cg.MarkLine(emitter, LineNumber);
    }

    /// <summary>
    /// Dumps the contents of this parse node to the ParseNode XML
    /// output under the specified parent node.
    /// </summary>
    /// <param name="root">The parent XML node</param>
    public override void Dump(ParseNodeXml root) {
        ParseNodeXml subNode = root.Node("Line");
        subNode.Attribute("Number", LineNumber.ToString());
    }
}