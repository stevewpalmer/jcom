// JFortran Compiler
// Helper functions for unit tests
//
// Authors:
//  Steve Palmer
//
// Copyright (C) 2013 Steve Palmer
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
// under the License

using JFortran;
using NUnit.Framework;
using TestUtilities;

namespace FortranTests;

internal abstract class FortranHelper : Helper {

    // Compile the given code and return the error count.
    public static Compiler HelperCompile(string[] code, FortranOptions opts) {
        Compiler comp = new(opts);
        comp.CompileLines(code);
        Assert.AreEqual(0, comp.Messages.ErrorCount, $"Compiler Errors : {string.Join("\n", comp.Messages)}");
        return comp;
    }

    // Compile the given code and return the error count.
    public static Compiler HelperCompile(string code, FortranOptions opts) {
        Compiler comp = new(opts);
        comp.CompileLines(code.Split('\n'));
        Assert.AreEqual(0, comp.Messages.ErrorCount, $"Compiler Errors : {string.Join("\n", comp.Messages)}");
        return comp;
    }
}