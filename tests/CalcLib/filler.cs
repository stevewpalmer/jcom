// JCalcLib
// Unit tests for the filler class
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
using System.Collections.Generic;
using JCalcLib;
using JComLib;
using NUnit.Framework;

namespace CalcLibTests;

public class FillerTests {

    /// <summary>
    /// Simple fill test
    /// </summary>
    [Test]
    public void SimpleFillerTest() {
        Sheet sheet = new(1);
        Cell cell1 = sheet.GetCell(new CellLocation("A1"), true);
        cell1.Value = new Variant(12);

        List<CellLocation> cellLocations = new();
        for (int row = 1; row < 10; row++) {
            cellLocations.Add(new CellLocation(1, row));
        }

        CellFiller filler = new(sheet, cellLocations);
        filler.Process();

        foreach (CellLocation location in cellLocations) {
            Cell cell = sheet.GetCell(location, false);
            Assert.AreEqual(new Variant(12), cell.Value);
        }

        // Make sure we didn't spill outside the range
        Cell afterLast = sheet.GetCell(new CellLocation("A11"), true);
        Assert.IsTrue(afterLast.IsEmptyCell);

        // A fill always requires a recalculate at the moment.
        Assert.IsTrue(sheet.NeedRecalculate);
    }

    /// <summary>
    /// Fill test an incrementing variant
    /// </summary>
    [Test]
    public void IncrementingFillerTest() {
        Sheet sheet = new(1);
        Cell cell1 = sheet.GetCell(new CellLocation("A1"), true);
        Cell cell2 = sheet.GetCell(new CellLocation("A2"), true);
        cell1.Value = new Variant(100);
        cell2.Value = new Variant(150);

        List<CellLocation> cellLocations = new();
        for (int row = 1; row < 10; row++) {
            cellLocations.Add(new CellLocation(1, row));
        }

        CellFiller filler = new(sheet, cellLocations);
        filler.Process();

        int expected = 100;
        foreach (CellLocation location in cellLocations) {
            Cell cell = sheet.GetCell(location, false);
            Assert.AreEqual(new Variant(expected), cell.Value);
            expected += 50;
        }
    }

    /// <summary>
    /// Test date filler
    /// </summary>
    [Test]
    public void VerifySimpleDateFill() {
        Sheet sheet = new(1);
        Cell cell1 = sheet.GetCell(new CellLocation("A1"), true);
        cell1.Content = "April 1983";
        cell1.CellFormat = CellFormat.DATE_MY;

        List<CellLocation> cellLocations = new();
        for (int row = 1; row < 10; row++) {
            cellLocations.Add(new CellLocation(1, row));
        }

        CellFiller filler = new(sheet, cellLocations);
        filler.Process();

        foreach (CellLocation location in cellLocations) {
            Cell cell = sheet.GetCell(location, false);
            Assert.AreEqual("Apr-1983", cell.Text);
            Assert.AreEqual(CellFormat.DATE_MY, cell.CellFormat);
        }
    }

    /// <summary>
    /// Incrementing date filler for Month-Year
    /// </summary>
    [Test]
    public void VerifyIncrementingMYFill() {
        Sheet sheet = new(1);
        Cell cell1 = sheet.GetCell(new CellLocation("A1"), true);
        Cell cell2 = sheet.GetCell(new CellLocation("A2"), true);
        cell1.Content = "April 1983";
        cell1.CellFormat = CellFormat.DATE_MY;
        cell2.Content = "May 1983";
        cell2.CellFormat = CellFormat.DATE_MY;

        List<CellLocation> cellLocations = new();
        for (int row = 1; row < 10; row++) {
            cellLocations.Add(new CellLocation(1, row));
        }

        CellFiller filler = new(sheet, cellLocations);
        filler.Process();

        DateTime startDate = new(1983, 4, 1);
        foreach (CellLocation location in cellLocations) {
            Cell cell = sheet.GetCell(location, false);
            Assert.AreEqual(startDate.ToString("MMM-yyyy"), cell.Text);
            Assert.AreEqual(CellFormat.DATE_MY, cell.CellFormat);
            startDate = startDate.AddMonths(1);
        }
    }

    /// <summary>
    /// Incrementing date filler for Day-Month
    /// </summary>
    [Test]
    public void VerifyIncrementingDMFill() {
        Sheet sheet = new(1);
        Cell cell1 = sheet.GetCell(new CellLocation("A1"), true);
        Cell cell2 = sheet.GetCell(new CellLocation("A2"), true);
        cell1.Content = "1 October 1983";
        cell1.CellFormat = CellFormat.DATE_DM;
        cell2.Content = "2 October 1983";
        cell2.CellFormat = CellFormat.DATE_DM;

        List<CellLocation> cellLocations = new();
        for (int row = 1; row < 10; row++) {
            cellLocations.Add(new CellLocation(1, row));
        }

        CellFiller filler = new(sheet, cellLocations);
        filler.Process();

        DateTime startDate = new(1983, 10, 1);
        foreach (CellLocation location in cellLocations) {
            Cell cell = sheet.GetCell(location, false);
            Assert.AreEqual(startDate.ToString("dd-MMM"), cell.Text);
            Assert.AreEqual(CellFormat.DATE_DM, cell.CellFormat);
            startDate = startDate.AddDays(1);
        }
    }

    /// <summary>
    /// Test a horizontal fill.
    /// </summary>
    [Test]
    public void VerifyHorizontalFill() {
        Sheet sheet = new(1);
        Cell cell1 = sheet.GetCell(new CellLocation("A1"), true);
        cell1.Value = new Variant(0.12);

        List<CellLocation> cellLocations = new();
        for (int column = 1; column < 10; column++) {
            cellLocations.Add(new CellLocation(column, 1));
        }

        CellFiller filler = new(sheet, cellLocations);
        filler.Process();

        foreach (CellLocation location in cellLocations) {
            Cell cell = sheet.GetCell(location, false);
            Assert.AreEqual(new Variant(0.12), cell.Value);
        }
    }

    /// <summary>
    /// Test filling empty cells.
    /// </summary>
    [Test]
    public void VerifyFillEmptyFill() {
        Sheet sheet = new(1);

        List<CellLocation> cellLocations = new();
        for (int row = 1; row < 10; row++) {
            cellLocations.Add(new CellLocation(1, row));
        }

        CellFiller filler = new(sheet, cellLocations);
        filler.Process();

        foreach (CellLocation location in cellLocations) {
            Cell cell = sheet.GetCell(location, false);
            Assert.IsTrue(cell.IsEmptyCell);
        }
    }

    /// <summary>
    /// Test that styles are replicated into filled cells
    /// </summary>
    [Test]
    public void VerifyStyleReplication() {
        Sheet sheet = new(1);
        Cell cell1 = sheet.GetCell(new CellLocation("A1"), true);
        cell1.Value = new Variant(100.0 / 3.0 / 100.0);
        cell1.CellFormat = CellFormat.PERCENT;
        cell1.Alignment = CellAlignment.CENTRE;
        cell1.Style = new CellStyle {
            BackgroundColour = AnsiColour.Red,
            TextColour = AnsiColour.BrightBlue,
            IsBold = true,
            IsItalic = true,
            IsUnderlined = true
        };
        cell1.DecimalPlaces = 4;

        List<CellLocation> cellLocations = new();
        for (int row = 1; row < 10; row++) {
            cellLocations.Add(new CellLocation(1, row));
        }

        CellFiller filler = new(sheet, cellLocations);
        filler.Process();

        foreach (CellLocation location in cellLocations) {
            Cell cell = sheet.GetCell(location, false);
            Assert.AreEqual("33.3333%", cell.Text);
            Assert.AreEqual(CellFormat.PERCENT, cell.CellFormat);
            Assert.AreEqual(CellAlignment.CENTRE, cell.Alignment);
            Assert.AreEqual(AnsiColour.Red, cell.Style.BackgroundColour);
            Assert.AreEqual(AnsiColour.BrightBlue, cell.Style.TextColour);
            Assert.IsTrue(cell.Style.IsBold);
            Assert.IsTrue(cell.Style.IsItalic);
            Assert.IsTrue(cell.Style.IsUnderlined);
        }
    }
}