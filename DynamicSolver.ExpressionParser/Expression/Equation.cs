using System;
using DynamicSolver.Abstractions.Expression;
using JetBrains.Annotations;

namespace DynamicSolver.ExpressionParser.Expression
{
    public class Equation : IEquation
    {
        public IVariablePrimitive Variable { get; }

        public IExpression Expression { get; }

        public Equation([NotNull] IVariablePrimitive variable, [NotNull] IExpression expression)
        {
            if (variable == null) throw new ArgumentNullException(nameof(variable));
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            Variable = variable;
            Expression = expression;
        }
    }
}