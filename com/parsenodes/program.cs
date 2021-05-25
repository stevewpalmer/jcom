// JCom Compiler Toolkit
// Top-level program builder
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
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace CCompiler {

    /// <summary>
    /// Specifies a parse node for a single program.
    /// </summary>
    public class ProgramParseNode : ParseNode {

        private readonly bool _isCOMVisible = true;
        private readonly bool _isCLSCompliant = true;
        private readonly Options _opts;

        private string _filename;
        private int _lineno;

        private AssemblyBuilder _ab;
        private ISymbolDocumentWriter _currentDoc;

        /// <summary>
        /// Constructs a language neutral code generator object.
        /// </summary>
        /// <param name="opts">Compiler options</param>
        public ProgramParseNode(Options opts) : base(ParseID.PROGRAM) {
            _opts = opts;
            _lineno = -1;
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
        /// Return the assembly's module builder.
        /// </summary>
        public ModuleBuilder Builder { get; set; }

        /// <summary>
        /// Gets or sets the parse node of the current procedure being
        /// compiled.
        /// </summary>
        /// <value>The current method symbol entry</value>
        public ProcedureParseNode CurrentProcedure { get; set; }

        /// <summary>
        /// Gets or sets the current type being compiled.
        /// </summary>
        /// <value>The current method symbol entry</value>
        public JType CurrentType { get; set; }

        /// <summary>
        /// Gets or sets the global symbol table.
        /// </summary>
        /// <value>The globals.</value>
        public SymbolCollection Globals { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this program is executable.
        /// An executable program has a Main method.
        /// </summary>
        /// <value><c>true</c> if this instance is executable; otherwise, <c>false</c>.</value>
        public bool IsExecutable { get; set; }

        /// <summary>
        /// Gets or sets the assembly version.
        /// </summary>
        /// <value>A string that specifies the assembly version</value>
        public string VersionString { get; set; }

        /// <summary>
        /// Gets or sets the output file name.
        /// </summary>
        /// <value>A string that specifies the output file name</value>
        public string OutputFile { get; set; }

        /// <summary>
        /// Specifies whether the assembly has debug information
        /// </summary>
        public bool GenerateDebug { get; set; }

        /// <summary>
        /// Gets or sets the program name.
        /// </summary>
        /// <value>A string that specifies the program name</value>
        public string Name { get; set; }

        /// <summary>
        /// Return the .NET version of the program name
        /// </summary>
        public string DotNetName {
            get {
                if (!string.IsNullOrEmpty(Name)) {
                    return Name.CapitaliseString();
                }
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets the root of the parse tree.
        /// </summary>
        /// <value>The parse tree root</value>
        public BlockParseNode Root { get; set; }

        /// <summary>
        /// Dumps the contents of this parse node to the ParseNode XML
        /// output under the specified parent node.
        /// </summary>
        /// <param name="root">The parent XML node</param>
        public override void Dump(ParseNodeXml root) {
            ParseNodeXml blockNode = root.Node("Program");
            blockNode.Attribute("Name", Name);
            blockNode.Attribute("OutputFile", OutputFile);
            blockNode.Attribute("VersionString", VersionString);
            blockNode.Attribute("IsExecutable", IsExecutable.ToString());
            blockNode.Attribute("GenerateDebug", GenerateDebug.ToString());
            Globals.Dump(blockNode);
            Root.Dump(blockNode);
        }

        /// <summary>
        /// Generate the code for the entire parse tree as specified by the
        /// given program definition.
        /// </summary>
        /// <param name="programDef">A program definition object</param>
        public void Generate() {
            try {
                AppDomain ad = AppDomain.CurrentDomain;
                AssemblyName an = new();

                an.Name = DotNetName;
                an.Version = new Version(VersionString);

                bool isSaveable = !string.IsNullOrEmpty(OutputFile);
                AssemblyBuilderAccess access = isSaveable ? AssemblyBuilderAccess.RunAndSave : AssemblyBuilderAccess.Run;
                _ab = ad.DefineDynamicAssembly(an, access);

                // Don't make the main class abstract if the program is being run from
                // memory as otherwise the caller will be unable to create an instance.
                if (isSaveable) {
                    Builder = _ab.DefineDynamicModule(DotNetName, OutputFilename, GenerateDebug);
                } else {
                    Builder = _ab.DefineDynamicModule(DotNetName, GenerateDebug);
                }

                // Make this assembly debuggable if the debug option was specified.
                if (GenerateDebug) {
                    AddDebuggable();
                }

                // All code below here needs to move to a TypeParseNode that defines a
                // single class/type. A TypeParseNode should be added as the child of a
                // ProgramParseNode by the client.

                // Create an implicit namespace using the output file name if
                // one is specified.
                string className = string.Empty;
                if (!string.IsNullOrEmpty(OutputFile)) {
                    className = string.Concat(OutputFile.CapitaliseString(), ".");
                }
                className = string.Concat(className, DotNetName);

                // Create the default type
                TypeAttributes typeAttributes = TypeAttributes.Public;
                if (isSaveable) {
                    typeAttributes |= TypeAttributes.BeforeFieldInit | TypeAttributes.Sealed;
                }
                CurrentType = new JType(Builder, className, typeAttributes);

                Globals.GenerateSymbols(this);
                foreach (ParseNode node in Root.Nodes) {
                    node.Generate(this);
                }

                // Close the constructor
                Emitter ctorEmitter = CurrentType.DefaultConstructor.Emitter;
                if (ctorEmitter != null) {
                    ctorEmitter.Emit0(OpCodes.Ret);
                    ctorEmitter.Save();
                }
            }
            catch (Exception e) {
                if (_opts.DevMode) {
                    throw;
                }
                Error($"Compiler error: {e.Message}");
            }
        }

        /// <summary>
        /// Save the generated assembly to disk. This requires a filename
        /// to have been previously set or this does nothing.
        /// </summary>
        public void Save() {
            try {
                AddCLSCompliant();
                AddCOMVisiblity();

                _ = CurrentType.CreateType;
                _ab.Save(OutputFilename);
            }
            catch (IOException) {
                Error($"Cannot write to output file {OutputFilename}");
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

                Type mainType = CurrentType.CreateType;
                if (mainType != null) {
                    MethodInfo mi = mainType.GetMethod(methodName);
                    if (mi != null) {
                        execResult.Result = mi.Invoke(null, null);
                        execResult.Success = true;
                    }
                }
                return execResult;
            }
            catch (TargetInvocationException e) {
                throw e.InnerException ?? e;
            }
        }

        /// <summary>
        /// Sets the specified method as the program start method.
        /// </summary>
        /// <param name="method">Method object</param>
        public void SetEntryPoint(JMethod method) {
            _ab.SetEntryPoint(method.Builder);
        }

        /// <summary>
        /// Sets the filename in the debug info.
        /// </summary>
        /// <param name="filename">Filename.</param>
        public void SetCurrentDocument(string filename) {
            _currentDoc = Builder.DefineDocument(filename, Guid.Empty, Guid.Empty, Guid.Empty);
        }

        /// <summary>
        /// Retrieves the current document.
        /// </summary>
        /// <returns>The current document.</returns>
        public ISymbolDocumentWriter GetCurrentDocument() {
            return _currentDoc;
        }

        /// <summary>
        /// Gets the output filename complete with extension.
        /// </summary>
        /// <value>The output filename.</value>
        public string OutputFilename {
            get {
                string outputFilename = Path.GetFileName(OutputFile);
                if (!Path.HasExtension(outputFilename)) {
                    outputFilename = Path.ChangeExtension(outputFilename, IsExecutable ? "exe" : "dll");
                }
                return outputFilename;
            }
        }

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
                Assembly dll = Assembly.LoadFile(typeDllPath);

                string name = Path.GetFileNameWithoutExtension(baseTypeName).CapitaliseString();
                baseType = dll.GetType(name + "." + name);
            } else {
                baseType = System.Type.GetType(baseTypeName);
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
        /// Sets the name of the file being compiled.
        /// </summary>
        /// <param name="filename">Name of the file being compiled</param>
        public void MarkFile(string filename) {
            if (GenerateDebug) {
                SetCurrentDocument(filename);
            }
            _filename = filename;
        }

        /// <summary>
        /// Sets the number of the line in the file being compiled.
        /// </summary>
        /// <param name="line">Line number to emit to the output</param>
        public void MarkLine(int line) {
            if (GenerateDebug && Emitter != null) {
                ISymbolDocumentWriter currentDoc = GetCurrentDocument();
                if (currentDoc != null) {
                    Emitter.MarkLinenumber(currentDoc, line);
                }
            }
            _lineno = line;
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
                Emitter.GenerateLoadAddress(sym);
            }
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
                return Emitter.GenerateLoadArray(identNode, useRef);
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
                Emitter.LoadSymbol(sym);
            } else {
                Emitter.GenerateLoadArgument(sym);
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
        /// Retrieve the label value of a SymbolParseNode.
        /// </summary>
        /// <param name="node">A Label parse node</param>
        /// <returns>A symbol entry representing the label</returns>
        public static Symbol GetLabel(ParseNode node) {
            if (node == null) {
                return null;
            }
            Debug.Assert(node is SymbolParseNode);
            SymbolParseNode identNode = (SymbolParseNode)node;
            return identNode.Symbol;
        }

        // Make this assembly fully debuggable.
        private void AddDebuggable() {
            Type type = typeof(DebuggableAttribute);
            ConstructorInfo ctor = type.GetConstructor(new[] { typeof(DebuggableAttribute.DebuggingModes) });
            CustomAttributeBuilder caBuilder = new(ctor, new object[] {
                                                        DebuggableAttribute.DebuggingModes.DisableOptimizations |
                                                        DebuggableAttribute.DebuggingModes.Default });
            Builder.SetCustomAttribute(caBuilder);
        }

        // Mark this assembly as CLS Compliant.
        private void AddCLSCompliant() {
            Type type = typeof(CLSCompliantAttribute);
            ConstructorInfo ctor = type.GetConstructor(new[] { typeof(bool) });
            CustomAttributeBuilder caBuilder = new(ctor, new object[] { _isCLSCompliant });
            Builder.SetCustomAttribute(caBuilder);
        }

        // Mark this assembly as COM Visible.
        private void AddCOMVisiblity() {
            Type type = typeof(ComVisibleAttribute);
            ConstructorInfo ctor = type.GetConstructor(new[] { typeof(bool) });
            CustomAttributeBuilder caBuilder = new(ctor, new object[] { _isCOMVisible });
            Builder.SetCustomAttribute(caBuilder);
        }
    }
}
