// JComal
// Unit tests for procedures and functions
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
using JComLib;
using NUnit.Framework;
using TestUtilities;

namespace ComalTests;

[TestFixture]
public class ProcFunc {

    // Test calling a procedure.
    [Test]
    public void TestProc1() {
        string code = @"
                FUNC test1
                  PROC1
                  RETURN TRUE
                ENDFUNC test1
                  PROC PROC1
                  RETURN
                ENDPROC PROC1
            ";
        Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
        Helper.HelperRunFloat(comp, "test1", 1);
    }

    // Test calling an open function.
    [Test]
    public void TestFunc1() {
        string code = @"
                FUNC test1
                  RETURN FUNC1
                ENDFUNC test1
                FUNC FUNC1
                  RETURN TRUE
                ENDFUNC FUNC1
            ";
        Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
        Helper.HelperRunFloat(comp, "test1", 1);
    }

    // Test calling a procedure with a reference parameter
    [Test]
    public void TestProc2() {
        string code = @"
                FUNC test1
                  A#:=0
                  PROC1(A#)
                  PROC2(A#)
                  RETURN A#
                ENDFUNC test1
                PROC PROC1(REF V#)
                  V#:=1
                ENDPROC PROC1
                PROC PROC2(V#)
                  V#:=0
                ENDPROC PROC2
            ";
        Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
        Helper.HelperRunFloat(comp, "test1", 1);
    }

    // Test calling a function
    [Test]
    public void TestFunc2() {
        string code = @"
                FUNC test'lower
                    DIM foo$ OF 7
                    foo$:=""HELLO""
                    IF lower$(foo$)<>""hello"" return false
                    return true
                ENDFUNC
                FUNC lower$(text$) CLOSED
                  FOR x:= 1 TO LEN(text$) DO
                    num:= ORD(text$(x: x))
                    IF num>= 65 AND num<= 90 THEN
                        num:+32
                    ENDIF
                    text$(x:x):= CHR$(num)
                  ENDFOR x
                  RETURN text$
                ENDFUNC lower$
            ";
        Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
        Helper.HelperRunFloat(comp, "test'lower", 1);
    }

    // Test using IMPORT in an open procedure is an error
    [Test]
    public void TestBadImport1() {
        string code = @"
                PROC test'proc'1
                   IMPORT foo
                ENDPROC
                FUNC test'func'1
                   IMPORT foo
                   RETURN 12
                ENDFUNC
                FUNC test'func'2 CLOSED
                   IMPORT foo
                   RETURN 13
                ENDFUNC
                FUNC foo
                   return TRUE
                ENDFUNC
            ";
        Message[] expectedErrors = [
            new(null, MessageLevel.Error, MessageCode.NOTINCLOSED, 110, null),
            new(null, MessageLevel.Error, MessageCode.NOTINCLOSED, 140, null)
        ];
        ComalHelper.HelperCompileAndCheckErrors(code, new ComalOptions(), expectedErrors, true);
    }

    // Test using duplicate IMPORT statements
    [Test]
    public void TestBadImport2() {
        string code = @"
                PROC test'proc'1 CLOSED
                   IMPORT foo,foo
                ENDPROC
                FUNC foo
                   return TRUE
                ENDFUNC
            ";
        Message[] expectedErrors = [
            new(null, MessageLevel.Error, MessageCode.ALREADYIMPORTED, 110, null)
        ];
        ComalHelper.HelperCompileAndCheckErrors(code, new ComalOptions(), expectedErrors, true);
    }

    // Test scope.
    [Test]
    public void TestProcFuncScope() {
        string code = @"
                FUNC scopetest$
                  DIM text$ OF 10
                  text$:=""happy""
                  testing
                  RETURN text$
                ENDFUNC
                PROC testing CLOSED
                  text$:=""sad""
                ENDPROC testing
            ";
        Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
        Helper.HelperRunString(comp, "scopetest$", "happy");
    }

    // Test scope of a variable in an open procedure. This should succeed
    // since PROC1 is not CLOSED so it should have access to Foo# in the
    // main scope.
    [Test]
    public void TestProc3() {
        string code = @"
                Foo# := 1234
                PROC1
                RETURN
                PROC PROC1
                IF Foo#=1234 THEN STOP
                RETURN
                ENDPROC PROC
            ";

        // Turn on interactive mode to prevent us generating the top level exception
        // handler in the code.
        ComalOptions opts = new() {
            Interactive = true
        };
        Compiler comp = ComalHelper.HelperCompile(code, opts);
        Assert.Throws(typeof(JComRuntimeException), delegate { comp.Execute("Main"); });
    }
}