using System;
using System.Collections.Generic;
using System.Linq;
using DynamicSolver.Abstractions.Expression;
using DynamicSolver.ViewModel.Annotations;

namespace DynamicSolver.ViewModel.Minimization
{
    public class MinimizationTaskInput
    {
        public IStatement Statement { get; }

        public IReadOnlyCollection<VariableViewModel> Variables { get; }

        public MinimizationTaskInput([NotNull] IStatement statement, [NotNull] IEnumerable<VariableViewModel> variables)
        {
            if (statement == null) throw new ArgumentNullException(nameof(statement));
            if (variables == null) throw new ArgumentNullException(nameof(variables));

            Statement = statement;
            Variables = variables.ToList();

            if(!Statement.Analyzer.GetVariablesSet().SetEquals(Variables.Select(v => v.VariableName)))
            {
                throw new ArgumentException("Variables set incompatible with statement.");
            }

            if(Variables.Any(v => !v.Value.HasValue))
            {
                throw new ArgumentException("Any variables has no value.");
            }
        }
    }
}