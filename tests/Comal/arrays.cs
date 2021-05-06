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

using System;
using CCompiler;
using JComal;
using NUnit.Framework;

namespace ComalTests {
    [TestFixture]

    public class Arrays {

        // Test Simple 1D array
        [Test]
        public void TestSimple1DArray() {
            string code = @"
                func test closed
                  dim a(10)
                  for x:=1 to 10 do
                    a(x):=x
                  endfor x
                  for x:=1 to 10 do
                    if a(x)<>x then return false
                  endfor x
                  return true
                endfunc
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Utilities.Helper.HelperRunFloat(comp, "test", 1);
        }

        // Test Simple 2D array
        [Test]
        public void TestSimple2DArray() {
            string code = @"
                func array'2d closed
                  dim a(10,10)
                  for x:=1 to 10 do
                    for y:=1 to 10 do
                      a(x,y):=x*y
                    endfor y
                  endfor x
                  for x:=1 to 10 do
                    for y:=1 to 10 do
                      if a(x,y)<>x*y then return false
                    endfor y
                  endfor x
                  return true
                endfunc array'2d
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Utilities.Helper.HelperRunFloat(comp, "array'2d", 1);
        }

        // Test 1D string array
        [Test]
        public void TestString1DArray() {
            string code = @"
                func test closed
                  dim a$(10) of 5
                  for x:=1 to 10 do
                    a$(x):=str$(x)
                  endfor x
                  for x:=1 to 10 do
                    if a$(x)<>str$(x) then return false
                  endfor x
                  return true
                endfunc
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Utilities.Helper.HelperRunFloat(comp, "test", 1);
        }

        // Test 2D string array
        [Test]
        public void TestString2DArray() {
            string code = @"
                func array'string'2d closed
                  dim a$(5,10) of 4
                  for x:=1 to 5 do
                    for y:=1 to 10 do
                      a$(x,y):=str$(x*y)
                    endfor y
                  endfor x
                  for x:=1 to 5 do
                    for y:=1 to 10 do
                      if a$(x,y)<>str$(x*y) then return false
                    endfor y
                  endfor x
                  return true
                endfunc array'string'2d
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Utilities.Helper.HelperRunFloat(comp, "array'string'2d", 1);
        }

        // Test catching inconsistent array dimensions
        [Test]
        public void TestInconsistentDimensions() {
            string code = @"
                func array'badd closed
                  dim a(10,10)
                  for x:=1 to 10 do
                    for y:=1 to 10 do
                      a(x):=x*y
                    endfor y
                  endfor x
                  for x:=1 to 10 do
                    for y:=1 to 10 do
                      if a(y)<>x*y then return false
                    endfor y
                   endfor x
                  return true
                endfunc array'badd
            ";
            Message[] expectedErrors = {
                new Message(null, MessageLevel.Error, MessageCode.MISSINGARRAYDIMENSIONS, 140, null),
                new Message(null, MessageLevel.Error, MessageCode.MISSINGARRAYDIMENSIONS, 190, null)
            };
            ComalHelper.HelperCompileAndCheckErrors(code, new ComalOptions(), expectedErrors);
        }

        // Test Simple bounded array
        [Test]
        public void TestSimpleBoundedArray() {
            string code = @"
                func test closed
                  dim a(30:40)
                  for x:=30 to 40 do
                    a(x):=x
                  endfor x
                  for x:=30 to 40 do
                    if a(x)<>x then return false
                  endfor x
                  return true
                endfunc
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Utilities.Helper.HelperRunFloat(comp, "test", 1);
        }

        // Test catching out of bounds error
        [Test]
        public void TestArrayOutOfBounds() {
            string code = @"
                func array'oob closed
                  dim a(30:40)
                  for x:=20 to 40 do
                    a(x):=x
                  endfor x
                  return false
                endfunc
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Assert.Throws(typeof(IndexOutOfRangeException), delegate { comp.Execute("array'oob"); });
        }

        // Test calling a function with an array by value.
        [Test]
        public void TestPassArrayByVal() {
            string code = @"
                func array'byval
                  dim a(10)
                  for x=1 to 10 do
                    a(x) := x*x
                  endfor
                  return test'by'val(a)
                endfunc
                func test'by'val(b())
                  for x=1 to 10 do
                    if b(x)<>x*x return false
                  endfor
                  return true
                endfunc
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Utilities.Helper.HelperRunFloat(comp, "array'byval", 1);
        }

        // Test calling a function with an array by reference.
        [Test]
        public void TestPassArrayByRef() {
            string code = @"
                func array'byref
                  dim a(10)
                  init'by'ref(a)
                  return test'by'val(a)
                endfunc
                proc init'by'ref(ref b())
                  for x=1 to 10 do
                    b(x)=x*x
                  endfor
                endproc
                func test'by'val(b())
                  for x=1 to 10 do
                    if b(x)<>x*x return false
                  endfor
                  return true
                endfunc
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Utilities.Helper.HelperRunFloat(comp, "array'byref", 1);
        }
    }
}