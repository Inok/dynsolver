using System;
using System.Collections.Generic;
using System.Linq;
using DynamicSolver.ExpressionCompiler.Interpreter;
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

        public Dictionary<string, double>[] Solve(Dictionary<string, double> initialConditions, double step, double modellingTime)
        {
            if (step <= 0) throw new ArgumentOutOfRangeException(nameof(step));
            if (modellingTime <= 0) throw new ArgumentOutOfRangeException(nameof(modellingTime));
            if (step > modellingTime) throw new ArgumentOutOfRangeException();

            var variables = _equationSystem.Equations.SelectMany(e => e.Function.Analyzer.Variables).Distinct().ToList();

            var modellingVariable = "t";
            if (!variables.Contains(modellingVariable))
            {
                var independentVariables = variables.Except(_equationSystem.Equations.Select(e => e.LeadingDerivative.Variable.Name)).Except(initialConditions.Keys).ToList();
                if (independentVariables.Count == 1)
                {
                    modellingVariable = independentVariables[0];
                }
                else
                {
                    throw new ArgumentException("Cannot determine modelling variable.");
                }
            }

            if (initialConditions.ContainsKey(modellingVariable))
            {
                throw new ArgumentException("Time variable should be not set at initial conditions.");
            }

            foreach (var v in variables)
            {
                if (v != modellingVariable && !initialConditions.ContainsKey(v))
                {
                    throw new ArgumentException($"Initial value of {v} not found.");
                }
            }

            var functions = _equationSystem.Equations.Select(e => new {variable = e.LeadingDerivative.Variable, func = new InterpretedFunction(e.Function)}).ToList();

            var results = new Dictionary<string, double>[(int)(modellingTime / step) + 1];

            var prevVars = initialConditions.ToDictionary(v => v.Key, v => v.Value);
            prevVars.Add(modellingVariable, 0);
            results[0] = prevVars;

            int i = 1;
            for (var t = step; t <= modellingTime; t = step * i)
            {
                var vars = prevVars.ToDictionary(v => v.Key, v => v.Value);
                vars[modellingVariable] = t;

                foreach (var function in functions)
                {
                    var arguments = function.func.OrderedArguments.Select(a => prevVars[a]).ToArray();
                    vars[function.variable.Name] = prevVars[function.variable.Name] + step * function.func.Execute(arguments);
                }

                results[i] = prevVars = vars;
                i++;
            }

            return results;
        }
    }
}