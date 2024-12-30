// JComal
// File keyword handling
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

using CCompiler;
using JComLib;

namespace JComal;

/// <summary>
/// Extension of the Compiler class to handle file keywords.
/// </summary>
public partial class Compiler {

    // OPEN
    //
    // Syntax: OPEN [FILE] <file>,<filename>,<mode>
    //
    // Opens a data file for reading or writing. The first integer expression
    // specifies the file handle.
    //
    private ExtCallParseNode KOpen() {

        // Optional FILE keyword
        TestToken(TokenID.KFILE);

        ExtCallParseNode node = GetFileManagerExtCallNode("OPEN");
        ParametersParseNode paramsNode = new();
        paramsNode.Add(IntegerExpression());
        ExpectToken(TokenID.COMMA);
        paramsNode.Add(CastNodeToType(StringExpression(), SymType.CHAR));
        ExpectToken(TokenID.COMMA);

        string mode = GetNextToken().ID switch {
            TokenID.KREAD => "r",
            TokenID.KWRITE => "w",
            TokenID.KAPPEND => "w+",
            TokenID.KRANDOM => "x",
            _ => "r"
        };
        paramsNode.Add(new StringParseNode(mode));

        // Random requires a record size
        ParseNode recordSizeParseNode = new NumberParseNode(0);
        if (mode == "x") {
            recordSizeParseNode = IntegerExpression();
        }
        paramsNode.Add(recordSizeParseNode);

        node.Parameters = paramsNode;
        return node;
    }

    // CREATE
    //
    // Syntax: CREATE <filename>,<record_count>,<record_size>
    //
    // Creates a random access file with the specified number of records,
    // each record of the given size.
    //
    private ExtCallParseNode KCreate() {

        ExtCallParseNode node = GetFileManagerExtCallNode("CREATE");
        ParametersParseNode paramsNode = new();
        paramsNode.Add(CastNodeToType(StringExpression(), SymType.CHAR));
        ExpectToken(TokenID.COMMA);
        paramsNode.Add(IntegerExpression());
        ExpectToken(TokenID.COMMA);
        paramsNode.Add(IntegerExpression());
        node.Parameters = paramsNode;
        return node;
    }

    // CLOSE
    //
    // Syntax: CLOSE [[FILE] <file>]
    //
    // Close the data file specified by the given integer expression, or
    // close all opened files if no integer expression is specified.
    //
    private ExtCallParseNode KClose() {

        // Optional FILE keyword
        TestToken(TokenID.KFILE);

        ExtCallParseNode node = GetFileManagerExtCallNode("CLOSE");
        ParametersParseNode paramsNode = new();
        if (!_currentLine.IsAtEndOfLine) {
            paramsNode.Add(IntegerExpression());
        }
        node.Parameters = paramsNode;
        return node;
    }

    // DELETE
    //
    // Syntax: DELETE <filename>
    //
    // Deletes the specified file
    //
    private ExtCallParseNode KDelete() {

        ExtCallParseNode node = GetFileManagerExtCallNode("DELETE");
        ParametersParseNode paramsNode = new();
        paramsNode.Add(CastNodeToType(StringExpression(), SymType.CHAR));
        node.Parameters = paramsNode;
        return node;
    }

    // WRITE
    //
    // Syntax: WRITE [FILE] <file> [,<record>]: values
    //
    // Writes the specified values to the given record of the random access file.
    //
    private ExtCallParseNode KWrite() {
        VarArgParseNode varargs = new();

        // Optional FILE number
        TestToken(TokenID.KFILE);

        ParseNode fileParseNode = IntegerExpression();
        ParseNode recnumParseNode = new NumberParseNode(0);

        SimpleToken token = GetNextToken();
        if (token.ID == TokenID.COMMA) {
            recnumParseNode = IntegerExpression();
        }
        else {
            _currentLine.PushToken(token);
        }
        ExpectToken(TokenID.COLON);

        while (!_currentLine.IsAtEndOfStatement) {
            ParseNode exprNode = Expression();
            if (exprNode.Type == SymType.DOUBLE) {
                exprNode = CastNodeToType(exprNode, SymType.FLOAT);
            }
            varargs.Add(exprNode);
            if (!_currentLine.IsAtEndOfStatement) {
                ExpectToken(TokenID.COMMA);
            }
        }

        ExtCallParseNode node = new("JComalLib.FileManager,comallib", "WRITE");
        ParametersParseNode paramsNode = new();
        paramsNode.Add(fileParseNode);
        paramsNode.Add(recnumParseNode);
        paramsNode.Add(varargs);
        node.Parameters = paramsNode;
        return node;
    }

    // READ (FILE)
    //
    // Syntax: READ [FILE] <file> [,<record>] : <vars>
    //
    // READs data from a disk RANDOM or direct access file that previously was OPENed as a RANDOM type file
    // and correctly specifying the fixed record length. Random access files reserve a fixed length for each
    // record. The actual record may be shorter, but the same amount of space is used. Any record can be read
    // at any time by specifying its record number.
    //
    private InputManagerParseNode KReadFile() {
        InputManagerParseNode node = new();
        List<IdentifierParseNode> identifiers = [];

        // Optional FILE number
        TestToken(TokenID.KFILE);

        ParseNode fileParseNode = IntegerExpression();
        ParseNode recnumParseNode = null;
        if (TestToken(TokenID.COMMA)) {
            recnumParseNode = IntegerExpression();
        }
        ExpectToken(TokenID.COLON);

        while (!_currentLine.IsAtEndOfStatement) {
            SimpleToken token = ExpectToken(TokenID.IDENT);
            if (token != null) {
                IdentifierToken identToken = token as IdentifierToken;
                IdentifierParseNode identNode = ParseIdentifierFromToken(identToken);
                identifiers.Add(identNode);
            }
            if (!_currentLine.IsAtEndOfStatement) {
                ExpectToken(TokenID.COMMA);
            }
        }

        node.FileHandle = fileParseNode;
        node.RecordNumber = recnumParseNode;
        node.Identifiers = identifiers.ToArray();
        return node;
    }

    // INPUT
    //
    // Syntax: INPUT [[FILE] <file>] [AT <row>,<col> [,<len>] :] [<prompt>:] <vars>[<mark>]
    //
    // If <file> is not specified:
    //
    // INPUT allows the user to enter data into a running program from the keyboard (the AT section is optional).
    // During the INPUT from keyboard request, the input area is a protected field extending to the end of the
    // line (unless the length part of the AT section is specified). A 0 length means only a carriage return will
    // be accepted. A 0 for the row or column means not to change it (stay in the same row or column). If the
    // <mark> is a comma, the cursor remains where it is after the reply. If it is a semicolon, spaces are printed
    // to the next zone (one space by default if ZONE is not specified), then the cursor remains at that position.
    //
    // If <file> is specified:
    //
    // INPUT FILE gets the data from the file specified, which must have been previously opened for reading.
    // INPUT FILE reads ASCII files, such as those created by PRINT FILE or a Word Processor with ASCII file
    // output (does not read files created by WRITE FILE statements). The prompt is optional and may be a
    // variable. Both AT and <mark> are not permitted with INPUT FILE.
    //
    private InputManagerParseNode KInput() {
        InputManagerParseNode node = new();

        ParseNode rowPosition = null;
        ParseNode columnPosition = null;
        ParseNode maximumWidth = new NumberParseNode(-1);
        SimpleToken token;

        // Optional FILE number
        TestToken(TokenID.KFILE);

        // Optional positional statements.
        if (TestToken(TokenID.KAT)) {
            rowPosition = IntegerExpression();
            ExpectToken(TokenID.COMMA);
            columnPosition = IntegerExpression();
            token = GetNextToken();
            if (token.ID == TokenID.COMMA) {
                maximumWidth = IntegerExpression();
            }
            else {
                _currentLine.PushToken(token);
            }
            ExpectToken(TokenID.COLON);
        }

        ParseNode fileParseNode = new NumberParseNode(IOConstant.Stdin);

        // Optional input prompt or possible FILE ID. Since the syntax is similar
        // for both, we differentiate based on the type of the expression.
        ParseNode promptNode = null;
        ParseNode exprNode = Expression();

        token = GetNextToken();
        if (token.ID == TokenID.COLON) {
            if (exprNode.IsString) {
                promptNode = exprNode;
            }
            else {
                fileParseNode = exprNode;
                fileParseNode.Type = SymType.INTEGER;
            }
            exprNode = Expression();
        }
        else {
            _currentLine.PushToken(token);
        }

        // Variables
        List<IdentifierParseNode> identifiers = [];
        bool hasStringIdentifier = false;
        bool isStringIdentifier = false;
        TokenID lastToken = TokenID.EOL;

        do {
            if (exprNode is not IdentifierParseNode identNode) {
                Messages.Error(MessageCode.EXPECTEDTOKEN, "Identifier expected");
                break;
            }
            isStringIdentifier = identNode.IsString;
            if (isStringIdentifier && identNode.IsArrayBase) {
                Messages.Error(MessageCode.EXPECTEDTOKEN, "Cannot INPUT into a string array");
            }
            else if (isStringIdentifier && hasStringIdentifier) {
                Messages.Error(MessageCode.EXPECTEDTOKEN, "Only one string variable permitted in INPUT");
            }
            if (isStringIdentifier) {
                hasStringIdentifier = true;
            }
            identifiers.Add(identNode);

            token = _currentLine.GetToken();
            lastToken = token.ID;
            if (token.ID == TokenID.SEMICOLON) {
                ExpectEndOfLine();
                break;
            }
            if (token.ID == TokenID.COMMA) {
                if (_currentLine.PeekToken().ID == TokenID.EOL) {
                    break;
                }
            }
            if (token.ID == TokenID.EOL) {
                break;
            }
            _currentLine.PushToken(token);
            ExpectToken(TokenID.COMMA);
            exprNode = Expression();
        } while (true);

        // Ensure any string variable is at the end of the list
        if (hasStringIdentifier && !isStringIdentifier) {
            Messages.Error(MessageCode.EXPECTEDTOKEN, "String variable must be last variable in INPUT");
        }

        bool isStdin = fileParseNode.IsConstant && fileParseNode.Value.IntValue == IOConstant.Stdin;

        // AT cannot be used with FILE
        if (rowPosition != null && columnPosition != null && !isStdin) {
            Messages.Error(MessageCode.ILLEGALATWITHFILE, "AT cannot be used with FILE");
        }

        LineTerminator terminator = LineTerminator.NEWLINE;
        if (lastToken == TokenID.COMMA) {
            terminator = LineTerminator.NEXTZONE;
        }
        if (lastToken == TokenID.SEMICOLON) {
            terminator = LineTerminator.NONE;
        }

        node.MaximumWidth = maximumWidth;
        node.RowPosition = rowPosition;
        node.ColumnPosition = columnPosition;
        node.FileHandle = fileParseNode;
        node.Prompt = promptNode ?? new StringParseNode(string.Empty);
        node.Terminator = terminator;
        node.Identifiers = identifiers.ToArray();
        return node;
    }

    // PRINT
    //
    // Syntax: PRINT [AT <row>,<col>:) [USING <form>:] <list>[<mark>)
    //
    // Prints items as specified. More than one item may be specified in one PRINT statement, separated by a
    // comma or semicolon. A comma is a null separator (no spaces between items). A semicolon ; prints
    // spaces to the next zone (one space by default if ZONE has not been specified).
    //
    private ExtCallParseNode KPrint() {
        VarArgParseNode varargs = new();
        List<char> formats = [];

        ParseNode rowPosition = null;
        ParseNode columnPosition = null;

        // Optional FILE number
        TestToken(TokenID.KFILE);

        // Optional positional statements.
        if (TestToken(TokenID.KAT)) {
            rowPosition = IntegerExpression();
            ExpectToken(TokenID.COMMA);
            columnPosition = IntegerExpression();
            ExpectToken(TokenID.COLON);
        }

        ParseNode fileParseNode = new NumberParseNode(IOConstant.Stdout);

        while (!_currentLine.IsAtEndOfStatement) {
            ParseNode exprNode;

            SimpleToken token = GetNextToken();
            switch (token.ID) {
                case TokenID.SEMICOLON:
                    formats.Add('V');
                    continue;
                case TokenID.COMMA:
                    formats.Add('H');
                    continue;
                case TokenID.APOSTROPHE:
                    formats.Add('N');
                    continue;
                case TokenID.TILDE:
                    formats.Add('6');
                    continue;

                case TokenID.KTAB:
                    ExpectToken(TokenID.LPAREN);
                    exprNode = IntegerExpression();
                    ExpectToken(TokenID.RPAREN);
                    varargs.Add(exprNode);
                    formats.Add('T');
                    continue;

                case TokenID.KUSING: {
                    if (formats.Count > 1) {
                        Messages.Error(MessageCode.UNEXPECTEDTOKEN, "Misplaced USING");
                        return null;
                    }

                    ExtCallParseNode usingFunc = new("JComalLib.Intrinsics,comallib", "USING");
                    ParametersParseNode usingParams = new();
                    usingFunc.Parameters = usingParams;
                    usingFunc.Type = SymType.CHAR;

                    VarArgParseNode usingArgs = new();

                    usingParams.Add(StringExpression());
                    usingParams.Add(usingArgs);

                    ExpectToken(TokenID.COLON);

                    while (!_currentLine.IsAtEndOfStatement) {

                        exprNode = Expression();
                        usingArgs.Add(exprNode);

                        TokenID tokenID = _currentLine.PeekToken().ID;
                        if (tokenID is TokenID.SEMICOLON or TokenID.APOSTROPHE or TokenID.TILDE or TokenID.EOL) {
                            break;
                        }
                        ExpectToken(TokenID.COMMA);
                    }

                    if (usingParams.Nodes.Count > 0) {
                        varargs.Add(usingFunc);
                        formats.Insert(0, 'S');
                    }
                    continue;
                }

                default:
                    _currentLine.PushToken(token);
                    exprNode = Expression();
                    token = GetNextToken();
                    if (token.ID == TokenID.COLON) {

                        if (formats.Count > 1) {
                            Messages.Error(MessageCode.UNEXPECTEDTOKEN, "Unexpected :");
                            return null;
                        }
                        fileParseNode = exprNode;
                        if (!fileParseNode.IsNumber) {
                            Messages.Error(MessageCode.NUMBEREXPECTED, "File number must be an integer");
                        }
                        fileParseNode.Type = SymType.INTEGER;
                        continue;
                    }
                    if (exprNode.Type == SymType.DOUBLE) {
                        exprNode = CastNodeToType(exprNode, SymType.FLOAT);
                    }
                    varargs.Add(exprNode);
                    switch (exprNode.Type) {
                        case SymType.INTEGER:
                            formats.Add('I');
                            break;
                        case SymType.FLOAT:
                            formats.Add('F');
                            break;
                        case SymType.FIXEDCHAR:
                        case SymType.CHAR:
                            formats.Add('S');
                            break;
                    }
                    _currentLine.PushToken(token);
                    break;
            }
        }

        bool isStdout = fileParseNode.IsConstant && fileParseNode.Value.IntValue == IOConstant.Stdout;

        // AT cannot be used with FILE
        if (rowPosition != null && columnPosition != null && !isStdout) {
            Messages.Error(MessageCode.ILLEGALATWITHFILE, "AT cannot be used with FILE");
        }

        ExtCallParseNode node = new("JComalLib.PrintManager,comallib", "WRITE");
        ParametersParseNode paramsNode = new();
        if (rowPosition != null && columnPosition != null) {
            paramsNode.Add(rowPosition);
            paramsNode.Add(columnPosition);
        }

        // Is this a simple PRINT statement to the console? If so, call the string version.
        if (isStdout && formats is ['S']) {
            paramsNode.Add(varargs.Nodes[0]);
        }
        else {
            paramsNode.Add(fileParseNode);
            paramsNode.Add(new StringParseNode(string.Join("", formats)));
            paramsNode.Add(varargs);
        }
        node.Parameters = paramsNode;
        return node;
    }
}