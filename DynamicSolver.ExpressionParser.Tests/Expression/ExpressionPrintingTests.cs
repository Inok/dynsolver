using DynamicSolver.Abstractions.Expression;
using DynamicSolver.ExpressionParser.Expression;
using NUnit.Framework;

namespace DynamicSolver.ExpressionParser.Tests.Expression
{
    [TestFixture]
    public class ExpressionPrintingTests
    {
        [Test]
        public void ComplexStatement_ToString()
        {
            IStatement statement = new Statement(
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
                            new NumericPrimitive("2")))
                ));

            Assert.That(statement.ToString(), Is.EqualTo("cos(-x * pi) / -((-1.5 - ln(x)) ^ 2)"));
        }
    }
}