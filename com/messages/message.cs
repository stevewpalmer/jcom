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

using System.Text;

namespace CCompiler;

/// <summary>
/// Represents a single compiler message.
/// </summary>
public class Message {

    /// <summary>
    /// Constructs a single message object.
    /// </summary>
    /// <param name="filename">Optional filename</param>
    /// <param name="level">Required message level</param>
    /// <param name="code">Optional numeric code identifying this message</param>
    /// <param name="line">Optional source code line number for the message</param>
    /// <param name="text">The required actual text of the message</param>
    public Message(string filename, MessageLevel level, MessageCode code, int line, string text) {
        Filename = filename;
        Level = level;
        Code = code;
        Line = line;
        Text = text;
    }

    /// <summary>
    /// Returns the message formatted for output.
    /// </summary>
    /// <returns></returns>
    public override string ToString() {
        StringBuilder str = new();
        if (Level == MessageLevel.Info) {
            str.Append(Text);
        }
        else {
            if (Filename != null) {
                str.Append(Filename);
            }
            if (Line != -1) {
                str.Append($"({Line}):");
            }
            if (Code != MessageCode.NONE) {
                if (str.Length > 0) {
                    str.Append(' ');
                }
                str.Append($"{Level} C{(int)Code}: ");
            }
            str.Append(Text);
        }
        return str.ToString();
    }

    /// <value>
    /// Returns the name of the file associated with this message. May be null.
    /// </value>
    public string Filename { get; }

    /// <value>
    /// Returns the level of this message as a <c>MessageLevel</c> type.
    /// </value>
    public MessageLevel Level { get; }

    /// <value>
    /// Returns the code of this message, or MessageCode.NONE if no code is associated.
    /// </value>
    public MessageCode Code { get; }

    /// <value>
    /// Returns the source code line number of this message.
    /// </value>
    public int Line { get; }

    /// <value>
    /// Returns the text of the message.
    /// </value>
    public string Text { get; }
}