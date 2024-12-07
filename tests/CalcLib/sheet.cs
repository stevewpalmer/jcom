// JCalcLib
// Unit tests for worksheets
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

public class SheetTests {

    // Test that a new sheet has default columns
    [Test]
    public void TestDefaultColumns() {
        Sheet sheet = new(1);
        for (int column = 1; column < Sheet.MaxColumns; column++) {
            Assert.AreEqual(Sheet.DefaultColumnWidth, sheet.ColumnWidth(column));
        }
        sheet.GetCell(1, 1, true);
        Assert.AreEqual(Sheet.DefaultColumnWidth, sheet.ColumnWidth(1));
        sheet.InsertColumn(1);
        Assert.AreEqual(Sheet.DefaultColumnWidth, sheet.ColumnWidth(1));
    }

    // Add a few cells to the sheet and read them back
    [Test]
    public void TestAddingCells() {
        Sheet sheet = new(1);
        Cell cell1 = sheet.GetCell(new CellLocation(1, 1), true);
        Cell cell2 = sheet.GetCell(new CellLocation("A2"), true);
        cell1.Content = "14";
        cell2.Content = "15";
        Cell cell3 = sheet.GetCell(new CellLocation(1, 1), true);
        Cell cell4 = sheet.GetCell(new CellLocation("A2"), true);
        Assert.AreEqual("14", cell3.Content);
        Assert.AreEqual("15", cell4.Content);
    }

    // Test changing cell properties on associated cells correctly marks
    // a sheet as modified.
    [Test]
    public void TestModifiedState() {
        Sheet sheet = new(1);
        Cell cell1 = new(sheet);
        Assert.IsFalse(sheet.Modified);
        cell1.Alignment = CellAlignment.RIGHT;
        Assert.IsTrue(sheet.Modified);

        sheet = new Sheet(1);
        _ = new Cell(sheet) {
            CellFormat = CellFormat.FIXED
        };
        Assert.IsTrue(sheet.Modified);

        sheet = new Sheet(1);
        _ = new Cell(sheet) {
            DecimalPlaces = 4
        };
        Assert.IsTrue(sheet.Modified);

        sheet = new Sheet(1);
        _ = new Cell(sheet) {
            UseThousandsSeparator = true
        };
        Assert.IsTrue(sheet.Modified);
    }
}