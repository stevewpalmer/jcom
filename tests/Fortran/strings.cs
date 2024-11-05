// JFortran Compiler
// Unit tests for strings
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

using JFortran;
using NUnit.Framework;
using TestUtilities;

namespace FortranTests;

[TestFixture]
public class Strings {

    // Test substring manipulation
    [Test]
    public void SubstringSet() {
        string[] code = {
            "      FUNCTION ITEST",
            "        CHARACTER METAL(2)*10, ITEST",
            "        METAL(1) = 'CADMIUM'",
            "        METAL(1)(3:4) = 'LO'",
            "        RETURN METAL(1)",
            "      END"
        };
        Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
        Helper.HelperRunString(comp, "ITEST", "CALOIUM");
    }
}