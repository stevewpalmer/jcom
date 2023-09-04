// JComal
// Line Tokenisation
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
// under the License.

using System.Diagnostics;
using System.Text;

namespace JComal; 

/// <summary>
/// Defines the line tokeniser.
/// </summary>
public class LineTokeniser {
    private string _line;
    private int _index;
    private char _pushedChar;

    private const char EOL = '\n';

    /// <summary>
    /// Tokenise the specified line
    /// </summary>
    /// <param name="line">Source line</param>
    /// <returns>Array of tokens</returns>
    public SimpleToken [] TokeniseLine(string line) {
        _line = line.Trim();
        _index = 0;

        List<SimpleToken> tokens = new();
        bool endOfLine = false;
        do {
            char ch = GetChar();
            switch (ch) {
                case '0': case '1': case '2': case '3': case '4':
                case '5': case '6': case '7': case '8': case '9':
                    PushChar(ch);
                    tokens.Add(ParseNumber());
                    break;

                case '.':
                    if (char.IsDigit(PeekChar())) {
                        PushChar(ch);
                        tokens.Add(ParseNumber());
                    }
                    break;

                case 'A': case 'B': case 'C': case 'D': case 'E':
                case 'F': case 'G': case 'H': case 'I': case 'J':
                case 'K': case 'L': case 'M': case 'N': case 'O':
                case 'P': case 'Q': case 'R': case 'S': case 'T':
                case 'U': case 'V': case 'W': case 'X': case 'Y':
                case 'Z': case 'a': case 'b': case 'c': case 'd':
                case 'e': case 'f': case 'g': case 'h': case 'i':
                case 'j': case 'k': case 'l': case 'm': case 'n':
                case 'o': case 'p': case 'q': case 'r': case 's':
                case 't': case 'u': case 'v': case 'w': case 'x':
                case 'y': case 'z': {
                    StringBuilder str = new();
                    while (char.IsLetterOrDigit(ch) || ch == '\'') {
                        str.Append(ch);
                        ch = GetChar();
                    }
                    if (ch == Consts.StringChar || ch == Consts.IntegerChar) {
                        str.Append(ch);
                    } else {
                        PushChar(ch);
                    }
                    if (str.Length > Consts.MaximumIdentifierLength) {
                        tokens.Add(new ErrorToken("Identifier {0} too long", str.ToString()));
                        str.Length = Consts.MaximumIdentifierLength;
                    }
                    TokenID tokenID = Tokens.StringToTokenID(str.ToString());
                    if (tokenID == TokenID.IDENT) {
                        tokens.Add(new IdentifierToken(str.ToString()));
                        break;
                    }
                    tokens.Add(new SimpleToken(tokenID));
                    break;
                    }

                case '"': {
                    StringBuilder str = new();

                    ch = GetChar();
                    while (ch != EOL) {
                        if (ch == '"') {
                            if (PeekChar() != '"') {
                                break;
                            }
                            ch = GetChar();
                        }
                        str.Append(ch);
                        ch = GetChar();
                    }
                    if (ch == EOL) {
                        tokens.Add(new ErrorToken("Missing \"", string.Empty));
                    }
                    tokens.Add(new StringToken(str.ToString()));
                    break;
                    }

                case ' ':
                    ch = GetChar();
                    while (ch == ' ') {
                        ch = GetChar();
                    }
                    PushChar(ch);
                    tokens.Add(new SimpleToken(TokenID.SPACE));
                    break;

                case '(':   tokens.Add(new SimpleToken(TokenID.LPAREN)); break;
                case ')':   tokens.Add(new SimpleToken(TokenID.RPAREN)); break;
                case ',':   tokens.Add(new SimpleToken(TokenID.COMMA)); break;
                case '=':   tokens.Add(new SimpleToken(TokenID.KEQ)); break;
                case '-':   tokens.Add(new SimpleToken(TokenID.MINUS)); break;
                case '+':   tokens.Add(new SimpleToken(TokenID.PLUS)); break;
                case '*':   tokens.Add(new SimpleToken(TokenID.MULTIPLY)); break;
                case ';':   tokens.Add(new SimpleToken(TokenID.SEMICOLON)); break;
                case '\'':  tokens.Add(new SimpleToken(TokenID.APOSTROPHE)); break;
                case '~':   tokens.Add(new SimpleToken(TokenID.TILDE)); break;
                case '!':   tokens.Add(new SimpleToken(TokenID.COMMENT)); break;

                case '/':
                    ch = GetChar();
                    if (ch == '/') {
                        tokens.Add(new SimpleToken(TokenID.COMMENT));
                        break;
                    }
                    PushChar(ch);
                    tokens.Add(new SimpleToken(TokenID.DIVIDE));
                    break;

                case ':':
                    ch = GetChar();
                    if (ch == '=') {
                        tokens.Add(new SimpleToken(TokenID.KASSIGN));
                        break;
                    }
                    if (ch == '+') {
                        tokens.Add(new SimpleToken(TokenID.KINCADD));
                        break;
                    }
                    if (ch == '-') {
                        tokens.Add(new SimpleToken(TokenID.KINCSUB));
                        break;
                    }
                    PushChar(ch);
                    tokens.Add(new SimpleToken(TokenID.COLON));
                    break;

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

                case '^':
                    tokens.Add(new SimpleToken(TokenID.EXP));
                    break;

                case EOL:
                    tokens.Add(new SimpleToken(TokenID.EOL));
                    endOfLine = true;
                    break;

                default:
                    tokens.Add(new ErrorToken($"Bad character {ch}", ch.ToString()));
                    break;
            }
        } while (!endOfLine);
        return tokens.ToArray();
    }

    // Take a peep at the next non-space character in the input stream.
    private char PeekChar() {
        if (_pushedChar != '\0' && _pushedChar != ' ' && _pushedChar != '\t') {
            return _pushedChar;
        }
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

    // Parse a decimal number.
    private SimpleToken ParseNumber() {
        StringBuilder str = new();
        bool isFloat = false;

        char ch = GetChar();
        while (char.IsDigit(ch)) {
            str.Append(ch);
            ch = GetChar();
        }
        if (ch == '.') {
            isFloat = true;
            str.Append(ch);
            ch = GetChar();
            while (char.IsDigit(ch)) {
                str.Append(ch);
                ch = GetChar();
            }
        }
        if (char.ToUpper(ch) == 'E' && !char.IsLetter(PeekChar())) {
            isFloat = true;
            str.Append('E');
            ch = GetChar();
            if (ch == '+' || ch == '-') {
                str.Append(ch);
                ch = GetChar();
            }
            while (char.IsDigit(ch)) {
                str.Append(ch);
                ch = GetChar();
            }
        }
        PushChar(ch);
        if (isFloat) {
            if (float.TryParse(str.ToString(), out float result)) {
                return new FloatToken(result);
            }
        } else {
            if (int.TryParse(str.ToString(), out int result)) {
                return new IntegerToken(result);
            }
        }
        return new ErrorToken($"Bad number {str}", str.ToString());
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
        if (AtLineEnd()) {
            return EOL;
        }
        return _line[_index++];
    }
}
