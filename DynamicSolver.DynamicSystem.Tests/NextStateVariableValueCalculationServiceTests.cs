using System.Collections.Generic;
using DynamicSolver.Expressions.Expression;
using DynamicSolver.Expressions.Parser;
using NUnit.Framework;

namespace DynamicSolver.DynamicSystem.Tests
{
    [TestFixture]
    public class NextStateVariableValueCalculationServiceTests
    {
        private IExpressionParser Parser { get; } = new ExpressionParser();

        private NextStateVariableValueCalculationService Service { get; } = new NextStateVariableValueCalculationService();

        [Test]
        public void Express_WhenInvalidParameters_ThrowsArgumentException()
        {
            var equationSystem = new ExplicitOrdinaryDifferentialEquationSystem(new[]
            {
                ExplicitOrdinaryDifferentialEquation.FromStatement(Parser.Parse("x1' = x1")),
            });

            Assert.That(() => Service.ExpressNextStateVariableValueExpressions(null, "h"), Throws.ArgumentNullException);
            Assert.That(() => Service.ExpressNextStateVariableValueExpressions(equationSystem, null), Throws.ArgumentException);
            Assert.That(() => Service.ExpressNextStateVariableValueExpressions(equationSystem, ""), Throws.ArgumentException);
            Assert.That(() => Service.ExpressNextStateVariableValueExpressions(equationSystem, "  "), Throws.ArgumentException);
        }

        [Test]
        public void Express_WhenEquationSystemContainsVariableEqualToStepVariable_ThrowsArgumentException()
        {
            var equations = new[]
            {
                ExplicitOrdinaryDifferentialEquation.FromStatement(Parser.Parse("x1' = -sin(x2)")),
                ExplicitOrdinaryDifferentialEquation.FromStatement(Parser.Parse("x2' = cos(x1)"))
            };
            var equationSystem = new ExplicitOrdinaryDifferentialEquationSystem(equations);

            Assert.That(() => Service.ExpressNextStateVariableValueExpressions(equationSystem, "x1"), Throws.ArgumentException);            
            Assert.That(() => Service.ExpressNextStateVariableValueExpressions(equationSystem, "x2"), Throws.ArgumentException);            
        }

        [Test]
        public void Express_LoopbackTest_CalculatesCorrectExpressions()
        {
            var equations = new []
            {
                ExplicitOrdinaryDifferentialEquation.FromStatement(Parser.Parse("x1' = -sin(x2)")),
                ExplicitOrdinaryDifferentialEquation.FromStatement(Parser.Parse("x2' = cos(x1)"))
            };
            var equationSystem = new ExplicitOrdinaryDifferentialEquationSystem(equations);

            var actual = Service.ExpressNextStateVariableValueExpressions(equationSystem, "h");

            IDictionary<string, IStatement> expected = new Dictionary<string, IStatement>()
            {
                ["x1"] = Parser.Parse("(1-h*(-(cos(x2)*0)))*x1 + (0 - h*(-(cos(x2)*1)))*x2"),
                ["x2"] = Parser.Parse("(0 - h*(-sin(x1)*1))*x1 + (1 - h*(-sin(x1)*0))*x2")
            };

            Assert.That(actual, Is.EquivalentTo(expected));
        }
    }
}