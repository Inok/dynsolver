using System;
using System.Collections.Generic;
using System.Linq;
using DynamicSolver.DynamicSystem.Solvers;
using DynamicSolver.DynamicSystem.Solvers.Explicit;
using DynamicSolver.DynamicSystem.Solvers.SemiImplicit;
using DynamicSolver.DynamicSystem.Step;
using DynamicSolver.Expressions.Execution.Compiler;
using DynamicSolver.Expressions.Execution.Interpreter;
using DynamicSolver.Expressions.Parser;
using NUnit.Framework;

namespace DynamicSolver.DynamicSystem.Tests.Solvers
{
    [TestFixture(typeof(ExplicitEulerSolver), 1)]
    [TestFixture(typeof(ExplicitMiddlePointDynamicSystemSolver), 2)]

    [TestFixture(typeof(RungeKutta4DynamicSystemSolver), 4)]
    [TestFixture(typeof(DormandPrince5DynamicSystemSolver), 5)]
    [TestFixture(typeof(DormandPrince7DynamicSystemSolver), 7)]
    [TestFixture(typeof(DormandPrince8DynamicSystemSolver), 8)]

    [TestFixture(typeof(KDDynamicSystemSolver), 2)]
    public class DynamicSystemSolverTests
    {
        private const double STEP = 0.1;
        private const int STEP_COUNT = (int)(100d / STEP);
        private readonly int _methodAccuracy;
        private readonly double _proportionTolerance;

        private readonly IDynamicSystemSolver _solver;

        private IExplicitOrdinaryDifferentialEquationSystem _equationSystem;
        private Func<double, double> _expectedX1;
        private Func<double, double> _expectedX2;

        public DynamicSystemSolverTests(Type solverType, int methodAccuracy)
        {
            _solver = (IDynamicSystemSolver)Activator.CreateInstance(solverType);
            _methodAccuracy = methodAccuracy;
            _proportionTolerance = 1;
        }

        public DynamicSystemSolverTests(IDynamicSystemSolver solver, int methodAccuracy, float tolerance)
        {
            _solver = solver;
            _methodAccuracy = methodAccuracy;
            _proportionTolerance = tolerance;
        }

        [SetUp]
        public void Setup()
        {
            var parser = new ExpressionParser();

            _equationSystem = new ExplicitOrdinaryDifferentialEquationSystem(
                new[]
                {
                    ExplicitOrdinaryDifferentialEquation.FromStatement(parser.Parse("x1'= -x1 - 2*x2 ")),
                    ExplicitOrdinaryDifferentialEquation.FromStatement(parser.Parse("x2'= 3*x1 - 4*x2"))
                },
                new DynamicSystemState(0, new Dictionary<string, double>() {["x1"] = 1, ["x2"] = 2}),
                new CompiledFunctionFactory()
            );

            _expectedX1 = t => -1d / 3 * Math.Exp(-2.5d * t) * (Math.Sqrt(15) * Math.Sin(Math.Sqrt(15) * t / 2) - 3 * Math.Cos(Math.Sqrt(15) * t / 2));
            _expectedX2 = t => 2 * Math.Exp(-2.5d * t) * Math.Cos(Math.Sqrt(15) * t / 2);
        }

        [Test]
        public void Description_OrderIsEqualToExpected()
        {
            Assert.That(_solver.Description.Order, Is.EqualTo(_methodAccuracy));
        }

        [Test]
        public void Solve_ReturnsResultEqualToSystemSolution()
        {
            var actual = _solver.Solve(_equationSystem, new FixedStepStrategy(STEP)).Take(STEP_COUNT).ToList();

            Assert.That(actual.Count, Is.EqualTo(STEP_COUNT));

            var accuracy = 2 * Math.Pow(STEP, _methodAccuracy);

            int i = 0;
            foreach (var actualValue in actual)
            {
                double t = STEP * ++i;
                Assert.That(actualValue.IndependentVariable, Is.EqualTo(t).Within(STEP * 10e-7));
                Assert.That(actualValue.DependentVariables.Keys, Is.EquivalentTo(new[] { "x1", "x2" }));
                Assert.That(actualValue.DependentVariables["x1"], Is.EqualTo(_expectedX1(t)).Within(accuracy), $"t = {t}, error = {_expectedX1(t) - actualValue.DependentVariables["x1"]}");
                Assert.That(actualValue.DependentVariables["x2"], Is.EqualTo(_expectedX2(t)).Within(accuracy), $"t = {t}, error = {_expectedX2(t) - actualValue.DependentVariables["x2"]}");
            }
        }

        [Test]
        public void Solve_WithProportionalStep_ErrorsHasProportionalValue()
        {
            var actual1 = _solver.Solve(_equationSystem, new FixedStepStrategy(STEP)).Take(STEP_COUNT).ToList();
            double error1 = 0;
            for (var i = 0; i < actual1.Count; i++)
            {
                var t = STEP * (i + 1);

                var err1 = Math.Abs(actual1[i].DependentVariables["x1"] - _expectedX1(t));
                var err2 = Math.Abs(actual1[i].DependentVariables["x2"] - _expectedX2(t));
                error1 = Math.Max(Math.Max(err1, err2), error1);
            }

            const double step2 = STEP * 2;
            var actual2 = _solver.Solve(_equationSystem, new FixedStepStrategy(STEP*2)).Take(STEP_COUNT / 2).ToList();
            double error2 = 0;
            for (var i = 0; i < actual2.Count; i++)
            {
                var t = step2 * (i + 1);

                var err1 = Math.Abs(actual2[i].DependentVariables["x1"] - _expectedX1(t));
                var err2 = Math.Abs(actual2[i].DependentVariables["x2"] - _expectedX2(t));

                error2 = Math.Max(Math.Max(err1, err2), error2);
            }

            Assert.That(error2 / error1, Is.GreaterThan(Math.Pow(2, _methodAccuracy) * _proportionTolerance));
        }
    }
}