// JEdit
// Buffer management
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

using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using JEdit.Resources;

namespace JEdit;

public class Buffer {
    private readonly LineTerminator _terminatorType;
    private List<string> _lines = new();
    private FileInfo? _fileInfo;

    /// <summary>
    /// Create a new empty buffer not associated with any file.
    /// </summary>
    public Buffer() {
        NewFile = true;
    }

    /// <summary>
    /// Creates a buffer with the specified file. The end of each line is represented
    /// by a single linefeed (ASCII 10) character for consistency but we determine and
    /// record the detected line terminator in the original file so that we can write
    /// out the correct terminator when the buffer is saved.
    /// </summary>
    /// <param name="filename">Name of file</param>
    public Buffer(string filename) {

        if (!string.IsNullOrEmpty(filename) && File.Exists(filename)) {
            string allText = File.ReadAllText(filename);
            _terminatorType = DetermineLineTerminator(allText);
            Content = allText;
        } else {
            _terminatorType = DefaultLineTerminator();
            Content = Consts.EndOfLine.ToString();
            NewFile = true;
        }
        Filename = filename;
    }

    /// <summary>
    /// Return the name of the buffer. This is the base part of the filename if
    /// there is one, or the New File string otherwise.
    /// </summary>
    public string Name => _fileInfo == null ? Edit.NewFile : _fileInfo.Name;

    /// <summary>
    /// Get or set the content of the buffer to the specified string. All existing
    /// content is replaced, even if modified.
    /// </summary>
    public string Content {
        set {
            _lines = SplitLines(value).Select(line => line).ToList();
            Invalidate(0, 0, _lines[^1].Length - 1, _lines.Count - 1);
        }
        get {
            string lineTerminator = _terminatorType switch {
                LineTerminator.CR => "\r",
                LineTerminator.LF => "\n",
                LineTerminator.CRLF => "\r\n",
                _ => Consts.EndOfLine.ToString()
            };
            StringBuilder lines = new StringBuilder();
            foreach (string line in _lines) {
                lines.Append(line.Replace(Consts.EndOfLine.ToString(), lineTerminator));
            }
            return lines.ToString();
        }
    }

    /// <summary>
    /// Return the extent of the block that has been invalidated by
    /// recent editing actions.
    /// </summary>
    public Extent InvalidateExtent { get; } = new();

    /// <summary>
    /// Return the fully qualified file name with path.
    /// </summary>
    public string Filename {
        get => _fileInfo == null ? string.Empty : _fileInfo.FullName;
        set => _fileInfo = string.IsNullOrEmpty(value) ? null : new FileInfo(value);
    }

    /// <summary>
    /// Whether the buffer has been modified since it was last
    /// changed.
    /// </summary>
    public bool Modified { get; private set; }

    /// <summary>
    /// Is this a new file (i.e. the file has a name but does not
    /// currently exist on disk).
    /// </summary>
    public bool NewFile { get; private set; }

    /// <summary>
    /// Number of lines in the buffer
    /// </summary>
    public int Length => _lines.Count;

    /// <summary>
    /// Return whether the cursor is at the start of the buffer
    /// </summary>
    public bool AtStartOfBuffer => Offset == 0 && LineIndex == 0;

    /// <summary>
    /// Return whether the cursor is at the end of the buffer
    /// </summary>
    private bool AtEndOfBuffer => Offset == _lines[^1].Length - 1 && LineIndex == _lines.Count - 1;

    /// <summary>
    /// Index of line being edited
    /// </summary>
    public int LineIndex { get; set; }

    /// <summary>
    /// Offset on line being edited
    /// </summary>
    public int Offset { get; set; }

    /// <summary>
    /// Return the current buffer cursor position.
    /// </summary>
    public Point Cursor => new(Offset, LineIndex);

    /// <summary>
    /// Retrieve the line at the specified index.
    /// </summary>
    /// <param name="index">Zero based index</param>
    /// <returns>Line, or null if the index is out of range</returns>
    public string GetLine(int index) {
        return index >= 0 && index < _lines.Count ? _lines[index] : string.Empty;
    }

    /// <summary>
    /// Replace the character at the current offset.
    /// </summary>
    /// <param name="ch">Character to replace</param>
    public void Replace(char ch) {
        StringBuilder line = PrepareLine();
        if (line[Offset] == Consts.EndOfLine) {
            line.Insert(Offset++, ch);
        }
        else {
            line[Offset++] = ch;
        }
        _lines[LineIndex] = line.ToString();
        Invalidate(Offset, LineIndex, Offset, LineIndex);
        Modified = true;
    }

    /// <summary>
    /// Insert the specified character at the current offset.
    /// </summary>
    /// <param name="ch">Character to insert</param>
    public void Insert(char ch) {
        StringBuilder line = PrepareLine();
        if (ch == Consts.EndOfLine) {
            string newLine = line.ToString(Offset, line.Length - Offset);
            _lines[LineIndex] = line.ToString(0, Offset) + Consts.EndOfLine;
            _lines.Insert(LineIndex + 1, newLine);
            Invalidate(0, LineIndex, _lines[^1].Length - 1, _lines.Count - 1);
            Offset = 0;
            LineIndex++;
        }
        else {
            line.Insert(Offset++, ch);
            _lines[LineIndex] = line.ToString();
            Invalidate(0, LineIndex, Offset, LineIndex);
        }
        Modified = true;
    }

    /// <summary>
    /// Insert the specified text at the current offset. Currently we do
    /// it the slow way using the Insert character method. Ideally we
    /// should do this more intelligently, such as treat Insert(char) as
    /// Insert(text[0]) without losing single char performance.
    /// </summary>
    /// <param name="text">Text to insert</param>
    public void Insert(string text) {
        foreach (char ch in text) {
            Insert(ch);
        }
    }

    /// <summary>
    /// Get the specified number of characters of text from the
    /// current cursor position.
    /// </summary>
    /// <param name="count">Number of characters to retrieve</param>
    /// <returns>A string containing the retrieved text</returns>
    public string GetText(int count) {
        StringBuilder text = new();
        int offset = Offset;
        int index = LineIndex;
        while (count > 0) {
            string line = GetLine(index++);
            int length = Math.Min(count, line.Length - offset);
            text.Append(line.AsSpan(offset, length));
            offset = 0;
            count -= length;
        }
        return text.ToString();
    }

    /// <summary>
    /// Delete the given number of characters from the current offset
    /// without moving the offset. If count is greater than the number
    /// of characters between the offset and the end of the buffer then
    /// it stops when it reaches the end of the buffer.
    /// </summary>
    /// <param name="count">The number of characters to delete</param>
    public void Delete(int count) {

        StringBuilder line = PrepareLine();
        int depth = 0;
        while (!AtEndOfBuffer && count-- > 0) {
            char ch = line[Offset];
            line.Remove(Offset, 1);
            if (ch == Consts.EndOfLine) {
                if (LineIndex + 1 == _lines.Count) {
                    break;
                }
                line.Append(_lines[LineIndex + 1]);
                _lines.RemoveAt(LineIndex + 1);
                depth = _lines.Count - LineIndex;
            }
        }
        Invalidate(0, LineIndex, Offset, LineIndex + depth);
        _lines[LineIndex] = line.ToString();
        Modified = true;
    }

    /// <summary>
    /// Write the buffer back to disk.
    /// </summary>
    public void Write() {
        if (Screen.Config.BackupFile && File.Exists(Filename)) {
            string backupFile = Filename + Consts.BackupExtension;
            File.Delete(backupFile);
            File.Copy(Filename, backupFile);
        }
        File.WriteAllText(Filename, Content);
        Modified = false;
        NewFile = false;
    }

    /// <summary>
    /// Return the current line ready for any editing actions. If the offset
    /// is in the virtual space then we pad the end of the line out to the
    /// offset with physical spaces.
    /// </summary>
    /// <returns>A StringBuilder object initialised with the line contents</returns>
    private StringBuilder PrepareLine() {
        StringBuilder line = new StringBuilder(_lines[LineIndex]);
        if (Offset >= line.Length) {
            int count = Math.Max(0, line.Length - 1);
            while (count < Offset) {
                line.Insert(count++, ' ');
            }
        }
        return line;
    }

    /// <summary>
    /// Add the specified area to the invalidate extent.
    /// </summary>
    private void Invalidate(int x1, int y1, int x2, int y2) {
        InvalidateExtent.Add(new Point(x1, y1));
        InvalidateExtent.Add(new Point(x2, y2));
    }

    /// <summary>
    /// Determine the default line terminator for the system.
    /// </summary>
    /// <returns>The default line terminator for the system</returns>
    private static LineTerminator DefaultLineTerminator() {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? LineTerminator.CRLF : LineTerminator.LF;
    }

    /// <summary>
    /// Determine the line terminator type by scanning for newlines in the
    /// text. If multiple terminators are found, use the first one.
    /// </summary>
    /// <param name="text">Text to analyse for the line terminator</param>
    private static LineTerminator DetermineLineTerminator(string text) {

        LineTerminator terminator = LineTerminator.NONE;
        foreach (char ch in text) {
            if (ch == '\n') {
                if (terminator == LineTerminator.CR) {
                    terminator = LineTerminator.CRLF;
                    break;
                }
                terminator = LineTerminator.LF;
                break;
            }
            if (ch == '\r') {
                terminator = LineTerminator.CR;
                continue;
            }
            if (terminator != LineTerminator.NONE) {
                break;
            }
        }
        return terminator;
    }

    /// <summary>
    /// Splits the given text into a series of lines, each line terminator
    /// replaced by the single EOL character.
    /// </summary>
    /// <param name="text">Text to split</param>
    /// <returns>An enumerable array of split lines</returns>
    private static IEnumerable<string> SplitLines(string text) {
        int start = 0;
        int index;

        while ((index = text.IndexOfAny(new[] { '\r', '\n' }, start)) != -1) {
            if (index - start >= 0) {
                int eolIndex = index;
                if (text[index] == '\r' && index < text.Length - 2 && text[index + 1] == '\n') {
                    ++index;
                }
                yield return text.Substring(start, eolIndex - start) + Consts.EndOfLine;
            }
            start = index + 1;
        }
        if (start < text.Length) {
            yield return text[start..];
        }
    }
}

