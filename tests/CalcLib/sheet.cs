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

using System;
using System.Collections.Generic;
using System.Drawing;
using JCalcLib;
using JComLib;
using NUnit.Framework;

namespace CalcLibTests;

public class SheetTests {

    /// <summary>
    /// Test properties on a new sheet.
    /// </summary>
    [Test]
    public void TestNewSheet() {
        Sheet sheet = new(1);
        Assert.AreEqual(1, sheet.SheetNumber);
        Assert.AreEqual("A1", sheet.ActiveCell.Address);
        Assert.AreEqual(1, Sheet.RowHeight);
    }

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

    /// <summary>
    /// Verify deletion of random columns
    /// </summary>
    [Test]
    public void TestDeleteColumns() {
        Sheet sheet = new(1);
        Random random = new();
        List<(CellLocation, Variant)> cells = [];
        for (int i = 0; i < 10; i++) {
            int column = random.Next(1, Sheet.MaxColumns);
            int row = random.Next(1, Sheet.MaxRows);
            Cell cell = sheet.GetCell(column, row, true);
            cell.Value = new Variant(random.Next(0, 3000));
            cells.Add((cell.Location, cell.Value));
        }
        foreach ((CellLocation cellLocation, Variant _) in cells) {
            sheet.DeleteColumn(cellLocation.Column);
        }
        foreach ((CellLocation cellLocation, Variant _) in cells) {
            Cell cell = sheet.GetCell(cellLocation, false);
            Assert.IsTrue(cell.IsEmptyCell);
        }
    }

    /// <summary>
    /// Verify deletion of random rows
    /// </summary>
    [Test]
    public void TestDeleteRows() {
        Sheet sheet = new(1);
        Random random = new();
        List<(CellLocation, Variant)> cells = [];
        for (int i = 0; i < 10; i++) {
            int column = random.Next(1, Sheet.MaxColumns);
            int row = random.Next(1, Sheet.MaxRows);
            Cell cell = sheet.GetCell(column, row, true);
            cell.Value = new Variant(random.Next(0, 3000));
            cells.Add((cell.Location, cell.Value));
        }
        foreach ((CellLocation cellLocation, Variant _) in cells) {
            sheet.DeleteRow(cellLocation.Row);
        }
        foreach ((CellLocation cellLocation, Variant _) in cells) {
            Cell cell = sheet.GetCell(cellLocation, false);
            Assert.IsTrue(cell.IsEmptyCell);
        }
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

    /// <summary>
    /// Verify changing the location of the active cell.
    /// </summary>
    [Test]
    public void TestLocation() {
        Sheet sheet = new(1);
        Assert.AreEqual("A1", sheet.ActiveCell.Address);
        sheet.Location = new CellLocation("B4");
        Assert.AreEqual("B4", sheet.ActiveCell.Address);
    }

    /// <summary>
    /// Test some simple sorting
    /// </summary>
    [Test]
    public void TestSorting() {
        Sheet sheet = new(1);
        double[][] data = {
            [72, 86, 99],
            [12, 25, 33],
            [43, 51, 64]
        };
        for (int row = 1; row < data.Length; row++) {
            for (int column = 1; column < data[row].Length; column++) {
                Cell cell = sheet.GetCell(new CellLocation(column, row), true);
                cell.Value = new Variant(data[row][column]);
            }
        }
        sheet.SortCells(2, false, sheet.GetCellExtent());
        for (int row = 2; row < data.Length; row++) {
            for (int column = 1; column < data[row].Length; column++) {
                Cell cell1 = sheet.GetCell(new CellLocation(column, row), true);
                Cell cell2 = sheet.GetCell(new CellLocation(column, row - 1), true);
                Assert.IsTrue(cell1.Value >= cell2.Value);
            }
        }
    }
}