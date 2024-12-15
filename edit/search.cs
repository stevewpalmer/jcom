// JEdit
// Search and translate
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

using System.Text.RegularExpressions;

namespace JEdit;

public class Search {
    private readonly Buffer _buffer;

    /// <summary>
    /// Search constructor with a specific buffer.
    /// </summary>
    /// <param name="buffer"></param>
    public Search(Buffer buffer) {
        _buffer = buffer;
        Row = _buffer.LineIndex;
        Column = _buffer.Offset - 1;
    }

    /// <summary>
    /// Row of match
    /// </summary>
    public int Row { get; private set; }

    /// <summary>
    /// Column of match
    /// </summary>
    public int Column { get; private set; }

    /// <summary>
    /// Search string
    /// </summary>
    public string SearchString { get; init; } = "";

    /// <summary>
    /// True if the search is case insensitive.
    /// </summary>
    public bool CaseInsensitive { get; init; }

    /// <summary>
    /// True if we search forward in the buffer, false
    /// if we search backward.
    /// </summary>
    public bool Forward { get; init; }

    /// <summary>
    /// True if we perform a regular expression search.
    /// </summary>
    public bool RegExp { get; init; }

    /// <summary>
    /// Replacement string if we're translating.
    /// </summary>
    public string ReplacementString { get; init; } = "";

    /// <summary>
    /// Count of translations performed.
    /// </summary>
    public int TranslateCount { get; set; }

    /// <summary>
    /// Whether the most recent match succeeded.
    /// </summary>
    public bool MatchSuccess { get; private set; }

    /// <summary>
    /// Length of matched text. For RegExp searches, this may be
    /// variable with each match. Otherwise it will be the length of
    /// the search string.
    /// </summary>
    public int MatchLength { get; private set; }

    /// <summary>
    /// Return the next instance of the search string in the buffer. If a match
    /// is found then matchPoint is set to the (column, row) of the matching string
    /// in offset and lineIndex coordinates, and the function returns true. If no
    /// match is found then the function returns false.
    /// </summary>
    public bool Next() {
        return Forward ? NextForward() : NextBack();
    }

    /// <summary>
    /// Find the next occurrence of the search string when searching
    /// forward through the file.
    /// </summary>
    /// <returns>True if we matched the search string, false otherwise</returns>
    private bool NextForward() {
        MatchSuccess = false;
        while (Row < _buffer.Length) {
            string line = _buffer.GetLine(Row);
            if (++Column == line.Length) {
                Column = 0;
                ++Row;
                continue;
            }
            int matchIndex;
            if (RegExp) {
                RegexOptions options = CaseInsensitive ? RegexOptions.IgnoreCase : RegexOptions.None;
                Regex regex = new(SearchString, options);
                Match m = regex.Match(line, Column);
                matchIndex = m.Success ? m.Index : -1;
                MatchLength = m.Length;
            }
            else {
                StringComparison type = CaseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                matchIndex = line.IndexOf(SearchString, Column, type);
                MatchLength = SearchString.Length;
            }
            if (matchIndex >= 0) {
                Column = matchIndex;
                MatchSuccess = true;
                break;
            }
            ++Row;
            Column = -1;
        }
        return MatchSuccess;
    }

    /// <summary>
    /// Locate the next occurrence of the search string when searching
    /// back through the file.
    /// </summary>
    private bool NextBack() {
        MatchSuccess = false;
        while (Row >= 0) {
            string line = _buffer.GetLine(Row);
            if (Column == -1) {
                Column = line.Length;
            }
            if (--Column == -1) {
                --Row;
                Column = -1;
                continue;
            }
            int matchIndex;
            if (RegExp) {
                RegexOptions options = CaseInsensitive ? RegexOptions.IgnoreCase : RegexOptions.None;
                Regex regex = new(SearchString, options | RegexOptions.RightToLeft);
                Match m = regex.Match(line, 0, Column);
                matchIndex = m.Success ? m.Index : -1;
                MatchLength = m.Length;
            }
            else {
                StringComparison type = CaseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                matchIndex = line.LastIndexOf(SearchString, Column, Column + 1, type);
                MatchLength = SearchString.Length;
            }
            if (matchIndex >= 0) {
                Column = matchIndex;
                MatchSuccess = true;
                break;
            }
            --Row;
            Column = -1;
        }
        return MatchSuccess;
    }
}