// tests
// Window management
//
// Authors:
//  Steven
//
// Copyright (C) 2025 Steven
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

[TestFixture]
[TestOf(typeof(FixedString))]
public class FixedStringTest {

    /// <summary>
    /// Create an empty fixed string
    /// </summary>
    [Test]
    public void TestSimpleString() {
        FixedString str1 = new("");
        Assert.IsTrue(str1.Length == 0);
        Assert.IsTrue(str1.RealLength == 0);
        Assert.AreEqual("", str1.ToString());
    }

    /// <summary>
    /// Test allocating a FixedString of a given size
    /// </summary>
    [Test]
    public void TestAllocatedSize() {
        FixedString str1 = new(4);
        Assert.IsTrue(str1.Length == 4);
        Assert.IsTrue(str1.RealLength == 0);
        Assert.AreEqual("", str1.ToString());
        Assert.Throws<IndexOutOfRangeException>(delegate { _ = new FixedString(-3); });

        // Set() respects the allocated size
        str1.Set("POTATO");
        Assert.AreEqual("POTA", str1.ToString());
        Assert.IsTrue(str1.Length == 4);
        Assert.IsTrue(str1.RealLength == 4);
        Assert.IsTrue(!str1.IsEmpty);

        // Assignment creates a new string with the length of the assignment
        // The next call to Set() will use the assigned length.
        str1 = "POTATO";
        Assert.AreEqual("POTATO", str1.ToString());
        Assert.IsTrue(str1.Length == 6);
        Assert.IsTrue(str1.RealLength == 6);
        str1.Set("CABBAGE");
        Assert.AreEqual("CABBAG", str1.ToString());
        Assert.IsTrue(str1.Length == 6);
        Assert.IsTrue(str1.RealLength == 6);

        // A shorter assignment
        str1.Set("PEA");
        Assert.AreEqual("PEA", str1.ToString());
        Assert.IsTrue(str1.Length == 6);
        Assert.IsTrue(str1.RealLength == 3);
        Assert.AreEqual("PEA   ", str1.ToCharArray());

        // Empty a string
        str1.Empty();
        Assert.AreEqual("", str1.ToString());
        Assert.AreEqual("      ", str1.ToCharArray());
        Assert.IsTrue(str1.Length == 6);
        Assert.IsTrue(str1.RealLength == 0);
        Assert.IsTrue(str1.IsEmpty);

        // Assigning a FixedString from another FixedString behaves the same
        // way as a string assignment above.
        FixedString str2 = new("PUPIL");
        str1 = str2;
        Assert.AreEqual("PUPIL", str1.ToString());
        Assert.IsTrue(str1.Length == 5);
        Assert.IsTrue(str1.RealLength == 5);

        // Create a FixedString from a character
        FixedString str3 = new('K');
        Assert.AreEqual("K", str3.ToString());
        Assert.IsTrue(str3.Length == 1);
        Assert.IsTrue(str3.RealLength == 1);

        // Create a FixedString from another
        FixedString str4 = new(str2);
        Assert.AreEqual("PUPIL", str4.ToString());
        Assert.IsTrue(str4.Length == 5);
        Assert.IsTrue(str4.RealLength == 5);
    }

    /// <summary>
    /// Test indexing a fixed string
    /// </summary>
    [Test]
    public void TestIndexing() {
        FixedString str2 = new("PUPIL");
        string str1 = "";
        for (int c = str2.Length - 1; c >= 0; c--) {
            str1 += str2[c];
        }
        Assert.AreEqual("LIPUP", str1);

        FixedString str3 = new(8);
        for (int d = 0; d < str2.Length; d++) {
            str3[d] = str2[d];
        }
        Assert.AreEqual("PUPIL", str3.ToString());
        Assert.AreEqual(8, str3.Length);
        Assert.AreEqual(5, str3.RealLength);
        Assert.AreEqual("PUPIL   ", str3.ToCharArray());

        Assert.Throws<IndexOutOfRangeException>(delegate { _ = str3[8]; });
        Assert.Throws<IndexOutOfRangeException>(delegate { str3[8] = 'P'; });
        Assert.Throws<IndexOutOfRangeException>(delegate { _ = str3[-1]; });
        Assert.Throws<IndexOutOfRangeException>(delegate { str3[-1] = 'P'; });
    }

    /// <summary>
    /// Test substrings
    /// </summary>
    [Test]
    public void TestSubstrings() {
        FixedString str1 = new(14);
        str1.Set("DOCTOR WHO!");
        Assert.AreEqual("WHO!   ", str1.Substring(8).ToString());
        Assert.AreEqual("TOR", str1.Substring(4, 6).ToString());
        Assert.Throws<IndexOutOfRangeException>(delegate { str1.Substring(0); });
        Assert.Throws<IndexOutOfRangeException>(delegate { str1.Substring(0, 15); });
        Assert.Throws<IndexOutOfRangeException>(delegate { str1.Substring(6, 5); });

        // Set() substring assignment. The parameters are the 1-based start and
        // end of str2 into which str1 is to be copied
        FixedString str2 = new(10);
        str2.Set(str1, 3, 5);
        Assert.AreEqual("  DOC", str2.ToString());
        str2.Set("MASTER", 6, 10);
        Assert.AreEqual("  DOCMASTE", str2.ToString());

        // Test Substring extraction via ToString(). Unlike Substring(),
        // this is 0-based and takes a length rather than an end index
        Assert.AreEqual("TOR WHO", str1.ToString(3, 7));
    }

    /// <summary>
    /// Test comparisins.
    /// </summary>
    [Test]
    public void TestComparison() {
        FixedString str1 = new("The Full Monty");
        FixedString str2 = str1;
        Assert.AreEqual("The Full Monty", str1.ToString());
        Assert.AreEqual(0, FixedString.Compare(str1, str2));
        Assert.AreEqual(0, FixedString.Compare(str1, str1));
        Assert.AreEqual(1, FixedString.Compare(str1, null));

        // Operator
        Assert.IsTrue(str1 == str2);
        Assert.IsFalse(str1 != str2);

        Assert.GreaterOrEqual(FixedString.Compare(str1, "THE FULL MONTY"), 1);
        Assert.LessOrEqual(FixedString.Compare(str1, "Wizards"), -1);
        Assert.Throws<ArgumentNullException>(delegate { FixedString.Compare(null, str2); });
    }
}