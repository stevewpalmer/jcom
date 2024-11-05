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
using JCalcLib;
using NUnit.Framework;

namespace CalcLibTests;

[TestFixture]
public class CellTests {

    // Create a cell and verify properties.
    [Test]
    public void VerifyDefaultProperties() {
        Cell cell = new Cell();
        Assert.AreEqual(0, cell.Row);
        Assert.AreEqual(0, cell.Column);
        Assert.AreEqual(CellAlignment.GENERAL, cell.Alignment);
        Assert.AreEqual(CellFormat.GENERAL, cell.Format);
    }

    // Verify that we correctly translate positions to cell locations
    [Test]
    public void VerifyRowAndColumnFromPositions() {
        Assert.AreEqual((1, 1), Cell.ColumnAndRowFromPosition("A1"));
        Assert.AreEqual((26, 1), Cell.ColumnAndRowFromPosition("Z1"));
        Assert.AreEqual((255, 1), Cell.ColumnAndRowFromPosition("IU1"));
        Assert.AreEqual((1, 4095), Cell.ColumnAndRowFromPosition("A4095"));
        Assert.AreEqual((0, 78), Cell.ColumnAndRowFromPosition("78"));
        Assert.AreEqual((0, 0), Cell.ColumnAndRowFromPosition(""));
        Assert.Throws(typeof(ArgumentNullException), delegate { Cell.ColumnAndRowFromPosition(null!); });
    }

    // Verify the correct position is returned for a cell
    [Test]
    public void VerifyPositionForColumnAndRow() {
        Assert.AreEqual("A1", new Cell { Row = 1, Column = 1}.Position);
        Assert.AreEqual("Z1", new Cell { Row = 1, Column = 26}.Position);
        Assert.AreEqual("IU4095", new Cell { Row = 4095, Column = 255}.Position);
    }

    // Verify general alignment
    [Test]
    public void VerifyGeneralAlignment() {
        Assert.AreEqual(" 45.8794", new Cell {
            Value = new CellValue {
                StringValue = "45.8794",
                Type = CellType.NUMBER
            },
            Alignment = CellAlignment.GENERAL,
            Format = CellFormat.GENERAL,
            DecimalPlaces = 2
        }.ToString(8));
        Assert.AreEqual("45.8794 ", new Cell {
            Value = new CellValue {
                StringValue = "45.8794",
                Type = CellType.TEXT
            },
            Alignment = CellAlignment.GENERAL,
            Format = CellFormat.GENERAL,
            DecimalPlaces = 2
        }.ToString(8));
        Assert.AreEqual(" 45.8794", new Cell {
            Value = new CellValue {
                StringValue = "45.8794",
                Type = CellType.TEXT
            },
            Alignment = CellAlignment.RIGHT,
            Format = CellFormat.GENERAL,
            DecimalPlaces = 2
        }.ToString(8));
        Assert.AreEqual("        ", new Cell {
            Value = new CellValue {
                StringValue = "45.8794",
                Type = CellType.NONE
            },
            Alignment = CellAlignment.GENERAL,
            Format = CellFormat.GENERAL,
            DecimalPlaces = 2
        }.ToString(8));
        Assert.Throws(typeof(ArgumentException), delegate {
            new Cell {
                Value = new CellValue {
                    StringValue = "45.8794",
                    Type = CellType.NUMBER
                },
                Format = (CellFormat)12,
                DecimalPlaces = 2
            }.ToString(8);
        });
    }

    // Verify alignments
    [Test]
    public void VerifyAlignments() {
        Assert.AreEqual("45.8794 ", new Cell {
            Value = new CellValue {
                StringValue = "45.8794",
                Type = CellType.NUMBER
            },
            Alignment = CellAlignment.LEFT,
            Format = CellFormat.GENERAL,
            DecimalPlaces = 2
        }.ToString(8));
        Assert.AreEqual(" 45.8794", new Cell {
            Value = new CellValue {
                StringValue = "45.8794",
                Type = CellType.NUMBER
            },
            Alignment = CellAlignment.RIGHT,
            Format = CellFormat.GENERAL,
            DecimalPlaces = 2
        }.ToString(8));
        Assert.AreEqual("45.8794 ", new Cell {
            Value = new CellValue {
                StringValue = "45.8794",
                Type = CellType.NUMBER
            },
            Alignment = CellAlignment.CENTRE,
            Format = CellFormat.GENERAL,
            DecimalPlaces = 2
        }.ToString(8));
        Assert.Throws(typeof(ArgumentException), delegate {
            new Cell {
                Value = new CellValue {
                    StringValue = "45.8794",
                    Type = CellType.NUMBER
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
            Value = new CellValue {
                StringValue = "45.8794",
                Type = CellType.NUMBER
            },
            Format = CellFormat.FIXED,
            DecimalPlaces = 2
        }.ToString(6));
        Assert.AreEqual("45.88   ", new Cell {
            Value = new CellValue {
                StringValue = "45.8794",
                Type = CellType.NUMBER
            },
            Format = CellFormat.FIXED,
            Alignment = CellAlignment.LEFT,
            DecimalPlaces = 2
        }.ToString(8));
        Assert.AreEqual("  45.88  ", new Cell {
            Value = new CellValue {
                StringValue = "45.8794",
                Type = CellType.NUMBER
            },
            Format = CellFormat.FIXED,
            Alignment = CellAlignment.CENTRE,
            DecimalPlaces = 2
        }.ToString(9));
        Assert.AreEqual("45.879", new Cell {
            Value = new CellValue {
                StringValue = "45.8794",
                Type = CellType.NUMBER
            },
            Format = CellFormat.FIXED,
            DecimalPlaces = 3
        }.ToString(6));
        Assert.AreEqual("******", new Cell {
            Value = new CellValue {
                StringValue = "45.8794",
                Type = CellType.NUMBER
            },
            Format = CellFormat.FIXED,
            DecimalPlaces = 4
        }.ToString(6));
        Assert.AreEqual("    46", new Cell {
            Value = new CellValue {
                StringValue = "45.8794",
                Type = CellType.NUMBER
            },
            Format = CellFormat.FIXED,
            DecimalPlaces = 0
        }.ToString(6));
        Assert.AreEqual("", new Cell {
            Value = new CellValue {
                StringValue = "45.8794",
                Type = CellType.NUMBER
            },
            Format = CellFormat.FIXED,
            DecimalPlaces = 0
        }.ToString(0));
    }

    // Verify scientific format formatting
    [Test]
    public void VerifyScientificFormat() {
        Assert.AreEqual("  4.59E+01", new Cell {
            Value = new CellValue {
                StringValue = "45.8794",
                Type = CellType.NUMBER
            },
            Format = CellFormat.SCIENTIFIC,
            DecimalPlaces = 2
        }.ToString(10));
        Assert.AreEqual("4.59E+01    ", new Cell {
            Value = new CellValue {
                StringValue = "45.8794",
                Type = CellType.NUMBER
            },
            Format = CellFormat.SCIENTIFIC,
            Alignment = CellAlignment.LEFT,
            DecimalPlaces = 2
        }.ToString(12));
        Assert.AreEqual("  4.59E+01  ", new Cell {
            Value = new CellValue {
                StringValue = "45.8794",
                Type = CellType.NUMBER
            },
            Format = CellFormat.SCIENTIFIC,
            Alignment = CellAlignment.CENTRE,
            DecimalPlaces = 2
        }.ToString(12));
        Assert.AreEqual("   4.588E+01", new Cell {
            Value = new CellValue {
                StringValue = "45.8794",
                Type = CellType.NUMBER
            },
            Format = CellFormat.SCIENTIFIC,
            DecimalPlaces = 3
        }.ToString(12));
        Assert.AreEqual("******", new Cell {
            Value = new CellValue {
                StringValue = "45.8794",
                Type = CellType.NUMBER
            },
            Format = CellFormat.SCIENTIFIC,
            DecimalPlaces = 4
        }.ToString(6));
        Assert.AreEqual(" 5E+01", new Cell {
            Value = new CellValue {
                StringValue = "45.8794",
                Type = CellType.NUMBER
            },
            Format = CellFormat.SCIENTIFIC,
            DecimalPlaces = 0
        }.ToString(6));
        Assert.AreEqual("", new Cell {
            Value = new CellValue {
                StringValue = "45.8794",
                Type = CellType.NUMBER
            },
            Format = CellFormat.SCIENTIFIC,
            DecimalPlaces = 0
        }.ToString(0));
    }

    // Verify comma format formatting
    [Test]
    public void VerifyCommaFormat() {
        Assert.AreEqual("1,234,567", new Cell {
            Value = new CellValue {
                StringValue = "1234567",
                Type = CellType.NUMBER
            },
            Format = CellFormat.COMMAS
        }.ToString(9));
        Assert.AreEqual("      999", new Cell {
            Value = new CellValue {
                StringValue = "999",
                Type = CellType.NUMBER
            },
            Format = CellFormat.COMMAS
        }.ToString(9));
        Assert.AreEqual("    (999)", new Cell {
            Value = new CellValue {
                StringValue = "-999",
                Type = CellType.NUMBER
            },
            Format = CellFormat.COMMAS
        }.ToString(9));
        Assert.AreEqual("(999)    ", new Cell {
            Value = new CellValue {
                StringValue = "-999",
                Type = CellType.NUMBER
            },
            Alignment = CellAlignment.LEFT,
            Format = CellFormat.COMMAS
        }.ToString(9));
        Assert.AreEqual(" (123,456) ", new Cell {
            Value = new CellValue {
                StringValue = "-123456",
                Type = CellType.NUMBER
            },
            Alignment = CellAlignment.CENTRE,
            Format = CellFormat.COMMAS
        }.ToString(11));
        Assert.AreEqual("********", new Cell {
            Value = new CellValue {
                StringValue = "-123456",
                Type = CellType.NUMBER
            },
            Alignment = CellAlignment.LEFT,
            Format = CellFormat.COMMAS
        }.ToString(8));
    }

    // Verify currency format formatting
    [Test]
    public void VerifyCurrencyFormat() {
        Assert.AreEqual("  £1,234,567", new Cell {
            Value = new CellValue {
                StringValue = "1234567",
                Type = CellType.NUMBER
            },
            Format = CellFormat.CURRENCY
        }.ToString(12));
        Assert.AreEqual("(£7,655.00) ", new Cell {
            Value = new CellValue {
                StringValue = "-7655",
                Type = CellType.NUMBER
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
            Value = new CellValue {
                StringValue = "5",
                Type = CellType.NUMBER
            },
            Format = CellFormat.BAR
        }.ToString(10));
        Assert.AreEqual("++++++++++", new Cell {
            Value = new CellValue {
                StringValue = "10",
                Type = CellType.NUMBER
            },
            Format = CellFormat.BAR
        }.ToString(10));
        Assert.AreEqual("          ", new Cell {
            Value = new CellValue {
                StringValue = "0",
                Type = CellType.NUMBER
            },
            Format = CellFormat.BAR
        }.ToString(10));
        Assert.AreEqual("-----     ", new Cell {
            Value = new CellValue {
                StringValue = "-5",
                Type = CellType.NUMBER
            },
            Format = CellFormat.BAR
        }.ToString(10));
        Assert.AreEqual("**********", new Cell {
            Value = new CellValue {
                StringValue = "12",
                Type = CellType.NUMBER
            },
            Format = CellFormat.BAR
        }.ToString(10));
    }

    // Verify percent format formatting
    [Test]
    public void VerifyPercentFormat() {
        Assert.AreEqual("       50%", new Cell {
            Value = new CellValue {
                StringValue = "0.5",
                Type = CellType.NUMBER
            },
            Format = CellFormat.PERCENT
        }.ToString(10));
        Assert.AreEqual("50.000%   ", new Cell {
            Value = new CellValue {
                StringValue = "0.5",
                Type = CellType.NUMBER
            },
            Alignment = CellAlignment.LEFT,
            DecimalPlaces = 3,
            Format = CellFormat.PERCENT
        }.ToString(10));
        Assert.AreEqual("   20%   ", new Cell {
            Value = new CellValue {
                StringValue = "0.2",
                Type = CellType.NUMBER
            },
            Alignment = CellAlignment.CENTRE,
            DecimalPlaces = 0,
            Format = CellFormat.PERCENT
        }.ToString(9));
        Assert.AreEqual(" 456700.00% ", new Cell {
            Value = new CellValue {
                StringValue = "4567",
                Type = CellType.NUMBER
            },
            Alignment = CellAlignment.CENTRE,
            DecimalPlaces = 2,
            Format = CellFormat.PERCENT
        }.ToString(12));
        Assert.AreEqual("**", new Cell {
            Value = new CellValue {
                StringValue = "0.2",
                Type = CellType.NUMBER
            },
            Format = CellFormat.PERCENT
        }.ToString(2));
    }

    // Verify the date formatting
    [Test]
    public void VerifyDateFormats() {
        Assert.AreEqual("    28-Dec", new Cell {
            Value = new CellValue {
                StringValue = "657435",
                Type = CellType.NUMBER
            },
            Format = CellFormat.DATE_DM
        }.ToString(10));
        Assert.AreEqual("28-Dec    ", new Cell {
            Value = new CellValue {
                StringValue = "657435",
                Type = CellType.NUMBER
            },
            Format = CellFormat.DATE_DM,
            Alignment = CellAlignment.LEFT
        }.ToString(10));
        Assert.AreEqual("28-Dec-3699 ", new Cell {
            Value = new CellValue {
                StringValue = "657435",
                Type = CellType.NUMBER
            },
            Format = CellFormat.DATE_DMY,
            Alignment = CellAlignment.LEFT
        }.ToString(12));
        Assert.AreEqual("  Dec-3699  ", new Cell {
            Value = new CellValue {
                StringValue = "657435",
                Type = CellType.NUMBER
            },
            Format = CellFormat.DATE_MY,
            Alignment = CellAlignment.CENTRE
        }.ToString(12));
        Assert.AreEqual("   -666435.0", new Cell {
            Value = new CellValue {
                StringValue = "-666435.0",
                Type = CellType.NUMBER
            },
            Format = CellFormat.DATE_MY
        }.ToString(12));
        Assert.AreEqual("   3058465", new Cell {
            Value = new CellValue {
                StringValue = "3058465",
                Type = CellType.NUMBER
            },
            Format = CellFormat.DATE_DM
        }.ToString(10));
        Assert.AreEqual("NOT A DATE", new Cell {
            Value = new CellValue {
                StringValue = "NOT A DATE",
                Type = CellType.NUMBER
            },
            Format = CellFormat.DATE_DM
        }.ToString(10));
    }
}