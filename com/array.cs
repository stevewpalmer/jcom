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
using System.Reflection.Emit;
using JComLib;

namespace CCompiler {

    /// <summary>
    /// Specifies a parse node that defines an array object
    /// value.
    /// </summary>
    public sealed class ArrayParseNode : ParseNode {

        /// <value>
        /// Gets or sets the parse node that defines the array.
        /// </value>
        public IdentifierParseNode Identifier { get; set; }

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
        /// <param name="cg">The code generator object</param>
        public override void Generate(CodeGenerator cg) {
            if (cg == null) {
                throw new ArgumentNullException(nameof(cg));
            }
            Symbol sym = Identifier.Symbol;
            
            // Simple array initialisation - just initialise the
            // specified element. More complex initialisation requires
            // a loop across the elements.
            Debug.Assert(sym.IsArray);
            if (Identifier.Indexes != null && Identifier.Indexes.Count > 0) {
                if (Identifier.Indexes.Count == 1) {
                    NumberParseNode number = (NumberParseNode)Identifier.Indexes[0];
                    Debug.Assert(sym.Dimensions[0].LowerBound.IsConstant);
                    int index = number.Value.IntValue + (0 - sym.Dimensions[0].LowerBound.Value.IntValue);
                    GenerateStoreToArray(cg, sym, index, RangeValue);
                } else {
                    Type [] paramTypes = new Type[Identifier.Indexes.Count + 1];
                    int index = 0;
                    
                    cg.LoadLocal(sym);
                    while (index < Identifier.Indexes.Count) {
                        NumberParseNode number = (NumberParseNode)Identifier.Indexes[index];
                        Debug.Assert(sym.Dimensions[index].LowerBound.IsConstant);
                        cg.Emitter.LoadInteger(number.Value.IntValue + (0 - sym.Dimensions[index].LowerBound.Value.IntValue));
                        paramTypes[index++] = typeof(int);
                    }
                    cg.Emitter.GenerateLoad(RangeValue);
                    paramTypes[index] = Variant.VariantTypeToSystemType(RangeValue.Type);
                    
                    cg.Emitter.Call(cg.GetMethodForType(sym.SystemType, "Set", paramTypes));
                }
            }
            else if (StartRange < EndRange + 4) {
                
                // When initialising arrays less than 4 values, unroll
                // the loop as it will be faster.
                int index = StartRange;
                while (index <= EndRange) {
                    GenerateStoreToArray(cg, sym, index++, RangeValue);
                }
            } else {
                
                // For large arrays, generate code to initialise the elements
                // to a specific value.
                Label label1 = cg.Emitter.CreateLabel();
                cg.Emitter.LoadInteger(StartRange);
                LocalDescriptor tempIndex = cg.Emitter.GetTemporary(typeof(int));
                cg.Emitter.StoreLocal(tempIndex);
                cg.Emitter.MarkLabel(label1);
                GenerateStoreToArray(cg, sym, tempIndex.Index, RangeValue);
                cg.Emitter.LoadLocal(tempIndex);
                cg.Emitter.LoadInteger(1);
                cg.Emitter.Add(SymType.INTEGER);
                cg.Emitter.Dup();
                cg.Emitter.StoreLocal(tempIndex);
                cg.Emitter.LoadInteger(EndRange);
                cg.Emitter.BranchLessOrEqual(label1);
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
        private void GenerateStoreToArray(CodeGenerator cg, Symbol sym, int index, Variant value) {
            cg.LoadLocal(sym);
            cg.Emitter.LoadInteger(index);
            cg.Emitter.GenerateLoad(value);
            cg.Emitter.ConvertType(Symbol.VariantTypeToSymbolType(value.Type), sym.Type);
            cg.Emitter.StoreElement(sym.Type);
        }
    }
}
