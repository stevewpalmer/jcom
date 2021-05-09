// JComal
// Main compiler class
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using CCompiler;
using JComLib;

namespace JComal {

    /// <summary>
    /// Main compiler class.
    /// </summary>
    public partial class Compiler : ICompiler {

        // Used to keep track of the block state
        private enum BlockState {
            NONE,
            PROGRAM,
            SUBFUNC,
            SPECIFICATION,
            STATEMENT,
            UNORDERED // Unordered must always be last
        }

        private readonly SymbolCollection _globalSymbols;
        private readonly ComalOptions _opts;
        private readonly CollectionParseNode _ptree;
        private readonly string _entryPointName;

        private BlockState _state;
        private Lines _ls;
        private ProgramDefinition _programDef;
        private SymbolCollection _localSymbols;
        private SymbolCollection _importSymbols;
        private ProcedureParseNode _currentProcedure;
        private bool _hasReturn;
        private bool _hasProgram;
        private Line _currentLine;
        private LoopParseNode _currentLoop;
        private int _currentLineNumber;
        private int _blockDepth;

        /// <summary>
        /// Return or set the list of compiler messages.
        /// </summary>
        public MessageCollection Messages { get; set; }

        /// <summary>
        /// Constructs a compiler object with the given options.
        /// </summary>
        /// <param name="opts">Compiler options</param>
        /// <param name="messages">Messages table</param>
        public Compiler(ComalOptions opts, MessageCollection messages) {
            _globalSymbols = new("Global");
            _localSymbols = null;
            _importSymbols = null;
            _ptree = new CollectionParseNode();
            Messages = messages;
            _entryPointName = "Main";
            _opts = opts;
        }

        /// <summary>
        /// Compile one file. If the filename ends with SaveFileExtension then it is
        /// assumed to be a Comal program file created by SAVE. Otherwise it is assumed
        /// to be a Comal source file.
        ///
        /// An exception is thrown if the file doesn't exist.
        /// </summary>
        /// <param name="filename">The full path and file name to be compiled</param>
        public void Compile(string filename) {

            Lines lines = new();

            string extension = Path.GetExtension(filename).ToLower();
            if (extension == Consts.ProgramFileExtension) {
                using Stream stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                ByteReader byteReader = new(stream);
                lines.Deserialize(byteReader);
            } else {
                LineTokeniser tokeniser = new();

                using (StreamReader sr = new(filename)) {
                    while (sr.Peek() != -1) {
                        string sourceLine = sr.ReadLine();
                        Line line = new(tokeniser.TokeniseLine(sourceLine));
                        lines.Add(line);
                    }
                }
            }
            CompileString(filename, lines);
        }

        /// <summary>
        /// Compile an string of source lines, optionally adding line numbers at the start.
        /// </summary>
        /// <param name="source">A string with source lines delimited by newlines</param>
        /// <param name="autoNumber">Specifies whether to add line numbers</param>
        public void CompileString(string source, bool autoNumber) {

            // Split the source
            string [] sourceLines = source.Split('\n');
            Lines lines = new();
            int lineNumber = 100;
            LineTokeniser tokeniser = new();

            // Auto-number the lines.
            for (int index = 0; index < sourceLines.Length; index++ ) {

                string sourceLine = sourceLines[index];
                if (!string.IsNullOrWhiteSpace(sourceLine)) {
                    if (autoNumber) {
                        sourceLine = string.Format("{0} {1}", lineNumber, sourceLine.Trim());
                        lineNumber += 10;
                    }
                    Line line = new(tokeniser.TokeniseLine(sourceLine));
                    lines.Add(line);
                }
            }
            CompileString(null, lines);
        }

        /// <summary>
        /// Compile an array of source lines.
        /// This function exists primarily for unit tests.
        /// </summary>
        /// <param name="lines">A Lines object representing the source file</param>
        public void CompileString(string [] sourceLines) {

            // Tokenise the file
            Lines lines = new();
            LineTokeniser tokeniser = new();
            foreach (string sourceLine in sourceLines) {
                Line line = new(tokeniser.TokeniseLine(sourceLine));
                lines.Add(line);
            }
            CompileString(null, lines);
        }

        /// <summary>
        /// Compile an array of source lines.
        /// This function exists primarily for unit tests.
        /// </summary>
        /// <param name="lines">A Lines object representing the source file</param>
        public void CompileLines(Lines lines) {
            CompileString(null, lines);
        }

        /// <summary>
        /// Convert the parse tree to executable code then save it to the
        /// filename specified in the options.
        /// </summary>
        public void Save() {
            try {
                CodeGenerator codegen = new(_opts);
                MarkExecutable();
                codegen.GenerateCode(_programDef);
                codegen.Save();
            }
            catch (CodeGeneratorException e) {
                Messages.Error(e.Filename, MessageCode.CODEGEN, e.Linenumber, e.Message);
            }
        }

        /// <summary>
        /// Convert the parse tree to executable code and then execute the
        /// code starting at the entry point function. The return value from the
        /// main function is returned as an object.
        /// </summary>
        /// <returns>An ExecutionResult object representing the result of the execution</returns>
        public ExecutionResult Execute() {
            return Execute(_entryPointName);
        }

        /// <summary>
        /// Convert the parse tree to executable code and then execute the
        /// resulting code. The return value from the specified entry point function
        /// is returned as an object.
        /// </summary>
        /// <param name="entryPointName">The name of the method to be called</param>
        /// <returns>An ExecutionResult object representing the result of the execution</returns>
        public ExecutionResult Execute(string entryPointName) {
            CodeGenerator codegen = new(_opts);
            MarkExecutable();
            codegen.GenerateCode(_programDef);
            return codegen.Run(entryPointName);
        }

        // Compile an array of source lines.
        private void CompileString(string filename, Lines lines) {
            try {
                _blockDepth = 0;
                _state = BlockState.NONE;

                // Create the top-level program node.
                if (_programDef == null) {
                    string moduleName = Path.GetFileNameWithoutExtension(_opts.OutputFile);
                    if (string.IsNullOrEmpty(moduleName)) {
                        moduleName = "Class";
                    }
                    _programDef = new();
                    _programDef.Name = moduleName;
                    _programDef.Globals = _globalSymbols;
                    _programDef.IsExecutable = true;
                    _programDef.Root = _ptree;
                }

                // Create special variables
                _globalSymbols.Add(new Symbol(Consts.ErrName, new SymFullType(SymType.INTEGER), SymClass.VAR, null, 0) {
                    Modifier = SymModifier.STATIC,
                    Value = new Variant(0)
                });
                _globalSymbols.Add(new Symbol(Consts.ErrText, new SymFullType(SymType.CHAR), SymClass.VAR, null, 0) {
                    Modifier = SymModifier.STATIC,
                    Value = new Variant(string.Empty)
                });

                CompileUnit(filename, lines);

                // Warn about exported methods that are not defined
                foreach (Symbol sym in _globalSymbols) {
                    if (sym.IsExported && !sym.Defined) {
                        Messages.Warning(MessageCode.MISSINGEXPORT, 1,
                            $"{sym.Name} marked as EXPORT but not found in source file");
                    }
                }

                // Dump file?
                if (_opts.Dump) {
                    XmlDocument xmlTree = ParseTreeXml.Tree(_programDef);
                    string outputFilename = Path.GetFileName(_opts.OutputFile);
                    outputFilename = Path.ChangeExtension(outputFilename, ".xml");
                    xmlTree.Save(outputFilename);
                }
            } catch (Exception e) {
                if (_opts.DevMode) {
                    throw;
                }
                Messages.Error(MessageCode.COMPILERFAILURE, $"Compiler error: {e.Message}");
            }
        }

        // Compile a code unit, such as an include file.
        private void CompileUnit(string filename, Lines lines) {

            _ls = lines;
            Messages.Filename = filename;

            // Mark this file.
            if (filename != null) {
                _ptree.Add(MarkFilename());
            }

            // Pre-scan to locate all PROC/FUNCs so we have their declaration
            // ahead of their usage
            Pass0();

            // Loop and parse one single line at a time.
            CompileBlock(_ptree, new[] { TokenID.ENDOFFILE });
        }

        // Do an initial pass-0 scan of all lines looking for PROC and FUNC tokens,
        // parsing their name, type and any parameters and enter those into the global
        // symbol table. This ensures that references to them ahead of time resolve
        // to the correct type, particularly for functions.
        private void Pass0() {

            Stack<Symbol> parents = new(new Symbol [] { null });

            // Add implicit entrypoint subroutine name.
            _globalSymbols.Add(_entryPointName, new SymFullType(), SymClass.SUBROUTINE, null, 0);

            foreach (Line line in _ls.AllLines) {
                int lineNumber = 0;

                _currentLine = line;
                SimpleToken token = GetNextToken();
                if (token.ID == TokenID.INTEGER) {
                    IntegerToken lineNumberToken = token as IntegerToken;
                    lineNumber = lineNumberToken.Value;
                    Messages.Linenumber = lineNumber;
                    token = GetNextToken();
                }
                if (token.ID == TokenID.KENDPROC || token.ID == TokenID.KENDFUNC) {
                    parents.Pop();
                }
                if (token.ID == TokenID.KPROC || token.ID == TokenID.KFUNC) {
                    SymClass klass = token.ID == TokenID.KPROC ? SymClass.SUBROUTINE : SymClass.FUNCTION;
                    IdentifierToken identToken = ParseIdentifier();
                    if (identToken != null) {

                        // Check method name hasn't already been declared.
                        string methodName = identToken.Name;
                        Symbol method = _globalSymbols.Get(methodName);
                        if (method != null && method.Defined && !method.IsExternal) {
                            Messages.Error(MessageCode.SUBFUNCDEFINED, $"{methodName} already defined");
                            SkipToEndOfLine();
                            continue;
                        }

                        // Parameter list.
                        Collection<Symbol> parameters = null;
                        if (methodName != _entryPointName) {
                            SymbolCollection symbolTable = new("Temp");
                            parameters = ParseParameterDecl(symbolTable, SymScope.PARAMETER);
                        }

                        // Add this method to the global symbol table now.
                        if (method == null) {
                            method = _globalSymbols.Add(methodName, new SymFullType(), klass, null, lineNumber);
                        }
                        if (klass == SymClass.FUNCTION) {
                            method.FullType = GetTypeFromName(methodName);
                        }

                        if (TestToken(TokenID.KEXTERNAL)) {
                            method.Modifier = SymModifier.EXTERNAL;
                            method.ExternalLibrary = ParseStringLiteral();
                        } else {
                            method.Parent = parents.Peek();
                            parents.Push(method);
                        }

                        method.Parameters = parameters;
                        method.Defined = true;
                        method.Class = klass;
                    }
                }
            }
        }

        // Add all symbols from _globalSymbols to the symbols class whose parent
        // is the specified symbol.
        private void AddChildSymbols(SymbolCollection symbols, Symbol parentSymbol) {

            foreach (Symbol childSymbol in _globalSymbols) {
                if (childSymbol.IsMethod && childSymbol.Parent == parentSymbol) {
                    symbols.Add(childSymbol);
                    AddChildSymbols(symbols, childSymbol);
                }
            }
        }

        // Compile a block within a function or procedure, or within a structured statement
        // such as WHILE, REPEAT or CASE/WHEN. The endTokens list specify tokens that can
        // end the block.
        private TokenID CompileBlock(CollectionParseNode node, TokenID [] endTokens) {

            SimpleToken token = new(TokenID.EOL);
            while (!_ls.EndOfFile) {

                _currentLine = _ls.NextLine;
                token = GetNextToken();

                // Possible initial line number
                if (token.ID == TokenID.INTEGER) {
                    IntegerToken lineNumberToken = token as IntegerToken;
                    _currentLineNumber = lineNumberToken.Value;
                    Messages.Linenumber = _currentLineNumber;
                    token = GetNextToken();
                }

                if (token.ID != TokenID.EOL) {
                    if (Array.IndexOf(endTokens, token.ID) >= 0) {
                        return token.ID;
                    }
                    node.Add(MarkLine());
                    CompileLine(token, node);
                }
            }
            if (endTokens[0] == TokenID.ENDOFFILE) {
                return token.ID;
            }
            string endTokenName = Tokens.TokenIDToString(endTokens[0]);
            Messages.Error(MessageCode.MISSINGENDSTATEMENT, $"Missing {endTokenName}");
            return token.ID;
        }

        // Parse a single line.
        private void CompileLine(SimpleToken token, CollectionParseNode statements) {

            ParseNode subnode = Statement(token);
            if (subnode != null) {
                statements.Add(subnode);
                ExpectEndOfLine();
            }
        }

        // Create a token node that marks the current file being compiled. The
        // code generator uses this to refer to the correct source file if any
        // errors are found during generation.
        private ParseNode MarkFilename() {
            return new MarkFilenameParseNode {Filename = Messages.Filename};
        }

        // Create a token node that marks the number of the current line being
        // compiled. The code generator uses this in conjunction with the
        // filename to refer to the location in the source file if any errors
        // are found during generation.
        private ParseNode MarkLine() {
            return new MarkLineParseNode {LineNumber = _currentLineNumber};
        }

        // Mark in the PROGRAM node whether or not this program is executable.
        // An executable program is one which has a PROGRAM statement.
        private void MarkExecutable() {
            if (_programDef != null) {
                _programDef.IsExecutable = _hasProgram;
            }
        }

        // Retrieve the next token for the current line and check for any
        // errors
        private SimpleToken GetNextToken() {

            SimpleToken token = _currentLine.GetToken();
            while (token.ID == TokenID.ERROR && Messages != null) {
                ErrorToken errorToken = token as ErrorToken;
                Messages.Error(MessageCode.EXPECTEDTOKEN, errorToken.Message);
                token = _currentLine.GetToken();
            }
            return token;
        }

        // Consult the symbol table for the current scope for the given label. Labels
        // cannot have scope other than the current block.
        private Symbol GetLabel(string label) {
            return _localSymbols.Get(label);
        }

        // Create an entry in the symbol table for the specified label.
        private Symbol GetMakeLabel(string label, bool isDeclaration) {
            Symbol sym = GetLabel(label);
            if (sym == null) {
                sym = _localSymbols.Add(label,
                                        new SymFullType(SymType.LABEL),
                                        SymClass.LABEL,
                                        null,
                                        _currentLineNumber);
                sym.Defined = isDeclaration;
            } else if (isDeclaration && sym.Defined) {
                Messages.Error(MessageCode.LABELALREADYDECLARED, $"Label {label} already declared");
            } else {
                sym.Defined = isDeclaration || sym.Defined;
            }
            if (isDeclaration) {
                sym.Depth = _blockDepth;
            } else {
                sym.IsReferenced = true;
            }
            return sym;
        }

        // Make sure we have an _EOD symbol, and create one otherwise.
        private Symbol GetMakeEODSymbol() {

            Symbol dataIndexSymbol = _globalSymbols.Get(Consts.EODName);
            if (dataIndexSymbol == null) {
                dataIndexSymbol = new(Consts.EODName, new SymFullType(SymType.INTEGER), SymClass.VAR, null, 0) {
                    Modifier = SymModifier.STATIC,
                    Value = new Variant(1)
                };
                _globalSymbols.Add(dataIndexSymbol);
            }
            return dataIndexSymbol;
        }

        // Make sure we have a _DATAINDEX for the READ statement, and create one otherwise.
        private Symbol GetMakeReadDataIndexSymbol() {

            Symbol dataIndexSymbol = _globalSymbols.Get(Consts.DataIndexName);
            if (dataIndexSymbol == null) {
                dataIndexSymbol = new(Consts.DataIndexName, new SymFullType(SymType.INTEGER), SymClass.VAR, null, 0) {
                    Modifier = SymModifier.STATIC,
                    Value = new Variant(0),
                    IsReferenced = true
                };
                _globalSymbols.Add(dataIndexSymbol);
            }
            return dataIndexSymbol;
        }

        // Make sure we have a _DATA for the READ statement, and create one otherwise.
        private Symbol GetMakeReadDataSymbol() {

            Symbol dataSymbol = _globalSymbols.Get(Consts.DataName);
            if (dataSymbol == null) {
                dataSymbol = new(Consts.DataName, new SymFullType(SymType.GENERIC), SymClass.VAR, null, 0) {
                    Modifier = SymModifier.STATIC | SymModifier.FLATARRAY,
                    IsReferenced = true
                };
                _globalSymbols.Add(dataSymbol);
            }
            return dataSymbol;
        }

        // Look up the symbol table for the specified identifier starting with the
        // current scope and working up to and including global/imports. If we're
        // in a closed procedure, _globalSymbols is ignored and _importSymbols is
        // used so that anything other than predefined 
        private Symbol GetSymbolForCurrentScope(string name) {
            Symbol sym = null;
            if (_localSymbols != null) {
                sym = _localSymbols.Get(name);
            }
            if (sym == null) {
                if (_importSymbols != null) {
                    sym = _importSymbols.Get(name);
                } else {
                    sym = _globalSymbols.Get(name);
                }
            }
            return sym;
        }

        // Look up the symbol table for the specified identifier starting with the
        // current scope and working up to and including global.
        private Symbol GetMakeSymbolForCurrentScope(string name) {
            Symbol sym = GetSymbolForCurrentScope(name);
            if (sym == null) {
                sym = MakeSymbolForCurrentScope(name);
            }
            return sym;
        }

        // Make a symbol for the current scope and initialise it.
        private Symbol MakeSymbolForCurrentScope(string name) {
            SymFullType symType = GetTypeFromName(name);
            Symbol sym = _localSymbols.Add(name, symType, SymClass.VAR, null, _currentLineNumber);
            sym.Defined = true;
            InitialiseToDefault(sym);
            return sym;
        }

        // Initialise the specified symbol to the default in Comal. Numbers are
        // initialised to 0 and strings are initialised to empty. If we're not
        // in strict mode, we also dimension the string to the default width.
        private void InitialiseToDefault(Symbol sym) {
            if (!sym.IsArray) {
                if (sym.Type == SymType.INTEGER) {
                    sym.Value = new Variant(0);
                }
                if (sym.Type == SymType.FLOAT) {
                    sym.Value = new Variant((float)0);
                }
                if (sym.Type == SymType.CHAR || sym.Type == SymType.FIXEDCHAR) {
                    sym.Value = new Variant(string.Empty);
                    if (!_opts.Strict) {
                        sym.FullType.Width = Consts.DefaultStringWidth;
                    }
                }
            }
        }

        // Determine the type of a variable from its name. Integer variables end
        // with Consts.IntegerChar, string variable with Consts.StringChar.
        // Anything else is a floating point.
        private SymFullType GetTypeFromName(string name) {
            if (name.EndsWith(Consts.IntegerChar.ToString())) {
                return new SymFullType(SymType.INTEGER);
            }
            if (name.EndsWith(Consts.StringChar.ToString())) {
                return new SymFullType(SymType.FIXEDCHAR, 0);
            }
            return new SymFullType(SymType.FLOAT);
        }

        // Create an ExtCallParseNode for the specified function in the Intrinsics
        // class with the inline flag set from the options.
        private ExtCallParseNode GetIntrinsicExtCallNode(string functionName) {

            return new("JComalLib.Intrinsics,jcomallib", functionName) {
                Inline = _opts.Inline
            };
        }

        // Create an ExtCallParseNode for the specified function in the FileManager
        // class with the inline flag set from the options.
        private ExtCallParseNode GetFileManagerExtCallNode(string functionName) {

            return new("JComalLib.FileManager,jcomallib", functionName);
        }

        // Create an ExtCallParseNode for the specified function in the Runtime
        // class with the inline flag set from the options.
        private ExtCallParseNode GetRuntimeExtCallNode(string functionName) {

            return new("JComLib.Runtime,jcomlib", functionName);
        }

        // Ensure that the next token in the input is the one expected and report an error otherwise.
        private SimpleToken ExpectToken(TokenID expectedID) {
            SimpleToken token = GetNextToken();
            if (token.ID != expectedID) {
                Messages.Error(MessageCode.EXPECTEDTOKEN, $"Expected '{Tokens.TokenIDToString(expectedID)}' not '{Tokens.TokenIDToString(token.ID)}'");
                return null;
            }
            return token;
        }

        // Replace the current token read with the new token if there is a match.
        private void ReplaceCurrentToken(TokenID oldToken, TokenID newToken) {
            SimpleToken token = _currentLine.PeekToken();
            if (token.ID == oldToken) {
                _currentLine.ReplaceToken(new SimpleToken(newToken));
            }
        }

        // Ensure that the next token in the input is the one expected and report an error otherwise.
        private SimpleToken InsertTokenIfMissing(TokenID expectedID) {
            SimpleToken token = _currentLine.PeekToken();
            if (token.ID != expectedID) {

                if (_opts.Strict) {
                    Messages.Error(MessageCode.EXPECTEDTOKEN, $"Expected '{Tokens.TokenIDToString(expectedID)}'");
                }
                _currentLine.InsertTokens(new [] {
                    new SimpleToken(TokenID.SPACE),
                    new SimpleToken(expectedID)
                });
                GetNextToken();
            }
            return GetNextToken();
        }

        // Eat the input to the end of the line. Useful when we hit a syntax error and want
        // to get to a clean state before continuing.
        private void SkipToEndOfLine() {
            SimpleToken token = GetNextToken();
            while (token.ID != TokenID.EOL && token.ID != TokenID.ENDOFFILE) {
                token = GetNextToken();
            }
        }

        // Check that the next token is the end of the line and report an error otherwise.
        private void ExpectEndOfLine() {
            if (!_currentLine.IsAtEndOfStatement) {
                Messages.Error(MessageCode.ENDOFSTATEMENT, "End of line expected");
                SkipToEndOfLine();
            }
        }

        // Check whether the next token is the one specified and skip it if so.
        private bool TestToken(TokenID id) {
            SimpleToken token = _currentLine.PeekToken();
            if (token.ID == id) {
                GetNextToken();
                return true;
            }
            return false;
        }

        // Verify that the identifier that appears after an ENDPROC, ENDFUNC or
        // ENDFOR statement matches the specified identifier and add it if it is
        // missing.
        private void CheckEndOfBlockName(IdentifierToken identToken, SimpleToken token) {

            if (token.ID == TokenID.IDENT && identToken != null) {
                IdentifierToken endIdentToken = token as IdentifierToken;
                if (endIdentToken.Name != identToken.Name) {
                    Messages.Error(MessageCode.UNDEFINEDLABEL, $"Mismatched name {endIdentToken.Name}");
                }
            }

            // End of statement? Add missing identifier
            if (token.ID == TokenID.EOL && identToken != null) {
                _currentLine.InsertTokens(new[] { new SimpleToken(TokenID.SPACE), identToken });
                GetNextToken();
            }
        }

        // Change the scope state
        // Report an error if the token is being used in the wrong place (such as a declaration
        // after an executable statement).
        private bool ChangeState(SimpleToken token) {

            BlockState newState = TokenToState(token);
            if (newState != BlockState.SUBFUNC && newState < _state) {
                Messages.Error(MessageCode.TOKENNOTPERMITTED, $"{token} not permitted here");
                return false;
            }
            if (newState != BlockState.UNORDERED) {
                _state = newState;
            }

            // If we're in a statement but we haven't defined a procedure yet, create the
            // default Main one.
            if (newState == BlockState.STATEMENT) {
                if (_currentProcedure == null) {
                    _ls.BackLine();
                    ParseProcFuncDefinition(SymClass.SUBROUTINE, TokenID.ENDOFFILE, _entryPointName);
                    return false;
                }
            }
            return true;
        }

        // Parse one statement.
        // Note that this may be called as part of a logical or block IF statement. Non-executable statements
        // won't be allowed in this context but, happily, if we're parsing an IF statement then we're already
        // in STATEMENT state.
        private ParseNode Statement(SimpleToken token) {

            // First make sure this statement is allowed here.
            if (ChangeState(token)) {
                switch (token.ID) {
                    case TokenID.KCASE:             return KCase();
                    case TokenID.KCLOSE:            return KClose();
                    case TokenID.KCOLOUR:           return KColour();
                    case TokenID.KCREATE:           return KCreate();
                    case TokenID.KCURSOR:           return KCursor();
                    case TokenID.KDATA:             return KData();
                    case TokenID.KDELETE:           return KDelete();
                    case TokenID.KDIM:              return KDim();
                    case TokenID.KDIR:              return KDir();
                    case TokenID.KEND:              return KEnd();
                    case TokenID.KEXEC:             return KExec();
                    case TokenID.KEXIT:             return KExit();
                    case TokenID.KEXPORT:           return KExport();
                    case TokenID.KFOR:              return KFor();
                    case TokenID.KFUNC:             return KFunc();
                    case TokenID.KGOTO:             return KGoto();
                    case TokenID.KIF:               return KIf();
                    case TokenID.KIMPORT:           return KImport();
                    case TokenID.KINPUT:            return KInput();
                    case TokenID.KLABEL:            return KLabel();
                    case TokenID.KLET:              return KAssignment();
                    case TokenID.KLOOP:             return KLoop();
                    case TokenID.KMODULE:           return KModule();
                    case TokenID.KOPEN:             return KOpen();
                    case TokenID.KNULL:             return null;
                    case TokenID.KPAGE:             return KPage();
                    case TokenID.KPRINT:            return KPrint();
                    case TokenID.KPROC:             return KProc();
                    case TokenID.KRANDOMIZE:        return KRandomize();
                    case TokenID.KREAD:             return KRead();
                    case TokenID.KREPEAT:           return KRepeat();
                    case TokenID.KRESTORE:          return KRestore();
                    case TokenID.KRETURN:           return KReturn();
                    case TokenID.KREPORT:           return KReport();
                    case TokenID.KSTOP:             return KStop();
                    case TokenID.KTRAP:             return KTrap();
                    case TokenID.KWHILE:            return KWhile();
                    case TokenID.KWRITE:            return KWrite();
                    case TokenID.KZONE:             return KZone();
                    case TokenID.EOL:               return null;

                    case TokenID.IDENT:
                        _currentLine.PushToken(token);
                        return KAssignment();
                }

                // Anything else is unparseable, so assume the rest of the line is
                // too and skip it.
                Messages.Error(MessageCode.UNEXPECTEDTOKEN, _currentLineNumber,
                    $"Unexpected {token} found in statement");
                SkipToEndOfLine();
            }
            return null;
        }

        // Returns the block state to which the specified token belongs.
        private BlockState TokenToState(SimpleToken token) {
            BlockState state = BlockState.NONE;
            switch (token.ID) {
                case TokenID.KMODULE:
                case TokenID.KEXPORT:
                    state = BlockState.PROGRAM;
                    break;

                case TokenID.IDENT:
                case TokenID.KCASE:
                case TokenID.KCLOSE:
                case TokenID.KCOLOUR:
                case TokenID.KCREATE:
                case TokenID.KCURSOR:
                case TokenID.KDATA:
                case TokenID.KDELETE:
                case TokenID.KDIM:
                case TokenID.KDIR:
                case TokenID.KEND:
                case TokenID.EOL:
                case TokenID.KEXEC:
                case TokenID.KEXIT:
                case TokenID.KFOR:
                case TokenID.KGOTO:
                case TokenID.KIF:
                case TokenID.KIMPORT:
                case TokenID.KINPUT:
                case TokenID.KLET:
                case TokenID.KLOOP:
                case TokenID.KNULL:
                case TokenID.KOPEN:
                case TokenID.KPAGE:
                case TokenID.KPRINT:
                case TokenID.KRANDOMIZE:
                case TokenID.KREAD:
                case TokenID.KREPEAT:
                case TokenID.KRETURN:
                case TokenID.KREPORT:
                case TokenID.KRESTORE:
                case TokenID.KSTOP:
                case TokenID.KTRAP:
                case TokenID.KWHILE:
                case TokenID.KWRITE:
                case TokenID.KZONE:
                    state = BlockState.STATEMENT;
                    break;

                case TokenID.KFUNC:
                case TokenID.KPROC:
                    state = BlockState.SUBFUNC;
                    break;

                default:
                    Debug.Assert(false, $"Unhandled token {token.ID} in TokenToState");
                    break;
            }
            return state;
        }

        // Generate a call to a subroutine given the specified identifier
        private ParseNode ExecWithIdentifier(IdentifierToken identToken) {
            Symbol sym = GetSymbolForCurrentScope(identToken.Name);
            if (sym == null) {
                Messages.Error(MessageCode.METHODNOTFOUND, $"{identToken.Name} not found");
                SkipToEndOfLine();
                return null;
            }

            // If this is an EXTERNAL symbol, create an ExtCall node for it
            ParseNode node;
            ParametersParseNode parameters = new();
            if (sym.Modifier == SymModifier.EXTERNAL) {
                node = new ExtCallParseNode {
                    Parameters = parameters,
                    LibraryName = sym.ExternalLibrary,
                    Name = sym.Name
                };
            } else {
                node = new CallParseNode {
                    ProcName = new IdentifierParseNode(sym),
                    Parameters = parameters
                };
            }

            sym.IsReferenced = true;

            int declParameterIndex = 0;

            if (!_currentLine.IsAtEndOfStatement) {
                ExpectToken(TokenID.LPAREN);
                if (_currentLine.PeekToken().ID != TokenID.RPAREN) {
                    SimpleToken token;
                    do {
                        if (declParameterIndex == sym.Parameters.Count) {
                            break;
                        }
                        ParseNode exprNode = Expression();
                        if (exprNode != null) {
                            Symbol symParameter = sym.Parameters[declParameterIndex];
                            bool valid = ValidateAssignmentTypes(symParameter.Type, exprNode.Type);
                            if (!valid) {
                                Messages.Error(MessageCode.TYPEMISMATCH, "Type mismatch in assignment");
                            }

                            // For string types, always enforce fixed char over char for parameters.
                            if (symParameter.Type == SymType.FIXEDCHAR) {
                                exprNode.Type = SymType.FIXEDCHAR;
                            }
                            parameters.Add(exprNode, symParameter.IsByRef);
                            declParameterIndex++;
                        }
                        token = _currentLine.PeekToken();
                        if (token.ID == TokenID.RPAREN) {
                            break;
                        }
                        ExpectToken(TokenID.COMMA);
                    } while (token.ID != TokenID.EOL);
                }
                ExpectToken(TokenID.RPAREN);
            }

            if (declParameterIndex != sym.Parameters.Count) {
                Messages.Error(MessageCode.PARAMETERCOUNTMISMATCH, "Parameter count mismatch");
            }
            return node;
        }

        // Validate an assignment of the exprNode to the specified identNode.
        private bool ValidateAssignment(IdentifierParseNode identNode, ParseNode exprNode) {
            return ValidateAssignmentTypes(identNode.Type, exprNode.Type);
        }

        // Verify that the type on the right hand side of an assignment can be
        // assigned to the left hand side.
        private bool ValidateAssignmentTypes(SymType toType, SymType fromType) {
            bool valid = false;
            
            switch (toType) {
                case SymType.CHAR:
                case SymType.FIXEDCHAR:
                    valid = Symbol.IsCharType(fromType);
                    break;
                    
                case SymType.INTEGER:
                case SymType.FLOAT:
                    valid = Symbol.IsNumberType(fromType);
                    break;
            }
            return valid;
        }

        // Recursive validation of a block and all sub-blocks. Currently the only
        // validation done at this point is for GO TO to verify we're not jumping
        // into the middle of an inner block.
        private void ValidateBlock(int level, CollectionParseNode blockNodes) {
            string filename = null;
            int line = 0;

            foreach (ParseNode node in blockNodes.Nodes) {
                switch (node.ID) {
                    case ParseID.COND: {
                        // Block IF statement. One sub-block for each
                        // IF...ELSE...ENDIF group.
                        ConditionalParseNode tokenNode = (ConditionalParseNode)node;
                        for (int m = 1; m < tokenNode.BodyList.Count; m += 2) {
                            if (tokenNode.BodyList[m] != null) {
                                ValidateBlock(level + 1, tokenNode.BodyList[m]);
                            }
                        }
                        break;
                    }

                    case ParseID.FILENAME: {
                        MarkFilenameParseNode tokenNode = (MarkFilenameParseNode)node;
                        filename = tokenNode.Filename;
                        break;
                    }

                    case ParseID.LINENUMBER: {
                        MarkLineParseNode tokenNode = (MarkLineParseNode)node;
                        line = tokenNode.LineNumber;
                        break;
                    }

                    case ParseID.LOOP:
                        ValidateBlock(level + 1, ((LoopParseNode)node).LoopBody);
                        break;

                    case ParseID.GOTO: {
                        GotoParseNode tokenNode = (GotoParseNode)node;
                        if (tokenNode.Nodes.Count > 0) {
                            SymbolParseNode symNode = (SymbolParseNode)tokenNode.Nodes[0];
                            Symbol sym = symNode.Symbol;
                            if (sym.Depth > level) {
                                    Messages.Error(filename,
                                                   MessageCode.GOTOINTOBLOCK,
                                                   line,
                                                   "GOTO into an inner block");
                            }
                        }
                        break;
                    }
                }
            }
        }
    }
}