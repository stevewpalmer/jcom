// JCom Compiler Toolkit
// Read parse node
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

using System.Reflection.Emit;
using JFortranLib;

namespace CCompiler {

    /// <summary>
    /// Specifies a parse node that defines a single READ item.
    /// </summary>
    public sealed class ReadItemParseNode : ParseNode {

        private const string LibraryName = "JFortranLib.IO,jforlib";
        private const string Name = "READ";

        /// <summary>
        /// Gets or sets the local index to store the return value.
        /// </summary>
        /// <value>The local index of the return.</value>
        public LocalDescriptor ReturnIndex { get; set; }

        /// <summary>
        /// Gets or sets the local index of the ReadManager instance
        /// to use for this item.
        /// </summary>
        /// <value>The local index of the ReadManager instance.</value>
        public LocalDescriptor ReadManagerIndex { get; set; }

        /// <summary>
        /// Gets or sets the list of parameters for the READ function.
        /// </summary>
        /// <value>The read parameters node</value>
        public ParametersParseNode ReadParamsNode { get; set; }

        /// <summary>
        /// Gets or sets the symbol representing the optional end label.
        /// This may be NULL if no END was specified in the control list.
        /// </summary>
        /// <value>The end label symbol parse node</value>
        public SymbolParseNode EndLabel { get; set; }

        /// <summary>
        /// Gets or sets the symbol representing the optional error label.
        /// This may be NULL if no ERR was specified in the control list.
        /// </summary>
        /// <value>The error label symbol parse node</value>
        public SymbolParseNode ErrLabel { get; set; }

        /// <summary>
        /// Dumps the contents of this parse node to the ParseNode XML
        /// output under the specified parent node.
        /// </summary>
        /// <param name="root">The parent XML node</param>
        public override void Dump(ParseNodeXml root) {
            ParseNodeXml blockNode = root.Node("ReadItem");
            ReadParamsNode.Dump(blockNode);
        }

        /// <summary>
        /// Emit the code to generate a call to the READ library function. A
        /// parse node must be provided which evaluates to the address of the
        /// identifier into which the data is read.
        /// </summary>
        /// <param name="emitter">Code emitter</param>
        /// <param name="cg">A CodeGenerator object</param>
        /// <param name="node">A parse node for the READ identifier</param>
        public override void Generate(Emitter emitter, ProgramParseNode cg, ParseNode node) {
            if (cg == null) {
                throw new ArgumentNullException(nameof(cg));
            }
            if (node is LoopParseNode loopNode) {
                loopNode.Callback = this;
                loopNode.Generate(emitter, cg);
            } else {
                Type readManagerType = typeof(ReadManager);
                List<Type> readParamTypes = new();

                emitter.LoadLocal(ReadManagerIndex);
                readParamTypes.Add(readManagerType);

                readParamTypes.AddRange(ReadParamsNode.Generate(emitter, cg));

                if (node is IdentifierParseNode identNode) {
                    if (identNode.IsArrayBase) {
                        emitter.LoadInteger(identNode.Symbol.ArraySize);
                        identNode.Generate(emitter, cg);

                        readParamTypes.Add(typeof(int));
                        readParamTypes.Add(Symbol.SymTypeToSystemType(identNode.Symbol.Type).MakeArrayType());
                    } else if (identNode.HasSubstring) {
                        cg.GenerateExpression(emitter, SymType.INTEGER, identNode.SubstringStart);
                        if (identNode.SubstringEnd != null) {
                            cg.GenerateExpression(emitter, SymType.INTEGER, identNode.SubstringEnd);
                        } else {
                            emitter.LoadInteger(-1);
                        }
                        cg.LoadAddress(emitter, identNode);
                        readParamTypes.Add(typeof(int));
                        readParamTypes.Add(typeof(int));
                        readParamTypes.Add(Symbol.SymTypeToSystemType(identNode.Symbol.Type).MakeByRefType());
                    } else {
                        cg.LoadAddress(emitter, identNode);
                        readParamTypes.Add(Symbol.SymTypeToSystemType(identNode.Symbol.Type).MakeByRefType());
                    }
                }

                emitter.Call(cg.GetMethodForType(LibraryName, Name, readParamTypes.ToArray()));
                emitter.StoreLocal(ReturnIndex);

                if (EndLabel != null) {
                    emitter.LoadLocal(ReturnIndex);
                    emitter.LoadInteger(0);
                    emitter.BranchEqual((Label)EndLabel.Symbol.Info);
                }
                if (ErrLabel != null) {
                    emitter.LoadLocal(ReturnIndex);
                    emitter.LoadInteger(-1);
                    emitter.BranchEqual((Label)ErrLabel.Symbol.Info);
                }
            }
        }
    }
}
