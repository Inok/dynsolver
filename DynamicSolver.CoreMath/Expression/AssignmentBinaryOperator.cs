﻿using JetBrains.Annotations;

namespace DynamicSolver.CoreMath.Expression
{
    public sealed class AssignmentBinaryOperator : BinaryOperator
    {
        protected override string OperatorToken { get; } = "=";

        public AssignmentBinaryOperator([NotNull] IExpression leftOperand, [NotNull] IExpression rightOperand) : base(leftOperand, rightOperand)
        {
        }
    }
}