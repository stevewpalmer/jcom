// JCom Compiler Toolkit
// Assignment parse node
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

using System.Diagnostics;
using System.Reflection;
using JComLib;

namespace CCompiler {

    /// <summary>
    /// Specifies a parse node that defines one or more assignment statements.
    /// </summary>
    public sealed class AssignmentParseNode : ParseNode {

        /// <summary>
        /// Create an AssignmentParseNode.
        /// </summary>
        public AssignmentParseNode() {
        }

        /// <summary>
        /// Create an AssignmentParseNode for a single assignment.
        /// </summary>
        /// <param name="identifier">Identifier to which value is assigned</param>
        /// <param name="value">ParseNode for the value to be assigned</param>
        public AssignmentParseNode(IdentifierParseNode identifier, ParseNode value) {
            Identifiers = new[] { identifier };
            ValueExpressions = new[] { value };
        }

        /// <summary>
        /// Gets or sets the identifier to which the value is assigned
        /// </summary>
        /// <value>The identifier parse node</value>
        public IdentifierParseNode [] Identifiers { get; set; }

        /// <summary>
        /// Gets or sets the parsenode for the value to be assigned.
        /// </summary>
        /// <value>The value parse node</value>
        public ParseNode[] ValueExpressions { get; set; }

        /// <summary>
        /// Dumps the contents of this parse node to the ParseNode XML
        /// output under the specified parent node.
        /// </summary>
        /// <param name="root">The parent XML node</param>
        public override void Dump(ParseNodeXml root) {
            ParseNodeXml blockNode = root.Node("Assign");
            foreach (IdentifierParseNode node in Identifiers) {
                node.Dump(blockNode);
            }
            foreach (ParseNode node in ValueExpressions) {
                node.Dump(blockNode);
            }
        }

        /// <summary>
        /// Emit the code to generate the assignment of an expression
        /// to an identifier. Various forms are permitted:
        /// 
        ///   identifier = value
        ///   identifier(array_indexes) = value
        ///   array = another_array
        ///   identifier(substring) = value
        /// 
        /// </summary>
        /// <param name="emitter">Code emitter</param>
        /// <param name="cg">A code generator object</param>
        public override void Generate(Emitter emitter, ProgramParseNode cg) {
            if (cg == null) {
                throw new ArgumentNullException(nameof(cg));
            }

            for (int arrayIndex = 0; arrayIndex < Identifiers.Length; arrayIndex++) {

                Debug.Assert(arrayIndex < ValueExpressions.Length);
                Symbol sym = Identifiers[arrayIndex].Symbol;
                if (!sym.IsReferenced) {
                    continue;
                }
                ParseNode valueExpression = ValueExpressions[arrayIndex];
                if (sym.IsArray) {
                    GenerateSaveToArray(emitter, cg, Identifiers[arrayIndex], valueExpression);
                    continue;
                }
                if (Identifiers[arrayIndex].HasSubstring) {
                    if (sym.IsParameter) {
                        emitter.LoadParameter(sym.ParameterIndex);
                    } else {
                        emitter.LoadSymbol(sym);
                    }
                    SymType exprType = cg.GenerateExpression(emitter, SymType.NONE, valueExpression);
                    GenerateSaveSubstring(emitter, cg, Identifiers[arrayIndex], exprType);
                    continue;
                }
                if (!sym.IsValueType) {
                    if (sym.IsParameter) {
                        emitter.LoadParameter(sym.ParameterIndex);
                    } else {
                        emitter.LoadSymbol(sym);
                    }
                    SymType wantedType = sym.Type;
                    if (wantedType == SymType.FIXEDCHAR && valueExpression.Type == SymType.CHAR) {
                        wantedType = SymType.CHAR;
                    }
                    SymType exprType = cg.GenerateExpression(emitter, wantedType, valueExpression);
                    if (sym.Type == SymType.FIXEDCHAR) {
                        MethodInfo meth = cg.GetMethodForType(typeof(FixedString), "Set", new[] { Symbol.SymTypeToSystemType(exprType) });
                        emitter.Call(meth);
                    }
                    continue;
                }
                if (sym.IsLocal) {
                    cg.GenerateExpression(emitter, sym.Type, valueExpression);
                    emitter.StoreLocal(sym);
                    continue;
                }
                GenerateStoreArgument(emitter, cg, valueExpression, sym);
            }
        }

        // Emit code that saves the result of an expression to an array element.
        private static void GenerateSaveToArray(Emitter emitter, ProgramParseNode cg, IdentifierParseNode identifier,
            ParseNode valueExpression) {

            Symbol sym = identifier.Symbol;
            
            Debug.Assert(sym.IsArray);
            if (identifier.IsArrayBase) {
                if (sym.IsParameter) {
                    GenerateStoreArgument(emitter, cg, valueExpression, sym);
                } else {
                    cg.GenerateExpression(emitter, SymType.NONE, valueExpression);
                    emitter.StoreLocal(sym.Index);
                }
                return;
            }
            cg.GenerateLoadArrayAddress(emitter, identifier);
            
            if (identifier.HasSubstring) {
                emitter.LoadArrayElement(sym);
                cg.GenerateExpression(emitter, SymType.FIXEDCHAR, valueExpression);
                GenerateSaveSubstring(emitter, cg, identifier, SymType.FIXEDCHAR);
                return;
            }
            if (sym.Type == SymType.FIXEDCHAR) {
                emitter.LoadArrayElement(sym);
                cg.GenerateExpression(emitter, SymType.NONE, valueExpression);
                emitter.Call(cg.GetMethodForType(typeof(FixedString), "Set",
                    new[] {
                        Symbol.SymTypeToSystemType(valueExpression.Type)
                    }));
                return;
            }
            
            cg.GenerateExpression(emitter, sym.Type, valueExpression);
            emitter.StoreArrayElement(sym);
        }

        // Generate the code to write an expression to a substring represented
        // by an identifier which should be fixed string type.
        private static void GenerateSaveSubstring(Emitter emitter, ProgramParseNode cg,
            IdentifierParseNode identifier, SymType charType) {

            Type baseType = Symbol.SymTypeToSystemType(charType);
            
            // Optimise for constant start/end values
            if (identifier.SubstringStart.IsConstant) {
                emitter.LoadInteger(identifier.SubstringStart.Value.IntValue);
            } else {
                cg.GenerateExpression(emitter, SymType.INTEGER, identifier.SubstringStart);
            }
            if (identifier.SubstringEnd == null) {
                emitter.LoadInteger(identifier.Symbol.FullType.Width);
            } else {
                if (identifier.SubstringEnd.IsConstant) {
                    emitter.LoadInteger(identifier.SubstringEnd.Value.IntValue);
                } else {
                    cg.GenerateExpression(emitter, SymType.INTEGER, identifier.SubstringEnd);
                }
            }
            emitter.Call(cg.GetMethodForType(typeof(FixedString), "Set", new [] { baseType, typeof(int), typeof(int) }));
        }

        // Emit the appropriate store parameter index opcode.
        private static void GenerateStoreArgument(Emitter emitter, ProgramParseNode cg,
            ParseNode value, Symbol sym) {

            switch (sym.Linkage) {
                case SymLinkage.BYVAL:
                    cg.GenerateExpression(emitter, sym.Type, value);
                    emitter.StoreParameter(sym.ParameterIndex);
                    break;
                    
                case SymLinkage.BYREF:
                    emitter.LoadParameter(sym.ParameterIndex);
                    cg.GenerateExpression(emitter, sym.Type, value);
                    emitter.StoreIndirect(sym.Type);
                    break;
            }
        }
    }
}
