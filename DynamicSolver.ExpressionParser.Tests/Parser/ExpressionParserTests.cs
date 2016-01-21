using System;
using System.Globalization;
using DynamicSolver.Abstractions.Expression;
using DynamicSolver.ExpressionParser.Expression;
using DynamicSolver.ExpressionParser.Parser;
using NUnit.Framework;

namespace DynamicSolver.ExpressionParser.Tests.Parser
{
    [TestFixture]
    public class ExpressionParserTests
    {
        private class ParserLoggingWrapper : IExpressionParser
        {
            private readonly IExpressionParser wrappedParser;

            public ParserLoggingWrapper(IExpressionParser parser)
            {
                wrappedParser = parser;
            }

            public IStatement Parse(string inputExpression)
            {
                Console.WriteLine($"Try to parse expression: <{inputExpression}>");

                try
                {
                    var result = wrappedParser.Parse(inputExpression);

                    Console.WriteLine("Result: " + result.Dump());
                    return result;
                }
                catch (Exception e) when(new Func<bool>(() => { Console.WriteLine(e); return false; })())
                {
                    throw;
                }
            }
        }

        private readonly IExpressionParser _parser = new ParserLoggingWrapper( new ExpressionParser.Parser.ExpressionParser());

        [TestCase((string)null)]
        [TestCase(" ")]
        [TestCase("   ")]
        public void Parse_EmptyInput_ThrowsArgumentException(string input)
        {
            Assert.That(() => _parser.Parse(input), Throws.TypeOf<ArgumentException>());
        }

        [TestCase("0", 0d)]
        [TestCase("1", 1d)]
        [TestCase("-2", -2d)]
        [TestCase("5.00", 5d)]
        [TestCase("-2.5", -2.5d)]
        [TestCase("42.9", 42.9d)]
        public void ParseNumeric_ReturnsCorrectStatement(string input, double expectedValue)
        {
            var actual = _parser.Parse(input);

            Assert.That(actual.Expression, Is.InstanceOf<NumericPrimitive>());

            var numeric = (NumericPrimitive)actual.Expression;
            Assert.That(numeric.Token, Is.EqualTo(input));
            Assert.That(numeric.Value, Is.EqualTo(expectedValue));
        }

        [TestCase(".5")]
        [TestCase("-")]
        [TestCase("-1.")]
        [TestCase("1.3.1")]
        public void ParseNumeric_InvalidInput_ThrowsFormatException(string input)
        {
            Assert.That(() => _parser.Parse(input), Throws.TypeOf<FormatException>());            
        }

        
        [TestCase("pi")]
        [TestCase("PI")]
        public void ParsePiConstant_ReturnsCorrectStatement(string input)
        {
            var actual = _parser.Parse(input);

            Assert.That(actual.Expression, Is.InstanceOf<ConstantPrimitive>());

            var constant = (ConstantPrimitive)actual.Expression;
            Assert.That(constant.Constant, Is.EqualTo(Constant.Pi));          
        }

        [TestCase("e")]
        [TestCase("E")]
        public void ParseEConstant_ReturnsCorrectStatement(string input)
        {
            var actual = _parser.Parse(input);

            Assert.That(actual.Expression, Is.InstanceOf<ConstantPrimitive>());

            var constant = (ConstantPrimitive)actual.Expression;
            Assert.That(constant.Constant, Is.EqualTo(Constant.E));          
        }

        [TestCase("cos(1.5)", "cos", "1.5")]
        [TestCase("ln(16)", "ln", "16")]
        [TestCase("lg(100)", "lg", "100")]
        [TestCase("sqrt(4)", "sqrt", "4")]
        [TestCase("sqrt((4))", "sqrt", "4")]
        [TestCase("lg5(25)", "lg5", "25")]
        [TestCase("sin( 1)", "sin", "1")]
        [TestCase("sin(1 )", "sin", "1")]
        [TestCase("sin(  1  )", "sin", "1")]
        public void ParseFunctionCall_ReturnsCorrectStatement(string input, string expectedFunctionName, string expectedArgumentToken)
        {
            var actual = _parser.Parse(input);

            Assert.That(actual.Expression, Is.InstanceOf<FunctionCall>());

            var function = (FunctionCall)actual.Expression;
            Assert.That(function.FunctionName, Is.EqualTo(expectedFunctionName));
            Assert.That(function.Argument, Is.InstanceOf<NumericPrimitive>());
            Assert.That(((NumericPrimitive)function.Argument).Token, Is.EqualTo(expectedArgumentToken));
            Assert.That(((NumericPrimitive)function.Argument).Value, Is.EqualTo(double.Parse(expectedArgumentToken, new NumberFormatInfo() {NumberDecimalSeparator = "."})));
        }

        [TestCase("cos()")]
        [TestCase("3sin(4)")]
        [TestCase("fn(1")]
        [TestCase("fn (1)")]
        [TestCase("fn1)")]
        public void ParseFunctionCall_InvalidInput_ThrowsFormatException(string input)
        {
            Assert.That(() => _parser.Parse(input), Throws.TypeOf<FormatException>());
        }

        [TestCase("x")]
        [TestCase("y2")]
        [TestCase("t_10")]
        [TestCase("x`")]
        [TestCase(" x")]
        [TestCase("y ")]
        [TestCase(" z ")]
        public void ParseIdentifier_ReturnsCorrectStatement(string input)
        {
            var actual = _parser.Parse(input);

            Assert.That(actual.Expression, Is.InstanceOf<VariablePrimitive>());

            var variable = (VariablePrimitive) actual.Expression;
            Assert.That(variable.Name, Is.EqualTo(input.Trim()));
        }

        [Test]
        public void ParseNegated_WhenIncludedNegation_ReturnsCorrectStatement()
        {
            var actual = _parser.Parse("-(-(-10))");

            Assert.That(actual.Expression, Is.InstanceOf<UnaryMinusOperator>());

            var op1 = ((UnaryMinusOperator)actual.Expression).Operand;
            Assert.That(op1, Is.InstanceOf<UnaryMinusOperator>());

            var op2 = ((UnaryMinusOperator)op1).Operand;
            Assert.That(op2, Is.InstanceOf<NumericPrimitive>());

            var numeric = (NumericPrimitive)op2;
            Assert.That(numeric.Token, Is.EqualTo("-10"));
            Assert.That(numeric.Value, Is.EqualTo(-10));

        }

        [TestCase("--2")]
        [TestCase("---1")]
        [TestCase("--x")]
        [TestCase("--e")]
        [TestCase("--cos(3)")]
        public void ParseNegated_WhenInvalidNegation_ThrowsFormatException(string input)
        {
            Assert.That(() => _parser.Parse(input), Throws.TypeOf<FormatException>());
        }

        [Test]
        public void ParseNegatedConstant_ReturnsCorrectStatement()
        {
            var actual = _parser.Parse("-pi");

            Assert.That(actual.Expression, Is.InstanceOf<UnaryMinusOperator>());

            var minus = (UnaryMinusOperator) actual.Expression;
            Assert.That(minus.Operand, Is.InstanceOf<ConstantPrimitive>());

            var constant = ((ConstantPrimitive)minus.Operand);
            Assert.That(constant.Constant, Is.EqualTo(Constant.Pi));
        }

    }
}