// JCalcLib
// Parse a formula tree
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
using System.Drawing;
using System.Reflection;
using System.Text;

namespace JCalcLib;

/// <summary>
/// List of cell node tokens
/// </summary>
public enum TokenID {
    ADDRESS = 1,
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
    TEXT,
    FUNCTION,
    COLON,
    CONCAT,
    COMMA,
    RANGE
}

/// <summary>
/// Cell address format
/// </summary>
public enum CellAddressFormat {
    RELATIVE,
    ABSOLUTE
}

/// <summary>
/// Implements a cell formula parser to construct the associated
/// formula tree from the input.
/// </summary>
public class FormulaParser {

    private const char EOL = '\n';

    private const string InvalidFormulaError = "Invalid formula";
    private readonly string _line;
    private readonly CellLocation _location;
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
    /// <param name="location">Location of cell containing formula being parsed</param>
    /// <exception cref="Exception">Errors found in the formula expression</exception>
    public FormulaParser(string line, CellLocation location) {
        _line = line.Trim();
        _location = location;
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

                case 'R' when PeekChar() == '(': {
                    StringBuilder str = new();
                    str.Append(ch);
                    str.Append(ExpectChar('('));
                    ch = GetChar();
                    if (ch == '-') {
                        str.Append(ch);
                        ch = GetChar();
                    }
                    while (char.IsDigit(ch)) {
                        str.Append(ch);
                        ch = GetChar();
                    }
                    PushChar(ch);
                    str.Append(ExpectChar(')'));
                    str.Append(ExpectChar('C'));
                    str.Append(ExpectChar('('));
                    ch = GetChar();
                    if (ch == '-') {
                        str.Append(ch);
                        ch = GetChar();
                    }
                    while (char.IsDigit(ch)) {
                        str.Append(ch);
                        ch = GetChar();
                    }
                    PushChar(ch);
                    str.Append(ExpectChar(')'));
                    if (str.Length > 15) {
                        // R(-4096)C(-255)
                        throw new FormatException(InvalidFormulaError);
                    }
                    tokens.Add(new CellAddressToken(CellAddressFormat.RELATIVE, str.ToString()));
                    break;
                }

                case >= 'A' and <= 'Z' or >= 'a' and <= 'z': {
                    StringBuilder str = new();
                    while (char.IsLetterOrDigit(ch) || ch == '!') {
                        str.Append(ch);
                        ch = GetChar();
                    }
                    PushChar(ch);

                    string name = str.ToString();
                    MethodInfo? methodInfo = FunctionNode.GetFunction(name);
                    if (methodInfo != null) {
                        tokens.Add(new FunctionToken(methodInfo));
                        break;
                    }
                    if (!CellLocation.TryParseAddress(name, out CellLocation _)) {
                        throw new FormatException(InvalidFormulaError);
                    }
                    tokens.Add(new CellAddressToken(CellAddressFormat.ABSOLUTE, name));
                    break;
                }

                case ' ':
                    ch = GetChar();
                    while (ch == ' ') {
                        ch = GetChar();
                    }
                    PushChar(ch);
                    break;

                case '\'':
                case '"': {
                    char endCh = ch;
                    StringBuilder str = new();
                    ch = GetChar();
                    while (ch != EOL && ch != endCh) {
                        str.Append(ch);
                        ch = GetChar();
                    }
                    if (ch != endCh) {
                        throw new FormatException(InvalidFormulaError);
                    }
                    tokens.Add(new StringToken(str.ToString()));
                    break;
                }

                case '(': tokens.Add(new SimpleToken(TokenID.LPAREN)); break;
                case ')': tokens.Add(new SimpleToken(TokenID.RPAREN)); break;
                case '=': tokens.Add(new SimpleToken(TokenID.KEQ)); break;
                case '-': tokens.Add(new SimpleToken(TokenID.MINUS)); break;
                case '+': tokens.Add(new SimpleToken(TokenID.PLUS)); break;
                case '*': tokens.Add(new SimpleToken(TokenID.MULTIPLY)); break;
                case '/': tokens.Add(new SimpleToken(TokenID.DIVIDE)); break;
                case '^': tokens.Add(new SimpleToken(TokenID.EXP)); break;
                case ':': tokens.Add(new SimpleToken(TokenID.COLON)); break;
                case '&': tokens.Add(new SimpleToken(TokenID.CONCAT)); break;
                case ',': tokens.Add(new SimpleToken(TokenID.COMMA)); break;

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
                    throw new FormatException(InvalidFormulaError);
            }
        } while (!endOfLine);
    }

    /// <summary>
    /// Run the formula and return the root of the node that
    /// represents the formula.
    /// </summary>
    /// <returns>A CellParseNode</returns>
    internal CellNode Parse() => Expression(0);

    /// <summary>
    /// Make sure the next character read is the specified
    /// character and then return it. Throw a FormatException
    /// otherwise.
    /// </summary>
    /// <param name="expectedChar">Character expected</param>
    /// <returns>The character read</returns>
    /// <exception cref="FormatException"></exception>
    private char ExpectChar(char expectedChar) {
        char ch = GetChar();
        if (ch != expectedChar) {
            throw new FormatException(InvalidFormulaError);
        }
        return ch;
    }

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

        char ch = GetCharSkipSpaces();
        while (char.IsDigit(ch)) {
            str.Append(ch);
            ch = GetCharSkipSpaces();
        }
        if (ch == '.') {
            str.Append(ch);
            ch = GetCharSkipSpaces();
            while (char.IsDigit(ch)) {
                str.Append(ch);
                ch = GetCharSkipSpaces();
            }
        }
        if (char.ToUpper(ch) == 'E' && !char.IsLetter(PeekChar())) {
            str.Append('E');
            ch = GetCharSkipSpaces();
            if (ch is '+' or '-') {
                str.Append(ch);
                ch = GetCharSkipSpaces();
            }
            while (char.IsDigit(ch)) {
                str.Append(ch);
                ch = GetCharSkipSpaces();
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
        throw new FormatException(InvalidFormulaError);
    }

    // Check whether we're at the end of the line.
    private bool AtLineEnd() {
        return _index == _line.Length;
    }

    /// Push back the last character read so that the next call to GetChar will
    /// retrieve that character.
    private void PushChar(char ch) {
        Debug.Assert(_pushedChar == '\0');
        _pushedChar = ch;
    }

    /// <summary>
    /// Get the next non-whitespace character.
    /// </summary>
    /// <returns>The next non-whitespace character or EOL if we reach the end of the line.</returns>
    private char GetCharSkipSpaces() {
        char ch;
        do {
            ch = GetChar();
        } while (ch is ' ' or '\t');
        return ch;
    }

    /// <summary>
    /// Read the next character from the stream. If we reach the end of the line check
    /// the next one for a continuation character. If one is found, consume
    /// the new line and return the next character. Otherwise, return EOL.
    /// </summary>
    /// <returns>The next non-whitespace character or EOL if we reach the end of the line.</returns>
    private char GetChar() {
        if (_pushedChar != '\0') {
            char ch = _pushedChar;
            _pushedChar = '\0';
            return ch;
        }
        return AtLineEnd() ? EOL : _line[_index++];
    }

    /// <summary>
    /// Parse the next token from the formula string.
    /// </summary>
    /// <returns>A SimpleToken represented the parsed token</returns>
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
            throw new FormatException(InvalidFormulaError);
        }
    }

    /// <summary>
    /// Parse an expression.
    /// </summary>
    /// <param name="level">Current precedence level</param>
    /// <returns>CellParseNode</returns>
    private CellNode Expression(int level) {
        CellNode op1 = Operand();
        bool done = false;

        while (!done) {
            SimpleToken token = GetNextToken();
            int preced;
            switch (token.ID) {
                case TokenID.KLE:
                case TokenID.KGE:
                case TokenID.KGT:
                case TokenID.KNE:
                case TokenID.KEQ:
                case TokenID.KLT:
                case TokenID.MINUS:
                case TokenID.PLUS:
                case TokenID.MULTIPLY:
                case TokenID.DIVIDE:
                case TokenID.CONCAT:
                case TokenID.EXP:
                    preced = Precedence(token.ID);
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
                op1 = new BinaryOpNode(token.ID, op1, Expression(preced));
            }
        }
        return op1;
    }

    /// <summary>
    /// Parse a single operand
    /// </summary>
    /// <returns>A CellParseNode</returns>
    private CellNode Operand() {
        SimpleToken token = GetNextToken();
        switch (token.ID) {
            case TokenID.LPAREN: {
                CellNode node = Expression(0);
                ExpectToken(TokenID.RPAREN);
                return node;
            }

            case TokenID.FUNCTION:
                FunctionToken functiontoken = (FunctionToken)token;
                return ParseArguments(functiontoken);

            case TokenID.PLUS:
                return Operand();

            case TokenID.MINUS:
                return new BinaryOpNode(TokenID.MINUS, new NumberNode(0), Expression(9));

            case TokenID.NUMBER: {
                NumberToken numberToken = (NumberToken)token;
                return new NumberNode(numberToken.Value);
            }

            case TokenID.TEXT: {
                StringToken stringToken = (StringToken)token;
                return new TextNode(stringToken.Value);
            }

            case TokenID.ADDRESS: {
                LocationNode location = ParseLocation(token);
                token = GetNextToken();
                if (token.ID != TokenID.COLON) {
                    PushToken(token);
                }
                else {
                    token = GetNextToken();
                    if (token.ID != TokenID.ADDRESS) {
                        throw new FormatException(InvalidFormulaError);
                    }
                    LocationNode endLocation = ParseLocation(token);
                    return new RangeNode(location, endLocation);
                }
                return location;
            }
        }
        throw new FormatException(InvalidFormulaError);
    }

    private LocationNode ParseLocation(SimpleToken token) {
        Debug.Assert(token is CellAddressToken);
        CellLocation absoluteLocation;
        Point relativeLocation;
        CellAddressToken addressToken = (CellAddressToken)token;
        if (addressToken.Format == CellAddressFormat.RELATIVE) {
            relativeLocation = Cell.PointFromRelativeAddress(addressToken.Address);
            absoluteLocation = new CellLocation { Column = relativeLocation.X + _location.Column, Row = relativeLocation.Y + _location.Row };
        }
        else {

            // If this is a reference to a cell on another sheet, there is no relative location.
            absoluteLocation = new CellLocation(addressToken.Address);
            relativeLocation = absoluteLocation.SheetName != null ? new Point(0, 0) : new Point(absoluteLocation.Column - _location.Column, absoluteLocation.Row - _location.Row);
        }
        return new LocationNode(absoluteLocation, relativeLocation);
    }

    /// <summary>
    /// Parse the argument list to a function based on the specified
    /// rules.
    /// </summary>
    /// <param name="functionToken">Function token</param>
    /// <returns>A function node</returns>
    private FunctionNode ParseArguments(FunctionToken functionToken) {
        List<CellNode> args = [];
        ExpectToken(TokenID.LPAREN);
        ParameterInfo[] parameters = functionToken.MethodInfo.GetParameters();
        int maxCount = parameters.Length;
        if (maxCount > 0) {
            bool isParam = parameters.Last().GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0;
            if (isParam) {
                maxCount = 255;
            }
        }
        while (maxCount-- > 0) {
            args.Add(Expression(0));
            SimpleToken token = GetNextToken();
            if (token.ID != TokenID.COMMA) {
                PushToken(token);
                break;
            }
        }
        ExpectToken(TokenID.RPAREN);
        return new FunctionNode(functionToken.MethodInfo, args.ToArray());
    }

    /// <summary>
    /// Return the operator precedence of the specified operator
    /// token. Higher values have higher precedence.
    /// </summary>
    /// <param name="id">Token ID</param>
    /// <returns>Precedence level</returns>
    internal static int Precedence(TokenID id) =>
        id switch {
            TokenID.FUNCTION => 20,
            TokenID.NUMBER => 20,
            TokenID.ADDRESS => 20,
            TokenID.TEXT => 20,
            TokenID.KGT => 5,
            TokenID.KGE => 5,
            TokenID.KLE => 5,
            TokenID.KNE => 5,
            TokenID.KEQ => 5,
            TokenID.KLT => 5,
            TokenID.CONCAT => 6,
            TokenID.PLUS => 7,
            TokenID.MINUS => 7,
            TokenID.MULTIPLY => 8,
            TokenID.DIVIDE => 8,
            TokenID.EXP => 10,
            _ => 0
        };

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
        /// <param name="format">Format of this cell address</param>
        /// <param name="address">A cell address</param>
        public CellAddressToken(CellAddressFormat format, string address) : base(TokenID.ADDRESS) {
            Address = address;
            Format = format;
        }

        /// <summary>
        /// Format of cell address. Relative or absolute
        /// </summary>
        public CellAddressFormat Format { get; }

        /// <summary>
        /// Returns the cell address.
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

    private class StringToken : SimpleToken {

        /// <summary>
        /// Creates a string token with the given value.
        /// </summary>
        /// <param name="value">The string</param>
        public StringToken(string value) : base(TokenID.TEXT) {
            Value = value;
        }

        /// <summary>
        /// Returns the string.
        /// </summary>
        public string Value { get; }
    }

    private class FunctionToken : SimpleToken {

        /// <summary>
        /// Creates a function token with the given name.
        /// </summary>
        /// <param name="methodInfo">A MethodInfo with details of the function call</param>
        public FunctionToken(MethodInfo methodInfo) : base(TokenID.FUNCTION) {
            MethodInfo = methodInfo;
        }

        /// <summary>
        /// Returns the function MethodInfo detailing the call semantics.
        /// </summary>
        public MethodInfo MethodInfo { get; }
    }
}