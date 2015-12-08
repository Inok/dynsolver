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

        [Test]
        public void Argument_Allowed_CompilesCorrectly()
        {
            var compiler = new ExpressionCompiler();

            var function = compiler.Compile("x1", new[] {"x1"});

            for(double arg = -3; arg <= 3; arg += 0.5)
            {
                Assert.That(function.Execute(new [] { arg }), Is.EqualTo(arg));
            }
        }

        [Test]
        public void Argument_TwoArguments_CompilesCorrectly([Range(-5.0, 5.0, 0.25)]double arg)
        {
            var secondArgMultiplier = 3;

            var compiler = new ExpressionCompiler();

            var function = compiler.Compile("x1 + x2", new[] { "x1", "x2" });

            Assert.That(function.Execute(new[] { arg, secondArgMultiplier * arg }), Is.EqualTo(arg + secondArgMultiplier * arg));            
        }

        [Test]
        public void Validation_ArgumentNotAllowed_Throws()
        {
            var compiler = new ExpressionCompiler();

            Assert.That(() => compiler.Compile("x1", new string[0]), Throws.InvalidOperationException);
            Assert.That(() => compiler.Compile("x1", new[] { "x" }), Throws.InvalidOperationException);
        }

        [Test]
        public void Validation_ExpressionInvalid_Throws()
        {
            var compiler = new ExpressionCompiler();

            Assert.That(() => compiler.Compile(null, new string[0]), Throws.ArgumentException);
            Assert.That(() => compiler.Compile("", new string[0]), Throws.ArgumentException);
            Assert.That(() => compiler.Compile(" ", new string[0]), Throws.ArgumentException);
        }

        [Test]
        public void Validation_AllowedArgumentsInvalid_Throws()
        {
            var compiler = new ExpressionCompiler();

            Assert.That(() => compiler.Compile("x", new string[] { null }), Throws.ArgumentException);
            Assert.That(() => compiler.Compile("x", new string[] { "" }), Throws.ArgumentException);
            Assert.That(() => compiler.Compile("x", new string[] { "  " }), Throws.ArgumentException);
            Assert.That(() => compiler.Compile("x", new string[] { "x1", "x1" }), Throws.ArgumentException);
        }

    }
}