﻿// JCom Compiler Toolkit
// Procedure parse node
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
using System.Diagnostics;
using System.Reflection.Emit;

namespace CCompiler;

/// <summary>
/// Specifies a parse node for a procedure.
/// </summary>
public sealed class ProcedureParseNode : ParseNode {

    /// <summary>
    /// Constructor
    /// </summary>
    public ProcedureParseNode() : base(ParseID.PROCEDURE) {
        Symbols = [];
        Body = new BlockParseNode();
    }

    /// <summary>
    /// Gets or sets the symbol table entry for this procedure.
    /// </summary>
    /// <value>The procedure symbol.</value>
    public Symbol ProcedureSymbol { get; set; }

    /// <summary>
    /// Gets or sets the collection of symbol tables for this procedure.
    /// There may be multiple symbol tables for each nested scope.
    /// </summary>
    /// <value>The local symbol table</value>
    public List<SymbolCollection> Symbols { get; }

    /// <summary>
    /// Gets a value indicating whether this procedure is the main program.
    /// </summary>
    /// <value><c>true</c> if this instance is main program; otherwise, <c>false</c>.</value>
    public bool IsMainProgram => ProcedureSymbol != null && ProcedureSymbol.Modifier.HasFlag(SymModifier.ENTRYPOINT);

    /// <summary>
    /// The root parse node for all initialisation statements.
    /// </summary>
    /// <value>The init list.</value>
    public CollectionParseNode InitList { get; set; }

    /// <summary>
    /// Gets or sets the collection of all labels defined within this
    /// procedure.
    /// </summary>
    /// <value>The label collection</value>
    public Collection<ParseNode> LabelList { get; set; }

    /// <value>
    /// Gets or sets the alternate return count.
    /// </value>
    public int AlternateReturnCount { get; set; }

    /// <summary>
    /// Gets or sets the parse block for the procedure body.
    /// </summary>
    /// <value>The parse node for the loop body.</value>
    public BlockParseNode Body { get; }

    /// <summary>
    /// Whether or not we handle exceptions
    /// </summary>
    public bool CatchExceptions { get; set; }

    /// <summary>
    /// Whether this method is closed (i.e. all identifiers are
    /// automatically local).
    /// </summary>
    public bool IsClosed { get; set; }

    /// <summary>
    /// Predefined label for the return statement for any
    /// inner parse nodes that issue a RETURN statement.
    /// </summary>
    public Label ReturnLabel { get; private set; }

    /// <summary>
    /// Predefined label for the return statement for any
    /// inner parse nodes that issue a RETURN statement.
    /// </summary>
    public LocalDescriptor ReturnIndex { get; set; }

    /// <summary>
    /// Generate the code to emit a procedure.
    /// </summary>
    /// <param name="cg">A CodeGenerator object</param>
    public override void Generate(ProgramParseNode cg) {
        ArgumentNullException.ThrowIfNull(cg);

        // Create the emitter for this method
        JMethod method = ProcedureSymbol.Info as JMethod;
        Debug.Assert(method != null);
        Emitter emitter = method.Emitter;
        cg.CurrentProcedure = this;

        ReturnLabel = emitter.CreateLabel();
        ReturnIndex = null;

        // Generate all locals for this method
        foreach (SymbolCollection symbols in Symbols) {
            symbols.GenerateSymbols(emitter, cg);
        }

        // Generate all the initialisation code
        if (InitList?.Nodes != null) {
            foreach (ParseNode initNode in InitList.Nodes) {
                initNode.Generate(emitter, cg);
            }
        }

        bool needTryBlock = ProcedureSymbol.Modifier.HasFlag(SymModifier.ENTRYPOINT) && CatchExceptions;

        // Generate the body of the procedure
        if (needTryBlock) {
            emitter.SetupTryCatchBlock();
            ++cg.HandlerLevel;
        }

        Body.Generate(emitter, cg);

        if (needTryBlock) {
            emitter.AddDefaultTryCatchHandlerBlock();
            emitter.CloseTryCatchBlock();
            --cg.HandlerLevel;
        }

        emitter.MarkLabel(ReturnLabel);

        if (ReturnIndex != null) {
            emitter.LoadLocal(ReturnIndex);
        }
        emitter.Return();
        emitter.Save();
    }

    /// <summary>
    /// Dumps the contents of this parse node to the ParseNode XML
    /// output under the specified parent node.
    /// </summary>
    /// <param name="root">The parent XML node</param>
    public override void Dump(ParseNodeXml root) {
        ParseNodeXml blockNode = root.Node("Procedure");
        blockNode.Attribute("Name", ProcedureSymbol.Name);
        blockNode.Attribute("IsMainProgram", IsMainProgram.ToString());
        blockNode.Attribute("CatchExceptions", CatchExceptions.ToString());
        blockNode.Attribute("AlternateReturnCount", AlternateReturnCount.ToString());

        ParseNodeXml localSymbols = blockNode.Node("LocalSymbols");
        foreach (SymbolCollection symbols in Symbols) {
            symbols.Dump(localSymbols);
        }

        if (InitList?.Nodes != null) {
            ParseNodeXml initNode = blockNode.Node("InitList");
            foreach (ParseNode init in InitList.Nodes) {
                init.Dump(initNode);
            }
        }
        ParseNodeXml statementNode = blockNode.Node("Body");
        Body.Dump(statementNode);
    }
}