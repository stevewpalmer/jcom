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
using System.Linq;
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
        Book workBook = new();
        Sheet sheet = workBook.Sheets.First();
        Assert.AreEqual("Sheet1", sheet.Name);
        Assert.AreEqual("A1", sheet.ActiveCell.Address);
        Assert.AreEqual(1, Sheet.RowHeight);
    }

    // Test that a new sheet has default columns
    [Test]
    public void TestDefaultColumns() {
        Book workBook = new();
        Sheet sheet = workBook.Sheets.First();
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
        Book workBook = new();
        Sheet sheet = workBook.Sheets.First();
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
        Book workBook = new();
        Sheet sheet = workBook.Sheets.First();
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

        Sheet sheet2 = workBook.AddSheet();
        Cell cell1 = sheet2.GetCell(new CellLocation("A1"), true);
        cell1.Value = new Variant("=SUM(A3:A4)");

        Cell cell2 = sheet2.GetCell(new CellLocation("A3"), true);
        Cell cell3 = sheet2.GetCell(new CellLocation("A4"), true);
        cell2.Value = new Variant(300);
        cell3.Value = new Variant(700);
        sheet2.Calculate();
        Assert.AreEqual(new Variant(1000), cell1.Value);

        sheet2.DeleteRow(2);
        sheet2.Calculate();

        Assert.AreEqual("=SUM(A2:A3)", cell1.Content);
        Assert.AreEqual(new Variant(1000), cell1.Value);
        Assert.AreEqual(new Variant(300), cell2.Value);
        Assert.AreEqual(new Variant(700), cell3.Value);
    }

    // Add a few cells to the sheet and read them back
    [Test]
    public void TestAddingCells() {
        Book workBook = new();
        Sheet sheet = workBook.Sheets.First();
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
        Book workBook = new();
        Sheet sheet = workBook.Sheets.First();
        Cell cell1 = new(sheet);
        Assert.IsFalse(sheet.Modified);
        cell1.Alignment = CellAlignment.RIGHT;
        Assert.IsTrue(sheet.Modified);

        sheet = workBook.AddSheet();
        _ = new Cell(sheet) {
            CellFormat = CellFormat.FIXED
        };
        Assert.IsTrue(sheet.Modified);

        sheet = workBook.AddSheet();
        _ = new Cell(sheet) {
            DecimalPlaces = 4
        };
        Assert.IsTrue(sheet.Modified);

        sheet = workBook.AddSheet();
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
        Book workBook = new();
        Sheet sheet = workBook.Sheets.First();
        Assert.AreEqual("A1", sheet.ActiveCell.Address);
        sheet.Location = new CellLocation("B4");
        Assert.AreEqual("B4", sheet.ActiveCell.Address);
    }

    /// <summary>
    /// Test some simple sorting
    /// </summary>
    [Test]
    public void TestSorting() {
        Book workBook = new();
        Sheet sheet = workBook.Sheets.First();
        double[][] data = [
            [72, 86, 99],
            [12, 25, 33],
            [43, 51, 64]
        ];
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

    /// <summary>
    /// Simple test of cross-sheet references.
    /// </summary>
    [Test]
    public void TestCrossSheetReference() {
        Book workBook = new();
        Sheet sheet1 = workBook.Sheets.First();
        Sheet sheet2 = workBook.AddSheet();

        sheet1.Name = "Savings";
        sheet2.Name = "Expenses";

        Cell cell1 = sheet1.GetCell(new CellLocation("A1"), true);
        cell1.Value = new Variant("45.89");

        Cell cell2 = sheet2.GetCell(new CellLocation("A1"), true);
        cell2.Value = new Variant("=Savings!A1");

        sheet2.Calculate();
        Assert.AreEqual(new Variant(45.89), cell2.Value);

        // A non-existent sheet reference evaluates to 0.
        cell2.Value = new Variant("=NonExistentSheet!A1");

        sheet2.Calculate();
        Assert.AreEqual(new Variant(0), cell2.Value);
    }
}