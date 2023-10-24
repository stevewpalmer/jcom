// JComal
// Unit tests for string expressions
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
using Utilities;

namespace ComalTests {
    [TestFixture]
    public class Strings {

        // Test the rule that for str1 IN str2, where str1 is
        // an empty string then the result is 0.
        [Test]
        public void EmptyStringIn() {
            string code = @"
                FUNC INTEST
                OFF1:="""" IN ""INDEX""
                RETURN OFF1
                ENDFUNC
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Helper.HelperRunFloat(comp, "INTEST", 0);
        }

        // Test substring manipulation.
        [Test]
        public void Substrings1() {
            string code = @"
                FUNC sub'test#
                DIM t$ OF 4,s$ OF 3
                t$:=""abcd""
                s$:=t$(2:4)
                IF s$<>""bcd"" THEN RETURN FALSE
                t$(1:2):=""ef""
                IF t$<>""efcd"" THEN RETURN FALSE
                RETURN TRUE
                ENDFUNC sub'test#
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Helper.HelperRunInteger(comp, "sub'test#", 1);
        }

        // Test substring manipulation with no end range
        [Test]
        public void Substrings2() {
            string code = @"
                FUNC sub'test#
                DIM t$ OF 4,s$ OF 2
                t$:=""abcd""
                s$:=t$(3:)
                IF s$<>""cd"" THEN RETURN FALSE
                IF t$(1:)<>""abcd"" THEN RETURN FALSE
                RETURN TRUE
                ENDFUNC sub'test#
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Helper.HelperRunInteger(comp, "sub'test#", 1);
        }

        // Test string concatenation and truncation
        [Test]
        public void StringConcat() {
            string code = @"
                FUNC concat
                DIM t$ OF 10
                t$:=""abcde""
                t$:+""fghijk""
                IF LEN(t$)<>10 THEN RETURN FALSE
                IF t$<>""abcdefghij"" THEN RETURN FALSE
                RETURN TRUE
                ENDFUNC concat
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Helper.HelperRunFloat(comp, "concat", 1);
        }

        // Test string concatenation where string is less than the
        // allocated size.
        [Test]
        public void StringConcat2() {
            string code = @"
                FUNC concat
                DIM t$ OF 10
                t$:=""ABC""
                t$:+""X""
                IF LEN(t$)<>4 THEN RETURN FALSE
                IF t$<>""ABCX "" THEN RETURN FALSE
                RETURN TRUE
                ENDFUNC concat
            ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Helper.HelperRunFloat(comp, "concat", 1);
        }

        // Test substring where string is shorter than the index
        [Test]
        public void Substring3() {
            string code = @"
                FUNC sub'test#
                    DIM t$ OF 8
                    t$:=""abcdefgh""
                    t$(2:4)=""y""
                    IF t$<>""ay  efgh"" THEN RETURN FALSE
                    t$(1:8)=""""
                    IF t$<>"""" THEN RETURN FALSE
                    RETURN TRUE
                ENDFUNC sub'test#
                ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Helper.HelperRunInteger(comp, "sub'test#", 1);
        }

        // Test array indexes and substrings together
        [Test]
        public void Substring4() {
            string code = @"
                FUNC array'and'substring$
                    DIM A$(10) OF 10,B$ OF 10
                    FOR X:=1 TO 10
                        A$(X)=""HELLO WORLD""
                        A$(X)(6:6)=CHR$(64+X)
                    NEXT X
                    FOR X:=1 TO 10
                        B$(X:X):=A$(X)(6:6)
                    NEXT X
                    RETURN B$
                ENDFUNC
                ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Helper.HelperRunString(comp, "array'and'substring$", "ABCDEFGHIJ");
        }

        // Test boolean value of an empty string
        [Test]
        public void EmptyStringBoolean() {
            string code = @"
                FUNC emptyString
                    DIM A$ OF 1
                    A$ := """"
                    IF A$ RETURN FALSE
                    A$ := ""A""
                    IF NOT A$ RETURN FALSE
                    RETURN TRUE
                ENDFUNC
                ";
            Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
            Helper.HelperRunString(comp, "emptyString", "1");
        }
    }
}