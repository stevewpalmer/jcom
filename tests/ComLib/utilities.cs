// JComLib
// Unit tests for the Utilities class
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

using System;
using NUnit.Framework;

namespace ComLibTests;

public enum EnumTester {
    NONE,

    [System.ComponentModel.Description("Test value 1")]
    TESTVALUE1
}

[TestFixture]
public class Utilities {

    [Test]
    public void TestAddExtensionIfMissing() {
        Assert.AreEqual("Filename.extension", JComLib.Utilities.AddExtensionIfMissing("Filename", "extension"));
        Assert.AreEqual("Filename.extension", JComLib.Utilities.AddExtensionIfMissing("Filename.extension", "ext"));
        Assert.AreEqual(null, JComLib.Utilities.AddExtensionIfMissing(null, "extension"));
        Assert.AreEqual("", JComLib.Utilities.AddExtensionIfMissing("", "extension"));
    }

    [Test]
    public void TestGetEnumDescription() {
        Assert.AreEqual("Test value 1", JComLib.Utilities.GetEnumDescription(EnumTester.TESTVALUE1));
        Assert.AreEqual("NONE", JComLib.Utilities.GetEnumDescription(EnumTester.NONE));
    }

    // Test the SpanBound function.
    [Test]
    public void TestSpanBound() {
        Assert.AreEqual("abcdef", JComLib.Utilities.SpanBound("abcdefghi", 0, 6));
        Assert.AreEqual("abcdefghi", JComLib.Utilities.SpanBound("abcdefghi", 0, 20));
        Assert.AreEqual("", JComLib.Utilities.SpanBound("abcdefghi", 5, 0));
        Assert.AreEqual("", JComLib.Utilities.SpanBound("abcdefghi", 15, 20));
        Assert.AreEqual("", JComLib.Utilities.SpanBound("abcdefghi", 60, 20));
    }

    // Test the ConstrainAndWrap function
    [Test]
    public void TestConstrainAndWrap() {
        Assert.AreEqual(10, JComLib.Utilities.ConstrainAndWrap(15, 10, 11));
        Assert.AreEqual(14, JComLib.Utilities.ConstrainAndWrap(5, 10, 15));
        Assert.AreEqual(41, JComLib.Utilities.ConstrainAndWrap(55, 41, 43));
        Assert.AreEqual(int.MinValue, JComLib.Utilities.ConstrainAndWrap(int.MaxValue, int.MinValue, int.MaxValue));
    }

    // Test the CentreString function
    [Test]
    public void TestCentreString() {
        Assert.AreEqual("  abcdef  ", JComLib.Utilities.CentreString("abcdef", 10));
        Assert.AreEqual("abcdef", JComLib.Utilities.CentreString("abcdef", 6));
        Assert.AreEqual("a", JComLib.Utilities.CentreString("abcdef", 1));
        Assert.Throws(typeof(ArgumentNullException), delegate { JComLib.Utilities.CentreString(null, 10); });
        Assert.AreEqual("          ", JComLib.Utilities.CentreString("", 10));
    }
}