// FORTRAN Runtime Library
// FORTRAN format record class
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

namespace JFortranLib;

/// <summary>
/// Flags that specify the optional plus sign requirement.
/// </summary>
public enum FormatOptionalPlus {
    Default,
    Always,
    Never
}

/// <summary>
/// Defines a class that specifies a single FORMAT record.
/// </summary>
public class FormatRecord {

    private string _rawString;

    /// <summary>
    /// Construct an empty FormatRecord instance.
    /// </summary>
    public FormatRecord() {
        PlusRequired = FormatOptionalPlus.Default;
    }

    /// <summary>
    /// Construct an FormatRecord instance containing the specified
    /// format character.
    /// </summary>
    /// <param name="ch">Format character</param>
    public FormatRecord(char ch) {
        FormatChar = ch;
        PlusRequired = FormatOptionalPlus.Default;
    }

    /// <summary>
    /// Construct a FormatRecord from another instance.
    /// </summary>
    /// <param name="record">The FormatRecord to copy</param>
    public FormatRecord(FormatRecord record) {
        if (record == null) {
            throw new ArgumentNullException(nameof(record));
        }
        FieldWidth = record.FieldWidth;
        FormatChar = record.FormatChar;
        Count = record.Count;
        Precision = record.Precision;
        ExponentWidth = record.ExponentWidth;
        PlusRequired = record.PlusRequired;
        RawString = record.RawString;
        BlanksAsZero = record.BlanksAsZero;
        Relative = record.Relative;
        LeftJustify = record.LeftJustify;
        SuppressCarriage = record.SuppressCarriage;
    }

    /// <summary>
    /// Construct a FormatRecord instance with the given
    /// width and precision.
    /// </summary>
    /// <param name="ch">Format character</param>
    /// <param name="width">Integer field width</param>
    /// <param name="precision">Integer precision</param>
    public FormatRecord(char ch, int width, int precision) {
        FormatChar = ch;
        FieldWidth = width;
        Precision = precision;
        ExponentWidth = 2;
        LeftJustify = ch != 'B';
        SuppressCarriage = false;
        PlusRequired = FormatOptionalPlus.Default;
    }

    /// <summary>
    /// Gets or sets the format char.
    /// </summary>
    /// <value>The format char.</value>
    public char FormatChar { get; set; }

    /// <summary>
    /// Gets or sets whether the carriage return is suppressed
    /// </summary>
    public bool SuppressCarriage { get; set; }

    /// <summary>
    /// Gets or sets the number of times this specifier is used.
    /// </summary>
    /// <value>The repeat count, at least 1</value>
    public int Count { get; set; }

    /// <summary>
    /// Gets or sets the width of the field.
    /// </summary>
    /// <value>The width of the field.</value>
    public int FieldWidth { get; set; }

    /// <summary>
    /// Gets or sets the precision.
    /// </summary>
    /// <value>The precision.</value>
    public int Precision { get; set; }

    /// <summary>
    /// Gets or sets the width of the exponent for E and D specifiers.
    /// </summary>
    /// <value>The precision.</value>
    public int ExponentWidth { get; set; }

    /// <summary>
    /// Gets or sets the scale factor of the output.
    /// </summary>
    /// <value>The precision.</value>
    public int ScaleFactor { get; set; }

    /// <summary>
    /// Gets or sets a flag that indicates whether blank characters in
    /// the input should be treated as zeroes.
    /// </summary>
    /// <value>The precision.</value>
    public bool BlanksAsZero { get; set; }

    /// <summary>
    /// Gets or sets a flag that indicates whether the output for this
    /// field should be right or left justified.
    /// </summary>
    public bool LeftJustify { get; set; }

    /// <summary>
    /// Gets or sets the flag that specifies whether the '+' symbol in
    /// numeric output is required or suppressed.
    /// </summary>
    /// <value>The <c>FormatOptionalPlus</c> setting for the plus sign</value>
    public FormatOptionalPlus PlusRequired { get; set; }

    /// <summary>
    /// Specifies whether or not this is a relative or absolute
    /// cursor movement for TL, TR T and X positioning statements..
    /// </summary>
    /// <value><c>true</c> if relative; otherwise, <c>false</c>.</value>
    public bool Relative { get; set; }

    /// <summary>
    /// Gets a value indicating whether this instance holds raw string.
    /// </summary>
    /// <value><c>true</c> if this instance holds raw string; otherwise, <c>false</c>.</value>
    public bool IsRawString => FormatChar == '$';

    /// <summary>
    /// Sets or returns the raw string value parsed from the
    /// specifier.
    /// </summary>
    /// <value>The raw string.</value>
    public string RawString {
        get => _rawString;
        set {
            _rawString = value;
            FormatChar = '$';
        }
    }

    /// <summary>
    /// Returns whether this is a positional record.
    /// </summary>
    /// <value><c>true</c> if this instance is positional; otherwise, <c>false</c>.</value>
    public bool IsPositional => FormatChar == 'T';

    /// <summary>
    /// Gets or sets a value indicating whether this is an end record.
    /// </summary>
    /// <value><c>true</c> if this is an end record; otherwise, <c>false</c>.</value>
    public bool IsEndRecord { get; set; }
}