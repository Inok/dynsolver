using System;
using System.Collections.Generic;
using System.Linq;
using DynamicSolver.DynamicSystem.Solver;
using DynamicSolver.Expressions.Execution.Interpreter;
using DynamicSolver.Expressions.Parser;
using NUnit.Framework;

namespace DynamicSolver.DynamicSystem.Tests
{
    [TestFixture(typeof(EulerDynamicSystemSolver), 1)]
    [TestFixture(typeof(RungeKuttaDynamicSystemSolver), 4)]
    //[TestFixture(typeof(DormanPrinceDynamicSystemSolver), 8)]
    public class DynamicSystemSolverTests<TSolver> where TSolver : IDynamicSystemSolver
    {
        private const double STEP = 0.01;
        private const int STEP_COUNT = (int)(10/STEP);
        private readonly int _methodAccuracy;
        
        private readonly TSolver _solver;

        private ExplicitOrdinaryDifferentialEquationSystem _equationSystem;
        private IDictionary<string, double> _initialValues;
        private Func<double, double> _expectedX1;
        private Func<double, double> _expectedX2;

        public DynamicSystemSolverTests(int methodAccuracy)
        {
            _solver = (TSolver) Activator.CreateInstance(typeof(TSolver), new InterpretedFunctionFactory());
            _methodAccuracy = methodAccuracy;
        }

        [SetUp]
        public void Setup()
        {
            var parser = new ExpressionParser();

            _equationSystem = new ExplicitOrdinaryDifferentialEquationSystem(new[]
                {
                    ExplicitOrdinaryDifferentialEquation.FromStatement(parser.Parse("x1'= -x1 - 2*x2")),
                    ExplicitOrdinaryDifferentialEquation.FromStatement(parser.Parse("x2'= 3*x1 - 4*x2"))
                });

            _initialValues = new Dictionary<string, double>() {["x1"] = 1, ["x2"] = 2};

            _expectedX1 = t => -1d / 3 * Math.Exp(-2.5d * t) * (Math.Sqrt(15) * Math.Sin(Math.Sqrt(15) * t / 2) - 3 * Math.Cos(Math.Sqrt(15) * t / 2));
            _expectedX2 = t => 2 * Math.Exp(-2.5d * t) * Math.Cos(Math.Sqrt(15) * t / 2);
        }

        [Test]
        public void Solve_ReturnsResultEqualToSystemSolution()
        {
            var actual = _solver.Solve(_equationSystem, _initialValues, STEP).Take(STEP_COUNT).ToList();

            Assert.That(actual.Count, Is.EqualTo(STEP_COUNT));

            var accuracy = 2*Math.Pow(STEP, _methodAccuracy);
            double t = STEP;
            foreach (var actualValue in actual)
            {
                Assert.That(actualValue.Keys, Is.EqualTo(new [] {"x1", "x2"}));
                Assert.That(actualValue["x1"], Is.EqualTo(_expectedX1(t)).Within(accuracy), $"t = {t}");
                Assert.That(actualValue["x2"], Is.EqualTo(_expectedX2(t)).Within(accuracy), $"t = {t}");

                t += STEP;
            }
        }

        [Test]
        public void Solve_WithProportionalStep_ErrorsHasProportionalValue()
        {
            const double step2 = STEP * 2;

            var actual1 = _solver.Solve(_equationSystem, _initialValues, STEP).Take(STEP_COUNT).ToList();
            double error1 = 0;
            for (var i = 0; i < actual1.Count; i++)
            {
                var t = STEP * (i + 1);

                var err1 = Math.Abs(actual1[i]["x1"] - _expectedX1(t));
                var err2 = Math.Abs(actual1[i]["x2"] - _expectedX2(t));
                error1 = Math.Max(Math.Max(err1, err2), error1);
            }

            var actual2 = _solver.Solve(_equationSystem, _initialValues, step2).Take(STEP_COUNT/2).ToList();
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
            Assert.That(error2 / error1, Is.EqualTo(Math.Pow(2, _methodAccuracy)).Within(0.5));
        }
    }
}