// JCom Compiler Toolkit
// Symbol Table management
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

namespace CCompiler; 

/// <summary>
/// Defines a full class representation complete with
/// the width of the type.
/// </summary>
public class SymFullType : IEquatable<SymFullType> {

    /// <summary>
    /// Initialises a default instance of SymFullType.
    /// </summary>
    public SymFullType() {
        Type = SymType.NONE;
        Width = 0;
    }

    /// <summary>
    /// Returns whether this object is identical to the given object.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to compare with the 
    /// current <see cref="SymFullType"/>.</param>
    /// <returns><c>true</c> if the specified <see cref="object"/> is 
    /// equal to the current
    /// <see cref="SymFullType"/>; otherwise, <c>false</c>.</returns>
    public override bool Equals(object obj) {
        if (!(obj is SymFullType)) {
            return false;
        }
        SymFullType symOther = (SymFullType)obj;
        return symOther.Type == Type && symOther.Width == Width;
    }

    /// <summary>
    /// Determines whether the specified <see cref="SymFullType"/> is
    /// equal to the current <see cref="SymFullType"/>.
    /// </summary>
    /// <param name="symOther">The <see cref="SymFullType"/> to compare
    /// with the current <see cref="SymFullType"/>.</param>
    /// <returns><c>true</c> if the specified <see cref="SymFullType"/>
    /// is equal to the current
    /// <see cref="SymFullType"/>; otherwise, <c>false</c>.</returns>
    public bool Equals(SymFullType symOther) {
        return symOther != null && symOther.Type == Type && symOther.Width == Width;
    }

    /// <summary>
    /// Serves as a hash function for a <see cref="SymFullType"/> object.
    /// </summary>
    /// <returns>A hash code for this instance that is suitable for use in hashing
    /// algorithms and data structures such as a hash table.</returns>
    public override int GetHashCode() {
        unchecked {
            int hash = 17;
            hash = hash * 23 + Type.GetHashCode();
            hash = hash * 23 + Width.GetHashCode();
            return hash;
        }
    }

    /// <summary>
    /// Initialises an instance of SymFullType with the given type and
    /// default width.
    /// </summary>
    /// <param name="type">A SymType type</param>
    public SymFullType(SymType type) {
        Type = type;
        Width = 1;
    }

    /// <summary>
    /// Initialises an instance of SymFullType with the given type and
    /// width.
    /// </summary>
    /// <param name="type">A SymType type</param>
    /// <param name="width">The width of the type</param>
    public SymFullType(SymType type, int width) {
        Type = type;
        Width = width;
    }

    /// <summary>
    /// Retrieves the base type of this full stype.
    /// </summary>
    public SymType Type { get; set; }

    /// <summary>
    /// Retrieves the width of this full type.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Returns a string that represents the current full type.
    /// </summary>
    /// <returns>A string that describes this full symbol type</returns>
    public override string ToString() {
        if (Width == 1) {
            return Type.ToString();
        }
        return $"{Type} *{Width}";
    }
}
