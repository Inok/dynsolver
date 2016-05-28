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
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using ReactiveUI;

namespace DynamicSolver.ViewModel.DynamicSystem
{
    public class SystemSolverViewModel : ReactiveObject
    {
        private string _error;
        private PlotModel _firstPlotModel;
        private PlotModel _secondPlotModel;

        public SystemInputViewModel InputViewModel { get; }

        public string Error
        {
            get { return _error; }
            set { this.RaiseAndSetIfChanged(ref _error, value); }
        }

        public IReactiveCommand Calculate { get; }

        public PlotModel FirstPlotModel
        {
            get { return _firstPlotModel; }
            set { this.RaiseAndSetIfChanged(ref _firstPlotModel, value); }
        }

        public PlotModel SecondPlotModel
        {
            get { return _secondPlotModel; }
            set { this.RaiseAndSetIfChanged(ref _secondPlotModel, value); }
        }

        public SystemSolverViewModel()
        {
            InputViewModel = new SystemInputViewModel(new ExpressionParser());

            var inputObservable = this.WhenAnyValue(m => m.InputViewModel.TaskInput);
            Calculate = ReactiveCommand.CreateAsyncTask(inputObservable.Select(input => input != null), CalculateAsync);
            inputObservable.InvokeCommand(this, m => m.Calculate);
        }


        private async Task CalculateAsync(object obj, CancellationToken token = default(CancellationToken))
        {
            var input = InputViewModel.TaskInput;
            if (input == null) return;

            Error = null;
            try
            {
                var functionFactory = new CompiledFunctionFactory();

                var solver = new RungeKuttaDynamicSystemSolver(functionFactory);
                var baselineSolver = new DormandPrince8DynamicSystemSolver(functionFactory);

                var actualValueTask = Task.Run(() => ProcessCalculations(input, solver, token), token);
                var deviationTask = Task.Run(() => ProcessDeviation(input, solver, baselineSolver, token), token);

                FirstPlotModel = FillPlotterWithResults(await actualValueTask, input);
                SecondPlotModel = FillPlotterWithResults(await deviationTask, input);
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
        }

        private static PlotModel FillPlotterWithResults(IList<IReadOnlyDictionary<string, double>> result, DynamicSystemSolverInput input)
        {
            var plot = new PlotModel();

            if (result.Count == 0)
            {
                return plot;
            }
            
            foreach (var key in result[0].Keys)
            {
                var line = new LineSeries();
                line.Points.AddRange(result.Select((d, k) => new DataPoint(k * input.Step, d[key])));
                line.Title = key;
                plot.Series.Add(line);
            }

            plot.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, MaximumPadding = 0.05, MinimumPadding = 0.01, Title = "t"});
            plot.Axes.Add(new LinearAxis { Position = AxisPosition.Left, MaximumPadding = 0.05, MinimumPadding = 0.01, Title = "Amplitude" });

            return plot;
        }

        private IList<IReadOnlyDictionary<string, double>> ProcessCalculations([Properties.NotNull] DynamicSystemSolverInput input, IDynamicSystemSolver solver, CancellationToken token)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            var itemsCount = (int) (input.ModellingLimit/input.Step);
            var startValues = input.Variables.ToDictionary(v => v.VariableName, v => v.Value.Value);
            return startValues.Yield().Concat(solver.Solve(input.System, startValues, input.Step).Take(itemsCount)).ToArray();
        }

        private IList<IReadOnlyDictionary<string, double>> ProcessDeviation([Properties.NotNull] DynamicSystemSolverInput input,
            IDynamicSystemSolver solver, IDynamicSystemSolver baselineSolver, CancellationToken token)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));


            var itemsCount = (int) (input.ModellingLimit/input.Step);
            var startValues = input.Variables.ToDictionary(v => v.VariableName, v => v.Value.Value);
            var actual = startValues.Yield().Concat(solver.Solve(input.System, startValues, input.Step));
            var baseline = startValues.Yield().Concat(baselineSolver.Solve(input.System, startValues, input.Step/10).Throttle(9, 9));

            return actual.Zip(baseline, (act, expected) => (IReadOnlyDictionary<string, double>)act.ToDictionary(p => p.Key, p => p.Value - expected[p.Key])).Take(itemsCount).ToArray();
        }
    }
}