using System;
using System.Linq;
using DynamicSolver.Abstractions;
using DynamicSolver.LinearAlgebra;
using DynamicSolver.LinearAlgebra.Derivative;
using DynamicSolver.Minimizer.MinimizationInterval;
using DynamicSolver.Minimizer.OneDimensionalSearch;
using Moq;
using NUnit.Framework;

namespace DynamicSolver.Minimizer.Tests.OneDimensionalSearch
{
    [TestFixture]
    public class GoldenRatioMethodTests
    {
        private static readonly OneDimensionalSearchSettings Settings = OneDimensionalSearchSettings.Default;
        private readonly IOneDimensionalSearchStrategy _strategy = new GoldenRatioMethod(Settings, new DerivativeSwannMethod(MinimizationIntervalSearchSettings.Default, new NumericalDerivativeCalculator(DerivativeCalculationSettings.Default)));

        [Test]
        public void SearchInterval_FoundsCorrectly()
        {
            var function = GetFunction(a => a[0]*a[0] + 2*a[0] + 3, 1);
            var actual = _strategy.SearchInterval(function, new Point(1), new Vector(1));

            Assert.That(actual.Length, Is.LessThanOrEqualTo(Settings.Accuracy));
            Assert.That(actual.Center.ToArray(), Is.EqualTo(new [] {-1.0}).Within(Settings.Accuracy));
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