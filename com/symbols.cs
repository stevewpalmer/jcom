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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Text;
using JComLib;

namespace CCompiler {
    
    /// <summary>
    /// Defines the symbol type.
    /// </summary>
    public enum SymType {
        NONE, INTEGER, FLOAT, DOUBLE, BOOLEAN, LABEL, CHAR, FIXEDCHAR, REF,
        VARARG, PROGRAM, COMMON, GENERIC, COMPLEX
    }

    /// <summary>
    /// Defines the symbol storage class.
    /// </summary>
    public enum SymClass {
        PROGRAM, VAR, COMMON, FUNCTION, SUBROUTINE, LABEL, INTRINSIC, INLINE
    }

    /// <summary>
    /// Defines the scope of a symbol
    /// </summary>
    public enum SymScope {
        LOCAL, PARAMETER, CONSTANT
    }

    /// <summary>
    /// For parameters, indicates how the parameter is passed.
    /// </summary>
    public enum SymLinkage {
        BYREF, BYVAL
    }

    /// <summary>
    /// Defines a set of modifiers applied to the symbol.
    /// </summary>
    [Flags]
    public enum SymModifier {
        EXTERNAL = 2,
        STATIC = 4,
        RETVAL = 8,
        FIXED = 16,
        FLATARRAY = 32,
        ENTRYPOINT = 64,
        EXPORTED = 128
    }

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
        /// Initialises a copy of the specified SymFullType.
        /// </summary>
        /// <param name="copy">A SymFullType object to copy</param>
        public SymFullType(SymFullType copy) {
            if (copy == null) {
                throw new ArgumentNullException(nameof(copy));
            }
            Type = copy.Type;
            Width = copy.Width;
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
                hash = (hash * 23) + Type.GetHashCode();
                hash = (hash * 23) + Width.GetHashCode();
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

    /// <summary>
    /// Defines a single array dimension. Separate lower and upper bounds can
    /// be specifies for languages that support this.
    /// </summary>
    public class SymDimension {

        /// <value>
        /// Gets or set the array lower bound.
        /// </value>
        public ParseNode LowerBound { get; set; }

        /// <value>
        /// Gets or sets the array upper bound.
        /// </value>
        public ParseNode UpperBound { get; set; }

        /// <value>
        /// Returns the size, in units, of this dimension or -1
        /// if the dimension is dynamic (ie. cannot be computed
        /// until runtime).
        /// </value>
        public int Size {
            get {
                if (UpperBound.IsConstant && LowerBound.IsConstant) {
                    return UpperBound.Value.IntValue - LowerBound.Value.IntValue + 1;
                }
                return -1;
            } 
        }

        /// <summary>
        /// Return the symbol dimension as a string..
        /// </summary>
        /// <returns>The symbol dimensions formatted as a string.</returns>
        public override string ToString() {
            if (LowerBound.IsConstant && LowerBound.Value.IntValue == 1) {
                return UpperBound.ToString();
            }
            return $"{LowerBound}:{UpperBound}";
        }
    }

    /// <summary>
    /// Defines a single symbol.
    /// </summary>
    public class Symbol {

        /// <summary>
        /// Initialises a new instance of the <c>Symbol</c> class.
        /// </summary>
        /// <param name="name">The string that contains the symbol name. Must not be null.</param>
        /// <param name="fullType">The symbol's full type as a <c>SymFullType</c></param>
        /// <param name="klass">The symbol class as a <c>SymClass</c></param>
        /// <param name="dimensions">For arrays, the optional array dimensions as a list of <c>SymDimension</c> items.</param>
        /// <param name="refLine">The referencing source file line for the symbol</param>
        public Symbol(string name, SymFullType fullType, SymClass klass, Collection<SymDimension> dimensions, int refLine) {
            Name = name;
            FullType = fullType;
            Defined = false;
            Info = null;
            Depth = 0;
            Dimensions = dimensions;
            Index = null;
            RefLine = refLine;
            Common = null;
            CommonIndex = -1;
            Class = klass;
            InlineValue = null;
            Value = new Variant();
            Linkage = SymLinkage.BYVAL;
            IsReferenced = false;
        }

        /// <value>
        /// Returns whether the symbol represents a function or subroutine parameter.
        /// </value>
        public bool IsParameter => Scope == SymScope.PARAMETER;

        /// <value>
        /// Returns whether the symbol represents a constant value.
        /// </value>
        public bool IsConstant => Scope == SymScope.CONSTANT;

        /// <value>
        /// Returns whether the symbol is local to a function, subroutine or
        /// statement block.
        /// </value>
        public bool IsLocal => Scope == SymScope.LOCAL;

        /// <value>
        /// Returns whether the symbol is an array.
        /// </value>
        public bool IsArray => Dimensions != null && Dimensions.Count > 0;

        /// <value>
        /// Returns whether the symbol is a statement label.
        /// </value>
        public bool IsLabel => Type == SymType.LABEL;

        /// <value>
        /// Returns whether the symbol is part of a Fortran COMMON block.
        /// </value>
        public bool IsInCommon => Common != null;

        /// <value>
        /// Returns whether the symbol is the name of a function or subroutine.
        /// </value>
        public bool IsMethod => Class == SymClass.FUNCTION || Class == SymClass.SUBROUTINE;

        /// <summary>
        /// Returns whether the symbol refers to a method imported from an external
        /// library.
        /// </summary>
        public bool IsImported => IsExternal && !string.IsNullOrEmpty(ExternalLibrary);

        /// <value>
        /// Returns whether the symbol is an intrinsic function name.
        /// </value>
        public bool IsIntrinsic => Class == SymClass.INTRINSIC && !IsExternal;

        /// <summary>
        /// Returns whether the symbol has EXTERNAL scope on it.
        /// </summary>
        public bool IsExternal => Modifier.HasFlag(SymModifier.EXTERNAL);

        /// <summary>
        /// Returns whether the symbol is a static scope on it.
        /// </summary>
        public bool IsStatic => Modifier.HasFlag(SymModifier.STATIC);

        /// <summary>
        /// Returns whether the symbol is exported for other modules to use.
        /// </summary>
        public bool IsExported => Modifier.HasFlag(SymModifier.EXPORTED);

        /// <summary>
        /// Returns whether the symbol is a fixed static on it.
        /// </summary>
        public bool IsFixedStatic => Modifier.HasFlag(SymModifier.FIXED) && Modifier.HasFlag(SymModifier.STATIC);

        /// <summary>
        /// Returns whether arrays are flattened. If the code generator supports multi-dimensional
        /// arrays as special types then flat arrays are simulated as 1 dimensional arrays.
        /// </summary>
        public bool IsFlatArray => Modifier.HasFlag(SymModifier.FLATARRAY);

        /// <summary>
        /// Returns whether the symbol is passed by reference.
        /// </summary>
        public bool IsByRef => Linkage == SymLinkage.BYREF;

        /// <summary>
        /// Returns whether the symbol is passed by reference.
        /// </summary>
        public bool IsInline => Class == SymClass.INLINE;

        /// <summary>
        /// Returns whether the symbol is actually used. Default is false unless the compiler
        /// explicitly sets this based on analysis.
        /// </summary>
        public bool IsReferenced { get; set; }

        /// <value>
        /// Gets or sets the <c>SymScope</c> scope of this symbol.
        /// </value>
        public SymScope Scope { get; set; }

        /// <value>
        /// Gets or sets the referencing source file line number for this symbol.
        /// </value>
        public int RefLine { get; set; }

        /// <value>
        /// Gets or sets the symbol name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the parse tree that defines the computed
        /// inline value for this symbol.
        /// </summary>
        public ParseNode InlineValue { get; set; }

        /// <summary>
        /// Gets or sets the full symbol type.
        /// </summary>
        /// <value>The full type.</value>
        public SymFullType FullType { get; set; }

        /// <summary>
        /// Retrieves the base type of this symbol.
        /// </summary>
        /// <value>The symbol base type</value>
        public SymType Type => FullType.Type;

        /// <summary>
        /// Gets or sets the <c>SymClass</c> storage class.
        /// </summary>
        /// <value>The class.</value>
        public SymClass Class { get; set; }

        /// <value>
        /// Gets or sets the <c>SymModifier</c> symbol visibility.
        /// </value>
        public SymModifier Modifier { get; set; }

        /// <value>
        /// Gets or sets the <c>SymLinkage</c> symbol linkage.
        /// </value>
        public SymLinkage Linkage { get; set; }

        /// <value>
        /// Gets or sets a flag which indicates whether the symbol has been
        /// explicitly defined for languages that support implicit declarations.
        /// </value>
        public bool Defined { get; set; }

        /// <value>
        /// Gets or sets the corresponding COMMON block symbol to which this
        /// symbol belongs. (Fortran only).
        /// </value>
        public Symbol Common { get; set; }

        /// <value>
        /// Gets or sets the index in the corresponding COMMON block symbol
        /// of this symbol. (Fortran only).
        /// </value>
        public int CommonIndex { get; set; }

        /// <summary>
        /// Returns whether this symbol references a symbol in a common block
        /// at the global scope. (Fortran only).
        /// </summary>
        /// <value><c>true</c> if this instance references a common; otherwise, <c>false</c>.</value>
        public bool IsReferenceCommon => IsInCommon && !IsStatic;

        /// <value>
        /// Gets or sets the array dimensions. Setting dimensions implicitly
        /// types this symbol as an array.
        /// </value>
        public Collection<SymDimension> Dimensions { get; set; }

        /// <value>
        /// Gets or sets a list of parameter symbols for function or
        /// subroutines. It is an error to set parameters on a symbol without
        /// first setting the symbol type as a method.
        /// </value>
        public Collection<Symbol> Parameters { get; set; }

        /// <value>
        /// Code Generator: Gets or sets the local variable reference
        /// associated with this symbol.
        /// </value>
        public LocalDescriptor Index { get; set; }

        /// <value>
        /// Code Generator: Sets the index of this parameter in the parameter
        /// list.
        /// </value>
        public int ParameterIndex { get; set; }

        /// <value>
        /// Gets or sets the corresponding return value symbol for functions
        /// for languages that support setting the function name to a value
        /// to return that value.
        /// </value>
        public Symbol RetVal { get; set; }

        /// <value>
        /// Gets or sets the parent symbol if this symbol is scoped to
        /// another.
        /// </value>
        public Symbol Parent { get; set; }

        /// <summary>
        /// Get or sets the external library name for EXTERNAL procedures
        /// or functions.
        /// </summary>
        public string ExternalLibrary { get; set; }

        /// <summary>
        /// Gets or retrieves the block depth at which this symbol was set.
        /// </summary>
        /// <value>The depth.</value>
        public int Depth { get; set; }

        /// <value>
        /// Gets or sets the default value for this symbol.
        /// </value>
        public Variant Value { get; set; }

        /// <value>
        /// Code Generator: Sets the associated code generator object that is
        /// associated with this symbol.
        /// </value>
        public object Info { get; set; }

        /// <summary>
        /// Gets or sets the values for an array.
        /// </summary>
        public Variant [] ArrayValues { get; set; }

        /// <summary>
        /// Return the total number of elements in this array, which is the
        /// product of all dimension sizes. Returns 0 if this is not an array.
        /// </summary>
        /// <value>The size of the array.</value>
        public int ArraySize {
            get {
                int arraySize = 0;
                if (IsArray) {
                    arraySize = 1;
                    for (int c = 0; c < Dimensions.Count; ++c) {
                        arraySize *= Dimensions[c].Size;
                    }
                }
                return arraySize;
            }
        }

        /// <summary>
        /// Returns the symbol name in a presentable format.
        /// </summary>
        /// <returns>The symbol table entry</returns>
        public override string ToString() {
            StringBuilder str = new();
            if (IsConstant) {
                str.Append("const ");
            }
            if (IsParameter) {
                str.Append("param ");
            }
            if (IsParameter) {
                str.Append("local ");
            }
            if (IsExternal) {
                str.Append("external ");
                if (!string.IsNullOrEmpty(ExternalLibrary)) {
                    str.Append("\"" + ExternalLibrary + "\" ");
                }
            }
            str.Append(FullType + " " + Name);
            if (IsArray && Dimensions.Count > 0) {
                str.Append("(");
                for (int c = 0; c < Dimensions.Count; ++c) {
                    if (c > 0) {
                        str.Append(",");
                    }
                    str.Append(Dimensions[c]);
                }
                str.Append(")");
            }
            if (!Defined) {
                str.Append(" (undef)");
            }
            if (Info != null) {
                str.AppendFormat(" [{0}]", Index);
            }
            return str.ToString();
        }

        /// <summary>
        /// Returns whether the given type is a value type. A value
        /// type is one which is natively represented by the machine
        /// and can be stored and passed around in a register.
        /// </summary>
        /// <returns><c>true</c> if this instance is value type the specified type; otherwise, <c>false</c>.</returns>
        public bool IsValueType => Type switch {
            SymType.BOOLEAN or SymType.CHAR or SymType.DOUBLE or SymType.FLOAT or SymType.INTEGER or SymType.COMPLEX => true,
            _ => false,
        };

        /// <summary>
        /// Maps a symbol table type to a system type.
        /// </summary>
        /// <param name="type">Symbol type</param>
        /// <returns>The corresponding system type</returns>
        public static Type SymTypeToSystemType(SymType type) {
            switch (type) {
                case SymType.FIXEDCHAR: return typeof(FixedString);
                case SymType.CHAR:      return typeof(string);
                case SymType.FLOAT:     return typeof(float);
                case SymType.DOUBLE:    return typeof(double);
                case SymType.INTEGER:   return typeof(int);
                case SymType.BOOLEAN:   return typeof(bool);
                case SymType.COMPLEX:   return typeof(Complex);
                case SymType.VARARG:    return typeof(object[]);
                case SymType.REF:       return typeof(IntPtr);
                case SymType.GENERIC:   return typeof(object);
            }
            Debug.Assert(false, $"No system type for {type}");
            return typeof(int);
        }

        /// <summary>
        /// Maps this symbol to a system type.
        /// </summary>
        /// <returns>The system type corresponding to the symbol</returns>
        public Type SystemType {
            get {
                if (IsArray) {
                    Type baseType = SymTypeToSystemType(Type);
                    int dimensionsCount = IsFlatArray ? 1 : Dimensions.Count;
                    return dimensionsCount == 1 ? baseType.MakeArrayType() : baseType.MakeArrayType(dimensionsCount);
                }
                if (IsExternal || IsMethod) {
                    return typeof(IntPtr);
                }
                return SymTypeToSystemType(Type);
            }
        }

        /// <summary>
        /// Return whether this symbol needs to be explicitly initialised in the code.
        /// </summary>
        public bool CanInitialise {
            get {
                if (ArrayValues != null && ArrayValues.Length > 0) {
                    return true;
                }
                if (IsArray) {
                    return false;
                }
                if (!Value.HasValue) {
                    return false;
                }
                if (Type == SymType.FIXEDCHAR && Value.StringValue.Length == 0) {
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Map a system table type to a symbol type.
        /// </summary>
        /// <param name="type">A system type</param>
        /// <returns>The symbol type that corresponds to the given system
        /// type or SymType.NONE if the system type is unrecognised.</returns>
        public static SymType SystemTypeToSymbolType(Type type) {
            if (type == null) {
                throw new ArgumentNullException(nameof(type));
            }
            switch (type.Name.ToLower()) {
                case "int32":           return SymType.INTEGER;
                case "float":           return SymType.FLOAT;
                case "single":          return SymType.FLOAT;
                case "double":          return SymType.DOUBLE;
                case "fixedstring":     return SymType.FIXEDCHAR;
                case "string":          return SymType.CHAR;
                case "bool":            return SymType.BOOLEAN;
                case "complex":         return SymType.COMPLEX;
                case "void":            return SymType.NONE;
            }
            Debug.Assert(false, "No symbol type for "+type);
            return SymType.NONE;
        }

        /// <summary>
        /// Returns whether the specific type is numeric type.
        /// </summary>
        /// <param name="type">The symbol type to check</param>
        /// <returns>True if the symbol type is numeric, false otherwise</returns>
        [Pure]
        public static bool IsNumberType(SymType type) {
            return type == SymType.DOUBLE ||
                   type == SymType.INTEGER ||
                   type == SymType.COMPLEX ||
                   type == SymType.FLOAT;
        }

        /// <summary>
        /// Returns whether the specified type is a floating point (non-integral) type.
        /// </summary>
        /// <param name="type">The symbol type to check</param>
        /// <returns><c>true</c> if type is a floating point type; otherwise, <c>false</c>.</returns>
        public static bool IsFloatingPointType(SymType type) {
            return type == SymType.DOUBLE ||
                   type == SymType.COMPLEX ||
                   type == SymType.FLOAT;
        }

        /// <summary>
        /// Returns whether the specific type is a character type.
        /// </summary>
        /// <param name="type">The symbol type to check</param>
        /// <returns>True if the symbol type is character, false otherwise</returns>
        [Pure]
        public static bool IsCharType(SymType type) {
            return type == SymType.CHAR || type == SymType.FIXEDCHAR;
        }
        
        /// <summary>
        /// Returns whether the specific type is a logical (boolean) type.
        /// </summary>
        /// <param name="type">The symbol type to check</param>
        /// <returns>True if the symbol type is logical, false otherwise</returns>
        [Pure]
        public static bool IsLogicalType(SymType type) {
            return type == SymType.BOOLEAN;
        }

        /// <summary>
        /// Return the symbol type that can contain the largest of the two given. The types
        /// must both be numeric. Thus if one type is double and the other is integer, an
        /// double is returned since it can contain a value of both types.
        /// </summary>
        /// <param name="op1">The first type to compare</param>
        /// <param name="op2">The second type to compare</param>
        /// <returns>The largest numeric type that can represent both types</returns>
        [Pure]
        public static SymType LargestType(SymType op1, SymType op2) {
            if (op1 == SymType.INTEGER) {
                return op2;
            }
            if (op1 == SymType.FLOAT) {
                return op2 == SymType.INTEGER ? op1 : op2;
            }
            return op1;
        }

        /// <summary>
        /// Map a Variant type to its equivalent symbol type.
        /// </summary>
        /// <param name="variantType">Variant type</param>
        /// <returns>Symbol type</returns>
        public static SymType VariantTypeToSymbolType(VariantType variantType) {

            switch (variantType) {
                case VariantType.BOOLEAN:   return SymType.BOOLEAN;
                case VariantType.NONE:      return SymType.NONE;
                case VariantType.INTEGER:   return SymType.INTEGER;
                case VariantType.FLOAT:     return SymType.FLOAT;
                case VariantType.DOUBLE:    return SymType.DOUBLE;
                case VariantType.COMPLEX:   return SymType.COMPLEX;
                case VariantType.STRING:    return SymType.CHAR;
            }
            Debug.Assert(false, $"Unhandled variant type {variantType}");
            return SymType.NONE;
        }

        /// <summary>
        /// Dumps the contents of this symbol item to the ParseNode XML
        /// output under the specified parent node.
        /// </summary>
        /// <param name="root">The parent XML node</param>
        public void Dump(ParseNodeXml root) {
            ParseNodeXml blockNode = root.Node("Symbol");
            blockNode.Attribute("Name", Name);
            blockNode.Write("Type", FullType.ToString());
            blockNode.Write("Scope", Scope.ToString());
            blockNode.Write("Class", Class.ToString());
            if (Modifier != 0) {
                blockNode.Write("Modifiers", Modifier.ToString());
            }
            if (Parent != null) {
                Parent.Dump(blockNode);
            }
            if (!string.IsNullOrEmpty(ExternalLibrary)) {
                blockNode.Write("ExternalLibrary", ExternalLibrary);
            }
        }
    }

    /// <summary>
    /// Defines an enumerable collection of symbols.
    /// </summary>
    public class SymbolCollection : IEnumerable<Symbol> {

        private readonly Dictionary<string, Symbol> _symbols = new();

        /// <summary>
        /// Initialises a new instance of the <c>SymbolCollection</c> class
        /// with the given friendly name. The name is used in the dump XML
        /// output to identify this collection.
        /// </summary>
        /// <param name="name">The name of this symbol collection</param>
        public SymbolCollection(string name) {
            Name = name;
            CaseSensitive = false;
        }

        /// <summary>
        /// Are symbols case sensitive? Default is no.
        /// </summary>
        public bool CaseSensitive { get; set; }

        /// <value>
        /// The name of this symbol collection.
        /// </value>
        public string Name { get; set; }
        
        /// <summary>
        /// Return the symbol table entry for the given identifier.
        /// </summary>
        /// <param name="name">An identifier</param>
        /// <returns>The symbol table entry for the identifier or null if the
        /// identifier does not exist.</returns>
        public Symbol Get(string name) {

            if (!CaseSensitive) {
                name = name.ToUpper();
                foreach (Symbol symbol in _symbols.Values) {
                    if (symbol.Name.ToUpper() == name) {
                        return symbol;
                    }
                }
                return null;
            }
            _symbols.TryGetValue(name, out Symbol sym);
            return sym;
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
        public virtual Symbol Add(string name, SymFullType fullType, SymClass klass, Collection<SymDimension> dimensions, int refLine) {
            if (name == null) {
                throw new ArgumentNullException(nameof(name));
            }
            if (fullType == null) {
                throw new ArgumentNullException(nameof(fullType));
            }
            Symbol newSymbol = new(name, fullType, klass, dimensions, refLine);
            _symbols[name] = newSymbol;
            return newSymbol;
        }

        /// <summary>
        /// Remove the specified name from the symbol table.
        /// </summary>
        /// <param name="type">Symbol type to be removed</param>
        public void Clear(string name) {

            if (_symbols.ContainsKey(name)) {
                _symbols.Remove(name);
            }
        }

        /// <summary>
        /// Add the specified symbol to this symbol table.
        /// </summary>
        /// <param name="sym">Symbol to be added</param>
        public void Add(Symbol sym) {

            string name = sym.Name;
            if (!CaseSensitive) {
                name = name.ToUpper();
            }
            _symbols[name] = sym ?? throw new ArgumentNullException(nameof(sym));
        }

        /// <summary>
        /// Add the specified symbol collection to this symbol table.
        /// </summary>
        /// <param name="symbols">Symbols to be added</param>
        public void Add(SymbolCollection symbols) {
            foreach (Symbol sym in symbols) {
                Add(sym);
            }
        }

        /// <summary>
        /// Removes the specified symbol from the symbol table.
        /// </summary>
        /// <param name="sym">Symbol to be removed</param>
        /// <returns>True if the symbol was successfully removed, false otherwise.</returns>
        public bool Remove(Symbol sym) {
            bool success;

            if (_symbols.ContainsValue(sym)) {
                string name = sym.Name;
                if (!CaseSensitive) {
                    name = name.ToUpper();
                }
                _symbols.Remove(name);
                success = true;
            } else {
                success = false;
            }
            return success;
        }

        /// <summary>
        /// Dumps the contents of this symbol collection to the ParseNode XML
        /// output under the specified parent node.
        /// </summary>
        /// <param name="root">The parent XML node</param>
        public void Dump(ParseNodeXml root) {
            ParseNodeXml blockNode = root.Node("SymbolTable");
            blockNode.Attribute("Name", Name);
            blockNode.Attribute("CaseSensitive", CaseSensitive.ToString());
            foreach (Symbol sym in _symbols.Values) {
                sym.Dump(blockNode);
            }
        }

        /// <summary>
        /// Enumerator for all messages.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public IEnumerator<Symbol> GetEnumerator()
        {
            return _symbols.Values.GetEnumerator();
        }

        // Non-generic enumerator
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    /// <summary>
    /// Defines a stack of symbol collections
    /// </summary>
    public class SymbolStack {

        private readonly List<SymbolCollection> _stack = new();

        /// <summary>
        /// Return the symbol table at the top of the stack. 
        /// </summary>
        public SymbolCollection Top => _stack[0];

        /// <summary>
        /// Return all symbols ordered from top downwards.
        /// </summary>
        public SymbolCollection [] All => _stack.ToArray();

        /// <summary>
        /// Add a new symbol table to the top of the stack
        /// </summary>
        public void Push(SymbolCollection symbols) {
            _stack.Insert(0, symbols);
        }

        /// <summary>
        /// Removes the symbol table at the top of the stack. You cannot
        /// remove the global symbol table.
        /// </summary>
        public void Pop() {
            Debug.Assert(_stack.Count > 1);
            _stack.RemoveAt(0);
        }
    }
}
