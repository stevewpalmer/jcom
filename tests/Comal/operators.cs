// JComal
// Unit tests for operators
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

    public class Operators {

        // Test assignments.
        [Test]
        public void TestAssignment() {
            string code = @"
                FUNC test'assign CLOSED
                  LET A# := 14
                  IF A# <> 14 THEN RETURN FALSE
                  DIM D$ OF 4
                  B# := 87; C := 14.5;D$:=""SPAM""
                  IF B# <> 87 THEN RETURN FALSE
                  IF C <> 14.5 THEN RETURN FALSE
                  IF D$ <> ""SPAM"" THEN RETURN FALSE
                  RETURN TRUE
                ENDFUNC test'assign
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Utilities.Helper.HelperRunFloat(comp, "test'assign", 1);
        }

        // Test the bitwise AND operator.
        [Test]
        public void TestAnd() {
            string code = @"
                FUNC test'and CLOSED
                  IF (TRUE AND TRUE)<> TRUE THEN RETURN FALSE
                  IF (TRUE AND FALSE)<> FALSE THEN RETURN FALSE
                  IF (FALSE AND TRUE)<> FALSE THEN RETURN FALSE
                  IF (FALSE AND FALSE)<> FALSE THEN RETURN FALSE
                  RETURN TRUE
                ENDFUNC test'and
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Utilities.Helper.HelperRunFloat(comp, "test'and", 1);
        }

        // Verify BITOR operator
        [Test]
        public void TestBitOR() {
            string code = @"
                FUNC test'bitor CLOSED
                    IF (3 BITOR 3)<>3 THEN RETURN FALSE
                    IF (3 BITOR 0)<>3 THEN RETURN FALSE
                    IF (5 BITOR 6)<>7 THEN RETURN FALSE
                    IF (0 BITOR 0)<>0 THEN RETURN FALSE
                    RETURN TRUE
                ENDFUNC test'bitor
                ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Utilities.Helper.HelperRunFloat(comp, "test'bitor", 1);
        }

        // Verify BITAND operator
        [Test]
        public void TestBitAND() {
            string code = @"
                FUNC test'bitand CLOSED
                    IF (3 BITAND 3)<>3 THEN RETURN FALSE
                    IF (3 BITAND 0)<>0 THEN RETURN FALSE
                    IF (5 BITAND 6)<>4 THEN RETURN FALSE
                    IF (0 BITAND 0)<>0 THEN RETURN FALSE
                    RETURN TRUE
                ENDFUNC test'bitand
                ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Utilities.Helper.HelperRunFloat(comp, "test'bitand", 1);
        }

        // Verify BITXOR operator
        [Test]
        public void TestBitXOR() {
            string code = @"
                FUNC test'bitxor CLOSED
                    IF (3 BITXOR 3)<>0 THEN RETURN FALSE
                    IF (3 BITXOR 0)<>3 THEN RETURN FALSE
                    IF (5 BITXOR 6)<>3 THEN RETURN FALSE
                    IF (0 BITXOR 0)<>0 THEN RETURN FALSE
                    RETURN TRUE
                ENDFUNC test'bitxor
                ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Utilities.Helper.HelperRunFloat(comp, "test'bitxor", 1);
        }

        // Verify logical AND operator
        [Test]
        public void TestLogicalAND() {
            string code = @"
                FUNC test'logical'and
                    counter := 0
                    p := 12 AND THEN inc'counter()
                    IF counter <> 1 THEN RETURN FALSE
                    p := 0 AND THEN inc'counter()
                    IF counter <> 1 THEN RETURN FALSE
                    RETURN TRUE
                ENDFUNC test'logical'and
                FUNC inc'counter
                    counter :+ 1
                    RETURN 1
                ENDFUNC
                ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Utilities.Helper.HelperRunFloat(comp, "test'logical'and", 1);
        }

        // Verify logical OR operator
        [Test]
        public void TestLogicalOR() {
            string code = @"
                FUNC test'logical'or
                    counter := 0
                    p := 12 OR THEN inc'counter()
                    IF counter <> 0 THEN RETURN FALSE
                    p := 0 OR THEN inc'counter()
                    IF counter <> 1 THEN RETURN FALSE
                    RETURN TRUE
                ENDFUNC test'logical'or
                FUNC inc'counter
                    counter :+ 1
                    RETURN 1
                ENDFUNC
                ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Utilities.Helper.HelperRunFloat(comp, "test'logical'or", 1);
        }
    }
}


