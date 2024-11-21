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

using JCalcLib;
using NUnit.Framework;

namespace CalcLibTests;

public class FunctionTests {

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
        cell1.Content = "56";
        cell2.Content = "78";
        cell3.Content = "12";
        cell4.UIContent = "=SUM(A1:A3)";
        Calculate calc = new Calculate(sheet);
        calc.Update();
        Assert.AreEqual("146", cell4.CellValue.Value);
    }

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
        cell4.UIContent = "=A1*(A2+A3)";
        Calculate calc = new Calculate(sheet);
        calc.Update();
        Assert.AreEqual("5040", cell4.CellValue.Value);
    }}