// JCom Compiler Toolkit
// External interface call parse node
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
using System.Reflection;

namespace CCompiler {

    /// <summary>
    /// Specifies a parse node that calls an external interface.
    /// </summary>
    public sealed class ExtCallParseNode : ParseNode {

        /// <summary>
        /// Creates an external interface call parse node.
        /// </summary>
        public ExtCallParseNode() {}

        /// <summary>
        /// Creates an external interface call parse node with a pre-specified
        /// library and method name
        /// </summary>
        /// <param name="libraryName">Library name</param>
        /// <param name="name">External method name</param>
        public ExtCallParseNode(string libraryName, string name) {
            LibraryName = libraryName;
            Name = name;
        }

        /// <summary>
        /// Gets or sets the name of the library that contains the
        /// external function.
        /// </summary>
        /// <value>The library name</value>
        public string LibraryName { get; set; }

        /// <summary>
        /// Gets or sets the external method name.
        /// </summary>
        /// <value>The external method name</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the parameters for the call.
        /// </summary>
        /// <value>The Parameters parse node</value>
        public ParametersParseNode Parameters { get; set; }

        /// <summary>
        /// Gets or sets a flag which indicates whether methods should be
        /// inlined if an inline generator is found for that method.
        /// </summary>
        /// <value><c>true</c> if inline; otherwise, <c>false</c>.</value>
        public bool Inline { get; set; }

        /// <summary>
        /// Return whether this function can be inlined.
        /// </summary>
        /// <returns><c>true</c> if this instance can be inlined; otherwise, <c>false</c>.</returns>
        public bool CanInline() {
            if (typeof(Inlined).GetMethod(Name, new [] { typeof(Emitter), typeof(Type) }) != null) {
                return true;
            }
            return typeof(Inlined).GetMethod(Name, new [] { typeof(Emitter) }) != null;
        }

        /// <summary>
        /// Dumps the contents of this parse node to the ParseNode XML
        /// output under the specified parent node.
        /// </summary>
        /// <param name="root">The parent XML node</param>
        public override void Dump(ParseNodeXml root) {
            ParseNodeXml blockNode = root.Node("ExtCall");
            blockNode.Attribute("LibraryName", LibraryName);
            blockNode.Attribute("Name", Name);
            blockNode.Attribute("Inline", Inline.ToString());
            if (Parameters != null) {
                Parameters.Dump(blockNode);
            }
        }

        /// <summary>
        /// Emit the code to call an external function complete with
        /// parameters that has no return value.
        /// </summary>
        /// <param name="emitter">The emitter</param>
        /// <param name="cg">A code generator object</param>
        public override void Generate(Emitter emitter, ProgramParseNode cg) {
            Generate(emitter, cg, SymType.NONE);
        }

        /// <summary>
        /// Emit the code to call an external function complete with
        /// parameters that returns the specified type.
        /// </summary>
        /// <param name="emitter">The emitter</param>
        /// <param name="cg">A code generator object</param>
        /// <param name="returnType">The expected return type</param>
        /// <returns>The actual return type from the function</returns>
        public override SymType Generate(Emitter emitter, ProgramParseNode cg, SymType returnType) {

            Type argType = typeof(void);
            Type [] paramTypes;

            // It is the caller responsibility to set the parameter
            // node types to match the external function types.
            if (Parameters != null) {
                paramTypes = Parameters.Generate(emitter, cg);
                if (paramTypes.Length > 0) {
                    argType = paramTypes[0];
                }
            } else {
                paramTypes = System.Type.EmptyTypes;
            }
            
            // For anything else, we emit the appropriate call to the library. If
            // inline is permitted, we check the library for an inline version of the
            // name and if one exists, invoke it to insert the inline code.
            MethodInfo meth;
            if (Inline) {
                
                // First try specific methods where different inline methods are
                // provided depending on the type.
                meth = typeof(Inlined).GetMethod(Name, new [] { typeof(Emitter), typeof(Type) });
                if (meth != null) {
                    object [] ilParams = { emitter, argType };
                    meth.Invoke(null, ilParams);
                    return Type;
                }
                
                // Otherwise try the type-less variant.
                meth = typeof(Inlined).GetMethod(Name, new [] { typeof(Emitter) });
                if (meth != null) {
                    object [] ilParams = { emitter };
                    meth.Invoke(null, ilParams);
                    return Type;
                }
            }
            
            meth = cg.GetMethodForType(LibraryName, Name, paramTypes);
            emitter.Call(meth);
            
            // If this method returns a value but we're invoking it as a
            // subroutine, discard the return value from the stack
            if (returnType == SymType.NONE && meth.ReturnType != typeof(void)) {
                emitter.Pop();
            }
            returnType = Symbol.SystemTypeToSymbolType(meth.ReturnType);
            if (Parameters != null) {
                Parameters.FreeLocalDescriptors();
            }
            return returnType;
        }
    }
}
