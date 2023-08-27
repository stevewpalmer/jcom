// JOs
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

namespace JShell; 

	public class CommandLine {
		private readonly string _line;
		private int _index;
    private char _pushedChar;

    private const char EOL = '\n';

    public CommandLine(string input) {
			_line = input;
			_index = 0;
		}

    // Retrieve the remainder of the command line as a series of
    // delimited string arguments.
    public string[] RestOfLine() {
        List<string> args = new();
        string argument = NextWord();
        while (argument != null) {
            args.Add(argument);
            argument = NextWord();
        }
        return args.ToArray();
    }

    // Retrieve the next word from the input. A single or double
    // quote wraps all text up to the next matching quote or the
    // end of the line. Otherwise a word is delimited by any space
    // character.
		public string NextWord() {

        char ch = GetChar();
        while (ch != EOL) {
            while (char.IsWhiteSpace(ch)) {
                ch = GetChar();
            }
            switch (ch) {
                default: {
                    StringBuilder str = new();
                    while (ch != EOL && !char.IsWhiteSpace(ch)) {
                        str.Append(ch);
                        ch = GetChar();
                    }
                    PushChar(ch);
                    return str.ToString();
                }

                case EOL:
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

    // Check whether we're at the end of the line. A '&' means we continue
    // onto the next line.
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
        if (AtLineEnd()) {
            return EOL;
        }
        return _line[_index++];
    }
}

