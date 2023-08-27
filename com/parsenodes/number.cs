// JCom Compiler Toolkit
// Number parse node
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

using JComLib;

namespace CCompiler; 

/// <summary>
/// Specifies a number parse node that stores a single constant number.
/// For simplicity, all numbers are stored by the compiler as doubles which
/// is the largest type that can store all number types. The Type on the
/// node distinguishes between the actual type though.
///
/// Beware that doing this introduces some boundary cases for integers
/// so be ready to handle those especially!
/// </summary>
public sealed class NumberParseNode : ParseNode {

    /// <summary>
    /// Creates a number parse node with the specified value.
    /// </summary>
    /// <param name="value">A double value for the node</param>
    public NumberParseNode(Variant value) : base(ParseID.NUMBER) {
        Value = value;
        Type = Symbol.VariantTypeToSymbolType(value.Type);
    }

    /// <summary>
    /// Creates a number parse node with the specified value.
    /// </summary>
    /// <param name="value">A double value for the node</param>
    public NumberParseNode(int value) : base(ParseID.NUMBER) {
        Value = new Variant(value);
        Type = SymType.INTEGER;
    }

    /// <summary>
    /// Returns whether this parse node represents a number.
    /// </summary>
    /// <value><c>true</c> if this instance is a number; otherwise, <c>false</c>.</value>
    public override bool IsNumber => true;

    /// <summary>
    /// Returns whether this parse node represents a constant.
    /// </summary>
    /// <value><c>true</c> if this instance is a constant; otherwise, <c>false</c>.</value>
    public override bool IsConstant => true;

    /// <summary>
    /// Emit this code to load the value to the stack.
    /// </summary>
    /// <param name="emitter">The emitter</param>
    /// <param name="cg">A CodeGenerator object</param>
    /// <param name="returnType">The type required by the caller</param>
    /// <returns>The symbol type of the value generated</returns>
    public override SymType Generate(Emitter emitter, ProgramParseNode cg, SymType returnType) {
        if (emitter == null) {
            throw new ArgumentNullException(nameof(emitter));
        }
        Variant actualValue = Value;
        switch (returnType) {
            case SymType.INTEGER:   actualValue = new Variant(Value.IntValue); break;
            case SymType.FLOAT:     actualValue = new Variant(Value.RealValue); break;
            case SymType.DOUBLE:    actualValue = new Variant(Value.DoubleValue); break;
        }
        emitter.LoadVariant(actualValue);
        return Symbol.VariantTypeToSymbolType(actualValue.Type);
    }

    /// <summary>
    /// Dumps the contents of this parse node to the ParseNode XML
    /// output under the specified parent node.
    /// </summary>
    /// <param name="root">The parent XML node</param>
    public override void Dump(ParseNodeXml root) {
        ParseNodeXml subNode = root.Node("Number");
        subNode.Attribute("Value", Value.ToString());
    }

    /// <summary>
    /// Returns the variant number value.
    /// </summary>
    public override Variant Value { get; set; }
}
