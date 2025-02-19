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
using System.Globalization;
using System.Linq;
using JCalcLib;
using JComLib;
using NUnit.Framework;
using TestUtilities;

namespace CalcLibTests;

public class FunctionTests {

    /// <summary>
    /// Verify the TIME function
    /// </summary>
    [Test]
    public void VerifyTime() {
        Book workBook = new();
        Sheet sheet = workBook.Sheets.First();
        Cell cell1 = sheet.GetCell(new CellLocation("A1"), true);
        cell1.Content = "=TIME(12,40,30)";
        cell1.CellFormat = CellFormat.TIME_HMS;
        sheet.Calculate();
        TimeOnly time = TimeOnly.Parse(cell1.TextForWidth(12));
        Assert.AreEqual(new TimeOnly(12, 40, 30), time);

        // Incorrect number of arguments to TIME()
        Assert.Throws<FormatException>(delegate { _ = new FormulaParser("=TIME(12)", cell1.Location).Parse(); });
    }

    /// <summary>
    /// Verify the LEN function
    /// </summary>
    [Test]
    public void VerifyLen() {
        Book workBook = new();
        Sheet sheet = workBook.Sheets.First();
        Cell cell1 = sheet.GetCell(new CellLocation("A1"), true);
        Cell cell2 = sheet.GetCell(new CellLocation("A2"), true);
        Cell cell3 = sheet.GetCell(new CellLocation("A3"), true);
        cell1.Content = "Edge of Darkness";
        cell2.Content = "=LEN(A1)";
        cell3.Content = "=LEN(CONCATENATE('HELLO',' ','WORLD'))";
        sheet.Calculate();
        Assert.AreEqual(new Variant(11), cell3.Value);
    }

    /// <summary>
    /// Verify the DATE function
    /// </summary>
    [Test]
    public void VerifyDate() {
        Book workBook = new();
        Sheet sheet = workBook.Sheets.First();
        Cell cell1 = sheet.GetCell(new CellLocation("A1"), true);
        cell1.Content = "=DATE(2024,2,17)";
        cell1.CellFormat = CellFormat.DATE_DMY;
        sheet.Calculate();
        DateTime time = DateTime.Parse(cell1.TextForWidth(12));
        Assert.AreEqual(new DateTime(2024, 2, 17), time);

        cell1.Content = "=DATE(12)";
        cell1.CellFormat = CellFormat.DATE_DMY;
        sheet.Calculate();
        Assert.IsTrue(cell1.Error);
    }

    /// <summary>
    /// Verify the EDATE function
    /// </summary>
    [Test]
    public void VerifyEDate() {
        Book workBook = new();
        Sheet sheet = workBook.Sheets.First();
        Cell cell1 = sheet.GetCell(new CellLocation("A1"), true);
        Cell cell2 = sheet.GetCell(new CellLocation("A2"), true);
        cell1.Content = "=DATE(2024,1,15)";
        cell1.CellFormat = CellFormat.DATE_DMY;
        cell2.CellFormat = CellFormat.DATE_DMY;

        cell2.Content = "=EDATE(A1,1)";
        sheet.Calculate();
        DateTime time = DateTime.Parse(cell2.Text);
        Assert.AreEqual(new DateTime(2024, 2, 15), time);

        cell2.Content = "=EDATE(A1,-1)";
        sheet.Calculate();
        time = DateTime.Parse(cell2.Text);
        Assert.AreEqual(new DateTime(2023, 12, 15), time);

        cell2.Content = "=EDATE(A1,2)";
        sheet.Calculate();
        time = DateTime.Parse(cell2.Text);
        Assert.AreEqual(new DateTime(2024, 3, 15), time);

        cell2.Content = "=EDATE(99999999,2)";
        sheet.Calculate();
        Assert.IsTrue(cell2.Error);
    }

    /// <summary>
    /// Verify the DAYS360 function
    /// </summary>
    [Test]
    public void VerifyDays360() {
        Book workBook = new();
        Sheet sheet = workBook.Sheets.First();
        Cell cell1 = sheet.GetCell(new CellLocation("A1"), true);
        Cell cell2 = sheet.GetCell(new CellLocation("A2"), true);
        Cell cell3 = sheet.GetCell(new CellLocation("A3"), true);
        Cell cell4 = sheet.GetCell(new CellLocation("A4"), true);
        Cell cell5 = sheet.GetCell(new CellLocation("A5"), true);
        Cell cell6 = sheet.GetCell(new CellLocation("A6"), true);
        cell1.Content = "=DATE(2011,1,1)";
        cell2.Content = "=DATE(2011,1,30)";
        cell3.Content = "=DATE(2011,2,1)";
        cell4.Content = "=DATE(2011,12,31)";
        cell5.Content = "=DATE(2011,2,28)";

        cell6.Content = "=DAYS360(A2,A3)";
        sheet.Calculate();
        Assert.AreEqual(new Variant(1), cell6.Value);

        cell6.Content = "=DAYS360(A1,A4)";
        sheet.Calculate();
        Assert.AreEqual(new Variant(360), cell6.Value);

        cell6.Content = "=DAYS360(A1,A3)";
        sheet.Calculate();
        Assert.AreEqual(new Variant(30), cell6.Value);

        cell6.Content = "=DAYS360(A4,A5)";
        sheet.Calculate();
        Assert.AreEqual(new Variant(300), cell6.Value);

        // Make sure it works in reverse!
        cell6.Content = "=DAYS360(A3,A1)";
        sheet.Calculate();
        Assert.AreEqual(new Variant(30), cell6.Value);

        cell6.Content = "=DAYS360(9999999,9999999)";
        sheet.Calculate();
        Assert.IsTrue(cell6.Error);
    }

    /// <summary>
    /// Verify the NOW function
    /// </summary>
    [Test]
    public void VerifyNow() {
        Book workBook = new();
        Sheet sheet = workBook.Sheets.First();
        Cell cell1 = sheet.GetCell(new CellLocation("A1"), true);
        cell1.Content = "=NOW()";
        cell1.CellFormat = CellFormat.DATE_DMY;
        sheet.Calculate();
        DateOnly date = DateOnly.Parse(cell1.TextForWidth(12));
        Assert.AreEqual(DateOnly.FromDateTime(DateTime.Now), date);

        sheet = workBook.AddSheet();
        cell1 = sheet.GetCell(new CellLocation("A1"), true);
        cell1.Content = "=NOW()";
        sheet.Calculate();
        Assert.AreEqual(DateTime.Now.ToString("dd/MM/yyyy H:mm"), cell1.Text);
    }

    /// <summary>
    /// Verify the TODAY function
    /// </summary>
    [Test]
    public void VerifyToday() {
        Book workBook = new();
        Sheet sheet = workBook.Sheets.First();
        Cell cell1 = sheet.GetCell(new CellLocation("A1"), true);
        cell1.Content = "=TODAY()";
        cell1.CellFormat = CellFormat.DATE_DMY;
        sheet.Calculate();
        DateOnly date = DateOnly.Parse(cell1.TextForWidth(12));
        Assert.AreEqual(DateOnly.FromDateTime(DateTime.Now), date);

        cell1.Content = "=YEAR(TODAY())";
        cell1.CellFormat = CellFormat.DATE_DMY;
        sheet.Calculate();
        Assert.AreEqual(new Variant(DateTime.Now.Year), cell1.Value);

        cell1.Content = "=YEAR(99999999)";
        cell1.CellFormat = CellFormat.DATE_DMY;
        sheet.Calculate();
        Assert.IsTrue(cell1.Error);

        cell1.Content = "=MONTH(TODAY())";
        cell1.CellFormat = CellFormat.DATE_DMY;
        sheet.Calculate();
        Assert.AreEqual(new Variant(DateTime.Now.Month), cell1.Value);
        Assert.AreEqual("=MONTH(TODAY())", cell1.Content);

        cell1.Content = "=MONTH(99999999)";
        cell1.CellFormat = CellFormat.DATE_DMY;
        sheet.Calculate();
        Assert.IsTrue(cell1.Error);

        cell1.Content = "=MONTH(TODAY())&YEAR(TODAY())";
        cell1.CellFormat = CellFormat.GENERAL;
        sheet.Calculate();
        Assert.AreEqual($"{DateTime.Now.Month}{DateTime.Now.Year}", cell1.Value.StringValue);
        Assert.AreEqual("=MONTH(TODAY())&YEAR(TODAY())", cell1.Content);
    }

    /// <summary>
    /// Verify the SUM function
    /// </summary>
    [Test]
    public void VerifySum() {
        Book workBook = new();
        Sheet sheet = workBook.Sheets.First();
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
        sheet.Calculate();
        Assert.AreEqual(new Variant("146"), cell5.Value);

        // Multiple ranges sum
        cell4.Content = "-7";
        cell5.Content = "=SUM(A1:A2,A3:A4)";
        sheet.Calculate();
        Assert.AreEqual(new Variant(139), cell5.Value);

        // Sum literals
        cell5.Content = "=SUM(40,50,60,70,80)";
        sheet.Calculate();
        Assert.AreEqual(new Variant(300), cell5.Value);

        // Sum with expressions
        cell5.Content = "=SUM(3*4,9/3)";
        sheet.Calculate();
        Assert.IsTrue(cell5.Value == 15);

        // Sum with strings and mixed expression types
        cell5.Content = "=SUM(A1,A2:A4,90,\"Hello\",4*6)";
        sheet.Calculate();
        Assert.IsTrue(cell5.Value == 253);

        // Empty SUM is an error
        Assert.Throws<FormatException>(delegate { _ = new FormulaParser("=SUM()", cell5.Location).Parse(); });
    }

    /// <summary>
    /// Verify the AVG function
    /// </summary>
    [Test]
    public void VerifyAverage() {
        Book workBook = new();
        Sheet sheet = workBook.Sheets.First();
        Cell cell1 = sheet.GetCell(new CellLocation("A1"), true);
        Cell cell2 = sheet.GetCell(new CellLocation("A2"), true);
        Cell cell3 = sheet.GetCell(new CellLocation("A3"), true);
        Cell cell4 = sheet.GetCell(new CellLocation("A4"), true);
        Cell cell5 = sheet.GetCell(new CellLocation("A5"), true);

        // Simple range average
        cell1.Content = "56";
        cell2.Content = "78";
        cell3.Content = "13";
        cell5.Content = "=AVG(A1:A3)";
        sheet.Calculate();
        Assert.AreEqual(new Variant("49"), cell5.Value);

        // Multiple ranges average
        cell4.Content = "-7";
        cell5.Content = "=AVG(A1:A2,A3:A4)";
        sheet.Calculate();
        Assert.AreEqual(new Variant(35), cell5.Value);

        // Average literals
        cell5.Content = "=AVG(40,50,60,70,80)";
        sheet.Calculate();
        Assert.AreEqual(new Variant(60), cell5.Value);

        // Average with expressions
        cell5.Content = "=AVG(3*4,9/3)";
        sheet.Calculate();
        Assert.IsTrue(cell5.Value == 7.5);

        // Average with strings and mixed expression types
        cell5.Content = "=AVG(A1,A2:A4,90,\"Hello\",4*6)";
        sheet.Calculate();
        Assert.IsTrue(Helper.DoubleCompare(cell5.Value.DoubleValue, 36.285714285714285));

        // Empty AVG is an error
        Assert.Throws<FormatException>(delegate { _ = new FormulaParser("=AVG()", cell5.Location).Parse(); });
    }

    /// <summary>
    /// Verify the CONCATENATE function
    /// </summary>
    [Test]
    public void VerifyConcatenate() {
        Book workBook = new();
        Sheet sheet = workBook.Sheets.First();
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
        sheet.Calculate();
        Assert.AreEqual(new Variant("Happy New Year"), cell5.Value);

        // Multiple ranges concatenation
        cell4.Content = " Everyone!";
        cell5.Content = "=CONCATENATE(A1:A2,A3:A4)";
        sheet.Calculate();
        Assert.AreEqual(new Variant("Happy New Year Everyone!"), cell5.Value);

        // Concatenate operator
        cell5.Content = "=A1&A2&A3";
        sheet.Calculate();
        Assert.AreEqual(new Variant("Happy New Year"), cell5.Value);

        // Concatenate operator
        cell5.Content = "='Foo'&'Bar'";
        sheet.Calculate();
        Assert.AreEqual(new Variant("FooBar"), cell5.Value);

        // Empty cells are skipped
        sheet.DeleteCell(cell2);
        sheet.DeleteCell(cell3);
        cell5.Content = "=CONCATENATE(A1:A2,A3:A4)";
        sheet.Calculate();
        Assert.AreEqual(new Variant("Happy Everyone!"), cell5.Value);

        // Concatenate literals
        cell5.Content = "=CONCATENATE(\"All\",\" applaud\", \" the\", \" NHS!\")";
        sheet.Calculate();
        Assert.AreEqual(new Variant("All applaud the NHS!"), cell5.Value);

        // Concatenate mixed types
        cell5.Content = "=CONCATENATE(\"The answer is \",3*4,\", right?\")";
        sheet.Calculate();
        Assert.AreEqual(new Variant("The answer is 12, right?"), cell5.Value);

        // Empty concatenation is an error
        Assert.Throws<FormatException>(delegate { _ = new FormulaParser("=CONCATENATE()", cell5.Location).Parse(); });
    }

    /// <summary>
    /// Verify the TEXT function
    /// </summary>
    [Test]
    public void VerifyTextFunction() {
        Book workBook = new();
        Sheet sheet = workBook.Sheets.First();
        Cell cell1 = sheet.GetCell(new CellLocation("A1"), true);
        Cell cell2 = sheet.GetCell(new CellLocation("A2"), true);
        Cell cell3 = sheet.GetCell(new CellLocation("A3"), true);
        Cell cell4 = sheet.GetCell(new CellLocation("A4"), true);
        Cell cell5 = sheet.GetCell(new CellLocation("A5"), true);

        string currencyChar = NumberFormatInfo.CurrentInfo.CurrencySymbol;

        cell1.Value = new Variant("Total comes to ");
        cell2.Value = new Variant(45.78);
        cell3.Value = new Variant(3.90);
        cell4.Value = new Variant("=SUM(A2:A3)");
        cell5.Value = new Variant($"=A1&TEXT(A4, '{currencyChar}#,##0.00')");
        sheet.Calculate();

        Assert.AreEqual($"Total comes to {currencyChar}49.68", cell5.Value.StringValue);

        // Verify longer dependency chain
        CellLocation [] dependents = workBook.Dependents(cell5.LocationWithSheet).ToArray();
        Assert.AreEqual(4, dependents.Length);
        Assert.AreEqual(cell1.LocationWithSheet, dependents[0]);
        Assert.AreEqual(cell4.LocationWithSheet, dependents[1]);
        Assert.AreEqual(cell2.LocationWithSheet, dependents[2]);
        Assert.AreEqual(cell3.LocationWithSheet, dependents[3]);

        cell1.Value = new Variant("Tomorrow will be ");
        cell2.Value = new Variant("10 June 2024");
        cell3.Value = new Variant("=A1&TEXT(A2, 'dd-mmm-yyyy')");
        sheet.Calculate();

        Assert.AreEqual("Tomorrow will be 10-Jun-2024", cell3.Value.StringValue);
    }
}