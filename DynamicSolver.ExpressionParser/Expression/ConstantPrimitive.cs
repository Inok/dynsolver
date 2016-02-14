using System;
using DynamicSolver.Abstractions.Expression;
using JetBrains.Annotations;

namespace DynamicSolver.ExpressionParser.Expression
{
    public class ConstantPrimitive : IPrimitive
    {
        [NotNull]
        public Constant Constant { get; }

        public ConstantPrimitive([NotNull] Constant constant)
        {
            if (constant == null) throw new ArgumentNullException(nameof(constant));
            Constant = constant;
        }

        public override string ToString()
        {
            return $"{Constant.Name}";
        }
    }
}