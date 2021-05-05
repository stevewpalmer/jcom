// FORTRAN Runtime Library
// Intrinsic functions
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
using System.Linq;
using System.Numerics;
using JComLib;

namespace JFortranLib {

    /// <summary>
    /// Fortran external intrinsics.
    /// </summary>
    public static class Intrinsics {

        /// <summary>
        /// Default randomizer seed to ensure that repeated iterations of
        /// the program return a consistent result if RANDOMIZE or RAND(x)
        /// are not used.
        /// </summary>
        private const int DEFAULT_SEED = 0x38267873;

        /// <summary>
        /// Random number seed
        /// </summary>
        private static Random _rand = new(DEFAULT_SEED);

        /// <summary>
        /// Returns the absolute of the given integer value.
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>The absolute value</returns>
        public static int ABS(ref int value) {
            return Math.Abs(value);
        }

        /// <summary>
        /// Returns the absolute of the given double precision value.
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>The absolute value</returns>
        public static double ABS(ref double value) {
            return Math.Abs(value);
        }

        /// <summary>
        /// Returns the absolute of the given float value.
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>The absolute value</returns>
        public static double ABS(ref float value) {
            return Math.Abs(value);
        }

        /// <summary>
        /// Returns the absolute of the given complex number.
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>The absolute value</returns>
        public static double ABS(ref Complex value) {
            return Complex.Abs(value);
        }

        /// <summary>
        /// Arc-cosine; the result is in the range 0 to + π.
        /// </summary>
        /// <param name="m">Value</param>
        /// <returns>The arc-cosine of m</returns>
        public static double ACOS(ref float m) {
            return Math.Acos(m);
        }

        /// <summary>
        /// Arc-cosine; the result is in the range 0 to + π.
        /// </summary>
        /// <param name="m">Value</param>
        /// <returns>The arc-cosine of m</returns>
        public static double ACOS(ref double m) {
            return Math.Acos(m);
        }

        /// <summary>
        /// Returns the imaginary portion of a complex number.
        /// </summary>
        /// <param name="m">Value</param>
        /// <returns>The imaginary part of the complex number m</returns>
        public static double AIMAG(ref Complex m) {
            return m.Imaginary;
        }

        /// <summary>
        /// Converts to integer by truncation.
        /// </summary>
        /// <param name="d">Value to truncate</param>
        /// <returns>The truncated value of d</returns>
        public static float AINT(ref float d) {
            return (int)d;
        }

        /// <summary>
        /// Logarithm to base e (where e=2.718...).
        /// Generic real type version.
        /// </summary>
        /// <param name="v">Value</param>
        /// <returns>The logarithm of v</returns>
        public static float ALOG(ref float v) {
            return (float)Math.Log(v);
        }

        /// <summary>
        /// Logarithm to base 10.
        /// Generic real type version.
        /// </summary>
        /// <param name="v">Value</param>
        /// <returns>The base 10 logarithm of v</returns>
        public static float ALOG10(ref float v) {
            return (float)Math.Log10(v);
        }

        /// <summary>
        /// Logarithm to base 10.
        /// Generic double version.
        /// </summary>
        /// <param name="v">Value</param>
        /// <returns>The base 10 logarithm of v</returns>
        public static float ALOG10(ref double v) {
            return (float)Math.Log10(v);
        }

        /// <summary>
        /// Returns the largest of its arguments.
        /// </summary>
        /// <param name="args">Arguments.</param>
        /// <returns>The largest value in the list args</returns>
        public static float AMAX0(params object[] args) {
            return (float)MAX(args);
        }
        
        /// <summary>
        /// Returns the largest of its arguments.
        /// </summary>
        /// <param name="args">Arguments.</param>
        /// <returns>The largest value in the list args</returns>
        public static float AMAX1(params object[] args) {
            return (float)MAX(args);
        }

        /// <summary>
        /// Returns the smallest of its arguments.
        /// </summary>
        /// <param name="args">Arguments.</param>
        /// <returns>The smallest value in the list args</returns>
        public static float AMIN0(params object[] args) {
            return (float)MIN(args);
        }

        /// <summary>
        /// Returns the smallest of its arguments.
        /// </summary>
        /// <param name="args">Arguments.</param>
        /// <returns>The smallest value in the list args</returns>
        public static float AMIN1(params object[] args) {
            return (float)MIN(args);
        }

        /// <summary>
        /// MOD intrinsic function.
        /// Generic real type version.
        /// </summary>
        /// <param name="m1">First value</param>
        /// <param name="m2">Second value</param>
        /// <returns>The remainder after m1 is divided by m2</returns>
        public static float AMOD(ref float m1, ref float m2) {
            return m1 % m2;
        }

        /// <summary>
        /// ANINT intrinsic function.
        /// Convert m to the nearest whole number.
        /// </summary>
        /// <param name="m">First value</param>
        /// <returns>The value m converted to the nearest whole number</returns>
        public static float ANINT(ref float m) {
            return m >= 0 ? (int)(m + 0.5f) : (int)(m - 0.5f);
        }

        /// <summary>
        /// ANINT intrinsic function.
        /// Convert m to the nearest whole number.
        /// </summary>
        /// <param name="m">First value</param>
        /// <returns>The value m converted to the nearest whole number</returns>
        public static double ANINT(ref double m) {
            return m >= 0 ? (int)(m + 0.5) : (int)(m - 0.5);
        }

        /// <summary>
        /// Returns the angle whose sine is the specified value.
        /// </summary>
        /// <param name="m">Value</param>
        /// <returns>The angle whose sine is the value m</returns>
        public static float ASIN(ref float m) {
            return (float)Math.Asin(m);
        }

        /// <summary>
        /// Returns the angle whose sine is the specified value.
        /// </summary>
        /// <param name="m">Value</param>
        /// <returns>The angle whose sine is the value m</returns>
        public static double ASIN(ref double m) {
            return Math.Asin(m);
        }

        /// <summary>
        /// Returns the angle whose tangent is the specified value.
        /// </summary>
        /// <param name="m1">Value</param>
        /// <returns>The angle whose tangent is m1</returns>
        public static float ATAN(ref float m1) {
            return (float)Math.Atan(m1);
        }

        /// <summary>
        /// Returns the angle whose tangent is the specified value.
        /// </summary>
        /// <param name="m1">Value</param>
        /// <returns>The angle whose tangent is m1</returns>
        public static double ATAN(ref double m1) {
            return Math.Atan(m1);
        }

        /// <summary>
        /// Arc-tangent of m1/m2 resolved into the correct quadrant, the result is in the range −π to + π. 
        /// It is an error to have both arguments zero.
        /// </summary>
        /// <param name="m1">First value</param>
        /// <param name="m2">Second value</param>
        /// <returns>The arc-tangent of m1/m2</returns>
        public static float ATAN2(ref float m1, ref float m2) {
            return (float)Math.Atan2(m1, m2);
        }

        /// <summary>
        /// Arc-tangent of m1/m2 resolved into the correct quadrant, the result is in the range −π to + π. 
        /// It is an error to have both arguments zero.
        /// </summary>
        /// <param name="m1">First value</param>
        /// <param name="m2">Second value</param>
        /// <returns>The arc-tangent of m1/m2</returns>
        public static double ATAN2(ref double m1, ref double m2) {
            return Math.Atan2(m1, m2);
        }

        /// <summary>
        /// Returns the absolute of the given value.
        /// </summary>
        /// <param name="d">Value</param>
        /// <returns>The absolute of the given complex number d</returns>
        public static float CABS(ref Complex d) {
            return (float)Complex.Abs(d);
        }

        /// <summary>
        /// Cosine of the angle in radians.
        /// </summary>
        /// <param name="d">Angle</param>
        /// <returns>The cosine of the given complex number d</returns>
        public static Complex CCOS(ref Complex d) {
            return Complex.Cos(d);
        }

        /// <summary>
        /// Returns Nth character in local character code table.
        /// </summary>
        /// <param name="n">Code of the character to return</param>
        /// <returns>The character whose ASCII value is represented by n</returns>
        public static string CHAR(int n) {
            return char.ToString((char)n);
        }

        /// <summary>
        /// Logarithm to base e (where e=2.718...)..
        /// </summary>
        /// <param name="v">Value</param>
        /// <returns>The logarithm of the given complex number v</returns>
        public static Complex CLOG(ref Complex v) {
            return Complex.Log(v);
        }

        /// <summary>
        /// Convert the specified value to a complex value.
        /// </summary>
        /// <param name="r">The real portion of the number</param>
        /// <param name="i">The imaginary portion of the number</param>
        /// <returns>A complex number comprising the given real and imaginary parts</returns>
        public static Complex CMPLX(int r, int i) {
            return new Complex(r, i);
        }

        /// <summary>
        /// Convert the specified value to a complex value.
        /// </summary>
        /// <param name="r">The real portion of the number</param>
        /// <param name="i">The imaginary portion of the number</param>
        /// <returns>A complex number comprising the given real and imaginary parts</returns>
        public static Complex CMPLX(double r, double i) {
            return new Complex(r, i);
        }

        /// <summary>
        /// Cosine of the angle in radians.
        /// </summary>
        /// <param name="m">Angle</param>
        /// <returns>The cosine of the angle m</returns>
        public static double COS(ref float m) {
            return Math.Cos(m);
        }

        /// <summary>
        /// Cosine of the angle in radians.
        /// </summary>
        /// <param name="m">Angle</param>
        /// <returns>The cosine of the angle m</returns>
        public static double COS(ref double m) {
            return Math.Cos(m);
        }

        /// <summary>
        /// Cosine of the angle in radians.
        /// </summary>
        /// <param name="m">Angle</param>
        /// <returns>The cosine of the complex number angle m</returns>
        public static Complex COS(ref Complex m) {
            return Complex.Cos(m);
        }

        /// <summary>
        /// Hyperbolic cosine of the angle in radians.
        /// </summary>
        /// <param name="m">Angle</param>
        /// <returns>The hyperbolic cosine of the angle m</returns>
        public static float COSH(ref float m) {
            return (float)Math.Cosh(m);
        }

        /// <summary>
        /// Hyperbolic cosine of the angle in radians.
        /// </summary>
        /// <param name="m">Angle</param>
        /// <returns>The hyperbolic cosine of the angle m</returns>
        public static double COSH(ref double m) {
            return Math.Cosh(m);
        }

        /// <summary>
        /// Sine of the angle in radians.
        /// </summary>
        /// <param name="d">Angle</param>
        /// <returns>The sine of the compexl number angle d</returns>
        public static Complex CSIN(ref Complex d) {
            return Complex.Sin(d);
        }

        /// <summary>
        /// SQRT intrinsic function
        /// Returns the square root of the given value.
        /// </summary>
        /// <param name="value">Input value</param>
        /// <returns>The square root of the complex number value</returns>
        public static Complex CSQRT(ref Complex value) {
            return Complex.Sqrt(value);
        }

        /// <summary>
        /// Returns the absolute of the given value.
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>The absolute of the value</returns>
        public static double DABS(ref double value) {
            return Math.Abs(value);
        }

        /// <summary>
        /// Arc-cosine; the result is in the range 0 to + π.
        /// </summary>
        /// <param name="m">Value</param>
        /// <returns>The arc-cosine of the angle m</returns>
        public static double DACOS(ref double m) {
            return Math.Acos(m);
        }

        /// <summary>
        /// Arc-sine; the result is in the range 0 to + π.
        /// </summary>
        /// <param name="m">Value</param>
        /// <returns>The arc-sine of the angle m</returns>
        public static double DASIN(ref double m) {
            return Math.Asin(m);
        }

        /// <summary>
        /// Returns the angle whose tangent is the specified value.
        /// </summary>
        /// <param name="m">Value</param>
        /// <returns>The angle whose tangent is the value m</returns>
        public static double DATAN(ref double m) {
            return Math.Atan(m);
        }

        /// <summary>
        /// Returns the angle whose tangent is the quotient of m1 and m2.
        /// </summary>
        /// <param name="m1">First value</param>
        /// <param name="m2">Second value</param>
        /// <returns>The angle whose tangent is the quotient of m1 and m2</returns>
        public static double DATAN2(ref double m1, ref double m2) {
            return Math.Atan2(m1, m2);
        }

        /// <summary>
        /// Comverts the specified integer to double.
        /// </summary>
        /// <param name="m">Value</param>
        /// <returns>The double version of the number m</returns>
        public static double DBLE(int m) {
            return m;
        }

        /// <summary>
        /// Comverts the specified float to double.
        /// </summary>
        /// <param name="m">Value</param>
        /// <returns>The double version of the number m</returns>
        public static double DBLE(float m) {
            return m;
        }

        /// <summary>
        /// Comverts the specified double to double.
        /// </summary>
        /// <param name="m">Value</param>
        /// <returns>The double version of the number m</returns>
        public static double DBLE(double m) {
            return m;
        }

        /// <summary>
        /// Returns the real portion of a complex number.
        /// </summary>
        /// <param name="m">Value</param>
        /// <returns>The double version of the complex number m</returns>
        public static double DBLE(Complex m) {
            return m.Real;
        }

        /// <summary>
        /// Cosine of the angle in radians.
        /// </summary>
        /// <param name="m">Angle</param>
        /// <returns>The cosine of the angle m</returns>
        public static double DCOS(ref double m) {
            return Math.Cos(m);
        }

        /// <summary>
        /// Hyperbolic cosine of the specified angle in radians.
        /// </summary>
        /// <param name="m">Angle</param>
        /// <returns>The hyperbolic cosine of the angle m</returns>
        public static double DCOSH(ref double m) {
            return Math.Cosh(m);
        }

        /// <summary>
        /// Returns the positive difference of arg1 and arg2, i.e. if arg1
        /// > arg2 it returns (arg1 - arg2), otherwise zero..
        /// </summary>
        /// <param name="arg1">Arg1</param>
        /// <param name="arg2">Arg2</param>
        /// <returns>The positive difference of arg1 and arg2</returns>
        public static double DDIM(ref double arg1, ref double arg2) {
            return (arg1 > arg2) ? (arg1 - arg2) : 0;
        }

        /// <summary>
        /// Returns the exponential, i.e. e to the power of the argument. This is the inverse of the natural logarithm.
        /// </summary>
        /// <param name="m">Value</param>
        /// <returns>The exponential of value m</returns>
        public static double DEXP(ref double m) {
            return Math.Exp(m);
        }

        /// <summary>
        /// Returns the positive difference of arg1 and arg2, i.e. if arg1
        /// > arg2 it returns (arg1 - arg2), otherwise zero..
        /// </summary>
        /// <param name="arg1">Arg1</param>
        /// <param name="arg2">Arg2</param>
        /// <returns>The positive difference of arg1 and arg2</returns>
        public static double DIM(ref float arg1, ref float arg2) {
            return (arg1 > arg2) ? (arg1 - arg2) : 0;
        }

        /// <summary>
        /// Returns the positive difference of arg1 and arg2, i.e. if arg1
        /// > arg2 it returns (arg1 - arg2), otherwise zero..
        /// </summary>
        /// <param name="arg1">Arg1</param>
        /// <param name="arg2">Arg2</param>
        /// <returns>The positive difference of arg1 and arg2</returns>
        public static double DIM(ref double arg1, ref double arg2) {
            return (arg1 > arg2) ? (arg1 - arg2) : 0;
        }

        /// <summary>
        /// Converts to integer by truncation.
        /// </summary>
        /// <param name="d">Value to truncate</param>
        /// <returns>The value m truncated to an integer</returns>
        public static double DINT(ref double d) {
            return (int)d;
        }

        /// <summary>
        /// Logarithm to base e (where e=2.718...)..
        /// </summary>
        /// <param name="v">Value</param>
        /// <returns>The logarithm of value v</returns>
        public static double DLOG(ref double v) {
            return Math.Log(v);
        }
        
        /// <summary>
        /// Logarithm to base 10.
        /// </summary>
        /// <param name="v">Value</param>
        /// <returns>The base 10 logarithm of value v</returns>
        public static double DLOG10(ref double v) {
            return Math.Log10(v);
        }
        
        /// <summary>
        /// Returns the largest of its arguments.
        /// </summary>
        /// <param name="args">Arguments.</param>
        /// <returns>The largest value in the list args</returns>
        public static double DMAX1(params object[] args) {
            return MAX(args);
        }
        
        /// <summary>
        /// Returns the smallest of its arguments.
        /// </summary>
        /// <param name="args">Arguments.</param>
        /// <returns>The smallest value in the list args</returns>
        public static double DMIN1(params object[] args) {
            return MIN(args);
        }

        /// <summary>
        /// DMOD intrinsic function.
        /// Generic double type version.
        /// </summary>
        /// <param name="m1">First value</param>
        /// <param name="m2">Second value</param>
        /// <returns>The remainder after m1 is divided by m2</returns>
        public static double DMOD(ref double m1, ref double m2) {
            return m1 % m2;
        }

        /// <summary>
        /// DNINT intrinsic function.
        /// Convert m to the nearest whole number.
        /// </summary>
        /// <param name="m">First value</param>
        /// <returns>The value m converted to the nearest whole number</returns>
        public static double DNINT(ref double m) {
            return m >= 0 ? (int)(m + 0.5) : (int)(m - 0.5);
        }

        /// <summary>
        /// Computes the double precision product of two real values.
        /// </summary>
        /// <param name="m1">Value 1</param>
        /// <param name="m2">Value 2</param>
        /// <returns>The product of values m1 and m2</returns>
        public static double DPROD(ref float m1, ref float m2) {
            return m1 * m2;
        }

        /// <summary>
        /// Computes the double precision product of two real values.
        /// </summary>
        /// <param name="m1">Value 1</param>
        /// <param name="m2">Value 2</param>
        /// <returns>The product of values m1 and m2</returns>
        public static double DPROD(ref double m1, ref double m2) {
            return m1 * m2;
        }

        /// <summary>
        /// Performs sign transfer: if arg2 is negative the result is −arg1,
        /// if arg2 is zero or positive the result is arg1..
        /// </summary>
        /// <param name="arg1">Arg1.</param>
        /// <param name="arg2">Arg2.</param>
        /// <returns>The absolute of arg1 if arg2 is zero or positive, otherwise the negative absolute of arg1</returns>
        public static double DSIGN(ref double arg1, ref double arg2) {
            return (arg2 < 0) ? -Math.Abs(arg1) : Math.Abs(arg1);
        }

        /// <summary>
        /// Sine of the angle in radians.
        /// </summary>
        /// <param name="d">Angle</param>
        /// <returns>The sine of the angle d</returns>
        public static double DSIN(ref double d) {
            return Math.Sin(d);
        }
        
        /// <summary>
        /// Returns the hyperbolic sine of the angle in radians.
        /// </summary>
        /// <param name="d">Angle</param>
        /// <returns>The hyperbolic sine of the angle d</returns>
        public static double DSINH(ref double d) {
            return Math.Sinh(d);
        }

        /// <summary>
        /// SQRT intrinsic function
        /// Returns the square root of the given value.
        /// </summary>
        /// <param name="value">Input value</param>
        /// <returns>The square root of the value</returns>
        public static double DSQRT(ref double value) {
            return Math.Sqrt(value);
        }
        
        /// <summary>
        /// Tangent of the angle in radians.
        /// </summary>
        /// <param name="d">Angle</param>
        /// <returns>The tangent of the angle d</returns>
        public static double DTAN(ref double d) {
            return Math.Tan(d);
        }

        /// <summary>
        /// Returns the hyperbolic tangent of the specified angle.
        /// </summary>
        /// <param name="d">Angle</param>
        /// <returns>The hyperbolic tangent of d</returns>
        public static double DTANH(ref double d) {
            return Math.Tanh(d);
        }

        /// <summary>
        /// Returns the exponential, i.e. e to the power of the argument.
        /// This is the inverse of the natural logarithm.
        /// </summary>
        /// <param name="m">Value</param>
        /// <returns>The exponential of the value m</returns>
        public static double EXP(ref float m) {
            return Math.Exp(m);
        }

        /// <summary>
        /// Returns the exponential, i.e. e to the power of the argument.
        /// This is the inverse of the natural logarithm.
        /// </summary>
        /// <param name="m">Value</param>
        /// <returns>The exponential of the value m</returns>
        public static double EXP(ref double m) {
            return Math.Exp(m);
        }

        /// <summary>
        /// Returns the exponential, i.e. e to the power of the argument.
        /// This is the inverse of the natural logarithm.
        /// </summary>
        /// <param name="m">Value</param>
        /// <returns>The exponential of the complex number value m</returns>
        public static Complex EXP(ref Complex m) {
            return Complex.Exp(m);
        }

        /// <summary>
        /// Comverts the specified integer to a float.
        /// </summary>
        /// <param name="m">Value</param>
        /// <returns>The value m as a floating point number</returns>
        public static float FLOAT(int m) {
            return m;
        }

        /// <summary>
        /// Returns the absolute of the given value.
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>The absolute of value</returns>
        public static int IABS(ref int value) {
            return Math.Abs(value);
        }

        /// <summary>
        /// Returns position of first character of the string in the local character code table.
        /// </summary>
        /// <param name="ch">Character</param>
        /// <returns>The ASCII value of the character ch</returns>
        public static int ICHAR(string ch) {
            if (ch == null) {
                throw new ArgumentNullException(nameof(ch));
            }
            return ch[0];
        }

        /// <summary>
        /// Returns position of first character of the string in the local character code table.
        /// </summary>
        /// <param name="ch">Character</param>
        /// <returns>The ASCII value of the character ch</returns>
        public static int ICHAR(FixedString ch) {
            if (ch == null) {
                throw new ArgumentNullException(nameof(ch));
            }
            return ch[0];
        }

        /// <summary>
        /// Converts to integer by truncation.
        /// </summary>
        /// <param name="d">Value to truncate</param>
        /// <returns>The value m truncated to an integer</returns>
        public static int IDINT(double d) {
            return (int)d;
        }

        /// <summary>
        /// Returns the positive difference of arg1 and arg2, i.e. if arg1
        /// > arg2 it returns (arg1 - arg2), otherwise zero..
        /// </summary>
        /// <param name="arg1">Arg1</param>
        /// <param name="arg2">Arg2</param>
        /// <returns>The positive difference of arg1 and arg2</returns>
        public static int IDIM(ref int arg1, ref int arg2) {
            return (arg1 > arg2) ? (arg1 - arg2) : 0;
        }

        /// <summary>
        /// IDNINT intrinsic function.
        /// Convert m to the nearest whole number.
        /// </summary>
        /// <param name="m">First value</param>
        /// <returns>The value m converted to the nearest whole number</returns>
        public static int IDNINT(ref double m) {
            return m >= 0 ? (int)(m + 0.5f) : (int)(m - 0.5f);
        }

        /// <summary>
        /// Returns the integer value of the given float.
        /// </summary>
        /// <param name="value">Character</param>
        /// <returns>The floating point value converted to an integer</returns>
        public static int IFIX(float value) {
            return (int)value;
        }

        /// <summary>
        /// Searches first string and returns position of second string within it, otherwise zero.
        /// The return position is 1 based so that a match in the first position of s1 returns 1
        /// and so on.
        /// </summary>
        /// <param name="s1">First string</param>
        /// <param name="s2">Second string</param>
        /// <returns>The 1 based offset of string s2 within s1, or 0 if not found</returns>
        public static int INDEX(ref FixedString s1, ref string s2) {
            if (s1 == null) {
                throw new ArgumentNullException(nameof(s1));
            }
            if (s2 == null) {
                throw new ArgumentNullException(nameof(s2));
            }
            if (s2.Length == 0) {
                return 0;
            }
            return s1.IndexOf(s2) + 1;
        }

        /// <summary>
        /// Searches first string and returns position of second string within it, otherwise zero.
        /// The return position is 1 based so that a match in the first position of s1 returns 1
        /// and so on.
        /// </summary>
        /// <param name="s1">First string</param>
        /// <param name="s2">Second string</param>
        /// <returns>The 1 based offset of string s2 within s1, or 0 if not found</returns>
        public static int INDEX(ref string s1, ref FixedString s2) {
            if (s1 == null) {
                throw new ArgumentNullException(nameof(s1));
            }
            if (s2 == null) {
                throw new ArgumentNullException(nameof(s2));
            }
            if (s2.Length == 0) {
                return 0;
            }
            return s1.IndexOf(s2.ToString(), StringComparison.Ordinal) + 1;
        }

        /// <summary>
        /// Searches first string and returns position of second string within it, otherwise zero.
        /// The return position is 1 based so that a match in the first position of s1 returns 1
        /// and so on.
        /// </summary>
        /// <param name="s1">First string</param>
        /// <param name="s2">Second string</param>
        /// <returns>The 1 based offset of string s2 within s1, or 0 if not found</returns>
        public static int INDEX(ref string s1, ref string s2) {
            if (s1 == null) {
                throw new ArgumentNullException(nameof(s1));
            }
            if (s2 == null) {
                throw new ArgumentNullException(nameof(s2));
            }
            if (s2.Length == 0) {
                return 0;
            }
            return s1.IndexOf(s2, StringComparison.Ordinal) + 1;
        }

        /// <summary>
        /// Searches first string and returns position of second string within it, otherwise zero..
        /// </summary>
        /// <param name="s1">First string</param>
        /// <param name="s2">Second string</param>
        /// <returns>The 1 based offset of string s2 within s1</returns>
        public static int INDEX(ref FixedString s1, ref FixedString s2) {
            if (s1 == null) {
                throw new ArgumentNullException(nameof(s1));
            }
            if (s2 == null) {
                throw new ArgumentNullException(nameof(s2));
            }
            if (s2.Length == 0) {
                return 0;
            }
            return s1.IndexOf(s2) + 1;
        }

        /// <summary>
        /// Searches first string starting from the given offset and returns position of second
        /// string within it, otherwise zero. The offset is 1 based so the offset at the first
        /// character of s1 is 1, and so on. The return position is 1 based so that a match in
        /// the first position of s1 returns 1 and so on.
        /// </summary>
        /// <param name="s1">First string</param>
        /// <param name="s2">Second string</param>
        /// <param name="offset">Offset within first string to search</param>
        /// <returns>The 1 based offset of string s2 within s1</returns>
        public static int INDEX(ref string s1, ref string s2, ref int offset) {
            if (s1 == null) {
                throw new ArgumentNullException(nameof(s1));
            }
            if (s2 == null) {
                throw new ArgumentNullException(nameof(s2));
            }
            if (s2 == "") {
                return 0;
            }
            if (offset < 0 || offset > s1.Length) {
                offset = 0;
            }
            return s1.IndexOf(s2, offset, StringComparison.Ordinal) + 1;
        }

        /// <summary>
        /// Converts to integer by truncation.
        /// </summary>
        /// <param name="d">Value to truncate</param>
        /// <returns>The value d truncated to an integer</returns>
        public static int INT(int d) {
            return d;
        }

        /// <summary>
        /// Converts to integer by truncation.
        /// </summary>
        /// <param name="d">Value to truncate</param>
        /// <returns>The value d truncated to an integer</returns>
        public static int INT(float d) {
            return (int)d;
        }

        /// <summary>
        /// Converts to integer by truncation.
        /// </summary>
        /// <param name="d">Value to truncate</param>
        /// <returns>The value d truncated to an integer</returns>
        public static int INT(double d) {
            return (int)d;
        }

        /// <summary>
        /// Converts to integer by truncation.
        /// </summary>
        /// <param name="d">Value to truncate</param>
        /// <returns>The value d truncated to an integer</returns>
        public static int INT(Complex d) {
            return (int)d.Real;
        }

        /// <summary>
        /// Performs sign transfer: if arg2 is negative the result is −arg1,
        /// if arg2 is zero or positive the result is arg1..
        /// </summary>
        /// <param name="arg1">Arg1.</param>
        /// <param name="arg2">Arg2.</param>
        /// <returns>The absolute of arg1 if arg2 is zero or positive, otherwise the negative absolute of arg1</returns>
        public static int ISIGN(ref int arg1, ref int arg2) {
            return (arg2 < 0) ? -Math.Abs(arg1) : Math.Abs(arg1);
        }

        /// <summary>
        /// Returns length of the character argument.
        /// </summary>
        /// <param name="s">Character argument</param>
        /// <returns>The length of string s</returns>
        public static int LEN(string s) {
            if (s == null) {
                throw new ArgumentNullException(nameof(s));
            }
            return s.Length;
        }

        /// <summary>
        /// Returns length of the character argument.
        /// </summary>
        /// <param name="s">Character argument</param>
        /// <returns>The length of string s</returns>
        public static int LEN(ref string s) {
            if (s == null) {
                throw new ArgumentNullException(nameof(s));
            }
            return s.Length;
        }

        /// <summary>
        /// Returns length of the character argument.
        /// </summary>
        /// <param name="s">Character argument</param>
        /// <returns>The length of string s</returns>
        public static int LEN(ref FixedString s) {
            if (s == null) {
                throw new ArgumentNullException(nameof(s));
            }
            return s.RealLength;
        }

        /// <summary>
        /// Lexical comparison using ASCII character code: returns true if arg1 > = arg2..
        /// </summary>
        /// <param name="s1">First string</param>
        /// <param name="s2">Second string</param>
        /// <returns>True if string s1 is lexically less than s2, false otherwise</returns>
        public static bool LGE(string s1, string s2) {
            return string.CompareOrdinal(s1, s2) >= 0;
        }

        /// <summary>
        /// Lexical comparison using ASCII character code: returns true if arg1 > = arg2..
        /// </summary>
        /// <param name="s1">First string</param>
        /// <param name="s2">Second string</param>
        /// <returns>True if string s1 is lexically less than s2, false otherwise</returns>
        public static bool LGE(FixedString s1, FixedString s2) {
            if (s1 == null) {
                throw new ArgumentNullException(nameof(s1));
            }
            if (s2 == null) {
                throw new ArgumentNullException(nameof(s2));
            }
            return s1.Compare(s2) >= 0;
        }

        /// <summary>
        /// Lexical comparison using ASCII character code: returns true if arg1 > arg2..
        /// </summary>
        /// <param name="s1">First string</param>
        /// <param name="s2">Second string</param>
        /// <returns>True if string s1 is lexically greater than s2, false otherwise</returns>
        public static bool LGT(string s1, string s2) {
            return string.CompareOrdinal(s1, s2) > 0;
        }

        /// <summary>
        /// Lexical comparison using ASCII character code: returns true if arg1 > arg2..
        /// </summary>
        /// <param name="s1">First string</param>
        /// <param name="s2">Second string</param>
        /// <returns>True if string s1 is lexically greater than s2, false otherwise</returns>
        public static bool LGT(FixedString s1, FixedString s2) {
            if (s1 == null) {
                throw new ArgumentNullException(nameof(s1));
            }
            if (s2 == null) {
                throw new ArgumentNullException(nameof(s2));
            }
            return s1.Compare(s2) > 0;
        }

        /// <summary>
        /// Lexical comparison using ASCII character code: returns true if arg1 is less
        /// than or equal to arg2.
        /// </summary>
        /// <param name="s1">First string</param>
        /// <param name="s2">Second string</param>
        /// <returns>True if string s1 is lexically less than or equal to s2, false otherwise</returns>
        public static bool LLE(string s1, string s2) {
            return string.CompareOrdinal(s1, s2) <= 0;
        }

        /// <summary>
        /// Lexical comparison using ASCII character code: returns true if arg1 is less
        /// than or equal to arg2.
        /// </summary>
        /// <param name="s1">First string</param>
        /// <param name="s2">Second string</param>
        /// <returns>True if string s1 is lexically less than or equal to s2, false otherwise</returns>
        public static bool LLE(FixedString s1, FixedString s2) {
            if (s1 == null) {
                throw new ArgumentNullException(nameof(s1));
            }
            if (s2 == null) {
                throw new ArgumentNullException(nameof(s2));
            }
            return s1.Compare(s2) <= 0;
        }

        /// <summary>
        /// Lexical comparison using ASCII character code: returns true if arg1 is less
        /// than arg2.
        /// </summary>
        /// <param name="s1">First string</param>
        /// <param name="s2">Second string</param>
        /// <returns>True if string s1 is lexically less than s2, false otherwise</returns>
        public static bool LLT(string s1, string s2) {
            return string.CompareOrdinal(s1, s2) < 0;
        }

        /// <summary>
        /// Lexical comparison using ASCII character code: returns true if arg1 is less
        /// than arg2.
        /// </summary>
        /// <param name="s1">First string</param>
        /// <param name="s2">Second string</param>
        /// <returns>True if string s1 is lexically less than s2, false otherwise</returns>
        public static bool LLT(FixedString s1, FixedString s2) {
            if (s1 == null) {
                throw new ArgumentNullException(nameof(s1));
            }
            if (s2 == null) {
                throw new ArgumentNullException(nameof(s2));
            }
            return s1.Compare(s2) < 0;
        }

        /// <summary>
        /// Logarithm to base e (where e=2.718...)..
        /// </summary>
        /// <param name="v">Value</param>
        /// <returns>The logarithm of the value v</returns>
        public static double LOG(double v) {
            return Math.Log(v);
        }

        /// <summary>
        /// Logarithm to base e (where e=2.718...)..
        /// </summary>
        /// <param name="v">Value</param>
        /// <returns>The logarithm of the value v</returns>
        public static double LOG(ref float v) {
            return Math.Log(v);
        }

        /// <summary>
        /// Logarithm to base e (where e=2.718...)..
        /// </summary>
        /// <param name="v">Value</param>
        /// <returns>The logarithm of the value v</returns>
        public static double LOG(ref double v) {
            return Math.Log(v);
        }

        /// <summary>
        /// Logarithm to base e (where e=2.718...)..
        /// </summary>
        /// <param name="v">Value</param>
        /// <returns>The logarithm of the complex number value v</returns>
        public static Complex LOG(ref Complex v) {
            return Complex.Log(v);
        }

        /// <summary>
        /// Logarithm to base 10.
        /// </summary>
        /// <param name="v">Value</param>
        /// <returns>The base 10 logarithm of the value v</returns>
        public static double LOG10(double v) {
            return Math.Log10(v);
        }

        /// <summary>
        /// Logarithm to base 10.
        /// </summary>
        /// <param name="v">Value</param>
        /// <returns>The base 10 logarithm of the value v</returns>
        public static double LOG10(ref float v) {
            return Math.Log10(v);
        }

        /// <summary>
        /// Logarithm to base 10.
        /// </summary>
        /// <param name="v">Value</param>
        /// <returns>The base 10 logarithm of the value v</returns>
        public static double LOG10(ref double v) {
            return Math.Log10(v);
        }
        
        /// <summary>
        /// Returns the largest of its arguments.
        /// </summary>
        /// <param name="args">Arguments.</param>
        /// <returns>The largest value in the list args</returns>
        public static int MAX0(params object[] args) {
            return (int)MAX(args);
        }
        
        /// <summary>
        /// Returns the largest of its arguments.
        /// </summary>
        /// <param name="args">Arguments.</param>
        /// <returns>The largest value in the list args</returns>
        public static int MAX1(params object[] args) {
            return (int)MAX(args);
        }

        /// <summary>
        /// Returns the largest of its arguments.
        /// </summary>
        /// <param name="args">Arguments.</param>
        /// <returns>The largest value in the list args</returns>
        public static double MAX(params object[] args) {
            if (args == null) {
                throw new ArgumentNullException(nameof(args));
            }
            if (args.Length == 0) {
                throw new ArgumentOutOfRangeException(nameof(args));
            }
            double max = Convert.ToDouble(args[0]);
            for (int c = 1; c < args.Length; ++c) {
                double arg = Convert.ToDouble(args[c]);
                if (arg > max) {
                    max = arg;
                }
            }
            return max;
        }
        
        /// <summary>
        /// Returns the smallest of its arguments.
        /// </summary>
        /// <param name="args">Arguments.</param>
        /// <returns>The smallest value in the list args</returns>
        public static int MIN1(params object[] args) {
            return (int)MIN(args);
        }
        
        /// <summary>
        /// Returns the smallest of its arguments.
        /// </summary>
        /// <param name="args">Arguments.</param>
        /// <returns>The smallest value in the list args</returns>
        public static int MIN0(params object[] args) {
            return (int)MIN(args);
        }

        /// <summary>
        /// Returns the smallest of its arguments.
        /// </summary>
        /// <param name="args">Arguments.</param>
        /// <returns>The smallest value in the list args</returns>
        public static double MIN(params object[] args) {
            if (args == null) {
                throw new ArgumentNullException(nameof(args));
            }
            if (args.Length == 0) {
                throw new ArgumentOutOfRangeException(nameof(args));
            }
            double min = Convert.ToDouble(args[0]);
            for (int c = 1; c < args.Length; ++c) {
                double arg = Convert.ToDouble(args[c]);
                if (arg < min) {
                    min = arg;
                }
            }
            return min;
        }

        /// <summary>
        /// MOD intrinsic function.
        /// </summary>
        /// <param name="m1">First value</param>
        /// <param name="m2">Second value</param>
        /// <returns>The remainder after m1 is divided by m2</returns>
        public static int MOD(ref int m1, ref int m2) {
            return m1 % m2;
        }
        
        /// <summary>
        /// MOD intrinsic function.
        /// </summary>
        /// <param name="m1">First value</param>
        /// <param name="m2">Second value</param>
        /// <returns>The remainder after m1 is divided by m2</returns>
        public static double MOD(ref double m1, ref double m2) {
            return m1 % m2;
        }

        /// <summary>
        /// NINT intrinsic function.
        /// Convert m to the nearest whole number.
        /// </summary>
        /// <param name="m">First value</param>
        /// <returns>The value m converted to the nearest whole number</returns>
        public static int NINT(ref float m) {
            return m >= 0 ? (int)(m + 0.5f) : (int)(m - 0.5f);
        }

        /// <summary>
        /// NINT intrinsic function.
        /// Convert m to the nearest whole number.
        /// </summary>
        /// <param name="m">First value</param>
        /// <returns>The value m converted to the nearest whole number</returns>
        public static int NINT(ref double m) {
            return m >= 0 ? (int)(m + 0.5f) : (int)(m - 0.5f);
        }

        /// <summary>
        /// RAD intrinsic function
        /// Converts degrees to radians.
        /// </summary>
        /// <param name="degrees">Value in degrees</param>
        /// <returns>Input value converted to radians</returns>
        public static float RAD(ref int degrees) {
            return Convert.ToSingle(degrees * (Math.PI/180.0));
        }

        /// <summary>
        /// RAD intrinsic function
        /// Converts degrees to radians.
        /// </summary>
        /// <param name="degrees">Value in degrees</param>
        /// <returns>Input value converted to radians</returns>
        public static float RAD(ref float degrees) {
            return Convert.ToSingle(degrees * (Math.PI / 180.0));
        }

        /// <summary>
        /// RAND intrinsic function
        /// Returns a random number.
        /// </summary>
        /// <param name="seed">Seed value</param>
        /// <returns>The next random number for the given seed</returns>
        public static double RAND(ref int seed) {
            _rand = new(seed);
            return _rand.NextDouble();
        }

        /// <summary>
        /// Converts to real.
        /// </summary>
        /// <param name="d">Value to convert</param>
        /// <returns>The floating point representation of value d</returns>
        public static float REAL(int d) {
            return d;
        }

        /// <summary>
        /// Converts to real.
        /// </summary>
        /// <param name="d">Value to convert</param>
        /// <returns>The floating point representation of value d</returns>
        public static float REAL(double d) {
            return (float)d;
        }

        /// <summary>
        /// Converts to real.
        /// </summary>
        /// <param name="d">Value to convert</param>
        /// <returns>The real portion of the complex number value d</returns>
        public static float REAL(Complex d) {
            return (float)d.Real;
        }

        /// <summary>
        /// Returns the rightmost characters of a string.
        /// </summary>
        /// <param name="s">Character argument</param>
        /// <param name="length">Length to return</param>
        /// <returns>Rightmost characters of string s</returns>
        public static string RIGHT(ref string s, ref int length) {
            if (s == null) {
                throw new ArgumentNullException(nameof(s));
            }
            if (length < 0 || length > s.Length) {
                length = s.Length;
            }
            return s.Substring(s.Length - length);
        }

        /// <summary>
        /// Performs sign transfer: if arg2 is negative the result is −arg1,
        /// if arg2 is zero or positive the result is arg1..
        /// </summary>
        /// <param name="arg1">Arg1.</param>
        /// <param name="arg2">Arg2.</param>
        /// <returns>The absolute of arg1 if arg2 is zero or positive, otherwise the negative absolute of arg1</returns>
        public static float SIGN(ref float arg1, ref float arg2) {
            return (arg2 < 0) ? -Math.Abs(arg1) : Math.Abs(arg1);
        }

        /// <summary>
        /// Performs sign transfer: if arg2 is negative the result is −arg1,
        /// if arg2 is zero or positive the result is arg1..
        /// </summary>
        /// <param name="arg1">Arg1.</param>
        /// <param name="arg2">Arg2.</param>
        /// <returns>The absolute of arg1 if arg2 is zero or positive, otherwise the negative absolute of arg1</returns>
        public static double SIGN(ref double arg1, ref double arg2) {
            return (arg2 < 0) ? -Math.Abs(arg1) : Math.Abs(arg1);
        }

        /// <summary>
        /// Performs sign transfer: if arg2 is negative the result is −arg1,
        /// if arg2 is zero or positive the result is arg1..
        /// </summary>
        /// <param name="arg1">Arg1.</param>
        /// <param name="arg2">Arg2.</param>
        /// <returns>The absolute of arg1 if arg2 is zero or positive, otherwise the negative absolute of arg1</returns>
        public static int SIGN(ref int arg1, ref int arg2) {
            return (arg2 < 0) ? -Math.Abs(arg1) : Math.Abs(arg1);
        }

        /// <summary>
        /// Sine of the angle in radians.
        /// </summary>
        /// <param name="d">Angle</param>
        /// <returns>The sine of the angle d</returns>
        public static float SIN(ref float d) {
            return (float)Math.Sin(d);
        }

        /// <summary>
        /// Sine of the angle in radians.
        /// </summary>
        /// <param name="d">Angle</param>
        /// <returns>The sine of the angle d</returns>
        public static double SIN(ref double d) {
            return Math.Sin(d);
        }

        /// <summary>
        /// Sine of the angle in radians.
        /// </summary>
        /// <param name="d">Angle</param>
        /// <returns>The sine of the angle d</returns>
        public static Complex SIN(ref Complex d) {
            return Complex.Sin(d);
        }

        /// <summary>
        /// Returns the hyperbolic sine of the angle in radians.
        /// </summary>
        /// <param name="d">Angle</param>
        /// <returns>The hyperbolic sine of the angle d</returns>
        public static double SINH(ref double d) {
            return Math.Sinh(d);
        }

        /// <summary>
        /// Returns the hyperbolic sine of the angle in radians.
        /// </summary>
        /// <param name="d">Angle</param>
        /// <returns>The hyperbolic sine of the angle d</returns>
        public static float SINH(ref float d) {
            return (float)Math.Sinh(d);
        }

        /// <summary>
        /// Returns size of the character argument.
        /// </summary>
        /// <param name="s">Character argument</param>
        /// <returns>The size of string s</returns>
        public static int SIZE(string s) {
            if (s == null) {
                throw new ArgumentNullException(nameof(s));
            }
            return s.Length;
        }

        /// <summary>
        /// Returns size of the character argument.
        /// </summary>
        /// <param name="s">Character argument</param>
        /// <returns>The size of string s</returns>
        public static int SIZE(ref string s) {
            if (s == null) {
                throw new ArgumentNullException(nameof(s));
            }
            return s.Length;
        }

        /// <summary>
        /// Returns size of the character argument.
        /// </summary>
        /// <param name="s">Character argument</param>
        /// <returns>The size of string s</returns>
        public static int SIZE(ref FixedString s) {
            if (s == null) {
                throw new ArgumentNullException(nameof(s));
            }
            return s.Length;
        }

        /// <summary>
        /// Returns the value of double d converted to a float.
        /// </summary>
        /// <param name="d">Double value</param>
        /// <returns>The value d converted to a floating point number</returns>
        public static float SNGL(double d) {
            return (float)d;
        }

        /// <summary>
        /// SQRT intrinsic function
        /// Returns the square root of the given value.
        /// </summary>
        /// <param name="value">Input value</param>
        /// <returns>The square root of the value</returns>
        public static float SQRT(ref float value) {
            return (float)Math.Sqrt(value);
        }

        /// <summary>
        /// SQRT intrinsic function
        /// Returns the square root of the given value.
        /// </summary>
        /// <param name="value">Input value</param>
        /// <returns>The square root of the value</returns>
        public static double SQRT(ref double value) {
            return Math.Sqrt(value);
        }

        /// <summary>
        /// SQRT intrinsic function
        /// Returns the square root of the given value.
        /// </summary>
        /// <param name="value">Input value</param>
        /// <returns>The square root of the complex number value</returns>
        public static Complex SQRT(ref Complex value) {
            return Complex.Sqrt(value);
        }

        /// <summary>
        /// Returns a substring of a string.
        /// </summary>
        /// <param name="s">Character argument</param>
        /// <param name="start">Start index</param>
        /// <param name="length">Length to return</param>
        /// <returns>Leftmost characters of string s</returns>
        public static string SUBSTRING(ref string s, ref int start, ref int length) {
            if (s == null) {
                throw new ArgumentNullException(nameof(s));
            }
            if (start < 0 || start >= s.Length) {
                return string.Empty;
            }
            if (length < 0 || start + length > s.Length) {
                length = s.Length - start;
            }
            return s.Substring(start, length);
        }

        /// <summary>
        /// Returns a given number of copies of a string.
        /// </summary>
        /// <param name="copies">Number of copies required</param>
        /// <param name="s">Character argument</param>
        /// <returns>Leftmost characters of string s</returns>
        public static string STRING(ref int copies, ref string s) {
            if (s == null) {
                throw new ArgumentNullException(nameof(s));
            }
            if (copies < 0) {
                throw new ArgumentException("Invalid count", nameof(s));
            }
            return string.Concat(Enumerable.Repeat(s, copies));
        }

        /// <summary>
        /// Tangent of the angle in radians.
        /// </summary>
        /// <param name="d">Angle</param>
        /// <returns>The tangent of d</returns>
        public static double TAN(ref float d) {
            return Math.Tan(d);
        }

        /// <summary>
        /// Tangent of the angle in radians.
        /// </summary>
        /// <param name="d">Angle</param>
        /// <returns>The tangent of d</returns>
        public static double TAN(ref double d) {
            return Math.Tan(d);
        }

        /// <summary>
        /// Returns the hyperbolic tangent of the specified angle.
        /// </summary>
        /// <param name="d">Angle</param>
        /// <returns>The hyperbolic tangent of d</returns>
        public static float TANH(ref float d) {
            return (float)Math.Tanh(d);
        }

        /// <summary>
        /// Returns the hyperbolic tangent of the specified angle.
        /// </summary>
        /// <param name="d">Angle</param>
        /// <returns>The hyperbolic tangent of d</returns>
        public static double TANH(ref double d) {
            return Math.Tanh(d);
        }
    }
}
