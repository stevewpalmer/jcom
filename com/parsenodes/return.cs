// JCom Compiler Toolkit
// Return parse node
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
/// Specifies a parse node for a return statement.
/// </summary>
public sealed class ReturnParseNode : ParseNode {

    /// <summary>
    /// Gets or sets the return expression.
    /// </summary>
    /// <value>The return expression.</value>
    public ParseNode ReturnExpression { get; set; }

    /// <summary>
    /// Dumps the contents of this parse node to the ParseNode XML
    /// output under the specified parent node.
    /// </summary>
    /// <param name="root">The parent XML node</param>
    public override void Dump(ParseNodeXml root) {
        ParseNodeXml blockNode = root.Node("Return");
        if (ReturnExpression != null) {
            ReturnExpression.Dump(blockNode);
        }
    }

    /// <summary>
    /// Generate the code to return from a procedure or GOSUB.
    /// </summary>
    /// <param name="emitter">Code emitter</param>
    /// <param name="cg">A CodeGenerator object</param>
    public override void Generate(Emitter emitter, ProgramParseNode cg) {
        if (cg == null) {
            throw new ArgumentNullException(nameof(cg));
        }
        if (cg.CurrentProcedure != null) {
            bool needStore = false;

            // Handle the case where we return via a procedure symbol (i.e. where
            // the return value is stored in a local with the name of the procedure)
            Symbol retVal = cg.CurrentProcedure.ProcedureSymbol.RetVal;
            if (retVal != null) {
                if (ReturnExpression != null) {
                    SymType thisType = cg.GenerateExpression(emitter, retVal.Type, ReturnExpression);
                    emitter.ConvertType(thisType, retVal.Type);
                } else {
                    if (retVal.Index == null) {
                        cg.Error($"Function {cg.CurrentProcedure.ProcedureSymbol.Name} does not return a value");
                    }
                    emitter.LoadLocal(retVal.Index);
                }
                needStore = true;

                // For alternate return, if the method is marked as supporting
                // them then it will be compiled as a function. So it must always
                // have a return value. A value of 0 means the default behaviour
                // (i.e. none of the labels specified are picked).
            } else if (cg.CurrentProcedure.AlternateReturnCount > 0) {
                if (ReturnExpression != null) {
                    cg.GenerateExpression(emitter, SymType.INTEGER, ReturnExpression);
                } else {
                    emitter.LoadInteger(0);
                }
                needStore = true;
            }

            // Otherwise process a straight RETURN <expression>
            else if (ReturnExpression != null) {
                SymType thisType = cg.GenerateExpression(emitter, SymType.NONE, ReturnExpression);
                retVal = cg.CurrentProcedure.ProcedureSymbol;
                emitter.ConvertType(thisType, retVal.Type);
                needStore = true;
            }

            // Store the value in the return index if one exists.
            if (needStore) {
                if (cg.CurrentProcedure.ReturnIndex == null) {
                    cg.CurrentProcedure.ReturnIndex = emitter.GetTemporary(Symbol.SymTypeToSystemType(cg.CurrentProcedure.ProcedureSymbol.Type));
                }
                emitter.StoreLocal(cg.CurrentProcedure.ReturnIndex);
            }
        }
        if (cg.HandlerLevel > 0) {
            emitter.Leave(cg.CurrentProcedure.ReturnLabel);
        }
        else {
            emitter.Branch(cg.CurrentProcedure.ReturnLabel);
        }
    }
}
