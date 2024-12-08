// JComLib
// Unit tests for the AnsiText class
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
using JCalcLib;
using JComLib;
using NUnit.Framework;

namespace ComLibTests;

public class TestANSIText {
    private const string CSI = @"[";

    // Verify the AnsiColour enum values
    [Test]
    public void VerifyValues() {
        Assert.AreEqual(AnsiColour.Black, 30);
        Assert.AreEqual(AnsiColour.Red, 31);
        Assert.AreEqual(AnsiColour.Green, 32);
        Assert.AreEqual(AnsiColour.Yellow, 33);
        Assert.AreEqual(AnsiColour.Blue, 34);
        Assert.AreEqual(AnsiColour.Magenta, 35);
        Assert.AreEqual(AnsiColour.Cyan, 36);
        Assert.AreEqual(AnsiColour.White, 37);
        Assert.AreEqual(AnsiColour.Grey, 90);
        Assert.AreEqual(AnsiColour.BrightRed, 91);
        Assert.AreEqual(AnsiColour.BrightGreen, 92);
        Assert.AreEqual(AnsiColour.BrightYellow, 93);
        Assert.AreEqual(AnsiColour.BrightBlue, 94);
        Assert.AreEqual(AnsiColour.BrightMagenta, 95);
        Assert.AreEqual(AnsiColour.BrightCyan, 96);
        Assert.AreEqual(AnsiColour.BrightWhite, 97);
    }

    // Test a plain ANSI text string with no formatting.
    [Test]
    public void TestPlainString() {
        AnsiText ansi = new([
            new AnsiTextSpan("HELLO WORLD")
        ]);
        Assert.IsTrue(ansi.Spans.Count == 1);
        Assert.AreEqual("HELLO WORLD", ansi.Spans[0].Text);
        Assert.AreEqual("HELLO WORLD", ansi.Text);
        Assert.AreEqual($"{CSI}97;40m", ansi.Spans[0].CS);
    }

    // Test creating an ANSI text string with the foreground colour
    // set to red and the background colour to cyan.
    [Test]
    public void TestInitialString() {
        AnsiText ansi = new([
            new AnsiTextSpan("HELLO WORLD") {
                ForegroundColour = AnsiColour.Red,
                BackgroundColour = AnsiColour.Cyan
            }
        ]);
        Assert.IsTrue(ansi.Spans.Count == 1);
        Assert.AreEqual("HELLO WORLD", ansi.Spans[0].Text);
        Assert.AreEqual("HELLO WORLD", ansi.Text);
        Assert.AreEqual($"{CSI}31;46m", ansi.Spans[0].CS);
    }

    // Test parsing a text string with the ANSI colour escape sequence that
    // sets the foreground colour to red and the background colour to cyan.
    [Test]
    public void TestEmbeddedSequence() {
        AnsiText ansi = new([
            new AnsiTextSpan("HELLO "),
            new AnsiTextSpan("WORLD") {
                ForegroundColour = AnsiColour.Red,
                BackgroundColour = AnsiColour.Green
            }
        ]);

        Assert.IsTrue(ansi.Spans.Count == 2);
        Assert.AreEqual("HELLO ", ansi.Spans[0].Text);
        Assert.AreEqual($"{CSI}97;40m", ansi.Spans[0].CS);
        Assert.AreEqual("WORLD", ansi.Spans[1].Text);
        Assert.AreEqual($"{CSI}31;42m", ansi.Spans[1].CS);
        Assert.AreEqual("HELLO WORLD", ansi.Text);
    }

    // Test parsing a text string with the ANSI colour escape sequence that
    // sets the foreground colour to red and the background colour to cyan.
    [Test]
    public void TestMultipleSequences() {
        AnsiText ansi = new([
            new AnsiTextSpan("HELLO "),
            new AnsiTextSpan("WORLD") {
                ForegroundColour = AnsiColour.Yellow,
                BackgroundColour = AnsiColour.Cyan
            },
            new AnsiTextSpan(" WELCOME TO LAS "),
            new AnsiTextSpan("VEGAS") {
                ForegroundColour = AnsiColour.Blue,
                BackgroundColour = AnsiColour.Magenta
            }
        ]);

        Assert.IsTrue(ansi.Spans.Count == 4);
        Assert.AreEqual("HELLO ", ansi.Spans[0].Text);
        Assert.AreEqual($"{CSI}97;40m", ansi.Spans[0].CS);
        Assert.AreEqual("WORLD", ansi.Spans[1].Text);
        Assert.AreEqual($"{CSI}33;46m", ansi.Spans[1].CS);
        Assert.AreEqual(" WELCOME TO LAS ", ansi.Spans[2].Text);
        Assert.AreEqual($"{CSI}97;40m", ansi.Spans[2].CS);
        Assert.AreEqual("VEGAS", ansi.Spans[3].Text);
        Assert.AreEqual($"{CSI}34;45m", ansi.Spans[3].CS);
        Assert.AreEqual("HELLO WORLD WELCOME TO LAS VEGAS", ansi.Text);
    }

    // Test that the Length property returns the raw string length
    [Test]
    public void TestLength() {
        AnsiText ansi = new([
            new AnsiTextSpan("HELLO "),
            new AnsiTextSpan("WORLD") {
                ForegroundColour = AnsiColour.Yellow,
                BackgroundColour = AnsiColour.Cyan
            },
            new AnsiTextSpan(" WELCOME TO LAS "),
            new AnsiTextSpan("VEGAS") {
                ForegroundColour = AnsiColour.Blue,
                BackgroundColour = AnsiColour.Magenta
            }
        ]);
        Assert.AreEqual(32, ansi.Length);
        Assert.AreEqual(0, new AnsiText(Array.Empty<AnsiTextSpan>()).Length);
    }

    // Test alignments
    [Test]
    public void TestAlignments() {
        Cell leftCell = new() { Alignment = CellAlignment.LEFT };
        Cell rightCell = new() { Alignment = CellAlignment.RIGHT };
        Cell centreCell = new() { Alignment = CellAlignment.CENTRE };
        Cell generalCell = new() { Value = new Variant(12), Alignment = CellAlignment.GENERAL };
        Cell generalTextCell = new() { Value = new Variant("MOUSE"), Alignment = CellAlignment.GENERAL };

        Assert.Throws(typeof(ArgumentOutOfRangeException), delegate { _ = new Cell { Alignment = (CellAlignment)10 }.AnsiAlignment; });

        Assert.AreEqual(AnsiAlignment.LEFT, leftCell.AnsiAlignment);
        Assert.AreEqual(AnsiAlignment.RIGHT, rightCell.AnsiAlignment);
        Assert.AreEqual(AnsiAlignment.CENTRE, centreCell.AnsiAlignment);
        Assert.AreEqual(AnsiAlignment.RIGHT, generalCell.AnsiAlignment);
        Assert.AreEqual(AnsiAlignment.LEFT, generalTextCell.AnsiAlignment);

        AnsiText none = new([
            new AnsiTextSpan("  PRETTY EASY  ") {
                Width = 30,
                Alignment = AnsiAlignment.NONE
            }
        ]);
        Assert.AreEqual(30, none.Length);
        Assert.AreEqual($"{CSI}97;40m  PRETTY EASY  {CSI}0m{CSI}97;40m               {CSI}0m", none.EscapedText);

        AnsiText simple = new([
            new AnsiTextSpan("HELLO") {
                Width = 10,
                Alignment = AnsiAlignment.LEFT
            },
            new AnsiTextSpan("WORLD!") {
                Width = 10,
                Alignment = AnsiAlignment.RIGHT
            }
        ]);
        Assert.AreEqual($"{CSI}97;40mHELLO{CSI}0m{CSI}97;40m     {CSI}0m{CSI}97;40m    {CSI}0m{CSI}97;40mWORLD!{CSI}0m", simple.EscapedText);

        AnsiText centered = new([
            new AnsiTextSpan("Centre") {
                Width = 11,
                Alignment = AnsiAlignment.CENTRE
            }
        ]);
        Assert.AreEqual($"{CSI}97;40m   {CSI}0m{CSI}97;40mCentre{CSI}0m{CSI}97;40m  {CSI}0m", centered.EscapedText);
    }

    // Test substring extraction
    [Test]
    public void TestSubstring() {
        AnsiText simple = new([
            new AnsiTextSpan("HELLO WORLD!")
        ]);
        Assert.AreEqual("LO WO", simple.Substring(3, 5).Text);
        Assert.AreEqual("WORLD!", simple.Substring(6, 20).Text);
        Assert.AreEqual("", simple.Substring(12, 20).Text);
        Assert.AreEqual("", simple.Substring(0, 0).Text);

        AnsiText longer = new AnsiText([
            new AnsiTextSpan("HELLO WORLD!") {
                ForegroundColour = AnsiColour.Blue,
                BackgroundColour = AnsiColour.Magenta
            }
        ]).Substring(3, 5);
        Assert.AreEqual("LO WO", longer.Spans[0].Text);
        Assert.AreEqual(AnsiColour.Blue, longer.Spans[0].ForegroundColour);
        Assert.AreEqual(AnsiColour.Magenta, longer.Spans[0].BackgroundColour);

        AnsiText ansi = new([
            new AnsiTextSpan("HELLO "),
            new AnsiTextSpan("WORLD") {
                ForegroundColour = AnsiColour.Yellow,
                BackgroundColour = AnsiColour.Cyan
            },
            new AnsiTextSpan(" WELCOME TO LAS "),
            new AnsiTextSpan("VEGAS") {
                ForegroundColour = AnsiColour.Blue,
                BackgroundColour = AnsiColour.Magenta
            }
        ]);
        Assert.AreEqual("HELLO WORLD ", ansi.Substring(0, 12).Text);
        Assert.AreEqual(3, ansi.Substring(0, 12).Spans.Count);
        Assert.AreEqual("WORLD ", ansi.Substring(6, 6).Text);
        Assert.AreEqual(2, ansi.Substring(6, 6).Spans.Count);
        Assert.AreEqual("VEGAS", ansi.Substring(27, 10).Text);
        Assert.AreEqual(1, ansi.Substring(27, 10).Spans.Count);

        AnsiText ansi2 = ansi.Substring(9, 10);
        Assert.AreEqual(2, ansi2.Spans.Count);
        Assert.AreEqual("LD", ansi2.Spans[0].Text);
        Assert.AreEqual(AnsiColour.Yellow, ansi2.Spans[0].ForegroundColour);
        Assert.AreEqual(AnsiColour.Cyan, ansi2.Spans[0].BackgroundColour);
        Assert.AreEqual(" WELCOME", ansi2.Spans[1].Text);
        Assert.AreEqual(AnsiColour.BrightWhite, ansi2.Spans[1].ForegroundColour);
        Assert.AreEqual(AnsiColour.Black, ansi2.Spans[1].BackgroundColour);
    }

    // Test changing the style of a portion of an AnsiText
    [Test]
    public void TestStyle() {
        AnsiText simple = new([
            new AnsiTextSpan("HELLO WORLD!")
        ]);
        simple.Style(0, 5, AnsiColour.Cyan, AnsiColour.Green);
        Assert.AreEqual(2, simple.Spans.Count);
        Assert.AreEqual($"{CSI}36;42m", simple.Spans[0].CS);
        Assert.AreEqual(" WORLD!", simple.Spans[1].Text);
        Assert.AreEqual($"{CSI}97;40m", simple.Spans[1].CS);

        AnsiText ansi = new([
            new AnsiTextSpan("HELLO "),
            new AnsiTextSpan("WORLD") {
                ForegroundColour = AnsiColour.Yellow,
                BackgroundColour = AnsiColour.Cyan
            },
            new AnsiTextSpan(" WELCOME TO LAS "),
            new AnsiTextSpan("VEGAS") {
                ForegroundColour = AnsiColour.Blue,
                BackgroundColour = AnsiColour.Magenta
            }
        ]);
        ansi.Style(8, 10, AnsiColour.Cyan, AnsiColour.Green);
        Assert.AreEqual(6, ansi.Spans.Count);
        Assert.AreEqual("HELLO ", ansi.Spans[0].Text);
        Assert.AreEqual("WO", ansi.Spans[1].Text);
        Assert.AreEqual("RLD", ansi.Spans[2].Text);
        Assert.AreEqual(AnsiColour.Cyan, ansi.Spans[2].ForegroundColour);
        Assert.AreEqual(AnsiColour.Green, ansi.Spans[2].BackgroundColour);
        Assert.AreEqual(" WELCOM", ansi.Spans[3].Text);
        Assert.AreEqual(AnsiColour.Cyan, ansi.Spans[3].ForegroundColour);
        Assert.AreEqual(AnsiColour.Green, ansi.Spans[3].BackgroundColour);
        Assert.AreEqual("E TO LAS ", ansi.Spans[4].Text);
        Assert.AreEqual("VEGAS", ansi.Spans[5].Text);
    }
}