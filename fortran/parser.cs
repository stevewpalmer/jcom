// JFortran Compiler
// General parsing routines
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

using System.Collections.ObjectModel;
using System.Diagnostics;
using CCompiler;
using JComLib;

namespace JFortran;

/// <summary>
/// Provides a class that encapsulates a list of I/O control statements.
/// </summary>
public class ControlList {
    private readonly Dictionary<string, ParseNode> _cilist;

    /// <summary>
    /// Constructs an empty ControlList instance.
    /// </summary>
    public ControlList() {
        _cilist = new Dictionary<string, ParseNode>();
    }

    /// <summary>
    /// Returns whether or not the control list contains a value for the
    /// given control name.
    /// </summary>
    /// <returns><c>true</c> if this list has name; otherwise, <c>false</c>.</returns>
    /// <param name="name">A control name</param>
    public bool Has(string name) {
        return _cilist.ContainsKey(name);
    }

    /// <summary>
    /// Indexer to provide access to list elements.
    /// </summary>
    /// <param name="name">List element name</param>
    public ParseNode this[string name] {
        get {
            _cilist.TryGetValue(name, out ParseNode node);
            return node;
        }
        set => _cilist[name] = value;
    }
}

public partial class Compiler {

    // Parse an optional type width specifier such as is used with
    // CHARACTER types.
    private int ParseTypeWidth(int defaultWidth) {
        SimpleToken token = _ls.GetToken();
        int width = defaultWidth;

        if (token.ID == TokenID.STAR) {
            if (_ls.PeekToken().ID == TokenID.LPAREN) {
                ExpectToken(TokenID.LPAREN);
                if (_ls.PeekToken().ID == TokenID.STAR) {
                    ExpectToken(TokenID.STAR);
                    width = 255;
                }
                else {
                    ParseIntegerValue(out width);
                }
                ExpectToken(TokenID.RPAREN);
            }
            else {
                if (ParseIntegerValue(out width)) {
                    if (width < 1) {
                        Messages.Error(MessageCode.BADTYPEWIDTH, "Type width cannot be less than 1");
                        width = defaultWidth;
                    }
                }
            }
        }
        else {
            _ls.BackToken();
        }
        return width;
    }

    // Parse an integer, real or string constant
    private Variant ParseConstantExpression() {
        ParseNode tokenNode = Expression();
        if (tokenNode is { IsConstant: true }) {
            return tokenNode.Value;
        }
        Messages.Error(MessageCode.CONSTANTEXPECTED, "Constant expression expected");
        return new Variant(0);
    }

    // Parse an integer, real or string constant
    private Variant ParseConstant() {
        ParseNode tokenNode = SimpleExpresion();
        if (tokenNode is { IsConstant: true }) {
            return tokenNode.Value;
        }
        Messages.Error(MessageCode.CONSTANTEXPECTED, "Constant expected");
        return new Variant(0);
    }

    // Parse a STOP/PAUSE argument
    private StringParseNode ParseStopPauseConstant() {
        string str = ParseStringLiteral();
        if (string.IsNullOrWhiteSpace(str)) {
            str = string.Empty;
        }
        else {
            if (char.IsDigit(str[0])) {
                if (!int.TryParse(str, out int value) || value > 99999) {
                    Messages.Error(MessageCode.BADSTOPFORMAT, "Illegal number code format");
                    str = string.Empty;
                }
            }
            else {
                str = str.Trim().Trim('"', '\'');
            }
        }
        return new StringParseNode(str);
    }

    // Parse and return a string literal
    private string ParseStringLiteral() {
        SimpleToken token = ExpectToken(TokenID.STRING);
        StringToken stringToken = (StringToken)token;
        return stringToken?.String;
    }

    // Parse an expression, collapse it and return the integer value or
    // raise an error otherwise.
    private bool ParseIntegerValue(out int intval) {
        ParseNode tokenNode = Expression();
        if (tokenNode is not { ID: ParseID.NUMBER }) {
            Messages.Error(MessageCode.INTEGEREXPECTED, "Integer value expected");
            intval = 0;
            return false;
        }
        intval = tokenNode.Value.IntValue;
        return true;
    }

    // Parse a variable length argument list.
    private VarArgParseNode ParseVarargList() {
        VarArgParseNode varargs = null;
        if (!IsAtEndOfLine()) {
            varargs = new VarArgParseNode();
            SimpleToken token;

            // Optional comma.
            SkipToken(TokenID.COMMA);

            do {
                if (_ls.PeekToken().ID == TokenID.LPAREN) {
                    _ls.GetToken();
                    varargs.Add(ParseImpliedDo());
                }
                else {
                    varargs.Add(Expression());
                }
                token = _ls.GetToken();
            } while (token.ID == TokenID.COMMA);
            _ls.BackToken();
        }
        return varargs;
    }

    // Parse a variable length argument list of references.
    private VarArgParseNode ParseVarargReferenceList() {
        VarArgParseNode varargs = null;
        if (!IsAtEndOfLine()) {
            varargs = new VarArgParseNode();
            SimpleToken token;

            // Optional comma.
            SkipToken(TokenID.COMMA);

            do {
                varargs.Add(ParseIdentifierWithImpliedDo());
                token = _ls.GetToken();
            } while (token.ID == TokenID.COMMA);
            _ls.BackToken();
        }
        return varargs;
    }

    // Parse a label
    private SymbolParseNode ParseLabel() {
        SimpleToken token = ExpectToken(TokenID.INTEGER);
        if (token != null) {
            IntegerToken intToken = (IntegerToken)token;
            return new SymbolParseNode(GetMakeLabel(intToken.Value.ToString(), false));
        }
        return null;
    }

    // Parse a boolean yes/no value
    private NumberParseNode ParseBoolean() {
        SimpleToken token = ExpectToken(TokenID.STRING);
        if (token != null) {
            StringToken stringToken = token as StringToken;
            Debug.Assert(stringToken != null);
            NumberParseNode node = new(stringToken.String.ToLower() == "yes" ? 1 : 0);
            return node;
        }
        return null;
    }

    // Parse an identifier. Also accept an implied DO loop for array
    // iteration.
    private ParseNode ParseIdentifierWithImpliedDo() {
        SimpleToken token = _ls.GetToken();
        if (token.ID == TokenID.LPAREN) {
            return ParseImpliedDo();
        }

        // It's an ordinary identifier reference
        _ls.BackToken();
        return ParseIdentifier();
    }

    // Parse an expression. Also accept an implied DO loop for array
    // iteration.
    private ParseNode ParseExpressionWithImpliedDo() {
        SimpleToken token = _ls.GetToken();
        if (token.ID == TokenID.LPAREN) {
            return ParseImpliedDo();
        }

        // It's an expression
        _ls.BackToken();
        return Expression();
    }

    // Parse an implied do.
    private LoopParseNode ParseImpliedDo() {
        LoopParseNode node = new() {

            // The first element of an implied DO loop is an expression
            // which uses the loop variable as an operand.
            LoopValue = ParseExpressionWithImpliedDo()
        };
        ExpectToken(TokenID.COMMA);

        // Control identifier
        node.LoopVariable = ParseBasicIdentifier();

        // Range. This is:
        //  start, end [,step]
        // where step defaults to 1 if omitted
        ExpectToken(TokenID.EQUOP);
        node.StartExpression = Expression();
        ExpectToken(TokenID.COMMA);
        node.EndExpression = Expression();
        if (_ls.PeekToken().ID == TokenID.COMMA) {
            ExpectToken(TokenID.COMMA);
            node.StepExpression = Expression();
        }
        ExpectToken(TokenID.RPAREN);
        return node;
    }

    // Parse an identifier with no subscript.
    private IdentifierParseNode ParseBasicIdentifier() {
        IdentifierToken identToken = ExpectIdentifierToken();
        if (identToken != null) {
            Symbol sym = GetMakeSymbolForCurrentScope(identToken.Name);
            if (sym == null) {
                Messages.Error(MessageCode.UNDEFINEDVARIABLE, $"Undefined identifier {identToken.Name}");
                return null;
            }
            IdentifierParseNode node = new(sym);
            sym.IsReferenced = true;
            return node;
        }
        return null;
    }

    // Parse an identifier. Possible syntaxes are:
    //    identifier
    //    identifier(index [,index]*)
    // where index can be an expression that evaluates to an integer or a range
    // expression of the format low:high where each side evaluates to an integer.
    private IdentifierParseNode ParseIdentifier() {
        IdentifierToken identToken = ExpectIdentifierToken();
        return identToken != null ? ParseIdentifierFromToken(identToken) : null;
    }

    // Parse an identifier from the specified token and assign the corresponding
    // symbol to the identifier parse node.
    private IdentifierParseNode ParseIdentifierFromToken(IdentifierToken identToken) {
        IdentifierParseNode node = ParseIdentifierParseNode();

        if (node != null) {
            Symbol sym = GetMakeSymbolForCurrentScope(identToken.Name);
            if (sym == null) {
                Messages.Error(MessageCode.UNDEFINEDVARIABLE, $"Undefined identifier {identToken.Name}");
                return null;
            }
            node.Symbol = sym;
            sym.IsReferenced = true;
        }
        return node;
    }

    // Parse an identifier parse node from the specified token.
    private IdentifierParseNode ParseIdentifierParseNode() {
        IdentifierParseNode node = new(null);
        Collection<ParseNode> indices = null;
        SimpleToken token = _ls.GetToken();
        bool isSubstring = false;

        while (token.ID == TokenID.LPAREN) {
            indices ??= [];
            if (_ls.PeekToken().ID != TokenID.RPAREN) {
                do {
                    ParseNode item = null;

                    if (_ls.PeekToken().ID == TokenID.RPAREN) {
                        SkipToken(TokenID.RPAREN);
                        break;
                    }
                    if (_ls.PeekToken().ID != TokenID.COLON) {
                        item = Expression();
                    }
                    token = _ls.GetToken();
                    if (token.ID == TokenID.COLON) {
                        isSubstring = true;
                        item ??= new NumberParseNode(1);
                        node.SubstringStart = item;
                        token = new SimpleToken(TokenID.COMMA);
                        continue;
                    }
                    if (isSubstring) {
                        node.SubstringEnd = item;
                        break;
                    }
                    indices.Add(item);
                } while (token.ID == TokenID.COMMA);
                _ls.BackToken();
            }
            ExpectToken(TokenID.RPAREN);
            token = _ls.GetToken();
        }

        node.Indexes = indices;
        _ls.BackToken();
        return node;
    }

    // Parse an expression and ensure that it evaluates to any of the
    // requested types. The first one in the list should be the
    // primary type as that is what gets shown in the error message.
    private ParseNode ParseExpressionOfTypes(SymType[] expectedType) {
        ParseNode expr = Expression();
        if (expectedType.Any(type => expr.Type == type)) {
            return expr;
        }
        Messages.Error(MessageCode.TYPEMISMATCH, $"{expectedType[0]} type expected");
        return expr;
    }

    // Parse a function or subroutine parameter declaration and return an
    // array of symbols for all parameters
    private Collection<Symbol> ParseParameterDecl(FortranSymbolCollection symbolTable,
        SymScope scope,
        out int countOfAlternateReturn) {
        SimpleToken token = _ls.GetToken();
        Collection<Symbol> parameters = [];

        countOfAlternateReturn = 0;
        if (token.ID == TokenID.LPAREN) {
            if (_ls.PeekToken().ID != TokenID.RPAREN) {
                do {
                    if (_ls.PeekToken().ID == TokenID.STAR) {
                        SkipToken(TokenID.STAR);
                        ++countOfAlternateReturn;
                    }
                    else {
                        IdentifierToken identToken = ExpectIdentifierToken();
                        if (identToken != null) {
                            Symbol sym = symbolTable.Get(identToken.Name);
                            if (sym != null) {
                                Messages.Error(MessageCode.PARAMETERDEFINED,
                                    $"Parameter {identToken.Name} already defined");
                            }
                            else {
                                sym = symbolTable.Add(identToken.Name, new SymFullType(), SymClass.VAR, null, _ls.LineNumber);
                                sym.Scope = scope;
                                if (!sym.IsArray && !sym.IsMethod && (sym.IsValueType || sym.Type == SymType.FIXEDCHAR)) {
                                    sym.Linkage = SymLinkage.BYREF;
                                }
                                parameters.Add(sym);
                            }
                            if (countOfAlternateReturn > 0) {
                                Messages.Error(MessageCode.ALTRETURNORDER, "Alternate return markers must be at the end");
                            }
                        }
                    }
                    token = _ls.GetToken();
                } while (token.ID == TokenID.COMMA);
                _ls.BackToken();
            }
            ExpectToken(TokenID.RPAREN);
        }
        else {
            _ls.BackToken();
        }
        return parameters;
    }

    // Parse a format specifier:
    // A format identifier identifies a format. A format identifier must be one of the following:
    // 1. The statement label of a FORMAT statement that appears in the same program unit as
    //    the format identifier.
    // 2. An integer variable name that has been assigned the statement label of a FORMAT statement
    //    that appears in the same program unit as the format identifier (10.3).
    // 3. A character array name (13.1.2).
    // 4. Any character expression except a character expression involving concatenation of an
    //    operand whose length specification is an asterisk in parentheses unless the operand is the
    //    symbolic name of a constant. Note that a character constant is permitted.
    // 5. An asterisk, specifying list-directed formatting.
    //
    // NOTE: We do not yet handle assigned labels, so option 2 is not supported here.
    //
    private ParseNode ParseFormatSpecifier() {
        SimpleToken token = _ls.PeekToken();

        if (token.ID == TokenID.INTEGER) {
            return ParseLabel();
        }
        if (token.ID == TokenID.STAR) {
            SkipToken(token.ID);
            return new StringParseNode("*");
        }
        return ParseExpressionOfTypes([SymType.CHAR, SymType.FIXEDCHAR]);
    }

    // Parse a unit specifier:
    // An external unit identifier is one of the following:
    //
    // 1. An integer expression i whose value must be zero or positive
    // 2. An asterisk, identifying a particular processor-determined external unit
    //    that is preconnected for formatted sequential access (12.9.2)
    //
    private ParseNode ParseUnitSpecifier() {
        SimpleToken token = _ls.PeekToken();

        if (token.ID == TokenID.STAR) {
            SkipToken(token.ID);
            return null;
        }
        return ParseExpressionOfTypes([SymType.INTEGER]);
    }

    // Parse a control list.
    // The return value is a dictionary of parsenodes where the key value is
    // the parameter name. The parse node may either be an expression or a
    // literal value depending on context.
    private ControlList ParseCIList(Collection<string> allowedSpecifiers) {
        ControlList cilist = new();
        ParseNode node = null;
        int index = 0;

        // Check for the simple format first
        if (_ls.PeekToken().ID != TokenID.LPAREN) {
            if (allowedSpecifiers.Contains("FMT")) {
                cilist["FMT"] = ParseFormatSpecifier();
            }
            else if (allowedSpecifiers.Contains("UNIT")) {
                cilist["UNIT"] = ParseUnitSpecifier();
            }
        }
        else {
            ExpectToken(TokenID.LPAREN);
            SimpleToken token;
            do {
                string paramName;

                token = _ls.GetToken();
                if (token.ID == TokenID.IDENT && _ls.PeekToken().ID == TokenID.EQUOP) {
                    IdentifierToken identToken = (IdentifierToken)token;
                    paramName = identToken.Name;
                    ExpectToken(TokenID.EQUOP);
                }
                else {
                    _ls.BackToken();
                    if (index == 0) {
                        paramName = "UNIT";
                    }
                    else if (index == 1) {
                        paramName = "FMT";
                    }
                    else {
                        Messages.Error(MessageCode.CILISTERROR, $"Parameter at position {index} must be named");
                        return null;
                    }
                }
                if (cilist.Has(paramName)) {
                    Messages.Error(MessageCode.CILISTSPECIFIED, $"Parameter {paramName} already specified");
                    return null;
                }
                if (!allowedSpecifiers.Contains(paramName)) {
                    Messages.Error(MessageCode.CILISTNOTALLOWED, $"Parameter {paramName} not allowed here");
                    return null;
                }
                switch (paramName) {
                    case "UNIT":
                        if (_ls.PeekToken().ID != TokenID.STAR) {
                            node = Expression();
                        }
                        else {
                            _ls.GetToken();
                        }
                        break;

                    case "FMT":
                        node = ParseFormatSpecifier();
                        break;

                    case "ADVANCE":
                        node = ParseBoolean();
                        break;

                    case "ACCESS":
                    case "FILE":
                    case "FORM":
                    case "STATUS":
                    case "BLANK":
                        node = ParseExpressionOfTypes([SymType.FIXEDCHAR, SymType.CHAR]);
                        break;

                    case "END":
                    case "ERR":
                        node = ParseLabel();
                        break;

                    case "REC":
                    case "RECL":
                        node = ParseExpressionOfTypes([SymType.INTEGER]);
                        break;

                    case "IOSTAT":
                    case "EXIST":
                    case "OPENED":
                    case "NUMBER":
                    case "NAMED":
                    case "NAME":
                    case "SEQUENTIAL":
                    case "DIRECT":
                    case "FORMATTED":
                    case "UNFORMATTED":
                    case "NEXTREC":
                        node = ParseIdentifier();
                        break;
                }
                if (node != null) {
                    cilist[paramName] = node;
                }
                ++index;
                token = _ls.GetToken();
            } while (token.ID == TokenID.COMMA);
            _ls.BackToken();
            ExpectToken(TokenID.RPAREN);
        }

        // Add FMT default
        if (!cilist.Has("FMT")) {
            cilist["FMT"] = new StringParseNode(string.Empty);
        }
        return cilist;
    }

    // Parse an identifier declaration
    private Symbol ParseIdentifierDeclaration(SymFullType fullType) {
        IdentifierToken identToken = ExpectIdentifierToken();
        Symbol sym = null;

        if (identToken != null) {

            // Ban any conflict with PROGRAM name or the current function
            Symbol globalSym = _globalSymbols.Get(identToken.Name);
            if (globalSym is { Type: SymType.PROGRAM }) {
                Messages.Error(MessageCode.IDENTIFIERISGLOBAL,
                    $"Identifier {identToken.Name} already has global declaration");
                SkipToEndOfLine();
                return null;
            }

            // Now check the local program unit
            sym = _localSymbols.Get(identToken.Name);

            // Handle array syntax and build a list of dimensions
            Collection<SymDimension> dimensions = ParseArrayDimensions();
            if (dimensions == null) {
                return null;
            }

            // If this is the main program, all dimensions must be constant
            if (_currentProcedure is { IsMainProgram: true }) {
                if (dimensions.Any(dim => dim.Size < 0)) {
                    Messages.Error(MessageCode.ARRAYILLEGALBOUNDS, "Array dimensions must be constant");
                }
            }

            // Check for width specifier. This always follows array bounds and if one is
            // specified, it only applies to this identifier so the original width, if any,
            // must be preserved.
            int width = fullType.Width;
            if (width == 0) {
                SymFullType impliedFullType = _localSymbols.ImplicitTypeForCharacter(identToken.Name[0]);
                width = impliedFullType.Width;
            }
            SymFullType thisFullType = new(fullType.Type, ParseTypeWidth(width));

            // Indicate this symbol is explicitly declared
            if (sym == null) {
                if (identToken.Name.Length > 6 && !_opts.F90) {
                    Messages.Error(MessageCode.IDENTIFIERTOOLONG, $"Identifier {identToken.Name} length too long");
                }
                sym = _localSymbols.Add(identToken.Name, thisFullType, SymClass.VAR, dimensions, _ls.LineNumber);
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
                sym.Modifier |= SymModifier.FLATARRAY; // FORTRAN always uses flat arrays
            }
        }
        return sym;
    }

    // Parse set of array dimensions if one is found.
    private Collection<SymDimension> ParseArrayDimensions() {
        Collection<SymDimension> dimensions = [];
        SimpleToken token = _ls.PeekToken();
        bool hasAssumedBound = false;

        if (token.ID == TokenID.LPAREN) {
            ExpectToken(TokenID.LPAREN);
            do {
                do {
                    ParseNode intVal;
                    if (_ls.PeekToken().ID == TokenID.STAR) {
                        SkipToken(TokenID.STAR);
                        intVal = new NumberParseNode(0);
                        hasAssumedBound = true;
                    }
                    else {
                        intVal = IntegerExpression();
                        if (intVal == null) {
                            SkipToEndOfLine();
                            return null;
                        }

                        // Assumed size declaration, which must be the last in
                        // the list. We use a value of 0 for this since the array
                        // calculations disregard it. However, we need to make sure
                        // this IS the last dimension.
                        if (hasAssumedBound) {
                            Messages.Error(MessageCode.ARRAYENDEXPECTED, "Dimensions not permitted after assumed bound");
                            SkipToEndOfLine();
                            return null;
                        }
                    }

                    SymDimension dim = new();

                    // Fortran arrays lower bounds start from 1 but one can
                    // specify a custom bound range with the lower:upper syntax.
                    ParseNode in1 = new NumberParseNode(1);
                    ParseNode in2 = intVal;
                    token = _ls.GetToken();
                    if (token.ID == TokenID.COLON) {
                        if (_ls.PeekToken().ID == TokenID.STAR) {
                            SkipToken(TokenID.STAR);
                            intVal = new NumberParseNode(0);
                            hasAssumedBound = true;
                        }
                        else {
                            intVal = IntegerExpression();
                        }
                        if (intVal != null) {
                            in1 = in2;
                            in2 = intVal;
                        }
                    }
                    else {
                        _ls.BackToken();
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
                    token = _ls.GetToken();
                } while (token.ID == TokenID.COMMA);
                _ls.BackToken();
                ExpectToken(TokenID.RPAREN);
                token = _ls.GetToken();
            } while (token.ID == TokenID.LPAREN);
            _ls.BackToken();
        }
        return dimensions;
    }
}