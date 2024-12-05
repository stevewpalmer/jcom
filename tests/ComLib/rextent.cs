// JComLib
// Unit tests for the RExtent class
//
// Authors:
//  Steve
//
// Copyright (C) 2024 Steve
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

using System.Drawing;
using JComLib;
using NUnit.Framework;

namespace ComLibTests;

[TestFixture]
[TestOf(typeof(RExtent))]
public class RExtentTest {
    private static readonly Point Uninitialised = new(-1, -1);
    private RExtent _rextent;

    [SetUp]
    public void Setup() {
        _rextent = new RExtent();
    }

    // Verify that a new instance of an RExtent has both Start
    // and End set to Uninitialised.
    [Test]
    public void VerifyNewRExtent() {
        Assert.AreEqual(Uninitialised, _rextent.Start);
        Assert.AreEqual(Uninitialised, _rextent.End);
    }

    // Verify the Add method.
    [Test]
    public void VerifyAdd() {

        // Adding a single point - both Start and End should have
        // the same value.
        Point point = new Point(5, 5);
        _rextent.Add(point);
        Assert.AreEqual(point, _rextent.Start);
        Assert.AreEqual(point, _rextent.End);

        // Adding multiple points - the Start will be the top left
        // point and the End will be the bottom right.
        Point point1 = new Point(5, 5);
        Point point2 = new Point(6, 6);
        Point point3 = new Point(4, 4);
        _rextent.Add(point1);
        _rextent.Add(point2);
        _rextent.Add(point3);
        Assert.AreEqual(point3, _rextent.Start);
        Assert.AreEqual(point2, _rextent.End);
    }

    // Verify the Contains method.
    [Test]
    public void VerifyContains() {
        Point point1 = new Point(2, 2);
        Point point2 = new Point(5, 5);
        _rextent.Add(point1);
        _rextent.Add(point2);
        Assert.IsTrue(_rextent.Contains(point1));
        Assert.IsTrue(_rextent.Contains(point2));
        Assert.IsTrue(_rextent.Contains(new Point(3, 3)));
        Assert.IsFalse(_rextent.Contains(new Point(6, 6)));
        Assert.IsFalse(_rextent.Contains(new Point(1, 2)));
        Assert.IsFalse(_rextent.Contains(new Point(2, 1)));
        Assert.IsFalse(_rextent.Contains(new Point(5, 6)));
        Assert.IsFalse(_rextent.Contains(new Point(6, 5)));
    }

    // Verify the Clear method
    [Test]
    public void VerifyClear() {
        Point point1 = new Point(2, 2);
        Point point2 = new Point(5, 5);
        _rextent.Add(point1);
        _rextent.Add(point2);
        _rextent.Clear();
        Assert.IsFalse(_rextent.Valid);
    }
}