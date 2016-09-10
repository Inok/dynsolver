using System;
using System.Collections.Generic;
using System.Linq;
using DynamicSolver.DynamicSystem.Solver;
using DynamicSolver.Expressions.Execution.Compiler;
using DynamicSolver.Expressions.Parser;
using NUnit.Framework;

namespace DynamicSolver.DynamicSystem.Tests
{
    [TestFixture(typeof(EulerDynamicSystemSolver), 1)]
    [TestFixture(typeof(ExtrapolationEulerDynamicSystemSolver), 3, 3)]
    [TestFixture(typeof(ExtrapolationEulerDynamicSystemSolver), 4, 4)]
    [TestFixture(typeof(ImplicitEulerDynamicSystemSolver), 1)]
    [TestFixture(typeof(ExtrapolationImplicitEulerDynamicSystemSolver), 3, 3)]
    [TestFixture(typeof(ExtrapolationImplicitEulerDynamicSystemSolver), 4, 4)]
    [TestFixture(typeof(KDDynamicSystemSolver), 1)]
    [TestFixture(typeof(ExtrapolationKDDynamicSystemSolver), 5, 3)]
    [TestFixture(typeof(ExtrapolationKDDynamicSystemSolver), 7, 4)]
    [TestFixture(typeof(RungeKutta4DynamicSystemSolver), 4)]
    [TestFixture(typeof(DormandPrince5DynamicSystemSolver), 5)]
    [TestFixture(typeof(DormandPrince8DynamicSystemSolver), 8)]
    [TestFixture(typeof(DormandPrince7DynamicSystemSolver), 7)]
    public class DynamicSystemSolverTests<TSolver> where TSolver : IDynamicSystemSolver
    {
        private const double STEP = 0.1;
        private const int STEP_COUNT = (int)(100d/STEP);
        private readonly int _methodAccuracy;
        
        private readonly TSolver _solver;

        private ExplicitOrdinaryDifferentialEquationSystem _equationSystem;
        private IReadOnlyDictionary<string, double> _initialValues;
        private Func<double, double> _expectedX1;
        private Func<double, double> _expectedX2;

        public DynamicSystemSolverTests(int methodAccuracy)
        {
            _solver = (TSolver) Activator.CreateInstance(typeof(TSolver), new CompiledFunctionFactory());
            _methodAccuracy = methodAccuracy;
        }

        public DynamicSystemSolverTests(int methodAccuracy, int extrapolationSolverStageCountArgument)
        {
            _solver = (TSolver) Activator.CreateInstance(typeof(TSolver), new CompiledFunctionFactory(), extrapolationSolverStageCountArgument);
            _methodAccuracy = methodAccuracy;
        }

        [SetUp]
        public void Setup()
        {
            var parser = new ExpressionParser();

            _equationSystem = new ExplicitOrdinaryDifferentialEquationSystem(new[]
                {
                    ExplicitOrdinaryDifferentialEquation.FromStatement(parser.Parse("x1'= -x1 - 2*x2 ")),
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

            var accuracy = 8.5 * Math.Pow(STEP, _methodAccuracy);

            int i = 0;
            foreach (var actualValue in actual)
            {
                double t = STEP * ++i;
                Assert.That(actualValue.Keys, Is.EquivalentTo(new [] {"x1", "x2"}));
                Assert.That(actualValue["x1"], Is.EqualTo(_expectedX1(t)).Within(accuracy), $"t = {t}, deviation = {_expectedX1(t) - actualValue["x1"]}");
                Assert.That(actualValue["x2"], Is.EqualTo(_expectedX2(t)).Within(accuracy), $"t = {t}, deviation = {_expectedX2(t) - actualValue["x2"]}");
            }
        }

        [Test]
        public void Solve_WithProportionalStep_ErrorsHasProportionalValue()
        {
            var actual1 = _solver.Solve(_equationSystem, _initialValues, STEP).Take(STEP_COUNT).ToList();
            double error1 = 0;
            for (var i = 0; i < actual1.Count; i++)
            {
                var t = STEP * (i + 1);

                var err1 = Math.Abs(actual1[i]["x1"] - _expectedX1(t));
                var err2 = Math.Abs(actual1[i]["x2"] - _expectedX2(t));
                error1 = Math.Max(Math.Max(err1, err2), error1);
            }

            const double step2 = STEP * 2;
            var actual2 = _solver.Solve(_equationSystem, _initialValues, step2).Take(STEP_COUNT/2).ToList();
            double error2 = 0;
            for (var i = 0; i < actual2.Count; i++)
            {
                var t = step2 * (i + 1);
                
                var err1 = Math.Abs(actual2[i]["x1"] - _expectedX1(t));
                var err2 = Math.Abs(actual2[i]["x2"] - _expectedX2(t));

                error2 = Math.Max(Math.Max(err1, err2), error2);
            }

            Assert.That(error2 / error1, Is.GreaterThan(Math.Pow(2, _methodAccuracy)));
        }
    }
}