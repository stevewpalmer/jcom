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

using System.Diagnostics;
using System.Reflection.Emit;
using JComLib;

namespace CCompiler;

/// <summary>
/// Specifies a binary operator parse node which encapsulates
/// an expression operator with two operands.
/// </summary>
public sealed class BinaryOpParseNode : ParseNode {

    /// <summary>
    /// Creates a binary parse node of the specified type.
    /// </summary>
    /// <param name="id">The ID of the operator</param>
    public BinaryOpParseNode(ParseID id) : base(id) { }

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
    /// <param name="emitter">Code emitter</param>
    /// <param name="cg">A CodeGenerator object</param>
    /// <param name="returnType">The type required by the caller</param>
    /// <returns>The symbol type of the value generated</returns>
    public override SymType Generate(Emitter emitter, ProgramParseNode cg, SymType returnType) {
        ArgumentNullException.ThrowIfNull(cg);
        switch (ID) {
            case ParseID.ADD: return GenerateAdd(emitter, cg);
            case ParseID.AND: return GenerateBitwiseAnd(emitter, cg);
            case ParseID.ANDTHEN: return GenerateLogicalAnd(emitter, cg);
            case ParseID.CONCAT: return GenerateConcat(emitter, cg);
            case ParseID.DIVIDE: return GenerateDivide(emitter, cg);
            case ParseID.EQ:
            case ParseID.EQV:
                return GenerateEq(emitter, cg);
            case ParseID.EXP: return GenerateExp(emitter, cg);
            case ParseID.GE: return GenerateGe(emitter, cg);
            case ParseID.GT: return GenerateGt(emitter, cg);
            case ParseID.IDIVIDE: return GenerateIDivide(emitter, cg);
            case ParseID.LE: return GenerateLe(emitter, cg);
            case ParseID.LT: return GenerateLt(emitter, cg);
            case ParseID.MERGE: return GenerateMerge(emitter, cg);
            case ParseID.MOD: return GenerateMod(emitter, cg);
            case ParseID.MULT: return GenerateMult(emitter, cg);
            case ParseID.NE:
            case ParseID.NEQV:
                return GenerateNe(emitter, cg);
            case ParseID.OR: return GenerateBitwiseOr(emitter, cg);
            case ParseID.ORTHEN: return GenerateLogicalOr(emitter, cg);
            case ParseID.SUB: return GenerateSub(emitter, cg);
            case ParseID.XOR: return GenerateXor(emitter, cg);
        }
        Debug.Assert(false, "Unsupported parse ID for BinaryOpParseNode");
        return Symbol.VariantTypeToSymbolType(Value.Type);
    }

    // Generate the code for a binary addition operator
    private SymType GenerateAdd(Emitter emitter, ProgramParseNode cg) {
        cg.GenerateExpression(emitter, Type, Left);
        cg.GenerateExpression(emitter, Type, Right);
        emitter.Add(Type);
        return Type;
    }

    // Generate the code for a binary subtraction operator
    private SymType GenerateSub(Emitter emitter, ProgramParseNode cg) {
        cg.GenerateExpression(emitter, Type, Left);
        cg.GenerateExpression(emitter, Type, Right);
        emitter.Sub(Type);
        return Type;
    }

    // Generate the code for a binary multiplication operator
    private SymType GenerateMult(Emitter emitter, ProgramParseNode cg) {
        cg.GenerateExpression(emitter, Type, Left);
        cg.GenerateExpression(emitter, Type, Right);
        emitter.Mul(Type);
        return Type;
    }

    // Generate the code for a binary division operator
    private SymType GenerateDivide(Emitter emitter, ProgramParseNode cg) {
        cg.GenerateExpression(emitter, Type, Left);
        cg.GenerateExpression(emitter, Type, Right);
        emitter.Div(Type);
        return Type;
    }

    // Generate the code for a binary division operator
    private SymType GenerateIDivide(Emitter emitter, ProgramParseNode cg) {
        cg.GenerateExpression(emitter, SymType.INTEGER, Left);
        cg.GenerateExpression(emitter, SymType.INTEGER, Right);
        emitter.IDiv(SymType.INTEGER);
        return SymType.INTEGER;
    }

    // Generate the code for a binary MOD operator
    private SymType GenerateMod(Emitter emitter, ProgramParseNode cg) {
        cg.GenerateExpression(emitter, Type, Left);
        cg.GenerateExpression(emitter, Type, Right);
        emitter.Mod(Type);
        return Type;
    }

    // Generate the code for a binary exponentiation operator
    private SymType GenerateExp(Emitter emitter, ProgramParseNode cg) {
        cg.GenerateExpression(emitter, SymType.DOUBLE, Left);
        cg.GenerateExpression(emitter, SymType.DOUBLE, Right);
        emitter.Call(cg.GetMethodForType(typeof(Math), "Pow", [typeof(double), typeof(double)]));
        return SymType.DOUBLE;
    }

    // Generate the code for a string merge operator
    private SymType GenerateMerge(Emitter emitter, ProgramParseNode cg) {
        Type charType = typeof(FixedString);

        cg.GenerateExpression(emitter, SymType.FIXEDCHAR, Left);
        cg.GenerateExpression(emitter, SymType.FIXEDCHAR, Right);

        emitter.Call(cg.GetMethodForType(charType, "Merge", [charType, charType]));
        return SymType.FIXEDCHAR;
    }

    // Generate the code for a string concatenation operator
    private SymType GenerateConcat(Emitter emitter, ProgramParseNode cg) {
        Type charType = Symbol.SymTypeToSystemType(Left.Type);

        cg.GenerateExpression(emitter, Left.Type, Left);
        cg.GenerateExpression(emitter, Left.Type, Right);

        emitter.Call(cg.GetMethodForType(charType, "Concat", [charType, charType]));
        return Left.Type;
    }

    // Generate the code for a bitwise AND operator
    private SymType GenerateBitwiseAnd(Emitter emitter, ProgramParseNode cg) {
        cg.GenerateExpression(emitter, Type, Left);
        cg.GenerateExpression(emitter, Type, Right);
        emitter.And();
        return Type;
    }

    // Generate the code for a logical AND operator
    private SymType GenerateLogicalAnd(Emitter emitter, ProgramParseNode cg) {
        Label skipLabel = emitter.CreateLabel();
        Label exitLabel = emitter.CreateLabel();
        cg.GenerateExpression(emitter, Type, Left);
        emitter.BranchIfFalse(skipLabel);
        cg.GenerateExpression(emitter, Type, Right);
        emitter.BranchIfFalse(skipLabel);
        emitter.LoadInteger(1);
        emitter.Branch(exitLabel);
        emitter.MarkLabel(skipLabel);
        emitter.LoadInteger(0);
        emitter.MarkLabel(exitLabel);
        return Type;
    }

    // Generate the code for a bitwise OR operator
    private SymType GenerateBitwiseOr(Emitter emitter, ProgramParseNode cg) {
        cg.GenerateExpression(emitter, Type, Left);
        cg.GenerateExpression(emitter, Type, Right);
        emitter.Or();
        return Type;
    }

    // Generate the code for a logical OR operator
    private SymType GenerateLogicalOr(Emitter emitter, ProgramParseNode cg) {
        Label skipLabel = emitter.CreateLabel();
        Label exitLabel = emitter.CreateLabel();
        cg.GenerateExpression(emitter, Type, Left);
        emitter.BranchIfTrue(skipLabel);
        cg.GenerateExpression(emitter, Type, Right);
        emitter.BranchIfTrue(skipLabel);
        emitter.LoadInteger(0);
        emitter.Branch(exitLabel);
        emitter.MarkLabel(skipLabel);
        emitter.LoadInteger(1);
        emitter.MarkLabel(exitLabel);
        return Type;
    }

    // Generate the code for an exclusive OR operator
    private SymType GenerateXor(Emitter emitter, ProgramParseNode cg) {
        cg.GenerateExpression(emitter, Type, Left);
        cg.GenerateExpression(emitter, Type, Right);
        emitter.Xor();
        return Type;
    }

    // Generate the code for a logical Less Than operator
    private SymType GenerateLt(Emitter emitter, ProgramParseNode cg) {
        SymType neededType = TypePromotion(Left, Right);
        cg.GenerateExpression(emitter, neededType, Left);
        cg.GenerateExpression(emitter, neededType, Right);
        if (Symbol.IsCharType(neededType)) {
            Type charType = Symbol.SymTypeToSystemType(neededType);
            emitter.Call(cg.GetMethodForType(charType, "Compare", [charType, charType]));
            emitter.LoadInteger(0);
        }
        emitter.CompareLesser();
        return Type;
    }

    // Generate the code for a logical Less Than or Equals operator
    private SymType GenerateLe(Emitter emitter, ProgramParseNode cg) {
        SymType neededType = TypePromotion(Left, Right);
        cg.GenerateExpression(emitter, neededType, Left);
        cg.GenerateExpression(emitter, neededType, Right);
        if (Symbol.IsCharType(neededType)) {
            Type charType = Symbol.SymTypeToSystemType(neededType);
            emitter.Call(cg.GetMethodForType(charType, "Compare", [charType, charType]));
            emitter.LoadInteger(0);
        }
        emitter.CompareGreater();
        emitter.LoadInteger(1);
        emitter.Xor();
        return Type;
    }

    // Generate the code for a logical Greater Than operator
    private SymType GenerateGt(Emitter emitter, ProgramParseNode cg) {
        SymType neededType = TypePromotion(Left, Right);
        cg.GenerateExpression(emitter, neededType, Left);
        cg.GenerateExpression(emitter, neededType, Right);
        if (Symbol.IsCharType(neededType)) {
            Type charType = Symbol.SymTypeToSystemType(neededType);
            emitter.Call(cg.GetMethodForType(charType, "Compare", [charType, charType]));
            emitter.LoadInteger(0);
        }
        emitter.CompareGreater();
        return Type;
    }

    // Generate the code for a logical Greater Than or Equals operator
    private SymType GenerateGe(Emitter emitter, ProgramParseNode cg) {
        SymType neededType = TypePromotion(Left, Right);
        cg.GenerateExpression(emitter, neededType, Left);
        cg.GenerateExpression(emitter, neededType, Right);
        if (Symbol.IsCharType(neededType)) {
            Type charType = Symbol.SymTypeToSystemType(neededType);
            emitter.Call(cg.GetMethodForType(charType, "Compare", [charType, charType]));
            emitter.LoadInteger(0);
        }
        emitter.CompareLesser();
        emitter.LoadInteger(1);
        emitter.Xor();
        return Type;
    }

    // Generate the code for a logical Equals operator
    private SymType GenerateEq(Emitter emitter, ProgramParseNode cg) {
        SymType neededType = TypePromotion(Left, Right);
        cg.GenerateExpression(emitter, neededType, Left);
        cg.GenerateExpression(emitter, neededType, Right);
        if (Symbol.IsCharType(neededType)) {
            Type charType = Symbol.SymTypeToSystemType(neededType);
            emitter.Call(cg.GetMethodForType(charType, "op_Equality", [charType, charType]));
        }
        else {
            emitter.CompareEquals();
        }
        return Type;
    }

    // Generate the code for a logical Not Equals operator
    private SymType GenerateNe(Emitter emitter, ProgramParseNode cg) {
        SymType neededType = TypePromotion(Left, Right);
        cg.GenerateExpression(emitter, neededType, Left);
        cg.GenerateExpression(emitter, neededType, Right);
        if (Symbol.IsCharType(neededType)) {
            Type charType = Symbol.SymTypeToSystemType(neededType);
            emitter.Call(cg.GetMethodForType(charType, "op_Equality", [charType, charType]));
        }
        else {
            emitter.CompareEquals();
        }
        emitter.LoadInteger(1);
        emitter.Xor();
        return Type;
    }

    // Given two parse nodes with types associated, this function returns the
    // largest type required for a consistent arithmetic operation between them.
    private static SymType TypePromotion(ParseNode p1, ParseNode p2) {
        return Symbol.LargestType(p1.Type, p2.Type);
    }
}