using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DynamicSolver.CoreMath.Execution.Compiler;
using DynamicSolver.CoreMath.Parser;
using DynamicSolver.DynamicSystem.Solvers;
using DynamicSolver.DynamicSystem.Solvers.Explicit;
using DynamicSolver.DynamicSystem.Solvers.Extrapolation;
using DynamicSolver.DynamicSystem.Solvers.SemiImplicit;
using NUnit.Framework;

namespace DynamicSolver.DynamicSystem.Tests.Experiments
{
    [TestFixture, Explicit]
    public class ModellingMethodExperiments
    {
        private const int STEPS_COUNT = 10000;
        private const double STEP = 0.01;

        private IExplicitOrdinaryDifferentialEquationSystem _equationSystem;
        private DynamicSystemState _initialState;

        [SetUp]
        public void Setup()
        {
            var parser = new ExpressionParser();

            _equationSystem = new ExplicitOrdinaryDifferentialEquationSystem(new[]
                {
                    ExplicitOrdinaryDifferentialEquation.FromExpression(parser.Parse("x'= x*(2 + 0,456 - 10*(x^2 + y^2)) + z^2 + y^2 + 2*y")),
                    ExplicitOrdinaryDifferentialEquation.FromExpression(parser.Parse("y'= -z^3 - (1 + y)*(z^2 + y^2 + 2*y) -4*x + 0,456*y")),
                    ExplicitOrdinaryDifferentialEquation.FromExpression(parser.Parse("z' = (1+y)*z^2 + x^2 - 0,0357")),
                },
                new CompiledFunctionFactory());

            _initialState = new DynamicSystemState(0, new Dictionary<string, double>() {["x"] = 0.1, ["y"] = 0, ["z"] = -0.1});
        }

        [Test]
        public void ModellingTime()
        {
            const bool parallelize = true;

            IDynamicSystemSolver[] solvers =
            {
/*
                new ExtrapolationSolver(new ExplicitEulerSolver(), 2, parallelize),
                new ExtrapolationSolver(new ExplicitEulerSolver(), 4, parallelize),
                new ExtrapolationSolver(new ExplicitEulerSolver(), 8, parallelize),

                new ExtrapolationSolver(new KDFirstExplicitDynamicSystemSolver(), 2, parallelize),
                new ExtrapolationSolver(new KDFirstExplicitDynamicSystemSolver(), 4, parallelize),
                new ExtrapolationSolver(new KDFirstExplicitDynamicSystemSolver(), 8, parallelize),

                new SymmetricExplicitMiddlePointExtrapolationSolver(2, parallelize),
                new SymmetricExplicitMiddlePointExtrapolationSolver(4, parallelize),
                new SymmetricExplicitMiddlePointExtrapolationSolver(8, parallelize),
                
*/

/*
                new DormandPrince8DynamicSystemSolver(),
                new ExtrapolationSolver(new ExplicitEulerSolver(), 8),
                new SymmetricExplicitMiddlePointExtrapolationSolver(4),
                new ExtrapolationSolver(new KDFirstExplicitDynamicSystemSolver(), 4),
*/

                new RungeKutta4DynamicSystemSolver(),
                new ExtrapolationSolver(new ExplicitEulerSolver(), 4),
                new ExtrapolationSolver(new SymmetricExplicitMiddlePointDynamicSystemSolver(), 2)


            };

            //warmup
            foreach (var solver in solvers)
            {
                using (var analyzer = new ExecutionTimeAnalyzer($"warmup {solver.Description.Name}"))
                {
                    for (var i = 0; i < 3; i++)
                    {
                        analyzer.StartIteration();
                        GC.KeepAlive(solver.Solve(_equationSystem, _initialState, new ModellingTaskParameters(STEP)).Take(STEPS_COUNT).Count());
                    }
                }
            }
            Console.WriteLine();

            foreach (var solver in solvers)
            {
                using (var analyzer = new ExecutionTimeAnalyzer($"{solver.Description.Name}"))
                {
                    for (var run = 0; run < 5; run++)
                    {
                        analyzer.StartIteration();
                        GC.KeepAlive(solver.Solve(_equationSystem, _initialState, new ModellingTaskParameters(STEP)).Take(STEPS_COUNT).Count());
                    }
                }
            }
        }

        [Test]
        public void StepToError()
        {
            IDynamicSystemSolver[] solvers =
            {
                new ExtrapolationSolver(new ExplicitEulerSolver(), 6),
                new ExtrapolationSolver(new SymmetricExplicitMiddlePointDynamicSystemSolver(), 3),
                new ExtrapolationSolver(new KDNewtonBasedDynamicSystemSolver(), 3)
            };


            var accuracies = new[] {1e-1, 1e-2, 1e-3, 1e-4, 1e-5, 1e-6, 1e-7, 1e-8};
            accuracies = accuracies.SelectMany(a => new[] {a * 5, a * 2, a}).ToArray();

            var modellingTime = 50.0;
            foreach (var solver in solvers)
            {
                var accIndex = 0;
                for (double step = 1; step > 0.005; step -= 0.002)
                {
                    var stepsCount = (int) (modellingTime / step);
                    var actual = solver.Solve(_equationSystem, _initialState, new ModellingTaskParameters(step)).Take(stepsCount).ToList();
                    var expected = new DormandPrince8DynamicSystemSolver()
                        .Solve(_equationSystem, _initialState, new ModellingTaskParameters(step / 10)).Take(stepsCount * 10).ToList();

                    var error = 0.0;
                    for (var i = 0; i < actual.Count; i++)
                    {
                        var actualValue = actual[i].DependentVariables;
                        var expectedValue = expected[i * 10 + 9].DependentVariables;
                        foreach (var variable in actualValue)
                        {
                            error = Math.Max(error, Math.Abs(variable.Value - expectedValue[variable.Key]));
                        }
                    }


                    if (error <= accuracies[accIndex])
                    {
                        Console.WriteLine(
                            $"{accuracies[accIndex],10:e1} | {solver.Description.Name,50} | {step,10:0.00#} | {error,10:e1}");
                        accIndex++;
                        if (accIndex >= accuracies.Length)
                        {
                            break;
                        }
                    }
                }
                Console.WriteLine("_____________________________________________________________");
            }
        }

        [Test]
        public void TimeToError()
        {
            IDynamicSystemSolver[] solvers =
            {
                new ExtrapolationSolver(new ExplicitEulerSolver(), 6),
                new ExtrapolationSolver(new SymmetricExplicitMiddlePointDynamicSystemSolver(), 3),
                new ExtrapolationSolver(new KDNewtonBasedDynamicSystemSolver(), 3)
            };


            var accuracies = new[] { 1e-1, 1e-2, 1e-3, 1e-4, 1e-5, 1e-6, 1e-7, 1e-8 };
            accuracies = accuracies.SelectMany(a => new []{a * 5, a*2, a}).ToArray();

            var modellingTime = 50.0;
            foreach (var solver in solvers)
            {
                var accIndex = 0;
                for (double step = 1; step > 0.005; step -= 0.002)
                {
                    var stepsCount = (int)(modellingTime / step);

                    var actual = solver.Solve(_equationSystem, _initialState, new ModellingTaskParameters(step)).Take(stepsCount).ToList();
                    var expected = new DormandPrince8DynamicSystemSolver().Solve(_equationSystem, _initialState, new ModellingTaskParameters(step / 10)).Take(stepsCount * 10).ToList();

                    var error = 0.0;
                    for (var i = 0; i < actual.Count; i++)
                    {
                        var actualValue = actual[i].DependentVariables;
                        var expectedValue = expected[i*10+9].DependentVariables;
                        foreach (var variable in actualValue)
                        {
                            error = Math.Max(error, Math.Abs(variable.Value - expectedValue[variable.Key]));
                        }
                    }
                    

                    if (error <= accuracies[accIndex])
                    {
                        var sw = new Stopwatch();
                        sw.Start();
                        for (var i = 0; i < 10; i++)
                        {
                            GC.KeepAlive(solver.Solve(_equationSystem, _initialState, new ModellingTaskParameters(step)).Take(stepsCount).Count());
                        }
                        sw.Stop();

                        var time = sw.ElapsedTicks / 10;

                        Console.WriteLine($"{accuracies[accIndex],10:e1} | {solver.Description.Name,50} | {time,10} | {error,10:e1}");
                        accIndex++;
                        if (accIndex >= accuracies.Length)
                        {
                            break;
                        }
                    }
                }
                Console.WriteLine("_____________________________________________________________");
            }
        }
    }
}