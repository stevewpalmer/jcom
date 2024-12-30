// JFortran Compiler
// Unit tests for the parser
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

namespace FortranTests;

[TestFixture]
public class ParsingTests {

    // Make sure all valid type formats are recognised.
    [Test]
    public void ParseVerifyTypeFormats() {
        string[] code = [
            "      INTEGER A",
            "      REAL B",
            "      CHARACTER C*5,D",
            "      DOUBLE PRECISION E",
            "      LOGICAL F,G,H"
        ];

        Compiler comp = new(new FortranOptions());
        comp.CompileString(code);
        Assert.AreEqual(0, comp.Messages.ErrorCount);
    }

    // Make sure redefinitions are caught.
    [Test]
    public void ParseVerifyRedefinitions() {
        string[] code = [
            "      INTEGER A",
            "      REAL B",
            "      CHARACTER B",
            "      DOUBLE PRECISION B",
            "      LOGICAL F,G,H"
        ];

        Compiler comp = new(new FortranOptions());
        comp.CompileString(code);
        Assert.AreEqual(2, comp.Messages.ErrorCount);
        Assert.AreEqual(MessageCode.IDENTIFIERREDEFINITION, comp.Messages[0].Code);
        Assert.AreEqual(3, comp.Messages[0].Line);
        Assert.AreEqual(MessageCode.IDENTIFIERREDEFINITION, comp.Messages[1].Code);
        Assert.AreEqual(4, comp.Messages[1].Line);
    }

    // Verify type assignment rules
    // 1. Can assign an integer to an integer type
    // 2. Can assign a real to a real type.
    // 3. Can assign an integer to a real type.
    // 4. Can assign a real to an integer type.
    [Test]
    public void ParseVerifyAssignments() {
        string[] code = [
            "      PROGRAM PARSETEST",
            "      INTEGER A,C",
            "      REAL B",
            "      A = 12",
            "      B = 4.67",
            "      B = A",
            "      C = 12.78",
            "      END"
        ];

        Compiler comp = new(new FortranOptions());
        comp.CompileString(code);
        Assert.AreEqual(0, comp.Messages.Count);
    }

    // Verify reject char to integer assignment
    // 1. Cannot assign a character to an integer.
    // 2. Cannot assign an integer to a character
    [Test]
    public void ParseVerifyIllegalChar() {
        string[] code = [
            "      PROGRAM PARSETEST",
            "      INTEGER A",
            "      CHARACTER B",
            "      B = 'A'",
            "      A = B",
            "      B = 89",
            "      END"
        ];

        Compiler comp = new(new FortranOptions());
        comp.CompileString(code);
        Assert.AreEqual(2, comp.Messages.Count);
        Assert.AreEqual(MessageCode.TYPEMISMATCH, comp.Messages[0].Code);
        Assert.AreEqual(5, comp.Messages[0].Line);
        Assert.AreEqual(MessageCode.TYPEMISMATCH, comp.Messages[1].Code);
        Assert.AreEqual(6, comp.Messages[1].Line);
    }

    // Make sure constants can't be assigned-to.
    [Test]
    public void ParseVerifyConstantAssignment() {
        string[] code = [
            "      PROGRAM PARSETEST",
            "      PARAMETER (I=12)",
            "      I = 45",
            "      END"
        ];

        Compiler comp = new(new FortranOptions());
        comp.CompileString(code);
        Assert.AreEqual(1, comp.Messages.Count);
        Assert.AreEqual(MessageCode.CANNOTASSIGNTOCONST, comp.Messages[0].Code);
        Assert.AreEqual(3, comp.Messages[0].Line);
    }

    // Verify divison by zero blows up the code
    // Note that literal division by zero in a constant is a compile-time
    // error and handled separately.
    [Test]
    public void ParseConstantDivisionByZero() {
        string[] code = [
            "      FUNCTION TEST",
            "        INTEGER I",
            "        I = 10/0",
            "        TEST=I",
            "      END"
        ];

        Compiler comp = new(new FortranOptions());
        comp.CompileString(code);
        Assert.AreEqual(1, comp.Messages.Count);
        Assert.AreEqual(MessageCode.DIVISIONBYZERO, comp.Messages[0].Code);
        Assert.AreEqual(3, comp.Messages[0].Line);
    }

    // Verify a general use of IMPLICIT
    [Test]
    public void ParseVerifyImplicit1() {
        string[] code = [
            "      PROGRAM PARSETEST",
            "      IMPLICIT INTEGER(A-Z)",
            "      A = 20",
            "      B = A",
            "      END"
        ];

        Compiler comp = new(new FortranOptions());
        comp.CompileString(code);
        Assert.AreEqual(0, comp.Messages.Count);
    }

    // Verify implicit none reports an undefined variable error
    [Test]
    public void ParseVerifyImplicitNone() {
        string[] code = [
            "      PROGRAM PARSETEST",
            "      IMPLICIT NONE",
            "      INTEGER A",
            "      A = 20",
            "      B = A",
            "      END"
        ];

        Compiler comp = new(new FortranOptions());
        comp.CompileString(code);
        Assert.AreEqual(1, comp.Messages.Count);
        Assert.AreEqual(MessageCode.UNDEFINEDVARIABLE, comp.Messages[0].Code);
        Assert.AreEqual(5, comp.Messages[0].Line);
    }

    // Verify an error is produced if implicit none follows
    // implicit
    [Test]
    public void ParseVerifyImplicitOrdering() {
        string[] code = [
            "      PROGRAM PARSETEST",
            "      IMPLICIT LOGICAL(L)",
            "      IMPLICIT NONE",
            "      END"
        ];

        Compiler comp = new(new FortranOptions());
        comp.CompileString(code);
        Assert.AreEqual(1, comp.Messages.Count);
        Assert.AreEqual(MessageCode.TOKENNOTPERMITTED, comp.Messages[0].Code);
    }

    // Verify logical can only be assigned truth values
    [Test]
    public void ParseVerifyLogicalAssignment() {
        string[] code = [
            "      PROGRAM PARSETEST",
            "      IMPLICIT NONE",
            "      LOGICAL L1,L2",
            "      L1 = .TRUE.",
            "      L2 = .FALSE.",
            "      L1 = L2",
            "      L1 = 90 .GT. 45",
            "      END"
        ];

        Compiler comp = new(new FortranOptions());
        comp.CompileString(code);
        Assert.AreEqual(0, comp.Messages.Count);
    }

    // Verify errors if attempting to assign non-truth values
    // to logical variables.
    [Test]
    public void ParseVerifyBadLogicalAssignment() {
        string[] code = [
            "      PROGRAM PARSETEST",
            "      IMPLICIT NONE",
            "      LOGICAL L1,L2",
            "      L1 = 12",
            "      L2 = 9.67",
            "      L1 = 'Ch'",
            "      END"
        ];

        Compiler comp = new(new FortranOptions());
        comp.CompileString(code);
        Assert.AreEqual(3, comp.Messages.Count);
        Assert.AreEqual(MessageCode.TYPEMISMATCH, comp.Messages[0].Code);
        Assert.AreEqual(MessageCode.TYPEMISMATCH, comp.Messages[1].Code);
        Assert.AreEqual(MessageCode.TYPEMISMATCH, comp.Messages[2].Code);
    }

    // Verify constant expression collapsing.
    [Test]
    public void ParseVerifyExpressionCollapsing() {
        string[] code = [
            "      PROGRAM PARSETEST",
            "      IMPLICIT NONE",
            "      INTEGER A(12+34), B(90-10), C(4-2:9-3)",
            "      END"
        ];

        Compiler comp = new(new FortranOptions());
        comp.CompileString(code);
        Assert.AreEqual(0, comp.Messages.ErrorCount);
    }
}