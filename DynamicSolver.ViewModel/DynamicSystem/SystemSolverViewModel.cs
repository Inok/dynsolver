using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using DynamicSolver.Abstractions.Tools;
using DynamicSolver.DynamicSystem.Solver;
using DynamicSolver.Expressions.Execution.Compiler;
using DynamicSolver.Expressions.Parser;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using ReactiveUI;

namespace DynamicSolver.ViewModel.DynamicSystem
{
    public class SystemSolverViewModel : ReactiveObject
    {
        private string _error;
        
        public SystemInputViewModel InputViewModel { get; }

        public string Error
        {
            get { return _error; }
            set { this.RaiseAndSetIfChanged(ref _error, value); }
        }

        public IReactiveCommand Calculate { get; }

        public ChartPlotter FirstPlotter { get; set; }

        public ChartPlotter SecondPlotter { get; set; }

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
                var eulerTask = Task.Run(() => ProcessCalculations(input, new EulerDynamicSystemSolver(functionFactory), token), token);
                var kdTask = Task.Run(() => ProcessCalculations(input, new RungeKuttaDynamicSystemSolver(functionFactory), token), token);

                FillPlotterWithResults(FirstPlotter, await eulerTask, input);
                FillPlotterWithResults(SecondPlotter, await kdTask, input);
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
        }

        private static void FillPlotterWithResults(Plotter2D plotter, IList<IDictionary<string, double>> result, DynamicSystemSolverInput input)
        {
            plotter.Children.RemoveAllOfType(typeof(LineGraph));
            plotter.Children.RemoveAllOfType(typeof(ElementMarkerPointsGraph));

            if (result.Count == 0)
            {
                return;
            }
            
            int i = 0;
            foreach (var key in result[0].Keys)
            {
                i++;
                var dataSource =
                    new EnumerableDataSource<Tuple<IDictionary<string, double>, double>>(
                        result.Select((d, k) => new Tuple<IDictionary<string, double>, double>(d, k*input.Step)));
                dataSource.SetXMapping(d => d.Item2);
                dataSource.SetYMapping(d => d.Item1[key]);

                var graph = new LineGraph(dataSource)
                {
                    LinePen = new Pen(new SolidColorBrush(new HsbColor((i*50.0)%360.0, 1.0, 1.0).ToArgbColor()), 2)
                };
                graph.AddToPlotter(plotter);
            }
            plotter.FitToView();
        }

        private IList<IDictionary<string, double>> ProcessCalculations([Properties.NotNull] DynamicSystemSolverInput input, IDynamicSystemSolver solver, CancellationToken token)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));


            var itemsCount = (int) (input.ModellingLimit/input.Step);
            int throttlingSkipCount = itemsCount > 1000 ? itemsCount / 1000 : 0;
            return solver.Solve(input.System, input.Variables.ToDictionary(v => v.VariableName, v => v.Value.Value), input.Step)
                .Take(itemsCount)
                .Throttle(throttlingSkipCount)
                .ToArray();
        }
    }
}