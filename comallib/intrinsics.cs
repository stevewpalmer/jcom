// COMAL Runtime Library
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

using JComLib;

namespace JComalLib; 

/// <summary>
/// Fortran external intrinsics.
/// </summary>
public static partial class Intrinsics {

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
    public static int ABS(int value) {
        return Math.Abs(value);
    }

    /// <summary>
    /// Returns the absolute of the given floating point value.
    /// </summary>
    /// <param name="value">Value</param>
    /// <returns>The absolute value</returns>
    public static float ABS(float value) {
        return Math.Abs(value);
    }

    /// <summary>
    /// Returns the angle whose tangent is the specified value.
    /// </summary>
    /// <param name="m1">Value</param>
    /// <returns>The angle whose tangent is m1</returns>
    public static double ATAN(double m1) {
        return Math.Atan(m1);
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
    /// Cosine of the angle in radians.
    /// </summary>
    /// <param name="d">Angle</param>
    /// <returns>The cosine of the angle d</returns>
    public static double COS(double d) {
        return Math.Cos(d);
    }

    /// <summary>
    /// Returns the exponential, i.e. e to the power of the argument.
    /// This is the inverse of the natural logarithm.
    /// </summary>
    /// <param name="m">Value</param>
    /// <returns>The exponential of the value m</returns>
    public static double EXP(double m) {
        return Math.Exp(m);
    }

    /// <summary>
    /// Rounds the specified value down to the smallest integral number
    /// </summary>
    /// <param name="m">Value</param>
    /// <returns>The value m rounded down</returns>
    public static double FLOOR(double m) {
        return Math.Floor(m);
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
    /// Performs an integer division of the form FLOOR(x/y) where x/y are
    /// computed as floating point values.
    /// </summary>
    /// <param name="left">Left operand</param>
    /// <param name="right">Right operand</param>
    /// <returns>Integer division result</returns>
    public static int IDIV(float left, float right) {
        return (int)Math.Floor(left / right);
    }

    /// <summary>
    /// Performs an integer division of the form FLOOR(x/y) where x/y are
    /// computed as floating point values.
    /// </summary>
    /// <param name="left">Left operand</param>
    /// <param name="right">Right operand</param>
    /// <returns>Integer division result</returns>
    public static int IDIV(int left, int right) {
        return (int)Math.Floor((float)left / right);
    }

    /// <summary>
    /// Performs an integer modulus of the form FLOOR(x/y) where x/y are
    /// computed as floating point values.
    /// </summary>
    /// <param name="left">Left operand</param>
    /// <param name="right">Right operand</param>
    /// <returns>Integer division result</returns>
    public static int IMOD(float left, float right) {
        return (int)(left - Math.Floor(left / right) * right);
    }

    /// <summary>
    /// Performs an integer modulus of the form FLOOR(x/y) where x/y are
    /// computed as floating point values.
    /// </summary>
    /// <param name="left">Left operand</param>
    /// <param name="right">Right operand</param>
    /// <returns>Integer division result</returns>
    public static int IMOD(int left, int right) {
        return (int)(left - Math.Floor((float)left / right) * right);
    }

    /// <summary>
    /// Searches first string and returns position of second string within it, otherwise zero.
    /// The return position is 1 based so that a match in the first position of s1 returns 1
    /// and so on.
    /// </summary>
    /// <param name="s1">First string</param>
    /// <param name="s2">Second string</param>
    /// <returns>The 1 based offset of string s2 within s1, or 0 if not found</returns>
    public static int INDEX(FixedString s1, FixedString s2) {
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
    public static int INDEX(string s1, string s2) {
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
    /// Logarithm to base 10.
    /// </summary>
    /// <param name="v">Value</param>
    /// <returns>The base 10 logarithm of the value v</returns>
    public static double LOG10(double v) {
        return Math.Log10(v);
    }

    /// <summary>
    /// RAND intrinsic function
    /// Returns a random number.
    /// </summary>
    /// <returns>The next random number for the given seed</returns>
    public static void RANDOMIZE() {
        _rand = new Random();
    }

    /// <summary>
    /// RAND intrinsic function
    /// Returns a random number.
    /// </summary>
    /// <param name="seed">Seed value</param>
    /// <returns>The next random number for the given seed</returns>
    public static void RANDOMIZE(int seed) {
        _rand = new Random(seed);
    }

    /// <summary>
    /// Emit inline code for REPORT.
    /// </summary>
    /// <param name="value">JComRuntimeErrors object</param>
    public static void REPORT(ref int value) {
        throw new JComRuntimeException((JComRuntimeErrors)value);
    }

    /// <summary>
    /// RND intrinsic function
    /// Returns a random number between 0 and 1.
    /// </summary>
    /// <returns>The next random number between 0 and 1</returns>
    public static float RND() {
        return (float)_rand.NextDouble();
    }

    /// <summary>
    /// RND intrinsic function
    /// Returns a random number in the two given ranges.
    /// </summary>
    /// <param name="start">Lower range of random number</param>
    /// <param name="end">Upper range of random number</param>
    /// <returns>The next random number in the given range</returns>
    public static float RND(int start, int end) {
        return Convert.ToSingle(_rand.Next(start, end));
    }

    /// <summary>
    /// Sine of the angle in radians.
    /// </summary>
    /// <param name="d">Angle</param>
    /// <returns>The sine of the angle d</returns>
    public static double SIN(double d) {
        return Math.Sin(d);
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
    /// Returns a string of the given number of spaces.
    /// </summary>
    /// <param name="count">Number of spaces required</param>
    /// <returns>A string with the given number of spaces</returns>
    public static string SPC(int count) {
        if (count < 0) {
            count = 0;
        }
        return new string(' ', count);
    }

    /// <summary>
    /// SQRT intrinsic function
    /// Returns the square root of the given value.
    /// </summary>
    /// <param name="value">Input value</param>
    /// <returns>The square root of the value</returns>
    public static double SQRT(double value) {
        return Math.Sqrt(value);
    }

    /// <summary>
    /// Tangent of the angle in radians.
    /// </summary>
    /// <param name="d">Angle</param>
    /// <returns>The tangent of d</returns>
    public static double TAN(double d) {
        return Math.Tan(d);
    }

    /// <summary>
    /// Set or retrieve the current value of TIME
    /// </summary>
    public static int TIME {
        get {
            DateTime now = DateTime.Now;
            return ((now.Hour * 60 + now.Minute) * 60 + now.Second) * 100;
        }
    }
}
