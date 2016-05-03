using System.Collections.Generic;
using System.Linq;
using DynamicSolver.Expressions.Execution;
using JetBrains.Annotations;

namespace DynamicSolver.DynamicSystem.Solver
{
    public class RungeKuttaButcherTableBasedDynamicSystemSolver : DynamicSystemSolver
    {
        private const int STAGE_COUNT = 4;

        private static readonly double[,] A = new double[STAGE_COUNT, STAGE_COUNT]
        {
            {0, 0, 0, 0},
            {1.0/2, 0, 0, 0},
            {0, 1.0/2, 0, 0},
            {0, 0, 1, 0}
        };

        private static readonly double[] B = new double[STAGE_COUNT]{ 1d/6, 2d/6, 2d/6, 1d/6};

        public RungeKuttaButcherTableBasedDynamicSystemSolver([NotNull] IExecutableFunctionFactory functionFactory, [NotNull] ExplicitOrdinaryDifferentialEquationSystem equationSystem) 
            : base(functionFactory, equationSystem)
        {
        }

        protected override double CalculateNextFunctionValue(ExecutableFunctionInfo function, IDictionary<string, double> variables, double step)
        {
            var k = new double[STAGE_COUNT];

            for (int s = 0; s < STAGE_COUNT; s++)
            {
                double increment = 0;

                for (int b = 0; b < s; b++)
                {
                    increment += A[s, b]*k[b];
                }

                var args = function.Function.OrderedArguments.Select(a => variables[a] + increment*step).ToArray();
                k[s] = function.Function.Execute(args);
            }

            double stepMultiplier = 0;
            for (int s = 0; s < STAGE_COUNT; s++)
            {
                stepMultiplier += k[s]*B[s];
            }

            return variables[function.Name] + step*stepMultiplier;
        }
    }
}