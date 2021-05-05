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

using System;
using System.Collections.Generic;
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
        /// <param name="cg">A CodeGenerator object</param>
        /// <param name="node">A parse node for the READ identifier</param>
        public override void Generate(CodeGenerator cg, ParseNode node) {
            if (cg == null) {
                throw new ArgumentNullException(nameof(cg));
            }
            if (node is LoopParseNode) {
                LoopParseNode loopNode = (LoopParseNode)node;
                loopNode.Callback = this;
                loopNode.Generate(cg);
            } else {
                Type readManagerType = typeof(ReadManager);
                List<Type> readParamTypes = new();
                
                cg.Emitter.LoadLocal(ReadManagerIndex);
                readParamTypes.Add(readManagerType);
                
                readParamTypes.AddRange(ReadParamsNode.Generate(cg));

                if (node is IdentifierParseNode) {
                    IdentifierParseNode identNode = (IdentifierParseNode)node;
                    if (identNode.IsArrayBase) {
                        cg.Emitter.LoadInteger(identNode.Symbol.ArraySize);
                        identNode.Generate(cg);
                        
                        readParamTypes.Add(typeof(int));
                        readParamTypes.Add(Symbol.SymTypeToSystemType(identNode.Symbol.Type).MakeArrayType());
                    } else if (identNode.HasSubstring) {
                        cg.GenerateExpression(SymType.INTEGER, identNode.SubstringStart);
                        if (identNode.SubstringEnd != null) {
                            cg.GenerateExpression(SymType.INTEGER, identNode.SubstringEnd);
                        } else {
                            cg.Emitter.LoadInteger(-1);
                        }
                        cg.LoadAddress(identNode);
                        readParamTypes.Add(typeof(int));
                        readParamTypes.Add(typeof(int));
                        readParamTypes.Add(Symbol.SymTypeToSystemType(identNode.Symbol.Type).MakeByRefType());
                    } else {
                        cg.LoadAddress(identNode);
                        readParamTypes.Add(Symbol.SymTypeToSystemType(identNode.Symbol.Type).MakeByRefType());
                    }
                }

                cg.Emitter.Call(cg.GetMethodForType(LibraryName, Name, readParamTypes.ToArray()));
                cg.Emitter.StoreLocal(ReturnIndex);
                
                if (EndLabel != null) {
                    cg.Emitter.LoadLocal(ReturnIndex);
                    cg.Emitter.LoadInteger(0);
                    cg.Emitter.BranchEqual((Label)EndLabel.Symbol.Info);
                }
                if (ErrLabel != null) {
                    cg.Emitter.LoadLocal(ReturnIndex);
                    cg.Emitter.LoadInteger(-1);
                    cg.Emitter.BranchEqual((Label)ErrLabel.Symbol.Info);
                }
            }
        }
    }

    /// <summary>
    /// Specifies a parse node that defines a READ statement.
    /// </summary>
    public sealed class ReadParseNode : ParseNode {

        /// <summary>
        /// Gets or sets the list of parameters to initialise the ReadManager
        /// object.
        /// </summary>
        /// <value>The read manager parameters node</value>
        public ParametersParseNode ReadManagerParamsNode { get; set; }

        /// <summary>
        /// Gets or sets the list of parameters for the READ function.
        /// </summary>
        /// <value>The read parameters node</value>
        public ParametersParseNode ReadParamsNode { get; set; }

        /// <summary>
        /// Gets or sets the list of arguments for the READ.
        /// </summary>
        /// <value>The argument list</value>
        public CollectionParseNode ArgList { get; set; }

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
            ParseNodeXml blockNode = root.Node("Read");
            ReadManagerParamsNode.Dump(blockNode.Node("ReadManagerParams"));
            ReadParamsNode.Dump(blockNode.Node("ReadParams"));
            ArgList.Dump(blockNode);
        }

        /// <summary>
        /// Emit the code to generate a call to the READ library function.
        /// </summary>
        /// <param name="cg">A CodeGenerator object</param>
        public override void Generate(CodeGenerator cg) {
            if (cg == null) {
                throw new ArgumentNullException(nameof(cg));
            }

            Type readManagerType = typeof(ReadManager);
            Type [] paramTypes = ReadManagerParamsNode.Generate(cg);

            cg.Emitter.CreateObject(readManagerType, paramTypes);
            LocalDescriptor objIndex = cg.Emitter.GetTemporary(readManagerType);
            cg.Emitter.StoreLocal(objIndex);
            
            if (EndLabel != null) {
                cg.Emitter.LoadLocal(objIndex);
                cg.Emitter.LoadInteger(1);
                cg.Emitter.Call(readManagerType.GetMethod("set_HasEnd", new [] { typeof(bool) }));
            }
            
            if (ErrLabel != null) {
                cg.Emitter.LoadLocal(objIndex);
                cg.Emitter.LoadInteger(1);
                cg.Emitter.Call(readManagerType.GetMethod("set_HasErr", new [] { typeof(bool) }));
            }
            
            LocalDescriptor index = cg.Emitter.GetTemporary(typeof(int));

            // Construct a parsenode that will be called for each item in the loop
            // including any implied DO loops.
            ReadItemParseNode itemNode = new();
            itemNode.ReturnIndex = index;
            itemNode.ErrLabel = ErrLabel;
            itemNode.EndLabel = EndLabel;
            itemNode.ReadParamsNode = ReadParamsNode;
            itemNode.ReadManagerIndex = objIndex;

            if (ArgList != null && ArgList.Nodes.Count > 0) {
                int countOfArgs = ArgList.Nodes.Count;
                
                for (int c = 0; c < countOfArgs; ++c) {
                    itemNode.Generate(cg, ArgList.Nodes[c]);
                    ReadParamsNode.FreeLocalDescriptors();
                }
            } else {
                itemNode.Generate(cg, null);
            }

            // Issue an EndRecord to complete this record read
            cg.Emitter.LoadLocal(objIndex);
            cg.Emitter.Call(cg.GetMethodForType(readManagerType, "EndRecord", System.Type.EmptyTypes));
            
            cg.Emitter.ReleaseTemporary(index);
            cg.Emitter.ReleaseTemporary(objIndex);
        }
    }
}
