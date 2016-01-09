using System;
using System.Globalization;
using DynamicSolver.Abstractions;
using NUnit.Framework;

namespace DynamicSolver.ExpressionCompiler.Tests
{
    [TestFixture]
    public class ExpressionCompilerTests
    {
        private static readonly NumberFormatInfo DoubleFormatInfo = new NumberFormatInfo() { CurrencyDecimalSeparator = "." };

        [Test]
        public void StaticExpression_CompilesCorrectly_Test()
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

            Assert.That(function.Execute(new double[0]), Is.EqualTo(Math.PI));
        }

        [Test]
        public void E_CompilesCorrectly_Test()
        {
            var compiler = new ExpressionCompiler();

            var function = compiler.Compile("e", new string[0]);

            Assert.That(function.Execute(new double[0]), Is.EqualTo(Math.E));
        }

        [Test]
        public void Exp_CompilesCorrectly_Test([Range(-2.0, 2.0, 0.2)] double arg)
        {
            var compiler = new ExpressionCompiler();

            var function = compiler.Compile($"exp({arg.ToString("F5", DoubleFormatInfo)})", new string[0]);

            Assert.That(function.Execute(new double[0]), Is.EqualTo(Math.Exp(arg)).Within(0.001));
        }

        [Test]
        public void Sin_CompilesCorrectly_Test([Range(-2.0, 2.0, 0.2)] double multiplier)
        {
            var compiler = new ExpressionCompiler();

            var function = compiler.Compile($"sin({multiplier.ToString("F5", DoubleFormatInfo)} * pi)", new string[0]);

            Assert.That(function.Execute(new double[0]), Is.EqualTo(Math.Sin(multiplier * Math.PI)).Within(0.001));
        }

        [Test]
        public void Cos_CompilesCorrectly_Test([Range(-2.0, 2.0, 0.2)] double multiplier)
        {
            var compiler = new ExpressionCompiler();

            var function = compiler.Compile($"cos({multiplier.ToString("F5", DoubleFormatInfo)} * pi)", new string[0]);

            Assert.That(function.Execute(new double[0]), Is.EqualTo(Math.Cos(multiplier * Math.PI)).Within(0.001));
        }

        [Test]
        public void Tg_CompilesCorrectly_Test([Values(-1.9, -1.5, -0.9, -0.5, 0.0, 0.5, 0.9, 1.5, 1.9)] double multiplier)
        {
            var compiler = new ExpressionCompiler();

            var function = compiler.Compile($"tg({multiplier.ToString("F5", DoubleFormatInfo)} * pi)", new string[0]);

            Assert.That(function.Execute(new double[0]), Is.EqualTo(Math.Tan(multiplier * Math.PI)).Within(0.001));
        }

        [Test]
        public void Ctg_CompilesCorrectly_Test([Values(-1.9, -1.5, -0.9, -0.5, 0.0, 0.5, 0.9, 1.5, 1.9)] double multiplier)
        {
            var compiler = new ExpressionCompiler();

            var function = compiler.Compile($"ctg({multiplier.ToString("F5", DoubleFormatInfo)} * pi)", new string[0]);

            Assert.That(function.Execute(new double[0]), Is.EqualTo(1.0 / Math.Tan(multiplier * Math.PI)).Within(0.001));
        }

        [Test]
        public void Pow_CompilesCorrectly_Test([Range(-1.0, 1.0, 0.5)] double arg, [Range(-2.0, 2.0, 0.5)] double pow)
        {
            var compiler = new ExpressionCompiler();

            var function = compiler.Compile($"pow({arg.ToString("F5", DoubleFormatInfo)}, {pow.ToString("F2", DoubleFormatInfo)})", new string[0]);

            Assert.That(function.Execute(new double[0]), Is.EqualTo(Math.Pow(arg, pow)).Within(0.001));
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
        public void ComplexExpression_CompilesCorrectly(
            [Values(-1.0, 0.0, 1.0)] double x1,
            [Values(-5.0, 1.0, 100.0)] double x2,
            [Values(-10.0, 0.0, 10.0)] double x3)
        {
            var compiler = new ExpressionCompiler();

            var function = compiler.Compile("-tg(x1*pi) + x2*cos(2*exp(x3)*pi) - pow(x3/(x2*e), 3*x1)", new[] {"x1", "x2", "x3"});

            Assert.That(function.Execute(new[] {x1, x2, x3}), Is.EqualTo(-Math.Tan(x1*Math.PI) + x2*Math.Cos(2*Math.Exp(x3)*Math.PI) - Math.Pow(x3/(x2*Math.E), 3*x1)));
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
            Assert.That(() => compiler.Compile("cos", new string[] { "cos" }), Throws.ArgumentException);
        }

    }
}