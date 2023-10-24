// FORTRAN Runtime Library
// FORTRAN format number class
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

using System.Numerics;
using System.Text;
using JComLib;

namespace JFortranLib;

/// <summary>
/// Internal class that simplifies number parsing.
/// </summary>
public sealed class FormatParser {

    private readonly string _buffer;
    private readonly bool _blankAsZero;
    private int _index;

    /// <summary>
    /// Initializes a new instance of the <see cref="FormatParser"/> class
    /// using the specified FormatRecord to control the number parsing.
    /// </summary>
    /// <param name="buffer">The string to parse</param>
    /// <param name="record">The FormatRecord to use</param>
    public FormatParser(string buffer, FormatRecord record) {
        _buffer = buffer;
        _blankAsZero = record != null && record.BlanksAsZero;

        // Skip initial whitespace in the buffer
        while (_index < _buffer.Length && char.IsWhiteSpace(_buffer[_index])) {
            ++_index;
        }
    }

    /// <summary>
    /// Returns the next character from the string or NUL if the end of the
    /// string was reached. Blanks are handled as per the blankAsZero flag. If
    /// the flag is set, '0' is returned for each blank. If the flag is clear,
    /// blanks are skipped.
    /// </summary>
    /// <returns>Next character or NUL</returns>
    public char Next() {
        char ch;
        while (true) {
            if (_index == _buffer.Length) {
                return '\0';
            }
            ch = _buffer[_index++];
            if (char.IsWhiteSpace(ch)) {
                if (_blankAsZero) {
                    return '0';
                }
                continue;
            }
            break;
        }
        return ch;
    }
}

/// <summary>
/// Defines a class that parses and outputs numbers using FORTRAN number
/// conventions and specifiers.
/// </summary>
public static class FormatNumber {

    private static readonly double[] powersOf10 = { 10, 100, 1.0e4, 1.0e8, 1.0e16, 1.0e32, 1.0e64, 1.0e128, 1.0e256 };

    /// <summary>
    /// Format a value represented by the object with default width and precision. The object must
    /// be a value or object type that can be represented as a string.
    /// </summary>
    /// <param name="value">An object containing a value</param>
    /// <returns>A string containing the value converted to a string</returns>
    public static string FormatValue(object value) {
        if (value is Complex) {
            return FormatComplex((Complex)value, new FormatRecord('F', 0, 1), new FormatRecord('F', 0, 1));
        }
        if (value is int) {
            return FormatInteger((int)value, new FormatRecord('I'));
        }
        if (value is bool) {
            return FormatBoolean((bool)value, new FormatRecord('L'));
        }
        if (value is float) {
            return FormatFloat((float)value, new FormatRecord('F'));
        }
        if (value is double) {
            return FormatDouble((double)value, new FormatRecord('D'));
        }
        return value.ToString();
    }

    /// <summary>
    /// Format the specified boolean value using FORTRAN L formatting rules.
    /// The width specifies the amount of space, in characters, into which
    /// the whole number must fit or otherwise the return value is a
    /// string consisting of '*'s. If the width is zero, the string is returned
    /// without any size constraint.
    /// </summary>
    /// <param name="value">A boolean value</param>
    /// <param name="record">A FormatRecord containing formatting settings</param>
    /// <returns>A string representation of the boolean value.</returns>
    public static string FormatBoolean(bool value, FormatRecord record) {
        if (record == null) {
            throw new ArgumentNullException(nameof(record));
        }
        string newString = value ? "T" : "F";
        return FormatToWidth(record.FieldWidth, newString);
    }

    /// <summary>
    /// Parses an integer from the given string. Leading blanks are ignored.
    /// Either '+' or '-' are accepted as sign characters and must appear after
    /// any leading blanks at the beginning of the string. The rest of the string
    /// must contain digits or blanks. Blanks are treated as either '0' or ignored
    /// depending on the BlanksAsZero flag in the record.
    /// </summary>
    /// <param name="intString">A string containing an integer number</param>
    /// <param name="record">A FormatRecord specifier</param>
    /// <returns>The integer value of the string</returns>
    public static int ParseInteger(string intString, FormatRecord record) {
        if (intString == null) {
            throw new ArgumentNullException(nameof(intString));
        }

        int intValue = 0;
        int sign = 1;

        FormatParser parser = new(intString, record);
        char ch = parser.Next();

        if (ch == '+') {
            ch = parser.Next();
        }
        else if (ch == '-') {
            sign = -1;
            ch = parser.Next();
        }
        while (ch != '\0') {
            if (!char.IsDigit(ch)) {
                throw new JComRuntimeException(JComRuntimeErrors.FORMAT_INVALID_NUMBER,
                    $"Illegal character {ch} in number");
            }
            intValue = intValue * 10 + (ch - '0');
            ch = parser.Next();
        }
        return intValue * sign;
    }

    /// <summary>
    /// Format the specified integer number using FORTRAN I formatting rules using
    /// an integer number. The width specifies the amount of space, in characters,
    /// into which the whole number must fit or otherwise the return value is a
    /// string consisting of '*'s. If the width is zero, the string is returned
    /// without any size constraint.
    ///
    /// The precision is the minimum number of leading zeroes that the number must
    /// contain. If this is zero, the number is not zero padded.
    /// </summary>
    /// <param name="value">An integer number</param>
    /// <param name="record">A FormatRecord containing formatting settings</param>
    /// <returns>A string representation of the integer number.</returns>
    public static string FormatInteger(int value, FormatRecord record) {
        if (record == null) {
            throw new ArgumentNullException(nameof(record));
        }

        StringBuilder str = new();
        int tempValue = Math.Abs(value);
        int fieldWidth = 0;

        do {
            char ch = (char)(tempValue % 10 + 48);
            str.Append(ch);
            tempValue /= 10;
            ++fieldWidth;
        } while (tempValue > 0);
        while (fieldWidth < record.Precision) {
            str.Append('0');
            ++fieldWidth;
        }
        if (value < 0) {
            str.Append('-');
        }
        else {
            if (record.PlusRequired == FormatOptionalPlus.Always) {
                str.Append('+');
            }
        }
        return FormatToWidth(record.FieldWidth, ReverseString(str.ToString()));
    }

    /// <summary>
    /// Parses a float from the given string. Leading blanks are ignored.
    /// Either '+' or '-' are accepted as sign characters and must appear after
    /// any leading blanks at the beginning of the string. The rest of the string
    /// must contain a fractional number of the format:
    ///
    /// [s][nnn].[fff][Emm]
    ///
    /// where [s] is the optional sign, [nnn] is the optional mantissa and [fff]
    /// is an optional fraction. If an exponent is specified, it must contain an
    /// exponentiation value.
    ///
    /// If no exponent is explicitly specified then any scale factor is applied to
    /// the number.
    ///
    /// Blanks are treated as either '0' or ignored depending on the BlanksAsZero
    /// flag in the record.
    /// </summary>
    /// <param name="floatString">A string containing a floating point number</param>
    /// <param name="record">A FormatRecord containing formatting settings</param>
    /// <returns>The floating point value of the string</returns>
    public static float ParseFloat(string floatString, FormatRecord record) {
        if (floatString == null) {
            throw new ArgumentNullException(nameof(floatString));
        }
        return (float)ParseDouble(floatString, record);
    }

    /// <summary>
    /// Format the specified floating point number using FORTRAN F formatting rules
    /// using a floating point number. The width specifies the amount of space, in
    /// characters, into which the whole number must fit or otherwise the return value
    /// is a string consisting of '*'s. If the width is zero, the string is returned
    /// without any size constraint.
    ///
    /// The precision is the number of units for the non-fractional part. The
    /// resulting number is truncated (and the exponent adjusted if appropriate) to fit
    /// into the given width. If the precision is zero, no truncation occurs and
    /// the number of digits in the non-fractional part is determined by the number.
    /// </summary>
    /// <param name="value">A floating point number</param>
    /// <param name="record">A FormatRecord containing formatting settings</param>
    /// <returns>A string representation of the floating point number.</returns>
    public static string FormatFloat(float value, FormatRecord record) {
        if (record == null) {
            throw new ArgumentNullException(nameof(record));
        }

        // Do exponential formatting if so requested
        if (record.FormatChar == 'E') {
            return FormatExponential(value, 'E', record);
        }

        // BUGBUG: Doesn't apply the FormatOptionalPlus flag.
        // BUGBUG: Doesn't apply the scaling factor

        string formatString;
        if (record.FieldWidth == 0 && record.Precision == 0) {
            formatString = "{0:G}";
        }
        else {
            formatString = "{0," + record.FieldWidth + ":F" + record.Precision + "}";
        }
        return FormatFloatToWidth(record, string.Format(formatString, value));
    }

    /// <summary>
    /// Parses a double from the given string. Leading blanks are ignored.
    /// Either '+' or '-' are accepted as sign characters and must appear after
    /// any leading blanks at the beginning of the string. The rest of the string
    /// must contain a fractional number of the format:
    ///
    /// [s][nnn].[fff][Dmm]
    ///
    /// where [s] is the optional sign, [nnn] is the optional mantissa and [fff]
    /// is an optional fraction. If an exponent is specified, it must contain an
    /// exponentiation value.
    ///
    /// If no exponent is explicitly specified then any scale factor is applied to
    /// the number.
    ///
    /// Blanks are treated as either '0' or ignored depending on the BlanksAsZero
    /// flag in the record.
    /// </summary>
    /// <param name="doubleString">A string containing a double precision number</param>
    /// <param name="record">A FormatRecord containing formatting settings</param>
    /// <returns>The double precision value of the string</returns>
    public static double ParseDouble(string doubleString, FormatRecord record) {
        if (doubleString == null) {
            throw new ArgumentNullException(nameof(doubleString));
        }

        bool inFraction = false;
        bool hasExponentPart = false;
        int exponent = 0;
        long mantissaPart = 0;
        int sign = 1;

        FormatParser parser = new(doubleString, record);
        char ch = parser.Next();

        if (ch == '+') {
            ch = parser.Next();
        }
        else if (ch == '-') {
            sign = -1;
            ch = parser.Next();
        }
        while (ch != '\0') {
            if (ch == '.') {
                if (inFraction) {
                    throw new JComRuntimeException(JComRuntimeErrors.FORMAT_INVALID_NUMBER);
                }
                inFraction = true;
            }
            else {
                if (!char.IsDigit(ch)) {
                    break;
                }
                mantissaPart = mantissaPart * 10 + (ch - '0');
                if (inFraction) {
                    --exponent;
                }
            }
            ch = parser.Next();
        }
        if ("EeDd".Contains(ch)) {
            ch = parser.Next();
            hasExponentPart = true;
        }
        if (hasExponentPart || ch == '+' || ch == '-') {
            int exponentSign = 1;
            int exponentPart = 0;

            if (ch == '+') {
                ch = parser.Next();
            }
            else if (ch == '-') {
                exponentSign = -1;
                ch = parser.Next();
            }
            if (ch == '\0') {
                throw new JComRuntimeException(JComRuntimeErrors.FORMAT_INVALID_NUMBER);
            }
            while (ch != '\0') {
                if (!char.IsDigit(ch)) {
                    throw new JComRuntimeException(JComRuntimeErrors.FORMAT_INVALID_NUMBER,
                        $"Illegal character {ch} in number");
                }
                exponentPart = exponentPart * 10 + (ch - '0');
                ch = parser.Next();
            }
            exponent += exponentSign * exponentPart;
            hasExponentPart = true;
        }
        else {
            if (ch != '\0') {
                throw new JComRuntimeException(JComRuntimeErrors.FORMAT_INVALID_NUMBER,
                    $"Illegal character {ch} in number");
            }
        }
        if (!hasExponentPart && record != null) {
            exponent -= record.ScaleFactor;
        }
        bool expSign = false;
        if (exponent < 0) {
            expSign = true;
            exponent = -exponent;
        }
        if (exponent > 511) {
            exponent = 511;
        }
        double doubleExponent = 1.0;
        for (int d = 0; exponent != 0; exponent >>= 1, ++d) {
            if ((exponent & 1) != 0) {
                doubleExponent *= powersOf10[d];
            }
        }
        double fraction = mantissaPart;
        if (expSign) {
            fraction /= doubleExponent;
        }
        else {
            fraction *= doubleExponent;
        }
        return fraction * sign;
    }

    /// <summary>
    /// Format the specified double precision number using FORTRAN F formatting rules
    /// using a double precision number. The width specifies the amount of space, in
    /// characters, into which the whole number must fit or otherwise the return value
    /// is a string consisting of '*'s. If the width is zero, the string is returned
    /// without any size constraint.
    ///
    /// The precisionWidth is the number of units for the non-fractional part. The
    /// resulting number is truncated (and the exponent adjusted if appropriate) to fit
    /// into the given width. If the precisionWidth is zero, no truncation occurs and
    /// the number of digits in the non-fractional part is determined by the number.
    /// </summary>
    /// <param name="value">A floating point number</param>
    /// <param name="record">A FormatRecord containing formatting settings</param>
    /// <returns>A string representation of the floating point number.</returns>
    public static string FormatDouble(double value, FormatRecord record) {
        if (record == null) {
            throw new ArgumentNullException(nameof(record));
        }

        // Do exponential formatting if so requested
        if (record.FormatChar == 'E') {
            return FormatExponential(value, 'E', record);
        }
        if (record.FormatChar == 'G') {
            if (value < 0.1 || value > Math.Pow(10, record.Precision)) {
                return FormatExponential(value, 'E', record);
            }
        }

        // BUGBUG: Doesn't apply the FormatOptionalPlus flag.
        // BUGBUG: Doesn't apply the scaling factor
        // BUGBUG: Exponent character should be 'D' for double.

        string formatString;
        if (record.FieldWidth == 0 && record.Precision == 0) {
            formatString = "{0:G}";
        }
        else {
            formatString = "{0," + record.FieldWidth + ":F" + record.Precision + "}";
        }
        return FormatFloatToWidth(record, string.Format(formatString, value));
    }

    /// <summary>
    /// Format the specified complex number using FORTRAN F formatting rules
    /// using a double precision number. The width specifies the amount of space, in
    /// characters, into which the whole number must fit or otherwise the return value
    /// is a string consisting of '*'s. If the width is zero, the string is returned
    /// without any size constraint.
    ///
    /// The precisionWidth is the number of units for the non-fractional part. The
    /// resulting number is truncated (and the exponent adjusted if appropriate) to fit
    /// into the given width. If the precisionWidth is zero, no truncation occurs and
    /// the number of digits in the non-fractional part is determined by the number.
    /// </summary>
    /// <param name="value">A complex number</param>
    /// <param name="recordReal">A FormatRecord containing formatting settings for the Real part</param>
    /// <param name="recordImg">A FormatRecord containing formatting settings for the Imaginary part</param>
    /// <returns>A string representation of the complex number.</returns>
    public static string FormatComplex(Complex value, FormatRecord recordReal, FormatRecord recordImg) {
        if (recordReal == null) {
            throw new ArgumentNullException(nameof(recordReal));
        }
        if (recordImg == null) {
            throw new ArgumentNullException(nameof(recordImg));
        }

        FormatRecord tempRecordReal = new(recordReal);
        FormatRecord tempRecordImg = new(recordImg);
        tempRecordReal.FieldWidth = 0;
        tempRecordImg.FieldWidth = 0;
        string realPart = recordReal.FormatChar switch {
            'D' => FormatDouble(value.Real, tempRecordReal),
            'F' => FormatFloat((float)value.Real, tempRecordReal),
            'E' => FormatExponential(value.Real, 'E', tempRecordReal),
            'G' => FormatExponential(value.Real, 'E', tempRecordReal),
            _ => throw new JComRuntimeException(JComRuntimeErrors.FORMAT_INVALID_FOR_COMPLEX,
                $"Invalid format specifier {recordReal.FormatChar} for COMPLEX")
        };
        string imgPart = recordImg.FormatChar switch {
            'D' => FormatDouble(value.Imaginary, tempRecordImg),
            'F' => FormatFloat((float)value.Imaginary, tempRecordImg),
            'E' => FormatExponential(value.Imaginary, 'E', tempRecordImg),
            'G' => FormatExponential(value.Imaginary, 'E', tempRecordImg),
            _ => throw new JComRuntimeException(JComRuntimeErrors.FORMAT_INVALID_FOR_COMPLEX,
                $"Invalid format specifier {recordImg.FormatChar} for COMPLEX")
        };
        if (recordReal.FieldWidth > 0 || recordImg.FieldWidth > 0) {
            return FormatToWidth(recordReal.FieldWidth + recordImg.FieldWidth + 2, $"{realPart}  {imgPart}");
        }

        return $"({realPart},{imgPart})";
    }

    /// <summary>
    /// Format the specified double using FORTRAN E and D formatting rules:
    ///
    /// [+] [0] . x1x2...xd exp where:
    /// • + signifies a plus or a minus (13.5.9)
    /// • x1,x2...xd are the d most significant digits of the value of the datum after rounding
    /// • exp is a decimal exponent.
    /// </summary>
    /// <param name="value">A double precision number number</param>
    /// <param name="exponentChar">The character to be used to indicate the exponent</param>
    /// <param name="record">A FormatRecord containing formatting settings</param>
    /// <returns>A string representation of the value formatted as an exponential number.</returns>
    public static string FormatExponential(double value, char exponentChar, FormatRecord record) {
        if (record == null) {
            throw new ArgumentNullException(nameof(record));
        }

        int precision = record.Precision;
        int leadingZeroes = 1;

        // Extract the exponent
        int exponent = GetExponent(value);
        double newValue = value * Math.Pow(10.0, -exponent);

        // BUGBUG: Doesn't apply the FormatOptionalPlus flag.

        while (newValue is >= 1.0 or < -1.0) {
            ++exponent;
            newValue = value * Math.Pow(10.0, -exponent);
        }

        if (record.ScaleFactor != 0) {
            newValue *= Math.Pow(10.0, record.ScaleFactor);
            exponent -= record.ScaleFactor;

            // Decimal normalization with scale factor
            if (record.ScaleFactor <= 0 && record.ScaleFactor > -record.ExponentWidth) {
                leadingZeroes = Math.Abs(record.ScaleFactor);
                precision = record.ExponentWidth - Math.Abs(record.ScaleFactor);
            }
            else if (record.ScaleFactor > 0 && record.ScaleFactor < record.ExponentWidth + 2) {
                precision = precision - record.ScaleFactor + 1;
            }
        }

        string mantissaFormat = new('0', leadingZeroes);
        string fractionalFormat = new('0', precision);
        string exponentPortion;

        if (record.ExponentWidth > 0) {
            string exponentFormat = new('0', record.ExponentWidth);
            exponentPortion = exponentChar + exponent.ToString("+" + exponentFormat + ";-" + exponentFormat);
        }
        else if (exponent <= 99) {
            exponentPortion = exponentChar + exponent.ToString("+00;-00");
        }
        else if (exponent < 999) {
            exponentPortion = exponent.ToString("+000;-000");
        }
        else {
            // TODO: Run-time failure here. Exponent too big for E/D format
            return string.Empty;
        }
        return FormatFloatToWidth(record, newValue.ToString(mantissaFormat + "." + fractionalFormat) + exponentPortion);
    }

    // Split a number into its component parts (sign, basic decimal value and exponent) so that it
    // can be reconstructed as per the required output format. Exponent is adjusted for FORTRAN
    // exponential format as opposed to scientific format.
    //
    // Credit to ja72 on StackOverflow.com:
    // (http://stackoverflow.com/questions/10145145/how-to-format-double-values-to-e-format-with-specified-exponent)
    //
    private static int GetExponent(double value) {
        int exponent;
        value = Math.Abs(value);
        if (value > 0.0) {
            if (value > 1.0) {
                exponent = (int)(Math.Floor(Math.Log10(value) / 3.0) * 3.0);
            }
            else {
                exponent = (int)(Math.Ceiling(Math.Log10(value) / 3.0) * 3.0);
            }
        }
        else {
            exponent = 0;
        }
        return exponent;
    }

    // Do post-floating point number formatting
    private static string FormatFloatToWidth(FormatRecord record, string newString) {
        if (record.PlusRequired == FormatOptionalPlus.Always) {
            newString = "+" + newString;
        }
        if (record.FieldWidth > 0) {
            int length = newString.Length;

            if (newString.StartsWith("0.") && length == record.FieldWidth + 1) {
                newString = newString[1..];
            }
            if (newString.StartsWith("-0.") && length == record.FieldWidth + 1) {
                newString = "-." + newString[3..];
            }
        }
        return FormatToWidth(record.FieldWidth, newString);
    }

    // Format the argument and verify it fits within the given width. If the size
    // exceeds the width then a string of asterisks is returned instead.
    private static string FormatToWidth(int width, string stringToFit) {
        return width == 0 ? stringToFit :
            stringToFit.Length <= width ? stringToFit.PadLeft(width) :
            new string('*', width);
    }

    // Reverse a string.
    private static string ReverseString(string stringToReverse) {
        char[] stringArray = stringToReverse.ToCharArray();
        Array.Reverse(stringArray);
        return new string(stringArray);
    }
}