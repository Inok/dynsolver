using System;
using System.Collections.Generic;
using System.Linq;
using DynamicSolver.Abstractions.Expression;
using JetBrains.Annotations;

namespace DynamicSolver.Minimizer
{
    public class MinimizationTaskInput
    {
        public IStatement Statement { get; }

        public IReadOnlyCollection<VariableValue> Variables { get; }

        public MinimizationTaskInput([NotNull] IStatement statement, [NotNull] IEnumerable<VariableValue> variables)
        {
            if (statement == null) throw new ArgumentNullException(nameof(statement));
            if (!statement.Analyzer.IsComputable) throw new ArgumentException("Statement should be computable, but it's not.", nameof(statement));
            if (variables == null) throw new ArgumentNullException(nameof(variables));

            Statement = statement;
            Variables = variables.ToList();

            if (!Statement.Analyzer.GetVariablesSet().SetEquals(Variables.Select(v => v.VariableName)))
            {
                throw new ArgumentException("Variables set incompatible with statement.");
            }

            if (Variables.Any(v => !v.Value.HasValue))
            {
                throw new ArgumentException("Any variables has no value.");
            }
        }
    }
}