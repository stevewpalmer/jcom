// FORTRAN Runtime Library
// Implements the ReadManager class
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
using JComLib;

namespace JFortranLib; 

/// <summary>
/// Implements ReadManager.
/// </summary>
public class ReadManager : IDisposable {
    private readonly FormatManager _format;
    private readonly IOFile _file;

    private bool _isDisposed;
    private string _line;
    private int _valueRepeat;
    private int _valueRepeatCount;
    private int _valueCharCount;
    private bool _hasValueRepeat;
    private int _readIndex;

    /// <summary>
    /// The value of an End Of File character
    /// </summary>
    public const char EOF = '\0';

    /// <summary>
    /// The value of an End Of Line character.
    /// </summary>
    public const char EOL = '\n';

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadManager"/> class for the
    /// given IO device, record index and format string.
    /// </summary>
    /// <param name="iodevice">The device to read from</param>
    /// <param name="record">The index of the record to read (for direct access)</param>
    /// <param name="formatString">Format string</param>
    public ReadManager(int iodevice, int record, string formatString) {
        _file = IOFile.Get(iodevice);
        if (_file == null) {
            _file = new IOFile(iodevice);
            _file.Open();
        }
        if (record >= 1) {
            _file.RecordIndex = record;
        }
        if (formatString == "*") {
            formatString = string.Empty;
            _file.IsFormatted = true;
        } else {
            _file.IsFormatted = !string.IsNullOrEmpty(formatString);
        }
        _format = new FormatManager(formatString) {
            BlanksAsZero = _file.Blank == 'Z'
        };
        _readIndex = -1;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadManager"/> class using
    /// the input string as the source.
    /// </summary>
    /// <param name="line">Input string to use</param>
    /// <param name="formatString">Format string</param>
    public ReadManager(string line, string formatString) {
        _line = line;
        _format = new FormatManager(formatString);
    }

    /// <summary>
    /// Gets or sets a value indicating whether a user-specified end handler
    /// has been defined.
    /// </summary>
    public bool HasEnd { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a user-specified error handler
    /// has been defined.
    /// </summary>
    public bool HasErr { get; set; }

    /// <summary>
    /// Marks the end of a record read.
    /// </summary>
    public void EndRecord() {
        if (_isDisposed) {
            throw new ObjectDisposedException(GetType().Name);
        }
        if (_file != null) {
            _file.RecordIndex += 1;
        }
    }

    /// <summary>
    /// Return the next FormatRecord for this read. If we reach the end, we
    /// reset and rescan from the beginning.
    /// </summary>
    /// <returns>A FormatRecord, or null if no format has been set</returns>
    public FormatRecord Record() {
        if (_isDisposed) {
            throw new ObjectDisposedException(GetType().Name);
        }
        if (_format == null || _format.IsEmpty()) {
            return null;
        }
        FormatRecord record = _format.Next();
        while (true) {
            if (_readIndex == -1) {
                ReadNextLine();
            }
            if (record == null) {
                _format.Reset();
                _readIndex = -1;
            }
            else if (record.IsPositional) {
                ProcessPosition(record);
            }
            else if (record.IsEndRecord) {
                _readIndex = -1;
            }
            else {
                break;
            }
            record = _format.Next();
        }
        return record;
    }

    /// <summary>
    /// Skips the next record. Returns the number of characters
    /// skipped, or 0 if we were at the end of the file before the
    /// skip. If an I/O error occurred, return -1.
    /// </summary>
    /// <returns>The number of characters skipped</returns>
    public int SkipRecord() {
        if (_isDisposed) {
            throw new ObjectDisposedException(GetType().Name);
        }
        int returnValue;
        try {
            returnValue = _file.SkipRecord();
        } catch (IOException) {
            returnValue = -1;
        }
        return TestAndThrowError(returnValue);
    }

    /// <summary>
    /// Reads an integer from the input file. The integer is read either
    /// as a formatted or unformatted value depending on how the file was
    /// opened.
    /// </summary>
    /// <param name="intValue">A reference to the integer to be set</param>
    /// <returns>The count of characters read. 0 means EOF, and -1 means an error</returns>
    public int ReadInteger(ref int intValue) {
        if (_isDisposed) {
            throw new ObjectDisposedException(GetType().Name);
        }
        int returnValue;
        try {
            if (IsFormatted()) {
                returnValue = ReadIntegerFormatted(ref intValue);
            } else {
                returnValue = ReadIntegerUnformatted(ref intValue);
            }
        } catch (IOException) {
            returnValue = -1;
        }
        return TestAndThrowError(returnValue);
    }

    /// <summary>
    /// Reads a boolean from the input file. The boolean is read either
    /// as a formatted or unformatted value depending on how the file was
    /// opened.
    /// </summary>
    /// <param name="boolValue">A reference to the boolean to be set</param>
    /// <returns>The count of characters read. 0 means EOF, and -1 means an error</returns>
    public int ReadBoolean(ref bool boolValue) {
        if (_isDisposed) {
            throw new ObjectDisposedException(GetType().Name);
        }
        int returnValue;
        try {
            if (IsFormatted()) {
                returnValue = ReadBooleanFormatted(ref boolValue);
            } else {
                returnValue = ReadBooleanUnformatted(ref boolValue);
            }
        } catch (IOException) {
            returnValue = -1;
        }
        return TestAndThrowError(returnValue);
    }

    /// <summary>
    /// Reads a floating point number from the input file. The number is
    /// read either as a formatted or unformatted value depending on how
    /// the file was opened.
    /// </summary>
    /// <param name="floatValue">A reference to the float to be set</param>
    /// <returns>The count of characters read. 0 means EOF, and -1 means an error</returns>
    public int ReadFloat(ref float floatValue) {
        if (_isDisposed) {
            throw new ObjectDisposedException(GetType().Name);
        }
        int returnValue;
        try {
            if (IsFormatted()) {
                returnValue = ReadFloatFormatted(ref floatValue);
            } else {
                returnValue = ReadFloatUnformatted(ref floatValue);
            }
        } catch (IOException) {
            returnValue = -1;
        }
        return TestAndThrowError(returnValue);
    }

    /// <summary>
    /// Reads a double precision number from the input file. The number is
    /// read either as a formatted or unformatted value depending on how
    /// the file was opened.
    /// </summary>
    /// <param name="doubleValue">A reference to the double to be set</param>
    /// <returns>The count of characters read. 0 means EOF, and -1 means an error</returns>
    public int ReadDouble(ref double doubleValue) {
        if (_isDisposed) {
            throw new ObjectDisposedException(GetType().Name);
        }
        int returnValue;
        try {
            if (IsFormatted()) {
                returnValue = ReadDoubleFormatted(ref doubleValue);
            } else {
                returnValue = ReadDoubleUnformatted(ref doubleValue);
            }
        } catch (IOException) {
            returnValue = -1;
        }
        return TestAndThrowError(returnValue);
    }

    /// <summary>
    /// Reads a string from the input file. The string is read either as
    /// a formatted or unformatted value depending on how the file was opened.
    /// </summary>
    /// <param name="fixedString">A reference to the string to be set</param>
    /// <returns>The count of characters read. 0 means EOF, and -1 means an error</returns>
    public int ReadString(ref FixedString fixedString) {
        if (_isDisposed) {
            throw new ObjectDisposedException(GetType().Name);
        }
        int returnValue;
        try {
            if (IsFormatted()) {
                returnValue = ReadStringFormatted(ref fixedString);
            } else {
                returnValue = ReadStringUnformatted(ref fixedString);
            }
        } catch (IOException) {
            returnValue = -1;
        }
        return TestAndThrowError(returnValue);
    }

    /// <summary>
    /// Releases all resource used by the <see cref="ReadManager"/> object.
    /// </summary>
    public void Dispose() {
        Dispose(true);
    }

    /// <summary>
    /// Releases all resource used by the <see cref="ReadManager"/> object.
    /// </summary>
    /// <param name="disposing">True if we're disposing</param>
    protected virtual void Dispose(bool disposing) {
        if (!_isDisposed) {
            if (disposing) {
                if (_file != null) {
                    _file.Dispose();
                }
            }
            _isDisposed = true;
        }
    }

    // Tests the return code and throws an exception if an error or EOF
    // was detected and no user defined handler has been specified.
    private int TestAndThrowError(int returnCode) {
        if (returnCode == -1 && !HasErr) {
            throw new JComRuntimeException(JComRuntimeErrors.IO_READ_ERROR);
        }
        if (returnCode == 0 && !HasEnd) {
            throw new JComRuntimeException(JComRuntimeErrors.UNEXPECTED_END_OF_FILE);
        }
        return returnCode;
    }

    // Read a fixed number of characters from the input.
    private string ReadChars(int width) {
        StringBuilder str = new();

        while (width-- > 0) {
            int ch = ReadChar();
            if (ch == EOF) {
                break;
            }
            str.Append((char)ch);
        }
        return str.ToString();
    }

    // Reads the next character from the input, triggering a load from the
    // input source if this is the first time ReadChar is called.
    private char ReadChar() {
        char ch = PeekChar();
        if (ch != EOF && ch != EOL) {
            ++_readIndex;
        }
        return ch;
    }

    // Peek at the next character in the source
    private char PeekChar() {
        if (_isDisposed) {
            throw new ObjectDisposedException(GetType().Name);
        }
        if (_readIndex == -1) {
            ReadNextLine();
            if (_line == null) {
                _readIndex = -1;
                return EOF;
            }
        }
        if (_readIndex == _line.Length) {
            _readIndex = -1;
            return EOL;
        }
        return _line[_readIndex];
    }

    // Push a char back onto the input queue. Doesn't
    // work across lines.
    private void BackChar() {
        if (_readIndex > 0) {
            --_readIndex;
        }
    }

    // Read the first non-whitespace character in the
    // input.
    private char ReadNonSpaceChar() {
        char ch = ReadChar();
        while (char.IsWhiteSpace(ch)) {
            ch = ReadChar();
        }
        return ch;
    }

    // Skip a separator character if one is found.
    private void SkipSeparators(char ch) {
        if (ch != ',') {
            BackChar();
        }
    }

    // Parse a possible repeat specifier in the list directed input. If
    // ch is a '*' character then str is a repeat value so parse off the
    // number and return true. Otherwise return false.
    private bool ParseRepeatSpecifier(char ch, string str) {
        if (ch != '*') { // Repeat specifier
            return false;
        }

        // Note that this doesn't reject m*n*0<value> syntax but we can
        // live with this since it is basically redundant.
        if (_valueRepeatCount > 0) {
            throw new JComRuntimeException(JComRuntimeErrors.FORMAT_MULTIPLE_REPEAT);
        }
        _valueRepeatCount = FormatNumber.ParseInteger(str, null) - 1;
        if (_valueRepeatCount < 0) {
            _valueRepeatCount = 0;
        }
        return true;
    }

    // Read an integer formatted from the file.
    private int ReadIntegerFormatted(ref int intValue) {
        int fieldWidth = 0;
        int charsRead = 0;

        // Handle a repeat count first
        if (_valueRepeatCount > 0) {
            if (_hasValueRepeat) {
                intValue = _valueRepeat;
            }
            --_valueRepeatCount;
            return _valueCharCount;
        }
        
        // For input formats, only the field width is used and it
        // determines the fixed number of characters to read.
        FormatRecord record = Record();
        if (record != null) {
            fieldWidth = record.FieldWidth;
        }
        string str = string.Empty;
        if (fieldWidth > 0) {
            str = ReadChars(fieldWidth);
            charsRead = str.Length;
        } else {
            StringBuilder strBuilder = new();

            char ch = ReadNonSpaceChar();
            for (int m = 0; m < 2; ++m) {
                if (ch == EOF) {
                    return 0;
                }
                while (char.IsDigit(ch) || ch == '+' || ch == '-') {
                    strBuilder.Append(ch);
                    ch = ReadChar();
                    ++charsRead;
                }
                str = strBuilder.ToString();
                if (!ParseRepeatSpecifier(ch, str)) {
                    break;
                }
                strBuilder.Clear();
                ch = ReadChar();
            }
            SkipSeparators(ch);
            charsRead = Math.Max(1, charsRead);
        }
        if (str.Length > 0) {
            intValue = _valueRepeat = FormatNumber.ParseInteger(str, record);
            _valueCharCount = charsRead;
            _hasValueRepeat = _valueRepeatCount > 0;
        }
        return charsRead;
    }

    // Read a boolean formatted from the file
    private int ReadBooleanFormatted(ref bool boolValue) {
        int fieldWidth = 0;
        int charsRead = 0;

        // Handle a repeat count first
        if (_valueRepeatCount > 0) {
            if (_hasValueRepeat) {
                boolValue = _valueRepeat != 0;
            }
            --_valueRepeatCount;
            return _valueCharCount;
        }

        // For input formats, only the field width is used and it
        // determines the fixed number of characters to read.
        FormatRecord record = Record();
        if (record != null) {
            fieldWidth = record.FieldWidth;
        }
        string str;
        if (fieldWidth > 0) {
            str = ReadChars(fieldWidth);
        } else {
            StringBuilder strBuilder = new();
            char ch = ReadNonSpaceChar();
            if (ch == EOF) {
                return 0;
            }

            // Optional '.' at start of a logical value
            if (ch == '.') {
                ch = ReadChar();
            }
            while (ch != EOF && ch != EOL && !char.IsWhiteSpace(ch) && ch != '/' && ch != ',') {
                strBuilder.Append(ch);
                ch = ReadChar();
            }
            SkipSeparators(ch);
            str = strBuilder.ToString();
        }
        if (str != null) {
            str = str.ToUpper().Trim();
            if (str.Length > 0) {
                boolValue = str[0] == 'T' || string.Compare(str, 0, ".T", 0, 2) == 0;
                charsRead = str.Length;
            }
            charsRead = Math.Max(1, charsRead);
        }
        return charsRead;
    }

    // Read a floating point number formatted from the file
    private int ReadFloatFormatted(ref float floatValue) {
        int fieldWidth = 0;
        int charsRead = 0;

        // Handle a repeat count first
        if (_valueRepeatCount > 0) {
            if (_hasValueRepeat) {
                floatValue = _valueRepeat;
            }
            --_valueRepeatCount;
            return _valueCharCount;
        }

        // For input formats, only the field width is used and it
        // determines the fixed number of characters to read.
        FormatRecord record = Record();
        if (record != null) {
            fieldWidth = record.FieldWidth;
        }
        string str = string.Empty;
        if (fieldWidth > 0) {
            str = ReadChars(fieldWidth);
            charsRead = str.Length;
        } else {
            StringBuilder strBuilder = new();
            List<char> validFloatChars = new() { '0','1','2','3','4','5','6','7','8','9','E','-','+','.' };
            
            char ch = ReadNonSpaceChar();
            for (int m = 0; m < 2; ++m) {
                if (ch == EOF) {
                    return 0;
                }
                while (validFloatChars.Contains(ch)) {
                    strBuilder.Append(ch);
                    ch = ReadChar();
                    ++charsRead;
                }
                str = strBuilder.ToString();
                if (!ParseRepeatSpecifier(ch, str)) {
                    break;
                }
                strBuilder.Clear();
                ch = ReadChar();
            }
            SkipSeparators(ch);
            charsRead = Math.Max(1, charsRead);
        }
        if (str.Length > 0) {
            floatValue = FormatNumber.ParseFloat(str, record);
            _valueCharCount = charsRead;
            _hasValueRepeat = _valueRepeatCount > 0;
        }
        return charsRead;
    }

    // Read a double precision number formatted from the file.
    private int ReadDoubleFormatted(ref double doubleValue) {
        int fieldWidth = 0;
        int charsRead = 0;

        // Handle a repeat count first
        if (_valueRepeatCount > 0) {
            if (_hasValueRepeat) {
                doubleValue = _valueRepeat;
            }
            --_valueRepeatCount;
            return _valueCharCount;
        }

        // For input formats, only the field width is used and it
        // determines the fixed number of characters to read.
        FormatRecord record = Record();
        if (record != null) {
            fieldWidth = record.FieldWidth;
        }
        string str = string.Empty;
        if (fieldWidth > 0) {
            str = ReadChars(fieldWidth);
            charsRead = str.Length;
        } else {
            StringBuilder strBuilder = new();
            List<char> validDoubleChars = new() { '0','1','2','3','4','5','6','7','8','9','D','-','+','.' };
            
            char ch = ReadNonSpaceChar();
            for (int m = 0; m < 2; ++m) {
                if (ch == EOF) {
                    return 0;
                }
                while (validDoubleChars.Contains(ch)) {
                    strBuilder.Append(ch);
                    ch = ReadChar();
                    ++charsRead;
                }
                str = strBuilder.ToString();
                if (!ParseRepeatSpecifier(ch, str)) {
                    break;
                }
                strBuilder.Clear();
                ch = ReadChar();
            }
            SkipSeparators(ch);
            charsRead = Math.Max(1, charsRead);
        }
        if (str.Length > 0) {
            doubleValue = FormatNumber.ParseDouble(str, record);
            _valueCharCount = charsRead;
            _hasValueRepeat = _valueRepeatCount > 0;
        }
        return charsRead;
    }

    // Read a string, formatted from the file.
    private int ReadStringFormatted(ref FixedString fixedstrVar) {
        int fieldWidth = 0;
        int charsRead = 0;
        int strSize = fixedstrVar.Length;

        // Handle a repeat count first
        if (_valueRepeatCount > 0) {
            if (_hasValueRepeat) {
                fixedstrVar = _valueRepeat.ToString();
            }
            --_valueRepeatCount;
            return _valueCharCount;
        }

        // For input formats, only the field width is used and it
        // determines the fixed number of characters to read.
        FormatRecord record = Record();
        if (record != null) {
            fieldWidth = record.FieldWidth;
        }
        if (fieldWidth > 0) {
            string realString = ReadChars(fieldWidth);
            int index = Math.Max(0, realString.Length - strSize);
            int length = Math.Min(strSize, realString.Length);
            
            fixedstrVar.Set(realString.Substring(index));
            charsRead = length;
        } else {
            StringBuilder str = new();
            char ch = ReadNonSpaceChar();
            if (ch == '\'' && record == null) {
                ch = ReadChar();
                while (ch != EOF && ch != EOL) {
                    if (ch == '\'') {
                        ch = ReadChar();
                        if (ch != '\'') {
                            break;
                        }
                    }
                    str.Append(ch);
                    ++charsRead;
                    ch = ReadChar();
                }
                SkipSeparators(ch);
            } else {
                while (charsRead < strSize && ch != EOL && ch != EOF) {
                    str.Append(ch);
                    ch = ReadChar();
                    ++charsRead;
                }
                BackChar();
            }
            fixedstrVar.Set(str.ToString());
        }
        return charsRead;
    }

    // Read an integer unformatted from the file.
    private int ReadIntegerUnformatted(ref int intValue) {
        return _file.ReadInteger(ref intValue);
    }

    // Read a boolean unformatted from the file.
    private int ReadBooleanUnformatted(ref bool boolValue) {
        return _file.ReadBoolean(ref boolValue);
    }

    // Read a float unformatted from the file.
    private int ReadFloatUnformatted(ref float floatValue) {
        return _file.ReadFloat(ref floatValue);
    }

    // Read a double unformatted from the file.
    private int ReadDoubleUnformatted(ref double doubleValue) {
        return _file.ReadDouble(ref doubleValue);
    }

    // Read a string unformatted from the file.
    private int ReadStringUnformatted(ref FixedString stringValue) {
        return _file.ReadString(ref stringValue, int.MaxValue);
    }

    // Returns whether this file has been opened for formatted read. For
    // internal reads, where _file is null, we always read formatted.
    private bool IsFormatted() {
        return _file == null || _file.IsFormatted;
    }

    // Read the next line from the input.
    private void ReadNextLine() {
        _line = _file.ReadLine();
        _readIndex = 0;
        _valueRepeatCount = 0;
    }

    // Process any positional directions in the given record.
    private void ProcessPosition(FormatRecord record) {
        if (record.IsPositional && _line != null) {
            if (record.Relative) {
                _readIndex += record.Count;
            } else {
                _readIndex = record.Count;
            }
            _readIndex = Math.Max(_readIndex, 0);
            _readIndex = Math.Min(_readIndex, _line.Length - 1);
        }
    }
}