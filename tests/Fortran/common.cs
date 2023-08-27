// JFortran Compiler
// Unit tests for COMMON
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

    public class CommonTests {

        // Basic COMMON test involving an unnamed and named COMMON block
        // and two subroutines that modify the global values.
        [Test]
        public void BasicCommonTest() {
            string [] code = {
                "      PROGRAM COMMONTEST",
                "      COMMON A,B",
                "      COMMON /FOO/ C,D",
                "      END",
                "      FUNCTION ITEST",
                "      COMMON // X,Y /FOO/ W,Z",
                "      DATA X,Y,W,Z /12.0,78.0,-3,100/",
                "      CALL FOOBAR1",
                "      CALL FOOBAR2",
                "      ITEST=X+Y+W+Z",
                "      RETURN",
                "      END",
                "      SUBROUTINE FOOBAR1",
                "      COMMON A1,B1",
                "      A1=10",
                "      B1=20",
                "      END",
                "      SUBROUTINE FOOBAR2",
                "      REAL J",
                "      COMMON /FOO/ J,B1",
                "      J=10",
                "      B1=20",
                "      END"
            };
            Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
            Helper.HelperRunInteger(comp, "ITEST", 60);
        }

        // COMMON test where the elements of a COMMON statement are built up
        // over several statements. Ensure that the corresponding COMMON statement
        // in the sub-programs match up.
        [Test]
        public void MultipartCommonTest() {
            string [] code = {
                "      FUNCTION ICOMMONTEST",
                "      COMMON A,B",
                "      COMMON // J,K",
                "      DATA A,B,J,K /12.0,78.0,-3,100/",
                "      RETURN ITEST()",
                "      END",
                "      FUNCTION ITEST",
                "      COMMON // X,Y,I,L",
                "      ITEST=X+Y+I+L",
                "      RETURN",
                "      END"
            };
            Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
            Helper.HelperRunInteger(comp, "ICOMMONTEST", 187);
        }

        // COMMON test where the elements of a COMMON statement are built up
        // over several statements. Ensure that the corresponding COMMON statement
        // in the sub-programs match up.
        [Test]
        public void SplitMultipartCommonTest() {
            string [] code = {
                "      FUNCTION ICOMMONTEST",
                "      COMMON A,B",
                "      COMMON /FOO/ J",
                "      COMMON // K,L",
                "      DATA A,B,J,K,L /12.0,78.0,-3,100,10/",
                "      RETURN ITEST()",
                "      END",
                "      FUNCTION ITEST",
                "      COMMON // X,Y /FOO/ M // I",
                "      COMMON J2",
                "      ITEST=X+Y+M+I+J2",
                "      RETURN",
                "      END"
            };
            Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
            Helper.HelperRunInteger(comp, "ICOMMONTEST", 197);
        }
    }
}
