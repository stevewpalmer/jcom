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

using System.Runtime.InteropServices;

namespace JEdit;

public class Buffer {
    private readonly LineTerminator _terminatorType;
    private List<string> _lines;

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
            _lines = new List<string> {Consts.EndOfLine.ToString()};
            NewFile = true;
        }
        Filename = filename ?? string.Empty;
    }

    /// <summary>
    /// Get or set the content of the buffer to the specified string. All existing
    /// content is replaced, even if modified.
    /// </summary>
    public string Content {
        set {
            _lines = value
                .Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
                .Select(l => l + Consts.EndOfLine)
                .ToList();
        }
        get {
            string lineTerminator = _terminatorType switch {
                LineTerminator.CR => "\r",
                LineTerminator.LF => "\n",
                LineTerminator.CRLF => "\r\n",
                _ => ""
            };
            return string.Join(lineTerminator, _lines);
        }
    }

    /// <summary>
    /// Return the fully qualified file name with path.
    /// </summary>
    public string FullFilename {
        get {
            if (string.IsNullOrEmpty(Filename)) {
                return string.Empty;
            }
            FileInfo fileInfo = new FileInfo(Filename);
            return fileInfo.FullName;
        }
    }

    /// <summary>
    /// Return the base part of the filename.
    /// </summary>
    public string BaseFilename => string.IsNullOrEmpty(Filename) ? "New File" : GetBaseFilename(Filename);

    /// <summary>
    /// Return the base filename given a fully or partially qualified
    /// filename.
    /// </summary>
    /// <param name="filename">Input filename</param>
    /// <returns>Base filename</returns>
    public static string GetBaseFilename(string filename) {
        FileInfo fileInfo = new FileInfo(filename);
        return fileInfo.Name;
    }
    
    /// <summary>
    /// Return the full filename given a fully or partially qualified
    /// filename.
    /// </summary>
    /// <param name="filename">Input filename</param>
    /// <returns>Full filename</returns>
    public static string GetFullFilename(string filename) {
        FileInfo fileInfo = new FileInfo(filename);
        return fileInfo.FullName;
    }

    /// <summary>
    /// Name of file associated with buffer, or empty string
    /// if the buffer has not yet been saved to a file
    /// </summary>
    public string Filename { get; set; }

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
    /// Index of line being edited
    /// </summary>
    public int LineIndex { get; set; }

    /// <summary>
    /// Offset on line being edited
    /// </summary>
    public int Offset { get; set; }

    /// <summary>
    /// Retrieve the line at the specified index.
    /// </summary>
    /// <param name="index">Zero based index</param>
    /// <returns>Line, or null if the index is out of range</returns>
    public string GetLine(int index) {
        return index >= 0 && index < _lines.Count ? _lines[index] : null;
    }

    /// <summary>
    /// Write the buffer back to disk.
    /// </summary>
    public void Write() {
        File.WriteAllText(FullFilename, Content);
        Modified = false;
        NewFile = false;
    }

    /// <summary>
    /// Determine the default line terminator for the system.
    /// </summary>
    private static LineTerminator DefaultLineTerminator() {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? LineTerminator.CRLF : LineTerminator.LF;
    }

    /// <summary>
    /// Determine the line terminator type by scanning for newlines in the
    /// text. If multiple terminators are found, use the first one.
    /// </summary>
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
}

