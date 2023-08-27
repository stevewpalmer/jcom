// JCom Runtime Library
// Fixed string class
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

using System.Runtime.CompilerServices;

namespace JComLib; 

/// <summary>
/// Fixed string management class
/// </summary>
public class FixedString : IEquatable<FixedString> {
    private readonly char [] _fixedString;

    /// <summary>
    /// Returns the length of this string.
    /// </summary>
    /// <returns>The length of the fixed string.</returns>
    public int RealLength { get; private set; }

    /// <summary>
    /// Returns the size of this allocated string which is the maximum
    /// number of characters the string can store.
    /// </summary>
    /// <returns>The size of the fixed string.</returns>
    public int Length { get; }

    /// <summary>
    /// Initialises a new fixed string instance with the given length.
    /// </summary>
    /// <param name="length">Required length of the string</param>
    public FixedString(int length) {
        if (length < 0) {
            throw new IndexOutOfRangeException(nameof(length));
        }
        _fixedString = new char[length];
        Length = length;
        RealLength = 0;
        Empty();
    }

    /// <summary>
    /// Initialises a new fixed string instance from an existing string. The
    /// length of the fixed string is set to that of the existing string.
    /// </summary>
    /// <param name="existingString">An existing string to initialise from</param>
    public FixedString(string existingString) {
        if (existingString == null) {
            throw new ArgumentNullException(nameof(existingString));
        }
        int length = existingString.Length;
        _fixedString = new char[length];
        Length = length;
        RealLength = length;
        Set(existingString);
    }

    /// <summary>
    /// Initialises a new fixed string instance from an existing string. The
    /// length of the fixed string is set to that of the existing string.
    /// </summary>
    /// <param name="existingString">An existing string to initialise from</param>
    public FixedString(FixedString existingString) {
        if (existingString == null) {
            throw new ArgumentNullException(nameof(existingString));
        }
        int length = existingString.Length;
        _fixedString = new char[length];
        Length = length;
        RealLength = length;
        Set(existingString);
    }

    /// <summary>
    /// Initialises a new fixed string instance from character. The
    /// length of the fixed string is that of a single character.
    /// </summary>
    /// <param name="ch">The character</param>
    public FixedString(char ch) {
        _fixedString = new char[1];
        Length = 1;
        RealLength = 1;
        _fixedString[0] = ch;
    }

    /// <summary>
    /// Initialise this fixed string to empty.
    /// </summary>
    public void Empty() {
        for (int c = 0; c < Length; ++c) {
            _fixedString[c] = ' ';
        }
    }

    /// <summary>
    /// Return whether the string is empty.
    /// </summary>
    public bool IsEmpty => RealLength == 0;

    /// <summary>
    /// Performs an implicit conversion between a native string and a
    /// fixed string.
    /// </summary>
    /// <param name="rhs">The source native string</param>
    /// <returns>Returns new FixedString object.</returns>
    public static implicit operator FixedString(string rhs) { 
        return new FixedString(rhs);            
    }

    /// <summary>
    /// Return this fixed string as a character array. The size of the array is the
    /// length of the fixed string.
    /// </summary>
    /// <returns>A character array containing the fixed string characters.</returns>
    public char [] ToCharArray() {
        return _fixedString;
    }

    /// <summary>
    /// Return the character at the specified index. Throws an ArgumentOutOfRange
    /// exception if the index is less than zero or outside the fixed string length.
    /// </summary>
    /// <param name="index">Index of the character to return</param>
    [IndexerName ("Chars")]
    public char this[int index] {
        get {
            if (index < 0 || index >= Length) {
                throw new IndexOutOfRangeException("index");
            }
            return _fixedString[index];
        }
        set {
            if (index < 0 || index >= Length) {
                throw new IndexOutOfRangeException("index");
            }
            _fixedString[index] = value;
        }
    }

    /// <summary>
    /// Return a substring of the fixed string.
    /// </summary>
    /// <param name="index">1 based index at which the substring starts</param>
    /// <returns>A native string containing the requested substring of the fixed string</returns>
    public FixedString Substring(int start) {
        if (start < 1 || start > Length) {
            throw new IndexOutOfRangeException("index");
        }
        return Substring(start, Length);
    }

    /// <summary>
    /// Return a substring of the fixed string.
    /// </summary>
    /// <param name="start">1 based index at which the substring starts</param>
    /// <param name="end">The 1 based index at which the substring ends</param>
    /// <returns>A native string containing the requested substring of the fixed string</returns>
    public FixedString Substring(int start, int end) {
        if (start < 1 || start > Length) {
            throw new IndexOutOfRangeException(nameof(start));
        }
        if (end < 1 || end > Length || end < start) {
            throw new IndexOutOfRangeException(nameof(end));
        }
        int length = end - start + 1;
        FixedString newString = new(length);
        int index2 = 0;
        start -= 1;
        while (start < Length && index2 < length) {
            newString[index2++] = _fixedString[start++];
        }
        while (index2 < length) {
            newString[index2++] = ' ';
        }
        newString.RealLength = length;
        return newString;
    }
    
    /// <summary>
    /// Assign a value to this fixed string using the given string as input.
    /// </summary>
    /// <param name="newString">The new native string to assign to this one</param>
    public void Set(string newString) {
        if (newString == null) {
            throw new ArgumentNullException(nameof(newString));
        }
        InternalSet(newString.ToCharArray());
    }

    /// <summary>
    /// Assign a value to this fixed string using the given fixed string as input.
    /// </summary>
    /// <param name="newString">The new fixed string to assign to this one</param>
    public void Set(FixedString newString) {
        if (newString == null) {
            throw new ArgumentNullException(nameof(newString));
        }
        InternalSet(newString.ToCharArray());
    }

    /// <summary>
    /// Copies the specifies string into this string at the given range. The start
    /// index must be 0 based and must not be larger than the end index. If the end
    /// index is beyond the string length then it is constrained to the string
    /// length.
    /// </summary>
    /// <param name="newString">The new fixed string to copy to this one</param>
    /// <param name="start">The 1 based index of the start of the range</param>
    /// <param name="end">The 1 based index of the end of the range</param>
    public void Set(FixedString newString, int start, int end) {
        if (newString == null) {
            throw new ArgumentNullException(nameof(newString));
        }
        if (start < 1 || end < start) {
            throw new IndexOutOfRangeException(nameof(start));
        }
        if (end < 1) {
            throw new IndexOutOfRangeException(nameof(end));
        }
        InternalSet(newString.ToCharArray(), start - 1, end - 1);
    }

    /// <summary>
    /// Copies the specifies string into this string at the given range. The start
    /// index must be 0 based and must not be larger than the end index. If the end
    /// index is beyond the string length then it is constrained to the string
    /// length.
    /// </summary>
    /// <param name="newString">The new fixed string to copy to this one</param>
    /// <param name="start">The 1 based index of the start of the range</param>
    /// <param name="end">The 1 based index of the end of the range</param>
    public void Set(string newString, int start, int end) {
        if (newString == null) {
            throw new ArgumentNullException(nameof(newString));
        }
        if (start < 1 || end < start) {
            throw new IndexOutOfRangeException(nameof(start));
        }
        if (end < 1) {
            throw new IndexOutOfRangeException(nameof(end));
        }
        InternalSet(newString.ToCharArray(), start - 1, end - 1);
    }
    
    /// <summary>
    /// Compare two fixed strings.
    /// </summary>
    /// <param name="s1">First string</param>
    /// <param name="s2">Second string</param>
    /// <returns>The return value is 0 if the two strings are equivalent, a negative value if
    /// the first string is lexically less than the second, or a positive value if the second
    /// string is lexically greater than the first.</returns>
    public static int Compare(FixedString s1, FixedString s2) {
        if (Equals(s1, null)) {
            throw new ArgumentNullException(nameof(s1));
        }
        if (Equals(s2, null)) {
            return 1;
        }
        if (s1.RealLength == s2.RealLength && ReferenceEquals(s1, s2)) {
            return 0;
        }
        int length = Math.Max(s1.RealLength, s2.RealLength);
        for (int c = 0; c < length; ++c) {
            char ch1 = (c < s1.Length) ? s1[c]: ' ';
            char ch2 = (c < s2.Length) ? s2[c]: ' ';
            if (ch1 != ch2) {
                return ch1 - ch2;
            }
        }
        return 0;
    }

    /// <summary>
    /// Compare this fixed string with the given string.
    /// </summary>
    /// <param name="stringToCompare">String to compare.</param>
    /// <returns>The return value is 0 if the two strings are equivalent, a negative value if
    /// the first string is lexically less than the second, or a positive value if the second
    /// string is lexically greater than the first.</returns>
    public int Compare(FixedString stringToCompare) {
        return Compare(this, stringToCompare);
    }

    /// <summary>
    /// Return the index of stringToFind in this string.
    /// </summary>
    /// <param name="stringToFind">String to find.</param>
    /// <returns>The zero based offset of the string, or -1 if the string is not found.</returns>
    public int IndexOf(FixedString stringToFind) {
        if (Equals(stringToFind, null)) {
            throw new ArgumentNullException(nameof(stringToFind));
        }
        int index1 = 0;
        int index2 = 0;
        int startIndex = 0;
        while (index1 < Length - stringToFind.Length) {
            if (_fixedString[index1] == stringToFind[index2]) {
                ++index1;
                ++index2;
            } else {
                index1 = ++startIndex;
                index2 = 0;
            }
            if (index2 == stringToFind.Length) {
                return startIndex;
            }
        }
        return -1;
    }

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is equal to the current <see cref="FixedString"/>.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to compare with the current <see cref="FixedString"/>.</param>
    /// <returns><c>true</c> if the specified <see cref="object"/> is equal to the current
    /// <see cref="FixedString"/>; otherwise, <c>false</c>.</returns>
    public override bool Equals(object obj) {
        if (!(obj is FixedString)) {
            return false;
        }
        FixedString otherString = (FixedString)obj;
        return Compare(otherString) == 0;
    }

    /// <summary>
    /// Determines whether the specified <see cref="FixedString"/> is equal to the current <see cref="FixedString"/>.
    /// </summary>
    /// <param name="otherString">The <see cref="FixedString"/> to compare with the current <see cref="FixedString"/>.</param>
    /// <returns><c>true</c> if the specified <see cref="FixedString"/> is equal to the current
    /// <see cref="FixedString"/>; otherwise, <c>false</c>.</returns>
    public bool Equals(FixedString otherString) {
        return Compare(otherString) == 0;
    }

    /// <summary>
    /// Implements the equality operator between two fixed strings.
    /// </summary>            
    /// <param name="s1">First string</param>
    /// <param name="s2">Second string</param>
    /// <returns>True if the two strings are equal, false otherwise</returns>
    public static bool operator ==(FixedString s1, FixedString s2) {
        return Compare(s1, s2) == 0;
    }

    /// <summary>
    /// Implements the non-equality operator between two fixed strings.
    /// </summary>            
    /// <param name="s1">First string</param>
    /// <param name="s2">Second string</param>
    /// <returns>True if the two strings are different, false otherwise</returns>
    public static bool operator !=(FixedString s1, FixedString s2) {
        return Compare(s1, s2) != 0;
    }

    /// <summary>
    /// Serves as a hash function for a <see><cref>CCompiler.SymFullType</cref>
    /// </see>object.
    /// </summary>
    /// <returns>A hash code for this instance that is suitable for use in hashing
    /// algorithms and data structures such as a hash table.</returns>
    public override int GetHashCode() {
        unchecked {
            int hash = 17;
            hash = (hash * 23) + _fixedString.GetHashCode();
            return hash;
        }
    }

    /// <summary>
    /// Overload the + operator on two fixed strings to concatenate them.
    /// </summary>            
    /// <param name="s1">First string</param>
    /// <param name="s2">Second string</param>
    /// <returns>The combined string</returns>
    public static FixedString operator +(FixedString s1, FixedString s2) {
        return Concat(s1, s2);
    }

    /// <summary>
    /// Merge two fixed strings. The resulting string size is a new fixed
    /// string whose size is the combined sizes of the two individual
    /// fixed strings.
    /// </summary>
    /// <param name="s1">First string</param>
    /// <param name="s2">Second string</param>
    /// <returns>The merged strings</returns>
    public static FixedString Merge(FixedString s1, FixedString s2) {
        if (Equals(s1, null)) {
            throw new ArgumentNullException(nameof(s1));
        }
        if (Equals(s2, null)) {
            throw new ArgumentNullException(nameof(s2));
        }
        int length1 = s1.Length;
        int length2 = s2.Length;
        FixedString combinedString = new(length1 + length2);
        combinedString.Copy(0, s1);
        combinedString.Copy(length1, s2);
        return combinedString;
    }

    /// <summary>
    /// Concatenate two fixed strings. The resulting string is a new fixed
    /// string whose size and length is the combined length of the two
    /// individual fixed strings.
    /// </summary>
    /// <param name="s1">First string</param>
    /// <param name="s2">Second string</param>
    /// <returns>The concatenated strings</returns>
    public static FixedString Concat(FixedString s1, FixedString s2) {
        if (Equals(s1, null)) {
            throw new ArgumentNullException(nameof(s1));
        }
        if (Equals(s2, null)) {
            throw new ArgumentNullException(nameof(s2));
        }
        int length1 = s1.RealLength;
        int length2 = s2.RealLength;
        FixedString combinedString = new(length1 + length2);
        combinedString.Copy(0, s1);
        combinedString.Copy(length1, s2);
        combinedString.RealLength = length1 + length2;
        return combinedString;
    }

    /// <summary>
    /// Return a substring of the fixed string.
    /// </summary>
    /// <param name="index">Zero based index at which the substring starts</param>
    /// <param name="length">The length of the substring in characters</param>
    /// <returns>A native string containing the requested substring of the fixed string</returns>
    public string ToString(int index, int length) {
        return new string(_fixedString, index, length);
    }

    /// <summary>
    /// Returns the native string version of this fixed string.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="FixedString"/>.</returns>
    public override string ToString() {
        return new string(_fixedString, 0, RealLength);
    }

    // Copy the characters from the specified string to this one at the
    // given offset. The length is respected so only as many characters
    // are copied as will fit.
    private void Copy(int offset, FixedString str) {
        int index = 0;
        while (offset < Length && index < str.Length) {
            _fixedString[offset] = str[index];
            ++offset;
            ++index;
        }
    }

    // Copy the characters from the specified string array into this
    // one, padding out the unused portion with spaces.
    private void InternalSet(char [] newStringArray) {
        int copyLength = Math.Min(newStringArray.Length, Length);
        int index = 0;
        
        while (index < copyLength) {
            _fixedString[index] = newStringArray[index];
            ++index;
        }
        while (index < Length) {
            _fixedString[index++] = ' ';
        }
        RealLength = copyLength;
    }

    // Copy the characters from the specified string array into this
    // one, starting at the given zero based start offset and ending
    // at the given zero based end offset.
    private void InternalSet(char [] newStringArray, int start, int end) {
        int pos = start;
        int endPos = Math.Min(Length, end + 1);
        int index = 0;
        
        int newStringLength = newStringArray.Length;
        
        while (pos < endPos && index < newStringLength) {
            _fixedString[pos++] = newStringArray[index++];
        }
        while (pos < endPos) {
            _fixedString[pos++] = ' ';
        }
        if (endPos > RealLength) {
            RealLength = endPos;
        }
    }
}