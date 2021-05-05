// JComal
// Expression parsing
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
using System.Diagnostics;
using CCompiler;

namespace JComal {

    public partial class Compiler {

        // Parse an expression and verify that the return result is an
        // integer type.
        private ParseNode IntegerExpression() {
            ParseNode node = Expression();
            if (node.Type == SymType.FLOAT) {

                // Floats are assumed to be castable to integers even with loss
                // of precision.
                if (node.IsNumber) {
                    node.Value = new Variant(node.Value.IntValue);
                }
                return CastNodeToType(node, SymType.INTEGER);
            }
            if (!node.IsInteger) {
                Messages.Error(MessageCode.INTEGEREXPECTED, "Integer expected");
            }
            return node;
        }

        // Parse an expression and verify that the return result is an
        // double type.
        private ParseNode DoubleExpression() {
            ParseNode node = Expression();
            if (node.Type != SymType.FLOAT && node.Type != SymType.INTEGER && node.Type != SymType.DOUBLE) {
                Messages.Error(MessageCode.INTEGEREXPECTED, "Number expected");
            }
            if (node.IsNumber) {
                node.Value = new Variant((double)node.Value.RealValue);
            }
            return CastNodeToType(node, SymType.DOUBLE);
        }

        // Parse an expression and verify that the return result is an
        // numeric type.
        private ParseNode NumericExpression() {

            ParseNode node = Expression();
            if (node.Type != SymType.FLOAT && node.Type != SymType.INTEGER && node.Type != SymType.DOUBLE) {
                Messages.Error(MessageCode.INTEGEREXPECTED, "Number expected");
            }
            if (node.Type == SymType.FLOAT) {
                if (node.IsNumber && node.Value.IntValue == node.Value.RealValue) {
                    node.Value = new Variant(node.Value.IntValue);
                    return CastNodeToType(node, SymType.INTEGER);
                }
            }
            if (node.Type == SymType.DOUBLE) {
                if (node.IsNumber && node.Value.IntValue == node.Value.DoubleValue) {
                    node.Value = new Variant(node.Value.IntValue);
                    return CastNodeToType(node, SymType.INTEGER);
                }
            }
            return CastNodeToType(node, SymType.FLOAT);
        }

        // Parse an expression and verify that the return result is an
        // string type.
        private ParseNode StringExpression() {
            ParseNode node = Expression();
            if (!node.IsString) {
                Messages.Error(MessageCode.STRINGEXPECTED, "String expected");
            }
            return node;
        }

        // Parse a non-expression operand
        private ParseNode SimpleExpression() {
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

            if (tokenNode.IsString) {
                return OptimiseConcatenation(node);
            }

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
            if (node is BinaryOpParseNode tokenNode) {
                tokenNode.Left = OptimiseExpressionTree(tokenNode.Left);
                tokenNode.Right = OptimiseExpressionTree(tokenNode.Right);

                if (tokenNode.IsNumber) {
                    NumberParseNode op1 = (NumberParseNode)tokenNode.Left;
                    NumberParseNode op2 = (NumberParseNode)tokenNode.Right;
                    try {
                        if (double.IsInfinity((op1.Value / op2.Value).DoubleValue)) {
                            throw new DivideByZeroException();
                        }
                        if (node.ID == ParseID.IDIVIDE) {
                            node = new NumberParseNode((int)Math.Floor(op1.Value.DoubleValue / op2.Value.DoubleValue));
                        } else {
                            node = new NumberParseNode(op1.Value / op2.Value);
                        }
                    }
                    catch (DivideByZeroException) {
                        Messages.Error(MessageCode.DIVISIONBYZERO, "Division by zero");
                    }
                }

                // Comal DIV behaves differently to the built-in DIV operator so we need
                // to correct this by calling the IDIV intrinsic instead.
                if (node.ID == ParseID.IDIVIDE) {

                    ExtCallParseNode modFunc = GetIntrinsicExtCallNode("IDIV");
                    modFunc.Parameters = new();
                    modFunc.Parameters.Add(CastNodeToType(tokenNode.Left, SymType.INTEGER));
                    modFunc.Parameters.Add(CastNodeToType(tokenNode.Right, SymType.INTEGER));
                    CastNodeToType(modFunc, SymType.INTEGER);
                    node = modFunc;
                }
            }
            return node;
        }

        // Optimise a modulus expression where both nodes are literal
        // values. Substitute the node with the result of the modulus.
        private ParseNode OptimiseModulus(ParseNode node) {
            if (node is BinaryOpParseNode tokenNode) {
                tokenNode.Left = OptimiseExpressionTree(tokenNode.Left);
                tokenNode.Right = OptimiseExpressionTree(tokenNode.Right);

                if (tokenNode.IsNumber) {
                    NumberParseNode op1 = (NumberParseNode)tokenNode.Left;
                    NumberParseNode op2 = (NumberParseNode)tokenNode.Right;
                    try {
                        double interimValue = op1.Value.DoubleValue - (Math.Floor(op1.Value.DoubleValue / op2.Value.DoubleValue) * op1.Value.DoubleValue);
                        if (double.IsInfinity(interimValue)) {
                            throw new DivideByZeroException();
                        }
                        node = new NumberParseNode((int)interimValue);
                    }
                    catch (DivideByZeroException) {
                        Messages.Error(MessageCode.DIVISIONBYZERO, "Division by zero");
                    }
                }

                // Comal MOD behaves differently to the built-in MOD operator so we need
                // to correct this by calling the IMOD intrinsic instead.
                ExtCallParseNode modFunc = GetIntrinsicExtCallNode("IMOD");
                modFunc.Parameters = new();
                modFunc.Parameters.Add(CastNodeToType(tokenNode.Left, SymType.INTEGER));
                modFunc.Parameters.Add(CastNodeToType(tokenNode.Right, SymType.INTEGER));
                CastNodeToType(modFunc, SymType.INTEGER);
                node = modFunc;
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
                    BinaryOpParseNode divNode = new(ParseID.DIVIDE);
                    divNode.Left = new NumberParseNode(new Variant(1));
                    divNode.Right = tokenNode.Left;
                    divNode.Type = tokenNode.Left.Type;
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
            
            if ((tokenNode.Left.ID == ParseID.STRING) && (tokenNode.Right.ID == ParseID.STRING)) {
                StringParseNode op1 = (StringParseNode)tokenNode.Left;
                StringParseNode op2 = (StringParseNode)tokenNode.Right;
                node = new StringParseNode(op1.Value + op2.Value);
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
                    case ParseID.IDIVIDE:   return OptimiseDivision(node);
                    case ParseID.MOD:       return OptimiseModulus(node);
                    case ParseID.SUB:       return OptimiseSubtraction(node);
                    case ParseID.EXP:       return OptimiseExponentation(node);

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

        // Parse and optimise an expression tree
        private ParseNode Expression() {
            return OptimiseExpressionTree(ParseExpression(0));
        }

        /// OPERATION                   ORDER OF PRECEDENCE
        /// exponentiate                      8
        /// multiply                          7
        /// divide                            7
        /// add                               6
        /// subtract                          6
        /// less than                         5
        /// less than or equal to             5
        /// equal to                          5
        /// not equal to                      5
        /// greater than or equal to          5
        /// greater than                      5
        /// not                               4
        /// and                               3
        /// or                                2
        /// xor                               1
        /// neqv                              1
        /// eqv                               1

        /// Parse an expression.
        private ParseNode ParseExpression(int level) {
            ParseNode op1 = Operand();
            bool done = false;

            while (!done) {
                SimpleToken token = GetNextToken();
                bool isRTL = false;
                ParseID parseID;

                int preced;
                switch (token.ID) {
                    case TokenID.KXOR:      parseID = ParseID.XOR;      preced = 1; break;
                    case TokenID.KBITXOR:   parseID = ParseID.XOR;      preced = 1; break;

                    case TokenID.KOR:       parseID = ParseID.OR;       preced = 2; break;
                    case TokenID.KBITOR:    parseID = ParseID.OR;       preced = 2; break;

                    case TokenID.KAND:      parseID = ParseID.AND;      preced = 3; break;
                    case TokenID.KBITAND:   parseID = ParseID.AND;      preced = 3; break;

                    case TokenID.KGT:       parseID = ParseID.GT;       preced = 5; break;
                    case TokenID.KGE:       parseID = ParseID.GE;       preced = 5; break;
                    case TokenID.KLE:       parseID = ParseID.LE;       preced = 5; break;
                    case TokenID.KNE:       parseID = ParseID.NE;       preced = 5; break;
                    case TokenID.KEQ:       parseID = ParseID.EQ;       preced = 5; break;
                    case TokenID.KLT:       parseID = ParseID.LT;       preced = 5; break;
                    case TokenID.KIN:       parseID = ParseID.INSTR;    preced = 5; break;

                    case TokenID.PLUS:      parseID = ParseID.ADD;      preced = 6; break;
                    case TokenID.MINUS:     parseID = ParseID.SUB;      preced = 6; break;

                    case TokenID.MULTIPLY:  parseID = ParseID.MULT;     preced = 7; break;
                    case TokenID.DIVIDE:    parseID = ParseID.DIVIDE;   preced = 7; break;
                    case TokenID.KDIV:      parseID = ParseID.IDIVIDE;  preced = 7; break;
                    case TokenID.KMOD:      parseID = ParseID.MOD;      preced = 7; break;

                    case TokenID.EXP:       parseID = ParseID.EXP;      preced = 10; isRTL = true; break;

                    default:
                        _currentLine.PushToken(token);
                        done = true;
                        continue;
                }
                if (level >= preced) {
                    _currentLine.PushToken(token);
                    done = true;
                } else {

                    // For operators that evaluate right to left (such as EXP), drop the
                    // precedence so that further occurrences of the same operator to
                    // the right are grouped together.
                    if (isRTL) {
                        --preced;
                    }

                    // Convert INSTR into a call into the INDEX intrinsic
                    if (parseID == ParseID.INSTR) {
                        ParseNode op2 = ParseExpression(preced);
                        if (!op1.IsString || !op2.IsString) {
                            Messages.Error(MessageCode.TYPEMISMATCH, "Type mismatch");
                        }

                        ExtCallParseNode node = GetIntrinsicExtCallNode("INDEX");
                        node.Parameters = new ParametersParseNode();
                        node.Parameters.Add(op1, false);
                        node.Parameters.Add(op2, false);
                        if (op1.Type == SymType.CHAR || op2.Type == SymType.CHAR) {
                            op1.Type = SymType.CHAR;
                            op2.Type = SymType.CHAR;
                        } else {
                            op1.Type = SymType.FIXEDCHAR;
                            op2.Type = SymType.FIXEDCHAR;
                        }
                        node.Type = SymType.INTEGER;
                        op1 = node;
                    } else {
                        op1 = ParseBinaryOpNode(parseID, preced, op1);
                    }
                }
            }
            return op1;
        }

        // Parse a binary operator node
        private ParseNode ParseBinaryOpNode(ParseID parseID, int preced, ParseNode opLeft) {
            return CreateBinaryOpNode(parseID, opLeft, ParseExpression(preced));
        }

        // Create a binary operator node
        private ParseNode CreateBinaryOpNode(ParseID parseID, ParseNode opLeft, ParseNode opRight) {
            if (parseID == ParseID.ADD && opLeft.IsString && opRight.IsString) {
                parseID = ParseID.CONCAT;
            }
            BinaryOpParseNode op = new(parseID);
            op.Left = opLeft;
            op.Right = opRight;
            return TypeEqualise(op);
        }

        // Parse a single operand
        private ParseNode Operand() {
            SimpleToken token = GetNextToken();
            switch (token.ID) {
                case TokenID.LPAREN: {
                    ParseNode node = Expression();
                    ExpectToken(TokenID.RPAREN);
                    return node;
                    }

                case TokenID.KNOT: {
                    UnaryOpParseNode node = new(ParseID.NOT);
                    node.Operand = ParseExpression(4);
                    return CastNodeToType(node, SymType.INTEGER);
                    }

                case TokenID.REAL: {
                    RealToken realToken = (RealToken)token;
                    return new NumberParseNode(new Variant(realToken.Value));
                    }

                case TokenID.KTRUE:
                    return new NumberParseNode(new Variant(1));

                case TokenID.KFALSE:
                    return new NumberParseNode(new Variant(0));

                case TokenID.KPI:
                    return new NumberParseNode(new Variant((float)Math.PI));

                case TokenID.PLUS:
                    return Operand();

                case TokenID.MINUS: {
                    UnaryOpParseNode node = new(ParseID.MINUS);
                    ParseNode op = ParseExpression(9);
                    node.Operand = op;
                    return CastNodeToType(node, op.Type);
                    }

                case TokenID.KERR: {
                    Symbol errSymbol = _globalSymbols.Get(Consts.ErrName);
                    IdentifierParseNode node = new(errSymbol);
                    node.Symbol.IsReferenced = true;
                    return node;
                    }

                case TokenID.KERRTEXT: {
                    Symbol errSymbol = _globalSymbols.Get(Consts.ErrText);
                    IdentifierParseNode node = new(errSymbol);
                    node.Symbol.IsReferenced = true;
                    return node;
                    }

                case TokenID.KZONE: {
                    ExtCallParseNode node = new("JComalLib.PrintManager,jcomallib", "get_Zone");
                    return CastNodeToType(node, SymType.INTEGER);
                    }

                case TokenID.KKEY:
                    return CastNodeToType(GetRuntimeExtCallNode("get_KEY"), SymType.CHAR);

                case TokenID.KCURCOL:
                    return CastNodeToType(GetRuntimeExtCallNode("get_CURCOL"), SymType.INTEGER);

                case TokenID.KCURROW:
                    return CastNodeToType(GetRuntimeExtCallNode("get_CURROW"), SymType.INTEGER);

                case TokenID.KFREEFILE:
                    return CastNodeToType(GetFileManagerExtCallNode("get_FREEFILE"), SymType.INTEGER);

                case TokenID.KEOF: {
                    ExtCallParseNode node = GetFileManagerExtCallNode("EOF");
                    ExpectToken(TokenID.LPAREN);
                    node.Parameters = new ParametersParseNode();
                    node.Parameters.Add(IntegerExpression());
                    ExpectToken(TokenID.RPAREN);
                    return CastNodeToType(node, SymType.INTEGER);
                    }

                case TokenID.KEOD: {
                    Symbol eodSymbol = GetMakeEODSymbol();
                    IdentifierParseNode node = new(eodSymbol);
                    node.Symbol.IsReferenced = true;
                    return node;
                    }

                case TokenID.KESC: {
                    ExtCallParseNode node = GetRuntimeExtCallNode("get_ESC");
                    return CastNodeToType(node, SymType.INTEGER);
                    }

                case TokenID.KRND: {
                    ExtCallParseNode node = GetIntrinsicExtCallNode("RND");
                    token = _currentLine.PeekToken();
                    if (token.ID == TokenID.LPAREN) {
                        ExpectToken(TokenID.LPAREN);
                        node.Parameters = new ParametersParseNode();
                        node.Parameters.Add(IntegerExpression(), true);
                        ExpectToken(TokenID.COMMA);
                        node.Parameters.Add(IntegerExpression(), true);
                        ExpectToken(TokenID.RPAREN);
                    }
                    return CastNodeToType(node, SymType.FLOAT);
                    }

                case TokenID.KSPC:      return KSpc();
                case TokenID.KSTR:      return KStr();
                case TokenID.KGET:      return KGet();
                case TokenID.KTIME:     return KGetTime();

                case TokenID.KLOG:      return InlineDouble("LOG10");
                case TokenID.KSIN:      return InlineDouble("SIN");
                case TokenID.KCOS:      return InlineDouble("COS");
                case TokenID.KTAN:      return InlineDouble("TAN");
                case TokenID.KATN:      return InlineDouble("ATAN");
                case TokenID.KSQR:      return InlineDouble("SQRT");
                case TokenID.KEXP:      return InlineDouble("EXP");
                case TokenID.KINT:      return InlineDouble("FLOOR");

                case TokenID.KSGN:      return InlineFloat("SGN", SymType.INTEGER);
                case TokenID.KABS:      return InlineGeneric("ABS");

                case TokenID.KCHR:      return InlineFixedChar("CHAR");

                case TokenID.KVAL:      return InlineString("VAL", SymType.FLOAT);
                case TokenID.KLEN:      return InlineString("LEN", SymType.INTEGER);
                case TokenID.KASC:      return InlineString("ICHAR", SymType.INTEGER);

                case TokenID.INTEGER: {
                    IntegerToken intToken = (IntegerToken)token;
                    return new NumberParseNode(new Variant((float)intToken.Value));
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

                    // Possible function?
                    Symbol sym = _globalSymbols.Get(identToken.Name);
                    if (sym != null) {
                        return FunctionOperand(identToken.Name, node);
                    }

                    // If we're an array we're done.
                    sym = _localSymbols.Get(identToken.Name);
                    if (sym != null && sym.IsArray) {
                        if (node.Indexes == null || sym.Dimensions.Count != node.Indexes.Count) {
                            Messages.Error(MessageCode.MISSINGARRAYDIMENSIONS, "Incorrect number of array indexes");
                        }
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

                    // Statement functions will have been predefined
                    if (!match && sym != null && sym.Class == SymClass.INLINE) {
                        match = true;
                    }
                    
                    // If there are any arguments, this is a function call
                    if (!match && node.Indexes != null && sym.Scope != SymScope.PARAMETER) {
                        return FunctionOperand(identToken.Name, node);
                    }

                    // Anything else is an identifier.
                    if (sym == null) {
                        sym = GetMakeSymbolForCurrentScope(identToken.Name);
                        if (sym == null) {
                            Messages.Error(MessageCode.UNDEFINEDVARIABLE, $"Undefined identifier {identToken.Name}");
                            return node;
                        }
                    }
                    if (node.HasSubstring && sym.Type != SymType.FIXEDCHAR) {
                        Messages.Error(MessageCode.TYPEMISMATCH, "Character type expected");
                        return node;
                    }
                    sym.IsReferenced = true;
                    node.Symbol = sym;
                    return node;
                }
            }
            Messages.Error(MessageCode.UNRECOGNISEDOPERAND, "Unrecognised operand");
            return new NumberParseNode(new Variant(0));
        }

        // Generate the code to call a function that takes a single string
        // argument and returns the specified type.
        private ParseNode InlineString(string name, SymType returnType = SymType.CHAR) {

            ExtCallParseNode node = GetIntrinsicExtCallNode(name);
            node.Parameters = new ParametersParseNode();
            ExpectToken(TokenID.LPAREN);
            node.Parameters.Add(StringExpression(), false);
            ExpectToken(TokenID.RPAREN);
            return CastNodeToType(node, returnType);
        }

        // Generate the code to call a generic function where the return type
        // is the same as that of the parameter.
        private ParseNode InlineGeneric(string name) {

            ExtCallParseNode node = GetIntrinsicExtCallNode(name);
            node.Type = SymType.DOUBLE;
            node.Parameters = new ParametersParseNode();
            ExpectToken(TokenID.LPAREN);
            ParseNode exprNode = NumericExpression();
            node.Parameters.Add(exprNode, false);
            ExpectToken(TokenID.RPAREN);
            return CastNodeToType(node, exprNode.Type);
        }

        // Generate the code to call a float function
        private ParseNode InlineFloat(string name, SymType returnType = SymType.FLOAT) {

            ExtCallParseNode node = GetIntrinsicExtCallNode(name);
            node.Type = SymType.DOUBLE;
            node.Parameters = new ParametersParseNode();
            ExpectToken(TokenID.LPAREN);
            node.Parameters.Add(NumericExpression(), false);
            ExpectToken(TokenID.RPAREN);
            return CastNodeToType(node, returnType);
        }

        // Generate the code to call a function that takes a single double
        // argument and returns a double.
        private ParseNode InlineDouble(string name) {

            ExtCallParseNode node = GetIntrinsicExtCallNode(name);
            node.Type = SymType.DOUBLE;
            node.Parameters = new ParametersParseNode();
            ExpectToken(TokenID.LPAREN);
            node.Parameters.Add(DoubleExpression(), false);
            ExpectToken(TokenID.RPAREN);
            return CastNodeToType(node, SymType.DOUBLE);
        }

        // Generate the code to call a fixed char function
        private ParseNode InlineFixedChar(string name) {

            ExtCallParseNode node = GetIntrinsicExtCallNode(name);
            node.Parameters = new ParametersParseNode();
            ExpectToken(TokenID.LPAREN);
            node.Parameters.Add(IntegerExpression(), false);
            ExpectToken(TokenID.RPAREN);
            return CastNodeToType(node, SymType.FIXEDCHAR);
        }

        // Parse the GET$ function.
        private ParseNode KGet() {

            ExpectToken(TokenID.LPAREN);
            ParseNode fileParseNode = IntegerExpression();
            ExpectToken(TokenID.COMMA);
            ParseNode countNode = IntegerExpression();
            ExpectToken(TokenID.RPAREN);

            ExtCallParseNode node = GetFileManagerExtCallNode("GET");
            node.Parameters = new();
            node.Parameters.Add(fileParseNode);
            node.Parameters.Add(countNode);
            node.Type = SymType.CHAR;
            return node;
        }

        // Parse the SPC$ function
        private ParseNode KSpc() {

            ExpectToken(TokenID.LPAREN);
            ParseNode countNode = IntegerExpression();
            ExpectToken(TokenID.RPAREN);
            if (countNode.IsNumber) {
                return new StringParseNode(new string(' ', countNode.Value.IntValue));
            }
            ExtCallParseNode node = GetIntrinsicExtCallNode("SPC");
            node.Parameters = new ParametersParseNode();
            node.Parameters.Add(countNode, false);
            node.Type = SymType.CHAR;
            return node;
        }

        // Parse the STR$ function
        private ParseNode KStr() {

            ExtCallParseNode node = GetIntrinsicExtCallNode("STR");
            node.Type = SymType.DOUBLE;
            node.Parameters = new ParametersParseNode();
            ExpectToken(TokenID.LPAREN);
            node.Parameters.Add(NumericExpression(), false);
            ExpectToken(TokenID.RPAREN);
            return CastNodeToType(node, SymType.CHAR);
        }

        // Generate code to retrieve the current value of TIME
        private ParseNode KGetTime() {

            ExtCallParseNode node = GetIntrinsicExtCallNode("get_TIME");
            return CastNodeToType(node, SymType.INTEGER);
        }

        // Parse a function call operand
        private ParseNode FunctionOperand(string name, IdentifierParseNode identNode) {

            CallParseNode node = new();

            // Look for this symbol name in the local table which means its type
            // was predefined.
            Symbol method = _globalSymbols.Get(name);
            if (method == null) {
                Messages.Error(MessageCode.UNDEFINEDFUNCTION, $"Undefined function {name}");
                return node;
            }

            method.IsReferenced = true;

            node.ProcName = new IdentifierParseNode(method);
            node.Parameters = new ParametersParseNode();
            node.Type = method.Type;

            CastNodeToType(node, method.Type);
            if (identNode.Indexes != null) {
                foreach (ParseNode t in identNode.Indexes) {
                    node.Parameters.Add(t, true);
                }
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
                    return NumericEqualise(node);

                case ParseID.LE:
                case ParseID.LT:
                case ParseID.GE:
                case ParseID.GT:
                case ParseID.EQ:
                case ParseID.NE:
                    return CastNodeToType(CompareEqualise(node), SymType.INTEGER);

                case ParseID.OR:
                case ParseID.XOR:
                case ParseID.AND:
                case ParseID.MOD:
                case ParseID.IDIVIDE:
                    return IntegerEqualise(node);

                case ParseID.CONCAT:
                    return StringEqualise(node);

                case ParseID.INSTR:
                    return CastNodeToType(StringEqualise(node), SymType.INTEGER);
            }
            Debug.Assert(false, $"Unhandled expression node ID {node.ID}");
            return null;
        }

        // Cast the specified node to the given type. This will result in the code
        // generator adding the necessary type casting instructions if needed.
        private ParseNode CastNodeToType(ParseNode node, SymType typeNeeded) {

            // For literal values, promote them to the required type by
            // doing variant conversion.
            if (node is NumberParseNode) {
                switch (typeNeeded) {
                    case SymType.INTEGER:
                        node.Value = new Variant(node.Value.IntValue);
                        break;

                    case SymType.FLOAT:
                        node.Value = new Variant(node.Value.RealValue);
                        break;

                    case SymType.DOUBLE:
                        node.Value = new Variant((double)node.Value.RealValue);
                        break;
                }
            }
            node.Type = typeNeeded;
            return node;
        }

        // Cast the specified node to the given type. This will result in the code
        // generator adding the necessary type casting instructions if needed.
        private BinaryOpParseNode CastNodeToType(BinaryOpParseNode node, SymType typeNeeded) {
            node.Type = typeNeeded;
            return node;
        }

        // Verify that both operands of a string operator are strings. This
        // is the only type that is permitted.
        private BinaryOpParseNode StringEqualise(BinaryOpParseNode node) {

            SymType type1 = node.Left.Type;
            SymType type2 = node.Right.Type;
            if (type1 == SymType.CHAR && type2 == SymType.CHAR) {
                return CastNodeToType(node, SymType.CHAR);
            }
            if (type1 == SymType.FIXEDCHAR || type2 == SymType.FIXEDCHAR) {
                if (Symbol.IsCharType(type2)) {
                    return CastNodeToType(node, SymType.FIXEDCHAR);
                }
            }
            Messages.Error(MessageCode.TYPEMISMATCH, "Type mismatch");
            return node;
        }

        // Do type equalisation for integer expressions. In this instance,
        // the type of the result is cast to the largest type that can
        // accommodate the two operands. Anything that evaluates to a non-
        // arithmetic value yields a type mismatch.
        private BinaryOpParseNode IntegerEqualise(BinaryOpParseNode node) {

            SymType type1 = node.Left.Type;
            SymType type2 = node.Right.Type;

            if (type1 == SymType.INTEGER) {
                switch (type2) {
                    case SymType.FLOAT:
                    case SymType.DOUBLE:
                    case SymType.INTEGER:
                        return CastNodeToType(node, SymType.INTEGER);
                }
            }
            if (type1 == SymType.FLOAT) {
                switch (type2) {
                    case SymType.FLOAT:
                    case SymType.DOUBLE:
                    case SymType.INTEGER:
                        return CastNodeToType(node, SymType.INTEGER);
                }
            }
            Messages.Error(MessageCode.TYPEMISMATCH, "Type mismatch");
            return node;
        }

        // Do type equalisation for arithmetic expressions. In this instance,
        // the type of the result is cast to the largest type that can
        // accommodate the two operands. Anything that evaluates to a non-
        // arithmetic value yields a type mismatch.
        private BinaryOpParseNode NumericEqualise(BinaryOpParseNode node) {

            SymType type1 = node.Left.Type;
            SymType type2 = node.Right.Type;

            if (type1 == SymType.INTEGER) {
                switch (type2) {
                    case SymType.DOUBLE:
                        return CastNodeToType(node, SymType.DOUBLE);

                    case SymType.FLOAT:
                        return CastNodeToType(node, SymType.FLOAT);

                    case SymType.INTEGER:
                        return CastNodeToType(node, SymType.INTEGER);
                }
            }
            if (type1 == SymType.FLOAT) {
                switch (type2) {
                    case SymType.DOUBLE:
                        return CastNodeToType(node, SymType.DOUBLE);

                    case SymType.FLOAT:
                    case SymType.INTEGER:
                        return CastNodeToType(node, SymType.FLOAT);
                }
            }
            if (type1 == SymType.DOUBLE) {
                switch (type2) {
                    case SymType.DOUBLE:
                    case SymType.FLOAT:
                    case SymType.INTEGER:
                        return CastNodeToType(node, SymType.DOUBLE);
                }
            }
            Messages.Error(MessageCode.TYPEMISMATCH, "Type mismatch");
            return node;
        }

        // Do type equalisation for comparision expressions. In this instance,
        // the type of the result is cast to the largest type that can
        // accommodate the two operands. Anything that evaluates to a non-
        // arithmetic value yields a type mismatch.
        private BinaryOpParseNode CompareEqualise(BinaryOpParseNode node) {

            SymType type1 = node.Left.Type;
            SymType type2 = node.Right.Type;

            if (type1 == SymType.INTEGER) {
                switch (type2) {
                    case SymType.DOUBLE:
                    case SymType.FLOAT:
                    case SymType.INTEGER:
                        return CastNodeToType(node, type2);
                }
            }
            if (type1 == SymType.FLOAT) {
                switch (type2) {
                    case SymType.DOUBLE:
                    case SymType.FLOAT:
                    case SymType.INTEGER:
                        return CastNodeToType(node, SymType.FLOAT);
                }
            }
            if (type1 == SymType.DOUBLE) {
                switch (type2) {
                    case SymType.DOUBLE:
                    case SymType.FLOAT:
                    case SymType.INTEGER:
                        return CastNodeToType(node, SymType.DOUBLE);
                }
            }
            if (type1 == SymType.CHAR) {
                switch (type2) {
                    case SymType.FIXEDCHAR:
                    case SymType.CHAR:
                        return CastNodeToType(node, type2);
                }
            }
            if (type1 == SymType.FIXEDCHAR) {
                switch (type2) {
                    case SymType.FIXEDCHAR:
                    case SymType.CHAR:
                        return CastNodeToType(node, SymType.FIXEDCHAR);
                }
            }
            Messages.Error(MessageCode.TYPEMISMATCH, "Type mismatch");
            return node;
        }
    }
}