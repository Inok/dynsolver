﻿using JetBrains.Annotations;

namespace DynamicSolver.CoreMath.Expression
{
    public sealed class DivideBinaryOperator : BinaryOperator
    {
        public override string OperatorToken { get; } = "/";

        public DivideBinaryOperator([NotNull] IExpression leftOperand, [NotNull] IExpression rightOperand) : base(leftOperand, rightOperand)
        {
        }
    }
}