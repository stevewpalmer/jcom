// JFortran Compiler
// Unit tests for arithmetic expressions
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

using System;
using JFortran;
using NUnit.Framework;

namespace FortranTests {
    [TestFixture]

    public class ArithmeticTests {

        // Test general constant usage.
        [Test]
        public void ArithTestConstantUsage() {
            string [] code = {
                "      FUNCTION ITEST",
                "      PARAMETER (PI=3.1459, PI2=2*PI)",
                "        REAL A",
                "        A = (PI + PI2) / 3.0",
                "        RETURN A*100",
                "      END"
            };
            Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
            Utilities.Helper.HelperRunInteger(comp, "ITEST", 314);
        }
        
        // Verify basic addition expression generation
        [Test]
        public void ArithBasicAddition() {
            string [] code = {
                "      FUNCTION TEST",
                "        INTEGER A",
                "        A = 20",
                "        B = A",
                "        RETURN A+B",
                "      END"
            };
            Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
            Utilities.Helper.HelperRunFloat(comp, "TEST", 40f);
        }

        // Verify basic subtraction expression generation
        [Test]
        public void ArithBasicSubtraction() {
            string [] code = {
                "      FUNCTION ITEST",
                "        INTEGER A,B",
                "        A = 4543",
                "        B = 784",
                "        RETURN A-B",
                "      END"
            };
            Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
            Utilities.Helper.HelperRunInteger(comp, "ITEST", 3759);
        }

        // Verify basic multiplication expression generation
        [Test]
        public void ArithBasicMultiplication() {
            string [] code = {
                "      FUNCTION ITEST",
                "        INTEGER A",
                "        INTEGER B",
                "        A = 90",
                "        B = 45",
                "        RETURN A*B",
                "      END"
            };
            Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
            Utilities.Helper.HelperRunInteger(comp, "ITEST", 4050);
        }

        // Verify basic division expression generation
        [Test]
        public void ArithBasicDivision() {
            string [] code = {
                "      FUNCTION TEST",
                "        REAL A",
                "        REAL B",
                "        A = 35.60",
                "        B = 1.78",
                "        RETURN A/B",
                "      END"
            };
            Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
            Utilities.Helper.HelperRunFloat(comp, "TEST", 20f);
        }

        // Verify implicit precedence
        [Test]
        public void ArithImplicitPrecedence1() {
            string [] code = {
                "      FUNCTION TEST",
                "        RETURN 10 + 4 * 6",
                "      END"
            };
            Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
            Utilities.Helper.HelperRunFloat(comp, "TEST", 34f);
        }

        // Verify implicit precedence
        // Exponential is higher than multiplication so this is (10**3) * 2
        [Test]
        public void ArithImplicitPrecedence2() {
            string [] code = {
                "      FUNCTION TEST",
                "        RETURN 10 ** 3 * 2",
                "      END"
            };
            Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
            Utilities.Helper.HelperRunFloat(comp, "TEST", 2000f);
        }

        // Verify divison by zero blows up the code
        // Note that literal division by zero in a constant is a compile-time
        // error and handled separately.
        [Test]
        public void ArithDivisionByZero() {
            string [] code = {
                "      FUNCTION ITEST",
                "        INTEGER I,J",
                "        I = 10",
                "        J = 0",
                "        RETURN I/J",
                "      END"
            };
            
            Compiler comp = new(new FortranOptions());
            comp.CompileString(code);
            Assert.AreEqual(0, comp.Messages.ErrorCount);
            Assert.Throws(typeof(DivideByZeroException), delegate { comp.Execute("ITEST"); });
        }

        // Verify expression simplification involving the three
        // exponential simplification we explicitly handle.
        [Test]
        public void ArithSimplificationExp() {
            string [] code = {
                "      FUNCTION SIMPLIFY1",
                "        REAL LH",
                "        LH=16",
                "        RETURN LH**(-1) + LH**0 + LH**1",
                "      END"
            };
            Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
            Utilities.Helper.HelperRunFloat(comp, "SIMPLIFY1", 17.0625f);
        }

        // Verify concatenation operator
        [Test]
        public void ArithConcatenation() {
            string [] code = {
                "      FUNCTION CONCATTEST",
                "        CHARACTER*6 CONCATTEST",
                "        CONCATTEST='90' // '45' // '34'",
                "        RETURN",
                "      END"
            };
            Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
            Utilities.Helper.HelperRunString(comp, "CONCATTEST", "904534");
        }

        // Verify concatenation operator
        [Test]
        public void ArithConcatenation2() {
            string[] code = {
                "      FUNCTION CONCATTEST",
                "        INTEGER CONCATTEST",
                "        CHARACTER CVN001*7",
                "        DATA CVN001 / 'ONE+TWO' /",
                "        CONCATTEST=LEN(CVN001//'EIGHTEEN')",
                "        RETURN",
                "      END"
            };
            Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
            Utilities.Helper.HelperRunInteger(comp, "CONCATTEST", 15);
        }

        // Verify concatenation operator
        [Test]
        public void ArithConcatenation3() {
            string[] code = {
                "      FUNCTION CONCATTEST",
                "        INTEGER CONCATTEST, IVCOMP",
                "        CHARACTER CVCOMP*65, CVN002*35",
                "        DATA CVN002 / 'THIS-IS-A-LONG-CHARACTER-STRING' / ",
                "        CVCOMP = 'A-LONG-CHARTER-PLANE'",
                "        IVCOMP=0",
                "        IF (CVCOMP.EQ.CVN002(9:19)//'TER-PLANE') IVCOMP = 1",
                "        CONCATTEST=IVCOMP",
                "      END"
            };
            Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
            Utilities.Helper.HelperRunInteger(comp, "CONCATTEST", 1);
        }
    }
}
