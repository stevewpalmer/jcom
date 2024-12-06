// JCalcLib
// Unit tests for the Cell formulae
//
// Authors:
//  Steve Palmer
//
// Copyright (C) 2024 Steven
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
        Sheet sheet = new();
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
    }

    // Verify the & operator
    [Test]
    public void VerifyConcatOperator() {
        Sheet sheet = new();
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
    /// Verify we catch circular references
    /// </summary>
    [Test]
    public void VerifyCircularReferences() {
        Sheet sheet = new();
        Cell cell1 = sheet.GetCell(new CellLocation("A1"), true);
        Cell cell2 = sheet.GetCell(new CellLocation("A2"), true);

        // Direct loop
        cell1.Content = "=A2";
        cell2.Content = "=A1";
        sheet.Calculate();
        Assert.AreEqual(" !ERR ", cell1.Text(6));
        Assert.AreEqual(" !ERR ", cell2.Text(6));
    }
}