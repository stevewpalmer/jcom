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

using System.Diagnostics;
using System.Globalization;
using JComLib;

namespace JComal;

/// <summary>
/// Lexical tokens. New tokens MUST be added at the end and existing
/// token values must NOT be changed as to preserve LOAD/SAVE file
/// fidelity.
/// </summary>
public enum TokenID {
    APOSTROPHE = 1,
    COLON = 2,
    COMMA = 3,
    COMMENT = 4,
    DIVIDE = 6,
    ENDOFFILE = 7,
    EOL = 8,
    ERROR = 9,
    EXP = 10,
    IDENT = 11,
    INTEGER = 12,
    KABS = 13,
    KAND = 14,
    KAPPEND = 15,
    KASC = 16,
    KASSIGN = 17,
    KAT = 18,
    KATN = 19,
    KAUTO = 20,
    KBITAND = 21,
    KBITOR = 22,
    KBITXOR = 23,
    KBYE = 24,
    KCASE = 25,
    KCAT = 26,
    KCHANGE = 27,
    KCHR = 28,
    KCLOSE = 29,
    KCLOSED = 30,
    KCOLOUR = 31,
    KCON = 32,
    KCOS = 33,
    KCREATE = 34,
    KCURCOL = 35,
    KCURROW = 36,
    KCURSOR = 37,
    KDATA = 38,
    KDEL = 39,
    KDELETE = 40,
    KDIM = 41,
    KDIR = 42,
    KDISPLAY = 43,
    KDIV = 44,
    KDO = 45,
    KEDIT = 46,
    KELIF = 47,
    KELSE = 48,
    KEND = 49,
    KENDCASE = 50,
    KENDFOR = 51,
    KENDFUNC = 52,
    KENDIF = 53,
    KENDLOOP = 54,
    KENDMODULE = 55,
    KENDPROC = 56,
    KENDTRAP = 57,
    KENDWHILE = 58,
    KENTER = 59,
    KEOD = 60,
    KEOF = 61,
    KEQ = 62,
    KERR = 63,
    KERRTEXT = 64,
    KESC = 65,
    KEXEC = 66,
    KEXIT = 67,
    KEXP = 68,
    KEXPORT = 69,
    KEXTERNAL = 70,
    KFALSE = 71,
    KFILE = 72,
    KFIND = 73,
    KFOR = 74,
    KFREEFILE = 75,
    KFUNC = 76,
    KGE = 77,
    KGET = 78,
    KGOTO = 79,
    KGT = 80,
    KHANDLER = 81,
    KIF = 82,
    KIMPORT = 83,
    KIN = 84,
    KINCADD = 85,
    KINCSUB = 86,
    KINPUT = 87,
    KINT = 88,
    KKEY = 89,
    KLABEL = 90,
    KLE = 91,
    KLEN = 92,
    KLET = 93,
    KLIST = 94,
    KLOAD = 95,
    KLOG = 96,
    KLOOP = 97,
    KLT = 98,
    KMERGE = 99,
    KMOD = 100,
    KMODULE = 101,
    KNE = 102,
    KNEW = 103,
    KNEXT = 104,
    KNOT = 105,
    KNULL = 106,
    KOF = 107,
    KOLD = 108,
    KOPEN = 109,
    KOR = 110,
    KOTHERWISE = 111,
    KPAGE = 112,
    KPI = 113,
    KPRINT = 114,
    KPROC = 115,
    KRANDOM = 116,
    KRANDOMIZE = 117,
    KREAD = 118,
    KREF = 119,
    KRENUM = 120,
    KREPEAT = 121,
    KREPORT = 122,
    KRESTORE = 123,
    KRETURN = 124,
    KRND = 125,
    KRUN = 126,
    KSAVE = 127,
    KSCAN = 128,
    KSELECT = 129,
    KSGN = 130,
    KSIN = 131,
    KSIZE = 132,
    KSPC = 133,
    KSQR = 134,
    KSTEP = 135,
    KSTOP = 136,
    KSTR = 137,
    KTAB = 138,
    KTAN = 139,
    KTHEN = 140,
    KTIME = 141,
    KTO = 142,
    KTRAP = 143,
    KTRUE = 144,
    KUNTIL = 145,
    KUSE = 146,
    KUSING = 147,
    KVAL = 148,
    KWHEN = 149,
    KWHILE = 150,
    KWRITE = 151,
    KXOR = 152,
    KZONE = 153,
    LPAREN = 154,
    MINUS = 155,
    MULTIPLY = 156,
    PLUS = 157,
    REAL = 158,
    RPAREN = 159,
    SEMICOLON = 160,
    SPACE = 161,
    STRING = 162,
    TILDE = 163
}

/// <summary>
/// Class that represents lexical tokens used by the lexical
/// analyser.
/// </summary>
public static class Tokens {

    // List of reserved keywords and their token values
    private static readonly Dictionary<string, TokenID> Keywords = new() {
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
        { "con",        TokenID.KCON        },
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
        { "external",   TokenID.KEXTERNAL   },
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
        { "select",     TokenID.KSELECT     },
        { "sgn",        TokenID.KSGN        },
        { "sin",        TokenID.KSIN        },
        { "size",       TokenID.KSIZE       },
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
        { "use",        TokenID.KUSE        },
        { "using",      TokenID.KUSING      },
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
        if (!Keywords.TryGetValue(str.ToLower(), out TokenID id)) {
            id = TokenID.IDENT;
        }
        return id;
    }

    /// <summary>
    /// Map a token to its string.
    /// </summary>
    /// <param name="id">A token ID</param>
    /// <returns>The associated string</returns>
    public static string TokenIDToString(TokenID id) {

        switch (id) {
            case TokenID.RPAREN:
                return ")";
            case TokenID.LPAREN:
                return "(";
            case TokenID.COMMA:
                return ",";
            case TokenID.COLON:
                return ":";
            case TokenID.DIVIDE:
                return "/";
            case TokenID.PLUS:
                return "+";
            case TokenID.MINUS:
                return "-";
            case TokenID.MULTIPLY:
                return "*";
            case TokenID.COMMENT:
                return "//";
            case TokenID.SPACE:
                return " ";
            case TokenID.APOSTROPHE:
                return "'";
            case TokenID.SEMICOLON:
                return ";";
            case TokenID.EXP:
                return "^";
            case TokenID.TILDE:
                return "~";
            case TokenID.KEQ:
                return "=";
            case TokenID.KGT:
                return ">";
            case TokenID.KLT:
                return "<";
            case TokenID.KGE:
                return ">=";
            case TokenID.KLE:
                return "<=";
            case TokenID.KNE:
                return "<>";
            case TokenID.KASSIGN:
                return ":=";
            case TokenID.KINCADD:
                return ":+";
            case TokenID.KINCSUB:
                return ":-";
            case TokenID.STRING:
                return "a string";
            case TokenID.IDENT:
                return "identifier";
            case TokenID.EOL:
                return "<EOL>";
            case TokenID.ENDOFFILE:
                return "<EOF>";
        }

        // Anything else here is a keyword token
        foreach (KeyValuePair<string, TokenID> pair in Keywords) {
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
    public TokenID ID {
        get;
    }

    /// <summary>
    /// Returns the string equivalent of the token.
    /// </summary>
    /// <returns>Token string</returns>
    public override string ToString() {
        return Tokens.TokenIDToString(ID);
    }

    /// <summary>
    /// Serialize this token to the specified byte stream.
    /// </summary>
    public virtual void Serialize(ByteWriter byteWriter) {
        byteWriter.WriteInteger((int)ID);
    }

    /// <summary>
    /// Deserialize a token from the byte stream.
    /// </summary>
    /// <param name="byteReader">Byte reader</param>
    /// <returns>A SimpleToken constructed from the byte stream</returns>
    public static SimpleToken Deserialize(ByteReader byteReader) {

        TokenID tokenID = (TokenID)byteReader.ReadInteger();
        return tokenID switch {
            TokenID.ERROR => ErrorToken.Deserialize(byteReader),
            TokenID.INTEGER => IntegerToken.Deserialize(byteReader),
            TokenID.IDENT => IdentifierToken.Deserialize(byteReader),
            TokenID.STRING => StringToken.Deserialize(byteReader),
            TokenID.REAL => FloatToken.Deserialize(byteReader),
            _ => new SimpleToken(tokenID)
        };
    }
}

/// <summary>
/// Specifies an error found in the token stream
/// </summary>
public class ErrorToken : SimpleToken {

    /// <summary>
    /// Returns the error message.
    /// </summary>
    public string Message {
        get;
    }

    /// <summary>
    /// Actual string found in token stream
    /// </summary>
    private string String {
        get;
    }

    /// <summary>
    /// Creates an error token with the specified string.
    /// </summary>
    /// <param name="message">Error message</param>
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

    /// <summary>
    /// Serialize this token to the specified byte stream.
    /// </summary>
    public override void Serialize(ByteWriter byteWriter) {
        base.Serialize(byteWriter);
        byteWriter.WriteString(Message);
        byteWriter.WriteString(String);
    }

    /// <summary>
    /// Deserialize an error token from the byte stream.
    /// </summary>
    /// <param name="byteReader">Byte reader</param>
    /// <returns>An ErrorToken constructed from the byte stream</returns>
    public new static ErrorToken Deserialize(ByteReader byteReader) {
        string message = byteReader.ReadString();
        string errorString = byteReader.ReadString();
        return new ErrorToken(message, errorString);
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
    public string String {
        get;
    }

    /// <summary>
    /// Returns the string equivalent of the token.
    /// </summary>
    /// <returns>Token string</returns>
    public override string ToString() {
        return "\"" + String + "\"";
    }

    /// <summary>
    /// Serialize this token to the specified byte stream.
    /// </summary>
    public override void Serialize(ByteWriter byteWriter) {
        base.Serialize(byteWriter);
        byteWriter.WriteString(String);
    }

    /// <summary>
    /// Deserialize a string token from the byte stream.
    /// </summary>
    /// <param name="byteReader">Byte stream</param>
    /// <returns>A StringToken constructed from the byte stream</returns>
    public new static StringToken Deserialize(ByteReader byteReader) {
        string value = byteReader.ReadString();
        return new StringToken(value);
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
    public int Value {
        get; set;
    }

    /// <summary>
    /// Returns the string equivalent of the token.
    /// </summary>
    /// <returns>Token string</returns>
    public override string ToString() {
        return Value.ToString();
    }

    /// <summary>
    /// Serialize this token to the specified byte stream.
    /// </summary>
    public override void Serialize(ByteWriter byteWriter) {
        base.Serialize(byteWriter);
        byteWriter.WriteInteger(Value);
    }

    /// <summary>
    /// Deserialize an integer token from the byte stream.
    /// </summary>
    /// <param name="byteReader">Byte reader</param>
    /// <returns>An IntegerToken constructed from the byte stream</returns>
    public new static IntegerToken Deserialize(ByteReader byteReader) {
        int value = byteReader.ReadInteger();
        return new IntegerToken(value);
    }
}

/// <summary>
/// Specifies a floating point (real) token with a single
/// floating point value.
/// </summary>
public class FloatToken : SimpleToken {

    /// <summary>
    /// Creates a real token with the given floating point value.
    /// </summary>
    /// <param name="value">A floating point value</param>
    public FloatToken(float value) : base(TokenID.REAL) {
        Value = value;
    }

    /// <summary>
    /// Returns the floating point value of the token.
    /// </summary>
    public float Value {
        get;
    }

    /// <summary>
    /// Returns the string equivalent of the token.
    /// </summary>
    /// <returns>Token string</returns>
    public override string ToString() {
        return Value.ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Serialize this token to the specified byte stream.
    /// </summary>
    public override void Serialize(ByteWriter byteWriter) {
        base.Serialize(byteWriter);
        byteWriter.WriteFloat(Value);
    }

    /// <summary>
    /// Deserialize an floating point token from the byte stream.
    /// </summary>
    /// <param name="byteReader">Byte reader</param>
    /// <returns>A FloatToken constructed from the byte stream</returns>
    public new static FloatToken Deserialize(ByteReader byteReader) {
        float value = byteReader.ReadFloat();
        return new FloatToken(value);
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
    public string Name {
        get;
    }

    /// <summary>
    /// Returns the string equivalent of the token.
    /// </summary>
    /// <returns>Token string</returns>
    public override string ToString() {
        return Name;
    }

    /// <summary>
    /// Serialize this token to the specified byte stream.
    /// </summary>
    public override void Serialize(ByteWriter byteWriter) {
        base.Serialize(byteWriter);
        byteWriter.WriteString(Name);
    }

    /// <summary>
    /// Deserialize an identifier token from the byte stream.
    /// </summary>
    /// <param name="byteReader">Byte reader</param>
    /// <returns></returns>
    public new static IdentifierToken Deserialize(ByteReader byteReader) {
        string name = byteReader.ReadString();
        return new IdentifierToken(name);
    }
}
