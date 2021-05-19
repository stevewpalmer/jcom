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

using System;
using System.Diagnostics.SymbolStore;
using System.Reflection;
using System.Reflection.Emit;
using JComLib;

namespace CCompiler {

    /// <summary>
    /// Defines a single Instruction base class that constructs an opcode that
    /// takes no parameters.
    /// </summary>
    public class Instruction {
        
        /// <summary>
        /// Create an Instruction object with the given opcode.
        /// </summary>
        /// <param name="op">An OpCode</param>
        public Instruction(OpCode op) {
            Code = op;
        }
        
        /// <summary>
        /// Create an Instruction object that takes no opcode.
        /// </summary>
        public Instruction() {}

        /// <summary>
        /// Generate MSIL code to emit a opcode that takes no parameters.
        /// </summary>
        /// <param name="il">ILGenerator object</param>
        public virtual void Generate(ILGenerator il) {
            if (il == null) {
                throw new ArgumentNullException(nameof(il));
            }
            if (Deleted) {
                return;
            }
            il.Emit(Code);
        }

        /// <summary>
        /// Get or set the instruction opcode.
        /// </summary>
        public OpCode Code { get; set; }

        /// <summary>
        /// Gets or sets a flag which indicates whether or not this
        /// instruction should be omitted.
        /// </summary>
        public bool Deleted { get; set; }
    }

    /// <summary>
    /// Defines an instruction class that constructs an opcode that takes
    /// a single label parameter.
    /// </summary>
    public class InstructionLabel : Instruction {

        /// <summary>
        /// Target of label.
        /// </summary>
        public Label Target { get; set; }

        /// <summary>
        /// Create an InstructionLabel object with the given opcode
        /// and label target.
        /// </summary>
        /// <param name="op">Opcode</param>
        /// <param name="target">Label target</param>
        public InstructionLabel(OpCode op, Label target) : base(op) {
            Target = target;
        }

        /// <summary>
        /// Generate MSIL code to emit a opcode with a label parameter.
        /// </summary>
        /// <param name="il">ILGenerator object</param>
        public override void Generate(ILGenerator il) {
            if (il == null) {
                throw new ArgumentNullException(nameof(il));
            }
            if (Deleted) {
                return;
            }
            il.Emit(Code, Target);
        }
    }

    /// <summary>
    /// Defines an instruction class that represents a branch
    /// destination point in the instruction sequence.
    /// </summary>
    public class InstructionLabelMarker : Instruction {

        /// <summary>
        /// Target of label.
        /// </summary>
        public Label Target { get; set; }

        /// <summary>
        /// Create an InstructionLabelMarker object with the specified
        /// label target.
        /// </summary>
        /// <param name="target">Label target</param>
        public InstructionLabelMarker(Label target) {
            Target = target;
        }
        
        /// <summary>
        /// Generate MSIL code to emit a label marker.
        /// </summary>
        /// <param name="il">ILGenerator object</param>
        public override void Generate(ILGenerator il) {
            if (il == null) {
                throw new ArgumentNullException(nameof(il));
            }
            if (Deleted) {
                return;
            }
            il.MarkLabel(Target);
        }
    }

    /// <summary>
    /// Defines an instruction class that constructs an opcode that
    /// takes a byte parameter.
    /// </summary>
    public class InstructionByte : Instruction {
        private readonly byte _offset;
        
        /// <summary>
        /// Create an InstructionByte object with the given opcode
        /// and byte offset.
        /// </summary>
        /// <param name="op">Opcode</param>
        /// <param name="offset">A byte offset value</param>
        public InstructionByte(OpCode op, byte offset) : base(op) {
            _offset = offset;
        }
        
        /// <summary>
        /// Generate MSIL code to emit a opcode that takes a byte parameter.
        /// </summary>
        /// <param name="il">ILGenerator object</param>
        public override void Generate(ILGenerator il) {
            if (il == null) {
                throw new ArgumentNullException(nameof(il));
            }
            if (Deleted) {
                return;
            }
            il.Emit(Code, _offset);
        }
    }

    /// <summary>
    /// Defines an instruction class that constructs an opcode that
    /// takes a MethodInfo parameter.
    /// </summary>
    public class InstructionMethod : Instruction {
        private readonly MethodInfo _meth;
        
        /// <summary>
        /// Create an InstructionMethod object with the given opcode
        /// and MethodInfo parameter.
        /// </summary>
        /// <param name="op">Opcode</param>
        /// <param name="meth">MethodInfo object</param>
        public InstructionMethod(OpCode op, MethodInfo meth) : base(op) {
            if (meth == null) {
                throw new ArgumentNullException(nameof(meth));
            }
            _meth = meth;
        }
        
        /// <summary>
        /// Generate MSIL code to emit a opcode that takes a MethodInfo
        /// parameter.
        /// </summary>
        /// <param name="il">ILGenerator object</param>
        public override void Generate(ILGenerator il) {
            if (il == null) {
                throw new ArgumentNullException(nameof(il));
            }
            if (Deleted) {
                return;
            }
            il.Emit(Code, _meth);
        }
    }

    /// <summary>
    /// Defines an instruction class that constructs an opcode that
    /// takes a FieldInfo parameter.
    /// </summary>
    public class InstructionField : Instruction {
        private readonly FieldInfo _field;

        /// <summary>
        /// Create an InstructionMethod object with the given opcode
        /// and FieldInfo parameter.
        /// </summary>
        /// <param name="op">Opcode</param>
        /// <param name="field">FieldInfo object</param>
        public InstructionField(OpCode op, FieldInfo field) : base(op) {
            if (field == null) {
                throw new ArgumentNullException(nameof(field));
            }
            _field = field;
        }
        
        /// <summary>
        /// Generate MSIL code to emit a opcode that takes a FieldInfo
        /// parameter.
        /// </summary>
        /// <param name="il">ILGenerator object</param>
        public override void Generate(ILGenerator il) {
            if (il == null) {
                throw new ArgumentNullException(nameof(il));
            }
            if (Deleted) {
                return;
            }
            il.Emit(Code, _field);
        }
    }

    /// <summary>
    /// Defines an instruction class that constructs an opcode that
    /// takes a ConstructorInfo parameter.
    /// </summary>
    public class InstructionCtor : Instruction {
        private readonly ConstructorInfo _ctor;
        
        /// <summary>
        /// Create an InstructionMCtor object with the given opcode
        /// and ConstructorInfo parameter.
        /// </summary>
        /// <param name="op">Opcode</param>
        /// <param name="ctor">ConstructorInfo object</param>
        public InstructionCtor(OpCode op, ConstructorInfo ctor) : base(op) {
            if (ctor == null) {
                throw new ArgumentNullException(nameof(ctor));
            }
            _ctor = ctor;
        }
        
        /// <summary>
        /// Generate MSIL code to emit a opcode that takes a ConstructorInfo
        /// parameter.
        /// </summary>
        /// <param name="il">ILGenerator object</param>
        public override void Generate(ILGenerator il) {
            if (il == null) {
                throw new ArgumentNullException(nameof(il));
            }
            if (Deleted) {
                return;
            }
            il.Emit(Code, _ctor);
        }
    }

    /// <summary>
    /// Defines an instruction class that constructs an opcode that takes
    /// a Type parameter.
    /// </summary>
    public class InstructionType : Instruction {
        private readonly Type _type;
        
        /// <summary>
        /// Create an InstructionType object with the given opcode
        /// and Type parameter.
        /// </summary>
        /// <param name="op">Opcode</param>
        /// <param name="type">Type object</param>
        public InstructionType(OpCode op, Type type) : base(op) {
            _type = type;
        }
        
        /// <summary>
        /// Generate MSIL code to emit a opcode that takes a Type parameter.
        /// </summary>
        /// <param name="il">ILGenerator object</param>
        public override void Generate(ILGenerator il) {
            if (il == null) {
                throw new ArgumentNullException(nameof(il));
            }
            if (Deleted) {
                return;
            }
            il.Emit(Code, _type);
        }
    }

    /// <summary>
    /// Defines an instruction class that constructs an opcode that takes
    /// an array of labels as its parameter.
    /// </summary>
    public class InstructionLabelArray : Instruction {
        private readonly Label [] _labels;
        
        /// <summary>
        /// Create an InstructionLabelArray object with the given opcode
        /// and an array of Label objects.
        /// </summary>
        /// <param name="op">Opcode</param>
        /// <param name="labels">Array of Label objects</param>
        public InstructionLabelArray(OpCode op, Label [] labels) : base(op) {
            if (labels == null) {
                throw new ArgumentNullException(nameof(labels));
            }
            _labels = labels;
        }
        
        /// <summary>
        /// Generate MSIL code to emit a opcode that takes an array of
        /// labels as a parameter.
        /// </summary>
        /// <param name="il">ILGenerator object</param>
        public override void Generate(ILGenerator il) {
            if (il == null) {
                throw new ArgumentNullException(nameof(il));
            }
            if (Deleted) {
                return;
            }
            il.Emit(Code, _labels);
        }
    }

    /// <summary>
    /// Defines an instruction class that constructs an opcode that takes
    /// an local variable index.
    /// </summary>
    public class InstructionLocal : Instruction {
        
        /// <summary>
        /// Create an InstructionInt object with the given opcode
        /// and integer value.
        /// </summary>
        /// <param name="op">Opcode</param>
        /// <param name="value">An integer value</param>
        public InstructionLocal(OpCode op, LocalDescriptor value) : base(op) {
            if (value == null) {
                throw new ArgumentNullException(nameof(value));
            }
            Value = value;
        }
        
        /// <summary>
        /// Gets the local variable reference.
        /// </summary>
        /// <value>The integer parameter assigned to this opcode</value>
        public LocalDescriptor Value { get; private set; }
        
        /// <summary>
        /// Generate MSIL code to emit a opcode that takes an integer
        /// parameter.
        /// </summary>
        /// <param name="il">ILGenerator object</param>
        public override void Generate(ILGenerator il) {
            if (il == null) {
                throw new ArgumentNullException(nameof(il));
            }
            if (Deleted) {
                return;
            }
            switch (Code.Name) {
                case "stloc":
                    switch (Value.Index) {
                        case 0: il.Emit(OpCodes.Stloc_0); return;
                        case 1: il.Emit(OpCodes.Stloc_1); return;
                        case 2: il.Emit(OpCodes.Stloc_2); return;
                        case 3: il.Emit(OpCodes.Stloc_3); return;
                    }
                    if (Value.Index < 256) {
                        il.Emit(OpCodes.Stloc_S, (byte)Value.Index);
                    } else {
                        il.Emit(OpCodes.Stloc, Value.Index);
                    }
                    break;

                case "ldloc":
                    switch (Value.Index) {
                        case 0: il.Emit(OpCodes.Ldloc_0); return;
                        case 1: il.Emit(OpCodes.Ldloc_1); return;
                        case 2: il.Emit(OpCodes.Ldloc_2); return;
                        case 3: il.Emit(OpCodes.Ldloc_3); return;
                    }
                    if (Value.Index < 256) {
                        il.Emit(OpCodes.Ldloc_S, (byte)Value.Index);
                    } else {
                        il.Emit(OpCodes.Ldloc, Value.Index);
                    }
                    break;

                case "ldloca":
                    if (Value.Index < 256) {
                        il.Emit(OpCodes.Ldloca_S, (byte)Value.Index);
                    } else {
                        il.Emit(OpCodes.Ldloca, Value.Index);
                    }
                    break;
                    
                default:
                    il.Emit(Code, Value.Index);
                    break;
            }
        }
    }

    /// <summary>
    /// Defines an instruction class that constructs an opcode that takes
    /// an integer parameter.
    /// </summary>
    public class InstructionInt : Instruction {

        /// <summary>
        /// Create an InstructionInt object with the given opcode
        /// and integer value.
        /// </summary>
        /// <param name="op">Opcode</param>
        /// <param name="value">An integer value</param>
        public InstructionInt(OpCode op, int value) : base(op) {
            Value = value;
        }

        /// <summary>
        /// Gets the integer parameter.
        /// </summary>
        /// <value>The integer parameter assigned to this opcode</value>
        public int Value { get; private set; }
        
        /// <summary>
        /// Generate MSIL code to emit a opcode that takes an integer
        /// parameter.
        /// </summary>
        /// <param name="il">ILGenerator object</param>
        public override void Generate(ILGenerator il) {
            if (il == null) {
                throw new ArgumentNullException(nameof(il));
            }
            if (Deleted) {
                return;
            }
            switch (Code.Name) {
                case "ldc.i4":
                    switch (Value) {
                        case -1: il.Emit(OpCodes.Ldc_I4_M1); return;
                        case 0:  il.Emit(OpCodes.Ldc_I4_0); return;
                        case 1:  il.Emit(OpCodes.Ldc_I4_1); return;
                        case 2:  il.Emit(OpCodes.Ldc_I4_2); return;
                        case 3:  il.Emit(OpCodes.Ldc_I4_3); return;
                        case 4:  il.Emit(OpCodes.Ldc_I4_4); return;
                        case 5:  il.Emit(OpCodes.Ldc_I4_5); return;
                        case 6:  il.Emit(OpCodes.Ldc_I4_6); return;
                        case 7:  il.Emit(OpCodes.Ldc_I4_7); return;
                        case 8:  il.Emit(OpCodes.Ldc_I4_8); return;
                    }
                    if (Value >= -128 && Value <= 127) {
                        il.Emit(OpCodes.Ldc_I4_S, (byte)Value);
                        return;
                    }
                    il.Emit(Code, Value);
                    break;

                default:
                    il.Emit(Code, Value);
                    break;
            }
        }
    }

    /// <summary>
    /// Defines an instruction class that constructs an opcode that takes
    /// a float parameter.
    /// </summary>
    public class InstructionFloat : Instruction {
        private readonly float _value;
        
        /// <summary>
        /// Create an InstructionFloat object with the given opcode
        /// and float value.
        /// </summary>
        /// <param name="op">Opcode</param>
        /// <param name="value">A float value</param>
        public InstructionFloat(OpCode op, float value) : base(op) {
            _value = value;
        }
        
        /// <summary>
        /// Generate MSIL code to emit a opcode that takes a float
        /// parameter.
        /// </summary>
        /// <param name="il">ILGenerator object</param>
        public override void Generate(ILGenerator il) {
            if (il == null) {
                throw new ArgumentNullException(nameof(il));
            }
            if (Deleted) {
                return;
            }
            il.Emit(Code, _value);
        }
    }

    /// <summary>
    /// Defines an instruction class that constructs an opcode that takes
    /// a double parameter.
    /// </summary>
    public class InstructionDouble : Instruction {
        private readonly double _value;
        
        /// <summary>
        /// Create an InstructionDouble object with the given opcode
        /// and double value.
        /// </summary>
        /// <param name="op">Opcode</param>
        /// <param name="value">A double value</param>
        public InstructionDouble(OpCode op, double value) : base(op) {
            _value = value;
        }
        
        /// <summary>
        /// Generate MSIL code to emit a opcode that takes a double
        /// parameter.
        /// </summary>
        /// <param name="il">ILGenerator object</param>
        public override void Generate(ILGenerator il) {
            if (il == null) {
                throw new ArgumentNullException(nameof(il));
            }
            if (Deleted) {
                return;
            }
            il.Emit(Code, _value);
        }
    }

    /// <summary>
    /// Defines an instruction class that constructs an opcode that takes
    /// a string parameter.
    /// </summary>
    public class InstructionString : Instruction {
        private readonly string _str;

        /// <summary>
        /// Create an InstructionString object with the given opcode
        /// and string value.
        /// </summary>
        /// <param name="op">Opcode</param>
        /// <param name="str">A string value</param>
        public InstructionString(OpCode op, string str) : base(op) {
            if (str == null) {
                throw new ArgumentNullException(nameof(str));
            }
            _str = str;
        }
        
        /// <summary>
        /// Generate MSIL code to emit a opcode that takes a string
        /// parameter.
        /// </summary>
        /// <param name="il">ILGenerator object</param>
        public override void Generate(ILGenerator il) {
            if (il == null) {
                throw new ArgumentNullException(nameof(il));
            }
            if (Deleted) {
                return;
            }
            switch (Code.Name) {
                case "ldstr":
                    if (_str.Length == 0) {
                        il.Emit(OpCodes.Ldsfld, typeof(string).GetField("Empty"));
                    } else {
                        il.Emit(OpCodes.Ldstr, _str);
                    }
                    break;

                default:
                    il.Emit(Code, _str);
                    break;
            }
        }
    }

    /// <summary>
    /// Defines an instruction class that constructs an indirect all opcode with
    /// specified calling convention, return type and parameter types.
    /// </summary>
    public class InstructionCalli : Instruction {
        private readonly CallingConventions _conv;
        private readonly Type _returnType;
        private readonly Type[] _parameterTypes;
        
        /// <summary>
        /// Create an InstructionCalli object with the given opcode,
        /// and function parameter definitions.
        /// </summary>
        /// <param name="op">Opcode</param>
        /// <param name="conv">The function calling convention</param>
        /// <param name="returnType">The return type</param>
        /// <param name="parameterTypes">An array of types for each parameter</param>
        public InstructionCalli(OpCode op, CallingConventions conv, Type returnType, Type[] parameterTypes) : base(op) {
            _conv = conv;
            _returnType = returnType;
            _parameterTypes = parameterTypes;
        }
        
        /// <summary>
        /// Generate MSIL code to emit a Calli indirect call with the given
        /// calling convention, return type and array of parameter types.
        /// </summary>
        /// <param name="il">ILGenerator object</param>
        public override void Generate(ILGenerator il) {
            if (il == null) {
                throw new ArgumentNullException(nameof(il));
            }
            if (Deleted) {
                return;
            }
            il.EmitCalli(OpCodes.Calli, _conv, _returnType, _parameterTypes, null);
        }
    }

    /// <summary>
    /// Defines an instruction class that constructs a source code marker.
    /// </summary>
    public class InstructionMarker : Instruction {
        private readonly ISymbolDocumentWriter _doc;
        private readonly int _linenumber;
        
        /// <summary>
        /// Create an InstructionMarker object to represent the source code
        /// file and line number of the current point in the sequence.
        /// </summary>
        /// <param name="doc">An ISymbolDocumentWriter object</param>
        /// <param name="linenumber">An integer line number</param>
        public InstructionMarker(ISymbolDocumentWriter doc, int linenumber) {
            _doc = doc;
            _linenumber = linenumber;
        }
        
        /// <summary>
        /// Generate MSIL code to emit an instruction marker at the current
        /// sequence in the output.
        /// </summary>
        /// <param name="il">ILGenerator object</param>
        public override void Generate(ILGenerator il) {
            if (il == null) {
                throw new ArgumentNullException(nameof(il));
            }
            if (Deleted) {
                return;
            }
            il.MarkSequencePoint(_doc, _linenumber, 1, _linenumber, 100);
        }
    }

    /// <summary>
    /// Defines an Instruction class for a Try/Catch block.
    /// </summary>
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
            if (il == null) {
                throw new ArgumentNullException(nameof(il));
            }
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
                    MethodInfo methodInfo = jcomRuntimeException.GetMethod("GeneralHandler", new[] { typeof(Exception) });
                    il.EmitCall(OpCodes.Call, methodInfo, new[] { typeof(Exception) });

                    il.Emit(OpCodes.Stloc_S, tmp1);

                    if (Err != null || ErrText != null) {
                        if (Err != null && Err.IsReferenced) {
                            il.Emit(OpCodes.Ldloc_S, tmp1);
                            il.EmitCall(OpCodes.Call, jcomRuntimeException.GetMethod("get_ErrorCode"), null);
                            il.Emit(OpCodes.Stsfld, (FieldInfo)Err.Info);
                        }
                        if (ErrText != null && ErrText.IsReferenced) {
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
                    MethodInfo methodInfo = jcomRuntimeException.GetMethod("GeneralHandlerNoThrow", new[] { typeof(Exception) });
                    il.EmitCall(OpCodes.Call, methodInfo, new[] { typeof(Exception) });

                    il.Emit(OpCodes.Stloc_S, tmp1);

                    Label skipMessage = il.DefineLabel();

                    il.Emit(OpCodes.Ldloc_S, tmp1);
                    il.EmitCall(OpCodes.Call, jcomRuntimeException.GetMethod("get_Type"), null);
                    il.Emit(OpCodes.Ldc_I4, (int)JComRuntimeExceptionType.END);
                    il.Emit(OpCodes.Beq, skipMessage);

                    il.Emit(OpCodes.Ldloc_S, tmp1);
                    il.EmitCall(OpCodes.Callvirt, jcomRuntimeException.GetMethod("get_Message"), null);

                    MethodInfo meth = typeof(Console).GetMethod("WriteLine", new[] { typeof(string) });
                    il.EmitCall(OpCodes.Call, meth, null);
                    il.MarkLabel(skipMessage);
                    break;
                }
            }
        }
    }
}
