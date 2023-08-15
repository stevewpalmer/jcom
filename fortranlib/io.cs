// FORTRAN Runtime Library
// I/O library functions
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
using JComLib;

namespace JFortranLib {

    /// <summary>
    /// Processor dependent error codes returned in IOSTAT
    /// </summary>
    public static class IOError {

        /// <summary>
        /// An attempt was made to open an existing file but the file
        /// did not exist.
        /// </summary>
        public const int FileNotFound = 1;

        /// <summary>
        /// An attempt was made to create a new file but a file with
        /// the specified name already existed.
        /// </summary>
        public const int FileAlreadyExists = 2;

        /// <summary>
        /// If the file being opened is a scratch file, a filename must not
        /// be specified in the Open statement.
        /// </summary>
        public const int FilenameSpecified = 3;

        /// <summary>
        /// An invalid value was specified for the STATUS parameter.
        /// </summary>
        public const int IllegalStatus = 5;

        /// <summary>
        /// The specified file could not be opened.
        /// </summary>
        public const int CannotOpen = 6;

        /// <summary>
        /// An error occurred while attempting to write an end of file
        /// record.
        /// </summary>
        public const int EndfileError = 8;

        /// <summary>
        /// An error occurred while attempting to backspace to the start
        /// of the previous record.
        /// </summary>
        public const int BackspaceError = 9;

        /// <summary>
        /// An invalid value was specified for the ACCESS parameter.
        /// </summary>
        public const int IllegalAccess = 10;

        /// <summary>
        /// An invalid value was specified for the FORM parameter.
        /// </summary>
        public const int IllegalForm = 11;

        /// <summary>
        /// An invalid value was specified for the BLANK parameter.
        /// </summary>
        public const int IllegalBlank = 12;

        /// <summary>
        /// An error occurred while writing to the file.
        /// </summary>
        public const int WriteError = 13;

        /// <summary>
        /// An error occurred while reading from the file.
        /// </summary>
        public const int ReadError = 14;
    }

    /// <summary>
    /// Fortran I/O functions.
    /// </summary>
    public static class IO {

        /// <summary>
        /// WRITE keyword
        /// Writes the format string to the device.
        /// </summary>
        /// <param name="writeManager">A WriteManager instance to use</param>
        /// <param name="iostat">A reference variable that will be set to the I/O status</param>
        /// <returns>A zero value if the operation succeeds, or -1 if the operation fails</returns>
        public static int WRITE(WriteManager writeManager, ref int iostat) {
            if (writeManager == null) {
                throw new ArgumentNullException(nameof(writeManager));
            }
            iostat = 0;
            return writeManager.WriteEmpty();
        }

        /// <summary>
        /// WRITE keyword
        /// Writes an array of integer value to the device.
        /// </summary>
        /// <param name="writeManager">A WriteManager instance to use</param>
        /// <param name="iostat">A reference variable that will be set to the I/O status</param>
        /// <param name="intArray">An array of integers to write</param>
        /// <returns>A zero value if the operation succeeds, or -1 if the operation fails</returns>
        public static int WRITE(WriteManager writeManager, ref int iostat, int [] intArray) {
            if (writeManager == null) {
                throw new ArgumentNullException(nameof(writeManager));
            }
            int totalCharsWritten = 0;
            foreach (int intVar in intArray) {
                int charsWritten = writeManager.WriteInteger(intVar);
                if (charsWritten == -1) {
                    iostat = IOError.WriteError;
                    return -1;
                }
                totalCharsWritten += charsWritten;
            }
            return totalCharsWritten;
        }

        /// <summary>
        /// WRITE keyword
        /// Writes an integer value to the device.
        /// </summary>
        /// <param name="writeManager">A WriteManager instance to use</param>
        /// <param name="iostat">A reference variable that will be set to the I/O status</param>
        /// <param name="intVar">An integer value to write</param>
        /// <returns>A zero value if the operation succeeds, or -1 if the operation fails</returns>
        public static int WRITE(WriteManager writeManager, ref int iostat, int intVar) {
            if (writeManager == null) {
                throw new ArgumentNullException(nameof(writeManager));
            }
            int charsWritten = writeManager.WriteInteger(intVar);
            if (charsWritten == -1) {
                iostat = IOError.WriteError;
            }
            return charsWritten;
        }

        /// <summary>
        /// WRITE keyword
        /// Writes an array of boolean values to the device.
        /// </summary>
        /// <param name="writeManager">A WriteManager instance to use</param>
        /// <param name="iostat">A reference variable that will be set to the I/O status</param>
        /// <param name="boolArray">An array of booleans to write</param>
        /// <returns>A zero value if the operation succeeds, or -1 if the operation fails</returns>
        public static int WRITE(WriteManager writeManager, ref int iostat, bool [] boolArray) {
            if (writeManager == null) {
                throw new ArgumentNullException(nameof(writeManager));
            }
            int totalCharsWritten = 0;
            foreach (bool boolVar in boolArray) {
                int charsWritten = writeManager.WriteBoolean(boolVar);
                if (charsWritten == -1) {
                    iostat = IOError.WriteError;
                    return -1;
                }
                totalCharsWritten += charsWritten;
            }
            return totalCharsWritten;
        }

        /// <summary>
        /// WRITE keyword
        /// Writes a boolean value to the device.
        /// </summary>
        /// <param name="writeManager">A WriteManager instance to use</param>
        /// <param name="iostat">A reference variable that will be set to the I/O status</param>
        /// <param name="boolVar">A boolean value to write</param>
        /// <returns>A zero value if the operation succeeds, or -1 if the operation fails</returns>
        public static int WRITE(WriteManager writeManager, ref int iostat, bool boolVar) {
            if (writeManager == null) {
                throw new ArgumentNullException(nameof(writeManager));
            }
            int charsWritten = writeManager.WriteBoolean(boolVar);
            if (charsWritten == -1) {
                iostat = IOError.WriteError;
            }
            return charsWritten;
        }

        /// <summary>
        /// WRITE keyword
        /// Writes an array of float value to the device.
        /// </summary>
        /// <param name="writeManager">A WriteManager instance to use</param>
        /// <param name="iostat">A reference variable that will be set to the I/O status</param>
        /// <param name="floatArray">An array of float values</param>
        /// <returns>A zero value if the operation succeeds, or -1 if the operation fails</returns>
        public static int WRITE(WriteManager writeManager, ref int iostat, float [] floatArray) {
            if (writeManager == null) {
                throw new ArgumentNullException(nameof(writeManager));
            }
            int totalCharsWritten = 0;
            foreach (float floatVar in floatArray) {
                int charsWritten = writeManager.WriteFloat(floatVar);
                if (charsWritten == -1) {
                    iostat = IOError.WriteError;
                    return -1;
                }
                totalCharsWritten += charsWritten;
            }
            return totalCharsWritten;
        }

        /// <summary>
        /// WRITE keyword
        /// Writes a float value to the device.
        /// </summary>
        /// <param name="writeManager">A WriteManager instance to use</param>
        /// <param name="iostat">A reference variable that will be set to the I/O status</param>
        /// <param name="floatVar">A float value to write</param>
        /// <returns>A zero value if the operation succeeds, or -1 if the operation fails</returns>
        public static int WRITE(WriteManager writeManager, ref int iostat, float floatVar) {
            if (writeManager == null) {
                throw new ArgumentNullException(nameof(writeManager));
            }
            int charsWritten = writeManager.WriteFloat(floatVar);
            if (charsWritten == -1) {
                iostat = IOError.WriteError;
            }
            return charsWritten;
        }

        /// <summary>
        /// WRITE keyword
        /// Writes an array of double value to the device.
        /// </summary>
        /// <param name="writeManager">A WriteManager instance to use</param>
        /// <param name="iostat">A reference variable that will be set to the I/O status</param>
        /// <param name="doubleArray">An array of double values</param>
        /// <returns>A zero value if the operation succeeds, or -1 if the operation fails</returns>
        public static int WRITE(WriteManager writeManager, ref int iostat, double [] doubleArray) {
            if (writeManager == null) {
                throw new ArgumentNullException(nameof(writeManager));
            }
            int totalCharsWritten = 0;
            foreach (double doubleVar in doubleArray) {
                int charsWritten = writeManager.WriteDouble(doubleVar);
                if (charsWritten == -1) {
                    iostat = IOError.WriteError;
                    return -1;
                }
                totalCharsWritten += charsWritten;
            }
            return totalCharsWritten;
        }

        /// <summary>
        /// WRITE keyword
        /// Writes a double value to the device.
        /// </summary>
        /// <param name="writeManager">A WriteManager instance to use</param>
        /// <param name="iostat">A reference variable that will be set to the I/O status</param>
        /// <param name="doubleVar">A double value to write</param>
        /// <returns>A zero value if the operation succeeds, or -1 if the operation fails</returns>
        public static int WRITE(WriteManager writeManager, ref int iostat, double doubleVar) {
            if (writeManager == null) {
                throw new ArgumentNullException(nameof(writeManager));
            }
            int charsWritten = writeManager.WriteDouble(doubleVar);
            if (charsWritten == -1) {
                iostat = IOError.WriteError;
            }
            return charsWritten;
        }

        /// <summary>
        /// WRITE keyword
        /// Writes an array of complex values to the device.
        /// </summary>
        /// <param name="writeManager">A WriteManager instance to use</param>
        /// <param name="iostat">A reference variable that will be set to the I/O status</param>
        /// <param name="complexArray">An array of complex value to write</param>
        /// <returns>A zero value if the operation succeeds, or -1 if the operation fails</returns>
        public static int WRITE(WriteManager writeManager, ref int iostat, Complex[] complexArray) {
            if (writeManager == null) {
                throw new ArgumentNullException(nameof(writeManager));
            }
            int totalCharsWritten = 0;

            foreach (Complex complexVar in complexArray) {
                int charsWritten = writeManager.WriteComplex(complexVar);
                if (charsWritten == -1) {
                    iostat = IOError.WriteError;
                    return -1;
                }
                totalCharsWritten += charsWritten;
            }
            return totalCharsWritten;
        }

        /// <summary>
        /// WRITE keyword
        /// Writes a complex value to the device.
        /// </summary>
        /// <param name="writeManager">A WriteManager instance to use</param>
        /// <param name="iostat">A reference variable that will be set to the I/O status</param>
        /// <param name="complexVar">A complex value to write</param>
        /// <returns>A zero value if the operation succeeds, or -1 if the operation fails</returns>
        public static int WRITE(WriteManager writeManager, ref int iostat, Complex complexVar) {
            if (writeManager == null) {
                throw new ArgumentNullException(nameof(writeManager));
            }
            int charsWritten = writeManager.WriteComplex(complexVar);
            if (charsWritten == -1) {
                iostat = IOError.WriteError;
                return -1;
            }
            return charsWritten;
        }

        /// <summary>
        /// WRITE keyword
        /// Writes an array of string values to the device.
        /// </summary>
        /// <param name="writeManager">A WriteManager instance to use</param>
        /// <param name="iostat">A reference variable that will be set to the I/O status</param>
        /// <param name="strArray">An array of string values to write</param>
        /// <returns>A zero value if the operation succeeds, or -1 if the operation fails</returns>
        public static int WRITE(WriteManager writeManager, ref int iostat, string [] strArray) {
            if (writeManager == null) {
                throw new ArgumentNullException(nameof(writeManager));
            }
            int totalCharsWritten = 0;
            foreach (string strVar in strArray) {
                int charsWritten = writeManager.WriteString(strVar);
                if (charsWritten == -1) {
                    iostat = IOError.WriteError;
                    return -1;
                }
                totalCharsWritten += charsWritten;
            }
            return totalCharsWritten;
        }

        /// <summary>
        /// WRITE keyword
        /// Writes a string value to the device.
        /// </summary>
        /// <param name="writeManager">A WriteManager instance to use</param>
        /// <param name="iostat">A reference variable that will be set to the I/O status</param>
        /// <param name="strVar">A string value to write</param>
        /// <returns>A zero value if the operation succeeds, or -1 if the operation fails</returns>
        public static int WRITE(WriteManager writeManager, ref int iostat, string strVar) {
            if (writeManager == null) {
                throw new ArgumentNullException(nameof(writeManager));
            }
            int charsWritten = writeManager.WriteString(strVar);
            if (charsWritten == -1) {
                iostat = IOError.WriteError;
            }
            return charsWritten;
        }

        /// <summary>
        /// WRITE keyword
        /// Writes an array of fixed string values to the device.
        /// </summary>
        /// <param name="writeManager">A WriteManager instance to use</param>
        /// <param name="iostat">A reference variable that will be set to the I/O status</param>
        /// <param name="fixedstrArray">An array of fixed string values to write</param>
        /// <returns>A zero value if the operation succeeds, or -1 if the operation fails</returns>
        public static int WRITE(WriteManager writeManager, ref int iostat, FixedString [] fixedstrArray) {
            if (writeManager == null) {
                throw new ArgumentNullException(nameof(writeManager));
            }
            int totalCharsWritten = 0;
            foreach (FixedString fixedstrVar in fixedstrArray) {
                int charsWritten = writeManager.WriteString(fixedstrVar.ToString());
                if (charsWritten == -1) {
                    iostat = IOError.WriteError;
                    return -1;
                }
                totalCharsWritten += charsWritten;
            }
            return totalCharsWritten;
        }

        /// <summary>
        /// WRITE keyword
        /// Writes a fixed string value to the device.
        /// </summary>
        /// <param name="writeManager">A WriteManager instance to use</param>
        /// <param name="iostat">A reference variable that will be set to the I/O status</param>
        /// <param name="fixedstrVar">A fixed string value to write</param>
        /// <returns>A zero value if the operation succeeds, or -1 if the operation fails</returns>
        public static int WRITE(WriteManager writeManager, ref int iostat, FixedString fixedstrVar) {
            if (writeManager == null) {
                throw new ArgumentNullException(nameof(writeManager));
            }
            int charsWritten = writeManager.WriteString(fixedstrVar.ToString());
            if (charsWritten == -1) {
                iostat = IOError.WriteError;
            }
            return charsWritten;
        }

        /// <summary>
        /// READ keyword
        /// Do an empty read, skipping the current record and setting the END flag as
        /// appropriate.
        /// </summary>
        /// <param name="readManager">A ReadManager instance to use</param>
        /// <param name="iostat">A reference variable that will be set to the I/O status</param>
        /// <returns>A zero value if the operation succeeds, or -1 if the operation fails</returns>
        public static int READ(ReadManager readManager, ref int iostat) {
            if (readManager == null) {
                throw new ArgumentNullException(nameof(readManager));
            }
            int charsRead = readManager.SkipRecord();
            if (charsRead < 0) {
                iostat = IOError.ReadError;
            }
            return charsRead;
        }

        /// <summary>
        /// READ keyword
        /// Read a series of integer numbers from the device into the specified array.
        /// </summary>
        /// <param name="readManager">A ReadManager instance to use</param>
        /// <param name="iostat">A reference variable that will be set to the I/O status</param>
        /// <param name="arraySize">The size of the array</param>
        /// <param name="intArray">An integer array to read into</param>
        /// <returns>A zero value if the operation succeeds, or -1 if the operation fails</returns>
        public static int READ(ReadManager readManager, ref int iostat, int arraySize, int[] intArray) {
            if (readManager == null) {
                throw new ArgumentNullException(nameof(readManager));
            }
            
            int totalCharsRead = 0;
            for (int index = 0; index < arraySize; ++index) {
                int value = 0;
                
                int charsRead = READ(readManager, ref iostat, ref value);
                if (charsRead <= 0) {
                    return charsRead;
                }
                totalCharsRead += charsRead;
                intArray[index] = value;
            }
            return totalCharsRead;
        }

        /// <summary>
        /// READ keyword
        /// Read an integer from the device into the specified identifier.
        /// </summary>
        /// <param name="readManager">A ReadManager instance to use</param>
        /// <param name="iostat">A reference variable that will be set to the I/O status</param>
        /// <param name="intVar">The integer identifier to read into</param>
        /// <returns>A zero value if the operation succeeds, or -1 if the operation fails</returns>
        public static int READ(ReadManager readManager, ref int iostat, ref int intVar) {
            if (readManager == null) {
                throw new ArgumentNullException(nameof(readManager));
            }
            int charsRead = readManager.ReadInteger(ref intVar);
            if (charsRead == -1) {
                iostat = IOError.ReadError;
            }
            return charsRead;
        }

        /// <summary>
        /// READ keyword
        /// Read a series of boolean values from the device into the specified array.
        /// </summary>
        /// <param name="readManager">A ReadManager instance to use</param>
        /// <param name="iostat">A reference variable that will be set to the I/O status</param>
        /// <param name="arraySize">The size of the array</param>
        /// <param name="boolArray">An booleanarray to read into</param>
        /// <returns>A zero value if the operation succeeds, or -1 if the operation fails</returns>
        public static int READ(ReadManager readManager, ref int iostat, int arraySize, bool[] boolArray) {
            if (readManager == null) {
                throw new ArgumentNullException(nameof(readManager));
            }
            
            int totalCharsRead = 0;
            for (int index = 0; index < arraySize; ++index) {
                bool value = false;
                
                int charsRead = READ(readManager, ref iostat, ref value);
                if (charsRead <= 0) {
                    return charsRead;
                }
                boolArray[index] = value;
                totalCharsRead += charsRead;
            }
            return totalCharsRead;
        }
        
        /// <summary>
        /// READ keyword
        /// Read a boolean from the device into the specified identifier.
        /// </summary>
        /// <param name="readManager">A ReadManager instance to use</param>
        /// <param name="iostat">A reference variable that will be set to the I/O status</param>
        /// <param name="boolVar">The boolean identifier to read into</param>
        /// <returns>A zero value if the operation succeeds, or -1 if the operation fails</returns>
        public static int READ(ReadManager readManager, ref int iostat, ref bool boolVar) {
            if (readManager == null) {
                throw new ArgumentNullException(nameof(readManager));
            }
            int charsRead = readManager.ReadBoolean(ref boolVar);
            if (charsRead == -1) {
                iostat = IOError.ReadError;
            }
            return charsRead;
        }

        /// <summary>
        /// READ keyword
        /// Read a series of floating point numbers from the device into the specified array.
        /// </summary>
        /// <param name="readManager">A ReadManager instance to use</param>
        /// <param name="iostat">A reference variable that will be set to the I/O status</param>
        /// <param name="arraySize">The size of the array</param>
        /// <param name="floatArray">An float array to read into</param>
        /// <returns>A zero value if the operation succeeds, or -1 if the operation fails</returns>
        public static int READ(ReadManager readManager, ref int iostat, int arraySize, float[] floatArray) {
            if (readManager == null) {
                throw new ArgumentNullException(nameof(readManager));
            }
            
            int totalCharsRead = 0;
            for (int index = 0; index < arraySize; ++index) {
                float value = 0;
                
                int charsRead = READ(readManager, ref iostat, ref value);
                if (charsRead <= 0) {
                    return charsRead;
                }
                floatArray[index] = value;
                totalCharsRead += charsRead;
            }
            return totalCharsRead;
        }

        /// <summary>
        /// READ keyword
        /// Read a float from the device into the specified identifier.
        /// </summary>
        /// <param name="readManager">A ReadManager instance to use</param>
        /// <param name="iostat">A reference variable that will be set to the I/O status</param>
        /// <param name="floatVar">The float identifier to read into</param>
        /// <returns>A zero value if the operation succeeds, or -1 if the operation fails</returns>
        public static int READ(ReadManager readManager, ref int iostat, ref float floatVar) {
            if (readManager == null) {
                throw new ArgumentNullException(nameof(readManager));
            }
            int charsRead = readManager.ReadFloat(ref floatVar);
            if (charsRead == -1) {
                iostat = IOError.ReadError;
            }
            return charsRead;
        }

        /// <summary>
        /// READ keyword
        /// Read a series of complex numbers from the device into the specified array. This is really
        /// reading two individual float parts and creating a single complex number from each.
        /// </summary>
        /// <param name="readManager">A ReadManager instance to use</param>
        /// <param name="iostat">A reference variable that will be set to the I/O status</param>
        /// <param name="arraySize">The size of the array</param>
        /// <param name="complexArray">A Complex array to read into</param>
        /// <returns>A zero value if the operation succeeds, or -1 if the operation fails</returns>
        public static int READ(ReadManager readManager, ref int iostat, int arraySize, Complex[] complexArray) {
            if (readManager == null) {
                throw new ArgumentNullException(nameof(readManager));
            }
            if (complexArray == null) {
                throw new ArgumentNullException(nameof(complexArray));
            }

            int totalCharsRead = 0;
            for (int index = 0; index < arraySize; ++index) {
                double realPart = 0;
                double imaginaryPart = 0;

                int charsRead = READ(readManager, ref iostat, ref realPart);
                if (charsRead <= 0) {
                    return charsRead;
                }
                totalCharsRead += charsRead;
                charsRead = READ(readManager, ref iostat, ref imaginaryPart);
                if (charsRead <= 0) {
                    return charsRead;
                }
                totalCharsRead += charsRead;
                complexArray[index] = new Complex(realPart, imaginaryPart);
            }
            return totalCharsRead;
        }

        /// <summary>
        /// READ keyword
        /// Read a complex number from the device into the specified identifier. This is really
        /// reading two individual float parts and creating a single complex number from each.
        /// </summary>
        /// <param name="readManager">A ReadManager instance to use</param>
        /// <param name="iostat">A reference variable that will be set to the I/O status</param>
        /// <param name="complexVar">The Complex identifier to read into</param>
        /// <returns>A zero value if the operation succeeds, or -1 if the operation fails</returns>
        public static int READ(ReadManager readManager, ref int iostat, ref Complex complexVar) {
            if (readManager == null) {
                throw new ArgumentNullException(nameof(readManager));
            }

            double realPart = 0;
            double imaginaryPart = 0;
            int charsRead = 0;

            charsRead += READ(readManager, ref iostat, ref realPart);
            charsRead += READ(readManager, ref iostat, ref imaginaryPart);
            complexVar = new Complex(realPart, imaginaryPart);
            return charsRead;
        }

        /// <summary>
        /// READ keyword
        /// Read a series of double precision numbers from the device into the specified array.
        /// </summary>
        /// <param name="readManager">A ReadManager instance to use</param>
        /// <param name="iostat">A reference variable that will be set to the I/O status</param>
        /// <param name="arraySize">The size of the array</param>
        /// <param name="doubleArray">A double array to read into</param>
        /// <returns>A zero value if the operation succeeds, or -1 if the operation fails</returns>
        public static int READ(ReadManager readManager, ref int iostat, int arraySize, double[] doubleArray) {
            if (readManager == null) {
                throw new ArgumentNullException(nameof(readManager));
            }
            
            int totalCharsRead = 0;
            for (int index = 0; index < arraySize; ++index) {
                double value = 0;

                int charsRead = READ(readManager, ref iostat, ref value);
                if (charsRead <= 0) {
                    return charsRead;
                }
                doubleArray[index] = value;
                totalCharsRead += charsRead;
            }
            return totalCharsRead;
        }

        /// <summary>
        /// READ keyword
        /// Read a double from the device into the specified identifier.
        /// </summary>
        /// <param name="readManager">A ReadManager instance to use</param>
        /// <param name="iostat">A reference variable that will be set to the I/O status</param>
        /// <param name="doubleVar">The double identifier to read into</param>
        /// <returns>A zero value if the operation succeeds, or -1 if the operation fails</returns>
        public static int READ(ReadManager readManager, ref int iostat, ref double doubleVar) {
            if (readManager == null) {
                throw new ArgumentNullException(nameof(readManager));
            }
            int charsRead = readManager.ReadDouble(ref doubleVar);
            if (charsRead == -1) {
                iostat = IOError.ReadError;
            }
            return charsRead;
        }

        /// <summary>
        /// READ keyword
        /// Read a series of fixed strings from the device into the specified array.
        /// </summary>
        /// <param name="readManager">A ReadManager instance to use</param>
        /// <param name="iostat">A reference variable that will be set to the I/O status</param>
        /// <param name="arraySize">The size of the array</param>
        /// <param name="strArray">A fixed string array to read into</param>
        /// <returns>A zero value if the operation succeeds, or -1 if the operation fails</returns>
        public static int READ(ReadManager readManager, ref int iostat, int arraySize, FixedString[] strArray) {
            if (readManager == null) {
                throw new ArgumentNullException(nameof(readManager));
            }
            if (strArray == null) {
                throw new ArgumentNullException(nameof(strArray));
            }

            int totalCharsRead = 0;
            for (int index = 0; index < arraySize; ++index) {
                int charsRead = READ(readManager, ref iostat, ref strArray[index]);
                if (charsRead <= 0) {
                    return charsRead;
                }
                totalCharsRead += charsRead;
            }
            return totalCharsRead;
        }

        /// <summary>
        /// READ keyword
        /// Read a string from the device into the specified identifier.
        /// </summary>
        /// <param name="readManager">A ReadManager instance to use</param>
        /// <param name="iostat">A reference variable that will be set to the I/O status</param>
        /// <param name="fixedstrVar">The fixed string identifier to read into</param>
        /// <returns>A zero value if the operation succeeds, or -1 if the operation fails</returns>
        public static int READ(ReadManager readManager, ref int iostat, ref FixedString fixedstrVar) {
            if (readManager == null) {
                throw new ArgumentNullException(nameof(readManager));
            }
            if (fixedstrVar == null) {
                throw new ArgumentNullException(nameof(fixedstrVar));
            }
            int charsRead = readManager.ReadString(ref fixedstrVar);
            if (charsRead == -1) {
                iostat = IOError.ReadError;
            }
            return charsRead;
        }

        /// <summary>
        /// READ keyword
        /// Read a string from the device into a substring of the specified fixed string.
        /// </summary>
        /// <param name="readManager">A ReadManager instance to use</param>
        /// <param name="iostat">A reference variable that will be set to the I/O status</param>
        /// <param name="start">The 1-based start index of the target string</param>
        /// <param name="end">The 1-based end index of the target string</param>
        /// <param name="fixedstrVar">The fixed string identifier to read into</param>
        /// <returns>A zero value if the operation succeeds, or -1 if the operation fails</returns>
        public static int READ(ReadManager readManager, ref int iostat, int start, int end, ref FixedString fixedstrVar) {
            if (readManager == null) {
                throw new ArgumentNullException(nameof(readManager));
            }
            if (fixedstrVar == null) {
                throw new ArgumentNullException(nameof(fixedstrVar));
            }
            if (end == -1) {
                end = fixedstrVar.Length;
            }
            FixedString fixedString = new(end - start + 1);
            int charsRead = readManager.ReadString(ref fixedString);
            if (charsRead == -1) {
                iostat = IOError.ReadError;
            } else {
                fixedstrVar.Set(fixedString, start, end);
            }
            return charsRead;
        }

        /// <summary>
        /// BACKSPACE keyword.
        /// Positions the file attached to the specified device to just before the preceding record.
        /// </summary>
        /// <param name="iodevice">The device number</param>
        /// <param name="iostat">A reference variable that will be set to the I/O status</param>
        /// <returns>A zero value if the operation succeeds, or -1 if the operation fails</returns>
        public static int BACKSPACE(int iodevice, ref int iostat) {
            iostat = 0;
            IOFile iofile = IOFile.Get(iodevice);
            if (iofile != null) {
                if (!iofile.IsSequential) {
                    iostat = IOError.BackspaceError;
                    return -1;
                }
                if (!iofile.Backspace()) {
                    iostat = IOError.EndfileError;
                    return -1;
                }
            }
            return 0;
        }

        /// <summary>
        /// ENDFILE keyword.
        /// For sequential files, writes an end-of-file record to the file and positions the file
        /// after this record (the terminal point). For direct access files, truncates the file
        /// after the current record.
        /// </summary>
        /// <param name="iodevice">The device number</param>
        /// <param name="iostat">A reference variable that will be set to the I/O status</param>
        /// <returns>A zero value if the operation succeeds, or -1 if the operation fails</returns>
        public static int ENDFILE(int iodevice, ref int iostat) {
            iostat = 0;
            IOFile iofile = IOFile.Get(iodevice);
            if (iofile == null) {
                iofile = new IOFile(iodevice);
                iofile.Open();
            }
            if (!iofile.EndFile()) {
                iostat = IOError.EndfileError;
                return -1;
            }
            return 0;
        }

        /// <summary>
        /// REWIND keyword.
        /// Rewind the file attached to the specified device back to the start position so that subsequent
        /// READ or WRITE statements commence from that position.
        /// </summary>
        /// <param name="iodevice">The device number</param>
        /// <param name="iostat">A reference variable that will be set to the I/O status</param>
        /// <returns>A zero value if the operation succeeds, or -1 if the operation fails</returns>
        public static int REWIND(int iodevice, ref int iostat) {
            iostat = 0;
            IOFile iofile = IOFile.Get(iodevice);
            if (iofile != null) {
                iofile.Rewind();
            }
            return 0;
        }

        /// <summary>
        /// INQUIRE keyword.
        /// </summary>
        /// <param name="iodevice">A device number</param>
        /// <param name="filename">A filename</param>
        /// <param name="iostat">A reference variable that will be set to the I/O status</param>
        /// <param name="exists">A reference variable that will be set to true if
        /// the file exists</param>
        /// <param name="opened">A reference variable that will be set to true if
        /// the file is opened</param>
        /// <param name="number">A reference variable that will be set to the device
        /// number of the file</param>
        /// <param name="named">A reference variable that will be set to true if
        /// the device is named</param>
        /// <param name="name">A reference variable that will be set to the name
        /// of the file</param>
        /// <param name="access">A reference variable that will be set to SEQUENTIAL if
        /// the file was opened for sequential access or DIRECT otherwise</param>
        /// <param name="sequential">A reference variable that will be set to YES</param>
        /// <param name="direct">A reference variable that will be set to YES</param>
        /// <param name="form">A reference variable that will be set to FORMATTED if
        /// the file was opened for formatted output or UNFORMATTED otherwise</param>
        /// <param name="formatted">A reference variable that will be set to YES</param>
        /// <param name="unformatted">A reference variable that will be set to YES</param>
        /// <param name="recl">A reference variable that will be set to the record length</param>
        /// <param name="nextrec">A reference variable that will be set to the next direct
        /// access record number</param>
        /// <param name="blank">A reference variable that will be set to NULL or ZERO</param>
        /// <returns>A zero value if the operation succeeds, or -1 if the operation fails</returns>
        public static int INQUIRE(int iodevice, string filename, ref int iostat, ref bool exists, ref bool opened, ref int number,
                                  ref bool named, ref FixedString name, ref FixedString access, ref FixedString sequential,
                                  ref FixedString direct, ref FixedString form, ref FixedString formatted, ref FixedString unformatted,
                                  ref int recl, ref int nextrec, ref FixedString blank) {
            bool isFileStatement = filename != null;
            IOFile ioobject;
            iostat = 0;

            if (isFileStatement) {
                string fixedFilename = filename.Trim();
                exists = File.Exists(fixedFilename);
                named = true;
                name = fixedFilename;
                ioobject = IOFile.Get(fixedFilename);
                opened = ioobject != null;
                if (opened) {
                    number = ioobject.Unit;
                }
            } else {
                exists = true;
                number = iodevice;
                ioobject = IOFile.Get(iodevice);
                opened = ioobject != null;
                if (opened) {
                    named = !string.IsNullOrEmpty(ioobject.Path);
                    name = named ? ioobject.Path : string.Empty;
                }
            }
            if (ioobject != null) {
                access = ioobject.IsSequential ? "SEQUENTIAL" : "DIRECT";
                form = ioobject.IsFormatted ? "FORMATTED" : "UNFORMATTED";
                blank = ioobject.Blank == '0' ? "ZERO" : "NULL";
                sequential = "YES";
                direct = "YES";
                formatted = "YES";
                unformatted = "YES";
                if (!ioobject.IsSequential) {
                    recl = ioobject.RecordLength;
                    nextrec = ioobject.RecordIndex;
                }
            }
            return 0;
        }

        /// <summary>
        /// FORTRAN file open.
        /// </summary>
        /// <param name="iodevice">The device number</param>
        /// <param name="iostat">A reference variable that will be set to the I/O status</param>
        /// <param name="filename">An optional file name</param>
        /// <param name="status">Specifies how the file is to be opened: OLD, NEW, UNKNOWN or SCRATCH</param>
        /// <param name="access">Specifies the access mode: DIRECT or SEQUENTIAL</param>
        /// <param name="form">Specifies the file formatting: FORMATTED or UNFORMATTED</param>
        /// <param name="recl">For formatted files, specifies the record size in units</param>
        /// <param name="blank">Specifies how blanks are treated in input: NULL or ZERO</param>
        /// <returns>A zero value if the operation succeeds, or -1 if the operation fails</returns>
        public static int OPEN(int iodevice, ref int iostat, string filename, string status, string access, string form, int recl, string blank) {
            char blankChar;

            // Default IOStat is no error
            iostat = 0;

            // Parse BLANK specifier.
            if (blank == null) {
                blankChar = ' ';
            } else {
                string fixedBlank = blank.ToUpper().Trim();
                if (fixedBlank == "NULL") {
                    blankChar = ' ';
                } else if (fixedBlank == "ZERO") {
                    blankChar = '0';
                } else {
                    iostat = IOError.IllegalBlank;
                    return -1;
                }
            }

            IOFile iofile = IOFile.Get(iodevice);
            if (iofile != null) {

                // If the FILE= specifier is not included in the OPEN statement, the file to be connected to the
                // unit is the same as the file to which the unit is connected.
                if (filename == null) {
                    return 0;
                }

                // If the file to be connected to the unit is the same as the file
                // to which the unit is connected, only the BLANK= specifier may
                // have a value different from the one currently in effect. Execution
                // of the OPEN statement causes the new value of the BLANK= specifier
                // to be in effect. The position of the file is unaffected.
                string fixedFilename = filename.Trim();
                if (string.Compare(fixedFilename, iofile.Path, StringComparison.CurrentCultureIgnoreCase) == 0) {
                    iofile.Blank = blankChar;
                    return 0;
                }

                // If the file to be connected to the unit is not the same as the file to which the unit is connected,
                // the effect is as if a CLOSE statement (12.10.2) without a STATUS= specifier had been executed for
                // the unit immediately prior to the execution of the OPEN statement.
                iofile.Handle.Close();
            } else {
                iofile = new IOFile(iodevice);
            }

            // Consult the status to determine how to open the file. A missing status is treated
            // as UNKNOWN.
            string fixedStatus = (status == null) ? "UNKNOWN" : status.ToUpper().Trim();

            if ((fixedStatus == "OLD" || fixedStatus == "NEW" || fixedStatus == "UNKNOWN") && filename != null) {
                bool fileExists = File.Exists(filename.Trim());
                if (fixedStatus == "UNKNOWN") {
                    fixedStatus = fileExists ? "OLD" : "NEW";
                }
                if (fixedStatus == "OLD") {
                    if (!fileExists) {
                        iostat = IOError.FileNotFound;
                        return -1;
                    }
                    iofile.IsNew = false;
                }
                if (fixedStatus == "NEW") {
                    if (fileExists) {
                        iostat = IOError.FileAlreadyExists;
                        return -1;
                    }
                    iofile.IsNew = true;
                }
                iofile.IsScratch = false;
            }

            // SCRATCH creates a temporary file. Filename cannot be specified here
            else if (fixedStatus == "SCRATCH") {
                if (filename != null) {
                    iostat = IOError.FilenameSpecified;
                    return -1;
                }
                filename = Path.GetTempFileName();
                iofile.IsScratch = true;
            }

            // STATUS must be UNKNOWN if we get here
            else if (fixedStatus != "UNKNOWN") {
                iostat = IOError.IllegalStatus;
                return -1;
            }

            // Check access. It must either be SEQUENTIAL or DIRECT. By
            // default we assume sequential access.
            if (access == null) {
                iofile.IsSequential = true;
            } else {
                string fixedAccess = access.ToUpper().Trim();
                if (fixedAccess == "SEQUENTIAL") {
                    iofile.IsSequential = true;
                } else if (fixedAccess == "DIRECT") {
                    iofile.IsSequential = false;
                } else {
                    iostat = IOError.IllegalAccess;
                    return -1;
                }
            }

            // Check whether the file is being written formatted or
            // unformatted. Access matters here too.
            if (form == null) {
                iofile.IsFormatted = iofile.IsSequential;
            } else {
                string fixedForm = form.ToUpper().Trim();
                if (fixedForm == "FORMATTED") {
                    iofile.IsFormatted = true;
                } else if (fixedForm == "UNFORMATTED") {
                    iofile.IsFormatted = false;
                } else {
                    iostat = IOError.IllegalForm;
                    return -1;
                }
            }

            // Save the record length
            iofile.RecordLength = recl;
            iofile.RecordIndex = 1;
            iofile.Blank = blankChar;
            iofile.Unit = iodevice;

            if (filename != null) {
                iofile.Path = filename.Trim();
            }

            if (!iofile.Open()) {
                iostat = IOError.CannotOpen;
                return -1;
            }
            return 0;
        }

        /// <summary>
        /// Close the specified I/O device and optionally delete it. Status can be one
        /// of the two behaviours:
        ///
        ///   KEEP - does nothing. The file continues to exist post-close.
        ///   DELETE - the file is deleted post-close.
        ///
        /// If the file was opened as a scratch file then DELETE is implied. Otherwise
        /// KEEP is implied.
        /// </summary>
        /// <param name="iodevice">The device number</param>
        /// <param name="iostat">A reference variable that will be set to the I/O status</param>
        /// <param name="status">A string that specifies the post-close behaviour</param>
        /// <returns>A zero value if the operation succeeds, or -1 if the operation fails</returns>
        public static int CLOSE(int iodevice, ref int iostat, string status) {
            iostat = 0;
            IOFile iofile = IOFile.Get(iodevice);
            if (iofile != null) {
                bool deleteFile = iofile.IsScratch;

                if (!string.IsNullOrWhiteSpace(status)) {
                    string fixedStatus = status.ToUpper().Trim();
                    if (fixedStatus == "DELETE") {
                        deleteFile = true;
                    } else if (fixedStatus == "KEEP") {
                        deleteFile = false;
                    } else {
                        iostat = IOError.IllegalStatus;
                        return -1;
                    }
                }
                iofile.Close(deleteFile);
                iofile.Dispose();
            }
            return 0;
        }
    }
}