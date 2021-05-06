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
        /// Get the next available free file number.
        /// </summary>
        public static int FREEFILE {
            get {
                return IOFile.FreeFileNumber();
            }
        }

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
        public static void OPEN(int iodevice, string filename, string mode) {

            IOFile file = IOFile.Get(iodevice);
            if (file != null) {
                throw new JComRuntimeException(JComRuntimeErrors.FILE_ALREADY_OPEN);
            }
            file = new IOFile(iodevice) {
                Path = filename
            };
            file.IsNew = mode == "w";
            file.IsFormatted = mode == "x";
            file.Open();
            if (file.Handle == null) {
                throw new JComRuntimeException(JComRuntimeErrors.CANNOT_OPEN_FILE);
            }
            if (mode == "w+") {
                file.SeekToEnd();
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
        /// Return whether we've reached the end of the specified file
        /// </summary>
        /// <param name="iodevice">The file number to test for EOF</param>
        /// <returns>True if we've reached EOF, false otherwise</returns>
        public bool EOF(int iodevice) {

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