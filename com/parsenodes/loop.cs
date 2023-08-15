// JCom Compiler Toolkit
// Loops parse node
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

using System.Reflection.Emit;
using JComLib;

namespace CCompiler {

    /// <summary>
    /// Specifies a parse node that defines a loop.
    /// </summary>
    public sealed class LoopParseNode : ParseNode {

        /// <summary>
        /// Creates a loop parse node.
        /// </summary>
        public LoopParseNode() : base(ParseID.LOOP) {
            StepExpression = new NumberParseNode(1);
        }

        /// <summary>
        /// Gets or sets the parse node containing the loop variable.
        /// </summary>
        /// <value>The loop variable parse node.</value>
        public IdentifierParseNode LoopVariable { get; set; }

        /// <summary>
        /// Gets or sets the parse node containing the single expression
        /// which is evaluated through the loop.
        /// </summary>
        /// <value>The loop value expression parse node.</value>
        public ParseNode LoopValue { get; set; }

        /// <summary>
        /// Gets or sets the loop start expression.
        /// </summary>
        /// <value>The parse node for the loop start expression.</value>
        public ParseNode StartExpression { get; set; }

        /// <summary>
        /// Gets or sets the loop end expression.
        /// </summary>
        /// <value>The parse node for the loop end expression.</value>
        public ParseNode EndExpression { get; set; }

        /// <summary>
        /// Gets or sets the loop step rate expression.
        /// </summary>
        /// <value>The parse node for the loop step expression.</value>
        public ParseNode StepExpression { get; set; }

        /// <summary>
        /// Gets or sets the parse tree that will be evaluated as part
        /// of the loop body passing through the loop value.
        /// </summary>
        /// <value>The parse node for the callback</value>
        public ParseNode Callback { get; set; }

        /// <summary>
        /// Gets or sets the parse block for the loop body.
        /// </summary>
        /// <value>The parse node for the loop body.</value>
        public BlockParseNode LoopBody { get; set; }

        /// <summary>
        /// Loop exit label for EXIT statement
        /// </summary>
        public Label ExitLabel { get; set; }

        /// <summary>
        /// Returns the iteration count as a positive integer, or -1 if
        /// the count cannot be determined.
        /// </summary>
        /// <returns>The iteration count, or -1</returns>
        public int IterationCount() {
            if (EndExpression == null || StartExpression == null || StepExpression == null) {
                return -1;
            }
            if (EndExpression.IsConstant && StartExpression.IsConstant && StepExpression.IsConstant) {
                if (LoopVariable != null) {
                    Symbol sym = LoopVariable.Symbol;
                    if (sym.Type == SymType.INTEGER) {
                        int startValue = StartExpression.Value.IntValue;
                        int endValue = EndExpression.Value.IntValue;
                        int stepValue = StepExpression.Value.IntValue;
                        return Math.Max(0, (endValue - startValue + stepValue) / stepValue);
                    }
                    if (sym.Type == SymType.FLOAT) {
                        float startValue = StartExpression.Value.RealValue;
                        float endValue = EndExpression.Value.RealValue;
                        float stepValue = StepExpression.Value.RealValue;
                        return Math.Max(0, (int)((endValue - startValue + stepValue) / stepValue));
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// Dumps the contents of this parse node to the ParseNode XML
        /// output under the specified parent node.
        /// </summary>
        /// <param name="root">The parent XML node</param>
        public override void Dump(ParseNodeXml root) {
            ParseNodeXml blockNode = root.Node("Loop");
            if (LoopVariable != null) {
                LoopVariable.Dump(blockNode.Node("LoopVariable"));
            }
            if (LoopValue != null) {
                LoopValue.Dump(blockNode.Node("LoopValue"));
            }
            if (StartExpression != null) {
                StartExpression.Dump(blockNode.Node("StartExpression"));
            }
            if (EndExpression != null) {
                EndExpression.Dump(blockNode.Node("EndExpression"));
            }
            if (StepExpression != null) {
                StepExpression.Dump(blockNode.Node("StepExpression"));
            }
            if (LoopBody != null) {
                LoopBody.Dump(blockNode.Node("LoopBody"));
            }
        }

        /// <summary>
        /// Emit the code to generate a loop.
        /// If there is a loop variable, we generate a FOR..NEXT loop
        /// If there is no end expression, we generate a WHILE loop.
        /// Otherwise we generate a REPEAT loop.
        /// </summary>
        /// <param name="emitter">Code emitter</param>
        /// <param name="cg">A code generator object</param>
        public override void Generate(Emitter emitter, ProgramParseNode cg) {
            if (LoopVariable != null) {
                GenerateForNextLoop(emitter, cg);
            } else if (EndExpression == null) {
                GenerateWhileLoop(emitter, cg);
            } else {
                GenerateRepeatLoop(emitter, cg);
            }
        }

        /// <summary>
        /// Emit the code to generate a while loop.
        /// </summary>
        /// <param name="emitter">Code emitter</param>
        /// <param name="cg">A code generator object</param>
        private void GenerateWhileLoop(Emitter emitter, ProgramParseNode cg) {
            Label loopStart = emitter.CreateLabel();
            ExitLabel = emitter.CreateLabel();

            // Skip the loop if the expression is a constant false
            if (StartExpression.IsConstant && !StartExpression.Value.BoolValue) {
                return;
            }

            emitter.MarkLabel(loopStart);

            // If this is a WHILE(true) loop, skip the initial test. 
            if (!(StartExpression.IsConstant && StartExpression.Value.BoolValue)) {
                cg.GenerateExpression(emitter, SymType.BOOLEAN, StartExpression);
                emitter.BranchIfFalse(ExitLabel);
            }

            LoopBody.Generate(emitter, cg);

            emitter.Branch(loopStart);
            emitter.MarkLabel(ExitLabel);
        }

        /// <summary>
        /// Emit the code to generate a repeat loop.
        /// </summary>
        /// <param name="emitter">Code emitter</param>
        /// <param name="cg">A code generator object</param>
        private void GenerateRepeatLoop(Emitter emitter, ProgramParseNode cg) {
            Label loopStart = emitter.CreateLabel();

            emitter.MarkLabel(loopStart);

            LoopBody.Generate(emitter, cg);

            cg.GenerateExpression(emitter, SymType.BOOLEAN, EndExpression);
            emitter.BranchIfFalse(loopStart);
        }

        /// <summary>
        /// Emit the code to generate for...next style loop.
        /// 
        /// The number of iterations is always fixed as per this formula:
        ///
        ///  iterations = MAX(0,INT((final-value - initial-value + step-size)/step-size)
        ///
        /// Thus we compute final-value, initial-value and step-size up-front so that if they
        /// reference a variable that changes within the loop body, we ignore those changes.
        /// </summary>
        /// <param name="emitter">Code emitter</param>
        /// <param name="cg">A code generator object</param>
        private void GenerateForNextLoop(Emitter emitter, ProgramParseNode cg) {
            Symbol sym = LoopVariable.Symbol;
            Label maxBranch = emitter.CreateLabel();
            Label loopStart = emitter.CreateLabel();
            Label loopEnd = emitter.CreateLabel();

            LocalDescriptor iterCount = emitter.GetTemporary(typeof(int));
            LocalDescriptor stepVar = emitter.GetTemporary(sym.SystemType);

            // Check for constant start, end and step expressions and simplify
            // the loop if so. If the loop iterations evaluate to zero then we skip
            // code generation altogether. However we do need to ensure that the loop
            // variable is initialised to the start value.
            int iterValue = IterationCount();
            if (iterValue >= 0) {
                if (sym.Type == SymType.INTEGER) {
                    emitter.LoadInteger(StartExpression.Value.IntValue);
                    emitter.StoreLocal(sym);
                }
                if (sym.Type == SymType.FLOAT) {
                    emitter.LoadFloat(StartExpression.Value.RealValue);
                    emitter.StoreLocal(sym);
                }
                if (iterValue == 0) {
                    return;
                }
                emitter.LoadInteger(iterValue);
            } else {
                GenerateLoopCount(emitter, cg, sym, stepVar);
                emitter.Dup();
                emitter.LoadInteger(0);
                emitter.BranchGreater(maxBranch);
                emitter.Pop();
                emitter.Branch(loopEnd);
                emitter.MarkLabel(maxBranch);
            }
            emitter.MarkLabel(loopStart);
            emitter.StoreLocal(iterCount);

            // The loop value is generally used as part of an implied
            // DO loop where the expression uses the loop variable as one
            // of its factors.
            if (LoopValue != null) {
                Callback.Generate(emitter, cg, LoopValue);
            }

            if (LoopBody != null) {
                LoopBody.Generate(emitter, cg);
            }

            emitter.LoadSymbol(sym);
            if (StepExpression.IsConstant) {
                if (sym.Type == SymType.INTEGER) {
                    emitter.LoadInteger(StepExpression.Value.IntValue);
                }
                if (sym.Type == SymType.FLOAT) {
                    emitter.LoadFloat(StepExpression.Value.RealValue);
                }
            } else {
                emitter.LoadLocal(stepVar);
            }
            emitter.Add(sym.Type);
            emitter.StoreLocal(sym);
            emitter.LoadLocal(iterCount);
            emitter.LoadInteger(1);
            emitter.Sub(sym.Type);
            emitter.Dup();
            emitter.BranchIfTrue(loopStart);
            emitter.Pop();
            emitter.MarkLabel(loopEnd);
            Emitter.ReleaseTemporary(iterCount);
            Emitter.ReleaseTemporary(stepVar);
        }

        // Emit the code that computes the number of iterations that the
        // loop requires.
        private void GenerateLoopCount(Emitter emitter, ProgramParseNode cg, Symbol sym, LocalDescriptor stepVar) {
            cg.GenerateExpression(emitter, sym.Type, EndExpression);
            if (StartExpression.IsConstant && StartExpression.Value.IntValue == 0) {
                // Start index is zero so no initial subtraction required.
                cg.GenerateExpression(emitter, sym.Type, StartExpression);
                emitter.StoreLocal(sym);
            } else {
                cg.GenerateExpression(emitter, sym.Type, StartExpression);
                emitter.Dup();
                emitter.StoreLocal(sym);
                emitter.Sub(sym.Type);
            }
            if (StepExpression.IsConstant) {
                int stepValue = StepExpression.Value.IntValue;
                emitter.LoadValue(sym.Type, new Variant(stepValue));
                emitter.Add(sym.Type);
                if (Math.Abs(stepValue) != 1) {
                    emitter.ConvertType(sym.Type, SymType.INTEGER);
                    emitter.LoadInteger(stepValue);
                    emitter.Div(SymType.INTEGER);
                }
            } else {
                cg.GenerateExpression(emitter, sym.Type, StepExpression);
                emitter.Dup();
                emitter.StoreLocal(stepVar);
                emitter.Add(sym.Type);
                emitter.LoadLocal(stepVar);
                emitter.Div(sym.Type);
            }
            emitter.ConvertType(sym.Type, SymType.INTEGER);
        }
    }
}
