// JCom Compiler Toolkit
// Class extensions
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

namespace CCompiler {

    /// <summary>
    /// A set of class extensions.
    /// </summary>
    public static class MyExtensions {

        /// <summary>
        /// Capitalises the string by making the first letter uppercase. The remaining
        /// characters in the string are preserved.
        /// </summary>
        /// <param name="str">Input string</param>
        /// <returns>The string with the initial letter uppercased.</returns>
        public static string CapitaliseString(this string str) {
            if (str == null) {
                throw new ArgumentNullException(nameof(str));
            }
            if (str.Length > 0) {
                char initialChar = char.ToUpper(str[0]);
                return string.Concat(initialChar.ToString(), str.Substring(1));
            }
            return str;
        }
    }   
}