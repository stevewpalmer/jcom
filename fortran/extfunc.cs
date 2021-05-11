// JFortran Compiler
// External Function definition
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
using System.Collections.ObjectModel;
using CCompiler;
using JComLib;

namespace JFortran {

    /// <summary>
    /// Exposes methods for specifying an external function.
    /// </summary>
    public class ExternalFunction {

        private struct FunctionDefinition {
            public string Name;
            public Symbol Symbol;
            public bool Include;
        }
        
        private readonly Collection<FunctionDefinition> _definitions;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalFunction"/> class.
        /// </summary>
        public ExternalFunction() {
            _definitions = new Collection<FunctionDefinition>();
            ParameterList = new Collection<string>();
        }

        /// <summary>
        /// Adds a parameter to the definition of this external function with the given
        /// name, type and linkage. Parameters must be added in the exact order in which
        /// they are defined in the external function.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="type">A SymType representing the parameter type.</param>
        /// <param name="linkage">A SymLinkage representing the parameter linkage.</param>
        /// <param name="include">Specifies whether this parameter should be included in ParametersNode.</param>
        public void Add(string name, SymType type, SymLinkage linkage, bool include = true) {
            FunctionDefinition definition = new() {
                Name = name,
                Symbol = new Symbol(name, new SymFullType(type), SymClass.VAR, null, 0)
            };
            definition.Symbol.Linkage = linkage;
            definition.Include = include;
            _definitions.Add(definition);
            ParameterList.Add(name);
        }

        /// <summary>
        /// Returns a ParametersParseNode object that represents the parameters defined
        /// for this external function using the values specified in the given control
        /// list dictionary. For parameters where no value is given, a default of 0 or
        /// NULL is substituted.
        /// </summary>
        /// <param name="cilist">A dictionary of control list values</param>
        /// <returns>A ParametersParseNode object.</returns>
        public ParametersParseNode ParametersNode(ControlList cilist) {
            if (cilist == null) {
                throw new ArgumentNullException(nameof(cilist));
            }
            ParametersParseNode paramList = new();
            foreach (FunctionDefinition def in _definitions) {
                if (def.Include) {
                    ParseNode exprNode;
                    if (!cilist.Has(def.Name)) {
                        if (Symbol.IsNumberType(def.Symbol.Type)) {
                            exprNode = new NumberParseNode(0);
                        } else if (Symbol.IsLogicalType(def.Symbol.Type)) {
                            exprNode = new NumberParseNode(new Variant(false));
                        } else {
                            exprNode = new NullParseNode {
                                Type = def.Symbol.Type
                            };
                        }
                    } else {
                        exprNode = cilist[def.Name];
                    }
                    paramList.Add(exprNode, def.Symbol);
                }
            }
            return paramList;
        }

        /// <summary>
        /// Gets a list of parameter names.
        /// </summary>
        public Collection<string> ParameterList { get; }
    }
}
