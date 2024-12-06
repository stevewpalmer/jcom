// JComLib
// Unit tests for Variant class
//
// Authors:
//  Steve Palmer
//
// Copyright (C) 2021-2024 Steve Palmer
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
using System.Collections.Generic;
using System.Numerics;
using JComLib;
using NUnit.Framework;
using TestUtilities;

namespace ComLibTests;

[TestFixture]
public class Variants {

    // Test simple variant assignment.
    [Test]
    public void Variants1() {
        Variant v1 = new(12);
        Assert.AreEqual(12, v1.IntValue);
        Assert.AreEqual(12.0, v1.RealValue);
        Assert.AreEqual(12, v1.DoubleValue);
        Assert.AreEqual(true, v1.BoolValue);
        Assert.AreEqual("12", v1.StringValue);
        Assert.AreEqual(VariantType.INTEGER, v1.Type);
        Assert.IsTrue(v1.IsNumber);
        Assert.IsFalse(v1.IsZero);
        Assert.AreEqual("12", v1.ToString());
        Assert.AreEqual(typeof(int), Variant.VariantTypeToSystemType(v1.Type));
    }

    // Test floating point variant.
    [Test]
    public void Variants2() {
        Variant v1 = new(12.56f);
        Assert.AreEqual(12, v1.IntValue);
        Assert.AreEqual(12.56f, v1.RealValue);
        Assert.IsTrue(Helper.DoubleCompare(12.56d, v1.DoubleValue));
        Assert.AreEqual(true, v1.BoolValue);
        Assert.AreEqual("12.56", v1.StringValue);
        Assert.AreEqual(VariantType.FLOAT, v1.Type);
        Assert.IsTrue(v1.IsNumber);
        Assert.IsFalse(v1.IsZero);
        Assert.AreEqual("12.56", v1.ToString());
        Assert.AreEqual(typeof(float), Variant.VariantTypeToSystemType(v1.Type));
        Assert.AreEqual(new Variant(178.2f), new Variant((object)178.2f));
    }

    // Test double variant.
    [Test]
    public void Variants3() {
        Variant v1 = new(194.599d);
        Assert.AreEqual(194, v1.IntValue);
        Assert.AreEqual(194.599f, v1.RealValue);
        Assert.AreEqual(194.599d, v1.DoubleValue);
        Assert.AreEqual(true, v1.BoolValue);
        Assert.AreEqual("194.599", v1.StringValue);
        Assert.AreEqual(VariantType.DOUBLE, v1.Type);
        Assert.IsTrue(v1.IsNumber);
        Assert.IsFalse(v1.IsZero);
        Assert.AreEqual("194.599", v1.ToString());
        Assert.AreEqual(typeof(double), Variant.VariantTypeToSystemType(v1.Type));
        Assert.AreEqual(new Variant(7777.77d), new Variant((object)7777.77d));
    }

    // Test boolean variant.
    [Test]
    public void Variants4() {
        Variant v1 = new(true);
        Assert.AreEqual(-1, v1.IntValue);
        Assert.AreEqual(-1f, v1.RealValue);
        Assert.AreEqual(-1d, v1.DoubleValue);
        Assert.AreEqual(true, v1.BoolValue);
        Assert.AreEqual("True", v1.StringValue);
        Assert.AreEqual(VariantType.BOOLEAN, v1.Type);
        Assert.IsFalse(v1.IsNumber);
        Assert.IsFalse(v1.IsZero);
        Assert.AreEqual("True", v1.ToString());
        Assert.AreEqual(new Variant(true), new Variant((object)true));
        Assert.AreEqual(typeof(bool), Variant.VariantTypeToSystemType(v1.Type));
    }

    // Test string variant.
    [Test]
    public void Variants5() {
        Variant v1 = new("67.4");
        Assert.AreEqual(67, v1.IntValue);
        Assert.AreEqual(67.4f, v1.RealValue);
        Assert.AreEqual(67.4d, v1.DoubleValue);
        Assert.AreEqual(true, v1.BoolValue);
        Assert.AreEqual("67.4", v1.StringValue);
        Assert.AreEqual(VariantType.STRING, v1.Type);
        Assert.IsFalse(v1.IsNumber);
        Assert.IsFalse(v1.IsZero);
        Assert.AreEqual("67.4", v1.ToString());
        Assert.AreEqual(new Variant("HELLO WORLD!"), new Variant((object)"HELLO WORLD!"));
        Assert.AreEqual(typeof(string), Variant.VariantTypeToSystemType(v1.Type));
    }

    // Test complex variant.
    [Test]
    public void Variants6() {
        Variant v1 = new(new Complex(45, 8));
        Assert.AreEqual(45, v1.IntValue);
        Assert.AreEqual(45f, v1.RealValue);
        Assert.AreEqual(45d, v1.DoubleValue);
        Assert.AreEqual(true, v1.BoolValue);
        Assert.AreEqual("<45; 8>", v1.StringValue);
        Assert.AreEqual(VariantType.COMPLEX, v1.Type);
        Assert.IsTrue(v1.IsNumber);
        Assert.IsFalse(v1.IsZero);
        Assert.AreEqual(new Variant(new Complex(5,-6)), new Variant((object)new Complex(5, -6)));
        Assert.AreEqual(typeof(Complex), Variant.VariantTypeToSystemType(v1.Type));
    }

    // Test zero.
    [Test]
    public void Variants7() {
        Variant v1 = new(0);
        Assert.IsTrue(v1.IsZero);
    }

    // Test equality.
    [Test]
    public void Variants8() {
        Variant v1 = new(78.12f);
        Variant v2 = new(78.12f);
        Assert.IsTrue(v1 == v2);
        Assert.IsTrue(v1.CompareTo(v2) == 0);
        Assert.IsFalse(v1 == null);
        Assert.IsTrue(v1 is not null);
        Assert.AreEqual(v1, v2);
        Assert.AreEqual(v1, new Variant(78.12));
        Assert.AreEqual(new Variant(), new Variant());
        Assert.IsTrue(v1 == 78.12f);
        Assert.IsTrue(78.12f == v1);

        Variant v3 = new(100.4f);
        Assert.IsTrue(v1 != v3);
        Assert.IsTrue(v1.CompareTo(v3) != 0);
        Assert.AreNotEqual(v1, v3);
        Assert.IsTrue(v3 != 78.12f);
        Assert.IsTrue(78.12f != v3);

        Variant v4 = new(1267);
        Assert.IsTrue(v1 != v4);
        Assert.IsTrue(v1.CompareTo(v4) != 0);
        Assert.AreNotEqual(v1, v3);
        Assert.IsTrue(v4 == 1267);
        Assert.IsTrue(1267 == v4);
        Assert.IsTrue(v4 != 934);
        Assert.IsTrue(934 != v4);
        Assert.IsTrue(v4.Compare(1267));

        Variant v5 = new(260.200d);
        Assert.IsTrue(v1 != v5);
        Assert.IsTrue(v1.CompareTo(v5) != 0);
        Assert.AreNotEqual(v1, v5);
        Assert.IsTrue(v5 == 260.200d);
        Assert.IsTrue(260.200d == v5);
        Assert.IsTrue(v5 != 934.0d);
        Assert.IsTrue(934.0d != v5);
    }

    // Test HasValue.
    [Test]
    public void Variants9() {
        Variant v1 = new(78.12);
        Variant v2 = new();
        Assert.IsTrue(v1.HasValue);
        Assert.IsTrue(!v2.HasValue);
        Assert.Throws(typeof(ArgumentException), delegate { _ = Variant.VariantTypeToSystemType(v2.Type); });
        Assert.Throws(typeof(NotImplementedException), delegate { _ = new Variant(45m); });
    }

    // Test simple variant addition.
    [Test]
    public void Variants10() {
        Variant v1 = new(78.0f);
        Variant v2 = new(12);
        Assert.AreEqual(new Variant(90f), v1 + v2);
        Assert.AreEqual(90, (v1 + v2).IntValue);
        Assert.AreEqual(90, v1.Add(v2).IntValue);

        Variant v3 = new(78);
        Variant v4 = new(12);
        Assert.AreEqual(new Variant(90), v3 + v4);

        Variant v5 = new(78.0d);
        Variant v6 = new(12.0d);
        Assert.AreEqual(new Variant(90.0d), v5 + v6);

        Variant v7 = new(new Complex(45, 8));
        Variant v8 = new(new Complex(13, 4));
        Assert.AreEqual(new Variant(new Complex(58, 12)), v7 + v8);

        Variant v9 = new("ONE");
        Variant v10 = new("TWO");
        Assert.AreEqual(new Variant("ONETWO"), v9 + v10);

        Variant v11 = new(true);
        Variant v12 = new(false);
        Assert.Throws(typeof(InvalidOperationException), delegate { _ = v11 + v12; });

        Assert.Throws(typeof(ArgumentNullException), delegate { _ = null + v2; });
        Assert.Throws(typeof(ArgumentNullException), delegate { _ = v1 + null; });    }

    // Test simple variant subtraction.
    [Test]
    public void Variants11() {
        Variant v1 = new(670);
        Variant v2 = new(49);
        Assert.AreEqual(v1 - v2, new Variant(621));
        Assert.AreEqual((v1 - v2).IntValue, 621);
        Assert.AreEqual(v1.Subtract(v2).IntValue, 621);

        Variant v3 = new(670.0f);
        Variant v4 = new(49.0f);
        Assert.AreEqual(new Variant(621f), v3 - v4);

        Variant v5 = new(670.0d);
        Variant v6 = new(49.0d);
        Assert.AreEqual(new Variant(621.0d), v5 - v6);

        Variant v7 = new(new Complex(45, 8));
        Variant v8 = new(new Complex(13, 4));
        Assert.AreEqual(new Variant(new Complex(32, 4)), v7 - v8);

        Variant v9 = new("ONE");
        Variant v10 = new("TWO");
        Assert.Throws(typeof(InvalidOperationException), delegate { _ = v9 - v10; });

        Variant v11 = new(true);
        Variant v12 = new(false);
        Assert.Throws(typeof(InvalidOperationException), delegate { _ = v11 - v12; });

        Assert.Throws(typeof(ArgumentNullException), delegate { _ = null - v2; });
        Assert.Throws(typeof(ArgumentNullException), delegate { _ = v1 - null; });
    }

    // Test simple variant multiplication.
    [Test]
    public void Variants12() {
        Variant v1 = new(45);
        Variant v2 = new(6);
        Assert.AreEqual(v1 * v2, new Variant(270));
        Assert.AreEqual((v1 * v2).RealValue, 270);
        Assert.AreEqual(v1.Multiply(v2).RealValue, 270);

        Variant v3 = new(45.0f);
        Variant v4 = new(6.0f);
        Assert.AreEqual(v3 * v4, new Variant(270.0f));

        Variant v5 = new(45.0d);
        Variant v6 = new(6.0d);
        Assert.AreEqual(v5 * v6, new Variant(270.0d));

        Variant v7 = new(new Complex(45, 8));
        Variant v8 = new(new Complex(13, 4));
        Assert.AreEqual(v7 * v8, new Variant(new Complex(553, 284)));

        Variant v9 = new("ONE");
        Variant v10 = new("TWO");
        Assert.Throws(typeof(InvalidOperationException), delegate { _ = v9 * v10; });

        Assert.Throws(typeof(ArgumentNullException), delegate { _ = null * v2; });
        Assert.Throws(typeof(ArgumentNullException), delegate { _ = v1 * null; });
    }

    // Test simple variant division.
    [Test]
    public void Variants13() {
        Variant v1 = new(8342f);
        Variant v2 = new(410f);
        Assert.IsTrue(Helper.FloatCompare((v1 / v2).RealValue, new Variant(20.34634).RealValue));
        Assert.IsTrue(Helper.FloatCompare(v1.Divide(v2).RealValue, new Variant(20.34634).RealValue));

        Variant v3 = new(8342);
        Variant v4 = new(410);
        Assert.AreEqual(v3 / v4, new Variant(20));

        Variant v5 = new(8342d);
        Variant v6 = new(410d);
        Assert.IsTrue(Helper.DoubleCompare((v5 / v6).DoubleValue, new Variant(20.34634).DoubleValue));
        Assert.IsTrue(Helper.DoubleCompare(v5.Divide(v6).DoubleValue, new Variant(20.34634).DoubleValue));

        Variant v7 = new(new Complex(45, 8));
        Variant v8 = new(new Complex(13, 4));
        Variant result = new(v7 / v8);
        Assert.IsTrue(Helper.DoubleCompare(result.ComplexValue.Real, new Variant(3.3351351351351353).DoubleValue));

        Variant v9 = new("ONE");
        Variant v10 = new("TWO");
        Assert.Throws(typeof(InvalidOperationException), delegate { _ = v9 / v10; });

        Assert.Throws(typeof(ArgumentNullException), delegate { _ = null / v2; });
        Assert.Throws(typeof(ArgumentNullException), delegate { _ = v1 / null; });
    }

    // Test simple variant modulus.
    [Test]
    public void Variants14() {
        Variant v1 = new(89);
        Variant v2 = new(7);
        Assert.AreEqual(v1 % v2, new Variant(5));

        Variant v3 = new(89.0f);
        Variant v4 = new(7.0f);
        Assert.AreEqual(v3 % v4, new Variant(5.0f));

        Variant v5 = new(89.0d);
        Variant v6 = new(7.0d);
        Assert.AreEqual(v5 % v6, new Variant(5.0d));

        Variant v7 = new("ONE");
        Variant v8 = new("TWO");
        Assert.Throws(typeof(InvalidOperationException), delegate { _ = v7 % v8; });

        Assert.Throws(typeof(ArgumentNullException), delegate { _ = null % v2; });
        Assert.Throws(typeof(ArgumentNullException), delegate { _ = v1 % null; });
    }

    // Test simple variant power.
    [Test]
    public void Variants15() {
        Variant v1 = new(89);
        Variant v2 = new(3);
        Assert.AreEqual(new Variant(Math.Pow(89, 3)), v1.Pow(v2));
        Assert.IsTrue(704969 == v1.Pow(v2));

        Variant v3 = new(89.0f);
        Variant v4 = new(3.0f);
        Assert.IsTrue(704969.0f == v3.Pow(v4));
        Assert.AreEqual(new Variant(704969.0f), v3.Pow(v4));

        Variant v5 = new(89.0d);
        Variant v6 = new(3.0d);
        Assert.IsTrue(704969.0d == v5.Pow(v6));
        Assert.AreEqual(new Variant(704969.0d), v5.Pow(v6));

        Variant v7 = new(new Complex(20, 3));
        Variant v8 = new(new Complex(2, 4));
        Assert.AreEqual(new Variant(new Complex(218.9382832563314, -53.8472034693398)), v7.Pow(v8));

        Variant v9 = new("TEST");
        Variant v10 = new("DATA");
        Assert.Throws(typeof(InvalidOperationException), delegate { _ = v9.Pow(v10); });
    }

    // Test variant greater than and less than comparisons.
    [Test]
    public void Variants16() {
        Variant v1 = new(89);
        Variant v2 = new(3);
        Assert.IsTrue(v1 > v2);
        Assert.IsTrue(v2 < v1);

        Variant v3 = new(17.4f);
        Variant v4 = new(97.2f);
        Assert.IsTrue(v4 > v3);
        Assert.IsTrue(v3 < v4);

        Assert.IsFalse(null < v4);
        Assert.IsFalse(null > v4);
        Assert.IsFalse(null < v4);
        Assert.IsTrue(v3 > null);
    }

    // Test negation
    [Test]
    public void Variants17() {
        Variant v1 = new(89);
        Assert.AreEqual(new Variant(-89), -v1);
        Assert.IsTrue(-89 == -v1);
        Assert.IsTrue(-89 == v1.Negate());
        Assert.IsTrue(89 == - -v1);

        Variant v2 = new(919.99f);
        Assert.IsTrue(Helper.FloatCompare(new Variant(-919.99f).RealValue, -v2.RealValue));
        Assert.IsTrue(Helper.FloatCompare(new Variant(-919.99f).RealValue, v2.Negate().RealValue));

        Variant v3 = new(0.632d);
        Assert.IsTrue(Helper.DoubleCompare(new Variant(-0.632d).RealValue, -v3.DoubleValue));
        Assert.IsTrue(Helper.DoubleCompare(new Variant(-0.632d).RealValue, v3.Negate().DoubleValue));

        Variant v4 = new("A STRING");
        Assert.Throws(typeof(InvalidOperationException), delegate { _ = -v4; });
        Assert.Throws(typeof(InvalidOperationException), delegate { _ = v4.Negate(); });

        Variant v5 = new(new Complex(45.12, -0.12));
        Assert.IsTrue(Helper.DoubleCompare(new Variant(-45.12).DoubleValue, (-v5).ComplexValue.Real));
    }

    // Test the use of a variant in a dictionary
    [Test]
    public void Variants18() {
        Dictionary<Variant, string> testDict = new() {
            { new Variant("TEST"), "Found TEST!" },
            { new Variant("TEST2"), "Found TEST2!" },
            { new Variant(14.56d), "Found 14.56d!" },
            { new Variant(14.56f), "Found 14.56f!" },
            { new Variant(14902), "Found 14902!" },
            { new Variant(true), "Found True!" }
        };
        Assert.AreEqual("Found TEST!", testDict[new Variant("TEST")]);
        Assert.AreEqual("Found 14.56d!", testDict[new Variant(14.56d)]);
        Assert.AreEqual("Found 14.56f!", testDict[new Variant(14.56f)]);
        Assert.AreEqual("Found 14902!", testDict[new Variant(14902)]);
        Assert.AreEqual("Found True!", testDict[new Variant(true)]);
    }
}