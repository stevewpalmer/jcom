// JFortran Compiler
// Unit tests for lexical analysis
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

using System;
using CCompiler;
using JFortran;
using NUnit.Framework;

namespace FortranTests {
    [TestFixture]
    
    public class LexicalTests {

        // Verify that a label is correctly parsed from one line
        // and that it is not present on the second line.
        [Test]
        public void ValidateHasLabel() {
            string [] code = {
                "100   INTEGER A",
                "      STOP"
            };
            FortranOptions opts = new();
            MessageCollection messages = new(opts);
            Lexer ls = new(code, opts, messages);
            Assert.IsTrue(ls.GetKeyword().ID == TokenID.KINTEGER);
            Assert.IsTrue(ls.HasLabel);
            Assert.IsTrue(ls.Label == "100");
            Assert.IsTrue(ls.GetToken().ID == TokenID.IDENT);
            Assert.IsTrue(messages.ErrorCount == 0);
        }

        // Validate continuation character support
        [Test]
        public void ValidateContinuationCharacter() {
            string [] code = {
                "100   INTEGER STA",
                "     1TION"
            };
            FortranOptions opts = new();
            MessageCollection messages = new(opts);
            Lexer ls = new(code, opts, messages);
            Assert.IsTrue(ls.GetKeyword().ID == TokenID.KINTEGER);

            SimpleToken token = ls.GetToken();
            Assert.IsTrue(token.ID == TokenID.IDENT);

            IdentifierToken identToken = (IdentifierToken)token;
            Assert.AreEqual(identToken.Name, "STATION");
        }

        // Validate that digit 0 specifies a new line.
        [Test]
        public void ValidateDigit0ContinuationCharacter() {
            string [] code = {
                "100   INTEGER BAR",
                "     0INTEGER FOO"
            };
            FortranOptions opts = new();
            MessageCollection messages = new(opts);
            Lexer ls = new(code, opts, messages);
            Assert.IsTrue(ls.GetKeyword().ID == TokenID.KINTEGER);

            SimpleToken token = ls.GetToken();
            Assert.IsTrue(token.ID == TokenID.IDENT);

            IdentifierToken identToken = (IdentifierToken)token;
            Assert.AreEqual(identToken.Name, "BAR");

            Assert.IsTrue(ls.GetToken().ID == TokenID.EOL);
            Assert.IsTrue(ls.GetKeyword().ID == TokenID.KINTEGER);
        }

        // Validate Fortran 77 extension continuation character support
        [Test]
        public void ValidateF77ExtContinuationCharacter() {
            string [] code = {
                "100   INTEGER STA&",
                "      TION"
            };
            FortranOptions opts = new();
            MessageCollection messages = new(opts);
            Lexer ls = new(code, opts, messages);
            Assert.IsTrue(ls.GetKeyword().ID == TokenID.KINTEGER);
            
            SimpleToken token = ls.GetToken();
            Assert.IsTrue(token.ID == TokenID.IDENT);
            
            IdentifierToken identToken = (IdentifierToken)token;
            Assert.AreEqual(identToken.Name, "STATION");
        }

        // Verify we maximum 19 continuation lines allowed
        [Test]
        public void ValidateMaximumContinuationLines() {
            string [] code = {
                "      INTEGER A,",
                "     1B,",
                "     2C,",
                "     3D,",
                "     4E,",
                "     5F,",
                "     6G,",
                "     7H,",
                "     8I,",
                "     9J,",
                "     AK,",
                "     BL,",
                "     CM,",
                "     DN,",
                "     EO,",
                "     FP,",
                "     GQ,",
                "     HR,",
                "     IS,",
                "     JT"
            };

            Compiler comp = new(new FortranOptions());
            comp.CompileString(code);
            Assert.AreEqual(0, comp.Messages.ErrorCount);
        }

        // Verify we catch > 19 continuation lines and report an error
        // for the maximum line.
        [Test]
        public void ValidateExceedMaximumContinuationLines() {
            string [] code = {
                "      INTEGER A,",
                "     1B,",
                "     2C,",
                "     3D,",
                "     4E,",
                "     5F,",
                "     6G,",
                "     7H,",
                "     8I,",
                "     9J,",
                "     AK,",
                "     BL,",
                "     CM,",
                "     DN,",
                "     EO,",
                "     FP,",
                "     GQ,",
                "     HR,",
                "     IS,",
                "     JT",
                "     KU"
            };

            Compiler comp = new(new FortranOptions());
            comp.CompileString(code);
            Assert.AreEqual(1, comp.Messages.ErrorCount);
            Assert.AreEqual(21, comp.Messages[0].Line);
            Assert.AreEqual(MessageCode.TOOMANYCONTINUATION, comp.Messages[0].Code);
        }

        // Validate tab support
        [Test]
        public void ValidateTabDelimiter() {
            string [] code = {
                "100\tINTEGER A",
                "\tREAL B"
            };
            FortranOptions opts = new();
            MessageCollection messages = new(opts);
            Lexer ls = new(code, opts, messages);
            Assert.IsTrue(ls.GetKeyword().ID == TokenID.KINTEGER);
            Assert.IsTrue(ls.HasLabel);
            Assert.IsTrue(ls.Label == "100");

            SimpleToken token = ls.GetToken();
            Assert.IsTrue(token.ID == TokenID.IDENT);
            
            IdentifierToken identToken = (IdentifierToken)token;
            Assert.AreEqual(identToken.Name, "A");

            Assert.IsTrue(ls.GetToken().ID == TokenID.EOL);
            Assert.IsTrue(ls.GetKeyword().ID == TokenID.KREAL);
            
            token = ls.GetToken();
            Assert.IsTrue(token.ID == TokenID.IDENT);
            
            identToken = (IdentifierToken)token;
            Assert.AreEqual(identToken.Name, "B");
        }

        // Validate debug lines support
        [Test]
        public void ValidateDebug() {
            string [] code = {
                "      INTEGER A",
                "D     REAL B",
                "      INTEGER C"
            };
            FortranOptions opts = new();
            MessageCollection messages = new(opts);
            Lexer ls = new(code, opts, messages);
            Assert.IsTrue(ls.GetKeyword().ID == TokenID.KINTEGER);
            Assert.IsTrue(ls.GetToken().ID == TokenID.IDENT);
            Assert.IsTrue(ls.GetToken().ID == TokenID.EOL);
            Assert.IsTrue(ls.GetKeyword().ID == TokenID.KINTEGER);

            // Now it should pick up 'D' lines.
            opts.GenerateDebug = true;
            ls = new Lexer(code, opts, messages);
            Assert.IsTrue(ls.GetKeyword().ID == TokenID.KINTEGER);
            Assert.IsTrue(ls.GetToken().ID == TokenID.IDENT);
            Assert.IsTrue(ls.GetToken().ID == TokenID.EOL);
            Assert.IsTrue(ls.GetKeyword().ID == TokenID.KREAL);
        }

        // Validate integer parsing
        [Test]
        public void ValidateIntegerParsing() {
            string [] code = {
                "      1230976AA"
            };
            FortranOptions opts = new();
            MessageCollection messages = new(opts);
            Lexer ls = new(code, opts, messages);
            SimpleToken token = ls.GetToken();
            Assert.IsTrue(token.ID == TokenID.INTEGER);
            
            IntegerToken intToken = (IntegerToken)token;
            Assert.AreEqual(intToken.Value, 1230976);
        }

        // Validate real number parsing
        [Test]
        public void ValidateRealParsing() {
            string [] code = {
                "      123.0976AA",
                "      123.0976E-2",
                "      .5"
            };
            FortranOptions opts = new();
            MessageCollection messages = new(opts);
            Lexer ls = new(code, opts, messages);

            SimpleToken token = ls.GetToken();
            Assert.IsTrue(token.ID == TokenID.REAL);
            RealToken realToken = (RealToken)token;
            Assert.IsTrue(Math.Abs(realToken.Value - 123.0976f) < float.Epsilon, "Expected 123.0976 but saw " + realToken.Value);

            Assert.IsTrue(ls.GetToken().ID == TokenID.IDENT);
            Assert.IsTrue(ls.GetToken().ID == TokenID.EOL);

            token = ls.GetToken();
            Assert.IsTrue(token.ID == TokenID.REAL);
            realToken = (RealToken)token;
            Assert.IsTrue(Math.Abs(realToken.Value - 123.0976E-2f) < float.Epsilon, "Expected 123.0976E-2 but saw " + realToken.Value);

            Assert.IsTrue(ls.GetToken().ID == TokenID.EOL);

            token = ls.GetToken();
            Assert.IsTrue(token.ID == TokenID.REAL);
            realToken = (RealToken)token;
            Assert.IsTrue(Math.Abs(realToken.Value - 0.5f) < float.Epsilon, "Expected 0.5 but saw " + realToken.Value);
        }

        // Validate double precision number parsing
        [Test]
        public void ValidateDoublePrecisionParsing() {
            string [] code = {
                "      123.0976D4AA",
                "      123.0976D-2"
            };
            FortranOptions opts = new();
            MessageCollection messages = new(opts);
            Lexer ls = new(code, opts, messages);

            SimpleToken token = ls.GetToken();
            Assert.IsTrue(token.ID == TokenID.DOUBLE);
            DoubleToken realToken = (DoubleToken)token;
            Assert.IsTrue(Utilities.Helper.DoubleCompare(realToken.Value, 123.0976E4), "Expected 123.0976E4 but saw " + realToken.Value);
            
            Assert.IsTrue(ls.GetToken().ID == TokenID.IDENT);
            Assert.IsTrue(ls.GetToken().ID == TokenID.EOL);
            
            token = ls.GetToken();
            Assert.IsTrue(token.ID == TokenID.DOUBLE);
            realToken = (DoubleToken)token;
            Assert.IsTrue(Utilities.Helper.DoubleCompare(realToken.Value, 123.0976E-2), "Expected 123.0976E-2 but saw " + realToken.Value);
        }

        // Validate hexadecimal number parsing
        [Test]
        public void ValidateHexParsing() {
            string [] code = {
                "      $CC4DE",
                "      Z'CC4DE'",
                "      Z\"CC4DE\"",
                "      $AGG"
            };
            FortranOptions opts = new();
            MessageCollection messages = new(opts);
            Lexer ls = new(code, opts, messages);

            SimpleToken token = ls.GetToken();
            Assert.IsTrue(token.ID == TokenID.INTEGER);
            IntegerToken intToken = (IntegerToken)token;
            Assert.AreEqual(intToken.Value, 0xCC4DE);

            Assert.IsTrue(ls.GetToken().ID == TokenID.EOL);
            
            token = ls.GetToken();
            Assert.IsTrue(token.ID == TokenID.INTEGER);
            intToken = (IntegerToken)token;
            Assert.AreEqual(intToken.Value, 0xCC4DE);

            Assert.IsTrue(ls.GetToken().ID == TokenID.EOL);
            
            token = ls.GetToken();
            Assert.IsTrue(token.ID == TokenID.INTEGER);
            intToken = (IntegerToken)token;
            Assert.AreEqual(intToken.Value, 0xCC4DE);

            Assert.IsTrue(ls.GetToken().ID == TokenID.EOL);
            
            token = ls.GetToken();
            Assert.IsTrue(token.ID == TokenID.INTEGER);
            intToken = (IntegerToken)token;
            Assert.AreEqual(intToken.Value, 0xA);

            Assert.IsTrue(ls.GetToken().ID == TokenID.IDENT);
        }


        // Validate octal number parsing
        [Test]
        public void ValidateOctalParsing() {
            string [] code = {
                "      O'745'",
                "      O\"340\"",
                "      O'892'"
            };
            FortranOptions opts = new();
            MessageCollection messages = new(opts);
            Lexer ls = new(code, opts, messages);

            SimpleToken token = ls.GetToken();
            Assert.IsTrue(token.ID == TokenID.INTEGER);
            IntegerToken intToken = (IntegerToken)token;
            Assert.AreEqual(intToken.Value, 485);

            Assert.IsTrue(ls.GetToken().ID == TokenID.EOL);
            
            token = ls.GetToken();
            Assert.IsTrue(token.ID == TokenID.INTEGER);
            intToken = (IntegerToken)token;
            Assert.AreEqual(intToken.Value, 224);

            Assert.IsTrue(ls.GetToken().ID == TokenID.EOL);
            
            ls.GetToken();
            Assert.IsTrue(messages.ErrorCount > 0);
            Assert.IsTrue(messages[0].Line == 3);
            Assert.IsTrue(messages[0].Code == MessageCode.BADNUMBERFORMAT);
        }

        // Validate binary number parsing
        [Test]
        public void ValidateBinaryParsing() {
            string [] code = {
                "      B'1010101111'",
                "      b\"111\"",
                "      B'121'"
            };
            FortranOptions opts = new();
            MessageCollection messages = new(opts);
            Lexer ls = new(code, opts, messages);

            SimpleToken token = ls.GetToken();
            Assert.IsTrue(token.ID == TokenID.INTEGER);
            IntegerToken intToken = (IntegerToken)token;
            Assert.AreEqual(intToken.Value, 687);

            Assert.IsTrue(ls.GetToken().ID == TokenID.EOL);
            
            token = ls.GetToken();
            Assert.IsTrue(token.ID == TokenID.INTEGER);
            intToken = (IntegerToken)token;
            Assert.AreEqual(intToken.Value, 7);

            Assert.IsTrue(ls.GetToken().ID == TokenID.EOL);
            
            ls.GetToken();
            Assert.IsTrue(messages.ErrorCount > 0);
            Assert.IsTrue(messages[0].Line == 3);
            Assert.IsTrue(messages[0].Code == MessageCode.BADNUMBERFORMAT);
        }

        // Validate string parsing
        [Test]
        public void ValidateStringParsing() {
            string [] code = {
                "      PRINT \"AbCDEf\""
            };
            FortranOptions opts = new();
            MessageCollection messages = new(opts);
            Lexer ls = new(code, opts, messages);
            Assert.IsTrue(ls.GetKeyword().ID == TokenID.KPRINT);

            SimpleToken token = ls.GetToken();
            Assert.IsTrue(token.ID == TokenID.STRING);

            StringToken stringToken = (StringToken)token;
            Assert.AreEqual(stringToken.String, "AbCDEf");
        }

        // Validate backslash handling
        [Test]
        public void ValidateBackslashStringParsing() {
            string [] code = {
                "      \"Ab\\tCDEf\\n\""
            };
            FortranOptions opts = new();
            MessageCollection messages = new(opts);
            Lexer ls = new(code, opts, messages);
            SimpleToken token = ls.GetToken();
            Assert.IsTrue(token.ID == TokenID.STRING);
            
            StringToken stringToken = (StringToken)token;
            Assert.AreEqual(stringToken.String, "Ab\tCDEf\n");

            // Turn on the option to treat backslash character as
            // a backslash and not a special character.
            opts.Backslash = true;
            ls = new Lexer(code, opts, messages);
            token = ls.GetToken();
            Assert.IsTrue(token.ID == TokenID.STRING);
            
            stringToken = (StringToken)token;
            Assert.AreEqual(stringToken.String, "Ab\\tCDEf\\n");
        }
    }
}

