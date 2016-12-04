using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using DynamicSolver.DynamicSystem.Solver;
using DynamicSolver.Expressions.Execution.Compiler;
using DynamicSolver.Expressions.Parser;
using DynamicSolver.ViewModel.Common.Busy;
using DynamicSolver.ViewModel.Common.ErrorList;
using DynamicSolver.ViewModel.Common.Select;
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
        private PlotModel _valuePlotModel;
        private PlotModel _deviationPlotModel;
        private TimeSpan? _elapsedTime;

        public DynamicSystemTaskViewModel TaskViewModel { get; }

        [NotNull]
        public ErrorListViewModel ErrorListViewModel { get; } = new ErrorListViewModel();

        [NotNull]
        public BusyIndicatorViewModel BusyViewModel { get; } = new BusyIndicatorViewModel();


        public IReactiveCommand Calculate { get; }

        public SelectViewModel<IDynamicSystemSolver> SolverSelect { get; }

        public PlotModel ValuePlotModel
        {
            get { return _valuePlotModel; }
            set { this.RaiseAndSetIfChanged(ref _valuePlotModel, value); }
        }

        public PlotModel DeviationPlotModel
        {
            get { return _deviationPlotModel; }
            set { this.RaiseAndSetIfChanged(ref _deviationPlotModel, value); }
        }

        public TimeSpan? ElapsedTime
        {
            get { return _elapsedTime; }
            set { this.RaiseAndSetIfChanged(ref _elapsedTime, value); }
        }

        public SystemSolverViewModel([NotNull] IScreen hostScreen, [NotNull] IEnumerable<IDynamicSystemSolver> solvers)
        {
            if (hostScreen == null) throw new ArgumentNullException(nameof(hostScreen));
            if (solvers == null) throw new ArgumentNullException(nameof(solvers));

            HostScreen = hostScreen;

            var solverSelect = new SelectViewModel<IDynamicSystemSolver>(false);
            foreach (var solver in solvers)
            {
                solverSelect.AddItem(solver.ToString(), solver);
            }

            solverSelect.SelectedItem = solverSelect.Items.FirstOrDefault();

            SolverSelect = solverSelect;

            TaskViewModel = new DynamicSystemTaskViewModel(new ExpressionParser());

            var inputObservable = this.WhenAnyValue(m => m.TaskViewModel.TaskInput, m => m.SolverSelect.SelectedItem);
            Calculate = ReactiveCommand.CreateAsyncTask(inputObservable.Select(input => input.Item1 != null && input.Item2 != null), CalculateAsync);
            inputObservable.InvokeCommand(this, m => m.Calculate);
        }

        private async Task CalculateAsync(object obj, CancellationToken token = default(CancellationToken))
        {
            var input = TaskViewModel.TaskInput;
            if (input == null) return;

            ErrorListViewModel.Errors.Clear();

            using (BusyViewModel.CreateScope())
            {
                try
                {
                    var solver = SolverSelect.SelectedItem.Value;
                    var baselineSolver = new DormandPrince8DynamicSystemSolver(new CompiledFunctionFactory());

                    var plotters = await Task.Run(() => FillPlotters(input, solver, baselineSolver), token);

                    ValuePlotModel = plotters.Item1;
                    DeviationPlotModel = plotters.Item2;
                    ElapsedTime = plotters.Item3;
                }
                catch (Exception ex)
                {
                    ValuePlotModel = null;
                    DeviationPlotModel = null;
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

        private static Tuple<PlotModel, PlotModel, TimeSpan> FillPlotters([NotNull] DynamicSystemSolverInput input, [NotNull] IDynamicSystemSolver solver, [NotNull] IDynamicSystemSolver baselineSolver)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (solver == null) throw new ArgumentNullException(nameof(solver));
            if (baselineSolver == null) throw new ArgumentNullException(nameof(baselineSolver));

            var itemsCount = (int)(input.ModellingLimit / input.Step);
            var startValues = input.Variables;

            var sw = new Stopwatch();
            sw.Start();
            var actual = startValues.Yield().Concat(solver.Solve(input.System, startValues, input.Step)).Take(itemsCount).ToList();
            sw.Stop();

            var baseline = startValues.Yield().Concat(baselineSolver.Solve(input.System, startValues, input.Step / 10).Skipping(9, 9)).Take(itemsCount);

            var solves = actual.Zip(baseline, (act, b) => new { actual = act, baseline = b});
            
            var names = input.System.Equations.Select(e => e.LeadingDerivative.Variable.Name).ToList();
            var lines = names.Select(n => new {name = n, value = new LineSeries {Title = n}, deviation = new LineSeries {Title = n}}).ToList();
            
            int k = 0;
            foreach (var point in solves)
            {
                var time = ++k * input.Step;
                foreach (var line in lines)
                {
                    var val = point.actual[line.name];
                    line.value.Points.Add(new DataPoint(time, val));
                    line.deviation.Points.Add(new DataPoint(time, point.baseline[line.name] - val));
                }
            }

            var actualPlot = new PlotModel();
            actualPlot.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, MaximumPadding = 0.05, MinimumPadding = 0.01, Title = "t" });
            actualPlot.Axes.Add(new LinearAxis { Position = AxisPosition.Left, MaximumPadding = 0.05, MinimumPadding = 0.01, Title = "Value" });

            var deviationPlot = new PlotModel();
            deviationPlot.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, MaximumPadding = 0.05, MinimumPadding = 0.01, Title = "t" });
            deviationPlot.Axes.Add(new LinearAxis { Position = AxisPosition.Left, MaximumPadding = 0.05, MinimumPadding = 0.01, Title = "Deviation" });

            foreach (var lineSeries in lines)
            {
                actualPlot.Series.Add(lineSeries.value);
                deviationPlot.Series.Add(lineSeries.deviation);
            }

            return new Tuple<PlotModel, PlotModel, TimeSpan>(actualPlot, deviationPlot, sw.Elapsed);
        }

        public string UrlPathSegment => nameof(SystemSolverViewModel);

        public IScreen HostScreen { get; }
    }
}