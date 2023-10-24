// JFortran Compiler
// Unit tests for statement functions
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
    public class StatementFunctions {

        // A simple statement function. Verify that the scope of
        // arguments is constrained to the statement function.
        [Test]
        public void StatementFunction1() {
            string[] code = {
                "      FUNCTION ITEST",
                "        INTEGER B",
                "        B=12",
                "        F(B)=B*B",
                "        RETURN B+F(7)",
                "      END"
            };
            Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
            Helper.HelperRunInteger(comp, "ITEST", 61);
        }

        // A statement function that calls another statement function.
        [Test]
        public void StatementFunction2() {
            string[] code = {
                "      FUNCTION ITEST",
                "        INTEGER B",
                "        B=12",
                "        G(H,I)=(H*I)+H+I",
                "        F(B)=G(B,3)*G(B,4)",
                "        RETURN B+F(7)",
                "      END"
            };
            Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
            Helper.HelperRunInteger(comp, "ITEST", 1221);
        }
    }
}