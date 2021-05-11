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

using System;
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
        /// Gets or sets the parse node for the loop body.
        /// </summary>
        /// <value>The parse node for the loop body.</value>
        public CollectionParseNode LoopBody { get; set; }

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
                blockNode = blockNode.Node("LoopBody");
                foreach (ParseNode node in LoopBody.Nodes) {
                    node.Dump(blockNode);
                }
            }
        }

        /// <summary>
        /// Emit the code to generate a loop.
        /// If there is a loop variable, we generate a FOR..NEXT loop
        /// If there is no end expression, we generate a WHILE loop.
        /// Otherwise we generate a REPEAT loop.
        /// </summary>
        /// <param name="cg">A code generator object</param>
        public override void Generate(CodeGenerator cg) {
            if (LoopVariable != null) {
                GenerateForNextLoop(cg);
            } else if (EndExpression == null) {
                GenerateWhileLoop(cg);
            } else {
                GenerateRepeatLoop(cg);
            }
        }

        /// <summary>
        /// Emit the code to generate a while loop.
        /// </summary>
        /// <param name="cg">A code generator object</param>
        private void GenerateWhileLoop(CodeGenerator cg) {
            Label loopStart = cg.Emitter.CreateLabel();
            ExitLabel = cg.Emitter.CreateLabel();

            // Skip the loop if the expression is a constant false
            if (StartExpression.IsConstant && !StartExpression.Value.BoolValue) {
                return;
            }

            cg.Emitter.MarkLabel(loopStart);

            // If this is a WHILE(true) loop, skip the initial test. 
            if (!(StartExpression.IsConstant && StartExpression.Value.BoolValue)) {
                cg.GenerateExpression(SymType.BOOLEAN, StartExpression);
                cg.Emitter.BranchIfFalse(ExitLabel);
            }
            CollectionParseNode block = LoopBody;
            foreach (ParseNode t in block.Nodes) {
                t.Generate(cg);
            }
            cg.Emitter.Branch(loopStart);
            cg.Emitter.MarkLabel(ExitLabel);
        }

        /// <summary>
        /// Emit the code to generate a repeat loop.
        /// </summary>
        /// <param name="cg">A code generator object</param>
        private void GenerateRepeatLoop(CodeGenerator cg) {
            Label loopStart = cg.Emitter.CreateLabel();

            cg.Emitter.MarkLabel(loopStart);
            CollectionParseNode block = LoopBody;
            foreach (ParseNode t in block.Nodes) {
                t.Generate(cg);
            }
            cg.GenerateExpression(SymType.BOOLEAN, EndExpression);
            cg.Emitter.BranchIfFalse(loopStart);
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
        /// <param name="cg">A code generator object</param>
        private void GenerateForNextLoop(CodeGenerator cg) {
            Symbol sym = LoopVariable.Symbol;
            Label maxBranch = cg.Emitter.CreateLabel();
            Label loopStart = cg.Emitter.CreateLabel();
            Label loopEnd = cg.Emitter.CreateLabel();

            LocalDescriptor iterCount = cg.Emitter.GetTemporary(typeof(int));
            LocalDescriptor stepVar = cg.Emitter.GetTemporary(sym.SystemType);

            // Check for constant start, end and step expressions and simplify
            // the loop if so. If the loop iterations evaluate to zero then we skip
            // code generation altogether. However we do need to ensure that the loop
            // variable is initialised to the start value.
            int iterValue = IterationCount();
            if (iterValue >= 0) {
                if (sym.Type == SymType.INTEGER) {
                    cg.Emitter.LoadInteger(StartExpression.Value.IntValue);
                    cg.Emitter.StoreLocal(sym.Index);
                }
                if (sym.Type == SymType.FLOAT) {
                    cg.Emitter.LoadFloat(StartExpression.Value.RealValue);
                    cg.Emitter.StoreLocal(sym.Index);
                }
                if (iterValue == 0) {
                    return;
                }
                cg.Emitter.LoadInteger(iterValue);
            } else {
                GenerateLoopCount(cg, sym, stepVar);
                cg.Emitter.Dup();
                cg.Emitter.LoadInteger(0);
                cg.Emitter.BranchGreater(maxBranch);
                cg.Emitter.Pop();
                cg.Emitter.Branch(loopEnd);
                cg.Emitter.MarkLabel(maxBranch);
            }
            cg.Emitter.MarkLabel(loopStart);
            cg.Emitter.StoreLocal(iterCount);

            // The loop value is generally used as part of an implied
            // DO loop where the expression uses the loop variable as one
            // of its factors.
            if (LoopValue != null) {
                Callback.Generate(cg, LoopValue);
            }

            if (LoopBody != null) {
                CollectionParseNode block = LoopBody;
                foreach (ParseNode t in block.Nodes) {
                    t.Generate(cg);
                }
            }

            cg.Emitter.LoadLocal(sym.Index);
            if (StepExpression.IsConstant) {
                if (sym.Type == SymType.INTEGER) {
                    cg.Emitter.LoadInteger(StepExpression.Value.IntValue);
                }
                if (sym.Type == SymType.FLOAT) {
                    cg.Emitter.LoadFloat(StepExpression.Value.RealValue);
                }
            } else {
                cg.Emitter.LoadLocal(stepVar);
            }
            cg.Emitter.Add(sym.Type);
            cg.Emitter.StoreLocal(sym.Index);
            cg.Emitter.LoadLocal(iterCount);
            cg.Emitter.LoadInteger(1);
            cg.Emitter.Sub(sym.Type);
            cg.Emitter.Dup();
            cg.Emitter.BranchIfTrue(loopStart);
            cg.Emitter.Pop();
            cg.Emitter.MarkLabel(loopEnd);
            cg.Emitter.ReleaseTemporary(iterCount);
            cg.Emitter.ReleaseTemporary(stepVar);
        }

        // Emit the code that computes the number of iterations that the
        // loop requires.
        private void GenerateLoopCount(CodeGenerator cg, Symbol sym, LocalDescriptor stepVar) {
            cg.GenerateExpression(sym.Type, EndExpression);
            if (StartExpression.IsConstant && StartExpression.Value.IntValue == 0) {
                // Start index is zero so no initial subtraction required.
                cg.GenerateExpression(sym.Type, StartExpression);
                cg.Emitter.StoreLocal(sym.Index);
            } else {
                cg.GenerateExpression(sym.Type, StartExpression);
                cg.Emitter.Dup();
                cg.Emitter.StoreLocal(sym.Index);
                cg.Emitter.Sub(sym.Type);
            }
            if (StepExpression.IsConstant) {
                int stepValue = StepExpression.Value.IntValue;
                cg.Emitter.LoadValue(sym.Type, new Variant(stepValue));
                cg.Emitter.Add(sym.Type);
                if (stepValue != 1) {
                    cg.Emitter.LoadInteger(stepValue);
                    cg.Emitter.Div(sym.Type);
                }
            } else {
                cg.GenerateExpression(sym.Type, StepExpression);
                cg.Emitter.Dup();
                cg.Emitter.StoreLocal(stepVar);
                cg.Emitter.Add(sym.Type);
                cg.Emitter.LoadLocal(stepVar);
                cg.Emitter.Div(sym.Type);
            }
            cg.Emitter.ConvertType(sym.Type, SymType.INTEGER);
        }
    }
}
