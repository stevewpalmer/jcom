// JCalcLib
// Unit tests for the Cell formula parsing
//
// Authors:
//  Steve Palmer
//
// Copyright (C) 2024 Steve Palmer
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
using JCalcLib;
using NUnit.Framework;

namespace CalcLibTests;

[TestFixture]
public class FormulaTests {

    // Verify that the correct parse node is returned for
    // a value on a cell.
    [Test]
    public void VerifyCellValueParseNode() {
        CellValue cell1 = new CellValue { Content = "12" };
        CellValue cell2 = new CellValue { Content = "HELLO" };
        CellValue cell3 = new CellValue { Content = "=A1+A2" };

        Assert.IsTrue(cell1.ParseNode is NumberParseNode);
        Assert.IsTrue(cell2.ParseNode is TextParseNode);
        Assert.IsTrue(cell3.ParseNode is BinaryOpParseNode);
        Assert.AreEqual(TokenID.NUMBER, cell1.ParseNode.Op);
        Assert.AreEqual(12, ((NumberParseNode)cell1.ParseNode).Value.DoubleValue);
        Assert.AreEqual(TokenID.TEXT, cell2.ParseNode.Op);
        Assert.AreEqual("HELLO", ((TextParseNode)cell2.ParseNode).Value);
        Assert.AreEqual(TokenID.PLUS, cell3.ParseNode.Op);
    }

    // Verify the creation of a BinaryOpParseNode
    [Test]
    public void VerifyBinaryOpParseNode() {
        NumberParseNode left = new NumberParseNode(16);
        TextParseNode right = new TextParseNode(TokenID.ADDRESS, "B4");
        BinaryOpParseNode pn = new BinaryOpParseNode(TokenID.KGT, left, right);

        Assert.AreEqual(pn.Op, TokenID.KGT);
        Assert.AreEqual(pn.Left, left);
        Assert.AreEqual(pn.Right, right);
    }

    // Verify a binary addition formula
    [Test]
    public void VerifyAddition() {
        CellValue cell1 = new CellValue { Content = "=A1+A2" };
        Assert.IsTrue(cell1.ParseNode is BinaryOpParseNode);

        BinaryOpParseNode pn = (BinaryOpParseNode)cell1.ParseNode;
        Assert.AreEqual(TokenID.PLUS, pn.Op);
        Assert.IsTrue(pn.Left is TextParseNode);
        Assert.IsTrue(pn.Right is TextParseNode);

        TextParseNode left = (TextParseNode)pn.Left;
        Assert.AreEqual(TokenID.ADDRESS, left.Op);
        Assert.AreEqual("A1", left.Value);

        TextParseNode right = (TextParseNode)pn.Right;
        Assert.AreEqual(TokenID.ADDRESS, right.Op);
        Assert.AreEqual("A2", right.Value);
    }

    // Verify a binary multiplication formula
    [Test]
    public void VerifyMultiplication() {
        CellValue cell1 = new CellValue { Content = "=A1*14" };
        Assert.IsTrue(cell1.ParseNode is BinaryOpParseNode);

        BinaryOpParseNode pn = (BinaryOpParseNode)cell1.ParseNode;
        Assert.AreEqual(TokenID.MULTIPLY, pn.Op);
        Assert.IsTrue(pn.Left is TextParseNode);
        Assert.IsTrue(pn.Right is NumberParseNode);

        TextParseNode left = (TextParseNode)pn.Left;
        Assert.AreEqual(TokenID.ADDRESS, left.Op);
        Assert.AreEqual("A1", left.Value);

        NumberParseNode right = (NumberParseNode)pn.Right;
        Assert.AreEqual(TokenID.NUMBER, right.Op);
        Assert.AreEqual(14, right.Value.DoubleValue);
    }

    // Verify a unary minus formula which is treated as a binary
    // operation of subtracting the value from 0
    [Test]
    public void VerifyUnaryMinus() {
        CellValue cell1 = new CellValue { Content = "=-B3" };
        Assert.IsTrue(cell1.ParseNode is BinaryOpParseNode);

        BinaryOpParseNode pn = (BinaryOpParseNode)cell1.ParseNode;
        Assert.AreEqual(TokenID.MINUS, pn.Op);
        Assert.IsTrue(pn.Left is NumberParseNode);
        Assert.IsTrue(pn.Right is TextParseNode);

        NumberParseNode left = (NumberParseNode)pn.Left;
        Assert.AreEqual(TokenID.NUMBER, left.Op);
        Assert.AreEqual(0, left.Value.DoubleValue);

        TextParseNode right = (TextParseNode)pn.Right;
        Assert.AreEqual(TokenID.ADDRESS, right.Op);
        Assert.AreEqual("B3", right.Value);
    }

    // Verify a compound expression of multiple operators
    // and operands.
    [Test]
    public void VerifyCompoundExpression() {
        CellValue cell1 = new CellValue { Content = "=(G190^.5) * (H190 + 40) > .67" };
        Assert.IsTrue(cell1.ParseNode is BinaryOpParseNode);

        BinaryOpParseNode pn = (BinaryOpParseNode)cell1.ParseNode;
        Assert.AreEqual(TokenID.KGT, pn.Op);
        Assert.IsTrue(pn.Left is BinaryOpParseNode);
        Assert.IsTrue(pn.Right is NumberParseNode);

        BinaryOpParseNode left = (BinaryOpParseNode)pn.Left;
        Assert.AreEqual(TokenID.MULTIPLY, left.Op);

        NumberParseNode right = (NumberParseNode)pn.Right;
        Assert.AreEqual(TokenID.NUMBER, right.Op);
        Assert.IsTrue(Math.Abs(0.67 - right.Value.DoubleValue) < 0.01);
    }

    // Verify the <> operator
    [Test]
    public void VerifyNotEquality() {
        CellValue cell1 = new CellValue { Content = "=B4<>.67"};
        Assert.IsTrue(cell1.ParseNode is BinaryOpParseNode);
        BinaryOpParseNode pn = (BinaryOpParseNode)cell1.ParseNode;
        Assert.AreEqual(TokenID.KNE, pn.Op);
    }

    // Verify the < operator
    [Test]
    public void VerifyLessThan() {
        CellValue cell1 = new CellValue { Content = "=B4<P87"};
        Assert.IsTrue(cell1.ParseNode is BinaryOpParseNode);
        BinaryOpParseNode pn = (BinaryOpParseNode)cell1.ParseNode;
        Assert.AreEqual(TokenID.KLT, pn.Op);
    }

    // Verify the <= operator
    [Test]
    public void VerifyLessThanOrEquals() {
        CellValue cell1 = new CellValue { Content = "=896675<=P87"};
        Assert.IsTrue(cell1.ParseNode is BinaryOpParseNode);
        BinaryOpParseNode pn = (BinaryOpParseNode)cell1.ParseNode;
        Assert.AreEqual(TokenID.KLE, pn.Op);
    }

    // Verify the >= operator
    [Test]
    public void VerifyGreaterThanOrEquals() {
        CellValue cell1 = new CellValue { Content = "=896675>=P87"};
        Assert.IsTrue(cell1.ParseNode is BinaryOpParseNode);
        BinaryOpParseNode pn = (BinaryOpParseNode)cell1.ParseNode;
        Assert.AreEqual(TokenID.KGE, pn.Op);
    }

    // Verify the = operator
    [Test]
    public void VerifyEquality() {
        CellValue cell1 = new CellValue { Content = "=B4=90.12E+4"};
        Assert.IsTrue(cell1.ParseNode is BinaryOpParseNode);
        BinaryOpParseNode pn = (BinaryOpParseNode)cell1.ParseNode;
        Assert.AreEqual(TokenID.KEQ, pn.Op);
    }

    // Verify an exception is thrown when a bad operand is
    // specified in the formula.
    [Test]
    public void VerifyBadOperand() {
        CellValue cell1 = new CellValue { Content = "=+B4**H7"};
        Assert.Throws(typeof(FormatException), delegate { CellParseNode _ = cell1.ParseNode; });

        CellValue cell2 = new CellValue { Content = "={}{}"};
        Assert.Throws(typeof(FormatException), delegate { CellParseNode _ = cell2.ParseNode; });

        CellValue cell3 = new CellValue { Content = "=AAB7656"};
        Assert.Throws(typeof(FormatException), delegate { CellParseNode _ = cell3.ParseNode; });
    }

    // Verify an exception is thrown when a bad number is
    // specified in the formula.
    [Test]
    public void VerifyBadNumber() {
        CellValue cell1 = new CellValue { Content = "=+B4*07E "};
        Assert.Throws(typeof(FormatException), delegate { CellParseNode _ = cell1.ParseNode; });
    }

    // Verify spaces in an expression are ignored.
    [Test]
    public void VerifySpaces() {
        CellValue cell1 = new CellValue { Content = "=   B4 -  I87 "};
        Assert.IsTrue(cell1.ParseNode is BinaryOpParseNode);
        BinaryOpParseNode pn = (BinaryOpParseNode)cell1.ParseNode;
        Assert.AreEqual(TokenID.MINUS, pn.Op);
        Assert.IsTrue(pn.Left is TextParseNode);
        Assert.IsTrue(pn.Right is TextParseNode);
    }

    // Verify that specifying a percentage in a number converts it
    // to the appropriate representation.
    [Test]
    public void VerifyPercentNumber() {
        CellValue cell1 = new CellValue { Content = "=A3/16%"};
        Assert.IsTrue(cell1.ParseNode is BinaryOpParseNode);

        BinaryOpParseNode pn = (BinaryOpParseNode)cell1.ParseNode;
        Assert.AreEqual(TokenID.DIVIDE, pn.Op);
        Assert.IsTrue(pn.Right is NumberParseNode);

        NumberParseNode right = (NumberParseNode)pn.Right;
        Assert.IsTrue(Math.Abs(0.16 - right.Value.DoubleValue) < 0.01);
    }

    // Verify an expression with parenthesis creates the correct
    // parse tree.
    [Test]
    public void VerifyParenthesis() {
        CellValue cell1 = new CellValue { Content = "=A1*(A2+A3)"};
        Assert.IsTrue(cell1.ParseNode is BinaryOpParseNode);

        BinaryOpParseNode pn = (BinaryOpParseNode)cell1.ParseNode;
        Assert.AreEqual(TokenID.MULTIPLY, pn.Op);
        Assert.IsTrue(pn.Left is TextParseNode);
        Assert.IsTrue(pn.Right is BinaryOpParseNode);

        TextParseNode left = (TextParseNode)pn.Left;
        Assert.AreEqual(TokenID.ADDRESS, left.Op);
        Assert.AreEqual("A1", left.Value);

        BinaryOpParseNode right = (BinaryOpParseNode)pn.Right;
        Assert.AreEqual(TokenID.PLUS, right.Op);

        // Missing closing parenthesis should throw an exception
        CellValue cell2 = new CellValue { Content = "=A1*(A2+A3"};
        Assert.Throws(typeof(FormatException), delegate { CellParseNode _ = cell2.ParseNode; });
    }
}