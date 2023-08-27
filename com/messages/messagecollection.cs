// JCom Compiler Toolkit
// Compiler messages
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
using System.Diagnostics;

namespace CCompiler; 

/// <summary>
/// A collection of all compiler issued messages including errors, warnings and
/// informational messages.
/// </summary>
public class MessageCollection : IEnumerable<Message> {
    private readonly List<Message> _messages = new();
    private readonly Options _opts;

    /// <summary>
    /// Constructs a message collection using the given options.
    /// </summary>
    /// <param name="opts">Options</param>
    public MessageCollection(Options opts) {
        _opts = opts;
        Linenumber = -1;
        Interactive = false;
    }

    /// <value>
    /// Sets or retrieves the default filename for use by the shorter ErrorAtLine and
    /// WarningAtLine methods.
    /// </value>
    public bool Interactive { get; set; }

    /// <value>
    /// Sets or retrieves the default filename for use by the shorter ErrorAtLine and
    /// WarningAtLine methods.
    /// </value>
    public string Filename { get; set; }

    /// <value>
    /// Sets or retrieves the current line number reported in messages.
    /// </value>
    public int Linenumber { get; set; }

    /// <summary>
    /// Clear the messages list.
    /// </summary>
    public void Clear() {
        _messages.Clear();
        Linenumber = 0;
        ErrorCount = 0;
    }

    /// <summary>
    /// Adds the specified informational string to the message list.
    /// </summary>
    /// <param name="str">The string to write</param>
    public void Info(string str) {
        _messages.Add(new Message(null, MessageLevel.Info, MessageCode.NONE, Linenumber, str));
    }

    /// <summary>
    /// Adds the specified error to the message list with the given code and
    /// increments the internal count of errors found.
    /// </summary>
    /// <param name="code">The error code</param>
    /// <param name="str">The error string to write</param>
    public void Error(MessageCode code, string str) {
        Error(Filename, code, Linenumber, str);
    }

    /// <summary>
    /// Adds the specified error to the message list with the given code and
    /// line number and increments the internal count of errors found.
    /// </summary>
    /// <param name="code">The error code</param>
    /// <param name="line">The line number at which the error was detected</param>
    /// <param name="str">The error string to write</param>
    public void Error(MessageCode code, int line, string str) {
        Error(Filename, code, line, str);
    }

    /// <summary>
    /// Adds the specified error to the message list with the given filename,
    /// code, line number and increments the internal count of errors found.
    /// </summary>
    /// <param name="filename">An optional filename</param>
    /// <param name="code">The error code</param>
    /// <param name="line">The line number at which the error was detected</param>
    /// <param name="str">The error string to write</param>
    public void Error(string filename, MessageCode code, int line, string str) {
        Debug.Assert((int)code >= 0x1000);
        if (Interactive) {
            string errorString = str;
            if (line > 0) {
                errorString += " in line " + line;
            }
            throw new ApplicationException(errorString);
        }
        if (ErrorCount == 100) {
            _messages.Add(new Message(filename,
                                      MessageLevel.Error,
                                      MessageCode.TOOMANYCONTINUATION,
                                      line,
                                      "Too many errors. Compilation stopped"));
            throw new ApplicationException();
        }
        _messages.Add(new Message(filename, MessageLevel.Error, code, line, str));
        ++ErrorCount;
    }

    /// <summary>
    /// Adds the specified warning to the message list.
    /// </summary>
    /// <param name="code">The warning code</param>
    /// <param name="level">The warning level</param>
    /// <param name="str">The warning string to write</param>
    public void Warning(MessageCode code, int level, string str) {
        Warning(Filename, code, level, Linenumber, str);
    }

    /// <summary>
    /// Adds the specified warning to the message list with the given line number.
    /// </summary>
    /// <param name="code">The warning code</param>
    /// <param name="level">The warning level</param>
    /// <param name="line">The line number at which the warning was detected</param>
    /// <param name="str">The warning string to write</param>
    public void Warning(MessageCode code, int level, int line, string str) {
        Warning(Filename, code, level, line, str);
    }

    /// <summary>
    /// Adds the specified warning to the message list with the given line number. If
    /// the warning level is above the minimum then it is simply ignored. If warnings
    /// are treated as errors, the warning is promoted to an error.
    /// </summary>
    /// <param name="filename">An optional filename</param>
    /// <param name="code">The error code</param>
    /// <param name="level">The warning level</param>
    /// <param name="line">The line number at which the error was detected</param>
    /// <param name="str">The error string to write</param>
    public void Warning(string filename, MessageCode code, int level, int line, string str) {
        if (_opts.WarnAsError) {
            Error(filename, code, line, str);
        } else {
            if (level <= _opts.WarnLevel) {
                Debug.Assert((int)code >= 0x1000);
                _messages.Add(new Message(filename, MessageLevel.Warning, code, line, str));
            }
        }
    }

    /// <value>
    /// Returns the count of errors found so far.
    /// </value>
    public int ErrorCount { get; private set; }

    /// <value>
    /// Return the count of messages.
    /// </value>
    public int Count => _messages.Count;

    /// <value>
    /// Returns the message at the specified index.
    /// </value>
    /// <param name="index">A zero based index</param>
    public Message this[int index] => _messages[index];

    /// <summary>
    /// Enumerator for all messages.
    /// </summary>
    /// <returns>The enumerator.</returns>
    public IEnumerator<Message> GetEnumerator() {
        _messages.Sort(delegate (Message t1, Message t2) { return t1.Line - t2.Line; });
        for (int i = 0; i < _messages.Count; i++) {
            yield return _messages[i];
        }
    }

    // Private enumerator required for class compliance.
    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }
}
