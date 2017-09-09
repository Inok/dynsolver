using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using DynamicSolver.CoreMath.Syntax;
using DynamicSolver.CoreMath.Syntax.Parser;
using DynamicSolver.DynamicSystem;
using DynamicSolver.DynamicSystem.Solvers;
using DynamicSolver.ViewModel.Common.Edit;
using JetBrains.Annotations;
using ReactiveUI;

namespace DynamicSolver.ViewModel.DynamicSystem
{
    public class DynamicSystemTaskViewModel : ReactiveObject
    {
        private readonly ObservableAsPropertyHelper<ExplicitOrdinaryDifferentialEquationSystemDefinition> _equationSystem;

        [NotNull]
        public ExplicitOrdinaryDifferentialEquationSystemViewModel EquationSystemInputViewModel { get; }

        [NotNull, ItemNotNull]
        public IReactiveList<EditViewModel<double?>> Variables { get; }

        public ExplicitOrdinaryDifferentialEquationSystemDefinition EquationSystem => _equationSystem.Value;

        public DynamicSystemTaskViewModel([NotNull] IExpressionParser parser)
        {
            if (parser == null) throw new ArgumentNullException(nameof(parser));

            EquationSystemInputViewModel = new ExplicitOrdinaryDifferentialEquationSystemViewModel(parser);
            Variables = new ReactiveList<EditViewModel<double?>>
            {
                ChangeTrackingEnabled = true
            };

            var parseResult = this.WhenAnyValue(m => m.EquationSystemInputViewModel.EquationSystem);

            parseResult.Where(r => r != null)
                .Throttle(TimeSpan.FromSeconds(0.2), DispatcherScheduler.Current)
                .Subscribe(CreateVariables);

            _equationSystem = Observable.Merge(
                    parseResult.Select(_ => Unit.Default),
                    this.WhenAnyObservable(m => m.Variables.Changed).Select(_ => Unit.Default),
                    this.WhenAnyObservable(m => m.Variables.ItemChanged).Select(_ => Unit.Default))
                .Throttle(TimeSpan.FromSeconds(0.5), DispatcherScheduler.Current)
                .Select(_ => GetTaskInput())
                .ToProperty(this, m => m.EquationSystem);
        }

        private void CreateVariables(IReadOnlyCollection<ExplicitOrdinaryDifferentialEquation> system)
        {
            if (system == null)
            {
                Variables.Clear();
                return;
            }

            var vars = system.Select(e => e.LeadingDerivative.Variable.Name)
                .Concat(system.SelectMany(e => new SyntaxExpressionAnalyzer(e.Function).Variables))
                .Distinct(StringComparer.Ordinal)
                .Select(v => new
                {
                    name = v,
                    value = Variables.FirstOrDefault(a => a.Name.Equals(v, StringComparison.Ordinal))?.Value
                });
            
            Variables.Clear();

            foreach (var variable in vars)
            {
                Variables.Add(new EditViewModel<double?>(variable.name, variable.value));
            }
        }

        private ExplicitOrdinaryDifferentialEquationSystemDefinition GetTaskInput()
        {
            if (EquationSystemInputViewModel.EquationSystem == null || Variables.Any(v => !v.Value.HasValue))
            {
                return null;
            }

            return new ExplicitOrdinaryDifferentialEquationSystemDefinition(
                EquationSystemInputViewModel.EquationSystem,
                new DynamicSystemState(0, Variables.ToDictionary(v => v.Name, v => v.Value.Value))
            );
        }
    }
}