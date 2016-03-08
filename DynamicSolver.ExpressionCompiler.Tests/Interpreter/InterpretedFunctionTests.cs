using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DynamicSolver.Abstractions.Tools;
using DynamicSolver.ExpressionCompiler.Interpreter;
using DynamicSolver.ExpressionParser.Expression;
using JetBrains.Annotations;
using NUnit.Framework;

namespace DynamicSolver.ExpressionCompiler.Tests.Interpreter
{
    [TestFixture]
    public class InterpretedFunctionTests
    {
        [NotNull] private readonly ExpressionParser.Parser.ExpressionParser _parser = new ExpressionParser.Parser.ExpressionParser();


        [Test]
        public void Consttructor_InvalidArguments_Throws()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.That(() => new InterpretedFunction(null), Throws.ArgumentNullException);
        }

        [Test]
        public void Consttructor_UnknownFunctionCall_Throws()
        {
            Assert.That(() => new InterpretedFunction(new UnaryMinusOperator(new FunctionCall("foo", new NumericPrimitive("0")))), Throws.ArgumentException);
        }


        [TestCase("1 + tg(10)^(-e) + cos(1*pi)/3", new string[] {})]
        [TestCase("x", new [] {"x"})]
        [TestCase("x2 + x1 - x4 / x0", new [] {"x0", "x1", "x2", "x4"})]
        [TestCase("a + b^(-c) + cos(1*pi)/d", new[] {"a", "b", "c", "d"})]
        public void OrderedArguments_HasValidItems(string expression, string[] expectedArguments)
        {
            var statement = _parser.Parse(expression);
            var function = new InterpretedFunction(statement.Expression);

            Assert.That(function.OrderedArguments, Is.Ordered.And.Unique.And.EqualTo(expectedArguments));
        }

        [Test]
        public void Execute_WithDictionary_NullArguments_Throws()
        {
            var statement = _parser.Parse("1 + 1");
            var function = new InterpretedFunction(statement.Expression);

            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.That(() => function.Execute((IReadOnlyDictionary<string, double>)null), Throws.ArgumentNullException);
        }

        [TestCase("x - y", "")]
        [TestCase("1", "x=2")]
        [TestCase("x - y", "z=10")]
        [TestCase("x - y", "x=1")]
        [TestCase("x - y", "y=2")]
        [TestCase("x - y", "x=1;y=2;z=10")]
        public void Execute_WithDictionary_InvalidArguments_Throws(string expression, string arguments)
        {
            var args = arguments.Split(';').Where(s => !string.IsNullOrEmpty(s)).Select(arg => arg.Split('=')).ToDictionary(arg => arg[0], arg => double.Parse(arg[1]));
            Console.WriteLine(args.Dump());

            var statement = _parser.Parse(expression);
            var function = new InterpretedFunction(statement.Expression);

            Assert.That(() => function.Execute(args), Throws.ArgumentException);
        }

        [TestCase("3.75", 3.75, "")]
        [TestCase("1 + 2", 3.0, "")]
        [TestCase("2 - e", 2.0 - Math.E, "")]
        [TestCase("-2 * -pi", -2.0*-Math.PI, "")]
        [TestCase("-(1 + e)", -(1.0 + Math.E), "")]
        [TestCase("sin(pi/6)", 0.5, "")]
        [TestCase("-cos(pi/3)", -0.5, "")]
        [TestCase("tg(pi)", 0, "")]
        [TestCase("ctg(pi/4)", 1, "")]
        [TestCase("lg(100)", 2, "")]
        [TestCase("ln(e^3.5)", 3.5, "")]
        [TestCase("exp(1)", Math.E, "")]
        [TestCase("x", 5.3, "x=5.3")]
        [TestCase("b^a", 8.0, "a=3.0;b=2.0")]
        [TestCase("x1 + x2 * x3", 7, "x1=1;x2=2;x3=3")]
        [TestCase("sin((a - 1) * pi)", 1, "a=1.5")]
        [TestCase("a + (a / 2) ^ a", 20, "a=4")]
        public void Execute_WithDictionary_CalculatesAExpected(string expression, double expected, string variables)
        {
            var args = variables.Split(';').Where(s => !string.IsNullOrEmpty(s)).Select(arg => arg.Split('=')).ToDictionary(arg => arg[0], arg => double.Parse(arg[1], new NumberFormatInfo() {NumberDecimalSeparator = "."}));
            Console.WriteLine(args.Dump());

            var statement = _parser.Parse(expression);
            var function = new InterpretedFunction(statement.Expression);

            Assert.That(function.Execute(args), Is.EqualTo(expected).Within(Math.Max(Math.Abs(expected * 1e-5), 1e-10)));
        }

        [Test]
        public void Execute_WithArray_NullArguments_Throws()
        {
            var statement = _parser.Parse("1 + 1");
            var function = new InterpretedFunction(statement.Expression);

            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.That(() => function.Execute((double[])null), Throws.ArgumentNullException);
        }

        [TestCase("1", 5.0)]
        [TestCase("x - y")]
        [TestCase("x - y", 1.0)]
        [TestCase("x - y", 1.0, 2.0, 3.0)]
        public void Execute_WithArray_InvalidArguments_Throws(string expression, params double[] args)
        {
            var statement = _parser.Parse(expression);
            var function = new InterpretedFunction(statement.Expression);

            Assert.That(() => function.Execute(args), Throws.ArgumentException);
        }


        [TestCase("3.75", 3.75, new double[0])]
        [TestCase("1 + 2", 3.0, new double[0])]
        [TestCase("2 - e", 2.0 - Math.E, new double[0])]
        [TestCase("-2 * -pi", -2.0*-Math.PI, new double[0])]
        [TestCase("-(1 + e)", -(1.0 + Math.E), new double[0])]
        [TestCase("sin(pi/6)", 0.5, new double[0])]
        [TestCase("-cos(pi/3)", -0.5, new double[0])]
        [TestCase("tg(pi)", 0, new double[0])]
        [TestCase("ctg(pi/4)", 1, new double[0])]
        [TestCase("lg(100)", 2, new double[0])]
        [TestCase("ln(e^3.5)", 3.5, new double[0])]
        [TestCase("exp(1)", Math.E, new double[0])]
        [TestCase("x", 5.3, new[] {5.3})]
        [TestCase("b^a", 8.0, new[] {3.0, 2.0})]
        [TestCase("x1 + x2 * x3", 7, new[] {1.0, 2.0, 3.0})]
        [TestCase("sin((a - 1) * pi)", 1, new[] {1.5})]
        [TestCase("a + (a / 2)^a", 20, new [] {4.0})]
        public void Execute_WithArray_CalculatesAExpected(string expression, double expected, double[] args)
        {
            var statement = _parser.Parse(expression);
            var function = new InterpretedFunction(statement.Expression);

            Assert.That(function.Execute(args), Is.EqualTo(expected).Within(Math.Max(Math.Abs(expected * 1e-5), 1e-10)));
        }
    }
}