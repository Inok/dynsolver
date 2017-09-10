using System;
using JetBrains.Annotations;

namespace DynamicSolver.Modelling.Solvers
{
    public class DynamicSystemSolverDescription
    {
        [NotNull]
        public string Name { get; }

        public int Order { get; }

        public bool IsSymmetric { get; }
        
        public bool UseEvenExtrapolationCoefficients { get; }
        
        public DynamicSystemSolverDescription([NotNull] string name, int order, bool isSymmetric, bool useEvenExtrapolationCoefficients = false)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Value cannot be null or empty.", nameof(name));
            if (order <= 0) throw new ArgumentOutOfRangeException(nameof(order));
            
            Name = name;
            Order = order;
            IsSymmetric = isSymmetric;
            UseEvenExtrapolationCoefficients = useEvenExtrapolationCoefficients;
        }
    }
}