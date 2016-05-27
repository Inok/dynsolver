using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace DynamicSolver.DynamicSystem
{
    public class ExplicitOrdinaryDifferentialEquationSystem
    {
        public IReadOnlyCollection<ExplicitOrdinaryDifferentialEquation> Equations { get; }
        
        public int Dimension => Equations.Count;
        
        public ExplicitOrdinaryDifferentialEquationSystem([NotNull] IEnumerable<ExplicitOrdinaryDifferentialEquation> equations)
        {
            if (equations == null) throw new ArgumentNullException(nameof(equations));

            var list = equations.ToList();
            ValidateEquationsSystem(list);

            Equations = list;
        }

        private static void ValidateEquationsSystem([NotNull] ICollection<ExplicitOrdinaryDifferentialEquation> equations)
        {
            if(equations == null) throw new ArgumentNullException(nameof(equations));
            if(equations.Count <= 0) throw new ArgumentException("Equations collection is empty.");

            var leadingDerivatives = new HashSet<VariableDerivative>(VariableDerivative.Comparer);
            var functionDerivatives = new HashSet<VariableDerivative>(VariableDerivative.Comparer);
            foreach (var eq in equations)
            {
                if (!leadingDerivatives.Add(eq.LeadingDerivative))
                {
                    throw new FormatException($"Leading derivatives has duplicates: {eq.LeadingDerivative}");
                }

                var derivatives = new DerivativeAnalyzer(eq.Function.Expression).AllVariableDerivatives();
                foreach (var d in derivatives)
                {
                    functionDerivatives.Add(d);
                }
            }

            var maxOrderVariableDerivatives = functionDerivatives.GroupBy(d => d.Variable, d => d.Order)
                .Select(g => new VariableDerivative(g.Key, g.AsEnumerable().Max()))
                .ToList();
            if (maxOrderVariableDerivatives.Any(d => leadingDerivatives.Any(ld => ld.Variable.Equals(d.Variable) && ld.Order <= d.Order)))
            {
                throw new FormatException("Functions has any derivative with order greater then or equal to leading variable derivatives order.");
            }

            if (maxOrderVariableDerivatives.Any(d => leadingDerivatives.All(ld => !ld.Variable.Equals(d.Variable))))
            {
                throw new FormatException("Functions has an derivative of variable that not presented at leading derivatives.");
            }
        }
    }
}