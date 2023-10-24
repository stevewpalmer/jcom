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

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Xml;
using CCompiler;
using JComLib;

namespace JComal;

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

    private readonly ComalOptions _opts;
    private readonly string _entryPointName;
    private readonly ProgramParseNode _program;

    private BlockState _state;
    private Lines _ls;
    private SymbolCollection _importSymbols;
    private bool _hasReturn;
    private bool _hasProgram;
    private Line _currentLine;
    private LoopParseNode _currentLoop;
    private int _currentLineNumber;
    private int _blockDepth;

    /// <summary>
    /// Return or set the list of compiler messages.
    /// </summary>
    public MessageCollection Messages { get; init; }

    /// <summary>
    /// Symbol table stack
    /// </summary>
    private SymbolStack SymbolStack { get; }

    /// <summary>
    /// Global methods symbol table
    /// </summary>
    private SymbolCollection Globals { get; }

    /// <summary>
    /// Current procedure being parsed
    /// </summary>
    private ProcedureParseNode CurrentProcedure { get; set; }

    /// <summary>
    /// Constructs a compiler object with the given options.
    /// </summary>
    /// <param name="opts">Compiler options</param>
    public Compiler(ComalOptions opts) {

        SymbolStack = new SymbolStack();
        Globals = new SymbolCollection("Globals");
        SymbolStack.Push(Globals);

        _importSymbols = null;

        string moduleName = Path.GetFileNameWithoutExtension(opts.OutputFile);
        if (string.IsNullOrEmpty(moduleName)) {
            moduleName = "Class";
        }

        // Top level parse node that defines the program
        _program = new ProgramParseNode(opts) {
            Globals = Globals,
            Root = new BlockParseNode(),
            GenerateDebug = opts.GenerateDebug,
            VersionString = opts.VersionString,
            OutputFile = opts.OutputFile,
            IsExecutable = true,
            Name = moduleName
        };

        Messages = new MessageCollection(opts) {
            Interactive = opts.Interactive
        };
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
        }
        else {
            LineTokeniser tokeniser = new();

            using StreamReader sr = new(filename);
            int lineNumber = 10;
            bool lineNumberPatching = false;
            while (sr.Peek() != -1) {
                string sourceLine = sr.ReadLine();
                if (!string.IsNullOrWhiteSpace(sourceLine)) {
                    Line line = new(tokeniser.TokeniseLine(sourceLine));
                    if (line.LineNumber == 0) {
                        line.LineNumber = lineNumber;
                        lineNumber += 10;
                        if (!lineNumberPatching) {
                            lineNumberPatching = true;
                            Messages.Warning(MessageCode.LINENUMBERPATCHING, 2,
                                $"Source file is missing line numbers. Line numbers will be assumed starting from 10.");
                        }
                    }
                    else if (lineNumberPatching) {

                        // If we have to patch line numbers in then we need to update
                        // any existing numbers to ensure the sequence is contiguous. Source
                        // files with just partial line numbers will elicit a warning.
                        line.LineNumber = lineNumber;
                        lineNumber += 10;
                    }
                    lines.Add(line);
                }
            }
        }
        CompileLines(filename, lines);
    }

    /// <summary>
    /// Compile an string of source lines, optionally adding line numbers at the start.
    /// </summary>
    /// <param name="source">A string with source lines delimited by newlines</param>
    /// <param name="autoNumber">Specifies whether to add line numbers</param>
    public void CompileString(string source, bool autoNumber) {

        // Split the source
        string[] sourceLines = source.Split('\n');
        Lines lines = new();
        int lineNumber = 100;
        LineTokeniser tokeniser = new();

        // Auto-number the lines.
        foreach (string t in sourceLines) {
            string sourceLine = t;
            if (!string.IsNullOrWhiteSpace(sourceLine)) {
                if (autoNumber) {
                    sourceLine = $"{lineNumber} {sourceLine.Trim()}";
                    lineNumber += 10;
                }
                Line line = new(tokeniser.TokeniseLine(sourceLine));
                lines.Add(line);
            }
        }
        CompileLines(null, lines);
    }

    /// <summary>
    /// Compile an array of source lines.
    /// </summary>
    /// <param name="lines">A Lines object representing the source file</param>
    public void CompileLines(Lines lines) {
        CompileLines(null, lines);
    }

    /// <summary>
    /// Compile a collection of lines into the given method.
    /// </summary>
    /// <param name="methodName">Method name</param>
    /// <param name="lines">A Lines object representing the method body</param>
    public void CompileMethod(string methodName, Lines lines) {

        // Find an existing instance of this method
        ProcedureParseNode activeMethod = null;
        foreach (ParseNode node in _program.Root.Nodes) {
            if (node is ProcedureParseNode procNode && procNode.ProcedureSymbol.Name == methodName) {
                activeMethod = procNode;
                break;
            }
        }

        // If the method already exists, clear the body of the method.
        // Otherwise create this method and add it to the root of the
        // program.
        if (activeMethod != null) {
            activeMethod.Body.Clear();
        }
        else {
            Symbol method = Globals.Add(methodName, new SymFullType(), SymClass.SUBROUTINE, null, 0);
            method.Defined = true;

            SymbolCollection localSymbols = new("Local");
            activeMethod = new ProcedureParseNode {
                ProcedureSymbol = method
            };
            activeMethod.Symbols.Add(localSymbols);
            _program.Root.Nodes.Add(activeMethod);
        }

        // Now compile the lines into the method body.
        CurrentProcedure = activeMethod;
        _currentLineNumber = 0;
        _ls = lines;

        CompileBlock(activeMethod.Body, new[] { TokenID.ENDOFFILE });
    }

    /// <summary>
    /// Convert the parse tree to executable code then save it to the
    /// filename specified in the options.
    /// </summary>
    public void Save() {
        try {
            _program.IsExecutable = _hasProgram;
            _program.Generate();
            _program.Save();
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
    public void Execute() {
        Execute(_entryPointName);
    }

    /// <summary>
    /// Convert the parse tree to executable code and then execute the
    /// resulting code. The return value from the specified entry point function
    /// is returned as an object.
    /// </summary>
    /// <param name="entryPointName">The name of the method to be called</param>
    /// <returns>An ExecutionResult object representing the result of the execution</returns>
    public ExecutionResult Execute(string entryPointName) {
        _program.IsExecutable = _hasProgram;
        _program.Generate();
        return _program.Run(entryPointName);
    }

    // Compile an array of source lines.
    private void CompileLines(string filename, Lines lines) {
        try {

            _ls = lines;
            _blockDepth = 0;
            _currentLineNumber = 0;
            _state = BlockState.NONE;

            // Mark this file.
            if (filename != null) {
                Messages.Filename = filename;
                _program.Root.Add(MarkFilename());
            }

            CreateDefaultGlobals();

            // Pre-scan to locate all PROC/FUNCs so we have their declaration
            // ahead of their usage
            Pass0();

            // Compile everything to the end of the file.
            CompileBlock(_program.Root, new[] { TokenID.ENDOFFILE });

            // Warn about exported methods that are not defined
            foreach (Symbol sym in Globals) {
                if (sym.IsExported && !sym.Defined) {
                    Messages.Warning(MessageCode.MISSINGEXPORT, 1,
                        $"{sym.Name} marked as EXPORT but not found in source file");
                }
            }

            // Dump file?
            if (_opts.Dump) {
                XmlDocument xmlTree = ParseTreeXml.Tree(_program);
                string outputFilename = Path.GetFileName(_opts.OutputFile);
                outputFilename = Path.ChangeExtension(outputFilename, ".xml");
                xmlTree.Save(outputFilename);
            }
        }
        catch (Exception e) {
            if (_opts.DevMode || _opts.Interactive) {
                throw;
            }
            Messages.Error(MessageCode.COMPILERFAILURE, $"Compiler error: {e.Message}");
        }
    }

    // Create default globals
    private void CreateDefaultGlobals() {

        // Create special variables
        Globals.Add(new Symbol(Consts.ErrName, new SymFullType(SymType.INTEGER), SymClass.VAR, null, 0) {
            Modifier = SymModifier.STATIC | SymModifier.HIDDEN,
            Value = new Variant(0)
        });
        Globals.Add(new Symbol(Consts.ErrText, new SymFullType(SymType.CHAR), SymClass.VAR, null, 0) {
            Modifier = SymModifier.STATIC | SymModifier.HIDDEN,
            Value = new Variant(string.Empty)
        });
    }

    // Do an initial pass-0 scan of all lines looking for PROC and FUNC tokens,
    // parsing their name, type and any parameters and enter those into the global
    // symbol table. This ensures that references to them ahead of time resolve
    // to the correct type, particularly for functions.
    private void Pass0() {

        Stack<Symbol> parents = new(new Symbol[] { null });

        // Add implicit entrypoint subroutine name.
        Globals.Add(new Symbol(_entryPointName, new SymFullType(), SymClass.SUBROUTINE, null, 0) {
            Modifier = SymModifier.HIDDEN
        });
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
            if (token.ID is TokenID.KENDPROC or TokenID.KENDFUNC) {
                parents.Pop();
            }
            if (token.ID is TokenID.KPROC or TokenID.KFUNC) {
                SymClass klass = token.ID == TokenID.KPROC ? SymClass.SUBROUTINE : SymClass.FUNCTION;
                IdentifierToken identToken = ParseIdentifier();
                if (identToken != null) {

                    // Check method name hasn't already been declared.
                    string methodName = identToken.Name;
                    Symbol method = Globals.Get(methodName);
                    if (method is { Defined: true, IsExternal: false }) {
                        Messages.Error(MessageCode.SUBFUNCDEFINED, $"{methodName} already defined");
                        parents.Push(method);
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
                    method ??= Globals.Add(methodName, new SymFullType(), klass, null, lineNumber);
                    if (klass == SymClass.FUNCTION) {
                        method.FullType = GetTypeFromName(methodName);
                    }

                    if (TestToken(TokenID.KEXTERNAL)) {
                        method.Modifier = SymModifier.EXTERNAL;
                        method.ExternalLibrary = ParseStringLiteral();
                    }
                    else {
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

    // Add all symbols from Globals to the symbols class whose parent
    // is the specified symbol.
    private void AddChildSymbols(SymbolCollection symbols, Symbol parentSymbol) {

        foreach (Symbol childSymbol in Globals) {
            if (childSymbol.IsMethod && childSymbol.Parent == parentSymbol) {
                symbols.Add(childSymbol);
                AddChildSymbols(symbols, childSymbol);
            }
        }
    }

    // Compile a block within a function or procedure, or within a structured statement
    // such as WHILE, REPEAT or CASE/WHEN. The endTokens list specify tokens that can
    // end the block.
    private TokenID CompileBlock(BlockParseNode node, TokenID[] endTokens) {

        SimpleToken token = new(TokenID.EOL);
        while (!_ls.EndOfFile) {

            _currentLine = _ls.NextLine;
            token = GetNextToken();

            // Possible initial line number
            if (token.ID == TokenID.INTEGER) {
                IntegerToken lineNumberToken = token as IntegerToken;
                _currentLineNumber = _opts.IDE ? _ls.Index : lineNumberToken.Value;
                Messages.Linenumber = _currentLineNumber;
                token = GetNextToken();
            }

            if (token.ID != TokenID.EOL) {
                if (Array.IndexOf(endTokens, token.ID) >= 0) {
                    return token.ID;
                }
                if (_state != BlockState.NONE) {
                    node.Add(MarkLine());
                }
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
    private void CompileLine(SimpleToken token, BlockParseNode statements) {

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
        return new MarkFilenameParseNode { Filename = Messages.Filename };
    }

    // Create a token node that marks the number of the current line being
    // compiled. The code generator uses this in conjunction with the
    // filename to refer to the location in the source file if any errors
    // are found during generation.
    private ParseNode MarkLine() {
        return new MarkLineParseNode {
            LineNumber = _ls.Index,
            DisplayableLineNumber = _currentLineNumber
        };
    }

    // Retrieve the next token for the current line and check for any
    // errors
    private SimpleToken GetNextToken() {

        SimpleToken token = _currentLine.GetToken();
        while (token is ErrorToken errorToken) {
            Messages?.Error(MessageCode.EXPECTEDTOKEN, errorToken.Message);
            token = _currentLine.GetToken();
        }
        return token;
    }

    // Consult the symbol table for the current scope for the given label. Labels
    // cannot have scope other than the current block.
    private Symbol GetLabel(string label) {
        return SymbolStack.Top.Get(label);
    }

    // Create an entry in the symbol table for the specified label.
    private Symbol GetMakeLabel(string label, bool isDeclaration) {
        Symbol sym = GetLabel(label);
        if (sym == null) {
            sym = SymbolStack.Top.Add(label,
                new SymFullType(SymType.LABEL),
                SymClass.LABEL,
                null,
                _currentLineNumber);
            sym.Defined = isDeclaration;
        }
        else if (isDeclaration && sym.Defined) {
            Messages.Error(MessageCode.LABELALREADYDECLARED, $"Label {label} already declared");
        }
        else {
            sym.Defined = isDeclaration || sym.Defined;
        }
        if (isDeclaration) {
            sym.Depth = _blockDepth;
        }
        else {
            sym.IsReferenced = true;
        }
        return sym;
    }

    // Make sure we have an _EOD symbol, and create one otherwise.
    private Symbol GetMakeEODSymbol() {

        Symbol symbol = Globals.Get(Consts.EODName);
        if (symbol == null) {
            symbol = new Symbol(Consts.EODName, new SymFullType(SymType.INTEGER), SymClass.VAR, null, 0) {
                Modifier = SymModifier.STATIC,
                Value = new Variant(1)
            };
            Globals.Add(symbol);
        }
        return symbol;
    }

    // Make sure we have a _DATAINDEX for the READ statement, and create one otherwise.
    private Symbol GetMakeReadDataIndexSymbol() {

        Symbol symbol = Globals.Get(Consts.DataIndexName);
        if (symbol == null) {
            symbol = new Symbol(Consts.DataIndexName, new SymFullType(SymType.INTEGER), SymClass.VAR, null, 0) {
                Modifier = SymModifier.STATIC,
                Value = new Variant(0),
                IsReferenced = true
            };
            Globals.Add(symbol);
        }
        return symbol;
    }

    // Make sure we have a _DATA for the READ statement, and create one otherwise.
    private Symbol GetMakeReadDataSymbol() {

        Symbol symbol = Globals.Get(Consts.DataName);
        if (symbol == null) {
            symbol = new Symbol(Consts.DataName, new SymFullType(SymType.GENERIC), SymClass.VAR, null, 0) {
                Modifier = SymModifier.STATIC | SymModifier.FLATARRAY,
                IsReferenced = true
            };
            Globals.Add(symbol);
        }
        return symbol;
    }

    // Look up the symbol table for the specified identifier starting with the
    // current scope and working up to and including global/imports. If we're
    // in a closed procedure, the global symbol tables are ignored and _importSymbols
    // is used so that anything other than predefined
    private Symbol GetSymbolForCurrentScope(string name) {

        Symbol sym = null;
        foreach (SymbolCollection symbols in SymbolStack.All) {
            if ((symbols == Globals || symbols == Globals) && _importSymbols != null) {
                sym = _importSymbols.Get(name);
            }
            else {
                sym = symbols.Get(name);
            }
            if (sym != null) {
                break;
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

    // Make a symbol for the current scope and initialise it. If we're in the main
    // program, all symbols go into the global symbol table.
    private Symbol MakeSymbolForCurrentScope(string name) {

        Symbol sym;

        if (!CurrentProcedure.IsClosed) {
            sym = MakeSymbolForScope(name, Globals);
            sym.Modifier |= SymModifier.STATIC;
        }
        else {
            sym = MakeSymbolForScope(name, SymbolStack.Top);
        }
        return sym;
    }

    // Make a symbol for the current scope and initialise it. If we're in the main
    // program, all symbols go into the global symbol table.
    private Symbol MakeSymbolForScope(string name, SymbolCollection symbols) {

        SymFullType symType = GetTypeFromName(name);
        Symbol sym = symbols.Add(name, symType, SymClass.VAR, null, _currentLineNumber);
        sym.Defined = true;
        InitialiseToDefault(sym);
        return sym;
    }

    // Initialise fixed char strings to empty. We don't attempt to initialise
    // other types as MSIL will do this automatically for us.
    private void InitialiseToDefault(Symbol sym) {

        if (sym is { IsArray: false, Type: SymType.FIXEDCHAR }) {
            sym.Value = new Variant(string.Empty);
            if (!_opts.Strict) {
                sym.FullType.Width = Consts.DefaultStringWidth;
            }
        }
    }

    // Determine the type of a variable from its name. Integer variables end
    // with Consts.IntegerChar, string variable with Consts.StringChar.
    // Anything else is a floating point.
    private static SymFullType GetTypeFromName(string name) {
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

        return new ExtCallParseNode("JComalLib.Intrinsics,comallib", functionName) {
            Inline = _opts.Inline
        };
    }

    // Create an ExtCallParseNode for the specified function in the FileManager
    // class with the inline flag set from the options.
    private static ExtCallParseNode GetFileManagerExtCallNode(string functionName) {

        return new ExtCallParseNode("JComalLib.FileManager,comallib", functionName);
    }

    // Create an ExtCallParseNode for the specified function in the Runtime
    // class with the inline flag set from the options.
    private static ExtCallParseNode GetRuntimeExtCallNode(string functionName) {

        return new ExtCallParseNode("JComLib.Runtime,comlib", functionName);
    }

    // Ensure that the next token in the input is the one expected and report an error otherwise.
    private SimpleToken ExpectToken(TokenID expectedID) {
        SimpleToken token = GetNextToken();
        if (token.ID != expectedID) {
            Messages.Error(MessageCode.EXPECTEDTOKEN,
                $"Expected '{Tokens.TokenIDToString(expectedID)}' not '{Tokens.TokenIDToString(token.ID)}'");
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
    private void InsertTokenIfMissing(TokenID expectedID) {
        SimpleToken token = _currentLine.PeekToken();
        if (token.ID != expectedID) {

            if (_opts.Strict) {
                Messages.Error(MessageCode.EXPECTEDTOKEN,
                    $"Expected '{Tokens.TokenIDToString(expectedID)}' not '{Tokens.TokenIDToString(token.ID)}'");
            }
            _currentLine.InsertTokens(new[] {
                new SimpleToken(TokenID.SPACE),
                new SimpleToken(expectedID)
            });
            GetNextToken();
        }
        GetNextToken();
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
            if (CurrentProcedure == null) {
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
                case TokenID.KCASE:
                    return KCase();
                case TokenID.KCLOSE:
                    return KClose();
                case TokenID.KCOLOUR:
                    return KColour();
                case TokenID.KCREATE:
                    return KCreate();
                case TokenID.KCURSOR:
                    return KCursor();
                case TokenID.KDATA:
                    return KData();
                case TokenID.KDELETE:
                    return KDelete();
                case TokenID.KDIM:
                    return KDim();
                case TokenID.KDIR:
                    return KDir();
                case TokenID.KEND:
                    return KEnd();
                case TokenID.KEXEC:
                    return KExec();
                case TokenID.KEXIT:
                    return KExit();
                case TokenID.KEXPORT:
                    return KExport();
                case TokenID.KFOR:
                    return KFor();
                case TokenID.KFUNC:
                    return KFunc();
                case TokenID.KGOTO:
                    return KGoto();
                case TokenID.KIF:
                    return KIf();
                case TokenID.KIMPORT:
                    return KImport();
                case TokenID.KINPUT:
                    return KInput();
                case TokenID.KLABEL:
                    return KLabel();
                case TokenID.KLET:
                    return KAssignment();
                case TokenID.KLOOP:
                    return KLoop();
                case TokenID.KMODULE:
                    return KModule();
                case TokenID.KOPEN:
                    return KOpen();
                case TokenID.KNULL:
                    return null;
                case TokenID.KPAGE:
                    return KPage();
                case TokenID.KPRINT:
                    return KPrint();
                case TokenID.KPROC:
                    return KProc();
                case TokenID.KRANDOMIZE:
                    return KRandomize();
                case TokenID.KREAD:
                    return KRead();
                case TokenID.KREPEAT:
                    return KRepeat();
                case TokenID.KRESTORE:
                    return KRestore();
                case TokenID.KRETURN:
                    return KReturn();
                case TokenID.KREPORT:
                    return KReport();
                case TokenID.KSTOP:
                    return KStop();
                case TokenID.KTRAP:
                    return KTrap();
                case TokenID.KUSE:
                    return KUse();
                case TokenID.KWHILE:
                    return KWhile();
                case TokenID.KWRITE:
                    return KWrite();
                case TokenID.KZONE:
                    return KZone();
                case TokenID.EOL:
                    return null;

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
    private static BlockState TokenToState(SimpleToken token) {
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
            case TokenID.KELSE:
            case TokenID.KENDIF:
            case TokenID.KENDFUNC:
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
            case TokenID.KUSE:
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
        if (sym.Modifier.HasFlag(SymModifier.EXTERNAL)) {
            node = new ExtCallParseNode {
                Parameters = parameters,
                LibraryName = sym.ExternalLibrary,
                Name = sym.Name
            };
        }
        else {
            node = new CallParseNode {
                ProcName = new IdentifierParseNode(sym),
                Parameters = parameters
            };
        }

        sym.IsReferenced = true;
        if (sym.Class == SymClass.FUNCTION) {
            node.Type = sym.Type;
        }

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

                        // If parameter is REF, exprNode must be an identifier
                        if (symParameter.IsByRef && exprNode is not IdentifierParseNode) {
                            Messages.Error(MessageCode.REFMISMATCH, "REF requires an identifier");
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
    private static bool ValidateAssignment(IdentifierParseNode identNode, ParseNode exprNode) {
        if (identNode.IsArrayBase) {
            if (exprNode is not IdentifierParseNode { IsArrayBase: true }) {
                return false;
            }
        }
        return ValidateAssignmentTypes(identNode.Type, exprNode.Type);
    }

    // Verify that the type on the right hand side of an assignment can be
    // assigned to the left hand side.
    private static bool ValidateAssignmentTypes(SymType toType, SymType fromType) {
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
    private void ValidateBlock(int level, BlockParseNode blockNodes) {
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
                    line = tokenNode.DisplayableLineNumber;
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