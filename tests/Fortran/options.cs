// JFortran Compiler
// Unit tests for the Options class
//
// Authors:
//  Steve Palmer
//
// Copyright (C) 2013 Steve Palmer
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
using JFortran;
using NUnit.Framework;

namespace FortranTests {
    [TestFixture]
    public class OptionsTest {

        // Validate the MessageCode table hasn't been borked
        [Test]
        public void ValidateMessageCodeTable() {
            Assert.AreEqual((int)MessageCode.NONE, 0);
            Assert.AreEqual((int)MessageCode.EXPECTEDTOKEN, 0x1000);
            Assert.AreEqual((int)MessageCode.BADOPTION, 0x103E);
        }

        // Verify that the default options are what we expect.
        [Test]
        public void ValidateDefaultOptions() {
            FortranOptions opts = new();

            Assert.IsFalse(opts.Backslash);
            Assert.IsFalse(opts.GenerateDebug);
        }

        // Verify that options are correctly parsed.
        [Test]
        public void ValidateOptionParsing() {
            FortranOptions opts = new();
            string[] args = {
                "--backslash",
                "--invalidoption",
                "--debug" };
            Assert.IsFalse(opts.Parse(args));

            Assert.IsTrue(opts.Backslash);
            Assert.IsTrue(opts.Messages.Count == 1);
            Assert.IsTrue(opts.Messages[0].Code == MessageCode.BADOPTION);
        }

        // Verify that input filenames are correctly parsed
        [Test]
        public void ValidateFilenameParsing() {
            FortranOptions opts = new();
            string[] args = {
                "--backslash",
                "testfile1.f",
                "--debug",
                "testfile2.f"
            };

            Assert.IsTrue(opts.Parse(args));
            Assert.IsTrue(opts.SourceFiles.Count == 2);
            Assert.AreEqual(opts.SourceFiles[0], "testfile1.f");
            Assert.AreEqual(opts.SourceFiles[1], "testfile2.f");
            Assert.IsTrue(opts.Backslash);
            Assert.IsTrue(opts.GenerateDebug);
        }
    }
}

