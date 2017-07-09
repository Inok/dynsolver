using System;
using System.Globalization;
using DynamicSolver.CoreMath.Semantic.Model;
using DynamicSolver.CoreMath.Semantic.Print;
using NUnit.Framework;

namespace DynamicSolver.CoreMath.Tests.Semantic.Print
{
    [TestFixture]
    public class SemanticPrinterTests
    {
        private readonly SemanticPrinter _semanticPrinter = new SemanticPrinter();

        [SetUp]
        public void Setup()
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        }

        [TestCase(0d, "0")]
        [TestCase(1d, "1")]
        [TestCase(-42d, "-42")]
        [TestCase(1.23456789d, "1.23456789")]
        [TestCase(1.23456789123456789d, "1.23456789123457")]
        [TestCase(-987.654E-42, "-9.87654E-40")]
        [TestCase(1000E+20, "1E+23")]
        [TestCase(double.Epsilon, "4.94065645841247E-324")]
        [TestCase(double.MinValue, "-1.79769313486232E+308")]
        [TestCase(double.MaxValue, "1.79769313486232E+308")]
        [TestCase(Math.PI, "3.14159265358979")]
        [TestCase(Math.E, "2.71828182845905")]
        public void PrintElement_Constant_PrintsValue(double value, string expected)
        {
            var actual = _semanticPrinter.PrintElement(new Constant(value));

            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("x")]
        [TestCase("y")]
        [TestCase("t1")]
        public void PrintElement_Variable_WithExplicitName_PrintsName(string name)
        {
            var actual = _semanticPrinter.PrintElement(new Variable(name));

            Assert.That(actual, Is.EqualTo(name));
        }

        [Test]
        public void PrintElement_Variable_DifferentVariablesWithSameExplicitName_ThrowsInvalidOperationException()
        {
            var element = new AddOperation(new Variable("x"), new Variable("x"));
            Assert.That(() => _semanticPrinter.PrintElement(element), Throws.InvalidOperationException);
        }

        [Test]
        public void PrintElement_Variable_WithNoName_PrintsGeneratedName()
        {
            var actual = _semanticPrinter.PrintElement(new Variable());
            Assert.That(actual, Is.EqualTo("_gen$1"));
        }

        [Test]
        public void PrintElement_ManyVariables_WithNoName_PrintsDifferentNames()
        {
            var actual = _semanticPrinter.PrintElement(new AddOperation(new AddOperation(new Variable(), new Variable()), new Variable()));
            Assert.That(actual, Is.EqualTo("((_gen$1 + _gen$2) + _gen$3)"));
        }
        
        [Test]
        public void PrintElement_Variable_MultipleWithNoName_PrintsSameName()
        {
            var variable = new Variable();
            var actual = _semanticPrinter.PrintElement(new AddOperation(variable, variable));
            Assert.That(actual, Is.EqualTo("(_gen$1 + _gen$1)"));
        }

        [Test]
        public void PrintElement_Variable_VariableWithExplicitNameEqualToGeneratedName_ThrowsInvalidOperationException()
        {
            var genFirstElement = new AddOperation(new Variable("_gen$1"), new Variable());
            Assert.That(() => _semanticPrinter.PrintElement(genFirstElement), Throws.InvalidOperationException);

            var explicitFirstElement = new AddOperation(new Variable(), new Variable("_gen$1"));
            Assert.That(() => _semanticPrinter.PrintElement(explicitFirstElement), Throws.InvalidOperationException);
        }

        [Test]
        public void PrintElement_MinusOperation_PrintsMinusOperator()
        {
            Assert.That(_semanticPrinter.PrintElement(new MinusOperation(new Constant(1))), Is.EqualTo("-1"));
            Assert.That(_semanticPrinter.PrintElement(new MinusOperation(new Constant(2))), Is.EqualTo("-2"));
            Assert.That(_semanticPrinter.PrintElement(new MinusOperation(new Constant(-1))), Is.EqualTo("-(-1)"));
            Assert.That(_semanticPrinter.PrintElement(new MinusOperation(new Variable("x"))), Is.EqualTo("-x"));
            Assert.That(_semanticPrinter.PrintElement(new MinusOperation(new MinusOperation(new Variable("x")))), Is.EqualTo("-(-x)"));
            Assert.That(_semanticPrinter.PrintElement(new MinusOperation(new AddOperation(new Variable("x"), new Variable("y")))), Is.EqualTo("-(x + y)"));
            Assert.That(_semanticPrinter.PrintElement(new SubtractOperation(new MinusOperation(new Variable("x")), new MinusOperation(new Variable("y")))), Is.EqualTo("(-x - -y)"));
        }

        [Test]
        public void PrintElement_AddOperation_PrintsAddOperator()
        {
            var actual = _semanticPrinter.PrintElement(new AddOperation(new Constant(1), new Constant(2)));
            Assert.That(actual, Is.EqualTo("(1 + 2)"));
        }

        [Test]
        public void PrintElement_AddOperation_Multiple_PrintsScoped()
        {
            var actual = _semanticPrinter.PrintElement(new AddOperation(
                new AddOperation(new Constant(1), new Constant(2)),
                new AddOperation(new Constant(3), new Constant(4))));
            Assert.That(actual, Is.EqualTo("((1 + 2) + (3 + 4))"));
        }
        
        [Test]
        public void PrintElement_SubtractOperation_PrintsSubtractOperator()
        {
            var actual = _semanticPrinter.PrintElement(new SubtractOperation(new Constant(1), new Constant(2)));
            Assert.That(actual, Is.EqualTo("(1 - 2)"));
        }

        [Test]
        public void PrintElement_SubtractOperation_Multiple_PrintsScoped()
        {
            var actual = _semanticPrinter.PrintElement(new SubtractOperation(
                new SubtractOperation(new Constant(1), new Constant(2)),
                new SubtractOperation(new Constant(3), new Constant(4))));
            Assert.That(actual, Is.EqualTo("((1 - 2) - (3 - 4))"));
        }
        
        [Test]
        public void PrintElement_MultiplyOperation_PrintsMultiplyOperator()
        {
            var actual = _semanticPrinter.PrintElement(new MultiplyOperation(new Constant(1), new Constant(2)));
            Assert.That(actual, Is.EqualTo("(1 * 2)"));
        }

        [Test]
        public void PrintElement_MultiplyOperation_Multiple_PrintsScoped()
        {
            var actual = _semanticPrinter.PrintElement(new MultiplyOperation(
                new MultiplyOperation(new Constant(1), new Constant(2)),
                new MultiplyOperation(new Constant(3), new Constant(4))));
            Assert.That(actual, Is.EqualTo("((1 * 2) * (3 * 4))"));
        }
        
        [Test]
        public void PrintElement_DivideOperation_PrintsDivideOperator()
        {
            var actual = _semanticPrinter.PrintElement(new DivideOperation(new Constant(1), new Constant(2)));
            Assert.That(actual, Is.EqualTo("(1 / 2)"));
        }

        [Test]
        public void PrintElement_DivideOperation_Multiple_PrintsScoped()
        {
            var actual = _semanticPrinter.PrintElement(new DivideOperation(
                new DivideOperation(new Constant(1), new Constant(2)),
                new DivideOperation(new Constant(3), new Constant(4))));
            Assert.That(actual, Is.EqualTo("((1 / 2) / (3 / 4))"));
        }
        
        [Test]
        public void PrintElement_PowOperation_PrintsPowOperator()
        {
            var actual = _semanticPrinter.PrintElement(new PowOperation(new Constant(1), new Constant(2)));
            Assert.That(actual, Is.EqualTo("(1 ^ 2)"));
        }

        [Test]
        public void PrintElement_PowOperation_Multiple_PrintsScoped()
        {
            var actual = _semanticPrinter.PrintElement(new PowOperation(
                new PowOperation(new Constant(1), new Constant(2)),
                new PowOperation(new Constant(3), new Constant(4))));
            Assert.That(actual, Is.EqualTo("((1 ^ 2) ^ (3 ^ 4))"));
        }
    }
}