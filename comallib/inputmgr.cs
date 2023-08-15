// COMAL Runtime Library
// Implements the InputManager class
//
// Authors:
//  Steve Palmer
//
// Copyright (C) 2021 Steve Palmer
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

using JComLib;

namespace JComalLib {

    public class InputManager {

        private readonly IOFile _file;

        /// <summary>
        /// Construct an InputManager for sequential file I/O.
        /// </summary>
        /// <param name="iodevice">The device to read from</param>
        /// <param name="recordNumber">The number of the record to read</param>
        public InputManager(int iodevice) {

            _file = IOFile.Get(iodevice);
            if (_file == null) {
                throw new JComRuntimeException(JComRuntimeErrors.FILE_NOT_OPEN);
            }
            if (_file.RecordLength > 0) {
                throw new JComRuntimeException(JComRuntimeErrors.FILE_OPEN_FOR_RANDOM_ACCESS);
            }
            _file.IsFormatted = false;
        }

        /// <summary>
        /// Construct an InputManager for random access file I/O.
        /// </summary>
        /// <param name="iodevice">The device to read from</param>
        /// <param name="recordNumber">The number of the record to read</param>
        public InputManager(int iodevice, int recordNumber) {

            _file = IOFile.Get(iodevice);
            if (_file == null) {
                throw new JComRuntimeException(JComRuntimeErrors.FILE_NOT_OPEN);
            }
            if (_file.RecordLength == 0) {
                throw new JComRuntimeException(JComRuntimeErrors.FILE_NOT_OPEN_FOR_RANDOM_ACCESS);
            }
            _file.IsFormatted = false;
            _file.RecordIndex = recordNumber;
        }

        /// <summary>
        /// Construct an InputManager for repeated input calls from the specified
        /// device.
        /// </summary>
        /// <param name="iodevice">The device to read from</param>
        /// <param name="promptString">Optional prompt string</param>
        /// <param name="carriageReturn">Whether to issue a newline at the end of input</param>
        public InputManager(int iodevice, string promptString, LineTerminator terminator):
            this(0, 0, -1, iodevice, promptString, terminator) {
        }

        /// <summary>
        /// Construct an InputManager for repeated input calls from the specified
        /// device. For input from the console, a row and column can be specified to indicate
        /// where the input field occurs on the screen. The maxWidth indicates the maximum number
        /// of characters to input, or 0 if no constraint is required. The prompt string is
        /// displayed at the (row,column) if specified or the current cursor position. If no
        /// prompt is specified, a "?" is displayed instead.
        ///
        /// For input from files, the row, column, maxWidth and promptString are ignored.
        /// </summary>
        /// <param name="row">Row at which console input starts</param>
        /// <param name="column">Column at which console input starts</param>
        /// <param name="maxWidth">Maximum input width</param>
        /// <param name="iodevice">The device to read from</param>
        /// <param name="promptString">Optional prompt string</param>
        /// <param name="carriageReturn">Whether to issue a newline at the end of input</param>
        public InputManager(int row, int column, int maxWidth, int iodevice, string promptString, LineTerminator terminator) {

            _file = IOFile.Get(iodevice);
            if (_file == null) {
                throw new JComRuntimeException(JComRuntimeErrors.FILE_NOT_OPEN);
            }

            if (string.IsNullOrEmpty(promptString)) {
                promptString = "? ";
            }

            // For console input, display the prompt. For any other file, the
            // prompt is ignored.
            if (_file is StdinIOFile) {
                StdinIOFile file = _file as StdinIOFile;
                file.Terminator = terminator;
                file.Width = maxWidth;
                file.Zone = FileManager.Zone;

                IOFile stdoutFile = new StdoutIOFile();
                if (row > 0 && column > 0) {
                    Console.SetCursorPosition(column - 1, row - 1);
                }
                stdoutFile.WriteLine(promptString, false);
            }
        }

        /// <summary>
        /// Reads from the input into the specified float variable.
        /// </summary>
        /// <param name="floatValue">Float value to be read</param>
        public void READ(ref float floatValue) {
            _file.ReadFloat(ref floatValue);
        }

        /// <summary>
        /// Reads from the input into the specified float array.
        /// </summary>
        /// <param name="floatArray">Float array to be read</param>
        public void READ(ref float [] floatArray) {
            for (int index = 0; index < floatArray.Length; index++) {
                _file.ReadFloat(ref floatArray[index]);
            }
        }

        /// <summary>
        /// Reads from the input into the specified integer variable.
        /// </summary>
        /// <param name="intValue">Integer value that was read</param>
        public void READ(ref int intValue) {
            _file.ReadInteger(ref intValue);
        }

        /// <summary>
        /// Reads from the input into the specified integer array.
        /// </summary>
        /// <param name="intArray">Integer array to be read</param>
        public void READ(ref int[] intArray) {
            for (int index = 0; index < intArray.Length; index++) {
                _file.ReadInteger(ref intArray[index]);
            }
        }

        /// <summary>
        /// Reads from the input into the specified string variable.
        /// </summary>
        /// <param name="stringData">String value that was read</param>
        public void READ(ref string stringValue) {
            _file.ReadString(ref stringValue, int.MaxValue);
        }

        /// <summary>
        /// Reads from the input into the specified string array.
        /// </summary>
        /// <param name="stringArray">String array to be read</param>
        public void READ(ref string[] stringArray) {
            for (int index = 0; index < stringArray.Length; index++) {
                _file.ReadString(ref stringArray[index], int.MaxValue);
            }
        }

        /// <summary>
        /// Reads from the input into the specified fixed string variable.
        /// </summary>
        /// <param name="fixedStringValue">String value that was read</param>
        public void READ(ref FixedString fixedStringValue) {
            _file.ReadString(ref fixedStringValue, int.MaxValue);
        }

        /// <summary>
        /// Reads from the input into the specified fixed string array.
        /// </summary>
        /// <param name="fixedstringArray">String array to be read</param>
        public void READ(ref FixedString[] fixedstringArray) {
            for (int index = 0; index < fixedstringArray.Length; index++) {
                _file.ReadString(ref fixedstringArray[index], int.MaxValue);
            }
        }
    }
}