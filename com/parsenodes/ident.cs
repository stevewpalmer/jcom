// JCom Compiler Toolkit
// Identifier parse node
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

namespace CCompiler {

    /// <summary>
    /// Specifies an Identifier parse node that stores a symbol table
    /// reference along with any optional array indexes.
    /// </summary>
    public class IdentifierParseNode : ParseNode {
        private Symbol _symbol;
        
        /// <summary>
        /// Creates an identifier parse node with the specified symbol.
        /// No indexes are created and the type of the node is set from
        /// the symbol type.
        /// </summary>
        /// <param name="sym">A symbol entry</param>
        public IdentifierParseNode(Symbol sym) : base(ParseID.IDENT) {
            Symbol = sym;
        }

        /// <summary>
        /// Creates an identifier parse node with the specified symbol and
        /// array index. This is a convenient and quick notation for doing
        /// a simple 1D assignment.
        /// </summary>
        /// <param name="sym">A symbol entry</param>
        /// <param name="index">An array index</param>
        public IdentifierParseNode(Symbol sym, int index) : base(ParseID.IDENT) {
            Symbol = sym;
            Indexes = new Collection<ParseNode> {
                new NumberParseNode(index)
            };
        }
        
        /// <summary>
        /// Set or return the optional array indexes for this identifier. If
        /// no indexes have been set, value can be null.
        /// </summary>
        public Collection<ParseNode> Indexes { get; set; }
        
        /// <summary>
        /// Set or return the start offset for a substring if this is a
        /// character identifier.
        /// </summary>
        public ParseNode SubstringStart { get; set; }
        
        /// <summary>
        /// Set or return the end offset for a substring if this is a
        /// character identifier.
        /// </summary>
        public ParseNode SubstringEnd { get; set; }

        /// <summary>
        /// Returns whether this identifier has a substring.
        /// </summary>
        public bool HasSubstring => SubstringStart != null;

        /// <summary>
        /// Returns the symbol table entry corresponding to this identifier.
        /// </summary>
        public Symbol Symbol {
            get => _symbol;
            set {
                _symbol = value;
                Type = (_symbol != null) ? _symbol.Type : SymType.NONE;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this identifier is references an array
        /// itself with no subscripts.
        /// </summary>
        /// <value><c>true</c> if this instance is array base; otherwise, <c>false</c>.</value>
        public bool IsArrayBase => Indexes == null && Symbol.IsArray;

        /// <summary>
        /// Gets a value indicating whether this identifier has array indexes.
        /// </summary>
        /// <value><c>true</c> if this instance has array indexes; otherwise, <c>false</c>.</value>
        public bool HasIndexes => Indexes != null && Indexes.Count > 0;

        /// <summary>
        /// Implements the base code generator for the node to invoke a
        /// statement implementation.
        /// </summary>
        /// <param name="cg">The code generator object</param>
        public override void Generate(ProgramParseNode cg) {
            Generate(cg, Type);
        }

        /// <summary>
        /// Emit this code to load the identifier to the stack.
        /// </summary>
        /// <param name="cg">A CodeGenerator object</param>
        /// <param name="returnType">The type required by the caller</param>
        /// <returns>The symbol type of the value generated</returns>
        public override SymType Generate(ProgramParseNode cg, SymType returnType) {
            if (cg == null) {
                throw new ArgumentNullException(nameof(cg));
            }
            Symbol sym = Symbol;
            SymType thisType;
            
            if (sym.Class == SymClass.INLINE) {
                GenerateInline(cg);
                thisType = sym.Type;
            } else if (sym.IsArray) {
                thisType = cg.GenerateLoadFromArray(this, false);
            } else if (sym.IsIntrinsic || sym.IsExternal) {
                cg.Emitter.LoadFunction(sym);
                thisType = SymType.INTEGER;
            } else if (sym.IsParameter) {
                cg.Emitter.GenerateLoadArgument(sym);
                thisType = sym.Type;
            } else if (sym.Class == SymClass.FUNCTION) {
                cg.Emitter.LoadFunction(sym);
                thisType = SymType.INTEGER;
            } else if (sym.IsLocal) {
                cg.Emitter.LoadSymbol(sym);
                thisType = sym.Type;
            } else {
                Debug.Assert(false, "Unknown identifier type (not local OR parameter)");
                thisType = SymType.NONE;
            }
            if (HasSubstring) {
                thisType = GenerateLoadSubstring(cg);
            }
            return thisType;
        }

        /// <summary>
        /// Dumps the contents of this parse node to the ParseNode XML
        /// output under the specified parent node.
        /// </summary>
        /// <param name="root">The parent XML node</param>
        public override void Dump(ParseNodeXml root) {
            ParseNodeXml blockNode = root.Node("Identifier");
            blockNode.Attribute("Name", Symbol.Name);
            if (HasIndexes) {
                ParseNodeXml subNode = blockNode.Node("Indexes");
                foreach (ParseNode index in Indexes) {
                    index.Dump(subNode);
                }
            }
            if (HasSubstring) {
                ParseNodeXml subNode = blockNode.Node("Substring");
                SubstringStart.Dump(subNode);
                if (SubstringEnd != null) {
                    SubstringEnd.Dump(subNode);
                }
            }
        }

        // Generate the code to extract a substring from the character string
        // at the top of the stack.
        private SymType GenerateLoadSubstring(ProgramParseNode cg) {
            Type charType = Symbol.SymTypeToSystemType(Type);

            if (SubstringStart.IsConstant) {
                cg.Emitter.LoadInteger(SubstringStart.Value.IntValue);
            } else {
                cg.GenerateExpression(SymType.INTEGER, SubstringStart);
            }
            if (SubstringEnd == null) {
                cg.Emitter.Call(cg.GetMethodForType(charType, "Substring", new [] { typeof(int) }));
            } else {
                if (SubstringEnd.IsConstant) {
                    cg.Emitter.LoadInteger(SubstringEnd.Value.IntValue);
                } else {
                    cg.GenerateExpression(SymType.INTEGER, SubstringEnd);
                }
                cg.Emitter.Call(cg.GetMethodForType(charType, "Substring", new [] { typeof(int), typeof(int) }));
            }
            return Type;
        }

        // Emit the code to insert inline code from a symbol. If the identifier
        // represents an inline function then the parse tree for each argument is
        // assigned to its parameters.
        private void GenerateInline(ProgramParseNode cg) {
            Symbol sym = Symbol;
            
            if (Indexes != null) {
                int paramIndex = 0;
                foreach (ParseNode param in Indexes) {
                    if (sym.Parameters != null && paramIndex < sym.Parameters.Count) {
                        Symbol symParam = sym.Parameters[paramIndex];
                        symParam.Class = SymClass.INLINE;
                        symParam.InlineValue = param;
                        symParam.FullType = new SymFullType(param.Type);
                        ++paramIndex;
                    }
                }
            }
            
            // Because we may have adjusted the type of the parameters based
            // on the assigned values, we now need to rescan the tree and
            // equalise and fix the types on the nodes.
            AdjustNodeType(sym.InlineValue);
            cg.GenerateExpression(sym.Type, sym.InlineValue);
        }

        // Scan the expression tree and adjust the node type to the type
        // determined by the arithmetic operation on its operators.
        private void AdjustNodeType(ParseNode node) {
            if (node == null) {
                return;
            }
            switch (node.ID) {
                case ParseID.IDENT: {
                    IdentifierParseNode identNode = (IdentifierParseNode)node;
                    node.Type = identNode.Symbol.Type;
                    break;
                }
                    
                case ParseID.ADD:
                case ParseID.SUB:
                case ParseID.MULT:
                case ParseID.EXP:
                case ParseID.MOD:
                case ParseID.DIVIDE: {
                    BinaryOpParseNode tokenNode = (BinaryOpParseNode)node;
                    AdjustNodeType(tokenNode.Left);
                    AdjustNodeType(tokenNode.Right);
                    
                    SymType type1 = tokenNode.Left.Type;
                    SymType type2 = tokenNode.Right.Type;
                    
                    node.Type = Symbol.LargestType(type1, type2);
                    break;
                }
            }
        }
    }
}
