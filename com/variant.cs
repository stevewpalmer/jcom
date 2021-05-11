// JCom Compiler Toolkit
// Variant class
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
using System.Globalization;
using System.Numerics;

namespace CCompiler {
    
    /// <summary>
    /// Encapsulates a symbol value in any supported type and provides an
    /// interface for setting and retrieving the value.
    /// </summary>
    public sealed class Variant {
        private double _value;
        private Complex _complexValue;

        public Variant() {
            HasValue = false;
            Type = SymType.NONE;
        }

        /// <summary>
        /// Instantiates a boolean variant.
        /// </summary>
        /// <param name="value">Boolean value to set</param>
        public Variant(bool value) {
            Set(value);
        }

        /// <summary>
        /// Instantiates an integer variant.
        /// </summary>
        /// <param name="value">Integer value to set</param>
        public Variant(int value) {
            Set(value);
        }

        /// <summary>
        /// Instantiates a floating point variant.
        /// </summary>
        /// <param name="value">Float  value to set</param>
        public Variant(float value) {
            Set(value);
        }

        /// <summary>
        /// Instantiates a double variant.
        /// </summary>
        /// <param name="value">Double value to set</param>
        public Variant(double value) {
            Set(value);
        }

        /// <summary>
        /// Instantiates a string variant.
        /// </summary>
        /// <param name="value">String value to set</param>
        public Variant(string value) {
            Set(value);
        }

        /// <summary>
        /// Instantiates a complex variant.
        /// </summary>
        /// <param name="value">Complex value to set</param>
        public Variant(Complex value) {
            Set(value);
        }

        /// <summary>
        /// Conversion operator to create an integer variant with the
        /// specified value.
        /// </summary>
        /// <param name="i">An integer value</param>
        /// <returns>A new Variant representing the specified integer value</returns>
        public static explicit operator Variant(int i) {
            return new Variant(i);
        }

        /// <summary>
        /// Implements the addition operator on variants. The type of the result is
        /// the largest of the two specified types.
        /// </summary>
        /// <param name="v1">First variant</param>
        /// <param name="v2">Second variant</param>
        /// <returns>A Variant that contains the result of adding v to this variant</returns>
        public static Variant operator +(Variant v1, Variant v2) {
            if (v1 == null) {
                throw new ArgumentNullException(nameof(v1));
            }
            if (v2 == null) {
                throw new ArgumentNullException(nameof(v2));
            }
            return Symbol.LargestType(v1.Type, v2.Type) switch {
                SymType.DOUBLE =>   new Variant(v1.DoubleValue + v2.DoubleValue),
                SymType.INTEGER =>  new Variant(v1.IntValue + v2.IntValue),
                SymType.FLOAT =>    new Variant(v1.RealValue + v2.RealValue),
                SymType.COMPLEX =>  new Variant(v1.ComplexValue + v2.ComplexValue),
                SymType.CHAR =>     new Variant(v1.StringValue + v2.StringValue),
                _ => throw new InvalidOperationException("Addition not permitted on type"),
            };
        }

        /// <summary>
        /// Provides an alternative to operator+ for languages that do not support
        /// operator overloading.
        /// </summary>
        /// <param name="v">The variant to be added to this one</param>
        /// <returns>A Variant that contains the result of adding v to this variant</returns>
        public Variant Add(Variant v) {
            return this + v;
        }

        /// <summary>
        /// Returns the result when negation is applied to v1. The return type is
        /// the type of v1.
        /// </summary>
        /// <param name="v1">Left operand</param>
        /// <returns>A Variant that contains the result of negating this variant</returns>
        public static Variant operator -(Variant v1) {
            if (v1 == null) {
                throw new ArgumentNullException(nameof(v1));
            }
            return v1.Type switch {
                SymType.DOUBLE =>   new Variant(-v1.DoubleValue),
                SymType.INTEGER =>  new Variant(-v1.IntValue),
                SymType.FLOAT =>    new Variant(-v1.RealValue),
                SymType.COMPLEX =>  new Variant(-v1.ComplexValue),
                _ => throw new InvalidOperationException("Unary minus not permitted on type"),
            };
        }

        /// <summary>
        /// Provides an alternative to unary operator- for languages that do not support
        /// operator overloading.
        /// </summary>
        /// <returns>A Variant that contains the result of negating this variant</returns>
        public Variant Negate() {
            return -this;
        }

        /// <summary>
        /// Returns the result when v2 is subtracted from v1. The type of the result is
        /// the largest of the two specified types.
        /// </summary>
        /// <param name="v1">Left operand</param>
        /// <param name="v2">Right operand</param>
        /// <returns>A Variant that contains the result of subtracting v from this variant</returns>
        public static Variant operator -(Variant v1, Variant v2) {
            if (v1 == null) {
                throw new ArgumentNullException(nameof(v1));
            }
            if (v2 == null) {
                throw new ArgumentNullException(nameof(v2));
            }
            return Symbol.LargestType(v1.Type, v2.Type) switch {
                SymType.DOUBLE =>   new Variant(v1.DoubleValue - v2.DoubleValue),
                SymType.INTEGER =>  new Variant(v1.IntValue - v2.IntValue),
                SymType.FLOAT =>    new Variant(v1.RealValue - v2.RealValue),
                SymType.COMPLEX =>  new Variant(v1.ComplexValue - v2.ComplexValue),
                _ => throw new InvalidOperationException("Subtraction not permitted on type"),
            };
        }

        /// <summary>
        /// Provides an alternative to operator- for languages that do not support
        /// operator overloading.
        /// </summary>
        /// <param name="v">The variant to be subtracted from this one</param>
        /// <returns>A Variant that contains the result of subtracting v from this variant</returns>
        public Variant Subtract(Variant v) {
            return this - v;
        }

        /// <summary>
        /// Returns the result when v1 is multiplied by v2. The type of the result is
        /// the largest of the two specified types.
        /// </summary>
        /// <param name="v1">Left operand</param>
        /// <param name="v2">Right operand</param>
        /// <returns>A Variant that contains the result of multiplying v with this variant</returns>
        public static Variant operator *(Variant v1, Variant v2) {
            if (v1 == null) {
                throw new ArgumentNullException(nameof(v1));
            }
            if (v2 == null) {
                throw new ArgumentNullException(nameof(v2));
            }
            return Symbol.LargestType(v1.Type, v2.Type) switch {
                SymType.DOUBLE =>   new Variant(v1.DoubleValue * v2.DoubleValue),
                SymType.INTEGER =>  new Variant(v1.IntValue * v2.IntValue),
                SymType.FLOAT =>    new Variant(v1.RealValue * v2.RealValue),
                SymType.COMPLEX =>  new Variant(v1.ComplexValue * v2.ComplexValue),
                _ => throw new InvalidOperationException("Multiplication not permitted on type"),
            };
        }

        /// <summary>
        /// Provides an alternative to operator* for languages that do not support
        /// operator overloading.
        /// </summary>
        /// <param name="v">The variant to be multiplied by this one</param>
        /// <returns>A Variant that contains the result of multiplying v with this variant</returns>
        public Variant Multiply(Variant v) {
            return this * v;
        }

        /// <summary>
        /// Returns the result when v1 is divided by v2. If v2 is zero
        /// then an exception is thrown. The type of the result is
        /// the largest of the two specified types.
        /// </summary>
        /// <param name="v1">Left operand</param>
        /// <param name="v2">Right operand</param>
        /// <returns>A Variant that contains the result of dividing this variant by v</returns>
        public static Variant operator /(Variant v1, Variant v2) {
            if (v1 == null) {
                throw new ArgumentNullException(nameof(v1));
            }
            if (v2 == null) {
                throw new ArgumentNullException(nameof(v2));
            }
            return Symbol.LargestType(v1.Type, v2.Type) switch {
                SymType.DOUBLE =>   new Variant(v1.DoubleValue / v2.DoubleValue),
                SymType.INTEGER =>  new Variant(v1.IntValue / v2.IntValue),
                SymType.FLOAT =>    new Variant(v1.RealValue / v2.RealValue),
                SymType.COMPLEX =>  new Variant(v1.ComplexValue / v2.ComplexValue),
                _ => throw new InvalidOperationException("Division not permitted on type"),
            };
        }

        /// <summary>
        /// Provides an alternative to operator/ for languages that do not support
        /// operator overloading.
        /// </summary>
        /// <param name="v">The variant to be divided into this one</param>
        /// <returns>A Variant that contains the result of dividing this variant by v</returns>
        public Variant Divide(Variant v) {
            return this / v;
        }

        /// <summary>
        /// Returns the modulus (remainder) when v1 is divided by v2. If v2 is zero
        /// then an exception is thrown. The type of the result is
        /// the largest of the two specified types.
        /// </summary>
        /// <param name="v1">Left operand</param>
        /// <param name="v2">Right operand</param>
        /// <returns>A Variant that contains the remainder after dividing this variant by v</returns>
        public static Variant operator %(Variant v1, Variant v2) {
            if (v1 == null) {
                throw new ArgumentNullException(nameof(v1));
            }
            if (v2 == null) {
                throw new ArgumentNullException(nameof(v2));
            }
            return Symbol.LargestType(v1.Type, v2.Type) switch {
                SymType.DOUBLE =>   new Variant(v1.DoubleValue % v2.DoubleValue),
                SymType.INTEGER =>  new Variant(v1.IntValue % v2.IntValue),
                SymType.FLOAT =>    new Variant(v1.RealValue % v2.RealValue),
                _ => throw new InvalidOperationException("Modulo not permitted on type"),
            };
        }

        /// <summary>
        /// Provides an alternative to operator% for languages that do not support
        /// operator overloading.
        /// </summary>
        /// <param name="v">The variant for which the remainder is to be obtained</param>
        /// <returns>A Variant that contains the remainder after dividing this variant by v</returns>
        public Variant Modulus(Variant v) {
            return this % v;
        }

        /// <summary>
        /// Returns the value of the variant raised to the specified power. The type of the
        /// result is the type of the largest of the base or the power. Note that the results
        /// are undefined if the variant base is negative.
        /// </summary>
        /// <param name="v">A variant that specifies the power</param>
        /// <returns>A Variant that contains the result of raising this variant to the power of v</returns>
        public Variant Pow(Variant v) {
            if (v == null) {
                throw new ArgumentNullException(nameof(v));
            }
            return Symbol.LargestType(Type, v.Type) switch {
                SymType.DOUBLE =>   new Variant(Math.Pow(DoubleValue, v.DoubleValue)),
                SymType.INTEGER =>  new Variant((int)Math.Pow(IntValue, v.IntValue)),
                SymType.FLOAT =>    new Variant((float)Math.Pow(RealValue, v.RealValue)),
                SymType.COMPLEX =>  new Variant(Complex.Pow(ComplexValue, v.ComplexValue)),
                _ => throw new InvalidOperationException("Exponentiation not permitted on type"),
            };
        }

        /// <summary>
        /// Sets the boolean value of a variant.
        /// </summary>
        /// <param name="boolValue">Boolean value to set</param>
        public void Set(bool boolValue) {
            BoolValue = boolValue;
            IntValue = boolValue ? -1 : 0;
            _value = IntValue;
            StringValue = BoolValue.ToString();
            _complexValue = boolValue ? -1 : 0;
            Type = SymType.BOOLEAN;
            HasValue = true;
        }

        /// <summary>
        /// Sets the integer value of a variant.
        /// </summary>
        /// <param name="intValue">Integer value to set</param>
        public void Set(int intValue) {
            _value = intValue;
            IntValue = intValue;
            BoolValue = intValue != 0;
            StringValue = intValue.ToString();
            _complexValue = _value;
            Type = SymType.INTEGER;
            HasValue = true;
        }

        /// <summary>
        /// Sets the float value of a variant.
        /// </summary>
        /// <param name="floatValue">Float value to set</param>
        public void Set(float floatValue) {
            _value = floatValue;
            IntValue = (int)floatValue;
            BoolValue = (int)floatValue != 0;
            StringValue = floatValue.ToString(CultureInfo.InvariantCulture);
            _complexValue = _value;
            Type = SymType.FLOAT;
            HasValue = true;
        }

        /// <summary>
        /// Sets the double value of a variant.
        /// </summary>
        /// <param name="doubleValue">Double value to set</param>
        public void Set(double doubleValue) {
            _value = doubleValue;
            IntValue = (int)doubleValue;
            BoolValue = (int)doubleValue != 0;
            StringValue = doubleValue.ToString(CultureInfo.InvariantCulture);
            _complexValue = _value;
            Type = SymType.DOUBLE;
            HasValue = true;
        }

        /// <summary>
        /// Sets the string value of a variant.
        /// </summary>
        /// <param name="stringValue">String value to set</param>
        public void Set(string stringValue) {
            double.TryParse(stringValue, out _value);
            IntValue = (int)_value;
            BoolValue = IntValue != 0;
            StringValue = stringValue;
            _complexValue = _value;
            Type = SymType.CHAR;
            HasValue = true;
        }

        /// <summary>
        /// Sets the complex value of a variant.
        /// </summary>
        /// <param name="complexValue">Complex value to set</param>
        public void Set(Complex complexValue) {
            _value = complexValue.Real;
            IntValue = (int)complexValue.Real;
            BoolValue = (int)complexValue.Real != 0;
            StringValue = complexValue.ToString();
            _complexValue = complexValue;
            Type = SymType.COMPLEX;
            HasValue = true;
        }

        /// <summary>
        /// Returns whether the variant value is zero.
        /// </summary>
        /// <returns><c>true</c> if the variant is zero; otherwise, <c>false</c>.</returns>
        public bool IsZero => Compare(new Variant(0));

        /// <summary>
        /// Compares a variant against an integer value.
        /// </summary>
        /// <param name="v">The integer value to compare against</param>
        /// <returns><c>true</c> if the variant is zero; otherwise, <c>false</c>.</returns>
        public bool Compare(int v) {
            return Compare(new Variant(v));
        }

        /// <summary>
        /// Compares a variant against another variant.
        /// </summary>
        /// <param name="v">The variant to compare against</param>
        /// <returns><c>true</c> if the two variants match; otherwise, <c>false</c>.</returns>
        public bool Compare(Variant v) {
            if (v == null) {
                throw new ArgumentNullException(nameof(v));
            }
            return Type switch {
                SymType.INTEGER =>  IntValue == v.IntValue,
                SymType.FLOAT =>    _value.CompareTo(v.RealValue) == 0,
                SymType.DOUBLE =>   _value.CompareTo(v.DoubleValue) == 0,
                SymType.COMPLEX =>  _complexValue == v.ComplexValue,
                _ => false,
            };
        }

        /// <summary>
        /// Returns the boolean value of this variant.
        /// </summary>
        public bool BoolValue { get; private set; }

        /// <summary>
        /// Returns the integer value of this variant.
        /// </summary>
        public int IntValue { get; private set; }

        /// <summary>
        /// Returns the float value of this variant.
        /// </summary>
        public float RealValue => (float)_value;

        /// <summary>
        /// Returns the string value of this variant.
        /// </summary>
        public string StringValue { get; private set; }

        /// <summary>
        /// Returns the double value of this variant.
        /// </summary>
        public double DoubleValue => _value;

        /// <summary>
        /// Returns the complex value of this variant.
        /// </summary>
        public Complex ComplexValue => _complexValue;

        /// <summary>
        /// Returns whether this variant has an explicit value
        /// set.
        /// </summary>
        public bool HasValue { get; private set; }

        /// <summary>
        /// Returns the current underlying type of the variant.
        /// </summary>
        public SymType Type { get; private set; }

        /// <summary>
        /// Returns the variant value as a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return StringValue;
        }
    }
}
