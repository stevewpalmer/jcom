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
using System.Drawing;
using JCalcLib;
using NUnit.Framework;

namespace CalcLibTests;

[TestFixture]
public class FormulaTests {

    // Verify that the correct parse node is returned for
    // a value on a cell.
    [Test]
    public void VerifyCellValueParseNode() {
        Cell cell1 = new Cell { Content = "12" };
        Cell cell2 = new Cell { Content = "HELLO" };
        Cell cell3 = new Cell { Content = "=A1+A2" };

        Assert.IsTrue(cell1.ParseNode is NumberParseNode);
        Assert.IsTrue(cell2.ParseNode is TextParseNode);
        Assert.IsTrue(cell3.ParseNode is BinaryOpParseNode);
        Assert.AreEqual(TokenID.NUMBER, cell1.ParseNode.Op);
        Assert.AreEqual(12, ((NumberParseNode)cell1.ParseNode).Value.DoubleValue);
        Assert.AreEqual(TokenID.TEXT, cell2.ParseNode.Op);
        Assert.AreEqual("HELLO", ((TextParseNode)cell2.ParseNode).Value);
        Assert.AreEqual(TokenID.PLUS, cell3.ParseNode.Op);

        Assert.AreEqual("", CellParseNode.TokenToString(TokenID.EOL));
    }

    // Verify the creation of a BinaryOpParseNode
    [Test]
    public void VerifyBinaryOpParseNode() {
        NumberParseNode left = new NumberParseNode(16);
        LocationParseNode right = new LocationParseNode(Cell.LocationFromAddress("B4"), new Point(4, 4));
        BinaryOpParseNode pn = new BinaryOpParseNode(TokenID.KGT, left, right);

        Assert.AreEqual(pn.Op, TokenID.KGT);
        Assert.AreEqual(pn.Left, left);
        Assert.AreEqual(pn.Right, right);
        Assert.AreEqual("16>B4", pn.ToString());
    }

    // Verify a binary addition formula
    [Test]
    public void VerifyAddition() {
        Cell cell1 = new Cell { Content = "=A1+A2" };
        Assert.IsTrue(cell1.ParseNode is BinaryOpParseNode);

        BinaryOpParseNode pn = (BinaryOpParseNode)cell1.ParseNode;
        Assert.AreEqual(TokenID.PLUS, pn.Op);
        Assert.IsTrue(pn.Left is LocationParseNode);
        Assert.IsTrue(pn.Right is LocationParseNode);

        LocationParseNode left = (LocationParseNode)pn.Left;
        Assert.AreEqual(TokenID.ADDRESS, left.Op);
        Assert.AreEqual(Cell.LocationFromAddress("A1"), left.AbsoluteLocation);

        LocationParseNode right = (LocationParseNode)pn.Right;
        Assert.AreEqual(TokenID.ADDRESS, right.Op);
        Assert.AreEqual(Cell.LocationFromAddress("A2"), right.AbsoluteLocation);
    }

    // Verify a binary multiplication formula
    [Test]
    public void VerifyMultiplication() {
        Cell cell1 = new Cell { Content = "=A1*14" };
        Assert.IsTrue(cell1.ParseNode is BinaryOpParseNode);

        BinaryOpParseNode pn = (BinaryOpParseNode)cell1.ParseNode;
        Assert.AreEqual(TokenID.MULTIPLY, pn.Op);
        Assert.IsTrue(pn.Left is LocationParseNode);
        Assert.IsTrue(pn.Right is NumberParseNode);

        LocationParseNode left = (LocationParseNode)pn.Left;
        Assert.AreEqual(TokenID.ADDRESS, left.Op);
        Assert.AreEqual(Cell.LocationFromAddress("A1"), left.AbsoluteLocation);

        NumberParseNode right = (NumberParseNode)pn.Right;
        Assert.AreEqual(TokenID.NUMBER, right.Op);
        Assert.AreEqual(14, right.Value.DoubleValue);
    }

    // Verify a unary minus formula which is treated as a binary
    // operation of subtracting the value from 0
    [Test]
    public void VerifyUnaryMinus() {
        Cell cell1 = new Cell { Content = "=-B3" };
        Assert.IsTrue(cell1.ParseNode is BinaryOpParseNode);

        BinaryOpParseNode pn = (BinaryOpParseNode)cell1.ParseNode;
        Assert.AreEqual(TokenID.MINUS, pn.Op);
        Assert.IsTrue(pn.Left is NumberParseNode);
        Assert.IsTrue(pn.Right is LocationParseNode);

        NumberParseNode left = (NumberParseNode)pn.Left;
        Assert.AreEqual(TokenID.NUMBER, left.Op);
        Assert.AreEqual(0, left.Value.DoubleValue);

        LocationParseNode right = (LocationParseNode)pn.Right;
        Assert.AreEqual(TokenID.ADDRESS, right.Op);
        Assert.AreEqual(Cell.LocationFromAddress("B3"), right.AbsoluteLocation);
    }

    // Verify a compound expression of multiple operators
    // and operands.
    [Test]
    public void VerifyCompoundExpression() {
        Cell cell1 = new Cell { Content = "=(G190^.5) * (H190 + 40) > .67" };
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

        Assert.AreEqual("=G190^0.5*(H190+40)>0.67", cell1.UIContent);
    }

    // Verify the <> operator
    [Test]
    public void VerifyNotEquality() {
        Cell cell1 = new Cell { Content = "=B4<>.67"};
        Assert.IsTrue(cell1.ParseNode is BinaryOpParseNode);
        BinaryOpParseNode pn = (BinaryOpParseNode)cell1.ParseNode;
        Assert.AreEqual(TokenID.KNE, pn.Op);
        Assert.AreEqual("=B4<>0.67", cell1.UIContent);
    }

    // Verify the < operator
    [Test]
    public void VerifyLessThan() {
        Cell cell1 = new Cell { Content = "=B4<P87"};
        Assert.IsTrue(cell1.ParseNode is BinaryOpParseNode);
        BinaryOpParseNode pn = (BinaryOpParseNode)cell1.ParseNode;
        Assert.AreEqual(TokenID.KLT, pn.Op);
        Assert.AreEqual("=B4<P87", cell1.UIContent);
    }

    // Verify the <= operator
    [Test]
    public void VerifyLessThanOrEquals() {
        Cell cell1 = new Cell { Content = "=896675<=P87"};
        Assert.IsTrue(cell1.ParseNode is BinaryOpParseNode);
        BinaryOpParseNode pn = (BinaryOpParseNode)cell1.ParseNode;
        Assert.AreEqual(TokenID.KLE, pn.Op);
        Assert.AreEqual("=896675<=P87", cell1.UIContent);
    }

    // Verify the >= operator
    [Test]
    public void VerifyGreaterThanOrEquals() {
        Cell cell1 = new Cell { Content = "=896675>=P87"};
        Assert.IsTrue(cell1.ParseNode is BinaryOpParseNode);
        BinaryOpParseNode pn = (BinaryOpParseNode)cell1.ParseNode;
        Assert.AreEqual(TokenID.KGE, pn.Op);
        Assert.AreEqual("=896675>=P87", cell1.UIContent);
    }

    // Verify the = operator
    [Test]
    public void VerifyEquality() {
        Cell cell1 = new Cell { Content = "=B4=90.12E+4"};
        Assert.IsTrue(cell1.ParseNode is BinaryOpParseNode);
        BinaryOpParseNode pn = (BinaryOpParseNode)cell1.ParseNode;
        Assert.AreEqual(TokenID.KEQ, pn.Op);
        Assert.AreEqual("=B4=901200", cell1.UIContent);
    }

    // Verify an exception is thrown when a bad operand is
    // specified in the formula.
    [Test]
    public void VerifyBadOperand() {
        Cell cell1 = new Cell { Content = "=+B4**H7"};
        Assert.Throws(typeof(FormatException), delegate { CellParseNode _ = cell1.ParseNode; });

        Cell cell2 = new Cell { Content = "={}{}"};
        Assert.Throws(typeof(FormatException), delegate { CellParseNode _ = cell2.ParseNode; });

        Cell cell3 = new Cell { Content = "=AAB7656"};
        Assert.Throws(typeof(FormatException), delegate { CellParseNode _ = cell3.ParseNode; });
    }

    // Verify an exception is thrown when a bad number is
    // specified in the formula.
    [Test]
    public void VerifyBadNumber() {
        Cell cell1 = new Cell { Content = "=+B4*07E "};
        Assert.Throws(typeof(FormatException), delegate { CellParseNode _ = cell1.ParseNode; });
    }

    // Verify spaces in an expression are ignored.
    [Test]
    public void VerifySpaces() {
        Cell cell1 = new Cell { Content = "=   B4 -  I87 "};
        Assert.IsTrue(cell1.ParseNode is BinaryOpParseNode);
        BinaryOpParseNode pn = (BinaryOpParseNode)cell1.ParseNode;
        Assert.AreEqual(TokenID.MINUS, pn.Op);
        Assert.IsTrue(pn.Left is LocationParseNode);
        Assert.IsTrue(pn.Right is LocationParseNode);
        Assert.AreEqual("=B4-I87", cell1.UIContent);
    }

    // Verify that specifying a percentage in a number converts it
    // to the appropriate representation.
    [Test]
    public void VerifyPercentNumber() {
        Cell cell1 = new Cell { Content = "=A3/16%"};
        Assert.IsTrue(cell1.ParseNode is BinaryOpParseNode);

        BinaryOpParseNode pn = (BinaryOpParseNode)cell1.ParseNode;
        Assert.AreEqual(TokenID.DIVIDE, pn.Op);
        Assert.IsTrue(pn.Right is NumberParseNode);

        NumberParseNode right = (NumberParseNode)pn.Right;
        Assert.IsTrue(Math.Abs(0.16 - right.Value.DoubleValue) < 0.01);
        Assert.AreEqual("=A3/0.16", cell1.UIContent);
    }

    // Verify an expression with parenthesis creates the correct
    // parse tree.
    [Test]
    public void VerifyParenthesis() {
        Cell cell1 = new Cell { Content = "=A1*(A2+A3)"};
        Assert.IsTrue(cell1.ParseNode is BinaryOpParseNode);

        BinaryOpParseNode pn = (BinaryOpParseNode)cell1.ParseNode;
        Assert.AreEqual(TokenID.MULTIPLY, pn.Op);
        Assert.IsTrue(pn.Left is LocationParseNode);
        Assert.IsTrue(pn.Right is BinaryOpParseNode);

        LocationParseNode left = (LocationParseNode)pn.Left;
        Assert.AreEqual(TokenID.ADDRESS, left.Op);
        Assert.AreEqual(Cell.LocationFromAddress("A1"), left.AbsoluteLocation);

        BinaryOpParseNode right = (BinaryOpParseNode)pn.Right;
        Assert.AreEqual(TokenID.PLUS, right.Op);

        Assert.AreEqual("=A1*(A2+A3)", cell1.UIContent);

        Cell cell3 = new Cell { Content = "= ( A1 + A2 ) * A3"};
        Assert.AreEqual("=(A1+A2)*A3", cell3.UIContent);

        // Missing closing parenthesis should throw an exception
        Cell cell2 = new Cell { Content = "=A1*(A2+A3"};
        Assert.Throws(typeof(FormatException), delegate { CellParseNode _ = cell2.ParseNode; });
    }
}