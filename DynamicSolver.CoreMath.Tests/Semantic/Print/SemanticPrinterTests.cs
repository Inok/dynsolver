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
    }
}