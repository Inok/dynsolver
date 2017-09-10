using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using DynamicSolver.Core.Execution;
using DynamicSolver.Core.Syntax.Parser;
using DynamicSolver.Modelling;
using DynamicSolver.Modelling.Solvers;
using DynamicSolver.Modelling.Solvers.Explicit;
using DynamicSolver.ViewModel.Common.Busy;
using DynamicSolver.ViewModel.Common.ErrorList;
using Inok.Tools.Linq;
using JetBrains.Annotations;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using ReactiveUI;

namespace DynamicSolver.ViewModel.DynamicSystem
{
    public class SystemSolverViewModel : ReactiveObject, IRoutableViewModel
    {
        private readonly IExecutableFunctionFactory _functionFactory;
        private PlotModel _valuePlotModel;
        private PlotModel _errorPlotModel;
        private TimeSpan? _elapsedTime;
        private bool _showAbsoluteError;

        public DynamicSystemTaskViewModel TaskViewModel { get; }
        
        public ModellingSettingsViewModel ModellingSettingsViewModel { get; }
        
        public BatchModellingSettingsViewModel BatchModellingSettingsViewModel { get; }

        [NotNull]
        public ErrorListViewModel ErrorListViewModel { get; } = new ErrorListViewModel();

        [NotNull]
        public BusyIndicatorViewModel BusyViewModel { get; } = new BusyIndicatorViewModel();

        public bool ShowAbsoluteError
        {
            get => _showAbsoluteError;
            set => this.RaiseAndSetIfChanged(ref _showAbsoluteError, value);
        }

        public ReactiveCommand Calculate { get; }

        public PlotModel ValuePlotModel
        {
            get => _valuePlotModel;
            set => this.RaiseAndSetIfChanged(ref _valuePlotModel, value);
        }

        public PlotModel ErrorPlotModel
        {
            get => _errorPlotModel;
            set => this.RaiseAndSetIfChanged(ref _errorPlotModel, value);
        }

        public TimeSpan? ElapsedTime
        {
            get => _elapsedTime;
            set => this.RaiseAndSetIfChanged(ref _elapsedTime, value);
        }

        public SystemSolverViewModel(
            [NotNull] IScreen hostScreen,
            [NotNull] IExecutableFunctionFactory functionFactory,
            [NotNull] IEnumerable<IDynamicSystemSolver> solvers)
        {
            if (hostScreen == null) throw new ArgumentNullException(nameof(hostScreen));
            if (functionFactory == null) throw new ArgumentNullException(nameof(functionFactory));
            if (solvers == null) throw new ArgumentNullException(nameof(solvers));

            HostScreen = hostScreen;
            _functionFactory = functionFactory;

            TaskViewModel = new DynamicSystemTaskViewModel(new SyntaxParser());
            ModellingSettingsViewModel = new ModellingSettingsViewModel(solvers);
            BatchModellingSettingsViewModel = new BatchModellingSettingsViewModel(solvers, TaskViewModel, _functionFactory);

            var inputObservable = this.WhenAnyValue(
                m => m.TaskViewModel.EquationSystem,
                m => m.ModellingSettingsViewModel.ModellingSettings,
                m => m.ShowAbsoluteError
            );
            
            Calculate = ReactiveCommand.CreateFromTask(CalculateAsync, inputObservable.Select(input => input.Item1 != null && input.Item1.Equations.Count > 0 && input.Item2 != null));
            inputObservable.Select(_ => Unit.Default).InvokeCommand(this, m => m.Calculate);
        }

        private async Task CalculateAsync(CancellationToken token = default(CancellationToken))
        {
            var input = TaskViewModel.EquationSystem;
            if (input == null) return;

            ErrorListViewModel.Errors.Clear();

            using (BusyViewModel.CreateScope())
            {
                try
                {
                    var plotters = await Task.Run(() => FillPlotters(input, ModellingSettingsViewModel.ModellingSettings, ShowAbsoluteError), token);

                    ValuePlotModel = plotters.actual;
                    ErrorPlotModel = plotters.error;
                    ElapsedTime = plotters.elapsed;
                }
                catch (Exception ex)
                {
                    ValuePlotModel = null;
                    ErrorPlotModel = null;
                    ElapsedTime = null;
                    ErrorListViewModel.Errors.Add(new ErrorViewModel
                    {
                        Level = ErrorLevel.Error,
                        Source = "solver",
                        Message = ex.Message
                    });
                }
            }
        }

        private async Task<(PlotModel actual, PlotModel error, TimeSpan elapsed)> FillPlotters(
            [NotNull] ExplicitOrdinaryDifferentialEquationSystemDefinition input,
            [NotNull] ModellingSettings modellingSettings,
            bool showAbsoluteError)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (modellingSettings == null) throw new ArgumentNullException(nameof(modellingSettings));

            var itemsCount = (int)(modellingSettings.Time / modellingSettings.Step);
            var startValues = input.InitialState;

            var definition = new ExplicitOrdinaryDifferentialEquationSystem(input.Equations, _functionFactory);

            var actualTask = Task.Run(() =>
            {
                var solver = modellingSettings.Solver;
                var parameters = new ModellingTaskParameters(modellingSettings.Step);
                
                var result = new DynamicSystemState[itemsCount];
                result[0] = startValues;

                var sw = new Stopwatch();
                sw.Start();
                
                var actualSolve = solver.Solve(definition, startValues, parameters);
               
                var i = 1;
                foreach (var state in actualSolve.Take(itemsCount - 1))
                {
                    result[i++] = state;
                }

                sw.Stop();
                return (result: result, elapsed: sw.Elapsed);
            });
            
            var baselineTask = Task.Run(() =>
            {
                var solver = new DormandPrince8DynamicSystemSolver();
                var parameters = new ModellingTaskParameters(modellingSettings.Step / 10);
                
                var result = new DynamicSystemState[itemsCount];
                result[0] = startValues;

                var sw = new Stopwatch();
                sw.Start();

                var baselineSolve = solver.Solve(definition, startValues, parameters);
                
                var i = 1;
                foreach (var state in baselineSolve.Skipping(9, 9).Take(itemsCount - 1))
                {
                    result[i++] = state;
                }

                sw.Stop();
                return (result: result, elapsed: sw.Elapsed);
            });

            var (actual, elapsed) = await actualTask;
            var (baseline, _) = await baselineTask;

            var names = input.Equations.Select(e => e.LeadingDerivative.Variable.Name).ToList();
            var lines = names.Select(n => new { name = n, value = new LineSeries { Title = n }, error = new LineSeries { Title = n } }).ToArray();
            
            for (var i = 0; i < itemsCount; i++)
            {
                var time = actual[i].IndependentVariable;
                foreach (var line in lines)
                {
                    var val = actual[i].DependentVariables[line.name];
                    line.value.Points.Add(new DataPoint(time, val));

                    var error = baseline[i].DependentVariables[line.name] - val;
                    line.error.Points.Add(new DataPoint(time, showAbsoluteError ? Math.Abs(error) : error));
                }
            }

            var actualPlot = new PlotModel();
            actualPlot.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, MaximumPadding = 0.05, MinimumPadding = 0.01, Title = "t" });
            actualPlot.Axes.Add(new LinearAxis { Position = AxisPosition.Left, MaximumPadding = 0.05, MinimumPadding = 0.01, Title = "Value" });

            var errorPlot = new PlotModel();
            errorPlot.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, MaximumPadding = 0.05, MinimumPadding = 0.01, Title = "t" });
            errorPlot.Axes.Add(new LinearAxis { Position = AxisPosition.Left, MaximumPadding = 0.05, MinimumPadding = 0.01, Title = "Error" });

            foreach (var lineSeries in lines)
            {
                actualPlot.Series.Add(lineSeries.value);
                errorPlot.Series.Add(lineSeries.error);
            }

            return (actualPlot, errorPlot, elapsed);
        }

        public string UrlPathSegment => nameof(SystemSolverViewModel);

        public IScreen HostScreen { get; }
    }
}