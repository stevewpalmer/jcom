// JCom Compiler Toolkit
// Core code generation class
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using JComLib;

namespace CCompiler {
    
    /// <summary>
    /// Defines a single execution result that holds the result
    /// of calling Execute on the generated code.
    /// </summary>
    public class ExecutionResult {

        /// <summary>
        /// Gets or sets whether the execution succeeded.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets an object representing the return value
        /// of the method executed.
        /// </summary>
        public object Result { get; set; }
    }

    /// <summary>
    /// Defines the code generation class for the compiler.
    /// </summary>
    public class CodeGenerator {
        private Program _prog;
        private readonly Options _opts;
        private string _filename;
        private int _lineno;

        /// <summary>
        /// Constructs a language neutral code generator object.
        /// </summary>
        /// <param name="opts">Compiler options</param>
        public CodeGenerator(Options opts) {
            _opts = opts;
            _lineno = -1;
        }

        /// <summary>
        /// Save the generated assembly to disk. This requires a filename
        /// to have been previously set or this does nothing.
        /// </summary>
        public void Save() {
            try {
                _prog.Save();
            } catch (IOException) {
                Error($"Cannot write to output file {_prog.OutputFilename}");
            }
        }

        /// <summary>
        /// Execute the generated code and return the result.
        /// </summary>
        /// <param name="methodName">The name of the method to be invoked.</param>
        /// <returns>The result as an object</returns>
        public ExecutionResult Run(string methodName) {
            try {
                ExecutionResult execResult = new() {
                    Success = false,
                    Result = null
                };

                Type mainType = _prog.GetMainType();
                if (mainType != null) {
                    MethodInfo mi = mainType.GetMethod(methodName);
                    if (mi != null) {
                        execResult.Result = mi.Invoke(null, null);
                        execResult.Success = true;
                    }
                }
                return execResult;
            } catch (TargetInvocationException e) {
                throw e.InnerException ?? e;
            }
        }

        /// <summary>
        /// Generate the code for the entire parse tree as specified by the
        /// given program definition.
        /// </summary>
        /// <param name="programDef">A program definition object</param>
        public void GenerateCode(ProgramDefinition programDef) {
            if (programDef == null) {
                throw new ArgumentNullException(nameof(programDef));
            }
            try {
                _prog = new Program(_opts, programDef.Name, programDef.IsExecutable);

                GenerateSymbols(programDef.Globals);

                foreach (ParseNode node in programDef.Root.Nodes) {
                    node.Generate(this);
                }
            } catch (Exception e) {
                if (_opts.DevMode) {
                    throw;
                }
                Error($"Compiler error: {e.Message}");
            }
        }

        /// <summary>
        /// Throws an error exception that includes the line number and file name of the
        /// location of the error.
        /// </summary>
        /// <param name="lineNumber">The number of the line</param>
        /// <param name="errorString">The error string</param>
        public void Error(int lineNumber, string errorString) {
            throw new CodeGeneratorException(lineNumber, _filename, errorString);
        }

        /// <summary>
        /// Throws an error exception that includes the line number and file name of the
        /// location of the error.
        /// </summary>
        /// <param name="errorString">The error string</param>
        public void Error(string errorString) {
            throw new CodeGeneratorException(_lineno, _filename, errorString);
        }

        /// <summary>
        /// Return the current emitter.
        /// </summary>
        /// <value>The emitter.</value>
        public Emitter Emitter { get; set; }

        /// <summary>
        /// Gets or sets the parse node of the current procedure being
        /// compiled.
        /// </summary>
        /// <value>The current method symbol entry</value>
        public ProcedureParseNode CurrentProcedure { get; set; }

        /// <summary>
        /// Gets the MethodInfo for the given method name in the type and throws an error exception
        /// if it is not found. The return value is thus guaranteed to be non-null.
        /// </summary>
        /// <param name="baseTypeName">The name of the type containing the method</param>
        /// <param name="methodName">The method name</param>
        /// <param name="paramTypes">An array of parameter types</param>
        /// <returns>The method for the given base type</returns>
        public MethodInfo GetMethodForType(string baseTypeName, string methodName, Type[] paramTypes) {
            if (baseTypeName == null) {
                throw new ArgumentNullException(nameof(baseTypeName));
            }
            if (methodName == null) {
                throw new ArgumentNullException(nameof(methodName));
            }
            Type baseType;
            if (!baseTypeName.Contains(",")) {

                // This is a Comal external library so, for the purpose of simplifying
                // the syntax, we make some assumptions:
                //
                // 1. The library is in the same folder as the current program.
                // 2. The library is a ".dll" if no explicit extension is given.
                // 3. The namespace is the library basename.
                // 4. The class is the library basename.
                //
                // Assumptions 3 and 4 derive from the way CCompiler builds the
                // library. The class name is always the source filename without
                // an extension and formatted as per CapitaliseString().
                //
                // If more flexibility is required, the caller simply needs to specify
                // the ExternalLibrary fully qualified. E.g:
                //
                // Namespace.Class,Assembly
                //
                string currentDirectory = Directory.GetCurrentDirectory();
                string typeDllPath = Path.Combine(currentDirectory, baseTypeName);
                if (string.IsNullOrEmpty(Path.GetExtension(typeDllPath))) {
                    typeDllPath = Path.ChangeExtension(typeDllPath, ".dll");
                }
                var dll = Assembly.LoadFile(typeDllPath);

                string name = Path.GetFileNameWithoutExtension(baseTypeName).CapitaliseString();
                baseType = dll.GetType(name + "." + name);
            } else {
                baseType = Type.GetType(baseTypeName);
            }
            if (baseType == null) {
                Error($"Type {baseTypeName} not found");
            }
            return GetMethodForType(baseType, methodName, paramTypes);
        }

        /// <summary>
        /// Gets the MethodInfo for the given method name in the type and throws an error exception
        /// if it is not found. The return value is thus guaranteed to be non-null.
        /// </summary>
        /// <param name="baseType">The type containing the method</param>
        /// <param name="methodName">The method name</param>
        /// <param name="paramTypes">An array of parameter types</param>
        /// <returns>The method for the given base type</returns>
        public MethodInfo GetMethodForType(Type baseType, string methodName, Type[] paramTypes) {
            if (baseType == null) {
                throw new ArgumentNullException(nameof(baseType));
            }
            if (methodName == null) {
                throw new ArgumentNullException(nameof(methodName));
            }
            MethodInfo meth;
            try {
                meth = baseType.GetMethod(methodName, paramTypes);
            } catch (AmbiguousMatchException) {
                meth = null;
            }
            if (meth == null) {
                Error($"{methodName} method not found for type {baseType}");
            }
            Debug.Assert(meth != null);
            return meth;
        }

        /// <summary>
        /// Marks the file.
        /// </summary>
        /// <param name="filename">Name of the file to emit</param>
        public void MarkFile(string filename) {
            if (_opts.GenerateDebug) {
                _prog?.SetCurrentDocument(filename);
            }
            _filename = filename;
        }

        /// <summary>
        /// Marks the line.
        /// </summary>
        /// <param name="line">Line number to emit to the output</param>
        public void MarkLine(int line) {
            if (_prog != null && Emitter != null) {
                ISymbolDocumentWriter currentDoc = _prog.GetCurrentDocument();
                if (_opts.GenerateDebug && currentDoc != null) {
                    Emitter.MarkLinenumber(currentDoc, line);
                }
            }
            _lineno = line;
        }

        /// <summary>
        /// Emit the code to generate the referenced symbols from the given symbol
        /// collection. Where a value is specified, we also initialise the symbol
        /// with the given value.
        /// </summary>
        /// <param name="symbols">Symbol collection</param>
        public void GenerateSymbols(SymbolCollection symbols) {
            if (symbols == null) {
                throw new ArgumentNullException(nameof(symbols));
            }
            bool needConstructor = false;
            foreach (Symbol sym in symbols) {
                if (sym.IsImported) {
                    continue;
                }
                if (sym.IsMethod && sym.Defined && !sym.IsParameter) {
                    _prog.CreateMethod(sym);
                    continue;
                }
                if (sym.IsArray) {
                    InitDynamicArray(sym);
                }
                if (sym.IsReferenced) {
                    switch (sym.Type) {
                        case SymType.GENERIC:
                            if (sym.IsStatic) {
                                // Static array of objects
                                sym.Info = StaticEmitter.CreateStatic(_prog.GetCurrentType(), sym);
                                needConstructor = true;
                            }
                            break;

                        case SymType.DOUBLE:
                        case SymType.CHAR:
                        case SymType.FIXEDCHAR:
                        case SymType.INTEGER:
                        case SymType.FLOAT:
                        case SymType.COMPLEX:
                        case SymType.BOOLEAN: {
                            if (sym.IsLocal && !sym.IsIntrinsic && !sym.IsReferenceCommon && !sym.IsMethod) {
                                if (sym.IsFixedStatic) {
                                    sym.Info = StaticEmitter.CreateFixedStatic(_prog.GetCurrentType(), sym);
                                } else if (sym.IsStatic) {
                                    sym.Info = StaticEmitter.CreateStatic(_prog.GetCurrentType(), sym);
                                    needConstructor = true;
                                } else {
                                    sym.Index = Emitter.CreateLocal(sym);
                                }
                                if (sym.IsArray) {
                                    Emitter.CreateArray(sym);
                                    Emitter.StoreLocal(sym);
                                    if (sym.Type == SymType.FIXEDCHAR) {
                                        InitFixedStringArray(sym);
                                    }
                                } else if (sym.Type == SymType.FIXEDCHAR) {
                                    Emitter.CreateString(sym);
                                    Emitter.StoreLocal(sym);
                                }
                                if (!sym.IsFixedStatic && Emitter != null) {
                                    Emitter.InitialiseSymbol(sym);
                                }
                            }
                            break;
                        }

                        case SymType.LABEL:
                            sym.Info = Emitter.CreateLabel();
                            break;
                    }
                }
            }

            // If we have any statics then we need a constructor to
            // initialise those statics.
            if (needConstructor) {
                Emitter ctorEmitter = null;
                foreach (Symbol sym in symbols) {
                    if (sym.IsStatic && !sym.IsFixedStatic && sym.CanInitialise && sym.IsReferenced) {
                        if (ctorEmitter == null) {
                            ctorEmitter = _prog.CreateConstructor();
                        }
                        ctorEmitter.InitialiseSymbol(sym);
                    }
                }
                if (ctorEmitter != null) {
                    ctorEmitter.Emit0(OpCodes.Ret);
                    ctorEmitter.Save();
                }
            }
        }

        // Initialise a dynamic array.
        // If the array's dimensions are non-integral then we pre-calculate the dimension
        // bound and store locally, and set the dimension reference to that local element.
        private void InitDynamicArray(Symbol sym) {
            foreach (SymDimension dim in sym.Dimensions) {
                if (!dim.LowerBound.IsConstant) {
                    LocalDescriptor lowBound = Emitter.GetTemporary(typeof(int));
                    GenerateExpression(SymType.INTEGER, dim.LowerBound);
                    Emitter.StoreLocal(lowBound);
                    dim.LowerBound = new LocalParseNode(lowBound);
                }
                if (!dim.UpperBound.IsConstant) {
                    LocalDescriptor upperBound = Emitter.GetTemporary(typeof(int));
                    GenerateExpression(SymType.INTEGER, dim.UpperBound);
                    Emitter.StoreLocal(upperBound);
                    dim.UpperBound = new LocalParseNode(upperBound);
                }
            }
        }

        // Generate the code to initialise a fixed string array by calling
        // the Length on every element.
        private void InitFixedStringArray(Symbol sym) {
            if (sym.Dimensions.Count > 1 && !sym.IsFlatArray) {
                Error("Cannot initialise multi-dimensional arrays");
            } else {
                LocalDescriptor count = Emitter.GetTemporary(typeof(int));
                Label loopStart = Emitter.CreateLabel();
                Emitter.LoadInteger(0);
                Emitter.StoreLocal(count);
                Emitter.MarkLabel(loopStart);
                LoadLocal(sym);
                Emitter.LoadLocal(count);
                Emitter.LoadInteger(sym.FullType.Width);
                Emitter.CreateObject(typeof(FixedString), new [] { typeof(int) });
                Emitter.StoreArrayElement(sym);
                Emitter.LoadLocal(count);
                Emitter.LoadInteger(1);
                Emitter.Add(SymType.INTEGER);
                Emitter.StoreLocal(count);
                Emitter.LoadLocal(count);
                Emitter.LoadInteger(sym.ArraySize);
                Emitter.BranchLess(loopStart);
                Emitter.ReleaseTemporary(count);
            }
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
                LoadLocal(sym);
            } else {
                GenerateLoadArgument(sym);
            }
            return SymType.REF;
        }

        /// <summary>
        /// Emit code that loads a value from an array. There are some special cases here.
        /// </summary>
        /// <param name="identNode">Ident parse node</param>
        /// <param name="useRef">If set to <c>true</c> load the element address</param>
        /// <returns>The symbol type of the array element</returns>
        public SymType GenerateLoadFromArray(IdentifierParseNode identNode, bool useRef) {
            if (identNode == null) {
                throw new ArgumentNullException(nameof(identNode));
            }
            Symbol sym = identNode.Symbol;

            // Handle loading the base array as opposed to an element
            Debug.Assert(sym.IsArray);
            if (identNode.IsArrayBase) {
                return GenerateLoadArray(identNode, useRef);
            }

            // OK, we're loading an array element.
            GenerateLoadArrayAddress(identNode);
            if (useRef) {
                Emitter.LoadArrayElementReference(sym);
            } else {
                Emitter.LoadArrayElement(sym);
            }
            return sym.Type;
        }

        /// <summary>
        /// Emit the code that loads the base array and the offset of the
        /// indexed element to the top of the stack.
        /// </summary>
        /// <param name="identNode">Parse node for array identifier</param>
        public void GenerateLoadArrayAddress(IdentifierParseNode identNode) {
            if (identNode == null) {
                throw new ArgumentNullException(nameof(identNode));
            }

            Symbol sym = identNode.Symbol;
            if (sym.IsLocal) {
                LoadLocal(sym);
            } else {
                GenerateLoadArgument(sym);
            }
            for (int c = 0; c < identNode.Indexes.Count; ++c) {
                ParseNode indexNode = identNode.Indexes[c];
                if (indexNode.IsConstant) {
                    NumberParseNode intNode = (NumberParseNode)indexNode;
                    if (sym.Dimensions[c].LowerBound.IsConstant) {
                        Emitter.LoadInteger(0 - sym.Dimensions[c].LowerBound.Value.IntValue + intNode.Value.IntValue);
                    } else {
                        Emitter.LoadInteger(0);
                        GenerateExpression(SymType.INTEGER, sym.Dimensions[c].LowerBound);
                        Emitter.Sub(SymType.INTEGER);
                        Emitter.LoadInteger(intNode.Value.IntValue);
                        Emitter.Add(SymType.INTEGER);
                    }
                } else {
                    GenerateExpression(SymType.INTEGER, indexNode);
                    if (sym.Dimensions[c].LowerBound.IsConstant) {
                        int lowBound = sym.Dimensions [c].LowerBound.Value.IntValue;
                        if (lowBound != 0) {
                            Emitter.LoadInteger(0 - lowBound);
                            Emitter.Add(SymType.INTEGER);
                        }
                    } else {
                        Emitter.LoadInteger(0);
                        GenerateExpression(SymType.INTEGER, sym.Dimensions[c].LowerBound);
                        Emitter.Sub(SymType.INTEGER);
                        Emitter.Add(SymType.INTEGER);
                    }
                }
                if (sym.IsFlatArray && c > 0) {
                    for (int m = c - 1; m >= 0; --m) {
                        int arraySize = sym.Dimensions[m].Size;
                        if (arraySize >= 0) {
                            Emitter.LoadInteger(arraySize);
                        } else {
                            GenerateExpression(SymType.INTEGER, sym.Dimensions[m].UpperBound);
                            GenerateExpression(SymType.INTEGER, sym.Dimensions[m].LowerBound);
                            Emitter.Sub(SymType.INTEGER);
                            Emitter.LoadInteger(1);
                            Emitter.Add(SymType.INTEGER);
                        }
                        Emitter.Mul(SymType.INTEGER);
                    }
                    Emitter.Add(SymType.INTEGER);
                }
            }
        }

        /// <summary>
        /// Generate code for an expression tree.
        /// </summary>
        /// <param name="typeNeeded">The type to which the expression should be converted if it
        /// does not evaluate to that type natively.</param>
        /// <param name="rootNode">The ParseNode of the root of the expression tree.</param>
        /// <returns>The type of the generated expression</returns>
        public SymType GenerateExpression(SymType typeNeeded, ParseNode rootNode) {
            if (rootNode == null) {
                throw new ArgumentNullException(nameof(rootNode));
            }
            SymType thisType = rootNode.Generate(this, typeNeeded);
            return Emitter.ConvertType(thisType, typeNeeded);
        }

        /// <summary>
        /// Emit the code to load a local variable onto the stack. Different code
        /// is emitted depending on whether the variable is a static.
        /// </summary>
        /// <param name="sym">A Symbol object representing the variable</param>
        /// <returns>The SymType of the variable loaded</returns>
        public SymType LoadLocal(Symbol sym) {
            if (sym == null) {
                throw new ArgumentNullException(nameof(sym));
            }
            if (sym.IsInCommon) {
                Symbol symCommon = sym.Common;
                List<Symbol> commonList = (List<Symbol>)symCommon.Info;
                sym = commonList[sym.CommonIndex];
            }
            if (sym.IsStatic) {
                Emitter.LoadStatic((FieldInfo)sym.Info);
            } else {
                Emitter.LoadLocal(sym.Index);
            }
            return sym.Type;
        }

        /// <summary>
        /// Emit the load of an address of full symbol. This may either be
        /// the address of a local object or the address of an array element.
        /// </summary>
        /// <param name="identNode">An IdentifierParseNode representing the variable
        /// or array element whose address should be emitted.</param>
        public void LoadAddress(IdentifierParseNode identNode) {
            if (identNode == null) {
                throw new ArgumentNullException(nameof(identNode));
            }
            Symbol sym = identNode.Symbol;
            if (sym.IsArray) {
                GenerateLoadFromArray(identNode, true);
            } else {
                GenerateLoadAddress(sym);
            }
        }

        // Emit the load of an address of a simple symbol
        private void GenerateLoadAddress(Symbol sym) {
            if (sym.IsStatic) {
                Emitter.LoadStaticAddress((FieldInfo)sym.Info);
            } else if (sym.IsMethod) {
                Emitter.LoadFunction(sym);
            } else if (sym.IsLocal) {
                Emitter.LoadLocalAddress(sym.Index);
            } else if (sym.IsParameter) {
                Emitter.LoadParameterAddress(sym.ParameterIndex);
            } else {
                Debug.Assert(false, $"Cannot load the address of {sym}");
            }
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
                    Emitter.LoadParameter(sym.ParameterIndex);
                    break;

                case SymLinkage.BYREF:
                    Emitter.LoadParameter(sym.ParameterIndex);
                    if (sym.IsValueType) {
                        Emitter.LoadIndirect(sym.Type);
                    }
                    break;
            }
        }

        /// <summary>
        /// Retrieve the label value of a SymbolParseNode.
        /// </summary>
        /// <param name="node">A Label parse node</param>
        /// <returns>A symbol entry representing the label</returns>
        public Symbol GetLabel(ParseNode node) {
            if (node == null) {
                return null;
            }
            Debug.Assert(node is SymbolParseNode);
            SymbolParseNode identNode = (SymbolParseNode)node;
            return identNode.Symbol;
        }
    }
}
