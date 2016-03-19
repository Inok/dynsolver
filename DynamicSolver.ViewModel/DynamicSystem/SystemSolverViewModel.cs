using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using DynamicSolver.Abstractions.Tools;
using DynamicSolver.DynamicSystem;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using ReactiveUI;

namespace DynamicSolver.ViewModel.DynamicSystem
{
    public class SystemSolverViewModel : ReactiveObject
    {
        private string _error;
        private Dictionary<string, double>[] _result;

        public SystemInputViewModel InputViewModel { get; }

        public string Error
        {
            get { return _error; }
            set { this.RaiseAndSetIfChanged(ref _error, value); }
        }

        public Dictionary<string, double>[] Result
        {
            get { return _result; }
            set { this.RaiseAndSetIfChanged(ref _result, value); }
        }

        private readonly ObservableAsPropertyHelper<string> _resultDump;
        public string ResultDump => _resultDump.Value;

        public IReactiveCommand Calculate { get; }

        public ChartPlotter Plotter { get; set; }

        public SystemSolverViewModel()
        {
            InputViewModel = new SystemInputViewModel(new ExpressionParser.Parser.ExpressionParser());

            var inputObservable = this.WhenAnyValue(m => m.InputViewModel.TaskInput);
            Calculate = ReactiveCommand.CreateAsyncTask(inputObservable.Select(input => input != null), CalculateAsync);
            inputObservable.InvokeCommand(this, m => m.Calculate);            

            _resultDump = this.WhenAnyValue(m => m.Result).Select(r => r.Dump()).ToProperty(this, m => m.ResultDump);
        }


        private async Task CalculateAsync(object obj, CancellationToken token = default(CancellationToken))
        {
            var input = InputViewModel.TaskInput;
            if (input == null) return;

            Error = null;
            try
            {
                var result = await Task.Run(() => ProcessCalculations(input, token), token);
                Result = result.Item1;

                Plotter.Children.RemoveAllOfType(typeof(LineGraph));
                Plotter.Children.RemoveAllOfType(typeof(ElementMarkerPointsGraph));

                if(!Result.Any())
                {
                    return;
                }

                int i = 0;
                foreach (var key in Result[0].Keys.Where(k => k != result.Item2))
                {
                    var dataSource = new EnumerableDataSource<Dictionary<string, double>>(Result);
                    dataSource.SetXMapping(d => d[result.Item2]);
                    dataSource.SetYMapping(d => d[key]);

                    var graph = new LineGraph(dataSource) {LinePen = new Pen(Brushes.Blue, 2)};
                    
                    Plotter.Children.Add(graph);
                    i++;
                }
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
        }

        private Tuple<Dictionary<string, double>[], string> ProcessCalculations([Annotations.NotNull] DynamicSystemSolverInput input, CancellationToken token)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            var solver = new EulerDynamicSystemSolver(input.System);

            return solver.Solve(input.Variables.ToDictionary(v => v.VariableName, v => v.Value.Value), input.Step, input.ModellingTime);
        }
    }
}