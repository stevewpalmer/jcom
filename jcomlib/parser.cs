// JCom Runtime Library
// Command line parsing
//
// Authors:
//  Steve Palmer
//
// Copyright (C) 2023 Steve Palmer
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

namespace JComLib;

public class Parser {
    private readonly string _line;
    private int _index;
    private char _pushedChar;
    private const char Eol = '\n';

    /// <summary>
    /// Create an instance with the specified string
    /// </summary>
    /// <param name="input">String to parse</param>
    public Parser(string input) {
        _line = input;
        _index = 0;
    }

    /// <summary>
    /// Make a copy of the specified parser object.
    /// </summary>
    public Parser(Parser copy) {
        _line = copy._line;
        _index = copy._index;
        _pushedChar = copy._pushedChar;
    }

    /// <summary>
    /// Retrieve the remainder of the command line as a series of
    /// delimited string arguments.
    /// </summary>
    /// <returns>String array of arguments</returns>
    public string[] RestOfLine() {
        List<string> args = new();
        string argument = NextWord();
        while (argument != null) {
            args.Add(argument);
            argument = NextWord();
        }
        return args.ToArray();
    }

    /// <summary>
    /// Read the remainder of the command line and expand any wildcards.
    /// </summary>
    /// <returns>String array of arguments</returns>
    public IEnumerable<string> ReadAndExpandWildcards() {
        string[] matchfiles = RestOfLine();
        if (!matchfiles.Any()) {
            matchfiles = new[] { "*" };
        }
        string[] allfiles = matchfiles.SelectMany(f => Directory.GetFiles(".", f, SearchOption.TopDirectoryOnly)).ToArray();
        allfiles = Array.ConvertAll(allfiles, f => f.ToLower());
        Array.Sort(allfiles);
        return allfiles;
    }

    /// <summary>
    /// Retrieve the next word from the input. A single or double
    /// quote wraps all text up to the next matching quote or the
    /// end of the line. Otherwise a word is delimited by any space
    /// character.
    /// </summary>
    /// <returns>Next word, or null if no more words to read</returns>
    public string NextWord() {

        char ch = GetChar();
        while (ch != Eol) {
            while (char.IsWhiteSpace(ch)) {
                if ((ch = GetChar()) == Eol) {
                    break;
                }
            }
            switch (ch) {
                default: {
                    StringBuilder str = new();
                    while (ch != Eol && !char.IsWhiteSpace(ch)) {
                        str.Append(ch);
                        ch = GetChar();
                    }
                    PushChar(ch);
                    return str.ToString();
                }

                case Eol:
                    break;

                case '\'':
                case '"': {
                    char endCh = ch;
                    StringBuilder str = new();
                    ch = GetChar();
                    while (ch != Eol && ch != endCh) {
                        str.Append(ch);
                        ch = GetChar();
                    }
                    if (ch != endCh) {
                        PushChar(ch);
                    }
                    return str.ToString();
                }
            }
        }
        return null;
    }

    /// Push back the last character read so that the next call to GetChar will
    /// retrieve that character.
    private void PushChar(char ch) {
        Debug.Assert(_pushedChar == '\0');
        _pushedChar = ch;
    }

    // Check whether we're at the end of the line.
    private bool AtLineEnd() {
        return _index == _line.Length;
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
        return AtLineEnd() ? Eol : _line[_index++];
    }
}

