// JCom Compiler Toolkit
// Parse tree node classes
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

namespace CCompiler; 

/// <summary>
/// Specifies a parse node that stores a variable size list of
/// sub-parse nodes.
/// </summary>
public class CollectionParseNode : ParseNode {

    /// <summary>
    /// Creates a Token parse node with the specified parse ID.
    /// </summary>
    public CollectionParseNode() {
        Nodes = new Collection<ParseNode>();
    }

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
        ParseNodeXml blockNode = root.Node("Block");
        foreach (ParseNode node in Nodes) {
            node.Dump(blockNode);
        }
    }
}
