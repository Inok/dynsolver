using System.Collections.Generic;
using System.Linq;
using DynamicSolver.Expressions.Execution;
using JetBrains.Annotations;

namespace DynamicSolver.DynamicSystem.Solver
{
    public abstract class ButcherTableDynamicSystemSolver : DynamicSystemSolver<Dictionary<string, int>>, IDynamicSystemSolver
    {
        protected abstract double[,] A { get; }
        protected abstract double[] B { get; }
        
        protected ButcherTableDynamicSystemSolver([NotNull] IExecutableFunctionFactory functionFactory) : base(functionFactory)
        {
        
        }

        protected override Dictionary<string, int> GetExtraArguments(ExplicitOrdinaryDifferentialEquationSystem equationSystem, IList<ExecutableFunctionInfo> functions, IReadOnlyDictionary<string, double> initialConditions, double step)
        {
            return functions.Select((f, i) => new { f, i }).ToDictionary(p => p.f.Name, p => p.i);
        }

        protected override IReadOnlyDictionary<string, double> GetNextValues(IList<ExecutableFunctionInfo> functions, IReadOnlyDictionary<string, double> variables, double step, Dictionary<string, int> extra)
        {
            var k = new double[B.Length, functions.Count];
            var arguments = new double[functions.Count];

            for (int s = 0; s < B.Length; s++)
            {
                for (var f = 0; f < functions.Count; f++)
                {
                    var name = functions[f].Name;
                    double sum = 0.0;
                    for (int b = 0; b < s; b++)
                    {
                        sum += A[s, b]*k[b, f];
                    }

                    arguments[f] = variables[name] + step*sum;
                }

                for (var f = 0; f < functions.Count; f++)
                {
                    var function = functions[f].Function;
                    var args = function.OrderedArguments.Select(a => arguments[extra[a]]).ToArray();
                    k[s,f] = function.Execute(args);
                }
            }

            var vars = new Dictionary<string, double>();

            for (var f = 0; f < functions.Count; f++)
            {
                var function = functions[f];
                vars[function.Name] = variables[function.Name] + step* B.Select((b, i) => k[i, f] * b).Sum();
            }

            return vars;
        }
    }
}