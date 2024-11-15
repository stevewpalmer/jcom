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

    // Test parsing a plain text string with no ANSI escape sequences.
    [Test]
    public void TestPlainString() {
        AnsiText ansi = new AnsiText("HELLO WORLD");
        Assert.IsTrue(ansi.Spans.Count == 1);
        Assert.AreEqual("HELLO WORLD", ansi.Spans[0].Text);
        Assert.AreEqual(ConsoleColor.White, ansi.Spans[0].Foreground);
        Assert.AreEqual(ConsoleColor.Black, ansi.Spans[0].Background);
    }

    // Test parsing a text string with the ANSI colour escape sequence that
    // sets the foreground colour to red and the background colour to cyan.
    [Test]
    public void TestInitialString() {
        AnsiText ansi = new AnsiText("\u001b[31;36mHELLO WORLD");
        Assert.IsTrue(ansi.Spans.Count == 1);
        Assert.AreEqual("HELLO WORLD", ansi.Spans[0].Text);
        Assert.AreEqual(ConsoleColor.Red, ansi.Spans[0].Foreground);
        Assert.AreEqual(ConsoleColor.Cyan, ansi.Spans[0].Background);
    }

    // Test parsing a malformed escape sequence.
    [Test]
    public void TestBadEscapeString() {
        Assert.Throws(typeof(FormatException), delegate { _ = new AnsiText("\u001bHELLO WORLD");});
        Assert.Throws(typeof(FormatException), delegate { _ = new AnsiText("\u001b[12;12HELLO WORLD");});
        Assert.Throws(typeof(FormatException), delegate { _ = new AnsiText("\u001b[");});

        AnsiText ansi = new AnsiText("\u001b[40;40mHELLO WORLD");
        Assert.IsTrue(ansi.Spans.Count == 1);
        Assert.AreEqual("HELLO WORLD", ansi.Spans[0].Text);
        Assert.AreEqual(ConsoleColor.Gray, ansi.Spans[0].Foreground);
        Assert.AreEqual(ConsoleColor.Gray, ansi.Spans[0].Background);
    }

    // Test parsing a text string with the ANSI colour escape sequence that
    // sets the foreground colour to red and the background colour to cyan.
    [Test]
    public void TestEmbeddedSequence() {
        AnsiText ansi = new AnsiText("HELLO \u001b[31;32mWORLD");
        Assert.IsTrue(ansi.Spans.Count == 2);
        Assert.AreEqual("HELLO ", ansi.Spans[0].Text);
        Assert.AreEqual(ConsoleColor.White, ansi.Spans[0].Foreground);
        Assert.AreEqual(ConsoleColor.Black, ansi.Spans[0].Background);
        Assert.AreEqual("WORLD", ansi.Spans[1].Text);
        Assert.AreEqual(ConsoleColor.Red, ansi.Spans[1].Foreground);
        Assert.AreEqual(ConsoleColor.Green, ansi.Spans[1].Background);
    }

    // Test parsing a text string with the ANSI colour escape sequence that
    // sets the foreground colour to red and the background colour to cyan.
    [Test]
    public void TestMultipleSequences() {
        AnsiText ansi = new AnsiText("HELLO \u001b[33;36mWORLD\u001b[0m WELCOME TO LAS \u001b[34;35mVEGAS");
        Assert.IsTrue(ansi.Spans.Count == 4);
        Assert.AreEqual("HELLO ", ansi.Spans[0].Text);
        Assert.AreEqual(ConsoleColor.White, ansi.Spans[0].Foreground);
        Assert.AreEqual(ConsoleColor.Black, ansi.Spans[0].Background);
        Assert.AreEqual("WORLD", ansi.Spans[1].Text);
        Assert.AreEqual(ConsoleColor.Yellow, ansi.Spans[1].Foreground);
        Assert.AreEqual(ConsoleColor.Cyan, ansi.Spans[1].Background);
        Assert.AreEqual(" WELCOME TO LAS ", ansi.Spans[2].Text);
        Assert.AreEqual(ConsoleColor.White, ansi.Spans[2].Foreground);
        Assert.AreEqual(ConsoleColor.Black, ansi.Spans[2].Background);
        Assert.AreEqual("VEGAS", ansi.Spans[3].Text);
        Assert.AreEqual(ConsoleColor.Blue, ansi.Spans[3].Foreground);
        Assert.AreEqual(ConsoleColor.Magenta, ansi.Spans[3].Background);
    }
}