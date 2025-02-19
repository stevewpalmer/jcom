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
using System.Linq;
using System.Text;
using JCalcLib;
using NUnit.Framework;

namespace CalcLibTests;

[TestFixture]
public class ParsingTests {

    // Verify that the correct parse node is returned for
    // a value on a cell.
    [Test]
    public void VerifyCellValueParseNode() {
        Book workBook = new();
        Sheet sheet = workBook.Sheets.First();
        Cell cell1 = new(sheet) { Content = "=A2+A3" };

        Assert.IsTrue(cell1.FormulaTree is BinaryOpNode);
        Assert.AreEqual(TokenID.PLUS, cell1.FormulaTree.Op);

        Assert.AreEqual("?", CellNode.TokenToString(TokenID.EOL));

        Cell cell2 = new(sheet) { Content = "=A2+A3" };
        Assert.IsTrue(cell2.HasFormula);
        Assert.AreEqual("=A2+A3", cell2.Content);
    }

    // Verify the creation of a BinaryOpParseNode
    [Test]
    public void VerifyBinaryOpParseNode() {
        NumberNode left = new(16);
        LocationNode right = new(new CellLocation("B4"), new Point(4, 4));
        BinaryOpNode pn = new(TokenID.KGT, left, right);

        Assert.AreEqual(pn.Op, TokenID.KGT);
        Assert.AreEqual(pn.Left, left);
        Assert.AreEqual(pn.Right, right);
        Assert.AreEqual("16>B4", pn.ToString());
    }

    // Verify a binary addition formula
    [Test]
    public void VerifyAddition() {
        Book workBook = new();
        Sheet sheet = workBook.Sheets.First();
        Cell cell1 = new(sheet) { Content = "=A2+A3" };
        Assert.IsTrue(cell1.FormulaTree is BinaryOpNode);

        BinaryOpNode pn = (BinaryOpNode)cell1.FormulaTree;
        Assert.AreEqual(TokenID.PLUS, pn.Op);
        Assert.IsTrue(pn.Left is LocationNode);
        Assert.IsTrue(pn.Right is LocationNode);

        LocationNode left = (LocationNode)pn.Left;
        Assert.AreEqual(TokenID.ADDRESS, left.Op);
        Assert.AreEqual(new CellLocation("A2"), left.AbsoluteLocation);

        LocationNode right = (LocationNode)pn.Right;
        Assert.AreEqual(TokenID.ADDRESS, right.Op);
        Assert.AreEqual(new CellLocation("A3"), right.AbsoluteLocation);
    }

    // Verify a binary multiplication formula
    [Test]
    public void VerifyMultiplication() {
        Book workBook = new();
        Sheet sheet = workBook.Sheets.First();
        Cell cell1 = new(sheet) { Content = "=A2*14" };
        Assert.IsTrue(cell1.FormulaTree is BinaryOpNode);

        BinaryOpNode pn = (BinaryOpNode)cell1.FormulaTree;
        Assert.AreEqual(TokenID.MULTIPLY, pn.Op);
        Assert.IsTrue(pn.Left is LocationNode);
        Assert.IsTrue(pn.Right is NumberNode);

        LocationNode left = (LocationNode)pn.Left;
        Assert.AreEqual(TokenID.ADDRESS, left.Op);
        Assert.AreEqual(new CellLocation("A2"), left.AbsoluteLocation);

        NumberNode right = (NumberNode)pn.Right;
        Assert.AreEqual(TokenID.NUMBER, right.Op);
        Assert.AreEqual(14, right.Value.DoubleValue);
    }

    // Verify a unary minus formula which is treated as a binary
    // operation of subtracting the value from 0
    [Test]
    public void VerifyUnaryMinus() {
        Book workBook = new();
        Sheet sheet = workBook.Sheets.First();
        Cell cell1 = new(sheet) { Content = "=-B3" };
        Assert.IsTrue(cell1.FormulaTree is BinaryOpNode);

        BinaryOpNode pn = (BinaryOpNode)cell1.FormulaTree;
        Assert.AreEqual(TokenID.MINUS, pn.Op);
        Assert.IsTrue(pn.Left is NumberNode);
        Assert.IsTrue(pn.Right is LocationNode);

        NumberNode left = (NumberNode)pn.Left;
        Assert.AreEqual(TokenID.NUMBER, left.Op);
        Assert.AreEqual(0, left.Value.DoubleValue);

        LocationNode right = (LocationNode)pn.Right;
        Assert.AreEqual(TokenID.ADDRESS, right.Op);
        Assert.AreEqual(new CellLocation("B3"), right.AbsoluteLocation);
    }

    // Verify a compound expression of multiple operators
    // and operands.
    [Test]
    public void VerifyCompoundExpression() {
        Book workBook = new();
        Sheet sheet = workBook.Sheets.First();
        Cell cell1 = new(sheet) { Content = "=(G190^.5) * (H190 + 40) > .67" };
        Assert.IsTrue(cell1.FormulaTree is BinaryOpNode);

        BinaryOpNode pn = (BinaryOpNode)cell1.FormulaTree;
        Assert.AreEqual(TokenID.KGT, pn.Op);
        Assert.IsTrue(pn.Left is BinaryOpNode);
        Assert.IsTrue(pn.Right is NumberNode);

        BinaryOpNode left = (BinaryOpNode)pn.Left;
        Assert.AreEqual(TokenID.MULTIPLY, left.Op);

        NumberNode right = (NumberNode)pn.Right;
        Assert.AreEqual(TokenID.NUMBER, right.Op);
        Assert.IsTrue(Math.Abs(0.67 - right.Value.DoubleValue) < 0.01);

        Assert.AreEqual("=G190^0.5*(H190+40)>0.67", cell1.Content);
    }

    // Verify the <> operator
    [Test]
    public void VerifyNotEquality() {
        Book workBook = new();
        Sheet sheet = workBook.Sheets.First();
        Cell cell1 = new(sheet) { Content = "=B4<>.67" };
        Assert.IsTrue(cell1.FormulaTree is BinaryOpNode);
        BinaryOpNode pn = (BinaryOpNode)cell1.FormulaTree;
        Assert.AreEqual(TokenID.KNE, pn.Op);
        Assert.AreEqual("=B4<>0.67", cell1.Content);
    }

    // Verify the < operator
    [Test]
    public void VerifyLessThan() {
        Book workBook = new();
        Sheet sheet = workBook.Sheets.First();
        Cell cell1 = new(sheet) { Content = "=B4<P87" };
        Assert.IsTrue(cell1.FormulaTree is BinaryOpNode);
        BinaryOpNode pn = (BinaryOpNode)cell1.FormulaTree;
        Assert.AreEqual(TokenID.KLT, pn.Op);
        Assert.AreEqual("=B4<P87", cell1.Content);
    }

    // Verify the <= operator
    [Test]
    public void VerifyLessThanOrEquals() {
        Book workBook = new();
        Sheet sheet = workBook.Sheets.First();
        Cell cell1 = new(sheet) { Content = "=896675<=P87" };
        Assert.IsTrue(cell1.FormulaTree is BinaryOpNode);
        BinaryOpNode pn = (BinaryOpNode)cell1.FormulaTree;
        Assert.AreEqual(TokenID.KLE, pn.Op);
        Assert.AreEqual("=896675<=P87", cell1.Content);
    }

    // Verify the >= operator
    [Test]
    public void VerifyGreaterThanOrEquals() {
        Book workBook = new();
        Sheet sheet = workBook.Sheets.First();
        Cell cell1 = new(sheet) { Content = "=+896675>=P87" };
        Assert.IsTrue(cell1.FormulaTree is BinaryOpNode);
        BinaryOpNode pn = (BinaryOpNode)cell1.FormulaTree;
        Assert.AreEqual(TokenID.KGE, pn.Op);
        Assert.AreEqual("=896675>=P87", cell1.Content);
    }

    // Verify the = operator
    [Test]
    public void VerifyEquality() {
        Book workBook = new();
        Sheet sheet = workBook.Sheets.First();
        Cell cell1 = new(sheet) { Content = "=B4=90.12E+4" };
        Assert.IsTrue(cell1.FormulaTree is BinaryOpNode);
        BinaryOpNode pn = (BinaryOpNode)cell1.FormulaTree;
        Assert.AreEqual(TokenID.KEQ, pn.Op);
        Assert.AreEqual("=B4=901200", cell1.Content);
    }

    // Verify applying a fixup to a node
    [Test]
    public void VerifyFixup() {
        Book workBook = new();
        Sheet sheet = workBook.Sheets.First();
        Cell cell1 = new(sheet) { Location = new CellLocation { Row = 1, Column = 8 }, Content = "=E5" };
        cell1.FixupFormula(2, 2, 1);
        Assert.AreEqual(cell1.Content, "=F6");

        Cell cell2 = new(sheet) { Location = new CellLocation { Row = 1, Column = 8 }, Content = "=E5*F7" };
        cell2.FixupFormula(2, 2, 1);
        Assert.AreEqual(cell2.Content, "=F6*G8");

        // Test bad fixup
        Cell cell3 = new(sheet) { Location = new CellLocation { Row = 1, Column = 8 }, Content = "=A1" };
        cell3.FixupFormula(0, 1, -1);
        Assert.IsTrue(cell3.FormulaTree is LocationNode);
        Assert.IsTrue(((LocationNode)cell3.FormulaTree).Error);

        cell3 = new Cell(sheet) { Location = new CellLocation { Row = 1, Column = 8 }, Content = "=A1" };
        cell3.FixupFormula(1, 0, -1);
        Assert.IsTrue(cell3.FormulaTree is LocationNode);
        Assert.IsTrue(((LocationNode)cell3.FormulaTree).Error);
    }

    // Verify the different address parsing formats
    [Test]
    public void VerifyAddressParsing() {
        Cell cell1 = new() { Location = new CellLocation { Row = 1, Column = 8 } };
        FormulaParser parser = new("R(4)C(-3)", cell1.Location);
        CellNode node = parser.Parse();
        Assert.IsTrue(node.Op == TokenID.ADDRESS);
        LocationNode addressNode = (LocationNode)node;
        Assert.AreEqual(new CellLocation { Row = 5, Column = 5 }, addressNode.AbsoluteLocation);
        Assert.AreEqual(new Point { X = -3, Y = 4 }, addressNode.RelativeLocation);
        Assert.AreEqual(new CellLocation { Row = 5, Column = 5 }, addressNode.ToAbsolute(cell1.Location));

        // Apply a fixup to the location
        addressNode.FixupAddress(cell1.Location, 2, 2, 1);
        Assert.AreEqual(new CellLocation { Row = 6, Column = 6 }, addressNode.AbsoluteLocation);
        Assert.AreEqual(new Point { X = -2, Y = 5 }, addressNode.RelativeLocation);

        Assert.AreEqual(new Point(-13, 44), Cell.PointFromRelativeAddress("R(44)C(-13)"));
        Assert.AreEqual(new Point(7, -8), Cell.PointFromRelativeAddress("R(-8)C(7)"));
        Assert.AreEqual(new Point(-25, -20), Cell.PointFromRelativeAddress("R(-20)C(-25)"));

        Assert.AreEqual("R(2)C(3)", Cell.LocationToAddress(new Point(3, 2)));
        Assert.AreEqual("R(-2)C(-3)", Cell.LocationToAddress(new Point(-3, -2)));

        Assert.Throws<ArgumentException>(delegate { _ = Cell.PointFromRelativeAddress("P(-20)C(-25)"); });
        Assert.Throws<ArgumentException>(delegate { _ = Cell.PointFromRelativeAddress("R-20C-25"); });
        Assert.Throws<ArgumentException>(delegate { _ = Cell.PointFromRelativeAddress("R(-20)C-25"); });
        Assert.Throws<ArgumentException>(delegate { _ = Cell.PointFromRelativeAddress("R(-20)C(-25"); });
        Assert.Throws<ArgumentException>(delegate { _ = Cell.PointFromRelativeAddress("R(-3320)C(-3325"); });
    }

    // Verify an exception is thrown when a bad operand is
    // specified in the formula.
    [Test]
    public void VerifyBadOperand() {
        Cell cell1 = new();
        Assert.Throws<FormatException>(delegate { _ = new FormulaParser("{}{}", cell1.Location); });
        Assert.Throws<FormatException>(delegate { _ = new FormulaParser("AAB7656", cell1.Location); });
        Assert.Throws<FormatException>(delegate { _ = new FormulaParser("R(-12)C", cell1.Location); });
        Assert.Throws<FormatException>(delegate { _ = new FormulaParser("R(-12902)C(4000)", cell1.Location); });
        Assert.Throws<FormatException>(delegate { _ = new FormulaParser("+B4_H7", cell1.Location); });
        Assert.Throws<FormatException>(delegate { _ = new FormulaParser("*89", cell1.Location).Parse(); });
        Assert.Throws<FormatException>(delegate { _ = new FormulaParser("'HELLO", cell1.Location).Parse(); });
    }

    // Verify an exception is thrown when a bad number is
    // specified in the formula.
    [Test]
    public void VerifyBadNumber() {
        const string badNumber = "=+B4*07E ";
        Cell cell1 = new() { Content = badNumber };
        Assert.IsFalse(cell1.HasFormula);
        Assert.Throws<FormatException>(delegate { _ = new FormulaParser(badNumber[1..], cell1.Location); });
    }

    // Verify spaces in an expression are ignored.
    [Test]
    public void VerifySpaces() {
        Book workBook = new();
        Sheet sheet = workBook.Sheets.First();
        Cell cell1 = new(sheet) { Content = "=   B4 -  I87 " };
        Assert.IsTrue(cell1.FormulaTree is BinaryOpNode);
        BinaryOpNode pn = (BinaryOpNode)cell1.FormulaTree;
        Assert.AreEqual(TokenID.MINUS, pn.Op);
        Assert.IsTrue(pn.Left is LocationNode);
        Assert.IsTrue(pn.Right is LocationNode);
        Assert.AreEqual("=B4-I87", cell1.Content);

        Cell cell2 = new(sheet) { Content = "=   B4 * 0.3E + 4 " };
        Assert.IsTrue(cell1.FormulaTree is BinaryOpNode);
        pn = (BinaryOpNode)cell2.FormulaTree;
        Assert.IsNotNull(pn);
        Assert.AreEqual(TokenID.MULTIPLY, pn.Op);
        Assert.IsTrue(pn.Left is LocationNode);
        Assert.IsTrue(pn.Right is NumberNode);
        Assert.AreEqual("=B4*3000", cell2.Content);
    }

    // Verify that specifying a percentage converts it
    // to the appropriate representation.
    [Test]
    public void VerifyPercentNumber() {
        Book workBook = new();
        Sheet sheet = workBook.Sheets.First();
        Cell cell1 = new(sheet) { Content = "=A3/16%" };
        Assert.IsTrue(cell1.FormulaTree is BinaryOpNode);

        BinaryOpNode pn = (BinaryOpNode)cell1.FormulaTree;
        Assert.AreEqual(TokenID.DIVIDE, pn.Op);
        Assert.IsTrue(pn.Right is NumberNode);
        Assert.AreEqual("R(2)C(0)/0.16", pn.ToRawString());
        Assert.AreEqual("0.16", pn.Right.ToRawString());

        NumberNode right = (NumberNode)pn.Right;
        Assert.IsTrue(Math.Abs(0.16 - right.Value.DoubleValue) < 0.01);
        Assert.AreEqual("=A3/0.16", cell1.Content);
    }

    // Verify an expression with parenthesis creates the correct
    // parse tree.
    [Test]
    public void VerifyParenthesis() {
        Book workBook = new();
        Sheet sheet = workBook.Sheets.First();
        Cell cell1 = new(sheet) { Content = "=A2*(A3+A4)" };
        Assert.IsTrue(cell1.FormulaTree is BinaryOpNode);

        BinaryOpNode pn = (BinaryOpNode)cell1.FormulaTree;
        Assert.AreEqual(TokenID.MULTIPLY, pn.Op);
        Assert.IsTrue(pn.Left is LocationNode);
        Assert.IsTrue(pn.Right is BinaryOpNode);

        LocationNode left = (LocationNode)pn.Left;
        Assert.AreEqual(TokenID.ADDRESS, left.Op);
        Assert.AreEqual(new CellLocation("A2"), left.AbsoluteLocation);

        BinaryOpNode right = (BinaryOpNode)pn.Right;
        Assert.AreEqual(TokenID.PLUS, right.Op);

        Assert.AreEqual("=A2*(A3+A4)", cell1.Content);

        Cell cell3 = new(sheet) { Content = "= ( A2 + A3 ) * A4" };
        Assert.AreEqual("=(A2+A3)*A4", cell3.Content);

        // Missing closing parenthesis should throw an exception
        Cell cell2 = new(sheet);
        Assert.Throws<FormatException>(delegate { _ = new FormulaParser("A1*(A2+A3", cell2.Location).Parse(); });
    }

    /// <summary>
    /// Verify that returning ToString on a TextNode returns the text
    /// content correctly wrapped in quotes.
    /// </summary>
    [Test]
    public void VerifyTextNode() {
        Book workBook = new();
        Sheet sheet = workBook.Sheets.First();
        Cell cell1 = sheet.GetCell(new CellLocation("A1"), true);
        cell1.Content = "='Some text goes here'";
        Assert.IsTrue(cell1.FormulaTree is TextNode);

        TextNode tn = cell1.FormulaTree as TextNode;
        Assert.NotNull(tn);
        Assert.AreEqual("\"Some text goes here\"", tn.ToString());
        Assert.AreEqual("\"Some text goes here\"", tn.ToRawString());
    }

    /// <summary>
    /// Make sure operator precedence is correct.
    /// </summary>
    [Test]
    public void VerifyPrecedence() {
        Book workBook = new();
        Sheet sheet = workBook.Sheets.First();
        Cell cell1 = sheet.GetCell(new CellLocation("A1"), true);
        cell1.Content = "=SUM(1,2)+NOW()*4";
        Assert.IsTrue(cell1.FormulaTree is BinaryOpNode);

        // SUM has higher precedence than that * operator.
        BinaryOpNode pn = (BinaryOpNode)cell1.FormulaTree;
        Assert.IsTrue(FormulaParser.Precedence(pn.Left.Op) > FormulaParser.Precedence(pn.Right.Op));

        // NOW() and a number have equal precedence
        pn = (BinaryOpNode)pn.Right;
        Assert.IsTrue(FormulaParser.Precedence(pn.Left.Op) == FormulaParser.Precedence(pn.Right.Op));

        cell1.Content = "=TODAY()&CONCATENATE(TIME(12,40,3),TIME(9,12,45))";
        Assert.IsTrue(cell1.FormulaTree is BinaryOpNode);

        // Functions have equal precedence
        pn = (BinaryOpNode)cell1.FormulaTree;
        Assert.IsTrue(FormulaParser.Precedence(pn.Left.Op) == FormulaParser.Precedence(pn.Right.Op));

        // Verify precedences
        Assert.IsTrue(FormulaParser.Precedence(TokenID.FUNCTION) == 20);
        Assert.IsTrue(FormulaParser.Precedence(TokenID.NUMBER) == 20);
        Assert.IsTrue(FormulaParser.Precedence(TokenID.ADDRESS) == 20);
        Assert.IsTrue(FormulaParser.Precedence(TokenID.TEXT) == 20);
        Assert.IsTrue(FormulaParser.Precedence(TokenID.KGT) == 5);
        Assert.IsTrue(FormulaParser.Precedence(TokenID.KGE) == 5);
        Assert.IsTrue(FormulaParser.Precedence(TokenID.KLE) == 5);
        Assert.IsTrue(FormulaParser.Precedence(TokenID.KNE) == 5);
        Assert.IsTrue(FormulaParser.Precedence(TokenID.KEQ) == 5);
        Assert.IsTrue(FormulaParser.Precedence(TokenID.KLT) == 5);
        Assert.IsTrue(FormulaParser.Precedence(TokenID.CONCAT) == 6);
        Assert.IsTrue(FormulaParser.Precedence(TokenID.PLUS) == 7);
        Assert.IsTrue(FormulaParser.Precedence(TokenID.MINUS) == 7);
        Assert.IsTrue(FormulaParser.Precedence(TokenID.MULTIPLY) == 8);
        Assert.IsTrue(FormulaParser.Precedence(TokenID.DIVIDE) == 8);
        Assert.IsTrue(FormulaParser.Precedence(TokenID.EXP) == 10);

        // Impossible precedences return 0
        Assert.IsTrue(FormulaParser.Precedence(TokenID.EOL) == 0);
    }

    // Verify parsing the SUM function
    [Test]
    public void VerifySum() {
        Book workBook = new();
        Sheet sheet = workBook.Sheets.First();
        Cell cell = new(sheet) { Location = new CellLocation { Row = 1, Column = 1 }, Content = "=SUM(A2:A4)" };
        Assert.IsNotNull(cell.FormulaTree);
        Assert.IsTrue(cell.FormulaTree.GetType() == typeof(FunctionNode));

        FunctionNode functionNode = (FunctionNode)cell.FormulaTree;
        Assert.AreEqual(TokenID.FUNCTION, functionNode.Op);
        Assert.AreEqual("SUM(A2:A4)", functionNode.ToString());
        Assert.AreEqual("SUM(R(1)C(0):R(3)C(0))", functionNode.ToRawString());

        RangeNode range = (RangeNode)functionNode.Parameters[0];
        Assert.IsTrue(range.RangeStart != null);
        Assert.IsTrue(range.RangeEnd != null);

        LocationNode rangeStart = range.RangeStart;
        LocationNode rangeEnd = range.RangeEnd;
        Assert.AreEqual("A2:A4", range.ToString());
        Assert.AreEqual("R(1)C(0):R(3)C(0)", range.ToRawString());
        Assert.AreEqual(new CellLocation { Column = 1, Row = 2 }, rangeStart.AbsoluteLocation);
        Assert.AreEqual(new CellLocation { Column = 1, Row = 4 }, rangeEnd.AbsoluteLocation);

        StringBuilder str = new();
        foreach (CellLocation loc in range.RangeIterator(cell.Location)) {
            str.Append(loc.Column);
            str.Append(loc.Row);
        }
        Assert.AreEqual("121314", str.ToString());

        range.FixupAddress(cell.Location, 1, 1, 1);
        Assert.AreEqual(new CellLocation { Column = 2, Row = 3 }, rangeStart.AbsoluteLocation);
        Assert.AreEqual(new CellLocation { Column = 2, Row = 5 }, rangeEnd.AbsoluteLocation);

        Assert.Throws<FormatException>(delegate { _ = new FormulaParser("SUM(12:TEXT)", cell.Location).Parse(); });
        Assert.Throws<FormatException>(delegate { _ = new FormulaParser("SUM(A3:12)", cell.Location).Parse(); });
    }
}