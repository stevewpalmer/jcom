// Accounts
// Menu Item
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

namespace JAccounts;

public class TMenuItem {

    /// <summary>
    /// Short-cut key used to invoke menu item.
    /// </summary>
    public char ShortcutKey { get; }

    /// <summary>
    /// Short name of menu item.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Descriptive name of menu item.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="shortcutKey">Short-cut key</param>
    /// <param name="name">Menu name</param>
    /// <param name="title">Menu description</param>
    public TMenuItem(char shortcutKey, string name, string title) {
        ShortcutKey = shortcutKey;
        Name = name;
        Title = title;
    }
}