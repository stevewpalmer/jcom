// JCom Compiler Toolkit
// Dump the parse tree as XML
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

using System.Xml;

namespace CCompiler;

/// <summary>
/// Defines a single parse tree dump node.
/// </summary>
public class ParseNodeXml {

    private readonly XmlDocument _doc;
    private readonly XmlNode _thisNode;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParseNodeXml" /> class.
    /// </summary>
    /// <param name="doc">The Xml root document</param>
    /// <param name="node">The XmlNode associated with this node</param>
    private ParseNodeXml(XmlDocument doc, XmlNode node) {
        _doc = doc;
        _thisNode = node;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParseNodeXml" /> class.
    /// </summary>
    /// <param name="doc">The Xml root document</param>
    /// <param name="nodeName">The name of this node</param>
    public ParseNodeXml(XmlDocument doc, string nodeName) {
        _doc = doc;
        _thisNode = _doc.CreateElement(nodeName);
        _doc.AppendChild(_thisNode);
    }

    /// <summary>
    /// Create a new node under this node with the given name.
    /// </summary>
    /// <param name="name">A name for the new node</param>
    /// <returns>The new group node</returns>
    public ParseNodeXml Node(string name) {
        XmlNode newNode = _doc.CreateElement(name);
        _thisNode.AppendChild(newNode);
        return new ParseNodeXml(_doc, newNode);
    }

    /// <summary>
    /// Add the specified attribute name and value to this node.
    /// </summary>
    /// <param name="attributeName">Attribute name</param>
    /// <param name="attributeValue">Attribute value</param>
    public void Attribute(string attributeName, string attributeValue) {
        if (_thisNode == null) {
            throw new InvalidOperationException("No parent node defined");
        }
        if (_thisNode.Attributes == null) {
            throw new InvalidOperationException("No attributes on parent node");
        }
        XmlAttribute attr = _doc.CreateAttribute(attributeName);
        attr.Value = attributeValue;
        _thisNode.Attributes.Append(attr);
    }

    /// <summary>
    /// Write the specified nodeName and value into this node.
    /// </summary>
    /// <param name="nodeName">Node name</param>
    /// <param name="value">Node value</param>
    public void Write(string nodeName, string value) {
        if (_thisNode == null) {
            throw new InvalidOperationException("No parent node defined");
        }
        XmlElement element = _doc.CreateElement(nodeName);
        element.InnerText = value;
        _thisNode.AppendChild(element);
    }
}

/// <summary>
/// Implements the static ParseTreeDump class which dumps the parse
/// tree to an XML document.
/// </summary>
public static class ParseTreeXml {

    /// <summary>
    /// Generate and return an XmlDocument that represents the
    /// parse tree from the given root node down.
    /// </summary>
    /// <param name="rootNode">Parse tree root node</param>
    /// <returns>The XmlDocument for the root node</returns>
    public static XmlDocument Tree(ParseNode rootNode) {
        XmlDocument doc = new();

        XmlNode docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
        doc.AppendChild(docNode);

        rootNode.Dump(new ParseNodeXml(doc, "Root"));
        return doc;
    }
}