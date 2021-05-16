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

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using JComLib;

namespace CCompiler {

    /// <summary>
    /// List of valid parse IDs for ParseNode
    /// </summary>
    public enum ParseID { ADD, AND, CONCAT, COND, DIVIDE, EQ, EQUOP, EQV, EXP, FILENAME, GE, BREAK,
        GOTO, GT, IDENT, INSTR, LABEL, LE, LINENUMBER, LOOP, LT, MINUS, MULT, NE, NEQV, NOT, NUMBER,
        OR, PLUS, STRING, SUB, UNKNOWN, XOR, MOD, IDIVIDE, MERGE }

    /// <summary>
    /// Specifies a base parse node. Do not create instances of this node
    /// directly but create the appropriately typed derivation depending on
    /// what you need to store.
    /// </summary>
    public abstract class ParseNode {

        /// <summary>
        /// Creates a parse node with the specified parse ID.
        /// </summary>
        protected ParseNode() {
            ID = ParseID.UNKNOWN;
            Type = SymType.NONE;
        }

        /// <summary>
        /// Creates a parse node with the specified parse ID.
        /// </summary>
        /// <param name="id">A parse ID</param>
        protected ParseNode(ParseID id) {
            ID = id;
        }
        
        /// <summary>
        /// Returns the parse ID of this node.
        /// </summary>
        public ParseID ID { get; private set; }

        /// <summary>
        /// Returns the optional symbol type of this node.
        /// </summary>
        public SymType Type { get; set; }

        /// <summary>
        /// Returns whether this parse node represents a number.
        /// </summary>
        /// <value><c>true</c> if this instance is a number; otherwise, <c>false</c>.</value>
        public bool IsInteger => Type == SymType.INTEGER;

        /// <summary>
        /// Returns whether this parse node represents a string.
        /// </summary>
        /// <value><c>true</c> if this instance is a string; otherwise, <c>false</c>.</value>
        public bool IsString => Type == SymType.CHAR || Type == SymType.FIXEDCHAR;

        /// <summary>
        /// Returns whether this parse node represents a number.
        /// </summary>
        /// <value><c>true</c> if this instance is a number; otherwise, <c>false</c>.</value>
        public virtual bool IsNumber => false;

        /// <summary>
        /// Returns whether this parse node represents a constant.
        /// </summary>
        /// <value><c>true</c> if this instance is a constant; otherwise, <c>false</c>.</value>
        public virtual bool IsConstant => false;

        /// <summary>
        /// Implements the base Value method. A root parsenode does not implement
        /// a value and thus will throw an exception.
        /// </summary>
        public virtual Variant Value {
            get => throw new InvalidOperationException("ParseNode does not implement Value");
            set => throw new InvalidOperationException("ParseNode does not implement Value");
        }

        /// <summary>
        /// Implements the base code generator for the node to invoke a
        /// statement implementation.
        /// </summary>
        /// <param name="cg">The code generator object</param>
        public virtual void Generate(CodeGenerator cg) {
            throw new InvalidOperationException("ParseNode does not implement Generate");
        }

        /// <summary>
        /// Implements the base code generator for the node to invoke a
        /// function implementation with a symbol type.
        /// </summary>
        /// <param name="cg">The code generator object</param>
        /// <param name="returnType">The expected type of the return value</param>
        /// <returns>The computed type</returns>
        public virtual SymType Generate(CodeGenerator cg, SymType returnType) {
            throw new InvalidOperationException("ParseNode does not implement Generate");
        }

        /// <summary>
        /// Implements the base code generator for the node to invoke a
        /// function implementation with a parse node value.
        /// </summary>
        /// <param name="cg">The code generator object</param>
        /// <param name="node">A parse node supplied to the generator function</param>
        /// <returns>The computed type</returns>
        public virtual void Generate(CodeGenerator cg, ParseNode node) {
            throw new InvalidOperationException("ParseNode does not implement Generate");
        }

        /// <summary>
        /// Dumps the contents of this parse node to the ParseNode XML
        /// output under the specified parent node.
        /// </summary>
        /// <param name="root">The parent XML node</param>
        public abstract void Dump(ParseNodeXml root);
    }

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

    /// <summary>
    /// Specifies a parse node that stores the index of a local identifier.
    /// This parse node is used during optimisation and array initialisation
    /// and is not intended to be used during compilation.
    /// </summary>
    public class LocalParseNode : ParseNode {
        private readonly LocalDescriptor _local;

        /// <summary>
        /// Creates a local parse node with the specified local index.
        /// </summary>
        /// <param name="local">The local identifier index</param>
        public LocalParseNode(LocalDescriptor local) {
            _local = local;
        }

        /// <summary>
        /// Dumps the contents of this parse node to the ParseNode XML
        /// output under the specified parent node.
        /// </summary>
        /// <param name="root">The parent XML node</param>
        public override void Dump(ParseNodeXml root) {
            ParseNodeXml subNode = root.Node("Local");
            subNode.Attribute("Index", _local.Index.ToString());
        }

        /// <summary>
        /// Implements the base code generator for the node to invoke a
        /// function implementation with a symbol type.
        /// </summary>
        /// <param name="cg">The code generator object</param>
        /// <param name="returnType">The expected type of the return value</param>
        /// <returns>The computed type</returns>
        public override SymType Generate(CodeGenerator cg, SymType returnType) {
            if (cg == null) {
                throw new ArgumentNullException(nameof(cg));
            }
            cg.Emitter.LoadLocal(_local);
            return Symbol.SystemTypeToSymbolType(_local.Type);
        }
    }

    /// <summary>
    /// Specifies a Symbol parse node that stores a simple symbol table
    /// reference. (Note that this may be merged into IdentifierParseNode
    /// at a later date.)
    /// </summary>
    public class SymbolParseNode : ParseNode {

        /// <summary>
        /// Creates a symbol parse node for the specified symbol.
        /// </summary>
        /// <param name="sym">A symbol entry</param>
        public SymbolParseNode(Symbol sym) : base(ParseID.LABEL) {
            Symbol = sym;
            Type = sym.Type;
        }

        /// <summary>
        /// Emit this code to load the value to the stack.
        /// </summary>
        /// <param name="cg">A CodeGenerator object</param>
        /// <param name="returnType">The type required by the caller</param>
        /// <returns>The symbol type of the value generated</returns>
        public override SymType Generate(CodeGenerator cg, SymType returnType) {
            if (cg == null) {
                throw new ArgumentNullException(nameof(cg));
            }
            return cg.Emitter.LoadSymbol(Symbol);
        }

        /// <summary>
        /// Dumps the contents of this parse node to the ParseNode XML
        /// output under the specified parent node.
        /// </summary>
        /// <param name="root">The parent XML node</param>
        public override void Dump(ParseNodeXml root) {
            ParseNodeXml subNode = root.Node("Symbol");
            subNode.Attribute("Name", Symbol.Name);
        }
        
        /// <summary>
        /// Returns the symbol table entry corresponding to this node.
        /// </summary>
        public Symbol Symbol { get; private set; }
    }

    /// <summary>
    /// Specifies a Symbol Table parse node that stores a reference to a
    /// symbol collection.
    /// </summary>
    public class SymbolTableParseNode : ParseNode {

        /// <summary>
        /// Creates a symbol table parse node with the specified symbol
        /// collection.
        /// </summary>
        /// <param name="symbols">A symbol collection</param>
        public SymbolTableParseNode(SymbolCollection symbols) {
            Symbols = symbols;
        }

        /// <summary>
        /// Returns the symbol collection.
        /// </summary>
        public SymbolCollection Symbols { get; private set; }

        /// <summary>
        /// Dumps the contents of this parse node to the ParseNode XML
        /// output under the specified parent node.
        /// </summary>
        /// <param name="root">The parent XML node</param>
        public override void Dump(ParseNodeXml root) {
            ParseNodeXml blockNode = root.Node("Symbols");
            foreach (Symbol symbol in Symbols) {
                symbol.Dump(blockNode);
            }
        }
    }
}
