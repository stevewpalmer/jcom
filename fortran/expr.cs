// JFortran Compiler
// Expression parsing
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

using System.Diagnostics;
using System.Numerics;
using CCompiler;
using JComLib;

namespace JFortran; 

public partial class Compiler {

    // Parse an expression and verify that the return result is an
    // integer type.
    private ParseNode IntegerExpression() {
        ParseNode node = Expression();
        if (node == null || !node.IsInteger) {
            Messages.Error(MessageCode.INTEGEREXPECTED, "Integer expression expected");
            node = null;
        }
        return node;
    }

    // Parse a non-expression operand
    private ParseNode SimpleExpresion() {
        return OptimiseExpressionTree(Operand());
    }

    // Optimise a negation expression where both nodes are literal
    // values. Substitute the node with the result of the negation.
    private ParseNode OptimiseMinus(ParseNode node) {
        UnaryOpParseNode tokenNode = (UnaryOpParseNode)node;
        tokenNode.Operand = OptimiseExpressionTree(tokenNode.Operand);
        if (tokenNode.Operand.IsNumber) {
            NumberParseNode op1 = (NumberParseNode)tokenNode.Operand;
            node = new NumberParseNode(-op1.Value);
        }
        return node;
    }

    // Optimise an addition expression where both nodes are literal
    // values. Substitute the node with the result of the addition.
    private ParseNode OptimiseAddition(ParseNode node) {
        BinaryOpParseNode tokenNode = (BinaryOpParseNode)node;
        tokenNode.Left = OptimiseExpressionTree(tokenNode.Left);
        tokenNode.Right = OptimiseExpressionTree(tokenNode.Right);

        if (tokenNode.IsNumber) {
            NumberParseNode op1 = (NumberParseNode)tokenNode.Left;
            NumberParseNode op2 = (NumberParseNode)tokenNode.Right;
            node = new NumberParseNode(op1.Value + op2.Value);
        }

        // Check for zero simplification
        if (tokenNode.Left.IsNumber) {
            if (tokenNode.Left.Value.IsZero) {
                return tokenNode.Right;
            }
        }
        if (tokenNode.Right.IsNumber) {
            if (tokenNode.Right.Value.IsZero) {
                return tokenNode.Left;
            }
        }
        return node;
    }

    // Optimise a multiplication expression where both nodes are literal
    // values. Substitute the node with the result of the multiplication.
    private ParseNode OptimiseMultiplication(ParseNode node) {
        BinaryOpParseNode tokenNode = (BinaryOpParseNode)node;
        tokenNode.Left = OptimiseExpressionTree(tokenNode.Left);
        tokenNode.Right = OptimiseExpressionTree(tokenNode.Right);

        if (tokenNode.IsNumber) {
            NumberParseNode op1 = (NumberParseNode)tokenNode.Left;
            NumberParseNode op2 = (NumberParseNode)tokenNode.Right;
            node = new NumberParseNode(op1.Value * op2.Value);
        }

        // Check for zero simplification
        if (tokenNode.Left.IsNumber) {
            if (tokenNode.Left.Value.IsZero) {
                return new NumberParseNode(0);
            }
            if (tokenNode.Left.Value.Compare(1)) {
                return tokenNode.Right;
            }
        }
        if (tokenNode.Right.IsNumber) {
            if (tokenNode.Right.Value.IsZero) {
                return new NumberParseNode(0);
            }
            if (tokenNode.Right.Value.Compare(1)) {
                return tokenNode.Left;
            }
        }
        return node;
    }

    // Optimise a division expression where both nodes are literal
    // values. Substitute the node with the result of the division.
    private ParseNode OptimiseDivision(ParseNode node) {
        BinaryOpParseNode tokenNode = (BinaryOpParseNode)node;
        tokenNode.Left = OptimiseExpressionTree(tokenNode.Left);
        tokenNode.Right = OptimiseExpressionTree(tokenNode.Right);
        
        if (tokenNode.IsNumber) {
            NumberParseNode op1 = (NumberParseNode)tokenNode.Left;
            NumberParseNode op2 = (NumberParseNode)tokenNode.Right;
            try {
                node = new NumberParseNode(op1.Value / op2.Value);
            } catch (DivideByZeroException) {
                Messages.Error(MessageCode.DIVISIONBYZERO, "Constant division by zero");
            }
        }
        return node;
    }

    // Optimise a subtraction expression where both nodes are literal
    // values. Substitute the node with the result of the subtraction.
    private ParseNode OptimiseSubtraction(ParseNode node) {
        BinaryOpParseNode tokenNode = (BinaryOpParseNode)node;
        tokenNode.Left = OptimiseExpressionTree(tokenNode.Left);
        tokenNode.Right = OptimiseExpressionTree(tokenNode.Right);
        
        if (tokenNode.IsNumber) {
            NumberParseNode op1 = (NumberParseNode)tokenNode.Left;
            NumberParseNode op2 = (NumberParseNode)tokenNode.Right;
            node = new NumberParseNode(op1.Value - op2.Value);
        }

        // Check for zero simplification
        if (tokenNode.Right.IsNumber) {
            if (tokenNode.Right.Value.IsZero) {
                return tokenNode.Left;
            }
        }
        return node;
    }

    // Optimise an exponentation expression where both nodes are literal
    // values. Substitute the node with the result of the exponentation.
    private ParseNode OptimiseExponentation(ParseNode node) {
        BinaryOpParseNode tokenNode = (BinaryOpParseNode)node;
        tokenNode.Left = OptimiseExpressionTree(tokenNode.Left);
        tokenNode.Right = OptimiseExpressionTree(tokenNode.Right);

        if (tokenNode.IsNumber) {
            NumberParseNode op1 = (NumberParseNode)tokenNode.Left;
            NumberParseNode op2 = (NumberParseNode)tokenNode.Right;
            node = new NumberParseNode(op1.Value.Pow(op2.Value));
        }

        // x raised to the powers of -1, 0 and 1 all yield constant expressions
        // so we can simplify that right now.
        if (tokenNode.Right.IsNumber) {
            Variant rightValue = tokenNode.Right.Value;
            if (rightValue.Compare(-1)) {
                BinaryOpParseNode divNode = new(ParseID.DIVIDE) {
                    Left = new NumberParseNode(new Variant(1)),
                    Right = tokenNode.Left,
                    Type = tokenNode.Left.Type
                };
                return divNode;
            }
            if (rightValue.IsZero) {
                return new NumberParseNode(1);
            }
            if (rightValue.Compare(1)) {
                return tokenNode.Left;
            }
        }
        return node;
    }

    // Concatenate two literal strings
    private ParseNode OptimiseConcatenation(ParseNode node) {
        BinaryOpParseNode tokenNode = (BinaryOpParseNode)node;
        tokenNode.Left = OptimiseExpressionTree(tokenNode.Left);
        tokenNode.Right = OptimiseExpressionTree(tokenNode.Right);
        
        if (tokenNode.Left.ID == ParseID.STRING && tokenNode.Right.ID == ParseID.STRING) {
            StringParseNode op1 = (StringParseNode)tokenNode.Left;
            StringParseNode op2 = (StringParseNode)tokenNode.Right;
            if (op1.Type == SymType.FIXEDCHAR || op2.Type == SymType.FIXEDCHAR) {
                FixedString leftString = new(op1.Value.StringValue);
                FixedString rightString = new(op2.Value.StringValue);
                node = new StringParseNode(FixedString.Merge(leftString, rightString).ToString());
            } else {
                node = new StringParseNode(op1.Value + op2.Value);
            }
        }
        return node;
    }

    // Optimise the parse tree generated by parsing an expression in order to collapse
    // arithmetic operations with constant operands.
    private ParseNode OptimiseExpressionTree(ParseNode node) {
        if (node != null) {
            switch (node.ID) {
                case ParseID.MINUS:     return OptimiseMinus(node);
                case ParseID.ADD:       return OptimiseAddition(node);
                case ParseID.MULT:      return OptimiseMultiplication(node);
                case ParseID.DIVIDE:    return OptimiseDivision(node);
                case ParseID.SUB:       return OptimiseSubtraction(node);
                case ParseID.EXP:       return OptimiseExponentation(node);
                case ParseID.MERGE:     return OptimiseConcatenation(node);

                default:
                    if (node is BinaryOpParseNode tokenNode) {
                        tokenNode.Left = OptimiseExpressionTree(tokenNode.Left);
                        tokenNode.Right = OptimiseExpressionTree(tokenNode.Right);
                    }
                    break;
            }
        }
        return node;
    }

    // Return whether the node is a possible complex part
    private static bool IsComplexPart(ParseNode node) {
        return node.ID == ParseID.NUMBER && (node.Type == SymType.INTEGER || node.Type == SymType.FLOAT);
    }

    // Parse and optimise an expression tree
    private ParseNode Expression() {
        return OptimiseExpressionTree(ParseExpression(0));
    }

    /// OPERATION                   OPERATOR     ORDER OF PRECEDENCE
    /// exponentiate                 **                8
    /// multiply                     *                 7
    /// divide                       /                 7
    /// add                          +                 6
    /// subtract                     -                 6
    /// less than                    .LT.              5
    /// less than or equal to        .LE.              5
    /// equal to                     .EQ.              5
    /// not equal to                 .NE.              5
    /// greater than or equal to     .GE.              5
    /// greater than                 .GT.              5
    /// not                          .NOT.             4
    /// and                          .AND.             3
    /// or                           .OR.              2
    /// xor                          .XOR.             1
    /// neqv                         .NEQV.            1
    /// eqv                          .EQV.             1

    /// Parse an expression.
    private ParseNode ParseExpression(int level) {
        ParseNode op1 = Operand();
        bool done = false;

        while (!done) {
            SimpleToken token = _ls.GetToken();
            bool isRTL = false;
            ParseID parseID;

            int preced;
            switch (token.ID) {
                case TokenID.KEQV:      parseID = ParseID.EQV;      preced = 1; break;
                case TokenID.KNEQV:     parseID = ParseID.NEQV;     preced = 1; break;
                case TokenID.KXOR:      parseID = ParseID.XOR;      preced = 1; break;

                case TokenID.KOR:       parseID = ParseID.OR;       preced = 2; break;

                case TokenID.KAND:      parseID = ParseID.AND;      preced = 3; break;

                case TokenID.KGT:       parseID = ParseID.GT;       preced = 5; break;
                case TokenID.KGE:       parseID = ParseID.GE;       preced = 5; break;
                case TokenID.KLE:       parseID = ParseID.LE;       preced = 5; break;
                case TokenID.KEQ:       parseID = ParseID.EQ;       preced = 5; break;
                case TokenID.KNE:       parseID = ParseID.NE;       preced = 5; break;
                case TokenID.EQUOP:     parseID = ParseID.EQ;       preced = 5; break;
                case TokenID.KLT:       parseID = ParseID.LT;       preced = 5; break;

                case TokenID.PLUS:      parseID = ParseID.ADD;      preced = 6; break;
                case TokenID.MINUS:     parseID = ParseID.SUB;      preced = 6; break;

                case TokenID.STAR:      parseID = ParseID.MULT;     preced = 7; break;
                case TokenID.DIVIDE:    parseID = ParseID.DIVIDE;   preced = 7; break;

                case TokenID.CONCAT:    parseID = ParseID.MERGE;    preced = 8; break;

                case TokenID.EXP:       parseID = ParseID.EXP;      preced = 10; isRTL = true; break;

                default:
                    _ls.BackToken();
                    done = true;
                    continue;
            }
            if (level >= preced) {
                _ls.BackToken();
                done = true;
            } else {

                // For operators that evaluate right to left (such as EXP), drop the
                // precedence so that further occurrences of the same operator to
                // the right are grouped together.
                if (isRTL) {
                    --preced;
                }
                BinaryOpParseNode op = new(parseID) {
                    Left = op1,
                    Right = ParseExpression(preced)
                };
                op1 = TypeEqualise(op);
            }
        }
        return op1;
    }

    /// Parse a single operand
    private ParseNode Operand() {
        SimpleToken token = _ls.GetToken();
        switch (token.ID) {
            case TokenID.LPAREN: {
                ParseNode node = Expression();
                if (IsComplexPart(node) && _ls.PeekToken().ID == TokenID.COMMA) {
                    _ls.GetToken();
                    ParseNode node2 = Expression();
                    ExpectToken(TokenID.RPAREN);
                    if (!IsComplexPart(node2)) {
                        Messages.Error(MessageCode.BADCOMPLEX, "Malformed complex number");
                    }
                    NumberParseNode realPart = (NumberParseNode)node;
                    NumberParseNode imgPart = (NumberParseNode)node2;
                    Complex complexNumber = new(realPart.Value.RealValue, imgPart.Value.RealValue);
                    return new NumberParseNode(new Variant(complexNumber));
                }
                ExpectToken(TokenID.RPAREN);
                return node;
                }

            case TokenID.KNOT: {
                    UnaryOpParseNode node = new(ParseID.NOT) {
                        Operand = ParseExpression(4),
                        Type = SymType.BOOLEAN
                    };
                    return node;
                }

            case TokenID.REAL: {
                RealToken realToken = (RealToken)token;
                return new NumberParseNode(new Variant(realToken.Value));
                }

            case TokenID.DOUBLE: {
                DoubleToken doubleToken = (DoubleToken)token;
                return new NumberParseNode(new Variant(doubleToken.Value));
                }

            case TokenID.KTRUE:
                return new NumberParseNode(new Variant(true));

            case TokenID.KFALSE:
                return new NumberParseNode(new Variant(false));

            case TokenID.PLUS:
                return Operand();

            case TokenID.MINUS: {
                UnaryOpParseNode node = new(ParseID.MINUS);
                ParseNode op = ParseExpression(9);
                node.Operand = op;
                node.Type = op.Type;
                return node;
                }

            case TokenID.INTEGER: {
                IntegerToken intToken = (IntegerToken)token;
                return new NumberParseNode(new Variant(intToken.Value));
                }
                
            case TokenID.STRING: {
                StringToken stringToken = (StringToken)token;
                return new StringParseNode(stringToken.String);
                }

            case TokenID.IDENT: {
                IdentifierToken identToken = (IdentifierToken)token;
                bool match = false;

                // Parse the entire identifier, including array and
                // substring references.
                IdentifierParseNode node = ParseIdentifierParseNode();

                // Constant types are returned as-is.
                Symbol sym = GetSymbolForCurrentScope(identToken.Name);
                if (sym != null && sym.IsConstant) {
                    sym.IsReferenced = true;
                    if (Symbol.IsCharType(sym.Type)) {
                        return new StringParseNode(sym.Value);
                    }
                    return new NumberParseNode(sym.Value);
                }

                // If we're an array we're done.
                if (sym != null && sym.IsArray) {
                    match = true;
                }

                // Substrings? Must be character type
                if (!match && node.HasSubstring) {
                    sym = GetMakeSymbolForCurrentScope(identToken.Name);
                    if (sym != null && sym.Type != SymType.FIXEDCHAR) {
                        Messages.Error(MessageCode.TYPEMISMATCH, "Character type expected");
                        return null;
                    }
                    match = true;
                }

                // Special handling for intrinsics
                if (!match && (sym == null || !sym.IsParameter) && node.Indexes != null) {
                    if (Intrinsics.IsIntrinsic(identToken.Name)) {
                        return IntrinsicOperand(identToken.Name.ToUpper(), node);
                    }
                }
                
                // Statement functions will have been predefined
                if (!match && sym != null && sym.Class == SymClass.INLINE) {
                    match = true;
                }
                
                // If there are any arguments, this is a function call
                if (!match && node.Indexes != null) {
                    return FunctionOperand(identToken.Name.ToUpper(), node);
                }

                // Anything else is an identifier.
                if (sym == null) {
                    sym = GetMakeSymbolForCurrentScope(identToken.Name);
                    if (sym == null) {
                        Messages.Error(MessageCode.UNDEFINEDVARIABLE, $"Undefined identifier {identToken.Name}");
                        return null;
                    }
                }
                if (node.HasSubstring && sym.Type != SymType.FIXEDCHAR) {
                    Messages.Error(MessageCode.TYPEMISMATCH, "Character type expected");
                    return null;
                }
                sym.IsReferenced = true;
                node.Symbol = sym;
                return node;
            }
        }
        Messages.Error(MessageCode.UNRECOGNISEDOPERAND, "Unrecognised operand");
        return null;
    }

    // Parse an intrinsic function call
    private ParseNode IntrinsicOperand(string name, IdentifierParseNode node) {
        IntrDefinition intrDefinition = Intrinsics.IntrinsicDefinition(name);
        Debug.Assert(intrDefinition != null);

        ExtCallParseNode tokenNode = new("JFortranLib.Intrinsics,forlib", intrDefinition.FunctionName);

        SymType argType = SymType.NONE;
        int countOfParams = 0;
       
        bool isVarArg = intrDefinition.Count == ArgCount.TwoOrMore;
        ParametersParseNode paramsNode = new();
        VarArgParseNode argList = new();

        // Parameters to inlined instrinsics are always passed by value.
        bool useByRef = !_opts.Inline || !tokenNode.CanInline();
        if (!intrDefinition.IsPermittedInIntrinsic) {
            useByRef = false;
        }

        for (int c = 0; c < node.Indexes.Count; ++c) {
            ParseNode exprNode = node.Indexes[c];

            if (c > 0 && !ValidateAssignmentTypes(exprNode.Type, argType)) {
                Messages.Error(MessageCode.TYPEMISMATCH, $"All arguments to {name} must be of the same type");
            }
            argType = exprNode.Type;

            if (!intrDefinition.IsValidArgType(argType)) {
                Messages.Error(MessageCode.TYPEMISMATCH, $"Invalid argument type for {name}");
            }
            if (intrDefinition.RequiredType != SymType.GENERIC) {
                exprNode.Type = intrDefinition.RequiredType;
            }

            if (isVarArg) {
                argList.Add(exprNode);
            } else {
                paramsNode.Add(exprNode, useByRef);
            }
            ++countOfParams;
        }
        if (isVarArg) {
            paramsNode.Add(argList, useByRef);
        }
        tokenNode.Parameters = paramsNode;

        // Make sure actual and expected arguments match
        bool match = false;
        switch (intrDefinition.Count) {
            case ArgCount.One: match = countOfParams == 1; break;
            case ArgCount.OneOrTwo: match = countOfParams == 1 || countOfParams == 2; break;
            case ArgCount.Two: match = countOfParams == 2; break;
            case ArgCount.TwoOrMore: match = countOfParams >= 2; break;
            default: Debug.Assert(false, "Unhandled ArgCount!"); break;
        }
        if (!match) {
            Messages.Error(MessageCode.WRONGNUMBEROFARGUMENTS, $"Wrong number of arguments for {name}");
        }

        // Set return type. GENERIC means use the type of the argument
        Debug.Assert(!(intrDefinition.ReturnType == SymType.GENERIC && argType == SymType.NONE), "argType cannot be null here!");
        tokenNode.Type = intrDefinition.ReturnType == SymType.GENERIC ? argType : intrDefinition.ReturnType;
        tokenNode.Inline = _opts.Inline;
        return tokenNode;
    }

    // Parse a function call operand
    private ParseNode FunctionOperand(string name, IdentifierParseNode identNode) {
        Symbol sym = GetSymbolForCurrentScope(name);
        SymType type = SymType.NONE;

        // Look for this symbol name in the local table which means its type
        // was predefined.
        Symbol symLocal = _localSymbols.Get(name);
        if (symLocal != null && symLocal.Scope != SymScope.PARAMETER && symLocal.Class != SymClass.FUNCTION && !symLocal.IsIntrinsic) {
            type = symLocal.Type;
            _localSymbols.Remove(symLocal);
            sym = _globalSymbols.Get(name);
        }

        if (sym == null) {
            sym = _globalSymbols.Add(name, new SymFullType(type), SymClass.FUNCTION, null, _ls.LineNumber);
        }

        // If this was a parameter now being used as a function, change its
        // class and type.
        if (sym.Class != SymClass.FUNCTION) {
            sym.Class = SymClass.FUNCTION;
            sym.Defined = true;
            sym.Linkage = SymLinkage.BYVAL;
        }
        sym.IsReferenced = true;

        CallParseNode node = new() {
            ProcName = new IdentifierParseNode(sym),
            Parameters = new ParametersParseNode(),
            Type = sym.Type
        };
        foreach (ParseNode t in identNode.Indexes) {
            node.Parameters.Add(t, true);
        }
        return node;
    }

    // Do type equalisation on the expression node and report an error if the
    // two operands types are mismatched (eg. string and integer)
    private BinaryOpParseNode TypeEqualise(BinaryOpParseNode node) {
        // Don't check a bogus parse tree.
        if (node.Left == null || node.Right == null) {
            return node;
        }
        switch (node.ID) {
            case ParseID.ADD:
            case ParseID.SUB:
            case ParseID.MULT:
            case ParseID.DIVIDE:
            case ParseID.EXP:
                return ArithmeticEqualise(node);

            case ParseID.MERGE:
                return StringEqualise(node);

            case ParseID.OR:
            case ParseID.AND:
            case ParseID.LE:
            case ParseID.LT:
            case ParseID.GE:
            case ParseID.GT:
            case ParseID.EQ:
            case ParseID.NE:
            case ParseID.EQV:
            case ParseID.NEQV:
            case ParseID.EQUOP:
            case ParseID.ANDTHEN:
            case ParseID.ORTHEN:
                return LogicalEqualise(node);
        }
        Debug.Assert(false, "Unhandled expression node token");
        return null;
    }

    // Verify that both operands of a string operator are strings. This
    // is the only type that is permitted.
    private BinaryOpParseNode StringEqualise(BinaryOpParseNode node) {
        SymType type1 = node.Left.Type;
        SymType type2 = node.Right.Type;
        if (type1 == SymType.CHAR && type2 == SymType.CHAR) {
            node.Type = SymType.CHAR;
            return node;
        }
        if (type1 == SymType.FIXEDCHAR || type2 == SymType.FIXEDCHAR) {
            if (Symbol.IsCharType(type2)) {
                node.Type = SymType.FIXEDCHAR;
                return node;
            }
        }
        Messages.Error(MessageCode.TYPEMISMATCH, "Character operands expected");
        return node;
    }

    // Do type equalisation for arithmetic expressions. In this instance,
    // the type of the result is cast to the largest type that can
    // accommodate the two operands. Anything that evaluates to a non-
    // arithmetic value yields a type mismatch.
    private BinaryOpParseNode ArithmeticEqualise(BinaryOpParseNode node) {
        SymType type1 = node.Left.Type;
        SymType type2 = node.Right.Type;

        if (type1 == SymType.INTEGER) {
            switch (type2) {
                case SymType.DOUBLE:
                    node.Type = SymType.DOUBLE;
                    return node;
                    
                case SymType.FLOAT:
                    node.Type = SymType.FLOAT;
                    return node;

                case SymType.COMPLEX:
                    node.Type = SymType.COMPLEX;
                    return node;

                case SymType.INTEGER:
                    node.Type = SymType.INTEGER;
                    return node;
            }
        }
        if (type1 == SymType.FLOAT) {
            switch (type2) {
                case SymType.DOUBLE:
                    node.Type = SymType.DOUBLE;
                    return node;

                case SymType.COMPLEX:
                    node.Type = SymType.COMPLEX;
                    return node;
            
                case SymType.FLOAT:
                case SymType.INTEGER:
                    node.Type = SymType.FLOAT;
                    return node;
            }
        }
        if (type1 == SymType.DOUBLE) {
            switch (type2) {
                case SymType.DOUBLE:
                case SymType.FLOAT:
                case SymType.INTEGER:
                    node.Type = SymType.DOUBLE;
                    return node;
            }
        }
        if (type1 == SymType.COMPLEX) {
            switch (type2) {
                case SymType.INTEGER:
                case SymType.FLOAT:
                case SymType.COMPLEX:
                    node.Type = SymType.COMPLEX;
                    return node;
            }
        }
        if (type1 == SymType.FIXEDCHAR) {
            switch (type2) {
                case SymType.FIXEDCHAR:
                    node.Type = SymType.FIXEDCHAR;
                    return node;
            }
        }
        Messages.Error(MessageCode.TYPEMISMATCH, "Type mismatch in expression");
        return node;
    }

    // Do type equalisation for logical expressions. In this instance,
    // the two operands must match (numeric vs. numeric, char vs. char
    // or logical vs. logical). The result is always logical.
    private BinaryOpParseNode LogicalEqualise(BinaryOpParseNode node) {
        SymType type1 = node.Left.Type;
        SymType type2 = node.Right.Type;

        if (type1 == SymType.INTEGER ||
            type1 == SymType.FLOAT ||
            type1 == SymType.DOUBLE) {
            switch (type2) {
                case SymType.DOUBLE:
                case SymType.FLOAT:
                case SymType.INTEGER:
                    node.Type = SymType.BOOLEAN;
                    return node;
            }
        }
        if (Symbol.IsCharType(type1) && Symbol.IsCharType(type2)) {
            node.Type = SymType.BOOLEAN;
            return node;
        }
        if (Symbol.IsLogicalType(type1) && Symbol.IsLogicalType(type2)) {
            node.Type = SymType.BOOLEAN;
            return node;
        }
        Messages.Error(MessageCode.TYPEMISMATCH, "Type mismatch in expression");
        return node;
    }
}