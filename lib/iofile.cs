// JCom Runtime Library
// I/O file class functions
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JComLib {

    /// <summary>
    /// Special I/O constants.
    /// </summary>
    public static class IOConstant {

        /// <summary>
        /// The unit number for the standard input device. This
        /// is typically the keyboard.
        /// </summary>
        public const int Stdin = 5;

        /// <summary>
        /// The unit number for the standard output device. This
        /// is typically the console.
        /// </summary>
        public const int Stdout = 6;
    }

    /// <summary>
    /// Behaviour at end of input
    /// </summary>
    public enum LineTerminator {

        /// <summary>
        /// Do nothing
        /// </summary>
        NONE = 0,

        /// <summary>
        /// Advance cursor to the next line
        /// </summary>
        NEWLINE = 1,

        /// <summary>
        /// Advance cursor to the next zone
        /// </summary>
        NEXTZONE = 2
    }

    /// <summary>
    /// A class that encapsulates the standard output file.
    /// </summary>            
    public sealed class StdoutIOFile : IOFile {

        // Class variables
        private bool _skipClear;

        public StdoutIOFile() : base(IOConstant.Stdout) {}

        /// <summary>
        /// Stdout always produces formatted output.
        /// </summary>
        public override bool IsFormatted => true;

        /// <summary>
        /// Write a string to the console. Note that the string is treated as a single line
        /// and includes an implied newline if the output device is not direct access. Any
        /// embedded newlines are written but do not contribute the the record count.
        /// </summary>
        /// <param name="str">The string to write</param>
        /// <returns>The number of characters written to the device</returns>
        public override int WriteLine(string str, bool carriageAtEnd) {
            int charsWritten = 0;

            if (str.Length == 0) {
                Console.WriteLine();
            } else if (IsFirstWriteColumnSpecial) {
                string toWrite = str.Substring(1);
                switch (str[0]) {
                    default:
                        Console.WriteLine(toWrite);
                        break;

                    case '+':
                        Console.Write(toWrite);
                        Console.Write('\r'); // Move to start of same line
                        break;

                    case '0':
                        Console.WriteLine();
                        Console.WriteLine(toWrite);
                        break;

                    case '1':
                        // Doing Console.Clear with redirection causes an exception
                        // on Windows, although, oddly enough, not with Mono. The
                        // property to test for redirection is .NET 4.5 only. So we
                        // hack and catch the exception and then use a flag for any
                        // future outputs in this session.
                        if (!_skipClear) {
                            try {
                                Console.Clear();
                            } catch (IOException) {
                                _skipClear = true;
                            }
                        }
                        Console.WriteLine(toWrite);
                        break;
                }
                charsWritten = toWrite.Length;
            } else if (carriageAtEnd) {
                Console.WriteLine(str);
            } else {
                Console.Write(str);
            }
            return charsWritten;
        }
    }

    /// <summary>
    /// A class that encapsulates the standard input file.
    /// </summary>            
    public sealed class StdinIOFile : IOFile {

        public StdinIOFile() : base(IOConstant.Stdin) {
            Width = -1;
            Terminator = LineTerminator.NEWLINE;
        }

        /// <summary>
        /// Stdin always takes formatted input.
        /// </summary>
        public override bool IsFormatted => true;

        /// <summary>
        /// Behaviour of the cursor at the end of input.
        /// </summary>
        public LineTerminator Terminator { get; set; }

        /// <summary>
        /// Zone width for next zone terminator.
        /// </summary>
        public int Zone { get; set; }

        /// <summary>
        /// Maximum width of input.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Read the specified number of characters from the console.
        /// </summary>
        /// <param name="count">Number of characters to read</param>
        /// <returns>The string read</returns>
        public override string ReadChars(int count) {

            StringBuilder stringBuilder = new();
            while (count > 0) {
                ConsoleKeyInfo keyInfo = Console.ReadKey();
                stringBuilder.Append(keyInfo.KeyChar);
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Read a line from the console.
        /// </summary>
        /// <returns>A line of text, or null if there is an error.</returns>
        public override string ReadLine() {
            ReadLine readLine = new() {
                Terminator = Terminator,
                MaxWidth = Width,
                Zone = Zone
            };
            return readLine.Read(string.Empty);
        }
    }

    /// <summary>
    /// A class that encapsulates information about a file. A file is identified by
    /// its I/O unit number which is any valid integer. Two I/O unit numbers are
    /// pre-defined for standard output and standard input and are identified by the
    /// <see cref="IOConstant">IOConstant</c> numbers.
    ///
    /// A file can be either sequential or direct, based on the IsSequential property.
    /// A sequential file is accessed by reading through the data in sequence. A
    /// direct file obtains the data by seeking to fixed offsets.
    ///
    /// A file can also be either Formatted or Unformatted.
    /// </summary>            
    public class IOFile : IDisposable {

        private static Dictionary<int, IOFile> _filemap = new();
        private static IOFile stdinFile = new StdinIOFile();
        private static IOFile stdoutFile = new StdoutIOFile();

        /// <summary>
        /// The value of an End Of File character
        /// </summary>
        public const char EOF = '\0';

        /// <summary>
        /// The value of an End Of Line character.
        /// </summary>
        public const char EOL = '\n';

        // Class variables
        private bool _isDisposed;
        private bool _isFormatted;
        private byte [] _readBuffer;
        private int _readBufferIndex;
        private int _readBufferSize;
        private string _line;
        private byte[] _writeBuffer = new byte[256];
        private int _writeBufferIndex;
        private int _writeBufferSize = 256;
        private int _recordLength;
        private int _recordIndex = 1;
        private int _readIndex;

        /// <summary>
        /// The system file handle object obtained by the Open statement.
        /// </summary>
        public int Unit { get; set; }

        /// <summary>
        /// The system file handle object obtained by the Open statement.
        /// </summary>
        public FileStream Handle { get; set; }

        /// <summary>
        /// The fully qualified path to the original file.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Specifies whether or not this is a temporary scratch file.
        /// </summary>
        public bool IsScratch { get; set; }

        /// <summary>
        /// Specifies whether the first column of the stdout device is treated as
        /// a control character that determines line spacing. This is typically
        /// used in FORTRAN only.
        /// </summary>
        /// <value><c>true</c> if this the first character is a control character otherwise, <c>false</c>.</value>
        public bool IsFirstWriteColumnSpecial { get; set; }

        /// <summary>
        /// Specifies the handling of blanks in input.
        /// </summary>
        public char Blank { get; set; }

        /// <summary>
        /// Specifies whether or not this is a new file or an existing file.
        /// </summary>
        public bool IsNew { get; set; }

        /// <summary>
        /// Specifies whether or not file access is direct or sequential.
        /// </summary>
        public bool IsSequential { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IOFile"/> class.
        /// </summary>
        /// <param name="iodevice">The device number for the file</param>
        public IOFile(int iodevice) {
            Unit = iodevice;
            IsFirstWriteColumnSpecial = false;
            IsNew = true;
            IsSequential = true;
            _isFormatted = true;
            _filemap[iodevice] = this;
            _readIndex = -1;
        }

        /// <summary>
        /// Retrieve the IOFile for the given unit number.
        /// </summary>
        /// <param name="iodevice">Unit number</param>
        /// <returns>The IOFile instance for the unit, or null if the unit is not opened</returns>
        public static IOFile Get(int iodevice) {
            return _filemap.ContainsKey(iodevice) ? _filemap[iodevice] : null;
        }

        /// <summary>
        /// Retrieve the IOFile for the given filename.
        /// </summary>
        /// <param name="filename">The file name</param>
        /// <returns>The IOFile instance for the file or null if the file is not opened</returns>
        public static IOFile Get(string filename) {
            foreach (KeyValuePair<int, IOFile> pair in _filemap) {
                IOFile ioFile = pair.Value;
                if (ioFile.Path == filename) {
                    return ioFile;
                }
            }
            return null;
        }

        /// <summary>
        /// Return the next available free file number. This is absolutely NOT thread
        /// safe for obvious reasons but it attempts to avoid collisions by generating
        /// a random iodevice not in the _filemap table from a range large enough to
        /// be unique.
        /// </summary>
        /// <returns>Free file number</returns>
        public static int FreeFileNumber() {
            Random rand = new();
            int iodevice;

            do {
                iodevice = rand.Next(1, int.MaxValue);
            } while (_filemap.ContainsKey(iodevice));
            return iodevice;
        }

        /// <summary>
        /// A predefined IOFile object that represents the standard
        /// input device.
        /// </summary>
        public static IOFile StdinFile { get => stdinFile; set => stdinFile = value; }

        /// <summary>
        /// A predefined IOFile object that represents the standard
        /// output device.
        /// </summary>
        public static IOFile StdoutFile { get => stdoutFile; set => stdoutFile = value; }

        /// <summary>
        /// Physically open the file.
        /// </summary>
        /// <returns>True if file opened, false otherwise</returns>
        public bool Open() {
            FileMode mode;
            FileAccess accessMode;

            if (IsScratch || IsNew) {
                mode = FileMode.Create;
                accessMode = FileAccess.ReadWrite;
            } else {
                mode = FileMode.Open;
                accessMode = FileAccess.ReadWrite;
            }

            // If no filename supplied then we make one using the standard
            // FORTRAN unit specific filename convention.
            if (Path == null) {
                Path = $"FORT.{Unit:00}";
                if (!File.Exists(Path)) {
                    mode = FileMode.Create;
                    accessMode = FileAccess.ReadWrite;
                }
            }
            try {
                Handle = File.Open(Path, mode, accessMode);
            } catch (IOException) {
                Handle = null;
            } catch (UnauthorizedAccessException) {
                Handle = null;
            }
            return Handle != null;
        }

        /// <summary>
        /// Close all opened files.
        /// </summary>
        public static void CloseAll() {

            foreach (IOFile s in _filemap.Select(p => p.Value).ToList()) {
                s.Close(false);
            }
        }

        /// <summary>
        /// Close this IOFile and remove it from the list of open
        /// devices. If deleteFile is set, also delete it.
        /// </summary>
        /// <param name="deleteFile">True if the file is to be deleted after closing</param>
        public void Close(bool deleteFile) {
            Flush();
            if (Handle != null) {
                Handle.Close();
            }
            if (deleteFile && Path != null) {
                File.Delete(Path);
            }
            if (Unit != IOConstant.Stdin && Unit != IOConstant.Stdout) {
                if (_filemap.ContainsKey(Unit)) {
                    _filemap.Remove(Unit);
                }
            }
        }

        /// <summary>
        /// Seek to the end of the file
        /// </summary>
        public void SeekToEnd() {
            Handle.Seek(0, SeekOrigin.End);
        }

        /// <summary>
        /// Write an EOF record at the current position. Only valid in sequential
        /// mode.
        /// </summary>
        /// <returns>True if we succeeded, false if there was an error</returns>
        public bool EndFile() {
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (Handle == null) {
                Open();
                if (Handle == null) {
                    return false;
                }
            }
            if (IsSequential) {
                long currentPos = CurrentPositionInFile();
                Handle.SetLength(currentPos);
            }
            ClearReadBuffer();
            return true;
        }

        /// <summary>
        /// Rewinds the file back to the beginning. This is supported for both sequential
        /// and direct access files. Execution of a REWIND statement for a file that is
        /// connected but does not exist is permitted but has no effect.
        /// </summary>
        public void Rewind() {
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (Handle != null) {
                if (IsSequential) {
                    Handle.Seek(0, SeekOrigin.Begin);
                }
                ClearReadBuffer();
            }
        }

        /// <summary>
        /// Does a backspace to the previous record.
        /// </summary>
        /// <returns>True if the backspace succeeded, false otherwise.</returns>
        public bool Backspace() {
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().Name);
            }

            bool returnValue = false;
            if (Handle != null && IsSequential) {
                returnValue = IsFormatted ? FormattedBackspace() : UnformattedBackspace();
            }
            return returnValue;
        }

        /// <summary>
        /// Skip to the start of the next record. The return value
        /// is the number of characters or bytes skipped. A value of
        /// zero means we've hit the end of the file.
        /// </summary>
        /// <returns>The number of bytes skipped</returns>
        public int SkipRecord() {
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (Handle == null) {
                return -1;
            }
            int skipCount = 0;
            if (IsFormatted) {
                byte outByte = 0;
                while (ReadByte(ref outByte) && outByte != '\n') {
                    ++skipCount;
                }
            } else {
                skipCount = ReadRecord();
            }
            return skipCount;
        }

        /// <summary>
        /// Flush any unwritten data from the write buffer to disk.
        /// </summary>
        public void Flush() {
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (_writeBufferIndex > 0) {
                WriteRecord();
            }
        }
        
        /// <summary>
        /// If this file is opened for reading, read a line from the device. If the
        /// file is opened non-formatted then this has no effect.
        /// </summary>
        /// <returns>A line of text, or null if there is an error.</returns>
        public virtual string ReadLine() {
            string stringRead;

            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (Handle == null || !IsFormatted) {
                return null;
            }
            if (RecordLength == 0) {
                stringRead = ReadFormattedLine();
            } else {
                byte [] data = new byte[RecordLength];
                int bytesRead = ReadBytes(data, RecordLength);
                stringRead = Encoding.ASCII.GetString(data, 0, bytesRead);
            }
            return stringRead;
        }

        /// <summary>
        /// Write a string to the device. Note that the string is treated as a single line
        /// and includes an implied newline if the output device is not direct access. Any
        /// embedded newlines are written but do not contribute the the record count.
        /// </summary>
        /// <param name="str">The string to write</param>
        /// <param name="carriageAtEnd">Whether a carriage return is appended</param>
        /// <returns>The number of characters written to the device</returns>
        public virtual int WriteLine(string str, bool carriageAtEnd) {
            int charsWritten;
            
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (Handle == null || !IsFormatted) {
                return -1;
            }
            if (RecordLength == 0) {
                string strToWrite = str;
                if (carriageAtEnd) {
                    strToWrite += "\r\n";
                }
                charsWritten = WriteEncodedString(strToWrite, strToWrite.Length);
            } else {
                charsWritten = WriteEncodedString(str, RecordLength);
            }
            Flush();
            return charsWritten;
        }

        /// <summary>
        /// Read the specified number of characters from the file stream.
        /// </summary>
        /// <param name="count">The number of characters to read</param>
        /// <returns>The string read</returns>
        public virtual string ReadChars(int count) {
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().Name);
            }
            string value = string.Empty;
            ReadCharacters(ref value, count);
            return value;
        }

        /// <summary>
        /// Read an integer from the file stream.
        /// </summary>
        /// <param name="intValue">A reference to the integer to be set</param>
        /// <returns>The number of bytes read</returns>
        public int ReadInteger(ref int intValue) {
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (IsFormatted) {
                StringBuilder strBuilder = new();

                char ch = ReadNonSpaceChar();
                if (ch == EOF) {
                    return 0;
                }
                int charsRead = 0;
                while (char.IsDigit(ch) || ch == '+' || ch == '-') {
                    strBuilder.Append(ch);
                    ch = ReadChar();
                    ++charsRead;
                }
                SkipSeparators(ch);
                if (strBuilder.Length > 0) {
                    intValue = Convert.ToInt32(strBuilder.ToString());
                }
                return charsRead;
            } else {
                int intSize = sizeof(int);
                byte[] intBuffer = new byte[intSize];
                if (ReadBytes(intBuffer, intSize) != intSize) {
                    return 0;
                }
                intValue = BitConverter.ToInt32(intBuffer, 0);
                return intSize;
            }
        }

        /// <summary>
        /// Write an integer to the file stream.
        /// </summary>
        /// <param name="value">The integer value to write</param>
        /// <returns>The number of bytes written</returns>
        public int WriteInteger(int value) {
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (Handle == null) {
                return -1;
            }
            int intSize = sizeof(int);
            byte [] intBuffer = BitConverter.GetBytes(value);
            WriteBytes(intBuffer, intSize);
            return intSize;
        }

        /// <summary>
        /// Read a floating point value from the file stream.
        /// </summary>
        /// <param name="floatValue">A reference to the float to be set</param>
        /// <returns>The number of bytes read</returns>
        public int ReadFloat(ref float floatValue) {
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (IsFormatted) {
                StringBuilder strBuilder = new();

                char ch = ReadNonSpaceChar();
                if (ch == EOF) {
                    return 0;
                }
                int charsRead = 0;
                List<char> validFloatChars = new() { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'E', '-', '+', '.' };
                while (validFloatChars.Contains(ch)) {
                    strBuilder.Append(ch);
                    ch = ReadChar();
                    ++charsRead;
                }
                SkipSeparators(ch);
                if (strBuilder.Length > 0) {
                    floatValue = Convert.ToSingle(strBuilder.ToString());
                }
                return charsRead;
            } else {
                int intSize = sizeof(float);
                byte[] intBuffer = new byte[intSize];
                if (ReadBytes(intBuffer, intSize) != intSize) {
                    return 0;
                }
                floatValue = BitConverter.ToSingle(intBuffer, 0);
                return intSize;
            }
        }

        /// <summary>
        /// Write an float to the file stream.
        /// </summary>
        /// <param name="value">The float value to write</param>
        /// <returns>The number of bytes written</returns>
        public int WriteFloat(float value) {
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (Handle == null) {
                return -1;
            }
            int intSize = sizeof(float);
            byte [] intBuffer = BitConverter.GetBytes(value);
            WriteBytes(intBuffer, intSize);
            return intSize;
        }

        /// <summary>
        /// Read a double value from the file stream.
        /// </summary>
        /// <param name="doubleValue">A reference to the double to be set</param>
        /// <returns>The number of bytes read</returns>
        public int ReadDouble(ref double doubleValue) {
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (IsFormatted) {
                StringBuilder strBuilder = new();

                char ch = ReadNonSpaceChar();
                if (ch == EOF) {
                    return 0;
                }
                int charsRead = 0;
                List<char> validDoubleChars = new() { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'D', '-', '+', '.' };
                while (validDoubleChars.Contains(ch)) {
                    strBuilder.Append(ch);
                    ch = ReadChar();
                    ++charsRead;
                }
                SkipSeparators(ch);
                if (strBuilder.Length > 0) {
                    doubleValue = Convert.ToDouble(strBuilder.ToString());
                }
                return charsRead;
            } else {
                int intSize = sizeof(double);
                byte[] intBuffer = new byte[intSize];
                if (ReadBytes(intBuffer, intSize) != intSize) {
                    return 0;
                }
                doubleValue = BitConverter.ToDouble(intBuffer, 0);
                return intSize;
            }
        }

        /// <summary>
        /// Write a double to the file stream.
        /// </summary>
        /// <param name="value">The double value to write</param>
        /// <returns>The number of bytes written</returns>
        public int WriteDouble(double value) {
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (Handle == null) {
                return -1;
            }
            int intSize = sizeof(double);
            byte [] intBuffer = BitConverter.GetBytes(value);
            WriteBytes(intBuffer, intSize);
            return intSize;
        }

        /// <summary>
        /// Read a boolean from the file stream.
        /// </summary>
        /// <param name="boolValue">A reference to the boolean to be set</param>
        /// <returns>The number of bytes read</returns>
        public int ReadBoolean(ref bool boolValue) {
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().Name);
            }
            int intSize = sizeof(bool);
            byte [] intBuffer = new byte[intSize];
            if (ReadBytes(intBuffer, intSize) != intSize) {
                return 0;
            }
            boolValue = BitConverter.ToBoolean(intBuffer, 0);
            return intSize;
        }

        /// <summary>
        /// Write a boolean to the file stream.
        /// </summary>
        /// <param name="value">The boolean value to write</param>
        /// <returns>The number of bytes written</returns>
        public int WriteBoolean(bool value) {
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (Handle == null) {
                return -1;
            }
            int intSize = sizeof(bool);
            byte [] intBuffer = BitConverter.GetBytes(value);
            WriteBytes(intBuffer, intSize);
            return intSize;
        }

        /// <summary>
        /// Read a given number of characters from the data file.
        /// </summary>
        /// <param name="strValue">A reference to the string to be set</param>
        /// <param name="count">Maximum number of characters to read</param>
        /// <returns>The number of bytes read</returns>
        public int ReadCharacters(ref string strValue, int count) {
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (IsFormatted) {
                StringBuilder strBuilder = new();

                if (count > 0) {
                    char ch = ReadChar();
                    int charsRead = 0;
                    while (ch != EOF && charsRead < count) {
                        strBuilder.Append(ch);
                        ch = ReadChar();
                        ++charsRead;
                    }
                    SkipSeparators(ch);
                }
                strValue = strBuilder.ToString();
                return strValue.Length;
            } else {
                int intSize = sizeof(int);
                byte[] intBuffer = new byte[intSize];
                if (ReadBytes(intBuffer, intSize) != intSize) {
                    return 0;
                }
                int stringLength = BitConverter.ToInt32(intBuffer, 0);
                byte[] data = new byte[stringLength];
                int bytesRead = ReadBytes(data, stringLength);
                strValue = Encoding.ASCII.GetString(data, 0, bytesRead);
                return bytesRead + sizeof(int);
            }
        }

        /// <summary>
        /// Read a string from an formatted data file into a fixed string.
        /// The string is truncated to fit the fixed string length.
        /// </summary>
        /// <param name="strValue">A reference to the string to be set</param>
        /// <returns>The number of bytes read</returns>
        public int ReadString(ref FixedString strValue, int count) {
            string value = null;
            ReadString(ref value, count);
            strValue.Set(value);
            return count;
        }

        /// <summary>
        /// Read a string from an unformatted data file. The first part of the
        /// string in the file is the length as an integer. The string follows
        /// on from the length.
        /// </summary>
        /// <param name="strValue">A reference to the string to be set</param>
        /// <param name="count">Maximum number of characters to read</param>
        /// <returns>The number of bytes read</returns>
        public int ReadString(ref string strValue, int count) {
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (IsFormatted) {
                StringBuilder strBuilder = new();

                if (count > 0) {
                    char ch = ReadChar();
                    int charsRead = 0;
                    while (ch != EOL && charsRead < count) {
                        if (ch == '\r') {
                            ch = ReadChar();
                            break;
                        }
                        strBuilder.Append(ch);
                        ch = ReadChar();
                        ++charsRead;
                    }
                    SkipSeparators(ch);
                }
                strValue = strBuilder.ToString();
                return strValue.Length;
            } else {
                int intSize = sizeof(int);
                byte[] intBuffer = new byte[intSize];
                if (ReadBytes(intBuffer, intSize) != intSize) {
                    return 0;
                }
                int stringLength = BitConverter.ToInt32(intBuffer, 0);
                byte[] data = new byte[stringLength];
                int bytesRead = ReadBytes(data, stringLength);
                strValue = Encoding.ASCII.GetString(data, 0, bytesRead);
                return bytesRead + sizeof(int);
            }
        }

        /// <summary>
        /// Write a string to an unformatted data file. The length of the
        /// string as an integer is written first, followed by the string itself.
        /// </summary>
        /// <param name="value">The string to write</param>
        /// <returns>The number of bytes written</returns>
        public int WriteString(string value) {
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (Handle == null) {
                return -1;
            }
            int intSize = sizeof(int);
            byte [] intBuffer = BitConverter.GetBytes(value.Length);
            WriteBytes(intBuffer, intSize);
            WriteEncodedString(value, value.Length);
            return intSize + value.Length;
        }
        
        /// <summary>
        /// Specifies whether or not the file is opened for formatted access.
        /// </summary>
        public virtual bool IsFormatted {
            get => _isFormatted;
            set {
                if (_isDisposed) {
                    throw new ObjectDisposedException(GetType().Name);
                }
                _isFormatted = value;
            }
        }

        /// <summary>
        /// Return whether we've reached the end of the file.
        /// </summary>
        public bool IsEndOfFile {
            get {
                if (IsFormatted) {
                    return PeekChar() == EOF;
                }
                return Handle.Position == Handle.Length;
            }
        }

        /// <summary>
        /// Specifies the record length. A value of 0 means no length.
        /// </summary>
        public int RecordLength {
            get => _recordLength;
            set {
                if (_isDisposed) {
                    throw new ObjectDisposedException(GetType().Name);
                }
                _recordLength = value;
                if (_recordLength > 0) {
                    _writeBuffer = new byte[_recordLength];
                    _writeBufferIndex = 0;
                }
            }
        }

        /// <summary>
        /// Gets or sets the current record number for direct access.
        /// </summary>
        public int RecordIndex {
            get => _recordIndex;
            set {
                if (_isDisposed) {
                    throw new ObjectDisposedException(GetType().Name);
                }
                if (value < 1) {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                if (_recordIndex != value) {
                    _recordIndex = value;
                    if (RecordLength > 0 && Handle != null) {
                        int blockSize = RecordLength;
                        if (!IsFormatted) {
                            blockSize += sizeof(int) * 2;
                        }
                        if (_recordIndex * (long)blockSize > Handle.Length) {
                            Handle.SetLength(_recordIndex * (long)blockSize);
                        }
                        Handle.Seek((_recordIndex - 1) * (long)blockSize, SeekOrigin.Begin);
                        ClearReadBuffer();
                    } else if (!IsFormatted) {
                        ClearReadBuffer();
                    }
                }
            }
        }

        /// <summary>
        /// Releases all resource used by the <see cref="IOFile"/> object.
        /// </summary>
        public void Dispose() {
            Dispose(true);
        }
        
        /// <summary>
        /// Releases all resource used by the <see cref="IOFile"/> object.
        /// </summary>
        /// <param name="disposing">True if we're disposing</param>
        protected virtual void Dispose(bool disposing) {
            if (!_isDisposed) {
                if (disposing) {
                    if (Handle != null) {
                        Handle.Dispose();
                    }
                }
                _isDisposed = true;
            }
        }

        // Read a line of text from the data file where the line is terminated by
        // a newline character. The newline character is not included in the returned
        // string.
        private string ReadFormattedLine() {
            int bufferSize = 256;
            int bufferIndex = 0;
            byte [] data = new byte[bufferSize];

            while (true) {
                if (bufferIndex == bufferSize) {
                    bufferSize += 256;
                    Array.Resize(ref data, bufferSize);
                }
                byte outByte = 0;
                if (!ReadByte(ref outByte)) {
                    if (bufferIndex == 0) {
                        return null;
                    }
                    break;
                }
                if (outByte == '\n') {
                    break;
                }
                data[bufferIndex++] = outByte;
            }
            return Encoding.ASCII.GetString(data, 0, bufferIndex);
        }

        // Read a given number of bytes from the data file into the
        // data buffer and return the actual number of bytes read which may
        // be less than requested if we hit the EOF.
        private int ReadBytes(byte [] data, int bytesToRead) {
            int bufferIndex = 0;
            while (bufferIndex < bytesToRead) {
                byte outByte = 0;
                if (!ReadByte(ref outByte)) {
                    break;
                }
                data[bufferIndex++] = outByte;
            }
            return bufferIndex;
        }

        // Read a single byte from the data file.
        private bool ReadByte(ref byte outByte) {
            if (_readBufferIndex == _readBufferSize) {
                ReadRecord();
            }
            if (_readBufferSize == 0) {
                return false;
            }
            outByte = _readBuffer[_readBufferIndex++];
            return true;
        }

        // Write a series of bytes to the write buffer.
        private void WriteBytes(byte[] data, int bytesToWrite) {
            int dataIndex = 0;

            while (dataIndex < bytesToWrite) {
                if (_writeBufferIndex == _writeBufferSize) {
                    _writeBufferSize += 256;
                    Array.Resize(ref _writeBuffer, _writeBufferSize);
                }
                _writeBuffer[_writeBufferIndex++] = data[dataIndex++];
            }
        }

        // Return the current position in the file which is the current position
        // of the internal file pointer, minus the index into the local buffer.
        private long CurrentPositionInFile() {
            return Handle.Seek(0L, SeekOrigin.Current) - (_readBufferSize - _readBufferIndex);
        }

        // Reset the read buffer so that the next call to ReadByte triggers
        // a read from the file. This is required when doing direct access or
        // issuing a positional call.
        private void ClearReadBuffer() {
            _readBufferSize = 0;
            _readBufferIndex = 0;
        }

        // Write a string encoded into the supported output format for the data file. If the string
        // is less than charsToWrite, it is left padded with spaces. Otherwise if it is longer then
        // it is truncated.
        private int WriteEncodedString(string strToWrite, int charsToWrite) {
            int count = Math.Min(Encoding.ASCII.GetByteCount(strToWrite), charsToWrite);
            byte [] data = new byte[charsToWrite];
            
            // Make sure that unused elements are set to spaces and not
            // NUL characters or bad things will happen on the READ.
            for (int c = 0; c < charsToWrite; ++c) {
                data[c] = (byte)' ';
            }
            
            Encoding.ASCII.GetBytes(strToWrite, 0, count, data, 0);
            WriteBytes(data, charsToWrite);
            return charsToWrite;
        }

        // Backspace one record in an unformatted file
        // At the time we're called, we'll always be pointing to the start
        // of the next record. The previous 'size' record will be just before
        // the current file position and will allow us to go back in offset
        // chunks.
        private bool UnformattedBackspace() {
            long currentPos = CurrentPositionInFile();
            
            // At ENDFILE? If so, exit now
            if (currentPos == Handle.Length || currentPos == 0) {
                return true;
            }
            int bufferSize = sizeof(int);
            byte [] data = new byte[bufferSize];

            int bytesToRead = bufferSize;
            currentPos -= bytesToRead;
            Handle.Seek(currentPos, SeekOrigin.Begin);
            int bytesRead = Handle.Read(data, 0, bytesToRead);

            // If less than 2 size records left then something went
            // wrong so bail out now.
            if (bytesRead < sizeof(int) * 2) {
                return false;
            }

            int intSize = BitConverter.ToInt32(data, 0);
            currentPos -= intSize + sizeof(int);
            if (currentPos < 0) {
                return false;
            }

            Handle.Seek(currentPos, SeekOrigin.Begin);
            return true;
        }

        // Backspace one record in a formatted file.
        // This basically involves reading a block of data one block from
        // where we are and then scanning back in the block for the first
        // newline or the start of the file. From that we can compute the
        // offset of the record in the file.
        private bool FormattedBackspace() {
            long currentPos = CurrentPositionInFile();
            
            // Do it the hard way. Work backward reading 4K chunks and look for
            // the last newline record in the block. The new position is just
            // after that.
            int bufferSize = 4096;
            byte [] data = new byte[bufferSize];
            int startOffset = 0;
            
            currentPos -= 1;
            while (currentPos > 0) {
                bool foundRecord = false;
                
                int bytesToRead = (int)Math.Min(currentPos, bufferSize);
                currentPos = Math.Max(startOffset, currentPos - bufferSize);
                Handle.Seek(currentPos, SeekOrigin.Begin);
                int bytesRead = Handle.Read(data, 0, bytesToRead);
                while (bytesRead-- > 0) {
                    if (data[bytesRead] == '\n') {
                        currentPos += bytesRead + 1;
                        foundRecord = true;
                        break;
                    }
                }
                if (foundRecord) {
                    break;
                }
            }
            Handle.Seek(currentPos, SeekOrigin.Begin);
            ClearReadBuffer();
            return true;
        }

        // For unformatted data, read the next record from the file. The
        // first part of the record is the record size, followed by the
        // record itself.
        private int ReadRecord() {
            if (IsFormatted) {
                if (_readBuffer == null) {
                    _readBuffer = new byte[4096];
                }
                _readBufferSize = Handle.Read(_readBuffer, 0, 4096);
            } else {
                int intSize = sizeof(int);
                byte [] intBuffer = new byte[intSize];
                Handle.Read(intBuffer, 0, intSize);

                _readBufferSize = BitConverter.ToInt32(intBuffer, 0);
                _readBuffer = new byte[_readBufferSize];
                _readBufferSize = Handle.Read(_readBuffer, 0, _readBufferSize);

                // Skip past the record size at the end.
                Handle.Read(intBuffer, 0, intSize);
            }
            _readBufferIndex = 0;
            return _readBufferSize;
        }

        // For unformatted data, write the current record to the file. The first
        // part of the record is the record size, followed by the record itself.
        private void WriteRecord() {
            int bytesToWrite = RecordLength;

            if (bytesToWrite == 0) {
                bytesToWrite = _writeBufferIndex;
            }
            if (IsFormatted) {
                if (bytesToWrite > 0) {
                    Handle.Write(_writeBuffer, 0, bytesToWrite);
                }
            } else {
                int intSize = sizeof(int);
                byte [] intBuffer = BitConverter.GetBytes(bytesToWrite);

                // The record format for an unformatted file is:
                //
                //   ssssddd...dddssss
                //
                // where ssss is the data size as an integer (typically 32-bits),
                // and ddd is the actual variable length data. The size is written
                // to the end of the record to faciliate backspacing.
                Handle.Write(intBuffer, 0, intSize);
                Handle.Write(_writeBuffer, 0, bytesToWrite);
                Handle.Write(intBuffer, 0, intSize);
            }
            ++_recordIndex;
            _writeBufferIndex = 0;
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

        // Read the next line from the input.
        private void ReadNextLine() {
            _line = ReadLine();
            _readIndex = 0;
        }
    }
}
