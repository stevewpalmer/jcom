﻿// JComal
// General parsing routines
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

using System.Collections.ObjectModel;
using System.Diagnostics;
using CCompiler;
using JComLib;

namespace JComal;

/// <summary>
/// Extension of the Compiler class to do general parsing.
/// </summary>
public partial class Compiler {

    // Parse an integer, real or string constant
    private Variant ParseConstant() {
        ParseNode tokenNode = SimpleExpression();
        if (tokenNode is { IsConstant: true }) {
            return tokenNode.Value;
        }
        Messages.Error(MessageCode.CONSTANTEXPECTED, "Constant expected");
        return new Variant(0);
    }

    // Parse and return a string literal
    private string ParseStringLiteral() {
        SimpleToken token = ExpectToken(TokenID.STRING);
        StringToken stringToken = (StringToken)token;
        return stringToken?.String;
    }

    // Parse an identifier
    private IdentifierToken ParseIdentifier() {
        SimpleToken token = ExpectToken(TokenID.IDENT);
        return token as IdentifierToken;
    }

    // Parse a label
    private SymbolParseNode ParseLabel() {
        IdentifierToken identToken = ParseIdentifier();
        return identToken != null ? new SymbolParseNode(GetMakeLabel(identToken.Name, false)) : null;
    }

    // Parse an identifier from the specified token and assign the corresponding
    // symbol to the identifier parse node.
    private IdentifierParseNode ParseIdentifierFromToken(IdentifierToken identToken) {
        IdentifierParseNode node = ParseIdentifierParseNode();
        Debug.Assert(node != null);

        Symbol sym = GetMakeSymbolForCurrentScope(identToken.Name);
        Debug.Assert(sym != null);

        // Make sure strings have been explicitly DIM'd in strict mode. We set an
        // explicit width to avoid this error being thrown several times.
        if (sym is { IsParameter: false, Type: SymType.FIXEDCHAR } && sym.FullType.Width == 0 && _opts.Strict) {
            Messages.Error(MessageCode.MISSINGSTRINGDECLARATION, "String width must be specified with DIM before use");
            sym.FullType.Width = Consts.DefaultStringWidth;
        }

        // Make sure array indexes match only where they are specified
        if (sym.IsArray && node.Indexes != null && sym.Dimensions.Count != node.Indexes.Count) {
            Messages.Error(MessageCode.MISSINGARRAYDIMENSIONS, "Incorrect number of array indexes");
        }

        // Substring specification can only be used on strings
        else if (node.HasSubstring && sym.Type != SymType.FIXEDCHAR) {
            Messages.Error(MessageCode.TYPEMISMATCH, "Substrings can only be used on string variables");
        }

        // If this a string and we've got array indexes? If so then this is
        // a non-standard substring reference. Fix this up unless strict is
        // specified in which case this is an error
        else if (sym is { Type: SymType.FIXEDCHAR, IsArray: false } && node.HasIndexes) {
            Debug.Assert(node.Indexes != null);
            if (_opts.Strict || node.Indexes.Count > 1) {
                Messages.Error(MessageCode.BADSUBSTRINGSPEC, "Substring must have start and end specification");
            }
            else {
                node.SubstringStart = node.Indexes[0];
                node.SubstringEnd = node.Indexes[0];
                node.Indexes = null;
            }
        }
        node.Symbol = sym;
        sym.IsReferenced = true;
        return node;
    }

    // Parse an identifier parse node from the specified token.
    private IdentifierParseNode ParseIdentifierParseNode() {
        IdentifierParseNode node = new(null);
        Collection<ParseNode> indices = null;
        SimpleToken token = GetNextToken();

        while (token.ID == TokenID.LPAREN) {
            indices ??= [];
            if (_currentLine.PeekToken().ID != TokenID.RPAREN) {
                do {
                    ParseNode item = null;

                    if (token.ID == TokenID.RPAREN) {
                        token = GetNextToken();
                        break;
                    }
                    if (_currentLine.PeekToken().ID != TokenID.COLON) {
                        item = IntegerExpression();
                    }
                    token = GetNextToken();
                    if (token.ID == TokenID.COLON) {
                        item ??= new NumberParseNode(1);
                        node.SubstringStart = item;
                        item = null;
                        token = GetNextToken();
                        if (token.ID != TokenID.RPAREN) {
                            _currentLine.PushToken(token);
                            item = IntegerExpression();
                            token = GetNextToken();
                        }
                        node.SubstringEnd = item;
                        continue;
                    }
                    indices.Add(item);
                } while (token.ID == TokenID.COMMA);
                _currentLine.PushToken(token);
            }
            ExpectToken(TokenID.RPAREN);
            token = GetNextToken();
        }

        node.Indexes = indices;
        _currentLine.PushToken(token);
        return node;
    }

    // Parse a PROC or FUNC definition including the declaration and the proc/func
    // body.
    private ParseNode ParseProcFuncDefinition(SymClass klass, TokenID endToken, string methodName) {

        ProcedureParseNode node = new();
        IdentifierToken identToken = null;
        bool isImplicit = endToken == TokenID.ENDOFFILE;

        // Add the name to the global scope, ensuring it hasn't already been
        // defined.
        if (methodName == null) {
            identToken = ParseIdentifier();
            if (identToken == null) {
                SkipToEndOfLine();
                return null;
            }
            methodName = identToken.Name;
        }

        // Method should exist in _globalSymbols due to Pass 0.
        Symbol method = Globals.Get(methodName);
        Debug.Assert(method != null);
        method.Defined = true;
        method.Class = klass;

        // Save return statement requirement
        bool savedHasReturn = _hasReturn;
        _hasReturn = false;

        // New local symbol table for this block
        SymbolCollection localSymbols = new("Local");
        SymbolStack.Push(localSymbols);

        // Parameter list.
        Collection<Symbol> parameters = null;
        if (methodName != _entryPointName) {
            parameters = ParseParameterDecl(SymbolStack.Top, SymScope.PARAMETER);
        }
        method.Parameters = parameters;

        // EXTERNAL?
        if (!isImplicit && TestToken(TokenID.KEXTERNAL)) {
            method.Modifier = SymModifier.EXTERNAL;
            SkipToEndOfLine();
            return null;
        }

        // Closed? If so, we create an _importSymbols table so to which any symbols
        // referenced by IMPORT are added. We also implicitly add any imported
        // symbols from our parent procedure if we're a nested procedure as there
        // is an implicit trust relationship between us. In addition, we add any
        // other nested procedures to _importSymbols since those are assumed to be
        // imported by default.
        SymbolCollection savedImportSymbols = _importSymbols;
        if (!isImplicit && !_currentLine.IsAtEndOfLine) {

            ExpectToken(TokenID.KCLOSED);

            _importSymbols = new SymbolCollection("Import");
            if (savedImportSymbols != null) {
                _importSymbols.Add(savedImportSymbols);
            }
            _importSymbols.Add(method);
            AddChildSymbols(_importSymbols, method);
            node.IsClosed = true;
        }

        if (klass == SymClass.FUNCTION) {
            method.FullType = GetTypeFromName(methodName);
        }

        // Don't catch run-time exceptions if we're running in
        // the interpreter.
        node.CatchExceptions = _opts is { Interactive: false, DevMode: false };

        if (methodName == _entryPointName) {
            method.Modifier |= SymModifier.ENTRYPOINT;
            if (_opts.Run) {
                method.Modifier |= SymModifier.EXPORTED;
            }
            _hasProgram = true;
        }

        node.ProcedureSymbol = method;
        node.Symbols.Add(localSymbols);
        node.LabelList = [];

        ProcedureParseNode savedCurrentProcedure = CurrentProcedure;
        CurrentProcedure = node;

        _program.Root.Add(node);

        // Compile the body of the procedure
        ++_blockDepth;
        CompileBlock(node.Body, [endToken]);
        --_blockDepth;

        // Make sure we have a RETURN statement.
        if (!_hasReturn) {
            if (klass == SymClass.FUNCTION) {
                Messages.Error(MessageCode.ILLEGALRETURN, $"Missing RETURN in {methodName}");
            }
            node.Body.Add(new ReturnParseNode());
        }

        // Check identifier matches
        SimpleToken token = GetNextToken();
        CheckEndOfBlockName(identToken, token);

        // Validate the block.
        foreach (Symbol sym in SymbolStack.Top) {
            if (sym.IsLabel && !sym.Defined) {
                Messages.Error(MessageCode.UNDEFINEDLABEL, sym.RefLine, $"Undefined label {sym.Name}");
                continue;
            }

            // For non-array characters, if there's no value, set the empty string
            if (sym is { Type: SymType.FIXEDCHAR, IsArray: false } && !sym.Value.HasValue) {
                sym.Value = new Variant(string.Empty);
            }

            if (sym is { IsReferenced: false, IsHidden: false } && !sym.Modifier.HasFlag(SymModifier.RETVAL)) {
                string scopeName = sym.IsParameter ? "parameter" : sym.IsLabel ? "label" : "variable";
                Messages.Warning(MessageCode.UNUSEDVARIABLE,
                    3,
                    sym.RefLine,
                    $"Unused {scopeName} {sym.Name} in function");
            }
        }
        ValidateBlock(1, node.Body);
        _state = BlockState.SPECIFICATION;
        CurrentProcedure = savedCurrentProcedure;
        _importSymbols = savedImportSymbols;

        // Remove the local symbol table
        if (!isImplicit) {
            SymbolStack.Pop();
        }

        _hasReturn = savedHasReturn;
        return null;
    }

    // Parse a function or subroutine parameter declaration and return an
    // array of symbols for all parameters
    private Collection<Symbol> ParseParameterDecl(SymbolCollection symbolTable, SymScope scope) {
        SimpleToken token = GetNextToken();
        Collection<Symbol> parameters = [];

        if (token.ID == TokenID.LPAREN) {
            if (_currentLine.PeekToken().ID != TokenID.RPAREN) {
                do {
                    SymLinkage linkage = SymLinkage.BYVAL;
                    if (TestToken(TokenID.KREF)) {
                        linkage = SymLinkage.BYREF;
                    }
                    IdentifierDefinition identifier = ParseIdentifierDefinition();

                    if (identifier != null) {
                        Symbol sym = symbolTable.Get(identifier.Name);
                        if (sym != null) {
                            Messages.Error(MessageCode.PARAMETERDEFINED, $"Parameter {identifier.Name} already defined");
                        }
                        else {
                            SymFullType symType = GetTypeFromName(identifier.Name);
                            sym = symbolTable.Add(identifier.Name,
                                symType,
                                SymClass.VAR,
                                identifier.Dimensions,
                                _currentLineNumber);
                            sym.Scope = scope;
                            sym.Linkage = linkage;
                            sym.Defined = true;
                            parameters.Add(sym);
                        }
                    }
                    token = GetNextToken();
                } while (token.ID == TokenID.COMMA);
                _currentLine.PushToken(token);
            }
            ExpectToken(TokenID.RPAREN);
        }
        else {
            _currentLine.PushToken(token);
        }
        return parameters;
    }

    // Parse an identifier with optional array dimensions.
    private IdentifierDefinition ParseIdentifierDefinition() {

        IdentifierToken identToken = ParseIdentifier();

        // Array parameters must be specified with (). Multiple
        // dimensions are indicated with commas within the
        // parenthesis.
        Collection<SymDimension> dimensions = [];
        if (TestToken(TokenID.LPAREN)) {
            do {
                dimensions.Add(new SymDimension {
                    LowerBound = new NumberParseNode(1),
                    UpperBound = new NumberParseNode(1)
                });
            } while (TestToken(TokenID.COMMA));
            ExpectToken(TokenID.RPAREN);
        }
        return new IdentifierDefinition {
            Name = identToken.Name,
            Dimensions = dimensions
        };
    }

    // Parse an identifier declaration
    private Symbol ParseIdentifierDeclaration(SymFullType fullType) {
        IdentifierToken identToken = ParseIdentifier();
        Symbol sym = null;

        if (identToken != null) {

            // Ban any conflict with PROGRAM name or the current function
            Symbol globalSym = Globals.Get(identToken.Name);
            if (globalSym is { Type: SymType.PROGRAM }) {
                Messages.Error(MessageCode.IDENTIFIERISGLOBAL,
                    $"Identifier {identToken.Name} already has global declaration");
                SkipToEndOfLine();
                return null;
            }

            // Now check the local program unit
            sym = GetSymbolForCurrentScope(identToken.Name);

            // Handle array syntax and build a list of dimensions
            Collection<SymDimension> dimensions = ParseArrayDimensions();
            if (dimensions == null) {
                return null;
            }

            // Indicate this symbol is explicitly declared
            SymFullType thisFullType = GetTypeFromName(identToken.Name);
            if (sym == null) {
                sym = MakeSymbolForCurrentScope(identToken.Name);
                sym.Dimensions = dimensions;
            }
            else {
                if (fullType.Type != SymType.NONE) {
                    sym.FullType = thisFullType;
                }
                if (dimensions.Count > 0) {
                    sym.Dimensions = dimensions;
                }
            }

            // If this is applying type to a function, update the function too
            if (globalSym != null && sym == globalSym.RetVal) {
                globalSym.FullType = thisFullType;
            }

            // BUGBUG: This should be done in the Symbols class when the
            // identifier becomes an array.
            if (sym.IsArray) {
                sym.Linkage = SymLinkage.BYVAL;
                sym.Modifier |= SymModifier.FLATARRAY; // Comal always uses flat arrays
            }
        }
        return sym;
    }

    // Parse set of array dimensions if one is found.
    private Collection<SymDimension> ParseArrayDimensions() {
        Collection<SymDimension> dimensions = [];

        while (TestToken(TokenID.LPAREN)) {
            do {
                ParseNode intVal = IntegerExpression();
                if (intVal == null) {
                    SkipToEndOfLine();
                    return null;
                }

                SymDimension dim = new();

                // Arrays lower bounds start from 1 but one can
                // specify a custom bound range with the lower:upper syntax.
                ParseNode in1 = new NumberParseNode(1);
                ParseNode in2 = intVal;
                if (TestToken(TokenID.COLON)) {
                    intVal = IntegerExpression();
                    if (intVal != null) {
                        in1 = in2;
                        in2 = intVal;
                    }
                }
                if (in2.IsConstant && in1.IsConstant) {
                    if (in2.Value.IntValue > 0 && in2.Value.IntValue < in1.Value.IntValue) {
                        Messages.Error(MessageCode.ARRAYILLEGALBOUNDS, "Illegal bounds in array");
                    }
                }
                dim.LowerBound = in1;
                dim.UpperBound = in2;
                if (dimensions.Count == 7) {
                    Messages.Error(MessageCode.TOOMANYDIMENSIONS, "Too many dimensions in array");
                }
                else {
                    dimensions.Add(dim);
                }
            } while (TestToken(TokenID.COMMA));
            ExpectToken(TokenID.RPAREN);
        }
        return dimensions;
    }
}