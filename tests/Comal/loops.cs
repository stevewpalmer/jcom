// JComal
// Unit tests for loops
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

    public class Loops {

        // Test LOOP and conditional EXIT THEN
        [Test]
        public void TestLoop1() {
            string code = @"
                FUNC test1loop CLOSED
                  count:=0
                  LOOP
                    EXIT WHEN count=2
                    count:=count+1
                  ENDLOOP
                  IF count<>2 THEN RETURN FALSE
                  RETURN TRUE
                ENDFUNC test1loop
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Utilities.Helper.HelperRunFloat(comp, "test1loop", 1);
        }

        // Test unconditional EXIT
        [Test]
        public void TestLoop2() {
            string code = @"
                FUNC test2loop CLOSED
                  LOOP
                    EXIT
                    RETURN FALSE
                  ENDLOOP
                  RETURN TRUE
                ENDFUNC test2loop
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Utilities.Helper.HelperRunFloat(comp, "test2loop", 1);
        }

        // Test unconditional EXIT with GOTO
        [Test]
        public void TestLoop3() {
            string code = @"
                FUNC test3loop CLOSED
                  LOOP
                    GOTO SkipExit
                    EXIT
                    SkipExit:
                    RETURN FALSE
                  ENDLOOP
                  RETURN TRUE
                ENDFUNC test3loop
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Utilities.Helper.HelperRunFloat(comp, "test3loop", 0);
        }

        // Test WHILE loop with false condition at start
        [Test]
        public void TestLoop4() {
            string code = @"
                FUNC test4loop CLOSED
                  WHILE FALSE DO
                    RETURN FALSE
                  ENDWHILE
                  RETURN TRUE
                ENDFUNC test4loop
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Utilities.Helper.HelperRunFloat(comp, "test4loop", 1);
        }

        // Test WHILE loop
        [Test]
        public void TestLoop5() {
            string code = @"
                FUNC test5loop# CLOSED
                  c#:=1
                  WHILE c#<10 DO
                    c#:=c#+1
                  ENDWHILE
                  RETURN c#
                ENDFUNC test5loop#
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Utilities.Helper.HelperRunInteger(comp, "test5loop#", 10);
        }

        // Test single line REPEAT loop
        [Test]
        public void TestLoop6() {
            string code = @"
                FUNC test6loop# CLOSED
                  c#:=1
                  REPEAT c#:=c#+1 UNTIL c#=10
                  RETURN c#
                ENDFUNC test6loop#
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Utilities.Helper.HelperRunInteger(comp, "test6loop#", 10);
        }

        // Test single line WHILE loop
        [Test]
        public void TestLoop7() {
            string code = @"
                FUNC test7loop# CLOSED
                  c#:=1
                  WHILE c#<10 DO c#:=c#+1
                  RETURN c#
                ENDFUNC test7loop#
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Utilities.Helper.HelperRunInteger(comp, "test7loop#", 10);
        }

        // Test simple FOR loop
        [Test]
        public void TestForLoop1() {
            string code = @"
                FUNC for'test'1 CLOSED
                    total:=0
                    FOR x:=1 TO 10
                        total:+x
                    ENDFOR
                    RETURN total
                ENDFUNC
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Utilities.Helper.HelperRunFloat(comp, "for'test'1", 55);
        }

        // Verify that the FOR loop variable is local
        // to the loop body.
        [Test]
        public void TestForLoopVariable() {
            string code = @"
                FUNC for'var'test CLOSED
                    total:=0
                    x:=99
                    FOR x:=1 TO 10
                        total:+x
                    ENDFOR
                    RETURN x
                ENDFUNC
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Utilities.Helper.HelperRunFloat(comp, "for'var'test", 99);
        }

        // Test FOR loop with STEP
        [Test]
        public void TestForLoop2() {
            string code = @"
                FUNC for'test'2 CLOSED
                    total:=0
                    FOR x=1 TO 100 STEP 2
                        total:+x
                    ENDFOR
                    RETURN total
                ENDFUNC
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Utilities.Helper.HelperRunFloat(comp, "for'test'2", 2500);
        }

        // Test FOR loop with negative step
        [Test]
        public void TestForLoop3() {
            string code = @"
                FUNC for'test'3 CLOSED
                    total:=0
                    FOR x=10 TO 1 STEP -2 DO
                        total:+x
                    ENDFOR
                    RETURN total
                ENDFUNC
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Utilities.Helper.HelperRunFloat(comp, "for'test'3", 30);
        }

        // Test nested FOR loops
        [Test]
        public void TestForLoop4() {
            string code = @"
                FUNC for'test'4 CLOSED
                    total:=0
                    FOR x=1 TO 10 DO
                        FOR y:=1 TO 10 DO
                            total:+x*y
                        NEXT y
                    ENDFOR x
                    RETURN total
                ENDFUNC
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Utilities.Helper.HelperRunFloat(comp, "for'test'4", 3025);
        }

        // Test single line FOR loop
        [Test]
        public void TestForLoop5() {
            string code = @"
                FUNC for'test'5 CLOSED
                    total:=0
                    FOR x=100 TO 200 DO total :+ x
                    RETURN total
                ENDFUNC
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Utilities.Helper.HelperRunFloat(comp, "for'test'5", 15150);
        }
    }
}


