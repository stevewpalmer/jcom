// JCom Compiler Toolkit
// Variant class
//
// Authors:
//  Steve Palmer
//
// Copyright (C) 2021-2024 Steve Palmer
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

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;

namespace JComLib;

/// <summary>
/// Variant type
/// </summary>
public enum VariantType {
    NONE,
    INTEGER,
    FLOAT,
    DOUBLE,
    BOOLEAN,
    COMPLEX,
    STRING
}

/// <summary>
/// Encapsulates a value in any supported type and provides an
/// interface for setting and retrieving the value.
/// </summary>
public class Variant {

    public Variant() {
        HasValue = false;
        Type = VariantType.NONE;
    }

    /// <summary>
    /// Instantiates a variant from an object.
    /// </summary>
    /// <param name="value">Object value to set</param>
    public Variant(object value) {
        if (value is Variant other) {
            Set(other);
            return;
        }
        if (value is float floatValue) {
            Set(floatValue);
            return;
        }
        if (value is int intValue) {
            Set(intValue);
            return;
        }
        if (value is double doubleValue) {
            Set(doubleValue);
            return;
        }
        if (value is string stringValue) {
            Set(stringValue);
            return;
        }
        if (value is bool boolValue) {
            Set(boolValue);
            return;
        }
        if (value is Complex complexValue) {
            Set(complexValue);
            return;
        }
        throw new NotImplementedException($"Invalid object type {value}");
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
    /// <param name="value">Float value to set</param>
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
    /// Implements the addition operator on variants. The type of the result is
    /// the largest of the two specified types.
    /// </summary>
    /// <param name="v1">First variant</param>
    /// <param name="v2">Second variant</param>
    /// <returns>A Variant that contains the result of adding v to this variant</returns>
    public static Variant operator +(Variant v1, Variant v2) {
        ArgumentNullException.ThrowIfNull(v1);
        ArgumentNullException.ThrowIfNull(v2);
        return LargestType(v1.Type, v2.Type) switch {
            VariantType.DOUBLE => new Variant(v1.DoubleValue + v2.DoubleValue),
            VariantType.INTEGER => new Variant(v1.IntValue + v2.IntValue),
            VariantType.FLOAT => new Variant(v1.RealValue + v2.RealValue),
            VariantType.COMPLEX => new Variant(v1.ComplexValue + v2.ComplexValue),
            VariantType.STRING => new Variant(v1.StringValue + v2.StringValue),
            _ => throw new InvalidOperationException("Addition not permitted on type")
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
        ArgumentNullException.ThrowIfNull(v1);
        return v1.Type switch {
            VariantType.DOUBLE => new Variant(-v1.DoubleValue),
            VariantType.INTEGER => new Variant(-v1.IntValue),
            VariantType.FLOAT => new Variant(-v1.RealValue),
            VariantType.COMPLEX => new Variant(-v1.ComplexValue),
            _ => throw new InvalidOperationException("Unary minus not permitted on type")
        };
    }

    /// <summary>
    /// Provides an alternative to unary operator minus for languages that do not support
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
        ArgumentNullException.ThrowIfNull(v1);
        ArgumentNullException.ThrowIfNull(v2);
        return LargestType(v1.Type, v2.Type) switch {
            VariantType.DOUBLE => new Variant(v1.DoubleValue - v2.DoubleValue),
            VariantType.INTEGER => new Variant(v1.IntValue - v2.IntValue),
            VariantType.FLOAT => new Variant(v1.RealValue - v2.RealValue),
            VariantType.COMPLEX => new Variant(v1.ComplexValue - v2.ComplexValue),
            _ => throw new InvalidOperationException("Subtraction not permitted on type")
        };
    }

    /// <summary>
    /// Provides an alternative to operator minus for languages that do not support
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
        ArgumentNullException.ThrowIfNull(v1);
        ArgumentNullException.ThrowIfNull(v2);
        return LargestType(v1.Type, v2.Type) switch {
            VariantType.DOUBLE => new Variant(v1.DoubleValue * v2.DoubleValue),
            VariantType.INTEGER => new Variant(v1.IntValue * v2.IntValue),
            VariantType.FLOAT => new Variant(v1.RealValue * v2.RealValue),
            VariantType.COMPLEX => new Variant(v1.ComplexValue * v2.ComplexValue),
            _ => throw new InvalidOperationException("Multiplication not permitted on type")
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
        ArgumentNullException.ThrowIfNull(v1);
        ArgumentNullException.ThrowIfNull(v2);
        return LargestType(v1.Type, v2.Type) switch {
            VariantType.DOUBLE => new Variant(v1.DoubleValue / v2.DoubleValue),
            VariantType.INTEGER => new Variant(v1.IntValue / v2.IntValue),
            VariantType.FLOAT => new Variant(v1.RealValue / v2.RealValue),
            VariantType.COMPLEX => new Variant(v1.ComplexValue / v2.ComplexValue),
            _ => throw new InvalidOperationException("Division not permitted on type")
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
        ArgumentNullException.ThrowIfNull(v1);
        ArgumentNullException.ThrowIfNull(v2);
        return LargestType(v1.Type, v2.Type) switch {
            VariantType.DOUBLE => new Variant(v1.DoubleValue % v2.DoubleValue),
            VariantType.INTEGER => new Variant(v1.IntValue % v2.IntValue),
            VariantType.FLOAT => new Variant(v1.RealValue % v2.RealValue),
            _ => throw new InvalidOperationException("Modulo not permitted on type")
        };
    }

    /// <summary>
    /// Returns the value of the variant raised to the specified power. The type of the
    /// result is the type of the largest of the base or the power. Note that the results
    /// are undefined if the variant base is negative.
    /// </summary>
    /// <param name="v">A variant that specifies the power</param>
    /// <returns>A Variant that contains the result of raising this variant to the power of v</returns>
    public Variant Pow(Variant v) {
        ArgumentNullException.ThrowIfNull(v);
        return LargestType(Type, v.Type) switch {
            VariantType.DOUBLE => new Variant(Math.Pow(DoubleValue, v.DoubleValue)),
            VariantType.INTEGER => new Variant((int)Math.Pow(IntValue, v.IntValue)),
            VariantType.FLOAT => new Variant((float)Math.Pow(RealValue, v.RealValue)),
            VariantType.COMPLEX => new Variant(Complex.Pow(ComplexValue, v.ComplexValue)),
            _ => throw new InvalidOperationException("Exponentiation not permitted on type")
        };
    }


    /// <summary>
    /// Sets a variant from another variant.
    /// </summary>
    /// <param name="otherVariant">Other variant to set</param>
    private void Set(Variant otherVariant) {
        BoolValue = otherVariant.BoolValue;
        IntValue = otherVariant.IntValue;
        DoubleValue = otherVariant.DoubleValue;
        RealValue = otherVariant.RealValue;
        StringValue = otherVariant.StringValue;
        ComplexValue = otherVariant.ComplexValue;
        Type = otherVariant.Type;
        HasValue = otherVariant.HasValue;
    }

    /// <summary>
    /// Sets the boolean value of a variant.
    /// </summary>
    /// <param name="boolValue">Boolean value to set</param>
    public void Set(bool boolValue) {
        BoolValue = boolValue;
        IntValue = boolValue ? -1 : 0;
        DoubleValue = IntValue;
        RealValue = IntValue;
        StringValue = BoolValue.ToString();
        ComplexValue = boolValue ? -1 : 0;
        Type = VariantType.BOOLEAN;
        HasValue = true;
    }

    /// <summary>
    /// Sets the integer value of a variant.
    /// </summary>
    /// <param name="intValue">Integer value to set</param>
    public void Set(int intValue) {
        DoubleValue = intValue;
        RealValue = intValue;
        IntValue = intValue;
        BoolValue = intValue != 0;
        StringValue = intValue.ToString();
        ComplexValue = intValue;
        Type = VariantType.INTEGER;
        HasValue = true;
    }

    /// <summary>
    /// Sets the float value of a variant.
    /// </summary>
    /// <param name="floatValue">Float value to set</param>
    private void Set(float floatValue) {
        RealValue = floatValue;
        DoubleValue = floatValue;
        IntValue = (int)floatValue;
        BoolValue = (int)floatValue != 0;
        StringValue = floatValue.ToString(CultureInfo.InvariantCulture);
        ComplexValue = floatValue;
        Type = VariantType.FLOAT;
        HasValue = true;
    }

    /// <summary>
    /// Sets the double value of a variant.
    /// </summary>
    /// <param name="doubleValue">Double value to set</param>
    private void Set(double doubleValue) {
        DoubleValue = doubleValue;
        RealValue = (float)doubleValue;
        IntValue = (int)doubleValue;
        BoolValue = (int)doubleValue != 0;
        StringValue = doubleValue.ToString(CultureInfo.InvariantCulture);
        ComplexValue = doubleValue;
        Type = VariantType.DOUBLE;
        HasValue = true;
    }

    /// <summary>
    /// Sets the string value of a variant.
    /// </summary>
    /// <param name="stringValue">String value to set</param>
    public void Set(string stringValue) {
        double doubleValue = double.TryParse(stringValue, out double v) ? v : 0;
        DoubleValue = doubleValue;
        RealValue = (float)doubleValue;
        IntValue = (int)DoubleValue;
        BoolValue = IntValue != 0;
        StringValue = stringValue;
        ComplexValue = DoubleValue;
        Type = VariantType.STRING;
        HasValue = true;
    }

    /// <summary>
    /// Sets the complex value of a variant.
    /// </summary>
    /// <param name="complexValue">Complex value to set</param>
    private void Set(Complex complexValue) {
        DoubleValue = complexValue.Real;
        RealValue = (float)complexValue.Real;
        IntValue = (int)complexValue.Real;
        BoolValue = (int)complexValue.Real != 0;
        StringValue = complexValue.ToString();
        ComplexValue = complexValue;
        Type = VariantType.COMPLEX;
        HasValue = true;
    }

    /// <summary>
    /// Returns whether the variant value is zero.
    /// </summary>
    /// <returns><c>true</c> if the variant is zero; otherwise, <c>false</c>.</returns>
    public bool IsZero => CompareTo(new Variant(0)) == 0;

    /// <summary>
    /// Returns whether the variant is a number.
    /// </summary>
    /// <returns><c>true</c> if the variant is a number; otherwise, <c>false</c>.</returns>
    public bool IsNumber => Type is VariantType.INTEGER or VariantType.DOUBLE or VariantType.FLOAT or VariantType.COMPLEX;

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is equal to
    /// the current <see cref="Variant"/>.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to compare with the current <see cref="Variant"/>.</param>
    /// <returns><c>true</c> if the specified <see cref="object"/> is equal to the current
    /// <see cref="Variant"/>; otherwise, <c>false</c>.</returns>
    public override bool Equals(object obj) {
        return obj is Variant s2 && CompareTo(s2) == 0;
    }

    /// <summary>
    /// Serves as a hash function for a <see><cref>Variant</cref>. Note that we use
    /// two mutable values to identify this Variant. This is safe because two
    /// distinct Variant objects with the same value should hash to the same
    /// result in order to match in a collection.
    /// </see>object.
    /// </summary>
    /// <returns>A hash code for this instance that is suitable for use in hashing
    /// algorithms and data structures such as a hash table.</returns>
    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public override int GetHashCode() {
        unchecked {
            return 17 * 23 + (StringValue + Type).GetHashCode();
        }
    }

    /// <summary>
    /// Implements the equality operator between two variants.
    /// </summary>
    /// <param name="s1">First variant</param>
    /// <param name="s2">Second variant</param>
    /// <returns>True if the two variants are equal, false otherwise</returns>
    public static bool operator ==(Variant s1, Variant s2) => s1 is null ? s2 is null : s1.CompareTo(s2) == 0;

    /// <summary>
    /// Implements the not-equality operator between two variants.
    /// </summary>
    /// <param name="s1">First variant</param>
    /// <param name="s2">Second variant</param>
    /// <returns>True if the two variants are equal, false otherwise</returns>
    public static bool operator !=(Variant s1, Variant s2) => s1 is null ? s2 is not null : s1.CompareTo(s2) != 0;

    /// <summary>
    /// Implements the equality operator between a variant and a
    /// floating point type.
    /// </summary>
    /// <param name="s1">First variant</param>
    /// <param name="s2">Floating point value</param>
    /// <returns>True if the two values are equal, false otherwise</returns>
    public static bool operator ==(Variant s1, float s2) => s1 is not null && s1.CompareTo(new Variant(s2)) == 0;

    /// <summary>
    /// Implements the not-equality operator between a variant and a
    /// floating point type.
    /// </summary>
    /// <param name="s1">First variant</param>
    /// <param name="s2">Floating point value</param>
    /// <returns>True if the two values are not equal, false otherwise</returns>
    public static bool operator !=(Variant s1, float s2) => s1 is not null && s1.CompareTo(new Variant(s2)) != 0;

    /// <summary>
    /// Implements the equality operator between a floating point type
    /// and a variant.
    /// </summary>
    /// <param name="s1">Floating point value</param>
    /// <param name="s2">Second variant</param>
    /// <returns>True if the two values are equal, false otherwise</returns>
    public static bool operator ==(float s1, Variant s2) => s2 is not null && s1.CompareTo(s2.RealValue) == 0;

    /// <summary>
    /// Implements the not-equality operator between a floating point
    /// type and a variant.
    /// </summary>
    /// <param name="s1">Floating point value</param>
    /// <param name="s2">Second variant</param>
    /// <returns>True if the two values are not equal, false otherwise</returns>
    public static bool operator !=(float s1, Variant s2) => s2 is not null && s1.CompareTo(s2.RealValue) != 0;

    /// <summary>
    /// Implements the equality operator between a variant and a
    /// double type.
    /// </summary>
    /// <param name="s1">First variant</param>
    /// <param name="s2">Double value</param>
    /// <returns>True if the two values are equal, false otherwise</returns>
    public static bool operator ==(Variant s1, double s2) => s1 is not null && s1.CompareTo(new Variant(s2)) == 0;

    /// <summary>
    /// Implements the not-equality operator between a variant and a
    /// double type.
    /// </summary>
    /// <param name="s1">First variant</param>
    /// <param name="s2">Double value</param>
    /// <returns>True if the two values are not equal, false otherwise</returns>
    public static bool operator !=(Variant s1, double s2) => s1 is not null && s1.CompareTo(new Variant(s2)) != 0;

    /// <summary>
    /// Implements the equality operator between a double type
    /// and a variant.
    /// </summary>
    /// <param name="s1">Double value</param>
    /// <param name="s2">Second variant</param>
    /// <returns>True if the two values are equal, false otherwise</returns>
    public static bool operator ==(double s1, Variant s2) => s2 is not null && s1.CompareTo(s2.DoubleValue) == 0;

    /// <summary>
    /// Implements the not-equality operator between a double
    /// type and a variant.
    /// </summary>
    /// <param name="s1">Double value</param>
    /// <param name="s2">Second variant</param>
    /// <returns>True if the two values are not equal, false otherwise</returns>
    public static bool operator !=(double s1, Variant s2) => s2 is not null && s1.CompareTo(s2.DoubleValue) != 0;

    /// <summary>
    /// Implements the equality operator between a variant and an
    /// integer type.
    /// </summary>
    /// <param name="s1">First variant</param>
    /// <param name="s2">Integer value</param>
    /// <returns>True if the two values are equal, false otherwise</returns>
    public static bool operator ==(Variant s1, int s2) => s1 is not null && s1.CompareTo(new Variant(s2)) == 0;

    /// <summary>
    /// Implements the not-equality operator between a variant and an
    /// integer type.
    /// </summary>
    /// <param name="s1">First variant</param>
    /// <param name="s2">Floating point value</param>
    /// <returns>True if the two values are not equal, false otherwise</returns>
    public static bool operator !=(Variant s1, int s2) => s1 is not null && s1.CompareTo(new Variant(s2)) != 0;

    /// <summary>
    /// Implements the equality operator between an integer type
    /// and a variant.
    /// </summary>
    /// <param name="s1">Integer value</param>
    /// <param name="s2">Second variant</param>
    /// <returns>True if the two values are equal, false otherwise</returns>
    public static bool operator ==(int s1, Variant s2) => s2 is not null && s1.CompareTo(s2.IntValue) == 0;

    /// <summary>
    /// Implements the not-equality operator between an integer
    /// type and a variant.
    /// </summary>
    /// <param name="s1">Integer value</param>
    /// <param name="s2">Second variant</param>
    /// <returns>True if the two values are not equal, false otherwise</returns>
    public static bool operator !=(int s1, Variant s2) => s2 is not null && s1.CompareTo(s2.IntValue) != 0;

    /// <summary>
    /// Implements the greater than operator between two variants.
    /// </summary>
    /// <param name="s1">First variant</param>
    /// <param name="s2">Second variant</param>
    /// <returns>True if the two variants are equal, false otherwise</returns>
    public static bool operator >(Variant s1, Variant s2) {
        if (s1 is null) {
            return false;
        }
        return s1.CompareTo(s2) > 0;
    }

    /// <summary>
    /// Implements the greater than or equals operator between two variants.
    /// </summary>
    /// <param name="s1">First variant</param>
    /// <param name="s2">Second variant</param>
    /// <returns>True if the two variants are equal, false otherwise</returns>
    public static bool operator >=(Variant s1, Variant s2) {
        if (s1 is null) {
            return false;
        }
        return s1.CompareTo(s2) >= 0;
    }

    /// <summary>
    /// Implements the less than operator between two variants.
    /// </summary>
    /// <param name="s1">First variant</param>
    /// <param name="s2">Second variant</param>
    /// <returns>True if the two variants are equal, false otherwise</returns>
    public static bool operator <(Variant s1, Variant s2) {
        if (s1 is null) {
            return false;
        }
        return s1.CompareTo(s2) < 0;
    }

    /// <summary>
    /// Implements the less than or equls operator between two variants.
    /// </summary>
    /// <param name="s1">First variant</param>
    /// <param name="s2">Second variant</param>
    /// <returns>True if the two variants are equal, false otherwise</returns>
    public static bool operator <=(Variant s1, Variant s2) {
        if (s1 is null) {
            return false;
        }
        return s1.CompareTo(s2) <= 0;
    }

    /// <summary>
    /// Compares a variant against an integer value.
    /// </summary>
    /// <param name="v">The integer value to compare against</param>
    /// <returns><c>true</c> if the variant is zero; otherwise, <c>false</c>.</returns>
    public bool Compare(int v) {
        return CompareTo(new Variant(v)) == 0;
    }

    /// <summary>
    /// Compares a variant against another variant.
    /// </summary>
    /// <param name="v">The variant to compare against</param>
    /// <returns><c>true</c> if the two variants match; otherwise, <c>false</c>.</returns>
    public int CompareTo(Variant v) {
        if (v is null) {
            return 1;
        }
        return Type switch {
            VariantType.BOOLEAN => BoolValue.CompareTo(v.BoolValue),
            VariantType.INTEGER => IntValue.CompareTo(v.IntValue),
            VariantType.FLOAT => RealValue.CompareTo(v.RealValue),
            VariantType.DOUBLE => DoubleValue.CompareTo(v.DoubleValue),
            VariantType.COMPLEX => ComplexValue.Real.CompareTo(v.ComplexValue.Real),
            VariantType.STRING => string.Compare(StringValue, v.StringValue, StringComparison.Ordinal),
            _ => 0
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
    public float RealValue { get; private set; }

    /// <summary>
    /// Returns the string value of this variant.
    /// </summary>
    public string StringValue { get; private set; }

    /// <summary>
    /// Returns the double value of this variant.
    /// </summary>
    public double DoubleValue { get; private set; }

    /// <summary>
    /// Returns the complex value of this variant.
    /// </summary>
    public Complex ComplexValue { get; private set; }

    /// <summary>
    /// Returns whether this variant has an explicit value
    /// set.
    /// </summary>
    public bool HasValue { get; private set; }

    /// <summary>
    /// Returns the current underlying type of the variant.
    /// </summary>
    public VariantType Type { get; private set; }

    /// <summary>
    /// Returns the variant value as a string.
    /// </summary>
    /// <returns></returns>
    public override string ToString() {
        return StringValue;
    }

    /// <summary>
    /// Maps a variant type to a system type.
    /// </summary>
    /// <param name="type">Variant type</param>
    /// <returns>The corresponding system type</returns>
    public static Type VariantTypeToSystemType(VariantType type) {
        return type switch {
            VariantType.STRING => typeof(string),
            VariantType.FLOAT => typeof(float),
            VariantType.DOUBLE => typeof(double),
            VariantType.INTEGER => typeof(int),
            VariantType.BOOLEAN => typeof(bool),
            VariantType.COMPLEX => typeof(Complex),
            _ => throw new ArgumentException($"No system type for {type}")
        };
    }

    // Return the variant type that can contain the largest of the two given. The types
    // must both be numeric. Thus, if one type is double and the other is integer, a
    // double is returned since it can contain a value of both types.
    private static VariantType LargestType(VariantType op1, VariantType op2) {
        if (op1 == VariantType.INTEGER) {
            return op2;
        }
        if (op1 == VariantType.FLOAT) {
            return op2 == VariantType.INTEGER ? op1 : op2;
        }
        return op1;
    }
}