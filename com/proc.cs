// JCom Compiler Toolkit
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

using System;
using System.Collections.ObjectModel;
using System.Reflection.Emit;

namespace CCompiler {

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
        /// <param name="cg">A CodeGenerator object</param>
        public override void Generate(CodeGenerator cg) {
            if (cg == null) {
                throw new ArgumentNullException(nameof(cg));
            }
            if (cg.CurrentProcedure != null) {

                // Handle the case where we return via a procedure symbol (i.e. where
                // the return value is stored in a local with the name of the procedure)
                Symbol retVal = cg.CurrentProcedure.ProcedureSymbol.RetVal;
                if (retVal != null) {
                    if (ReturnExpression != null) {
                        SymType thisType = cg.GenerateExpression(retVal.Type, ReturnExpression);
                        cg.Emitter.ConvertType(thisType, retVal.Type);
                    } else {
                        if (retVal.Index == null) {
                            cg.Error($"Function {cg.CurrentProcedure.ProcedureSymbol.Name} does not return a value");
                        }
                        cg.Emitter.LoadLocal(retVal.Index);
                    }

                // For alternate return, if the method is marked as supporting
                // them then it will be compiled as a function. So it must always
                // have a return value. A value of 0 means the default behaviour
                // (i.e. none of the labels specified are picked).
                } else if (cg.CurrentProcedure.AlternateReturnCount > 0) {
                    if (ReturnExpression != null) {
                        cg.GenerateExpression(SymType.INTEGER, ReturnExpression);
                    } else {
                        cg.Emitter.LoadInteger(0);
                    }
                }

                // Otherwise process a straight RETURN <expression>
                else if (ReturnExpression != null) {
                    SymType thisType = cg.GenerateExpression(SymType.NONE, ReturnExpression);
                    retVal = cg.CurrentProcedure.ProcedureSymbol;
                    cg.Emitter.ConvertType(thisType, retVal.Type);
                }
            }
            cg.Emitter.Return(); 
        }
    }

    /// <summary>
    /// Specifies a parse node for a procedure.
    /// </summary>
    public sealed class ProcedureParseNode : CollectionParseNode {

        /// <summary>
        /// Gets or sets the symbol table entry for this procedure.
        /// </summary>
        /// <value>The procedure symbol.</value>
        public Symbol ProcedureSymbol { get; set; }

        /// <summary>
        /// Gets or sets the symbol table for this procedure.
        /// </summary>
        /// <value>The local symbol table</value>
        public SymbolCollection LocalSymbols { get; set; }

        /// <summary>
        /// Gets a value indicating whether this procedure is the main program.
        /// </summary>
        /// <value><c>true</c> if this instance is main program; otherwise, <c>false</c>.</value>
        public bool IsMainProgram {
            get {
                if (ProcedureSymbol != null) {
                    return ProcedureSymbol.Modifier.HasFlag(SymModifier.ENTRYPOINT);
                }
                return false;
            }
        }

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

        /// <summary>
        /// Gets or sets the optional return value expression
        /// </summary>
        /// <value>The parse node for the return value</value>
        public ParseNode ValueExpression { get; set; }

        /// <value>
        /// Gets or sets the alternate return count.
        /// </value>
        public int AlternateReturnCount { get; set; }

        /// <summary>
        /// Whether or not we handle exceptions
        /// </summary>
        public bool CatchExceptions { get; set; }

        /// <summary>
        /// Generate the code to emit a procedure.
        /// </summary>
        /// <param name="cg">A CodeGenerator object</param>
        public override void Generate(CodeGenerator cg) {
            if (cg == null) {
                throw new ArgumentNullException(nameof(cg));
            }

            // Create the emitter for this method
            cg.Emitter = new Emitter((MethodBuilder)ProcedureSymbol.Info);
            cg.CurrentProcedure = this;

            // Generate all locals for this method
            if (LocalSymbols != null) {
                cg.GenerateSymbols(LocalSymbols);
            }

            // Generate all the initialisation code
            if (InitList?.Nodes != null) {
                foreach (ParseNode initNode in InitList.Nodes) {
                    initNode.Generate(cg);
                }
            }

            bool needTryBlock = ProcedureSymbol.Modifier.HasFlag(SymModifier.ENTRYPOINT) && CatchExceptions;

            // Generate the body of the procedure
            if (needTryBlock) {
                cg.Emitter.SetupTryCatchBlock();
            }
            foreach (ParseNode statement in Nodes) {
                statement.Generate(cg);
            }
            if (needTryBlock) {
                cg.Emitter.AddDefaultTryCatchHandlerBlock();
                cg.Emitter.CloseTryCatchBlock();
            }
            cg.Emitter.Return();
            cg.Emitter.Save();
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
            LocalSymbols.Dump(blockNode);

            if (InitList?.Nodes != null) {
                ParseNodeXml initNode = blockNode.Node("InitList");
                foreach (ParseNode init in InitList.Nodes) {
                    init.Dump(initNode);
                }
            }
            ParseNodeXml statementNode = blockNode.Node("Statements");
            foreach (ParseNode statement in Nodes) {
                statement.Dump(statementNode);
            }
        }
    }
}
