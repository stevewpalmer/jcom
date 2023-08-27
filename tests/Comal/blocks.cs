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
using JComLib;
using NUnit.Framework;
using Utilities;

namespace ComalTests {
    [TestFixture]

    public class Blocks {

        // Test NULL statement
        [Test]
        public void TestNull() {
            string code = @"
                func test closed
                  null
                  return true
                endfunc
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Helper.HelperRunFloat(comp, "test", 1);
        }

        // Test ERR in trap handler
        [Test]
        public void TestErrInTrap() {
            string code = @"
                func test closed
                  trap
                    if err<>0 then return false
                    report 13
                  handler
                    if err<>13 then return false
                  endtrap
                  return true
                endfunc
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Helper.HelperRunFloat(comp, "test", 1);
        }

        // Test ERRTEXT$ in trap handler
        [Test]
        public void TestErrTextInTrap() {
            string code = @"
                func test closed
                  trap
                    if len(errtext$)<>0 then return false
                    report 5
                  handler
                    if len(errtext$)<>0 then return true
                  endtrap
                  return false
                endfunc
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Helper.HelperRunFloat(comp, "test", 1);
        }

        // Test trapping divisions by zero for both integer and
        // floating point.
        [Test]
        public void TestDivisionByZeroTrap() {
            string code = @"
                func test closed
                  trap
                    a# := 10
                    b# := 0
                    c# := a#/b#
                  handler
                    if err<>4 return false
                    endtrap
                  trap
                    a := 10
                    b := 0
                    c := a/b
                  handler
                    if err<>4 return false
                  endtrap
                  return true
                endfunc
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Helper.HelperRunFloat(comp, "test", 1);
        }

        // Test END doesn't get caught by trap handler since it raises its own
        // exception.
        [Test]
        public void TestEndInTrap() {
            string code = @"
                func test closed
                  trap
                    end
                  handler
                    return false
                  endtrap
                  return true
                endfunc
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Assert.Throws(typeof(JComRuntimeException), delegate { comp.Execute("test"); });
        }
    }
}
