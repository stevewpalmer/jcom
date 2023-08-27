// JCom Compiler Toolkit
// Internal procedure call parse node
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
using System.Reflection.Emit;

namespace CCompiler; 

/// <summary>
/// Specifies a parse node that calls an internal procedure.
/// </summary>
public sealed class CallParseNode : ParseNode {

    /// <summary>
    /// Creates an internal call parse node.
    /// </summary>
    public CallParseNode() {
        AlternateReturnLabels = new Collection<Symbol>();
    }

    /// <summary>
    /// Gets or sets the identifier parse node that specifies the
    /// internal procedure being called.
    /// </summary>
    /// <value>An identifier parse node</value>
    public IdentifierParseNode ProcName { get; set; }

    /// <summary>
    /// Gets or sets the parameters parse node that specifies the
    /// parameters for the procedure call.
    /// </summary>
    /// <value>A ParametersParseNode</value>
    public ParametersParseNode Parameters { get; set; }

    /// <summary>
    /// Gets or sets the list of alternate return labels associated
    /// with this procedure.
    /// </summary>
    /// <value>A ParametersParseNode</value>
    public Collection<Symbol> AlternateReturnLabels { get; set; }

    /// <summary>
    /// Dumps the contents of this parse node to the ParseNode XML
    /// output under the specified parent node.
    /// </summary>
    /// <param name="root">The parent XML node</param>
    public override void Dump(ParseNodeXml root) {
        ParseNodeXml blockNode = root.Node("Call");
        ProcName.Dump(blockNode);
        Parameters.Dump(blockNode);
    }

    /// <summary>
    /// Emit the code to call an internal function complete with
    /// parameters.
    /// </summary>
    /// <param name="emitter">Code emitter</param>
    /// <param name="cg">A code generator object</param>
    public override void Generate(Emitter emitter, ProgramParseNode cg) {
        InternalGenerate(emitter, cg, SymClass.SUBROUTINE, "subroutine");
    }

    /// <summary>
    /// Emit the code to call an internal function complete with
    /// parameters.
    /// </summary>
    /// <param name="emitter">Code emitter</param>
    /// <param name="cg">A code generator object</param>
    /// <param name="returnType">The expected return type</param>
    /// <returns>The actual return type from the function</returns>
    public override SymType Generate(Emitter emitter, ProgramParseNode cg, SymType returnType) {
        return InternalGenerate(emitter, cg, SymClass.FUNCTION, "function");
    }

    // Internal generation logic for a subroutine or function call.
    private SymType InternalGenerate(Emitter emitter, ProgramParseNode cg, SymClass callType, string callName) {
        Symbol sym = ProcName.Symbol;
        SymType thisType = SymType.NONE;
        
        if (!sym.Defined) {
            cg.Error(sym.RefLine, $"Undefined {callName} {sym.Name}");
        }
        if (sym.Class != callType) {
            cg.Error(sym.RefLine, $"{sym.Name} is not a {callName}");
        }
        
        Type[] paramTypes = Parameters.Generate(emitter, cg, sym);
        
        if (sym.IsParameter) {
            ProcName.Generate(emitter, cg);
            emitter.CallIndirect(sym.Type, paramTypes);
            thisType = sym.Type;
        } else {
            if (sym.Info is JMethod method) {
                emitter.Call(method.Builder);
                thisType = sym.Type;
            }
        }

        // Sanity check, make sure alternate return labels are only specified
        // for subroutines.
        if (AlternateReturnLabels.Count > 0 && sym.Class != SymClass.SUBROUTINE) {
            throw new CodeGeneratorException("Codegen error: Alternate return labels only permitted for subroutines");
        }

        // Post-alternate return branching
        if (AlternateReturnLabels.Count > 0) {
            Label [] jumpTable = new Label[AlternateReturnLabels.Count];
            for (int c = 0; c < AlternateReturnLabels.Count; ++c) {
                Symbol symLabel = AlternateReturnLabels[c];
                jumpTable[c] = (Label)symLabel.Info;
            }
            emitter.LoadInteger(1);
            emitter.Sub(SymType.INTEGER);
            emitter.Switch(jumpTable);
        }

        Parameters.FreeLocalDescriptors();
        return thisType;
    }
}
