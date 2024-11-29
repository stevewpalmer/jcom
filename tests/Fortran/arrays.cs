// JFortran Compiler
// Unit tests for arrays
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

using CCompiler;
using JFortran;
using NUnit.Framework;
using TestUtilities;

namespace FortranTests;

[TestFixture]
public class ArrayTests {

    // Verify all array declarations are valid.
    [Test]
    public void ArrayVerifySyntax() {
        string[] code = [
            "      PROGRAM ARRAYVERIFY",
            "      IMPLICIT NONE",
            "      INTEGER A(12), B(90), C(4:9)",
            "      REAL D(2)(4), E(4,5)",
            "      REAL F",
            "      DIMENSION F(23)",
            "      END"
        ];
        FortranHelper.HelperCompile(code, new FortranOptions());
    }

    // Verify basic arrays.
    [Test]
    public void ArrayVerifyArraySyntax() {
        string[] code = [
            "      FUNCTION ITEST",
            "        INTEGER A(2)",
            "        A(1) = 45",
            "        A(2) = 78",
            "        RETURN A(1) + A(2)",
            "      END"
        ];
        Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
        Helper.HelperRunInteger(comp, "ITEST", 123);
    }

    // Verify non 1-based array references.
    [Test]
    public void ArrayVerifyNon1ArraySyntax() {
        string[] code = [
            "      FUNCTION ITEST",
            "        INTEGER A(-3:2), B",
            "        A(-3) = 45",
            "        B = 4",
            "        A(B-2) = 78",
            "        RETURN A(-3) + A(B-2)",
            "      END"
        ];
        Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
        Helper.HelperRunInteger(comp, "ITEST", 123);
    }

    // Verify passing arrays to functions
    [Test]
    public void ArrayVerifyArrayToFunction() {
        string[] code = [
            "      FUNCTION TEST",
            "      PARAMETER (MAXVAL = 10)",
            "      REAL ARRAY(MAXVAL)",
            "      ARRAY(2) = 4.2",
            "      TEST=FOO(ARRAY)",
            "      END",
            "      FUNCTION FOO(R)",
            "      PARAMETER (MAXVAL = 10)",
            "      REAL R(MAXVAL)",
            "      RETURN R(2)",
            "      END"
        ];
        Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
        Helper.HelperRunFloat(comp, "TEST", 4.2f);
    }

    // Verify passing arrays and dimensioning them in the
    // function
    [Test]
    public void ArrayVerifyDynamicArrays() {
        string[] code = [
            "      FUNCTION TEST",
            "      PARAMETER (MAXVAL = 10)",
            "      REAL ARRAY(MAXVAL)",
            "      ARRAY(2) = 4.2",
            "      TEST=FOO(ARRAY,MAXVAL)",
            "      END",
            "      FUNCTION FOO(R,N)",
            "      INTEGER N",
            "      REAL R(N)",
            "      RETURN R(N-8)",
            "      END"
        ];
        Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
        Helper.HelperRunFloat(comp, "TEST", 4.2f);
    }

    // Make sure we error out with dynamic arrays in the main
    // program. Since MAXVAL isn't initialised, this is nonsense but
    // the compiler should catch it.
    [Test]
    public void ArrayVerifyIllegalDeclaration() {
        string[] code = [
            "      PROGRAM FOO",
            "      INTEGER MAXVAL",
            "      REAL ARRAY(MAXVAL)",
            "      ARRAY(2) = 4.2",
            "      TEST=FOO(ARRAY,MAXVAL)",
            "      END"
        ];

        Compiler comp = new(new FortranOptions());
        comp.CompileString(code);
        Assert.AreEqual(1, comp.Messages.ErrorCount);
        Assert.AreEqual(MessageCode.ARRAYILLEGALBOUNDS, comp.Messages[0].Code);
        Assert.AreEqual(3, comp.Messages[0].Line);
    }

    // Modifying the array dimensions passed as a parameter should
    // not modify the actual array dimensions.
    //[Test]
    public void ArrayVerifyDimensions1() {
        string[] code = [
            "      FUNCTION TEST",
            "      REAL ARRAY(2,2)",
            "      ARRAY(2,2) = 4.2",
            "      TEST=FOO(ARRAY,2,2)",
            "      END",
            "      FUNCTION FOO(R,M,N)",
            "      INTEGER M,N",
            "      REAL R(M,N)",
            "      M=0",
            "      RETURN R(2,2)",
            "      END"
        ];
        Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
        Helper.HelperRunFloat(comp, "TEST", 4.2f);
    }

    // Verify assumed sized arrays
    [Test]
    public void ArrayAssumedSizeArrays() {
        string[] code = [
            "      FUNCTION TEST",
            "      REAL ARRAY(2)",
            "      ARRAY(1) = 45",
            "      ARRAY(2) = 73",
            "      TEST=FOO(ARRAY)",
            "      END",
            "      FUNCTION FOO(R)",
            "      REAL R(*)",
            "      RETURN R(1)+R(2)",
            "      END"
        ];
        Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
        Helper.HelperRunFloat(comp, "TEST", 118);
    }
}