using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using DynamicSolver.CoreMath.Analysis;
using DynamicSolver.CoreMath.Parser;
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

            var actualVariables = system.SelectMany(e => new ExpressionAnalyzer(e.Function).Variables)
                .Concat(system.Select(e => e.LeadingDerivative.Variable.Name))
                .Distinct(StringComparer.Ordinal)
                .ToList();

            Variables.RemoveAll(Variables.Where(v => !actualVariables.Contains(v.Name)).ToList());

            foreach (var variable in actualVariables.Except(Variables.Select(v => v.Name).ToList(), StringComparer.Ordinal))
            {
                Variables.Add(new EditViewModel<double?>(variable, null));
            }

            Variables.Sort(Comparer<EditViewModel<double?>>.Create((v1, v2) => StringComparer.Ordinal.Compare(v1.Name, v2.Name)));
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