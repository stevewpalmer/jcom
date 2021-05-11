// JCom Compiler Toolkit
// VarType class
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
using System.Diagnostics;
using System.Globalization;
using System.Numerics;

namespace JComLib {

    /// <summary>
    /// Variant type
    /// </summary>
    public enum VarTypeType {
        NONE,
        INTEGER,
        FLOAT,
        DOUBLE,
        BOOLEAN,
        COMPLEX,
        STRING
    }

    /// <summary>
    /// Encapsulates a symbol value in any supported type and provides an
    /// interface for setting and retrieving the value.
    /// </summary>
    public class VarType {
        private double _value;
        private Complex _complexValue;

        public VarType() {
            HasValue = false;
            Type = VarTypeType.NONE;
        }

        /// <summary>
        /// Instantiates a variant from an object.
        /// </summary>
        /// <param name="value">Object value to set</param>
        public VarType(object value) {
            if (value is float floatValue)      { Set(floatValue); return; }
            if (value is int intValue)          { Set(intValue); return; }
            if (value is double doubleValue)    { Set(doubleValue); return; }
            if (value is string stringValue)    { Set(stringValue); return; }
            if (value is bool boolValue)        { Set(boolValue); return; }
            if (value is Complex complexValue)  { Set(complexValue); return; }
            Debug.Assert(false, "Invalid object type");
        }

        /// <summary>
        /// Instantiates a boolean variant.
        /// </summary>
        /// <param name="value">Boolean value to set</param>
        public VarType(bool value) {
            Set(value);
        }

        /// <summary>
        /// Instantiates an integer variant.
        /// </summary>
        /// <param name="value">Integer value to set</param>
        public VarType(int value) {
            Set(value);
        }

        /// <summary>
        /// Instantiates a floating point variant.
        /// </summary>
        /// <param name="value">Float value to set</param>
        public VarType(float value) {
            Set(value);
        }

        /// <summary>
        /// Instantiates a double variant.
        /// </summary>
        /// <param name="value">Double value to set</param>
        public VarType(double value) {
            Set(value);
        }

        /// <summary>
        /// Instantiates a string variant.
        /// </summary>
        /// <param name="value">String value to set</param>
        public VarType(string value) {
            Set(value);
        }

        /// <summary>
        /// Instantiates a complex variant.
        /// </summary>
        /// <param name="value">Complex value to set</param>
        public VarType(Complex value) {
            Set(value);
        }

        /// <summary>
        /// Conversion operator to create an integer variant with the
        /// specified value.
        /// </summary>
        /// <param name="i">An integer value</param>
        /// <returns>A new VarType representing the specified integer value</returns>
        public static explicit operator VarType(int i) {
            return new VarType(i);
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
            Type = VarTypeType.BOOLEAN;
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
            Type = VarTypeType.INTEGER;
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
            Type = VarTypeType.FLOAT;
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
            Type = VarTypeType.DOUBLE;
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
            Type = VarTypeType.STRING;
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
            Type = VarTypeType.COMPLEX;
            HasValue = true;
        }

        /// <summary>
        /// Returns whether the variant value is zero.
        /// </summary>
        /// <returns><c>true</c> if the variant is zero; otherwise, <c>false</c>.</returns>
        public bool IsZero => Compare(new VarType(0));

        /// <summary>
        /// Returns whether the variant is a number.
        /// </summary>
        /// <returns><c>true</c> if the variant is a number; otherwise, <c>false</c>.</returns>
        public bool IsNumber => Type == VarTypeType.INTEGER ||
                                Type == VarTypeType.DOUBLE ||
                                Type == VarTypeType.FLOAT ||
                                Type == VarTypeType.COMPLEX;

        /// <summary>
        /// Compares a variant against an integer value.
        /// </summary>
        /// <param name="v">The integer value to compare against</param>
        /// <returns><c>true</c> if the variant is zero; otherwise, <c>false</c>.</returns>
        public bool Compare(int v) {
            return Compare(new VarType(v));
        }

        /// <summary>
        /// Compares a variant against another variant.
        /// </summary>
        /// <param name="v">The variant to compare against</param>
        /// <returns><c>true</c> if the two variants match; otherwise, <c>false</c>.</returns>
        public bool Compare(VarType v) {
            if (v == null) {
                throw new ArgumentNullException(nameof(v));
            }
            return Type switch {
                VarTypeType.INTEGER =>  IntValue == v.IntValue,
                VarTypeType.FLOAT =>    _value.CompareTo(v.RealValue) == 0,
                VarTypeType.DOUBLE =>   _value.CompareTo(v.DoubleValue) == 0,
                VarTypeType.COMPLEX =>  _complexValue == v.ComplexValue,
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
        public VarTypeType Type { get; private set; }

        /// <summary>
        /// Returns the variant value as a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return StringValue;
        }
    }
}
