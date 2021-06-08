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

namespace CCompiler {

    /// <summary>
    /// Compiler error and warning message codes.
    ///
    /// There's a complete set of codes for all compilers since some errors are
    /// usually consistent across many of them (such as TYPEMISMATCH). All codes
    /// should start from 0x1000 onwards mainly so the code is always a fixed length
    /// 4-digit number in the error message.
    /// 
    /// Do NOT insert new codes in the middle. We have a unit test
    /// that tries to validate that the codes do not change.
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
        MISSINGSTRINGDECLARATION,
        LIBRARYNAMEEXPECTED,
        LIBRARYNOTFOUND
    }
}
