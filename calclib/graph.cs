// JCalcLib
// DAG for cell dependencies
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

using System.Diagnostics;

namespace JCalcLib;

public class CellGraph {
    private readonly Dictionary<CellLocation, HashSet<CellLocation>> _dependees = [];
    private readonly Dictionary<CellLocation, HashSet<CellLocation>> _dependents = [];

    /// <summary>
    /// Clear the graph prior to a rebuild.
    /// </summary>
    public void Clear() {
        _dependents.Clear();
        _dependees.Clear();
    }

    /// <summary>
    /// Add an edge between two cell locations such that <paramref name="from" />
    /// has a dependency on <paramref name="to" />.
    /// </summary>
    /// <param name="from">Dependee cell</param>
    /// <param name="to">Dependency cell</param>
    public void AddEdge(CellLocation from, CellLocation to) {
        Debug.Assert(from.SheetName != null);
        Debug.Assert(to.SheetName != null);
        if (!_dependents.TryGetValue(from, out HashSet<CellLocation>? value1)) {
            value1 = [];
            _dependents.Add(from, value1);
        }
        value1.Add(to);
        if (!_dependees.TryGetValue(to, out HashSet<CellLocation>? value2)) {
            value2 = [];
            _dependees.Add(to, value2);
        }
        value2.Add(from);
    }

    /// <summary>
    /// Remove the specified location from the list of dependencies and
    /// dependees.
    /// </summary>
    /// <param name="from">Cell location</param>
    public void RemoveEdges(CellLocation from) {
        Debug.Assert(from.SheetName != null);
        foreach (CellLocation to in _dependents.Keys) {
            _dependents[to].Remove(from);
        }
        foreach (CellLocation to in _dependees.Keys) {
            _dependees[to].Remove(from);
        }
        _dependents.Remove(from);
        _dependees.Remove(from);
    }

    /// <summary>
    /// Retrieve a full list of dependencies on the specified cell location.
    /// </summary>
    /// <param name="from">Cell location</param>
    /// <returns>List of dependencies</returns>
    public IEnumerable<CellLocation> GetDependents(CellLocation from) {
        Debug.Assert(from.SheetName != null);
        HashSet<CellLocation> result = [];
        InternalGetDependents(from, result);
        return result;
    }

    /// <summary>
    /// Walk the list of neighbours of the specified cell location and add it to
    /// the list of results. Stops if we detect a cycle due to the cell location
    /// already existing in the result set.
    /// </summary>
    /// <param name="from">Cell location to locate</param>
    /// <param name="result">Cumulative list of neighbours</param>
    private void InternalGetDependents(CellLocation from, HashSet<CellLocation> result) {
        if (result.Contains(from)) {
            return;
        }
        if (_dependents.TryGetValue(from, out HashSet<CellLocation>? dependent)) {
            foreach (CellLocation to in dependent) {
                result.Add(to);
                result.UnionWith(GetDependents(to));
            }
        }
    }
}