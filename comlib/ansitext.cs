// Jcom Runtime Libary
// ANSI text support
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

namespace JComLib;

public class AnsiText {

    /// <summary>
    /// Empty AnsiText string
    /// </summary>
    public AnsiText(IEnumerable<AnsiTextSpan> spans) {
        Spans = spans.ToList();
    }

    /// <summary>
    /// List of spans
    /// </summary>
    public List<AnsiTextSpan> Spans { get; private set; }

    /// <summary>
    /// Length of the rendered text
    /// </summary>
    public int Length => Spans.Sum(span => span.Length);

    /// <summary>
    /// Return the raw text
    /// </summary>
    public string Text => string.Join("", Spans.Select(span => span.Text));

    /// <summary>
    /// Return the Ansi text
    /// </summary>
    public string EscapedText => string.Join("", Spans.Select(span => span.EscapedText()));

    /// <summary>
    /// Return a substring of an AnsiText string starting from start
    /// and for the length number of characters. If start is outside
    /// the string length, an empty string is returned. If start plus
    /// length is longer than the string length, the string is
    /// truncated.
    /// </summary>
    /// <param name="start">Zero based start index</param>
    /// <param name="length">Length required</param>
    /// <returns></returns>
    public AnsiText Substring(int start, int length) {
        int spanIndex = 0;
        List<AnsiTextSpan> spans = [];
        while (spanIndex < Spans.Count && Spans[spanIndex].Text.Length <= start) {
            start -= Spans[spanIndex++].Text.Length;
        }
        while (spanIndex < Spans.Count && length > 0) {
            int textLength = Spans[spanIndex].Text.Length;
            int spanWidth = Math.Min(Math.Min(textLength, length), textLength - start);
            AnsiTextSpan co = new(Spans[spanIndex]) {
                Text = Spans[spanIndex].Text.Substring(start, spanWidth),
                Width = Math.Min(Spans[spanIndex].Width, length)
            };
            spans.Add(co);
            length -= co.Length;
            start = 0;
            spanIndex++;
        }
        if (spans.Count == 0) {
            spans.Add(new AnsiTextSpan(string.Empty));
        }
        return new AnsiText(spans);
    }

    /// <summary>
    /// Style a portion of the AnsiText.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="length"></param>
    /// <param name="fg"></param>
    /// <param name="bg"></param>
    public void Style(int start, int length, int fg, int bg) {
        AnsiText preStyle = Substring(0, start);
        AnsiText newStyle = Substring(start, length);
        AnsiText postStyle = Substring(start + length, Length);

        List<AnsiTextSpan> newSpans = [];
        if (preStyle.Length > 0) {
            newSpans.AddRange(preStyle.Spans);
        }
        if (newStyle.Length > 0) {
            newSpans.AddRange(newStyle.Spans.Select(n => new AnsiTextSpan(n) {
                ForegroundColour = fg,
                BackgroundColour = bg
            }));
        }
        if (postStyle.Length > 0) {
            newSpans.AddRange(postStyle.Spans);
        }
        Spans = newSpans;
    }

}