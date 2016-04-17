using System.Collections.Generic;
using System.Linq;
using DynamicSolver.Expressions.Execution.Interpreter;
using DynamicSolver.Expressions.Parser;
using NUnit.Framework;

namespace DynamicSolver.DynamicSystem.Tests
{
    [TestFixture]
    public class EulerDynamicSystemSolverTests
    {
        [Test]
        public void CheckCalculatesCorrectly()
        {
            var parser = new ExpressionParser();

            var solver = new EulerDynamicSystemSolver(new InterpretedFunctionFactory(),
                new ExplicitOrdinaryDifferentialEquationSystem(new[]
                {
                    ExplicitOrdinaryDifferentialEquation.FromStatement(parser.Parse("t'= 1")),
                    ExplicitOrdinaryDifferentialEquation.FromStatement(parser.Parse("x'= -cos(x) + sin(t)")),
                }));

            var actual = solver.Solve(new Dictionary<string, double>() {["x"] = 1, ["t"] = 0}, 0.2).Take(5).ToList();

            Assert.That(actual.Count, Is.EqualTo(5));
            Assert.That(actual[0], Is.EqualTo(new Dictionary<string, double>() { ["x"] = 0.8919, ["t"] = 0.2 }).Within(0.0001));
            Assert.That(actual[1], Is.EqualTo(new Dictionary<string, double>() { ["x"] = 0.8061, ["t"] = 0.4 }).Within(0.0001));
            Assert.That(actual[2], Is.EqualTo(new Dictionary<string, double>() { ["x"] = 0.7455, ["t"] = 0.6 }).Within(0.0001));
            Assert.That(actual[3], Is.EqualTo(new Dictionary<string, double>() { ["x"] = 0.7115, ["t"] = 0.8 }).Within(0.0001));
            Assert.That(actual[4], Is.EqualTo(new Dictionary<string, double>() { ["x"] = 0.7035, ["t"] = 1.0 }).Within(0.0001));
        }
    }
}