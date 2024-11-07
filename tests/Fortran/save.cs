// JFortran Compiler
// Unit tests for SAVE statements
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
using TestUtilities;

namespace FortranTests;

[TestFixture]
public class SaveTests {

    // Verify basic SAVE to save the state of a single
    // keyword.
    [Test]
    public void SaveInFunction() {
        string[] code = [
            "      FUNCTION ITEST",
            "        DO 10 I=1,10",
            "          ITEST = ITEST + FOO()",
            " 10     CONTINUE",
            "      END",
            "      FUNCTION FOO()",
            "        INTEGER B",
            "        SAVE B",
            "        B = B + 1",
            "        RETURN B",
            "      END"
        ];
        Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
        Helper.HelperRunInteger(comp, "ITEST", 55);
    }

    // Verify the use of SAVE to save all local identifiers
    [Test]
    public void SaveAllInFunction() {
        string[] code = [
            "      FUNCTION ITEST",
            "        DO 10 I=1,10",
            "          ITEST = ITEST + FOO()",
            " 10     CONTINUE",
            "      END",
            "      FUNCTION FOO()",
            "        INTEGER B,C,D",
            "        SAVE",
            "        B = B + 1",
            "        C = C + 2",
            "        D = D + 4",
            "        RETURN B+C+D",
            "      END"
        ];
        Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
        Helper.HelperRunInteger(comp, "ITEST", 385);
    }
}