using System.Collections.Generic;
using DynamicSolver.CoreMath.Execution.Compiler;
using DynamicSolver.CoreMath.Execution.Interpreter;
using DynamicSolver.CoreMath.Parser;
using NUnit.Framework;

namespace DynamicSolver.DynamicSystem.Tests.Experiments
{
    [TestFixture, Explicit]
    public class ExecutionStrategyExperiments
    {
        private const string EXPRESSION = "x1 + 2*x2 - cos(x3)*e/ln(x4)";
        private readonly int[] _iterationsCount = { 1, 10, 25, 50, 75, 100, 150, 200, 350, 500, 750, 1000, 2000, 5000, 10000 };

        [Test]
        public void ExecutionTime_Compiler()
        {
            var parser = new ExpressionParser();
            var statement = parser.Parse(EXPRESSION);

            var arguments = new double[] { 1, 2, 3, 4 };

            var compiler = new CompiledFunctionFactory();

            //warmup
            for (var i = 0; i < 10; i++)
            {
                using (var analyzer = new ExecutionTimeAnalyzer("warmup"))
                {
                    analyzer.StartIteration();
                    compiler.Create(statement).Execute(arguments);
                }
            }

            foreach (var count in _iterationsCount)
            {
                using (var analyzer = new ExecutionTimeAnalyzer($"{count,6} iterations"))
                {
                    for (var run = 0; run < 10; run++)
                    {
                        analyzer.StartIteration();
                        var function = compiler.Create(statement);
                        for (var i = 0; i < count; i++)
                        {
                            function.Execute(arguments);
                        }
                    }
                }
            }
        }

        [Test]
        public void ExecutionTime_Interpreter()
        {
            var parser = new ExpressionParser();
            var statement = parser.Parse(EXPRESSION);

            var arguments = new Dictionary<string, double>() { { "x1", 1 }, { "x2", 2 }, { "x3", 3 }, { "x4", 4 } };

            var interpreter = new InterpretedFunctionFactory();

            //warmup
            for (var i = 0; i < 10; i++)
            {
                using (var analyzer = new ExecutionTimeAnalyzer("warmup"))
                {
                    analyzer.StartIteration();
                    interpreter.Create(statement).Execute(arguments);
                }
            }

            foreach (var count in _iterationsCount)
            {
                using (var analyzer = new ExecutionTimeAnalyzer($"{count,6} iterations"))
                {
                    for (var run = 0; run < 10; run++)
                    {
                        analyzer.StartIteration();
                        var function = interpreter.Create(statement);
                        for (var i = 0; i < count; i++)
                        {
                            function.Execute(arguments);
                        }
                    }
                }
            }
        }
    }
}