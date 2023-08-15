// JCobol
// Main compiler class
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

using CCompiler;

namespace JCobol {

    /// <summary>
    /// Main Cobol compiler class.
    /// </summary>
    public partial class Compiler : ICompiler {

        private readonly CobolOptions _opts;

        /// <summary>
        /// Constructs a Fortran compiler object with the given options.
        /// </summary>
        /// <param name="opts">Compiler options</param>
        public Compiler(CobolOptions opts) {
            Messages = new MessageCollection(opts);
            _opts = opts;
        }

        /// <summary>
        /// Return or set the list of compiler messages.
        /// </summary>
        public MessageCollection Messages { get; set; }

        internal void Compile(string srcfile) {
            throw new NotImplementedException();
        }

        internal void Save() {
            throw new NotImplementedException();
        }

        internal void Execute() {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Convert the parse tree to executable code and then execute the
        /// resulting code. The return value from the specified entry point function
        /// is returned as an object.
        /// </summary>
        /// <param name="entryPointName">The name of the method to be called</param>
        /// <returns>An ExecutionResult object representing the result of the execution</returns>
        public ExecutionResult Execute(string entryPointName) {
            return null;
        }
    }
}