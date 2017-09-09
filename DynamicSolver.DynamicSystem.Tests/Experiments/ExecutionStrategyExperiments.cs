using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DynamicSolver.CoreMath.Execution.Compiler;
using DynamicSolver.CoreMath.Execution.Interpreter;
using DynamicSolver.CoreMath.Syntax.Parser;
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
            var parser = new SyntaxParser();
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
            var parser = new SyntaxParser();
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


        [Test]
        public void ExecutionTime_Compiler_Init()
        {
            var parser = new SyntaxParser();
            var statement = parser.Parse(EXPRESSION);

            var factory = new CompiledFunctionFactory();

            //warmup
            for (var i = 0; i < 10; i++)
            {
                using (var analyzer = new ExecutionTimeAnalyzer("warmup"))
                {
                    analyzer.StartIteration();
                    var function = factory.Create(statement);
                    GC.KeepAlive(function);
                }
            }

            foreach (var count in _iterationsCount)
            {
                using (var analyzer = new ExecutionTimeAnalyzer($"{count,6} iterations"))
                {
                    for (var run = 0; run < 100; run++)
                    {
                        analyzer.StartIteration();
                        var function = factory.Create(statement);
                        GC.KeepAlive(function);
                    }
                }
            }
        }

        [Test]
        public void ExecutionTime_Interpreter_Init()
        {
            var parser = new SyntaxParser();
            var statement = parser.Parse(EXPRESSION);

            var factory = new InterpretedFunctionFactory();

            //warmup
            for (var i = 0; i < 10; i++)
            {
                using (var analyzer = new ExecutionTimeAnalyzer("warmup"))
                {
                    analyzer.StartIteration();
                    var function = factory.Create(statement);
                    GC.KeepAlive(function);
                }
            }

            foreach (var count in _iterationsCount)
            {
                using (var analyzer = new ExecutionTimeAnalyzer($"{count,6} iterations"))
                {
                    for (var run = 0; run < 100; run++)
                    {
                        analyzer.StartIteration();
                        var function = factory.Create(statement);
                        GC.KeepAlive(function);
                    }
                }
            }
        }

        [Test]
        public void ExecutionTime_Compiler_Execute()
        {
            var parser = new SyntaxParser();
            var statement = parser.Parse(EXPRESSION);

            var arguments = new Dictionary<string, double> { { "x1", 1.0 }, { "x2", 2.0 }, { "x3", 3.0 }, { "x4", 4.0 } };
            //var arguments = new double[] { 1, 2, 3, 4 };

            var factory = new CompiledFunctionFactory();
            var function = factory.Create(statement);

            //warmup
            for (var i = 0; i < 10; i++)
            {
                using (var analyzer = new ExecutionTimeAnalyzer("warmup"))
                {
                    analyzer.StartIteration();
                    function.Execute(arguments);
                }
            }

            foreach (var count in _iterationsCount)
            {
                using (var analyzer = new ExecutionTimeAnalyzer($"{count,6} iterations"))
                {
                    for (var run = 0; run < 100; run++)
                    {
                        analyzer.StartIteration();
                        for (var i = 0; i < count; i++)
                        {
                            function.Execute(arguments);
                        }
                    }
                }
            }
        }


        [Test]
        public void ExecutionTime_Interpreter_Execute()
        {
            var parser = new SyntaxParser();
            var statement = parser.Parse(EXPRESSION);

            //var arguments = new Dictionary<string, double> { { "x1", 1.0 }, { "x2", 2.0 }, { "x3", 3.0 }, { "x4", 4.0 } };
            var arguments = new double[] { 1, 2, 3, 4 };

            var factory = new InterpretedFunctionFactory();
            var function = factory.Create(statement);

            //warmup
            for (var i = 0; i < 10; i++)
            {
                using (var analyzer = new ExecutionTimeAnalyzer("warmup"))
                {
                    analyzer.StartIteration();
                    function.Execute(arguments);
                }
            }

            foreach (var count in _iterationsCount)
            {
                using (var analyzer = new ExecutionTimeAnalyzer($"{count,6} iterations"))
                {
                    for (var run = 0; run < 100; run++)
                    {
                        analyzer.StartIteration();
                        for (var i = 0; i < count; i++)
                        {
                            function.Execute(arguments);
                        }
                    }
                }
            }
        }

        [Test]
        public void ExecutionTime_Compiler_Complex()
        {
            var parser = new SyntaxParser();
            var statement = parser.Parse(EXPRESSION);

            var arguments = new Dictionary<string, double> { { "x1", 1.0 }, { "x2", 2.0 }, { "x3", 3.0 }, { "x4", 4.0 } };

            var factory = new CompiledFunctionFactory();
            

            //warmup
            for (var i = 0; i < 10; i++)
            {
                using (var analyzer = new ExecutionTimeAnalyzer("warmup"))
                {
                    analyzer.StartIteration();
                    var function = factory.Create(statement);
                    function.Execute(arguments);
                }
            }

            foreach (var count in _iterationsCount)
            {
                using (var analyzer = new ExecutionTimeAnalyzer($"{count,6} iterations"))
                {
                    for (var run = 0; run < 100; run++)
                    {
                        analyzer.StartIteration();
                        var function = factory.Create(statement);
                        for (var i = 0; i < count; i++)
                        {
                            function.Execute(arguments);
                        }
                    }
                }
            }
        }

        [Test]
        public void ExecutionTime_Interpreter_Complex()
        {
            var parser = new SyntaxParser();
            var statement = parser.Parse(EXPRESSION);

            var arguments = new Dictionary<string, double> { { "x1", 1.0 }, { "x2", 2.0 }, { "x3", 3.0 }, { "x4", 4.0 } };

            var factory = new InterpretedFunctionFactory();

            //warmup
            for (var i = 0; i < 10; i++)
            {
                using (var analyzer = new ExecutionTimeAnalyzer("warmup"))
                {
                    analyzer.StartIteration();
                    var function = factory.Create(statement);
                    function.Execute(arguments);
                }
            }

            foreach (var count in _iterationsCount)
            {
                using (var analyzer = new ExecutionTimeAnalyzer($"{count,6} iterations"))
                {
                    for (var run = 0; run < 100; run++)
                    {
                        analyzer.StartIteration();
                        var function = factory.Create(statement);
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