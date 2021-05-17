// JCom Compiler Toolkit
// Write parse node
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
    /// Specifies a parse node that defines a single write item.
    /// </summary>
    public sealed class WriteItemParseNode : ParseNode {

        private const string _libraryName = "JFortranLib.IO,jforlib";
        private const string _name = "WRITE";

        /// <summary>
        /// Gets or sets the local index to store the return value.
        /// </summary>
        /// <value>The local index of the return.</value>
        public LocalDescriptor ReturnIndex { get; set; }

        /// <summary>
        /// Gets or sets the local index of the WriteManager instance
        /// to use for this item.
        /// </summary>
        /// <value>The local index of the WriteManager instance.</value>
        public LocalDescriptor WriteManagerIndex { get; set; }

        /// <summary>
        /// Gets or sets the list of parameters for the write function.
        /// </summary>
        /// <value>The write parameters node</value>
        public ParametersParseNode WriteParamsNode { get; set; }

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
            ParseNodeXml blockNode = root.Node("WriteItem");
            WriteParamsNode.Dump(blockNode);
        }

        /// <summary>
        /// Emit the code to generate a call to the write library function. A
        /// parse node must be specified which evaluates to the value to be
        /// written.
        /// </summary>
        /// <param name="cg">A CodeGenerator object</param>
        /// <param name="node">A parse node for the WRITE identifier</param>
        public override void Generate(CodeGenerator cg, ParseNode node) {
            if (cg == null) {
                throw new ArgumentNullException(nameof(cg));
            }
            if (node is LoopParseNode loopNode) {
                loopNode.Callback = this;
                loopNode.Generate(cg);
            } else {
                Type writeManagerType = typeof(WriteManager);
                List<Type> writeParamTypes = new();
                
                cg.Emitter.LoadLocal(WriteManagerIndex);
                writeParamTypes.Add(writeManagerType);

                if (WriteParamsNode != null) {
                    writeParamTypes.AddRange(WriteParamsNode.Generate(cg));
                }

                if (node != null) {
                    ParameterParseNode exprParam = new(node);
                    writeParamTypes.Add(exprParam.Generate(cg));
                }

                cg.Emitter.Call(cg.GetMethodForType(_libraryName, _name, writeParamTypes.ToArray()));
                cg.Emitter.StoreLocal(ReturnIndex);
                
                if (ErrLabel != null) {
                    cg.Emitter.LoadLocal(ReturnIndex);
                    cg.Emitter.LoadInteger(-1);
                    cg.Emitter.BranchEqual((Label)ErrLabel.Symbol.Info);
                }
            }
        }
    }

    /// <summary>
    /// Specifies a parse node that defines a write statement.
    /// </summary>
    public sealed class WriteParseNode : ParseNode {

        /// <summary>
        /// Gets or sets the list of parameters to initialise the WriteManager
        /// object.
        /// </summary>
        /// <value>The write manager parameters node</value>
        public ParametersParseNode WriteManagerParamsNode { get; set; }

        /// <summary>
        /// Gets or sets the list of parameters for the write function.
        /// </summary>
        /// <value>The write parameters node</value>
        public ParametersParseNode WriteParamsNode { get; set; }

        /// <summary>
        /// Gets or sets the list of arguments for the write.
        /// </summary>
        /// <value>The argument list</value>
        public CollectionParseNode ArgList { get; set; }

        /// <summary>
        /// Gets or sets the symbol representing the optional error label.
        /// This may be NULL if no ERR was specified in the control list.
        /// </summary>
        /// <value>The error label symbol parse node</value>
        public SymbolParseNode ErrLabel { get; set; }

        /// <summary>
        /// Gets or sets a flag which indicates whether the first column of the
        /// output string has special meaning to the console.
        /// </summary>
        /// <value><c>true</c> if first column special; otherwise, <c>false</c>.</value>
        public bool FirstColumnSpecial { get; set; }

        /// <summary>
        /// Emit the code to generate a call to the WRITE library function.
        /// 
        /// This interface implements the statement version. Refer to the
        /// comments for the function version below for more details.
        /// </summary>
        /// <param name="cg">A code generator object</param>
        public override void Generate(CodeGenerator cg) {
            Generate(cg, Type);
        }

        /// <summary>
        /// Emit the code to generate a call to the WRITE library function.
        /// 
        /// The WRITE function may be invoked as either a subroutine or a function depending
        /// on whether it is being used for formatting a string or writing to a device. The
        /// returnType parameter should direct how it should be used. If returnType is
        /// SymType.NONE then a subroutine call is being made and the return value will
        /// also be SymType.NONE and the stack will be cleared of any value returned from
        /// the function. If the returnType is not SymType.NONE then the return value is the
        /// type of the value on the top of the stack.
        /// </summary>
        /// <param name="cg">A CodeGenerator object</param>
        /// <param name="returnType">The expected type of the returned value</param>
        /// <returns>The type of the value on the stack or SymType.NONE</returns>
        public override SymType Generate(CodeGenerator cg, SymType returnType) {
            if (cg == null) {
                throw new ArgumentNullException(nameof(cg));
            }

            Type writeManagerType = typeof(WriteManager);
            Type [] paramTypes = WriteManagerParamsNode.Generate(cg);
            
            cg.Emitter.CreateObject(writeManagerType, paramTypes);
            LocalDescriptor objIndex = cg.Emitter.GetTemporary(writeManagerType);
            cg.Emitter.StoreLocal(objIndex);
            
            if (ErrLabel != null) {
                cg.Emitter.LoadLocal(objIndex);
                cg.Emitter.LoadInteger(1);
                cg.Emitter.Call(writeManagerType.GetMethod("set_HasErr", new [] { typeof(bool) }));
            }

            if (FirstColumnSpecial) {
                cg.Emitter.LoadLocal(objIndex);
                cg.Emitter.LoadInteger(1);
                cg.Emitter.Call(writeManagerType.GetMethod("SetFirstColumnSpecial", new [] { typeof(bool) }));
            }

            // Disable use of separators for BASIC output
            cg.Emitter.LoadLocal(objIndex);
            cg.Emitter.LoadInteger(0);
            cg.Emitter.Call(writeManagerType.GetMethod("set_UseSeparators", new[] { typeof(bool) }));

            LocalDescriptor index = cg.Emitter.GetTemporary(typeof(int));

            // Construct a parsenode that will be called for each item in the loop
            // including any implied DO loops.
            WriteItemParseNode itemNode = new();
            itemNode.ReturnIndex = index;
            itemNode.ErrLabel = ErrLabel;
            itemNode.WriteParamsNode = WriteParamsNode;
            itemNode.WriteManagerIndex = objIndex;

            if (ArgList != null && ArgList.Nodes.Count > 0) {
                int countOfArgs = ArgList.Nodes.Count;
                for (int c = 0; c < countOfArgs; ++c) {
                    itemNode.Generate(cg, ArgList.Nodes[c]);
                    WriteParamsNode.FreeLocalDescriptors();
                }
            } else {
                itemNode.Generate(cg, null);
            }

            // Issue an EndRecord to complete this record write
            cg.Emitter.LoadLocal(objIndex);
            cg.Emitter.Call(cg.GetMethodForType(writeManagerType, "EndRecord", System.Type.EmptyTypes));

            // Discard the return value from the stack if we don't need it.
            if (returnType == SymType.NONE) {
                cg.Emitter.Pop();
            }

            Emitter.ReleaseTemporary(index);
            Emitter.ReleaseTemporary(objIndex);
            return SymType.CHAR;
        }

        /// <summary>
        /// Dumps the contents of this parse node to the ParseNode XML
        /// output under the specified parent node.
        /// </summary>
        /// <param name="root">The parent XML node</param>
        public override void Dump(ParseNodeXml root) {
            ParseNodeXml blockNode = root.Node("Write");
            blockNode.Attribute("FirstColumnSpecial", FirstColumnSpecial.ToString());
            WriteManagerParamsNode.Dump(blockNode.Node("WriteManagerParams"));
            WriteParamsNode.Dump(blockNode.Node("WriteParams"));
            if (ArgList != null) {
                ArgList.Dump(blockNode);
            }
        }
    }
}
