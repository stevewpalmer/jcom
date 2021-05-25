// JComal
// Keyword handling
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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using CCompiler;
using JComLib;

namespace JComal {

    /// <summary>
    /// Main compiler class.
    /// </summary>
    public partial class Compiler {

        // MODULE
        //
        // Syntax: MODULE <string constant>
        //
        // Specifies the name of this module. By default, the module name is
        // taken from the source filename so this provides a way to specify
        // an alternative module name.
        //
        private ParseNode KModule() {

            SimpleToken token = GetNextToken();
            if (token is not IdentifierToken identToken) {
                Messages.Error(MessageCode.MODULENAMEEXPECTED,
                               "Module name expected");
            } else {
                _program.Name = identToken.Name;
                ExpectEndOfLine();
            }
            CompileBlock(_program.Root, new[] { TokenID.KENDMODULE });
            return null;
        }

        // EXPORT
        //
        // Syntax: EXPORT <string constant>
        //
        // Specifies that the procedure or function should be exported and
        // made available to external programs.
        //
        private ParseNode KExport() {

            SimpleToken token = GetNextToken();
            if (token is not IdentifierToken identToken) {
                Messages.Error(MessageCode.PROCFUNCNAMEEXPECTED,
                               "Procedure or function name expected");
            } else {
                string methodName = identToken.Name;
                Symbol sym = Globals.Get(methodName);
                if (sym != null && sym.IsExported) {
                    Messages.Error(MessageCode.ALREADYEXPORTED,
                                   $"{methodName} already exported");
                }
                if (sym == null) {
                    sym = Globals.Add(methodName,
                                             new SymFullType(),
                                             SymClass.SUBROUTINE,
                                             null,
                                             _currentLineNumber);
                    sym.Modifier |= SymModifier.EXPORTED;
                }
                ExpectEndOfLine();
            }
            return null;
        }

        // DATA
        //
        // Syntax: DATA <constant values>*
        //
        // Adds data to the internal _DATA symbol. This causes a static array
        // of variants to be created when the program starts. We also add the
        // _DATAINDEX index if one is not present so that the READ statement
        // has an index to maintain. Multiple DATA statements are allowed and
        // each one adds to the same array. Local DATA statements aren't
        // supported.
        //
        private ParseNode KData() {
            SimpleToken token;

            List<Variant> valueList = new();

            Symbol dataSymbol = GetMakeReadDataSymbol();
            if (dataSymbol.ArrayValues != null) {
                valueList.AddRange(dataSymbol.ArrayValues);
            }

            Variant valueNode;
            do {
                valueNode = ParseConstant();
                if (valueNode.Type == VariantType.INTEGER) {
                    valueNode = new Variant(valueNode.RealValue);
                }
                token = GetNextToken();
                valueList.Add(valueNode);
            } while (token.ID == TokenID.COMMA);
            _currentLine.PushToken(token);

            dataSymbol.ArrayValues = valueList.ToArray();
            dataSymbol.Dimensions = new Collection<SymDimension>() {
                new() {
                    LowerBound = new NumberParseNode(0),
                    UpperBound = new NumberParseNode(valueList.Count - 1)
                }
            };

            // Ensure _EOD is initialised to 0 since we have one DATA
            // statement
            Symbol eodSymbol = GetMakeEODSymbol();
            Debug.Assert(eodSymbol != null);
            eodSymbol.Value = new Variant(0);

            // Make sure we have a _DATAINDEX
            GetMakeReadDataIndexSymbol();
            return null;
        }

        // READ
        //
        // Syntax: READ var
        //
        // Reads a value from the _DATA array into the specified variables.
        //
        private ParseNode KRead() {
            ReadDataParseNode node = new();

            List<IdentifierParseNode> identifiers = new();

            // Discrimate READ data and READ FILE based on the first
            // tokens. READ FILE or READ <int> will definitely be a
            // READFILE statement.
            SimpleToken token = _currentLine.PeekToken();
            if (token.ID == TokenID.KFILE) {
                return KReadFile();
            }
            if (token.ID == TokenID.INTEGER) {
                return KReadFile();
            }

            IdentifierToken identToken = ParseIdentifier();

            while (true) {
                IdentifierParseNode identNode = ParseIdentifierFromToken(identToken);
                if (identNode == null) {
                    break;
                }
                identifiers.Add(identNode);
                token = GetNextToken();
                if (token.ID == TokenID.COLON && identifiers.Count == 1) {

                    // OK, we got READ FILE <var>: where a variable was
                    // used to specify the file number. So this is definitely
                    // a READFILE statement so we have to go all the way back
                    // to the start of the line, get and throw away the line
                    // number and start again in KReadFile.
                    _currentLine.Reset();
                    token = GetNextToken();
                    if (token.ID != TokenID.INTEGER) {
                        _currentLine.PushToken(token);
                    }
                    return KReadFile();
                }
                if (token.ID != TokenID.COMMA) {
                    _currentLine.PushToken(token);
                    break;
                }
                identToken = ParseIdentifier();
            }
            node.Identifiers = identifiers.ToArray();
            node.DataIndex = GetMakeReadDataIndexSymbol();
            node.DataArray = GetMakeReadDataSymbol();
            node.EndOfData = GetMakeEODSymbol();
            return node;
        }

        // RESTORE
        //
        // Syntax: RESTORE
        //
        // Resets the data index back 0. Labels are not supported but, if they are, we can
        // use them to specific a particular index at which RESTORE should reset to.
        //
        private ParseNode KRestore() {

            AssignmentParseNode node = new();
            node.Identifiers = new[] {
                new IdentifierParseNode(GetMakeReadDataIndexSymbol()),
                new IdentifierParseNode(GetMakeEODSymbol()),
            };
            node.ValueExpressions = new[] {
                new NumberParseNode(0),
                new NumberParseNode(0)
            };
            return node;
        }

        // PROC
        //
        // Syntax: PROC <procedure_identifier> <head_appendix> <eol>
        //    <procedure_block>
        // ENDPROC <procedure_identifier> <eol>
        //
        private ParseNode KProc() {
            return ParseProcFuncDefinition(SymClass.SUBROUTINE, TokenID.KENDPROC, null);
        }

        // FUNC
        //
        // FUNC <function_identifier> <head_appendix> <eol>
        //    <function_block>
        // ENDFUNC <function_identifier> <eol>
        //
        private ParseNode KFunc() {
            return ParseProcFuncDefinition(SymClass.FUNCTION, TokenID.KENDFUNC, null);
        }

        // RETURN
        //
        // RETURN [<expression>]
        //
        // Assigns the value specified after the RETURN to the function and returns control to the calling
        // statement. RETURN may also be used to terminate a procedure early. An expression is not allowed
        // for RETURN within a PROC, but is mandatory for RETURN within a function.
        //
        private ParseNode KReturn() {

            ReturnParseNode node = new();
            if (!_currentLine.IsAtEndOfLine && CurrentProcedure.ProcedureSymbol.Class == SymClass.SUBROUTINE) {
                Messages.Error(MessageCode.ILLEGALRETURN, "Cannot RETURN a value");
                return null;
            }
            if (_currentLine.IsAtEndOfLine && CurrentProcedure.ProcedureSymbol.Class == SymClass.FUNCTION) {
                Messages.Error(MessageCode.ILLEGALRETURN, "Must RETURN a value");
                return null;
            }
            if (!_currentLine.IsAtEndOfLine) {

                Symbol thisSymbol = CurrentProcedure.ProcedureSymbol;
                ParseNode exprNode = Expression();
                if (exprNode != null) {
                    if (!ValidateAssignmentTypes(thisSymbol.Type, exprNode.Type)) {
                        Messages.Error(MessageCode.TYPEMISMATCH, "Type mismatch in RETURN");
                    }
                }
                node.ReturnExpression = exprNode;
            }
            _hasReturn = true;
            return node;
        }

        // IMPORT
        //
        // IMPORT <procedurename>
        //
        // Makes the specified procedure visible to the current closed procedure so that
        // it can be called.
        //
        private ParseNode KImport() {

            if (_importSymbols == null) {
                Messages.Error(MessageCode.NOTINCLOSED, "IMPORT can only be used in a CLOSED procedure or function");
            } else {
                do {
                    IdentifierDefinition identToken = ParseIdentifierDefinition();
                    if (identToken == null) {
                        break;
                    }
                    Symbol sym = Globals.Get(identToken.Name);
                    if (sym == null) {
                        Messages.Error(MessageCode.METHODNOTFOUND, $"{identToken.Name} not found");
                    }
                    else if (_importSymbols.Get(identToken.Name) != null) {
                        Messages.Error(MessageCode.ALREADYIMPORTED, $"{identToken.Name} is already IMPORTed");
                    }
                    else if (sym != null) {
                        _importSymbols.Add(sym);
                    }
                    if (_currentLine.IsAtEndOfLine) {
                        break;
                    }
                    ExpectToken(TokenID.COMMA);
                } while (true);
            }
            SkipToEndOfLine();
            return null;
        }

        // LET
        //
        // Syntax: [LET] identifier=<expression>
        //
        // Handles assignments.
        //
        private ParseNode KAssignment() {

            IdentifierToken identToken = ParseIdentifier();
            if (identToken == null) {
                return null;
            }
            Symbol sym = GetSymbolForCurrentScope(identToken.Name);

            // Possible label?
            SimpleToken token = GetNextToken();
            if (token.ID == TokenID.COLON) {
                sym = GetMakeLabel(identToken.Name, true);
                return new MarkLabelParseNode { Label = sym };
            }

            // Possible procedure call? This is where we need to resolve A(X) between a
            // substring assignment, an array assignment or a method call. If A is defined
            // and is not a method, it can't be a procedure call.
            if (sym == null || !sym.Defined || (sym.Defined && sym.IsMethod)) {
                if (token.ID == TokenID.EOL || token.ID == TokenID.LPAREN) {
                    _currentLine.PushToken(token);
                    return ExecWithIdentifier(identToken);
                }
            }

            // Must be an assignment statement then.
            _currentLine.PushToken(token);
            List<IdentifierParseNode> identifiers = new();
            List<ParseNode> values = new();
            while (true) {
                IdentifierParseNode identNode = ParseIdentifierFromToken(identToken);
                if (identNode == null) {
                    SkipToEndOfLine();
                    break;
                }
                identifiers.Add(identNode);

                // If we're not strict, allow = for := but replace it if found
                if (!_opts.Strict) {
                    ReplaceCurrentToken(TokenID.KEQ, TokenID.KASSIGN);
                }

                token = GetNextToken();
                ParseNode exprNode;
                switch (token.ID) {
                    case TokenID.KINCADD:
                        exprNode = OptimiseExpressionTree(CreateBinaryOpNode(ParseID.ADD, identNode, Expression()));
                        break;

                    case TokenID.KINCSUB:
                        exprNode = OptimiseExpressionTree(CreateBinaryOpNode(ParseID.SUB, identNode, Expression()));
                        break;

                    default:
                        _currentLine.PushToken(token);
                        ExpectToken(TokenID.KASSIGN);
                        exprNode = Expression();
                        break;
                }
                if (exprNode != null) {
                    bool valid = ValidateAssignment(identNode, exprNode);
                    if (!valid) {
                        Messages.Error(MessageCode.TYPEMISMATCH, "Type mismatch in assignment");
                    }
                    values.Add(exprNode);
                }
                token = GetNextToken();
                if (token.ID != TokenID.SEMICOLON) {
                    _currentLine.PushToken(token);
                    break;
                }
                identToken = ParseIdentifier();
            }

            return new AssignmentParseNode() {
                Identifiers = identifiers.ToArray(),
                ValueExpressions = values.ToArray()
            };
        }

        // DIM
        //
        // Syntax: DIM
        //
        // Used to apply a dimension to a prior declared variable. Can be used to
        // declare a variable assuming implicit type is permitted for the name.
        //
        private ParseNode KDim() {
            SymFullType fullType = new();
            SimpleToken token;

            do {
                Symbol sym = ParseIdentifierDeclaration(fullType);
                token = GetNextToken();
                if (token.ID == TokenID.KOF) {
                    ParseNode intVal = IntegerExpression();
                    if (sym.Type != SymType.FIXEDCHAR) {
                        Messages.Error(MessageCode.INVALIDOF, "OF can only be used with strings");
                    }
                    if (!intVal.IsConstant) {
                        Messages.Error(MessageCode.CONSTANTEXPECTED, "Number expected");
                    }
                    sym.FullType.Width = intVal.Value.IntValue;
                    token = GetNextToken();
                }
            } while (token.ID == TokenID.COMMA);
            _currentLine.PushToken(token);
            ExpectEndOfLine();
            return null;
        }

        // CASE
        //
        // Syntax: CASE <expression> OF...WHEN <expression>
        //
        // Begins a CASE structure, allowing a multiple choice decision with as many specific WHEN sections as
        // needed. A default OTHERWISE section may be included that is executed if none of -the WHEN sections match
        // Begins a CASE structure, allowing a multiple choice decision with as many specific WHEN sections as needed.
        // A default OTHERWISE section may be included that is executed if none of the WHEN sections match the
        // condition (which can be either string or numeric). Statement blocks following each WHEN are indented
        // when listed, but the CASE, WHEN and OTHERWISE statements are not}. The system will insert the word OF
        // for you if you don't type it.
        //
        private ParseNode KCase() {

            ConditionalParseNode parseNode = new();
            ParseNode exprNode = Expression();

            InsertTokenIfMissing(TokenID.KOF);
            ExpectEndOfLine();

            SimpleToken token;
            while (!_ls.EndOfFile) {

                _currentLine = _ls.NextLine;
                token = GetNextToken();
                if (token.ID != TokenID.EOL) {

                    // Possible initial line number
                    if (token.ID == TokenID.INTEGER) {
                        IntegerToken lineNumberToken = token as IntegerToken;
                        _currentLineNumber = lineNumberToken.Value;
                        Messages.Linenumber = _currentLineNumber;
                        token = GetNextToken();
                    }
                }

                // If this WHEN or OTHERWISE? If it is WHEN, we create a conditional that compares
                // the expression of the WHEN statement with the original CASE expression.
                ParseNode conditionalNode = null;
                if (token.ID == TokenID.KWHEN) {

                    conditionalNode = ParseBinaryOpNode(ParseID.EQ, 0, exprNode);
                    while (TestToken(TokenID.COMMA)) {
                        ParseNode nextConditional = ParseBinaryOpNode(ParseID.EQ, 0, exprNode);
                        conditionalNode = CreateBinaryOpNode(ParseID.OR, conditionalNode, nextConditional);
                    }
                } else if (token.ID != TokenID.KOTHERWISE) {
                    Messages.Error(MessageCode.MISSINGENDSTATEMENT, "WHEN or OTHERWISE expected");
                }
                ExpectEndOfLine();

                // Parse the body of a WHEN or OTHERWISE statement.
                BlockParseNode block = new();
                TokenID endTokenID = CompileBlock(block, new[] {
                    TokenID.KENDCASE,
                    TokenID.KOTHERWISE,
                    TokenID.KWHEN });
                parseNode.Add(conditionalNode, block);

                // End of statement?
                if (endTokenID == TokenID.KENDCASE) {
                    break;
                }
                _ls.BackLine();
            }

            ExpectEndOfLine();
            return parseNode;
        }

        // EXEC
        //
        // Syntax: [EXEC] <identifier> [<parameters>] [CLOSED]
        //
        // Call a subroutine with specified parameters
        //
        private ParseNode KExec() {

            IdentifierToken identToken = ParseIdentifier();
            if (identToken == null) {
                SkipToEndOfLine();
                return null;
            }
            return ExecWithIdentifier(identToken);
        }

        // ZONE
        //
        // Syntax: ZONE <numeric expression>
        //
        // Sets the PRINT zone
        //
        private ParseNode KZone() {
            ExtCallParseNode node = GetFileManagerExtCallNode("set_Zone");
            node.Parameters = new();
            node.Parameters.Add(IntegerExpression(), false);
            return node;
        }

        // RANDOMIZE
        //
        // Syntax: RANDOMIZE [<numeric expression>]
        //
        // Instantiates the random number generator with a new seed.
        //
        private ParseNode KRandomize() {

            ExtCallParseNode node = GetIntrinsicExtCallNode("RANDOMIZE");
            if (!_currentLine.IsAtEndOfStatement) {
                node.Parameters = new();
                node.Parameters.Add(IntegerExpression(), false);
            }
            return node;
        }

        // IF keyword
        //
        // Syntax: IF <expression> THEN statements ELSE statements
        //
        // Conditional statement evaluation.
        //
        private ParseNode KIf() {
            ConditionalParseNode node = new();

            ParseNode expr = Expression();
            InsertTokenIfMissing(TokenID.KTHEN);

            BlockParseNode statements;
            if (!_currentLine.IsAtEndOfLine) {

                // Single line IF
                SimpleToken token = GetNextToken();
                statements = new();
                CompileLine(token, statements);
                node.Add(expr, statements);
            } else {
                TokenID endToken;
                do {
                    statements = new();
                    endToken = CompileBlock(statements, new[] { TokenID.KENDIF, TokenID.KELIF, TokenID.KELSE });
                    node.Add(expr, statements);
                    if (endToken == TokenID.KELIF) {

                        expr = Expression();
                        InsertTokenIfMissing(TokenID.KTHEN);
                        ExpectEndOfLine();
                    } else if (endToken == TokenID.KELSE) {

                        // We mark the end of the sequence of IF blocks with
                        // a null expression.
                        expr = null;
                        ExpectEndOfLine();
                    }
                } while (endToken == TokenID.KELIF || endToken == TokenID.KELSE);
            }
            return node;
        }

        // TRAP
        //
        // Syntax: TRAP <statements> HANDLER <statements> ENDTRAP
        //         TRAP ESC +|-
        //
        // Sets up a trap handler or controls the behaviour of the
        // ESC key.
        //
        private ParseNode KTrap() {

            // Is this a TRAP ESC?
            if (TestToken(TokenID.KESC)) {
                SimpleToken token = GetNextToken();
                bool escFlag = false;
                switch (token.ID) {
                    case TokenID.PLUS:  escFlag = true; break;
                    case TokenID.MINUS: escFlag = false; break;
                    default:
                        Messages.Error(MessageCode.UNEXPECTEDTOKEN, "+ or - expected after TRAP ESC");
                        break;
                }
                ExtCallParseNode node = GetRuntimeExtCallNode("SETESCAPE");
                node.Parameters = new ParametersParseNode();
                node.Parameters.Add(new NumberParseNode(new Variant(escFlag)));
                return node;
            }

            TrappableParseNode parseNode = new() {
                Body = new BlockParseNode(),
                Handler = new BlockParseNode(),
                Err = Globals.Get(Consts.ErrName),
                Message = Globals.Get(Consts.ErrText)
            };
            CompileBlock(parseNode.Body, new[] { TokenID.KENDTRAP, TokenID.KHANDLER });
            CompileBlock(parseNode.Handler, new[] { TokenID.KENDTRAP });
            return parseNode;
        }

        // FOR
        //
        // Syntax: FOR <numeric identifier>=<numeric expression> TO <numeric expression> [STEP <step value>] DO
        //
        // Loop
        //
        private ParseNode KFor() {

            // Need a new local symbol table as Comal FOR keywords are
            // local to the FOR block
            SymbolCollection forSymbols = new("_LOCAL");
            SymbolStack.Push(forSymbols);
            CurrentProcedure.LocalSymbols.Add(forSymbols);

            // Control identifier
            IdentifierToken identToken = ParseIdentifier();
            if (identToken == null) {
                SkipToEndOfLine();
                return null;
            }

            LoopParseNode node = new();

            Symbol symLoop = MakeSymbolForScope(identToken.Name, forSymbols);
            symLoop.IsReferenced = true;
            node.LoopVariable = new IdentifierParseNode(symLoop);

            // If we're not strict, allow = but replace with := if found
            if (!_opts.Strict) {
                ReplaceCurrentToken(TokenID.KEQ, TokenID.KASSIGN);
            }
            ExpectToken(TokenID.KASSIGN);

            // Loop starting value
            node.StartExpression = Expression();

            // TO keyword
            ExpectToken(TokenID.KTO);

            // Loop ending value
            node.EndExpression = Expression();

            // Optional increment?
            SimpleToken token = GetNextToken();
            if (token.ID == TokenID.KSTEP) {
                node.StepExpression = Expression();
            } else {
                _currentLine.PushToken(token);
            }

            // DO keyword
            InsertTokenIfMissing(TokenID.KDO);

            node.LoopBody = new();
            if (!_currentLine.IsAtEndOfLine) {

                // Short format
                token = GetNextToken();
                ParseNode statement = Statement(token);
                if (statement != null) {
                    node.LoopBody.Add(statement);
                    ExpectEndOfLine();
                }
            } else {
                // Long format
                CompileBlock(node.LoopBody, new[] { TokenID.KNEXT });

                // Check identifier matches
                token = GetNextToken();
                CheckEndOfBlockName(identToken, token);
            }

            // Warn if the loop will be skipped
            if (node.IterationCount() == 0) {
                Messages.Warning(MessageCode.LOOPSKIPPED, 2, "Loop will be skipped because iteration count is zero");
            }

            SymbolStack.Pop();
            return node;
        }

        // REPEAT
        //
        // Syntax: REPEAT body UNTIL <integer expression>
        //
        // Repeats a loop until a condition is satisified at the
        // end of the loop.
        private ParseNode KRepeat() {

            LoopParseNode node = new();
            node.LoopBody = new();

            if (!_currentLine.IsAtEndOfLine) {

                SimpleToken token = GetNextToken();
                ParseNode subnode = Statement(token);
                if (subnode != null) {
                    node.LoopBody.Add(subnode);
                }
                ExpectToken(TokenID.KUNTIL);
            } else {
                CompileBlock(node.LoopBody, new[] { TokenID.KUNTIL });
            }
            node.EndExpression = IntegerExpression();
            return node;
        }

        // WHILE
        //
        // Syntax: WHILE <condition> DO <body> ENDWHILE
        //         WHILE <condition> DO <statement>
        //
        // While a condition is satisified, repeat the loop. Both
        // multi-line and single line versions are supported.
        //
        private ParseNode KWhile() {

            LoopParseNode node = new();
            node.StartExpression = IntegerExpression();

            InsertTokenIfMissing(TokenID.KDO);

            node.LoopBody = new();
            if (!_currentLine.IsAtEndOfLine) {

                SimpleToken token = GetNextToken();
                ParseNode subnode = Statement(token);
                if (subnode != null) {
                    node.LoopBody.Add(subnode);
                }
            } else {
                CompileBlock(node.LoopBody, new[] { TokenID.KENDWHILE });
            }
            return node;
        }

        // LOOP
        //
        // Syntax: LOOP body ENDLOOP
        //
        // Creates a loop construct that requires an EXIT statement
        // in the body to exit
        //
        private ParseNode KLoop() {

            ExpectEndOfLine();

            LoopParseNode node = new();
            LoopParseNode previousLoop = _currentLoop;
            _currentLoop = node;
            node.LoopBody = new();
            CompileBlock(node.LoopBody, new[] { TokenID.KENDLOOP });

            node.StartExpression = new NumberParseNode(new Variant(1));

            _currentLoop = previousLoop;
            return node;
        }

        // EXIT
        //
        // Syntax: EXIT [WHEN <condition>]
        //
        // Exits from a LOOP statement. If a condition is specified then
        // the EXIT occurs if the condition is satisified.
        //
        private ParseNode KExit() {

            if (_currentLoop == null) {
                Messages.Error(MessageCode.BADEXIT, "Cannot EXIT without a LOOP statement");
                _currentLine.SkipToEndOfLine();
                return null;
            }

            // This is just a very simple break statement.
            BreakParseNode parseNode = new();
            parseNode.ScopeParseNode = _currentLoop;
            if (!_currentLine.IsAtEndOfStatement) {
                ExpectToken(TokenID.KWHEN);
                parseNode.BreakExpression = IntegerExpression();
            }
            return parseNode;
        }

        // LABEL
        //
        // Syntax: [LABEL] identifier:
        //
        // Assigns a label name to the line. This label is only referenced by GOTO
        // It is non-executable and may be placed anywhere within a program
        // as a one line statement.
        //
        private ParseNode KLabel() {

            IdentifierToken identToken = ParseIdentifier();
            if (identToken == null) {
                SkipToEndOfLine();
                return null;
            }
            ExpectToken(TokenID.COLON);
            Symbol sym = GetMakeLabel(identToken.Name, true);
            return new MarkLabelParseNode { Label = sym };
        }

        // GOTO
        //
        // Syntax: GOTO <label name>
        //
        // Statement - Transfers program execution to the line with the
        // specified label name.
        //
        private ParseNode KGoto() {
            return new GotoParseNode(ParseLabel());
        }

        // COLOUR
        //
        // Syntax: COLOUR <colcode>
        //
        // Change the text background and foreground colour.
        //
        private ParseNode KColour() {
            ExtCallParseNode node = GetRuntimeExtCallNode("COLOUR");
            node.Parameters = new ParametersParseNode();
            node.Parameters.Add(IntegerExpression());
            return node;
        }

        // CURSOR
        //
        // Syntax: CURSOR <row>,<column>
        //
        // Move the cursor on the screen to the specified row and column.
        //
        private ParseNode KCursor() {
            ExtCallParseNode node = GetRuntimeExtCallNode("CURSOR");
            node.Parameters = new ParametersParseNode();
            node.Parameters.Add(IntegerExpression());
            ExpectToken(TokenID.COMMA);
            node.Parameters.Add(IntegerExpression());
            return node;
        }

        // REPORT
        //
        // Syntax: REPORT [<errnum>]
        //
        // REPORTs an error where <errnum> is an optional error number taken
        // from MessageCode. If REPORT is used within a TRAP handler then the
        // error is caught and _ERR and _ERRTEXT will be assigned the
        // specified errnum and it's associated message.
        //
        private ParseNode KReport() {

            ExtCallParseNode node = GetRuntimeExtCallNode("REPORT");
            node.Inline = true;
            node.Parameters = new ParametersParseNode();
            if (!_currentLine.IsAtEndOfStatement) {
                node.Parameters.Add(IntegerExpression());
            } else {
                node.Parameters.Add(new NumberParseNode(0));
            }
            return node;
        }

        // STOP
        //
        // Syntax: STOP [<message>]
        //
        // Terminates program execution with an optional message.
        //
        private ParseNode KStop() {

            ExtCallParseNode node = GetRuntimeExtCallNode("STOP");
            node.Parameters = new ParametersParseNode();
            if (!_currentLine.IsAtEndOfStatement) {
                node.Parameters.Add(CastNodeToType(StringExpression(), SymType.CHAR));
            } else {
                node.Parameters.Add(new StringParseNode(""));
            }
            node.Parameters.Add(new NumberParseNode(new Variant(_currentLineNumber)));
            return node;
        }

        // END
        //
        // Syntax: END [<message>]
        //
        // ENDs the current program with an optional message.
        //
        private ParseNode KEnd() {

            ExtCallParseNode node = GetRuntimeExtCallNode("END");
            node.Parameters = new ParametersParseNode();
            if (!_currentLine.IsAtEndOfStatement) {
                node.Parameters.Add(CastNodeToType(StringExpression(), SymType.CHAR));
            } else {
                node.Parameters.Add(new StringParseNode(""));
            }
            node.Parameters.Add(new NumberParseNode(new Variant(_currentLineNumber)));
            return node;
        }

        // PAGE
        //
        // Syntax: PAGE
        //
        // Clears the screen and puts the cursor at the top left corner (1,1). If output is to another device,
        // a CHR$(12) is sent (form feed).
        //
        private ParseNode KPage() {
            return GetRuntimeExtCallNode("CLS");
        }

        // DIR
        //
        // Syntax: DIR [<string>]
        //
        // Displays a catalog of the current directory with the optional wildcard
        // specification.
        //
        private ParseNode KDir() {

            ExtCallParseNode node = GetRuntimeExtCallNode("CATALOG");
            node.Parameters = new ParametersParseNode();
            if (!_currentLine.IsAtEndOfStatement) {
                node.Parameters.Add(CastNodeToType(StringExpression(), SymType.CHAR));
            } else {
                node.Parameters.Add(new StringParseNode("*"));
            }
            return node;
        }
    }
}
