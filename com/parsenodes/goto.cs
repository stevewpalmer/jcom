// JCom Compiler Toolkit
// GOTO parse node
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
/// Specifies a parse node for a GOTO statement.
/// </summary>
public sealed class GotoParseNode : ParseNode {

    /// <summary>
    /// Creates a goto parse node.
    /// </summary>
    public GotoParseNode() : base(ParseID.GOTO) {
        Nodes = new Collection<ParseNode>();
    }

    /// <summary>
    /// Creates a goto parse node with the specified label.
    /// </summary>
    public GotoParseNode(ParseNode label) : base(ParseID.GOTO) {
        Nodes = new Collection<ParseNode> {
            label
        };
    }

    /// <summary>
    /// Gets or sets a value indicating whether the index into
    /// the switch table is zero or 1 based.
    /// </summary>
    /// <value><c>true</c> if the index is zero based; otherwise, <c>false</c>.</value>
    public bool IsZeroBased { get; set; }

    /// <summary>
    /// Gets or sets an expression for a computed GOTO.
    /// </summary>
    /// <value>The parse node for the expression</value>
    public ParseNode ValueExpression { get; set; }

    /// <summary>
    /// Adds the given parsenode as a child of this token node.
    /// </summary>
    /// <param name="node">The Parsenode to add</param>
    public void Add(ParseNode node) {
        Nodes.Add(node);
    }

    /// <summary>
    /// Returns a list of all child nodes.
    /// </summary>
    public Collection<ParseNode> Nodes { get; private set; }

    /// <summary>
    /// Dumps the contents of this parse node to the ParseNode XML
    /// output under the specified parent node.
    /// </summary>
    /// <param name="root">The parent XML node</param>
    public override void Dump(ParseNodeXml root) {
        ParseNodeXml blockNode = root.Node("Goto");
        blockNode.Attribute("IsZeroBased", IsZeroBased.ToString());
        if (ValueExpression != null) {
            ValueExpression.Dump(blockNode);
        }
        foreach (ParseNode node in Nodes) {
            node.Dump(blockNode);
        }
    }

    /// <summary>
    /// Emit the code to generate a GOTO statement.
    /// </summary>
    /// <param name="emitter">The emitter</param>
    /// <param name="cg">A code generator object</param>
    public override void Generate(Emitter emitter, ProgramParseNode cg) {
        if (cg == null) {
            throw new ArgumentNullException(nameof(cg));
        }
        if (ValueExpression == null) {
            Symbol sym = ProgramParseNode.GetLabel(Nodes[0]);
            emitter.Branch((Label)sym.Info);
        } else {
            Collection<ParseNode> labelNodes = Nodes;

            if (labelNodes == null || labelNodes.Count == 0) {
                labelNodes = cg.CurrentProcedure.LabelList;
            }

            Label [] jumpTable = new Label[labelNodes.Count];
            for (int c = 0; c < labelNodes.Count; ++c) {
                Symbol sym = ProgramParseNode.GetLabel(labelNodes[c]);
                if (sym.Type == SymType.LABEL) {
                    jumpTable[c] = (Label)sym.Info;
                }
            }
            cg.GenerateExpression(emitter, SymType.INTEGER, ValueExpression);
            if (!IsZeroBased) {
                emitter.LoadInteger(1);
                emitter.Sub(SymType.INTEGER);
            }
            emitter.Switch(jumpTable);
        }
    }
}
