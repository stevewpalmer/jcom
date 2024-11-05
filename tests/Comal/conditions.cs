// JComal
// Unit tests for conditions
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
using TestUtilities;

namespace ComalTests;

[TestFixture]
public class Conditions {

  // Test IF true THEN
  [Test]
  public void TestTrueConstant() {
    string code = @"
                FUNC test CLOSED
                  count:=0
                  IF TRUE THEN
                    count:=count+1
                  ELSE
                    count:=0
                  ENDIF
                  IF count<>1 THEN RETURN FALSE
                  RETURN TRUE
                ENDFUNC test
            ";
    Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
    Helper.HelperRunFloat(comp, "test", 1);
  }

  // Test IF and ELSE
  [Test]
  public void TestIf1() {
    string code = @"
                FUNC test CLOSED
                  count:=0
                  flip:=0
                  WHILE count<10 DO
                    IF flip=0 THEN
                      count:=count+2
                      flip:=1
                    ELSE
                      count:=count+1
                      flip:=0
                    ENDIF
                  ENDWHILE
                  RETURN count
                ENDFUNC test
            ";
    Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
    Helper.HelperRunFloat(comp, "test", 11);
  }

  // Test IF, ELSE and ELIF
  [Test]
  public void TestIf3() {
    string code = @"
                FUNC test CLOSED
                  count:=0
                  flip:=0
                  WHILE count<20 DO
                    IF flip=0 THEN
                      count:=count+2
                      flip:=1
                    ELIF flip=1 THEN
                      count:=count+3
                      flip:=2
                    ELSE
                      count:=count+1
                      flip:=0
                    ENDIF
                  ENDWHILE
                  RETURN count
                ENDFUNC test
            ";
    Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
    Helper.HelperRunFloat(comp, "test", 20);
  }

  // Test CASE
  [Test]
  public void TestCase() {
    string code = @"
                FUNC test CLOSED
                  FOR X:=1 to 4 do
                    case x OF
                    WHEN 1
                      IF x<>1 then return false
                    when 2
                      if x<>2 then return false
                    OTHERWISE
                      if x< 3 then return false
                    ENDCASE
                  ENDFOR
                  RETURN TRUE
                ENDFUNC test
            ";
    Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
    Helper.HelperRunFloat(comp, "test", 1);
  }

  // Test CASE with strings
  [Test]
  public void TestCaseStrings() {
    string code = @"
                FUNC test CLOSED
                  DIM A$ OF 3
                  A$:=""FOO""
                  case A$ OF
                  WHEN ""BAR"",""FOO""
                    return true
                  WHEN ""foo"",""FoO""
                    return false
                  ENDCASE
                  return false
                ENDFUNC test
            ";
    Compiler comp = ComalHelper.HelperCompile(code, new ComalOptions());
    Helper.HelperRunFloat(comp, "test", 1);
  }
}