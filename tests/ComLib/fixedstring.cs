// JComLib
// Unit tests for the FixedString class
//
// Authors:
//  Steve Palmer
//
// Copyright (C) 2025 Steve Palmer
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
using System.Collections.Generic;
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
        Assert.Throws<IndexOutOfRangeException>(delegate { str2.Set("MASTER", 0, 11); });
        Assert.Throws<IndexOutOfRangeException>(delegate { str1.Set(str2, 0, 11); });

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

        Assert.AreEqual(0, str1.Compare(str2));
        Assert.AreEqual(0, str1.Compare(str1));
        Assert.AreEqual(1, str1.Compare(null));

        Assert.AreEqual(str1, str2);
        Assert.IsTrue(str1.Equals((object)str2));
        Assert.IsFalse(str1.Equals(new object()));

        // Operator
        Assert.IsTrue(str1 == str2);
        Assert.IsFalse(str1 != str2);

        Assert.GreaterOrEqual(FixedString.Compare(str1, "THE FULL MONTY"), 1);
        Assert.LessOrEqual(FixedString.Compare(str1, "Wizards"), -1);
        Assert.Throws<ArgumentNullException>(delegate { FixedString.Compare(null, str2); });
    }

    /// <summary>
    /// Test the use of FixedString in a dictionary. This tests equality and hashing.
    /// </summary>
    [Test]
    public void TestDictionary() {

        // First check hashing of different strings
        // str1, str2 and str3 are all different objects with the same value.
        FixedString str1 = "Consideration";
        FixedString str2 = "Consideration";
        FixedString str3 = str1;
        FixedString str4 = "Accomplishment";

        // Ensure hash of the same object is identical on each run
        Assert.AreEqual(str1.GetHashCode(), str1.GetHashCode());
        Assert.AreEqual(str1.GetHashCode(), str2.GetHashCode());
        Assert.AreEqual(str3.GetHashCode(), str1.GetHashCode());
        Assert.AreNotEqual(str1.GetHashCode(), str4.GetHashCode());

        FixedString redRGB = new("FF0000");
        FixedString greenRGB = "00FF00";

        // String is cast to FixedString.
        Dictionary<FixedString, FixedString> dic = new() {
            { "Blue", "0000FF" },
            { "Red", redRGB },
            { "Green", greenRGB }
        };
        Assert.IsTrue(dic["Blue"].Equals("0000FF"));
        Assert.IsTrue(dic["Blue"] == "0000FF");
        Assert.IsTrue(dic["Red"] == redRGB);
        Assert.IsTrue(dic["Green"].Compare("00FF00") == 0);

        // Test using FixedString keys
        FixedString blue = new("Blue");
        FixedString green = new("Green");
        FixedString red = new("Red");
        Assert.IsTrue(dic[blue] == "0000FF");
        Assert.IsTrue(dic[red] == "FF0000");
        Assert.IsTrue(dic[green].Compare(greenRGB) == 0);
    }

    /// <summary>
    /// Test concatenation.
    /// </summary>
    [Test]
    public void TestConcatenation() {
        FixedString str1 = new(22);
        FixedString str2 = new("In the bleak ");
        FixedString str3 = new("midwinter");

        // Test opAddition
        str1.Set(str2 + str3);
        Assert.AreEqual("In the bleak midwinter", str1.ToString());
        Assert.AreEqual(22, str1.RealLength);

        // Ensure strings are truncated to fit the destination
        FixedString str4 = new("NOW IS THE");
        FixedString str5 = new(14);
        str5.Set(str4 + " TIME FOR ALL GOOD");
        Assert.AreEqual("NOW IS THE TIM", str5.ToString());
        Assert.AreEqual(14, str5.RealLength);

        // Test of blanks
        str5.Set(new FixedString("") + str5 + "");
        Assert.AreEqual("NOW IS THE TIM", str5.ToString());
        Assert.AreEqual(14, str5.RealLength);

        // Test of emptiness
        FixedString str6 = new(35);
        str6.Set(str4 + " TIME FOR ALL GOOD MEN");
        Assert.AreEqual("NOW IS THE TIME FOR ALL GOOD MEN", str6.ToString());
        Assert.AreEqual("NOW IS THE TIME FOR ALL GOOD MEN   ", str6.ToCharArray());
        Assert.AreEqual(32, str6.RealLength);
        Assert.AreEqual(35, str6.Length);

        Assert.Throws<ArgumentNullException>(delegate { _ = new FixedString("Blue") + null; });
        Assert.Throws<ArgumentNullException>(delegate { _ = null + new FixedString("Blue"); });
    }

    /// <summary>
    /// Test merging. Unlike concatenation, merging works on the full FixedString
    /// allocation size.
    /// </summary>
    [Test]
    public void TestMerging() {
        FixedString str1 = new(10);
        FixedString str2 = new(12);
        str1.Set("This is ");
        str2.Set("a string");
        FixedString str3 = FixedString.Merge(str1, str2);
        Assert.AreEqual(22, str3.Length);
        Assert.AreEqual("This is   a string    ", str3.ToString());

        Assert.Throws<ArgumentNullException>(delegate { _ = FixedString.Merge(new FixedString("Blue"), null); });
        Assert.Throws<ArgumentNullException>(delegate { _ = FixedString.Merge(null, new FixedString("Blue")); });
    }

    /// <summary>
    /// Test IndexOf for searching a string and returning the 0-based index
    /// </summary>
    [Test]
    public void TestIndexOf() {
        FixedString str1 = "A little rose of happiness!";
        FixedString str2 = new(3);
        str2.Set("of");

        Assert.AreEqual(0, str1.IndexOf(new FixedString("A little")));
        Assert.AreEqual(9, str1.IndexOf("rose"));
        Assert.AreEqual(-1, str1.IndexOf("GANYMEDE"));
        Assert.AreEqual(10, str1.IndexOf("o"));

        // This will match the space following 'of' in str2
        Assert.AreEqual(14, str1.IndexOf(str2));
        Assert.Throws<ArgumentNullException>(delegate { _ = str1.IndexOf(null); });
    }
}