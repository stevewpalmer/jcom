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
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using JCalcLib;
using JFortranLib;
using NUnit.Framework;

namespace CalcLibTests;

public class FunctionTests {

    /// <summary>
    /// Verify the SUM function
    /// </summary>
    [Test]
    public void VerifySum() {
        Cell cell = new Cell { Location = new CellLocation { Row = 1, Column = 1 }, UIContent = "=SUM(A2:A4)"};
        Assert.IsTrue(cell.ParseNode.GetType() == typeof(FunctionParseNode));

        FunctionParseNode functionNode = (FunctionParseNode)cell.ParseNode;
        Assert.AreEqual(TokenID.KSUM, functionNode.Op);
        Assert.AreEqual("SUM(A2:A4)", functionNode.ToString());
        Assert.AreEqual("SUM(R(1)C(0):R(3)C(0))", functionNode.ToRawString());

        RangeParseNode range = (RangeParseNode)functionNode.Parameters[0];
        Assert.IsTrue(range.RangeStart != null);
        Assert.IsTrue(range.RangeEnd != null);

        LocationParseNode rangeStart = range.RangeStart;
        LocationParseNode rangeEnd = range.RangeEnd;
        Assert.AreEqual("A2:A4", range.ToString());
        Assert.AreEqual("R(1)C(0):R(3)C(0)", range.ToRawString());
        Assert.AreEqual(new CellLocation { Column = 1, Row = 2}, rangeStart.AbsoluteLocation);
        Assert.AreEqual(new CellLocation { Column = 1, Row = 4}, rangeEnd.AbsoluteLocation);

        StringBuilder str = new();
        foreach (CellLocation loc in range.RangeIterator(cell.Location)) {
            str.Append(loc.Column);
            str.Append(loc.Row);
        }
        Assert.AreEqual("121314", str.ToString());

        range.FixupAddress(cell.Location, 1, 1, 1);
        Assert.AreEqual(new CellLocation { Column = 2, Row = 3}, rangeStart.AbsoluteLocation);
        Assert.AreEqual(new CellLocation { Column = 2, Row = 5}, rangeEnd.AbsoluteLocation);

        Assert.Throws(typeof(FormatException), delegate { _ = new FormulaParser("SUM(12:TEXT)", cell.Location).Parse(); });
        Assert.Throws(typeof(FormatException), delegate { _ = new FormulaParser("SUM(A3:12)", cell.Location).Parse(); });
    }
}