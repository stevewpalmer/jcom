// JCalcLib
// A cell parse tree
//
// Authors:
//  Steve Palmer
//
// Copyright (C) 2024 Steve Palmer
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

using System.Diagnostics;
using System.Text;
using JComLib;

namespace JCalcLib;

/// <summary>
/// List of cell parse node tokens
/// </summary>
public enum TokenID {
    ADDRESS,
    NUMBER,
    MULTIPLY,
    DIVIDE,
    LPAREN,
    RPAREN,
    KEQ,
    MINUS,
    PLUS,
    KNE,
    KLE,
    KLT,
    KGE,
    KGT,
    EXP,
    EOL,
    TEXT
}

/// <summary>
/// Basic cell parse node
/// </summary>
/// <param name="tokenID">Node token ID</param>
public class CellParseNode(TokenID tokenID) {

    /// <summary>
    /// Operator
    /// </summary>
    public TokenID Op { get; } = tokenID;
}

/// <summary>
/// Represents a parse node that holds a binary operation.
/// </summary>
/// <param name="tokenID">Node token ID</param>
/// <param name="left">Left part of expression</param>
/// <param name="right">Right part of expression</param>
public class BinaryOpParseNode(TokenID tokenID, CellParseNode left, CellParseNode right) : CellParseNode(tokenID) {

    /// <summary>
    /// Left child node
    /// </summary>
    public CellParseNode Left { get; } = left;

    /// <summary>
    /// Right child node
    /// </summary>
    public CellParseNode Right { get; } = right;
}

/// <summary>
/// Represents a parse node that holds a numeric value.
/// </summary>
/// <param name="value">Double value</param>
public class NumberParseNode(double value) : CellParseNode(TokenID.NUMBER) {

    /// <summary>
    /// Value of node
    /// </summary>
    public Variant Value { get; } = new(value);
}

/// <summary>
/// Represents a parse node that holds a string value.
/// </summary>
/// <param name="tokenID">Node token ID</param>
/// <param name="value">String value</param>
public class TextParseNode(TokenID tokenID, string value) : CellParseNode(tokenID) {

    /// <summary>
    /// Value of node
    /// </summary>
    public string Value { get; } = value;
}

/// <summary>
/// Implements a cell formula parser to construct the associated
/// parse tree from the expression.
/// </summary>
public class FormulaParser {

    private const char EOL = '\n';
    private readonly string _line;
    private readonly List<SimpleToken> tokens = [];
    private int _index;
    private char _pushedChar;
    private SimpleToken? _pushedToken;
    private int _tindex;

    /// <summary>
    /// Initialise a formula parser with the specified input and create
    /// a token queue.
    /// </summary>
    /// <param name="line">Formula to be parsed</param>
    /// <exception cref="Exception">Errors found in the formula expression</exception>
    public FormulaParser(string line) {
        _line = line.Trim();
        _index = 0;
        _tindex = 0;
        _pushedToken = null;

        bool endOfLine = false;
        do {
            char ch = GetChar();
            switch (ch) {
                case >= '0' and <= '9':
                    PushChar(ch);
                    tokens.Add(ParseNumber());
                    break;

                case '.':
                    if (char.IsDigit(PeekChar())) {
                        PushChar(ch);
                        tokens.Add(ParseNumber());
                    }
                    break;

                case >= 'A' and <= 'Z' or >= 'a' and <= 'z': {
                    StringBuilder str = new();
                    while (char.IsLetter(ch)) {
                        str.Append(ch);
                        ch = GetChar();
                    }
                    while (char.IsDigit(ch)) {
                        str.Append(ch);
                        ch = GetChar();
                    }
                    PushChar(ch);
                    if (str.Length > 6) {
                        throw new FormatException("Invalid formula.");
                    }
                    tokens.Add(new CellAddressToken(str.ToString()));
                    break;
                }

                case ' ':
                    ch = GetChar();
                    while (ch == ' ') {
                        ch = GetChar();
                    }
                    PushChar(ch);
                    break;

                case '(': tokens.Add(new SimpleToken(TokenID.LPAREN)); break;
                case ')': tokens.Add(new SimpleToken(TokenID.RPAREN)); break;
                case '=': tokens.Add(new SimpleToken(TokenID.KEQ)); break;
                case '-': tokens.Add(new SimpleToken(TokenID.MINUS)); break;
                case '+': tokens.Add(new SimpleToken(TokenID.PLUS)); break;
                case '*': tokens.Add(new SimpleToken(TokenID.MULTIPLY)); break;
                case '/': tokens.Add(new SimpleToken(TokenID.DIVIDE)); break;
                case '^': tokens.Add(new SimpleToken(TokenID.EXP)); break;

                case '<':
                    ch = GetChar();
                    if (ch == '>') {
                        tokens.Add(new SimpleToken(TokenID.KNE));
                        break;
                    }
                    if (ch == '=') {
                        tokens.Add(new SimpleToken(TokenID.KLE));
                        break;
                    }
                    PushChar(ch);
                    tokens.Add(new SimpleToken(TokenID.KLT));
                    break;

                case '>':
                    ch = GetChar();
                    if (ch == '=') {
                        tokens.Add(new SimpleToken(TokenID.KGE));
                        break;
                    }
                    PushChar(ch);
                    tokens.Add(new SimpleToken(TokenID.KGT));
                    break;

                case EOL:
                    tokens.Add(new SimpleToken(TokenID.EOL));
                    endOfLine = true;
                    break;

                default:
                    throw new FormatException("Invalid formula.");
            }
        } while (!endOfLine);
    }

    /// <summary>
    /// Parse the formula and return the root of the parse node that
    /// represents the formula.
    /// </summary>
    /// <returns>A CellParseNode</returns>
    public CellParseNode Parse() => ParseExpression(0);

    /// <summary>
    /// Take a peep at the next non-space character in the input stream
    /// </summary>
    /// <returns>The next character</returns>
    private char PeekChar() {
        int tempIndex = _index;
        if (tempIndex >= 0) {
            while (tempIndex < _line.Length) {
                char ch = _line[tempIndex];
                if (ch != ' ' && ch != '\t') {
                    return ch;
                }
                ++tempIndex;
            }
        }
        return EOL;
    }

    /// <summary>
    /// Parse a decimal number
    /// </summary>
    /// <returns>Token representing the number</returns>
    private NumberToken ParseNumber() {
        StringBuilder str = new();

        char ch = GetChar();
        while (char.IsDigit(ch)) {
            str.Append(ch);
            ch = GetChar();
        }
        if (ch == '.') {
            str.Append(ch);
            ch = GetChar();
            while (char.IsDigit(ch)) {
                str.Append(ch);
                ch = GetChar();
            }
        }
        if (char.ToUpper(ch) == 'E' && !char.IsLetter(PeekChar())) {
            str.Append('E');
            ch = GetChar();
            if (ch is '+' or '-') {
                str.Append(ch);
                ch = GetChar();
            }
            while (char.IsDigit(ch)) {
                str.Append(ch);
                ch = GetChar();
            }
        }
        int factor = 1;
        if (ch == '%') {
            factor = 100;
        }
        else {
            PushChar(ch);
        }
        if (double.TryParse(str.ToString(), out double result)) {
            return new NumberToken(result / factor);
        }
        throw new FormatException("Invalid number.");
    }

    // Check whether we're at the end of the line. A '&' means we continue
    // onto the next line.
    private bool AtLineEnd() {
        return _index == _line.Length;
    }

    /// Push back the last character read so that the next call to GetChar will
    /// retrieve that character.
    private void PushChar(char ch) {
        Debug.Assert(_pushedChar == '\0');
        _pushedChar = ch;
    }

    /// Read the next character from the stream. If we reach the end of the line check
    /// the next one for a continuation character. If one is found, consume
    /// the new line and return the next character. Otherwise return EOL.
    private char GetChar() {
        if (_pushedChar != '\0') {
            char ch = _pushedChar;
            _pushedChar = '\0';
            return ch;
        }
        return AtLineEnd() ? EOL : _line[_index++];
    }

    /// <summary>
    /// Parse the next token from the formula string
    /// </summary>
    /// <returns></returns>
    private SimpleToken GetNextToken() {
        SimpleToken token;
        if (_pushedToken != null) {
            token = _pushedToken;
            _pushedToken = null;
        }
        else {
            Debug.Assert(_tindex < tokens.Count);
            token = tokens[_tindex++];
        }
        return token;
    }

    /// <summary>
    /// Move the token index back one token in the queue.
    /// </summary>
    private void PushToken(SimpleToken token) {
        _pushedToken = token;
    }

    /// <summary>
    /// Ensure that the next token in the input is the one expected and report an error otherwise.
    /// </summary>
    /// <param name="expectedID">Expected token</param>
    private void ExpectToken(TokenID expectedID) {
        SimpleToken token = GetNextToken();
        if (token.ID != expectedID) {
            throw new FormatException("Invalid token.");
        }
    }

    /// <summary>
    /// Parse an expression.
    /// </summary>
    /// <param name="level">Current precedence level</param>
    /// <returns>CellParseNode</returns>
    private CellParseNode ParseExpression(int level) {
        CellParseNode op1 = Operand();
        bool done = false;

        while (!done) {
            SimpleToken token = GetNextToken();
            int preced;
            switch (token.ID) {
                case TokenID.KGT:
                    preced = 5;
                    break;
                case TokenID.KGE:
                    preced = 5;
                    break;
                case TokenID.KLE:
                    preced = 5;
                    break;
                case TokenID.KNE:
                    preced = 5;
                    break;
                case TokenID.KEQ:
                    preced = 5;
                    break;
                case TokenID.KLT:
                    preced = 5;
                    break;
                case TokenID.PLUS:
                    preced = 6;
                    break;
                case TokenID.MINUS:
                    preced = 6;
                    break;
                case TokenID.MULTIPLY:
                    preced = 7;
                    break;
                case TokenID.DIVIDE:
                    preced = 7;
                    break;
                case TokenID.EXP:
                    preced = 10;
                    break;
                default:
                    PushToken(token);
                    done = true;
                    continue;
            }
            if (level >= preced) {
                PushToken(token);
                done = true;
            }
            else {
                op1 = new BinaryOpParseNode(token.ID, op1, ParseExpression(preced));
            }
        }
        return op1;
    }

    /// <summary>
    /// Parse a single operand
    /// </summary>
    /// <returns>A CellParseNode</returns>
    private CellParseNode Operand() {
        SimpleToken token = GetNextToken();
        switch (token.ID) {
            case TokenID.LPAREN: {
                CellParseNode node = ParseExpression(0);
                ExpectToken(TokenID.RPAREN);
                return node;
            }

            case TokenID.PLUS:
                return Operand();

            case TokenID.MINUS:
                return new BinaryOpParseNode(TokenID.MINUS, new NumberParseNode(0), ParseExpression(9));

            case TokenID.NUMBER: {
                NumberToken numberToken = (NumberToken)token;
                return new NumberParseNode(numberToken.Value);
            }

            case TokenID.ADDRESS: {
                CellAddressToken identToken = (CellAddressToken)token;
                return new TextParseNode(TokenID.ADDRESS, identToken.Address);
            }
        }
        throw new FormatException("Invalid operand.");
    }

    private class SimpleToken {

        /// <summary>
        /// Creates a simple token of the specified ID
        /// </summary>
        /// <param name="id">Token ID</param>
        internal SimpleToken(TokenID id) {
            ID = id;
        }

        /// <summary>
        /// Token ID
        /// </summary>
        public TokenID ID { get; }
    }

    private class CellAddressToken : SimpleToken {

        /// <summary>
        /// Creates a cell address token with the given address.
        /// </summary>
        /// <param name="address">A cell address</param>
        public CellAddressToken(string address) : base(TokenID.ADDRESS) {
            Address = address;
        }

        /// <summary>
        /// Returns the identifier name.
        /// </summary>
        public string Address { get; }
    }

    private class NumberToken : SimpleToken {

        /// <summary>
        /// Creates a number token with the given value.
        /// </summary>
        /// <param name="value">The numeric value</param>
        public NumberToken(double value) : base(TokenID.NUMBER) {
            Value = value;
        }

        /// <summary>
        /// Returns the value.
        /// </summary>
        public double Value { get; }
    }
}