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
using System.Reflection;

namespace CCompiler {

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
        public IEnumerator<Symbol> GetEnumerator() {
            return _symbols.Values.GetEnumerator();
        }

        // Non-generic enumerator
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        /// <summary>
        /// Emit the code to generate the referenced symbols from the given symbol
        /// collection. Where a value is specified, we also initialise the symbol
        /// with the given value.
        /// </summary>
        /// <param name="symbols">Symbol collection</param>
        public void GenerateSymbols(ProgramParseNode cg) {

            foreach (Symbol sym in this) {
                if (sym.IsImported) {
                    continue;
                }

                // Methods may be defined but not reference, but still need to be
                // created as they may be exported. (Should we automatically set
                // the reference flag on them?)
                if (sym.IsMethod && sym.Defined && !sym.IsParameter) {

                    // Don't make the entrypoint method public if we're saving this program
                    // to a file. However make it public if the -run option is specified as
                    // otherwise we can't access the entrypoint method internally.
                    MethodAttributes methodAttributes = MethodAttributes.Static;
                    if (!sym.Modifier.HasFlag(SymModifier.ENTRYPOINT) || sym.IsExported) {
                        methodAttributes |= MethodAttributes.Public;
                    }

                    JMethod metb = cg.CurrentType.CreateMethod(sym, methodAttributes);

                    sym.Info = metb;
                    if (sym.Modifier.HasFlag(SymModifier.ENTRYPOINT)) {
                        cg.SetEntryPoint(metb);
                    }
                    continue;
                }
                if (!sym.IsReferenced) {
                    continue;
                }
                Debug.Assert(cg.Emitter != null, "Emitter required for non-method symbols!");
                if (sym.IsArray) {
                    if (sym.IsStatic) {
                        foreach (SymDimension dim in sym.Dimensions) {
                            if (!dim.LowerBound.IsConstant) {
                                FieldInfo lowBound = cg.CurrentType.TemporaryField(typeof(int));
                                cg.GenerateExpression(SymType.INTEGER, dim.LowerBound);
                                cg.Emitter.StoreStatic(lowBound);
                                dim.LowerBound = new StaticParseNode(lowBound);
                            }
                            if (!dim.UpperBound.IsConstant) {
                                FieldInfo upperBound = cg.CurrentType.TemporaryField(typeof(int));
                                cg.GenerateExpression(SymType.INTEGER, dim.UpperBound);
                                cg.Emitter.StoreStatic(upperBound);
                                dim.UpperBound = new StaticParseNode(upperBound);
                            }
                        }
                    } else {
                        foreach (SymDimension dim in sym.Dimensions) {
                            if (!dim.LowerBound.IsConstant) {
                                LocalDescriptor lowBound = cg.Emitter.GetTemporary(typeof(int));
                                cg.GenerateExpression(SymType.INTEGER, dim.LowerBound);
                                cg.Emitter.StoreLocal(lowBound);
                                dim.LowerBound = new LocalParseNode(lowBound);
                            }
                            if (!dim.UpperBound.IsConstant) {
                                LocalDescriptor upperBound = cg.Emitter.GetTemporary(typeof(int));
                                cg.GenerateExpression(SymType.INTEGER, dim.UpperBound);
                                cg.Emitter.StoreLocal(upperBound);
                                dim.UpperBound = new LocalParseNode(upperBound);
                            }
                        }
                    }
                }
                switch (sym.Type) {
                    case SymType.GENERIC:
                        if (sym.IsStatic) {
                            // Static array of objects
                            sym.Info = cg.CurrentType.CreateField(sym);
                            cg.Emitter.InitialiseSymbol(sym);
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
                                if (sym.IsStatic) {
                                    sym.Info = cg.CurrentType.CreateField(sym);
                                } else {
                                    sym.Index = cg.Emitter.CreateLocal(sym);
                                }
                                cg.Emitter.InitialiseSymbol(sym);
                            }
                            break;
                        }

                    case SymType.LABEL:
                        sym.Info = cg.Emitter.CreateLabel();
                        break;
                }
            }
        }
    }
}
