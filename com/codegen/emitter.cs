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

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using JComLib;

namespace CCompiler {

    /// <summary>
    /// Defines a class that emits MSIL opcodes and operands
    /// into a specific method or constructor.
    /// </summary>
    public class Emitter {
        private readonly ILGenerator _il;
        private readonly Collection<LocalDescriptor> _temp;
        private readonly Collection<Instruction> _code;

        /// <summary>
        /// Specifies whether to generate debuggable code
        /// </summary>
        public bool IsDebuggable { get; set; }

        /// <summary>
        /// Constructs an Emitter using the specified MethodBuilder.
        /// </summary>
        /// <param name="metb">MethodBuilder</param>
        public Emitter(MethodBuilder metb) {
            if (metb == null) {
                throw new ArgumentNullException(nameof(metb));
            }
            _il = metb.GetILGenerator();
            _temp = new Collection<LocalDescriptor>();
            _code = new Collection<Instruction>();
        }

        /// <summary>
        /// Constructs an Emitter using the specified ConstructorBuilder.
        /// </summary>
        /// <param name="cntb">ConstructorBuilder</param>
        public Emitter(ConstructorBuilder cntb) {
            if (cntb == null) {
                throw new ArgumentNullException(nameof(cntb));
            }
            _il = cntb.GetILGenerator();
            _temp = new Collection<LocalDescriptor>();
            _code = new Collection<Instruction>();
        }

        /// <summary>
        /// Save the code emitted so far to the method.
        /// </summary>
        public void Save() {
            Peephole.Optimise(_code);
            foreach (Instruction inst in _code) {
                inst.Generate(_il);
            }
            _code.Clear();
        }

        /// <summary>
        /// Emit the code to begin a try/catch block.
        /// </summary>
        public void SetupTryCatchBlock() {
            _code.Add(new InstructionTryCatch(EmitExceptionHandlerType.Try));
        }

        /// <summary>
        /// Emit the code to add the default exception handler for the main
        /// program.
        /// </summary>
        public void AddTryCatchHandlerBlock(Symbol err, Symbol errText) {
            _code.Add(new InstructionTryCatch(EmitExceptionHandlerType.Catch, err, errText));
        }

        /// <summary>
        /// Emit the code to add the default exception handler for the main
        /// program.
        /// </summary>
        public void AddDefaultTryCatchHandlerBlock() {
            _code.Add(new InstructionTryCatch(EmitExceptionHandlerType.DefaultCatch));
        }

        /// <summary>
        /// Emit the code to close the try/catch block.
        /// </summary>
        public void CloseTryCatchBlock() {
            _code.Add(new InstructionTryCatch(EmitExceptionHandlerType.EndCatch));
        }

        /// <summary>
        /// Emit the specified line number into the current point in the generated
        /// code.
        /// </summary>
        /// <param name="doc">Document.</param>
        /// <param name="linenumber">The line number to emit</param>
        public void MarkLinenumber(ISymbolDocumentWriter doc, int linenumber) {
            if (doc != null) {
                _code.Add(new InstructionMarker(doc, linenumber));
            }
        }

        /// <summary>
        /// Emit the specified label into the current point in the generated
        /// code.
        /// </summary>
        /// <param name="lab">The Label to emit</param>
        public void MarkLabel(Label lab) {
            _code.Add(new InstructionLabelMarker(lab));
        }

        /// <summary>
        /// Emit the instruction to branch to the given label.
        /// </summary>
        /// <param name="lab">Label destination</param>
        public void Branch(Label lab) {
            Emit0(OpCodes.Br, lab);
        }

        /// <summary>
        /// Emit the instruction to branch to the given label.
        /// </summary>
        /// <param name="lab">Label destination</param>
        public void BranchLess(Label lab) {
            Emit0(OpCodes.Blt, lab);
        }

        /// <summary>
        /// Emit the instruction to branch to the given label.
        /// </summary>
        /// <param name="lab">Label destination</param>
        public void BranchLessOrEqual(Label lab) {
            Emit0(OpCodes.Ble_Un_S, lab);
        }

        /// <summary>
        /// Emit the instruction to branch to the given label.
        /// </summary>
        /// <param name="lab">Label destination</param>
        public void BranchGreater(Label lab) {
            Emit0(OpCodes.Bgt, lab);
        }

        /// <summary>
        /// Emit the instruction to branch to the given label.
        /// </summary>
        /// <param name="lab">Label destination</param>
        public void BranchEqual(Label lab) {
            Emit0(OpCodes.Beq, lab);
        }

        /// <summary>
        /// Emit the instruction to branch to the given label.
        /// </summary>
        /// <param name="lab">Label destination</param>
        public void BranchIfTrue(Label lab) {
            Emit0(OpCodes.Brtrue, lab);
        }

        /// <summary>
        /// Emit the instruction to branch to the given label.
        /// </summary>
        /// <param name="lab">Label destination</param>
        public void BranchIfFalse(Label lab) {
            Emit0(OpCodes.Brfalse, lab);
        }

        /// <summary>
        /// Emit the instruction to compare the two items at the
        /// top of the stack for equality.
        /// </summary>
        public void CompareEquals() {
            Emit0(OpCodes.Ceq);
        }

        /// <summary>
        /// Emit the instruction to compare whether the first item
        /// at the top of the stack is greater than the second.
        /// </summary>
        public void CompareGreater() {
            Emit0(OpCodes.Cgt);
        }

        /// <summary>
        /// Emit the instruction to compare whether the first item
        /// at the top of the stack is less than the second.
        /// </summary>
        public void CompareLesser() {
            Emit0(OpCodes.Clt);
        }

        /// <summary>
        /// Emit the instruction to logical XOR between the two
        /// items at the top of the stack.
        /// </summary>
        public void Xor() {
            Emit0(OpCodes.Xor);
        }

        /// <summary>
        /// Emit the instruction to logical AND between the two
        /// items at the top of the stack.
        /// </summary>
        public void And() {
            Emit0(OpCodes.And);
        }

        /// <summary>
        /// Emit the instruction to logical OR between the two
        /// items at the top of the stack.
        /// </summary>
        public void Or() {
            Emit0(OpCodes.Or);
        }

        /// <summary>
        /// Emit the instruction to return from the current
        /// program unit.
        /// </summary>
        public void Return() {
            Emit0(OpCodes.Ret);
        }

        /// <summary>
        /// Emit the instruction to duplicate the item at the top of
        /// the stack.
        /// </summary>
        public void Dup() {
            Emit0(OpCodes.Dup);
        }

        /// <summary>
        /// Emit the instruction to discard the item at the top of
        /// the stack.
        /// </summary>
        public void Pop() {
            Emit0(OpCodes.Pop);
        }

        /// <summary>
        /// Create a new label to be used as the destination of a
        /// branch instruction.
        /// </summary>
        /// <returns>A new label</returns>
        public Label CreateLabel() {
            return _il.DefineLabel();
        }

        /// <summary>
        /// Create a local variable from the given symbol and return the
        /// index of the variable in the local index table.
        /// </summary>
        /// <param name="sym">Symbol</param>
        public void CreateLocal(Symbol sym) {
            if (sym == null) {
                throw new ArgumentNullException(nameof(sym));
            }
            LocalBuilder lb = _il.DeclareLocal(sym.SystemType);
        #if GENERATE_NATIVE_BINARIES
            if (IsDebuggable) {
                lb.SetLocalSymInfo(sym.Name);
            }
        #endif
            sym.Index = AssignLocal(sym.SystemType, lb.LocalIndex);
        }

        /// <summary>
        /// Create a local variable of the given type and return the index
        /// of the variable in the local index table.
        /// </summary>
        /// <param name="type">Type of the local</param>
        /// <returns>The integer index of the new local</returns>
        public LocalDescriptor CreateLocal(Type type) {
            LocalBuilder lb = _il.DeclareLocal(type);
            return AssignLocal(type, lb.LocalIndex);
        }

        /// <summary>
        /// Creates a fixed string using the width specified by the symbol and
        /// stores it at the top of the stack.
        /// </summary>
        /// <param name="sym">Symbol</param>
        public void CreateFixedString(Symbol sym) {
            if (sym == null) {
                throw new ArgumentNullException(nameof(sym));
            }
            Debug.Assert(sym.Type == SymType.FIXEDCHAR);
            Type baseType = typeof(FixedString);
            LoadInteger(sym.FullType.Width);
            CreateObject(baseType, new [] { typeof(int) });
        }

        /// <summary>
        /// Creates a simple 1 dimensional array of the given size and type.
        /// </summary>
        /// <param name="count">Size of the array</param>
        /// <param name="itemsType">Type of the items in the array</param>
        public void CreateSimpleArray(int count, Type itemsType) {
            if (count < 0) {
                throw new InvalidOperationException("CreateSimpleArray does not support dynamic arrays");
            }
            LoadInteger(count);
            Emit0(OpCodes.Newarr, itemsType);
        }

        /// <summary>
        /// Creates a simple 1 dimensional array of the given type and using the
        /// size on the top of the stack..
        /// </summary>
        /// <param name="itemsType">Type of the items in the array</param>
        public void CreateArray(Type itemsType) {
            Emit0(OpCodes.Newarr, itemsType);
        }

        /// <summary>
        /// Create an object of the specified type and with the given list of
        /// parameter types.
        /// </summary>
        /// <param name="baseType">Base type.</param>
        /// <param name="paramTypes">Parameter types.</param>
        public void CreateObject(Type baseType, Type[] paramTypes) {
            if (baseType == null) {
                throw new ArgumentNullException(nameof(baseType));
            }
            if (paramTypes == null) {
                throw new ArgumentNullException(nameof(paramTypes));
            }
            Emit0(OpCodes.Newobj, baseType.GetConstructor(paramTypes));
        }

        /// <summary>
        /// Emit the instructions to call a method.
        /// </summary>
        /// <param name="method">A MethodInfo method</param>
        public void Call(MethodInfo method) {
            Emit0(OpCodes.Call, method);
        }

        /// <summary>
        /// Emit the instructions to call a method whose address has been
        /// pushed to the top of the stack.
        /// </summary>
        /// <param name="type">Return type of the method</param>
        /// <param name="paramTypes">Parameter types</param>
        public void CallIndirect(SymType type, Type[] paramTypes) {
            Emit0(OpCodes.Calli, CallingConventions.Standard, Symbol.SymTypeToSystemType(type), paramTypes);
        }

        /// <summary>
        /// Emit a switch statement.
        /// </summary>
        /// <param name="labels">Labels.</param>
        public void Switch(Label[] labels) {
            Emit0(OpCodes.Switch, labels);
        }

        /// <summary>
        /// Emit the code to add values of the specified type.
        /// </summary>
        /// <param name="type">Type of the arguments</param>
        public void Add(SymType type) {
            switch (type) {
                case SymType.COMPLEX:
                    Emit0(OpCodes.Call, typeof(Complex).GetMethod("op_Addition", new [] { typeof(Complex), typeof(Complex) } ));
                    break;

                default:
                    Emit0(OpCodes.Add);
                    break;
            }
        }

        /// <summary>
        /// Emit the code to subtract values of the specified type.
        /// </summary>
        /// <param name="type">Type of the arguments</param>
        public void Sub(SymType type) {
            switch (type) {
                case SymType.COMPLEX:
                    Emit0(OpCodes.Call, typeof(Complex).GetMethod("op_Subtraction", new [] { typeof(Complex), typeof(Complex) } ));
                    break;
                    
                default:
                    Emit0(OpCodes.Sub);
                    break;
            }
        }

        /// <summary>
        /// Emit the code to negate a value of the specified type.
        /// </summary>
        /// <param name="type">Type of the arguments</param>
        public void Neg(SymType type) {
            switch (type) {
                case SymType.COMPLEX:
                    Emit0(OpCodes.Call, typeof(Complex).GetMethod("op_UnaryNegation", new [] { typeof(Complex) } ));
                    break;
                    
                default:
                    Emit0(OpCodes.Neg);
                    break;
            }
        }

        /// <summary>
        /// Emit the code to multiply values of the specified type.
        /// </summary>
        /// <param name="type">Type of the arguments</param>
        public void Mul(SymType type) {
            switch (type) {
                case SymType.COMPLEX:
                    Emit0(OpCodes.Call, typeof(Complex).GetMethod("op_Multiply", new [] { typeof(Complex), typeof(Complex) } ));
                    break;
                    
                default:
                    Emit0(OpCodes.Mul);
                    break;
            }
        }

        /// <summary>
        /// Emit the code to divide values of the specified type.
        /// </summary>
        /// <param name="type">Type of the arguments</param>
        public void Div(SymType type) {
            switch (type) {
                case SymType.COMPLEX:
                    Emit0(OpCodes.Call, typeof(Complex).GetMethod("op_Division", new [] { typeof(Complex), typeof(Complex) } ));
                    break;
                    
                default:
                    Emit0(OpCodes.Div);

                    // Floating point divisions by zero do not throw an exception but set the Nan on the
                    // number. So we need to test for this and explicitly throw an exception. This makes the
                    // code slower and more bloated though so we may want to have a switch to turn this off.
                    if (type == SymType.FLOAT || type == SymType.DOUBLE) {
                        Type actualType = Symbol.SymTypeToSystemType(type);
                        Label skipException = _il.DefineLabel();
                        Emit0(OpCodes.Dup);
                        Emit0(OpCodes.Call, actualType.GetMethod("IsInfinity", new[] { actualType }));
                        Emit0(OpCodes.Brfalse, skipException);
                        Emit0(OpCodes.Ldc_I4, (int)JComRuntimeErrors.DIVISION_BY_ZERO);
                        Emit0(OpCodes.Newobj, typeof(JComRuntimeException).GetConstructor(new[] { typeof(JComRuntimeErrors) }));
                        Emit0(OpCodes.Throw);
                        MarkLabel(skipException);
                    }
                    break;
            }
        }

        /// <summary>
        /// Emit the code to divide two integer values.
        /// </summary>
        /// <param name="type">Type of the arguments</param>
        public void IDiv(SymType type) {
            switch (type) {
                case SymType.COMPLEX:
                    throw new InvalidOperationException("Complex does not support IDIV");

                default:
                    Emit0(OpCodes.Div);
                    Emit0(OpCodes.Conv_R8);
                    Emit0(OpCodes.Call, typeof(Math).GetMethod("Floor", new[] { typeof(double) }));
                    Emit0(OpCodes.Conv_I4);
                    break;
            }
        }

        /// <summary>
        /// Emit the code to return the remainder of the division of the specified type.
        /// </summary>
        /// <param name="type">Type of the arguments</param>
        public void Mod(SymType type) {
            switch (type) {
                case SymType.COMPLEX:
                    throw new InvalidOperationException("Complex does not support modulus");

                default:
                    Emit0(OpCodes.Rem);
                    break;
            }
        }

        /// <summary>
        /// Checks whether the actual type of the last operation matches the type
        /// needed and, if not, emits the appropriate conversion to the required type.
        /// If typeNeeded is NONE then no conversion is performed..
        /// </summary>
        /// <returns>The type that the last operation was converted to</returns>
        /// <param name="actualType">The actual type of the operation</param>
        /// <param name="typeNeeded">The type needed.</param>
        public SymType ConvertType(SymType actualType, SymType typeNeeded) {
            if (actualType == SymType.REF) {
                return actualType;
            }
            if (actualType != typeNeeded) {
                switch (typeNeeded) {
                    case SymType.NONE:      typeNeeded = actualType; break;
                    case SymType.LABEL:     typeNeeded = actualType; break;
                    case SymType.DOUBLE:    Emit0(OpCodes.Conv_R8); break;
                    case SymType.FLOAT:     Emit0(OpCodes.Conv_R4); break;
                    case SymType.INTEGER:   Emit0(OpCodes.Conv_I4); break;

                    case SymType.BOOLEAN:
                        if (actualType == SymType.FIXEDCHAR) {
                            Emit0(OpCodes.Call, typeof(FixedString).GetMethod("get_IsEmpty", Type.EmptyTypes));
                            Emit0(OpCodes.Not);
                            break;
                        }
                        if (actualType == SymType.CHAR) {
                            Emit0(OpCodes.Call, typeof(FixedString).GetMethod("IsNullOrEmpty", Type.EmptyTypes));
                            Emit0(OpCodes.Not);
                            break;
                        }
                        Emit0(OpCodes.Conv_I1);
                        break;

                    case SymType.CHAR:
                        if (actualType == SymType.FIXEDCHAR) {
                            Emit0(OpCodes.Callvirt, typeof(FixedString).GetMethod("ToString", Type.EmptyTypes ));
                        }
                        break;

                    case SymType.FIXEDCHAR: {
                        Type fromType = Symbol.SymTypeToSystemType(actualType);
                        Emit0(OpCodes.Call, typeof(FixedString).GetMethod("op_Implicit", new [] { fromType } ));
                        break;
                    }

                    case SymType.COMPLEX: {
                        Type fromType = Symbol.SymTypeToSystemType(actualType);
                        Emit0(OpCodes.Call, typeof(Complex).GetMethod("op_Implicit", new [] { fromType } ));
                        break;
                    }
                }
            }
            return typeNeeded;
		}

        /// <summary>
        /// Emit the code that loads an entire array. This is generally
        /// emitting the base address of the array if useRef is specified, or
        /// the array itself otherwise.
        /// </summary>
        /// <returns>The type of the array</returns>
        /// <param name="identNode">An IdentifierParseNode object representing
        /// the array variable.</param>
        /// <param name="useRef">If set to <c>true</c> use emit the address of
        /// the array</param>
        public SymType GenerateLoadArray(IdentifierParseNode identNode, bool useRef) {
            if (identNode == null) {
                throw new ArgumentNullException(nameof(identNode));
            }
            Symbol sym = identNode.Symbol;

            if (useRef) {
                GenerateLoadAddress(sym);
            } else if (sym.IsLocal) {
                LoadSymbol(sym);
            } else {
                GenerateLoadArgument(sym);
            }
            return SymType.REF;
        }

        /// <summary>
        /// Emit the appropriate load parameter index opcode.
        /// </summary>
        /// <param name="sym">Symbol from which to emit</param>
        public void GenerateLoadArgument(Symbol sym) {
            if (sym == null) {
                throw new ArgumentNullException(nameof(sym));
            }
            switch (sym.Linkage) {
                case SymLinkage.BYVAL:
                    LoadParameter(sym.ParameterIndex);
                    break;

                case SymLinkage.BYREF:
                    LoadParameter(sym.ParameterIndex);
                    if (sym.IsArray) {
                        LoadIndirect(SymType.REF);
                        break;
                    }
                    if (sym.IsValueType) {
                        LoadIndirect(sym.Type);
                    }
                    break;
            }
        }

        /// <summary>
        /// Emit the load of an address of a simple symbol
        /// </summary>
        /// <param name="sym">Symbol to load</param>
        public void GenerateLoadAddress(Symbol sym) {
            if (sym.IsStatic) {
                LoadStaticAddress((FieldInfo)sym.Info);
            } else if (sym.IsMethod) {
                LoadFunction(sym);
            } else if (sym.IsLocal) {
                LoadLocalAddress(sym.Index);
            } else if (sym.IsParameter) {
                LoadParameterAddress(sym.ParameterIndex);
            } else {
                Debug.Assert(false, $"Cannot load the address of {sym}");
            }
        }

        /// <summary>
        /// Generate the code to initialise a fixed string array by calling
        /// the Length on every element. The size of the array must be on the
        /// top of the stack on entry.
        /// </summary>
        /// <param name="sym">Fixed string array symbol</param>
        public void InitFixedStringArray(Symbol sym) {
            LocalDescriptor total = GetTemporary(typeof(int));
            StoreLocal(total);
            LocalDescriptor count = GetTemporary(typeof(int));
            Label loopStart = CreateLabel();
            LoadInteger(0);
            StoreLocal(count);
            MarkLabel(loopStart);
            LoadSymbol(sym);
            LoadLocal(count);
            LoadInteger(sym.FullType.Width);
            CreateObject(typeof(FixedString), new[] { typeof(int) });
            StoreArrayElement(sym);
            LoadLocal(count);
            LoadInteger(1);
            Add(SymType.INTEGER);
            StoreLocal(count);
            LoadLocal(count);
            LoadLocal(total);
            BranchLess(loopStart);
            ReleaseTemporary(count);
        }

        /// <summary>
        /// Emit the code to load a local variable onto the stack. Different code
        /// is emitted depending on whether the variable is a static.
        /// </summary>
        /// <param name="sym">A Symbol object representing the variable</param>
        /// <returns>The SymType of the variable loaded</returns>
        public SymType LoadSymbol(Symbol sym) {
            if (sym == null) {
                throw new ArgumentNullException(nameof(sym));
            }
            if (sym.IsInCommon) {
                Symbol symCommon = sym.Common;
                List<Symbol> commonList = (List<Symbol>)symCommon.Info;
                sym = commonList[sym.CommonIndex];
            }
            if (sym.IsStatic) {
                LoadStatic((FieldInfo)sym.Info);
            } else {
                LoadLocal(sym.Index);
            }
            return sym.Type;
        }

        /// <summary>
        /// Emit the code to load the the specified variant value.
        /// </summary>
        /// <param name="em">Emitter</param>
        /// <param name="value">The value to be loaded</param>
        public void LoadVariant(Variant value) {
            if (value == null) {
                throw new ArgumentNullException(nameof(value));
            }
            switch (value.Type) {
                case VariantType.STRING:    LoadString(value.StringValue); break;
                case VariantType.FLOAT:     LoadFloat(value.RealValue); break;
                case VariantType.DOUBLE:    LoadDouble(value.DoubleValue); break;
                case VariantType.BOOLEAN:   LoadBoolean(value.BoolValue); break;
                case VariantType.INTEGER:   LoadInteger(value.IntValue); break;
                case VariantType.COMPLEX:   LoadComplex(value.ComplexValue); break;

                default:
                    Debug.Assert(false, $"GenerateLoad: Unsupported type {value.Type}");
                    break;
            }
        }

        /// <summary>
        /// Emit the save the value on the top of the stack to a symbol.
        /// </summary>
        /// <param name="sym">The symbol to which the value should be stored</param>
        /// <returns>The type of the value stored</returns>
        public void StoreSymbol(Symbol sym) {
            if (sym == null) {
                throw new ArgumentNullException(nameof(sym));
            }
            if (sym.IsInCommon) {
                Symbol symCommon = sym.Common;
                List<Symbol> commonList = (List<Symbol>)symCommon.Info;
                sym = commonList[sym.CommonIndex];
            }
            if (sym.IsStatic) {
                StoreStatic((FieldInfo)sym.Info);
            } else {
                StoreLocal(sym.Index);
            }
        }

        /// <summary>
        /// Emits the code to convert the item at the stop of the stack
        /// to the given system type. Only integer, float and double are
        /// supported with this method.
        /// </summary>
        /// <param name="typeNeeded">System type needed</param>
        public void ConvertSystemType(Type typeNeeded) {
            if (typeNeeded == null) {
                throw new ArgumentNullException(nameof(typeNeeded));
            }
            switch (typeNeeded.Name.ToLower()) {
                case "int":       Emit0(OpCodes.Conv_I4); break;
                case "single":    Emit0(OpCodes.Conv_R4); break;
                case "double":    Emit0(OpCodes.Conv_R8); break;

                default:
                    Debug.Assert(false, $"ConvertSystemType: Unsupported type {typeNeeded}");
                    break;
            }
        }

        /// <summary>
        /// Emit the code to load null onto the stack.
        /// </summary>
        public void LoadNull() {
            Emit0(OpCodes.Ldnull);
        }

        /// <summary>
        /// Emit the code to load a value of the specified type onto the stack.
        /// </summary>
        /// <param name="type">The type wanted</param>
        /// <param name="value">A variant value</param>
        public void LoadValue(SymType type, Variant value) {
            if (value == null) {
                throw new ArgumentNullException(nameof(value));
            }
            switch (type) {
                case SymType.INTEGER:   LoadInteger(value.IntValue); break;
                case SymType.FLOAT:     LoadFloat(value.RealValue); break;
                case SymType.DOUBLE:    LoadDouble(value.DoubleValue); break;

                default:
                    Debug.Assert(false, $"LoadValue: Unsupported type {type}");
                    break;
            }
        }

        /// <summary>
        /// Emit the code to load an integer value,
        /// optimising to the short form where possible.
        /// </summary>
        /// <param name="value">Integer value</param>
        public void LoadInteger(int value) {
            Emit0(OpCodes.Ldc_I4, value);
        }

        /// <summary>
        /// EMit the code to load a floating point value.
        /// </summary>
        /// <param name="value">Float value</param>
        public void LoadFloat(float value) {
            Emit0(OpCodes.Ldc_R4, value);
        }

        /// <summary>
        /// Emit the code to load a double value.
        /// </summary>
        /// <param name="value">Double value</param>
        public void LoadDouble(double value) {
            Emit0(OpCodes.Ldc_R8, value);
        }

        /// <summary>
        /// Emit the code to load a boolean value.
        /// </summary>
        /// <param name="value">Boolean value</param>
        public void LoadBoolean(bool value) {
            LoadInteger(value ? -1 : 0);
        }

        /// <summary>
        /// Emit the code to load a string value. Strings are always loaded
        /// as CHAR type rather than FIXEDCHAR.
        /// </summary>
        /// <param name="value">String value</param>
        public void LoadString(string value) {
            if (value == null) {
                throw new ArgumentNullException(nameof(value));
            }
            Emit0(OpCodes.Ldstr, value);
        }

        /// <summary>
        /// Emit the code to create a complex value.
        /// </summary>
        /// <param name="value">Complex value</param>
        public void LoadComplex(Complex value) {
            LoadDouble(value.Real);
            LoadDouble(value.Imaginary);
            Emit0(OpCodes.Newobj, value.GetType().GetConstructor(new [] { typeof(double), typeof(double) } ));
        }

        /// <summary>
        /// Emit the code to load a local variable.
        /// </summary>
        /// <param name="value">The local variable reference</param>
        public void LoadLocal(LocalDescriptor value) {
            if (value == null) {
                throw new ArgumentNullException(nameof(value));
            }
            Emit0(OpCodes.Ldloc, value);
        }

        /// <summary>
        /// Emit the code to load a static.
        /// </summary>
        /// <param name="fi">The FieldInfo structure</param>
        public void LoadStatic(FieldInfo fi) {
            Emit0(OpCodes.Ldsfld, fi);
        }

        /// <summary>
        /// Emit the code to load the address of a static variable.
        /// </summary>
        /// <param name="fi">The FieldInfo structure</param>
        public void LoadStaticAddress(FieldInfo fi) {
            if (fi == null) {
                throw new ArgumentNullException(nameof(fi));
            }
            Emit0(OpCodes.Ldsflda, fi);
        }

        /// <summary>
        /// Emit the code to load the address of a local variable.
        /// </summary>
        /// <param name="value">The local variable reference</param>
        public void LoadLocalAddress(LocalDescriptor value) {
            if (value == null) {
                throw new ArgumentNullException(nameof(value));
            }
            Emit0(OpCodes.Ldloca, value);
        }

        /// <summary>
        /// Generate code to load a value indirectly.
        /// </summary>
        /// <param name="type">The type of the value</param>
        public void LoadIndirect(SymType type) {
            switch (type) {
                case SymType.DOUBLE:    Emit0(OpCodes.Ldind_R8); break;
                case SymType.FLOAT:     Emit0(OpCodes.Ldind_R4); break;
                case SymType.INTEGER:   Emit0(OpCodes.Ldind_I4); break;
                case SymType.BOOLEAN:   Emit0(OpCodes.Ldind_I1); break;
                case SymType.CHAR:      Emit0(OpCodes.Ldind_Ref); break;
                case SymType.REF:       Emit0(OpCodes.Ldind_Ref); break;

                default:
                    Debug.Assert(false, $"LoadIndirect: Unsupported type {type}");
                    break;
            }
        }

        /// <summary>
        /// Emit the code to load the parameter at the given index.
        /// </summary>
        /// <param name="index">Index of the parameter</param>
        public void LoadParameter(int index) {
            switch (index) {
                case 0:  Emit0(OpCodes.Ldarg_0); break;
                case 1:  Emit0(OpCodes.Ldarg_1); break;
                case 2:  Emit0(OpCodes.Ldarg_2); break;
                case 3:  Emit0(OpCodes.Ldarg_3); break;
                default: Emit0(OpCodes.Ldarg, index); break;
            }
        }

        /// <summary>
        /// Emit the code to load the address of the a parameter at
        /// the given index.
        /// </summary>
        /// <param name="index">Index of the parameter</param>
        public void LoadParameterAddress(int index) {
            if (index < 256) {
                Emit0(OpCodes.Ldarga_S, (byte)index);
            } else {
                Emit0(OpCodes.Ldarga, index);
            }
        }

        /// <summary>
        /// Emit the code to load the address of the given symbol.
        /// </summary>
        /// <param name="sym">A symbol</param>
        public void LoadFunction(Symbol sym) {
            if (sym == null) {
                throw new ArgumentNullException(nameof(sym));
            }
            if (sym.Info is JMethod method) {
                Emit0(OpCodes.Ldftn, method.Builder);
            }
            if (sym.Info is MethodInfo methodInfo) {
                Emit0(OpCodes.Ldftn, methodInfo);
            }
        }

        /// <summary>
        /// Emit the code to load an array element given indexes pushed
        /// on the top of the stack.
        /// </summary>
        /// <param name="sym">Array symbol</param>
        public void LoadArrayElement(Symbol sym) {
            if (sym == null) {
                throw new ArgumentNullException(nameof(sym));
            }
            if (sym.Dimensions.Count > 1 && !sym.IsFlatArray) {
                Type [] paramTypes = new Type[sym.Dimensions.Count];
                for (int c = 0; c < sym.Dimensions.Count; ++c) {
                    paramTypes[c] = typeof(int);
                }
                Type baseType = sym.SystemType;
                Emit0(OpCodes.Call, baseType.GetMethod("Get", paramTypes));
            } else {
                Emit0(OpCodes.Ldelem, Symbol.SymTypeToSystemType(sym.Type));
            }
        }

        /// <summary>
        /// Emit the code to load the address of an array element
        /// given indexes pushed on the top of the stack.
        /// </summary>
        /// <param name="sym">Array symbol</param>
        public void LoadArrayElementReference(Symbol sym) {
            if (sym == null) {
                throw new ArgumentNullException(nameof(sym));
            }
            if (sym.Dimensions.Count > 1 && !sym.IsFlatArray) {
                Type [] paramTypes = new Type[sym.Dimensions.Count];
                for (int c = 0; c < sym.Dimensions.Count; ++c) {
                    paramTypes[c] = typeof(int);
                }
                Type baseType = sym.SystemType;
                Emit0(OpCodes.Call, baseType.GetMethod("Address", paramTypes));
            } else {
                Emit0(OpCodes.Ldelema, Symbol.SymTypeToSystemType(sym.Type));
            }
        }

        /// <summary>
        /// Emit the code to store a value indirectly. The stack must contain
        /// two values: the value to store and then the indirection offset.
        /// </summary>
        /// <param name="type">Type of the value to be stored</param>
        public void StoreIndirect(SymType type) {
            switch (type) {
                case SymType.DOUBLE:    Emit0(OpCodes.Stind_R8); break;
                case SymType.FLOAT:     Emit0(OpCodes.Stind_R4); break;
                case SymType.INTEGER:   Emit0(OpCodes.Stind_I4); break;
                case SymType.BOOLEAN:   Emit0(OpCodes.Stind_I4); break;

                default:
                    Debug.Assert(false, $"StoreIndirect: Unsupported type {type}");
                    break;
            }
        }

        /// <summary>
        /// Emit the code to store a local variable onto the stack. Different code
        /// is emitted depending on whether the variable is a static.
        /// </summary>
        /// <param name="sym">A Symbol object representing the variable</param>
        /// <returns>The SymType of the variable loaded</returns>
        public SymType StoreLocal(Symbol sym) {
            if (sym == null) {
                throw new ArgumentNullException(nameof(sym));
            }
            if (sym.IsInCommon) {
                Symbol symCommon = sym.Common;
                List<Symbol> commonList = (List<Symbol>)symCommon.Info;
                sym = commonList[sym.CommonIndex];
            }
            if (sym.IsStatic) {
                StoreStatic((FieldInfo)sym.Info);
            } else {
                StoreLocal(sym.Index);
            }
            return sym.Type;
        }

        /// <summary>
        /// Emit the appropriate store local index opcode.
        /// </summary>
        /// <param name="value">The local variable reference</param>
        public void StoreLocal(LocalDescriptor value) {
            Emit0(OpCodes.Stloc, value);
        }

        /// <summary>
        /// Emit the code to store to a static.
        /// </summary>
        /// <param name="fi">The FieldInfo structure</param>
        public void StoreStatic(FieldInfo fi) {
            Emit0(OpCodes.Stsfld, fi);
        }

        /// <summary>
        /// Emit the code to save the value at the top of the stack to
        /// the parameter index.
        /// </summary>
        /// <param name="index">Index of the parameter</param>
        public void StoreParameter(int index) {
            if (index < 256) {
                Emit0(OpCodes.Starg_S, (byte)index);
            } else {
                Emit0(OpCodes.Starg, index);
            }
        }

        /// <summary>
        /// Emit the code to store the value on the top of the stack.
        /// </summary>
        /// <param name="type">Type to store</param>
        public void StoreElement(SymType type) {
            Emit0(OpCodes.Stelem, Symbol.SymTypeToSystemType(type));
        }

        /// <summary>
        /// Emit the code to load an array element given indexes pushed
        /// on the top of the stack.
        /// </summary>
        /// <param name="sym">Array symbol</param>
        public void StoreArrayElement(Symbol sym) {
            if (sym == null) {
                throw new ArgumentNullException(nameof(sym));
            }
            if (sym.Dimensions.Count > 1 && !sym.IsFlatArray) {
                Type [] paramTypes = new Type[sym.Dimensions.Count + 1];
                for (int c = 0; c < sym.Dimensions.Count; ++c) {
                    paramTypes[c] = typeof(int);
                }
                Type baseType = sym.SystemType;
                paramTypes[sym.Dimensions.Count] = Symbol.SymTypeToSystemType(sym.Type);
                Emit0(OpCodes.Call, baseType.GetMethod("Set", paramTypes));
            } else {
                StoreElement(sym.Type);
            }
        }

        /// <summary>
        /// Emit the code to store the address of an element.
        /// </summary>
        public void StoreElementAddress() {
            Emit0(OpCodes.Box, typeof(IntPtr));
            Emit0(OpCodes.Stelem, typeof(object));
        }

        /// <summary>
        /// Emit the code to store the value on the top of the stack
        /// as a reference type.
        /// </summary>
        /// <param name="type">Type to store</param>
        public void StoreElementReference(SymType type) {
            switch (type) {
                case SymType.INTEGER:
                case SymType.BOOLEAN:
                case SymType.FLOAT:
                case SymType.DOUBLE:
                case SymType.COMPLEX:
                    Type sysType = Symbol.SymTypeToSystemType(type);
                    Emit0(OpCodes.Box, sysType);
                    break;
            }
            Emit0(OpCodes.Stelem_Ref);
        }

        /// <summary>
        /// Emit the code to load the value on the top of the stack
        /// as a reference type.
        /// </summary>
        /// <param name="type">Type to store</param>
        public void LoadElementReference(SymType type) {
            Emit0(OpCodes.Ldelem_Ref);
            switch (type) {
                case SymType.INTEGER:
                case SymType.BOOLEAN:
                case SymType.FLOAT:
                case SymType.DOUBLE:
                case SymType.COMPLEX:
                case SymType.FIXEDCHAR:
                case SymType.CHAR:
                    Type sysType = Symbol.SymTypeToSystemType(type);
                    Emit0(OpCodes.Unbox_Any, sysType);
                    break;
            }
        }

        /// <summary>
        /// Emit an opcode with no operands.
        /// </summary>
        /// <param name="op">The opcode to emit</param>
        public void Emit0(OpCode op) {
            _code.Add(new Instruction(op));
        }
        
        /// <summary>
        /// Emit an opcode with a MethodInfo parameter.
        /// </summary>
        /// <param name="op">The opcode to emit</param>
        /// <param name="operand">A MethodInfo parameter</param>
        public void Emit0(OpCode op, MethodInfo operand) {
            _code.Add(new InstructionMethod(op, operand));
        }
        
        /// <summary>
        /// Emit an opcode with a ConstructorInfo token.
        /// </summary>
        /// <param name="op">The opcode to emit</param>
        /// <param name="operand">A ConstructorInfo parameter</param>
        public void Emit0(OpCode op, ConstructorInfo operand) {
            _code.Add(new InstructionCtor(op, operand));
        }

        /// <summary>
        /// Obtain a temporary variable of the given type.
        /// </summary>
        /// <param name="type">The type requested</param>
        /// <returns>A local descriptor that references the temporary variable</returns>
        public LocalDescriptor GetTemporary(Type type) {
            foreach (LocalDescriptor t in _temp) {
                if (t.Type == type && !t.InUse) {
                    t.InUse = true;
                    return t;
                }
            }
            return CreateLocal(type);
        }

        /// <summary>
        /// Releases the specified temporary variable for reuse.
        /// </summary>
        /// <param name="temp">A local descriptor for the temporary variable</param>
        public static void ReleaseTemporary(LocalDescriptor temp) {
            if (temp == null) {
                throw new ArgumentNullException(nameof(temp));
            }
            temp.InUse = false;
        }

        // Create a local descriptor for a local variable at the
        // given index and with the given type.
        private LocalDescriptor AssignLocal(Type type, int index) {
            LocalDescriptor newTemp = new() {
                Type = type,
                Index = index,
                InUse = true
            };
            _temp.Add(newTemp);
            return newTemp;
        }

        // Emit an opcode with an 8-bit destination
        private void Emit0(OpCode op, byte operand) {
            _code.Add(new InstructionByte(op, operand));
        }
        
        // Emit an opcode with a branch destination
        private void Emit0(OpCode op, Label operand) {
            _code.Add(new InstructionLabel(op, operand));
        }

        // Emit an opcode with a FieldInfo token
        private void Emit0(OpCode op, FieldInfo operand) {
            _code.Add(new InstructionField(op, operand));
        }

        // Emit an opcode with a type
        private void Emit0(OpCode op, Type operand) {
            _code.Add(new InstructionType(op, operand));
        }

        // Emit an opcode with a Label array
        private void Emit0(OpCode op, Label [] operand) {
            _code.Add(new InstructionLabelArray(op, operand));
        }

        // Emit an opcode with an local variable
        private void Emit0(OpCode op, LocalDescriptor value) {
            _code.Add(new InstructionLocal(op, value));
        }

        // Emit an opcode with an integer
        private void Emit0(OpCode op, int operand) {
            _code.Add(new InstructionInt(op, operand));
        }

        // Emit an opcode with a float
        private void Emit0(OpCode op, float operand) {
            _code.Add(new InstructionFloat(op, operand));
        }

        // Emit an opcode with a double
        private void Emit0(OpCode op, double operand) {
            _code.Add(new InstructionDouble(op, operand));
        }

        // Emit an opcode with a string
        private void Emit0(OpCode op, string operand) {
            _code.Add(new InstructionString(op, operand));
        }

        // Emit an opcode to Calli.
        private void Emit0(OpCode op, CallingConventions unmanagedCallConv, Type returnType, Type[] parameterTypes) {
            _code.Add(new InstructionCalli(op, unmanagedCallConv, returnType, parameterTypes));
        }
    }
}
