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
using ExcelNumberFormat;
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

        Assert.Throws(typeof(FormatException), delegate { _ = new CellLocation(null!); });
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

    // Verify that setting a cell value to a time string converts it
    // to the correct OADate representation.
    [Test]
    public void VerifyTryParseTime() {
        Assert.AreEqual("45620.520833333336", new Cell { UIContent = "12:30" }.CellValue.Value);
        Assert.AreEqual("45620", new Cell { UIContent = "00:00:00" }.CellValue.Value);
        Assert.AreEqual("45620.99998842592", new Cell { UIContent = "23:59:59" }.CellValue.Value);
        Assert.AreEqual("45620.30039351852", new Cell { UIContent = "7:12:34 AM" }.CellValue.Value);
        Assert.AreEqual("7pm", new Cell { UIContent = "7pm" }.CellValue.Value);
        Assert.AreEqual("8.04 AM", new Cell { UIContent = "8.04 AM" }.CellValue.Value);
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
            CellFormat = CellFormat.GENERAL,
            DecimalPlaces = 3,
            Alignment = CellAlignment.CENTRE
        };
        Cell cell2 = new Cell {
            CellValue = new CellValue {
                Value = "67.9"
            },
            Location = new CellLocation { Column = 1, Row = 17 },
            CellFormat = CellFormat.PERCENT,
            DecimalPlaces = 1,
            Alignment = CellAlignment.RIGHT
        };
        cell1.Swap(cell2);
        Assert.AreEqual(cell1.CellValue.Value, "67.9");
        Assert.AreEqual(cell1.Location.Column, 3);
        Assert.AreEqual(cell1.Location.Row, 8);
        Assert.AreEqual(cell1.CellFormat, CellFormat.PERCENT);
        Assert.AreEqual(cell1.Decimal, 1);
        Assert.AreEqual(cell1.Alignment, CellAlignment.RIGHT);
    }

    // Verify the CreateFrom method
    [Test]
    public void VerifyCellCopyConstructor() {
        Cell cell = new Cell {
            Content = "67.9",
            Location = new CellLocation { Column = 1, Row = 17 },
            CellFormat = CellFormat.PERCENT,
            DecimalPlaces = 1,
            Alignment = CellAlignment.RIGHT
        };

        Sheet sheet = new Sheet();

        Cell newCell = new Cell(sheet, cell);
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
            CellValue = new CellValue {
                Value = "45.8794"
            },
            CellFormat = CellFormat.GENERAL
        }.FormatDescription);
        Assert.AreEqual("(R)", new Cell {
            CellValue = new CellValue {
                Value = "45.8794"
            },
            CellFormat = CellFormat.TEXT
        }.FormatDescription);
        Assert.AreEqual("(F3)", new Cell {
            CellValue = new CellValue {
                Value = "45.8794"
            },
            CellFormat = CellFormat.FIXED,
            DecimalPlaces = 3
        }.FormatDescription);
        Assert.AreEqual("(S1)", new Cell {
            CellValue = new CellValue {
                Value = "45.8794"
            },
            CellFormat = CellFormat.SCIENTIFIC,
            DecimalPlaces = 1
        }.FormatDescription);
        Assert.AreEqual("(P2)", new Cell {
            CellValue = new CellValue {
                Value = "45.8794"
            },
            CellFormat = CellFormat.PERCENT,
            DecimalPlaces = 2
        }.FormatDescription);
        Assert.AreEqual("(C4)", new Cell {
            CellValue = new CellValue {
                Value = "45.8794"
            },
            CellFormat = CellFormat.CURRENCY,
            DecimalPlaces = 4
        }.FormatDescription);
        Assert.AreEqual("(D1)", new Cell {
            CellValue = new CellValue {
                Value = "48794"
            },
            CellFormat = CellFormat.DATE_DMY
        }.FormatDescription);
        Assert.AreEqual("(D2)", new Cell {
            CellValue = new CellValue {
                Value = "48794"
            },
            CellFormat = CellFormat.DATE_DM
        }.FormatDescription);
        Assert.AreEqual("(D3)", new Cell {
            CellValue = new CellValue {
                Value = "48794"
            },
            CellFormat = CellFormat.DATE_MY
        }.FormatDescription);
        Assert.AreEqual("(TM)", new Cell {
            CellValue = new CellValue {
                Value = "48794"
            },
            CellFormat = CellFormat.TIME
        }.FormatDescription);
        Assert.AreEqual("(#,##0)", new Cell {
            CellValue = new CellValue {
                Value = "45.8794"
            },
            CellFormat = CellFormat.CUSTOM,
            CustomFormatString = "#,##0",
            DecimalPlaces = 2
        }.FormatDescription);
        Assert.AreEqual("(11)", new Cell {
            CellValue = new CellValue {
                Value = "45.8794"
            },
            CellFormat = (CellFormat)11,
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
        }.FormattedText(8));
        Assert.AreEqual("Text    ", new Cell {
            Content = "Text",
            Alignment = CellAlignment.GENERAL,
            CellFormat = CellFormat.GENERAL,
            DecimalPlaces = 2
        }.FormattedText(8));
        Assert.AreEqual(" 45.8794", new Cell {
            Content = "45.8794",
            Alignment = CellAlignment.RIGHT,
            CellFormat = CellFormat.GENERAL,
            DecimalPlaces = 2
        }.FormattedText(8));
        Assert.AreEqual("        ", new Cell {
            CellValue = new CellValue(),
            Alignment = CellAlignment.GENERAL,
            CellFormat = CellFormat.GENERAL,
            DecimalPlaces = 2
        }.FormattedText(8));
        Assert.Throws(typeof(ArgumentException), delegate {
            new Cell {
                Content = "45.8794",
                CellFormat = (CellFormat)12,
                DecimalPlaces = 2
            }.FormattedText(8);
        });
    }

    // Verify the TEXT format
    [Test]
    public void VerifyTextFormat() {
        Assert.AreEqual("45.8794 ", new Cell {
            CellValue = new CellValue {
                Value = "45.8794"
            },
            Alignment = CellAlignment.LEFT,
            CellFormat = CellFormat.TEXT,
            DecimalPlaces = 2
        }.FormattedText(8));
        Assert.AreEqual("HELLO WO", new Cell {
            CellValue = new CellValue {
                Value = "HELLO WORLD!"
            },
            Alignment = CellAlignment.LEFT,
            CellFormat = CellFormat.TEXT
        }.FormattedText(8));
        Assert.Catch(typeof(ArgumentException), delegate {
            NumberFormats.GetFormat(CellFormat.TEXT, false, 5);
        });
    }

    // Verify alignments
    [Test]
    public void VerifyAlignments() {
        Assert.AreEqual("45.8794 ", new Cell {
            CellValue = new CellValue {
                Value = "45.8794"
            },
            Alignment = CellAlignment.LEFT,
            CellFormat = CellFormat.GENERAL,
            DecimalPlaces = 2
        }.FormattedText(8));
        Assert.AreEqual(" 45.8794", new Cell {
            CellValue = new CellValue {
                Value = "45.8794"
            },
            Alignment = CellAlignment.RIGHT,
            CellFormat = CellFormat.GENERAL,
            DecimalPlaces = 2
        }.FormattedText(8));
        Assert.AreEqual("45.8794 ", new Cell {
            CellValue = new CellValue {
                Value = "45.8794"
            },
            Alignment = CellAlignment.CENTRE,
            CellFormat = CellFormat.GENERAL,
            DecimalPlaces = 2
        }.FormattedText(8));
        Assert.Throws(typeof(ArgumentException), delegate {
            new Cell {
                CellValue = new CellValue {
                    Value = "45.8794"
                },
                Alignment = (CellAlignment)4,
                CellFormat = CellFormat.GENERAL,
                DecimalPlaces = 2
            }.FormattedText(8);
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
        }.FormattedText(10));
    }

    // Verify fixed format formatting
    [Test]
    public void VerifyFixedFormat() {
        Assert.AreEqual(" 45.88", new Cell {
            Content = "45.8794",
            CellFormat = CellFormat.FIXED,
            DecimalPlaces = 2
        }.FormattedText(6));
        Assert.AreEqual("45.88   ", new Cell {
            Content = "45.8794",
            CellFormat = CellFormat.FIXED,
            Alignment = CellAlignment.LEFT,
            DecimalPlaces = 2
        }.FormattedText(8));
        Assert.AreEqual("  45.88  ", new Cell {
            Content = "45.8794",
            CellFormat = CellFormat.FIXED,
            Alignment = CellAlignment.CENTRE,
            DecimalPlaces = 2
        }.FormattedText(9));
        Assert.AreEqual("45.879", new Cell {
            Content = "45.8794",
            CellFormat = CellFormat.FIXED,
            DecimalPlaces = 3
        }.FormattedText(6));
        Assert.AreEqual("******", new Cell {
            Content = "45.8794",
            CellFormat = CellFormat.FIXED,
            DecimalPlaces = 4
        }.FormattedText(6));
        Assert.AreEqual("   46.", new Cell {
            Content = "45.8794",
            CellFormat = CellFormat.FIXED,
            DecimalPlaces = 0
        }.FormattedText(6));
        Assert.AreEqual("", new Cell {
            Content = "45.8794",
            CellFormat = CellFormat.FIXED,
            DecimalPlaces = 0
        }.FormattedText(0));
    }

    // Verify scientific format formatting
    [Test]
    public void VerifyScientificFormat() {
        Assert.AreEqual("  4.59E+01", new Cell {
            Content = "45.8794",
            CellFormat = CellFormat.SCIENTIFIC,
            DecimalPlaces = 2
        }.FormattedText(10));
        Assert.AreEqual("4.59E+01    ", new Cell {
            Content = "45.8794",
            CellFormat = CellFormat.SCIENTIFIC,
            Alignment = CellAlignment.LEFT,
            DecimalPlaces = 2
        }.FormattedText(12));
        Assert.AreEqual("  4.59E+01  ", new Cell {
            Content = "45.8794",
            CellFormat = CellFormat.SCIENTIFIC,
            Alignment = CellAlignment.CENTRE,
            DecimalPlaces = 2
        }.FormattedText(12));
        Assert.AreEqual("   4.588E+01", new Cell {
            Content = "45.8794",
            CellFormat = CellFormat.SCIENTIFIC,
            DecimalPlaces = 3
        }.FormattedText(12));
        Assert.AreEqual("******", new Cell {
            Content = "45.8794",
            CellFormat = CellFormat.SCIENTIFIC,
            DecimalPlaces = 4
        }.FormattedText(6));
        Assert.AreEqual("5.E+01", new Cell {
            Content = "45.8794",
            CellFormat = CellFormat.SCIENTIFIC,
            DecimalPlaces = 0
        }.FormattedText(6));
        Assert.AreEqual("", new Cell {
            Content = "45.8794",
            CellFormat = CellFormat.SCIENTIFIC,
            DecimalPlaces = 0
        }.FormattedText(0));
    }

    // Verify currency format formatting
    [Test]
    public void VerifyCurrencyFormat() {
        Assert.AreEqual("  £1,234,567.00", new Cell {
            Content = "1234567",
            CellFormat = CellFormat.CURRENCY
        }.FormattedText(15));
        Assert.AreEqual("-£7,655.00  ", new Cell {
            Content = "-7655",
            DecimalPlaces = 2,
            Alignment = CellAlignment.LEFT,
            CellFormat = CellFormat.CURRENCY
        }.FormattedText(12));
    }

    // Verify percent format formatting
    [Test]
    public void VerifyPercentFormat() {
        Assert.AreEqual("       50.00%", new Cell {
            Content = "0.5",
            CellFormat = CellFormat.PERCENT
        }.FormattedText(13));
        Assert.AreEqual("50.000%   ", new Cell {
            Content = "0.5",
            Alignment = CellAlignment.LEFT,
            DecimalPlaces = 3,
            CellFormat = CellFormat.PERCENT
        }.FormattedText(10));
        Assert.AreEqual("  20.%   ", new Cell {
            Content = "0.2",
            Alignment = CellAlignment.CENTRE,
            DecimalPlaces = 0,
            CellFormat = CellFormat.PERCENT
        }.FormattedText(9));
        Assert.AreEqual(" 456700.00% ", new Cell {
            Content = "4567",
            Alignment = CellAlignment.CENTRE,
            DecimalPlaces = 2,
            CellFormat = CellFormat.PERCENT
        }.FormattedText(12));
        Assert.AreEqual("**", new Cell {
            Content = "0.2",
            CellFormat = CellFormat.PERCENT
        }.FormattedText(2));
    }

    // Verify the date formatting
    [Test]
    public void VerifyDateFormats() {
        Assert.AreEqual("    28-Dec", new Cell {
            Content = "657435",
            CellFormat = CellFormat.DATE_DM
        }.FormattedText(10));
        Assert.AreEqual("28-Dec    ", new Cell {
            Content = "657435",
            CellFormat = CellFormat.DATE_DM,
            Alignment = CellAlignment.LEFT
        }.FormattedText(10));
        Assert.AreEqual("28-Dec-3699 ", new Cell {
            Content = "657435",
            CellFormat = CellFormat.DATE_DMY,
            Alignment = CellAlignment.LEFT
        }.FormattedText(12));
        Assert.AreEqual("  Dec-3699  ", new Cell {
            Content = "657435",
            CellFormat = CellFormat.DATE_MY,
            Alignment = CellAlignment.CENTRE
        }.FormattedText(12));
        Assert.AreEqual("NOT A DATE", new Cell {
            Content = "NOT A DATE",
            CellFormat = CellFormat.DATE_DM
        }.FormattedText(10));
    }

    // Verify the time formatting
    [Test]
    public void VerifyTimeFormats() {
        Assert.AreEqual(" 12:30:00 PM", new Cell {
            Content = "45619.520833333336",
            CellFormat = CellFormat.TIME
        }.FormattedText(12));
        Assert.AreEqual(" 12:00:00 AM", new Cell {
            Content = "45619",
            CellFormat = CellFormat.TIME
        }.FormattedText(12));
        Assert.AreEqual(" 11:59:59 PM", new Cell {
            Content = "45619.99998842592",
            CellFormat = CellFormat.TIME
        }.FormattedText(12));
        Assert.AreEqual("  7:12:34 AM", new Cell {
            Content = "45619.30039351852",
            CellFormat = CellFormat.TIME
        }.FormattedText(12));
    }

    // Style a cell and ensure correct render string
    [Test]
    public void TestCellStyle() {
        Cell cell1 = new Cell {
            Content = "HELLO WORLD",
            Alignment = CellAlignment.RIGHT,
            Style = new CellStyle {
                ForegroundColour = AnsiColour.Cyan,
                BackgroundColour = AnsiColour.Green,
                IsBold = true
            }
        };
        Assert.AreEqual("\u001b[36;42m\u001b[1m    HELLO WORLD\u001b[0m", cell1.AnsiTextSpan(15).EscapedString());

        Cell cell2 = new Cell {
            Content = "HELLO WORLD",
            Alignment = CellAlignment.LEFT,
            Style = new CellStyle {
                IsItalic = true,
                IsUnderlined = true
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
            DecimalPlaces = 4,
            Alignment = CellAlignment.CENTRE,
            UseThousandsSeparator = true,
            Style = new CellStyle {
                ForegroundColour = AnsiColour.Blue,
                BackgroundColour = AnsiColour.BrightGreen
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
        Assert.IsTrue(cell3.UseThousandsSeparator);
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