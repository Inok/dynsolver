using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using DynamicSolver.Common.Extensions;
using DynamicSolver.DynamicSystem.Solver;
using DynamicSolver.Expressions.Execution.Compiler;
using DynamicSolver.Expressions.Parser;
using JetBrains.Annotations;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using ReactiveUI;

namespace DynamicSolver.ViewModel.DynamicSystem
{
    public class SystemSolverViewModel : ReactiveObject
    {
        private bool _isBusy;
        private string _error;
        private PlotModel _valuePlotModel;
        private PlotModel _deviationPlotModel;
        private SystemSolverSelectItemViewModel _selectedSolver;

        public SystemInputViewModel InputViewModel { get; }

        public string Error
        {
            get { return _error; }
            set { this.RaiseAndSetIfChanged(ref _error, value); }
        }

        public IReactiveCommand Calculate { get; }

        public IReadOnlyCollection<SystemSolverSelectItemViewModel> SolverSelectItems { get; private set; }

        public SystemSolverSelectItemViewModel SelectedSolver
        {
            get { return _selectedSolver; }
            set { this.RaiseAndSetIfChanged(ref _selectedSolver, value); }
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set { this.RaiseAndSetIfChanged(ref _isBusy, value); }
        }

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

        public SystemSolverViewModel()
        {
            InputViewModel = new SystemInputViewModel(new ExpressionParser());

            var inputObservable = this.WhenAnyValue(m => m.InputViewModel.TaskInput, m => m.SelectedSolver);
            Calculate = ReactiveCommand.CreateAsyncTask(inputObservable.Select(input => input.Item1 != null && input.Item2 != null), CalculateAsync);
            inputObservable.InvokeCommand(this, m => m.Calculate);

            var functionFactory = new CompiledFunctionFactory();
            SolverSelectItems = new List<SystemSolverSelectItemViewModel>()
            {
                new SystemSolverSelectItemViewModel("Euler", new EulerDynamicSystemSolver(functionFactory)),
                new SystemSolverSelectItemViewModel("Euler Extr-3", new ExtrapolationEulerDynamicSystemSolver(functionFactory, 3)),
                new SystemSolverSelectItemViewModel("Euler Extr-4", new ExtrapolationEulerDynamicSystemSolver(functionFactory, 4)),
                new SystemSolverSelectItemViewModel("RK 4", new RungeKutta4DynamicSystemSolver(functionFactory)),
                new SystemSolverSelectItemViewModel("KD", new KDDynamicSystemSolver(functionFactory)),
                new SystemSolverSelectItemViewModel("DOPRI 5", new DormandPrince5DynamicSystemSolver(functionFactory)),
                new SystemSolverSelectItemViewModel("DOPRI 7", new DormandPrince7DynamicSystemSolver(functionFactory)),
                new SystemSolverSelectItemViewModel("DOPRI 8", new DormandPrince8DynamicSystemSolver(functionFactory))
            };

            SelectedSolver = SolverSelectItems.FirstOrDefault();
        }

        private async Task CalculateAsync(object obj, CancellationToken token = default(CancellationToken))
        {
            var input = InputViewModel.TaskInput;
            if (input == null) return;

            Error = null;
            try
            {
                var solver = SelectedSolver.Solver;
                var baselineSolver = new DormandPrince8DynamicSystemSolver(new CompiledFunctionFactory());

                IsBusy = true;

                var plotters = await Task.Run(() => FillPlotters(input, solver, baselineSolver), token);

                ValuePlotModel = plotters.Item1;
                DeviationPlotModel = plotters.Item2;
            }
            catch (Exception ex)
            {
                ValuePlotModel = null;
                DeviationPlotModel = null;
                Error = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private static Tuple<PlotModel, PlotModel> FillPlotters([NotNull] DynamicSystemSolverInput input, [NotNull] IDynamicSystemSolver solver, [NotNull] IDynamicSystemSolver baselineSolver)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (solver == null) throw new ArgumentNullException(nameof(solver));
            if (baselineSolver == null) throw new ArgumentNullException(nameof(baselineSolver));

            var itemsCount = (int)(input.ModellingLimit / input.Step);
            var startValues = input.Variables.ToDictionary(v => v.VariableName, v => v.Value.Value);

            var actual = startValues.Yield().Concat(solver.Solve(input.System, startValues, input.Step));
            var baseline = startValues.Yield().Concat(baselineSolver.Solve(input.System, startValues, input.Step / 10).Throttle(9, 9));

            var solves = actual.Zip(baseline, (act, b) => new { actual = act, baseline = b}).Take(itemsCount);
            
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

            return new Tuple<PlotModel, PlotModel>(actualPlot, deviationPlot);
        }
    }
}