using System;
using System.Linq;
using DynamicSolver.Abstractions;
using DynamicSolver.LinearAlgebra;
using DynamicSolver.LinearAlgebra.Derivative;
using DynamicSolver.Minimizer.MinimizationInterval;
using Moq;
using NUnit.Framework;

namespace DynamicSolver.Minimizer.Tests.MinimizationInterval
{
    [TestFixture]
    public class DerivativeSwannMethodTests
    {
        private const double ACCURACY = 10e-6;

        private readonly IMinimizationIntervalSearchStrategy _strategy = new DerivativeSwannMethod(new MinimizationIntervalSearchSettings(0.5, 100, 10e-8), new NumericalDerivativeCalculator(DerivativeCalculationSettings.Default));

        [TestCase(1, -0.5, -2.5)]
        [TestCase(2, 0.5, -1.5)]
        [TestCase(-1, -1, -0.5)]
        [TestCase(5, 1.5, -2.5)]
        public void SearchInterval_WhenReachable_FoundsCorrectly(double start, double from, double to)
        {
            var function = GetFunction(a => a[0]*a[0] + 2*a[0] + 3, 1);
            var actual = _strategy.SearchInterval(function, new Point(new[] {start}), new Vector(1));

            Assert.That(actual.First.ToArray(), Is.EqualTo(new [] {from}).Within(ACCURACY));
            Assert.That(actual.Second.ToArray(), Is.EqualTo(new [] {to}).Within(ACCURACY));
        }

        private static IExecutableFunction GetFunction(Func<double[], double> func, int argumentsCount)
        {
            var mock = new Mock<IExecutableFunction>();
            mock.Setup(f => f.Execute(It.IsAny<double[]>())).Returns(func);
            mock.SetupGet(f => f.OrderedArguments).Returns(Enumerable.Range(1, argumentsCount).Select(i => "x" + i).ToArray());
            return mock.Object;
        }
    }
}