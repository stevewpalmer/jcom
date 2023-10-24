// JFortran Compiler
// Unit tests for intrinsics
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
// under the License

using System;
using System.Numerics;
using JFortran;
using NUnit.Framework;
using Utilities;

namespace FortranTests {
    [TestFixture]
    public class IntrinsicsTests {

        // Verify INTRINSIC and EXTERNAL to pass an intrinsic to
        // a function. Note: this is skipped on Mono as the runtime for
        // passing externals like this is broken.
        [Test]
        public void ExtIntrExternal() {
            if (Type.GetType("Mono.Runtime") == null) {
                string[] code = {
                    "      DOUBLE PRECISION FUNCTION INTRTEST",
                    "      INTRINSIC DSIN",
                    "      INTRTEST=CALCIT(DSIN,23.0D0)",
                    "      END",
                    "      FUNCTION CALCIT(F,Y)",
                    "      DOUBLE PRECISION F,Y,CALCIT",
                    "      EXTERNAL F",
                    "      CALCIT=F(Y)",
                    "      END"
                };
                Compiler comp = FortranHelper.HelperCompile(code, new FortranOptions());
                Helper.HelperRunDouble(comp, "INTRTEST", -0.8462204);
            }
        }

        // Verify ABS()
        [Test]
        public void ExtIntrAbs() {
            string[] code = {
                "      FUNCTION IABS",
                "        IABS = ABS(-94)",
                "        RETURN",
                "      END",
                "      FUNCTION RABS",
                "        RABS = ABS(-7.4)",
                "        RETURN",
                "      END",
                "      FUNCTION DABS",
                "        DOUBLE PRECISION DABS",
                "        DABS = ABS(-2.6D0)",
                "        RETURN",
                "      END"
            };
            FortranOptions opts = new();
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunInteger(comp, "IABS", 94);
            Helper.HelperRunDouble(comp, "DABS", 2.6);
            Helper.HelperRunFloat(comp, "RABS", 7.4f);

            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunInteger(comp, "IABS", 94);
            Helper.HelperRunDouble(comp, "DABS", 2.6);
            Helper.HelperRunFloat(comp, "RABS", 7.4f);
        }

        // Verify ACOS().
        [Test]
        public void ExtIntrAcos() {
            string[] code = {
                "      FUNCTION RACOS",
                "        RACOS = ACOS(0.4)",
                "        RETURN",
                "      END",
                "      FUNCTION DACOS",
                "        DOUBLE PRECISION DACOS, V",
                "        V = 0.4",
                "        DACOS = ACOS(V)",
                "        RETURN",
                "      END"
            };
            FortranOptions opts = new();
            double expected = Math.Acos(0.4);
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunFloat(comp, "RACOS", (float)expected);
            Helper.HelperRunDouble(comp, "DACOS", expected);
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunFloat(comp, "RACOS", (float)expected);
            Helper.HelperRunDouble(comp, "DACOS", expected);
        }

        // Verify AINT().
        [Test]
        public void ExtIntrAInt() {
            string[] code = {
                "      FUNCTION RAINT",
                "        RAINT = AINT(3.2)",
                "        RETURN",
                "      END",
                "      FUNCTION DAINT",
                "        DOUBLE PRECISION DAINT",
                "        DAINT = AINT(9.8)",
                "        RETURN",
                "      END"
            };
            FortranOptions opts = new();
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunFloat(comp, "RAINT", 3);
            Helper.HelperRunDouble(comp, "DAINT", 9);
            opts.Inline = false;
            Helper.HelperRunFloat(comp, "RAINT", 3);
            Helper.HelperRunDouble(comp, "DAINT", 9);
        }

        // Verify ALOG().
        [Test]
        public void ExtIntrALog() {
            string[] code = {
                "      FUNCTION RALOG",
                "        RALOG = ALOG(0.4)",
                "        RETURN",
                "      END"
            };
            FortranOptions opts = new();
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            float expected = (float)Math.Log(0.4);
            Helper.HelperRunFloat(comp, "RALOG", expected);
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunFloat(comp, "RALOG", expected);
        }

        // Verify ALOG10() and LOG10().
        [Test]
        public void ExtIntrALog10() {
            string[] code = {
                "      FUNCTION RALOG10",
                "        RETURN ALOG10(0.4)",
                "      END",
                "      FUNCTION RLOG10",
                "        RETURN LOG10(0.4)",
                "      END",
                "      FUNCTION DDLOG10",
                "        DOUBLE PRECISION DDLOG10",
                "        RETURN LOG10(0.4D0)",
                "      END"
            };
            FortranOptions opts = new();
            double expected = Math.Log10(0.4);
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunFloat(comp, "RALOG10", (float)expected);
            Helper.HelperRunFloat(comp, "RLOG10", (float)expected);
            Helper.HelperRunDouble(comp, "DDLOG10", expected);
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunFloat(comp, "RALOG10", (float)expected);
            Helper.HelperRunFloat(comp, "RLOG10", (float)expected);
            Helper.HelperRunDouble(comp, "DDLOG10", expected);
        }

        // Verify AMOD().
        [Test]
        public void ExtIntrAMod() {
            string[] code = {
                "      FUNCTION RAMOD",
                "        RETURN AMOD(97.75, 4.75)",
                "      END"
            };
            FortranOptions opts = new();
            float expected = 97.75f % 4.75f;
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunFloat(comp, "RAMOD", expected);
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunFloat(comp, "RAMOD", expected);
        }

        // Verify ANINT() and DNINT().
        [Test]
        public void ExtIntrNInt() {
            string[] code = {
                "      FUNCTION RANINT",
                "        RANINT = ANINT(3.2)",
                "        RETURN",
                "      END",
                "      FUNCTION DANINT",
                "        DOUBLE PRECISION DANINT",
                "        DANINT = ANINT(-9.8D0)",
                "        RETURN",
                "      END",
                "      FUNCTION DDNINT",
                "        DOUBLE PRECISION DDNINT",
                "        DDNINT = DNINT(-9.8D0)",
                "        RETURN",
                "      END"
            };
            FortranOptions opts = new();
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunFloat(comp, "RANINT", 3);
            Helper.HelperRunDouble(comp, "DANINT", -10);
            Helper.HelperRunDouble(comp, "DDNINT", -10);
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunFloat(comp, "RANINT", 3);
            Helper.HelperRunDouble(comp, "DANINT", -10);
            Helper.HelperRunDouble(comp, "DDNINT", -10);
        }

        // Verify ASIN().
        [Test]
        public void ExtIntrAsin() {
            string[] code = {
                "      FUNCTION RASIN",
                "        RASIN = ASIN(0.4)",
                "        RETURN",
                "      END",
                "      FUNCTION DASIN",
                "        DOUBLE PRECISION DASIN, V",
                "        V = 0.4",
                "        DASIN = ASIN(V)",
                "        RETURN",
                "      END"
            };
            FortranOptions opts = new();
            double expected = Math.Asin(0.4);
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunFloat(comp, "RASIN", (float)expected);
            Helper.HelperRunDouble(comp, "DASIN", expected);
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunFloat(comp, "RASIN", (float)expected);
            Helper.HelperRunDouble(comp, "DASIN", expected);
        }

        // Verify ATAN().
        [Test]
        public void ExtIntrAtan() {
            string[] code = {
                "      FUNCTION RATAN",
                "        RATAN = ATAN(0.4)",
                "        RETURN",
                "      END",
                "      FUNCTION DATAN",
                "        DOUBLE PRECISION DATAN, V",
                "        V = 0.4",
                "        DATAN = ATAN(V)",
                "        RETURN",
                "      END"
            };
            FortranOptions opts = new();
            double expected = Math.Atan(0.4);
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunFloat(comp, "RATAN", (float)expected);
            Helper.HelperRunDouble(comp, "DATAN", expected);
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunFloat(comp, "RATAN", (float)expected);
            Helper.HelperRunDouble(comp, "DATAN", expected);
        }

        // Verify ATAN2().
        [Test]
        public void ExtIntrAtan2() {
            string[] code = {
                "      FUNCTION RATAN2",
                "        RATAN2 = ATAN2(-71.2,9.3)",
                "        RETURN",
                "      END",
                "      FUNCTION DATAN2",
                "        DOUBLE PRECISION DATAN2",
                "        RETURN ATAN2(-71.2D0,9.3D0)",
                "      END"
            };
            FortranOptions opts = new();
            double expected = Math.Atan2(-71.2, 9.3);
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunFloat(comp, "RATAN2", (float)expected);
            Helper.HelperRunDouble(comp, "DATAN2", expected);
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunFloat(comp, "RATAN2", (float)expected);
            Helper.HelperRunDouble(comp, "DATAN2", expected);
        }

        // Verify CABS()
        [Test]
        public void ExtIntrCabs() {
            string[] code = {
                "      FUNCTION RCABS",
                "        REAL RCABS",
                "        RETURN CABS((-12,3))",
                "      END"
            };
            FortranOptions opts = new();
            float expected = (float)Complex.Abs(new Complex(-12, 3));
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunFloat(comp, "RCABS", expected);
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunFloat(comp, "RCABS", expected);
        }

        // Verify CCOS()
        [Test]
        public void ExtIntrCcos() {
            string[] code = {
                "      FUNCTION RCCOS",
                "        COMPLEX RCCOS",
                "        RETURN CCOS((32,1))",
                "      END"
            };
            FortranOptions opts = new();
            Complex expected = Complex.Cos(new Complex(32, 1));
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunComplex(comp, "RCCOS", expected);
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunComplex(comp, "RCCOS", expected);
        }

        // Verify CHAR()
        [Test]
        public void ExtIntrChar() {
            string[] code = {
                "      FUNCTION CCHAR",
                "        CHARACTER CCHAR",
                "        CCHAR = CHAR(65)",
                "        RETURN",
                "      END"
            };
            FortranOptions opts = new();
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunString(comp, "CCHAR", "A");
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunString(comp, "CCHAR", "A");
        }

        // Verify CLOG()
        [Test]
        public void ExtIntrClog() {
            string[] code = {
                "      FUNCTION RCLOG",
                "        COMPLEX RCLOG",
                "        RETURN CLOG((32,1))",
                "      END"
            };
            FortranOptions opts = new();
            Complex expected = Complex.Log(new Complex(32, 1));
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunComplex(comp, "RCLOG", expected);
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunComplex(comp, "RCLOG", expected);
        }

        // Verify CMPLX()
        [Test]
        public void ExtIntrCmplx() {
            string[] code = {
                "      FUNCTION CCMPLX",
                "        COMPLEX CCMPLX",
                "        RETURN CMPLX(65,12)",
                "      END"
            };
            FortranOptions opts = new();
            Complex expected = new(65, 12);
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunComplex(comp, "CCMPLX", expected);
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunComplex(comp, "CCMPLX", expected);
        }

        // Verify COS().
        [Test]
        public void ExtIntrCos() {
            string[] code = {
                "      FUNCTION RCOS",
                "        RCOS = COS(0.4)",
                "        RETURN",
                "      END",
                "      FUNCTION DCOS",
                "        DOUBLE PRECISION DCOS, V",
                "        V = 0.4",
                "        DCOS = COS(V)",
                "        RETURN",
                "      END",
                "      FUNCTION CCOS",
                "        COMPLEX CCOS",
                "        RETURN COS((12,3))",
                "      END"
            };
            FortranOptions opts = new();
            double expected = Math.Cos(0.4);
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunFloat(comp, "RCOS", (float)expected);
            Helper.HelperRunDouble(comp, "DCOS", expected);
            Helper.HelperRunComplex(comp, "CCOS", Complex.Cos(new Complex(12, 3)));
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunFloat(comp, "RCOS", (float)expected);
            Helper.HelperRunDouble(comp, "DCOS", expected);
            Helper.HelperRunComplex(comp, "CCOS", Complex.Cos(new Complex(12, 3)));
        }

        // Verify COSH().
        [Test]
        public void ExtIntrCosh() {
            string[] code = {
                "      FUNCTION RCOSH",
                "        RETURN COSH(0.8)",
                "      END",
                "      FUNCTION DCOSH",
                "        DOUBLE PRECISION DCOSH, V",
                "        V = 0.8D0",
                "        DCOSH = COSH(V)",
                "        RETURN",
                "      END"
            };
            FortranOptions opts = new();
            double expected = Math.Cosh(0.8);
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunFloat(comp, "RCOSH", (float)expected);
            Helper.HelperRunDouble(comp, "DCOSH", expected);
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunFloat(comp, "RCOSH", (float)expected);
            Helper.HelperRunDouble(comp, "DCOSH", expected);
        }

        // Verify CSIN()
        [Test]
        public void ExtIntrCsin() {
            string[] code = {
                "      FUNCTION CCSIN",
                "        COMPLEX CCSIN",
                "        RETURN CSIN((32,1))",
                "      END"
            };
            FortranOptions opts = new();
            Complex expected = Complex.Sin(new Complex(32, 1));
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunComplex(comp, "CCSIN", expected);
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunComplex(comp, "CCSIN", expected);
        }

        // Verify CSQRT()
        [Test]
        public void ExtIntrCsqrt() {
            string[] code = {
                "      FUNCTION CCSQRT",
                "        COMPLEX CCSQRT",
                "        RETURN CSQRT((49,5))",
                "      END"
            };
            FortranOptions opts = new();
            Complex expected = Complex.Sqrt(new Complex(49, 5));
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunComplex(comp, "CCSQRT", expected);
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunComplex(comp, "CCSQRT", expected);
        }

        // Verify DABS()
        [Test]
        public void ExtIntrDabs() {
            string[] code = {
                "      FUNCTION DDABS",
                "        DOUBLE PRECISION DDABS",
                "        DDABS = DABS(-2.6D0)",
                "        RETURN",
                "      END"
            };
            FortranOptions opts = new();
            double expected = Math.Abs(-2.6);
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunDouble(comp, "DDABS", expected);
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunDouble(comp, "DDABS", expected);
        }

        // Verify DACOS().
        [Test]
        public void ExtIntrDacos() {
            string[] code = {
                "      FUNCTION DDACOS",
                "        DOUBLE PRECISION DDACOS",
                "        DDACOS = DACOS(0.5D0)",
                "        RETURN",
                "      END"
            };
            FortranOptions opts = new();
            double expected = Math.Acos(0.5);
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunDouble(comp, "DDACOS", expected);
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunDouble(comp, "DDACOS", expected);
        }

        // Verify DASIN().
        [Test]
        public void ExtIntrDasin() {
            string[] code = {
                "      FUNCTION DDASIN",
                "        DOUBLE PRECISION DDASIN",
                "        DDASIN = DASIN(0.5D0)",
                "        RETURN",
                "      END"
            };
            FortranOptions opts = new();
            double expected = Math.Asin(0.5);
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunDouble(comp, "DDASIN", expected);
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunDouble(comp, "DDASIN", expected);
        }

        // Verify DATAN().
        [Test]
        public void ExtIntrDatan() {
            string[] code = {
                "      FUNCTION DDATAN",
                "        DOUBLE PRECISION DDATAN",
                "        DDATAN = DATAN(0.5D0)",
                "        RETURN",
                "      END"
            };
            FortranOptions opts = new();
            double expected = Math.Atan(0.5);
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunDouble(comp, "DDATAN", expected);
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunDouble(comp, "DDATAN", expected);
        }

        // Verify DATAN2().
        [Test]
        public void ExtIntrDatan2() {
            string[] code = {
                "      FUNCTION DDATAN2",
                "        DOUBLE PRECISION DDATAN2",
                "        RETURN DATAN2(0.5D0, 0.45D0)",
                "      END"
            };
            FortranOptions opts = new();
            double expected = Math.Atan2(0.5, 0.45);
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunDouble(comp, "DDATAN2", expected);
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunDouble(comp, "DDATAN2", expected);
        }

        // Verify DBLE().
        [Test]
        public void ExtIntrDble() {
            string[] code = {
                "      FUNCTION DDBLEI",
                "        DOUBLE PRECISION DDBLEI",
                "        RETURN DBLE(12)",
                "      END",
                "      FUNCTION DDBLER",
                "        DOUBLE PRECISION DDBLER",
                "        RETURN DBLE(12.5)",
                "      END",
                "      FUNCTION DDBLED",
                "        DOUBLE PRECISION DDBLED",
                "        RETURN DBLE(13.0D2)",
                "      END",
                "      FUNCTION DDBLEC",
                "        DOUBLE PRECISION DDBLEC",
                "        RETURN DBLE((13,5))",
                "      END"
            };
            FortranOptions opts = new();
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunDouble(comp, "DDBLEI", 12);
            Helper.HelperRunDouble(comp, "DDBLER", 12.5);
            Helper.HelperRunDouble(comp, "DDBLED", 1300);
            Helper.HelperRunDouble(comp, "DDBLEC", 13);
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunDouble(comp, "DDBLEI", 12);
            Helper.HelperRunDouble(comp, "DDBLER", 12.5);
            Helper.HelperRunDouble(comp, "DDBLED", 1300);
            Helper.HelperRunDouble(comp, "DDBLEC", 13);
        }

        // Verify DCOS().
        [Test]
        public void ExtIntrDcos() {
            string[] code = {
                "      FUNCTION DDCOS",
                "        DOUBLE PRECISION DDCOS",
                "        DDCOS = DCOS(0.5D0)",
                "        RETURN",
                "      END"
            };
            FortranOptions opts = new();
            double expected = Math.Cos(0.5);
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunDouble(comp, "DDCOS", expected);
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunDouble(comp, "DDCOS", expected);
        }

        // Verify DCOSH().
        [Test]
        public void ExtIntrDcosh() {
            string[] code = {
                "      FUNCTION DDCOSH",
                "        DOUBLE PRECISION DDCOSH",
                "        RETURN DCOSH(0.147D0)",
                "      END"
            };
            FortranOptions opts = new();
            double expected = Math.Cosh(0.147);
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunDouble(comp, "DDCOSH", expected);
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunDouble(comp, "DDCOSH", expected);
        }

        // Verify EXP() and DEXP().
        [Test]
        public void ExtIntrDexp() {
            string[] code = {
                "      FUNCTION REXP",
                "        REAL REXP",
                "        RETURN EXP(3.2)",
                "      END",
                "      FUNCTION RDEXP",
                "        DOUBLE PRECISION RDEXP",
                "        RETURN EXP(3.2D0)",
                "      END",
                "      FUNCTION DDEXP",
                "        DOUBLE PRECISION DDEXP",
                "        RETURN DEXP(3.2D0)",
                "      END",
                "      FUNCTION CCEXP",
                "        COMPLEX CCEXP",
                "        RETURN EXP((3.2,0))",
                "      END"
            };
            FortranOptions opts = new();
            double expected = Math.Exp(3.2);
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunFloat(comp, "REXP", (float)expected);
            Helper.HelperRunDouble(comp, "DDEXP", expected);
            Helper.HelperRunDouble(comp, "RDEXP", expected);
            Helper.HelperRunComplex(comp, "CCEXP", Complex.Exp(new Complex(3.2, 0)));
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunFloat(comp, "REXP", (float)expected);
            Helper.HelperRunDouble(comp, "DDEXP", expected);
            Helper.HelperRunDouble(comp, "RDEXP", expected);
            Helper.HelperRunComplex(comp, "CCEXP", Complex.Exp(new Complex(3.2, 0)));
        }

        // Verify DINT().
        [Test]
        public void ExtIntrDint() {
            string[] code = {
                "      FUNCTION DDINT",
                "        DOUBLE PRECISION DDINT",
                "        RETURN DINT(89.38D0)",
                "      END"
            };
            FortranOptions opts = new();
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunDouble(comp, "DDINT", 89);
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunDouble(comp, "DDINT", 89);
        }

        // Verify DLOG().
        [Test]
        public void ExtIntrDLog() {
            string[] code = {
                "      FUNCTION RDLOG",
                "        DOUBLE PRECISION RDLOG",
                "        RDLOG = DLOG(0.4D0)",
                "        RETURN",
                "      END"
            };
            FortranOptions opts = new();
            double expected = Math.Log(0.4);
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunDouble(comp, "RDLOG", expected);
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunDouble(comp, "RDLOG", expected);
        }

        // Verify DLOG10().
        [Test]
        public void ExtIntrDLog10() {
            string[] code = {
                "      FUNCTION RDLOG10",
                "        DOUBLE PRECISION RDLOG10",
                "        RDLOG10 = DLOG10(3.72D0)",
                "        RETURN",
                "      END"
            };
            FortranOptions opts = new();
            double expected = Math.Log10(3.72);
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunDouble(comp, "RDLOG10", expected);
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunDouble(comp, "RDLOG10", expected);
        }

        // Verify MOD()
        // Test integer, real and double interfaces
        // Test both inline and library versions.
        [Test]
        public void ExtIntrMod() {
            string[] code = {
                "      FUNCTION IMODTEST",
                "        IMODTEST = MOD(65, 11)",
                "        RETURN",
                "      END",
                "      FUNCTION AMODTEST",
                "        AMODTEST = AMOD(63.5, 3.5)",
                "        RETURN",
                "      END",
                "      FUNCTION DMODTEST",
                "        DOUBLE PRECISION DMODTEST",
                "        DMODTEST = DMOD(63.5D0, 3.5D0)",
                "        RETURN",
                "      END"
            };
            FortranOptions opts = new();
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunInteger(comp, "IMODTEST", 10);
            Helper.HelperRunFloat(comp, "AMODTEST", 0.5f);
            Helper.HelperRunDouble(comp, "DMODTEST", 0.5);
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunInteger(comp, "IMODTEST", 10);
            Helper.HelperRunFloat(comp, "AMODTEST", 0.5f);
            Helper.HelperRunDouble(comp, "DMODTEST", 0.5);
        }

        // Verify DPROD().
        [Test]
        public void ExtIntrDprod() {
            string[] code = {
                "      FUNCTION DDPROD",
                "        DOUBLE PRECISION DDPROD",
                "        DDPROD = DPROD(12.4,3.8)",
                "        RETURN",
                "      END"
            };
            FortranOptions opts = new();
            double expected = 12.4 * 3.8;
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunDouble(comp, "DDPROD", expected);
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunDouble(comp, "DDPROD", expected);
        }

        // Verify DSIN().
        [Test]
        public void ExtIntrDsin() {
            string[] code = {
                "      FUNCTION DDSIN",
                "        DOUBLE PRECISION DDSIN",
                "        DDSIN = DSIN(0.5D0)",
                "        RETURN",
                "      END"
            };
            FortranOptions opts = new();
            double expected = Math.Sin(0.5);
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunDouble(comp, "DDSIN", expected);
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunDouble(comp, "DDSIN", expected);
        }

        // Verify DSINH().
        [Test]
        public void ExtIntrDsinh() {
            string[] code = {
                "      FUNCTION DDSINH",
                "        DOUBLE PRECISION DDSINH",
                "        RETURN DSINH(0.147D0)",
                "      END"
            };
            FortranOptions opts = new();
            double expected = Math.Sinh(0.147);
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunDouble(comp, "DDSINH", expected);
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunDouble(comp, "DDSINH", expected);
        }

        // Verify DSQRT()
        [Test]
        public void ExtIntrDsqrt() {
            string[] code = {
                "      FUNCTION DDSQRT",
                "        DOUBLE PRECISION DDSQRT",
                "        RETURN DSQRT(49D0)",
                "      END"
            };
            FortranOptions opts = new();
            double expected = Math.Sqrt(49);
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunDouble(comp, "DDSQRT", expected);
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunDouble(comp, "DDSQRT", expected);
        }

        // Verify DTAN().
        [Test]
        public void ExtIntrDtan() {
            string[] code = {
                "      FUNCTION DDTAN",
                "        DOUBLE PRECISION DDTAN",
                "        DDTAN = DTAN(0.5D0)",
                "        RETURN",
                "      END"
            };
            FortranOptions opts = new();
            double expected = Math.Tan(0.5);
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunDouble(comp, "DDTAN", expected);
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunDouble(comp, "DDTAN", expected);
        }

        // Verify DTANH().
        [Test]
        public void ExtIntrDtanh() {
            string[] code = {
                "      FUNCTION DDTANH",
                "        DOUBLE PRECISION DDTANH",
                "        RETURN DTANH(0.147D0)",
                "      END"
            };
            FortranOptions opts = new();
            double expected = Math.Tanh(0.147);
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunDouble(comp, "DDTANH", expected);
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunDouble(comp, "DDTANH", expected);
        }

        // Verify FLOAT().
        [Test]
        public void ExtIntrFloat() {
            string[] code = {
                "      FUNCTION RFLOATI",
                "        RETURN FLOAT(12)",
                "      END"
            };
            FortranOptions opts = new();
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunFloat(comp, "RFLOATI", 12f);
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunFloat(comp, "RFLOATI", 12f);
        }

        // Verify IABS().
        [Test]
        public void ExtIntrIabs() {
            string[] code = {
                "      FUNCTION ITEST1",
                "        RETURN IABS(-12)",
                "      END",
                "      FUNCTION ITEST2",
                "        RETURN IABS(73)",
                "      END"
            };
            FortranOptions opts = new();
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunInteger(comp, "ITEST1", 12);
            Helper.HelperRunInteger(comp, "ITEST2", 73);
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunInteger(comp, "ITEST1", 12);
            Helper.HelperRunInteger(comp, "ITEST2", 73);
        }

        // Verify ICHAR().
        [Test]
        public void ExtIntrIchar() {
            string[] code = {
                "      FUNCTION ITEST1",
                "        RETURN ICHAR(\"foo\")",
                "      END",
                "      FUNCTION ITEST2",
                "        CHARACTER *3 STR1",
                "        STR1 = 'HELLO'",
                "        RETURN ICHAR(STR1)",
                "      END"
            };
            FortranOptions opts = new();
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunInteger(comp, "ITEST1", 102);
            Helper.HelperRunInteger(comp, "ITEST2", 72);
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunInteger(comp, "ITEST1", 102);
            Helper.HelperRunInteger(comp, "ITEST2", 72);
        }

        // Verify IDINT().
        [Test]
        public void ExtIntrIdint() {
            string[] code = {
                "      FUNCTION ITEST",
                "        RETURN IDINT(12.45D1)",
                "      END"
            };
            FortranOptions opts = new();
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunInteger(comp, "ITEST", 124);
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunInteger(comp, "ITEST", 124);
        }

        // Verify IFIX() and INT().
        [Test]
        public void ExtIntrIfix() {
            string[] code = {
                "      FUNCTION ITEST",
                "        RETURN IFIX(12.45E1)",
                "      END",
                "      FUNCTION ITEST2",
                "        RETURN INT(12.45E1)",
                "      END"
            };
            FortranOptions opts = new();
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunInteger(comp, "ITEST", 124);
            Helper.HelperRunInteger(comp, "ITEST2", 124);
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunInteger(comp, "ITEST", 124);
            Helper.HelperRunInteger(comp, "ITEST2", 124);
        }

        // Verify INT().
        [Test]
        public void ExtIntrInt() {
            string[] code = {
                "      FUNCTION JINTI",
                "        RETURN INT(12)",
                "      END",
                "      FUNCTION JINTR",
                "        RETURN INT(12.5)",
                "      END",
                "      FUNCTION JINTD",
                "        RETURN INT(13.0D2)",
                "      END",
                "      FUNCTION JINTC",
                "        RETURN INT((13,5))",
                "      END"
            };
            FortranOptions opts = new();
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunInteger(comp, "JINTI", 12);
            Helper.HelperRunInteger(comp, "JINTR", 12);
            Helper.HelperRunInteger(comp, "JINTD", 1300);
            Helper.HelperRunInteger(comp, "JINTC", 13);
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunInteger(comp, "JINTI", 12);
            Helper.HelperRunInteger(comp, "JINTR", 12);
            Helper.HelperRunInteger(comp, "JINTD", 1300);
            Helper.HelperRunInteger(comp, "JINTC", 13);
        }

        // Verify LEN().
        [Test]
        public void ExtIntrLen() {
            string[] code = {
                "      FUNCTION ITEST1",
                "        RETURN LEN('HALO3')",
                "      END",
                "      FUNCTION ITEST2",
                "        RETURN LEN('')",
                "      END",
                "      FUNCTION ITEST3",
                "        CHARACTER *4 STR",
                "        STR='FORTRAN'//'77'",
                "        RETURN LEN(STR)",
                "      END"
            };
            FortranOptions opts = new();
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunInteger(comp, "ITEST1", 5);
            Helper.HelperRunInteger(comp, "ITEST2", 0);
            Helper.HelperRunInteger(comp, "ITEST3", 4);
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunInteger(comp, "ITEST1", 5);
            Helper.HelperRunInteger(comp, "ITEST2", 0);
            Helper.HelperRunInteger(comp, "ITEST3", 4);
        }

        // Verify LOG().
        [Test]
        public void ExtIntrLog() {
            string[] code = {
                "      FUNCTION RLOG",
                "        RLOG = LOG(0.4)",
                "        RETURN",
                "      END",
                "      FUNCTION DLOG",
                "        DOUBLE PRECISION DLOG, V",
                "        V = 0.4",
                "        DLOG = LOG(V)",
                "        RETURN",
                "      END",
                "      FUNCTION CCLOG",
                "        COMPLEX CCLOG",
                "        RETURN LOG((12,3))",
                "      END"
            };
            FortranOptions opts = new();
            double expected = Math.Log(0.4);
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunFloat(comp, "RLOG", (float)expected);
            Helper.HelperRunDouble(comp, "DLOG", expected);
            Helper.HelperRunComplex(comp, "CCLOG", Complex.Log(new Complex(12, 3)));
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunFloat(comp, "RLOG", (float)expected);
            Helper.HelperRunDouble(comp, "DLOG", expected);
            Helper.HelperRunComplex(comp, "CCLOG", Complex.Log(new Complex(12, 3)));
        }

        // Verify REAL().
        [Test]
        public void ExtIntrReal() {
            string[] code = {
                "      FUNCTION DREALI",
                "        RETURN REAL(12)",
                "      END",
                "      FUNCTION DREALR",
                "        RETURN REAL(12.5)",
                "      END",
                "      FUNCTION DREALD",
                "        RETURN REAL(13.0D2)",
                "      END",
                "      FUNCTION DREALC",
                "        RETURN REAL((13,5))",
                "      END"
            };
            FortranOptions opts = new();
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunFloat(comp, "DREALI", 12);
            Helper.HelperRunFloat(comp, "DREALR", 12.5f);
            Helper.HelperRunFloat(comp, "DREALD", 1300);
            Helper.HelperRunFloat(comp, "DREALC", 13);
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunFloat(comp, "DREALI", 12);
            Helper.HelperRunFloat(comp, "DREALR", 12.5f);
            Helper.HelperRunFloat(comp, "DREALD", 1300);
            Helper.HelperRunFloat(comp, "DREALC", 13);
        }

        // Verify SIN().
        [Test]
        public void ExtIntrSin() {
            string[] code = {
                "      FUNCTION RSIN",
                "        RSIN = SIN(0.4)",
                "        RETURN",
                "      END",
                "      FUNCTION DSIN",
                "        DOUBLE PRECISION DSIN, V",
                "        V = 0.4",
                "        DSIN = SIN(V)",
                "        RETURN",
                "      END",
                "      FUNCTION CSIN",
                "        COMPLEX CSIN",
                "        RETURN SIN((12,3))",
                "      END"
            };
            FortranOptions opts = new();
            double expected = Math.Sin(0.4);
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunFloat(comp, "RSIN", (float)expected);
            Helper.HelperRunDouble(comp, "DSIN", expected);
            Helper.HelperRunComplex(comp, "CSIN", Complex.Sin(new Complex(12, 3)));
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunFloat(comp, "RSIN", (float)expected);
            Helper.HelperRunDouble(comp, "DSIN", expected);
            Helper.HelperRunComplex(comp, "CSIN", Complex.Sin(new Complex(12, 3)));
        }

        // Verify SINH().
        [Test]
        public void ExtIntrSinh() {
            string[] code = {
                "      FUNCTION RSINH",
                "        RETURN SINH(0.8)",
                "      END",
                "      FUNCTION DSINH",
                "        DOUBLE PRECISION DSINH, V",
                "        V = 0.8D0",
                "        DSINH = SINH(V)",
                "        RETURN",
                "      END"
            };
            FortranOptions opts = new();
            double expected = Math.Sinh(0.8);
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunFloat(comp, "RSINH", (float)expected);
            Helper.HelperRunDouble(comp, "DSINH", expected);
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunFloat(comp, "RSINH", (float)expected);
            Helper.HelperRunDouble(comp, "DSINH", expected);
        }

        // Verify SNGL().
        [Test]
        public void ExtIntrSngl() {
            string[] code = {
                "      FUNCTION RTEST",
                "        RETURN SNGL(12.45D1)",
                "      END"
            };
            FortranOptions opts = new();
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunFloat(comp, "RTEST", 12.45E1f);
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunFloat(comp, "RTEST", 12.45E1f);
        }

        // Verify SQRT().
        [Test]
        public void ExtIntrSqrt() {
            string[] code = {
                "      FUNCTION RSQRT",
                "        RSQRT = SQRT(0.4)",
                "        RETURN",
                "      END",
                "      FUNCTION DSQRT",
                "        DOUBLE PRECISION DSQRT, V",
                "        V = 0.4",
                "        DSQRT = SQRT(V)",
                "        RETURN",
                "      END",
                "      FUNCTION CSQRT",
                "        COMPLEX CSQRT",
                "        RETURN SQRT((12,3))",
                "      END"
            };
            FortranOptions opts = new();
            double expected = Math.Sqrt(0.4);
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunFloat(comp, "RSQRT", (float)expected);
            Helper.HelperRunDouble(comp, "DSQRT", expected);
            Helper.HelperRunComplex(comp, "CSQRT", Complex.Sqrt(new Complex(12, 3)));
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunFloat(comp, "RSQRT", (float)expected);
            Helper.HelperRunDouble(comp, "DSQRT", expected);
            Helper.HelperRunComplex(comp, "CSQRT", Complex.Sqrt(new Complex(12, 3)));
        }

        // Verify TAN().
        [Test]
        public void ExtIntrTan() {
            string[] code = {
                "      FUNCTION RTAN",
                "        RTAN = TAN(0.4)",
                "        RETURN",
                "      END",
                "      FUNCTION DTAN",
                "        DOUBLE PRECISION DTAN, V",
                "        V = 0.4",
                "        DTAN = TAN(V)",
                "        RETURN",
                "      END"
            };
            FortranOptions opts = new();
            double expected = Math.Tan(0.4);
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunFloat(comp, "RTAN", (float)expected);
            Helper.HelperRunDouble(comp, "DTAN", expected);
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunFloat(comp, "RTAN", (float)expected);
            Helper.HelperRunDouble(comp, "DTAN", expected);
        }

        // Verify TANH().
        [Test]
        public void ExtIntrTanh() {
            string[] code = {
                "      FUNCTION RTANH",
                "        RETURN TANH(0.8)",
                "      END",
                "      FUNCTION DTANH",
                "        DOUBLE PRECISION DTANH, V",
                "        V = 0.8D0",
                "        DTANH = TANH(V)",
                "        RETURN",
                "      END"
            };
            FortranOptions opts = new();
            double expected = Math.Tanh(0.8);
            Compiler comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunFloat(comp, "RTANH", (float)expected);
            Helper.HelperRunDouble(comp, "DTANH", expected);
            opts.Inline = false;
            comp = FortranHelper.HelperCompile(code, opts);
            Helper.HelperRunFloat(comp, "RTANH", (float)expected);
            Helper.HelperRunDouble(comp, "DTANH", expected);
        }
    }
}