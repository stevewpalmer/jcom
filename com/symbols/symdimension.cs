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

namespace CCompiler; 

/// <summary>
/// Defines a single array dimension. Separate lower and upper bounds can
/// be specifies for languages that support this.
/// </summary>
public class SymDimension {

    /// <value>
    /// Gets or set the array lower bound.
    /// </value>
    public ParseNode LowerBound { get; set; }

    /// <value>
    /// Gets or sets the array upper bound.
    /// </value>
    public ParseNode UpperBound { get; set; }

    /// <value>
    /// Returns the size, in units, of this dimension or -1
    /// if the dimension is dynamic (ie. cannot be computed
    /// until runtime).
    /// </value>
    public int Size {
        get {
            if (UpperBound.IsConstant && LowerBound.IsConstant) {
                return UpperBound.Value.IntValue - LowerBound.Value.IntValue + 1;
            }
            return -1;
        }
    }

    /// <summary>
    /// Return the symbol dimension as a string..
    /// </summary>
    /// <returns>The symbol dimensions formatted as a string.</returns>
    public override string ToString() {
        if (LowerBound.IsConstant && LowerBound.Value.IntValue == 1) {
            return UpperBound.ToString();
        }
        return $"{LowerBound}:{UpperBound}";
    }
}
