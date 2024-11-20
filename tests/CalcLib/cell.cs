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

namespace CalcLibTests;

[TestFixture]
public class CellTests {

    // Create a cell and verify properties.
    [Test]
    public void VerifyDefaultProperties() {
        Cell cell = new Cell();
        Assert.AreEqual(1, cell.Location.Row);
        Assert.AreEqual(1, cell.Location.Column);
        Assert.AreEqual(CellAlignment.GENERAL, cell.Alignment);
        Assert.AreEqual(CellFormat.GENERAL, cell.CellFormat);
        Assert.IsTrue(cell.IsEmptyCell);
    }

    // Verify that we correctly translate positions to cell locations
    [Test]
    public void VerifyRowAndColumnFromPositions() {
        CellLocation a1 = new CellLocation("A1");
        Assert.AreEqual(1, a1.Row);
        Assert.AreEqual(1, a1.Column);

        CellLocation z1 = new CellLocation("Z1");
        Assert.AreEqual(1, z1.Row);
        Assert.AreEqual(26, z1.Column);

        CellLocation iu1 = new CellLocation("IU1");
        Assert.AreEqual(1, iu1.Row);
        Assert.AreEqual(255, iu1.Column);

        CellLocation a4095 = new CellLocation("A4095");
        Assert.AreEqual(4095, a4095.Row);
        Assert.AreEqual(1, a4095.Column);

        Assert.Throws(typeof(ArgumentNullException), delegate { new CellLocation(null!); });
    }

    // Verify comparing two cell locations. This exercises the equality
    // operations on CellLocation.
    [Test]
    public void VerifyCompareCellLocations() {
        CellLocation a1 = new CellLocation("A1");
        CellLocation a2 = new CellLocation { Row = 1, Column = 1 };
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
        Assert.AreEqual("45387", new Cell { UIContent = "5-Apr" }.CellValue.Value);
        Assert.AreEqual("45017", new Cell { UIContent = "Apr-2023" }.CellValue.Value);
        Assert.AreEqual("45069", new Cell { UIContent = "23-May-2023" }.CellValue.Value);
        Assert.AreEqual("45387", new Cell { UIContent = "5 - Apr" }.CellValue.Value);
        Assert.AreEqual("12-XYZ", new Cell { UIContent = "12-XYZ" }.CellValue.Value);
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
        Cell number15 = new Cell { Content = "15" };
        Cell text = new Cell { Content = "TEXT" };
        Assert.AreEqual(number15.CellValue.Value, "15");
        Assert.AreEqual(number15.Content, "15");
        Assert.AreEqual(number15.CellValue.Type, CellType.NUMBER);
        Assert.AreEqual(text.CellValue.Value, "TEXT");
        Assert.AreEqual(text.Content, "TEXT");
        Assert.AreEqual(text.CellValue.Type, CellType.TEXT);
        Assert.AreEqual(new Cell { Content = "=A1+B2" }.CellValue.Type, CellType.FORMULA);
    }

    // Verify the comparison of two cell values
    [Test]
    public void VerifyCompareTo() {
        Assert.IsTrue(new Cell { Content = "15" }.CellValue > new Cell { Content = "12" }.CellValue);
        Assert.IsTrue(new Cell { Content = "8" }.CellValue < new Cell { Content = "12" }.CellValue);
        Assert.IsTrue(new Cell { Content = "HELLO" }.CellValue > new Cell { Content = "CHAIN" }.CellValue);
        Assert.IsTrue(new Cell { Content = "APPLE" }.CellValue < new Cell { Content = "orange" }.CellValue);
        Assert.IsTrue(new Cell().CellValue.CompareTo(new Cell { Content = "12" }.CellValue) > 0);
        Assert.IsTrue(new Cell { Content = "APPLE" }.CellValue.CompareTo(null) > 0);
    }

    // Verify swapping two cells.
    [Test]
    public void VerifySwapCell() {
        Cell cell1 = new Cell {
            CellValue = new CellValue {
                Value = "45.8794"
            },
            Location = new CellLocation { Column = 3, Row = 8 },
            Format = CellFormat.GENERAL,
            Decimal = 3,
            Align = CellAlignment.CENTRE
        };
        Cell cell2 = new Cell {
            CellValue = new CellValue {
                Value = "67.9"
            },
            Location = new CellLocation { Column = 1, Row = 17 },
            Format = CellFormat.PERCENT,
            Decimal = 1,
            Align = CellAlignment.RIGHT
        };
        cell1.Swap(cell2);
        Assert.AreEqual(cell1.CellValue.Value, "67.9");
        Assert.AreEqual(cell1.Location.Column, 3);
        Assert.AreEqual(cell1.Location.Row, 8);
        Assert.AreEqual(cell1.CellFormat, CellFormat.PERCENT);
        Assert.AreEqual(cell1.Decimal, 1);
        Assert.AreEqual(cell1.Alignment, CellAlignment.RIGHT);
    }

    // Verify the format description for the cell
    [Test]
    public void VerifyCellFormatDescription() {
        Assert.AreEqual("(G)", new Cell {
            CellValue = new CellValue {
                Value = "45.8794"
            },
            Format = CellFormat.GENERAL
        }.FormatDescription);
        Assert.AreEqual("(T)", new Cell {
            CellValue = new CellValue {
                Value = "45.8794"
            },
            Format = CellFormat.TEXT
        }.FormatDescription);
        Assert.AreEqual("(F3)", new Cell {
            CellValue = new CellValue {
                Value = "45.8794"
            },
            Format = CellFormat.FIXED,
            Decimal = 3
        }.FormatDescription);
        Assert.AreEqual("(S1)", new Cell {
            CellValue = new CellValue {
                Value = "45.8794"
            },
            Format = CellFormat.SCIENTIFIC,
            Decimal = 1
        }.FormatDescription);
        Assert.AreEqual("(P2)", new Cell {
            CellValue = new CellValue {
                Value = "45.8794"
            },
            Format = CellFormat.PERCENT,
            Decimal = 2
        }.FormatDescription);
        Assert.AreEqual("(,2)", new Cell {
            CellValue = new CellValue {
                Value = "45.8794"
            },
            Format = CellFormat.COMMAS,
            Decimal = 2
        }.FormatDescription);
        Assert.AreEqual("(C4)", new Cell {
            CellValue = new CellValue {
                Value = "45.8794"
            },
            Format = CellFormat.CURRENCY,
            Decimal = 4
        }.FormatDescription);
        Assert.AreEqual("(D1)", new Cell {
            CellValue = new CellValue {
                Value = "48794"
            },
            Format = CellFormat.DATE_DMY
        }.FormatDescription);
        Assert.AreEqual("(D2)", new Cell {
            CellValue = new CellValue {
                Value = "48794"
            },
            Format = CellFormat.DATE_DM
        }.FormatDescription);
        Assert.AreEqual("(D3)", new Cell {
            CellValue = new CellValue {
                Value = "48794"
            },
            Format = CellFormat.DATE_MY
        }.FormatDescription);
        Assert.AreEqual("(?)", new Cell {
            CellValue = new CellValue {
                Value = "45.8794"
            },
            Format = (CellFormat)11,
            Decimal = 1
        }.FormatDescription);
    }

    // Verify the cell ToString
    [Test]
    public void VerifyToString() {
        Assert.AreEqual("12", new Cell { Content = "12" }.CellValue.ToString());
        Assert.AreEqual("\"World\"", new Cell { Content = "World" }.CellValue.ToString());
    }

    // Verify general alignment
    [Test]
    public void VerifyGeneralAlignment() {
        Assert.AreEqual(" 45.8794", new Cell {
            Content = "45.8794",
            Align = CellAlignment.GENERAL,
            Format = CellFormat.GENERAL,
            Decimal = 2
        }.ToString(8));
        Assert.AreEqual("Text    ", new Cell {
            Content = "Text",
            Align = CellAlignment.GENERAL,
            Format = CellFormat.GENERAL,
            Decimal = 2
        }.ToString(8));
        Assert.AreEqual(" 45.8794", new Cell {
            Content = "45.8794",
            Align = CellAlignment.RIGHT,
            Format = CellFormat.GENERAL,
            Decimal = 2
        }.ToString(8));
        Assert.AreEqual("        ", new Cell {
            CellValue = new CellValue(),
            Align = CellAlignment.GENERAL,
            Format = CellFormat.GENERAL,
            Decimal = 2
        }.ToString(8));
        Assert.Throws(typeof(ArgumentException), delegate {
            new Cell {
                Content = "45.8794",
                Format = (CellFormat)12,
                Decimal = 2
            }.ToString(8);
        });
    }

    // Verify the TEXT format
    [Test]
    public void VerifyTextFormat() {
        Assert.AreEqual("45.8794 ", new Cell {
            CellValue = new CellValue {
                Value = "45.8794"
            },
            Align = CellAlignment.LEFT,
            Format = CellFormat.TEXT,
            Decimal = 2
        }.ToString(8));
        Assert.AreEqual("HELLO WO", new Cell {
            CellValue = new CellValue {
                Value = "HELLO WORLD!"
            },
            Align = CellAlignment.LEFT,
            Format = CellFormat.TEXT
        }.ToString(8));
    }

    // Verify alignments
    [Test]
    public void VerifyAlignments() {
        Assert.AreEqual("45.8794 ", new Cell {
            CellValue = new CellValue {
                Value = "45.8794"
            },
            Align = CellAlignment.LEFT,
            Format = CellFormat.GENERAL,
            Decimal = 2
        }.ToString(8));
        Assert.AreEqual(" 45.8794", new Cell {
            CellValue = new CellValue {
                Value = "45.8794"
            },
            Align = CellAlignment.RIGHT,
            Format = CellFormat.GENERAL,
            Decimal = 2
        }.ToString(8));
        Assert.AreEqual("45.8794 ", new Cell {
            CellValue = new CellValue {
                Value = "45.8794"
            },
            Align = CellAlignment.CENTRE,
            Format = CellFormat.GENERAL,
            Decimal = 2
        }.ToString(8));
        Assert.Throws(typeof(ArgumentException), delegate {
            new Cell {
                CellValue = new CellValue {
                    Value = "45.8794"
                },
                Align = (CellAlignment)4,
                Format = CellFormat.GENERAL,
                Decimal = 2
            }.ToString(8);
        });
    }

    // Verify fixed format formatting
    [Test]
    public void VerifyFixedFormat() {
        Assert.AreEqual(" 45.88", new Cell {
            Content = "45.8794",
            Format = CellFormat.FIXED,
            Decimal = 2
        }.ToString(6));
        Assert.AreEqual("45.88   ", new Cell {
            Content = "45.8794",
            Format = CellFormat.FIXED,
            Align = CellAlignment.LEFT,
            Decimal = 2
        }.ToString(8));
        Assert.AreEqual("  45.88  ", new Cell {
            Content = "45.8794",
            Format = CellFormat.FIXED,
            Align = CellAlignment.CENTRE,
            Decimal = 2
        }.ToString(9));
        Assert.AreEqual("45.879", new Cell {
            Content = "45.8794",
            Format = CellFormat.FIXED,
            Decimal = 3
        }.ToString(6));
        Assert.AreEqual("******", new Cell {
            Content = "45.8794",
            Format = CellFormat.FIXED,
            Decimal = 4
        }.ToString(6));
        Assert.AreEqual("    46", new Cell {
            Content = "45.8794",
            Format = CellFormat.FIXED,
            Decimal = 0
        }.ToString(6));
        Assert.AreEqual("", new Cell {
            Content = "45.8794",
            Format = CellFormat.FIXED,
            Decimal = 0
        }.ToString(0));
    }

    // Verify scientific format formatting
    [Test]
    public void VerifyScientificFormat() {
        Assert.AreEqual("  4.59E+01", new Cell {
            Content = "45.8794",
            Format = CellFormat.SCIENTIFIC,
            Decimal = 2
        }.ToString(10));
        Assert.AreEqual("4.59E+01    ", new Cell {
            Content = "45.8794",
            Format = CellFormat.SCIENTIFIC,
            Align = CellAlignment.LEFT,
            Decimal = 2
        }.ToString(12));
        Assert.AreEqual("  4.59E+01  ", new Cell {
            Content = "45.8794",
            Format = CellFormat.SCIENTIFIC,
            Align = CellAlignment.CENTRE,
            Decimal = 2
        }.ToString(12));
        Assert.AreEqual("   4.588E+01", new Cell {
            Content = "45.8794",
            Format = CellFormat.SCIENTIFIC,
            Decimal = 3
        }.ToString(12));
        Assert.AreEqual("******", new Cell {
            Content = "45.8794",
            Format = CellFormat.SCIENTIFIC,
            Decimal = 4
        }.ToString(6));
        Assert.AreEqual(" 5E+01", new Cell {
            Content = "45.8794",
            Format = CellFormat.SCIENTIFIC,
            Decimal = 0
        }.ToString(6));
        Assert.AreEqual("", new Cell {
            Content = "45.8794",
            Format = CellFormat.SCIENTIFIC,
            Decimal = 0
        }.ToString(0));
    }

    // Verify comma format formatting
    [Test]
    public void VerifyCommaFormat() {
        Assert.AreEqual("1,234,567.00", new Cell {
            Content = "1234567",
            Format = CellFormat.COMMAS
        }.ToString(12));
        Assert.AreEqual("   999.00", new Cell {
            Content = "999",
            Format = CellFormat.COMMAS
        }.ToString(9));
        Assert.AreEqual(" (999.00)", new Cell {
            Content = "-999",
            Format = CellFormat.COMMAS
        }.ToString(9));
        Assert.AreEqual("(999.00) ", new Cell {
            Content = "-999",
            Align = CellAlignment.LEFT,
            Format = CellFormat.COMMAS
        }.ToString(9));
        Assert.AreEqual(" (123,456.00) ", new Cell {
            Content = "-123456",
            Align = CellAlignment.CENTRE,
            Format = CellFormat.COMMAS
        }.ToString(14));
        Assert.AreEqual("********", new Cell {
            Content = "-123456",
            Align = CellAlignment.LEFT,
            Format = CellFormat.COMMAS
        }.ToString(8));
    }

    // Verify currency format formatting
    [Test]
    public void VerifyCurrencyFormat() {
        Assert.AreEqual("  £1,234,567.00", new Cell {
            Content = "1234567",
            Format = CellFormat.CURRENCY
        }.ToString(15));
        Assert.AreEqual("(£7,655.00) ", new Cell {
            Content = "-7655",
            Decimal = 2,
            Align = CellAlignment.LEFT,
            Format = CellFormat.CURRENCY
        }.ToString(12));
    }

    // Verify percent format formatting
    [Test]
    public void VerifyPercentFormat() {
        Assert.AreEqual("       50.00%", new Cell {
            Content = "0.5",
            Format = CellFormat.PERCENT
        }.ToString(13));
        Assert.AreEqual("50.000%   ", new Cell {
            Content = "0.5",
            Align = CellAlignment.LEFT,
            Decimal = 3,
            Format = CellFormat.PERCENT
        }.ToString(10));
        Assert.AreEqual("   20%   ", new Cell {
            Content = "0.2",
            Align = CellAlignment.CENTRE,
            Decimal = 0,
            Format = CellFormat.PERCENT
        }.ToString(9));
        Assert.AreEqual(" 456700.00% ", new Cell {
            Content = "4567",
            Align = CellAlignment.CENTRE,
            Decimal = 2,
            Format = CellFormat.PERCENT
        }.ToString(12));
        Assert.AreEqual("**", new Cell {
            Content = "0.2",
            Format = CellFormat.PERCENT
        }.ToString(2));
    }

    // Verify the date formatting
    [Test]
    public void VerifyDateFormats() {
        Assert.AreEqual("    28-Dec", new Cell {
            Content = "657435",
            Format = CellFormat.DATE_DM
        }.ToString(10));
        Assert.AreEqual("28-Dec    ", new Cell {
            Content = "657435",
            Format = CellFormat.DATE_DM,
            Align = CellAlignment.LEFT
        }.ToString(10));
        Assert.AreEqual("28-Dec-3699 ", new Cell {
            Content = "657435",
            Format = CellFormat.DATE_DMY,
            Align = CellAlignment.LEFT
        }.ToString(12));
        Assert.AreEqual("  Dec-3699  ", new Cell {
            Content = "657435",
            Format = CellFormat.DATE_MY,
            Align = CellAlignment.CENTRE
        }.ToString(12));
        Assert.AreEqual("   -666435.0", new Cell {
            Content = "-666435.0",
            Format = CellFormat.DATE_MY
        }.ToString(12));
        Assert.AreEqual("   3058465", new Cell {
            Content = "3058465",
            Format = CellFormat.DATE_DM
        }.ToString(10));
        Assert.AreEqual("NOT A DATE", new Cell {
            Content = "NOT A DATE",
            Format = CellFormat.DATE_DM
        }.ToString(10));
    }

    // Style a cell and ensure correct render string
    [Test]
    public void TestCellStyle() {
        Cell cell1 = new Cell {
            Content = "HELLO WORLD",
            Align = CellAlignment.RIGHT,
            Style = new CellStyle {
                Foreground = AnsiColour.Cyan,
                Background = AnsiColour.Green,
                Bold = true
            }
        };
        Assert.AreEqual("\u001b[36;42m\u001b[1m    HELLO WORLD\u001b[0m", cell1.AnsiTextSpan(15).EscapedString());

        Cell cell2 = new Cell {
            Content = "HELLO WORLD",
            Align = CellAlignment.LEFT,
            Style = new CellStyle {
                Italic = true,
                Underline = true
            }
        };
        Assert.AreEqual("\u001b[97;40m\u001b[3m\u001b[4mHELLO WORLD    \u001b[0m", cell2.AnsiTextSpan(15).EscapedString());
    }

    // Test changing the cell factory changes the default
    // properties of a cell.
    [Test]
    public void TestCellFactory() {
        Cell cell1 = new Cell();
        Cell cell3 = new Cell {
            Decimal = 4,
            Align = CellAlignment.CENTRE,
            Format = CellFormat.COMMAS,
            Style = new CellStyle {
                Foreground = AnsiColour.Blue,
                Background = AnsiColour.BrightGreen
            }
        };
        Assert.AreEqual(2, cell1.DecimalPlaces);
        Assert.AreEqual(CellAlignment.GENERAL, cell1.Alignment);
        Assert.AreEqual(CellFormat.GENERAL, cell1.CellFormat);

        // Save cell factory
        int dp = CellFactory.DecimalPlaces;
        CellAlignment align = CellFactory.Alignment;
        int fg = CellFactory.ForegroundColour;
        int bg = CellFactory.BackgroundColour;
        CellFormat fmt = CellFactory.Format;

        // cell1 should inherit factory defaults as no explicit change
        // to the properties were made. cell3 should retain the explicit
        // change to the properties.
        CellFactory.DecimalPlaces = 5;
        CellFactory.Alignment = CellAlignment.LEFT;
        CellFactory.ForegroundColour = AnsiColour.BrightYellow;
        CellFactory.BackgroundColour = AnsiColour.Blue;
        CellFactory.Format = CellFormat.SCIENTIFIC;

        Assert.AreEqual(5, cell1.DecimalPlaces);
        Assert.AreEqual(CellAlignment.LEFT, cell1.Alignment);
        Assert.AreEqual(CellFormat.SCIENTIFIC, cell1.CellFormat);
        Assert.AreEqual(AnsiColour.BrightYellow, cell1.Style.ForegroundColour);
        Assert.AreEqual(AnsiColour.Blue, cell1.Style.BackgroundColour);

        Assert.AreEqual(4, cell3.DecimalPlaces);
        Assert.AreEqual(CellAlignment.CENTRE, cell3.Alignment);
        Assert.AreEqual(CellFormat.COMMAS, cell3.CellFormat);
        Assert.AreEqual(AnsiColour.Blue, cell3.Style.ForegroundColour);
        Assert.AreEqual(AnsiColour.BrightGreen, cell3.Style.BackgroundColour);

        // Creating a new cell after changing the factory should pick
        // up the new factory defaults
        Cell cell2 = new Cell();
        Assert.AreEqual(5, cell2.DecimalPlaces);
        Assert.AreEqual(CellAlignment.LEFT, cell2.Alignment);
        Assert.AreEqual(CellFormat.SCIENTIFIC, cell2.CellFormat);
        Assert.AreEqual(AnsiColour.BrightYellow, cell2.Style.ForegroundColour);
        Assert.AreEqual(AnsiColour.Blue, cell2.Style.BackgroundColour);

        // Reset cell factory
        CellFactory.DecimalPlaces = dp;
        CellFactory.Alignment = align;
        CellFactory.ForegroundColour = fg;
        CellFactory.BackgroundColour = bg;
        CellFactory.Format = fmt;
    }
}