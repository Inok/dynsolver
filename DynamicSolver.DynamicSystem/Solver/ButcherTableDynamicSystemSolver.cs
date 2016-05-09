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

        public IEnumerable<IDictionary<string, double>> Solve(ExplicitOrdinaryDifferentialEquationSystem equationSystem, IDictionary<string, double> initialConditions, double step)
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

        private Dictionary<string, double> GetNextValues(ExecutableFunctionInfo[] functions, Dictionary<string, int> nameToIndex, IDictionary<string, double> variables, double step)
        {
            var vars = variables.ToDictionary(v => v.Key, v => v.Value);
            
            var k = new double[B.Length, functions.Length];

            for (int s = 0; s < B.Length; s++)
            {
                for (var i = 0; i < functions.Length; i++)
                {
                    Func<int, double> getIncrement = fi =>
                    {
                        double increment = 0;
                        for (int b = 0; b < s; b++)
                        {
                            increment += A[s, b]*k[b, fi];
                        }
                        return increment;
                    };

                    var function = functions[i];
                    var args = function.Function.OrderedArguments.Select(a => variables[a] + getIncrement(nameToIndex[a])*step).ToArray();
                    k[s,i] = function.Function.Execute(args);
                }
            }

            for (var i = 0; i < functions.Length; i++)
            {
                double stepMultiplier = 0;
                for (int s = 0; s < B.Length; s++)
                {
                    stepMultiplier += k[s, i]*B[s];
                }
                var function = functions[i];
                vars[function.Name] = variables[function.Name] + step*stepMultiplier;
            }

            return vars;
        }
    }
}