// JEdit
// Extent class
//
// Authors:
//  Steve Palmer
//
// Copyright (C) 2023 Steve Palmer
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

namespace JEdit;

public class Extent {
    private static readonly Point Uninitalised = new(-1, -1);

    /// <summary>
    /// Create an empty Extent
    /// </summary>
    public Extent() {
        Start = Uninitalised;
        End = Uninitalised;
    }

    /// <summary>
    /// Add a new point to the extent, increasing the size of the
    /// extent if the point falls outside its existing range.
    /// </summary>
    public Extent Add(Point point) {
        if (Before(point, Start)) {
            Start = point;
        }
        if (After(point, End)) {
            End = point;
        }
        return this;
    }

    /// <summary>
    /// Reduce the extent to the specified points.
    /// </summary>
    public void Subtract(Point p1, Point p2) {
        if (After(p1, Start)) {
            Start = p1;
        }
        if (Before(p2, End)) {
            End = p2;
        }
    }

    /// <summary>
    /// Clear the extent
    /// </summary>
    public void Clear() {
        Start = Uninitalised;
        End = Uninitalised;
    }

    /// <summary>
    /// Return the start of the extent.
    /// </summary>
    public Point Start { get; private set; }

    /// <summary>
    /// Return the end of the extent.
    /// </summary>
    public Point End { get; private set; }

    /// <summary>
    /// Returns whether the extent has a valid range
    /// </summary>
    public bool Valid => Start != Uninitalised && End != Uninitalised;

    /// <summary>
    /// Return whether point p1 is before point p2 in the extent.
    /// </summary>
    /// <param name="p1">First point to test</param>
    /// <param name="p2">Second point to test</param>
    /// <returns>True if point p1 is before point p2, false otherwise.</returns>
    private static bool Before(Point p1, Point p2) =>
        p1 == Uninitalised || p2 == Uninitalised ||
        p1.Y < p2.Y || p1.Y == p2.Y && p1.X < p2.X;

    /// <summary>
    /// Return whether point p1 is after point p2 in the extent.
    /// </summary>
    /// <param name="p1">First point to test</param>
    /// <param name="p2">Second point to test</param>
    /// <returns>True if point p1 is after point p2, false otherwise.</returns>
    private static bool After(Point p1, Point p2) =>
        p1 == Uninitalised || p2 == Uninitalised ||
        p1.Y > p2.Y || p1.Y == p2.Y && p1.X > p2.X;
}