// JFortran Compiler
// Lexical token classes
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
// under the License.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace JFortran {

    /// <summary>
    /// Lexical tokens.
    /// </summary>
    public enum TokenID {
        COLON, COMMA, DIVIDE, DOUBLE, ENDOFFILE, EOL, EQUOP, EXP, IDENT, INTEGER,
        KASSIGN, KAND, KBACKSPACE, KBLOCK, KBLOCKDATA, KCALL, KCHARACTER, KCLOSE,
        KCOMMON, KCOMPLEX, KCONTINUE, KDATA, KDIMENSION, KDO, KDOUBLE, KDPRECISION,
        KELSE, KELSEIF, KEND, KENDDO, KENDFILE, KENDIF, KENTRY, KEQ, KEQUIVALENCE,
        KEQV, KEXTERNAL, KFALSE, KFORMAT, KFUNCTION, KGE, KGO, KGOTO, KGT, KIF,
        KIMPLICIT, KIMPLICITNONE, KINCLUDE, KINQUIRE, KINTEGER, KINTRINSIC, KLE,
        KLOGICAL, KLT, KNE, KNEQV, KNONE, KNOT, KOPEN, KOR, KPAUSE, KPARAMETER,
        KPRECISION, KPRINT, KPROGRAM, KREAD, KREAL, KRETURN, KREWIND, KSAVE,
        KSTMTFUNC, KSTOP, KSUBROUTINE, KTHEN, KTO, KTRUE, KWHILE, KWRITE, KXOR,
        LPAREN, MINUS, STAR, PLUS, CONCAT, REAL, RPAREN, STRING }

    /// <summary>
    /// Class that represents lexical tokens used by the lexical
    /// analyser.
    /// </summary>
    public static class Tokens {

        // List of reserved Fortran keywords and their token values
        private static readonly Dictionary<string, TokenID> _keywords = new() {
            { "assign",     TokenID.KASSIGN },
            { "backspace",  TokenID.KBACKSPACE },
            { "block",      TokenID.KBLOCK },
            { "character",  TokenID.KCHARACTER },
            { "call",       TokenID.KCALL },
            { "close",      TokenID.KCLOSE },
            { "common",     TokenID.KCOMMON },
            { "complex",    TokenID.KCOMPLEX },
            { "continue",   TokenID.KCONTINUE },
            { "dimension",  TokenID.KDIMENSION },
            { "data",       TokenID.KDATA },
            { "do",         TokenID.KDO },
            { "double",     TokenID.KDOUBLE },
            { "else",       TokenID.KELSE },
            { "elseif",     TokenID.KELSEIF },
            { "end",        TokenID.KEND },
            { "enddo",      TokenID.KENDDO },
            { "endfile",    TokenID.KENDFILE },
            { "endif",      TokenID.KENDIF },
            { "entry",      TokenID.KENTRY },
            { "equivalence",TokenID.KEQUIVALENCE },
            { "external",   TokenID.KEXTERNAL },
            { "format",     TokenID.KFORMAT },
            { "function",   TokenID.KFUNCTION },
            { "go",         TokenID.KGO },
            { "goto",       TokenID.KGOTO },
            { "if",         TokenID.KIF },
            { "implicit",   TokenID.KIMPLICIT },
            { "include",    TokenID.KINCLUDE },
            { "inquire",    TokenID.KINQUIRE },
            { "integer",    TokenID.KINTEGER },
            { "intrinsic",  TokenID.KINTRINSIC },
            { "logical",    TokenID.KLOGICAL },
            { "none",       TokenID.KNONE },
            { "open",       TokenID.KOPEN },
            { "pause",      TokenID.KPAUSE },
            { "parameter",  TokenID.KPARAMETER },
            { "precision",  TokenID.KPRECISION },
            { "print",      TokenID.KPRINT },
            { "program",    TokenID.KPROGRAM },
            { "read",       TokenID.KREAD },
            { "real",       TokenID.KREAL },
            { "return",     TokenID.KRETURN },
            { "rewind",     TokenID.KREWIND },
            { "save",       TokenID.KSAVE },
            { "stop",       TokenID.KSTOP },
            { "subroutine", TokenID.KSUBROUTINE },
            { "then",       TokenID.KTHEN },
            { "to",         TokenID.KTO },
            { "while",      TokenID.KWHILE },
            { "write",      TokenID.KWRITE },
            { ".and.",      TokenID.KAND },
            { ".eq.",       TokenID.KEQ },
            { ".eqv.",      TokenID.KEQV },
            { ".false.",    TokenID.KFALSE },
            { ".ge.",       TokenID.KGE },
            { ".gt.",       TokenID.KGT },
            { ".le.",       TokenID.KLE },
            { ".lt.",       TokenID.KLT },
            { ".or.",       TokenID.KOR },
            { ".ne.",       TokenID.KNE },
            { ".neqv.",     TokenID.KNEQV },
            { ".not.",      TokenID.KNOT },
            { ".true.",     TokenID.KTRUE },
            { ".xor.",      TokenID.KXOR }
        };

        /// <summary>
        /// Map a keyword string to a token.
        /// </summary>
        /// <param name="str">A keyword string</param>
        /// <returns>The associated token ID, or TokenID.IDENT</returns>
        public static TokenID StringToTokenID(string str) {
            if (str == null) {
                throw new ArgumentNullException(nameof(str));
            }
            if (!_keywords.TryGetValue(str.ToLower(), out TokenID id)) {
                id = TokenID.IDENT;
            }
            return id;
        }

        /// <summary>
        /// Map a token to its string.
        /// </summary>
        /// <param name="id">A token ID</param>
        /// <returns>The associated string</returns>
        public static string TokenIDToString(TokenID id) {
            foreach (KeyValuePair<string, TokenID> pair in _keywords) {
                if (id.Equals(pair.Value)) {
                    return pair.Key.ToUpper();
                }
            }
            
            // Anything else here is a non-keyword token
            switch (id) {
                case TokenID.KSTMTFUNC:     return "statement function";
                case TokenID.KDPRECISION:   return "DOUBLE PRECISION";
                case TokenID.KIMPLICITNONE: return "IMPLICIT NONE";
                case TokenID.IDENT:         return "identifier";
                case TokenID.INTEGER:       return "number";
                case TokenID.REAL:          return "real number";
                case TokenID.STRING:        return "string";
                case TokenID.EQUOP:         return "=";
                case TokenID.RPAREN:        return ")";
                case TokenID.LPAREN:        return "(";
                case TokenID.COMMA:         return ",";
                case TokenID.COLON:         return ":";
                case TokenID.STAR:          return "*";
                case TokenID.DIVIDE:        return "/";
                case TokenID.PLUS:          return "+";
                case TokenID.MINUS:         return "-";
                case TokenID.EOL:           return "end of line";
                case TokenID.ENDOFFILE:     return "end of file";
            }
            
            // If we get here, we added a new token but forgot to add it to
            // the switch table above.
            Debug.Assert(false, $"TokenIDToString doesn't understand {id}");
            return "None";
        }
    }

    /// <summary>
    /// Specifies a simple token with no additional data.
    /// </summary>
    public class SimpleToken {

        /// <summary>
        /// Creates a simple token with the given token ID.
        /// </summary>
        /// <param name="id">Token ID</param>
        public SimpleToken(TokenID id) {
            ID = id;
        }

        /// <summary>
        /// Returns the token ID.
        /// </summary>
        public TokenID ID { get; private set; }

        /// <summary>
        /// Returns the keyword ID of the token.
        /// </summary>
        public virtual TokenID KeywordID => ID;

        /// <summary>
        /// Returns the string equivalent of the token.
        /// </summary>
        /// <returns>Token string</returns>
        public override string ToString() {
            return Tokens.TokenIDToString(ID);
        }
    }

    /// <summary>
    /// Specifies a string token with a literal string value.
    /// </summary>
    public class StringToken : SimpleToken {

        /// <summary>
        /// Creates a string token with the specified string.
        /// </summary>
        /// <param name="str">A string value</param>
        public StringToken(string str) : base(TokenID.STRING) {
            String = str;
        }

        /// <summary>
        /// Returns the literal string value of the token.
        /// </summary>
        public string String { get; private set; }
    }

    /// <summary>
    /// Specifies an integer token with a single integer value.
    /// </summary>
    public class IntegerToken : SimpleToken {

        /// <summary>
        /// Creates an integer token with the specified integer value.
        /// </summary>
        /// <param name="value">An integer value</param>
        public IntegerToken(int value) : base(TokenID.INTEGER) {
            Value = value;
        }

        /// <summary>
        /// Returns the integer value of the token.
        /// </summary>
        public int Value { get; private set; }
    }

    /// <summary>
    /// Specifies a floating point (real) token with a single
    /// floating point value.
    /// </summary>
    public class RealToken : SimpleToken {

        /// <summary>
        /// Creates a real token with the given floating point value.
        /// </summary>
        /// <param name="value">A floating point value</param>
        public RealToken(float value) : base(TokenID.REAL) {
            Value = value;
        }

        /// <summary>
        /// Returns the floating point value of the token.
        /// </summary>
        public float Value { get; private set; }
    }

    /// <summary>
    /// Specifies a double (precision) floating point token with
    /// a single double value.
    /// </summary>
    public class DoubleToken : SimpleToken {

        /// <summary>
        /// Creates a double token with the given double value.
        /// </summary>
        /// <param name="value">A double value</param>
        public DoubleToken(double value) : base(TokenID.DOUBLE) {
            Value = value;
        }

        /// <summary>
        /// Returns the double value of the token.
        /// </summary>
        public double Value { get; private set; }
    }

    /// <summary>
    /// Specifies an identifier token with a identifier name that is
    /// valid in the rules of the language.
    /// </summary>
    public class IdentifierToken : SimpleToken {

        /// <summary>
        /// Creates an identifier token with the given name.
        /// </summary>
        /// <param name="name">A identifer name string</param>
        public IdentifierToken(string name) : base(TokenID.IDENT) {
            Name = name;
        }

        /// <summary>
        /// Returns the ID of this identifier. If the name maps to a known
        /// keyword, the keyword ID is returned. Otherwise TokenID.IDENT is
        /// returned.
        /// </summary>
        public override TokenID KeywordID => Tokens.StringToTokenID(Name);

        /// <summary>
        /// Returns the identifier name.
        /// </summary>
        public string Name { get; private set; }
    }
}
