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

namespace CCompiler; 

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
    /// <param name="emitter">The emitter</param>
    /// <param name="cg">A CodeGenerator object</param>
    /// <returns>The system type corresponding to this parameter.</returns>
    public new Type Generate(Emitter emitter, ProgramParseNode cg) {
        if (cg == null) {
            throw new ArgumentNullException(nameof(cg));
        }
        return Generate(emitter, cg, _symbol, new Temporaries(emitter));
    }

    /// <summary>
    /// Generate the code to push one parameter onto the caller stack using the
    /// symbol specified in the constructor.
    /// </summary>
    /// <param name="emitter">The emitter</param>
    /// <param name="cg">A CodeGenerator object</param>
    /// <param name="locals">A Temporaries collection for any local temporaries</param>
    /// <returns>The system type corresponding to this parameter.</returns>
    public Type Generate(Emitter emitter, ProgramParseNode cg, Temporaries locals) {
        return Generate(emitter, cg, _symbol, locals);
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
    /// <param name="emitter">The emitter</param>
    /// <param name="cg">A CodeGenerator object</param>
    /// <param name="symParam">Symbol entry for the called function</param>
    /// <param name="locals">A Temporaries collection for any local temporaries</param>
    /// <returns>The system type corresponding to this parameter.</returns>
    public Type Generate(Emitter emitter, ProgramParseNode cg, Symbol symParam, Temporaries locals) {
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
                GenerateLoadSubArray(emitter, cg, identNode, symParam, locals);
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
                    emitter.LoadParameter(symIdent.ParameterIndex);
                    if (!isByRef && symIdent.IsValueType && symIdent.IsByRef) {
                        emitter.LoadIndirect(identType);
                    }
                    emitter.ConvertType(symIdent.Type, identType);
                } else {

                    // Passing an array by reference
                    if (symIdent.IsArray && !identNode.HasIndexes) {
                        emitter.GenerateLoadArray(identNode, false);
                        return symIdent.SystemType;
                    }
                    if (isByRef) {
                        cg.LoadAddress(emitter, identNode);
                    } else {
                        identNode.Generate(emitter, cg);
                        if (symParam != null) {
                            if (!symParam.IsMethod) {
                                emitter.ConvertType(identNode.Type, symParam.Type);
                            }
                            identType = symParam.Type;
                        } else {
                            emitter.ConvertType(symIdent.Type, identType);
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

            SymType exprType = cg.GenerateExpression(emitter, _paramNode.Type, _paramNode);
            emitter.StoreLocal(index);
            emitter.LoadLocalAddress(index);
            return Symbol.SymTypeToSystemType(exprType).MakeByRefType();
        }

        // Byval argument passing
        SymType neededType = (symParam != null) ? symParam.Type : Type;
        SymType thisType = cg.GenerateExpression(emitter, neededType, _paramNode);
        if (symParam != null) {
            thisType = symParam.Type;
        }
        return Symbol.SymTypeToSystemType(thisType);
    }

    // Emit the code that loads part of an entire array by making a copy of
    // the array to the destination array dimensions and copying from the
    // given offset.
    private static void GenerateLoadSubArray(Emitter emitter, ProgramParseNode cg, IdentifierParseNode identNode,
        Symbol symParam, Temporaries locals) {

        if (!identNode.HasIndexes) {
            emitter.GenerateLoadArray(identNode, symParam.IsByRef);
            return;
        }
        
        LocalDescriptor index = locals.New(symParam.SystemType);
        
        cg.GenerateLoadArrayAddress(emitter, identNode);
        emitter.CreateSimpleArray(symParam.ArraySize, Symbol.SymTypeToSystemType(symParam.Type));
        emitter.Dup();
        emitter.StoreLocal(index);
        emitter.LoadInteger(0);
        emitter.LoadInteger(symParam.ArraySize);
        emitter.Call(typeof(Array).GetMethod("Copy", new [] { typeof(Array), typeof(int), typeof(Array), typeof(int), typeof(int) }));
        emitter.LoadLocal(index);
    }
}
