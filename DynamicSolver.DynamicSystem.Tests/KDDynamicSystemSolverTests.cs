using System;
using System.Collections.Generic;
using System.Linq;
using DynamicSolver.DynamicSystem.Solver;
using DynamicSolver.Expressions.Execution.Compiler;
using DynamicSolver.Expressions.Parser;
using Inok.Tools.Dump;
using Inok.Tools.Linq;
using NUnit.Framework;
// ReSharper disable InconsistentNaming

namespace DynamicSolver.DynamicSystem.Tests
{
    [TestFixture]
    public class KDDynamicSystemSolverTests
    {
        [Test]
        public void Test()
        {
            var parser = new ExpressionParser();
            var solver = new KDDynamicSystemSolver(new CompiledFunctionFactory());

            var system = new ExplicitOrdinaryDifferentialEquationSystem(new[]
            {
                ExplicitOrdinaryDifferentialEquation.FromStatement(parser.Parse("x'= -x - 2*y")),
                ExplicitOrdinaryDifferentialEquation.FromStatement(parser.Parse("y'= 3*x - 4*y"))
            });

            var initial = new Dictionary<string, double>()
            {
                ["x"] = 1,
                ["y"] = 2
            };

            var solve = solver.Solve(system, initial, 2);

            Console.WriteLine(initial.Yield().Concat(solve.Take(3)).Dump(new DumpSettings {IncludeTypes = false, Indented = true}));
        }
    }
}