// JCom Compiler Toolkit
// Conditional parse node
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
using System.Reflection.Emit;
using JComLib;

namespace CCompiler {

    /// <summary>
    /// Specifies a parse node that defines an arithmetic based conditional
    /// block such as a FORTRAN arithmetic IF.
    /// </summary>
    public sealed class ArithmeticConditionalParseNode : CollectionParseNode {

        /// <summary>
        /// Gets or sets the expression that is evaluated to determine
        /// the condition.
        /// </summary>
        /// <value>The value expression.</value>
        public ParseNode ValueExpression { get; set; }

        /// <summary>
        /// Emit the code to generate an arithmetic conditional block.
        /// </summary>
        /// <param name="cg">A code generator object</param>
        public override void Generate(ProgramParseNode cg) {
            if (cg == null) {
                throw new ArgumentNullException(nameof(cg));
            }
            SymType exprType = cg.GenerateExpression(SymType.NONE, ValueExpression);
            Symbol label1 = cg.GetLabel(Nodes[0]);
            Symbol label2 = cg.GetLabel(Nodes[1]);
            Symbol label3 = cg.GetLabel(Nodes[2]);
            LocalDescriptor tempIndex = cg.Emitter.GetTemporary(Symbol.SymTypeToSystemType(exprType));
            cg.Emitter.StoreLocal(tempIndex);
            cg.Emitter.LoadLocal(tempIndex);
            cg.Emitter.LoadValue(exprType, new Variant(0));
            cg.Emitter.BranchLess((Label)label1.Info);
            cg.Emitter.LoadLocal(tempIndex);
            cg.Emitter.LoadValue(exprType, new Variant(0));
            cg.Emitter.BranchEqual((Label)label2.Info);
            cg.Emitter.Branch((Label)label3.Info);
            Emitter.ReleaseTemporary(tempIndex);
        }
    }
}
