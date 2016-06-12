﻿using JetBrains.Annotations;

namespace DynamicSolver.Expressions.Expression
{
    public sealed class AssignmentBinaryOperator : BinaryOperator
    {
        public override string OperatorToken { get; } = "=";

        public AssignmentBinaryOperator([NotNull] IExpression leftOperand, [NotNull] IExpression rightOperand) : base(leftOperand, rightOperand)
        {
        }
    }
}