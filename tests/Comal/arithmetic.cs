// JComal
// Unit tests for arithmetic expressions
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
using JComal;
using NUnit.Framework;

namespace ComalTests {
    [TestFixture]

    public class Arithmetic {

        // Test general constant usage.
        [Test]
        public void ArithTestConstantUsage() {
            string code = @"
                FUNC ITEST
                  WIDTH:=7
                  HEIGHT:=9
                  AREA:=WIDTH*HEIGHT
                  RETURN AREA
                ENDFUNC
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Utilities.Helper.HelperRunFloat(comp, "ITEST", 63);
        }

        // Verify basic addition expression generation
        [Test]
        public void ArithBasicAddition() {
            string code = @"
                FUNC TEST#
                  LET A# := 20
                  B# := A#
                  RETURN A#+B#
                ENDFUNC
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Utilities.Helper.HelperRunInteger(comp, "TEST#", 40);
        }

        // Verify basic subtraction expression generation
        [Test]
        public void ArithBasicSubtraction() {
            string code = @"
                FUNC TEST#
                  LET A# := 4543
                  B# := 784
                  RETURN A#-B#
                ENDFUNC
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Utilities.Helper.HelperRunInteger(comp, "TEST#", 3759);
        }

        // Verify basic multiplication expression generation
        [Test]
        public void ArithBasicMultiplication() {
            string code = @"
                FUNC TEST#
                  LET A# := 90
                  LET B# := 45
                  RETURN A#*B#
                ENDFUNC
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Utilities.Helper.HelperRunInteger(comp, "TEST#", 4050);
        }

        // Verify basic division expression generation
        [Test]
        public void ArithBasicDivision() {
            string code = @"
                FUNC TEST
                  LET A := 35.60
                  LET B := 1.78
                  RETURN A/B
                ENDFUNC
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Utilities.Helper.HelperRunFloat(comp, "TEST", 20f);
        }

        // Verify implicit precedence
        [Test]
        public void ArithImplicitPrecedence1() {
            string code = @"
                FUNC TEST
                  RETURN 10 + 4 * 6
                ENDFUNC
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Utilities.Helper.HelperRunFloat(comp, "TEST", 34f);
        }

        // Verify implicit precedence
        // Exponential is higher than multiplication so this is (10^3) * 2
        [Test]
        public void ArithImplicitPrecedence2() {
            string code = @"
                FUNC TEST
                  RETURN 10 ^ 3 * 2
                ENDFUNC
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Utilities.Helper.HelperRunFloat(comp, "TEST", 2000f);
        }

        // Verify divison by zero blows up the code
        // Note that literal division by zero in a constant is a compile-time
        // error and handled separately.
        [Test]
        public void ArithDivisionByZero() {
            string code = @"
                FUNC ITEST
                  I# := 10
                  J# := 0
                  RETURN I#/J#
                ENDFUNC
            ";

            ComalOptions opts = new();
            Compiler comp = new(opts, new(opts));
            comp.CompileString(code, true);
            Assert.AreEqual(0, comp.Messages.ErrorCount);
            Assert.Throws(typeof(DivideByZeroException), delegate { comp.Execute("ITEST"); });
        }

        // Verify expression simplification involving the three
        // exponential simplification we explicitly handle.
        [Test]
        public void ArithSimplificationExp() {
            string code = @"
                FUNC SIMPLIFY1
                  LH:=16
                  RETURN LH^(-1) + LH^0 + LH^1
                ENDFUNC
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Utilities.Helper.HelperRunFloat(comp, "SIMPLIFY1", 17.0625f);
        }

        // Verify concatenation operator
        [Test]
        public void ArithConcatenation() {
            string code = @"
                FUNC CONCATTEST$
                  RETURN ""90"" + ""45"" + ""34""
                ENDFUNC
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Utilities.Helper.HelperRunString(comp, "CONCATTEST$", "904534");
        }

        // Verify DIV operator adheres to the Comal Common standard for
        // INT(x/y) where INT is implemented as FLOOR().
        [Test]
        public void ArithDiv() {
            string code = @"
                FUNC DivTest#
                  IF 7 DIV (-3) <> -3 THEN RETURN FALSE
                  A# := -7
                  B# := 3
                  IF A# DIV B# <> -3 THEN RETURN FALSE
                  AR := -7
                  BR := 3
                  IF AR DIV BR <> -3 THEN RETURN FALSE
                  RETURN TRUE
                ENDFUNC
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Utilities.Helper.HelperRunInteger(comp, "DivTest#", 1);
        }

        // Verify MOD operator
        [Test]
        public void ArithMod() {
            string code = @"
                FUNC ModTest#
                  IF 7 MOD -3 <> -2 THEN RETURN FALSE
                  A# := -7
                  B# := 3
                  IF A# MOD B# <> 2 THEN RETURN FALSE
                  AR := -7
                  BR := -3
                  IF AR MOD BR <> -1 THEN RETURN FALSE
                  RETURN TRUE
                ENDFUNC
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Utilities.Helper.HelperRunInteger(comp, "ModTest#", 1);
        }

        // Verify incremental add operator
        [Test]
        public void IncrAdd() {
            string code = @"
                FUNC IncrAdd#
                  A# := -7
                  A# :+ 3
                  IF A# <> -4 THEN RETURN FALSE
                  AR := 47
                  AR :+ -3
                  IF AR <> 44 THEN RETURN FALSE
                  RETURN TRUE
                ENDFUNC
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Utilities.Helper.HelperRunInteger(comp, "IncrAdd#", 1);
        }

        // Verify incremental subtraction operator
        [Test]
        public void IncrSub() {
            string code = @"
                FUNC IncrSub#
                  A# := 40
                  A# :- 12
                  IF A# <> 28 THEN RETURN FALSE
                  AR := 47
                  AR :- -3
                  IF AR <> 50 THEN RETURN FALSE
                  RETURN TRUE
                ENDFUNC
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Utilities.Helper.HelperRunInteger(comp, "IncrSub#", 1);
        }
    }
}
