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
using TestUtilities;

namespace ComalTests;

[TestFixture]
public class Functions {

    // Test Abs() function.
    [Test]
    public void TestAbs() {
        const string code = @"
                func test'abs closed
                  if abs(1)<>1 then return false
                  if abs(-1)<>1 then return false
                  if abs(0)<>0 then return false
                  if abs(-3.9)<>3.9 then return false
                  return true
                endfunc
            ";
        ComalOptions opts = new();
        Compiler comp = ComalHelper.HelperCompile(code, opts);
        Helper.HelperRunFloat(comp, "test'abs", 1);
        opts.Inline = false;
        comp = ComalHelper.HelperCompile(code, opts);
        Helper.HelperRunFloat(comp, "test'abs", 1);
    }

    // Test Atn() function.
    [Test]
    public void TestAtn() {
        const string code = @"
                FUNC test'atn CLOSED
                  B := 0.5
                  B# := 4
                  IF ATN(0)<>0 THEN RETURN FALSE
                  IF ABS(ATN(1)-PI/4)>0.000001 THEN RETURN FALSE
                  IF ABS(ATN(TAN(0.5))-0.5)>0.000001 THEN RETURN FALSE
                  IF ABS(ATN(TAN(B))-B)>0.000001 THEN RETURN FALSE
                  IF INT(ATN(TAN(B#))-B#)>0 THEN RETURN FALSE
                  RETURN TRUE
                ENDFUNC
            ";
        ComalOptions opts = new();
        Compiler comp = ComalHelper.HelperCompile(code, opts);
        Helper.HelperRunFloat(comp, "test'atn", 1);
        opts.Inline = false;
        comp = ComalHelper.HelperCompile(code, opts);
        Helper.HelperRunFloat(comp, "test'atn", 1);
    }

    // Test Sqr() function.
    [Test]
    public void TestSqr() {
        const string code = @"
                FUNC test'sqr CLOSED
                  BVS := SQR(1.6)
                  AVS := SQR(0.625) * BVS
                  IF AVS<>1 THEN RETURN FALSE
                  IF ABS(SQR(1000)-31.62277)>0.00001 THEN RETURN FALSE
                  IF ABS(SQR(8)*SQR(8)-8)>0.00001 THEN RETURN FALSE
                RETURN TRUE
                ENDFUNC
            ";
        ComalOptions opts = new();
        Compiler comp = ComalHelper.HelperCompile(code, opts);
        Helper.HelperRunFloat(comp, "test'sqr", 1);
        opts.Inline = false;
        comp = ComalHelper.HelperCompile(code, opts);
        Helper.HelperRunFloat(comp, "test'sqr", 1);
    }

    // Test Cos() function.
    [Test]
    public void TestCos() {
        const string code = @"
                FUNC test'cos CLOSED
                  B := 0.5
                  B# := 4
                  IF ABS(COS(0)-1)>0.000001 THEN RETURN FALSE
                  IF ABS(COS(PI/3)-0.5)>0.000001 THEN RETURN FALSE
                  IF ABS(COS(B)-COS(B))>0 THEN RETURN FALSE
                  IF INT(COS(B#)-B#)<>-5 THEN RETURN FALSE
                  RETURN TRUE
                ENDFUNC
            ";
        ComalOptions opts = new();
        Compiler comp = ComalHelper.HelperCompile(code, opts);
        Helper.HelperRunFloat(comp, "test'cos", 1);
        opts.Inline = false;
        comp = ComalHelper.HelperCompile(code, opts);
        Helper.HelperRunFloat(comp, "test'cos", 1);
    }

    // Test Sin() function.
    [Test]
    public void TestSin() {
        const string code = @"
                FUNC test'sin CLOSED
                  B := 0.5
                  B# := 4
                  IF SIN(0)<>0 THEN RETURN FALSE
                  IF ABS(SIN(PI/3)-SQR(3)/2)>0.000001 THEN RETURN FALSE
                  IF ABS(SIN(B)-SIN(B))>0 THEN RETURN FALSE
                  IF INT(SIN(B#)-B#)<>-5 THEN RETURN FALSE
                  RETURN TRUE
                ENDFUNC
            ";
        ComalOptions opts = new();
        Compiler comp = ComalHelper.HelperCompile(code, opts);
        Helper.HelperRunFloat(comp, "test'sin", 1);
        opts.Inline = false;
        comp = ComalHelper.HelperCompile(code, opts);
        Helper.HelperRunFloat(comp, "test'sin", 1);
    }

    // Test Ord() function.
    [Test]
    public void TestOrd() {
        const string code = @"
                FUNC test'ord CLOSED
                  IF CHR$(53)<>""5"" THEN RETURN FALSE
                  IF ORD(CHR$(53))<>53 THEN RETURN FALSE
                  RETURN TRUE
                ENDFUNC
            ";
        ComalOptions opts = new();
        Compiler comp = ComalHelper.HelperCompile(code, opts);
        Helper.HelperRunFloat(comp, "test'ord", 1);
        opts.Inline = false;
        comp = ComalHelper.HelperCompile(code, opts);
        Helper.HelperRunFloat(comp, "test'ord", 1);
    }

    // Test Log() function.
    [Test]
    public void TestLog() {
        const string code = @"
                FUNC test'log CLOSED
                  IF LOG(1)<>0 THEN RETURN FALSE
                  A := ABS(LOG(2.71828)-1)
                  IF A>0.5658 THEN RETURN FALSE
                  B := ABS(EXP(LOG(10)))
                  IF B>2.7183 THEN RETURN FALSE
                  RETURN TRUE
                ENDFUNC
            ";
        ComalOptions opts = new();
        Compiler comp = ComalHelper.HelperCompile(code, opts);
        Helper.HelperRunFloat(comp, "test'log", 1);
        opts.Inline = false;
        comp = ComalHelper.HelperCompile(code, opts);
        Helper.HelperRunFloat(comp, "test'log", 1);
    }

    // Test RANDOMIZE
    // Calls to RND in between must return the same value
    [Test]
    public void TestRandomize() {
        const string code = @"
                FUNC test'randomize CLOSED
                  RANDOMIZE 9
                  a:=RND
                  b:=RND(1,20)
                  RANDOMIZE 9
                  c:=RND
                  d:=RND(1,20)
                  IF a<>c RETURN FALSE
                  IF b<>d RETURN FALSE
                  RETURN TRUE
                ENDFUNC
            ";
        ComalOptions opts = new();
        Compiler comp = ComalHelper.HelperCompile(code, opts);
        Helper.HelperRunFloat(comp, "test'randomize", 1);
        opts.Inline = false;
        comp = ComalHelper.HelperCompile(code, opts);
        Helper.HelperRunFloat(comp, "test'randomize", 1);
    }

    // Test RND and RND(range)
    [Test]
    public void TestRndRange() {
        const string code = @"
                FUNC test'rnd'range CLOSED
                  FOR x:=1 TO 50 DO
                    r:=RND(1,20)
                    IF(r<1) OR(r>20) THEN RETURN FALSE
                    r:=RND
                    IF(r<0) OR(r>1) THEN RETURN FALSE
                  ENDFOR x
                  RETURN TRUE
                ENDFUNC
            ";
        ComalOptions opts = new();
        Compiler comp = ComalHelper.HelperCompile(code, opts);
        Helper.HelperRunFloat(comp, "test'rnd'range", 1);
        opts.Inline = false;
        comp = ComalHelper.HelperCompile(code, opts);
        Helper.HelperRunFloat(comp, "test'rnd'range", 1);
    }

    // Test SPC$
    [Test]
    public void TestSpc() {
        const string code = @"
                FUNC test'spc CLOSED
                  DIM A$ OF 11
                  A$ := ""HELLO"" + SPC$(1) + ""WORLD""
                  if A$<>""HELLO WORLD"" THEN RETURN FALSE
                  B# := 7
                  IF SPC$(B#)<>""       "" THEN RETURN FALSE
                  C := 3
                  IF SPC$(C+4)<>""       "" THEN RETURN FALSE
                  RETURN TRUE
                ENDFUNC
            ";
        ComalOptions opts = new();
        Compiler comp = ComalHelper.HelperCompile(code, opts);
        Helper.HelperRunFloat(comp, "test'spc", 1);
        opts.Inline = false;
        comp = ComalHelper.HelperCompile(code, opts);
        Helper.HelperRunFloat(comp, "test'spc", 1);
    }
}