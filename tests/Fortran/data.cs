// JFortran Compiler
// Unit tests for DATA statements
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
public class DataTests {

    // Verify basic DATA syntax to initialise a set of
    // variables where there's a 1:1 match.
    [Test]
    public void DataBasicSyntax() {
        string[] code = {
            "      FUNCTION ITEST",
            "        INTEGER A,B,C,D",
            "        DATA A,B,C,D /12,13,14,15/",
            "        RETURN A+B+C+D",
            "      END"
        };
        Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
        Helper.HelperRunInteger(comp, "ITEST", 54);
    }

    // Verify DATA syntax to use a repeat count to set
    // variables to the same value.
    [Test]
    public void DataRepeatCount() {
        string[] code = {
            "      FUNCTION ITEST",
            "        INTEGER A,B,C,D",
            "        DATA A,B,C,D /4*17/",
            "        RETURN A+B+C+D",
            "      END"
        };
        Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
        Helper.HelperRunInteger(comp, "ITEST", 68);
    }

    // Verify DATA syntax to use a repeat count to set
    // variables to the same minus value.
    [Test]
    public void DataRepeatCountWithMinus() {
        string[] code = {
            "      FUNCTION ITEST",
            "        INTEGER A,B,C,D",
            "        DATA A,B,C,D /4*-17/",
            "        RETURN A+B+C+D",
            "      END"
        };
        Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
        Helper.HelperRunInteger(comp, "ITEST", -68);
    }

    // Verify DATA syntax to set an array.
    [Test]
    public void DataArrayNonRepeat() {
        string[] code = {
            "      FUNCTION ITEST",
            "        INTEGER A(4)",
            "        DATA A /34,67,90,-12/",
            "        RETURN A(1)+A(2)+A(3)+A(4)",
            "      END"
        };
        Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
        Helper.HelperRunInteger(comp, "ITEST", 179);
    }

    // Verify DATA syntax to set an array using a repeat.
    [Test]
    public void DataArrayWithRepeat() {
        string[] code = {
            "      FUNCTION ITEST",
            "        INTEGER A(4)",
            "        DATA A /4*65/",
            "        RETURN A(1)+A(2)+A(3)+A(4)",
            "      END"
        };
        Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
        Helper.HelperRunInteger(comp, "ITEST", 260);
    }

    // Verify DATA syntax to set an array using separate repeats.
    [Test]
    public void DataArrayWithSeparateRepeats() {
        string[] code = {
            "      FUNCTION ITEST",
            "        INTEGER A(4)",
            "        DATA A /2*65, 2*12/",
            "        RETURN A(1)+A(2)+A(3)+A(4)",
            "      END"
        };
        Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
        Helper.HelperRunInteger(comp, "ITEST", 154);
    }

    // Verify DATA syntax to set a specific array element. Note that
    // A(3) is omitted.
    [Test]
    public void DataSpecificArrayElements() {
        string[] code = {
            "      FUNCTION ITEST",
            "        INTEGER A(4)",
            "        DATA A(1),A(2),A(4) /34,67,99/",
            "        RETURN A(1)+A(2)+A(4)",
            "      END"
        };
        Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
        Helper.HelperRunInteger(comp, "ITEST", 200);
    }

    // Verify that explicitly setting a DATA element overrides the DATA
    // set even if it precedes DATA.
    [Test]
    public void DataSpecificArrayElementsWithOverride() {
        string[] code = {
            "      FUNCTION ITEST",
            "        INTEGER A(4)",
            "        A(1)=450",
            "        DATA A(1),A(2),A(4) /19,34,99/",
            "        RETURN A(1)+A(2)+A(3)+A(4)",
            "      END"
        };
        Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
        Helper.HelperRunInteger(comp, "ITEST", 583);
    }

    // For arrays with indexes in DATA, repeat applies to the individual
    // elements.
    [Test]
    public void DataSpecificArrayElementsWithRepeat() {
        string[] code = {
            "      FUNCTION ITEST",
            "        INTEGER A(4)",
            "        DATA A(1),A(2),A(4) /3*77/",
            "        RETURN A(1)+A(2)+A(3)+A(4)",
            "      END"
        };
        Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
        Helper.HelperRunInteger(comp, "ITEST", 231);
    }

    // DATA statement with a simple implied DO loop and a repeat
    // for the values.
    [Test]
    public void DataImpliedDo1() {
        string[] code = {
            "      FUNCTION ITEST",
            "        INTEGER ODD(10)",
            "        DATA (ODD(I),I=1,10,2)/ 5 * 43/",
            "        DATA (ODD(I),I=2,10,2)/ 5 * 0 /",
            "        RETURN SUM(ODD)",
            "      END",
            "      INTEGER FUNCTION SUM(JRY)",
            "        INTEGER JRY(10)",
            "        SUM = 0",
            "        DO 100,I=1,10",
            "           SUM=SUM+JRY(I)",
            "100     CONTINUE",
            "      END"
        };
        Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
        Helper.HelperRunInteger(comp, "ITEST", 215);
    }

    // DATA statement with a simple implied DO loop and a series of
    // discrete values.
    [Test]
    public void DataImpliedDo2() {
        string[] code = {
            "      FUNCTION ITEST",
            "        INTEGER NUMBER(10)",
            "        DATA (NUMBER(I),I=1,10)/1,1,2,3,5,8,13,21,34,55/",
            "        RETURN SUM(NUMBER)",
            "      END",
            "      INTEGER FUNCTION SUM(JRY)",
            "        INTEGER JRY(10)",
            "        SUM = 0",
            "        DO 100,I=1,10",
            "           SUM=SUM+JRY(I)",
            "100     CONTINUE",
            "      END"
        };
        Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
        Helper.HelperRunInteger(comp, "ITEST", 143);
    }

    // Test initialisation of a CHARACTER with DATA
    [Test]
    public void DataCharacterSet() {
        string[] code = {
            "      FUNCTION ITEST",
            "        CHARACTER * 20 B, ITEST",
            "        DATA B /20*'A'/",
            "        RETURN B",
            "      END"
        };
        Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
        Helper.HelperRunString(comp, "ITEST", "AAAAAAAAAAAAAAAAAAAA");
    }

    // DATA statement with a nested implied DO loop
    // Currently broken in the compiler.
    /*
    [Test]
    public void DataImpliedDo3() {
        string [] code = {
            "      FUNCTION ITEST",
            "        INTEGER NUMBER(3,3)",
            "        DATA ((NUMBER(I,J),I=1,3),J=1,3)/1,2,3,5,8,13,21,34,55/",
            "        RETURN SUM(NUMBER)",
            "      END",
            "      INTEGER FUNCTION SUM(JRY)",
            "        INTEGER JRY(9)",
            "        SUM = 0",
            "        DO 100,I=1,9",
            "           SUM=SUM+JRY(I)",
            "100     CONTINUE",
            "      END"
        };
        Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
        Utilities.Helper.HelperRunInteger(comp, "ITEST", 98);
    }*/
}