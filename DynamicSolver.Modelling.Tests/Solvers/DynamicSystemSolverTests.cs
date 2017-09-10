using System;
using System.Collections.Generic;
using System.Linq;
using DynamicSolver.Core.Execution.Compiler;
using DynamicSolver.Core.Syntax.Parser;
using DynamicSolver.Modelling.Solvers;
using DynamicSolver.Modelling.Solvers.Explicit;
using DynamicSolver.Modelling.Solvers.SemiImplicit;
using NUnit.Framework;

namespace DynamicSolver.Modelling.Tests.Solvers
{
    [TestFixture(typeof(ExplicitEulerSolver), 1)]
    [TestFixture(typeof(EulerCromerSolver), 1)]

    [TestFixture(typeof(ExplicitMiddlePointDynamicSystemSolver), 2)]
    
    [TestFixture(typeof(RungeKutta4DynamicSystemSolver), 4)]
    [TestFixture(typeof(DormandPrince5DynamicSystemSolver), 5)]
    [TestFixture(typeof(DormandPrince7DynamicSystemSolver), 7)]
    [TestFixture(typeof(DormandPrince8DynamicSystemSolver), 8)]

    [TestFixture(typeof(KDNewtonBasedDynamicSystemSolver), 2)]    
    [TestFixture(typeof(KDFastImplicitDynamicSystemSolver), 2)]
    
    [TestFixture(typeof(KDFastDynamicSystemSolver), new object[] {4}, 2, 95)]
    public class DynamicSystemSolverTests
    {
        private const double STEP = 0.1;
        private const int STEP_COUNT = (int)(100d / STEP);
        private readonly int _methodAccuracy;
        private readonly double _proportionTolerance;
        private readonly double _absoluteErrorTolerance;

        private readonly IDynamicSystemSolver _solver;

        private IExplicitOrdinaryDifferentialEquationSystem _equationSystem;
        private DynamicSystemState _initialState;
        private Func<double, double> _expectedX1;
        private Func<double, double> _expectedX2;

        public DynamicSystemSolverTests(Type solverType, int methodAccuracy)
            : this((IDynamicSystemSolver) Activator.CreateInstance(solverType), methodAccuracy, null)
        {
            
        }

        public DynamicSystemSolverTests(Type solverType, int methodAccuracy, int proportionTolerancePercent)
            : this((IDynamicSystemSolver) Activator.CreateInstance(solverType), methodAccuracy, proportionTolerancePercent / 100f)
        {
        }

        public DynamicSystemSolverTests(Type solverType, object[] args, int methodAccuracy, int proportionTolerancePercent)
            : this((IDynamicSystemSolver) Activator.CreateInstance(solverType, args), methodAccuracy, proportionTolerancePercent / 100f)
        {
        }
        
        protected DynamicSystemSolverTests(Type solverType, int methodAccuracy, float? proportionTolerance = null)
            : this((IDynamicSystemSolver) Activator.CreateInstance(solverType), methodAccuracy, proportionTolerance)
        {
        }

        protected DynamicSystemSolverTests(
            IDynamicSystemSolver solver,
            int methodAccuracy,
            float? proportionTolerance = null)
        {
            _solver = solver;
            _methodAccuracy = methodAccuracy;
            _absoluteErrorTolerance = 3 * Math.Pow(STEP, methodAccuracy);
            _proportionTolerance = proportionTolerance ?? 1;
        }
        
        [SetUp]
        public void Setup()
        {
            var parser = new SyntaxParser();

            _equationSystem = new ExplicitOrdinaryDifferentialEquationSystem(
                new[]
                {
                    ExplicitOrdinaryDifferentialEquation.FromExpression(parser.Parse("x1'= -x1 - 2*x2 ")),
                    ExplicitOrdinaryDifferentialEquation.FromExpression(parser.Parse("x2'= 3*x1 - 4*x2"))
                },
                new CompiledFunctionFactory()
            );
            _initialState = new DynamicSystemState(0, new Dictionary<string, double>() {["x1"] = 1, ["x2"] = 2});

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
            var actual = _solver.Solve(_equationSystem, _initialState, new ModellingTaskParameters(STEP)).Take(STEP_COUNT).ToList();

            Assert.That(actual.Count, Is.EqualTo(STEP_COUNT));

            int i = 0;
            foreach (var actualValue in actual)
            {
                double t = STEP * ++i;
                Assert.That(actualValue.IndependentVariable, Is.EqualTo(t).Within(STEP * 10e-7));
                Assert.That(actualValue.DependentVariables.Keys, Is.EquivalentTo(new[] { "x1", "x2" }));
                Assert.That(
                    actualValue.DependentVariables["x1"],
                    Is.EqualTo(_expectedX1(t)).Within(_absoluteErrorTolerance),
                    $"t = {t}, error = {_expectedX1(t) - actualValue.DependentVariables["x1"]}");
                Assert.That(
                    actualValue.DependentVariables["x2"],
                    Is.EqualTo(_expectedX2(t)).Within(_absoluteErrorTolerance),
                    $"t = {t}, error = {_expectedX2(t) - actualValue.DependentVariables["x2"]}");
            }
        }

        [Test]
        public void Solve_WithProportionalStep_ErrorsHasProportionalValue()
        {
            double error1 = 0;
            foreach (var state in _solver.Solve(_equationSystem, _initialState,  new ModellingTaskParameters(STEP)).Take(STEP_COUNT))
            {
                var err1 = Math.Abs(state.DependentVariables["x1"] - _expectedX1(state.IndependentVariable));
                var err2 = Math.Abs(state.DependentVariables["x2"] - _expectedX2(state.IndependentVariable));
                error1 = Math.Max(Math.Max(err1, err2), error1);
            }

            double error2 = 0;
            foreach (var state in _solver.Solve(_equationSystem, _initialState, new ModellingTaskParameters(STEP*2)).Take(STEP_COUNT / 2))
            {
                var err1 = Math.Abs(state.DependentVariables["x1"] - _expectedX1(state.IndependentVariable));
                var err2 = Math.Abs(state.DependentVariables["x2"] - _expectedX2(state.IndependentVariable));
                error2 = Math.Max(Math.Max(err1, err2), error2);
            }

            Assert.That(error2 / error1, Is.GreaterThan(Math.Pow(2, _methodAccuracy) * _proportionTolerance));
        }
    }
}