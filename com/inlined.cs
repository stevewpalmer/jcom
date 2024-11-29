// JCom Compiler Toolkit
// Inline functions
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

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Reflection.Emit;
using JComLib;
using JFortranLib;

namespace CCompiler;

/// <summary>
/// Inline-able functions. Each function generates the code that implements
/// itself as inlined code.
///
/// Where typeWanted is specified, the function must ensure that the value
/// on top of the stack is of that given type.
///
/// Note that the compiler has an option to disable inlining functions so
/// all functions here must have an instrinsic equivalent in JComLib that
/// will be used if inlining is disabled. Unit tests for functions will
/// generally test both methods to verify this is always the case.
///
/// </summary>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static class Inlined {

    /// <summary>
    /// Returns the absolute of the given value.
    /// </summary>
    /// <param name="em">Emitter object</param>
    /// <param name="typeWanted">The type of the argument</param>
    public static void ABS(Emitter em, Type typeWanted) {
        ArgumentNullException.ThrowIfNull(em);
        em.Emit0(OpCodes.Call, typeWanted == typeof(Complex) ?
            typeof(Complex).GetMethod("Abs", [typeWanted]) :
            typeof(Math).GetMethod("Abs", [typeWanted]));
    }

    /// <summary>
    /// Arc-cosine; the result is in the range 0 to + π.
    /// </summary>
    /// <param name="em">Emitter object</param>
    public static void ACOS(Emitter em) {
        ArgumentNullException.ThrowIfNull(em);
        em.Emit0(OpCodes.Call, typeof(Math).GetMethod("Acos", [typeof(double)]));
    }

    /// <summary>
    /// Converts to integer by truncation.
    /// </summary>
    /// <param name="em">Emitter object</param>
    /// <param name="typeWanted">The type of the argument</param>
    public static void AINT(Emitter em, Type typeWanted) {
        ArgumentNullException.ThrowIfNull(em);
        em.Emit0(OpCodes.Conv_I4);
        em.ConvertSystemType(typeWanted);
    }

    /// <summary>
    /// Logarithm to base e (where e=2.718...).
    /// Generic real type version.
    /// </summary>
    /// <param name="em">Emitter object</param>
    public static void ALOG(Emitter em) {
        ArgumentNullException.ThrowIfNull(em);
        em.Emit0(OpCodes.Call, typeof(Math).GetMethod("Log", [typeof(double)]));
    }

    /// <summary>
    /// Logarithm to base 10.
    /// Generic real type version.
    /// </summary>
    /// <param name="em">Emitter object</param>
    public static void ALOG10(Emitter em) {
        ArgumentNullException.ThrowIfNull(em);
        em.Emit0(OpCodes.Call, typeof(Math).GetMethod("Log10", [typeof(double)]));
    }

    /// <summary>
    /// AMOD intrinsic function.
    /// Generic real type version.
    /// </summary>
    /// <param name="em">Emitter object</param>
    public static void AMOD(Emitter em) {
        ArgumentNullException.ThrowIfNull(em);
        em.Emit0(OpCodes.Rem);
    }

    /// <summary>
    /// Returns the angle whose sine is the specified value.
    /// </summary>
    /// <param name="em">Emitter object</param>
    public static void ASIN(Emitter em) {
        ArgumentNullException.ThrowIfNull(em);
        em.Emit0(OpCodes.Call, typeof(Math).GetMethod("Asin", [typeof(double)]));
    }

    /// <summary>
    /// Returns the angle whose tangent is the specified value.
    /// </summary>
    /// <param name="em">Emitter object</param>
    /// <param name="typeWanted">The type of the argument</param>
    public static void ATAN(Emitter em, Type typeWanted) {
        ArgumentNullException.ThrowIfNull(em);
        if (typeWanted != typeof(double)) {
            em.Emit0(OpCodes.Conv_R8);
        }
        em.Emit0(OpCodes.Call, typeof(Math).GetMethod("Atan", [typeof(double)]));
        if (typeWanted == typeof(float)) {
            em.Emit0(OpCodes.Conv_R4);
        }
    }

    /// <summary>
    /// Returns the absolute of the given value.
    /// </summary>
    /// <param name="em">Emitter object</param>
    public static void CABS(Emitter em) {
        ArgumentNullException.ThrowIfNull(em);
        em.Emit0(OpCodes.Call, typeof(Complex).GetMethod("Abs", [typeof(Complex)]));
    }

    /// <summary>
    /// Cosine of the angle in radians.
    /// </summary>
    /// <param name="em">Emitter object</param>
    public static void CCOS(Emitter em) {
        ArgumentNullException.ThrowIfNull(em);
        em.Emit0(OpCodes.Call, typeof(Complex).GetMethod("Cos", [typeof(Complex)]));
    }

    /// <summary>
    /// Returns Nth character in local character code table.
    /// </summary>
    /// <param name="em">Emitter object</param>
    public static void CHAR(Emitter em) {
        ArgumentNullException.ThrowIfNull(em);
        em.Emit0(OpCodes.Conv_U2);
        em.CreateObject(typeof(FixedString), [typeof(char)]);
    }

    /// <summary>
    /// Logarithm to base e (where e=2.718...)..
    /// </summary>
    /// <param name="em">Emitter object</param>
    public static void CLOG(Emitter em) {
        ArgumentNullException.ThrowIfNull(em);
        em.Emit0(OpCodes.Call, typeof(Complex).GetMethod("Log", [typeof(Complex)]));
    }

    /// <summary>
    /// Convert the specified value to a complex value.
    /// </summary>
    /// <param name="em">Emitter object</param>
    public static void CMPLX(Emitter em) {
        ArgumentNullException.ThrowIfNull(em);
        em.Emit0(OpCodes.Newobj, typeof(Complex).GetConstructor([typeof(double), typeof(double)]));
    }

    /// <summary>
    /// Cosine of the angle in radians.
    /// </summary>
    /// <param name="em">Emitter object</param>
    /// <param name="typeWanted">The type of the argument</param>
    public static void COS(Emitter em, Type typeWanted) {
        ArgumentNullException.ThrowIfNull(em);
        if (typeWanted == typeof(Complex)) {
            em.Emit0(OpCodes.Call, typeof(Complex).GetMethod("Cos", [typeWanted]));
        }
        else {
            if (typeWanted == typeof(float)) {
                em.Emit0(OpCodes.Conv_R8);
            }
            em.Emit0(OpCodes.Call, typeof(Math).GetMethod("Cos", [typeof(double)]));
            if (typeWanted == typeof(float)) {
                em.Emit0(OpCodes.Conv_R4);
            }
        }
    }

    /// <summary>
    /// Hyperbolic cosine of the angle in radians.
    /// </summary>
    /// <param name="em">Emitter object</param>
    public static void COSH(Emitter em) {
        ArgumentNullException.ThrowIfNull(em);
        em.Emit0(OpCodes.Call, typeof(Math).GetMethod("Cosh", [typeof(double)]));
    }

    /// <summary>
    /// Sine of the angle in radians.
    /// </summary>
    /// <param name="em">Emitter object</param>
    public static void CSIN(Emitter em) {
        ArgumentNullException.ThrowIfNull(em);
        em.Emit0(OpCodes.Call, typeof(Complex).GetMethod("Sin", [typeof(Complex)]));
    }

    /// <summary>
    /// SQRT intrinsic function
    /// Returns the square root of the given value.
    /// </summary>
    /// <param name="em">Emitter object</param>
    public static void CSQRT(Emitter em) {
        ArgumentNullException.ThrowIfNull(em);
        em.Emit0(OpCodes.Call, typeof(Complex).GetMethod("Sqrt", [typeof(Complex)]));
    }

    /// <summary>
    /// Returns the absolute of the given value.
    /// </summary>
    /// <param name="em">Emitter object</param>
    public static void DABS(Emitter em) {
        ArgumentNullException.ThrowIfNull(em);
        em.Emit0(OpCodes.Call, typeof(Math).GetMethod("Abs", [typeof(double)]));
    }

    /// <summary>
    /// Arc-cosine; the result is in the range 0 to + π.
    /// </summary>
    /// <param name="em">Emitter object</param>
    public static void DACOS(Emitter em) {
        ArgumentNullException.ThrowIfNull(em);
        em.Emit0(OpCodes.Call, typeof(Math).GetMethod("Acos", [typeof(double)]));
    }

    /// <summary>
    /// Arc-sine; the result is in the range 0 to + π.
    /// </summary>
    /// <param name="em">Emitter object</param>
    public static void DASIN(Emitter em) {
        ArgumentNullException.ThrowIfNull(em);
        em.Emit0(OpCodes.Call, typeof(Math).GetMethod("Asin", [typeof(double)]));
    }

    /// <summary>
    /// Returns the angle whose tangent is the specified value.
    /// </summary>
    /// <param name="em">Emitter object</param>
    public static void DATAN(Emitter em) {
        ArgumentNullException.ThrowIfNull(em);
        em.Emit0(OpCodes.Call, typeof(Math).GetMethod("Atan", [typeof(double)]));
    }

    /// <summary>
    /// Returns the angle whose tangent is the quotient of m1 and m2.
    /// </summary>
    /// <param name="em">Emitter object</param>
    public static void DATAN2(Emitter em) {
        ArgumentNullException.ThrowIfNull(em);
        em.Emit0(OpCodes.Call, typeof(Math).GetMethod("Atan2", [typeof(double), typeof(double)]));
    }

    /// <summary>
    /// Emit inline code for DBLE function. For Complex this is a bit hacky and we redirect
    /// to the JComLib library to get the actual Real since this requires access to a property
    /// which is a reference access. Maybe fix this at some point.
    /// </summary>
    /// <param name="em">Emitter object</param>
    /// <param name="typeWanted">The type of the argument</param>
    public static void DBLE(Emitter em, Type typeWanted) {
        ArgumentNullException.ThrowIfNull(em);
        if (typeWanted == typeof(Complex)) {
            em.Emit0(OpCodes.Call, typeof(Intrinsics).GetMethod("DBLE", [typeof(Complex)]));
        }
        else {
            em.Emit0(OpCodes.Conv_R8);
        }
    }

    /// <summary>
    /// Cosine of the angle in radians.
    /// </summary>
    /// <param name="em">Emitter object</param>
    public static void DCOS(Emitter em) {
        ArgumentNullException.ThrowIfNull(em);
        em.Emit0(OpCodes.Call, typeof(Math).GetMethod("Cos", [typeof(double)]));
    }

    /// <summary>
    /// Hyperbolic cosine of the specified angle in radians.
    /// </summary>
    /// <param name="em">Emitter object</param>
    public static void DCOSH(Emitter em) {
        ArgumentNullException.ThrowIfNull(em);
        em.Emit0(OpCodes.Call, typeof(Math).GetMethod("Cosh", [typeof(double)]));
    }

    /// <summary>
    /// Convert radians to degrees.  This is computed as radians * (180/PI)
    /// </summary>
    /// <param name="em">Emitter object</param>
    public static void DEG(Emitter em) {
        ArgumentNullException.ThrowIfNull(em);
        em.LoadDouble(180 / Math.PI);
        em.Mul(SymType.DOUBLE);
    }

    /// <summary>
    /// Returns the exponential, i.e. e to the power of the argument. This is the inverse of the natural logarithm.
    /// </summary>
    /// <param name="em">Emitter object</param>
    public static void DEXP(Emitter em) {
        ArgumentNullException.ThrowIfNull(em);
        em.Emit0(OpCodes.Call, typeof(Math).GetMethod("Exp", [typeof(double)]));
    }

    /// <summary>
    /// Converts to integer by truncation.
    /// </summary>
    /// <param name="em">Emitter object</param>
    public static void DINT(Emitter em) {
        ArgumentNullException.ThrowIfNull(em);
        em.Emit0(OpCodes.Conv_I4);
        em.Emit0(OpCodes.Conv_R8);
    }

    /// <summary>
    /// Logarithm to base e (where e=2.718...)..
    /// </summary>
    /// <param name="em">Emitter object</param>
    public static void DLOG(Emitter em) {
        ArgumentNullException.ThrowIfNull(em);
        em.Emit0(OpCodes.Call, typeof(Math).GetMethod("Log", [typeof(double)]));
    }

    /// <summary>
    /// Logarithm to base 10.
    /// </summary>
    /// <param name="em">Emitter object</param>
    public static void DLOG10(Emitter em) {
        ArgumentNullException.ThrowIfNull(em);
        em.Emit0(OpCodes.Call, typeof(Math).GetMethod("Log10", [typeof(double)]));
    }

    /// <summary>
    /// DMOD intrinsic function.
    /// Generic double type version.
    /// </summary>
    /// <param name="em">Emitter object</param>
    public static void DMOD(Emitter em) {
        ArgumentNullException.ThrowIfNull(em);
        em.Emit0(OpCodes.Rem);
    }

    /// <summary>
    /// Emit inline code for DPROD function.
    /// </summary>
    /// <param name="em">Emitter object</param>
    public static void DPROD(Emitter em) {
        ArgumentNullException.ThrowIfNull(em);
        em.Emit0(OpCodes.Mul);
    }

    /// <summary>
    /// Sine of the angle in radians.
    /// </summary>
    /// <param name="em">Emitter object</param>
    public static void DSIN(Emitter em) {
        ArgumentNullException.ThrowIfNull(em);
        em.Emit0(OpCodes.Call, typeof(Math).GetMethod("Sin", [typeof(double)]));
    }

    /// <summary>
    /// Returns the hyperbolic sine of the angle in radians.
    /// </summary>
    /// <param name="em">Emitter object</param>
    public static void DSINH(Emitter em) {
        ArgumentNullException.ThrowIfNull(em);
        em.Emit0(OpCodes.Call, typeof(Math).GetMethod("Sinh", [typeof(double)]));
    }

    /// <summary>
    /// SQRT intrinsic function
    /// Returns the square root of the given value.
    /// </summary>
    /// <param name="em">Emitter object</param>
    public static void DSQRT(Emitter em) {
        ArgumentNullException.ThrowIfNull(em);
        em.Emit0(OpCodes.Call, typeof(Math).GetMethod("Sqrt", [typeof(double)]));
    }

    /// <summary>
    /// Tangent of the angle in radians.
    /// </summary>
    /// <param name="em">Emitter object</param>
    public static void DTAN(Emitter em) {
        ArgumentNullException.ThrowIfNull(em);
        em.Emit0(OpCodes.Call, typeof(Math).GetMethod("Tan", [typeof(double)]));
    }

    /// <summary>
    /// Returns the hyperbolic tangent of the specified angle.
    /// </summary>
    /// <param name="em">Emitter object</param>
    public static void DTANH(Emitter em) {
        ArgumentNullException.ThrowIfNull(em);
        em.Emit0(OpCodes.Call, typeof(Math).GetMethod("Tanh", [typeof(double)]));
    }

    /// <summary>
    /// Returns the exponential, i.e. e to the power of the argument. This is the
    /// inverse of the natural logarithm.
    /// </summary>
    /// <param name="em">Emitter object</param>
    /// <param name="typeWanted">The type of the argument</param>
    public static void EXP(Emitter em, Type typeWanted) {
        ArgumentNullException.ThrowIfNull(em);
        if (typeWanted == typeof(Complex)) {
            em.Emit0(OpCodes.Call, typeof(Complex).GetMethod("Exp", [typeof(Complex)]));
        }
        else {
            if (typeWanted != typeof(double)) {
                em.Emit0(OpCodes.Conv_R8);
            }
            em.Emit0(OpCodes.Call, typeof(Math).GetMethod("Exp", [typeof(double)]));
            if (typeWanted == typeof(float)) {
                em.Emit0(OpCodes.Conv_R4);
            }
        }
    }

    /// <summary>
    /// Converts an integer to a REAL type.
    /// </summary>
    /// <param name="em">Emitter object</param>
    public static void FLOAT(Emitter em) {
        ArgumentNullException.ThrowIfNull(em);
        em.Emit0(OpCodes.Conv_R4);
    }

    /// <summary>
    /// Returns the absolute of the given value.
    /// </summary>
    /// <param name="em">Emitter object</param>
    public static void IABS(Emitter em) {
        ArgumentNullException.ThrowIfNull(em);
        em.Emit0(OpCodes.Conv_R8);
        em.Emit0(OpCodes.Call, typeof(Math).GetMethod("Abs", [typeof(double)]));
        em.Emit0(OpCodes.Conv_I4);
    }

    /// <summary>
    /// Returns position of first character of the string in the local character code table.
    /// </summary>
    /// <param name="em">Emitter object</param>
    /// <param name="typeWanted">The type of the argument</param>
    public static void ICHAR(Emitter em, Type typeWanted) {
        ArgumentNullException.ThrowIfNull(em);
        ArgumentNullException.ThrowIfNull(typeWanted);
        em.LoadInteger(0);
        em.Emit0(OpCodes.Call, typeWanted.GetMethod("get_Chars", [typeof(int)]));
        if (typeWanted == typeof(float)) {
            em.Emit0(OpCodes.Conv_R4);
        }
    }

    /// <summary>
    /// Converts to integer by truncation.
    /// </summary>
    /// <param name="em">Emitter object</param>
    public static void IDINT(Emitter em) {
        ArgumentNullException.ThrowIfNull(em);
        em.Emit0(OpCodes.Conv_I4);
    }

    /// <summary>
    /// Returns the integer value of the given float.
    /// </summary>
    /// <param name="em">Emitter object</param>
    public static void IFIX(Emitter em) {
        ArgumentNullException.ThrowIfNull(em);
        em.Emit0(OpCodes.Conv_I4);
    }

    /// <summary>
    /// Emit inline code for INT function.
    /// </summary>
    /// <param name="em">Emitter object</param>
    /// <param name="typeWanted">The type of the argument</param>
    public static void INT(Emitter em, Type typeWanted) {
        ArgumentNullException.ThrowIfNull(em);
        if (typeWanted == typeof(Complex)) {
            em.Emit0(OpCodes.Call, typeof(Intrinsics).GetMethod("INT", [typeof(Complex)]));
        }
        else {
            em.Emit0(OpCodes.Conv_I4);
        }
    }

    /// <summary>
    /// Rounds the specified value down to the smallest integral number
    /// </summary>
    /// <param name="em">Emitter object</param>
    /// <param name="typeWanted">The type of the argument</param>
    public static void FLOOR(Emitter em, Type typeWanted) {
        ArgumentNullException.ThrowIfNull(em);
        ArgumentNullException.ThrowIfNull(typeWanted);
        if (typeWanted != typeof(double)) {
            em.Emit0(OpCodes.Conv_R8);
        }
        em.Emit0(OpCodes.Call, typeof(Math).GetMethod("Floor", [typeof(double)]));
        if (typeWanted == typeof(float)) {
            em.Emit0(OpCodes.Conv_R4);
        }
    }

    /// <summary>
    /// Returns length of the character argument.
    /// </summary>
    /// <param name="em">Emitter object</param>
    /// <param name="typeWanted">The type of the argument</param>
    public static void LEN(Emitter em, Type typeWanted) {
        ArgumentNullException.ThrowIfNull(em);
        ArgumentNullException.ThrowIfNull(typeWanted);
        if (typeWanted == typeof(FixedString)) {
            em.Emit0(OpCodes.Call, typeWanted.GetMethod("get_RealLength"));
        }
        if (typeWanted == typeof(string)) {
            em.Emit0(OpCodes.Call, typeWanted.GetMethod("get_Length"));
        }
    }

    /// <summary>
    /// Logarithm to base e (where e=2.718...)..
    /// </summary>
    /// <param name="em">Emitter object</param>
    /// <param name="typeWanted">The type of the argument</param>
    public static void LOG(Emitter em, Type typeWanted) {
        ArgumentNullException.ThrowIfNull(em);
        if (typeWanted == typeof(Complex)) {
            em.Emit0(OpCodes.Call, typeof(Complex).GetMethod("Log", [typeWanted]));
        }
        else {
            if (typeWanted != typeof(double)) {
                em.Emit0(OpCodes.Conv_R8);
            }
            em.Emit0(OpCodes.Call, typeof(Math).GetMethod("Log", [typeof(double)]));
            if (typeWanted == typeof(float)) {
                em.Emit0(OpCodes.Conv_R4);
            }
        }
    }

    /// <summary>
    /// Logarithm to base 10.
    /// </summary>
    /// <param name="em">Emitter object</param>
    /// <param name="typeWanted">The type of the argument</param>
    public static void LOG10(Emitter em, Type typeWanted) {
        ArgumentNullException.ThrowIfNull(em);
        if (typeWanted != typeof(double)) {
            em.Emit0(OpCodes.Conv_R8);
        }
        em.Emit0(OpCodes.Call, typeof(Math).GetMethod("Log10", [typeof(double)]));
        if (typeWanted == typeof(float)) {
            em.Emit0(OpCodes.Conv_R4);
        }
    }

    /// <summary>
    /// Inline generator for MOD function.
    /// </summary>
    /// <param name="em">Emitter object</param>
    public static void MOD(Emitter em) {
        ArgumentNullException.ThrowIfNull(em);
        em.Emit0(OpCodes.Rem);
    }

    /// <summary>
    /// Convert degrees to radians. This is computed as degrees * (PI/180)
    /// </summary>
    /// <param name="em">Emitter object</param>
    public static void RAD(Emitter em) {
        ArgumentNullException.ThrowIfNull(em);
        em.LoadDouble(Math.PI / 180.0);
        em.Mul(SymType.DOUBLE);
    }

    /// <summary>
    /// Emit inline code for REAL function.
    /// </summary>
    /// <param name="em">Emitter object</param>
    /// <param name="typeWanted">The type of the argument</param>
    public static void REAL(Emitter em, Type typeWanted) {
        ArgumentNullException.ThrowIfNull(em);
        if (typeWanted == typeof(Complex)) {
            em.Emit0(OpCodes.Call, typeof(Intrinsics).GetMethod("REAL", [typeof(Complex)]));
        }
        else {
            em.Emit0(OpCodes.Conv_R4);
        }
    }

    /// <summary>
    /// Emit inline code for REPORT.
    /// </summary>
    /// <param name="em">Emitter object</param>
    public static void REPORT(Emitter em) {
        ArgumentNullException.ThrowIfNull(em);
        em.CreateObject(typeof(JComRuntimeException), [typeof(JComRuntimeErrors)]);
        em.Emit0(OpCodes.Throw);
    }

    /// <summary>
    /// Emit inline code for SGN function.
    /// </summary>
    /// <param name="em">Emitter object</param>
    /// <param name="typeWanted">The type of the argument</param>
    public static void SGN(Emitter em, Type typeWanted) {
        ArgumentNullException.ThrowIfNull(em);
        em.Emit0(OpCodes.Call, typeof(Math).GetMethod("Sign", [typeWanted]));
    }

    /// <summary>
    /// Sine of the angle in radians.
    /// </summary>
    /// <param name="em">Emitter object</param>
    /// <param name="typeWanted">The type of the argument</param>
    public static void SIN(Emitter em, Type typeWanted) {
        ArgumentNullException.ThrowIfNull(em);
        if (typeWanted == typeof(Complex)) {
            em.Emit0(OpCodes.Call, typeof(Complex).GetMethod("Sin", [typeWanted]));
        }
        else {
            if (typeWanted != typeof(double)) {
                em.Emit0(OpCodes.Conv_R8);
            }
            em.Emit0(OpCodes.Call, typeof(Math).GetMethod("Sin", [typeof(double)]));
            if (typeWanted == typeof(float)) {
                em.Emit0(OpCodes.Conv_R4);
            }
        }
    }

    /// <summary>
    /// Returns the hyperbolic sine of the angle in radians.
    /// </summary>
    /// <param name="em">Emitter object</param>
    public static void SINH(Emitter em) {
        ArgumentNullException.ThrowIfNull(em);
        em.Emit0(OpCodes.Call, typeof(Math).GetMethod("Sinh", [typeof(double)]));
    }

    /// <summary>
    /// Returns length of the character argument.
    /// </summary>
    /// <param name="em">Emitter object</param>
    /// <param name="typeWanted">The type of the argument</param>
    public static void SIZE(Emitter em, Type typeWanted) {
        ArgumentNullException.ThrowIfNull(em);
        ArgumentNullException.ThrowIfNull(typeWanted);
        em.Emit0(OpCodes.Call, typeWanted.GetMethod("get_Length"));
    }

    /// <summary>
    /// Emit inline code for SNGL function.
    /// </summary>
    /// <param name="em">Emitter object</param>
    public static void SNGL(Emitter em) {
        ArgumentNullException.ThrowIfNull(em);
        em.Emit0(OpCodes.Conv_R4);
    }

    /// <summary>
    /// Emit inline code for the ToString function.
    /// </summary>
    /// <param name="em">Emitter object</param>
    /// <param name="typeWanted">The type of the argument</param>
    public static void STR(Emitter em, Type typeWanted) {
        ArgumentNullException.ThrowIfNull(em);
        em.Emit0(OpCodes.Call, typeof(Convert).GetMethod("ToString", [typeWanted]));
    }

    /// <summary>
    /// SQRT intrinsic function
    /// Returns the square root of the given value.
    /// </summary>
    /// <param name="em">Emitter object</param>
    /// <param name="typeWanted">The type of the argument</param>
    public static void SQRT(Emitter em, Type typeWanted) {
        ArgumentNullException.ThrowIfNull(em);
        if (typeWanted == typeof(Complex)) {
            em.Emit0(OpCodes.Call, typeof(Complex).GetMethod("Sqrt", [typeWanted]));
        }
        else {
            if (typeWanted != typeof(double)) {
                em.Emit0(OpCodes.Conv_R8);
            }
            em.Emit0(OpCodes.Call, typeof(Math).GetMethod("Sqrt", [typeof(double)]));
            if (typeWanted == typeof(float)) {
                em.Emit0(OpCodes.Conv_R4);
            }
        }
    }

    /// <summary>
    /// Tangent of the angle in radians.
    /// </summary>
    /// <param name="em">Emitter object</param>
    /// <param name="typeWanted">The type of the argument</param>
    public static void TAN(Emitter em, Type typeWanted) {
        ArgumentNullException.ThrowIfNull(em);
        if (typeWanted != typeof(double)) {
            em.Emit0(OpCodes.Conv_R8);
        }
        em.Emit0(OpCodes.Call, typeof(Math).GetMethod("Tan", [typeof(double)]));
        if (typeWanted == typeof(float)) {
            em.Emit0(OpCodes.Conv_R4);
        }
    }

    /// <summary>
    /// Returns the hyperbolic tangent of the specified angle.
    /// </summary>
    /// <param name="em">Emitter object</param>
    public static void TANH(Emitter em) {
        ArgumentNullException.ThrowIfNull(em);
        em.Emit0(OpCodes.Call, typeof(Math).GetMethod("Tanh", [typeof(double)]));
    }

    /// <summary>
    /// Convert a string to a float.
    /// </summary>
    /// <param name="em">Emitter object</param>
    public static void VAL(Emitter em) {
        ArgumentNullException.ThrowIfNull(em);
        em.Emit0(OpCodes.Call, typeof(Convert).GetMethod("ToSingle", [typeof(string)]));
    }
}