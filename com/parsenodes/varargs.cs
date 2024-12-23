// JCom Compiler Toolkit
// Variable argument list parse node
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
/// Specifies a variable argument parse node which encapsulates an
/// variable size array of arguments for use by external functions.
/// </summary>
public sealed class VarArgParseNode : CollectionParseNode {

    /// <summary>
    /// Whether the arguments are being passed by value or by reference.
    /// If PassByRef is set, all arguments must be identifiers.
    /// </summary>
    public bool PassByRef { get; set; }

    /// <summary>
    /// Emit the code to create an array of variable arguments and evaluate
    /// and store each argument in the array. On exit, the address of the
    /// array is left on the top of the stack.
    /// </summary>
    /// <param name="emitter">Code emitter</param>
    /// <param name="cg">A CodeGenerator object</param>
    /// <param name="returnType">The type required by the caller</param>
    /// <returns>The symbol type of the value generated</returns>
    public override SymType Generate(Emitter emitter, ProgramParseNode cg, SymType returnType) {
        ArgumentNullException.ThrowIfNull(cg);
        int argCount = Nodes.Count;
        emitter.CreateSimpleArray(argCount, typeof(object));
        for (int c = 0; c < argCount; ++c) {
            emitter.Dup();
            emitter.LoadInteger(c);
            if (PassByRef) {
                IdentifierParseNode identNode = (IdentifierParseNode)Nodes[c];
                cg.LoadAddress(emitter, identNode);
                emitter.StoreElementReference(identNode.Type);
            }
            else {
                emitter.StoreElementReference(cg.GenerateExpression(emitter, Nodes[c].Type, Nodes[c]));
            }
        }
        return SymType.VARARG;
    }
}