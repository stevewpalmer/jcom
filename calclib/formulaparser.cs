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
using System.Drawing;
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
    TEXT,
    KSUM,
    KNOW,
    KTODAY,
    COLON,
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
/// Basic cell parse node
/// </summary>
/// <param name="tokenID">Node token ID</param>
public class CellParseNode(TokenID tokenID) {

    /// <summary>
    /// Operator
    /// </summary>
    public TokenID Op { get; } = tokenID;

    /// <summary>
    /// Convert a token ID to its string representation.
    /// </summary>
    /// <param name="tokenId">Token ID</param>
    /// <returns>String</returns>
    public static string TokenToString(TokenID tokenId) =>
        tokenId switch {
            TokenID.PLUS => "+",
            TokenID.EXP => "^",
            TokenID.MINUS => "-",
            TokenID.MULTIPLY => "*",
            TokenID.DIVIDE => "/",
            TokenID.KLE => "<=",
            TokenID.KLT => "<",
            TokenID.KGE => ">=",
            TokenID.KGT => ">",
            TokenID.KEQ => "=",
            TokenID.KNE => "<>",
            TokenID.COLON => ":",
            TokenID.KSUM => "SUM",
            TokenID.KNOW => "NOW",
            TokenID.KTODAY => "TODAY",
            _ => ""
        };

    /// <summary>
    /// Convert this parse node to its raw string.
    /// </summary>
    /// <returns>String</returns>
    public virtual string ToRawString() => TokenToString(Op);

    /// <summary>
    /// Convert this parse node to its string.
    /// </summary>
    /// <returns>String</returns>
    public override string ToString() => TokenToString(Op);

    /// <summary>
    /// Fix up any address references on the node.
    /// </summary>
    /// <param name="location"></param>
    /// <param name="column">Column to fix</param>
    /// <param name="row">Row to fix</param>
    /// <param name="offset">Offset to be applied to the column and/or row</param>
    public virtual bool FixupAddress(CellLocation location, int column, int row, int offset) {
        return false;
    }
}

/// <summary>
/// Parse node for a function call.
/// </summary>
/// <param name="tokenID">Node token ID</param>
public class FunctionParseNode(TokenID tokenID, CellParseNode[] parameters) : CellParseNode(tokenID) {

    /// <summary>
    /// Function parameter list
    /// </summary>
    public CellParseNode[] Parameters { get; } = parameters;

    /// <summary>
    /// Convert this parse node to its raw string.
    /// </summary>
    /// <returns>String</returns>
    public override string ToRawString() => $"{TokenToString(Op)}({string.Join(",", Parameters.Select(p => p.ToRawString()))})";

    /// <summary>
    /// Convert this parse node to its string.
    /// </summary>
    /// <returns>String</returns>
    public override string ToString() => $"{TokenToString(Op)}({string.Join(",", Parameters.Select(p => p.ToString()))})";

    /// <summary>
    /// Fix up any address references on the node.
    /// </summary>
    /// <param name="location"></param>
    /// <param name="column">Column to fix</param>
    /// <param name="row">Row to fix</param>
    /// <param name="offset">Offset to be applied to the column and/or row</param>
    public override bool FixupAddress(CellLocation location, int column, int row, int offset) {
        bool fixup = false;
        foreach (CellParseNode parameter in Parameters) {
            if (parameter.FixupAddress(location, column, row, offset)) {
                fixup = true;
            }
        }
        return fixup;
    }
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

    /// <summary>
    /// Fix up any address references on the node.
    /// </summary>
    /// <param name="location"></param>
    /// <param name="column">Column to fix</param>
    /// <param name="row">Row to fix</param>
    /// <param name="offset">Offset to be applied to the column and/or row</param>
    public override bool FixupAddress(CellLocation location, int column, int row, int offset) {
        bool left = Left.FixupAddress(location, column, row, offset);
        bool right = Right.FixupAddress(location, column, row, offset);
        return left || right;
    }

    /// <summary>
    /// Convert this parse node to its raw string.
    /// </summary>
    /// <returns>String</returns>
    public override string ToRawString() {
        string left = Left.ToRawString();
        string right = Right.ToRawString();
        return FormatToString(left, right);
    }

    /// <summary>
    /// Convert this parse node to its string. For binary operations we
    /// add the appropriate parenthesis if the precedence of either side
    /// of the expression is less than this one.
    /// </summary>
    /// <returns>String</returns>
    public override string ToString() {
        string left = Left.ToString();
        string right = Right.ToString();
        return FormatToString(left, right);
    }

    private string FormatToString(string left, string right) {
        if (FormulaParser.Precedence(Op) > FormulaParser.Precedence(Left.Op)) {
            left = $"({left})";
        }
        if (FormulaParser.Precedence(Op) > FormulaParser.Precedence(Right.Op)) {
            right = $"({right})";
        }
        return left + TokenToString(Op) + right;
    }
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

    /// <summary>
    /// Convert this parse node to its raw string.
    /// </summary>
    /// <returns>String</returns>
    public override string ToRawString() => ToString();

    /// <summary>
    /// Convert this parse node to its string.
    /// </summary>
    /// <returns>String</returns>
    public override string ToString() {
        return Value.StringValue;
    }
}

/// <summary>
/// Represents a parse node that holds a string value.
/// </summary>
/// <param name="value">String value</param>
public class TextParseNode(string value) : CellParseNode(TokenID.TEXT) {

    /// <summary>
    /// Value of node
    /// </summary>
    public string Value { get; } = value;

    /// <summary>
    /// Convert this parse node to its raw string.
    /// </summary>
    /// <returns>String</returns>
    public override string ToRawString() => ToString();

    /// <summary>
    /// Convert this parse node to its string.
    /// </summary>
    /// <returns>String</returns>
    public override string ToString() {
        return Value;
    }
}

/// <summary>
/// Represents a parse node that holds a relative cell location.
/// </summary>
/// <param name="absoluteLocation">Absolute location</param>
/// <param name="relativeLocation">Relative location</param>
public class LocationParseNode(CellLocation absoluteLocation, Point relativeLocation) : CellParseNode(TokenID.ADDRESS) {

    /// <summary>
    /// Absolute location
    /// </summary>
    public CellLocation AbsoluteLocation { get; private set; } = absoluteLocation;

    /// <summary>
    /// Absolute location
    /// </summary>
    public Point RelativeLocation { get; private set; } = relativeLocation;

    /// <summary>
    /// True if the cell location now contains an error.
    /// </summary>
    public bool Error { get; private set; }

    /// <summary>
    /// Fix up any address references on the node.
    /// </summary>
    /// <param name="location">Location of this cell</param>
    /// <param name="column">Column to fix</param>
    /// <param name="row">Row to fix</param>
    /// <param name="offset">Offset to be applied to the column and/or row</param>
    public override bool FixupAddress(CellLocation location, int column, int row, int offset) {
        bool needRecalculate = false;
        if (column > 0) {
            if (AbsoluteLocation.Column + offset < 1) {
                Error = true;
            }
            else {
                if (AbsoluteLocation.Column >= column) {
                    AbsoluteLocation = AbsoluteLocation with { Column = AbsoluteLocation.Column + offset };
                }
                if (RelativeLocation.X < column) {
                    RelativeLocation = RelativeLocation with { X = AbsoluteLocation.Column - location.Column };
                }
            }
            needRecalculate = true;
        }
        if (row > 0) {
            if (AbsoluteLocation.Row + offset < 1) {
                Error = true;
            }
            else {
                if (AbsoluteLocation.Row >= row) {
                    AbsoluteLocation = AbsoluteLocation with { Row = AbsoluteLocation.Row + offset };
                }
                if (RelativeLocation.Y < row) {
                    RelativeLocation = RelativeLocation with { Y = AbsoluteLocation.Row - location.Row };
                }
            }
            needRecalculate = true;
        }
        return needRecalculate;
    }

    /// <summary>
    /// Convert this parse node to its raw string. The raw string is the internal
    /// representation used for copying cells in a location independent way.
    /// </summary>
    /// <returns>String</returns>
    public override string ToRawString() {
        return Error ? "!ERR" : Cell.LocationToAddress(RelativeLocation);
    }

    /// <summary>
    /// Convert this parse node to its string.
    /// </summary>
    /// <returns>String</returns>
    public override string ToString() {
        return Error ? "!ERR" : AbsoluteLocation.Address;
    }

    /// <summary>
    /// Convert the relative address to an absolute cell reference
    /// </summary>
    /// <returns>CellLocation</returns>
    public CellLocation ToAbsolute(CellLocation sourceCell) {
        return new CellLocation {
            Column = sourceCell.Column + RelativeLocation.X,
            Row = sourceCell.Row + RelativeLocation.Y
        };
    }
}

/// <summary>
/// A cell range of the format Start:End where Start and End are each
/// a LocationParseNode.
/// </summary>
/// <param name="start">Start of range</param>
/// <param name="end">End of range</param>
public class RangeParseNode(LocationParseNode start, LocationParseNode end) : CellParseNode(TokenID.RANGE) {

    /// <summary>
    /// Start of range
    /// </summary>
    public LocationParseNode RangeStart { get; } = start;

    /// <summary>
    /// End of range
    /// </summary>
    public LocationParseNode RangeEnd { get; } = end;

    /// <summary>
    /// Fix up any address references on the node.
    /// </summary>
    /// <param name="location"></param>
    /// <param name="column">Column to fix</param>
    /// <param name="row">Row to fix</param>
    /// <param name="offset">Offset to be applied to the column and/or row</param>
    public override bool FixupAddress(CellLocation location, int column, int row, int offset) {
        bool start = RangeStart.FixupAddress(location, column, row, offset);
        bool end = RangeEnd.FixupAddress(location, column, row, offset);
        return start || end;
    }

    /// <summary>
    /// Convert this parse node to its raw string. The raw string is the internal
    /// representation used for copying cells in a location independent way.
    /// </summary>
    /// <returns>String</returns>
    public override string ToRawString() {
        return $"{RangeStart.ToRawString()}:{RangeEnd.ToRawString()}";
    }

    /// <summary>
    /// Convert this parse node to its string.
    /// </summary>
    /// <returns>String</returns>
    public override string ToString() {
        return $"{RangeStart}:{RangeEnd}";
    }

    /// <summary>
    /// Return an iterator over the range defined by the start and end.
    /// </summary>
    /// <returns>The next cell location in the iterator, or null</returns>
    public IEnumerable<CellLocation> RangeIterator(CellLocation sourceCell) {
        CellLocation rangeStart = RangeStart.ToAbsolute(sourceCell);
        CellLocation rangeEnd = RangeEnd.ToAbsolute(sourceCell);
        int startColumn = Math.Min(rangeStart.Column, rangeEnd.Column);
        int endColumn = Math.Max(rangeStart.Column, rangeEnd.Column);
        int startRow = Math.Min(rangeStart.Row, rangeEnd.Row);
        int endRow = Math.Max(rangeStart.Row, rangeEnd.Row);

        for (int column = startColumn; column <= endColumn; column++) {
            for (int row = startRow; row <= endRow; row++) {
                yield return new CellLocation { Column = column, Row = row };
            }
        }
    }
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
    private readonly CellLocation _location;

    private const string InvalidFormulaError = "Invalid formula";

    /// <summary>
    /// List of built-in functions
    /// </summary>
    private readonly Dictionary<string, TokenID> _functions = new() {
        { "SUM", TokenID.KSUM },
        { "NOW", TokenID.KNOW },
        { "TODAY", TokenID.KTODAY }
    };

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
                    if (str.Length > 15) { // R(-4096)C(-255)
                        throw new FormatException(InvalidFormulaError);
                    }
                    tokens.Add(new CellAddressToken(CellAddressFormat.RELATIVE, str.ToString()));
                    break;
                }

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
                    if (_functions.TryGetValue(str.ToString().ToUpper(), out TokenID tokenId)) {
                        tokens.Add(new SimpleToken(tokenId));
                    }
                    else {
                        if (str.Length > 6) {
                            throw new FormatException(InvalidFormulaError);
                        }
                        tokens.Add(new CellAddressToken(CellAddressFormat.ABSOLUTE, str.ToString()));
                    }
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
                case ':': tokens.Add(new SimpleToken(TokenID.COLON)); break;

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
    /// Run the formula and return the root of the parse node that
    /// represents the formula.
    /// </summary>
    /// <returns>A CellParseNode</returns>
    public CellParseNode Parse() => ParseExpression(0);

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

    /// Read the next character from the stream. If we reach the end of the line check
    /// the next one for a continuation character. If one is found, consume
    /// the new line and return the next character. Otherwise, return EOL.
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
    private CellParseNode ParseExpression(int level) {
        CellParseNode op1 = Operand();
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

            case TokenID.KTODAY:
            case TokenID.KNOW:
                ExpectToken(TokenID.LPAREN);
                ExpectToken(TokenID.RPAREN);
                return new FunctionParseNode(token.ID, []);

            case TokenID.KSUM: {
                ExpectToken(TokenID.LPAREN);
                CellParseNode start = Operand();
                if (start is not LocationParseNode startRange) {
                    throw new FormatException(InvalidFormulaError);
                }
                ExpectToken(TokenID.COLON);
                CellParseNode end = Operand();
                if (end is not LocationParseNode endRange) {
                    throw new FormatException(InvalidFormulaError);
                }
                RangeParseNode rangeNode = new(startRange, endRange);
                ExpectToken(TokenID.RPAREN);
                return new FunctionParseNode(TokenID.KSUM, [rangeNode]);
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
                CellLocation absoluteLocation;
                Point relativeLocation;
                if (identToken.Format == CellAddressFormat.RELATIVE) {
                    relativeLocation = Cell.PointFromRelativeAddress(identToken.Address);
                    absoluteLocation = new CellLocation { Column = relativeLocation.X + _location.Column, Row = relativeLocation.Y + _location.Row};
                }
                else {
                    absoluteLocation = new CellLocation(identToken.Address);
                    relativeLocation = new Point(absoluteLocation.Column - _location.Column, absoluteLocation.Row - _location.Row);
                }
                return new LocationParseNode(absoluteLocation, relativeLocation);
            }
        }
        throw new FormatException(InvalidFormulaError);
    }

    /// <summary>
    /// Return the operator precedence of the specified operator
    /// token. Higher values have higher precedence.
    /// </summary>
    /// <param name="id">Token ID</param>
    /// <returns>Precedence level</returns>
    internal static int Precedence(TokenID id) =>
        id switch {
            TokenID.NUMBER => 20,
            TokenID.ADDRESS => 20,
            TokenID.KGT => 5,
            TokenID.KGE => 5,
            TokenID.KLE => 5,
            TokenID.KNE => 5,
            TokenID.KEQ => 5,
            TokenID.KLT => 5,
            TokenID.PLUS => 6,
            TokenID.MINUS => 6,
            TokenID.MULTIPLY => 7,
            TokenID.DIVIDE => 7,
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