// JComLib
// Unit tests for Using function
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
// under the License

using NUnit.Framework;
using JComalLib;

namespace ComLibTests {
    [TestFixture]

    public class Using {

        // Test simple USING patterns.
        [Test]
        public void Using1() {
            object[] args = { 12.34f };
            Assert.AreEqual("12.34", Intrinsics.USING("##.##", args));
            Assert.AreEqual(" 12.340", Intrinsics.USING("###.###", args));
            Assert.AreEqual("*****", Intrinsics.USING("#.###", args));
        }

        // Test USING with negative numbers.
        [Test]
        public void Using2() {
            object[] args = { -89.45f };
            Assert.AreEqual("-89.45", Intrinsics.USING("-##.##", args));
            Assert.AreEqual(" -89.450", Intrinsics.USING("-###.###", args));
            Assert.AreEqual("-89.450", Intrinsics.USING("###.###", args));
        }

        // Test USING with extra characters.
        [Test]
        public void Using3() {
            object[] args = { 67543 };
            Assert.AreEqual("The number is 67543 exactly", Intrinsics.USING("The number is ##### exactly", args));
            Assert.AreEqual("£67543.00ex.VAT", Intrinsics.USING("£#####.##ex.VAT", args));
            Assert.AreEqual("Overflow ****test", Intrinsics.USING("Overflow ####test", args));
        }

        // Pathological cases.
        [Test]
        public void Using4() {
            object[] args = { 0.78 };
            Assert.AreEqual("", Intrinsics.USING("", args));
            Assert.AreEqual("-", Intrinsics.USING("-", args));
            Assert.AreEqual("---...---", Intrinsics.USING("---...---", args));
            Assert.AreEqual("....", Intrinsics.USING("....", args));
        }
    }
}
