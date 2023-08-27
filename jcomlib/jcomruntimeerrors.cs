// JCom Compiler Toolkit
// JComRuntimeException class
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

using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace JComLib; 

/// <summary>
/// Defines the type of run time exception encountered.
/// </summary>
public enum JComRuntimeErrors {

    /// <summary>
    /// File cannot be opened
    /// </summary>
    [Description("Cannot Open File")]
    CANNOT_OPEN_FILE = 1,

    /// <summary>
    /// File already open
    /// </summary>
    [Description("File Already Open")]
    FILE_ALREADY_OPEN = 2,

    /// <summary>
    /// File not open
    /// </summary>
    [Description("File Not Open")]
    FILE_NOT_OPEN = 3,

    /// <summary>
    /// Division by zero
    /// </summary>
    [Description("Division By Zero")]
    DIVISION_BY_ZERO = 4,

    /// <summary>
    /// Unexpected end of file
    /// </summary>
    [Description("Unexpected End Of File On Read")]
    UNEXPECTED_END_OF_FILE = 5,

    /// <summary>
    /// General I/O error on read
    /// </summary>
    [Description("Unexpected I/O Error On Read")]
    IO_READ_ERROR = 6,

    /// <summary>
    /// General I/O error on write
    /// </summary>
    [Description("Unexpected I/O Error On Write")]
    IO_WRITE_ERROR = 7,

    /// <summary>
    /// Index out of range
    /// </summary>
    [Description("Array Index Out Of Range")]
    INDEX_OUT_OF_RANGE = 8,

    /// <summary>
    /// File cannot be created
    /// </summary>
    [Description("Cannot Create File")]
    CANNOT_CREATE_FILE = 9,

    /// <summary>
    /// File is not open for random access
    /// </summary>
    [Description("File was not opened for RANDOM access")]
    FILE_NOT_OPEN_FOR_RANDOM_ACCESS = 10,

    /// <summary>
    /// Zone value incorrect
    /// </summary>
    [Description("Zone value is negative")]
    ZONE_VALUE_INCORRECT = 11,

    /// <summary>
    /// File already open for random access
    /// </summary>
    [Description("File already opened for RANDOM access")]
    FILE_OPEN_FOR_RANDOM_ACCESS = 12,

    /// <summary>
    /// Format record mismatch (Fortran)
    /// </summary>
    [Description("Format Record Mismatch")]
    FORMAT_RECORD_MISMATCH = 100,

    /// <summary>
    /// Illegal format character (Fortran)
    /// </summary>
    [Description("Illegal Character in Format Specifier")]
    FORMAT_ILLEGAL_CHARACTER = 101,

    /// <summary>
    /// Invalid repeat count in format (Fortran)
    /// </summary>
    [Description("Repeat Count Cannot Be Less Than 0")]
    FORMAT_ILLEGAL_REPEAT_VALUE = 102,

    /// <summary>
    /// Mismatch in parenthesis in format record (Fortran)
    /// </summary>
    [Description("Parenthesis Mismatch In Format Specifier")]
    FORMAT_PARENTHESIS_MISMATCH = 103,

    /// <summary>
    /// Repeat count not valid in context (Fortran)
    /// </summary>
    [Description("Repeat Count Not Permitted Here")]
    FORMAT_INVALID_REPEAT_COUNT = 104,

    /// <summary>
    /// Unclosed group in format statement (Fortran)
    /// </summary>
    [Description("Unclosed Format Specifier Group")]
    FORMAT_UNCLOSED_GROUP = 105,

    /// <summary>
    /// Format specifier is missing a mandatory value
    /// </summary>
    [Description("Format Specifier Requires Value")]
    FORMAT_MISSING_VALUE = 106,

    /// <summary>
    /// Invalid number specification (Fortran)
    /// </summary>
    [Description("Invalid Number In Format Specifier")]
    FORMAT_INVALID_NUMBER = 107,

    /// <summary>
    /// Format character not valid for Complex data type
    /// </summary>
    [Description("Invalid Format Specifier for Complex Data Type")]
    FORMAT_INVALID_FOR_COMPLEX = 108,

    /// <summary>
    /// Multiple repeat in format specifier.
    /// </summary>
    [Description("Multiple repeat specifiers")]
    FORMAT_MULTIPLE_REPEAT = 109,

    /// <summary>
    /// System error
    /// </summary>
    SYSTEM_ERROR = 9999
}

/// <summary>
/// Defines the type of run time exception encountered.
/// </summary>
public enum JComRuntimeExceptionType {

    /// <summary>
    /// Application issued an END statement
    /// </summary>
    END = 0,

    /// <summary>
    /// Application issued a STOP statement
    /// </summary>
    STOP,

    /// <summary>
    /// Application experienced a run-time failure
    /// </summary>
    FAILURE
}

/// <summary>
/// Defines a JComRuntimeException for exceptions found
/// during JCom runtime.
/// </summary>
[Serializable]
[ComVisible (true)]
public class JComRuntimeException : Exception {

    /// <summary>
    /// Exception type
    /// </summary>
    public JComRuntimeExceptionType Type { get; set; }

    /// <summary>
    /// Line number at which exception occurred
    /// </summary>
    public int LineNumber { get; set; }

    /// <summary>
    /// Optional run-time exception error code
    /// </summary>
    public JComRuntimeErrors ErrorCode { get; set; }

    /// <summary>
    /// Constructs an empty JComRuntimeException instance.
    /// </summary>
    public JComRuntimeException() : 
        base("The requested operation caused a JCom runtime exception") {
    }

    /// <summary>
    /// Constructs an JComRuntimeException> instance with
    /// the specified error code.
    /// </summary>
    /// <param name="errorCode">The error code.</param>
    public JComRuntimeException(JComRuntimeErrors errorCode) : base(GetEnumDescription(errorCode)) {
        ErrorCode = errorCode;
        Type = JComRuntimeExceptionType.FAILURE;
    }

    /// <summary>
    /// Constructs an JComRuntimeException> instance with
    /// the specified error code and message.
    /// </summary>
    /// <param name="errorCode">The error code.</param>
    /// <param name="message">The exception message.</param>
    public JComRuntimeException(JComRuntimeErrors errorCode, string message) : base(message) {
        ErrorCode = errorCode;
        Type = JComRuntimeExceptionType.FAILURE;
    }

    /// <summary>
    /// Constructs an JComRuntimeException> instance with
    /// the specified exception message.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public JComRuntimeException(string message) : base(message) {
    }

    /// <summary>
    /// Constructs an JComRuntimeException> instance with
    /// the specified exception message.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception object that yielded this exception.</param>
    public JComRuntimeException(string message, Exception innerException) : base(message, innerException) {
    }

    /// <summary>
    /// General purpose exception handler to convert .NET exceptions into JComRuntimeException
    /// types.
    /// </summary>
    /// <param name="e">Exception</param>
    /// <returns>A JComRuntimeException object</returns>
    public static JComRuntimeException GeneralHandlerNoThrow(Exception e) {
        if (e is IndexOutOfRangeException) {
            return new JComRuntimeException(JComRuntimeErrors.INDEX_OUT_OF_RANGE);
        }
        if (e is ArithmeticException) {
            return new JComRuntimeException(JComRuntimeErrors.DIVISION_BY_ZERO);
        }
        if (e is not JComRuntimeException jce) {
            return new JComRuntimeException(JComRuntimeErrors.SYSTEM_ERROR, e.Message);
        }
        return jce;
    }

    /// <summary>
    /// General purpose exception handler to convert .NET exceptions into JComRuntimeException
    /// types. If the exception is not a FAILURE type, it is rethrown.
    /// </summary>
    /// <param name="e">Exception</param>
    /// <returns>A JComRuntimeException object</returns>
    public static JComRuntimeException GeneralHandler(Exception e) {
        JComRuntimeException jce = GeneralHandlerNoThrow(e);
        if (jce.Type != JComRuntimeExceptionType.FAILURE) {
            throw jce;
        }
        return jce;
    }

    /// <summary>
    /// Constructs an JComRuntimeException instance with
    /// the specified exception message.
    /// </summary>
    /// <param name="info">A SerializationInfo object.</param>
    /// <param name="context">A StreamingContext object.</param>
    protected JComRuntimeException(SerializationInfo info, StreamingContext context) : base(info, context) {
    }

    // Helper function that returns a description on an enum.
    private static string GetEnumDescription(Enum value) {

        FieldInfo fi = value.GetType().GetField(value.ToString());
        if (fi != null && fi.GetCustomAttributes(typeof(DescriptionAttribute), false) is DescriptionAttribute[] attributes) {
            if (attributes != null && attributes.Any()) {
                return attributes.First().Description;
            }
        }
        return value.ToString();
    }
}
