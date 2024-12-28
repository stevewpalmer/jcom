// JCom Compiler Toolkit
// Emitter for MSIL
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
using System.Reflection;
using System.Reflection.Emit;
using JComLib;

namespace CCompiler;

/// <summary>
/// Defines an Instruction class for a Try/Catch block.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
public class InstructionTryCatch : Instruction {

    /// <summary>
    /// Creates an InstructionTryCatch object to represent an
    /// exception handler.
    /// </summary>
    /// <param name="type">Type of this block</param>
    public InstructionTryCatch(EmitExceptionHandlerType type) {
        TryCatchType = type;
    }

    /// <summary>
    /// Creates an InstructionTryCatch object to represent an
    /// exception handler.
    /// </summary>
    /// <param name="type">Type of this block</param>
    /// <param name="err">Optional error value symbol</param>
    /// <param name="errText">Optional error text symbol</param>
    public InstructionTryCatch(EmitExceptionHandlerType type, Symbol err, Symbol errText) {
        TryCatchType = type;
        Err = err;
        ErrText = errText;
    }

    /// <summary>
    /// Get or set the type of this block.
    /// </summary>
    public EmitExceptionHandlerType TryCatchType { get; set; }

    /// <summary>
    /// Symbol to which exception value is saved
    /// </summary>
    public Symbol Err { get; set; }

    /// <summary>
    /// Symbol to which exception message is saved
    /// </summary>
    public Symbol ErrText { get; set; }

    /// <summary>
    /// Generate MSIL code to emit an instruction marker at the current
    /// sequence in the output.
    /// </summary>
    /// <param name="il">ILGenerator object</param>
    public override void Generate(ILGenerator il) {
        ArgumentNullException.ThrowIfNull(il);
        if (Deleted) {
            return;
        }
        switch (TryCatchType) {
            case EmitExceptionHandlerType.Try:
                il.BeginExceptionBlock();
                break;

            case EmitExceptionHandlerType.Catch: {

                // This catch handler is used by the try...catch logic in the program.
                Type runtimeException = typeof(Exception);
                Type jcomRuntimeException = typeof(JComRuntimeException);

                il.BeginCatchBlock(runtimeException);

                LocalBuilder tmp1 = il.DeclareLocal(jcomRuntimeException);
                MethodInfo methodInfo = jcomRuntimeException.GetMethod("GeneralHandler", [typeof(Exception)]);
                il.EmitCall(OpCodes.Call, methodInfo, null);

                il.Emit(OpCodes.Stloc_S, tmp1);

                if (Err != null || ErrText != null) {
                    if (Err is { IsReferenced: true }) {
                        il.Emit(OpCodes.Ldloc_S, tmp1);
                        il.EmitCall(OpCodes.Call, jcomRuntimeException.GetMethod("get_ErrorCode"), null);
                        il.Emit(OpCodes.Stsfld, (FieldInfo)Err.Info);
                    }
                    if (ErrText is { IsReferenced: true }) {
                        il.Emit(OpCodes.Ldloc_S, tmp1);
                        il.EmitCall(OpCodes.Callvirt, jcomRuntimeException.GetMethod("get_Message"), null);
                        il.Emit(OpCodes.Stsfld, (FieldInfo)ErrText.Info);
                    }
                }
                break;
            }

            case EmitExceptionHandlerType.EndCatch:
                il.EndExceptionBlock();
                break;

            case EmitExceptionHandlerType.DefaultCatch: {

                // The default catch is the top-level exception handler around which we wrap
                // the entire application.
                Type runtimeException = typeof(Exception);
                Type jcomRuntimeException = typeof(JComRuntimeException);

                il.BeginCatchBlock(runtimeException);

                LocalBuilder tmp1 = il.DeclareLocal(jcomRuntimeException);
                MethodInfo methodInfo = jcomRuntimeException.GetMethod("GeneralHandlerNoThrow", [typeof(Exception)]);
                il.EmitCall(OpCodes.Call, methodInfo, null);

                il.Emit(OpCodes.Stloc_S, tmp1);

                Label skipMessage = il.DefineLabel();

                il.Emit(OpCodes.Ldloc_S, tmp1);
                il.EmitCall(OpCodes.Call, jcomRuntimeException.GetMethod("get_Type"), null);
                il.Emit(OpCodes.Ldc_I4, (int)JComRuntimeExceptionType.END);

                il.Emit(OpCodes.Ldloc_S, tmp1);
                il.EmitCall(OpCodes.Callvirt, jcomRuntimeException.GetMethod("get_Message"), null);

                MethodInfo meth = typeof(Console).GetMethod("WriteLine", [typeof(string)]);
                il.EmitCall(OpCodes.Call, meth, null);
                il.MarkLabel(skipMessage);
                break;
            }
        }
    }
}