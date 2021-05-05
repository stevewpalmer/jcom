// JComal
// Unit tests for functions
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

using JComal;
using NUnit.Framework;

namespace ComalTests {
    [TestFixture]

    public class Data {

        // Test simple DATA read
        [Test]
        public void TestRead1Data() {
            string code = @"
                FUNC read'simple'data
                  READ A
                  RETURN A
                ENDFUNC
                DATA 15
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Utilities.Helper.HelperRunFloat(comp, "read'simple'data", 15);
        }

        // Test multiple DATA reads
        [Test]
        public void TestReadXData() {
            string code = @"
                FUNC read'multiple'data
                  READ A,B#,C,D#
                  RETURN (A+B#)*(C-D#)
                ENDFUNC
                DATA 47,13,10,8
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Utilities.Helper.HelperRunFloat(comp, "read'multiple'data", 120);
        }

        // Test EOD
        [Test]
        public void TestEOD() {
            string code = @"
                FUNC test'eod
                  IF EOD THEN RETURN 0
                  READ A,B#,C,D#
                  RETURN EOD
                ENDFUNC test'eod
                DATA 47,13,10,8
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Utilities.Helper.HelperRunFloat(comp, "test'eod", 1);
        }

        // Test that EOD is 1 if there are no DATA statements
        // in the program.
        [Test]
        public void TestDefaultEOD() {
            string code = @"
                FUNC test'default'eod
                  IF EOD THEN RETURN 1
                  RETURN FALSE
                ENDFUNC test'default'eod
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Utilities.Helper.HelperRunFloat(comp, "test'default'eod", 1);
        }

        // Test RESTORE
        [Test]
        public void TestRestore() {
            string code = @"
                FUNC test'restore
                  READ A,B#
                  RESTORE
                  READ C,D#
                  RETURN (A+B#)*(C-D#)
                ENDFUNC
                DATA 47,13,10,8
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Utilities.Helper.HelperRunFloat(comp, "test'restore", 2040);
        }

        // Test RESTORE resets EOD
        [Test]
        public void TestRestoreResetEOD() {
            string code = @"
                FUNC test'restore'reset
                  READ A,B#,C,D#
                  IF NOT EOD THEN RETURN FALSE
                  RESTORE
                  RETURN NOT EOD
                ENDFUNC
                DATA 47,13,10,8
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Utilities.Helper.HelperRunFloat(comp, "test'restore'reset", 1);
        }
    }
}
