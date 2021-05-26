// JCom Compiler Toolkit
// Debug Trace Helper
//
// Authors:
//  Steven Palmer
//
// Copyright (C) 2021 Steven Palmer
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
using System.Diagnostics;
using System.Reflection;

namespace CCompiler {

    /// <summary>
    /// For Mono/.NET Framework on macOS, we need to override the
    /// behaviour of Debug.Assert in debug mode so it actually
    /// does something useful.
    /// </summary>
    public class CCompilerTraceListener : TraceListener {

        /// <summary>
        /// Write a message to the console without a newline
        /// </summary>
        /// <param name="message">Message to write</param>
        public override void Write(string message) {
            Console.Write(message);
        }

        /// <summary>
        /// Write a message to the console with a newline
        /// </summary>
        /// <param name="message">Message to write</param>
        public override void WriteLine(string message) {
            Console.WriteLine(message);
        }

        /// <summary>
        /// Write a fail message to the console.
        /// </summary>
        /// <param name="message">Message to write</param>
        public override void Fail(string message) {
            Fail(message, string.Empty);
        }

        /// <summary>
        /// Write a fail message to the console
        /// </summary>
        /// <param name="message1">Message to write</param>
        /// <param name="message2">Message to write</param>
        public override void Fail(string message1, string message2) {
            if (null == message2)
                message2 = string.Empty;

            Console.WriteLine("{0}: {1}", message1, message2);
            Console.WriteLine("Stack Trace:");

            StackTrace trace = new(true);
            foreach (StackFrame frame in trace.GetFrames()) {
                MethodBase frameClass = frame.GetMethod();
                Console.WriteLine("  {2}.{3} {0}:{1}",
                                   frame.GetFileName(),
                                   frame.GetFileLineNumber(),
                                   frameClass.DeclaringType,
                                   frameClass.Name);
            }

        #if DEBUG
            Environment.Exit(1);
        #endif
        }
    }
}
