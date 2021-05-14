// JCom Compiler Toolkit
// ParametersParseNode class
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

namespace CCompiler {

    /// <summary>
    /// Specifies a single parameter parse node.
    /// </summary>
    public class ParameterParseNode : ParseNode {
        private readonly ParseNode _paramNode;
        private readonly Symbol _symbol;

        /// <summary>
        /// Creates a subroutine or function parameters parse node.
        /// </summary>
        /// <param name="paramNode">A ParseNode object that contains the parameter value</param>
        public ParameterParseNode(ParseNode paramNode) {
            _paramNode = paramNode;
        }

        /// <summary>
        /// Creates a subroutine or function parameters parse node using the
        /// given symbol as the parameter type.
        /// </summary>
        /// <param name="paramNode">A ParseNode object that contains the parameter value</param>
        /// <param name="symbol">A Symbol table item that represents the parameter name</param>
        public ParameterParseNode(ParseNode paramNode, Symbol symbol) {
            _paramNode = paramNode;
            _symbol = symbol;
        }

        /// <value>
        /// Gets or sets a value indicating whether this parameter is passed by
        /// reference.
        /// </value>
        public bool IsByRef { get; set; }

        /// <summary>
        /// Dumps the contents of this parse node to the ParseNode XML
        /// output under the specified parent node.
        /// </summary>
        /// <param name="root">The parent XML node</param>
        public override void Dump(ParseNodeXml root) {
            ParseNodeXml blockNode = root.Node("Parameter");
            _paramNode.Dump(blockNode);
            if (_symbol != null) {
                _symbol.Dump(blockNode);
            }
        }

        /// <summary>
        /// Generate the code to push one parameter onto the caller stack using the
        /// symbol specified in the constructor.
        /// </summary>
        /// <param name="cg">A CodeGenerator object</param>
        /// <returns>The system type corresponding to this parameter.</returns>
        public new Type Generate(CodeGenerator cg) {
            if (cg == null) {
                throw new ArgumentNullException(nameof(cg));
            }
            return Generate(cg, _symbol, new Temporaries(cg.Emitter));
        }

        /// <summary>
        /// Generate the code to push one parameter onto the caller stack using the
        /// symbol specified in the constructor.
        /// </summary>
        /// <param name="cg">A CodeGenerator object</param>
        /// <param name="locals">A Temporaries collection for any local temporaries</param>
        /// <returns>The system type corresponding to this parameter.</returns>
        public Type Generate(CodeGenerator cg, Temporaries locals) {
            return Generate(cg, _symbol, locals);
        }

        /// <summary>
        /// Generate the code to push one parameter onto the caller stack. There are
        /// three different approaches here:
        /// 
        /// 1. Passing a defined variable. We either pass the address of the
        ///    variable (which may be local, parameter or static) if the corresponding
        ///    argument is BYREF, or we pass the value.
        /// 2. Passing a computed value. A computed value has no storage so if the
        ///    corresponding argument is BYREF, we need to allocate storage for it
        ///    and pass the address of the storage.
        /// 3. Finally, a computed value where the argument is BYVAL is passed on
        ///    the stack.
        /// 
        /// </summary>
        /// <param name="cg">A CodeGenerator object</param>
        /// <param name="symParam">Symbol entry for the called function</param>
        /// <param name="locals">A Temporaries collection for any local temporaries</param>
        /// <returns>The system type corresponding to this parameter.</returns>
        public Type Generate(CodeGenerator cg, Symbol symParam, Temporaries locals) {
            if (cg == null) {
                throw new ArgumentNullException(nameof(cg));
            }
            if (locals == null) {
                throw new ArgumentNullException(nameof(locals));
            }

            // Set some flags up-front
            bool isByRef = (symParam != null) ? symParam.IsByRef : IsByRef;
            bool isArray = (symParam != null) && symParam.IsArray;

            // The argument is an array, so the source must either be an array or
            // an array reference.
            if (isArray && _paramNode.ID == ParseID.IDENT) {
                IdentifierParseNode identNode = (IdentifierParseNode)_paramNode;
                Symbol symIdent = identNode.Symbol;
                if (symIdent.IsArray) {
                    GenerateLoadSubArray(cg, identNode, symParam, locals);
                    return symIdent.SystemType;
                }
            }

            // Parameter is an identifier. If passing by reference, pass the address
            // of the identifier in the local context. Otherwise extract and pass the
            // value.
            if (_paramNode.ID == ParseID.IDENT) {
                IdentifierParseNode identNode = (IdentifierParseNode)_paramNode;
                SymType identType = identNode.Type;
                Symbol symIdent = identNode.Symbol;

                if (!symIdent.IsInline && !identNode.HasSubstring) {

                    // If we're passing an existing parameter, it is already
                    // a reference so don't double it up.
                    if (symIdent.IsParameter) {
                        cg.Emitter.LoadParameter(symIdent.ParameterIndex);
                        if (!isByRef && symIdent.IsValueType && symIdent.IsByRef) {
                            cg.Emitter.LoadIndirect(identType);
                        }
                    } else {

                        // Passing an array by reference
                        if (symIdent.IsArray && !identNode.HasIndexes) {
                            cg.GenerateLoadArray(identNode, false);
                            return symIdent.SystemType;
                        }
                        if (isByRef) {
                            cg.LoadAddress(identNode);
                        } else {
                            identNode.Generate(cg);
                            if (symParam != null) {
                                if (!symParam.IsMethod) {
                                    cg.Emitter.ConvertType(identNode.Type, symParam.Type);
                                }
                                identType = symParam.Type;
                            } else {
                                cg.Emitter.ConvertType(symIdent.Type, identType);
                            }
                        }
                    }
                    Type paramType = Symbol.SymTypeToSystemType(identType);
                    if (isByRef) {
                        paramType = paramType.MakeByRefType();
                    }
                    return paramType;
                }
            }
            
            // For reference function parameters, if this argument is not an
            // addressable object, such as a literal value or an expression,
            // then we need to generate local storage for the result and pass
            // the address of that.
            if (isByRef) {
                LocalDescriptor index = locals.New(_paramNode.Type);

                SymType exprType = cg.GenerateExpression(_paramNode.Type, _paramNode);
                cg.Emitter.StoreLocal(index);
                cg.Emitter.LoadLocalAddress(index);
                return Symbol.SymTypeToSystemType(exprType).MakeByRefType();
            }

            // Byval argument passing
            SymType neededType = (symParam != null) ? symParam.Type : Type;
            SymType thisType = cg.GenerateExpression(neededType, _paramNode);
            if (symParam != null) {
                thisType = symParam.Type;
            }
            return Symbol.SymTypeToSystemType(thisType);
        }

        // Emit the code that loads part of an entire array by making a copy of
        // the array to the destination array dimensions and copying from the
        // given offset.
        private void GenerateLoadSubArray(CodeGenerator cg, IdentifierParseNode identNode, Symbol symParam, Temporaries locals) {
            if (!identNode.HasIndexes) {
                cg.GenerateLoadArray(identNode, symParam.IsByRef);
                return;
            }
            
            LocalDescriptor index = locals.New(symParam.SystemType);
            
            cg.GenerateLoadArrayAddress(identNode);
            cg.Emitter.CreateSimpleArray(symParam.ArraySize, Symbol.SymTypeToSystemType(symParam.Type));
            cg.Emitter.Dup();
            cg.Emitter.StoreLocal(index);
            cg.Emitter.LoadInteger(0);
            cg.Emitter.LoadInteger(symParam.ArraySize);
            cg.Emitter.Call(typeof(Array).GetMethod("Copy", new [] { typeof(Array), typeof(int), typeof(Array), typeof(int), typeof(int) }));
            cg.Emitter.LoadLocal(index);
        }
    }

    /// <summary>
    /// Specifies a parameters parse node.
    /// </summary>
    public class ParametersParseNode : ParseNode {
        private Temporaries _locals;

        /// <summary>
        /// Creates a subroutine or function parameters parse node.
        /// </summary>
        public ParametersParseNode() {
            Nodes = new Collection<ParameterParseNode>();
        }

        /// <summary>
        /// Adds the given parameter to this set of parameters.
        /// </summary>
        /// <param name="node">The Parsenode to add</param>
        public void Add(ParseNode node) {
            if (node == null) {
                throw new ArgumentNullException(nameof(node));
            }
            Add(node, false);
        }

        /// <summary>
        /// Adds the given parameter to this set of parameters and specify how the
        /// parameter should be passed to the function.
        /// </summary>
        /// <param name="node">The Parsenode to add</param>
        /// <param name="useByRef">Whether the parameter should be passed by value or reference</param>
        public void Add(ParseNode node, bool useByRef) {
            if (node == null) {
                throw new ArgumentNullException(nameof(node));
            }
            ParameterParseNode paramNode = new(node) {
                Type = node.Type,
                IsByRef = useByRef
            };
            Nodes.Add(paramNode);
        }

        /// <summary>
        /// Adds the given parameter to this set of parameters.
        /// </summary>
        /// <param name="node">The Parsenode to add</param>
        /// <param name="symbol">The symbol associated with the parameter</param>
        public void Add(ParseNode node, Symbol symbol) {
            if (node == null) {
                throw new ArgumentNullException(nameof(node));
            }
            ParameterParseNode paramNode = new(node, symbol);
            paramNode.Type = node.Type;
            Nodes.Add(paramNode);
        }

        /// <summary>
        /// A collection of ParameterParseNode elements for this list.
        /// </summary>
        public Collection<ParameterParseNode> Nodes { get; private set; }

        /// <summary>
        /// Frees the local descriptors allocated during code generation.
        /// </summary>
        public void FreeLocalDescriptors() {
            if (_locals != null) {
                _locals.Free();
            }
        }

        /// <summary>
        /// Dumps the contents of this parse node to the ParseNode XML
        /// output under the specified parent node.
        /// </summary>
        /// <param name="root">The parent XML node</param>
        public override void Dump(ParseNodeXml root) {
            ParseNodeXml blockNode = root.Node("Parameters");
            foreach (ParameterParseNode node in Nodes) {
                node.Dump(blockNode);
            }
        }

        /// <summary>
        /// Generate the code to push the specified parameters onto the caller
        /// stack. Unless the called function or subroutine is being called
        /// indirectly (and thus we may not have knowledge of its parameter
        /// count or types), the number of parameters in the caller and callee
        /// must agree.
        /// </summary>
        /// <param name="cg">A CodeGenerator object</param>
        /// <returns>A list of system types corresponding to the computed parameters.</returns>
        public new Type [] Generate(CodeGenerator cg) {

            if (cg == null) {
                throw new ArgumentNullException(nameof(cg));
            }

            _locals = new Temporaries(cg.Emitter);
            
            Type [] paramTypes = new Type[Nodes.Count];
            for (int c = 0; c < Nodes.Count; ++c) {
                ParameterParseNode paramNode = Nodes[c];
                paramTypes[c] = paramNode.Generate(cg, _locals);
            }
            return paramTypes;
        }

        /// <summary>
        /// Generate the code to push the specified parameters onto the caller
        /// stack. Unless the called function or subroutine is being called
        /// indirectly (and thus we may not have knowledge of its parameter
        /// count or types), the number of parameters in the caller and callee
        /// must agree.
        /// </summary>
        /// <param name="cg">A CodeGenerator object</param>
        /// <param name="sym">Symbol entry for the called function</param>
        /// <returns>A list of system types corresponding to the computed parameters.</returns>
        public Type[] Generate(CodeGenerator cg, Symbol sym) {

            if (cg == null) {
                throw new ArgumentNullException(nameof(cg));
            }
            if (sym == null) {
                throw new ArgumentNullException(nameof(sym));
            }

            int callerParameterCount = Nodes.Count;
            int calleeParameterCount = (sym.Parameters != null) ? sym.Parameters.Count : 0;
            
            if (!sym.IsParameter && callerParameterCount != calleeParameterCount) {
                cg.Error($"Parameter count mismatch for {sym.Name}");
            }

            _locals = new Temporaries(cg.Emitter);

            Type [] paramTypes = new Type[callerParameterCount];
            for (int c = 0; c < Nodes.Count; ++c) {
                ParameterParseNode paramNode = Nodes[c];
                Symbol symParam = sym.Parameters?[c];
                paramTypes[c] = paramNode.Generate(cg, symParam, _locals);
            }
            return paramTypes;
        }
    }
}
