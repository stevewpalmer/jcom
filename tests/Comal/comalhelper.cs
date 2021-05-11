// JComal
// Helper functions for unit tests
//
// Authors:
//  Steve Palmer
//
// Copyright (C) 2021 Steve Palmer
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

using CCompiler;
using JComal;
using NUnit.Framework;
using Utilities;

namespace ComalTests {

    public class ComalHelper : Helper {

        // Compile the given code and return the error count.
        public static Compiler HelperCompile(string code, ComalOptions opts) {
            opts.Run = true;
            Compiler comp = new(opts);
            comp.CompileString(code, true);
            Assert.AreEqual(0, comp.Messages.ErrorCount, string.Format("Compiler Errors : {0}", string.Join("\n", comp.Messages)));
            return comp;
        }

        // Compile the given code and check the specified errors occurred.
        public static void HelperCompileAndCheckErrors(string code, ComalOptions opts, Message [] expectedErrors) {
            opts.Run = true;
            Compiler comp = new(opts);
            comp.CompileString(code, true);
            Assert.AreEqual(expectedErrors.Length, comp.Messages.ErrorCount);
            for (int index = 0; index < expectedErrors.Length; index++) {
                Assert.AreEqual(expectedErrors[index].Code, comp.Messages[index].Code);
                Assert.AreEqual(expectedErrors[index].Line, comp.Messages[index].Line);
                Assert.AreEqual(expectedErrors[index].Level, comp.Messages[index].Level);
            }
        }
    }
}
