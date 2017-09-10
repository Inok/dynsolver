namespace DynamicSolver.Modelling.Solvers.Explicit
{
    public class RungeKutta4DynamicSystemSolver : ButcherTableDynamicSystemSolver
    {
        public override DynamicSystemSolverDescription Description { get; } = new DynamicSystemSolverDescription("Runge-Kutta 4", 4, false);

        private static readonly double[,] ACoefficients =
        {
            {0, 0, 0, 0},
            {1.0 / 2, 0, 0, 0},
            {0, 1.0 / 2, 0, 0},
            {0, 0, 1, 0}
        };

        private static readonly double[] BCoefficients = {1d / 6, 2d / 6, 2d / 6, 1d / 6};

        protected override double[,] A => ACoefficients;
        protected override double[] B => BCoefficients;
    }
}