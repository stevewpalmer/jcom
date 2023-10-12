// JEdit
// Status bar prompt history
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

namespace JEdit;

public class History {

    private static readonly Dictionary<string, History> PromptHistory = new();
    private List<string> _items { get; } = new();
    private int _historyIndex;

    /// <summary>
    /// Return the history cache for the given prompt. If one doesn't exist
    /// then it is created and added to the list.
    /// </summary>
    /// <param name="prompt">Prompt</param>
    /// <returns>History cache</returns>
    public static History Get(string prompt) {
        if (!PromptHistory.TryGetValue(prompt, out History? history)) {
            history = new History();
            PromptHistory.Add(prompt, history);
        }
        return history;
    }

    /// <summary>
    /// Add a new item at the start of the list, removing one from
    /// the end if we're already at the maximum length. If the new
    /// item is the same as the first item in the history then we
    /// skip it.
    /// </summary>
    /// <param name="newItem">New string to be added</param>
    public void Add(IEnumerable<char> newItem) {
        if (_items.Count == Consts.MaxCommandHistory) {
            _items.RemoveAt(_items.Count - 1);
        }
        string newString = string.Join("", newItem);
        if (_items.Count == 0 || _items.Count > 0 && _items[0] != newString) {
            _items.Insert(0, string.Join("", newString));
        }
        _historyIndex = -1;
    }

    /// <summary>
    /// Get the next item from the list, wrapping round when we
    /// reach the end.
    /// </summary>
    /// <returns>The next history item</returns>
    public string Next() {
        if (_items.Count > 0) {
            ++_historyIndex;
            if (_historyIndex == _items.Count) {
                _historyIndex = 0;
            }
            return _items[_historyIndex];
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
            --_historyIndex;
            if (_historyIndex < 0) {
                _historyIndex = _items.Count - 1;
            }
            return _items[_historyIndex];
        }
        return string.Empty;
    }
}