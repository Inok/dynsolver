using System;
using System.Collections.Generic;
using System.Linq;
using DynamicSolver.Abstractions;
using DynamicSolver.Expressions.Execution;
using JetBrains.Annotations;

namespace DynamicSolver.DynamicSystem.Solver
{
    public abstract class ButcherTableDynamicSystemSolver : IDynamicSystemSolver
    {
        protected struct ExecutableFunctionInfo
        {
            public string Name { get; }
            public IExecutableFunction Function { get; }

            public ExecutableFunctionInfo(string name, IExecutableFunction function)
            {
                Name = name;
                Function = function;
            }
        }

        protected abstract double[,] A { get; }
        protected abstract double[] B { get; }
        
        [NotNull] private readonly IExecutableFunctionFactory _functionFactory;
        
        protected ButcherTableDynamicSystemSolver([NotNull] IExecutableFunctionFactory functionFactory)
        {
            if (functionFactory == null) throw new ArgumentNullException(nameof(functionFactory));
            _functionFactory = functionFactory;            
        }

        public IEnumerable<IReadOnlyDictionary<string, double>> Solve(ExplicitOrdinaryDifferentialEquationSystem equationSystem, IReadOnlyDictionary<string, double> initialConditions, double step)
        {
            if (step <= 0) throw new ArgumentOutOfRangeException(nameof(step));

            if (equationSystem.Equations.Any(e => e.LeadingDerivative.Order > 1))
            {
                throw new ArgumentException($"{nameof(EulerDynamicSystemSolver)} supports only equations with order = 1.");
            }

            if (!new HashSet<string>(equationSystem.Equations.SelectMany(e => e.Function.Analyzer.Variables)).SetEquals(initialConditions.Keys))
            {
                throw new ArgumentException("Initial values has different set of arguments from equation system.");
            }

            var functions = equationSystem.Equations
                .Select(e => new ExecutableFunctionInfo(e.LeadingDerivative.Variable.Name, _functionFactory.Create(e.Function)))
                .ToArray();
            var nameToIndex = functions.Select((f, i) => new { f, i }).ToDictionary(p => p.f.Name, p => p.i);

            var lastValues = initialConditions;
            while(true)
            {
                yield return lastValues = GetNextValues(functions, nameToIndex, lastValues, step);
            }            
        }

        private IReadOnlyDictionary<string, double> GetNextValues(ExecutableFunctionInfo[] functions, Dictionary<string, int> nameToIndex, IReadOnlyDictionary<string, double> variables, double step)
        {
            var k = new double[B.Length, functions.Length];
            var arguments = new double[functions.Length];

            for (int s = 0; s < B.Length; s++)
            {
                for (var f = 0; f < functions.Length; f++)
                {
                    var name = functions[f].Name;
                    double sum = 0.0;
                    for (int b = 0; b < s; b++)
                    {
                        sum += A[s, b]*k[b, f];
                    }

                    arguments[f] = variables[name] + step*sum;
                }

                for (var f = 0; f < functions.Length; f++)
                {
                    var function = functions[f].Function;
                    var args = function.OrderedArguments.Select(a => arguments[nameToIndex[a]]).ToArray();
                    k[s,f] = function.Execute(args);
                }
            }

            var vars = new Dictionary<string, double>();

            for (var f = 0; f < functions.Length; f++)
            {
                var function = functions[f];
                vars[function.Name] = variables[function.Name] + step* B.Select((b, i) => k[i, f] * b).Sum();
            }

            return vars;
        }
    }
}