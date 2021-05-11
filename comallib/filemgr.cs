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

using System.IO;
using JComLib;

namespace JComalLib {

    public class FileManager {

        /// <summary>
        /// Default zone.
        /// </summary>
        public const int DefaultZone = 0;

        /// <summary>
        /// Default static constructor
        /// </summary>
        static FileManager() {
            Zone = DefaultZone;
        }

        /// <summary>
        /// Get or set the default zone
        /// </summary>
        public static int Zone { get; set; }

        /// <summary>
        /// Get the next available free file number.
        /// </summary>
        public static int FREEFILE => IOFile.FreeFileNumber();

        /// <summary>
        /// Get the specified number of characters from the given device.
        /// </summary>
        /// <param name="iodevice">The file number to get characters from</param>
        /// <param name="count">The number of characters wanted</param>
        /// <returns></returns>
        public static string GET(int iodevice, int count) {

            IOFile file = IOFile.Get(iodevice);
            if (file == null) {
                throw new JComRuntimeException(JComRuntimeErrors.FILE_NOT_OPEN);
            }
            return file.ReadChars(count);
        }

        /// <summary>
        /// Open the specified file and assign it the given iodevice
        /// number. The mode string specifies how the file is opened:
        ///
        /// mode = "w" - file is created and opened for writing.
        /// mode = "r" - file is opened for reading.
        /// mode = "w+" - file is opened for writing at the end.
        /// mode = "x" - file is opened for read-write random access.
        /// 
        /// </summary>
        /// <param name="iodevice">The file number for the opened file</param>
        /// <param name="filename">The filename</param>
        /// <param name="mode">The file open mode</param>
        /// <param name="recordSize">Record size, for random access files</param>
        public static void OPEN(int iodevice, string filename, string mode, int recordSize) {

            IOFile file = IOFile.Get(iodevice);
            if (file != null) {
                throw new JComRuntimeException(JComRuntimeErrors.FILE_ALREADY_OPEN);
            }
            file = new IOFile(iodevice) {
                Path = filename
            };
            file.IsNew = mode == "w";
            file.IsSequential = mode != "x";
            if ((mode == "w+" || mode == "x") && !File.Exists(filename)) {
                file.IsNew = true;
            }
            file.Open();
            if (file.Handle == null) {
                throw new JComRuntimeException(JComRuntimeErrors.CANNOT_OPEN_FILE);
            }
            if (mode == "w+") {
                file.SeekToEnd();
            }
            if (!file.IsSequential) {
                file.RecordLength = recordSize;
                if (recordSize > 0) {
                    file.IsFormatted = false;
                }
            }
        }

        /// <summary>
        /// Creates a file and populates it with empty records of the given size.
        /// </summary>
        /// <param name="filename">Name of file being created</param>
        /// <param name="recordCount">Number of records</param>
        /// <param name="recordSize">Size of each record</param>
        public static void CREATE(string filename, int recordCount, int recordSize) {

            IOFile file = new(FREEFILE) {
                Path = filename,
                IsNew = true
            };
            file.Open();
            if (file.Handle == null) {
                throw new JComRuntimeException(JComRuntimeErrors.CANNOT_CREATE_FILE);
            }
            file.RecordLength = recordSize;
            file.RecordIndex = recordCount;
            file.Close(false);
        }

        /// <summary>
        /// Close the specified file. An exception is thrown if the file was
        /// not previously opened.
        /// </summary>
        /// <param name="iodevice">The file number to close</param>
        public static void CLOSE(int iodevice) {

            IOFile file = IOFile.Get(iodevice);
            if (file == null) {
                throw new JComRuntimeException(JComRuntimeErrors.FILE_NOT_OPEN);
            }
            file.Close(false);
        }

        /// <summary>
        /// Close all opened files.
        /// </summary>
        public static void CLOSE() {
            IOFile.CloseAll();
        }

        /// <summary>
        /// Writes the data to an unformatted random access file.
        /// </summary>
        /// <param name="iodevice">The file number to write to</param>
        /// <param name="recordNumber">The record number to write to</param>
        /// <param name="args">Write arguments</param>
        public static void WRITE(int iodevice, int recordNumber, params object[] args) {

            IOFile file = IOFile.Get(iodevice);
            if (file == null) {
                throw new JComRuntimeException(JComRuntimeErrors.FILE_NOT_OPEN);
            }
            if (recordNumber > 0) {
                if (file.RecordLength == 0) {
                    throw new JComRuntimeException(JComRuntimeErrors.FILE_NOT_OPEN_FOR_RANDOM_ACCESS);
                }
                file.RecordIndex = recordNumber;
            }
            foreach (object arg in args) {
                if (arg is double doubleValue) {
                    file.WriteDouble(doubleValue);
                    continue;
                }
                if (arg is double[] doubleArray) {
                    foreach (double doubleElement in doubleArray) {
                        file.WriteDouble(doubleElement);
                    }
                    continue;
                }
                if (arg is float floatValue) {
                    file.WriteFloat(floatValue);
                    continue;
                }
                if (arg is float[] floatArray) {
                    foreach (float floatElement in floatArray) {
                        file.WriteFloat(floatElement);
                    }
                    continue;
                }
                if (arg is string stringValue) {
                    file.WriteString(stringValue);
                    continue;
                }
                if (arg is string[] stringArray) {
                    foreach (string floatElement in stringArray) {
                        file.WriteString(floatElement);
                    }
                    continue;
                }
                if (arg is FixedString fixedStringValue) {
                    file.WriteString(fixedStringValue.ToString());
                    continue;
                }
                if (arg is FixedString[] fixedStringArray) {
                    foreach (FixedString fixedStringElement in fixedStringArray) {
                        file.WriteString(fixedStringElement.ToString());
                    }
                    continue;
                }
                if (arg is int intValue) {
                    file.WriteInteger(intValue);
                    continue;
                }
                if (arg is int[] intArray) {
                    foreach (int intElement in intArray) {
                        file.WriteInteger(intElement);
                    }
                    continue;
                }
                if (arg is bool boolValue) {
                    file.WriteBoolean(boolValue);
                    continue;
                }
                if (arg is bool[] boolArray) {
                    foreach (bool boolElement in boolArray) {
                        file.WriteBoolean(boolElement);
                    }
                    continue;
                }
            }
            file.Flush();
        }

        /// <summary>
        /// Return whether we've reached the end of the specified file
        /// </summary>
        /// <param name="iodevice">The file number to test for EOF</param>
        /// <returns>True if we've reached EOF, false otherwise</returns>
        public static bool EOF(int iodevice) {

            IOFile file = IOFile.Get(iodevice);
            if (file == null) {
                throw new JComRuntimeException(JComRuntimeErrors.FILE_NOT_OPEN);
            }
            return file.IsEndOfFile;
        }

        /// <summary>
        /// Delete the specified file from the file system
        /// </summary>
        /// <param name="filename">Name of file to delete</param>
        public static void DELETE(string filename) {
            File.Delete(filename);
        }
    }
}