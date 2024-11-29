// SymbolCollection
// Test the SymbolCollection class
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
// under the License.

using System.Linq;
using CCompiler;
using NUnit.Framework;

namespace ComTests;

public class ComSymbolCollection {

    // Verify a basic symbol collection
    [Test]
    public void TestBasic() {
        SymbolCollection s = new SymbolCollection("_TEST123");
        Assert.AreEqual("_TEST123", s.Name);
        s.Add(new Symbol("Dummy123", new SymFullType(SymType.DOUBLE), SymClass.VAR, [], 45));
        Assert.IsNotNull(s.Get("DUMMY123"));
        Symbol dummy123 = s.Get("Dummy123");
        Assert.IsTrue(dummy123.Type == SymType.DOUBLE);
        Assert.IsTrue(dummy123.Class == SymClass.VAR);
        Assert.IsTrue(dummy123.RefLine == 45);

        // Removal
        s.Remove(dummy123);
        Assert.IsNull(s.Get("Dummy123"));
        Assert.IsNull(s.Get("DUMMY123"));
    }

    // Verify a collection with multiple symbols
    [Test]
    public void TestMultipleSymbols() {
        SymbolCollection s = new SymbolCollection("_TEST123");
        Assert.AreEqual("_TEST123", s.Name);
        s.Add("A$", new SymFullType(SymType.CHAR), SymClass.VAR, [], 45);
        s.Add("B%", new SymFullType(SymType.DOUBLE), SymClass.VAR, [], 53);
        s.Add("MYFUNC", new SymFullType(SymType.INTEGER), SymClass.FUNCTION, [], 100);
        Assert.IsNotNull(s.Get("B%"));

        string allNames = s.Aggregate("", (current, sym) => current + sym.Name);
        Assert.AreEqual("A$B%MYFUNC", allNames);
    }

    // Verify a case sensitive symbol collection
    [Test]
    public void TestCaseSensitive() {
        SymbolCollection s = new SymbolCollection("_TEST123_CASE", true) {
            new Symbol("Dummy123", new SymFullType(SymType.DOUBLE), SymClass.VAR, [], 45)
        };
        Assert.IsNull(s.Get("DUMMY123"));
        Symbol dummy123 = s.Get("Dummy123");
        Assert.IsTrue(dummy123.Type == SymType.DOUBLE);
        Assert.IsTrue(dummy123.Class == SymClass.VAR);
        Assert.IsTrue(dummy123.RefLine == 45);

        // Removal
        s.Remove(dummy123);
        Assert.IsNull(s.Get("Dummy123"));
        Assert.IsNull(s.Get("DUMMY123"));
    }
}