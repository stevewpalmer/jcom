// JFortran Compiler
// Unit tests for conditional constructs
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

namespace FortranTests {

    public class ConditionalTests {

        // Arithmetic IF block validation
        [Test]
        public void ConditionalArithmeticIF1() {
            string [] code = {
                "      FUNCTION ITEST",
                "        A = 45",
                "        IF (A.LT.47) A= 90",
                "        ITEST = A",
                "      END"
            };
            Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
            Utilities.Helper.HelperRunInteger(comp, "ITEST", 90);
        }

        // Arithmetic IF block validation
        [Test]
        public void ConditionalArithmeticIF2() {
            string [] code = {
                "      FUNCTION ITEST",
                "        A = 88",
                "        IF (A.LT.47) A= 90",
                "        ITEST = A",
                "      END"
            };
            Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
            Utilities.Helper.HelperRunInteger(comp, "ITEST", 88);
        }

        // Block IF block validation
        [Test]
        public void ConditionalBlockIF1() {
            string [] code = {
                "      FUNCTION ITEST",
                "        A = 45",
                "        IF (A.LT.47) THEN",
                "          ITEST = 33",
                "        ENDIF",
                "      END"
            };
            Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
            Utilities.Helper.HelperRunInteger(comp, "ITEST", 33);
        }

        // Block IF block validation
        [Test]
        public void ConditionalBlockIF2() {
            string [] code = {
                "      FUNCTION ITEST",
                "        A = 45",
                "        IF (A.GT.47) THEN",
                "          ITEST = 33",
                "        ELSE",
                "          ITEST = 67",
                "        ENDIF",
                "      END"
            };
            Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
            Utilities.Helper.HelperRunInteger(comp, "ITEST", 67);
        }

        // Block IF block validation
        [Test]
        public void ConditionalBlockIF3() {
            string [] code = {
                "      FUNCTION ITEST",
                "        A = 45",
                "        IF (A.GT.47) THEN",
                "          ITEST = 33",
                "        ELSE IF (A.EQ.45) THEN",
                "          ITEST = 81",
                "        ELSE",
                "          ITEST = 67",
                "        ENDIF",
                "      END"
            };
            Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
            Utilities.Helper.HelperRunInteger(comp, "ITEST", 81);
        }

        // Block IF block validation
        [Test]
        public void ConditionalBlockIF4() {
            string [] code = {
                "      FUNCTION ITEST",
                "        A = 45",
                "        IF (A.GT.47) THEN",
                "          ITEST = 33",
                "        ELSE IF (A.EQ.43) THEN",
                "          ITEST = 11",
                "        ELSE IF (A.EQ.45) THEN",
                "          ITEST = 81",
                "        ELSE",
                "          ITEST = 67",
                "        ENDIF",
                "      END"
            };
            Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
            Utilities.Helper.HelperRunInteger(comp, "ITEST", 81);
        }

        // Block IF block validation
        [Test]
        public void ConditionalBlockIF5() {
            string [] code = {
                "      FUNCTION ITEST",
                "        A = 45",
                "        IF (A.LT.47) THEN",
                "           IF (A.EQ.46) THEN",
                "             ITEST=-44",
                "           ELSE",
                "             ITEST=177",
                "           ENDIF",
                "        ENDIF",
                "      END"
            };
            Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
            Utilities.Helper.HelperRunInteger(comp, "ITEST", 177);
        }

        // Block IF block validation
        [Test]
        public void ConditionalBlockIF6() {
            string [] code = {
                "      FUNCTION ITEST",
                "        A = 55",
                "        IF (A.LT.47) THEN",
                "           IF (A.EQ.46) THEN",
                "             ITEST=-44",
                "           ELSE",
                "             ITEST=177",
                "           ENDIF",
                "        ELSE",
                "          ITEST=94",
                "        ENDIF",
                "      END"
            };
            Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
            Utilities.Helper.HelperRunInteger(comp, "ITEST", 94);
        }
    }
}
