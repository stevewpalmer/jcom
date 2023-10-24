// JCom Compiler Toolkit
// Unary operator parse node
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
using JComLib;

namespace CCompiler;

/// <summary>
/// Specifies a unary operator parse node which encapsulates
/// an expression operator with a single operand.
/// </summary>
public sealed class UnaryOpParseNode : ParseNode {

    /// <summary>
    /// Creates a unary parse node of the specified type.
    /// </summary>
    /// <param name="id">The ID of the operator</param>
    public UnaryOpParseNode(ParseID id) : base(id) { }

    /// <summary>
    /// Returns whether this unary operator has a numeric operand.
    /// </summary>
    /// <value><c>true</c> if this instance is a number; otherwise, <c>false</c>.</value>
    public override bool IsNumber => Operand.IsNumber;

    /// <summary>
    /// Gets or sets the operand.
    /// </summary>
    /// <value>The Parse node for the operand</value>
    public ParseNode Operand { get; set; }

    /// <summary>
    /// Dumps the contents of this parse node to the ParseNode XML
    /// output under the specified parent node.
    /// </summary>
    /// <param name="root">The parent XML node</param>
    public override void Dump(ParseNodeXml root) {
        ParseNodeXml blockNode = root.Node("UnaryOp");
        blockNode.Attribute("ID", ID.ToString());
        Operand.Dump(blockNode);
    }

    /// <summary>
    /// Emit this code to load the value to the stack.
    /// </summary>
    /// <param name="emitter">Code emitter</param>
    /// <param name="cg">A CodeGenerator object</param>
    /// <param name="returnType">The type required by the caller</param>
    /// <returns>The symbol type of the value generated</returns>
    public override SymType Generate(Emitter emitter, ProgramParseNode cg, SymType returnType) {
        if (cg == null) {
            throw new ArgumentNullException(nameof(cg));
        }
        switch (ID) {
            case ParseID.MINUS: return GenerateMinus(emitter, cg);
            case ParseID.NOT: return GenerateNot(emitter, cg);
        }
        Debug.Assert(false, "Unsupported parse ID for UnaryOpParseNode");
        return Symbol.VariantTypeToSymbolType(Value.Type);
    }

    // Generate the code for a unary NOT logical operator
    private SymType GenerateNot(Emitter emitter, ProgramParseNode cg) {

        // HANDLE a NOT on a fixed char by representing it as a call to the IsEmpty
        // property.
        if (Operand.Type == SymType.FIXEDCHAR) {
            cg.GenerateExpression(emitter, SymType.FIXEDCHAR, Operand);
            emitter.Call(typeof(FixedString).GetMethod("get_IsEmpty", Array.Empty<Type>()));
            return SymType.BOOLEAN;
        }
        cg.GenerateExpression(emitter, Type, Operand);
        emitter.LoadInteger(0);
        emitter.CompareEquals();
        return Type;
    }

    // Generate the code for a unary minus operator
    private SymType GenerateMinus(Emitter emitter, ProgramParseNode cg) {
        cg.GenerateExpression(emitter, Type, Operand);
        emitter.Neg(Type);
        return Type;
    }
}