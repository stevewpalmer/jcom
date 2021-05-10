// JCom Compiler Toolkit
// Read data parse node
//
// Authors:
//  Steve Palmer
//
// Copyright (C) 2021 Steve Palmer
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
using System.Reflection;
using System.Reflection.Emit;

namespace CCompiler {

    /// <summary>
    /// Specifies a parse node that defines a single READ DATA item.
    /// </summary>
    public sealed class ReadDataParseNode : ParseNode {

        /// <summary>
        /// Gets or sets the symbol used to track the read data index
        /// </summary>
        /// <value>The Data Index symbol.</value>
        public Symbol DataIndex { get; set; }

        /// <summary>
        /// Gets or sets the symbol used to track the read data items
        /// </summary>
        /// <value>The Data symbol.</value>
        public Symbol DataArray { get; set; }

        /// <summary>
        /// Gets or sets the symbol used to track the end of the data
        /// </summary>
        /// <value>The EOD symbol.</value>
        public Symbol EndOfData { get; set; }

        /// <summary>
        /// Collection of identifiers that are assigned the read data.
        /// </summary>
        /// <value>The local index of the ReadManager instance.</value>
        public IdentifierParseNode[] Identifiers { get; set; }

        /// <summary>
        /// Dumps the contents of this parse node to the ParseNode XML
        /// output under the specified parent node.
        /// </summary>
        /// <param name="root">The parent XML node</param>
        public override void Dump(ParseNodeXml root) {
            ParseNodeXml blockNode = root.Node("ReadData");
            if (DataIndex != null) {
                DataIndex.Dump(blockNode.Node("DataIndex"));
            }
            if (DataArray != null) {
                DataIndex.Dump(blockNode.Node("DataArray"));
            }
            if (EndOfData != null) {
                DataIndex.Dump(blockNode.Node("EndOfData"));
            }
            foreach (IdentifierParseNode identNode in Identifiers) {
                identNode.Dump(blockNode);
            }
        }

        /// <summary>
        /// Emit the code to read the next DATA item.
        /// </summary>
        /// <param name="cg">A CodeGenerator object</param>
        /// <param name="node">A parse node for the READ identifier</param>
        public override void Generate(CodeGenerator cg) {
            if (cg == null) {
                throw new ArgumentNullException(nameof(cg));
            }

            Label endRead = cg.Emitter.CreateLabel();

            foreach (IdentifierParseNode identifier in Identifiers) {

                // Check for READ past the end of DATA. (This should really
                // result in an exception being thrown.)
                cg.Emitter.LoadStatic((FieldInfo)DataIndex.Info);
                cg.Emitter.LoadInteger(DataArray.ArraySize);
                cg.Emitter.BranchEqual(endRead);

                // Store the data in the identifier
                Symbol sym = identifier.Symbol;

                if (sym.IsArray) {
                    cg.GenerateLoadArrayAddress(identifier);
                }
                cg.Emitter.LoadStatic((FieldInfo)DataArray.Info);
                cg.Emitter.LoadStatic((FieldInfo)DataIndex.Info);

                // Numbers in DATA are always floats
                SymType identifierType = identifier.Type;
                if (identifierType == SymType.INTEGER) {
                    identifierType = SymType.FLOAT;
                }
                if (identifierType == SymType.FIXEDCHAR) {
                    identifierType = SymType.CHAR;
                }
                cg.Emitter.LoadElementReference(identifierType);
                cg.Emitter.ConvertType(identifierType, identifier.Type);
                if (sym.IsArray) {
                    cg.Emitter.StoreArrayElement(sym);
                }
                else if (sym.IsLocal) {
                    cg.Emitter.StoreLocal(sym);
                }

                // Advance the data index. If EndOfData is referenced then set
                // that to 1 if the data index is equal to the array size, 0
                // otherwise.
                cg.Emitter.LoadStatic((FieldInfo)DataIndex.Info);
                cg.Emitter.LoadInteger(1);
                cg.Emitter.Add(DataIndex.Type);
                if (EndOfData.Info != null) {
                    cg.Emitter.Dup();
                    cg.Emitter.StoreStatic((FieldInfo)DataIndex.Info);
                    cg.Emitter.LoadInteger(DataArray.ArraySize);
                    cg.Emitter.Emit0(OpCodes.Ceq);
                    cg.Emitter.StoreStatic((FieldInfo)EndOfData.Info);
                } else {
                    cg.Emitter.StoreStatic((FieldInfo)DataIndex.Info);
                }
            }

            cg.Emitter.MarkLabel(endRead);
        }
    }
}
