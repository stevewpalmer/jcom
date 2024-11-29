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
using TestUtilities;

namespace ComalTests;

[TestFixture]
public class Arrays {

    // Test Simple 1D array
    [Test]
    public void TestSimple1DArray() {
        const string code = @"
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
        Helper.HelperRunFloat(comp, "test", 1);
    }

    // Test Simple 2D array
    [Test]
    public void TestSimple2DArray() {
        const string code = @"
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
        Helper.HelperRunFloat(comp, "array'2d", 1);
    }

    // Test 1D string array
    [Test]
    public void TestString1DArray() {
        const string code = @"
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
        Helper.HelperRunFloat(comp, "test", 1);
    }

    // Test 2D string array
    [Test]
    public void TestString2DArray() {
        const string code = @"
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
        Helper.HelperRunFloat(comp, "array'string'2d", 1);
    }

    // Test 1D dynamic array
    [Test]
    public void Test1DDynamicArray() {
        const string code = @"
                func array'dynamic'1d closed
                  arysize := 13
                  dim a(arysize)
                  for x:=1 to arysize do
                     a(x):=x*x
                  endfor x
                  for x:=1 to 13 do
                    if a(x)<>x*x then return false
                  endfor x
                  return true
                endfunc array'dynamic'1d
            ";
        Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
        Helper.HelperRunFloat(comp, "array'dynamic'1d", 1);
    }

    // Test 1D dynamic array with a range
    [Test]
    public void Test1DDynamicArrayWithRange() {
        const string code = @"
                func array'dynamic'1d'range closed
                  lowr := -5
                  highr := 5
                  dim a(lowr:highr)
                  for x:=lowr to highr do
                     a(x):=x*x
                  endfor x
                  for x:=lowr to highr do
                    if a(x)<>x*x then return false
                  endfor x
                  return true
                endfunc array'dynamic'1d'range
            ";
        Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
        Helper.HelperRunFloat(comp, "array'dynamic'1d'range", 1);
    }

    // Test 1D dynamic string array
    [Test]
    public void Test1DDynamicStringArray() {
        const string code = @"
                func array'dynamic'string'1d closed
                  arysize := 13
                  dim a$(arysize) of 5
                  for x:=1 to arysize do
                     a$(x):=str$(x*y)
                  endfor x
                  for x:=1 to 13 do
                    if a$(x)<>str$(x*y) then return false
                  endfor x
                  return true
                endfunc array'dynamic'string'1d
            ";
        Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions { Strict = true });
        Helper.HelperRunFloat(comp, "array'dynamic'string'1d", 1);
    }

    // Test redimensioning 1D dynamic array. After redimensioning,
    // all array elements will have been reset to 0.
    [Test]
    public void Test1DRedimDynamicArray() {
        const string code = @"
                func array'redim'dynamic'1d closed
                  arysize := 13
                  dim a(arysize)
                  for x:=1 to arysize do
                     a(x):=x*x
                  endfor x
                  arysize := 20
                  dim a(arysize)
                  for x:=1 to 20 do
                    if a(x)<>0 then return false
                  endfor x
                  return true
                endfunc array'redim'dynamic'1d
            ";
        Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
        Helper.HelperRunFloat(comp, "array'redim'dynamic'1d", 1);
    }

    // Test 2D dynamic array
    [Test]
    public void Test2DDynamicStringArray() {
        const string code = @"
                func array'dynamic'2d closed
                  max'x := 5
                  max'y := 7
                  dim a(max'x, max'y)
                  for x:=1 to max'x do
                     for y:=1 to max'y do
                        a(x,y):=x*y
                     endfor y
                  endfor x
                  for x:=1 to max'x do
                     for y:=1 to max'y do
                        if a(x,y)<>x*y then return false
                     endfor y
                  endfor x
                  return true
                endfunc array'dynamic'2d
            ";
        Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions { Strict = true });
        Helper.HelperRunFloat(comp, "array'dynamic'2d", 1);
    }

    // Test 1D dynamic array with a range
    [Test]
    public void Test2DDynamicArrayWithRange() {
        const string code = @"
                func array'dynamic'2d'range closed
                  max'x'low := 0
                  max'x'high := 20
                  max'y'low := 10
                  max'y'high := 17
                  dim a(max'x'low:max'x'high, max'y'low:max'y'high)
                  for x:=max'x'low to max'x'high do
                     for y:=max'y'low to max'y'high do
                        a(x,y):=x*y
                     endfor y
                  endfor x
                  for x:=max'x'high to max'x'low step -1 do
                     for y:=max'y'high to max'y'low step -1 do
                        if a(x,y)<>x*y then return false
                     endfor y
                  endfor x
                  return true
                endfunc array'dynamic'2d'range
            ";
        Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
        Helper.HelperRunFloat(comp, "array'dynamic'2d'range", 1);
    }

    // Test catching inconsistent array dimensions
    [Test]
    public void TestInconsistentDimensions() {
        const string code = @"
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
        Message[] expectedErrors = [
            new(null, MessageLevel.Error, MessageCode.MISSINGARRAYDIMENSIONS, 140, null),
            new(null, MessageLevel.Error, MessageCode.MISSINGARRAYDIMENSIONS, 190, null)
        ];
        ComalHelper.HelperCompileAndCheckErrors(code, new ComalOptions(), expectedErrors, true);
    }

    // Test Simple bounded array
    [Test]
    public void TestSimpleBoundedArray() {
        const string code = @"
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
        Helper.HelperRunFloat(comp, "test", 1);
    }

    // Test catching out of bounds error
    [Test]
    public void TestArrayOutOfBounds() {
        const string code = @"
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
        const string code = @"
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
        Helper.HelperRunFloat(comp, "array'byval", 1);
    }

    // Test calling a function with an array by reference.
    [Test]
    public void TestPassArrayByRef() {
        const string code = @"
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
        Helper.HelperRunFloat(comp, "array'byref", 1);
    }
}