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
        Assert.AreEqual(CellFormat.GENERAL, cell.Format);
    }

    // Verify that we correctly translate positions to cell locations
    [Test]
    public void VerifyRowAndColumnFromPositions() {
        CellLocation a1 = Cell.LocationFromAddress("A1");
        Assert.AreEqual(1, a1.Row);
        Assert.AreEqual(1, a1.Column);

        CellLocation z1 = Cell.LocationFromAddress("Z1");
        Assert.AreEqual(1, z1.Row);
        Assert.AreEqual(26, z1.Column);

        CellLocation iu1 = Cell.LocationFromAddress("IU1");
        Assert.AreEqual(1, iu1.Row);
        Assert.AreEqual(255, iu1.Column);

        CellLocation a4095 = Cell.LocationFromAddress("A4095");
        Assert.AreEqual(4095, a4095.Row);
        Assert.AreEqual(1, a4095.Column);

        Assert.Throws(typeof(ArgumentNullException), delegate { Cell.LocationFromAddress(null!); });
    }

    // Verify the correct position is returned for a cell
    [Test]
    public void VerifyPositionForColumnAndRow() {
        Assert.AreEqual("A1", new Cell().Address);
        Assert.AreEqual("Z1", new Cell { Location = new CellLocation { Column = 26, Row = 1}}.Address);
        Assert.AreEqual("IU4095", new Cell { Location = new CellLocation { Column = 255, Row = 4095}}.Address);
    }

    // Verify the Location property
    [Test]
    public void VerifyLocation() {
        Assert.AreEqual(new Point(1, 1), new Cell().Location.Point);
        Assert.AreEqual(new Point(12, 12), new Cell { Location = new CellLocation { Column = 12, Row = 12}}.Location.Point);
    }

    // Verify the cell ToString
    [Test]
    public void VerifyToString() {
        Assert.AreEqual("12", new CellValue { Value = "12" }.ToString());
        Assert.AreEqual("\"World\"", new CellValue { Value = "World" }.ToString());
    }

    // Verify general alignment
    [Test]
    public void VerifyGeneralAlignment() {
        Assert.AreEqual(" 45.8794", new Cell {
            CellValue = new CellValue {
                Value = "45.8794"
            },
            Alignment = CellAlignment.GENERAL,
            Format = CellFormat.GENERAL,
            DecimalPlaces = 2
        }.ToString(8));
        Assert.AreEqual("Text    ", new Cell {
            CellValue = new CellValue {
                Value = "Text"
            },
            Alignment = CellAlignment.GENERAL,
            Format = CellFormat.GENERAL,
            DecimalPlaces = 2
        }.ToString(8));
        Assert.AreEqual(" 45.8794", new Cell {
            CellValue = new CellValue {
                Value = "45.8794"
            },
            Alignment = CellAlignment.RIGHT,
            Format = CellFormat.GENERAL,
            DecimalPlaces = 2
        }.ToString(8));
        Assert.AreEqual("        ", new Cell {
            CellValue = new CellValue(),
            Alignment = CellAlignment.GENERAL,
            Format = CellFormat.GENERAL,
            DecimalPlaces = 2
        }.ToString(8));
        Assert.Throws(typeof(ArgumentException), delegate {
            new Cell {
                CellValue = new CellValue {
                    Value = "45.8794"
                },
                Format = (CellFormat)12,
                DecimalPlaces = 2
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
            Alignment = CellAlignment.LEFT,
            Format = CellFormat.TEXT,
            DecimalPlaces = 2
        }.ToString(8));
    }

    // Verify alignments
    [Test]
    public void VerifyAlignments() {
        Assert.AreEqual("45.8794 ", new Cell {
            CellValue = new CellValue {
                Value = "45.8794"
            },
            Alignment = CellAlignment.LEFT,
            Format = CellFormat.GENERAL,
            DecimalPlaces = 2
        }.ToString(8));
        Assert.AreEqual(" 45.8794", new Cell {
            CellValue = new CellValue {
                Value = "45.8794"
            },
            Alignment = CellAlignment.RIGHT,
            Format = CellFormat.GENERAL,
            DecimalPlaces = 2
        }.ToString(8));
        Assert.AreEqual("45.8794 ", new Cell {
            CellValue = new CellValue {
                Value = "45.8794"
            },
            Alignment = CellAlignment.CENTRE,
            Format = CellFormat.GENERAL,
            DecimalPlaces = 2
        }.ToString(8));
        Assert.Throws(typeof(ArgumentException), delegate {
            new Cell {
                CellValue = new CellValue {
                    Value = "45.8794"
                },
                Alignment = (CellAlignment)4,
                Format = CellFormat.GENERAL,
                DecimalPlaces = 2
            }.ToString(8);
        });
    }

    // Verify fixed format formatting
    [Test]
    public void VerifyFixedFormat() {
        Assert.AreEqual(" 45.88", new Cell {
            CellValue = new CellValue {
                Value = "45.8794"
            },
            Format = CellFormat.FIXED,
            DecimalPlaces = 2
        }.ToString(6));
        Assert.AreEqual("45.88   ", new Cell {
            CellValue = new CellValue {
                Value = "45.8794"
            },
            Format = CellFormat.FIXED,
            Alignment = CellAlignment.LEFT,
            DecimalPlaces = 2
        }.ToString(8));
        Assert.AreEqual("  45.88  ", new Cell {
            CellValue = new CellValue {
                Value = "45.8794"
            },
            Format = CellFormat.FIXED,
            Alignment = CellAlignment.CENTRE,
            DecimalPlaces = 2
        }.ToString(9));
        Assert.AreEqual("45.879", new Cell {
            CellValue = new CellValue {
                Value = "45.8794"
            },
            Format = CellFormat.FIXED,
            DecimalPlaces = 3
        }.ToString(6));
        Assert.AreEqual("******", new Cell {
            CellValue = new CellValue {
                Value = "45.8794"
            },
            Format = CellFormat.FIXED,
            DecimalPlaces = 4
        }.ToString(6));
        Assert.AreEqual("    46", new Cell {
            CellValue = new CellValue {
                Value = "45.8794"
            },
            Format = CellFormat.FIXED,
            DecimalPlaces = 0
        }.ToString(6));
        Assert.AreEqual("", new Cell {
            CellValue = new CellValue {
                Value = "45.8794"
            },
            Format = CellFormat.FIXED,
            DecimalPlaces = 0
        }.ToString(0));
    }

    // Verify scientific format formatting
    [Test]
    public void VerifyScientificFormat() {
        Assert.AreEqual("  4.59E+01", new Cell {
            CellValue = new CellValue {
                Value = "45.8794"
            },
            Format = CellFormat.SCIENTIFIC,
            DecimalPlaces = 2
        }.ToString(10));
        Assert.AreEqual("4.59E+01    ", new Cell {
            CellValue = new CellValue {
                Value = "45.8794"
            },
            Format = CellFormat.SCIENTIFIC,
            Alignment = CellAlignment.LEFT,
            DecimalPlaces = 2
        }.ToString(12));
        Assert.AreEqual("  4.59E+01  ", new Cell {
            CellValue = new CellValue {
                Value = "45.8794"
            },
            Format = CellFormat.SCIENTIFIC,
            Alignment = CellAlignment.CENTRE,
            DecimalPlaces = 2
        }.ToString(12));
        Assert.AreEqual("   4.588E+01", new Cell {
            CellValue = new CellValue {
                Value = "45.8794"
            },
            Format = CellFormat.SCIENTIFIC,
            DecimalPlaces = 3
        }.ToString(12));
        Assert.AreEqual("******", new Cell {
            CellValue = new CellValue {
                Value = "45.8794"
            },
            Format = CellFormat.SCIENTIFIC,
            DecimalPlaces = 4
        }.ToString(6));
        Assert.AreEqual(" 5E+01", new Cell {
            CellValue = new CellValue {
                Value = "45.8794"
            },
            Format = CellFormat.SCIENTIFIC,
            DecimalPlaces = 0
        }.ToString(6));
        Assert.AreEqual("", new Cell {
            CellValue = new CellValue {
                Value = "45.8794"
            },
            Format = CellFormat.SCIENTIFIC,
            DecimalPlaces = 0
        }.ToString(0));
    }

    // Verify comma format formatting
    [Test]
    public void VerifyCommaFormat() {
        Assert.AreEqual("1,234,567", new Cell {
            CellValue = new CellValue {
                Value = "1234567"
            },
            Format = CellFormat.COMMAS
        }.ToString(9));
        Assert.AreEqual("      999", new Cell {
            CellValue = new CellValue {
                Value = "999"
            },
            Format = CellFormat.COMMAS
        }.ToString(9));
        Assert.AreEqual("    (999)", new Cell {
            CellValue = new CellValue {
                Value = "-999"
            },
            Format = CellFormat.COMMAS
        }.ToString(9));
        Assert.AreEqual("(999)    ", new Cell {
            CellValue = new CellValue {
                Value = "-999"
            },
            Alignment = CellAlignment.LEFT,
            Format = CellFormat.COMMAS
        }.ToString(9));
        Assert.AreEqual(" (123,456) ", new Cell {
            CellValue = new CellValue {
                Value = "-123456"
            },
            Alignment = CellAlignment.CENTRE,
            Format = CellFormat.COMMAS
        }.ToString(11));
        Assert.AreEqual("********", new Cell {
            CellValue = new CellValue {
                Value = "-123456"
            },
            Alignment = CellAlignment.LEFT,
            Format = CellFormat.COMMAS
        }.ToString(8));
    }

    // Verify currency format formatting
    [Test]
    public void VerifyCurrencyFormat() {
        Assert.AreEqual("  £1,234,567", new Cell {
            CellValue = new CellValue {
                Value = "1234567"
            },
            Format = CellFormat.CURRENCY
        }.ToString(12));
        Assert.AreEqual("(£7,655.00) ", new Cell {
            CellValue = new CellValue {
                Value = "-7655"
            },
            DecimalPlaces = 2,
            Alignment = CellAlignment.LEFT,
            Format = CellFormat.CURRENCY
        }.ToString(12));
    }

    // Verify bar format formatting
    [Test]
    public void VerifyBarFormat() {
        Assert.AreEqual("+++++     ", new Cell {
            CellValue = new CellValue {
                Value = "5"
            },
            Format = CellFormat.BAR
        }.ToString(10));
        Assert.AreEqual("++++++++++", new Cell {
            CellValue = new CellValue {
                Value = "10"
            },
            Format = CellFormat.BAR
        }.ToString(10));
        Assert.AreEqual("          ", new Cell {
            CellValue = new CellValue {
                Value = "0"
            },
            Format = CellFormat.BAR
        }.ToString(10));
        Assert.AreEqual("-----     ", new Cell {
            CellValue = new CellValue {
                Value = "-5"
            },
            Format = CellFormat.BAR
        }.ToString(10));
        Assert.AreEqual("**********", new Cell {
            CellValue = new CellValue {
                Value = "12"
            },
            Format = CellFormat.BAR
        }.ToString(10));
    }

    // Verify percent format formatting
    [Test]
    public void VerifyPercentFormat() {
        Assert.AreEqual("       50%", new Cell {
            CellValue = new CellValue {
                Value = "0.5"
            },
            Format = CellFormat.PERCENT
        }.ToString(10));
        Assert.AreEqual("50.000%   ", new Cell {
            CellValue = new CellValue {
                Value = "0.5"
            },
            Alignment = CellAlignment.LEFT,
            DecimalPlaces = 3,
            Format = CellFormat.PERCENT
        }.ToString(10));
        Assert.AreEqual("   20%   ", new Cell {
            CellValue = new CellValue {
                Value = "0.2"
            },
            Alignment = CellAlignment.CENTRE,
            DecimalPlaces = 0,
            Format = CellFormat.PERCENT
        }.ToString(9));
        Assert.AreEqual(" 456700.00% ", new Cell {
            CellValue = new CellValue {
                Value = "4567"
            },
            Alignment = CellAlignment.CENTRE,
            DecimalPlaces = 2,
            Format = CellFormat.PERCENT
        }.ToString(12));
        Assert.AreEqual("**", new Cell {
            CellValue = new CellValue {
                Value = "0.2"
            },
            Format = CellFormat.PERCENT
        }.ToString(2));
    }

    // Verify the date formatting
    [Test]
    public void VerifyDateFormats() {
        Assert.AreEqual("    28-Dec", new Cell {
            CellValue = new CellValue {
                Value = "657435"
            },
            Format = CellFormat.DATE_DM
        }.ToString(10));
        Assert.AreEqual("28-Dec    ", new Cell {
            CellValue = new CellValue {
                Value = "657435"
            },
            Format = CellFormat.DATE_DM,
            Alignment = CellAlignment.LEFT
        }.ToString(10));
        Assert.AreEqual("28-Dec-3699 ", new Cell {
            CellValue = new CellValue {
                Value = "657435"
            },
            Format = CellFormat.DATE_DMY,
            Alignment = CellAlignment.LEFT
        }.ToString(12));
        Assert.AreEqual("  Dec-3699  ", new Cell {
            CellValue = new CellValue {
                Value = "657435"
            },
            Format = CellFormat.DATE_MY,
            Alignment = CellAlignment.CENTRE
        }.ToString(12));
        Assert.AreEqual("   -666435.0", new Cell {
            CellValue = new CellValue {
                Value = "-666435.0"
            },
            Format = CellFormat.DATE_MY
        }.ToString(12));
        Assert.AreEqual("   3058465", new Cell {
            CellValue = new CellValue {
                Value = "3058465"
            },
            Format = CellFormat.DATE_DM
        }.ToString(10));
        Assert.AreEqual("NOT A DATE", new Cell {
            CellValue = new CellValue {
                Value = "NOT A DATE"
            },
            Format = CellFormat.DATE_DM
        }.ToString(10));
    }
}