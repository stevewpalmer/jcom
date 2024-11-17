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
using JComLib;
using NUnit.Framework;

namespace ComLibTests;

public class TestANSIText {

    // Test a plain ANSI text string with no formatting.
    [Test]
    public void TestPlainString() {
        AnsiText ansi = new AnsiText([
            new AnsiText.AnsiTextSpan("HELLO WORLD")
        ]);
        Assert.IsTrue(ansi.Spans.Count == 1);
        Assert.AreEqual("HELLO WORLD", ansi.Spans[0].Text);
        Assert.AreEqual("HELLO WORLD", ansi.Text);
        Assert.AreEqual("\u001b[97;40m", ansi.Spans[0].CS);
    }

    // Test creating an ANSI text string with the foreground colour
    // set to red and the background colour to cyan.
    [Test]
    public void TestInitialString() {
        AnsiText ansi = new AnsiText([
            new AnsiText.AnsiTextSpan("HELLO WORLD") {
                ForegroundColour = AnsiColour.Red,
                BackgroundColour = AnsiColour.Cyan
            }
        ]);
        Assert.IsTrue(ansi.Spans.Count == 1);
        Assert.AreEqual("HELLO WORLD", ansi.Spans[0].Text);
        Assert.AreEqual("HELLO WORLD", ansi.Text);
        Assert.AreEqual("\u001b[31;46m", ansi.Spans[0].CS);
    }

    // Test parsing a text string with the ANSI colour escape sequence that
    // sets the foreground colour to red and the background colour to cyan.
    [Test]
    public void TestEmbeddedSequence() {
        AnsiText ansi = new AnsiText([
            new AnsiText.AnsiTextSpan("HELLO "),
            new AnsiText.AnsiTextSpan("WORLD") {
                ForegroundColour = AnsiColour.Red,
                BackgroundColour = AnsiColour.Green
            },
        ]);

        Assert.IsTrue(ansi.Spans.Count == 2);
        Assert.AreEqual("HELLO ", ansi.Spans[0].Text);
        Assert.AreEqual("\u001b[97;40m", ansi.Spans[0].CS);
        Assert.AreEqual("WORLD", ansi.Spans[1].Text);
        Assert.AreEqual("\u001b[31;42m", ansi.Spans[1].CS);
        Assert.AreEqual("HELLO WORLD", ansi.Text);
    }

    // Test parsing a text string with the ANSI colour escape sequence that
    // sets the foreground colour to red and the background colour to cyan.
    [Test]
    public void TestMultipleSequences() {
        AnsiText ansi = new AnsiText([
            new AnsiText.AnsiTextSpan("HELLO "),
            new AnsiText.AnsiTextSpan("WORLD") {
                ForegroundColour = AnsiColour.Yellow,
                BackgroundColour = AnsiColour.Cyan
            },
            new AnsiText.AnsiTextSpan(" WELCOME TO LAS "),
            new AnsiText.AnsiTextSpan("VEGAS") {
                ForegroundColour = AnsiColour.Blue,
                BackgroundColour = AnsiColour.Magenta
            }
        ]);

        Assert.IsTrue(ansi.Spans.Count == 4);
        Assert.AreEqual("HELLO ", ansi.Spans[0].Text);
        Assert.AreEqual("\u001b[97;40m", ansi.Spans[0].CS);
        Assert.AreEqual("WORLD", ansi.Spans[1].Text);
        Assert.AreEqual("\u001b[33;46m", ansi.Spans[1].CS);
        Assert.AreEqual(" WELCOME TO LAS ", ansi.Spans[2].Text);
        Assert.AreEqual("\u001b[97;40m", ansi.Spans[2].CS);
        Assert.AreEqual("VEGAS", ansi.Spans[3].Text);
        Assert.AreEqual("\u001b[34;45m", ansi.Spans[3].CS);
        Assert.AreEqual("HELLO WORLD WELCOME TO LAS VEGAS", ansi.Text);
    }

    // Test that the Length property returns the raw string length
    [Test]
    public void TestLength() {
        AnsiText ansi = new AnsiText([
            new AnsiText.AnsiTextSpan("HELLO "),
            new AnsiText.AnsiTextSpan("WORLD") {
                ForegroundColour = AnsiColour.Yellow,
                BackgroundColour = AnsiColour.Cyan
            },
            new AnsiText.AnsiTextSpan(" WELCOME TO LAS "),
            new AnsiText.AnsiTextSpan("VEGAS") {
                ForegroundColour = AnsiColour.Blue,
                BackgroundColour = AnsiColour.Magenta
            }
        ]);
        Assert.AreEqual(32, ansi.Length);
        Assert.AreEqual(0, new AnsiText(Array.Empty<AnsiText.AnsiTextSpan>()).Length);
    }

    // Test substring extraction
    [Test]
    public void TestSubstring() {
        AnsiText simple = new AnsiText([
            new AnsiText.AnsiTextSpan("HELLO WORLD!")
        ]);
        Assert.AreEqual("LO WO", simple.Substring(3, 5).Text);
        Assert.AreEqual("WORLD!", simple.Substring(6, 20).Text);
        Assert.AreEqual("", simple.Substring(12, 20).Text);
        Assert.AreEqual("", simple.Substring(0, 0).Text);

        AnsiText ansi = new AnsiText([
            new AnsiText.AnsiTextSpan("HELLO "),
            new AnsiText.AnsiTextSpan("WORLD") {
                ForegroundColour = AnsiColour.Yellow,
                BackgroundColour = AnsiColour.Cyan
            },
            new AnsiText.AnsiTextSpan(" WELCOME TO LAS "),
            new AnsiText.AnsiTextSpan("VEGAS") {
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
    }

    // Test changing the style of a portion of an AnsiText
    [Test]
    public void TestStyle() {
        AnsiText simple = new AnsiText([
            new AnsiText.AnsiTextSpan("HELLO WORLD!")
        ]);
        simple.Style(0, 5, AnsiColour.Cyan, AnsiColour.Green);
        Assert.AreEqual(2, simple.Spans.Count);
        Assert.AreEqual("\u001b[36;42m", simple.Spans[0].CS);
        Assert.AreEqual(" WORLD!", simple.Spans[1].Text);
        Assert.AreEqual("\u001b[97;40m", simple.Spans[1].CS);

        AnsiText ansi = new AnsiText([
            new AnsiText.AnsiTextSpan("HELLO "),
            new AnsiText.AnsiTextSpan("WORLD") {
                ForegroundColour = AnsiColour.Yellow,
                BackgroundColour = AnsiColour.Cyan
            },
            new AnsiText.AnsiTextSpan(" WELCOME TO LAS "),
            new AnsiText.AnsiTextSpan("VEGAS") {
                ForegroundColour = AnsiColour.Blue,
                BackgroundColour = AnsiColour.Magenta
            }
        ]);
        ansi.Style(8, 10, AnsiColour.Cyan, AnsiColour.Green);
        Assert.AreEqual(5, ansi.Spans.Count);
        Assert.AreEqual("HELLO ", ansi.Spans[0].Text);
        Assert.AreEqual("WO", ansi.Spans[1].Text);
        Assert.AreEqual("RLD WELCOM", ansi.Spans[2].Text);
        Assert.AreEqual(AnsiColour.Cyan, ansi.Spans[2].ForegroundColour);
        Assert.AreEqual(AnsiColour.Green, ansi.Spans[2].BackgroundColour);
        Assert.AreEqual("E TO LAS ", ansi.Spans[3].Text);
        Assert.AreEqual("VEGAS", ansi.Spans[4].Text);
    }
}