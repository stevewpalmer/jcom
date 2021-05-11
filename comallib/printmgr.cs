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
            WRITE(0, 0, IOConstant.Stdout, "S", new object[] { stringToWrite });
        }

        /// <summary>
        /// Writes the specified string to the stdout device followed by a newline.
        /// </summary>
        /// <param name="stringToWrite">String to write</param>
        public static void WRITE(FixedString stringToWrite) {
            WRITE(0, 0, IOConstant.Stdout, "S", new object[] { stringToWrite });
        }

        /// <summary>
        /// Writes the specified string array to the stdout device followed by a newline.
        /// </summary>
        /// <param name="stringArray">String array to write</param>
        public static void WRITE(FixedString[] stringArray) {
            WRITE(0, 0, IOConstant.Stdout, "S", new object[] { stringArray });
        }

        /// <summary>
        /// Writes the specified string array to the stdout device followed by a newline.
        /// </summary>
        /// <param name="stringArray">String array to write</param>
        public static void WRITE(string[] stringArray) {
            WRITE(0, 0, IOConstant.Stdout, "S", new object[] { stringArray });
        }

        /// <summary>
        /// Writes the specified string to the stdout device followed by a newline.
        /// </summary>
        /// <param name="row">Row at which to write. 0 means don't change the row.</param>
        /// <param name="column">Column at which to write. 0 means don't change the column.</param>
        /// <param name="stringToWrite">String to write</param>
        public static void WRITE(int row, int column, string stringToWrite) {
            WRITE(row, column, IOConstant.Stdout, "S", new object[] { stringToWrite });
        }

        /// <summary>
        /// Writes the specified string to the stdout device followed by a newline.
        /// </summary>
        /// <param name="row">Row at which to write. 0 means don't change the row.</param>
        /// <param name="column">Column at which to write. 0 means don't change the column.</param>
        /// <param name="stringToWrite">String to write</param>
        public static void WRITE(int row, int column, FixedString stringToWrite) {
            WRITE(row, column, IOConstant.Stdout, "S", new object[] { stringToWrite });
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
        /// <param name="row">Row at which to write. 0 means don't change the row.</param>
        /// <param name="column">Column at which to write. 0 means don't change the column.</param>
        /// <param name="iodevice">The device to read from</param>
        /// <param name="formatString">Format string</param>
        /// <param name="args">Write arguments</param>
        public static void WRITE(int row, int column, int iodevice, string formatString, params object[] args) {

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
                            if (args[argIndex] is string[] stringArray) {
                                foreach (string stringElement in stringArray) {
                                    WriteValue(output, stringElement.ToString(), fieldWidth);
                                    output.Append(" ");
                                }
                            } else if (args[argIndex] is FixedString[] fixedStringArray) {
                                foreach (FixedString fixedStringElement in fixedStringArray) {
                                    WriteValue(output, fixedStringElement.ToString(), fieldWidth);
                                    output.Append(" ");
                                }
                            } else {
                                if (args[argIndex] is FixedString fixedString) {
                                    WriteValue(output, fixedString.ToString(), fieldWidth);
                                } else if (args[argIndex] is string nativeString) {
                                    WriteValue(output, nativeString, fieldWidth);
                                }
                            }
                            argIndex++;
                            break;
                        }
                    case 'I': {
                            if (args[argIndex] is int intValue) {
                                WriteValue(output, intValue, useHex, fieldWidth);
                            }
                            else if (args[argIndex] is int[] intArray) {
                                foreach (int intElement in intArray) {
                                    WriteValue(output, intElement, useHex, fieldWidth);
                                    output.Append(" ");
                                }
                            }
                            argIndex++;
                            break;
                        }
                    case 'F': {
                            if (args[argIndex] is float floatValue) {
                                WriteValue(output, floatValue, useHex, fieldWidth);
                            }
                            else if (args[argIndex] is float[] floatArray) {
                                foreach (float floatElement in floatArray) {
                                    WriteValue(output, floatElement, useHex, fieldWidth);
                                    output.Append(" ");
                                }
                            }
                            argIndex++;
                            break;
                        }
                }
                useHex = false;
            }
            bool hasNewline = fmtIndex == 0 || (formats[fmtIndex - 1] != 'V' && formats[fmtIndex - 1] != 'H');
            file.WriteLine(output.ToString(), hasNewline);
            file.Flush();
        }

        // Write an integer value
        private static void WriteValue(StringBuilder output, int intValue, bool useHex, int fieldWidth) {
            string result = useHex ? intValue.ToString("X") : intValue.ToString();
            if (fieldWidth > 0 && result.Length < fieldWidth) {
                result = result.PadLeft(fieldWidth);
            }
            output.Append(result);
        }

        // Write a float value
        private static void WriteValue(StringBuilder output, float floatValue, bool useHex, int fieldWidth) {
            string result = useHex ? floatValue.ToString("X") : floatValue.ToString();
            if (fieldWidth > 0 && result.Length < fieldWidth) {
                result = result.PadLeft(fieldWidth);
            }
            output.Append(result);
        }

        // Write a string value
        private static void WriteValue(StringBuilder output, string stringValue, int fieldWidth) {
            if (fieldWidth > 0 && stringValue.Length < fieldWidth) {
                stringValue = stringValue.PadRight(fieldWidth);
            }
            output.Append(stringValue);
        }
    }
}