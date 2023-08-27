// JFortran Compiler
// Fortran instrinsic statements
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

using CCompiler;

namespace JFortran; 

/// <summary>
/// Indicates how many arguments the function accepts. This isn't
/// necessarily a fixed number so this is a flag instead.
/// </summary>
public enum ArgCount {
    One,
    OneOrTwo,
    Two,
    TwoOrMore
}

/// <summary>
/// Defines a single intrinsic, including information about the argument and return
/// type and any inlined delegate.
/// </summary>
public sealed class IntrDefinition {
        
    // Bitmask for valid argument types
    private const int I = 1;
    private const int R = 2;
    private const int D = 4;
    private const int L = 8;
    private const int S = 16;
    private const int C = 32;
    private const int X = 64;
    
    /// <summary>
    ///  Constructs a single intrinsic definition.
    /// </summary>
    /// <param name="count">Indicates how many arguments the function takes</param>
    /// <param name="types">Bitmask representing the valid argument types</param>
    /// <param name="requiredType">Type to which the argument must be cast</param>
    /// <param name="returnType">Symbol representing the return type</param>
    public IntrDefinition(string functionName, ArgCount count, int types, SymType requiredType, SymType returnType) {
        FunctionName = functionName;
        Count = count;
        Types = types;
        RequiredType = requiredType;
        ReturnType = returnType;
    }
        
    /// <summary>
    /// Returns whether the given symbol type is valid as an argument for
    /// this intrinsic. Note that this does not handle type coercion.
    /// </summary>
    /// <param name="type">The symbol type to check</param>
    /// <returns>True if the type is valid, false otherwise</returns>
    public bool IsValidArgType(SymType type) {
        return type switch {
            SymType.CHAR => (Types & S) != 0,
            SymType.FIXEDCHAR => (Types & S) != 0,
            SymType.INTEGER => (Types & I) != 0,
            SymType.FLOAT => (Types & R) != 0,
            SymType.DOUBLE => (Types & D) != 0,
            SymType.BOOLEAN => (Types & L) != 0,
            SymType.COMPLEX => (Types & C) != 0,
            _ => false,
        };
    }
        
    /// <summary>
    /// Return the arguments type, or GENERIC if the types are mixed.
    /// </summary>
    /// <returns>The symbol type of the arguments.</returns>
    public SymType ArgType() {
        return Types switch {
            I => SymType.INTEGER,
            R => SymType.FLOAT,
            D => SymType.DOUBLE,
            L => SymType.BOOLEAN,
            S => SymType.FIXEDCHAR,
            C => SymType.COMPLEX,
            _ => SymType.GENERIC,
        };
    }

    /// <summary>
    /// Determines whether this instance is permitted in an INTRINSIC statement.
    /// </summary>
    /// <returns><c>true</c> if this instance is permitted in INTRINSIC; otherwise, <c>false</c>.</returns>
    public bool IsPermittedInIntrinsic => (Types & X) == 0;

    /// <summary>
    /// Returns the actual type to which each argument of a multi-type intrinsic
    /// must be cast before the intrinsic is called. This is typically the same as
    /// the computed argument in which case RequiredType returns SymType.GENERIC
    /// to indicate that no casting is required.
    /// </summary>
    public SymType RequiredType { get; private set; }

    /// <value>
    /// Returns the number of arguments accepted by the intrinsic function.
    /// </value>
    public ArgCount Count { get; private set; }

    /// <value>
    /// Returns the symbol type of the return value of the intrinsic function.
    /// </value>
    public SymType ReturnType { get; private set; }

    /// <summary>
    /// Actual name of intrinsic in JComLib
    /// </summary>
    public string FunctionName { get; private set; }
        
    // Private automatic property accessors
    private int Types { get; set; }
}
    
/// <summary>
/// Class that provides details of built-in Fortran intrinsics.
/// </summary>
public static class Intrinsics {
        
    // Bitmask for valid argument types
    // TODO: Duplicated in IntrDefinition because I wanted to keep these
    //  compact. But fix the duplication!
    private const int I = 1;
    private const int R = 2;
    private const int D = 4;
    private const int S = 16;
    private const int C = 32;
    private const int X = 64; // Specifies that function is not permitted in INTRINSIC

    // List of intrinsics and their types:
    //
    // * Name, in upper case.
    // * Number of arguments (ArgCount)
    // * Type of input arguments. Note that both arguments must be of the same type but the compiler
    //   is free to cast as appropriate.
    // * Required type
    // * Return type.
    //
    private static readonly Dictionary<string, IntrDefinition> _intrinsics = new() {
        { "ABS",        new IntrDefinition("ABS",     ArgCount.One,          I|R|D|C,      SymType.GENERIC,  SymType.GENERIC) },
        { "ACOS",       new IntrDefinition("ACOS",    ArgCount.One,          R|D,          SymType.GENERIC,  SymType.DOUBLE)  },
        { "ALOG",       new IntrDefinition("ALOG",    ArgCount.One,          R,            SymType.GENERIC,  SymType.FLOAT)   },
        { "ALOG10",     new IntrDefinition("ALOG10",  ArgCount.One,          R,            SymType.GENERIC,  SymType.FLOAT)   },
        { "AINT",       new IntrDefinition("AINT",    ArgCount.One,          R|D,          SymType.GENERIC,  SymType.FLOAT)   },
        { "AIMAG",      new IntrDefinition("AIMAG",   ArgCount.One,          C,            SymType.GENERIC,  SymType.FLOAT)   },
        { "AMAX0",      new IntrDefinition("AMAX0",   ArgCount.TwoOrMore,    I|X,          SymType.GENERIC,  SymType.FLOAT)   },
        { "AMAX1",      new IntrDefinition("AMAX1",   ArgCount.TwoOrMore,    R|X,          SymType.GENERIC,  SymType.FLOAT)   },
        { "AMIN0",      new IntrDefinition("AMIN0",   ArgCount.TwoOrMore,    I|X,          SymType.GENERIC,  SymType.FLOAT)   },
        { "AMIN1",      new IntrDefinition("AMIN1",   ArgCount.TwoOrMore,    R|X,          SymType.GENERIC,  SymType.FLOAT)   },
        { "AMOD",       new IntrDefinition("AMOD",    ArgCount.Two,          R,            SymType.GENERIC,  SymType.FLOAT)   },
        { "ANINT",      new IntrDefinition("ANINT",   ArgCount.One,          R|D,          SymType.GENERIC,  SymType.GENERIC) },
        { "ASIN",       new IntrDefinition("ASIN",    ArgCount.One,          R|D,          SymType.GENERIC,  SymType.GENERIC) },
        { "ATAN",       new IntrDefinition("ATAN",    ArgCount.One,          R|D,          SymType.GENERIC,  SymType.GENERIC) },
        { "ATAN2",      new IntrDefinition("ATAN2",   ArgCount.Two,          R|D,          SymType.GENERIC,  SymType.DOUBLE)  },
        { "CABS",       new IntrDefinition("CABS",    ArgCount.One,          C,            SymType.GENERIC,  SymType.FLOAT)   },
        { "CCOS",       new IntrDefinition("CCOS",    ArgCount.One,          C,            SymType.GENERIC,  SymType.COMPLEX) },
        { "CEXP",       new IntrDefinition("CEXP",    ArgCount.One,          C,            SymType.GENERIC,  SymType.COMPLEX) },
        { "CHAR",       new IntrDefinition("CHAR",    ArgCount.One,          I|X,          SymType.GENERIC,  SymType.FIXEDCHAR) },
        { "CLOG",       new IntrDefinition("CLOG",    ArgCount.One,          C,            SymType.GENERIC,  SymType.COMPLEX) },
        { "CONJG",      new IntrDefinition("CONJG",   ArgCount.One,          C,            SymType.GENERIC,  SymType.COMPLEX) },
        { "COS",        new IntrDefinition("COS",     ArgCount.One,          R|D|C,        SymType.GENERIC,  SymType.GENERIC) },
        { "COSH",       new IntrDefinition("COSH",    ArgCount.One,          R|D,          SymType.GENERIC,  SymType.GENERIC) },
        { "CMPLX",      new IntrDefinition("CMPLX",   ArgCount.OneOrTwo,     I|R|D|C|X,    SymType.DOUBLE,   SymType.COMPLEX) },
        { "CSIN",       new IntrDefinition("CSIN",    ArgCount.One,          C,            SymType.GENERIC,  SymType.COMPLEX) },
        { "CSQRT",      new IntrDefinition("CSQRT",   ArgCount.One,          C,            SymType.GENERIC,  SymType.COMPLEX) },
        { "DABS",       new IntrDefinition("DABS",    ArgCount.One,          D,            SymType.GENERIC,  SymType.DOUBLE)  },
        { "DACOS",      new IntrDefinition("DACOS",   ArgCount.One,          D,            SymType.GENERIC,  SymType.DOUBLE)  },
        { "DASIN",      new IntrDefinition("DASIN",   ArgCount.One,          D,            SymType.GENERIC,  SymType.DOUBLE)  },
        { "DATAN",      new IntrDefinition("DATAN",   ArgCount.One,          D,            SymType.GENERIC,  SymType.DOUBLE)  },
        { "DATAN2",     new IntrDefinition("DATAN2",  ArgCount.Two,          D,            SymType.GENERIC,  SymType.DOUBLE)  },
        { "DBLE",       new IntrDefinition("DBLE",    ArgCount.One,          I|R|D|C|X,    SymType.GENERIC,  SymType.DOUBLE)  },
        { "DCOS",       new IntrDefinition("DCOS",    ArgCount.One,          D,            SymType.GENERIC,  SymType.DOUBLE)  },
        { "DCOSH",      new IntrDefinition("DCOSH",   ArgCount.One,          D,            SymType.GENERIC,  SymType.DOUBLE)  },
        { "DDIM",       new IntrDefinition("DDIM",    ArgCount.Two,          D,            SymType.GENERIC,  SymType.DOUBLE)  },
        { "DEXP",       new IntrDefinition("DEXP",    ArgCount.One,          D,            SymType.GENERIC,  SymType.DOUBLE)  },
        { "DIM",        new IntrDefinition("DIM",     ArgCount.Two,          I|R|D,        SymType.GENERIC,  SymType.GENERIC) },
        { "DINT",       new IntrDefinition("DINT",    ArgCount.One,          D,            SymType.GENERIC,  SymType.DOUBLE)  },
        { "DLOG",       new IntrDefinition("DLOG",    ArgCount.One,          D,            SymType.GENERIC,  SymType.DOUBLE)  },
        { "DLOG10",     new IntrDefinition("DLOG10",  ArgCount.One,          D,            SymType.GENERIC,  SymType.DOUBLE)  },
        { "DMAX1",      new IntrDefinition("DMAX1",   ArgCount.TwoOrMore,    D|X,          SymType.GENERIC,  SymType.DOUBLE) },
        { "DMIN1",      new IntrDefinition("DMIN1",   ArgCount.TwoOrMore,    D|X,          SymType.GENERIC,  SymType.DOUBLE) },
        { "DMOD",       new IntrDefinition("DMOD",    ArgCount.Two,          D,            SymType.GENERIC,  SymType.DOUBLE) },
        { "DNINT",      new IntrDefinition("DNINT",   ArgCount.One,          D,            SymType.GENERIC,  SymType.DOUBLE) },
        { "DPROD",      new IntrDefinition("DPROD",   ArgCount.Two,          R,            SymType.GENERIC,  SymType.DOUBLE) },
        { "DSIGN",      new IntrDefinition("DSIGN",   ArgCount.Two,          D,            SymType.GENERIC,  SymType.DOUBLE) },
        { "DSIN",       new IntrDefinition("DSIN",    ArgCount.One,          D,            SymType.GENERIC,  SymType.DOUBLE) },
        { "DSINH",      new IntrDefinition("DSINH",   ArgCount.One,          D,            SymType.GENERIC,  SymType.DOUBLE) },
        { "DSQRT",      new IntrDefinition("DSQRT",   ArgCount.One,          D,            SymType.GENERIC,  SymType.DOUBLE) },
        { "DTAN",       new IntrDefinition("DTAN",    ArgCount.One,          D,            SymType.GENERIC,  SymType.DOUBLE) },
        { "DTANH",      new IntrDefinition("DTANH",   ArgCount.One,          D,            SymType.GENERIC,  SymType.DOUBLE) },
        { "EXP",        new IntrDefinition("EXP",     ArgCount.One,          R|D|C,        SymType.GENERIC,  SymType.GENERIC) },
        { "FLOAT",      new IntrDefinition("FLOAT",   ArgCount.One,          I|X,          SymType.GENERIC,  SymType.FLOAT)   },
        { "IABS",       new IntrDefinition("IABS",    ArgCount.One,          I,            SymType.GENERIC,  SymType.INTEGER) },
        { "ICHAR",      new IntrDefinition("ICHAR",   ArgCount.One,          S|X,          SymType.GENERIC,  SymType.INTEGER) },
        { "IDIM",       new IntrDefinition("IDIM",    ArgCount.Two,          I,            SymType.GENERIC,  SymType.INTEGER) },
        { "IDINT",      new IntrDefinition("IDINT",   ArgCount.One,          D|X,          SymType.GENERIC,  SymType.INTEGER) },
        { "IDNINT",     new IntrDefinition("IDNINT",  ArgCount.One,          D,            SymType.GENERIC,  SymType.INTEGER) },
        { "IFIX",       new IntrDefinition("IFIX",    ArgCount.One,          R|X,          SymType.GENERIC,  SymType.INTEGER) },
        { "INDEX",      new IntrDefinition("INDEX",   ArgCount.Two,          S,            SymType.GENERIC,  SymType.INTEGER) },
        { "INT",        new IntrDefinition("INT",     ArgCount.One,          I|R|D|C|X,    SymType.GENERIC,  SymType.INTEGER) },
        { "ISIGN",      new IntrDefinition("ISIGN",   ArgCount.Two,          I,            SymType.GENERIC,  SymType.INTEGER) },
        { "LEN",        new IntrDefinition("SIZE",    ArgCount.One,          S,            SymType.GENERIC,  SymType.INTEGER) },
        { "LGE",        new IntrDefinition("LGE",     ArgCount.Two,          S|X,          SymType.GENERIC,  SymType.BOOLEAN) },
        { "LGT",        new IntrDefinition("LGT",     ArgCount.Two,          S|X,          SymType.GENERIC,  SymType.BOOLEAN) },
        { "LLE",        new IntrDefinition("LLE",     ArgCount.Two,          S|X,          SymType.GENERIC,  SymType.BOOLEAN) },
        { "LLT",        new IntrDefinition("LLT",     ArgCount.Two,          S|X,          SymType.GENERIC,  SymType.BOOLEAN) },
        { "LOG",        new IntrDefinition("LOG",     ArgCount.One,          R|D|C,        SymType.GENERIC,  SymType.GENERIC) },
        { "LOG10",      new IntrDefinition("LOG10",   ArgCount.One,          R|D,          SymType.GENERIC,  SymType.GENERIC) },
        { "MAX",        new IntrDefinition("MAX",     ArgCount.TwoOrMore,    I|R|D|X,      SymType.GENERIC,  SymType.GENERIC) },
        { "MAX0",       new IntrDefinition("MAX0",    ArgCount.TwoOrMore,    I|X,          SymType.GENERIC,  SymType.INTEGER) },
        { "MAX1",       new IntrDefinition("MAX1",    ArgCount.TwoOrMore,    R|X,          SymType.GENERIC,  SymType.INTEGER) },
        { "MIN",        new IntrDefinition("MIN",     ArgCount.TwoOrMore,    I|R|D|X,      SymType.GENERIC,  SymType.GENERIC) },
        { "MIN0",       new IntrDefinition("MIN0",    ArgCount.TwoOrMore,    I|X,          SymType.GENERIC,  SymType.INTEGER) },
        { "MIN1",       new IntrDefinition("MIN1",    ArgCount.TwoOrMore,    R|X,          SymType.GENERIC,  SymType.INTEGER) },
        { "MOD",        new IntrDefinition("MOD",     ArgCount.Two,          I|R|D,        SymType.GENERIC,  SymType.GENERIC) },
        { "NINT",       new IntrDefinition("NINT",    ArgCount.One,          R|D,          SymType.GENERIC,  SymType.INTEGER) },
        { "RAND",       new IntrDefinition("RAND",    ArgCount.One,          I,            SymType.GENERIC,  SymType.DOUBLE)  },
        { "REAL",       new IntrDefinition("REAL",    ArgCount.One,          I|R|D|C|X,    SymType.GENERIC,  SymType.FLOAT)   },
        { "SIGN",       new IntrDefinition("SIGN",    ArgCount.Two,          I|R|D,        SymType.GENERIC,  SymType.GENERIC) },
        { "SIN",        new IntrDefinition("SIN",     ArgCount.One,          R|D|C,        SymType.GENERIC,  SymType.GENERIC) },
        { "SINH",       new IntrDefinition("SINH",    ArgCount.One,          R|D,          SymType.GENERIC,  SymType.GENERIC) },
        { "SNGL",       new IntrDefinition("SNGL",    ArgCount.One,          D|X,          SymType.GENERIC,  SymType.FLOAT)   },
        { "SQRT",       new IntrDefinition("SQRT",    ArgCount.One,          R|D|C,        SymType.GENERIC,  SymType.GENERIC) },
        { "TAN",        new IntrDefinition("TAN",     ArgCount.One,          R|D,          SymType.GENERIC,  SymType.GENERIC) },
        { "TANH",       new IntrDefinition("TANH",    ArgCount.One,          R|D,          SymType.GENERIC,  SymType.GENERIC) }
    };
    
    /// <summary>
    /// Return whether the specified name is an intrinsic function.
    /// </summary>
    /// <returns>True if the name is an intrinsic function</returns>
    /// <param name="name">A name to check</param>
    public static bool IsIntrinsic(string name) {
        if (name == null) {
            throw new ArgumentNullException(nameof(name));
        }
        return _intrinsics.ContainsKey(name.ToUpper());
    }
    
    /// <summary>
    /// Return the intrinsic definition for the given intrinsic name.
    /// </summary>
    /// <returns>An IntrDefinition object or null</returns>
    /// <param name="name">The intrinsic name</param>
    public static IntrDefinition IntrinsicDefinition(string name) {
        if (name == null) {
            throw new ArgumentNullException(nameof(name));
        }
        if (_intrinsics.TryGetValue(name.ToUpper(), out IntrDefinition intrDefinition)) {
            return intrDefinition;
        }
        return null;
    }
}