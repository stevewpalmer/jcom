// JCalcLib
// Unit tests for the Cell class
//
// Authors:
//  Steve Palmer
//
// Copyright (C) 2021 Steve Palmer
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
// under the License

using System;
using System.Drawing;
using JCalcLib;
using JComLib;
using NUnit.Framework;
using TestUtilities;

namespace CalcLibTests;

[TestFixture]
public class CellTests {

    // Create a cell and verify properties.
    [Test]
    public void VerifyDefaultProperties() {
        Cell cell = new();
        Assert.AreEqual(1, cell.Location.Row);
        Assert.AreEqual(1, cell.Location.Column);
        Assert.AreEqual(CellAlignment.GENERAL, cell.Alignment);
        Assert.AreEqual(CellFormat.GENERAL, cell.CellFormat);
        Assert.IsTrue(cell.IsEmptyCell);
    }

    // Verify that we correctly translate positions to cell locations
    [Test]
    public void VerifyRowAndColumnFromPositions() {
        CellLocation a1 = new("A1");
        Assert.IsNull(a1.SheetName);
        Assert.AreEqual(1, a1.Row);
        Assert.AreEqual(1, a1.Column);

        CellLocation z1 = new("Z1");
        Assert.IsNull(z1.SheetName);
        Assert.AreEqual(1, z1.Row);
        Assert.AreEqual(26, z1.Column);

        CellLocation iu1 = new("IU1");
        Assert.IsNull(iu1.SheetName);
        Assert.AreEqual(1, iu1.Row);
        Assert.AreEqual(255, iu1.Column);

        CellLocation a4095 = new("A4095");
        Assert.IsNull(a4095.SheetName);
        Assert.AreEqual(4095, a4095.Row);
        Assert.AreEqual(1, a4095.Column);

        CellLocation sheet1a4095 = new("Sheet1:A4095");
        Assert.AreEqual("Sheet1", sheet1a4095.SheetName);
        Assert.AreEqual(4095, sheet1a4095.Row);
        Assert.AreEqual(1, sheet1a4095.Column);

        Assert.Throws(typeof(FormatException), delegate { _ = new CellLocation(null!); });
        Assert.Throws<FormatException>(delegate { _ = new CellLocation(null!); });
    }

    // Verify comparing two cell locations. This exercises the equality
    // operations on CellLocation.
    [Test]
    public void VerifyCompareCellLocations() {
        CellLocation a1 = new("A1");
        CellLocation a2 = new() { Row = 1, Column = 1 };
        Assert.AreEqual(a1, a2);
        Assert.IsTrue(a1 == a2);
        Assert.IsFalse(a1 != a2);
        Assert.IsTrue(a1.Equals((object)a2));
        Assert.AreEqual(a1.GetHashCode(), a2.GetHashCode());
    }

    // Verify the correct address is returned for a cell
    [Test]
    public void VerifyAddressForColumnAndRow() {
        Assert.AreEqual("A1", new Cell().Address);
        Assert.AreEqual("Z1", new Cell { Location = new CellLocation { Column = 26, Row = 1 } }.Address);
        Assert.AreEqual("IU4095", new Cell { Location = new CellLocation { Column = 255, Row = 4095 } }.Address);
        Assert.AreEqual(1, Cell.AddressToColumn(new Cell().Address));
        Assert.AreEqual(26, Cell.AddressToColumn(new Cell { Location = new CellLocation { Column = 26, Row = 1 } }.Address));
        Assert.AreEqual(255, Cell.AddressToColumn(new Cell { Location = new CellLocation { Column = 255, Row = 4095 } }.Address));
    }

    // Verify that setting a cell value to a date string converts it
    // to the correct OADate representation.
    [Test]
    public void VerifyTryParseDate() {
        Assert.AreEqual(new Variant(45387), new Cell { Content = "5 Apr" }.Value);
        Assert.AreEqual(new Variant(45017), new Cell { Content = "Apr 2023" }.Value);
        Assert.AreEqual(new Variant(45069), new Cell { Content = "23 May 2023" }.Value);
        Assert.AreEqual(new Variant(45387), new Cell { Content = "5  Apr" }.Value);
        Assert.AreEqual(new Variant("12-XYZ"), new Cell { Content = "12-XYZ" }.Value);
    }

    // Verify that setting a cell value to a time string converts it
    // to the correct OADate representation.
    [Test]
    public void VerifyTryParseTime() {
        double datePart = DateTime.Today.ToOADate();
        Assert.AreEqual(new Variant(datePart + 0.520833333336), new Cell { Content = "12:30" }.Value);
        Assert.AreEqual(new Variant(datePart + 0), new Cell { Content = "00:00:00" }.Value);
        Assert.AreEqual(new Variant(datePart + 0.99998842592), new Cell { Content = "23:59:59" }.Value);
        Assert.AreEqual(new Variant(datePart + 0.30039351852), new Cell { Content = "7:12:34 AM" }.Value);
        Assert.AreEqual(new Variant("Seven o'clock"), new Cell { Content = "Seven o'clock" }.Value);
    }

    // Verify the Location property
    [Test]
    public void VerifyLocation() {
        Assert.AreEqual(new Point(1, 1), new Cell().Location.Point);
        Assert.AreEqual(new Point(12, 12), new Cell { Location = new CellLocation { Column = 12, Row = 12 } }.Location.Point);
    }

    // Verify that cell content and value match
    [Test]
    public void VerifyCellContent() {
        Cell number15 = new() { Content = "15" };
        Cell text = new() { Content = "TEXT" };
        Assert.AreEqual(number15.Value, new Variant(15));
        Assert.AreEqual(number15.Content, "15");
        Assert.IsTrue(number15.Value.IsNumber);
        Assert.AreEqual(text.Value, new Variant("TEXT"));
        Assert.AreEqual(text.Content, "TEXT");
        Assert.IsTrue(new Cell(new Sheet(1)) { Content = "=A1+B2" }.HasFormula);

        Cell date = new() { Content = "4 June 1980" };
        Assert.AreEqual("4-June-1980", date.Text);

        Cell time = new() { Content = "7:30 pm" };
        Assert.AreEqual("7:30 PM", time.Text);

        Cell empty = new();
        Assert.AreEqual("", empty.Content);
    }

    // Verify the comparison of two cell values
    [Test]
    public void VerifyCompareTo() {
        Assert.IsTrue(new Cell { Content = "15" }.Value > new Cell { Content = "12" }.Value);
        Assert.IsTrue(new Cell { Content = "8" }.Value < new Cell { Content = "12" }.Value);
        Assert.IsTrue(new Cell { Content = "HELLO" }.Value > new Cell { Content = "CHAIN" }.Value);
        Assert.IsTrue(new Cell { Content = "APPLE" }.Value < new Cell { Content = "orange" }.Value);
        Assert.IsTrue(new Cell().Value.CompareTo(new Cell { Content = "12" }.Value) == 0);
        Assert.IsTrue(new Cell { Content = "APPLE" }.Value.CompareTo(null) == 1);
    }

    // Verify swapping two cells.
    [Test]
    public void VerifySwapCell() {
        Cell cell1 = new() {
            Content = "45.8794",
            Location = new CellLocation { Column = 3, Row = 8 },
            CellFormat = CellFormat.GENERAL,
            DecimalPlaces = 3,
            Alignment = CellAlignment.CENTRE
        };
        Cell cell2 = new() {
            Content = "67.9",
            Location = new CellLocation { Column = 1, Row = 17 },
            CellFormat = CellFormat.PERCENT,
            DecimalPlaces = 1,
            Alignment = CellAlignment.RIGHT
        };
        cell1.Swap(cell2);
        Assert.AreEqual(cell1.Value, new Variant("67.9"));
        Assert.AreEqual(cell1.Location.Column, 3);
        Assert.AreEqual(cell1.Location.Row, 8);
        Assert.AreEqual(cell1.CellFormat, CellFormat.PERCENT);
        Assert.AreEqual(cell1.Decimal, 1);
        Assert.AreEqual(cell1.Alignment, CellAlignment.RIGHT);
    }

    // Verify the CreateFrom method
    [Test]
    public void VerifyCellCopyConstructor() {
        Cell cell = new() {
            Content = "67.9",
            Location = new CellLocation { Column = 1, Row = 17 },
            CellFormat = CellFormat.PERCENT,
            DecimalPlaces = 1,
            Alignment = CellAlignment.RIGHT
        };

        Sheet sheet = new();

        Cell newCell = new(sheet, cell);
        Assert.AreEqual(17, newCell.Location.Row);
        Assert.AreEqual(1, newCell.Location.Column);
        Assert.AreEqual(CellAlignment.RIGHT, newCell.Alignment);
        Assert.AreEqual(CellFormat.PERCENT, newCell.CellFormat);
        Assert.IsFalse(newCell.IsEmptyCell);
        Assert.AreEqual("67.9", newCell.Content);

        // Verify that there's no reference back to the original cell
        cell.Content = "967.84";
        cell.Location = new CellLocation { Column = 9, Row = 9 };
        Assert.AreEqual("67.9", newCell.Content);
        Assert.AreEqual("A17", newCell.Location.Address);
    }

    // Verify the format description for the cell
    [Test]
    public void VerifyCellFormatDescription() {
        Assert.AreEqual("(G)", new Cell {
            Content = "45.8794",
            CellFormat = CellFormat.GENERAL
        }.FormatDescription);
        Assert.AreEqual("(R)", new Cell {
            Content = "45.8794",
            CellFormat = CellFormat.TEXT
        }.FormatDescription);
        Assert.AreEqual("(F3)", new Cell {
            Content = "45.8794",
            CellFormat = CellFormat.FIXED,
            DecimalPlaces = 3
        }.FormatDescription);
        Assert.AreEqual("(S1)", new Cell {
            Content = "45.8794",
            CellFormat = CellFormat.SCIENTIFIC,
            DecimalPlaces = 1
        }.FormatDescription);
        Assert.AreEqual("(P2)", new Cell {
            Content = "45.8794",
            CellFormat = CellFormat.PERCENT,
            DecimalPlaces = 2
        }.FormatDescription);
        Assert.AreEqual("(C4)", new Cell {
            Content = "45.8794",
            CellFormat = CellFormat.CURRENCY,
            DecimalPlaces = 4
        }.FormatDescription);
        Assert.AreEqual("(D1)", new Cell {
            Content = "48794",
            CellFormat = CellFormat.DATE_DMY
        }.FormatDescription);
        Assert.AreEqual("(D2)", new Cell {
            Content = "48794",
            CellFormat = CellFormat.DATE_DM
        }.FormatDescription);
        Assert.AreEqual("(D3)", new Cell {
            Content = "48794",
            CellFormat = CellFormat.DATE_MY
        }.FormatDescription);
        Assert.AreEqual("(T1)", new Cell {
            Content = "48794",
            CellFormat = CellFormat.TIME_HMSZ
        }.FormatDescription);
        Assert.AreEqual("(T2)", new Cell {
            Content = "48794",
            CellFormat = CellFormat.TIME_HM
        }.FormatDescription);
        Assert.AreEqual("(T3)", new Cell {
            Content = "48794",
            CellFormat = CellFormat.TIME_HMS
        }.FormatDescription);
        Assert.AreEqual("(T4)", new Cell {
            Content = "48794",
            CellFormat = CellFormat.TIME_HMZ
        }.FormatDescription);
        Assert.AreEqual("(#,##0)", new Cell {
            Content = "45.8794",
            CellFormat = CellFormat.CUSTOM,
            CustomFormatString = "#,##0",
            DecimalPlaces = 2
        }.FormatDescription);
        Assert.AreEqual("(14)", new Cell {
            Content = "45.8794",
            CellFormat = (CellFormat)14,
            DecimalPlaces = 1
        }.FormatDescription);
    }

    // Verify general alignment
    [Test]
    public void VerifyGeneralAlignment() {
        Assert.AreEqual(" 45.8794", new Cell {
            Content = "45.8794",
            Alignment = CellAlignment.GENERAL,
            CellFormat = CellFormat.GENERAL,
            DecimalPlaces = 2
        }.TextForWidth(8));
        Assert.AreEqual("Text    ", new Cell {
            Content = "Text",
            Alignment = CellAlignment.GENERAL,
            CellFormat = CellFormat.GENERAL,
            DecimalPlaces = 2
        }.TextForWidth(8));
        Assert.AreEqual(" 45.8794", new Cell {
            Content = "45.8794",
            Alignment = CellAlignment.RIGHT,
            CellFormat = CellFormat.GENERAL,
            DecimalPlaces = 2
        }.TextForWidth(8));
        Assert.AreEqual("        ", new Cell {
            Content = "",
            Alignment = CellAlignment.GENERAL,
            CellFormat = CellFormat.GENERAL,
            DecimalPlaces = 2
        }.TextForWidth(8));
        Assert.Throws<ArgumentException>(delegate {
            new Cell {
                Content = "45.8794",
                CellFormat = (CellFormat)14,
                DecimalPlaces = 2
            }.TextForWidth(8);
        });
    }

    // Verify the TEXT format
    [Test]
    public void VerifyTextFormat() {
        Assert.AreEqual("45.8794 ", new Cell {
            Content = "45.8794",
            Alignment = CellAlignment.LEFT,
            CellFormat = CellFormat.TEXT,
            DecimalPlaces = 2
        }.TextForWidth(8));
        Assert.AreEqual("HELLO WO", new Cell {
            Content = "HELLO WORLD!",
            Alignment = CellAlignment.LEFT,
            CellFormat = CellFormat.TEXT
        }.TextForWidth(8));
        Assert.Catch<ArgumentException>(delegate {
            NumberFormats.GetFormat(CellFormat.TEXT, false, 5);
        });
    }

    // Verify alignments
    [Test]
    public void VerifyAlignments() {
        Assert.AreEqual("45.8794 ", new Cell {
            Content = "45.8794",
            Alignment = CellAlignment.LEFT,
            CellFormat = CellFormat.GENERAL,
            DecimalPlaces = 2
        }.TextForWidth(8));
        Assert.AreEqual("45.8794", new Cell {
            Content = "45.8794",
            Alignment = CellAlignment.RIGHT,
            CellFormat = CellFormat.GENERAL,
            DecimalPlaces = 2
        }.Text);
        Assert.AreEqual("45.8794 ", new Cell {
            Content = "45.8794",
            Alignment = CellAlignment.CENTRE,
            CellFormat = CellFormat.GENERAL,
            DecimalPlaces = 2
        }.TextForWidth(8));
        Assert.Throws<ArgumentException>(delegate {
            new Cell {
                Content = "45.8794",
                Alignment = (CellAlignment)4,
                CellFormat = CellFormat.GENERAL,
                DecimalPlaces = 2
            }.TextForWidth(8);
        });
    }

    // Test custom number formats
    [Test]
    public void VerifyCustomFormat() {
        Assert.AreEqual("(8,745.88)", new Cell {
            Content = "-8745.8794",
            CellFormat = CellFormat.CUSTOM,
            CustomFormatString = "#,##0.00_);(#,##0.00)",
            DecimalPlaces = 2
        }.TextForWidth(10));
    }

    // Verify fixed format formatting
    [Test]
    public void VerifyFixedFormat() {
        Assert.AreEqual(" 45.88", new Cell {
            Content = "45.8794",
            CellFormat = CellFormat.FIXED,
            DecimalPlaces = 2
        }.TextForWidth(6));
        Assert.AreEqual("45.88", new Cell {
            Content = "45.8794",
            CellFormat = CellFormat.FIXED,
            Alignment = CellAlignment.LEFT,
            DecimalPlaces = 2
        }.Text);
        Assert.AreEqual("  45.88  ", new Cell {
            Content = "45.8794",
            CellFormat = CellFormat.FIXED,
            Alignment = CellAlignment.CENTRE,
            DecimalPlaces = 2
        }.TextForWidth(9));
        Assert.AreEqual("45.879", new Cell {
            Content = "45.8794",
            CellFormat = CellFormat.FIXED,
            DecimalPlaces = 3
        }.TextForWidth(6));
        Assert.AreEqual("******", new Cell {
            Content = "45.8794",
            CellFormat = CellFormat.FIXED,
            DecimalPlaces = 4
        }.TextForWidth(6));
        Assert.AreEqual("   46.", new Cell {
            Content = "45.8794",
            CellFormat = CellFormat.FIXED,
            DecimalPlaces = 0
        }.TextForWidth(6));
        Assert.AreEqual("", new Cell {
            Content = "45.8794",
            CellFormat = CellFormat.FIXED,
            DecimalPlaces = 0
        }.TextForWidth(0));
    }

    // Verify scientific format formatting
    [Test]
    public void VerifyScientificFormat() {
        Assert.AreEqual("  4.59E+01", new Cell {
            Content = "45.8794",
            CellFormat = CellFormat.SCIENTIFIC,
            DecimalPlaces = 2
        }.TextForWidth(10));
        Assert.AreEqual("4.59E+01    ", new Cell {
            Content = "45.8794",
            CellFormat = CellFormat.SCIENTIFIC,
            Alignment = CellAlignment.LEFT,
            DecimalPlaces = 2
        }.TextForWidth(12));
        Assert.AreEqual("  4.59E+01  ", new Cell {
            Content = "45.8794",
            CellFormat = CellFormat.SCIENTIFIC,
            Alignment = CellAlignment.CENTRE,
            DecimalPlaces = 2
        }.TextForWidth(12));
        Assert.AreEqual("   4.588E+01", new Cell {
            Content = "45.8794",
            CellFormat = CellFormat.SCIENTIFIC,
            DecimalPlaces = 3
        }.TextForWidth(12));
        Assert.AreEqual("******", new Cell {
            Content = "45.8794",
            CellFormat = CellFormat.SCIENTIFIC,
            DecimalPlaces = 4
        }.TextForWidth(6));
        Assert.AreEqual("5.E+01", new Cell {
            Content = "45.8794",
            CellFormat = CellFormat.SCIENTIFIC,
            DecimalPlaces = 0
        }.TextForWidth(6));
        Assert.AreEqual("", new Cell {
            Content = "45.8794",
            CellFormat = CellFormat.SCIENTIFIC,
            DecimalPlaces = 0
        }.TextForWidth(0));
    }

    // Verify currency format formatting
    [Test]
    public void VerifyCurrencyFormat() {
        Assert.AreEqual("  £1,234,567.00", new Cell {
            Content = "1234567",
            CellFormat = CellFormat.CURRENCY
        }.TextForWidth(15));
        Assert.AreEqual("-£7,655.00  ", new Cell {
            Content = "-7655",
            DecimalPlaces = 2,
            Alignment = CellAlignment.LEFT,
            CellFormat = CellFormat.CURRENCY
        }.TextForWidth(12));
    }

    // Verify percent format formatting
    [Test]
    public void VerifyPercentFormat() {
        Assert.AreEqual("       50.00%", new Cell {
            Content = "0.5",
            CellFormat = CellFormat.PERCENT
        }.TextForWidth(13));
        Assert.AreEqual("50.000%   ", new Cell {
            Content = "0.5",
            Alignment = CellAlignment.LEFT,
            DecimalPlaces = 3,
            CellFormat = CellFormat.PERCENT
        }.TextForWidth(10));
        Assert.AreEqual("  20.%   ", new Cell {
            Content = "0.2",
            Alignment = CellAlignment.CENTRE,
            DecimalPlaces = 0,
            CellFormat = CellFormat.PERCENT
        }.TextForWidth(9));
        Assert.AreEqual(4, new Cell {
            Content = "0.2",
            Alignment = CellAlignment.CENTRE,
            DecimalPlaces = 0,
            CellFormat = CellFormat.PERCENT
        }.Width);
        Assert.AreEqual(" 456700.00% ", new Cell {
            Content = "4567",
            Alignment = CellAlignment.CENTRE,
            DecimalPlaces = 2,
            CellFormat = CellFormat.PERCENT
        }.TextForWidth(12));
        Assert.AreEqual("**", new Cell {
            Content = "0.2",
            CellFormat = CellFormat.PERCENT
        }.TextForWidth(2));
    }

    // Verify the date formatting
    [Test]
    public void VerifyDateFormats() {
        Assert.AreEqual("    28-Dec", new Cell {
            Content = "657435",
            CellFormat = CellFormat.DATE_DM
        }.TextForWidth(10));
        Assert.AreEqual("28-Dec    ", new Cell {
            Content = "657435",
            CellFormat = CellFormat.DATE_DM,
            Alignment = CellAlignment.LEFT
        }.TextForWidth(10));
        Assert.AreEqual("28-Dec-3699 ", new Cell {
            Content = "657435",
            CellFormat = CellFormat.DATE_DMY,
            Alignment = CellAlignment.LEFT
        }.TextForWidth(12));
        Assert.AreEqual("  Dec-3699  ", new Cell {
            Content = "657435",
            CellFormat = CellFormat.DATE_MY,
            Alignment = CellAlignment.CENTRE
        }.TextForWidth(12));
        Assert.AreEqual("NOT A DATE", new Cell {
            Content = "NOT A DATE",
            CellFormat = CellFormat.DATE_DM
        }.TextForWidth(10));
    }

    // Verify the time formatting
    [Test]
    public void VerifyTimeFormats() {
        Assert.AreEqual(" 12:30:00 PM", new Cell {
            Content = "45619.520833333336",
            CellFormat = CellFormat.TIME_HMSZ
        }.TextForWidth(12));
        Assert.AreEqual(" 12:00:00 AM", new Cell {
            Content = "45619",
            CellFormat = CellFormat.TIME_HMSZ
        }.TextForWidth(12));
        Assert.AreEqual("     2:30 PM", new Cell {
            Content = "0.604305556",
            CellFormat = CellFormat.TIME_HMZ
        }.TextForWidth(12));
        Assert.AreEqual("     14:30", new Cell {
            Content = "0.604305556",
            CellFormat = CellFormat.TIME_HM
        }.TextForWidth(10));
        Assert.AreEqual(" 11:59:59 PM", new Cell {
            Content = "45619.99998842592",
            CellFormat = CellFormat.TIME_HMSZ
        }.TextForWidth(12));
        Assert.AreEqual("  7:12:34 AM", new Cell {
            Content = "45619.30039351852",
            CellFormat = CellFormat.TIME_HMSZ
        }.TextForWidth(12));
        Assert.AreEqual("   7:12:34", new Cell {
            Content = "45619.30039351852",
            CellFormat = CellFormat.TIME_HMS
        }.TextForWidth(10));
    }

    // Style a cell and ensure correct render string
    [Test]
    public void TestCellStyle() {
        Sheet sheet1 = new(1);
        Cell cell1 = new(sheet1) {
            Content = "HELLO WORLD",
            Alignment = CellAlignment.RIGHT,
            Style = new CellStyle(sheet1) {
                TextColour = AnsiColour.Cyan,
                BackgroundColour = AnsiColour.Green,
                IsBold = true
            }
        };
        Assert.AreEqual(@"[36;42m    [0m[36;42m[1mHELLO WORLD[0m", cell1.AnsiTextSpan(15).EscapedText);

        Cell cell2 = new(sheet1) {
            Content = "HELLO WORLD",
            Alignment = CellAlignment.LEFT,
            Style = new CellStyle(sheet1) {
                IsItalic = true,
                IsUnderlined = true
            }
        };
        Assert.AreEqual(@"[97;40m[3m[4mHELLO WORLD[0m[97;40m    [0m", cell2.AnsiTextSpan(15).EscapedText);

        // Verify CellStyles are copied from another cell
        Sheet sheet2 = new(2);
        Cell cell3 = new(sheet2, cell2);
        Assert.AreEqual(cell3.Style.TextColour, cell2.Style.TextColour);
        Assert.AreEqual(cell3.Style.BackgroundColour, cell2.Style.BackgroundColour);
        Assert.AreEqual(cell3.Style.IsBold, cell2.Style.IsBold);
        Assert.AreEqual(cell3.Style.IsItalic, cell2.Style.IsItalic);
        Assert.AreEqual(cell3.Style.IsUnderlined, cell2.Style.IsUnderlined);

        // Make sure modifying the style on an associated cell marks the sheet
        // as modified
        cell3.Style.TextColour = AnsiColour.Cyan;
        Assert.IsTrue(sheet2.Modified);
    }

    // Test the semantics of Value and Content, with constants and
    // formula.
    [Test]
    public void TestValueAndContent() {
        Sheet sheet = new();
        Cell cellA1 = sheet.GetCell(new CellLocation(1, 1), true);
        Cell cellA2 = sheet.GetCell(new CellLocation(1, 2), true);
        Cell cellA3 = sheet.GetCell(new CellLocation(1, 3), true);

        cellA1.Content = "HELLO WORLD";
        Assert.AreEqual(new Variant("HELLO WORLD"), cellA1.Value);
        Assert.AreEqual(11, cellA1.Width);

        cellA1.Content = "14.90";
        Assert.AreEqual(new Variant(14.90), cellA1.Value);
        cellA2.Content = "67.90";
        Assert.AreEqual(new Variant(67.90), cellA2.Value);
        cellA3.Content = "=SUM(A1:A2)";

        // Expect 0 since we've set a formula but not yet run a calculation
        // so there is not yet any computed value.
        Assert.AreEqual(new Variant(0), cellA3.Value);
        Assert.AreEqual("=SUM(A1:A2)", cellA3.Content);
        Assert.IsTrue(cellA3.HasFormula);
        sheet.Calculate();

        // Now we should have a real value
        Assert.IsTrue(Helper.DoubleCompare(new Variant(82.8).DoubleValue, cellA3.Value.DoubleValue));

        // Changing the Value to something that isn't a formula should now return
        // false for the HasFormula property.
        cellA3.Value = new Variant(999.99);
        Assert.AreEqual(new Variant(999.99), cellA3.Value);
        Assert.IsFalse(cellA3.HasFormula);
    }

    // Test changing the cell factory changes the default
    // properties of a cell.
    [Test]
    public void TestCellFactory() {
        Cell cell1 = new();
        Cell cell3 = new() {
            DecimalPlaces = 4,
            Alignment = CellAlignment.CENTRE,
            UseThousandsSeparator = true,
            Style = new CellStyle {
                TextColour = AnsiColour.Blue,
                BackgroundColour = AnsiColour.BrightGreen
            }
        };
        Assert.AreEqual(2, cell1.DecimalPlaces);
        Assert.AreEqual(CellAlignment.GENERAL, cell1.Alignment);
        Assert.AreEqual(CellFormat.GENERAL, cell1.CellFormat);

        // Save cell factory
        int dp = CellFactory.DecimalPlaces;
        CellAlignment align = CellFactory.Alignment;
        int fg = CellFactory.TextColour;
        int bg = CellFactory.BackgroundColour;
        CellFormat fmt = CellFactory.Format;

        // cell1 should inherit factory defaults as no explicit change
        // to the properties were made. cell3 should retain the explicit
        // change to the properties.
        CellFactory.DecimalPlaces = 5;
        CellFactory.Alignment = CellAlignment.LEFT;
        CellFactory.TextColour = AnsiColour.BrightYellow;
        CellFactory.BackgroundColour = AnsiColour.Blue;
        CellFactory.Format = CellFormat.SCIENTIFIC;

        Assert.AreEqual(5, cell1.DecimalPlaces);
        Assert.AreEqual(CellAlignment.LEFT, cell1.Alignment);
        Assert.AreEqual(CellFormat.SCIENTIFIC, cell1.CellFormat);
        Assert.AreEqual(AnsiColour.BrightYellow, cell1.Style.TextColour);
        Assert.AreEqual(AnsiColour.Blue, cell1.Style.BackgroundColour);

        Assert.AreEqual(4, cell3.DecimalPlaces);
        Assert.AreEqual(CellAlignment.CENTRE, cell3.Alignment);
        Assert.IsTrue(cell3.UseThousandsSeparator);
        Assert.AreEqual(AnsiColour.Blue, cell3.Style.TextColour);
        Assert.AreEqual(AnsiColour.BrightGreen, cell3.Style.BackgroundColour);

        // Creating a new cell after changing the factory should pick
        // up the new factory defaults
        Cell cell2 = new();
        Assert.AreEqual(5, cell2.DecimalPlaces);
        Assert.AreEqual(CellAlignment.LEFT, cell2.Alignment);
        Assert.AreEqual(CellFormat.SCIENTIFIC, cell2.CellFormat);
        Assert.AreEqual(AnsiColour.BrightYellow, cell2.Style.TextColour);
        Assert.AreEqual(AnsiColour.Blue, cell2.Style.BackgroundColour);

        // Reset cell factory
        CellFactory.DecimalPlaces = dp;
        CellFactory.Alignment = align;
        CellFactory.TextColour = fg;
        CellFactory.BackgroundColour = bg;
        CellFactory.Format = fmt;
    }

    /// <summary>
    /// Test that retrieving the content of a date or time formatted cell retrieves the
    /// formatted date or time string.
    /// </summary>
    [Test]
    public void TestFormattedDate() {
        Sheet sheet = new(1);
        Cell cell1 = sheet.GetCell(1, 1, true);
        cell1.Content = "4 June 1966";
        cell1.CellFormat = CellFormat.DATE_DMY;
        Assert.AreEqual("04/06/1966", cell1.Content);

        cell1.Content = "3:45 pm";
        cell1.CellFormat = CellFormat.TIME_HMSZ;
        Assert.AreEqual("15:45", cell1.Content);
    }

    // Verify retrieving the AnsiText content of a cell.
    [Test]
    public void VerifySpilledTextCell() {
        Sheet sheet = new(1);
        Cell cell1 = sheet.GetCell(1, 1, true);
        cell1.Value = new Variant("This is a very long text string");
        AnsiTextSpan ansiText = sheet.GetCell(1, 1, false).AnsiTextForWidth(10, true);
        Assert.AreEqual("This is a ", ansiText.Text);

        ansiText = sheet.GetCell(2, 1, false).AnsiTextForWidth(10, true);
        Assert.AreEqual("very long ", ansiText.Text);

        ansiText = sheet.GetCell(4, 1, false).AnsiTextForWidth(10, true);
        Assert.AreEqual("g", ansiText.Text);
    }
}