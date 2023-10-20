// Accounts
// A single account record
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

using System.Text.Json.Serialization;

namespace JAccounts;

public class TRecord {

    /// <summary>
    /// Default constructor for JSON deserialization.
    /// </summary>
    [JsonConstructor]
    public TRecord() {}

    /// <summary>
    /// Construct a TRecord with the given properties.
    /// </summary>
    /// <param name="theName">Record name</param>
    /// <param name="theValue">Record value</param>
    /// <param name="theDate">Date of transaction</param>
    public TRecord(string theName, double theValue, TDate theDate) {
        Name = theName;
        Value = theValue;
        Date = theDate;
    }

    /// <summary>
    /// Record name
    /// </summary>
    [JsonInclude]
    public string Name { get; set; } = "";

    /// <summary>
    /// Record value
    /// </summary>
    [JsonInclude]
    public double Value { get; set; }

    /// <summary>
    /// Date of transaction
    /// </summary>
    [JsonInclude]
    public TDate Date { get; set; } = new();
}