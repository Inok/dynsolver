using System;
using System.Linq;
using DynamicSolver.Abstractions;
using DynamicSolver.LinearAlgebra;
using DynamicSolver.LinearAlgebra.Derivative;
using DynamicSolver.Minimizer.MinimizationInterval;
using DynamicSolver.Minimizer.MultiDimensionalSearch;
using DynamicSolver.Minimizer.OneDimensionalSearch;
using Moq;
using NUnit.Framework;

namespace DynamicSolver.Minimizer.Tests.MultiDimensionalSearch
{
    [TestFixture]
    public class HookeJeevesMethodTests
    {
        private static readonly HookeJeevesSearchSettings Settings = HookeJeevesSearchSettings.Default;
        private static readonly NumericalDerivativeCalculator NumericalDerivativeCalculator = new NumericalDerivativeCalculator(DerivativeCalculationSettings.Default);

        private readonly IMultiDimensionalSearchStrategy _strategy = new HookeJeevesMethod(new GoldenRatioMethod(new DerivativeSwannMethod(DerivativeSwannMethodSettings.Default, NumericalDerivativeCalculator), DirectedSearchSettings.Default), NumericalDerivativeCalculator, Settings);

        [Test]
        public void Search_FoundsCorrectly()
        {
            var function = GetFunction(x => x[0]*x[0] + 3* x[1] * x[1] + 3 *x[0] * x[1] + x[0], 2);
            var actual = _strategy.Search(function, new Point(1, -2));

            Assert.That(actual.ToArray(), Is.EqualTo(new [] {-2.0, 1.0}).Within(Settings.Accuracy));            
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