// JCom Runtime Library
// Circular list data structure
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

namespace JComLib;

public class CircularList {

    private List<string> _items { get; } = new();
    private readonly int _maxCount;
    private int _listIndex;

    /// <summary>
    /// Create a CircularList with the specified maximum number
    /// of items. If none is specified then the default is 20 items.
    /// </summary>
    /// <param name="maxCount">Maximum number of items in list</param>
    public CircularList(int maxCount = 20) {
        _maxCount = maxCount;
    }

    /// <summary>
    /// Add a new item at the start of the list, removing one from
    /// the end if we're already at the maximum length. If the new
    /// item is the same as the first item in the history then we
    /// skip it.
    /// </summary>
    /// <param name="newString">New string to be added</param>
    protected void Add(string newString) {
        if (_items.Count == _maxCount) {
            _items.RemoveAt(_items.Count - 1);
        }
        if (_items.Count == 0 || _items.Count > 0 && _items[0] != newString) {
            _items.Insert(0, newString);
        }
        _listIndex = -1;
    }

    /// <summary>
    /// Add a new item at the end of the list, removing one from
    /// the start if we're already at the maximum length. If the new
    /// item is the same as the last item in the history then we
    /// skip it.
    /// </summary>
    /// <param name="newString">New string to be added</param>
    public void Append(string newString) {
        if (_items.Count == _maxCount) {
            _items.RemoveAt(0);
        }
        if (_items.Count == 0 || _items.Count > 0 && _items[^1] != newString) {
            _items.Add(newString);
        }
        _listIndex = -1;
    }

    /// <summary>
    /// Get the next item from the list, wrapping round when we
    /// reach the end.
    /// </summary>
    /// <returns>The next history item</returns>
    public string Next() {
        if (_items.Count > 0) {
            ++_listIndex;
            if (_listIndex == _items.Count) {
                _listIndex = 0;
            }
            return _items[_listIndex];
        }
        return string.Empty;
    }

    /// <summary>
    /// Get the previous item from the list, wrapping round when we
    /// reach the end.
    /// </summary>
    /// <returns>The previous history item</returns>
    public string Previous() {
        if (_items.Count > 0) {
            --_listIndex;
            if (_listIndex < 0) {
                _listIndex = _items.Count - 1;
            }
            return _items[_listIndex];
        }
        return string.Empty;
    }
}