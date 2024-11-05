// JComal
// Unit tests for the Options class
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

using CCompiler;
using JComal;
using NUnit.Framework;

namespace ComalTests;

[TestFixture]
public class OptionsTest {

    // Validate the Tokens IDs are consistent.
    [Test]
    public void ValidateTokenEnums() {
        Assert.AreEqual((int)TokenID.KENDCASE, 50);
        Assert.AreEqual((int)TokenID.KMOD, 100);
        Assert.AreEqual((int)TokenID.KRESTORE, 123);
    }

    // Verify that the default options are what we expect.
    [Test]
    public void ValidateDefaultOptions() {
        ComalOptions opts = new();

        Assert.IsFalse(opts.Interactive);
        Assert.IsFalse(opts.GenerateDebug);
        Assert.IsFalse(opts.Run);
    }

    // Verify that options are correctly parsed.
    // Because no filename is specified, Interactive will be set.
    [Test]
    public void ValidateOptionParsing() {
        ComalOptions opts = new();
        string[] args = {
            "--noinline",
            "--invalidoption",
            "--debug"
        };
        Assert.IsFalse(opts.Parse(args));

        Assert.IsFalse(opts.Inline);
        Assert.IsTrue(opts.GenerateDebug);
        Assert.IsFalse(opts.Interactive);
        Assert.IsTrue(opts.Messages.Count == 1);
        Assert.IsTrue(opts.Messages[0].Code == MessageCode.BADOPTION);
    }

    // Verify that input filenames are correctly parsed
    [Test]
    public void ValidateFilenameParsing() {
        ComalOptions opts = new();
        string[] args = {
            "testfile1.cml",
            "--debug",
            "testfile2.cml"
        };

        Assert.IsTrue(opts.Parse(args));
        Assert.IsTrue(opts.SourceFiles.Count == 2);
        Assert.AreEqual(opts.SourceFiles[0], "testfile1.cml");
        Assert.AreEqual(opts.SourceFiles[1], "testfile2.cml");
        Assert.IsTrue(opts.GenerateDebug);
    }

    // Verify that a filename without an extension is assumed to
    // have the list file extension by default.
    [Test]
    public void ValidateFilenameExtension() {
        ComalOptions opts = new();
        string[] args = {
            "testfile1.",
            "--debug",
            "testfile2"
        };

        Assert.IsTrue(opts.Parse(args));
        Assert.IsTrue(opts.SourceFiles.Count == 2);
        Assert.AreEqual(opts.SourceFiles[0], "testfile1.lst");
        Assert.AreEqual(opts.SourceFiles[1], "testfile2.lst");
        Assert.IsTrue(opts.GenerateDebug);
    }
}