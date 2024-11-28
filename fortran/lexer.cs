// JFortran Compiler
// Lexical analysis
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

using System.Diagnostics;
using System.Text;
using CCompiler;

namespace JFortran;

/// <summary>
/// Defines the Fortran lexical analyser.
/// </summary>
public class Lexer {

    private readonly string[] _lines;
    private readonly FortranOptions _opts;
    private readonly MessageCollection _messages;
    private readonly List<SimpleToken> _tokens;

    private string _line;
    private int _index;
    private int _continuationCount;
    private char _pushedChar;
    private int _tindex;

    private const char EOL = '\n';
    private const char EOF = '\0';

    /// <summary>
    /// Initialises an instance of the <c>Lexer</c> class.
    /// </summary>
    /// <param name="lines">An array of input strings</param>
    /// <param name="opts">An instance of the <c>Options</c> class</param>
    /// <param name="messages">An instance of the <c>MessageCollection</c> class</param>
    public Lexer(string[] lines, FortranOptions opts, MessageCollection messages) {
        _lines = lines;
        _index = -1;
        _tindex = 0;
        _opts = opts;
        _messages = messages;
        _tokens = [];
    }

    /// <summary>
    /// Returns whether or not a label was found on the current
    /// line.
    /// </summary>
    public bool HasLabel { get; private set; }

    /// <summary>
    /// Returns the label found on the current line
    /// </summary>
    public string Label { get; private set; }

    /// <summary>
    /// Return the current line number.
    /// </summary>
    public int LineNumber { get; private set; }

    /// <summary>
    /// Read the parameter to a FORMAT string.
    /// </summary>
    /// <returns>The format.</returns>
    private string ReadFormat() {
        StringBuilder str = new();
        int start = 0;

        char ch = GetNextChar();
        if (ch == '(') {
            start = 1;
        }
        while (ch != EOL && ch != EOF) {
            str.Append(ch);
            ch = GetChar();
        }
        PushChar(ch);

        int length = str.Length;
        while (length > 0) {
            ch = str[length - 1];
            if (ch != ' ') {
                if (ch == ')') {
                    --length;
                }
                break;
            }
            --length;
        }
        str.Length = length;
        return str.ToString(start, length - start);
    }

    /// <summary>
    /// Move the token index back one token in the queue.
    /// </summary>
    public void BackToken() {
        Debug.Assert(_tindex > 0);
        --_tindex;
    }

    /// <summary>
    /// Peek at the next token in the input stream.
    /// </summary>
    /// <returns>A valid token</returns>
    public SimpleToken PeekToken() {
        return _tindex < _tokens.Count ? _tokens[_tindex] : null;
    }

    /// <summary>
    /// Peek at the next keyword tokenID in the input stream.
    /// </summary>
    /// <returns>A valid token ID</returns>
    public TokenID PeekKeyword() {
        return PeekToken().KeywordID;
    }

    /// <summary>
    /// Get the next keyword from the source file, or TokenID.IDENT if the keyword
    /// isn't recognised but looks like a valid identifier.
    ///
    /// This function also handles merging keywords that are represented by
    /// multiple tokens into a single keyword token. Thus:
    ///
    ///   GO TO -> GOTO
    ///   DOUBLE PRECISION -> DOUBLEPRECISION
    ///   (etc...)
    ///
    /// </summary>
    /// <returns>A SimpleToken representing the keyword found</returns>
    public SimpleToken GetKeyword() {
        SimpleToken token = GetToken();
        if (token is not IdentifierToken) {
            return token;
        }
        TokenID id = token.KeywordID;
        if (id == TokenID.IDENT) {
            return token;
        }
        if (id == TokenID.KGO) {
            // BUGBUG: Hack until full tokeniser implemented.
            LocalExpectToken(TokenID.KTO);
            id = TokenID.KGOTO;
            _tokens[_tindex - 2] = new SimpleToken(id);
            _tokens.RemoveAt(--_tindex);
        }
        else if (id == TokenID.KEND && PeekKeyword() == TokenID.KIF) {
            // BUGBUG: Hack until full tokeniser implemented.
            LocalExpectToken(TokenID.KIF);
            id = TokenID.KENDIF;
            _tokens[_tindex - 2] = new SimpleToken(id);
            _tokens.RemoveAt(--_tindex);
        }
        else if (id == TokenID.KELSE && PeekKeyword() == TokenID.KIF) {
            // BUGBUG: Hack until full tokeniser implemented.
            LocalExpectToken(TokenID.KIF);
            id = TokenID.KELSEIF;
            _tokens[_tindex - 2] = new SimpleToken(id);
            _tokens.RemoveAt(--_tindex);
        }
        else if (id == TokenID.KDOUBLE) {
            // BUGBUG: Hack until full tokeniser implemented.
            LocalExpectToken(TokenID.KPRECISION);
            id = TokenID.KDPRECISION;
            _tokens[_tindex - 2] = new SimpleToken(id);
            _tokens.RemoveAt(--_tindex);
        }
        else if (id == TokenID.KBLOCK) {
            // BUGBUG: Hack until full tokeniser implemented.
            LocalExpectToken(TokenID.KDATA);
            id = TokenID.KBLOCKDATA;
            _tokens[_tindex - 2] = new SimpleToken(id);
            _tokens.RemoveAt(--_tindex);
        }
        else if (id == TokenID.KIMPLICIT && PeekKeyword() == TokenID.KNONE) {
            GetToken();
            id = TokenID.KIMPLICITNONE;
        }
        return new SimpleToken(id);
    }

    /// <summary>
    /// Return the next token from the source file. Returns a token representing
    /// what was found. For strings, identifiers and numbers it returns a SimpleToken
    /// derived class that includes the actual value found. The caller should cast
    /// the return token to the appropriate type to access the value.
    /// </summary>
    /// <returns>A SimpleToken representing the token found</returns>
    public SimpleToken GetToken() {
        if (_tindex == _tokens.Count) {
            SimpleToken token;
            _tokens.Clear();
            do {
                token = ReadToken();
                _tokens.Add(token);

                // BUGBUG: This is a hack until the full tokeniser is implemented.
                TokenID keywordID = token.KeywordID;
                if (keywordID == TokenID.KFORMAT) {
                    _tokens.Add(new StringToken(ReadFormat()));
                }
                if (keywordID == TokenID.KSTOP) {
                    _tokens.Add(new StringToken(ReadFormat()));
                }
                if (keywordID == TokenID.KPAUSE) {
                    _tokens.Add(new StringToken(ReadFormat()));
                }
            } while (token.ID != TokenID.EOL && token.ID != TokenID.ENDOFFILE);
            _tindex = 0;
        }
        return _tokens[_tindex++];
    }

    // Ensure that the next token in the input is the one expected and report an error otherwise.
    // BUGBUG: Remove when full tokeniser implemented!
    private void LocalExpectToken(TokenID expectedID) {
        SimpleToken token = GetToken();
        if (token.KeywordID != expectedID) {
            _messages.Error(MessageCode.EXPECTEDTOKEN,
                $"Expected {Tokens.TokenIDToString(expectedID)}, but saw {token.KeywordID} instead");
            BackToken();
        }
    }

    // Return the next token from the source file. Returns a token representing
    // what was found. For strings, identifiers and numbers it returns a SimpleToken
    // derived class that includes the actual value found. The caller should cast
    // the return token to the appropriate type to access the value.
    private SimpleToken ReadToken() {
        int countOfUnrecognised = 0;
        do {
            char ch = GetNextChar();
            switch (ch) {
                case '$': // Hexadecimal literals
                    _messages.Warning(MessageCode.NONSTANDARDHEX, 4, "Non-standard hexadecimal specification");
                    return ParseBasedNumber(16);

                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    PushChar(ch);
                    return ParseNumber();

                case '!': // Comment
                    return new SimpleToken(TokenID.EOL);

                case '.': {
                    if (char.IsDigit(PeekChar())) {
                        PushChar(ch);
                        return ParseNumber();
                    }

                    StringBuilder str = new();

                    do {
                        str.Append(ch);
                        ch = GetNextChar();
                    } while (char.IsLetter(ch));

                    // Last character must be a '.'
                    str.Append(ch);

                    TokenID id = Tokens.StringToTokenID(str.ToString());
                    if (id == TokenID.IDENT) {
                        _messages.Error(MessageCode.UNRECOGNISEDKEYWORD, $"Unrecognised keyword: {str}");
                    }
                    return new SimpleToken(id);
                }

                case 'A':
                case 'B':
                case 'C':
                case 'D':
                case 'E':
                case 'F':
                case 'G':
                case 'H':
                case 'I':
                case 'J':
                case 'K':
                case 'L':
                case 'M':
                case 'N':
                case 'O':
                case 'P':
                case 'Q':
                case 'R':
                case 'S':
                case 'T':
                case 'U':
                case 'V':
                case 'W':
                case 'X':
                case 'Y':
                case 'Z':
                case 'a':
                case 'b':
                case 'c':
                case 'd':
                case 'e':
                case 'f':
                case 'g':
                case 'h':
                case 'i':
                case 'j':
                case 'k':
                case 'l':
                case 'm':
                case 'n':
                case 'o':
                case 'p':
                case 'q':
                case 'r':
                case 's':
                case 't':
                case 'u':
                case 'v':
                case 'w':
                case 'x':
                case 'y':
                case 'z': {
                    // Look for possible number base representations first
                    char chNext = PeekChar();
                    bool isQuoteNextChar = chNext is '\'' or '"';
                    if (ch is 'B' or 'b' && isQuoteNextChar) {
                        return ParseBasedNumber(2);
                    }
                    if (ch is 'Z' or 'z' && isQuoteNextChar) {
                        return ParseBasedNumber(16);
                    }
                    if (ch is 'O' or 'o' && isQuoteNextChar) {
                        return ParseBasedNumber(8);
                    }

                    // At this point is can either be an identifier or a keyword
                    // Fortran doesn't distinguish between either at this point.
                    StringBuilder str = new();
                    while (char.IsLetterOrDigit(ch) || ch == '_') {
                        str.Append(char.ToUpper(ch));
                        ch = GetChar();
                    }
                    PushChar(ch);
                    if (str.Length > 31) {
                        _messages.Error(MessageCode.IDENTIFIERTOOLONG, $"Identifier {str} too long");
                    }
                    return new IdentifierToken(str.ToString());
                }

                case '"':
                case '\'':
                    return new StringToken(ParseString(ch));

                case '(': return new SimpleToken(TokenID.LPAREN);
                case ')': return new SimpleToken(TokenID.RPAREN);
                case ',': return new SimpleToken(TokenID.COMMA);
                case '=': return new SimpleToken(TokenID.EQUOP);
                case '-': return new SimpleToken(TokenID.MINUS);
                case '+': return new SimpleToken(TokenID.PLUS);
                case ':': return new SimpleToken(TokenID.COLON);

                case '/':
                    ch = GetNextChar();
                    if (ch != '/') {
                        PushChar(ch);
                        return new SimpleToken(TokenID.DIVIDE);
                    }
                    return new SimpleToken(TokenID.CONCAT);

                case '*':
                    ch = GetNextChar();
                    if (ch != '*') {
                        PushChar(ch);
                        return new SimpleToken(TokenID.STAR);
                    }
                    return new SimpleToken(TokenID.EXP);

                case EOL:
                    return new SimpleToken(TokenID.EOL);

                case EOF:
                    return new SimpleToken(TokenID.ENDOFFILE);
            }

            // Error - not a character we recognise. If there are multiple ones then
            // we only report the first one per token request.
            if (countOfUnrecognised == 0) {
                _messages.Error(MessageCode.UNRECOGNISEDCHARACTER, $"Unrecognised character '{ch}' in input");
                ++countOfUnrecognised;
            }
        } while (true);
    }

    // Parse a string that commenced with the specified delimiter.
    private string ParseString(char chDelim) {
        StringBuilder str = new();
        bool didWarnOnce = false;

        char ch = GetChar();
        while (ch != EOL && ch != EOF) {
            if (ch == chDelim) {
                // Two consecutive string delimiters represent that string
                // delimiter within the string.
                char chTmp = GetChar();
                if (chTmp != chDelim) {
                    PushChar(chTmp);
                    break;
                }
            }
            else if (ch == '\\' && !_opts.Backslash) {
                ch = GetChar();
                switch (ch) {
                    case 'n':
                        ch = '\n';
                        break;
                    case 't':
                        ch = '\t';
                        break;
                    case 'b':
                        ch = '\b';
                        break;
                    case 'f':
                        ch = '\f';
                        break;
                    case '0':
                        ch = '\0';
                        break;
                    case '\'':
                        ch = '\'';
                        break;
                    case '"':
                        ch = '"';
                        break;
                    case '\\':
                        ch = '\\';
                        break;
                }
                if (!didWarnOnce) {
                    _messages.Warning(MessageCode.NONSTANDARDESCAPES, 4, "Non-standard escape sequence in strings");
                    didWarnOnce = true;
                }
            }
            str.Append(ch);
            ch = GetChar();
        }
        if (ch != chDelim) {
            _messages.Error(MessageCode.UNTERMINATEDSTRING, "Unterminated string");
        }
        return str.ToString();
    }

    // Get the next non-space character on the line.
    private char GetNextChar() {
        char ch;

        do {
            ch = GetChar();
        } while (ch is ' ' or '\t');
        return ch;
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

    // Returns whether the specified character is valid in the given
    // number base.
    private static bool IsValidInBase(char ch, int numberBase) {
        switch (numberBase) {
            case 2: return ch is '0' or '1';
            case 8: return ch is >= '0' and <= '7';
            case 16: return ch is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F';
        }
        Debug.Assert(false, $"IsValidInBase called with unsupported number base {numberBase}");
        return false;
    }

    // Parse a number in the specified base.
    // The number may be enclosed in single quotes as part of a base specification
    // so we skip those if they're present.
    private IntegerToken ParseBasedNumber(int numberBase) {
        StringBuilder str = new();
        char ch = GetNextChar();
        char chDelim = '\0';

        if (ch is '\'' or '"') {
            chDelim = ch;
            ch = GetNextChar();
        }
        while (IsValidInBase(ch, numberBase)) {
            str.Append(ch);
            ch = GetNextChar();
        }
        if (chDelim == '\0' || ch == chDelim) {
            if (ch != chDelim) {
                PushChar(ch);
            }
            try {
                return new IntegerToken(Convert.ToInt32(str.ToString(), numberBase));
            }
            catch (ArgumentException) { }
        }
        _messages.Error(MessageCode.BADNUMBERFORMAT, "Illegal number format");
        return new IntegerToken(0);
    }

    // Try to match a logical operand token
    private bool MatchOperator(char ch) {
        StringBuilder str = new();
        int savedIndex = _index;
        char savedPushedChar = _pushedChar;
        bool match = false;

        if (ch == '.') {
            str.Append(ch);
            ch = GetNextChar();
            while (char.IsLetter(ch)) {
                str.Append(ch);
                ch = GetNextChar();
            }
            if (ch == '.') {
                str.Append(ch);
            }
            match = Tokens.StringToTokenID(str.ToString()) switch {
                TokenID.KAND or TokenID.KEQ or TokenID.KEQV or TokenID.KFALSE or TokenID.KGE or TokenID.KGT or
                TokenID.KLE or TokenID.KLT or TokenID.KOR or TokenID.KNE or TokenID.KNEQV or TokenID.KNOT or
                TokenID.KTRUE or TokenID.KXOR => true,
                _ => match
            };
        }
        _pushedChar = savedPushedChar;
        _index = savedIndex;
        return match;
    }

    // Parse a decimal number.
    private SimpleToken ParseNumber() {
        StringBuilder str = new();
        bool isFloat = false;
        bool isDouble = false;

        char ch = GetNextChar();
        while (char.IsDigit(ch)) {
            str.Append(ch);
            ch = GetNextChar();
        }
        if (ch == '.' && !MatchOperator(ch)) {
            isFloat = true;
            str.Append(ch);
            ch = GetNextChar();
            while (char.IsDigit(ch)) {
                str.Append(ch);
                ch = GetNextChar();
            }
        }
        if ((char.ToUpper(ch) == 'E' || char.ToUpper(ch) == 'D') && !char.IsLetter(PeekChar())) {
            isFloat = true;
            isDouble = char.ToUpper(ch) == 'D';
            str.Append('E');
            ch = GetNextChar();
            if (ch is '+' or '-') {
                str.Append(ch);
                ch = GetNextChar();
            }
            while (char.IsDigit(ch)) {
                str.Append(ch);
                ch = GetNextChar();
            }
        }
        PushChar(ch);
        if (isDouble) {
            if (double.TryParse(str.ToString(), out double result)) {
                return new DoubleToken(result);
            }
        }
        else if (isFloat) {
            if (float.TryParse(str.ToString(), out float result)) {
                return new RealToken(result);
            }
        }
        else {
            if (int.TryParse(str.ToString(), out int result)) {
                return new IntegerToken(result);
            }
        }
        _messages.Error(MessageCode.BADNUMBERFORMAT, "Illegal number format");
        return new IntegerToken(0);
    }

    // Check whether we're at the end of the line. A '&' means we continue
    // onto the next line.
    private bool AtLineEnd() {
        return _index == _line.Length || (_line[_index] == '&' && _index + 1 == _line.Length);
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
        if (_index < 0) {
            HasLabel = false;
        }
        while (_index < 0 || AtLineEnd()) {
            if (_index >= 0 && !HasContinuation()) {
                _index = -1;
                _continuationCount = -1;
                return EOL;
            }
            if (!ReadLine()) {
                return EOF; // End of file
            }
            if (_continuationCount == 20) {
                _messages.Error(MessageCode.TOOMANYCONTINUATION, "Too many continuation lines");
            }
            ++_continuationCount;
        }
        return _line[_index++];
    }

    // Check if this line continues to the next
    // Three different checks done here:
    //  1. Look for '&' at the end of the current line.
    //  2. Look for a non-space or '0' in column 6 in fixed format mode.
    //  3. Look for a non-space or '0' following a tab in free format mode.
    private bool HasContinuation() {
        if (!string.IsNullOrEmpty(_line) && _line[^1] == '&') {
            return true;
        }
        string nextLine;
        int lineIndex = LineNumber;
        do {
            if (lineIndex == _lines.Length) {
                return false;
            }
            nextLine = _lines[lineIndex];
            ++lineIndex;
        } while (ShouldSkipLine(nextLine));
        for (int c = 0; c < nextLine.Length && c < 6; ++c) {
            if (nextLine[c] == '\t') {
                return nextLine[c + 1] > '0' && nextLine[c + 1] <= '9';
            }
        }
        if (nextLine.Length <= 5) {
            return false;
        }
        return nextLine[5] != ' ' && nextLine[5] != '0';
    }

    // Check whether the specified line is a comment or disabled debug line
    private bool ShouldSkipLine(string line) {
        if (_opts.F90) {
            foreach (char ch in line) {
                if (char.IsWhiteSpace(ch)) {
                    continue;
                }
                return ch == '!';
            }
        }
        return line.Length > 0 && (char.ToUpper(line[0]) == 'C' || line[0] == '*' || (char.ToUpper(line[0]) == 'D' && !_opts.GenerateDebug));
    }

    // Read a line from the source file and do initial parsing on
    // the first few columns. Return false if end of file.
    private bool ReadLine() {
        int len;

        // Get from stream, ignoring any lines which are
        // entirely comments.
        do {
            if (LineNumber == _lines.Length) {
                return false;
            }
            _line = _lines[LineNumber++];
            _index = 0;
            len = _line.Length;
        } while (ShouldSkipLine(_line));
        _messages.Linenumber = LineNumber;

        // For Fortran 90, lines are free format.
        if (_opts.F90) {
            return true;
        }

        // Ignore anything after column 72
        if (_line.Length > 72) {
            _line = _line[..72];
        }

        // First five columns are statement labels. However a tab can be used to
        // break this fixed convention and delimit a label from a statement.
        bool hasTab = false;

        int label = 0;
        while (_index < len && _index < 5) {
            char ch = _line[_index++];
            if (ch == '\t') {
                hasTab = true;
                break;
            }
            if (ch != ' ') {
                if (!char.IsDigit(ch)) {
                    _messages.Error(MessageCode.ILLEGALCHARACTERINLABEL,
                        $"Illegal character {ch} in statement label");
                    return true;
                }
                label = label * 10 + (ch - '0');
            }
        }
        if (label > 0) {
            Label = label.ToString();
            HasLabel = true;
        }

        // Check for a continuation character.
        if (hasTab) {
            if (_index < len) {
                char ch = _line[_index];
                if (char.IsDigit(ch) && ch != '0') {
                    ++_index;
                }
            }
        }
        else if (len > 5) {
            ++_index;
        }

        // Everything else is a statement line
        return true;
    }
}