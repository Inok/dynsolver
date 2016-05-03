using System;
using System.Collections.Generic;
using System.Linq;
using DynamicSolver.DynamicSystem.Solver;
using DynamicSolver.Expressions.Execution.Interpreter;
using DynamicSolver.Expressions.Parser;
using NUnit.Framework;

namespace DynamicSolver.DynamicSystem.Tests
{
    [TestFixture]
    public class RungeKuttaSystemSolverTests
    {
        private ExplicitOrdinaryDifferentialEquationSystem _equationSystem;
        private IDictionary<string, double> _initialValues;
        private Func<double, double> _expectedX1;
        private Func<double, double> _expectedX2;

        [SetUp]
        public void Setup()
        {
            var parser = new ExpressionParser();

            _equationSystem = new ExplicitOrdinaryDifferentialEquationSystem(new[]
                {
                    ExplicitOrdinaryDifferentialEquation.FromStatement(parser.Parse("x1'= 3*x1 - x2")),
                    ExplicitOrdinaryDifferentialEquation.FromStatement(parser.Parse("x2'= 4*x1 - x2"))
                });

            _initialValues = new Dictionary<string, double>() {["x1"] = 5, ["x2"] = 8};

            _expectedX1 = t => (5 + 2*t) * Math.Exp(t);
            _expectedX2 = t => (8 + 4*t) * Math.Exp(t);
        }

        [Test]
        public void Solve_ReturnsResultEqualToSystemSolution()
        {
            const double step = 0.01;
            const int count = 1000;
            const double accuracy = 0.0001;

            var solver = new ExplicitRungeKuttaDynamicSystemSolver(new InterpretedFunctionFactory(), _equationSystem);

            var actual = solver.Solve(_initialValues, step).Take(count).ToList();

            Assert.That(actual.Count, Is.EqualTo(count));

            double t = step;
            foreach (var actualValue in actual)
            {
                Assert.That(actualValue.Keys, Is.EqualTo(new [] {"x1", "x2"}));
                Assert.That(actualValue["x1"], Is.EqualTo(_expectedX1(t)).Within(accuracy), $"t = {t}");
                Assert.That(actualValue["x2"], Is.EqualTo(_expectedX2(t)).Within(accuracy), $"t = {t}");

                t += step;
            }
        }

        [Test]
        public void Solve_WithProportionalStep_ErrorsHasProportionalValue()
        {
            const double step1 = 0.01;
            const double step2 = 0.02;
            const int time = 100;

            var solver = new ExplicitRungeKuttaDynamicSystemSolver(new InterpretedFunctionFactory(), _equationSystem);

            var actual1 = solver.Solve(_initialValues, step1).Take((int)(time/ step1)).ToList();
            double error1 = 0;
            for (var i = 0; i < actual1.Count; i++)
            {
                var t = step1 * (i + 1);

                var err1 = Math.Abs(actual1[i]["x1"] - _expectedX1(t));
                var err2 = Math.Abs(actual1[i]["x2"] - _expectedX2(t));
                error1 = Math.Max(Math.Max(err1, err2), error1);
            }

            var actual2 = solver.Solve(_initialValues, step2).Take((int) (time / step2)).ToList();
            double error2 = 0;
            for (var i = 0; i < actual2.Count; i++)
            {
                var t = step2 * (i + 1);
                var expectedX1 = -1d / 3 * Math.Exp(-5 * t / 2) * (Math.Sqrt(15) * Math.Sin(Math.Sqrt(15) * t / 2) - 3 * Math.Cos(Math.Sqrt(15) * t / 2));
                var expectedX2 = 2 * Math.Exp(-5 * t / 2) * Math.Cos(Math.Sqrt(15) * t / 2);

                var err1 = Math.Abs(actual2[i]["x1"] - expectedX1);
                var err2 = Math.Abs(actual2[i]["x2"] - expectedX2);

                error2 = Math.Max(Math.Max(err1, err2), error2);
            }
            Assert.That(error2 / error1, Is.EqualTo(16));
        }
    }
}