// JComLib
// Unit tests for the Parser class
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
// under the License

using System.IO;
using System.Linq;
using JComLib;
using NUnit.Framework;

namespace ComLibTests;

[TestFixture]
public class ParserTest {
    [TestCase("test input", ExpectedResult = new[] { "test", "input" })]
    [TestCase("   test input   ", ExpectedResult = new[] { "test", "input" })]
    [TestCase("'test input'", ExpectedResult = new[] { "test input" })]
    [TestCase("\"test input\"", ExpectedResult = new[] { "test input" })]
    [TestCase("'test' input", ExpectedResult = new[] { "test", "input" })]
    [TestCase("\"test\" input", ExpectedResult = new[] { "test", "input" })]
    public string[] TestRestOfLine(string input) {
        Parser parser = new(input);
        return parser.RestOfLine();
    }

    [TestCase("*.txt", "*.cs")]
    public void TestReadAndExpandWildcards(string pattern1, string pattern2) {
        string[] content = [$"{pattern1} -p \"{pattern2}\""];
        File.WriteAllLines("test.txt", content);
        File.WriteAllLines("test.cs", content);

        Parser parser = new(File.ReadAllText("test.txt"));
        string[] result = parser.ReadAndExpandWildcards().ToArray();
        Assert.AreEqual(result.Length, 2);

        // Results are sorted.
        Assert.IsTrue(result[0] == "./test.cs");
        Assert.IsTrue(result[1] == "./test.txt");
    }

    // Test the behaviour of the NextWord method to return the next word from the
    // string, allowing for quoted phrases.
    [TestCase("test string", ExpectedResult = "test")]
    [TestCase("   test string   ", ExpectedResult = "test")]
    [TestCase("\"test string\"", ExpectedResult = "test string")]
    [TestCase("\'test string\'", ExpectedResult = "test string")]
    [TestCase("\'test string", ExpectedResult = "test string")]
    public string TestNextWord(string input) {
        Parser parser = new(input);
        return parser.NextWord();
    }

    /// <summary>
    /// Test making a copy of an existing parser and verify that
    /// the two function independently.
    /// </summary>
    [Test]
    public void TestParserCopy() {
        Parser parser = new("England expects that every man will do his duty");
        Assert.AreEqual("England", parser.NextWord());
        Assert.AreEqual("expects", parser.NextWord());
        Assert.AreEqual("that", parser.NextWord());

        Parser newParser = new(parser);
        Assert.AreEqual("every", newParser.NextWord());
        Assert.AreEqual("man", newParser.NextWord());

        Assert.AreEqual("every", parser.NextWord());
        Assert.AreEqual("man", parser.NextWord());
    }

    // Test the behaviour of GetChar and PushChar.
    [Test]
    public void TestPushChar_GetChar() {
        Parser parser = new("abcdef");
        Assert.AreEqual('a', parser.GetChar());

        // Push character back
        parser.PushChar('a');
        Assert.AreEqual('a', parser.GetChar());

        // Test reaching end of input
        _ = new Parser(string.Empty);
    }
}