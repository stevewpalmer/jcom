// JCom Compiler Toolkit
// Compiler messages
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

using System.Text;

namespace CCompiler {

    /// <summary>
    /// Enumerator that specifies the various types of messages:
    /// info: this is a raw text string with no filename, code or line number.
    /// error: a compiler error complete with error code and optional filename and line number.
    /// warning: a compiler warning complete with error code and optional filename and line number.
    /// </summary>
    public enum MessageLevel { Info, Warning, Error }

    /// <summary>
    /// Represents a single compiler message.
    /// </summary>
    public class Message {

        /// <summary>
        /// Constructs a single message object.
        /// </summary>
        /// <param name="filename">Optional filename</param>
        /// <param name="level">Required message level</param>
        /// <param name="code">Optional numeric code identifying this message</param>
        /// <param name="line">Optional source code line number for the message</param>
        /// <param name="text">The required actual text of the message</param>
        public Message(string filename, MessageLevel level, MessageCode code, int line, string text) {
            Filename = filename;
            Level = level;
            Code = code;
            Line = line;
            Text = text;
        }

        /// <summary>
        /// Returns the message formatted for output.
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            StringBuilder str = new();
            if (Level == MessageLevel.Info) {
                str.Append(Text);
            } else {
                if (Filename != null) {
                    str.Append(Filename);
                }
                if (Line != -1) {
                    str.AppendFormat("({0}):", Line);
                }
                if (Code != MessageCode.NONE) {
                    if (str.Length > 0) {
                        str.Append(' ');
                    }
                    str.AppendFormat("{0} CF{1}: ", Level, (int)Code);
                }
                str.Append(Text);
            }
            return str.ToString();
        }

        /// <value>
        /// Returns the name of the file associated with this message. May be null.
        /// </value>
        public string Filename { get; private set; }

        /// <value>
        /// Returns the level of this message as a <c>MessageLevel</c> type.
        /// </value>
        public MessageLevel Level { get; private set; }

        /// <value>
        /// Returns the code of this message, or MessageCode.NONE if no code is associated.
        /// </value>
        public MessageCode Code { get; private set; }

        /// <value>
        /// Returns the source code line number of this message.
        /// </value>
        public int Line { get; private set; }

        /// <value>
        /// Returns the text of the message.
        /// </value>
        public string Text { get; private set; }
    }
        
    /// <summary>
    /// Error and warning message codes.
    /// Do NOT insert new codes in the middle. We have a unit test
    /// that validates that the codes do not change.
    /// </summary>
    public enum MessageCode {
        NONE = 0x0,
        EXPECTEDTOKEN = 0x1000,
        LABELNOTALLOWED,
        LABELALREADYDECLARED,
        TOKENNOTPERMITTED,
        IFNOTPERMITTED,
        DONOTPERMITTED,
        PARAMETERCOUNTMISMATCH,
        TYPEMISMATCH,
        LABELEXPECTED,
        NESTEDSUBFUNC,
        SUBFUNCDEFINED,
        PARAMETERDEFINED,
        SUBROUTINERETURN,
        IDENTIFIERISGLOBAL,
        UNDEFINEDLABEL,
        BADTYPEWIDTH,
        IDENTIFIERDIMENSIONSEXISTS,
        IDENTIFIERREDEFINITION,
        ARRAYILLEGALBOUNDS,
        TOOMANYDIMENSIONS,
        MISSINGARRAYDIMENSIONS,
        NONSTANDARDHEX,
        NONSTANDARDESCAPES,
        IMPLICITSINGLECHAR,
        IMPLICITCHAREXISTS,
        RPARENEXPECTED,
        IMPLICITSYNTAXERROR,
        LPARENEXPECTED,
        IMPLICITRANGEERROR,
        CONSTANTEXPECTED,
        INTEGEREXPECTED,
        STRINGEXPECTED,
        NUMERICEXPECTED,
        UNRECOGNISEDOPERAND,
        IDENTIFIERLONGERTHAN6,
        IMPLICITNONENOTSTANDARD,
        MISSINGDOENDLABEL,
        ENDOFSTATEMENT,
        NUMBERTOOLARGE,
        UNRECOGNISEDKEYWORD,
        IDENTIFIERTOOLONG,
        UNRECOGNISEDCHARACTER,
        UNTERMINATEDSTRING,
        ILLEGALCHARACTERINLABEL,
        UNDEFINEDVARIABLE,
        BADNUMBERFORMAT,
        DIVISIONBYZERO,
        CANNOTASSIGNTOCONST,
        NUMBEREXPECTED,
        NOTSUPPORTED,
        NONSTANDARDENDDO,
        ILLEGALRETURN,
        MISSINGENDSTATEMENT,
        TOOMANYCONTINUATION,
        ENDNOTPERMITTED,
        WRONGNUMBEROFARGUMENTS,
        NONSTANDARDINTRINSIC,
        INCLUDEERROR,
        CODEGEN,
        SOURCEFILENOTFOUND,
        OPTIONERROR,
        UNDEFINEDINTRINSIC,
        NOOUTPUTFILE,
        BADOPTION,
        BADEXTENSION,
        MISSINGSOURCEFILE,
        GOTOINTOBLOCK,
        CILISTERROR,
        CILISTSPECIFIED,
        COMPILERFAILURE,
        BADREPEATCOUNT,
        BADSTOPFORMAT,
        BADCOMPLEX,
        NOALTERNATERETURN,
        COMMONEXISTS,
        ALREADYINCOMMON,
        COMMONMISMATCH,
        UNUSEDVARIABLE,
        BADWARNLEVEL,
        UNEXPECTEDTOKEN,
        CILISTNOTALLOWED,
        UNITREQUIRED,
        ENDNOTINWRITE,
        FORMATERROR,
        TOOFEWARGUMENTS,
        BADCOMPILEROPT,
        LOOPSKIPPED,
        ALTRETURNNOTALLOWED,
        ALTRETURNINDEXRANGE,
        ALTRETURNORDER,
        NOTALLOWEDININTRINSIC,
        ARRAYENDEXPECTED,
        NONCONSTANTDATALOOP,
        BADEXIT,
        ILLEGALATWITHFILE,
        MODULENAMEEXPECTED,
        PROCFUNCNAMEEXPECTED,
        ALREADYEXPORTED,
        MISSINGEXPORT,
        NOTINCLOSED,
        METHODNOTFOUND,
        ALREADYIMPORTED,
        UNDEFINEDFUNCTION,
        BADSUBSTRINGSPEC,
        INVALIDOF,
        REFMISMATCH,
        MISSINGSTRINGDECLARATION
    }
}
