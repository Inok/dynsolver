using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using DynamicSolver.DynamicSystem;
using DynamicSolver.Expressions.Parser;
using DynamicSolver.ViewModel.Common.Edit;
using JetBrains.Annotations;
using ReactiveUI;

namespace DynamicSolver.ViewModel.DynamicSystem
{
    public class DynamicSystemTaskViewModel : ReactiveObject
    {
        private double _step = 0.01;
        private double _time = 10;

        private readonly ObservableAsPropertyHelper<DynamicSystemSolverInput> _taskInput;

        [NotNull]
        public ExplicitOrdinaryDifferentialEquationSystemViewModel EquationSystemInputViewModel { get; }

        [NotNull, ItemNotNull]
        public IReactiveList<EditViewModel<double?>> Variables { get; }

        public double Step
        {
            get { return _step; }
            set { this.RaiseAndSetIfChanged(ref _step, value); }
        }

        public double Time
        {
            get { return _time; }
            set { this.RaiseAndSetIfChanged(ref _time, value); }
        }

        public DynamicSystemSolverInput TaskInput  => _taskInput.Value;
        

        public DynamicSystemTaskViewModel([NotNull] IExpressionParser parser)
        {
            if (parser == null) throw new ArgumentNullException(nameof(parser));
            
            EquationSystemInputViewModel = new ExplicitOrdinaryDifferentialEquationSystemViewModel(parser);
            Variables = new ReactiveList<EditViewModel<double?>>
            {
                ChangeTrackingEnabled = true
            };


            var parseResult = this.WhenAnyValue(m => m.EquationSystemInputViewModel.EquationSystem);

            parseResult.Where(r => r != null).Throttle(TimeSpan.FromSeconds(0.5), DispatcherScheduler.Current).Subscribe(CreateVariables);

            _taskInput = Observable.Merge(
                    parseResult.Select(_ => Unit.Default),
                    this.WhenAnyObservable(m => m.Variables.Changed).Select(_ => Unit.Default),
                    this.WhenAnyObservable(m => m.Variables.ItemChanged).Select(_ => Unit.Default),
                    this.WhenAnyValue(m => m.Step).Select(_ => Unit.Default),
                    this.WhenAnyValue(m => m.Time).Select(_ => Unit.Default))
                .Throttle(TimeSpan.FromSeconds(0.5), DispatcherScheduler.Current)
                .Select(_ => GetTaskInput())
                .ToProperty(this, m => m.TaskInput);
        }

        private void CreateVariables(ExplicitOrdinaryDifferentialEquationSystem system)
        {
            if (system == null)
            {
                Variables.Clear();
                return;
            }

            var actualVariables = system.Equations
                .SelectMany(e => e.Function.Analyzer.Variables)
                .Concat(system.Equations.Select(e => e.LeadingDerivative.Variable.Name))
                .Distinct(StringComparer.Ordinal).ToList();

            Variables.RemoveAll(Variables.Where(v => !actualVariables.Contains(v.Name)).ToList());
            
            foreach (var variable in actualVariables.Except(Variables.Select(v => v.Name).ToList(), StringComparer.Ordinal))
            {
                Variables.Add(new EditViewModel<double?>(variable, null));
            }

            Variables.Sort(Comparer<EditViewModel<double?>>.Create((v1, v2) => StringComparer.Ordinal.Compare(v1.Name, v2.Name)));
        }

        private DynamicSystemSolverInput GetTaskInput()
        {
            if (EquationSystemInputViewModel.EquationSystem == null)
            {
                return null;
            }

            if (Variables.Any(v => !v.Value.HasValue))
            {
                return null;
            }

            return new DynamicSystemSolverInput(EquationSystemInputViewModel.EquationSystem, Variables.ToDictionary(v => v.Name, v => v.Value.Value), Step, Time);
        }
    }
}