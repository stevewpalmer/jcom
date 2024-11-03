// JCom Runtime Library
// Helper functions
//
// Authors:
//  Steven Palmer
//
// Copyright (C) 2021 Steven Palmer
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

using System.ComponentModel;
using System.Reflection;

namespace JComLib;

public static class Utilities {

    /// <summary>
    /// Add the specified extension to the filename is no extension was
    /// already supplied.
    /// </summary>
    /// <param name="filename">Filename</param>
    /// <param name="extension">Extension to supply</param>
    /// <returns>Filename with an extension</returns>
    public static string AddExtensionIfMissing(string filename, string extension) {
        if (string.IsNullOrEmpty(Path.GetExtension(filename))) {
            return Path.ChangeExtension(filename, extension);
        }
        return filename;
    }

    /// <summary>
    /// Helper function that returns a description on an enum. If the enum has no
    /// explicit description attribute then the enum name is returned.
    /// </summary>
    /// <param name="value">Enum for which description is to be returned</param>
    /// <returns>The enum description or name</returns>
    public static string GetEnumDescription(Enum value) {

        string valueName = value.ToString();
        FieldInfo field = value.GetType().GetField(valueName);
        if (field != null && field.GetCustomAttributes(typeof(DescriptionAttribute), false) is DescriptionAttribute[] attributes) {
            if (attributes.Any()) {
                valueName = attributes.First().Description;
            }
        }
        return valueName;
    }

    /// <summary>
    /// Return a section of the specified string starting from the given index
    /// and for the specified length. If the index and length are outside the
    /// string range, we return an empty string.
    /// </summary>
    /// <param name="str">String to span</param>
    /// <param name="index">Zero based start index</param>
    /// <param name="length">Length to return</param>
    /// <returns>The substring indexed and of the given length or an empty string</returns>
    public static string SpanBound(string str, int index, int length) {
        if (index >= str.Length) {
            index = 0;
            length = 0;
        }
        length = Math.Min(length, str.Length - index);
        return str.Substring(index, length);
    }

    /// <summary>
    /// Constrain a value between the minimum and maximum points, wrapping the
    /// value around to the maximum if it is less than the minimum, and to the
    /// minimum if it exceeds the maximum.
    /// </summary>
    /// <param name="value">Value to constrain</param>
    /// <param name="minimum">Smallest extent</param>
    /// <param name="maximum">Largest extent</param>
    /// <returns>Constrained value</returns>
    public static int ConstrainAndWrap(int value, int minimum, int maximum) {
        if (value < minimum) {
            value = maximum - 1;
        }
        if (value >= maximum) {
            value = minimum;
        }
        return value;
    }

    /// <summary>
    /// Centre a string within a given width
    /// </summary>
    /// <param name="str">The string to centre</param>
    /// <param name="length">The width in which to centre</param>
    /// <returns>String centred within the given width</returns>
    public static string CentreString(string str, int length) {
        int padding = Math.Max(0, ((length - str.Length) / 2) + str.Length);
        return str.PadLeft(padding, ' ').PadRight(length, ' ');
    }
}