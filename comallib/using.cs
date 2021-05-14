// COMAL Runtime Library
// USING function
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

    public static partial class Intrinsics {

        /// <summary>
        /// Format a string using the specified template and arguments.
        /// </summary>
        /// <param name="template">Template</param>
        /// <param name="args">Arguments</param>
        /// <returns>Formatted string</returns>
        public static string USING(string template, params object[] args) {

            StringBuilder str = new();
            int length = template.Length;
            int index = 0;
            Variant value = null;
            int argIndex = 0;

            while (index < length) {

                if (value == null && argIndex < args.Length) {
                    value = new Variant(args[argIndex++]);
                }

                int startIndex = index;
                char ch = template[index++];
                if (ch == '-' || ch == '#' || ch == '.') {

                    int signPart = 0;
                    int integerPart = 0;
                    int fractionPart = 0;
                    int decimalPart = 0;

                    while (ch == '-') {
                        ++signPart;
                        if (index == length) {
                            break;
                        }
                        ch = template[index++];
                    }
                    while (ch == '#') {
                        ++integerPart;
                        if (index == length) {
                            break;
                        }
                        ch = template[index++];
                    }
                    while (ch == '.') {
                        ++decimalPart;
                        if (index == length) {
                            break;
                        }
                        ch = template[index++];
                    }
                    while (ch == '#') {
                        ++fractionPart;
                        if (index == length) {
                            break;
                        }
                        ch = template[index++];
                    }
                    bool valid = signPart <= 1 && decimalPart <= 1;
                    valid = valid && (integerPart > 0 || fractionPart > 0);
                    if (!valid) {
                        while (startIndex < index) {
                            str.Append(template[startIndex]);
                            startIndex++;
                        }
                        continue;
                    }
                    if (index < length) {
                        index--;
                    }

                    if (signPart > 0) {
                        ++integerPart;
                    }
                    int fieldWidth = integerPart + decimalPart + fractionPart;

                    if (value != null && value.IsNumber) {
                        string strNumber = value.RealValue.ToString();
                        string[] parts = strNumber.Split('.');

                        string integerString = string.Empty;
                        string fractionString = string.Empty;
                        string decimalString = string.Empty;

                        // Integer part?
                        if (integerPart > 0) {
                            integerString = parts[0].PadLeft(integerPart);
                        }

                        // Fraction separator?
                        if (decimalPart > 0) {
                            decimalString = ".";
                        }

                        // Fraction?
                        if (fractionPart > 0) {

                            if (parts.Length > 1) {
                                fractionString = parts[1].PadRight(fractionPart, '0');
                            } else {
                                fractionString = new string('0', fractionPart);
                            }
                            if (fractionString.Length > fractionPart) {
                                double roundedFraction = Math.Round(value.RealValue, fractionPart);
                                parts = roundedFraction.ToString().Split('.');
                                fractionString = parts[1];
                            }
                        }

                        if (integerString.Length + decimalString.Length + fractionString.Length > fieldWidth) {
                            str.Append(new string('*', fieldWidth));
                        } else {
                            str.Append(integerString);
                            str.Append(decimalString);
                            str.Append(fractionString);
                        }
                    }
                    value = null;
                    continue;
                }
                str.Append(ch);
            }
            return str.ToString();
        }
    }
}
