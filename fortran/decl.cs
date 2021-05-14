// JFortran Compiler
// Declaration statements parser
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
using System.Collections.ObjectModel;
using System.Diagnostics;
using CCompiler;
using JComLib;

namespace JFortran {

    /// <summary>
    /// Main Fortran compiler class.
    /// </summary>
    public partial class Compiler {

        // BLOCKDATA keyword.
        private ParseNode KBlockData() {
            // TODO: Implement
            Messages.Error(MessageCode.NOTSUPPORTED, "BLOCK DATA not yet supported");
            SkipToEndOfLine();
            return null;
        }

        /// COMMON keyword
        /// Syntax: COMMON ident [,ident]
        private ParseNode KCommon() {
            SymFullType fullType = new(SymType.NONE);
            List<Symbol> commonSymbols = null;
            bool isNewBlock = true;
            Symbol symCommon = null;

            do {
                SimpleToken token = _ls.GetToken();
                string name = "_COMMON";

                if (token.ID == TokenID.DIVIDE) {
                    IdentifierToken identToken = ExpectIdentifierToken();
                    if (identToken == null) {
                        SkipToEndOfLine();
                        return null;
                    }
                    name = identToken.Name;
                    isNewBlock = true;
                    ExpectToken(TokenID.DIVIDE);
                } else if (token.ID == TokenID.CONCAT) {
                    name = "_COMMON";
                    isNewBlock = true;
                } else {
                    _ls.BackToken();
                }
                if (isNewBlock) {
                    symCommon = _globalSymbols.Get(name);
                    if (symCommon == null) {
                        symCommon = _globalSymbols.Add(name, new SymFullType(SymType.COMMON), SymClass.COMMON, null, _ls.LineNumber);
                        commonSymbols = new List<Symbol>();
                        symCommon.Info = commonSymbols;
                        symCommon.CommonIndex = 0;
                    }
                    commonSymbols = (List<Symbol>)symCommon.Info;
                    isNewBlock = false;
                }
                Symbol sym = ParseIdentifierDeclaration(fullType);
                if (sym != null) {
                    if (sym.IsInCommon) {
                        Messages.Error(MessageCode.ALREADYINCOMMON, $"{sym.Name} is already in a COMMON block");
                    } else {
                        sym.CommonIndex = symCommon.CommonIndex;
                        sym.Common = symCommon;
                        sym.IsReferenced = true;

                        // Symbols in the primary COMMON block need to be static so
                        // they can be referenced from sub-programs
                        if (sym.CommonIndex < commonSymbols.Count) {
                            Symbol symInCommon = commonSymbols[sym.CommonIndex];
                            sym.FullType = symInCommon.FullType;
                        } else {
                            sym.Modifier |= SymModifier.STATIC;
                            commonSymbols.Add(sym);
                        }
                        symCommon.CommonIndex += 1;
                    }
                    SkipToken(TokenID.COMMA);
                }
            } while (!IsAtEndOfLine());
            return null;
        }

        /// SAVE keyword
        /// Syntax: SAVE ident [,ident]
        private ParseNode KSave() {
            if (!IsAtEndOfLine()) {
                SimpleToken token;
                do {
                    IdentifierToken identToken = ExpectIdentifierToken();
                    if (identToken != null) {
                        Symbol sym = _localSymbols.Get(identToken.Name);
                        if (sym != null) {
                            sym.Modifier |= SymModifier.STATIC;
                        }
                    }
                    token = _ls.GetToken();
                } while (token.ID == TokenID.COMMA);
                _ls.BackToken();
            } else {
                _saveAll = true;
            }
            return null;
        }

        // EQUIVALENCE keyword
        private ParseNode KEquivalence() {
            // TODO: Implement
            Messages.Error(MessageCode.NOTSUPPORTED, "EQUIVALENCE not yet supported");
            SkipToEndOfLine();
            return null;
        }

        // Parse a PARAMETER statement
        // This is a simple list of the format (ident=value [,ident=value])
        // Identifiers cannot have been previously defined and value can be an expression
        // but must evaluate to a constant.
        private ParseNode KParameter() {
            SimpleToken token;
            
            ExpectToken(TokenID.LPAREN);
            do {
                IdentifierToken identToken = ExpectIdentifierToken();
                if (identToken != null) {
                    Symbol sym = GetSymbolForCurrentScope(identToken.Name);
                    if (sym != null && sym.Scope == SymScope.CONSTANT) {
                        Messages.Error(MessageCode.IDENTIFIERREDEFINITION, "Identifier already defined");
                        SkipToEndOfLine();
                        return null;
                    }
                    if (sym == null) {
                        sym = _localSymbols.Get(identToken.Name);
                        if (sym == null) {
                            sym = _localSymbols.Add(identToken.Name, new SymFullType(SymType.NONE), SymClass.VAR, null, _ls.LineNumber);
                        }
                    }
                    sym.Defined = true;
                    ExpectToken(TokenID.EQUOP);
                    sym.Value = ParseConstantExpression();
                    sym.Scope = SymScope.CONSTANT;
                }
                token = _ls.GetToken();
            } while (token.ID == TokenID.COMMA);
            _ls.BackToken();
            ExpectToken(TokenID.RPAREN);
            return null;
        }

        // DATA keyword
        // Syntax: DATA list-of-vars/values/
        private ParseNode KData() {
            SimpleToken token;

            do {
                List<ParseNode> idList = new();
                List<Variant> valueList = new();
                do {
                    // BUGBUG: Check for duplicate identifier in ANY data
                    // statement. Need to set a symbol flag.
                    ParseNode node = ParseIdentifierWithImpliedDo();
                    if (node == null) {
                        SkipToEndOfLine();
                        return null;
                    }
                    idList.Add(node);
                    token = _ls.GetToken();
                } while(token.ID == TokenID.COMMA);
                _ls.BackToken();

                Variant repeatNode;
                Variant valueNode;

                ExpectToken(TokenID.DIVIDE);
                do {
                    valueNode = ParseConstant();
                    repeatNode = new Variant(1);

                    token = _ls.GetToken();
                    if (token.ID == TokenID.STAR) {
                        if (valueNode.IntValue < 1) {
                            Messages.Error(MessageCode.BADREPEATCOUNT, "Repeat count must be positive and non-zero");
                        } else {
                            repeatNode = valueNode;
                        }
                        valueNode = ParseConstant();
                        token = _ls.GetToken();
                    }
                    valueList.Add(repeatNode);
                    valueList.Add(valueNode);
                } while(token.ID == TokenID.COMMA);
                _ls.BackToken();
                ExpectToken(TokenID.DIVIDE);

                IdentifierParseNode idNode = null;
                int idIndex = 0;
                int valueIndex = 0;
                int repeatCount = 0;
                int offset = 0;

                while (idIndex < idList.Count || valueIndex < valueList.Count) {
                    if (idIndex < idList.Count) {
                        ParseNode parseNode = idList[idIndex++];
                        if (parseNode.ID == ParseID.LOOP) {
                            LoopParseNode loopNode = (LoopParseNode)parseNode;

                            // Make sure the loop range evaluates to a constant.
                            int loopCount = loopNode.IterationCount();
                            if (loopCount == -1) {
                                Messages.Error(MessageCode.NONCONSTANTDATALOOP, "Implied DO loop in DATA must be a constant");
                                SkipToEndOfLine();
                                return null;
                            }

                            // Also make sure that the loop control is an identifier. It should be an
                            // array identifier but we don't particularly check for this. Maybe we should?
                            if (loopNode.LoopValue.ID != ParseID.IDENT) {
                                Messages.Error(MessageCode.NONCONSTANTDATALOOP, "Implied DO loop in DATA must be an identifier");
                                SkipToEndOfLine();
                                return null;
                            }

                            // Generate the implied DO loop as a sequence of assignment statements that are
                            // executed during the procedure initialisation phase. All the different value
                            // representations should be handled here.
                            //
                            // 1. If the value is a sequence, then there should be as many values as
                            //    required by the loop count. If there are less, the remainder of the loop
                            //    is ignored.
                            // 2. If the value has a repeat count, we expand the value by the repeat count
                            //    and use subsequent values if there are still iterations left in the loop
                            //    counter.
                            // 3. Values beyond the end of the loop count are assigned to the other identifiers
                            //    specified in the DATA statement, if any.
                            //
                            IdentifierParseNode loopIdent = (IdentifierParseNode)loopNode.LoopValue;
                            Symbol loopSymbol = loopIdent.Symbol;

                            int loopIndex = loopNode.StartExpression.Value.IntValue;
                            while (loopCount > 0) {
                                if (repeatCount == 0 && valueIndex < valueList.Count) {
                                    valueIndex++;
                                    valueNode = valueList[valueIndex++];
                                }

                                AssignmentParseNode assignNode = new(
                                    new IdentifierParseNode(loopSymbol, loopIndex),
                                    new NumberParseNode(valueNode)
                                );
                                AddInit(assignNode);

                                if (repeatCount > 0) {
                                    --repeatCount;
                                }
                                loopIndex += loopNode.StepExpression.Value.IntValue;
                                --loopCount;
                            }
                            continue;
                        }
                        Debug.Assert(parseNode.ID == ParseID.IDENT);
                        idNode = (IdentifierParseNode)parseNode;
                        offset = 0;
                    }
                    if (repeatCount == 0 && valueIndex < valueList.Count) {
                        repeatNode = valueList[valueIndex++];
                        valueNode = valueList[valueIndex++];
                        repeatCount = repeatNode.IntValue;
                    }
                    Debug.Assert(idNode != null);
                    Symbol sym = idNode.Symbol;
                    if (sym.IsArray) {
                        ArrayParseNode arrayNode = new() {
                            Identifier = idNode
                        };
                        if (idNode.Indexes != null) {
                            arrayNode.StartRange = 0;
                            arrayNode.EndRange = 0;
                            arrayNode.RangeValue = valueNode;
                            AddInit(arrayNode);
                        } else {
                            arrayNode.StartRange = offset;
                            arrayNode.EndRange = offset + (repeatCount - 1);
                            arrayNode.RangeValue = valueNode;
                            AddInit(arrayNode);
                            offset += repeatCount;
                            repeatCount = 1;
                        }
                    } else if (idNode.HasSubstring) {
                        AssignmentParseNode assignNode = new(idNode, new StringParseNode(valueNode.StringValue));
                        AddInit(assignNode);
                    } else {
                        sym.Value = valueNode;
                    }
                    --repeatCount;
                }
                token = _ls.GetToken();
            } while (token.ID == TokenID.COMMA);
            _ls.BackToken();
            return null;
        }

        // FUNCTION keyword.
        private ParseNode KFunction() {
            return KSubFunc(SymClass.FUNCTION, null, new SymFullType());
        }

        // SUBROUTINE keyword
        private ParseNode KSubroutine() {
            return KSubFunc(SymClass.SUBROUTINE, null, new SymFullType());
        }

        // Starts a subroutine block. For this, we create a separate symbol table
        // to supplement the main one.
        private ParseNode KSubFunc(SymClass klass, string methodName, SymFullType returnType) {
            ProcedureParseNode node = new();
            EnsureNoLabel();

            // Add the name to the global scope, ensuring it hasn't already been
            // defined.
            if (methodName == null) {
                IdentifierToken identToken = ExpectIdentifierToken();
                if (identToken == null) {
                    SkipToEndOfLine();
                    return null;
                }
                methodName = identToken.Name;
            }

            Symbol method = _globalSymbols.Get(methodName);
            if (method != null && method.Defined && !method.IsExternal) {
                Messages.Error(MessageCode.SUBFUNCDEFINED, $"{methodName} already defined");
                SkipToEndOfLine();
                return null;
            }

            // Reset the COMMON indexes for this program unit
            foreach (Symbol sym in _globalSymbols) {
                if (sym.Type == SymType.COMMON) {
                    sym.CommonIndex = 0;
                }
            }

            // New local symbol table for this block
            _localSymbols = new FortranSymbolCollection("Local");
            _hasReturn = false;

            // Parameter list allowed for subroutines and functions, but
            // not the main program.
            Collection<Symbol> parameters = null;
            int alternateReturnCount = 0;

            switch (klass) {
                case SymClass.PROGRAM:
                    parameters = new Collection<Symbol>();
                    klass = SymClass.SUBROUTINE;
                    methodName = _entryPointName;
                    break;

                case SymClass.FUNCTION:
                    parameters = ParseParameterDecl(_localSymbols, SymScope.PARAMETER, out alternateReturnCount);
                    break;

                case SymClass.SUBROUTINE:
                    parameters = ParseParameterDecl(_localSymbols, SymScope.PARAMETER, out alternateReturnCount);
                    if (alternateReturnCount > 0) {
                        returnType = new SymFullType(SymType.INTEGER);
                    }
                    break;
            }

            // Don't allow alternate returns for anything except subroutines
            if (alternateReturnCount > 0 && klass != SymClass.SUBROUTINE) {
                Messages.Error(MessageCode.ALTRETURNNOTALLOWED, "Alternate return only permitted for subroutines");
            }

            // Add this method to the global symbol table now.
            if (method == null) {
                method = _globalSymbols.Add(methodName, new SymFullType(), klass, null, _ls.LineNumber);
            }

            method.Parameters = parameters;
            method.Defined = true;
            method.Class = klass;

            if (returnType.Type != SymType.NONE) {
                method.FullType = returnType;
            }

            if (methodName == _entryPointName) {
                method.Modifier |= SymModifier.ENTRYPOINT;
                _hasProgram = true;
            }

            // Special case for functions. Create a local symbol with the same
            // name to be used for the return value.
            if (klass == SymClass.FUNCTION || alternateReturnCount > 0) {
                method.RetVal = _localSymbols.Add(methodName, returnType, SymClass.VAR, null, _ls.LineNumber);
                method.RetVal.Modifier = SymModifier.RETVAL;
            }
            
            node.ProcedureSymbol = method;
            node.LocalSymbols.Add(_localSymbols);
            node.LabelList = new Collection<ParseNode>();

            _currentProcedure = node;
            _currentProcedure.AlternateReturnCount = alternateReturnCount;

            // Always catch run-time exceptions.
            _currentProcedure.CatchExceptions = true;

            _initList = new CollectionParseNode();
            node.InitList = _initList;

            SimpleToken token = _ls.GetKeyword();
            while (token.ID != TokenID.ENDOFFILE) {
                if (token.ID != TokenID.EOL) {
                    ParseNode labelNode = CheckLabel();
                    if (labelNode != null) {
                        node.Body.Add(labelNode);
                    }
                    if (token.ID == TokenID.KEND) {
                        break;
                    }
                    ParseNode lineNode = Statement(token);
                    if (lineNode != null) {
                        node.Body.Add(MarkLine());
                        node.Body.Add(lineNode);
                    }
                    ExpectEndOfLine();
                }
                token = _ls.GetKeyword();
            }

            // If we hit the end of the file first then we're missing
            // a mandatory END statement.
            if (token.ID != TokenID.KEND) {
                Messages.Error(MessageCode.MISSINGENDSTATEMENT, "Missing END statement");
            }

            // Make sure we have a RETURN statement.
            if (!_hasReturn) {
                node.Body.Add(new ReturnParseNode());
            }

            // Validate the block.
            foreach (Symbol sym in _localSymbols) {
                if (sym.IsLabel && !sym.Defined) {
                    Messages.Error(MessageCode.UNDEFINEDLABEL, sym.RefLine, $"Undefined label {sym.Name}");
                }
                if (_saveAll && sym.IsLocal) {
                    sym.Modifier |= SymModifier.STATIC;
                }
                
                // For non-array characters, if there's no value, set the empty string
                if (sym.Type == SymType.FIXEDCHAR && !sym.IsArray && !sym.Value.HasValue) {
                    sym.Value = new Variant(string.Empty);
                }
                
                if (!sym.IsReferenced && !sym.Modifier.HasFlag(SymModifier.RETVAL)) {
                    string scopeName = sym.IsParameter ? "parameter" : sym.IsLabel ? "label" : "variable";
                    Messages.Warning(MessageCode.UNUSEDVARIABLE,
                                      3,
                                      sym.RefLine,
                        $"Unused {scopeName} {sym.Name} in function");
                }
            }
            ValidateBlock(0, node.Body);
            _state = BlockState.SPECIFICATION; 
            return node;
        }

        // INTRINSIC keyword
        // Specifies the given names to be intrinsic functions. These are added to
        // the symbol table as INTRINSIC types so they can be passed as arguments.
        private ParseNode KIntrinsic() {
            SimpleToken token;
            EnsureNoLabel();
            do {
                IdentifierToken identToken = ExpectIdentifierToken();
                if (identToken != null) {
                    Symbol sym = _localSymbols.Get(identToken.Name);
                    if (sym == null) {
                        sym = _localSymbols.Add(identToken.Name, new SymFullType(), SymClass.INTRINSIC, null, _ls.LineNumber);
                    } else {
                        Messages.Error(MessageCode.IDENTIFIERREDEFINITION,
                            $"Intrinsic {identToken.Name} already declared");
                    }
                    sym.Defined = true;
                    sym.Info = null;

                    Type libraryType = typeof(JFortranLib.Intrinsics);
                    IntrDefinition intrDefinition = Intrinsics.IntrinsicDefinition(sym.Name);

                    if (intrDefinition != null) {

                        if (!intrDefinition.IsPermittedInIntrinsic) {
                            Messages.Error(MessageCode.NOTALLOWEDININTRINSIC, $"{sym.Name} not permitted in INTRINSIC");
                        }

                        int paramCount = 1;
                        switch (intrDefinition.Count) {
                            case ArgCount.One:       paramCount = 1; break;
                            case ArgCount.OneOrTwo:  paramCount = 2; break;
                            case ArgCount.Two:       paramCount = 2; break;
                            case ArgCount.TwoOrMore: paramCount = 1; break;
                        }
                        Type [] paramTypes = new Type[paramCount];
                        for (int c = 0; c < paramCount; ++c) {
                            SymType argType = intrDefinition.ArgType();
                            if (argType == SymType.GENERIC) {

                                // If this is a generic intrinsic then pick the largest argument
                                // type on the assumption that is what the argument will promote
                                // to. BUGBUG: This is actually wrong though. SQRT, for example,
                                // defaults to REAL.
                                if (intrDefinition.IsValidArgType(SymType.DOUBLE)) {
                                    argType = SymType.DOUBLE;
                                } else if (intrDefinition.IsValidArgType(SymType.FLOAT)) {
                                    argType = SymType.FLOAT;
                                } else {
                                    argType = SymType.INTEGER;
                                }
                            }
                            paramTypes[c] = Symbol.SymTypeToSystemType(argType).MakeByRefType();
                        }
                        sym.Info = libraryType.GetMethod(intrDefinition.FunctionName, paramTypes);
                        sym.FullType = new SymFullType(intrDefinition.ReturnType);
                        sym.Linkage = SymLinkage.BYVAL;
                    }
                    if (sym.Info == null) {
                        Messages.Error(MessageCode.UNDEFINEDINTRINSIC, sym.RefLine, $"Undefined intrinsic {sym.Name}");
                    }
                }
                token = _ls.GetToken();
            } while (token.ID == TokenID.COMMA);
            _ls.BackToken();
            return null;
        }

        // EXTERNAL keyword
        // Specifies the given names to be an external function or subroutine.
        // as arguments.
        private ParseNode KExternal() {
            SimpleToken token;
            EnsureNoLabel();
            do {
                IdentifierToken identToken = ExpectIdentifierToken();
                if (identToken != null) {
                    Symbol sym = _localSymbols.Get(identToken.Name);

                    // External scope being given to a dummy argument
                    if (sym != null && sym.IsParameter) {
                        sym.Class = SymClass.FUNCTION;
                    } else {
                        SymFullType fullType = new();
                        if (sym != null) {
                            fullType = sym.FullType;
                            _localSymbols.Remove(sym);
                        }
                        sym = _globalSymbols.Add(identToken.Name, fullType, SymClass.FUNCTION, null, _ls.LineNumber);
                        sym.Modifier |= SymModifier.EXTERNAL;
                        sym.Defined = true;
                    }
                    sym.Linkage = SymLinkage.BYVAL;
                }
                token = _ls.GetToken();
            } while (token.ID == TokenID.COMMA);
            _ls.BackToken();
            return null;
        }

        /// DIMENSION keyword
        /// Used to apply a dimension to a prior declared variable. Can be used to
        /// declare a variable assuming implicit type is permitted for the name.
        private ParseNode KDimension() {
            SymFullType fullType = new(SymType.NONE);
            SimpleToken token;

            do {
                Symbol sym = ParseIdentifierDeclaration(fullType);
                if (sym != null) {
                    if (sym.Dimensions == null || sym.Dimensions.Count == 0) {
                        Messages.Error(MessageCode.MISSINGARRAYDIMENSIONS, "Array dimensions expected");
                    }
                    sym.Defined = false;
                }
                token = _ls.GetToken();
            } while (token.ID == TokenID.COMMA);
            _ls.BackToken();
            return null;
        }
        
        /// All the type declarations, one by one.
        private ParseNode KCharacter()  { return KDeclaration(SymType.FIXEDCHAR); }
        private ParseNode KInteger()    { return KDeclaration(SymType.INTEGER); }
        private ParseNode KLogical()    { return KDeclaration(SymType.BOOLEAN); }
        private ParseNode KDouble()     { return KDeclaration(SymType.DOUBLE); }
        private ParseNode KReal()       { return KDeclaration(SymType.FLOAT); }
        private ParseNode KComplex()    { return KDeclaration(SymType.COMPLEX); }

        /// Handle a declaration statement of the specified type.
        private ParseNode KDeclaration(SymType type) {
            SymFullType fullType = new(type);
            SimpleToken token;

            if (Symbol.IsCharType(type)) {
                fullType.Width = ParseTypeWidth(0);
            }

            // Could be a function declaration preceded by type?
            if (_ls.PeekKeyword() == TokenID.KFUNCTION) {
                _ls.GetToken();
                return KSubFunc(SymClass.FUNCTION, null, fullType);
            }

            do {
                Symbol sym = ParseIdentifierDeclaration(fullType);
                if (sym != null) {
                    if (sym.Defined) {
                        Messages.Error(MessageCode.IDENTIFIERREDEFINITION, $"Identifier {sym.Name} already declared");
                    }
                    if (sym.IsParameter) {
                        if (!sym.IsArray && !sym.IsMethod && sym.IsValueType) {
                            sym.Linkage = SymLinkage.BYREF;
                        } else {
                            sym.Linkage = SymLinkage.BYVAL;
                        }
                    }
                    sym.Defined = true;
                }
                token = _ls.GetToken();
            } while (token.ID == TokenID.COMMA);
            _ls.BackToken();
            return null;
        }

        /// IMPLICIT NONE keyword
        /// Mark all identifiers as requiring explicit types.
        private ParseNode KImplicitNone() {
            EnsureNoLabel();
            Messages.Warning(MessageCode.IMPLICITNONENOTSTANDARD, 4, "IMPLICIT NONE is not standard");
            _localSymbols.Implicit('A', 'Z', new SymFullType(SymType.NONE));
            return null;
        }

        // Maps a type keyword token to a symbol type.
        private static SymType TokenToType(TokenID token) {
            switch (token) {
                case TokenID.KINTEGER:          return SymType.INTEGER;
                case TokenID.KREAL:             return SymType.FLOAT;
                case TokenID.KDPRECISION:       return SymType.DOUBLE;
                case TokenID.KLOGICAL:          return SymType.BOOLEAN;
                case TokenID.KCHARACTER:        return SymType.FIXEDCHAR;
                case TokenID.KCOMPLEX:          return SymType.COMPLEX;
                    
                default:
                    Debug.Assert(true, "TokenToType called with invalid argument");
                    return SymType.NONE;
            }
        }

        /// IMPLICIT keyword
        /// Syntax: IMPLICIT type (c1-c2) - all identifiers beginning with the letter or letter range c1 to c2
        ///             are implicitly typed with the given type.
        private ParseNode KImplicit() {
            SimpleToken token;
            EnsureNoLabel();
            do {
                token = _ls.GetKeyword();
                switch (token.ID) {
                    case TokenID.KINTEGER:
                    case TokenID.KDPRECISION:
                    case TokenID.KCHARACTER:
                    case TokenID.KLOGICAL:
                    case TokenID.KCOMPLEX:
                    case TokenID.KREAL: {
                        SymFullType fullType = new(TokenToType(token.ID)) {
                            Width = ParseTypeWidth(1)
                        };

                        ExpectToken(TokenID.LPAREN);
                        do {
                            IdentifierToken identToken = ExpectIdentifierToken();
                            if (identToken != null) {
                                char ch1 = identToken.Name[0];
                                char ch2 = ch1;

                                if (ch1 < 'A' || ch1 > 'Z') {
                                    Messages.Error(MessageCode.IMPLICITSINGLECHAR, "IMPLICIT must have a single character");
                                }

                                token = _ls.GetToken();
                                if (token.ID == TokenID.MINUS) {
                                    identToken = ExpectIdentifierToken();
                                    if (identToken != null) {
                                        ch2 = identToken.Name[0];
                                        if (ch2 < 'A' || ch2 > 'Z') {
                                            Messages.Error(MessageCode.IMPLICITSINGLECHAR, "IMPLICIT must have a single character");
                                        }
                                        if (ch2 < ch1) {
                                            Messages.Error(MessageCode.IMPLICITRANGEERROR, "IMPLICIT character range out of sequence");
                                        }
                                        token = _ls.GetToken();
                                    }
                                }

                                // Check for duplicate definitions
                                for (char chTmp = ch1; chTmp <= ch2; ++chTmp) {
                                    if (_localSymbols.IsImplicitSet(chTmp)) {
                                        Messages.Error(MessageCode.IMPLICITCHAREXISTS,
                                            $"Character {chTmp} already specified in an IMPLICIT");
                                    }
                                    _localSymbols.SetImplicit(chTmp);
                                }

                                _localSymbols.Implicit(ch1, ch2, fullType);
                            }
                        } while (token.ID == TokenID.COMMA);
                        _ls.BackToken();
                        ExpectToken(TokenID.RPAREN);
                        _state = BlockState.IMPLICIT;
                        break;
                    }

                    default:
                        Messages.Error(MessageCode.IMPLICITSYNTAXERROR, "Syntax error in IMPLICIT statement");
                        break;
                }
                token = _ls.GetToken();
            } while (token.ID == TokenID.COMMA);
            _ls.BackToken();
            return null;
        }
    }
}