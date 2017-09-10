using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using DynamicSolver.App.ViewModel.Common.Select;
using DynamicSolver.Modelling.Solvers;
using DynamicSolver.Modelling.Solvers.Extrapolation;
using JetBrains.Annotations;
using ReactiveUI;

namespace DynamicSolver.App.ViewModel.DynamicSystem
{
    public class ModellingSettingsViewModel : ReactiveObject
    {
        private double _step = 0.01;
        private double _time = 10;

        [NotNull]
        public SelectViewModel<IDynamicSystemSolver> SolverSelect { get; }
        
        [NotNull]
        public SelectViewModel<int?> ExtrapolationStagesSelect { get; }
        
        private readonly ObservableAsPropertyHelper<ModellingSettings> _modellingSettings;
        public ModellingSettings ModellingSettings => _modellingSettings.Value;

        public double Step
        {
            get => _step;
            set => this.RaiseAndSetIfChanged(ref _step, value);
        }

        public double Time
        {
            get => _time;
            set => this.RaiseAndSetIfChanged(ref _time, value);
        }

        public ModellingSettingsViewModel([NotNull] IEnumerable<IDynamicSystemSolver> solvers)
        {
            if (solvers == null) throw new ArgumentNullException(nameof(solvers));

            var solverSelect = new SelectViewModel<IDynamicSystemSolver>(false);
            foreach (var solver in solvers)
            {
                solverSelect.AddItem(solver.Description.Name, solver);
            }
            solverSelect.SelectedItem = solverSelect.Items.FirstOrDefault();
            SolverSelect = solverSelect;

            var extrapolationStagesSelect = new SelectViewModel<int?>()
            {
                Items =
                {
                    new SelectItemViewModel<int?>("Disabled", null),
                    new SelectItemViewModel<int?>("1 stage", 1),
                    new SelectItemViewModel<int?>("2 stages", 2),
                    new SelectItemViewModel<int?>("3 stages", 3),
                    new SelectItemViewModel<int?>("4 stages", 4),
                    new SelectItemViewModel<int?>("5 stages", 5),
                    new SelectItemViewModel<int?>("6 stages", 6),
                    new SelectItemViewModel<int?>("7 stages", 7),
                    new SelectItemViewModel<int?>("8 stages", 8),
                }
            };
            extrapolationStagesSelect.SelectedItem = extrapolationStagesSelect.Items[0];
            ExtrapolationStagesSelect = extrapolationStagesSelect;
            
            _modellingSettings = this.WhenAnyValue(
                    m => m.SolverSelect.SelectedItem,
                    m => m.ExtrapolationStagesSelect.SelectedItem,
                    m => m.Step,
                    m => m.Time)
                .Throttle(TimeSpan.FromSeconds(0.5), DispatcherScheduler.Current)
                .Select(CreateModel)
                .ToProperty(this, m => m.ModellingSettings);
        }

        private static ModellingSettings CreateModel(Tuple<SelectItemViewModel<IDynamicSystemSolver>, SelectItemViewModel<int?>, double, double> model)
        {
            var selectedExtrapolation = model.Item2.Value;
            var solver = selectedExtrapolation != null
                ? new ExtrapolationSolver(model.Item1.Value, selectedExtrapolation.Value)
                : model.Item1.Value;

            return new ModellingSettings(solver, model.Item3, model.Item4);
        }
    }
}