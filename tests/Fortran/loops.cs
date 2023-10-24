// JFortran Compiler
// Unit tests for DO loop statements
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

using JFortran;
using NUnit.Framework;
using Utilities;

namespace FortranTests {
    [TestFixture]
    public class LoopTests {

        // Verify basic loop operation
        [Test]
        public void Loops1() {
            string[] code = {
                "      FUNCTION ITEST",
                "        DO 20,I=1,10",
                "          J=J+I",
                "20      CONTINUE",
                "        RETURN J",
                "      END"
            };
            Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
            Helper.HelperRunInteger(comp, "ITEST", 55);
        }

        // Verify nested loop operation
        [Test]
        public void Loops2() {
            string[] code = {
                "      FUNCTION ITEST",
                "        DO 20,I=1,10",
                "          DO 20,K=1,10",
                "            J=J+I",
                "20      CONTINUE",
                "        RETURN J",
                "      END"
            };
            Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
            Helper.HelperRunInteger(comp, "ITEST", 550);
        }

        // Verify loop control variable values at the end
        // of the loops.
        [Test]
        public void Loops3() {
            string[] code = {
                "      FUNCTION ITEST",
                "        DO 20,I=1,10",
                "          DO 20,K=1,10",
                "            J=J+I",
                "20      CONTINUE",
                "        RETURN I+K",
                "      END"
            };
            Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
            Helper.HelperRunInteger(comp, "ITEST", 22);
        }

        // Verify altering loop start,end and step variables do
        // not affect the number of iterations.
        [Test]
        public void Loops4() {
            string[] code = {
                "      FUNCTION ITEST",
                "        LSTART=2",
                "        LEND=10",
                "        LSTEP=2",
                "        DO 20,I=LSTART,LEND,LSTEP",
                "            J=J+1",
                "            LSTART=10",
                "            LEND=900",
                "            LSTEP=5",
                "20      CONTINUE",
                "        RETURN J",
                "      END"
            };
            Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
            Helper.HelperRunInteger(comp, "ITEST", 5);
        }

        // Verify negative iterations.
        [Test]
        public void Loops5() {
            string[] code = {
                "      FUNCTION ITEST",
                "        LSTART=40",
                "        LEND=4",
                "        LSTEP=-2",
                "        DO 20,I=LSTART,LEND,LSTEP",
                "            J=J+1",
                "20      CONTINUE",
                "        RETURN J",
                "      END"
            };
            Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
            Helper.HelperRunInteger(comp, "ITEST", 19);
        }

        // Verify zero loop execution when the end value is less
        // than the start value.
        [Test]
        public void Loops6() {
            string[] code = {
                "      FUNCTION ITEST",
                "        LSTART=50",
                "        LEND=30",
                "        DO 20,I=LSTART,LEND",
                "            J=J+1",
                "20      CONTINUE",
                "        RETURN J",
                "      END"
            };
            Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
            Helper.HelperRunInteger(comp, "ITEST", 0);
        }

        // Verify ENDDO to terminate the loop.
        [Test]
        public void Loops7() {
            string[] code = {
                "      FUNCTION ITEST",
                "        LSTART=1",
                "        LEND=20",
                "        DO I=LSTART,LEND",
                "            J=J+1",
                "        ENDDO",
                "        RETURN J",
                "      END"
            };
            Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
            Helper.HelperRunInteger(comp, "ITEST", 20);
        }

        // Verify DO WHILE loop.
        [Test]
        public void Loops8() {
            string[] code = {
                "      FUNCTION ITEST",
                "        LSTART=1",
                "        LEND=20",
                "        DO WHILE (LSTART.LE.LEND)",
                "            J=J+1",
                "            LSTART=LSTART+1",
                "        ENDDO",
                "        RETURN J",
                "      END"
            };
            Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
            Helper.HelperRunInteger(comp, "ITEST", 20);
        }

        // Verify DO WHILE loop using a terminating label
        // as the final statement
        [Test]
        public void Loops9() {
            string[] code = {
                "      FUNCTION ITEST",
                "        LSTART=1",
                "        LEND=20",
                "        DO 100 WHILE (LSTART.LE.LEND)",
                "            J=J+1",
                "            LSTART=LSTART+1",
                "100     CONTINUE",
                "        RETURN J",
                "      END"
            };
            Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
            Helper.HelperRunInteger(comp, "ITEST", 20);
        }

        // Verify zero start loop optimisation
        [Test]
        public void Loops10() {
            string[] code = {
                "      FUNCTION ITEST",
                "        LEND=9",
                "        DO 20,I=0,LEND",
                "          J=J+I",
                "20      CONTINUE",
                "        RETURN J",
                "      END"
            };
            Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
            Helper.HelperRunInteger(comp, "ITEST", 45);
        }

    }
}