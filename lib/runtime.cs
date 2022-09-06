// JCom Runtime Library
// System functions
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
using System.IO;

namespace JComLib {

    /// <summary>
    /// System functions.
    /// </summary>
    public static class Runtime {

        private static bool _escValue = false;
        private static bool _escHandlerInstalled = false;
        private static bool _escFlag = false;

        /// <summary>
        /// Return the value of the ESC key and also resets the ESC
        /// state back to false afterward.
        /// </summary>
        public static bool ESC {
            get {
                bool escValue = _escValue;
                _escValue = false;
                return escValue;
            }
            set => _escValue = value;
        }

        /// <summary>
        /// Set the behaviour of the ESC key. If flag is true then we trap
        /// the ESC key and set the value of the ESC variable. If flag is
        /// false, we allow the system to handle the ESC key which results in
        /// the current process termination.
        ///
        /// Note: On most systems, ESC is Ctrl+C.
        /// 
        /// </summary>
        /// <param name="flag"></param>
        public static void SETESCAPE(bool flag) {
            if (flag) {
                if (!_escHandlerInstalled) {
                    Console.CancelKeyPress += new ConsoleCancelEventHandler(EscHandler);
                    _escHandlerInstalled = true;
                }
            }
            _escFlag = flag;
        }

        /// <summary>
        /// Return the last key pressed.
        /// </summary>
        public static string KEY {
            get {
                if (Console.KeyAvailable) {
                    ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                    return keyInfo.KeyChar.ToString();
                }
                return string.Empty;
            }
        }

        /// <summary>
        /// PAUSE keyword
        /// Write the specified string to the console then wait for a key to be pressed
        /// before returning.
        /// </summary>
        /// <param name="str">The string to output</param>
        public static void PAUSE(string str) {
            if (string.IsNullOrEmpty(str)) {
                Console.WriteLine("PAUSE");
            } else {
                Console.WriteLine("PAUSE {0}", str);
            }
            Console.ReadKey(true);
        }

        /// <summary>
        /// CLS keyword
        /// Clear the console.
        /// </summary>
        public static void CLS() {
            Console.Clear();
        }

        /// <summary>
        /// STOP keyword.
        /// Output the specified string to the console and then halts execution.
        /// </summary>
        /// <param name="str">The string to output</param>
        /// <param name="lineNumber">Line number of the STOP statement</param>
        public static void STOP(string str, int lineNumber) {
            string stopMessage;
            if (string.IsNullOrEmpty(str)) {
                stopMessage = "STOP" + (lineNumber > 0 ? " AT LINE " + lineNumber : string.Empty);
            } else {
                stopMessage = $"STOP {str}";
            }
            throw new JComRuntimeException(stopMessage) {
                Type = JComRuntimeExceptionType.STOP,
                LineNumber = lineNumber
            };
        }

        /// <summary>
        /// Halts execution and optionally displays a message.
        /// </summary>
        /// <param name="str">The string to output</param>
        /// <param name="lineNumber">Line number of the END statement</param>
        public static void END(string str, int lineNumber) {
            throw new JComRuntimeException(str) {
                Type = JComRuntimeExceptionType.END,
                LineNumber = lineNumber
            };
        }

        /// <summary>
        /// Set the cursor position. Does nothing if the values are out of range.
        /// </summary>
        /// <param name="row">1-based row</param>
        /// <param name="column">1-based column</param>
        public static void CURSOR(int row, int column) {
            if (row > 0 && row <= Console.WindowHeight && column > 0 && column <= Console.WindowWidth) {
                Console.SetCursorPosition(column - 1, row - 1);
            }
        }

        /// <summary>
        /// Get the current column position of the cursor, with 1 being the left-most
        /// column.
        /// </summary>
        public static int CURCOL => Console.CursorLeft + 1;

        /// <summary>
        /// Get the current row position of the cursor, with 1 being the top row.
        /// </summary>
        public static int CURROW => Console.CursorTop + 1;

        /// <summary>
        /// Set the output colour of subsequent text. Colour index specifies either
        /// the foreground or background colour.
        /// </summary>
        /// <param name="colorIndex">The index of the colour to be output</param>
        public static void COLOUR(int colorIndex) {

            if (colorIndex >= 128) {
                ConsoleColor bgColour = ConsoleColor.Black;
                switch (colorIndex) {
                    case 128: bgColour = ConsoleColor.Black; break;
                    case 129: bgColour = ConsoleColor.Red; break;
                    case 130: bgColour = ConsoleColor.Green; break;
                    case 131: bgColour = ConsoleColor.Yellow; break;
                    case 132: bgColour = ConsoleColor.Blue; break;
                    case 133: bgColour = ConsoleColor.Magenta; break;
                    case 134: bgColour = ConsoleColor.Cyan; break;
                    case 135: bgColour = ConsoleColor.White; break;
                }
                Console.BackgroundColor = bgColour;
            }
            if (colorIndex < 128) {
                ConsoleColor fgColour = ConsoleColor.White;
                switch (colorIndex) {
                    case 0: fgColour = ConsoleColor.Black; break;
                    case 1: fgColour = ConsoleColor.Red; break;
                    case 2: fgColour = ConsoleColor.Green; break;
                    case 3: fgColour = ConsoleColor.Yellow; break;
                    case 4: fgColour = ConsoleColor.Blue; break;
                    case 5: fgColour = ConsoleColor.Magenta; break;
                    case 6: fgColour = ConsoleColor.Cyan; break;
                    case 7: fgColour = ConsoleColor.White; break;
                }
                Console.ForegroundColor = fgColour;
            }
        }

        /// <summary>
        /// Display a directory catalog to the console. If a wildcard pattern string
        /// is specified, that is used to filter the output.
        /// </summary>
        /// <param name="wildcard">Wildcard pattern to match</param>
        public static void CATALOG(string wildcard) {

            string[] allfiles = Directory.GetFiles(".", wildcard, SearchOption.TopDirectoryOnly);
            Array.Sort(allfiles);
            foreach (string file in allfiles) {
                FileInfo info = new(file);
                long size = info.Length;
                string sizeString;
                if (size < 1024) {
                    sizeString = size + "B";
                } else if (size < 1024 * 1024) {
                    sizeString = (size / 1024) + "K";
                } else {
                    sizeString = (size / (1024 * 1024)) + "M";
                }
                Console.WriteLine($"{info.Name,-20} {sizeString,5}");
            }
        }

        // ESC key handler. The handler is invoked when the system ESC key is pressed and
        // we have enabled ESC handling via SETESCAPE(). If _escFlag is true then we swallow
        // the escape and set the ESC property to true. If _escFlag is false, we allow the
        // system to process the escape.
        private static void EscHandler(object sender, ConsoleCancelEventArgs args) {
            args.Cancel = _escFlag;
            _escValue = true;
        }
    }
}