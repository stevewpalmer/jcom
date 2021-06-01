// JCom Compiler Toolkit
// Array parse node
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
using System.Reflection;
using System.Reflection.Emit;
using JComLib;

namespace CCompiler {

    /// <summary>
    /// Specifies a parse node that initialises an array object
    /// with a range of values.
    /// </summary>
    public sealed class ArrayParseNode : ParseNode {

        /// <value>
        /// Gets or sets the parse node that defines the array.
        /// </value>
        public IdentifierParseNode Identifier { get; set; }

        /// <value>
        /// Gets or sets the symbol defines the array. This overrides
        /// Identifier.
        /// </value>
        public Symbol Symbol { get; set; }

        /// <summary>
        /// Specifies whether to redimension the array
        /// </summary>
        public bool Redimension { get; set; }

        /// <value>
        /// Gets or sets the start range for the array initialisation.
        /// </value>
        public int StartRange { get; set; }

        /// <value>
        /// Gets or sets the end range for the array initialisation.
        /// </value>
        public int EndRange { get; set; }

        /// <value>
        /// Gets or sets the variant value to be assigned during the
        /// array initialisation.
        /// </value>
        public Variant RangeValue { get; set; }

        /// <summary>
        /// Implements the code to initialise the array.
        /// </summary>
        /// <param name="emitter">Code emitter</param>
        /// <param name="cg">The code generator object</param>
        public override void Generate(Emitter emitter, ProgramParseNode cg) {
            if (emitter == null) {
                throw new ArgumentNullException(nameof(emitter));
            }
            if (cg == null) {
                throw new ArgumentNullException(nameof(cg));
            }
            Symbol sym = Symbol ?? Identifier.Symbol;
            Debug.Assert(sym.IsArray);

            // Dynamic array initialisation
            // Handle dynamic local or static arrays by generating the code that
            // evaluates the upper or lower bounds and storing them for later.
            if (Redimension) {
                if (!sym.IsReferenced) {
                    return;
                }
                if (sym.IsStatic) {
                    foreach (SymDimension dim in sym.Dimensions) {
                        if (!dim.LowerBound.IsConstant) {
                            FieldInfo lowBound = cg.CurrentType.TemporaryField(typeof(int));
                            cg.GenerateExpression(emitter, SymType.INTEGER, dim.LowerBound);
                            emitter.StoreStatic(lowBound);
                            dim.LowerBound = new StaticParseNode(lowBound);
                        }
                        if (!dim.UpperBound.IsConstant) {
                            FieldInfo upperBound = cg.CurrentType.TemporaryField(typeof(int));
                            cg.GenerateExpression(emitter, SymType.INTEGER, dim.UpperBound);
                            emitter.StoreStatic(upperBound);
                            dim.UpperBound = new StaticParseNode(upperBound);
                        }
                    }
                } else {
                    foreach (SymDimension dim in sym.Dimensions) {
                        if (!dim.LowerBound.IsConstant) {
                            LocalDescriptor lowBound = emitter.GetTemporary(typeof(int));
                            cg.GenerateExpression(emitter, SymType.INTEGER, dim.LowerBound);
                            emitter.StoreLocal(lowBound);
                            dim.LowerBound = new LocalParseNode(lowBound);
                        }
                        if (!dim.UpperBound.IsConstant) {
                            LocalDescriptor upperBound = emitter.GetTemporary(typeof(int));
                            cg.GenerateExpression(emitter, SymType.INTEGER, dim.UpperBound);
                            emitter.StoreLocal(upperBound);
                            dim.UpperBound = new LocalParseNode(upperBound);
                        }
                    }
                }
                Type[] paramTypes = new Type[sym.Dimensions.Count];
                Type baseType = sym.SystemType;

                for (int c = 0; c < sym.Dimensions.Count; ++c) {
                    SymDimension dim = sym.Dimensions[c];
                    if (dim.UpperBound.IsConstant) {
                        emitter.LoadInteger(dim.UpperBound.Value.IntValue);
                    } else {
                        sym.Dimensions[c].UpperBound.Generate(emitter, cg, SymType.INTEGER);
                    }
                    if (!(dim.LowerBound.IsConstant && dim.LowerBound.Value.IntValue == 1)) {
                        if (dim.LowerBound.IsConstant) {
                            emitter.LoadInteger(dim.LowerBound.Value.IntValue);
                        } else {
                            sym.Dimensions[c].LowerBound.Generate(emitter, cg, SymType.INTEGER);
                        }
                        emitter.Sub(SymType.INTEGER);
                        emitter.LoadValue(SymType.INTEGER, new Variant(1));
                        emitter.Add(SymType.INTEGER);
                    }
                    paramTypes[c] = typeof(int);
                }
                if (sym.Type == SymType.FIXEDCHAR) {
                    emitter.Dup();
                }
                if (sym.Dimensions.Count == 1) {
                    emitter.CreateArray(Symbol.SymTypeToSystemType(sym.Type));
                } else {
                    emitter.CreateObject(baseType, paramTypes);
                }
                emitter.StoreSymbol(sym);
                if (sym.Type == SymType.FIXEDCHAR) {
                    emitter.InitFixedStringArray(sym);
                }
            }

            // Simple array initialisation - just initialise the
            // specified element. More complex initialisation requires
            // a loop across the elements.
            if (Identifier != null && Identifier.Indexes != null && Identifier.Indexes.Count > 0) {
                if (Identifier.Indexes.Count == 1) {
                    NumberParseNode number = (NumberParseNode)Identifier.Indexes[0];
                    Debug.Assert(sym.Dimensions[0].LowerBound.IsConstant);
                    int index = number.Value.IntValue + (0 - sym.Dimensions[0].LowerBound.Value.IntValue);
                    GenerateStoreToArray(emitter, sym, index, RangeValue);
                } else {
                    Type [] paramTypes = new Type[Identifier.Indexes.Count + 1];
                    int index = 0;

                    emitter.LoadSymbol(sym);
                    while (index < Identifier.Indexes.Count) {
                        NumberParseNode number = (NumberParseNode)Identifier.Indexes[index];
                        Debug.Assert(sym.Dimensions[index].LowerBound.IsConstant);
                        emitter.LoadInteger(number.Value.IntValue + (0 - sym.Dimensions[index].LowerBound.Value.IntValue));
                        paramTypes[index++] = typeof(int);
                    }
                    emitter.LoadVariant(RangeValue);
                    paramTypes[index] = Variant.VariantTypeToSystemType(RangeValue.Type);

                    emitter.Call(cg.GetMethodForType(sym.SystemType, "Set", paramTypes));
                }
            }
            else if (StartRange < EndRange + 4) {
                
                // When initialising arrays less than 4 values, unroll
                // the loop as it will be faster.
                int index = StartRange;
                while (index <= EndRange) {
                    GenerateStoreToArray(emitter, sym, index++, RangeValue);
                }
            } else {
                
                // For large arrays, generate code to initialise the elements
                // to a specific value.
                Label label1 = emitter.CreateLabel();
                emitter.LoadInteger(StartRange);
                LocalDescriptor tempIndex = emitter.GetTemporary(typeof(int));
                emitter.StoreLocal(tempIndex);
                emitter.MarkLabel(label1);
                GenerateStoreToArray(emitter, sym, tempIndex.Index, RangeValue);
                emitter.LoadLocal(tempIndex);
                emitter.LoadInteger(1);
                emitter.Add(SymType.INTEGER);
                emitter.Dup();
                emitter.StoreLocal(tempIndex);
                emitter.LoadInteger(EndRange);
                emitter.BranchLessOrEqual(label1);
            }
        }

        /// <summary>
        /// Dumps the contents of this parse node to the ParseNode XML
        /// output under the specified parent node.
        /// </summary>
        /// <param name="root">The parent XML node</param>
        public override void Dump(ParseNodeXml root) {
            ParseNodeXml blockNode = root.Node("Array");
            Identifier.Dump(blockNode);
            blockNode.Write("StartRange", StartRange.ToString());
            blockNode.Write("EndRange", EndRange.ToString());
            blockNode.Write("RangeValue", RangeValue.ToString());
        }

        // Emit code that writes the variant value to the given array index where
        // the array is specified by the symbol..
        private static void GenerateStoreToArray(Emitter emitter, Symbol sym, int index, Variant value) {
            emitter.LoadSymbol(sym);
            emitter.LoadInteger(index);
            emitter.LoadVariant(value);
            emitter.ConvertType(Symbol.VariantTypeToSymbolType(value.Type), sym.Type);
            emitter.StoreElement(sym.Type);
        }
    }
}
