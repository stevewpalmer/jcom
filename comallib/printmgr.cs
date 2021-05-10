// COMAL Runtime Library
// Implements the PrintManager class
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

using System;
using System.Text;
using JComLib;

namespace JComalLib {

    /// <summary>
    /// Implements PrintManager.
    /// </summary>
    public class PrintManager {

        /// <summary>
        /// Writes the specified string to the stdout device followed by a newline.
        /// </summary>
        /// <param name="stringToWrite">String to write</param>
        public static void WRITE(string stringToWrite) {
            IOFile file = IOFile.Get(IOConstant.Stdout);
            file.WriteLine(stringToWrite, true);
        }

        /// <summary>
        /// Writes the specified string to the stdout device followed by a newline.
        /// </summary>
        /// <param name="stringToWrite">String to write</param>
        public static void WRITE(FixedString stringToWrite) {
            IOFile file = IOFile.Get(IOConstant.Stdout);
            file.WriteLine(stringToWrite.ToString(), true);
        }

        /// <summary>
        /// Writes the specified string to the stdout device followed by a newline.
        /// </summary>
        /// <param name="row">Row at which to write. 0 means don't change the row.</param>
        /// <param name="column">Column at which to write. 0 means don't change the column.</param>
        /// <param name="stringToWrite">String to write</param>
        public static void WRITE(int row, int column, string stringToWrite) {
            IOFile file = IOFile.Get(IOConstant.Stdout);
            if (row > 0 && column > 0) {
                Console.SetCursorPosition(column - 1, row - 1);
            }
            file.WriteLine(stringToWrite, true);
        }

        /// <summary>
        /// Writes the specified string to the stdout device followed by a newline.
        /// </summary>
        /// <param name="row">Row at which to write. 0 means don't change the row.</param>
        /// <param name="column">Column at which to write. 0 means don't change the column.</param>
        /// <param name="stringToWrite">String to write</param>
        public static void WRITE(int row, int column, FixedString stringToWrite) {
            IOFile file = IOFile.Get(IOConstant.Stdout);
            if (row > 0 && column > 0) {
                Console.SetCursorPosition(column - 1, row - 1);
            }
            file.WriteLine(stringToWrite.ToString(), true);
        }

        /// <summary>
        /// Writes the specified data to the given I/O device using the format string.
        /// </summary>
        /// <param name="iodevice">The device to read from</param>
        /// <param name="formatString">Format string</param>
        /// <param name="args">Write arguments</param>
        public static void WRITE(int iodevice, string formatString, params object[] args) {
            WRITE(0, 0, iodevice, formatString, args);
        }

        /// <summary>
        /// Writes the specified data to the given I/O device using the format string.
        /// </summary>
        /// <param name="iodevice">The device to read from</param>
        /// <param name="formatString">Format string</param>
        /// <param name="args">Write arguments</param>
        public static void WRITE(int iodevice, FixedString formatString, params object[] args) {
            WRITE(0, 0, iodevice, formatString.ToString(), args);
        }

        /// <summary>
        /// Writes the specified data to the given I/O device using the format string.
        /// </summary>
        /// <param name="row">Row at which to write. 0 means don't change the row.</param>
        /// <param name="column">Column at which to write. 0 means don't change the column.</param>
        /// <param name="iodevice">The device to read from</param>
        /// <param name="formatString">Format string</param>
        /// <param name="args">Write arguments</param>
        public static void WRITE(int row, int column, int iodevice, FixedString formatString, params object[] args) {
            WRITE(row, column, iodevice, formatString.ToString(), args);
        }

        /// <summary>
        /// Writes the specified data to the given I/O device using the format string.
        /// </summary>
        /// <param name="row">Row at which to write. 0 means don't change the row.</param>
        /// <param name="column">Column at which to write. 0 means don't change the column.</param>
        /// <param name="iodevice">The device to read from</param>
        /// <param name="formatString">Format string</param>
        /// <param name="args">Write arguments</param>
        public static void WRITE(int row, int column, int iodevice, string formatString, params object [] args) {
            IOFile file = IOFile.Get(iodevice);
            if (file == null) {
                file = new IOFile(iodevice);
                file.Open();
            }

            char[] formats = formatString.ToCharArray();
            StringBuilder output = new();
            int fmtIndex = 0;
            int argIndex = 0;
            int fieldWidth = FileManager.Zone;
            bool useHex = false;
            if (row > 0 && column > 0 && iodevice == IOConstant.Stdout) {
                Console.SetCursorPosition(column - 1, row - 1);
            }

            // Validate zone
            if (FileManager.Zone < 0) {
                throw new JComRuntimeException(JComRuntimeErrors.ZONE_VALUE_INCORRECT);
            }

            while (fmtIndex < formatString.Length) {
                string result;
                switch (formats[fmtIndex++]) {
                    case 'H':
                        fieldWidth = FileManager.Zone;
                        break;
                    case 'V':
                        fieldWidth = 0;
                        output.Append(' ');
                        break;
                    case '6':
                        useHex = true;
                        continue;
                    case 'N':
                        file.WriteLine(output.ToString(), true);
                        output.Clear();
                        break;
                    case 'X': {
                            if (args[argIndex++] is int intValue) {
                                output.Append(new string(' ', intValue));
                            }
                            break;
                        }
                    case 'T': {
                            if (args[argIndex++] is int intValue) {
                                if (output.Length > intValue) {
                                    file.WriteLine(output.ToString(), true);
                                    output.Clear();
                                }
                                output.Append(new string(' ', intValue - output.Length));
                            }
                            break;
                        }
                    case 'S': {
                            if (fmtIndex < formats.Length && formats[fmtIndex] == 'V') {
                                fieldWidth = 0;
                            }
                            if (fmtIndex < formats.Length && formats[fmtIndex] == 'H') {
                                fieldWidth = FileManager.Zone;
                            }
                            string str;
                            if (args[argIndex] is FixedString) {
                                str = (args[argIndex] as FixedString).ToString();
                            } else {
                                str = args[argIndex] as string;
                            }
                            argIndex++;
                            if (str != null) {
                                result = str;
                                if (fieldWidth > 0 && result.Length < fieldWidth) {
                                    result = result.PadRight(fieldWidth);
                                }
                                output.Append(result);
                            }
                            break;
                        }
                    case 'I': {
                            if (args[argIndex++] is int intValue) {
                                result = useHex ? intValue.ToString("X") : intValue.ToString();
                                if (fieldWidth > 0 && result.Length < fieldWidth) {
                                    result = result.PadLeft(fieldWidth);
                                }
                                output.Append(result);
                            }
                            break;
                        }
                    case 'F': {
                            if (args[argIndex++] is float floatValue) {
                                result = useHex ? ((int)floatValue).ToString("X") : floatValue.ToString();
                                if (fieldWidth > 0 && result.Length < fieldWidth) {
                                    result = result.PadLeft(fieldWidth);
                                }
                                output.Append(result);
                            }
                            break;
                        }
                }
                useHex = false;
            }
            bool hasNewline = fmtIndex == 0 || (formats[fmtIndex - 1] != 'V' && formats[fmtIndex - 1] != 'H');
            file.WriteLine(output.ToString(), hasNewline);

            if (file != null) {
                file.Flush();
            }
        }
    }
}