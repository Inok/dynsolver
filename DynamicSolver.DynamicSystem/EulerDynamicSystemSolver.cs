using System;
using System.Collections.Generic;
using System.Linq;
using DynamicSolver.Abstractions;
using DynamicSolver.ExpressionCompiler.Interpreter;
using DynamicSolver.ExpressionParser.Expression;
using JetBrains.Annotations;

namespace DynamicSolver.DynamicSystem
{
    public class EulerDynamicSystemSolver
    {
        [NotNull]
        private readonly ExplicitOrdinaryDifferentialEquationSystem _equationSystem;

        public EulerDynamicSystemSolver([NotNull] ExplicitOrdinaryDifferentialEquationSystem equationSystem)
        {
            if (equationSystem == null) throw new ArgumentNullException(nameof(equationSystem));

            if(equationSystem.Equations.Any(e => e.LeadingDerivative.Order > 1))
            {
                throw new ArgumentException($"{nameof(EulerDynamicSystemSolver)} supports only equations with order = 1.");
            }

            _equationSystem = equationSystem;
        }

        public Dictionary<string, double>[] Solve(Dictionary<string, double> initialConditions, double step, double modellingLimit)
        {
            if (step <= 0) throw new ArgumentOutOfRangeException(nameof(step));
            if (modellingLimit <= 0) throw new ArgumentOutOfRangeException(nameof(modellingLimit));
            if (step > modellingLimit) throw new ArgumentOutOfRangeException();
            if (!new HashSet<string>(_equationSystem.Equations.SelectMany(e => e.Function.Analyzer.Variables)).SetEquals(initialConditions.Keys))
            {
                throw new ArgumentException("Initial values has different set of arguments from equation system.");
            }

            var results = new Dictionary<string, double>[(int)Math.Round(modellingLimit / step) + 1];
            
            results[0] = initialConditions.ToDictionary(v => v.Key, v => v.Value);

            var functions = _equationSystem.Equations
                .Select(e => new Tuple<VariablePrimitive, IExecutableFunction>(e.LeadingDerivative.Variable, new InterpretedFunction(e.Function)))
                .ToList();

            var lastValues = initialConditions;
            for(var i = 1; i < results.Length; i++)
            {
                results[i] = lastValues = GetNextValues(functions, lastValues, step);
            }

            return results;
        }

        private static Dictionary<string, double> GetNextValues(IEnumerable<Tuple<VariablePrimitive, IExecutableFunction>> functions, IReadOnlyDictionary<string, double> variables, double step)
        {
            var vars = variables.ToDictionary(v => v.Key, v => v.Value);

            foreach (var function in functions)
            {
                var arguments = function.Item2.OrderedArguments.Select(a => variables[a]).ToArray();
                vars[function.Item1.Name] = variables[function.Item1.Name] + step*function.Item2.Execute(arguments);
            }

            return vars;
        }
    }
}