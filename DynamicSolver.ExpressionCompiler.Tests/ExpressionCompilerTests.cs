using System;
using System.Globalization;
using NUnit.Framework;

namespace DynamicSolver.ExpressionCompiler.Tests
{
    [TestFixture]
    public class ExpressionCompilerTests
    {

        [Test]
        public void SimpleTest()
        {
            var compiler = new ExpressionCompiler();

            var function = compiler.Compile("1 + 2", new string[0]);

            Assert.That(function.Execute(new double[0]), Is.EqualTo(3));
        }

        [Test]
        public void Pi_CompilesCorrectly_Test()
        {
            var compiler = new ExpressionCompiler();

            var function = compiler.Compile("pi", new string[0]);

            Assert.That(function.Execute(new double[0]), Is.EqualTo(Math.PI).Within(0.001));
        }

        [Test]
        public void Sin_CompilesCorrectly_Test([Range(-2.0, 2.0, 0.2)] double multiplier)
        {
            var compiler = new ExpressionCompiler();

            var function = compiler.Compile($"sin({multiplier.ToString("F5", new NumberFormatInfo() {CurrencyDecimalSeparator = "."})} * pi)", new string[0]);

            Assert.That(function.Execute(new double[0]), Is.EqualTo(Math.Sin(multiplier * Math.PI)).Within(0.001));
        }

        [Test]
        public void Cos_CompilesCorrectly_Test([Range(-2.0, 2.0, 0.2)] double multiplier)
        {
            var compiler = new ExpressionCompiler();

            var function = compiler.Compile($"cos({multiplier.ToString("F5", new NumberFormatInfo() { CurrencyDecimalSeparator = "." })} * pi)", new string[0]);

            Assert.That(function.Execute(new double[0]), Is.EqualTo(Math.Cos(multiplier * Math.PI)).Within(0.001));
        }

    }
}