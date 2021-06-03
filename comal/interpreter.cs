// JComal
// Interpreter
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
using System.IO;
using System.Reflection;
using CCompiler;
using JComalLib;
using JComLib;

namespace JComal {

    public class Interpreter {

        /// <summary>
        /// Constructor
        /// </summary>
        public Interpreter() {
            IsAutoMode = false;
            Lines = new();
            ActiveCompiler = null;
        }

        /// <summary>
        /// Active compiler instance
        /// </summary>
        private static Compiler ActiveCompiler { get; set; }

        /// <summary>
        /// Return whether we're in auto line numbering mode
        /// </summary>
        private bool IsAutoMode { get; set; }

        /// <summary>
        /// Current automatic line number
        /// </summary>
        private int AutoLineNumber { get; set; }

        /// <summary>
        /// Current automatic line number steps
        /// </summary>
        private int AutoSteps { get; set; }

        /// <summary>
        /// Return whether the current program in memory has been
        /// modified since it was last saved.
        /// </summary>
        private bool IsModified { get; set; }

        /// <summary>
        /// Stored program lines in tokenised format
        /// </summary>
        private Lines Lines { get; set; }

        /// <summary>
        /// Run the compiler as an interpreter.
        /// </summary>
        /// <param name="opts">Command line options</param>
        public void Run(ComalOptions opts) {

            Console.WriteLine($"JComal {Options.AssemblyVersion}");
            Console.WriteLine();

            Lines oldLines = null;
            bool hasBye = false;

            opts.Run = true;
            IsModified = false;

            while (!hasBye) {
                string inputLine = ReadLine();
                if (inputLine == null) {
                    break;
                }

                try {
                    LineTokeniser tokeniser = new();
                    Line line = new(tokeniser.TokeniseLine(inputLine));

                    SimpleToken token = line.GetToken();
                    if (token.ID == TokenID.INTEGER) {
                        oldLines = null;
                        token = line.PeekToken();
                        if (token.ID == TokenID.EOL) {
                            IsAutoMode = false;
                        } else {
                            if (line.LineNumber < 1) {
                                throw new Exception("Invalid line number");
                            }
                            Lines.Add(line);
                            ActiveCompiler = null;
                            IsModified = true;
                        }
                        continue;
                    }
                    bool possibleStatement = false;
                    while (token.ID != TokenID.EOL) {
                        switch (token.ID) {

                            case TokenID.KBYE:
                                if (IsModified) {
                                    Console.Write("Program has not been saved. Are you sure (Y/N)? ");
                                    ConsoleKeyInfo key = Console.ReadKey(false);
                                    Console.WriteLine();
                                    if (char.ToUpper(key.KeyChar) == 'N') {
                                        break;
                                    }
                                }
                                hasBye = true;
                                break;

                            case TokenID.KMERGE:
                            case TokenID.KENTER:
                                KEnterOrMerge(line, tokeniser, token.ID == TokenID.KMERGE);
                                break;

                            case TokenID.KLOAD:
                                KLoad(line);
                                break;

                            case TokenID.KSAVE:
                                KSave(line);
                                break;

                            case TokenID.KAUTO:
                                KAuto(line);
                                break;

                            case TokenID.KEDIT:
                                KEdit(tokeniser, line);
                                break;

                            case TokenID.KDISPLAY:
                            case TokenID.KLIST:
                                KListOrDisplay(line, token.ID == TokenID.KLIST);
                                break;

                            case TokenID.KCHANGE:
                            case TokenID.KFIND:
                                KFindOrChange(line, token.ID == TokenID.KFIND);
                                break;

                            case TokenID.KDEL:
                                KDel(line);
                                break;

                            case TokenID.KRENUM:
                                KRenumber(line);
                                break;

                            case TokenID.KCAT:
                                KCat(line);
                                break;

                            case TokenID.KOLD:
                                if (oldLines != null) {
                                    Lines = oldLines;
                                }
                                break;

                            case TokenID.KNEW:
                                if (oldLines == null) {
                                    oldLines = new Lines(Lines);
                                }
                                ActiveCompiler = null;
                                Lines.Clear();
                                FileManager.Zone = FileManager.DefaultZone;
                                IsModified = false;
                                break;

                            case TokenID.KSCAN:
                                KScan();
                                break;

                            case TokenID.KRUN:
                                Lines.Reset();
                                ExecuteLines(opts, Lines);
                                Lines.Reset();
                                break;

                            case TokenID.KCON:
                            case TokenID.KSIZE:
                                throw new Exception("Not Implemented");

                            default:
                                possibleStatement = true;
                                ExecuteStatement(opts, line);
                                break;
                        }

                        // Handle ':' separator for statements
                        if (!possibleStatement) {
                            line.SkipToEndOfLine();
                        }
                        token = line.GetToken();
                        if (token.ID != TokenID.COLON && token.ID != TokenID.EOL) {
                            line.PushToken(token);
                        }
                    }
                }
                catch (Exception e) {
                    JComRuntimeException jce = JComRuntimeException.GeneralHandlerNoThrow(e);
                    if (!string.IsNullOrEmpty(jce.Message)) {
                        Console.WriteLine(jce.Message);
                    }
                    FileManager.CLOSE();
                }
            }
        }

        // AUTO
        //
        // Syntax: AUTO [<start>][,[<steps>]]
        //
        // Starts automatic line numbering from the first specified value in
        // steps of the second value. The defaults for both are 10.
        //
        private void KAuto(Line ls) {

            int steps = 10;
            int startLine = Lines.MaxLine + steps;

            GetCommandRange(ls, ref startLine, ref steps, TokenID.COMMA);
            if (!ls.IsAtEndOfLine) {
                SyntaxError();
            }
            IsAutoMode = true;
            AutoLineNumber = startLine;
            AutoSteps = steps;
        }

        // CAT
        //
        // Syntax: CAT [<string constant>]
        //
        // List all programs in the current directory. If the optional string
        // constant is specified, all files are shown that match the string.
        //
        private static void KCat(Line ls) {

            string wildcard = "*";
            if (!ls.IsAtEndOfLine) {
                SimpleToken token = ls.GetToken();
                StringToken filenameToken = token as StringToken;
                if (filenameToken == null) {
                    SyntaxError();
                }
                wildcard = filenameToken.String;
            }
            Runtime.CATALOG(wildcard);
        }

        // DEL
        //
        // Syntax: DEL <start>[-[<end>]]
        //
        // Deletes program lines. At least one line number is required
        // in the range.
        //
        private void KDel(Line ls) {

            int startLine = 1;
            int endLine = int.MaxValue;

            if (ls.IsAtEndOfLine) {
                SyntaxError();
            }
            GetCommandRange(ls, ref startLine, ref endLine);
            if (!ls.IsAtEndOfLine) {
                SyntaxError();
            }
            Lines.Delete(startLine, endLine);

            ActiveCompiler = null;
            IsModified = true;
        }

        // EDIT
        //
        // Syntax: EDIT <numeric_constant>
        //
        // Edit a single program line.
        //
        private void KEdit(LineTokeniser tokeniser, Line ls) {

            SimpleToken token = ls.GetToken();
            IntegerToken lineToken = token as IntegerToken;
            if (token == null) {
                SyntaxError();
            }
            Line line = Lines.Get(lineToken.Value);
            if (line == null) {
                throw new Exception("No such line");
            }

            Console.Write(line.LineNumber);

            ReadLine readLine = new();
            string editedLine = readLine.Read(line.PrintableLine(0, false));

            line = new(tokeniser.TokeniseLine(line.LineNumber + editedLine));
            Lines.Add(line);
            Lines.Reset();

            ActiveCompiler = null;
            IsModified = true;
        }

        // ENTER/MERGE
        //
        // Syntax: ENTER <filename>
        //         MERGE <filename>
        //
        // Loads a text version of a Comal program from disk and enters it into
        // memory. The program must have valid line numbers and warnings are issued
        // if they are out of sequence. ENTER clears the current program. MERGE
        // retains the current program and adds the new lines at the end, with the
        // line numebers being automatically renumbered.
        //
        // Text versions of Comal programs are assumed to have the ListFileExtension file
        // extension if none is supplied.
        //
        private void KEnterOrMerge(Line ls, LineTokeniser tokeniser, bool isMerge) {

            SimpleToken token = ls.GetToken();
            StringToken filenameToken = token as StringToken;
            if (filenameToken == null) {
                SyntaxError();
            }

            string filename = Utilities.AddExtensionIfMissing(filenameToken.String, Consts.SourceFileExtension);

            if (!File.Exists(filename)) {
                throw new Exception("File not found");
            }
            string[] sourceLines = File.ReadAllLines(filename);

            ActiveCompiler = null;

            int lastLine = 0;
            int lineNumber = Lines.MaxLine + 10;

            if (!isMerge) {
                Lines.Clear();
            }

            foreach (string sourceLine in sourceLines) {
                if (string.IsNullOrEmpty(sourceLine)) {
                    continue;
                }
                Line line = new(tokeniser.TokeniseLine(sourceLine));
                if (isMerge) {
                    line.LineNumber = lineNumber;
                    lineNumber += 10;
                } else {
                    // Possible enter of a source file with no line
                    // numbers? 
                    if (line.LineNumber == 0) {
                        line.LineNumber = lineNumber;
                        lineNumber += 10;
                    }
                    if (line.LineNumber < lastLine) {
                        Lines.Clear();
                        throw new Exception($"Out of sequence at line {line.LineNumber}");
                    }
                    if (Lines.Get(line.LineNumber) != null) {
                        Lines.Clear();
                        throw new Exception($"Duplicate line {line.LineNumber}");
                    }
                }
                Lines.Add(line);
                lastLine = line.LineNumber;
            }
            IsModified = true;
        }

        // FIND/CHANGE
        //
        // Syntax: FIND <string>
        //         CHANGE <string>,<string>
        //
        // Lists all lines that contain the specified string. If CHANGE is
        // specified, the string found is replaced with the replacement
        // string and the line is re-tokenised and saved.
        //
        private void KFindOrChange(Line ls, bool findOnly) {

            SimpleToken token = ls.GetToken();
            StringToken findToken;
            StringToken replaceToken = null;

            findToken = token as StringToken;
            if (findToken == null) {
                SyntaxError();
            }
            if (!findOnly) {
                token = ls.GetToken();
                if (token.ID != TokenID.COMMA) {
                    SyntaxError();
                }
                token = ls.GetToken();
                replaceToken = token as StringToken;
                if (replaceToken == null) {
                    SyntaxError();
                }
            }

            foreach (Line line in Lines.AllLines) {

                string lineToSearch = line.PrintableLine(0);
                if (lineToSearch.Contains(findToken.String)) {
                    if (!findOnly) {

                        Debug.Assert(replaceToken != null);
                        lineToSearch = lineToSearch.Replace(findToken.String, replaceToken.String);
                        LineTokeniser tokeniser = new();
                        Line newLine = new(tokeniser.TokeniseLine(lineToSearch));
                        Lines.Add(newLine);

                        lineToSearch = newLine.PrintableLine(0);
                        IsModified = true;
                    }
                    Console.WriteLine(lineToSearch);
                }
            }

            ActiveCompiler = null;
            IsModified = true;
        }

        // LIST/DISPLAY
        //
        // Syntax: LIST [filename] [<start>][-[end>]]
        //         DISPLAY [filename] [<start>][-[end>]]
        //
        // Lists the program to the console or a files. If listOnly is
        // true then we add line numbers to the output. If filename is
        // specified, we output to the file instead of the console.
        //
        private void KListOrDisplay(Line ls, bool listOnly) {

            int startLine = 1;
            int endLine = int.MaxValue;
            int indent = 0;

            GetCommandRange(ls, ref startLine, ref endLine);

            StreamWriter listFile = null;

            if (!ls.IsAtEndOfLine) {
                SimpleToken token = ls.GetToken();
                if (token.ID == TokenID.KTO) {
                    token = ls.GetToken();
                }
                StringToken filenameToken = token as StringToken;
                if (filenameToken == null) {
                    SyntaxError();
                }
                string filename = Utilities.AddExtensionIfMissing(filenameToken.String, Consts.SourceFileExtension);
                listFile = new StreamWriter(filename);
            }

            Lines.Reset();
            foreach (Line line in Lines.AllLines) {
                SimpleToken token = line.GetToken();
                if (token.ID == TokenID.INTEGER) {
                    token = line.GetToken();
                }
                if (indent > 0) {
                    switch (token.ID) {
                        case TokenID.KUNTIL:
                        case TokenID.KENDCASE:
                        case TokenID.KENDPROC:
                        case TokenID.KENDFUNC:
                        case TokenID.KNEXT:
                        case TokenID.KENDFOR:
                        case TokenID.KENDWHILE:
                        case TokenID.KENDLOOP:
                        case TokenID.KENDIF:
                        case TokenID.KELSE:
                        case TokenID.KELIF:
                        case TokenID.KENDTRAP:
                        case TokenID.KHANDLER:
                            --indent;
                            break;
                    }
                }
                if (line.LineNumber >= startLine && line.LineNumber <= endLine) {
                    string lineToWrite = line.PrintableLine(indent, listOnly);
                    if (listFile != null) {
                        listFile.WriteLine(lineToWrite);
                    } else {
                        Console.WriteLine(lineToWrite);
                    }
                }
                switch (token.ID) {
                    case TokenID.KREPEAT:
                    case TokenID.KCASE:
                    case TokenID.KPROC:
                    case TokenID.KFUNC:
                    case TokenID.KFOR:
                    case TokenID.KWHILE:
                    case TokenID.KLOOP:
                    case TokenID.KIF:
                    case TokenID.KELSE:
                    case TokenID.KELIF:
                    case TokenID.KHANDLER:
                        ++indent;
                        break;
                }
                if (token.ID == TokenID.KTRAP && line.PeekToken().ID != TokenID.KESC) {
                    ++indent;
                }
                if (indent > 0) {
                    while (!line.IsAtEndOfLine) {
                        token = line.GetToken();
                        switch (token.ID) {
                            case TokenID.KUNTIL:
                            case TokenID.KTHEN:
                            case TokenID.KDO:
                            case TokenID.KEXTERNAL:
                                if (!line.IsAtEndOfStatement) {
                                    --indent;
                                }
                                break;
                        }
                    }
                }
            }
            if (listFile != null) {
                listFile.Close();
            }
            Lines.Reset();
        }

        // LOAD
        //
        // Syntax: LOAD <filename>
        //
        // Loads a Comal program from disk in binary format.
        //
        private void KLoad(Line ls) {

            SimpleToken token = ls.GetToken();
            StringToken filenameToken = token as StringToken;
            if (filenameToken == null) {
                SyntaxError();
            }

            string filename = Utilities.AddExtensionIfMissing(filenameToken.String, Consts.ProgramFileExtension);

            if (!File.Exists(filename)) {
                throw new Exception("File not found");
            }

            ActiveCompiler = null;

            try {
                using Stream stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                ByteReader byteReader = new(stream);
                Lines.Deserialize(byteReader);
            } catch (Exception) {
                throw new Exception("Invalid program file");
            }
        }

        // RENUMBER
        //
        // Syntax: RENUMBER [<start>][,[<steps>]]
        //
        // Renumbers the lines in the program. If no start is specified,
        // the default is 10. If no steps are specified then the default is
        // also 10.
        //
        private void KRenumber(Line ls) {

            int start = 10;
            int steps = 10;

            GetCommandRange(ls, ref start, ref steps, TokenID.COMMA);
            if (!ls.IsAtEndOfLine) {
                SyntaxError();
            }
            Lines.Renumber(start, steps);

            ActiveCompiler = null;
            IsModified = true;
        }

        // SAVE
        //
        // Syntax: SAVE <filename>
        //
        // Save the program to disk in binary format.
        //
        private void KSave(Line ls) {

            SimpleToken token = ls.GetToken();
            StringToken filenameToken = token as StringToken;
            if (filenameToken == null) {
                SyntaxError();
            }

            string filename = Utilities.AddExtensionIfMissing(filenameToken.String, Consts.ProgramFileExtension);

            using (Stream stream = new FileStream(filename, FileMode.Create, FileAccess.Write)) {
                using BinaryWriter writer = new(stream);
                writer.Write(Lines.Serialize());
            }

            Console.WriteLine("Saved");

            IsModified = false;
        }

        // SCAN
        //
        // Syntax: SCAN
        //
        // Scans the source code for errors by performing a compile
        //
        private void KScan() {
            ComalOptions localOpts = new() {
                Interactive = false,
                WarnLevel = 4
            };
            MessageCollection messages = new(localOpts);
            ActiveCompiler = new(localOpts);
            Lines.Reset();
            ActiveCompiler.CompileLines(Lines);
            foreach (Message msg in messages) {
                Console.WriteLine(msg);
            }
            Lines.Reset();
        }

        // Throw a syntax error exception from command mode.
        private static void SyntaxError() {
            throw new Exception("Syntax Error");
        }

        // Retrieve two values that serve as optional arguments to the LIST, DELETE and
        // other commands. Alternatively a procedure or function name can be specified
        // in which case we return the start and end line numbers of the function or
        // procedure.
        private void GetCommandRange(Line ls, ref int startLine, ref int endLine, TokenID separatorToken = TokenID.MINUS) {

            if (!ls.IsAtEndOfLine) {

                SimpleToken token = ls.GetToken();
                if (token is IdentifierToken identToken) {
                    if (!Lines.LinesForProcedure(identToken.Name, ref startLine, ref endLine)) {
                        throw new Exception("Not Found");
                    }
                } else {
                    if (token.ID == TokenID.INTEGER) {
                        IntegerToken startLineToken = token as IntegerToken;
                        startLine = startLineToken.Value;

                        token = ls.GetToken();
                    }
                    if (token.ID != separatorToken) {
                        ls.PushToken(token);
                        if (ls.IsAtEndOfLine) {
                            endLine = startLine;
                        }
                    } else {
                        token = ls.GetToken();
                        if (token.ID == TokenID.INTEGER) {
                            IntegerToken endLineToken = token as IntegerToken;
                            endLine = endLineToken.Value;
                        }
                    }
                }
            }
        }

        // Compile and execute the given statement by compiling it as a method named
        // _Direct() which is added to the existing parse tree.
        //
        // This allows us to issue direct statements that call functions and procedures
        // in the program currently loaded into memory, assuming that program has first
        // been run or scanned. Note that this doesn't allow us to persist variables
        // across calls.
        //
        private static void ExecuteStatement(ComalOptions opts, Line line) {

            if (ActiveCompiler == null) {
                ActiveCompiler = new(opts);
            }
            ActiveCompiler.Messages.Interactive = opts.Interactive;
            ActiveCompiler.Messages.Clear();
            ActiveCompiler.CompileMethod("_Direct", new Lines(line));
            ActiveCompiler.Execute("_Direct");
            FileManager.CLOSE();
        }

        // Scan and execute the specified source lines.
        private static void ExecuteLines(ComalOptions opts, Lines lines) {

            ActiveCompiler = new(opts);
            ActiveCompiler.Messages.Interactive = opts.Interactive;
            ActiveCompiler.CompileLines(lines);
            ActiveCompiler.Execute();
            FileManager.CLOSE();
        }

        // Display interpreter prompt and read a line
        private string ReadLine() {

            string prefix = string.Empty;
            if (IsAutoMode) {
                prefix = AutoLineNumber + " ";
                AutoLineNumber += AutoSteps;
            }
            Console.Write(">" + prefix);
            ReadLine readLine = new() {
                AllowHistory = true
            };
            return prefix + readLine.Read(string.Empty);
        }
    }
}
