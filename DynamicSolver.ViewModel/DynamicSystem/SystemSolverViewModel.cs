using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using DynamicSolver.CoreMath.Execution;
using DynamicSolver.CoreMath.Parser;
using DynamicSolver.DynamicSystem;
using DynamicSolver.DynamicSystem.Solvers;
using DynamicSolver.DynamicSystem.Solvers.Explicit;
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
        private bool _showAbsoluteError = false;

        public DynamicSystemTaskViewModel TaskViewModel { get; }
        
        public ModellingSettingsViewModel ModellingSettingsViewModel { get; }

        [NotNull]
        public ErrorListViewModel ErrorListViewModel { get; } = new ErrorListViewModel();

        [NotNull]
        public BusyIndicatorViewModel BusyViewModel { get; } = new BusyIndicatorViewModel();


        public bool ShowAbsoluteError
        {
            get { return _showAbsoluteError; }
            set { this.RaiseAndSetIfChanged(ref _showAbsoluteError, value); }
        }

        public IReactiveCommand Calculate { get; }

        public PlotModel ValuePlotModel
        {
            get { return _valuePlotModel; }
            set { this.RaiseAndSetIfChanged(ref _valuePlotModel, value); }
        }

        public PlotModel ErrorPlotModel
        {
            get { return _errorPlotModel; }
            set { this.RaiseAndSetIfChanged(ref _errorPlotModel, value); }
        }

        public TimeSpan? ElapsedTime
        {
            get { return _elapsedTime; }
            set { this.RaiseAndSetIfChanged(ref _elapsedTime, value); }
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

            TaskViewModel = new DynamicSystemTaskViewModel(new ExpressionParser());
            ModellingSettingsViewModel = new ModellingSettingsViewModel(solvers);

            var inputObservable = this.WhenAnyValue(
                m => m.TaskViewModel.EquationSystem,
                m => m.ModellingSettingsViewModel.ModellingSettings,
                m => m.ShowAbsoluteError
            );
            Calculate = ReactiveCommand.CreateAsyncTask(inputObservable.Select(input => input.Item1 != null && input.Item2 != null), CalculateAsync);
            inputObservable.InvokeCommand(this, m => m.Calculate);
        }

        private async Task CalculateAsync(object obj, CancellationToken token = default(CancellationToken))
        {
            var input = TaskViewModel.EquationSystem;
            if (input == null) return;

            ErrorListViewModel.Errors.Clear();

            using (BusyViewModel.CreateScope())
            {
                try
                {
                    var plotters = await Task.Run(() => FillPlotters(input, ModellingSettingsViewModel.ModellingSettings, ShowAbsoluteError), token);

                    ValuePlotModel = plotters.Item1;
                    ErrorPlotModel = plotters.Item2;
                    ElapsedTime = plotters.Item3;
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

        private Tuple<PlotModel, PlotModel, TimeSpan> FillPlotters(
            [NotNull] ExplicitOrdinaryDifferentialEquationSystemDefinition input,
            [NotNull] ModellingSettings modellingSettings,
            bool showAbsoluteError)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (modellingSettings == null) throw new ArgumentNullException(nameof(modellingSettings));

            var solver = modellingSettings.Solver;
            var baselineSolver = new DormandPrince8DynamicSystemSolver();
            
            var itemsCount = (int)(modellingSettings.Time / modellingSettings.Step);
            var startValues = input.InitialState;

            var definition = new ExplicitOrdinaryDifferentialEquationSystem(input.Equations, input.InitialState, _functionFactory);

            var sw = new Stopwatch();
            sw.Start();
            var actual = startValues.Yield().Concat(solver.Solve(definition, new ModellingTaskParameters(modellingSettings.Step))).Take(itemsCount).ToList();
            sw.Stop();

            var baseline = startValues.Yield().Concat(baselineSolver.Solve(definition, new ModellingTaskParameters(modellingSettings.Step / 10)).Skipping(9, 9)).Take(itemsCount);

            var solves = actual.Zip(baseline, (act, b) => new { actual = act, baseline = b });

            var names = input.Equations.Select(e => e.LeadingDerivative.Variable.Name).ToList();
            var lines = names.Select(n => new { name = n, value = new LineSeries { Title = n }, error = new LineSeries { Title = n } }).ToList();

            foreach (var point in solves)
            {
                var time = point.actual.IndependentVariable;
                foreach (var line in lines)
                {
                    var val = point.actual.DependentVariables[line.name];
                    line.value.Points.Add(new DataPoint(time, val));

                    var error = point.baseline.DependentVariables[line.name] - val;
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

            return new Tuple<PlotModel, PlotModel, TimeSpan>(actualPlot, errorPlot, sw.Elapsed);
        }

        public string UrlPathSegment => nameof(SystemSolverViewModel);

        public IScreen HostScreen { get; }
    }
}