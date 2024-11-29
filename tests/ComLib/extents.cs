// JComLib
// Unit tests for the Extent class
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

using System.Drawing;
using JComLib;
using NUnit.Framework;

namespace ComLibTests;

[TestFixture]
public class Extents {
    private static readonly Point Uninitialised = new(-1, -1);
    private Extent _extent;

    [SetUp]
    public void SetUp() {
        _extent = new Extent();
    }

    [Test]
    // Verify that a new instance of an Extent has both Start
    // and End set to Uninitialised.
    public void TestEmptyExtent() {
        Assert.AreEqual(Uninitialised, _extent.Start);
        Assert.AreEqual(Uninitialised, _extent.End);
    }

    // Verify the Add method that extends an extent to
    // include the specified point
    [Test]
    public void VerifyAdd() {
        Point point = new Point(1, 1);
        _extent.Add(point);

        Assert.AreEqual(point, _extent.Start);
        Assert.AreEqual(point, _extent.End);
    }

    // Verify the Subtract method that reduces an extent to
    // exclude the specified point
    [Test]
    public void VerifySubtract() {
        Point start = new Point(1, 1);
        Point end = new Point(2, 2);
        _extent.Add(start);
        _extent.Add(end);

        Point newStart = new Point(1, 2);
        Point newEnd = new Point(2, 1);
        _extent.Subtract(newStart, newEnd);

        Assert.AreEqual(newStart, _extent.Start);
        Assert.AreEqual(newEnd, _extent.End);
    }

    // Verify the Contains method that tests whether a point
    // is contained within an extent.
    [Test]
    public void VerifyContains() {
        _extent.Add(new Point(4, 4));
        _extent.Add(new Point(12, 30));
        Assert.IsTrue(_extent.Contains(new Point(4, 4)));
        Assert.IsTrue(_extent.Contains(new Point(800, 4)));
        Assert.IsTrue(_extent.Contains(new Point(12, 30)));
        Assert.IsTrue(_extent.Contains(new Point(0, 16)));
        Assert.IsTrue(_extent.Contains(new Point(000, 16)));
        Assert.IsTrue(_extent.Contains(new Point(0, 16)));
        Assert.IsTrue(_extent.Contains(new Point(90, 12)));
        Assert.IsFalse(_extent.Contains(new Point(3, 4)));
        Assert.IsFalse(_extent.Contains(new Point(13, 30)));
        Assert.IsFalse(_extent.Contains(new Point(0, 0)));
        Assert.IsFalse(_extent.Contains(new Point(-1, -1)));
    }

    // Verify the Clear method that resets an extent to
    // uninitialised.
    [Test]
    public void VerifyClear() {
        Point point = new Point(1, 1);
        _extent.Add(point);
        _extent.Clear();

        Assert.AreEqual(Uninitialised, _extent.Start);
        Assert.AreEqual(Uninitialised, _extent.End);
    }

    [Test]
    public void VerifyValid() {
        Assert.IsFalse(_extent.Valid);
        _extent.Add(new Point(0, 0));
        Assert.IsTrue(_extent.Valid);
    }
}