// JComal
// Constants
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

namespace JComal; 

public static class Consts {

    /// <summary>
    /// Character used to suffix an integer variable
    /// </summary>
    public const char IntegerChar = '#';

    /// <summary>
    /// Character used to suffix an string variable
    /// </summary>
    public const char StringChar = '$';

    /// <summary>
    /// The name given to the end of read data variable in
    /// the symbol table.
    /// </summary>
    public const string EODName = "_EOD";

    /// <summary>
    /// The name given to the read data index variable in
    /// the symbol table.
    /// </summary>
    public const string DataIndexName = "_DATAINDEX";

    /// <summary>
    /// The name given to the read data array variable in
    /// the symbol table.
    /// </summary>
    public const string DataName = "_DATA";

    /// <summary>
    /// The name given to the global error number in
    /// the symbol table.
    /// </summary>
    public const string ErrName = "_ERR";

    /// <summary>
    /// The name given to the global error message in
    /// the symbol table.
    /// </summary>
    public const string ErrText = "_ERRTEXT$";

    /// <summary>
    /// Default width of strings that are not explicitly DIM
    /// and if strict mode is not enforced.
    /// </summary>
    public const int DefaultStringWidth = 40;

    /// <summary>
    /// Maximum length of an identifier.
    /// </summary>
    public const int MaximumIdentifierLength = 80;

    /// <summary>
    /// Comal program file suffix (created by SAVE).
    /// </summary>
    public const string ProgramFileExtension = ".cml";

    /// <summary>
    /// Comal source file suffix (created by LIST/DISPLAY).
    /// </summary>
    public const string SourceFileExtension = ".lst";
}
