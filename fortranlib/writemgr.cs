// FORTRAN Runtime Library
// Implements the WriteManager class
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

using System.Numerics;
using System.Text;
using JComLib;

namespace JFortranLib {

    /// <summary>
    /// Implements WriteManager.
    /// </summary>
    public class WriteManager : IDisposable {

        private readonly FormatManager _format;
        private readonly IOFile _file;
        private bool _isDisposed;
        private int _writeIndex;
        private int _writeMaxIndex;
        private int _writeItemsCount;
        private int _writeBufferSize = 256;
        private char[] _line = new char[256];

        /// <summary>
        /// Initializes a new instance of the <see cref="WriteManager"/> class for the
        /// given IO device, record index and format string.
        /// </summary>
        /// <param name="iodevice">The device to read from</param>
        /// <param name="record">The index of the record to write (for direct access)</param>
        /// <param name="formatString">Format string</param>
        public WriteManager(int iodevice, int record, string formatString) : this(iodevice, record, formatString, 1) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WriteManager"/> class for the
        /// given IO device, record index and format string.
        /// </summary>
        /// <param name="iodevice">The device to read from</param>
        /// <param name="record">The index of the record to write (for direct access)</param>
        /// <param name="formatString">Format string</param>
        /// <param name="advance">Whether we advance after a record</param>
        public WriteManager(int iodevice, int record, string formatString, int advance) {
            _file = IOFile.Get(iodevice);
            if (_file == null) {
                _file = new IOFile(iodevice);
                _file.Open();
            }
            if (record >= 1) {
                _file.RecordIndex = record;
            }
            if (advance == 0) {
                formatString += "$";
            }
            _file.IsFormatted = !string.IsNullOrEmpty(formatString);
            _format = new FormatManager(formatString);
            UseSeparators = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WriteManager"/> class using
        /// a specified format string.
        /// </summary>
        /// <param name="formatString">Format string</param>
        public WriteManager(string formatString) {
            _format = new FormatManager(formatString);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WriteManager"/> class using
        /// a specified format string.
        /// </summary>
        /// <param name="formatStringArray">Format string</param>
        public WriteManager(FixedString[] formatStringArray) {
            StringBuilder str = new();
            foreach (FixedString formatString in formatStringArray) {
                str.Append(formatString);
            }
            _format = new FormatManager(str.ToString());
        }

        /// <summary>
        /// Gets or sets a value indicating whether a user-specified error handler
        /// has been defined.
        /// </summary>
        public bool HasErr { get; set; }

        /// <summary>
        /// Marks the end of a record read and returns the last string
        /// formatted and written.
        /// </summary>
        /// <returns>The last string written</returns>
        public string EndRecord() {
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (_file != null) {
                _file.Flush();
            }
            if (IsFormatted()) {
                FormatRecord record = Record(false);
                FormatRecord lastRecord = record;
                while (record != null) {
                    lastRecord = record;
                    record = Record(false);
                }
                bool carriageReturn = true;
                if (lastRecord != null) {
                    carriageReturn = !lastRecord.SuppressCarriage;
                }
                if (_writeIndex > 0) {
                    string str = new(_line, 0, _writeMaxIndex);
                    if (_file != null) {
                        _file.WriteLine(str, carriageReturn);
                    }
                    _writeMaxIndex = 0;
                    _writeIndex = 0;
                    _writeItemsCount = 0;
                    return str;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Sets whether the first column of the output string is treated as
        /// a control code. If true, then the first character of each output line is
        /// interpreted as a Fortran control code. If false, the first character is
        /// output normally.
        /// </summary>
        /// <param name="isSpecial">Boolean flag that specifies whether the first column is special</param>
        public void SetFirstColumnSpecial(bool isSpecial) {
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (_file != null) {
                _file.IsFirstWriteColumnSpecial = isSpecial;
            }
        }

        /// <summary>
        /// Sets whether or not successive output is separated by single spaces.
        /// </summary>
        public bool UseSeparators { get; set; }

        /// <summary>
        /// Return the next FormatRecord for this write. If we reach the end and
        /// allowReset is permitted, we reset and rescan from the beginning.
        /// </summary>
        /// <param name="allowReset">True if I/O format rescan allowed</param>
        /// <returns>A FormatRecord, or null if the end of the format list was reached</returns>
        public FormatRecord Record(bool allowReset) {
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (_format == null || _format.IsEmpty()) {
                return null;
            }
            FormatRecord record = _format.Next();
            while (true) {
                if (record == null) {
                    if (!allowReset) {
                        break;
                    }
                    ProcessEndRecord();
                    _format.Reset();
                }
                else if (record.IsPositional) {
                    ProcessPosition(record);
                }
                else if (record.IsRawString && record.RawString.Length > 0) {
                    WriteChars(record.RawString);
                }
                else if (record.IsEndRecord) {
                    ProcessEndRecord();
                } else {
                    break;
                }
                record = _format.Next();
            }
            return record;
        }

        /// <summary>
        /// Writes an integer to the output file. The integer is written either as
        /// a formatted or unformatted value depending on how the file was opened.
        /// </summary>
        /// <param name="intValue">The integer value to be written</param>
        /// <returns>The count of characters read. 0 means EOF, and -1 means an error</returns>
        public int WriteInteger(int intValue) {
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().Name);
            }
            int returnValue;
            try {
                if (IsFormatted()) {
                    returnValue = WriteIntegerFormatted(intValue);
                } else {
                    returnValue = WriteIntegerUnformatted(intValue);
                }
            } catch (IOException) {
                returnValue = -1;
            }
            return TestAndThrowError(returnValue);
        }

        /// <summary>
        /// Writes a float to the output file. The float value is written either as
        /// a formatted or unformatted value depending on how the file was opened.
        /// </summary>
        /// <param name="floatValue">The floating point value to be written</param>
        /// <returns>The count of characters read. 0 means EOF, and -1 means an error</returns>
        public int WriteFloat(float floatValue) {
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().Name);
            }
            int returnValue;
            try {
                if (IsFormatted()) {
                    returnValue = WriteFloatFormatted(floatValue);
                } else {
                    returnValue = WriteFloatUnformatted(floatValue);
                }
            } catch (IOException) {
                returnValue = -1;
            }
            return TestAndThrowError(returnValue);
        }

        /// <summary>
        /// Writes a double to the output file. The double value is written either as
        /// a formatted or unformatted value depending on how the file was opened.
        /// </summary>
        /// <param name="doubleValue">The double value to be written</param>
        /// <returns>The count of characters read. 0 means EOF, and -1 means an error</returns>
        public int WriteDouble(double doubleValue) {
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().Name);
            }
            int returnValue;
            try {
                if (IsFormatted()) {
                    returnValue = WriteDoubleFormatted(doubleValue);
                } else {
                    returnValue = WriteDoubleUnformatted(doubleValue);
                }
            } catch (IOException) {
                returnValue = -1;
            }
            return TestAndThrowError(returnValue);
        }

        /// <summary>
        /// Writes a complex to the output file. The complex value is written either as
        /// a formatted or unformatted value depending on how the file was opened.
        /// </summary>
        /// <param name="complexValue">The complex value to be written</param>
        /// <returns>The count of characters read. 0 means EOF, and -1 means an error</returns>
        public int WriteComplex(Complex complexValue) {
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().Name);
            }
            int returnValue;
            try {
                if (IsFormatted()) {
                    returnValue = WriteComplexFormatted(complexValue);
                } else {
                    returnValue = WriteComplexUnformatted(complexValue);
                }
            } catch (IOException) {
                returnValue = -1;
            }
            return TestAndThrowError(returnValue);
        }

        /// <summary>
        /// Writes a boolean to the output file. The boolean value is written either as
        /// a formatted or unformatted value depending on how the file was opened.
        /// </summary>
        /// <param name="boolValue">The boolean value to be written</param>
        /// <returns>The count of characters read. 0 means EOF, and -1 means an error</returns>
        public int WriteBoolean(bool boolValue) {
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().Name);
            }
            int returnValue;
            try {
                if (IsFormatted()) {
                    returnValue = WriteBoolFormatted(boolValue);
                } else {
                    returnValue = WriteBoolUnformatted(boolValue);
                }
            } catch (IOException) {
                returnValue = -1;
            }
            return TestAndThrowError(returnValue);
        }

        /// <summary>
        /// Writes a string to the output file. The string is written either as
        /// a formatted or unformatted value depending on how the file was opened.
        /// </summary>
        /// <param name="str">The string to be written</param>
        /// <returns>The count of characters written. 0 means EOF, and -1 means an error</returns>
        public int WriteString(string str) {
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().Name);
            }
            int returnValue;
            try {
                if (IsFormatted()) {
                    returnValue = WriteStringFormatted(str);
                } else {
                    returnValue = WriteStringUnformatted(str);
                }
            } catch (IOException) {
                returnValue = -1;
            }
            return TestAndThrowError(returnValue);
        }

        /// <summary>
        /// Writes an empty record.
        /// </summary>
        /// <returns>The count of characters written. 0 means EOF, and -1 means an error</returns>
        public int WriteEmpty() {
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().Name);
            }
            int returnValue = 0;
            try {
                if (IsFormatted()) {
                    Record(false);
                    WriteChars(string.Empty);
                } else {
                    WriteStringUnformatted(string.Empty);
                }
            } catch (IOException) {
                returnValue = -1;
            }
            return TestAndThrowError(returnValue);
        }

        /// <summary>
        /// Releases all resource used by the <see cref="WriteManager"/> object.
        /// </summary>
        public void Dispose() {
            Dispose(true);
        }
        
        /// <summary>
        /// Releases all resource used by the <see cref="WriteManager"/> object.
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
                throw new JComRuntimeException(JComRuntimeErrors.IO_WRITE_ERROR);
            }
            return returnCode;
        }

        // Format a string as specified by an "A" record.
        private static string FormatString(string value, FormatRecord record) {
            string svalue = value;

            if (record.FieldWidth > 0) {
                if (value.Length > record.FieldWidth) {
                    svalue = value.Substring(0, record.FieldWidth);
                } else if (record.LeftJustify) {
                    svalue = value.PadRight(record.FieldWidth);
                } else {
                    svalue = value.PadLeft(record.FieldWidth);
                }
            }
            return svalue;
        }

        // Make sure the value is valid for the given format record type. Throw
        // a run-time exception if there's a mismatch.
        private static void VerifyFormatMatch(FormatRecord record, object value) {
            char ch = record.FormatChar;
            bool match = true;
            switch (ch) {
                case 'A':   match = value is string || value is FixedString; break;
                case 'I':   match = value is int; break;
                case 'L':   match = value is bool; break;
                case 'F':   match = value is float || value is double || value is Complex; break;    
                case 'G':   match = value is float || value is double || value is Complex; break;    
                case 'E':   match = value is float || value is double || value is Complex; break;    
            }
            if (!match) {
                string realName = value.GetType().Name;
                switch (realName.ToLower()) {
                    case "int32":       realName = "INTEGER"; break;
                    case "single":      realName = "REAL"; break;
                    case "double":      realName = "DOUBLE"; break;
                    case "string":      realName = "CHARACTER"; break;
                    case "fixedstring": realName = "CHARACTER"; break;
                }
                throw new JComRuntimeException(JComRuntimeErrors.FORMAT_RECORD_MISMATCH,
                            $"Format record mismatch: '{ch}' specifier and {realName} type");
            }
        }

        // Write a single character to the string buffer repeated
        // by the given count.
        private void WriteChar(char ch, int count) {
            while (count-- > 0) {
                if (_writeIndex >= _writeBufferSize) {
                    _writeBufferSize += 256;
                    Array.Resize(ref _line, _writeBufferSize);
                }
                _line[_writeIndex++] = ch;
            }
            if (_writeIndex > _writeMaxIndex) {
                _writeMaxIndex = _writeIndex;
            }
        }

        // Write characters to the string buffer
        private void WriteChars(string str) {
            foreach (char ch in str) {
                if (_writeIndex >= _writeBufferSize) {
                    _writeBufferSize += 256;
                    Array.Resize(ref _line, _writeBufferSize);
                }
                _line[_writeIndex++] = ch;
            }
            if (_writeIndex > _writeMaxIndex) {
                _writeMaxIndex = _writeIndex;
            }
        }

        // Write an integer formatted to the output file
        private int WriteIntegerFormatted(int intValue) {
            FormatRecord record = Record(true);
            if (record == null && _file != null && _file.IsFirstWriteColumnSpecial) {
                WriteChar(' ', 1);
            }
            string str;
            if (record != null) {
                VerifyFormatMatch(record, intValue);
                str = FormatNumber.FormatInteger(intValue, record);
            } else {
                if (_writeItemsCount > 0 && UseSeparators) {
                    WriteChar(' ', 1); // To neatly separate out the items
                }
                str = FormatNumber.FormatValue(intValue);
            }
            WriteChars(str);
            ++_writeItemsCount;
            return str.Length;
        }

        // Write a float value formatted to the output file
        private int WriteFloatFormatted(float floatValue) {
            FormatRecord record = Record(true);
            if (record == null && _file != null && _file.IsFirstWriteColumnSpecial) {
                WriteChar(' ', 1);
            }
            string str;
            if (record != null) {
                VerifyFormatMatch(record, floatValue);
                str = FormatNumber.FormatFloat(floatValue, record);
            } else {
                if (_writeItemsCount > 0 && UseSeparators) {
                    WriteChar(' ', 1); // To neatly separate out the items
                }
                str = FormatNumber.FormatValue(floatValue);
            }
            WriteChars(str);
            ++_writeItemsCount;
            return str.Length;
        }

        // Write a double value formatted to the output file
        private int WriteDoubleFormatted(double doubleValue) {
            FormatRecord record = Record(true);
            if (record == null && _file != null && _file.IsFirstWriteColumnSpecial) {
                WriteChar(' ', 1);
            }
            string str;
            if (record != null) {
                VerifyFormatMatch(record, doubleValue);
                str = FormatNumber.FormatDouble(doubleValue, record);
            } else {
                if (_writeItemsCount > 0 && UseSeparators) {
                    WriteChar(' ', 1); // To neatly separate out the items
                }
                str = FormatNumber.FormatValue(doubleValue);
            }
            WriteChars(str);
            ++_writeItemsCount;
            return str.Length;
        }

        // Write a complex value formatted to the output file
        private int WriteComplexFormatted(Complex complexValue) {
            FormatRecord record = Record(true);
            if (record == null && _file != null && _file.IsFirstWriteColumnSpecial) {
                WriteChar(' ', 1);
            }
            string str;

            if (record != null) {
                int charsWritten = 0;

                VerifyFormatMatch(record, complexValue);
                str = FormatNumber.FormatDouble(complexValue.Real, record);
                WriteChars(str);
                charsWritten += str.Length;

                record = Record(true);
                if (record != null) {
                    str = FormatNumber.FormatDouble(complexValue.Imaginary, record);
                    WriteChars(str);
                    charsWritten += str.Length;

                    ++_writeItemsCount;
                    return charsWritten;
                }
            }
            if (_writeItemsCount > 0 && UseSeparators) {
                WriteChar(' ', 1); // To neatly separate out the items
            }
            str = FormatNumber.FormatValue(complexValue);
            WriteChars(str);
            ++_writeItemsCount;
            return str.Length;
        }

        // Write a boolean value formatted to the output file
        private int WriteBoolFormatted(bool boolValue) {
            FormatRecord record = Record(true);
            if (record == null && _file != null && _file.IsFirstWriteColumnSpecial) {
                WriteChar(' ', 1);
            }
            string str;
            if (record != null) {
                VerifyFormatMatch(record, boolValue);
                str = FormatNumber.FormatBoolean(boolValue, record);
            } else {
                if (_writeItemsCount > 0 && UseSeparators) {
                    WriteChar(' ', 1); // To neatly separate out the items
                }
                str = FormatNumber.FormatValue(boolValue);
            }
            WriteChars(str);
            ++_writeItemsCount;
            return str.Length;
        }

        // Write a string formatted to the output file
        private int WriteStringFormatted(string str) {
            FormatRecord record = Record(true);
            if (record == null && _file != null && _file.IsFirstWriteColumnSpecial) {
                WriteChar(' ', 1);
            }
            if (record != null) {
                VerifyFormatMatch(record, str);
                str = FormatString(str, record);
            } else {
                if (_writeItemsCount > 0 && UseSeparators) {
                    WriteChar(' ', 1); // To neatly separate out the items
                }
            }
            WriteChars(str);
            ++_writeItemsCount;
            return str.Length;
        }

        // Write an integer unformatted to the output file
        private int WriteIntegerUnformatted(int intValue) {
            return _file.WriteInteger(intValue);
        }

        // Write a float unformatted to the output file
        private int WriteFloatUnformatted(float floatValue) {
            return _file.WriteFloat(floatValue);
        }

        // Write a double unformatted to the output file
        private int WriteDoubleUnformatted(double doubleValue) {
            return _file.WriteDouble(doubleValue);
        }

        // Write a complex unformatted to the output file
        private int WriteComplexUnformatted(Complex complexValue) {
            return _file.WriteDouble(complexValue.Real) + _file.WriteDouble(complexValue.Imaginary);
        }

        // Write a boolean unformatted to the output file
        private int WriteBoolUnformatted(bool boolValue) {
            return _file.WriteBoolean(boolValue);
        }

        // Write a string unformatted to the output file
        private int WriteStringUnformatted(string str) {
            return _file.WriteString(str);
        }

        // Returns whether this file has been opened for formatted read. For
        // internal reads, where _file is null, we always read formatted.
        private bool IsFormatted() {
            return (_file == null) || _file.IsFormatted;
        }

        // Process an end record by writing out the line so far
        // and reseting the write buffer.
        private void ProcessEndRecord() {
            string str = new(_line, 0, _writeMaxIndex);
            if (_file != null) {
                _file.WriteLine(str, true);
            }
            _writeMaxIndex = 0;
            _writeIndex = 0;
            _writeItemsCount = 0;
        }

        // Process any positional directions in the given record.
        private void ProcessPosition(FormatRecord record) {
            int currentWidth = _writeIndex;
            if (record.Relative) {
                currentWidth += record.Count;
            } else {
                currentWidth = record.Count - 1;
            }
            if (currentWidth > _writeMaxIndex) {
                _writeIndex = _writeMaxIndex;
                WriteChar(' ', currentWidth - _writeMaxIndex);
            } else {
                _writeIndex = currentWidth;
            }
        }
    }
}