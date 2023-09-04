// FORTRAN Runtime Library
// FORTRAN format string parsing
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

using System.Collections;
using System.Text;
using JComLib;

namespace JFortranLib; 

/// <summary>
/// Defines one format group which is a collection of format specifiers
/// enclosed in parenthesis.
/// </summary>
internal sealed class FormatGroup {

    /// <summary>
    /// Gets or sets the index of the group start.
    /// </summary>
    /// <value>The index of the group start</value>
    public int GroupStartIndex { get; set; }

    /// <summary>
    /// Gets or sets the group repeat count.
    /// </summary>
    /// <value>The group repeat count</value>
    public int GroupRepeat { get; set; }
}

/// <summary>
/// Defines a class that controls the interpretation of FORMAT statements
/// and issues FormatRecords to the I/O functions.
/// </summary>
public class FormatManager {

    private readonly string _formatString;
    private readonly int _fmtLength;

    private FormatOptionalPlus _plusRequired;
    private FormatRecord _lastRecord;
    private Stack _groups;
    private int _charIndex;
    private int _cRepeat;
    private int _scaleFactor;
    private bool _leftJustify;
    private int _lastFormatGroup;
    private bool _suppressCarriage;

    /// <summary>
    /// Constructs an instance of a FormatManager using the given
    /// format string.
    /// </summary>
    /// <param name="formatString">A FORTRAN format string.</param>
    public FormatManager(string formatString) {
        if (formatString == "*") {
            formatString = string.Empty;
        }
        _formatString = formatString;
        _fmtLength = _formatString.Length;
        _leftJustify = false;
        _plusRequired = FormatOptionalPlus.Default;
        _groups = new Stack();
    }

    /// <summary>
    /// Returns a flag that specifies whether the format string
    /// is empty.
    /// </summary>
    /// <returns><c>true</c> if the format is empty; otherwise, <c>false</c>.</returns>
    public bool IsEmpty() {
        return string.IsNullOrEmpty(_formatString);
    }

    /// <summary>
    /// Gets or sets a value indicating whether blank characters in
    /// the input are ignored or treated as '0' characters.
    /// </summary>
    /// <value><c>true</c> if blanks are '0'; otherwise, <c>false</c>.</value>
    public bool BlanksAsZero { set; get; }

    /// <summary>
    /// Reset this the format scan to the beginning so that the next
    /// call to Next() returns the first format record.
    /// </summary>
    public void Reset() {
        _charIndex = _lastFormatGroup;
        _cRepeat = 0;
        _groups = new Stack();
    }

    /// <summary>
    /// Returns the next FormatRecord from the string or null if
    /// there are no further records to retrieve.
    /// </summary>
    /// <returns>A FormatRecord, or null if we reached the end</returns>
    public FormatRecord Next() {

        // Repeat the last record if we've still some
        // more to go.
        if (_cRepeat > 1) {
            --_cRepeat;
            return _lastRecord;
        }

        // Otherwise find the next record and return that.
        StringBuilder str = new();
        bool inQuote = false;
        bool needLastRecord = false;

        while (_charIndex < _fmtLength) {
            char ch = NextChar();
            if (ch == '"' || ch == '\'') {
                inQuote = !inQuote;
                ++_charIndex;
            } else if (inQuote) {
                str.Append(ch);
                ++_charIndex;
            } else {
                if (ch == ',' || char.IsWhiteSpace(ch)) {
                    ++_charIndex;
                    continue;
                }

                // If we've gathered some raw string up to here
                // then return that now.
                if (str.Length > 0) {
                    _lastRecord = new FormatRecord {
                        RawString = str.ToString()
                    };
                    return _lastRecord;
                }

                // Remember this offset for _lastFormatGroup later
                int markedIndex = _charIndex-1;

                // Check and validate any repeat specifier. Note that X, P and H are required to have
                // a value preceding them. It's just not treated as repeat.
                bool hasPrefixValue = char.IsDigit(ch) || ch == '-' || ch == '+';
                int prefixValue = ExtractNumber(1);

                do {
                    ch = NextChar();
                    ++_charIndex;
                } while (char.IsWhiteSpace(ch));

                char formatChar = ch;

                // Valid format character?
                if ("IFEGLA:/()TXPSHDBJ$".IndexOf(formatChar) < 0) {
                    throw new JComRuntimeException(JComRuntimeErrors.FORMAT_ILLEGAL_CHARACTER,
                        $"Unknown format specifier character '{formatChar}'");
                }

                // End record?
                if (formatChar == ':') {
                    return null;
                }

                // Record separator?
                if (formatChar == '/') {
                    _scaleFactor = 0;
                    _lastRecord = new FormatRecord {
                        IsEndRecord = true
                    };
                    return _lastRecord;
                }

                // Group start
                if (formatChar == '(') {
                    if (prefixValue < 0) {
                        throw new JComRuntimeException(JComRuntimeErrors.FORMAT_ILLEGAL_REPEAT_VALUE);
                    }
                    FormatGroup formatGroup = new() {
                        GroupRepeat = prefixValue,
                        GroupStartIndex = _charIndex
                    };
                    _groups.Push(formatGroup);

                    // Also remember this format group start in the
                    // case of a rescan.
                    if (_groups.Count == 1) {
                        _lastFormatGroup = markedIndex;
                    }
                    continue;
                }

                // Group end
                if (formatChar == ')') {
                    FormatGroup formatGroup = (FormatGroup)_groups.Pop();
                    if (formatGroup == null) {
                        throw new JComRuntimeException(JComRuntimeErrors.FORMAT_PARENTHESIS_MISMATCH);
                    }
                    if (--formatGroup.GroupRepeat > 0) {
                        _charIndex = formatGroup.GroupStartIndex;
                        _groups.Push(formatGroup);
                    }
                    continue;
                }

                // Make sure a prefix value is specified for those format characters
                // that require one, and not specified for those that don't.
                if (formatChar == 'X' || formatChar == 'P'|| formatChar == 'H') {
                    if (!hasPrefixValue) {
                        throw new JComRuntimeException(JComRuntimeErrors.FORMAT_MISSING_VALUE,
                            $"'{formatChar}' specifier requires a value");
                    }
                } else {
                    if (hasPrefixValue && "IFEDGLA".IndexOf(formatChar) < 0) {
                        throw new JComRuntimeException(JComRuntimeErrors.FORMAT_INVALID_REPEAT_COUNT,
                            $"Repeat count not permitted with '{formatChar}' specifier");
                    }
                    if (prefixValue < 0) {
                        throw new JComRuntimeException(JComRuntimeErrors.FORMAT_ILLEGAL_REPEAT_VALUE);
                    }
                    _cRepeat = prefixValue;
                }

                // Handle cursor positioning. The following formats are recognised:
                //  T<n> - set the cursor position to offset <n> in the current record.
                //  TL<n> - move the cursor back <n> characters
                //  TR<n> - move the cursor forward <n> characters
                if (formatChar == 'T') {
                    _lastRecord = new FormatRecord {
                        FormatChar = 'T'
                    };
                    switch (NextChar()) {
                        case 'L':
                            ++_charIndex;
                            _lastRecord.Relative = true;
                            _lastRecord.Count = -ExtractNumber(0);
                            return _lastRecord;
                            
                        case 'R':
                            ++_charIndex;
                            _lastRecord.Relative = true;
                            _lastRecord.Count = ExtractNumber(0);
                            return _lastRecord;

                        default:
                            _lastRecord.Count = ExtractNumber(0);
                            return _lastRecord;
                    }
                }

                // Handle forward cursor movement. This is pretty much the
                // same as TR<n>.
                if (formatChar == 'X') {
                    _lastRecord = new FormatRecord {
                        FormatChar = 'T',
                        Relative = true,
                        Count = prefixValue
                    };
                    _cRepeat = 1;
                    return _lastRecord;
                }

                // Handle scale factor. This influences subsequent
                // formats on the same record.
                if (formatChar == 'P') {
                    _scaleFactor = prefixValue;
                    _cRepeat = 1;
                    continue;
                }

                // Handle blank specifier which controls whether blank
                // characters are ignored or treated as '0'.
                if (formatChar == 'B') {
                    BlanksAsZero = NextChar() == 'Z';
                    ++_charIndex;
                    continue;
                }

                // Handle field justification
                if (formatChar == 'J') {
                    _leftJustify = true;
                    continue;
                }

                // Handle carriage return suppression
                if (formatChar == '$') {
                    _suppressCarriage = true;
                    needLastRecord = true;
                    continue;
                }

                // Handle positive sign specification.
                if (formatChar == 'S') {
                    switch (NextChar()) {
                        case 'P':
                            ++_charIndex;
                            _plusRequired = FormatOptionalPlus.Always;
                            break;
                            
                        case 'S':
                            ++_charIndex;
                            _plusRequired = FormatOptionalPlus.Never;
                            break;
                            
                        default:
                            _plusRequired = FormatOptionalPlus.Default;
                            break;
                    }
                    continue;
                }

                // Hollerith character output
                // The prefix value is the count of subsequent characters in the format
                // string that are copied literally to the output.
                if (formatChar == 'H') {
                    while (prefixValue > 0 && _charIndex < _fmtLength) {
                        str.Append(NextChar());
                        --prefixValue;
                        ++_charIndex;
                    }
                    continue;
                }

                // If we get here then we're left with formatting characters that accept a
                // width and precision specifier. So parse those off.
                int precision = 1;
                int exponentWidth = 0;

                int fieldWidth = ExtractNumber(0);
                if (NextChar() == '.') {
                    ++_charIndex;
                    precision = ExtractNumber(0);
                    if ((formatChar == 'E' || formatChar == 'G') && NextChar() == 'E') {
                        ++_charIndex;
                        exponentWidth = ExtractNumber(2);
                    }
                }

                // We've got a full format specifier so return
                // that back to the caller.
                _lastRecord = new FormatRecord {
                    FormatChar = formatChar,
                    FieldWidth = fieldWidth,
                    Precision = precision,
                    Count = _cRepeat,
                    ExponentWidth = exponentWidth,
                    PlusRequired = _plusRequired,
                    ScaleFactor = _scaleFactor,
                    BlanksAsZero = BlanksAsZero,
                    LeftJustify = _leftJustify,
                    SuppressCarriage = _suppressCarriage
                };
                return _lastRecord;
            }
        }
        if (str.Length > 0 || needLastRecord) {
            _lastRecord = new FormatRecord {
                RawString = str.ToString(),
                SuppressCarriage = _suppressCarriage
            };
            return _lastRecord;
        }
        if (_groups.Count > 0) {
            throw new JComRuntimeException(JComRuntimeErrors.FORMAT_UNCLOSED_GROUP);
        }
        return null;
    }

    // Extract a number from the formatString starting at _charIndex. If
    // no number is detected, return defaultValue otherwise return the
    // number extracted. On return _charIndex is pointing at the first
    // non digit after the number.
    private int ExtractNumber(int defaultValue) {
        int value = defaultValue;
        int sign = 1;

        char ch = NextChar();

        if (ch == '-') {
            sign = -1;
            ++_charIndex;
            ch = NextChar();
        }
        if (ch == '+') {
            ++_charIndex;
            ch = NextChar();
        }
        if (char.IsDigit(ch)) {
            value = 0;
            do {
                value = value * 10 + (ch - '0');
                ++_charIndex;
                ch = NextChar();
            } while (char.IsDigit(ch));
        }
        return value * sign;
    }

    // Return the next character from the format string or NUL if
    // we got to the end.
    private char NextChar() {
        char ch = '\0';
        if (_charIndex < _fmtLength) {
            ch = _formatString[_charIndex];
        }
        return ch;
    }
}
