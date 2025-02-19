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
    private readonly Dictionary<CellLocation, HashSet<CellLocation>> _precedents = [];
    private readonly Dictionary<CellLocation, HashSet<CellLocation>> _dependents = [];

    /// <summary>
    /// Clear the graph.
    /// </summary>
    public void Clear() {
        _dependents.Clear();
        _precedents.Clear();
    }

    /// <summary>
    /// Add an edge between two cell locations such that <paramref name="from" />
    /// has a dependency on <paramref name="to" />.
    /// </summary>
    /// <param name="from">Precedent cell</param>
    /// <param name="to">Dependent cell</param>
    public void AddEdge(CellLocation from, CellLocation to) {
        Debug.Assert(from.SheetName != null);
        Debug.Assert(to.SheetName != null);
        if (!_dependents.TryGetValue(from, out HashSet<CellLocation>? value1)) {
            value1 = [];
            _dependents.Add(from, value1);
        }
        value1.Add(to);
        if (!_precedents.TryGetValue(to, out HashSet<CellLocation>? value2)) {
            value2 = [];
            _precedents.Add(to, value2);
        }
        value2.Add(from);
    }

    /// <summary>
    /// Remove all edges leading from the specified cell.
    /// </summary>
    /// <param name="from">Dependent cell</param>
    public void DeleteEdges(CellLocation from) {
        if (_dependents.TryGetValue(from, out HashSet<CellLocation>? value)) {
            foreach (CellLocation to in value) {
                _precedents[to].Remove(from);
            }
            _dependents[from] = [];
        }
    }

    /// <summary>
    /// Retrieve a full list of dependencies on the specified cell location.
    /// </summary>
    /// <param name="from">Cell location</param>
    /// <returns>List of dependencies</returns>
    public IEnumerable<CellLocation> GetDependents(CellLocation from) {
        Debug.Assert(from.SheetName != null);
        HashSet<CellLocation> result = [];
        InternalGetDependents(from, result, []);
        return result;
    }

    /// <summary>
    /// Retrieve a full list of precedents on the specified cell location.
    /// </summary>
    /// <param name="from">Cell location</param>
    /// <returns>List of precedents</returns>
    public IEnumerable<CellLocation> GetPrecedents(CellLocation from) {
        Debug.Assert(from.SheetName != null);
        HashSet<CellLocation> result = [];
        InternalGetPrecedents(from, result, []);
        return result;
    }

    /// <summary>
    /// Walk the list of neighbours of the specified cell location and add it to
    /// the list of results. Stops if we detect a cycle due to the cell location
    /// already existing in the result set.
    /// </summary>
    /// <param name="from">Cell location to locate</param>
    /// <param name="result">Cumulative list of neighbours</param>
    /// <param name="sources">List of cells walked</param>
    private void InternalGetDependents(CellLocation from, HashSet<CellLocation> result, List<CellLocation> sources) {
        if (sources.Contains(from)) {
            return;
        }
        sources.Add(from);
        if (_dependents.TryGetValue(from, out HashSet<CellLocation>? dependent)) {
            foreach (CellLocation to in dependent) {
                result.Add(to);
                InternalGetDependents(to, result, sources);
            }
        }
    }

    /// <summary>
    /// Walk the list of neighbours of the specified cell location and add it to
    /// the list of results. Stops if we detect a cycle due to the cell location
    /// already existing in the result set.
    /// </summary>
    /// <param name="from">Cell location to locate</param>
    /// <param name="result">Cumulative list of neighbours</param>
    /// <param name="sources">List of cells walked</param>
    private void InternalGetPrecedents(CellLocation from, HashSet<CellLocation> result, List<CellLocation> sources) {
        if (sources.Contains(from)) {
            return;
        }
        sources.Add(from);
        if (_precedents.TryGetValue(from, out HashSet<CellLocation>? precedents)) {
            foreach (CellLocation to in precedents) {
                result.Add(to);
                InternalGetPrecedents(to, result, sources);
            }
        }
    }
}