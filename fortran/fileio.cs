// JFortran Compiler
// File I/O parsing
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

using CCompiler;
using JComLib;

namespace JFortran;

/// <summary>
/// Main Fortran compiler class.
/// </summary>
public partial class Compiler {

    private ExternalFunction _openFunction;
    private ExternalFunction _closeFunction;
    private ExternalFunction _posFunction;
    private ExternalFunction _readFunctions;
    private ExternalFunction _ioCoreFunctions;
    private ExternalFunction _readFunctionString;
    private ExternalFunction _writeFunctions;
    private ExternalFunction _writeFunctionString;
    private ExternalFunction _inquireFunction;

    // OPEN keyword
    // Opens a data file for reading or writing.
    private ParseNode KOpen() {
        ExtCallParseNode node = new("JFortranLib.IO,forlib", "OPEN");

        InitFunctionNodes();

        ControlList cilist = ParseCIList(_openFunction.ParameterList);
        if (cilist == null) {
            SkipToEndOfLine();
            return null;
        }
        if (cilist["UNIT"] == null) {
            Messages.Error(MessageCode.UNITREQUIRED, "UNIT specifier required for OPEN");
            return null;
        }
        node.Parameters = _openFunction.ParametersNode(cilist);

        // Wrap into a conditional if an ERR label is specified
        if (cilist.Has("ERR")) {
            SwitchParseNode switchNode = new() {
                CompareExpression = node
            };
            switchNode.Add(new NumberParseNode(-1), cilist["ERR"]);
            return switchNode;
        }
        return node;
    }

    // CLOSE keyword
    // Close a data file.
    private ParseNode KClose() {
        ExtCallParseNode node = new("JFortranLib.IO,forlib", "CLOSE");

        InitFunctionNodes();

        ControlList cilist = ParseCIList(_closeFunction.ParameterList);
        if (cilist == null) {
            SkipToEndOfLine();
            return null;
        }
        if (cilist["UNIT"] == null) {
            Messages.Error(MessageCode.UNITREQUIRED, "UNIT specifier required for CLOSE");
            return null;
        }
        node.Parameters = _closeFunction.ParametersNode(cilist);

        // Wrap into a conditional if an ERR label is specified.
        if (cilist.Has("ERR")) {
            SwitchParseNode switchNode = new() {
                CompareExpression = node
            };
            switchNode.Add(new NumberParseNode(-1), cilist["ERR"]);
            return switchNode;
        }
        return node;
    }

    // INQUIRE keyword
    // Inquire of the state of a file.
    private ParseNode KInquire() {
        ExtCallParseNode node = new("JFortranLib.IO,forlib", "INQUIRE");

        InitFunctionNodes();

        ControlList cilist = ParseCIList(_inquireFunction.ParameterList);
        if (cilist == null) {
            SkipToEndOfLine();
            return null;
        }
        node.Parameters = _inquireFunction.ParametersNode(cilist);

        // Wrap into a conditional if an ERR label is specified.
        if (cilist.Has("ERR")) {
            SwitchParseNode switchNode = new();
            node.Type = SymType.INTEGER;
            switchNode.CompareExpression = node;
            switchNode.Add(new NumberParseNode(-1), cilist["ERR"]);
            return switchNode;
        }
        return node;
    }

    // READ keyword
    // Reads data into variables. Basically the run-time handles the I/O and the
    // format list, and we provide the parameters.
    private ParseNode KRead() {
        ReadParseNode node = new();

        InitFunctionNodes();

        ControlList cilist = ParseCIList(_readFunctions.ParameterList);
        if (cilist == null) {
            SkipToEndOfLine();
            return null;
        }

        node.ArgList = ParseVarargReferenceList();
        node.EndLabel = (SymbolParseNode)cilist["END"];
        node.ErrLabel = (SymbolParseNode)cilist["ERR"];
        node.ReadParamsNode = _ioCoreFunctions.ParametersNode(cilist);

        // If this is internal storage, create an expression that
        // uses the given character string as the input source.
        ParseNode unit = cilist["UNIT"];

        if (unit != null && unit.ID == ParseID.IDENT && Symbol.IsCharType(unit.Type)) {
            if (cilist.Has("REC")) {
                Messages.Error(MessageCode.CILISTNOTALLOWED, "Parameter REC not allowed here");
                SkipToEndOfLine();
                return null;
            }
            node.ReadManagerParamsNode = _readFunctionString.ParametersNode(cilist);
        }
        else {
            if (unit == null) {
                cilist["UNIT"] = new NumberParseNode(new Variant(IOConstant.Stdin));
            }
            node.ReadManagerParamsNode = _readFunctions.ParametersNode(cilist);
        }
        return node;
    }

    // WRITE keyword
    private ParseNode KWrite() {
        WriteParseNode node = new();

        InitFunctionNodes();

        ControlList cilist = ParseCIList(_writeFunctions.ParameterList);
        if (cilist == null) {
            SkipToEndOfLine();
            return null;
        }

        node.ArgList = ParseVarargList();
        node.ErrLabel = (SymbolParseNode)cilist["ERR"];

        // If this is internal storage, create an expression that
        // assigns the result to the character string
        ParseNode unit = cilist["UNIT"];

        if (unit != null && unit.ID == ParseID.IDENT && Symbol.IsCharType(unit.Type)) {
            node.WriteParamsNode = _ioCoreFunctions.ParametersNode(cilist);
            node.WriteManagerParamsNode = _writeFunctionString.ParametersNode(cilist);

            return new AssignmentParseNode((IdentifierParseNode)unit, node);
        }

        if (unit == null) {
            cilist["UNIT"] = new NumberParseNode(new Variant(IOConstant.Stdout));
        }
        node.WriteParamsNode = _ioCoreFunctions.ParametersNode(cilist);
        node.WriteManagerParamsNode = _writeFunctions.ParametersNode(cilist);
        return node;
    }

    // PRINT keyword
    private ParseNode KPrint() {
        WriteParseNode node = new();

        InitFunctionNodes();

        ControlList cilist = new() {
            ["FMT"] = ParseFormatSpecifier(),
            ["UNIT"] = new NumberParseNode(new Variant(IOConstant.Stdout))
        };

        if (!IsAtEndOfLine()) {
            SimpleToken token;

            ExpectToken(TokenID.COMMA);
            VarArgParseNode varargs = new();
            do {
                varargs.Add(ParseExpressionWithImpliedDo());
                token = _ls.GetToken();
            } while (token.ID == TokenID.COMMA);
            _ls.BackToken();
            node.ArgList = varargs;
        }

        node.WriteParamsNode = _ioCoreFunctions.ParametersNode(cilist);
        node.WriteManagerParamsNode = _writeFunctions.ParametersNode(cilist);
        return node;
    }

    // ENDFILE keyword
    private ParseNode KEndFile() {
        return PositionStatement("ENDFILE");
    }

    // REWIND keyword
    private ParseNode KRewind() {
        return PositionStatement("REWIND");
    }

    // BACKSPACE keyword
    private ParseNode KBackspace() {
        return PositionStatement("BACKSPACE");
    }

    // Generic function that handles ENDFILE, BACKSPACE and REWIND since they
    // all take the exact same parameters.
    private ParseNode PositionStatement(string keyword) {
        ExtCallParseNode node = new("JFortranLib.IO,forlib", keyword);

        InitFunctionNodes();

        ControlList cilist = ParseCIList(_posFunction.ParameterList);
        if (cilist == null) {
            SkipToEndOfLine();
            return null;
        }
        if (cilist["UNIT"] == null) {
            Messages.Error(MessageCode.UNITREQUIRED, "UNIT specifier required for REWIND");
            return null;
        }
        node.Parameters = _posFunction.ParametersNode(cilist);

        // Wrap into a conditional if an ERR label is specified.
        if (cilist.Has("ERR")) {
            SwitchParseNode switchNode = new() {
                CompareExpression = node
            };
            switchNode.Add(new NumberParseNode(-1), cilist["ERR"]);
            return switchNode;
        }
        return node;
    }

    // Initialise the ExternalFunction nodes for the various I/O statements
    // if they are not already initialised.
    private void InitFunctionNodes() {

        // WriteManager for external I/O
        if (_writeFunctions == null) {
            _writeFunctions = new ExternalFunction();
            _writeFunctions.Add("UNIT", SymType.INTEGER, SymLinkage.BYVAL);
            _writeFunctions.Add("REC", SymType.INTEGER, SymLinkage.BYVAL);
            _writeFunctions.Add("IOSTAT", SymType.INTEGER, SymLinkage.BYREF, false);
            _writeFunctions.Add("ERR", SymType.LABEL, SymLinkage.BYVAL, false);
            _writeFunctions.Add("FMT", SymType.CHAR, SymLinkage.BYVAL);
            _writeFunctions.Add("ADVANCE", SymType.INTEGER, SymLinkage.BYVAL, false);
        }

        // WriteManager for internal string function
        if (_writeFunctionString == null) {
            _writeFunctionString = new ExternalFunction();
            _writeFunctionString.Add("FMT", SymType.CHAR, SymLinkage.BYVAL);
        }

        // ReadManager for external I/O
        if (_readFunctions == null) {
            _readFunctions = new ExternalFunction();
            _readFunctions.Add("UNIT", SymType.INTEGER, SymLinkage.BYVAL);
            _readFunctions.Add("REC", SymType.INTEGER, SymLinkage.BYVAL);
            _readFunctions.Add("IOSTAT", SymType.INTEGER, SymLinkage.BYREF, false);
            _readFunctions.Add("ERR", SymType.LABEL, SymLinkage.BYVAL, false);
            _readFunctions.Add("END", SymType.LABEL, SymLinkage.BYVAL, false);
            _readFunctions.Add("FMT", SymType.CHAR, SymLinkage.BYVAL);
        }

        // Core I/O function
        if (_ioCoreFunctions == null) {
            _ioCoreFunctions = new ExternalFunction();
            _ioCoreFunctions.Add("IOSTAT", SymType.INTEGER, SymLinkage.BYREF);
        }

        // ReadManager for internal string function
        if (_readFunctionString == null) {
            _readFunctionString = new ExternalFunction();
            _readFunctionString.Add("UNIT", SymType.CHAR, SymLinkage.BYVAL);
            _readFunctionString.Add("FMT", SymType.CHAR, SymLinkage.BYVAL);
        }

        // Positional statement functions (BACKSPACE, REWIND and ENDFILE)
        if (_posFunction == null) {
            _posFunction = new ExternalFunction();
            _posFunction.Add("UNIT", SymType.INTEGER, SymLinkage.BYVAL);
            _posFunction.Add("IOSTAT", SymType.INTEGER, SymLinkage.BYREF);
            _posFunction.Add("ERR", SymType.LABEL, SymLinkage.BYVAL, false);
        }

        // OPEN statement functions
        if (_openFunction == null) {
            _openFunction = new ExternalFunction();
            _openFunction.Add("UNIT", SymType.INTEGER, SymLinkage.BYVAL);
            _openFunction.Add("IOSTAT", SymType.INTEGER, SymLinkage.BYREF);
            _openFunction.Add("ERR", SymType.LABEL, SymLinkage.BYVAL, false);
            _openFunction.Add("FILE", SymType.CHAR, SymLinkage.BYVAL);
            _openFunction.Add("STATUS", SymType.CHAR, SymLinkage.BYVAL);
            _openFunction.Add("ACCESS", SymType.CHAR, SymLinkage.BYVAL);
            _openFunction.Add("FORM", SymType.CHAR, SymLinkage.BYVAL);
            _openFunction.Add("RECL", SymType.INTEGER, SymLinkage.BYVAL);
            _openFunction.Add("BLANK", SymType.CHAR, SymLinkage.BYVAL);
        }

        // CLOSE statement functions
        if (_closeFunction == null) {
            _closeFunction = new ExternalFunction();
            _closeFunction.Add("UNIT", SymType.INTEGER, SymLinkage.BYVAL);
            _closeFunction.Add("IOSTAT", SymType.INTEGER, SymLinkage.BYREF);
            _closeFunction.Add("ERR", SymType.LABEL, SymLinkage.BYVAL, false);
            _closeFunction.Add("STATUS", SymType.CHAR, SymLinkage.BYVAL);
        }

        // INQUIRE statement functions
        if (_inquireFunction == null) {
            _inquireFunction = new ExternalFunction();
            _inquireFunction.Add("UNIT", SymType.INTEGER, SymLinkage.BYVAL);
            _inquireFunction.Add("FILE", SymType.CHAR, SymLinkage.BYVAL);
            _inquireFunction.Add("IOSTAT", SymType.INTEGER, SymLinkage.BYREF);
            _inquireFunction.Add("ERR", SymType.LABEL, SymLinkage.BYVAL, false);
            _inquireFunction.Add("EXIST", SymType.BOOLEAN, SymLinkage.BYREF);
            _inquireFunction.Add("OPENED", SymType.BOOLEAN, SymLinkage.BYREF);
            _inquireFunction.Add("NUMBER", SymType.INTEGER, SymLinkage.BYREF);
            _inquireFunction.Add("NAMED", SymType.BOOLEAN, SymLinkage.BYREF);
            _inquireFunction.Add("NAME", SymType.FIXEDCHAR, SymLinkage.BYREF);
            _inquireFunction.Add("ACCESS", SymType.FIXEDCHAR, SymLinkage.BYREF);
            _inquireFunction.Add("SEQUENTIAL", SymType.FIXEDCHAR, SymLinkage.BYREF);
            _inquireFunction.Add("DIRECT", SymType.FIXEDCHAR, SymLinkage.BYREF);
            _inquireFunction.Add("FORM", SymType.FIXEDCHAR, SymLinkage.BYREF);
            _inquireFunction.Add("FORMATTED", SymType.FIXEDCHAR, SymLinkage.BYREF);
            _inquireFunction.Add("UNFORMATTED", SymType.FIXEDCHAR, SymLinkage.BYREF);
            _inquireFunction.Add("RECL", SymType.INTEGER, SymLinkage.BYREF);
            _inquireFunction.Add("NEXTREC", SymType.INTEGER, SymLinkage.BYREF);
            _inquireFunction.Add("BLANK", SymType.FIXEDCHAR, SymLinkage.BYREF);
        }
    }
}