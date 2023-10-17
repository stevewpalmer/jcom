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

using JComLib;

namespace JEdit;

public class History : CircularList {

    private static readonly Dictionary<string, History> PromptHistory = new();

    /// <summary>
    /// Create a new history queue with the specified maximum number of
    /// items.
    /// </summary>
    private History() : base(Consts.MaxCommandHistory) { }

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
    /// <param name="newItem">New character array to be added</param>
    public void Add(IEnumerable<char> newItem) {
        base.Add(string.Join("", newItem));
    }
}