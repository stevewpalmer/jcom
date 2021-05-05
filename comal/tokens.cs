// JComal
// Lexical analysis
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
using System.Diagnostics;

namespace JComal {

    /// <summary>
    /// Lexical tokens. New tokens MUST be added at the end and existing
    /// token values must NOT be changed as to preserve LOAD/SAVE file
    /// fidelity.
    /// </summary>
    public enum TokenID {
        APOSTROPHE = 1,
        COLON = 2,
        COMMA = 3,
        CONCAT = 4,
        DIVIDE = 5,
        ENDOFFILE = 6,
        EOL = 7,
        ERROR = 8,
        EXP = 9,
        IDENT = 10,
        INTEGER = 11,
        KABS = 12,
        KAND = 13,
        KASC = 14,
        KASSIGN = 15,
        KAT = 16,
        KATN = 17,
        KAUTO = 18,
        KBYE = 19,
        KCASE = 20,
        KCAT = 21,
        KCHR = 22,
        KCLOSE = 23,
        KCLOSED = 24,
        KCOLOUR = 25,
        KCOS = 26,
        KDATA = 27,
        KDEL = 28,
        KDELETE = 29,
        KDIM = 30,
        KDISPLAY = 31,
        KDIV = 32,
        KDO = 33,
        KEDIT = 34,
        KELIF = 35,
        KELSE = 36,
        KEND = 37,
        KENDCASE = 38,
        KENDFOR = 39,
        KENDFUNC = 40,
        KENDIF = 41,
        KENDLOOP = 42,
        KENDPROC = 43,
        KENDTRAP = 44,
        KENDWHILE = 45,
        KEQ = 46,
        KERR = 47,
        KERRTEXT = 48,
        KEXEC = 49,
        KEXIT = 50,
        KEXP = 51,
        KFALSE = 52,
        KFIND = 53,
        KFOR = 54,
        KFUNC = 55,
        KGE = 56,
        KGOTO = 57,
        KGT = 58,
        KHANDLER = 59,
        KINCADD = 60,
        KINCSUB = 61,
        KIF = 62,
        KIN = 63,
        KINPUT = 64,
        KINT = 65,
        KLABEL = 66,
        KLE = 67,
        KLEN = 68,
        KLET = 69,
        KLIST = 70,
        KLOAD = 71,
        KLOG = 72,
        KLOOP = 73,
        KLT = 74,
        KMOD = 75,
        KNE = 76,
        KNEW = 77,
        KNEXT = 78,
        KNOT = 79,
        KNULL = 80,
        KOF = 81,
        KOLD = 82,
        KOPEN = 83,
        KOR = 84,
        KOTHERWISE = 85,
        KPAGE = 86,
        KPI = 87,
        KPRINT = 88,
        KPROC = 89,
        KRANDOMIZE = 90,
        KREAD = 91,
        KREF = 92,
        KRENUM = 93,
        KREPEAT = 94,
        KREPORT = 95,
        KRETURN = 96,
        KRND = 97,
        KRUN = 98,
        KSAVE = 99,
        KSCAN = 100,
        KSGN = 101,
        KSIN = 102,
        KSPC = 103,
        KSQR = 104,
        KSTEP = 105,
        KSTOP = 106,
        KSTR = 107,
        KTAB = 108,
        KTAN = 109,
        KTHEN = 110,
        KTIME = 111,
        KTO = 112,
        KTRAP = 113,
        KTRUE = 114,
        KUNTIL = 115,
        KVAL = 116,
        KWHEN = 117,
        KWHILE = 118,
        KWRITE = 119,
        KXOR = 120,
        KZONE = 121,
        LPAREN = 122,
        MINUS = 123,
        MULTIPLY = 124,
        PLUS = 125,
        REAL = 126,
        RPAREN = 127,
        SEMICOLON = 128,
        STRING = 129,
        SPACE = 130,
        TILDE = 131,
        COMMENT = 132,
        KENTER = 133,
        KEOF = 134,
        KEOD = 135,
        KRESTORE = 136,
        KCURSOR = 137,
        KFILE = 138,
        KAPPEND = 139,
        KESC = 140,
        KCREATE = 141,
        KCURCOL = 142,
        KCURROW = 143,
        KRANDOM = 144,
        KGET = 145,
        KKEY = 146,
        KMERGE = 147,
        KMODULE = 148,
        KEXPORT = 149,
        KFREEFILE = 150,
        KENDMODULE = 151,
        KBITAND = 152,
        KBITOR = 153,
        KBITXOR = 154,
        KCHANGE = 155,
        KDIR = 156,
        KIMPORT = 157
    }

    /// <summary>
    /// Class that represents lexical tokens used by the lexical
    /// analyser.
    /// </summary>
    public static class Tokens {
        
        // List of reserved keywords and their token values
        private static readonly Dictionary<string, TokenID> _keywords = new() {
            { "abs",        TokenID.KABS        },
            { "and",        TokenID.KAND        },
            { "append",     TokenID.KAPPEND     },
            { "at",         TokenID.KAT         },
            { "atn",        TokenID.KATN        },
            { "auto",       TokenID.KAUTO       },
            { "bitand",     TokenID.KBITAND     },
            { "bitor",      TokenID.KBITOR      },
            { "bitxor",     TokenID.KBITXOR     },
            { "bye",        TokenID.KBYE        },
            { "case",       TokenID.KCASE       },
            { "cat",        TokenID.KCAT        },
            { "change",     TokenID.KCHANGE     },
            { "chr$",       TokenID.KCHR        },
            { "close",      TokenID.KCLOSE      },
            { "closed",     TokenID.KCLOSED     },
            { "colour",     TokenID.KCOLOUR     },
            { "cos",        TokenID.KCOS        },
            { "create",     TokenID.KCREATE     },
            { "curcol",     TokenID.KCURCOL     },
            { "currow",     TokenID.KCURROW     },
            { "cursor",     TokenID.KCURSOR     },
            { "data",       TokenID.KDATA       },
            { "del",        TokenID.KDEL        },
            { "delete",     TokenID.KDELETE     },
            { "dim",        TokenID.KDIM        },
            { "dir",        TokenID.KDIR        },
            { "display",    TokenID.KDISPLAY    },
            { "div",        TokenID.KDIV        },
            { "do",         TokenID.KDO         },
            { "edit",       TokenID.KEDIT       },
            { "elif",       TokenID.KELIF       },
            { "else",       TokenID.KELSE       },
            { "end",        TokenID.KEND        },
            { "endcase",    TokenID.KENDCASE    },
            { "endfor",     TokenID.KNEXT       },
            { "endfunc",    TokenID.KENDFUNC    },
            { "endif",      TokenID.KENDIF      },
            { "endloop",    TokenID.KENDLOOP    },
            { "endmodule",  TokenID.KENDMODULE  },
            { "endproc",    TokenID.KENDPROC    },
            { "endtrap",    TokenID.KENDTRAP    },
            { "endwhile",   TokenID.KENDWHILE   },
            { "eod",        TokenID.KEOD        },
            { "eof",        TokenID.KEOF        },
            { "eor",        TokenID.KXOR        },
            { "err",        TokenID.KERR        },
            { "errtext$",   TokenID.KERRTEXT    },
            { "enter",      TokenID.KENTER      },
            { "esc",        TokenID.KESC        },
            { "exec",       TokenID.KEXEC       },
            { "exit",       TokenID.KEXIT       },
            { "exp",        TokenID.KEXP        },
            { "export",     TokenID.KEXPORT     },
            { "false",      TokenID.KFALSE      },
            { "file",       TokenID.KFILE       },
            { "find",       TokenID.KFIND       },
            { "for",        TokenID.KFOR        },
            { "freefile",   TokenID.KFREEFILE   },
            { "func",       TokenID.KFUNC       },
            { "get$",       TokenID.KGET        },
            { "goto",       TokenID.KGOTO       },
            { "handler",    TokenID.KHANDLER    },
            { "if",         TokenID.KIF         },
            { "import",     TokenID.KIMPORT     },
            { "in",         TokenID.KIN         },
            { "input",      TokenID.KINPUT      },
            { "int",        TokenID.KINT        },
            { "key$",       TokenID.KKEY        },
            { "label",      TokenID.KLABEL      },
            { "len",        TokenID.KLEN        },
            { "let",        TokenID.KLET        },
            { "list",       TokenID.KLIST       },
            { "load",       TokenID.KLOAD       },
            { "log",        TokenID.KLOG        },
            { "loop",       TokenID.KLOOP       },
            { "merge",      TokenID.KMERGE      },
            { "mod",        TokenID.KMOD        },
            { "module",     TokenID.KMODULE     },
            { "new",        TokenID.KNEW        },
            { "next",       TokenID.KNEXT       },
            { "null",       TokenID.KNULL       },
            { "not",        TokenID.KNOT        },
            { "of",         TokenID.KOF         },
            { "old",        TokenID.KOLD        },
            { "open",       TokenID.KOPEN       },
            { "or",         TokenID.KOR         },
            { "ord",        TokenID.KASC        },
            { "otherwise",  TokenID.KOTHERWISE  },
            { "page",       TokenID.KPAGE       },
            { "pi",         TokenID.KPI         },
            { "print",      TokenID.KPRINT      },
            { "proc",       TokenID.KPROC       },
            { "random",     TokenID.KRANDOM     },
            { "randomize",  TokenID.KRANDOMIZE  },
            { "read",       TokenID.KREAD       },
            { "ref",        TokenID.KREF        },
            { "rem",        TokenID.COMMENT     },
            { "renum",      TokenID.KRENUM      },
            { "repeat",     TokenID.KREPEAT     },
            { "report",     TokenID.KREPORT     },
            { "restore",    TokenID.KRESTORE    },
            { "return",     TokenID.KRETURN     },
            { "rnd",        TokenID.KRND        },
            { "run",        TokenID.KRUN        },
            { "save",       TokenID.KSAVE       },
            { "scan",       TokenID.KSCAN       },
            { "sgn",        TokenID.KSGN        },
            { "sin",        TokenID.KSIN        },
            { "spc$",       TokenID.KSPC        },
            { "sqr",        TokenID.KSQR        },
            { "step",       TokenID.KSTEP       },
            { "stop",       TokenID.KSTOP       },
            { "str$",       TokenID.KSTR        },
            { "tab",        TokenID.KTAB        },
            { "tan",        TokenID.KTAN        },
            { "then",       TokenID.KTHEN       },
            { "time",       TokenID.KTIME       },
            { "to",         TokenID.KTO         },
            { "trap",       TokenID.KTRAP       },
            { "true",       TokenID.KTRUE       },
            { "until",      TokenID.KUNTIL      },
            { "val",        TokenID.KVAL        },
            { "when",       TokenID.KWHEN       },
            { "while",      TokenID.KWHILE      },
            { "write",      TokenID.KWRITE      },
            { "zone",       TokenID.KZONE       }
        };

        /// <summary>
        /// Map a keyword string to a token.
        /// </summary>
        /// <param name="str">A keyword string</param>
        /// <returns>The associated token ID, or TokenID.IDENT</returns>
        public static TokenID StringToTokenID(string str) {
            if (str == null) {
                throw new ArgumentNullException(nameof(str));
            }
            if (!_keywords.TryGetValue(str.ToLower(), out TokenID id)) {
                id = TokenID.IDENT;
            }
            return id;
        }

        /// <summary>
        /// Returns whether or not the given token is a keyword
        /// </summary>
        /// <param name="id">A token ID</param>
        /// <returns>True if it is a keyword, false otherwise</returns>
        public static bool IsKeyword(TokenID id) {
            return _keywords.ContainsValue(id);
        }

        /// <summary>
        /// Map a token to its string.
        /// </summary>
        /// <param name="id">A token ID</param>
        /// <returns>The associated string</returns>
        public static string TokenIDToString(TokenID id) {

            switch (id) {
                case TokenID.RPAREN:        return ")";
                case TokenID.LPAREN:        return "(";
                case TokenID.COMMA:         return ",";
                case TokenID.COLON:         return ":";
                case TokenID.DIVIDE:        return "/";
                case TokenID.PLUS:          return "+";
                case TokenID.MINUS:         return "-";
                case TokenID.MULTIPLY:      return "*";
                case TokenID.COMMENT:       return "//";
                case TokenID.SPACE:         return " ";
                case TokenID.APOSTROPHE:    return "'";
                case TokenID.SEMICOLON:     return ";";
                case TokenID.EXP:           return "^";
                case TokenID.TILDE:         return "~";
                case TokenID.KEQ:           return "=";
                case TokenID.KGT:           return ">";
                case TokenID.KLT:           return "<";
                case TokenID.KGE:           return ">=";
                case TokenID.KLE:           return "<=";
                case TokenID.KNE:           return "<>";
                case TokenID.KASSIGN:       return ":=";
                case TokenID.KINCADD:       return ":+";
                case TokenID.KINCSUB:       return ":-";
                case TokenID.EOL:           return "<EOL>";
                case TokenID.ENDOFFILE:     return "<EOF>";
            }

            // Anything else here is a keyword token
            foreach (KeyValuePair<string, TokenID> pair in _keywords) {
                if (id.Equals(pair.Value)) {
                    return pair.Key.ToUpper();
                }
            }

            // If we get here, we added a new token but forgot to add it to
            // the switch table above.
            Debug.Assert(false, $"TokenIDToString doesn't understand {id}");
            return "None";
        }
    }

    /// <summary>
    /// Specifies a simple token with no additional data.
    /// </summary>
    public class SimpleToken {

        /// <summary>
        /// Creates a simple token with the given token ID.
        /// </summary>
        /// <param name="id">Token ID</param>
        public SimpleToken(TokenID id) {
            ID = id;
        }

        /// <summary>
        /// Returns the token ID.
        /// </summary>
        public TokenID ID { get; set; }

        /// <summary>
        /// Returns the string equivalent of the token.
        /// </summary>
        /// <returns>Token string</returns>
        public override string ToString() {
            return Tokens.TokenIDToString(ID);
        }
    }

    /// <summary>
    /// Specifies an error found in the token stream
    /// </summary>
    public class ErrorToken : SimpleToken {

        /// <summary>
        /// Returns the error message.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Actual string found in token stream
        /// </summary>
        public string String { get; private set; }

        /// <summary>
        /// Creates an error token with the specified string.
        /// </summary>
        /// <param name="str">A string value</param>
        public ErrorToken(string message, string str) : base(TokenID.ERROR) {
            String = str;
            Message = message;
        }

        /// <summary>
        /// Returns the string equivalent of the token.
        /// </summary>
        /// <returns>Token string</returns>
        public override string ToString() {
            return String;
        }
    }

    /// <summary>
    /// Specifies a string token with a literal string value.
    /// </summary>
    public class StringToken : SimpleToken {

        /// <summary>
        /// Creates a string token with the specified string.
        /// </summary>
        /// <param name="str">A string value</param>
        public StringToken(string str) : base(TokenID.STRING) {
            String = str;
        }

        /// <summary>
        /// Returns the literal string value of the token.
        /// </summary>
        public string String { get; set; }

        /// <summary>
        /// Returns the string equivalent of the token.
        /// </summary>
        /// <returns>Token string</returns>
        public override string ToString() {
            return "\"" + String + "\"";
        }
    }

    /// <summary>
    /// Specifies an integer token with a single integer value.
    /// </summary>
    public class IntegerToken : SimpleToken {

        /// <summary>
        /// Creates an integer token with the specified integer value.
        /// </summary>
        /// <param name="value">An integer value</param>
        public IntegerToken(int value) : base(TokenID.INTEGER) {
            Value = value;
        }

        /// <summary>
        /// Returns the integer value of the token.
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Returns the string equivalent of the token.
        /// </summary>
        /// <returns>Token string</returns>
        public override string ToString() {
            return Value.ToString();
        }
    }

    /// <summary>
    /// Specifies a floating point (real) token with a single
    /// floating point value.
    /// </summary>
    public class RealToken : SimpleToken {

        /// <summary>
        /// Creates a real token with the given floating point value.
        /// </summary>
        /// <param name="value">A floating point value</param>
        public RealToken(float value) : base(TokenID.REAL) {
            Value = value;
        }

        /// <summary>
        /// Returns the floating point value of the token.
        /// </summary>
        public float Value { get; set; }

        /// <summary>
        /// Returns the string equivalent of the token.
        /// </summary>
        /// <returns>Token string</returns>
        public override string ToString() {
            return Value.ToString();
        }
    }

    /// <summary>
    /// Specifies an identifier token with a identifier name that is
    /// valid in the rules of the language.
    /// </summary>
    public class IdentifierToken : SimpleToken {

        /// <summary>
        /// Creates an identifier token with the given name.
        /// </summary>
        /// <param name="name">A identifer name string</param>
        public IdentifierToken(string name) : base(TokenID.IDENT) {
            Name = name;
        }

        /// <summary>
        /// Returns the identifier name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Returns the string equivalent of the token.
        /// </summary>
        /// <returns>Token string</returns>
        public override string ToString() {
            return Name;
        }
    }
}
