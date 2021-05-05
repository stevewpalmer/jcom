// JCom Compiler Toolkit
// Binary operator parse node
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
using System.Diagnostics;
using JComLib;

namespace CCompiler {

    /// <summary>
    /// Specifies a binary operator parse node which encapsulates
    /// an expression operator with two operands.
    /// </summary>
    public sealed class BinaryOpParseNode : ParseNode {

        /// <summary>
        /// Creates a binary parse node of the specified type.
        /// </summary>
        /// <param name="id">The ID of the operator</param>
        public BinaryOpParseNode(ParseID id) : base(id) {}

        /// <summary>
        /// Returns whether this binary operator has numeric operands. Both
        /// operands must be numeric for this operator to be considered numeric.
        /// </summary>
        /// <value><c>true</c> if this instance is a number; otherwise, <c>false</c>.</value>
        public override bool IsNumber => Left.IsNumber && Right.IsNumber;

        /// <summary>
        /// Gets or sets the left hand operand.
        /// </summary>
        /// <value>The Parse node for the left hand operand</value>
        public ParseNode Left { get; set; }

        /// <summary>
        /// Gets or sets the right hand operand.
        /// </summary>
        /// <value>The Parse node for the right hand operand</value>
        public ParseNode Right { get; set; }

        /// <summary>
        /// Dumps the contents of this parse node to the ParseNode XML
        /// output under the specified parent node.
        /// </summary>
        /// <param name="root">The parent XML node</param>
        public override void Dump(ParseNodeXml root) {
            ParseNodeXml blockNode = root.Node("BinaryOp");
            blockNode.Attribute("ID", ID.ToString());
            Left.Dump(blockNode);
            Right.Dump(blockNode);
        }

        /// <summary>
        /// Emit this code to load the value to the stack.
        /// </summary>
        /// <param name="cg">A CodeGenerator object</param>
        /// <param name="returnType">The type required by the caller</param>
        /// <returns>The symbol type of the value generated</returns>
        public override SymType Generate(CodeGenerator cg, SymType returnType) {
            if (cg == null) {
                throw new ArgumentNullException(nameof(cg));
            }
            switch (ID) {
                case ParseID.ADD:       return GenerateAdd(cg);
                case ParseID.EQV:       return GenerateEq(cg);
                case ParseID.NEQV:      return GenerateNe(cg);
                case ParseID.XOR:       return GenerateXor(cg);
                case ParseID.OR:        return GenerateOr(cg);
                case ParseID.AND:       return GenerateAnd(cg);
                case ParseID.GT:        return GenerateGt(cg);
                case ParseID.GE:        return GenerateGe(cg);
                case ParseID.LE:        return GenerateLe(cg);
                case ParseID.EQ:        return GenerateEq(cg);
                case ParseID.NE:        return GenerateNe(cg);   
                case ParseID.LT:        return GenerateLt(cg);
                case ParseID.SUB:       return GenerateSub(cg);
                case ParseID.MULT:      return GenerateMult(cg);
                case ParseID.DIVIDE:    return GenerateDivide(cg);
                case ParseID.IDIVIDE:   return GenerateIDivide(cg);
                case ParseID.MOD:       return GenerateMod(cg);
                case ParseID.CONCAT:    return GenerateConcat(cg);
                case ParseID.MERGE:     return GenerateMerge(cg);
                case ParseID.EXP:       return GenerateExp(cg);
            }
            Debug.Assert(false, "Unsupported parse ID for BinaryOpParseNode");
            return Value.Type;
        }

        // Generate the code for a binary addition operator
        private SymType GenerateAdd(CodeGenerator cg) {
            cg.GenerateExpression(Type, Left);
            cg.GenerateExpression(Type, Right);
            cg.Emitter.Add(Type);
            return Type;
        }

        // Generate the code for a binary subtraction operator
        private SymType GenerateSub(CodeGenerator cg) {
            cg.GenerateExpression(Type, Left);
            cg.GenerateExpression(Type, Right);
            cg.Emitter.Sub(Type);
            return Type;
        }

        // Generate the code for a binary multiplication operator
        private SymType GenerateMult(CodeGenerator cg) {
            cg.GenerateExpression(Type, Left);
            cg.GenerateExpression(Type, Right);
            cg.Emitter.Mul(Type);
            return Type;
        }

        // Generate the code for a binary division operator
        private SymType GenerateDivide(CodeGenerator cg) {
            cg.GenerateExpression(Type, Left);
            cg.GenerateExpression(Type, Right);
            cg.Emitter.Div(Type);
            return Type;
        }

        // Generate the code for a binary division operator
        private SymType GenerateIDivide(CodeGenerator cg) {
            cg.GenerateExpression(SymType.INTEGER, Left);
            cg.GenerateExpression(SymType.INTEGER, Right);
            cg.Emitter.IDiv(SymType.INTEGER);
            return SymType.INTEGER;
        }

        // Generate the code for a binary MOD operator
        private SymType GenerateMod(CodeGenerator cg) {
            cg.GenerateExpression(Type, Left);
            cg.GenerateExpression(Type, Right);
            cg.Emitter.Mod(Type);
            return Type;
        }

        // Generate the code for a binary exponentiation operator
        private SymType GenerateExp(CodeGenerator cg) {
            cg.GenerateExpression(SymType.DOUBLE, Left);
            cg.GenerateExpression(SymType.DOUBLE, Right);
            cg.Emitter.Call(cg.GetMethodForType(typeof(Math), "Pow", new [] {typeof(double), typeof(double)}));
            return SymType.DOUBLE;
        }

        // Generate the code for a string merge operator
        private SymType GenerateMerge(CodeGenerator cg) {
            Type charType = typeof(FixedString);

            cg.GenerateExpression(SymType.FIXEDCHAR, Left);
            cg.GenerateExpression(SymType.FIXEDCHAR, Right);

            cg.Emitter.Call(cg.GetMethodForType(charType, "Merge", new[] { charType, charType }));
            return SymType.FIXEDCHAR;
        }

        // Generate the code for a string concatenation operator
        private SymType GenerateConcat(CodeGenerator cg) {
            Type charType = Symbol.SymTypeToSystemType(Left.Type);
            
            cg.GenerateExpression(Left.Type, Left);
            cg.GenerateExpression(Left.Type, Right);
            
            cg.Emitter.Call(cg.GetMethodForType(charType, "Concat", new [] { charType, charType }));
            return Left.Type;
        }

        // Generate the code for a logical AND operator
        private SymType GenerateAnd(CodeGenerator cg) {
            cg.GenerateExpression(Type, Left);
            cg.GenerateExpression(Type, Right);
            cg.Emitter.And();
            return Type;
        }

        // Generate the code for a logical OR operator
        private SymType GenerateOr(CodeGenerator cg) {
            cg.GenerateExpression(Type, Left);
            cg.GenerateExpression(Type, Right);
            cg.Emitter.Or();
            return Type;
        }

        // Generate the code for an exclusive OR operator
        private SymType GenerateXor(CodeGenerator cg) {
            cg.GenerateExpression(Type, Left);
            cg.GenerateExpression(Type, Right);
            cg.Emitter.Xor();
            return Type;
        }

        // Generate the code for a logical Less Than operator
        private SymType GenerateLt(CodeGenerator cg) {
            SymType neededType = TypePromotion(Left, Right);
            cg.GenerateExpression(neededType, Left);
            cg.GenerateExpression(neededType, Right);
            if (Symbol.IsCharType(neededType)) {
                Type charType = Symbol.SymTypeToSystemType(neededType);
                cg.Emitter.Call(cg.GetMethodForType(charType, "Compare", new [] {charType, charType}));
                cg.Emitter.LoadInteger(0);
            }
            cg.Emitter.CompareLesser();
            return Type;
        }

        // Generate the code for a logical Less Than or Equals operator
        private SymType GenerateLe(CodeGenerator cg) {
            SymType neededType = TypePromotion(Left, Right);
            cg.GenerateExpression(neededType, Left);
            cg.GenerateExpression(neededType, Right);
            if (Symbol.IsCharType(neededType)) {
                Type charType = Symbol.SymTypeToSystemType(neededType);
                cg.Emitter.Call(cg.GetMethodForType(charType, "Compare", new [] {charType, charType}));
                cg.Emitter.LoadInteger(0);
            }
            cg.Emitter.CompareGreater();
            cg.Emitter.LoadInteger(1);
            cg.Emitter.Xor();
            return Type;
        }

        // Generate the code for a logical Greater Than operator
        private SymType GenerateGt(CodeGenerator cg) {
            SymType neededType = TypePromotion(Left, Right);
            cg.GenerateExpression(neededType, Left);
            cg.GenerateExpression(neededType, Right);
            if (Symbol.IsCharType(neededType)) {
                Type charType = Symbol.SymTypeToSystemType(neededType);
                cg.Emitter.Call(cg.GetMethodForType(charType, "Compare", new [] {charType, charType}));
                cg.Emitter.LoadInteger(0);
            }
            cg.Emitter.CompareGreater();
            return Type;
        }

        // Generate the code for a logical Greater Than or Equals operator
        private SymType GenerateGe(CodeGenerator cg) {
            SymType neededType = TypePromotion(Left, Right);
            cg.GenerateExpression(neededType, Left);
            cg.GenerateExpression(neededType, Right);
            if (Symbol.IsCharType(neededType)) {
                Type charType = Symbol.SymTypeToSystemType(neededType);
                cg.Emitter.Call(cg.GetMethodForType(charType, "Compare", new [] {charType, charType}));
                cg.Emitter.LoadInteger(0);
            }
            cg.Emitter.CompareLesser();
            cg.Emitter.LoadInteger(1);
            cg.Emitter.Xor();
            return Type;
        }

        // Generate the code for a logical Equals operator
        private SymType GenerateEq(CodeGenerator cg) {
            SymType neededType = TypePromotion(Left, Right);
            cg.GenerateExpression(neededType, Left);
            cg.GenerateExpression(neededType, Right);
            if (Symbol.IsCharType(neededType)) {
                Type charType = Symbol.SymTypeToSystemType(neededType);
                cg.Emitter.Call(cg.GetMethodForType(charType, "op_Equality", new [] {charType, charType}));
            } else {
                cg.Emitter.CompareEquals();
            }
            return Type;
        }

        // Generate the code for a logical Not Equals operator
        private SymType GenerateNe(CodeGenerator cg) {
            SymType neededType = TypePromotion(Left, Right);
            cg.GenerateExpression(neededType, Left);
            cg.GenerateExpression(neededType, Right);
            if (Symbol.IsCharType(neededType)) {
                Type charType = Symbol.SymTypeToSystemType(neededType);
                cg.Emitter.Call(cg.GetMethodForType(charType, "op_Equality", new [] {charType, charType}));
            } else {
                cg.Emitter.CompareEquals();
            }
            cg.Emitter.LoadInteger(1);
            cg.Emitter.Xor();
            return Type;
        }

        // Given two parse nodes with types associated, this function returns the
        // largest type required for a consistent arithmetic operation between them.
        private SymType TypePromotion(ParseNode p1, ParseNode p2) {
            return Symbol.LargestType(p1.Type, p2.Type);
        }
    }
}
