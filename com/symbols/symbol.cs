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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Text;
using JComLib;

namespace CCompiler {

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
        /// Returns whether arrays are flattened. If the code generator supports multi-dimensional
        /// arrays as special types then flat arrays are simulated as 1 dimensional arrays.
        /// </summary>
        public bool IsFlatArray => Modifier.HasFlag(SymModifier.FLATARRAY);

        /// <summary>
        /// Hidden symbols are those created by the compiler, not the user.
        /// </summary>
        public bool IsHidden => Modifier.HasFlag(SymModifier.HIDDEN);

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
        /// Is this a dynamic array where one of the bounds is
        /// computed at run-time?
        /// </summary>
        public bool IsDynamicArray {
            get {
                if (IsArray) {
                    for (int c = 0; c < Dimensions.Count; ++c) {
                        if (Dimensions[c].Size < 0) {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

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
            if (Modifier.HasFlag(SymModifier.CONSTRUCTOR)) {
                str.Append("constructor ");
            }
            if (IsExternal) {
                str.Append("external ");
                if (!string.IsNullOrEmpty(ExternalLibrary)) {
                    str.Append("\"" + ExternalLibrary + "\" ");
                }
            }
            str.Append(FullType + " " + Name);
            if (IsArray && Dimensions.Count > 0) {
                str.Append('(');
                for (int c = 0; c < Dimensions.Count; ++c) {
                    if (c > 0) {
                        str.Append(',');
                    }
                    str.Append(Dimensions[c]);
                }
                str.Append(')');
            }
            if (!Defined) {
                str.Append(" (undef)");
            }
            if (Index != null) {
                str.AppendFormat(" [{0}]", Index.Index);
            }
            if (Info != null) {
                str.AppendFormat(" [{0}]", Info.ToString());
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
            string typeName = type.Name.ToLower();
            if (typeName.Contains("[]")) {
                typeName = typeName.Replace("[]", "");
            }
            if (typeName.Contains("&")) {
                typeName = typeName.Replace("&", "");
            }
            switch (typeName) {
                case "int32":           return SymType.INTEGER;
                case "float":           return SymType.FLOAT;
                case "single":          return SymType.FLOAT;
                case "double":          return SymType.DOUBLE;
                case "fixedstring":     return SymType.FIXEDCHAR;
                case "string":          return SymType.CHAR;
                case "bool":            return SymType.BOOLEAN;
                case "boolean":         return SymType.BOOLEAN;
                case "complex":         return SymType.COMPLEX;
                case "void":            return SymType.NONE;
            }
            Debug.Assert(false, $"No symbol type for {typeName}");
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
}
