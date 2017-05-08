using System;
using System.Collections.Generic;
using System.Linq;
using DynamicSolver.CoreMath.Analysis;
using DynamicSolver.CoreMath.Execution;
using DynamicSolver.DynamicSystem.Solvers;
using JetBrains.Annotations;

namespace DynamicSolver.DynamicSystem
{
    public class ExplicitOrdinaryDifferentialEquationSystem : IExplicitOrdinaryDifferentialEquationSystem
    {
        [NotNull]
        public IExecutableFunctionFactory FunctionFactory { get; private set; }

        [NotNull]
        public IReadOnlyCollection<ExplicitOrdinaryDifferentialEquation> Equations { get; private set; }

        [NotNull]
        public DynamicSystemState InitialState { get; private set; }

        private IReadOnlyList<ExecutableFunctionInfo> _executableFunctions;
        [NotNull] public IReadOnlyList<ExecutableFunctionInfo> ExecutableFunctions
        {
            get
            {
                if(_executableFunctions != null)
                {
                    return _executableFunctions;
                }
                _executableFunctions = Equations.Select(e => new ExecutableFunctionInfo(e.LeadingDerivative.Variable.Name, FunctionFactory.Create(e.Function))).ToList();
                return _executableFunctions;
            }
        }

        private Dictionary<Tuple<string, string>, ExecutableFunctionInfo> _jacobian;
        [NotNull] public Dictionary<Tuple<string, string>, ExecutableFunctionInfo> Jacobian
        {
            get
            {
                if (_jacobian != null)
                {
                    return _jacobian;
                }

                return _jacobian = new JacobianCalculationService().GetJacobianFunctions(this);
            }
        }

        public ExplicitOrdinaryDifferentialEquationSystem(
            [NotNull] IEnumerable<ExplicitOrdinaryDifferentialEquation> equations,
            [NotNull] DynamicSystemState initialState,
            [NotNull] IExecutableFunctionFactory functionFactory
        )
        {
            if (equations == null) throw new ArgumentNullException(nameof(equations));
            if (initialState == null) throw new ArgumentNullException(nameof(initialState));
            if (functionFactory == null) throw new ArgumentNullException(nameof(functionFactory));


            var equationList = equations.ToList();
            ValidateEquationsSystem(equationList);
            ValidateState(equationList, initialState);

            Equations = equationList;
            InitialState = initialState;
            FunctionFactory = functionFactory;
        }

        public IExplicitOrdinaryDifferentialEquationSystem WithInitialState([NotNull] DynamicSystemState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            ValidateState(Equations, state);

            return new OverridenEquationSystem(this, state);
        }

        private static void ValidateEquationsSystem([NotNull] ICollection<ExplicitOrdinaryDifferentialEquation> equations)
        {
            if (equations == null) throw new ArgumentNullException(nameof(equations));
            if (equations.Count <= 0) throw new ArgumentException("Equations collection is empty.");

            var leadingDerivatives = new HashSet<VariableDerivative>(VariableDerivative.Comparer);
            var functionDerivatives = new HashSet<VariableDerivative>(VariableDerivative.Comparer);
            foreach (var eq in equations)
            {
                if (!leadingDerivatives.Add(eq.LeadingDerivative))
                {
                    throw new FormatException($"Leading derivatives has duplicates: {eq.LeadingDerivative}");
                }
                if (eq.LeadingDerivative.Order > 1)
                {
                    throw new FormatException("Order of equations should be equal to 1.");
                }

                var derivatives = new DerivativeAnalyzer(eq.Function).AllVariableDerivatives();
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

        private static void ValidateState(IEnumerable<ExplicitOrdinaryDifferentialEquation> equations, DynamicSystemState initialState)
        {
            var leadingDerivativeNames = new HashSet<string>(equations.SelectMany(e => new ExpressionAnalyzer(e.Function).Variables));
            if (!leadingDerivativeNames.SetEquals(initialState.DependentVariables.Keys))
            {
                throw new FormatException("Initial values has different set of arguments from equation system.");
            }
        }

        private class OverridenEquationSystem : IExplicitOrdinaryDifferentialEquationSystem
        {
            private readonly IExplicitOrdinaryDifferentialEquationSystem _system;

            public IExecutableFunctionFactory FunctionFactory => _system.FunctionFactory;

            public IReadOnlyCollection<ExplicitOrdinaryDifferentialEquation> Equations => _system.Equations;

            public DynamicSystemState InitialState { get; }

            public IReadOnlyList<ExecutableFunctionInfo> ExecutableFunctions => _system.ExecutableFunctions;

            public Dictionary<Tuple<string, string>, ExecutableFunctionInfo> Jacobian => _system.Jacobian;


            public OverridenEquationSystem([NotNull] IExplicitOrdinaryDifferentialEquationSystem system, [NotNull] DynamicSystemState initialState)
            {
                _system = system;
                InitialState = initialState;
            }

            public IExplicitOrdinaryDifferentialEquationSystem WithInitialState(DynamicSystemState state)
            {
                return _system.WithInitialState(state);
            }
        }
    }
}