using System;
using System.Collections.Generic;
using System.Linq;
using DynamicSolver.CoreMath.Execution.Compiler;
using DynamicSolver.CoreMath.Parser;
using DynamicSolver.DynamicSystem.Solvers;
using DynamicSolver.DynamicSystem.Solvers.Explicit;
using DynamicSolver.DynamicSystem.Solvers.Extrapolation;
using DynamicSolver.DynamicSystem.Solvers.SemiImplicit;
using DynamicSolver.DynamicSystem.Step;
using NUnit.Framework;

namespace DynamicSolver.DynamicSystem.Tests.Experiments
{
    [TestFixture, Explicit]
    public class ModellingMethodExperiments
    {
        private const int STEPS_COUNT = 10000;
        private const double STEP = 0.1;

        private IExplicitOrdinaryDifferentialEquationSystem _equationSystem;

        [SetUp]
        public void Setup()
        {
            var parser = new ExpressionParser();

            _equationSystem = new ExplicitOrdinaryDifferentialEquationSystem(new[]
                {
                    ExplicitOrdinaryDifferentialEquation.FromStatement(parser.Parse("x'= x*(2 + 0,456 - 10*(x^2 + y^2)) + z^2 + y^2 + 2*y")),
                    ExplicitOrdinaryDifferentialEquation.FromStatement(parser.Parse("y'= -z^3 - (1 + y)*(z^2 + y^2 + 2*y) -4*x + 0,456*y")),
                    ExplicitOrdinaryDifferentialEquation.FromStatement(parser.Parse("z' = (1+y)*z^2 + x^2 - 0,0357")),
                },
                new DynamicSystemState(0, new Dictionary<string, double>() {["x"] = 0.1, ["y"] = 0, ["z"] = -0.1}),
                new CompiledFunctionFactory());
        }

        [Test]
        public void Modelling()
        {
            IDynamicSystemSolver[] solvers =
            {
                /*new EulerDynamicSystemSolver(functionFactory), 
                new KDDynamicSystemSolver(functionFactory),

                new ExtrapolationEulerDynamicSystemSolver(functionFactory, 2),
                new ExtrapolationEulerDynamicSystemSolver(functionFactory, 4),
                new ExtrapolationEulerDynamicSystemSolver(functionFactory, 6),
                new ExtrapolationEulerDynamicSystemSolver(functionFactory, 8),

                new ExtrapolationKDDynamicSystemSolver(functionFactory, 1),
                new ExtrapolationKDDynamicSystemSolver(functionFactory, 2),
                new ExtrapolationKDDynamicSystemSolver(functionFactory, 3),
                new ExtrapolationKDDynamicSystemSolver(functionFactory, 4),*/

/*
                new RungeKutta4DynamicSystemSolver(functionFactory),
                new ExtrapolationEulerDynamicSystemSolver(functionFactory, 4),
                new ExtrapolationKDDynamicSystemSolver(functionFactory, 2),
*/
                
                new DormandPrince8DynamicSystemSolver(),
                new ExtrapolationSolver(new ExplicitEulerSolver(), 8), 
                new ExtrapolationSolver(new KDDynamicSystemSolver(), 4)
            };

            var stepStrategyFactory = new FixedStepStrategy(STEP);

            //warmup
            foreach (var solver in solvers)
            {
                for (var i = 0; i < 3; i++)
                {
                    using (var analyzer = new ExecutionTimeAnalyzer($"warmup {solver}"))
                    {
                        analyzer.StartIteration();
                        var count = solver.Solve(_equationSystem, stepStrategyFactory).Take(STEPS_COUNT).Count();
                    }
                }
            }
            Console.WriteLine();

            foreach (var solver in solvers)
            {
                using (var analyzer = new ExecutionTimeAnalyzer($"{solver,25}"))
                {
                    for (var run = 0; run < 10; run++)
                    {
                        analyzer.StartIteration();
                        var count = solver.Solve(_equationSystem, stepStrategyFactory).Take(STEPS_COUNT).Count();
                    }
                }
            }
        }
    }
}