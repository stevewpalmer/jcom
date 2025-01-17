// JCalcLib
// Unit tests for the Cell formulae
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
using System.Linq;
using JCalcLib;
using JComLib;
using NUnit.Framework;

namespace CalcLibTests;

public class FormulaTests {

    /// <summary>
    /// Verify a simple expression
    /// </summary>
    [Test]
    public void VerifyExpression() {
        Book workBook = new();
        Sheet sheet = workBook.Sheets.First();
        Cell cell1 = sheet.GetCell(new CellLocation("A1"), true);
        Cell cell2 = sheet.GetCell(new CellLocation("A2"), true);
        Cell cell3 = sheet.GetCell(new CellLocation("A3"), true);
        Cell cell4 = sheet.GetCell(new CellLocation("A4"), true);
        cell1.Content = "56";
        cell2.Content = "78";
        cell3.Content = "12";
        cell4.Content = "=A1*(A2+A3)";
        sheet.Calculate();
        Assert.AreEqual(new Variant(5040), cell4.Value);

        CellLocation [] dependents = workBook.Dependents(cell4.LocationWithSheet).ToArray();
        Assert.AreEqual(3, dependents.Length);
        Assert.AreEqual(cell1.LocationWithSheet, dependents[0]);
        Assert.AreEqual(cell2.LocationWithSheet, dependents[1]);
        Assert.AreEqual(cell3.LocationWithSheet, dependents[2]);
    }

    // Verify the = operator
    [Test]
    public void VerifyEquals() {
        Book workBook = new();
        Sheet sheet = workBook.Sheets.First();
        Cell cell1 = sheet.GetCell(new CellLocation("A1"), true);
        Cell cell2 = sheet.GetCell(new CellLocation("A2"), true);
        Cell cell3 = sheet.GetCell(new CellLocation("A3"), true);

        cell1.Content = "34.56";
        cell2.Content = "34.56";
        cell3.Content = "=A1=A2";
        sheet.Calculate();
        Assert.AreEqual(new Variant(true), cell3.Value);

        cell1.Content = "HELLO";
        cell2.Content = "HELLO";
        cell3.Content = "=A1=A2";
        sheet.Calculate();
        Assert.AreEqual(new Variant(true), cell3.Value);
    }

    // Verify the <> operator
    [Test]
    public void VerifyNotEquals() {
        Book workBook = new();
        Sheet sheet = workBook.Sheets.First();
        Cell cell1 = sheet.GetCell(new CellLocation("A1"), true);
        Cell cell2 = sheet.GetCell(new CellLocation("A2"), true);
        Cell cell3 = sheet.GetCell(new CellLocation("A3"), true);

        cell1.Content = "40";
        cell2.Content = "0.2";
        cell3.Content = "=A1<>A2";
        sheet.Calculate();
        Assert.AreEqual(new Variant(true), cell3.Value);

        cell1.Content = "HELLO";
        cell2.Content = "Art";
        cell3.Content = "=A1<>A2";
        sheet.Calculate();
        Assert.AreEqual(new Variant(true), cell3.Value);
    }

    // Verify the >= operator
    [Test]
    public void VerifyGreaterThanOrEquals() {
        Book workBook = new();
        Sheet sheet = workBook.Sheets.First();
        Cell cell1 = sheet.GetCell(new CellLocation("A1"), true);
        Cell cell2 = sheet.GetCell(new CellLocation("A2"), true);
        Cell cell3 = sheet.GetCell(new CellLocation("A3"), true);

        cell1.Content = "56";
        cell2.Content = "33";
        cell3.Content = "=A1>=A2";
        sheet.Calculate();
        Assert.AreEqual(new Variant(true), cell3.Value);

        cell1.Content = "Blake";
        cell2.Content = "Austen";
        cell3.Content = "=A1>=A2";
        sheet.Calculate();
        Assert.AreEqual(new Variant(true), cell3.Value);
    }

    // Verify the > operator
    [Test]
    public void VerifyGreaterThan() {
        Book workBook = new();
        Sheet sheet = workBook.Sheets.First();
        Cell cell1 = sheet.GetCell(new CellLocation("A1"), true);
        Cell cell2 = sheet.GetCell(new CellLocation("A2"), true);
        Cell cell3 = sheet.GetCell(new CellLocation("A3"), true);

        cell1.Content = "-89";
        cell2.Content = "-103";
        cell3.Content = "=A1>A2";
        sheet.Calculate();
        Assert.AreEqual(new Variant(true), cell3.Value);

        cell1.Content = "Joy";
        cell2.Content = "Content";
        cell3.Content = "=A1>A2";
        sheet.Calculate();
        Assert.AreEqual(new Variant(true), cell3.Value);
    }

    // Verify the >= operator
    [Test]
    public void VerifyLessThanOrEquals() {
        Book workBook = new();
        Sheet sheet = workBook.Sheets.First();
        Cell cell1 = sheet.GetCell(new CellLocation("A1"), true);
        Cell cell2 = sheet.GetCell(new CellLocation("A2"), true);
        Cell cell3 = sheet.GetCell(new CellLocation("A3"), true);

        cell1.Content = "4E+02";
        cell2.Content = "12.3E+03";
        cell3.Content = "=A1<=A2";
        sheet.Calculate();
        Assert.AreEqual(new Variant(true), cell3.Value);

        cell1.Content = "Tate Modern";
        cell2.Content = "Union Museum";
        cell3.Content = "=A1<=A2";
        sheet.Calculate();
        Assert.AreEqual(new Variant(true), cell3.Value);
    }

    // Verify the < operator
    [Test]
    public void VerifyLessThan() {
        Book workBook = new();
        Sheet sheet = workBook.Sheets.First();
        Cell cell1 = sheet.GetCell(new CellLocation("A1"), true);
        Cell cell2 = sheet.GetCell(new CellLocation("A2"), true);
        Cell cell3 = sheet.GetCell(new CellLocation("A3"), true);

        cell1.Content = "-109";
        cell2.Content = "0.34";
        cell3.Content = "=A1<A2";
        sheet.Calculate();
        Assert.AreEqual(new Variant(true), cell3.Value);

        cell1.Content = "Poverty";
        cell2.Content = "Wealth";
        cell3.Content = "=A1<A2";
        sheet.Calculate();
        Assert.AreEqual(new Variant(true), cell3.Value);
    }

    // Verify the EXP operator
    [Test]
    public void VerifyEXP() {
        Book workBook = new();
        Sheet sheet = workBook.Sheets.First();
        Cell cell1 = sheet.GetCell(new CellLocation("A1"), true);
        Cell cell2 = sheet.GetCell(new CellLocation("A2"), true);
        Cell cell3 = sheet.GetCell(new CellLocation("A3"), true);

        cell1.Content = "20";
        cell2.Content = "3";
        cell3.Content = "=A1^A2";
        sheet.Calculate();
        Assert.AreEqual(new Variant(8000), cell3.Value);

        cell1.Content = "WORD";
        cell2.Content = "SPELL";
        cell3.Content = "=A1^A2";
        sheet.Calculate();
        Assert.AreEqual(new Variant(1), cell3.Value);
    }

    // Verify the minus operator
    [Test]
    public void VerifyMinus() {
        Book workBook = new();
        Sheet sheet = workBook.Sheets.First();
        Cell cell1 = sheet.GetCell(new CellLocation("A1"), true);
        Cell cell2 = sheet.GetCell(new CellLocation("A2"), true);
        Cell cell3 = sheet.GetCell(new CellLocation("A3"), true);

        cell1.Content = "20";
        cell2.Content = "3";
        cell3.Content = "=-A1*+A2";
        sheet.Calculate();
        Assert.AreEqual(new Variant(-60), cell3.Value);
    }

    // Verify the & operator
    [Test]
    public void VerifyConcatOperator() {
        Book workBook = new();
        Sheet sheet = workBook.Sheets.First();
        Cell cell1 = sheet.GetCell(new CellLocation("A1"), true);
        Cell cell2 = sheet.GetCell(new CellLocation("A2"), true);
        Cell cell3 = sheet.GetCell(new CellLocation("A3"), true);

        cell1.Content = " HELLO ";
        cell2.Content = " WORLD!";
        cell3.Content = "=A1&A2";
        sheet.Calculate();
        Assert.AreEqual(" HELLO  WORLD!", cell3.Value.StringValue);
        Assert.AreEqual("=A1&A2", cell3.Content);

        cell3.Content = "='HELLO '&' WORLD!'";
        sheet.Calculate();
        Assert.AreEqual("HELLO  WORLD!", cell3.Value.StringValue);
        Assert.AreEqual("=\"HELLO \"&\" WORLD!\"", cell3.Content);
    }

    /// <summary>
    /// Verify we catch an unhandled binary operator
    /// </summary>
    [Test]
    public void TestBadOperator() {
        BinaryOpNode pn = new(TokenID.LPAREN, new NumberNode(90), new NumberNode(4));
        Book workBook = new();
        Sheet sheet = workBook.Sheets.First();
        CalculationContext context = new() { Sheet = sheet };
        Assert.Throws<NotImplementedException>(delegate { pn.Evaluate(context); });
    }

    /// <summary>
    /// Verify we catch circular references
    /// </summary>
    [Test]
    public void VerifyCircularReferences() {
        Book workBook = new();
        Sheet sheet = workBook.Sheets.First();
        sheet.Name = "Summary";

        Cell cell1 = sheet.GetCell(new CellLocation("A1"), true);
        Cell cell2 = sheet.GetCell(new CellLocation("A2"), true);

        // Direct loop
        cell1.Content = "=A2";
        cell2.Content = "=A1";
        sheet.Calculate();
        Assert.AreEqual(" !ERR ", cell1.TextForWidth(6));
        Assert.AreEqual(" !ERR ", cell2.TextForWidth(6));

        // A more complex circular reference across multiple sheets
        // Here, cell A1 on all three sheets refer to cell A1 on the
        // previous sheet:
        //
        //  Expenses!A1 <= Savings!A1 <= Summary!A1
        Sheet sheet2 = workBook.AddSheet();
        Sheet sheet3 = workBook.AddSheet();
        sheet2.Name = "Expenses";
        sheet3.Name = "Savings";

        Cell cell3 = sheet2.GetCell(new CellLocation("A1"), true);
        Cell cell4 = sheet3.GetCell(new CellLocation("A1"), true);
        cell3.Value = new Variant("=Summary!A1");
        cell1.Value = new Variant("=Savings!A1");
        cell4.Value = new Variant("=Expenses!A1");
        //sheet2.Calculate();

        //Assert.AreEqual(" !ERR ", cell3.TextForWidth(6));
    }

    /// <summary>
    /// Make sure the NeedRecalculate property is set if we
    /// update columns or rows.
    /// </summary>
    [Test]
    public void VerifyNeedRecalculate() {
        Book workBook = new();
        Sheet sheet = workBook.Sheets.First();
        Cell cell1 = sheet.GetCell(new CellLocation("A1"), true);
        Cell cell2 = sheet.GetCell(new CellLocation("B2"), true);
        Cell cell3 = sheet.GetCell(new CellLocation("C3"), true);
        cell1.Content = "4.5";
        cell2.Content = "167.03";
        cell3.Content = "=A1*B2";
        sheet.Calculate();
        Assert.AreEqual(new Variant(751.635), cell3.Value);

        sheet.InsertColumn(1);
        sheet.InsertRow(2);
        Assert.IsTrue(sheet.NeedRecalculate);
        sheet.Calculate();
        Assert.IsFalse(sheet.NeedRecalculate);
        Assert.AreEqual(new Variant(751.635), cell3.Value);
        Assert.AreEqual("=B1*C3", cell3.Content);
        Assert.AreEqual("D4", cell3.Location.Address);
    }

    /// <summary>
    /// Verify the dependency chains.
    /// </summary>
    [Test]
    public void VerifyDependencies() {

    }
}