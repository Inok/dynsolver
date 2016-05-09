using DynamicSolver.Expressions.Execution;
using JetBrains.Annotations;

namespace DynamicSolver.DynamicSystem.Solver
{
    public class DormanPrinceDynamicSystemSolver : ButcherTableDynamicSystemSolver
    {
        private static readonly double[,] ACoefficients = {
            {0, 0, 0, 0},
            {1.0/2, 0, 0, 0},
            {0, 1.0/2, 0, 0},
            {0, 0, 1, 0}
        };

        private static readonly double[] BCoefficients = { 1d / 6, 2d / 6, 2d / 6, 1d / 6 };

        protected override double[,] A => ACoefficients;
        protected override double[] B => BCoefficients;

        public DormanPrinceDynamicSystemSolver([NotNull] IExecutableFunctionFactory functionFactory) : base(functionFactory)
        {
        }
    }
}