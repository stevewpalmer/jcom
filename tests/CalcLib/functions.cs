// JCalcLib
// Unit tests for the formula functions
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
using JComLib;
using NUnit.Framework;

namespace CalcLibTests;

public class FunctionTests {

    /// <summary>
    /// Verify a simple expression
    /// </summary>
    [Test]
    public void VerifyExpression() {
        Sheet sheet = new Sheet();
        Cell cell1 = sheet.GetCell(new CellLocation("A1"), true);
        Cell cell2 = sheet.GetCell(new CellLocation("A2"), true);
        Cell cell3 = sheet.GetCell(new CellLocation("A3"), true);
        Cell cell4 = sheet.GetCell(new CellLocation("A4"), true);
        cell1.Content = "56";
        cell2.Content = "78";
        cell3.Content = "12";
        cell4.Content = "=A1*(A2+A3)";
        Calculate calc = new Calculate(sheet);
        calc.Update();
        Assert.AreEqual(new Variant(5040), cell4.Value);
    }

    /// <summary>
    /// Verify the SUM function
    /// </summary>
    [Test]
    public void VerifySum() {
        Sheet sheet = new Sheet();
        Cell cell1 = sheet.GetCell(new CellLocation("A1"), true);
        Cell cell2 = sheet.GetCell(new CellLocation("A2"), true);
        Cell cell3 = sheet.GetCell(new CellLocation("A3"), true);
        Cell cell4 = sheet.GetCell(new CellLocation("A4"), true);
        Cell cell5 = sheet.GetCell(new CellLocation("A5"), true);

        // Simple range sum
        cell1.Content = "56";
        cell2.Content = "78";
        cell3.Content = "12";
        cell5.Content = "=SUM(A1:A3)";
        Calculate calc = new Calculate(sheet);
        calc.Update();
        Assert.AreEqual(new Variant("146"), cell5.Value);

        // Multiple ranges sum
        cell4.Content = "-7";
        cell5.Content = "=SUM(A1:A2,A3:A4)";
        calc = new Calculate(sheet);
        calc.Update();
        Assert.AreEqual(new Variant(139), cell5.Value);

        // Sum literals
        cell5.Content = "=SUM(40,50,60,70,80)";
        calc = new Calculate(sheet);
        calc.Update();
        Assert.AreEqual(new Variant(300), cell5.Value);

        // Sum with expressions
        cell5.Content = "=SUM(3*4,9/3)";
        calc = new Calculate(sheet);
        calc.Update();
        Assert.IsTrue(cell5.Value == 15);

        // Sum with strings and mixed expression types
        cell5.Content = "=SUM(A1,A2:A4,90,\"Hello\",4*6)";
        calc = new Calculate(sheet);
        calc.Update();
        Assert.IsTrue(cell5.Value == 253);

        // Empty sum!
        cell5.Content = "=SUM()";
        calc = new Calculate(sheet);
        calc.Update();
        Assert.AreEqual(new Variant(0), cell5.Value);
    }

    /// <summary>
    /// Verify the CONCATENATE function
    /// </summary>
    [Test]
    public void VerifyConcatenate() {
        Sheet sheet = new Sheet();
        Cell cell1 = sheet.GetCell(new CellLocation("A1"), true);
        Cell cell2 = sheet.GetCell(new CellLocation("A2"), true);
        Cell cell3 = sheet.GetCell(new CellLocation("A3"), true);
        Cell cell4 = sheet.GetCell(new CellLocation("A4"), true);
        Cell cell5 = sheet.GetCell(new CellLocation("A5"), true);

        // Simple concatenation
        cell1.Content = "Happy";
        cell2.Content = " New ";
        cell3.Content = "Year";
        cell5.Content = "=CONCATENATE(A1:A3)";
        Calculate calc = new Calculate(sheet);
        calc.Update();
        Assert.AreEqual(new Variant("Happy New Year"), cell5.Value);

        // Multiple ranges concatenation
        cell4.Content = " Everyone!";
        cell5.Content = "=CONCATENATE(A1:A2,A3:A4)";
        calc = new Calculate(sheet);
        calc.Update();
        Assert.AreEqual(new Variant("Happy New Year Everyone!"), cell5.Value);

        // Empty cells are skipped
        sheet.DeleteCell(cell2);
        sheet.DeleteCell(cell3);
        cell5.Content = "=CONCATENATE(A1:A2,A3:A4)";
        calc = new Calculate(sheet);
        calc.Update();
        Assert.AreEqual(new Variant("Happy Everyone!"), cell5.Value);

        // Concatenate literals
        cell5.Content = "=CONCATENATE(\"All\",\" applaud\", \" the\", \" NHS!\")";
        calc = new Calculate(sheet);
        calc.Update();
        Assert.AreEqual(new Variant("All applaud the NHS!"), cell5.Value);

        // Concatenate mixed types
        cell5.Content = "=CONCATENATE(\"The answer is \",3*4,\", right?\")";
        calc = new Calculate(sheet);
        calc.Update();
        Assert.AreEqual(new Variant("The answer is 12, right?"), cell5.Value);

        // Empty concatenation!
        cell5.Content = "=CONCATENATE()";
        calc = new Calculate(sheet);
        calc.Update();
        Assert.AreEqual(new Variant(""), cell5.Value);
    }
}