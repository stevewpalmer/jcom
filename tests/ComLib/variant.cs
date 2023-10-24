// JComLib
// Unit tests for Variant class
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

using System;
using System.Numerics;
using JComLib;
using NUnit.Framework;
using Utilities;

namespace ComLibTests {
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
            Variant v1 = new(78.12);
            Variant v2 = new(78.12);
            Assert.IsTrue(v1 == v2);
            Assert.IsTrue(v1.Compare(v2));
            Assert.IsFalse(v1 == null);
            Assert.IsTrue(v1 is not null);
            Assert.AreEqual(v1, v2);
            Assert.AreEqual(v1, new Variant(78.12));
        }

        // Test HasValue.
        [Test]
        public void Variants9() {
            Variant v1 = new(78.12);
            Variant v2 = new();
            Assert.IsTrue(v1.HasValue);
            Assert.IsTrue(!v2.HasValue);
        }

        // Test simple variant addition.
        [Test]
        public void Variants10() {
            Variant v1 = new(78.0f);
            Variant v2 = new(12);
            Assert.AreEqual(v1 + v2, new Variant(90f));
            Assert.AreEqual((v1 + v2).IntValue, 90);
            Assert.AreEqual(v1.Add(v2).IntValue, 90);
        }

        // Test simple variant subtraction.
        [Test]
        public void Variants11() {
            Variant v1 = new(670);
            Variant v2 = new(49);
            Assert.AreEqual(v1 - v2, new Variant(621));
            Assert.AreEqual((v1 - v2).IntValue, 621);
            Assert.AreEqual(v1.Subtract(v2).IntValue, 621);
        }

        // Test simple variant multiplication.
        [Test]
        public void Variants12() {
            Variant v1 = new(45);
            Variant v2 = new(6);
            Assert.AreEqual(v1 * v2, new Variant(270));
            Assert.AreEqual((v1 * v2).RealValue, 270);
            Assert.AreEqual(v1.Multiply(v2).RealValue, 270);
        }

        // Test simple variant division.
        [Test]
        public void Variants13() {
            Variant v1 = new(8342f);
            Variant v2 = new(410f);
            Assert.IsTrue(Helper.FloatCompare((v1 / v2).RealValue, new Variant(20.34634).RealValue));
            Assert.IsTrue(Helper.FloatCompare(v1.Divide(v2).RealValue, new Variant(20.34634).RealValue));
        }

        // Test simple variant modulus.
        [Test]
        public void Variants14() {
            Variant v1 = new(89);
            Variant v2 = new(7);
            Assert.AreEqual(v1 % v2, new Variant(5));
        }

        // Test simple variant power.
        [Test]
        public void Variants15() {
            Variant v1 = new(89);
            Variant v2 = new(3);
            Assert.AreEqual(v1.Pow(v2), new Variant(Math.Pow(89, 3)));
        }
    }
}