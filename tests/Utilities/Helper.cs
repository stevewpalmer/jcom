// JComal
// Helper functions for unit tests
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
// under the License

using System;
using System.Numerics;
using CCompiler;
using NUnit.Framework;

namespace Utilities {

    public class Helper {

        // Compile the given code, run the specified function and verify
        // that the integer result matches
        public static void HelperRunInteger(ICompiler comp, string entryPointName, int expectedValue) {
            ExecutionResult execResult = comp.Execute(entryPointName);
            Assert.IsTrue(execResult.Success, $"Execution Status : {execResult.Success}");
            Assert.IsTrue(expectedValue == (int)execResult.Result, $"Saw {execResult.Result} but expected {expectedValue}");
        }

        // Compile the given code, run the specified function and verify
        // that the string result matches
        public static void HelperRunString(ICompiler comp, string entryPointName, string expectedValue) {
            ExecutionResult execResult = comp.Execute(entryPointName);
            Assert.IsTrue(execResult.Success, $"Execution Status : {execResult.Success}");
            Assert.IsTrue(expectedValue == execResult.Result.ToString(), $"Saw {execResult.Result} but expected {expectedValue}");
        }

        // Compile the given code, run the specified function and verify
        // that the float result matches
        public static void HelperRunFloat(ICompiler comp, string entryPointName, float expectedValue) {
            ExecutionResult execResult = comp.Execute(entryPointName);
            Assert.IsTrue(execResult.Success, $"Execution Status : {execResult.Success}");
            Assert.IsTrue(FloatCompare(expectedValue, (float)execResult.Result), $"Saw {execResult.Result} but expected {expectedValue}");
        }

        // Compile the given code, run the specified function and verify
        // that the double precision result matches
        public static void HelperRunDouble(ICompiler comp, string entryPointName, double expectedValue) {
            ExecutionResult execResult = comp.Execute(entryPointName);
            Assert.IsTrue(execResult.Success, $"Execution Status : {execResult.Success}");
            Assert.IsTrue(DoubleCompare(expectedValue, (double)execResult.Result), $"Saw {execResult.Result} but expected {expectedValue}");
        }

        // Compile the given code, run the specified function and verify
        // that the double precision result matches
        public static void HelperRunComplex(ICompiler comp, string entryPointName, Complex expectedValue) {
            ExecutionResult execResult = comp.Execute(entryPointName);
            Assert.IsTrue(execResult.Success, $"Execution Status : {execResult.Success}");
            Assert.IsTrue(ComplexCompare(expectedValue, (Complex)execResult.Result), $"Saw {execResult.Result} but expected {expectedValue}");
        }

        /// <summary>
        /// Compare two double values for equality by verifying whether the absolute difference
        /// is within double Epsilon.
        /// </summary>
        /// <param name="d1">First value</param>
        /// <param name="d2">Second value</param>
        public static bool DoubleCompare(double d1, double d2) {
            return Math.Abs(d1 - d2) < 0.00001;
        }

        /// <summary>
        /// Compare two Complex values for equality by verifying whether the absolute difference
        /// of the real and imaginary parts is within double Epsilon.
        /// </summary>
        /// <param name="d1">First value</param>
        /// <param name="d2">Second value</param>
        public static bool ComplexCompare(Complex d1, Complex d2) {
            return DoubleCompare(d1.Real, d2.Real) && DoubleCompare(d1.Imaginary, d2.Imaginary);
        }

        /// <summary>
        /// Compare two float values for equality by verifying whether the absolute difference
        /// is within float Epsilon.
        /// </summary>
        /// <param name="r1">First value</param>
        /// <param name="r2">Second value</param>
        public static bool FloatCompare(float r1, float r2) {
            return Math.Abs(r1 - r2) < 0.00001;
        }
    }
}