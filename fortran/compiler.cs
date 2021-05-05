// JFortran Compiler
// Main compiler class
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
using System.IO;
using System.Xml;
using CCompiler;

namespace JFortran {

    /// <summary>
    /// Main Fortran compiler class.
    /// </summary>
    public partial class Compiler : ICompiler {

        // Used to keep track of the block state
        private enum BlockState {
            NONE,
            PROGRAM,
            SUBFUNC,
            IMPLICITNONE,
            IMPLICIT,
            SPECIFICATION,
            STATEMENT,
            UNORDERED // Unordered must always be last
        }

        private readonly FortranOptions _opts;
        private readonly CollectionParseNode _ptree;
        private readonly FortranSymbolCollection _globalSymbols;

        private BlockState _state;
        private Lexer _ls;
        private ProgramDefinition _programDef;
        private FortranSymbolCollection _localSymbols;
        private FortranSymbolCollection _stfSymbols;
        private ProcedureParseNode _currentProcedure;
        private CollectionParseNode _initList;
        private string _entryPointName;
        private bool _hasReturn;
        private bool _hasProgram;
        private bool _parsingIf;
        private int _blockDepth;
        private bool _saveAll;

        /// <summary>
        /// Constructs a Fortran compiler object with the given options.
        /// </summary>
        /// <param name="opts">Compiler options</param>
        public Compiler(FortranOptions opts) {
            _globalSymbols = new FortranSymbolCollection("Global"); // Functions and Subroutines
            _localSymbols = new FortranSymbolCollection("Local"); // Everything else including labels
            _ptree = new CollectionParseNode();
            Messages = new MessageCollection(opts);
            _entryPointName = "Main";
            _opts = opts;
        }

        /// <summary>
        /// Compile one file.
        /// An exception is thrown if the file doesn't exist.
        /// </summary>
        /// <param name="filename">The full path and file name to be compiled</param>
        public void Compile(string filename) {
            List<string> lines = new();
            using (StreamReader sr = new(filename)) {
                while (sr.Peek() != -1) {
                    string line = sr.ReadLine();
                    lines.Add(line);
                }
            }
            CompileString(filename, lines.ToArray());
        }

        /// <summary>
        /// Compile an array of source lines.
        /// This function exists primarily for unit tests.
        /// </summary>
        /// <param name="lines">An array of strings representing the source file</param>
        public void CompileString(string[] lines) {
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
            } catch (CodeGeneratorException e) {
                Messages.Error(e.Filename, MessageCode.CODEGEN, e.Linenumber, e.Message);
            }
        }

        /// <summary>
        /// Sets the name of the entry point function. This defaults to "Main" but
        /// can be overridden by the caller.
        /// </summary>
        /// <param name="newEntryPointName">New entry point name.</param>
        public void SetEntryPointName(string newEntryPointName) {
            _entryPointName = newEntryPointName;
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

        /// <summary>
        /// Return or set the list of compiler messages.
        /// </summary>
        public MessageCollection Messages { get; set; }

        // Compile an array of source lines.
        private void CompileString(string filename, string[] lines) {
            try {
                _parsingIf = false;
                _blockDepth = 0;
                _state = BlockState.NONE;
                
                // Create the top-level program node.
                if (_programDef == null) {
                    string moduleName = Path.GetFileNameWithoutExtension(_opts.OutputFile);
                    if (string.IsNullOrEmpty(moduleName)) {
                        moduleName = "Class";
                    }
                    _programDef = new ProgramDefinition {
                        Name = moduleName,
                        Globals = _globalSymbols,
                        IsExecutable = true,
                        Root = _ptree
                    };
                }

                CompileUnit(filename, lines);

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
        private void CompileUnit(string filename, string [] lines) {
            _ls = new Lexer(lines, _opts, Messages);
            Messages.Filename = filename;

            // Mark this file.
            _ptree.Add(MarkFilename());
            
            // Loop and parse one single line at a time.
            SimpleToken token = _ls.GetKeyword();
            while (token.ID != TokenID.ENDOFFILE) {
                if (token.ID != TokenID.EOL) {
                    ParseNode labelNode = CheckLabel();
                    if (labelNode != null) {
                        _ptree.Add(labelNode);
                    }
                    ParseNode node = Statement(token);
                    if (node != null) {
                        _ptree.Add(MarkLine());
                        _ptree.Add(node);
                    }
                    ExpectEndOfLine();
                }
                token = _ls.GetKeyword();
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
            return new MarkLineParseNode {LineNumber = _ls.LineNumber};
        }

        // Mark in the PROGRAM node whether or not this program is executable.
        // An executable program is one which has a PROGRAM statement.
        private void MarkExecutable() {
            if (_programDef != null) {
                _programDef.IsExecutable = _hasProgram;
            }
        }

        /// Ensure no label was specified for this line.
        private void EnsureNoLabel() {
            if (_ls.HasLabel) {
                Messages.Error(MessageCode.LABELNOTALLOWED, "Label not allowed here");
            }
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
                sym = _localSymbols.Add(label, new SymFullType(SymType.LABEL), SymClass.LABEL, null, _ls.LineNumber);
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

        // Look up the symbol table for the specified identifier starting with the
        // current scope and working up to and including global.
        private Symbol GetSymbolForCurrentScope(string name) {
            Symbol sym = null;

            if (_stfSymbols != null) {
                sym = _stfSymbols.Get(name);
            }
            if (sym == null) {
                sym = _localSymbols.Get(name);
                if (sym == null) {
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
                sym = _localSymbols.Get(name);
                if (sym == null) {
                    sym = _localSymbols.Add(name, new SymFullType(SymType.NONE), SymClass.VAR, null, _ls.LineNumber);
                }
            }
            return sym;
        }

        // Ensure that the next token in the input is the one expected and report an error otherwise.
        private SimpleToken ExpectToken(TokenID expectedID) {
            SimpleToken token = _ls.GetToken();
            if (token.KeywordID != expectedID) {
                Messages.Error(MessageCode.EXPECTEDTOKEN,
                    $"Expected '{Tokens.TokenIDToString(expectedID)}' but saw '{Tokens.TokenIDToString(token.KeywordID)}' instead");
                _ls.BackToken();
                return null;
            }
            return token;
        }

        // Ensure that the next token in the input is an identifier and return the identifier token suitably
        // cast if so. This is just a cast wrapper around ExpectToken().
        private IdentifierToken ExpectIdentifierToken() {
            SimpleToken token = ExpectToken(TokenID.IDENT);
            if (token != null) {
                return (IdentifierToken)token;
            }
            return null;
        }

        // Eat the input to the end of the line. Useful when we hit a syntax error and want
        // to get to a clean state before continuing.
        private void SkipToEndOfLine() {
            SimpleToken token = _ls.GetToken();
            while (token.ID != TokenID.EOL && token.ID != TokenID.ENDOFFILE) {
                token = _ls.GetToken();
            }
            _ls.BackToken();
        }

        // Check that the next token is the end of the line and report an error otherwise.
        private void ExpectEndOfLine() {
            SimpleToken token = _ls.GetToken();
            if (token.ID != TokenID.EOL && token.ID != TokenID.ENDOFFILE) {
                Messages.Error(MessageCode.ENDOFSTATEMENT, "End of statement expected");
                SkipToEndOfLine();
            }
        }

        // Return whether we are at the end of the current line.
        private bool IsAtEndOfLine() {
            SimpleToken token = _ls.PeekToken();
            return token.ID == TokenID.EOL || token.ID == TokenID.ENDOFFILE;
        }

        // Check whether the next token is the one specified and skip it if so.
        private void SkipToken(TokenID id) {
            SimpleToken token = _ls.GetToken();
            if (token.ID != id) {
                _ls.BackToken();
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
            if (newState == BlockState.STATEMENT) {
                if (_currentProcedure == null) {
                    _ls.BackToken();
                    _ptree.Add(KCreateProgram(_entryPointName));
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
                    case TokenID.KASSIGN:           return KAssign();
                    case TokenID.KBACKSPACE:        return KBackspace();
                    case TokenID.KBLOCKDATA:        return KBlockData();
                    case TokenID.KCALL:             return KCall();
                    case TokenID.KCHARACTER:        return KCharacter();
                    case TokenID.KCLOSE:            return KClose();
                    case TokenID.KCOMMON:           return KCommon();
                    case TokenID.KCOMPLEX:          return KComplex();
                    case TokenID.KCONTINUE:         return KContinue();
                    case TokenID.KDATA:             return KData();
                    case TokenID.KDIMENSION:        return KDimension();
                    case TokenID.KDO:               return KDo();
                    case TokenID.KDPRECISION:       return KDouble();
                    case TokenID.KENDFILE:          return KEndFile();
                    case TokenID.KENTRY:            return KEntry();
                    case TokenID.KEQUIVALENCE:      return KEquivalence();
                    case TokenID.KEXTERNAL:         return KExternal();
                    case TokenID.KFORMAT:           return KFormat();
                    case TokenID.KFUNCTION:         return KFunction();
                    case TokenID.KGOTO:             return KGoto();
                    case TokenID.KIF:               return KIf();
                    case TokenID.KIMPLICIT:         return KImplicit();
                    case TokenID.KIMPLICITNONE:     return KImplicitNone();
                    case TokenID.KINCLUDE:          return KInclude();
                    case TokenID.KINQUIRE:          return KInquire();
                    case TokenID.KINTEGER:          return KInteger();
                    case TokenID.KINTRINSIC:        return KIntrinsic();
                    case TokenID.KLOGICAL:          return KLogical();
                    case TokenID.KOPEN:             return KOpen();
                    case TokenID.KPAUSE:            return KPause();
                    case TokenID.KPARAMETER:        return KParameter();
                    case TokenID.KPRINT:            return KPrint();
                    case TokenID.KPROGRAM:          return KProgram();
                    case TokenID.KREAD:             return KRead();
                    case TokenID.KREAL:             return KReal();
                    case TokenID.KRETURN:           return KReturn();
                    case TokenID.KREWIND:           return KRewind();
                    case TokenID.KSAVE:             return KSave();
                    case TokenID.KSTOP:             return KStop();
                    case TokenID.KSUBROUTINE:       return KSubroutine();
                    case TokenID.KWRITE:            return KWrite();
                    case TokenID.IDENT:             return KAssignment();
                }

                // Anything else is unparseable, so assume the rest of the line is
                // too and skip it.
                Messages.Error(MessageCode.UNEXPECTEDTOKEN, _ls.LineNumber, $"Unexpected {token} found in statement");
                SkipToEndOfLine();
            }
            return null;
        }

        // Check for a label on the current line and if found, add to the
        // symbol table and generate a parsenode for the current position.
        private ParseNode CheckLabel() {
            if (_ls.HasLabel) {
                Symbol sym = GetMakeLabel(_ls.Label, true);
                return new MarkLabelParseNode {Label = sym};
            }
            return null;
        }

        // Returns the block state to which the specified token belongs. For example,
        // IMPLICIT must precede any declaration which must precede any executable
        // statement in the same program group.
        private BlockState TokenToState(SimpleToken token) {
            BlockState state = BlockState.NONE;
            switch (token.ID) {
                case TokenID.KPROGRAM:
                    state = BlockState.PROGRAM;
                    break;

                case TokenID.KIMPLICITNONE:
                    state = BlockState.IMPLICITNONE;
                    break;
                    
                case TokenID.KIMPLICIT:
                    state = BlockState.IMPLICIT;
                    break;

                case TokenID.KFORMAT:
                case TokenID.KINCLUDE:
                    // May appear anywhere.
                    state = BlockState.UNORDERED;
                    break;

                case TokenID.KINTEGER:
                case TokenID.KREAL:
                case TokenID.KCHARACTER:
                case TokenID.KDIMENSION:
                case TokenID.KLOGICAL:
                case TokenID.KDPRECISION:
                case TokenID.KCOMPLEX:
                case TokenID.KCOMMON:
                case TokenID.KPARAMETER:
                case TokenID.KINTRINSIC:
                case TokenID.KEXTERNAL:
                case TokenID.KEQUIVALENCE:
                case TokenID.KSAVE:
                    state = BlockState.SPECIFICATION;
                    break;

                case TokenID.IDENT:
                case TokenID.KSTMTFUNC:
                case TokenID.KCALL:
                case TokenID.KCLOSE:
                case TokenID.KCONTINUE:
                case TokenID.KDO:
                case TokenID.KEND:
                case TokenID.KGO:
                case TokenID.KGOTO:
                case TokenID.KIF:
                case TokenID.KINQUIRE:
                case TokenID.KOPEN:
                case TokenID.KPAUSE:
                case TokenID.KPRINT:
                case TokenID.KREAD:
                case TokenID.KRETURN:
                case TokenID.KSTOP:
                case TokenID.KWRITE:
                case TokenID.KENDFILE:
                case TokenID.KREWIND:
                case TokenID.KBACKSPACE:
                case TokenID.KASSIGN:
                case TokenID.KDATA:
                case TokenID.KENTRY:
                    state = BlockState.STATEMENT;
                    break;

                case TokenID.KSUBROUTINE:
                case TokenID.KFUNCTION:
                case TokenID.KBLOCKDATA:
                    state = BlockState.SUBFUNC;
                    break;
            }
            return state;
        }

        // ASSIGN keyword.
        // This is a straight assignment of a label to an identifier. The
        // code generator will differentiate and construct the right code
        // to perform the assignment of the label's internal ID.
        private ParseNode KAssign() {
            AssignmentParseNode assignNode = new();
            SymbolParseNode label = ParseLabel();

            ExpectToken(TokenID.KTO);

            int index;
            for (index = 0; index < _currentProcedure.LabelList.Count; ++index) {
                SymbolParseNode thisLabel = (SymbolParseNode)_currentProcedure.LabelList[index];
                if (thisLabel.Symbol == label.Symbol) {
                    break;
                }
            }
            if (index == _currentProcedure.LabelList.Count) {
                _currentProcedure.LabelList.Add(label);
            }

            assignNode.Identifiers = new[] { ParseBasicIdentifier() };
            assignNode.ValueExpressions = new[] { new NumberParseNode(index) };
            return assignNode;
        }

        // ENTRY keyword
        // Not supported yet
        private ParseNode KEntry() {
            Messages.Error(MessageCode.NOTSUPPORTED, "ENTRY not supported");
            SkipToEndOfLine();
            return null;
        }

        // INCLUDE keyword
        // Insert the contents of the specified file at the current point
        // in the lexical analysis.
        private ParseNode KInclude() {
            EnsureNoLabel();
            string filename = ParseStringLiteral();
            string savedFilename = Messages.Filename;
            Lexer savedLexer = _ls;
            if (filename != null) {
                List<string> lines = new();
                try {
                    string fullPath = Path.Combine(Path.GetDirectoryName(Messages.Filename), filename);
                    using (StreamReader sr = new(fullPath)) {
                        while (sr.Peek() != -1) {
                            string line = sr.ReadLine();
                            lines.Add(line);
                        }
                    }
                    CompileUnit(filename, lines.ToArray());
                    _ls = savedLexer;
                    Messages.Filename = savedFilename;
                } catch (IOException e) {
                    _ls = savedLexer;
                    Messages.Error(MessageCode.INCLUDEERROR, $"INCLUDE error: {e.Message}");
                }
            }
            return null;
        }

        // CALL keyword
        // Call a subroutine/function with specified parameters
        private ParseNode KCall() {
            IdentifierToken identToken = ExpectIdentifierToken();
            if (identToken == null) {
                SkipToEndOfLine();
                return null;
            }

            Symbol sym = GetSymbolForCurrentScope(identToken.Name);
            if (sym == null) {
                sym = _globalSymbols.Add(identToken.Name, new SymFullType(SymType.NONE), SymClass.SUBROUTINE, null, _ls.LineNumber);
            }

            // If this was a parameter now being used as a function, change its
            // class and type.
            if (sym.Class != SymClass.SUBROUTINE) {
                sym.Class = SymClass.SUBROUTINE;
                sym.Defined = true;
                sym.Linkage = SymLinkage.BYVAL;
            }
            sym.IsReferenced = true;

            CallParseNode node = new();
            node.ProcName = new IdentifierParseNode(sym);
            node.Parameters = new ParametersParseNode();

            bool hasAlternateReturn = false;

            if (!IsAtEndOfLine()) {
                ExpectToken(TokenID.LPAREN);
                if (_ls.PeekToken().ID != TokenID.RPAREN) {
                    SimpleToken token;
                    do {
                        if (_ls.PeekToken().ID == TokenID.STAR) {
                            // Alternate return label.
                            SkipToken(TokenID.STAR);
                            token = ExpectToken(TokenID.INTEGER);
                            if (token != null) {
                                IntegerToken intToken = (IntegerToken)token;
                                Symbol altLabel = GetMakeLabel(intToken.Value.ToString(), false);
                                node.AlternateReturnLabels.Add(altLabel);
                                hasAlternateReturn = true;
                            }
                        } else {
                            if (hasAlternateReturn) {
                                Messages.Error(MessageCode.ALTRETURNORDER, "Alternate return labels must be at the end");
                            }
                            ParseNode exprNode = Expression();
                            if (exprNode != null) {
                                node.Parameters.Add(exprNode, true);
                            }
                        }
                        token = _ls.GetToken();
                    } while (token.ID == TokenID.COMMA);
                    _ls.BackToken();
                }
                ExpectToken(TokenID.RPAREN);
            }
            return node;
        }

        // IF keyword
        // Conditional evaluation
        private ParseNode KIf() {
            if (_parsingIf) {
                Messages.Error(MessageCode.IFNOTPERMITTED, "Cannot specify an IF statement here");
                return null;
            }

            ExpectToken(TokenID.LPAREN);
            ParseNode expr = Expression();
            ExpectToken(TokenID.RPAREN);

            // Look at the next token to see which of the three IF statements
            // we're parsing.
            SimpleToken token = _ls.GetKeyword();
            if (token.ID == TokenID.KTHEN) {
                ExpectEndOfLine();
                return KBlockIf(expr);
            }

            if (token.ID == TokenID.INTEGER) {
                _ls.BackToken();
                return KArithmeticIf(expr);
            }

            _ls.BackToken();
            return KLogicalIf(expr);
        }

        // Block IF
        private ParseNode KBlockIf(ParseNode expr) {
            ConditionalParseNode node = new();
            TokenID id;

            do {
                ParseNode labelNode;

                CollectionParseNode statements = new();

                SimpleToken token = _ls.GetKeyword();
                ++_blockDepth;
                while (!IsEndOfIfBlock(token.ID)) {
                    labelNode = CheckLabel();
                    if (labelNode != null) {
                        statements.Add(labelNode);
                    }
                    ParseNode subnode = Statement(token);
                    if (subnode != null) {
                        statements.Add(MarkLine());
                        statements.Add(subnode);
                    }
                    ExpectEndOfLine();
                    token = _ls.GetKeyword();
                }
                --_blockDepth;

                // Labels on terminators are valid so we need
                // to check for and add those.
                labelNode = CheckLabel();
                if (labelNode != null) {
                    statements.Add(labelNode);
                }
                node.Add(expr, statements);

                id = token.ID;
                if (id == TokenID.KELSEIF) {
                    ExpectToken(TokenID.LPAREN);
                    expr = Expression();
                    ExpectToken(TokenID.RPAREN);
                    ExpectToken(TokenID.KTHEN);
                    ExpectEndOfLine();
                } else if (id == TokenID.KELSE) {
                    // We mark the end of the sequence of IF blocks with
                    // a null expression.
                    expr = null;
                    ExpectEndOfLine();
                }
            } while(id == TokenID.KELSEIF || id == TokenID.KELSE);
            _ls.BackToken();
            ExpectToken(TokenID.KENDIF);
            return node;
        }

        // Return whether the specified token marks the end of an inner IF block
        private bool IsEndOfIfBlock(TokenID id) {
            return id switch {
                TokenID.KELSEIF or TokenID.KELSE or TokenID.KENDIF or TokenID.ENDOFFILE => true,
                _ => false,
            };
        }

        // Logical IF
        private ParseNode KLogicalIf(ParseNode expr) {
            ConditionalParseNode node = new();
            _parsingIf = true;
            CollectionParseNode body = new();
            body.Add(Statement(_ls.GetKeyword()));
            _parsingIf = false;
            node.Add(expr, body);
            return node;
        }

        // Arithmetic IF
        private ParseNode KArithmeticIf(ParseNode expr) {
            ArithmeticConditionalParseNode node = new();
            node.ValueExpression = expr;
            node.Add(ParseLabel());
            ExpectToken(TokenID.COMMA);
            node.Add(ParseLabel());
            ExpectToken(TokenID.COMMA);
            node.Add(ParseLabel());
            return node;
        }

        // Continue statement
        // Does nothing, really. It's just there so a label has something to follow it
        // in the absence of anything more useful.
        private ParseNode KContinue() {
            return null;
        }

        // DO statement:
        // Syntax: DO label, var=start,end
        private ParseNode KDo() {
            if (_parsingIf) {
                Messages.Error(MessageCode.DONOTPERMITTED, "DO statement not permitted in IF statement");
                return null;
            }

            // Label.
            SymbolParseNode endLabelNode = null;

            // Can omit the end label if this will be a DO..ENDDO loop
            TokenID id = _ls.PeekKeyword();
            if (id == TokenID.INTEGER) {
                endLabelNode = ParseLabel();
  
                // Optional comma.
                SkipToken(TokenID.COMMA);
                id = _ls.PeekKeyword();
            }

            // Is this Do while?
            if (id == TokenID.KWHILE) {
                return KDoWhile(endLabelNode);
            }

            // Control identifier
            IdentifierToken identToken = ExpectIdentifierToken();
            if (identToken == null) {
                SkipToEndOfLine();
                return null;
            }

            LoopParseNode node = new();

            Symbol symLoop = GetMakeSymbolForCurrentScope(identToken.Name);
            symLoop.IsReferenced = true;
            node.LoopVariable = new IdentifierParseNode(symLoop);
            ExpectToken(TokenID.EQUOP);

            // Loop starting value
            node.StartExpression = Expression();
            ExpectToken(TokenID.COMMA);

            // Loop ending value
            node.EndExpression = Expression();

            // Optional increment?
            SimpleToken token = _ls.GetToken();
            if (token.ID == TokenID.COMMA) {
                node.StepExpression = Expression();
            } else {
                _ls.BackToken();
            }
            ExpectEndOfLine();

            // Get the body of the loop until we see the end label.
            node.LoopBody = ParseDoBody(endLabelNode);

            // Warn if the loop will be skipped
            if (node.IterationCount() == 0) {
                Messages.Warning(MessageCode.LOOPSKIPPED, 2, "Loop will be skipped because iteration count is zero");
            }
            return node;
        }

        // Parse a DO WHILE loop construct
        private ParseNode KDoWhile(SymbolParseNode endLabelNode) {
            LoopParseNode node = new();

            ExpectToken(TokenID.KWHILE);
            ExpectToken(TokenID.LPAREN);
            node.StartExpression = Expression();
            ExpectToken(TokenID.RPAREN);
            ExpectEndOfLine();
            node.LoopBody = ParseDoBody(endLabelNode);

            if (node.StartExpression.IsConstant && !node.StartExpression.Value.BoolValue) {
                Messages.Warning(MessageCode.LOOPSKIPPED, 2, "Loop will be skipped because the expression is false");
            }
            return node;
        }

        // Parse the body of a standard DO or DO WHILE loop.
        private CollectionParseNode ParseDoBody(SymbolParseNode endLabelNode) {
            CollectionParseNode statements = new();
            bool loopDone = false;
            SimpleToken token = _ls.GetKeyword();
            ++_blockDepth;
            do {
                ParseNode labelNode = CheckLabel();
                if (labelNode != null) {
                    statements.Add(labelNode);
                }
                if (token.ID == TokenID.KENDDO) {
                    Messages.Warning(MessageCode.NONSTANDARDENDDO, 4, "Use of ENDDO is non-standard");
                    loopDone = true;
                    break;
                }
                statements.Add(MarkLine());
                ParseNode subnode = Statement(token);
                if (subnode != null) {
                    statements.Add(subnode);
                }
                if (_ls.HasLabel && endLabelNode != null && _ls.Label == endLabelNode.Symbol.Name) {
                    loopDone = true;
                    break;
                }
                ExpectEndOfLine();
                token = _ls.GetKeyword();
            } while (token.ID != TokenID.ENDOFFILE);
            --_blockDepth;
            if (!loopDone) {
                Messages.Error(MessageCode.MISSINGDOENDLABEL, "End label for DO loop not found");
            }
            return statements;
        }

        // Parse an assignment statement.
        // Fairly straightforward.
        private ParseNode KAssignment() {
            AssignmentParseNode node = new();
            _ls.BackToken();

            IdentifierToken identToken = ExpectIdentifierToken();
            if (identToken == null) {
                SkipToEndOfLine();
                return null;
            }

            // Do a little work to see if this is a possible statement function. The logic
            // is: if the symbol is not already declared or it is but it isn't an array
            // then we're a statement function. Array element assignments here MUST have been
            // predefined.
            Symbol sym = _localSymbols.Get(identToken.Name);
            if (_ls.PeekToken().ID == TokenID.LPAREN && (sym == null || !sym.IsArray)) {
                return KStatementFunction(identToken);
            }

            IdentifierParseNode identNode = (IdentifierParseNode)ParseIdentifierFromToken(identToken);
            if (identNode != null) {

                // Can never assign to a constant
                if (identNode.Symbol.IsConstant) {
                    Messages.Error(MessageCode.CANNOTASSIGNTOCONST, "Cannot assign a value to a constant");
                    SkipToEndOfLine();
                    return null;
                }

                node.Identifiers = new[] { identNode };
                ExpectToken(TokenID.EQUOP);
                ParseNode exprNode = Expression();
                if (exprNode != null) {
                    bool valid = ValidateAssignmentTypes(identNode.Type, exprNode.Type);
                    if (!valid) {
                        Messages.Error(MessageCode.TYPEMISMATCH, "Type mismatch in assignment");
                    }
                    node.ValueExpressions = new[] { exprNode };
                }
            } else {
                SkipToEndOfLine();
            }
            return node;
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
                    
                case SymType.DOUBLE:
                case SymType.INTEGER:
                case SymType.FLOAT:
                case SymType.COMPLEX:
                    if (toType == SymType.COMPLEX) {
                        valid = fromType == SymType.COMPLEX;
                    } else {
                        valid = Symbol.IsNumberType(fromType);
                    }
                    break;

                case SymType.BOOLEAN:
                    valid = Symbol.IsLogicalType(fromType);
                    break;
            }
            return valid;
        }

        // Parse a statement function.
        // We basically save the generated expression parse tree in the symbol
        // to be inserted whenever a reference to the statement function is made.
        private ParseNode KStatementFunction(IdentifierToken identToken) {

            string methodName = identToken.Name;
            Symbol method = _localSymbols.Get(methodName);
            if (method != null && method.Defined && method.IsMethod) {
                Messages.Error(MessageCode.SUBFUNCDEFINED, $"Statement function {methodName} already defined");
                SkipToEndOfLine();
                return null;
            }

            // Create a special symbol table just for this statement function
            _stfSymbols = new FortranSymbolCollection(_localSymbols);

            // Parameter list expected
            Collection<Symbol> parameters = ParseParameterDecl(_stfSymbols, SymScope.LOCAL, out int altReturnCount);
            if (altReturnCount > 0) {
                Messages.Error(MessageCode.ALTRETURNNOTALLOWED, "Alternate return not permitted for statement functions");
            }
            if (method == null) {
                method = _localSymbols.Add(methodName, new SymFullType(), SymClass.FUNCTION, null, _ls.LineNumber);
            }
            method.Class = SymClass.INLINE;
            method.Parameters = parameters;
            method.Linkage = SymLinkage.BYVAL;
            method.Defined = true;

            ExpectToken(TokenID.EQUOP);
            method.InlineValue = Expression();

            // Blow away the temporary symbol table now we're out of scope
            _stfSymbols = null;
            return null;
        }

        // GOTO keyword
        // Three different types:
        //   Integer = Statement number
        //   Ident = assigned branch [[,] (x,y,z)]
        //   Computed GOTO (x,y,z),ident
        private ParseNode KGoto() {
            GotoParseNode node = new();
            SimpleToken token = _ls.GetToken();

            // Check for standard GOTO
            if (token.ID == TokenID.INTEGER) {
                _ls.BackToken();
                node.Add(ParseLabel());
                return node;
            }

            // Assigned GOTO?
            if (token.ID == TokenID.IDENT) {
                _ls.BackToken();
                node.ValueExpression = ParseBasicIdentifier();
                node.IsZeroBased = true;
                SkipToken(TokenID.COMMA);
                token = _ls.GetToken();
                if (token.ID == TokenID.LPAREN) {
                    do {
                        ParseLabel();
                        //node.Add();
                        token = _ls.GetToken();
                    } while (token.ID == TokenID.COMMA);
                    _ls.BackToken();
                    ExpectToken(TokenID.RPAREN);
                }
                return node;
            }

            // Parse a computed GOTO
            _ls.BackToken();
            ExpectToken(TokenID.LPAREN);
            do {
                node.Add(ParseLabel());
                token = _ls.GetToken();
            } while (token.ID == TokenID.COMMA);
            _ls.BackToken();
            ExpectToken(TokenID.RPAREN);

            // Comma is optional.
            SkipToken(TokenID.COMMA);

            // Expression for the computed GOTO
            node.ValueExpression = IntegerExpression();
            return node;
        }

        // PROGRAM keyword.
        // The check for _hasProgram is because some valid FORTRAN programs have
        // duplicate PROGRAM statements at the start of the file. We simply ignore
        // the second instance.
        private ParseNode KProgram() {
            EnsureNoLabel();

            // Multiple PROGRAM statements are legal but ignore
            // the second and subsequent ones.
            if (_hasProgram) {
                SkipToEndOfLine();
                return null;
            }

            IdentifierToken identToken = ExpectIdentifierToken();
            if (identToken == null) {
                SkipToEndOfLine();
                return null;
            }
            return KCreateProgram(_entryPointName);
        }

        // Create a PROGRAM node with the given keyword and insert it at
        // the beginning of the current block.
        private ParseNode KCreateProgram(string methodName) {
            return KSubFunc(SymClass.SUBROUTINE, methodName, new SymFullType());
        }

        // Add this node to list of initialisation nodes that are
        // executed when the method is loaded.
        private void AddInit(ParseNode initNode) {
            Debug.Assert(_initList != null);
            _initList.Add(initNode);
        }

        // FORMAT keyword
        // Add these to the symbol table as strings with the name _Fxxx where xxx is the label
        // number. This will allow them to be stored appropriately in the executable and passed
        // as strings to the IO routines in the library.
        private ParseNode KFormat() {
            // BUGBUG: Do NOT add the label to the parse tree!
            if (!_ls.HasLabel) {
                Messages.Error(MessageCode.LABELEXPECTED, "Label expected for FORMAT specifier");
                SkipToEndOfLine();
            } else {
                string label = _ls.Label;

                SimpleToken token = ExpectToken(TokenID.STRING);
                if (token == null) {
                    SkipToEndOfLine();
                    return null;
                }
                string str = ((StringToken)token).String;

                Symbol sym = _localSymbols.Get(label);
                if (sym == null) {
                    sym = _localSymbols.Add(label, new SymFullType(SymType.NONE), SymClass.VAR, null, _ls.LineNumber);
                }
                sym.FullType = new SymFullType(SymType.CHAR, str.Length);
                sym.Modifier = SymModifier.FIXED|SymModifier.STATIC;
                sym.Defined = true;
                sym.Value.Set(str);
            }
            return null;
        }

        // RETURN keyword
        // Returns from a subroutine or function.
        private ParseNode KReturn() {
            ReturnParseNode node = new();
            if (_currentProcedure == null) {
                Messages.Error(MessageCode.ILLEGALRETURN, "Cannot use a RETURN statement here");
                return null;
            }
            if (!IsAtEndOfLine()) {
                Symbol thisSymbol = _currentProcedure.ProcedureSymbol;
                if (thisSymbol.Class == SymClass.SUBROUTINE) {
                    node.ReturnExpression = IntegerExpression();
                    if (_currentProcedure.AlternateReturnCount == 0) {
                        Messages.Error(MessageCode.ALTRETURNNOTALLOWED, "Alternate return value not allowed for this subroutine");
                    }
                } else {
                    ParseNode exprNode = Expression();
                    if (!ValidateAssignmentTypes(thisSymbol.Type, exprNode.Type)) {
                        Messages.Error(MessageCode.TYPEMISMATCH, "Type mismatch in RETURN statement");
                    }
                    node.ReturnExpression = exprNode;
                }
            }
            _hasReturn = true;
            return node;
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
                                Messages.Error(filename, MessageCode.GOTOINTOBLOCK, line, "GOTO into an inner block");
                            }
                        }
                        break;
                    }
                }
            }
        }

        // STOP keyword
        // Stop the program.
        private ParseNode KStop() {
            ExtCallParseNode node = new("JComLib.Runtime,jcomlib", "STOP") {
                Parameters = new ParametersParseNode()
            };
            node.Parameters.Add(ParseStopPauseConstant());
            node.Parameters.Add(new NumberParseNode(new Variant(0)));
            return node;
        }

        // PAUSE keyword
        // Pauses the program and outputs the given string. Waits for user
        // input to continue.
        private ParseNode KPause() {
            ExtCallParseNode node = new("JComLib.Runtime,jcomlib", "PAUSE") {
                Parameters = new ParametersParseNode()
            };
            node.Parameters.Add(ParseStopPauseConstant());
            return node;
        }
    }
}