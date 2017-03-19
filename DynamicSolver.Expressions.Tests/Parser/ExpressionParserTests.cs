using System;
using System.Collections.Generic;
using System.Globalization;
using DynamicSolver.Expressions.Expression;
using DynamicSolver.Expressions.Parser;
using Inok.Tools.Dump;
using NUnit.Framework;

namespace DynamicSolver.Expressions.Tests.Parser
{
    [TestFixture]
    public class ExpressionParserTests
    {
        private readonly IExpressionParser _parser = new ExpressionParser();

        [TestCase((string)null)]
        [TestCase(" ")]
        [TestCase("   ")]
        public void Parse_EmptyInput_ThrowsArgumentException(string input)
        {
            Assert.That(() => _parser.Parse(input), Throws.TypeOf<ArgumentException>());
        }

        [TestCase("0", 1d)]
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
            Assert.That(((NumericPrimitive)function.Argument).Value, Is.EqualTo(double.Parse(expectedArgumentToken, new NumberFormatInfo {NumberDecimalSeparator = "."})));
        }

        [Test]
        public void ParseFunctionCall_WhenFunctionArgumentIsFunction_ReturnsCorrectStatement()
        {
            var actual = _parser.Parse("sin(cos(pi))");

            Assert.That(actual.Expression, Is.InstanceOf<FunctionCall>());

            var function = (FunctionCall)actual.Expression;
            Assert.That(function.FunctionName, Is.EqualTo("sin"));
            Assert.That(function.Argument, Is.InstanceOf<FunctionCall>());

            var nestedFunction = (FunctionCall)function.Argument;
            Assert.That(nestedFunction.FunctionName, Is.EqualTo("cos"));
            Assert.That(nestedFunction.Argument, Is.InstanceOf<ConstantPrimitive>());
            Assert.That(((ConstantPrimitive)nestedFunction.Argument).Constant, Is.EqualTo(Constant.Pi));            
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
        public void ParseDerive_ReturnsCorrectStatement()
        {
            var actual = _parser.Parse("x''");

            Assert.That(actual.Expression, Is.InstanceOf<DeriveUnaryOperator>());

            var derive = (DeriveUnaryOperator)actual.Expression;
            Assert.That(derive.Operand, Is.InstanceOf<DeriveUnaryOperator>());

            derive = (DeriveUnaryOperator)derive.Operand;
            Assert.That(derive, Is.InstanceOf<DeriveUnaryOperator>());

            Assert.That(derive.Operand, Is.InstanceOf<VariablePrimitive>());

            var variable = (VariablePrimitive) derive.Operand;
            Assert.That(variable.Name, Is.EqualTo("x"));
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

        [TestCase("'")]
        [TestCase("x-'")]
        [TestCase("x'(')")]
        [TestCase("e'")]
        [TestCase("pi'")]
        [TestCase("cos(x)'")]
        public void ParseDerive_WhenInvalid_ThrowsFormatException(string input)
        {
            Assert.That(() => _parser.Parse(input), Throws.TypeOf<FormatException>());
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

            var constant = (ConstantPrimitive)minus.Operand;
            Assert.That(constant.Constant, Is.EqualTo(Constant.Pi));
        }

        [TestCase("=", typeof(AssignmentBinaryOperator))]
        [TestCase("+", typeof(AddBinaryOperator))]
        [TestCase("-", typeof(SubtractBinaryOperator))]
        [TestCase("*", typeof(MultiplyBinaryOperator))]
        [TestCase("/", typeof(DivideBinaryOperator))]
        [TestCase("^", typeof(PowBinaryOperator))]
        public void ParseBinary_WhenFunctionCall_ReturnsCorrectStatement(string op, Type operatorType)
        {
            var actual = _parser.Parse($"cos(1){op}ln(t)");

            Assert.That(actual.Expression, Is.InstanceOf(operatorType));

            var multiply = (BinaryOperator)actual.Expression;
            Assert.That(multiply.LeftOperand, Is.InstanceOf<FunctionCall>());
            Assert.That(((FunctionCall)multiply.LeftOperand).FunctionName, Is.EqualTo("cos"));
            Assert.That(((FunctionCall)multiply.LeftOperand).Argument, Is.InstanceOf<NumericPrimitive>().With.Property(nameof(NumericPrimitive.Token)).EqualTo("1"));

            Assert.That(multiply.RightOperand, Is.InstanceOf<FunctionCall>());
            Assert.That(((FunctionCall)multiply.RightOperand).FunctionName, Is.EqualTo("ln"));
            Assert.That(((FunctionCall)multiply.RightOperand).Argument, Is.InstanceOf<VariablePrimitive>().With.Property(nameof(VariablePrimitive.Name)).EqualTo("t"));
        }

        [TestCase("=", typeof(AssignmentBinaryOperator))]
        [TestCase("+", typeof(AddBinaryOperator))]
        [TestCase("-", typeof(SubtractBinaryOperator))]
        [TestCase("*", typeof(MultiplyBinaryOperator))]
        [TestCase("/", typeof(DivideBinaryOperator))]
        [TestCase("^", typeof(PowBinaryOperator))]
        public void ParseBinary_WhenPrimitive_ReturnsCorrectStatement(string op, Type operatorType)
        {
            var actual = _parser.Parse($"x{op}2");

            Assert.That(actual.Expression, Is.InstanceOf(operatorType));

            var pow = (BinaryOperator) actual.Expression;
            Assert.That(pow.LeftOperand, Is.InstanceOf<VariablePrimitive>().With.Property(nameof(VariablePrimitive.Name)).EqualTo("x"));
            Assert.That(pow.RightOperand, Is.InstanceOf<NumericPrimitive>().With.Property(nameof(NumericPrimitive.Token)).EqualTo("2"));            
        }

        [TestCase("=", typeof(AssignmentBinaryOperator))]
        [TestCase("+", typeof(AddBinaryOperator))]
        [TestCase("-", typeof(SubtractBinaryOperator))]
        [TestCase("*", typeof(MultiplyBinaryOperator))]
        [TestCase("/", typeof(DivideBinaryOperator))]
        [TestCase("^", typeof(PowBinaryOperator))]
        public void ParseBinary_WhenParenthesized_ReturnsCorrectStatement(string op, Type operatorType)
        {
            var actual = _parser.Parse($"(5){op}(y)");

            Assert.That(actual.Expression, Is.InstanceOf(operatorType));

            var pow = (BinaryOperator) actual.Expression;
            Assert.That(pow.LeftOperand, Is.InstanceOf<NumericPrimitive>().With.Property(nameof(NumericPrimitive.Token)).EqualTo("5"));
            Assert.That(pow.RightOperand, Is.InstanceOf<VariablePrimitive>().With.Property(nameof(VariablePrimitive.Name)).EqualTo("y"));
        }

        [TestCase("=", typeof(AssignmentBinaryOperator))]
        [TestCase("+", typeof(AddBinaryOperator))]
        [TestCase("-", typeof(SubtractBinaryOperator))]
        [TestCase("*", typeof(MultiplyBinaryOperator))]
        [TestCase("/", typeof(DivideBinaryOperator))]
        [TestCase("^", typeof(PowBinaryOperator))]
        public void ParseBinary_WhenUnaryMinus_ReturnsCorrectStatement(string op, Type operatorType)
        {
            var actual = _parser.Parse($"-x {op} -2");

            Assert.That(actual.Expression, Is.InstanceOf(operatorType));

            var pow = (BinaryOperator) actual.Expression;
            Assert.That(pow.LeftOperand, Is.InstanceOf<UnaryMinusOperator>());
            Assert.That(((UnaryMinusOperator)pow.LeftOperand).Operand, Is.InstanceOf<VariablePrimitive>().With.Property(nameof(VariablePrimitive.Name)).EqualTo("x"));
            Assert.That(pow.RightOperand, Is.InstanceOf<NumericPrimitive>().With.Property(nameof(NumericPrimitive.Token)).EqualTo("-2"));
        }

        // ReSharper disable once UnusedMethodReturnValue.Local

        private static IEnumerable<object[]> InvalidBinaryCases()
        {
            var samples = new[] {"5#", "4# ", "#3", " #2", "1##0"};
            var operators = new[] {"^", "*", "/", " +", "="};
            foreach (var sample in samples)
            {
                foreach (var op in operators)
                {
                    yield return new object[] {sample.Replace("#", op)};
                }
            }

            yield return new object[] {"5-"};
            yield return new object[] {"4- "};            
        }

        [TestCaseSource(nameof(InvalidBinaryCases))]
        public void ParseBinary_WhenInvalid_ThrowsFormatException(string input)
        {
            Assert.That(() => _parser.Parse(input), Throws.TypeOf<FormatException>());
        }

        [Test]
        public void ParseComplexExpression_ReturnsCorrectStatement()
        {
            IStatement expected = new Statement(
                new AssignmentBinaryOperator(
                    new VariablePrimitive("y"),
                    new DivideBinaryOperator(
                        new FunctionCall("cos",
                            new MultiplyBinaryOperator(
                                new UnaryMinusOperator(new VariablePrimitive("x")),
                                new ConstantPrimitive(Constant.Pi))),
                        new UnaryMinusOperator(
                            new PowBinaryOperator(
                                new SubtractBinaryOperator(
                                    new NumericPrimitive("-1.5"),
                                    new FunctionCall("ln", new VariablePrimitive("x"))),
                                new SubtractBinaryOperator(
                                    new AddBinaryOperator(
                                        new NumericPrimitive("1"),
                                        new MultiplyBinaryOperator(
                                            new VariablePrimitive("x"),
                                            new ConstantPrimitive(Constant.E))
                                        ),
                                    new ConstantPrimitive(Constant.Pi))
                                ))))
                );

            Assert.That(_parser.Parse("y = cos(-x * pi) / -((-1.5 - ln(x)) ^ (1 + x * e - pi))").Dump(), Is.EqualTo(expected.Dump()));
        }
    }
}