// JCom Compiler Toolkit
// CodeGeneratorException class
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

using System.Runtime.Serialization;

namespace CCompiler;

/// <summary>
/// Defines a CodeGeneratorException for exceptions found
/// during code generation.
/// </summary>
[Serializable]
public class CodeGeneratorException : Exception {

    private int _linenumber;
    private string _filename;

    /// <summary>
    /// Constructs an empty <c>CodeGeneratorException</c> instance.
    /// </summary>
    public CodeGeneratorException() {
        Linenumber = -1;
    }

    /// <summary>
    /// Constructs an <c>CodeGeneratorException</c> instance with
    /// the specified exception message.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public CodeGeneratorException(string message) : base(message) {
        Linenumber = -1;
    }

    /// <summary>
    /// Constructs an <c>CodeGeneratorException</c> instance with
    /// the specified exception message.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception object that yielded this exception.</param>
    public CodeGeneratorException(string message, Exception innerException) : base(message, innerException) {
        Linenumber = -1;
    }

    /// <summary>
    /// Constructs an <c>CodeGeneratorException</c> instance with
    /// the specified exception message.
    /// </summary>
    /// <param name="info">A SerializationInfo object.</param>
    /// <param name="context">A StreamingContext object.</param>
    protected CodeGeneratorException(SerializationInfo info, StreamingContext context) : base(info, context) {
        Linenumber = -1;
    }

    /// <summary>
    /// Constructs a <c>CodeGeneratorException</c> instance.
    /// </summary>
    /// <param name="line">Line number</param>
    /// <param name="filename">Filename</param>
    /// <param name="message">Message</param>
    public CodeGeneratorException(int line, string filename, string message) : base(message) {
        Linenumber = line;
        Filename = filename;
    }

    /// <summary>
    /// Gets the line number at which the exception
    /// occurred.
    /// </summary>
    /// <value>A source code line number</value>
    public int Linenumber {
        get => _linenumber;
        set => _linenumber = value;
    }

    /// <summary>
    /// Gets the source file name at which the exception
    /// occurred.
    /// </summary>
    /// <value>The filename.</value>
    public string Filename {
        get => _filename;
        set => _filename = value;
    }

    /// <summary>
    //  Override GetObjectData to serialise the linenumber and filename objects.
    /// </summary>
    /// <param name="info">Info.</param>
    /// <param name="context">Context.</param>
    public override void GetObjectData(SerializationInfo info, StreamingContext context) {
        if (info == null) {
            throw new ArgumentNullException(nameof(info));
        }
        info.AddValue("_linenumber", _linenumber);
        info.AddValue("_filename", _filename);
        base.GetObjectData(info, context);
    }
}