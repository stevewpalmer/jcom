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
using NUnit.Framework;
using JComLib;

namespace ComLibTests;

[TestFixture]
public class Extents {
    private Extent _extent;
    private Point _uninitialised;

    [SetUp]
    public void SetUp() {
        _extent = new Extent();
        _uninitialised = new Point(-1, -1);
    }

    [Test]
    public void TestEmptyExtent() {
        Assert.AreEqual(_uninitialised, _extent.Start);
        Assert.AreEqual(_uninitialised, _extent.End);
    }

    [Test]
    public void TestAdd() {
        Point point = new Point(1, 1);
        _extent.Add(point);

        Assert.AreEqual(point, _extent.Start);
        Assert.AreEqual(point, _extent.End);
    }

    [Test]
    public void TestSubtract() {
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

    [Test]
    public void TestClear() {
        Point point = new Point(1, 1);
        _extent.Add(point);
        _extent.Clear();

        Assert.AreEqual(_uninitialised, _extent.Start);
        Assert.AreEqual(_uninitialised, _extent.End);
    }

    [Test]
    public void TestValid() {
        Assert.IsFalse(_extent.Valid);
        _extent.Add(new Point(0, 0));
        Assert.IsTrue(_extent.Valid);
    }
}