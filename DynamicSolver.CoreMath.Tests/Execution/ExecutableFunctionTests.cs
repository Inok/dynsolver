using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using DynamicSolver.CoreMath.Execution;
using DynamicSolver.CoreMath.Execution.Compiler;
using DynamicSolver.CoreMath.Execution.Interpreter;
using DynamicSolver.CoreMath.Syntax;
using DynamicSolver.CoreMath.Syntax.Model;
using DynamicSolver.CoreMath.Syntax.Parser;
using JetBrains.Annotations;
using NUnit.Framework;

namespace DynamicSolver.CoreMath.Tests.Execution
{
    [TestFixture(typeof(InterpretedFunction))]
    [TestFixture(typeof(CompiledFunction))]
    public class ExecutableFunctionTests<T> where T: IExecutableFunction
    {
        [NotNull] private readonly SyntaxParser _parser = new SyntaxParser();

        private static IExecutableFunction CreateFunction(ISyntaxExpression statement)
        {
            try
            {
                return (IExecutableFunction) Activator.CreateInstance(typeof(T), statement);
            }
            catch (TargetInvocationException ex) when(ex.InnerException != null)
            {
                throw ex.InnerException;
            }            
        }

        [Test]
        public void Constructor_InvalidArguments_Throws()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.That(() => CreateFunction(null), Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_UnknownFunctionCall_Throws()
        {
            Assert.That(() => CreateFunction(new UnaryMinusOperator(new FunctionCall("foo", new NumericPrimitive("0")))), Throws.ArgumentException);
        }


        [TestCase("1 + tg(10)^(-e) + cos(1*pi)/3", new string[] {})]
        [TestCase("x", new [] {"x"})]
        [TestCase("x2 + x1 - x4 / x0", new [] {"x0", "x1", "x2", "x4"})]
        [TestCase("a + b^(-c) + cos(1*pi)/d", new[] {"a", "b", "c", "d"})]
        public void OrderedArguments_HasValidItems(string expression, string[] expectedArguments)
        {
            var statement = _parser.Parse(expression);
            var function = CreateFunction(statement);

            Assert.That(function.OrderedArguments, Is.Ordered.And.Unique.And.EqualTo(expectedArguments));
        }

        [Test]
        public void Execute_WithDictionary_NullArguments_Throws()
        {
            var statement = _parser.Parse("1 + 1");
            var function = CreateFunction(statement);

            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.That(() => function.Execute((IReadOnlyDictionary<string, double>)null), Throws.ArgumentNullException);
        }

        [TestCase("x - y", "")]
        [TestCase("x - y", "z=10")]
        [TestCase("x - y", "x=1")]
        [TestCase("x - y", "y=2")]
        public void Execute_WithDictionary_InvalidArguments_Throws(string expression, string arguments)
        {
            var args = arguments.Split(';').Where(s => !string.IsNullOrEmpty(s)).Select(arg => arg.Split('=')).ToDictionary(arg => arg[0], arg => double.Parse(arg[1]));
            
            var statement = _parser.Parse(expression);
            var function = CreateFunction(statement);

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
        [TestCase("x - y", -1, "x=1;y=2;z=10")]
        [TestCase("1", 1, "x=2")]
        public void Execute_WithDictionary_CalculatesAExpected(string expression, double expected, string variables)
        {
            var args = variables.Split(';').Where(s => !string.IsNullOrEmpty(s)).Select(arg => arg.Split('=')).ToDictionary(arg => arg[0], arg => double.Parse(arg[1], new NumberFormatInfo() {NumberDecimalSeparator = "."}));
            
            var statement = _parser.Parse(expression);
            var function = CreateFunction(statement);

            Assert.That(function.Execute(args), Is.EqualTo(expected).Within(Math.Max(Math.Abs(expected * 1e-5), 1e-10)));
        }

        [Test]
        public void Execute_WithArray_NullArguments_Throws()
        {
            var statement = _parser.Parse("1 + 1");
            var function = CreateFunction(statement);

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
            var function = CreateFunction(statement);

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
            var function = CreateFunction(statement);

            Assert.That(function.Execute(args), Is.EqualTo(expected).Within(Math.Max(Math.Abs(expected * 1e-5), 1e-10)));
        }
    }
}