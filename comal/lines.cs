// JComal
// Line number management
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
using JComLib;

namespace JComal; 

/// <summary>
/// A single tokenised line
/// </summary>
public class Line {

    private readonly List<SimpleToken> _tokens;
    private int _tindex;
    private SimpleToken _pushedToken;

    /// <summary>
    /// Construct a Line object
    /// </summary>
    /// <param name="tokens">Tokens</param>
    public Line(SimpleToken[] tokens) {
        _tokens = tokens.ToList();
        Debug.Assert(_tokens.Count > 0);
        Debug.Assert(_tokens[^1].ID == TokenID.EOL);
        _tindex = 0;
    }

    /// <summary>
    /// Line number
    /// </summary>
    public int LineNumber {
        get {
            if (_tokens[0].ID == TokenID.INTEGER) {
                IntegerToken lineNumberToken = _tokens[0] as IntegerToken;
                return lineNumberToken.Value;
            }
            return 0;
        }
        set {
            if (_tokens[0].ID == TokenID.INTEGER) {
                IntegerToken lineNumberToken = _tokens[0] as IntegerToken;
                lineNumberToken.Value = value;
            } else {
                _tokens.Insert(0, new IntegerToken(value));
                _tokens.Insert(1, new SimpleToken(TokenID.SPACE));
            }
        }
    }

    /// <summary>
    /// Return whether we're at the end of the line.
    /// </summary>
    public bool IsAtEndOfLine {
        get {
            Debug.Assert(_tindex < Tokens.Length);
            SimpleToken token;
            if (_pushedToken != null) {
                token = _pushedToken;
            } else {
                int index = _tindex;
                while (Tokens[index].ID == TokenID.SPACE) {
                    ++index;
                }
                token = Tokens[index];
            }
            return token.ID is TokenID.EOL or TokenID.COMMENT;
        }
    }

    /// <summary>
    /// Return whether we're at the end of the current statement.
    /// </summary>
    public bool IsAtEndOfStatement {
        get {
            SimpleToken token = PeekToken();
            return token.ID is TokenID.COMMENT or TokenID.EOL or TokenID.KELSE;
        }
    }

    /// <summary>
    /// Skip to the end of the line
    /// </summary>
    public void SkipToEndOfLine() {
        Debug.Assert(Tokens.Length > 0 && Tokens[^1].ID == TokenID.EOL);
        _tindex = Tokens.Length - 1;
    }

    /// <summary>
    /// Move the token index back one token in the queue.
    /// </summary>
    public void PushToken(SimpleToken token) {
        _pushedToken = token;
    }

    /// <summary>
    /// Reset the token index to the start of the line.
    /// </summary>
    public void Reset() {
        _tindex = 0;
        _pushedToken = null;
    }

    /// <summary>
    /// Peek at the next token from the line
    /// </summary>
    /// <returns>SimpleToken</returns>
    public SimpleToken PeekToken() {

        Debug.Assert(_tindex < Tokens.Length);
        if (_pushedToken != null) {
            return _pushedToken;
        }
        int peekIndex = _tindex;
        while (Tokens[peekIndex].ID == TokenID.SPACE) {
            ++peekIndex;
        }
        return Tokens[peekIndex];
    }

    /// <summary>
    /// Replace the current token with the specified token.
    /// </summary>
    /// <param name="newToken">New token</param>
    public void ReplaceToken(SimpleToken newToken) {
        int index = _tindex;
        if (_pushedToken != null) {
            --index;
            _pushedToken = newToken;
        }
        Debug.Assert(index > 0 && index < Tokens.Length);
        _tokens[index] = newToken;
    }

    /// <summary>
    /// Insert the specified tokens into the token
    /// list at the current position.
    /// </summary>
    /// <param name="newTokens">Array of tokens to insert</param>
    public void InsertTokens(SimpleToken [] newTokens) {

        Debug.Assert(_tindex > 0);
        int index = _tindex;
        if (_pushedToken != null && _pushedToken.ID != TokenID.EOL) {
            --index;
        }
        TokenID lastToken = _tokens[index].ID;
        foreach (SimpleToken newToken in newTokens) {
            if (newToken.ID == TokenID.SPACE && lastToken == TokenID.SPACE) {
                continue;
            }
            _tokens.Insert(index++, newToken);
            lastToken = _tokens[index].ID;
        }
    }

    /// <summary>
    /// Get the next token from the line. Spaces and comments are
    /// skipped. EOL is returned if we're at the end of the line.
    /// </summary>
    /// <returns>SimpleToken</returns>
    public SimpleToken GetToken() {

        SimpleToken token;
        if (_pushedToken != null) {
            token = _pushedToken;
            _pushedToken = null;
        } else {
            Debug.Assert(_tindex < Tokens.Length);
            while (Tokens[_tindex].ID == TokenID.SPACE) {
                ++_tindex;
            }
            token = Tokens[_tindex];
            if (token.ID == TokenID.COMMENT) {
                while (Tokens[_tindex].ID != TokenID.EOL) {
                    ++_tindex;
                }
                token = Tokens[_tindex];
            }
            if (token.ID != TokenID.EOL) {
                ++_tindex;
            }
        }
        return token;
    }

    /// <summary>
    /// Tokenised line
    /// </summary>
    private SimpleToken [] Tokens => _tokens.ToArray();

    /// <summary>
    /// Return printable version of line
    /// </summary>
    public string PrintableLine(int indent, bool includeLineNumber = true) {

        StringBuilder line = new();
        int index = 0;
        if (Tokens.Length > 0 && Tokens[index].ID == TokenID.INTEGER) {
            if (includeLineNumber) {
                IntegerToken lineNumberToken = Tokens[index] as IntegerToken;
                line.Append($"{lineNumberToken.Value,5}");
            }
            index++;
        }
        while (indent-- > 0) {
            line.Append("  ");
        }
        while (index < Tokens.Length && Tokens[index].ID != TokenID.EOL) {
            line.Append(Tokens[index]);
            index++;
        }
        return line.ToString();
    }

    /// <summary>
    /// Serialize this program line to the specified byte stream.
    /// </summary>
    public void Serialize(ByteWriter byteWriter) {

        foreach (SimpleToken token in _tokens) {
            token.Serialize(byteWriter);
        }
    }

    /// <summary>
    /// Deserialize a program line from the byte reader into a Line
    /// </summary>
    /// <param name="byteReader">Byte reader</param>
    /// <returns></returns>
    public static Line Deserialize(ByteReader byteReader) {

        List<SimpleToken> tokens = new();
        SimpleToken token;
        do {
            token = SimpleToken.Deserialize(byteReader);
            tokens.Add(token);
        } while (token.ID != TokenID.EOL);
        return new Line(tokens.ToArray());

    }
}

/// <summary>
/// Storage of lines and line numbers
/// </summary>
public class Lines {

    private readonly List<Line> _lines;
    private int _currentLine;

    /// <summary>
    /// Copy constructor
    /// </summary>
    public Lines() {
        _lines = new List<Line>();
    }

    /// <summary>
    /// Copy constructor
    /// </summary>
    /// <param name="source">Original lines</param>
    public Lines(Lines source) {
        _lines = new List<Line>(source._lines);
        _currentLine = 0;
    }

    /// <summary>
    /// Initialise a Lines collection with the given line
    /// </summary>
    /// <param name="line">Line object</param>
    public Lines(Line line) {
        _lines = new List<Line> { line };
        _currentLine = 0;
    }

    /// <summary>
    /// Return whether we've iterated to the end of the list of lines.
    /// </summary>
    public bool EndOfFile => _currentLine == _lines.Count;

    /// <summary>
    /// Return the index of the current line
    /// </summary>
    public int Index => _currentLine;

    /// <summary>
    /// Reset to start of lines.
    /// </summary>
    public void Reset() {
        _currentLine = 0;
        foreach (Line line in _lines) {
            line.Reset();
        }
    }

    /// <summary>
    /// Move back to the previous line.
    /// </summary>
    public void BackLine() {
        if (_currentLine > 0) {
            Line nextLine = _lines[--_currentLine];
            nextLine.Reset();
        }
    }

    /// <summary>
    /// Return the next Line from the list of lines.
    /// </summary>
    public Line NextLine {
        get {
            if (_currentLine < _lines.Count) {
                Line nextLine = _lines[_currentLine++];
                nextLine.Reset();
                return nextLine;
            }
            return null;
        }
    }

    /// <summary>
    /// Return an array of all lines
    /// </summary>
    public Line[] AllLines => _lines.ToArray();

    /// <summary>
    /// Return the highest line number.
    /// </summary>
    public int MaxLine => _lines.Count == 0 ? 0 : _lines[^1].LineNumber;

    /// <summary>
    /// Returns a specified line given its line number.
    /// </summary>
    /// <param name="lineNumber">Number of the line required</param>
    public Line Get(int lineNumber) {
        return _lines.FirstOrDefault(line => line.LineNumber == lineNumber);
    }

    /// <summary>
    /// Add the specified line to the line numbers, replacing any existing
    /// line with the same number.
    /// </summary>
    /// <param name="line">Line object representing the tokenised line</param>
    public void Add(Line line) {
        Debug.Assert(line.LineNumber > 0);
        for (int index = 0; index < _lines.Count; ++index) {
            if (_lines[index].LineNumber == line.LineNumber) {
                _lines[index] = line;
                return;
            }
            if (_lines[index].LineNumber > line.LineNumber) {
                _lines.Insert(index, line);
                return;
            }
        }
        _lines.Add(line);
    }

    /// <summary>
    /// Deletes the specified range of lines
    /// </summary>
    /// <param name="start">Start line number</param>
    /// <param name="end">End line number</param>
    public void Delete(int start, int end) {

        for (int index = _lines.Count - 1; index >= 0; index--) {
            Line line = _lines[index];
            if (line.LineNumber >= start && line.LineNumber <= end) {
                _lines.RemoveAt(index);
            }
        }
    }

    /// <summary>
    /// Return the line numbers at which the specified procedure or function
    /// is defined.
    /// </summary>
    /// <param name="name">Procedure or function name</param>
    /// <param name="startLine">On success, this will be the start line</param>
    /// <param name="endLine">On success, this will be the end line</param>
    /// <returns>true if procedure found, false otherwise</returns>
    public bool LinesForProcedure(string name, ref int startLine, ref int endLine) {

        bool found = false;
        foreach (Line line in _lines) {
            if (line.GetToken() is IntegerToken lineNumber) {
                SimpleToken firstToken = line.GetToken();
                if (firstToken.ID is TokenID.KPROC or TokenID.KFUNC) {
                    if (line.GetToken() is IdentifierToken nameToken &&
                        string.Equals(nameToken.Name, name, StringComparison.CurrentCultureIgnoreCase)) {
                        startLine = lineNumber.Value;
                        found = true;
                    }
                }
                if (firstToken.ID is TokenID.KENDPROC or TokenID.KENDFUNC) {
                    if (found) {
                        endLine = lineNumber.Value;
                        break;
                    }
                }
            }
        }
        Reset();
        return found;
    }

    /// <summary>
    /// Renumber all lines starting with the given start value and
    /// incrementing with the step provided.
    /// </summary>
    /// <param name="start">Start line number</param>
    /// <param name="step">Steps</param>
    public void Renumber(int start, int step) {

        int number = start;
        foreach (Line line in _lines) {
            line.LineNumber = number;
            number += step;
        }
    }

    /// <summary>
    /// Clear all lines
    /// </summary>
    public void Clear() {
        _lines.Clear();
        _currentLine = 0;
    }

    /// <summary>
    /// Serialize the current program lines to a byte array for
    /// saving to disk.
    /// </summary>
    /// <returns>Byte array</returns>
    public byte [] Serialize() {

        ByteWriter byteWriter = new();
        foreach (Line line in _lines) {
            line.Serialize(byteWriter);
        }
        return byteWriter.Buffer;
    }

    /// <summary>
    /// Deserialize the byte array to program lines.
    /// </summary>
    /// <param name="byteReader">Byte reader</param>
    public void Deserialize(ByteReader byteReader) {
        do {
            Line line = Line.Deserialize(byteReader);
            Add(line);
        } while (!byteReader.End);
    }
}
