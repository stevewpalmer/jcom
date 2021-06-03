// JCom Compiler Toolkit
// Peephole optimisation of MSIL code
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
using System.Reflection.Emit;

namespace CCompiler {

    /// <summary>
    /// Defines and represents a Peephole optimiser and a set of static functions
    /// for performing peephole optimisation on a sequence of Instruction objects.
    /// </summary>
    public static class Peephole {

        /// <summary>
        /// The Lifetime class is used to track local variable lifetime
        /// when used with the OptimiseLocalUsage function.
        /// </summary>
        private sealed class Lifetime {

            /// <summary>
            /// Initialises a new Lifetime object using the specified local
            /// variable and the start index in the code..
            /// </summary>
            /// <param name="localVar">Local variable.</param>
            /// <param name="startIndex">Start index.</param>
            public Lifetime(LocalDescriptor localVar, int startIndex) {
                LocalVar = localVar;
                StartIndex = startIndex;
            }

            public LocalDescriptor LocalVar { get; private set; }
            public int StartIndex { get; private set; }
            public int EndIndex { get; set; }
        }

        /// <summary>
        /// Perform a sequence of peephole (localised) optimisation on the code. The code
        /// is generally expected to be self contained, with its own locals and arguments
        /// and terminate with a return instruction. Globals are disregarded as we have no
        /// access to those.
        /// </summary>
        /// <param name="code">A list of MSIL instructions.</param>
        public static void Optimise(Collection<Instruction> code) {
            //OptimiseLocalUsage(code);
            OptimiseBranch(code);
            OptimiseNotBranch(code);
            OptimiseReturn(code);
            OptimiseLoadStore(code);
            OptimiseReturnTail(code);
        }

        // Optimise a BRANCH to a label that is immediately following by removing
        // that branch.
        //
        private static void OptimiseReturn(Collection<Instruction> code) {

            for (int c = 0; c < code.Count - 2; ++c) {
                if (code[c].Code == OpCodes.Br && code[c + 1] is InstructionLabelMarker) {
                    InstructionLabel brLabel = code[c] as InstructionLabel;
                    InstructionLabelMarker label = code[c + 1] as InstructionLabelMarker;
                    if (brLabel.Target == label.Target) {
                        code[c].Deleted = true;
                    }
                }
            }
        }

        // Optimise a RETURN instruction that is the result of a comparision. This occurs when
        // doing:
        //
        //    IF expr1 <> expr2 RETURN
        //
        // Here the <> evaluates to a CEQ followed by an XOR(1) to inverse the result. This
        // optimisation replaces this with a direct true branch if they are equal.
        //
        private static void OptimiseBranch(Collection<Instruction> code) {

            for (int c = 0; c < code.Count - 5; ++c) {
                if (code[c].Code == OpCodes.Ceq &&
                    code[c + 1].Code == OpCodes.Ldc_I4 &&
                    code[c + 2].Code == OpCodes.Xor &&
                    code[c + 3].Code == OpCodes.Conv_I1 &&
                    code[c + 4].Code == OpCodes.Brfalse) {

                    InstructionLabel brFalse = (InstructionLabel)code[c + 4];
                    code[c] = new InstructionLabel(OpCodes.Beq, brFalse.Target) {
                        Code = OpCodes.Beq
                    };
                    code[c + 1].Deleted = true;
                    code[c + 2].Deleted = true;
                    code[c + 3].Deleted = true;
                    code[c + 4].Deleted = true;
                    c += 5;
                }
            }
        }

        // Optimise the following sequence to reduce the NOT and BRFALSE to a simple
        // BRTRUE call. This can occur because the CALL/NOT and BRFALSE may be widely
        // separated in the parse tree. This can also occur as the result of previous
        // optimisation.
        //
        //    CALL to boolean function
        //    NOT
        //    BRFALSE [label]
        //
        private static void OptimiseNotBranch(Collection<Instruction> code) {

            for (int c = 0; c < code.Count - 2; ++c) {
                if (code[c].Code == OpCodes.Not && code[c + 1].Code == OpCodes.Brfalse) {

                    InstructionLabel brFalse = (InstructionLabel)code[c + 1];
                    code[c] = new InstructionLabel(OpCodes.Brtrue, brFalse.Target) {
                        Code = OpCodes.Brtrue
                    };
                    code[c + 1].Deleted = true;
                    c += 2;
                }
            }
        }

        // Look for non-overlapping local variable usage of the same type which implies we can
        // reuse the storage rather than allocating more space.
        //
        // BUGBUG: This is turned off right now because it needs to handle branches which
        // occur in loops, which extend the lifetime of all variables within the block to the
        // end of that block.
        //
        private static void OptimiseLocalUsage(Collection<Instruction> code) {
            Dictionary<int, Lifetime> lifetimeSet = new();

            for (int c = 0; c < code.Count; ++c) {
                if (code[c].Code == OpCodes.Stloc ||
                    code[c].Code == OpCodes.Ldloc ||
                    code[c].Code == OpCodes.Ldloca) {
                    InstructionLocal loadInt = (InstructionLocal)code[c];
                    LocalDescriptor localVar = loadInt.Value;
                    Lifetime life;

                    if (!lifetimeSet.ContainsKey(localVar.Index)) {
                        life = new Lifetime(localVar, c);
                        lifetimeSet.Add(localVar.Index, life);
                    }

                    life = lifetimeSet[localVar.Index];
                    life.EndIndex = c;
                }
                if (code[c].Code == OpCodes.Ret) {
                    List<Lifetime> sortedLifetimeList = new(lifetimeSet.Values);
                    sortedLifetimeList.Sort(
                        delegate(Lifetime a, Lifetime b) {
                            return a.StartIndex.CompareTo(b.StartIndex);
                    });

                    // Now scan the sorted list. Any variable whose start index comes after
                    // the end index of the preceding variable can be replaced with the
                    // preceding variable index as long as types match.
                    for (int m = 1; m < sortedLifetimeList.Count; ++m) {
                        if (sortedLifetimeList[m].StartIndex > sortedLifetimeList[m-1].EndIndex &&
                            sortedLifetimeList[m].LocalVar.Type == sortedLifetimeList[m-1].LocalVar.Type) {
                            Console.WriteLine("Replaced local var {0} with {1}", sortedLifetimeList[m].LocalVar.Index, sortedLifetimeList[m-1].LocalVar.Index);
                            sortedLifetimeList[m].LocalVar.Index = sortedLifetimeList[m-1].LocalVar.Index;
                        }
                    }
                    lifetimeSet = new Dictionary<int, Lifetime>();
                }
            }
        }

        // Look for a store and load and use a dup to avoid the second load.
        //
        //     Store to local X
        //     Load from local X
        //
        // to:
        //
        //     Dup
        //     Store to local X
        //
        //
        private static void OptimiseLoadStore(Collection<Instruction> code) {
            for (int c = 0; c < code.Count - 2; ++c) {
                if (code[c].Code == OpCodes.Stloc && code[c + 1].Code == OpCodes.Ldloc) {
                    InstructionLocal storeInt = (InstructionLocal)code[c];
                    InstructionLocal loadInt = (InstructionLocal)code[c + 1];
                    if (loadInt.Value.Index == storeInt.Value.Index) {
                        code[c + 1] = code[c];
                        code[c] = new Instruction(OpCodes.Dup);
                    }
                    return;
                }
                if (code[c].Code == OpCodes.Stsfld && code[c + 1].Code == OpCodes.Ldsfld) {
                    InstructionField storeInt = (InstructionField)code[c];
                    InstructionField loadInt = (InstructionField)code[c + 1];
                    if (loadInt.Field == storeInt.Field) {
                        code[c + 1] = code[c];
                        code[c] = new Instruction(OpCodes.Dup);
                    }
                }
            }
        }

        // Look for redundant stack save/load just before the return statement. These typically are
        // of the form:
        //
        //     [Expression Evaluation that pushes a result to the stack]
        //     Store to local X
        //     Load from local X
        //     Return
        //
        // The store/load can be deleted. This typically occurs when the expression stores to
        // a function variable, followed by a return statement that loads the same function variable
        // to return to the caller.
        //
        private static void OptimiseReturnTail(Collection<Instruction> code) {
            int indexLast1 = -1;
            int indexLast2 = -1;
            for (int c = 0; c < code.Count; ++c) {
                if (code[c].Code == OpCodes.Ret && indexLast1 > 0 && indexLast2 > 0) {
                    if (code[indexLast2].Code == OpCodes.Stloc_0 && code[indexLast1].Code == OpCodes.Ldloc_0) {
                        code[indexLast1].Deleted = true;
                        code[indexLast2].Deleted = true;
                        return;
                    }
                    if (code[indexLast2].Code == OpCodes.Stloc_1 && code[indexLast1].Code == OpCodes.Ldloc_1) {
                        code[indexLast1].Deleted = true;
                        code[indexLast2].Deleted = true;
                        return;
                    }
                    if (code[indexLast2].Code == OpCodes.Stloc_2 && code[indexLast1].Code == OpCodes.Ldloc_2) {
                        code[indexLast1].Deleted = true;
                        code[indexLast2].Deleted = true;
                        return;
                    }
                    if (code[indexLast2].Code == OpCodes.Stloc_3 && code[indexLast1].Code == OpCodes.Ldloc_3) {
                        code[indexLast1].Deleted = true;
                        code[indexLast2].Deleted = true;
                        return;
                    }
                    if (code[indexLast2].Code == OpCodes.Stloc && code[indexLast1].Code == OpCodes.Ldloc) {
                        InstructionLocal storeInt = (InstructionLocal)code[indexLast2];
                        InstructionLocal loadInt = (InstructionLocal)code[indexLast1];
                        if (loadInt.Value.Index == storeInt.Value.Index) {
                            code[indexLast1].Deleted = true;
                            code[indexLast2].Deleted = true;
                        }
                        return;
                    }
                }
                InstructionTryCatch instructionTryCatch = code[c] as InstructionTryCatch;
                if (instructionTryCatch != null && indexLast1 > 0) {
                    InstructionTryCatch tryCatch = instructionTryCatch;
                    if (tryCatch.TryCatchType == EmitExceptionHandlerType.Catch && code[indexLast1].Code == OpCodes.Ret) {
                        code[indexLast1].Deleted = true;
                    }
                }
                if (code[c].Code.OpCodeType == OpCodeType.Primitive) {
                    indexLast2 = indexLast1;
                    indexLast1 = c;
                }
                if (code[c] is InstructionLabelMarker) {
                    indexLast1 = -1;
                    indexLast2 = -1;
                }
            }
        }
    }
}
