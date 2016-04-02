using DynamicSolver.ExpressionParser.Derivation;
using DynamicSolver.ExpressionParser.Expression;
using NUnit.Framework;

namespace DynamicSolver.ExpressionParser.Tests.Derivation
{
    [TestFixture]
    public class SymbolicDerivationServiceTests
    {
        private readonly ExpressionParser.Parser.ExpressionParser _parser = new ExpressionParser.Parser.ExpressionParser();
        private readonly SymbolicDerivationService _service = new SymbolicDerivationService();


        [Test(Description = "C' == 0")]
        public void ConstantStatement_ReturnsZero()
        {
            Assert.That(_service.GetDerivative(_parser.Parse("1"), "x"), Is.EqualTo(_parser.Parse("0")));
            Assert.That(_service.GetDerivative(_parser.Parse("0"), "x"), Is.EqualTo(_parser.Parse("0")));
            Assert.That(_service.GetDerivative(_parser.Parse("pi"), "x"), Is.EqualTo(_parser.Parse("0")));
            Assert.That(_service.GetDerivative(_parser.Parse("e"), "x"), Is.EqualTo(_parser.Parse("0")));
        }

        [Test(Description = "g' in respect of f == 0")]
        public void VariableStatement_WhenNotRespectVariable_ReturnsZero()
        {
            Assert.That(_service.GetDerivative(_parser.Parse("x"), "y"), Is.EqualTo(_parser.Parse("0")));
            Assert.That(_service.GetDerivative(_parser.Parse("y"), "x"), Is.EqualTo(_parser.Parse("0")));
            Assert.That(_service.GetDerivative(_parser.Parse("z"), "x"), Is.EqualTo(_parser.Parse("0")));
        }

        [Test(Description = "f' == 1")]
        public void VariableStatement_WhenRespectVariable_ReturnsOne()
        {
            Assert.That(_service.GetDerivative(_parser.Parse("x"), "x"), Is.EqualTo(_parser.Parse("1")));
            Assert.That(_service.GetDerivative(_parser.Parse("y"), "y"), Is.EqualTo(_parser.Parse("1")));
        }

        [Test(Description = "(-f)' == -(f')")]
        public void UnaryMinusStatement_ReturnsUnaryMinusOnOperandDerivative()
        {
            Assert.That(_service.GetDerivative(_parser.Parse("-x"), "x"), Is.EqualTo(WrapWithUnaryMinus("1")));
            Assert.That(_service.GetDerivative(_parser.Parse("-y"), "x"), Is.EqualTo(WrapWithUnaryMinus("0")));
            Assert.That(_service.GetDerivative(_parser.Parse("-pi"), "x"), Is.EqualTo(WrapWithUnaryMinus("0")));
        }

        [Test(Description = "(f + g)' == (f' + g')")]
        public void SumStatement_ReturnsSumOfDerivativesOfEachTerm()
        {
            Assert.That(_service.GetDerivative(_parser.Parse("1 + x"), "x"), Is.EqualTo(_parser.Parse("0 + 1")));
            Assert.That(_service.GetDerivative(_parser.Parse("e + y"), "x"), Is.EqualTo(_parser.Parse("0 + 0")));
        }

        [Test(Description = "(f - g)' == (f' - g')")]
        public void SubtractStatement_ReturnsSubtractOfDerivativesOfEachTerm()
        {
            Assert.That(_service.GetDerivative(_parser.Parse("1 - x"), "x"), Is.EqualTo(_parser.Parse("0 - 1")));
            Assert.That(_service.GetDerivative(_parser.Parse("e - y"), "x"), Is.EqualTo(_parser.Parse("0 - 0")));
        }

        [Test(Description = "(f*g)' == (f'*g + f*g')")]
        public void MultiplicationStatement_ReturnsExpressionTreeAccordingToDerivationRules()
        {
            Assert.That(_service.GetDerivative(_parser.Parse("x * x"), "x"), Is.EqualTo(_parser.Parse("1 * x + x * 1")));
            Assert.That(_service.GetDerivative(_parser.Parse("x * 3"), "x"), Is.EqualTo(_parser.Parse("1 * 3 + x * 0")));
            Assert.That(_service.GetDerivative(_parser.Parse("pi * x"), "x"), Is.EqualTo(_parser.Parse("0 * x + pi * 1")));
        }

        [Test(Description = "(f/g)' == ((f'*g - f*g') / g^2)")]
        public void DivideStatement_ReturnsExpressionTreeAccordingToDerivationRules()
        {
            Assert.That(_service.GetDerivative(_parser.Parse("x / x"), "x"), Is.EqualTo(_parser.Parse("(1 * x - x * 1) / x^2")));
            Assert.That(_service.GetDerivative(_parser.Parse("x / 3"), "x"), Is.EqualTo(_parser.Parse("(1 * 3 - x * 0) / 3^2")));
            Assert.That(_service.GetDerivative(_parser.Parse("pi / x"), "x"), Is.EqualTo(_parser.Parse("(0 * x - pi * 1) / x^2")));
        }

        [Test(Description = "e.g. sin(x)' = cos(x)")]
        public void TrivialFunctionStatement_ReturnsExpressionAccordingToTrivialFunctionDerivatives()
        {
            Assert.That(_service.GetDerivative(_parser.Parse("sin(x)"), "x"), Is.EqualTo(_parser.Parse("cos(x) * 1")));
            Assert.That(_service.GetDerivative(_parser.Parse("cos(x)"), "x"), Is.EqualTo(_parser.Parse("-sin(x) * 1")));
            Assert.That(_service.GetDerivative(_parser.Parse("tg(x)"), "x"), Is.EqualTo(_parser.Parse("(1/(cos(x))^2) * 1")));
            Assert.That(_service.GetDerivative(_parser.Parse("ctg(x)"), "x"), Is.EqualTo(_parser.Parse("(-1/(sin(x))^2) * 1")));
            Assert.That(_service.GetDerivative(_parser.Parse("ln(x)"), "x"), Is.EqualTo(_parser.Parse("(1/x) * 1")));
            Assert.That(_service.GetDerivative(_parser.Parse("lg(x)"), "x"), Is.EqualTo(_parser.Parse("(1/(x*ln(10))) * 1")));
            Assert.That(_service.GetDerivative(_parser.Parse("exp(x)"), "x"), Is.EqualTo(_parser.Parse("exp(x) * 1")));
        }

        [Test(Description = "(x(y))' == (x'(y) * y')")]
        public void CompositeFunctionStatement_ReturnsExpressionTreeAccordingToDerivationRules()
        {
            Assert.That(_service.GetDerivative(_parser.Parse("(x + 1)^(x)"), "x"), Is.EqualTo(_parser.Parse("(x + 1)^(x) * ((1 + 0) * (x /(x + 1)) + 1 * ln(x+1))")));
        }

        [Test(Description = "(f^g)' == f^g * (f'*g/f + g'*ln(f))")]
        public void PowStatement_ReturnsExpressionTreeAccordingToDerivationRules()
        {
            Assert.That(_service.GetDerivative(_parser.Parse("x^2"), "x"), Is.EqualTo(_parser.Parse("x^2 * (1*(2/x) + 0*ln(x))")));
            Assert.That(_service.GetDerivative(_parser.Parse("e^x"), "x"), Is.EqualTo(_parser.Parse("e^x * (0*(x/e) + 1*ln(e))")));
            Assert.That(_service.GetDerivative(_parser.Parse("(x + 1)^(x)"), "x"), Is.EqualTo(_parser.Parse("(x + 1)^(x) * ((1 + 0) * (x /(x + 1)) + 1 * ln(x+1))")));
        }

        private static Statement WrapWithUnaryMinus(string number)
        {
            return new Statement(new UnaryMinusOperator(new NumericPrimitive(number)));
        }
    }
}