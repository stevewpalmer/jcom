// JCom Compiler Toolkit
// Input manager parse node
//
// Authors:
//  Steve Palmer
//
// Copyright (C) 2021 Steve Palmer
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

using JComalLib;
using JComLib;

namespace CCompiler;

/// <summary>
/// Specifies a parse node that defines a single INPUT item.
/// </summary>
public sealed class InputManagerParseNode : ParseNode {

    /// <summary>
    /// Gets or sets the input prompt.
    /// </summary>
    public ParseNode Prompt { get; set; }

    /// <summary>
    /// Parse node that computes the file handle
    /// </summary>
    public ParseNode FileHandle { get; set; }

    /// <summary>
    /// Parse node that computes the record number
    /// </summary>
    public ParseNode RecordNumber { get; set; }

    /// <summary>
    /// Parse node that computes the input row position
    /// </summary>
    public ParseNode RowPosition { get; set; }

    /// <summary>
    /// Parse node that computes the input column position
    /// </summary>
    public ParseNode ColumnPosition { get; set; }

    /// <summary>
    /// Parse node that computes the maximum input width
    /// </summary>
    public ParseNode MaximumWidth { get; set; }

    /// <summary>
    /// Gets or sets the end of input line termination behaviour.
    /// </summary>
    public LineTerminator Terminator { get; set; }

    /// <summary>
    /// Gets or sets the list of identifiers for the input.
    /// </summary>
    /// <value>The read parameters node</value>
    public IdentifierParseNode[] Identifiers { get; set; }

    /// <summary>
    /// Dumps the contents of this parse node to the ParseNode XML
    /// output under the specified parent node.
    /// </summary>
    /// <param name="root">The parent XML node</param>
    public override void Dump(ParseNodeXml root) {
        ParseNodeXml blockNode = root.Node("InputManager");
        blockNode.Attribute("Terminator", Terminator.ToString());
        if (ColumnPosition != null) {
            ColumnPosition.Dump(blockNode);
        }
        if (RowPosition != null) {
            RowPosition.Dump(blockNode);
        }
        if (MaximumWidth != null) {
            MaximumWidth.Dump(blockNode);
        }
        if (FileHandle != null) {
            FileHandle.Dump(blockNode);
        }
        if (RecordNumber != null) {
            RecordNumber.Dump(blockNode);
        }
        if (Prompt != null) {
            Prompt.Dump(blockNode);
        }
        foreach (IdentifierParseNode identNode in Identifiers) {
            identNode.Dump(blockNode);
        }
    }

    /// <summary>
    /// Emit the code to generate a call to the InputManager library function. A
    /// parse node must be provided which evaluates to the address of the
    /// identifier into which the data is read.
    /// </summary>
    /// <param name="emitter">The emitter</param>
    /// <param name="cg">A CodeGenerator object</param>
    public override void Generate(Emitter emitter, ProgramParseNode cg) {

        ArgumentNullException.ThrowIfNull(emitter);
        ArgumentNullException.ThrowIfNull(cg);

        List<Type> constructorTypes = [];

        // Constructor arguments
        if (RowPosition != null && ColumnPosition != null) {
            cg.GenerateExpression(emitter, SymType.INTEGER, RowPosition);
            cg.GenerateExpression(emitter, SymType.INTEGER, ColumnPosition);
            cg.GenerateExpression(emitter, SymType.INTEGER, MaximumWidth);

            constructorTypes.Add(Symbol.SymTypeToSystemType(RowPosition.Type));
            constructorTypes.Add(Symbol.SymTypeToSystemType(ColumnPosition.Type));
            constructorTypes.Add(Symbol.SymTypeToSystemType(MaximumWidth.Type));
        }

        cg.GenerateExpression(emitter, SymType.INTEGER, FileHandle);
        constructorTypes.Add(typeof(int));

        if (Prompt != null) {
            cg.GenerateExpression(emitter, SymType.CHAR, Prompt);
            emitter.LoadInteger((int)Terminator);

            constructorTypes.Add(typeof(string));
            constructorTypes.Add(typeof(LineTerminator));
        }

        if (RecordNumber != null) {
            cg.GenerateExpression(emitter, SymType.INTEGER, RecordNumber);
            constructorTypes.Add(Symbol.SymTypeToSystemType(RecordNumber.Type));
        }

        Type inputManagerType = typeof(InputManager);
        emitter.CreateObject(inputManagerType, constructorTypes.ToArray());
        LocalDescriptor objIndex = emitter.GetTemporary(inputManagerType);
        emitter.StoreLocal(objIndex);

        foreach (IdentifierParseNode identNode in Identifiers) {

            List<Type> readParamTypes = [];

            emitter.LoadLocal(objIndex);
            if (identNode.IsArrayBase) {
                emitter.GenerateLoadArray(identNode, true);
                readParamTypes.Add(Symbol.SymTypeToSystemType(identNode.Symbol.Type).MakeArrayType().MakeByRefType());
            }
            else if (identNode.HasSubstring) {
                cg.GenerateExpression(emitter, SymType.INTEGER, identNode.SubstringStart);
                if (identNode.SubstringEnd != null) {
                    cg.GenerateExpression(emitter, SymType.INTEGER, identNode.SubstringEnd);
                }
                else {
                    emitter.LoadInteger(-1);
                }
                cg.LoadAddress(emitter, identNode);
                readParamTypes.Add(typeof(int));
                readParamTypes.Add(typeof(int));
                readParamTypes.Add(Symbol.SymTypeToSystemType(identNode.Symbol.Type).MakeByRefType());
            }
            else {
                cg.LoadAddress(emitter, identNode);
                readParamTypes.Add(Symbol.SymTypeToSystemType(identNode.Symbol.Type).MakeByRefType());
            }

            emitter.Call(cg.GetMethodForType(inputManagerType, "READ", readParamTypes.ToArray()));
        }
    }
}