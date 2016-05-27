using DynamicSolver.Expressions.Execution;
using JetBrains.Annotations;

namespace DynamicSolver.DynamicSystem.Solver
{
    public class DormandPrince5DynamicSystemSolver : ButcherTableDynamicSystemSolver
    {
        private static readonly double[,] ACoefficients =
        {
            {           0,            0,           0,           0,            0,      0,    0},
            {        1d/5,            0,           0,           0,            0,      0,    0},
            {       3d/40,        9d/40,           0,           0,            0,      0,    0},
            {      44d/45,      -56d/15,       32d/9,           0,            0,      0,    0},
            { 19372d/6561, -25360d/2187, 64448d/6561,   -212d/729,            0,      0,    0},
            {  9017d/3168,     -355d/33, 46732d/5247,     49d/176, -5103d/18656,      0,    0},
            {     35d/384,           0d,   500d/1113,    125d/192,  -2187d/6784, 11d/84,    0}
        };

        private static readonly double[] BCoefficients =
        {
            35d/384, 0d, 500d/1113, 125d/192, -2187d/6784, 11d/84, 0d
        };

        protected override double[,] A => ACoefficients;
        protected override double[] B => BCoefficients;

        public DormandPrince5DynamicSystemSolver([NotNull] IExecutableFunctionFactory functionFactory) : base(functionFactory)
        {
        }
    }
}