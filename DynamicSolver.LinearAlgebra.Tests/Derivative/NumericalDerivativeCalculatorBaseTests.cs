using System;
using System.Linq;
using DynamicSolver.Abstractions;
using DynamicSolver.LinearAlgebra.Derivative;
using Moq;
using NUnit.Framework;

namespace DynamicSolver.LinearAlgebra.Tests.Derivative
{
    [TestFixture(DerivativeMode.Auto)]
    [TestFixture(DerivativeMode.Center)]
    [TestFixture(DerivativeMode.Left)]
    [TestFixture(DerivativeMode.Right)]
    public class NumericalDerivativeCalculatorTests
    {
        private const double INCREMENT = 10e-8;
        private const double ACCURACY = 10e-6;

        private DerivativeCalculationSettings Settings { get; set; }

        private IDerivativeCalculationStrategy Calculator => new NumericalDerivativeCalculator(Settings);

        public NumericalDerivativeCalculatorTests(DerivativeMode mode)
        {
            Settings = new DerivativeCalculationSettings(INCREMENT, mode);
        }

        private static IExecutableFunction GetFunction(Func<double[], double> func, int argumentsCount)
        {
            var mock = new Mock<IExecutableFunction>();
            mock.Setup(f => f.Execute(It.IsAny<double[]>())).Returns(func);
            mock.SetupGet(f => f.OrderedArguments).Returns(Enumerable.Range(1, argumentsCount).Select(i => "x" + i).ToArray());
            return mock.Object;
        }

        [Test]
        public void Derivative_OneDimensional_ReturnsCorrect()
        {
            var function = GetFunction(args => args[0] * args[0] + 2*args[0] + 5, 1);
            var actual = Calculator.Derivative(function, new Point(new double[] {-2}));

            Assert.That(actual, Is.EqualTo(new [] { -2 }).Within(ACCURACY));            
        }

        [Test]
        public void Derivative_TwoDimensional_ReturnsCorrect()
        {
            var function = GetFunction(args => 2 * args[0] * args[0] + args[1] * args[1] + args[0] * args[1] + 5 * args[1], 2);
            var actual = Calculator.Derivative(function, new Point(new double[] {1, -2}));

            Assert.That(actual, Is.EqualTo(new [] { 2, 2 }).Within(ACCURACY));            
        }

        [Test]
        public void DerivativeByDirection_OneDimensional_ReturnsCorrect()
        {
            var function = GetFunction(args => args[0] * args[0] + 2*args[0] + 5, 1);
            var actual = Calculator.DerivativeByDirection(function, new Point(new double[] {-2}), new Vector(3));

            Assert.That(actual, Is.EqualTo(-2).Within(ACCURACY));            
        }

        [Test]
        public void DerivativeByDirection_TwoDimensional_ReturnsCorrect()
        {
            var function = GetFunction(args => 2 * args[0] * args[0] + args[1] * args[1] + args[0] * args[1] + 4 * args[1], 2);
            var actual = Calculator.DerivativeByDirection(function, new Point(new double[] {1, -2}), new Vector(3, -4));

            Assert.That(actual, Is.EqualTo(0.4).Within(ACCURACY));            
        }
    }
}