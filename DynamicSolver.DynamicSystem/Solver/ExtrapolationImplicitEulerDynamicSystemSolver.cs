using System.Collections.Generic;
using DynamicSolver.Expressions.Execution;
using JetBrains.Annotations;

namespace DynamicSolver.DynamicSystem.Solver
{
/*
    public class ExtrapolationImplicitEulerDynamicSystemSolver : ImplicitEulerDynamicSystemSolver
    {
        [NotNull] private readonly Extrapolator _extrapolator;

        public ExtrapolationImplicitEulerDynamicSystemSolver([NotNull] IExecutableFunctionFactory functionFactory, int extrapolationStageCount) : base(functionFactory)
        {
            _extrapolator = new Extrapolator(extrapolationStageCount, 1);
        }

        protected override IReadOnlyDictionary<string, double> GetNextValues(IList<ExecutableFunctionInfo> functions, IReadOnlyDictionary<string, double> variables, double step, Dictionary<string, IExecutableFunction> extra)
        {
            return _extrapolator.Extrapolate(variables, step, (v, st) => base.GetNextValues(functions, v, st, extra));
        }

        public override string ToString()
        {
            return $"Implicit Euler Extrapolation {_extrapolator.ExtrapolationStageCount}";
        }

    }
*/
}