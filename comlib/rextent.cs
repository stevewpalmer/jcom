// comlib
// Rectangular extent
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

namespace JComLib;

public class RExtent {
    private static readonly Point Uninitalised = new(-1, -1);

    /// <summary>
    /// Create an empty Extent
    /// </summary>
    public RExtent() {
        Start = Uninitalised;
        End = Uninitalised;
    }

    /// <summary>
    /// Add a new point to the extent, increasing the size of the
    /// extent if the point falls outside its existing range.
    /// </summary>
    public RExtent Add(Point point) {
        Start = Start == Uninitalised ? point : new Point(Math.Min(Start.X, point.X), Math.Min(Start.Y, point.Y));
        End = End == Uninitalised ? point : new Point(Math.Max(End.X, point.X), Math.Max(End.Y, point.Y));
        return this;
    }

    /// <summary>
    /// Merge the specified extent with this one.
    /// </summary>
    /// <param name="extent">Extent to merge</param>
    public void Add(RExtent extent) {
        Add(extent.Start);
        Add(extent.End);
    }

    /// <summary>
    /// Reduce the extent to the specified points.
    /// </summary>
    public void Subtract(Point p1, Point p2) {
        Start = Start == Uninitalised ? p1 : new Point(Math.Max(Start.X, p1.X), Math.Max(Start.Y, p1.Y));
        End = End == Uninitalised ? p2 : new Point(Math.Min(End.X, p2.X), Math.Min(End.Y, p2.Y));
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
    /// Return whether point is contained within this extent.
    /// </summary>
    /// <param name="point">Point to test</param>
    /// <returns>True if the point is within the extent, false otherwise</returns>
    public bool Contains(Point point) =>
        Valid && Rectangle.FromLTRB(Start.X, Start.Y, End.X + 1, End.Y + 1).Contains(point);

    /// <summary>
    /// Clear the extent
    /// </summary>
    public void Clear() {
        Start = Uninitalised;
        End = Uninitalised;
    }
}