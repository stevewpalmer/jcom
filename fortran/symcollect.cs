// JFortran Compiler
// Lexical token classes
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

using System.Collections.ObjectModel;
using System.Diagnostics;
using CCompiler;

namespace JFortran;

/// <summary>
/// Defines an enumerable collection of Fortran symbols.
/// </summary>
public class FortranSymbolCollection : SymbolCollection {

    private readonly SymFullType[] _typeMap = new SymFullType[26];
    private readonly bool[] _typeMapSet = new bool[26];

    /// <summary>
    /// Initializes a new instance of the <see cref="FortranSymbolCollection"/> class
    /// inheriting the implicit properties of the given symbol collection.
    /// </summary>
    /// <param name="symbols">The symbol collection to inherit</param>
    public FortranSymbolCollection(FortranSymbolCollection symbols) : base(symbols.Name) {
        if (symbols == null) {
            throw new ArgumentNullException(nameof(symbols));
        }
        _typeMap = symbols._typeMap;
        _typeMapSet = symbols._typeMapSet;
    }

    /// <summary>
    /// Initialises a new instance of the <c>FortranSymbolCollection</c> class
    /// with the given friendly name. We initialise the implicit type map to
    /// the standard Fortran defaults.
    /// </summary>
    /// <param name="name">The name of this symbol collection</param>
    public FortranSymbolCollection(string name) : base(name) {
        for (int c = 0; c < 26; ++c) {
            if (c + 'A' >= 'I' && c + 'A' <= 'N') {
                _typeMap[c] = new SymFullType(SymType.INTEGER);
            }
            else {
                _typeMap[c] = new SymFullType(SymType.FLOAT);
            }
            _typeMapSet[c] = false;
        }
    }

    /// <summary>
    /// Mark a specific implicit character as set. The character must be
    /// an upper or lower case letter.
    /// </summary>
    /// <param name="ch">An upper or lower case letter</param>
    public void SetImplicit(char ch) {
        Debug.Assert(char.IsLetter(ch));
        _typeMapSet[char.ToUpper(ch) - 'A'] = true;
    }

    /// <summary>
    /// Return whether a specific implicit character is set.
    /// </summary>
    /// <param name="ch">The character to check</param>
    /// <returns>True if an implication is set for that character</returns>
    public bool IsImplicitSet(char ch) {
        Debug.Assert(char.IsLetter(ch));
        return _typeMapSet[char.ToUpper(ch) - 'A'];
    }

    /// <summary>
    /// Return the default FullType for variables that start with the given character.
    /// </summary>
    /// <returns>The full type for character.</returns>
    /// <param name="ch">Character</param>
    public SymFullType ImplicitTypeForCharacter(char ch) {
        ch = char.ToUpper(ch);
        Debug.Assert(ch >= 'A' && ch <= 'Z');
        return _typeMap[ch - 'A'];
    }

    /// <summary>
    /// Sets the implicit type for a character letter range. chFirst must be less than
    /// or equal to chLast or an assertion is thrown. Both chFirst and chLast must be
    /// uppercase letters in the range A to Z inclusive
    /// </summary>
    /// <param name="chFirst">The first character in the range</param>
    /// <param name="chLast">The last character in the range</param>
    /// <param name="fullType">The full type associated to the range</param>
    public void Implicit(char chFirst, char chLast, SymFullType fullType) {
        Debug.Assert(chFirst >= 'A' && chFirst <= 'Z' && chLast >= 'A' && chLast <= 'Z');
        Debug.Assert(chFirst <= chLast);
        for (int c = chFirst - 'A'; c <= chLast - 'A'; ++c) {
            _typeMap[c] = fullType;
        }
    }

    /// <summary>
    /// Add the specified identifier to the symbol table with the given type.
    /// If type is TYP_NONE then consult the implicit array to determine what it
    /// should be.
    /// 
    /// For character types, width may be specified to indicate the width of the
    /// character array. For other types this has no purpose.
    /// 
    /// For arrays, explicit constant dimensions may be specified. By setting
    /// dimensions, the identifier is automatically marked as an array. A non-array
    /// identifier can be promoted to an array later by setting the dimensions
    /// separately.
    /// 
    /// The reference line is a line number in the source code where the identifier
    /// was first referenced. This is used later in error and warning messages.
    /// </summary>
    /// <param name="name">The identifier name</param>
    /// <param name="fullType">The full type of the identifier</param>
    /// <param name="klass">The class of the identifier</param>
    /// <param name="dimensions">Optional dimensions for array identifiers</param>
    /// <param name="refLine">Line number reference</param>
    /// <returns>A newly created symbol table entry or null.</returns>
    public override Symbol Add(string name, SymFullType fullType, SymClass klass, Collection<SymDimension> dimensions, int refLine) {
        if (name == null) {
            throw new ArgumentNullException(nameof(name));
        }
        if (fullType == null) {
            throw new ArgumentNullException(nameof(fullType));
        }
        if (fullType.Type == SymType.NONE) {
            char chFirst = char.ToUpper(name[0]);
            if (char.IsLetter(chFirst)) {
                fullType = _typeMap[chFirst - 'A'];
                if (fullType.Type == SymType.NONE) {
                    // This means IMPLICIT NONE and no type was specified, which
                    // is an error. But this isn't the right place to report.
                    return null;
                }
            }
        }
        return base.Add(name, fullType, klass, dimensions, refLine);
    }
}