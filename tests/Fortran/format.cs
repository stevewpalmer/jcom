// JFortran Compiler
// Unit tests for FORMAT
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
public class FormatTests {

    // Basic FORMAT verification
    [Test]
    public void FormatBasic() {
        string[] code = [
            "      FUNCTION FMTTEST",
            "        CHARACTER*26 STR, FMTTEST",
            "3       FORMAT (\"The sum of \",I,\" and \",I,\" is \",I)",
            "        WRITE (STR, 3) 12,36,12+36",
            "        FMTTEST = STR",
            "        RETURN",
            "      END"
        ];
        Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
        Helper.HelperRunString(comp, "FMTTEST", "The sum of 12 and 36 is 48");
    }

    // Array FORMAT verification
    [Test]
    public void FormatArrayRef() {
        string[] code = [
            "      FUNCTION FMTTEST",
            "        CHARACTER*18 STR, FMTTEST",
            "        INTEGER V(3)",
            "        V(1) = 12",
            "        V(2) = 24",
            "        V(3) = 36",
            "        WRITE (STR, \"3I6\") V",
            "        FMTTEST = STR",
            "        RETURN",
            "      END"
        ];
        Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
        Helper.HelperRunString(comp, "FMTTEST", "    12    24    36");
    }

    // Exponential format tests.
    [Test]
    public void FormatExpTests() {
        string[] code = [
            "      FUNCTION EXPTEST1",
            "        CHARACTER*7 STR, EXPTEST1",
            "        WRITE (STR, \"E7.2\") 678912.0",
            "        EXPTEST1=STR",
            "      END",
            "      FUNCTION EXPTEST2",
            "        CHARACTER*9 STR, EXPTEST2",
            "        WRITE (STR, \"E9.2\") -678912.0",
            "        EXPTEST2=STR",
            "      END",
            "      FUNCTION EXPTEST3",
            "        CHARACTER*11 STR, EXPTEST3",
            "        WRITE (STR, \"E11.2E4\") -678912.0",
            "        EXPTEST3=STR",
            "      END",
            "      FUNCTION EXPTEST4",
            "        CHARACTER*7 STR, EXPTEST4",
            "        WRITE (STR, \"E7.2\") 8.12D112",
            "        EXPTEST4=STR",
            "      END",
            "      FUNCTION EXPTEST5",
            "        CHARACTER*8 STR, EXPTEST5",
            "        WRITE (STR, \"E8.2\") 8.12D3",
            "        EXPTEST5=STR",
            "      END"
        ];
        Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
        Helper.HelperRunString(comp, "EXPTEST1", ".68E+06");
        Helper.HelperRunString(comp, "EXPTEST2", "-0.68E+06");
        Helper.HelperRunString(comp, "EXPTEST3", "-0.68E+0006");
        Helper.HelperRunString(comp, "EXPTEST4", ".81+113");
        Helper.HelperRunString(comp, "EXPTEST5", "0.81E+04");
    }
}