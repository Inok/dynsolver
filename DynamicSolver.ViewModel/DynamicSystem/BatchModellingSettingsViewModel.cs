using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DynamicSolver.CoreMath.Execution;
using DynamicSolver.CoreMath.Expression;
using DynamicSolver.DynamicSystem;
using DynamicSolver.DynamicSystem.Solvers;
using DynamicSolver.DynamicSystem.Solvers.Explicit;
using DynamicSolver.DynamicSystem.Solvers.Extrapolation;
using DynamicSolver.ViewModel.Common.Busy;
using DynamicSolver.ViewModel.Common.Select;
using Inok.Tools.Linq;
using JetBrains.Annotations;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Reporting;
using OxyPlot.Series;
using ReactiveUI;

namespace DynamicSolver.ViewModel.DynamicSystem
{
    public class BatchModellingSettingsViewModel : ReactiveObject
    {
        private readonly IExecutableFunctionFactory _functionFactory;
        public DynamicSystemTaskViewModel TaskViewModel { get; }
        private double _step = 0.01;
        private double _time = 10;

        [NotNull]
        public SelectViewModel<IDynamicSystemSolver> SolverSelect { get; }

        [NotNull]
        public SelectViewModel<int?> ExtrapolationStagesSelect { get; }

        private readonly ReactiveList<SolverViewModel> _solvers;
        [NotNull]
        public IReactiveList<SolverViewModel> Solvers => _solvers;

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
        
        [NotNull]
        public BusyIndicatorViewModel BusyViewModel { get; } = new BusyIndicatorViewModel();
        
        public ReactiveCommand AddSolver { get; }
        
        public ReactiveCommand BuildReport { get; }

        public BatchModellingSettingsViewModel([NotNull] IEnumerable<IDynamicSystemSolver> solvers, [NotNull] DynamicSystemTaskViewModel taskViewModel, IExecutableFunctionFactory functionFactory)
        {
            if (solvers == null) throw new ArgumentNullException(nameof(solvers));
            _functionFactory = functionFactory;

            TaskViewModel = taskViewModel ?? throw new ArgumentNullException(nameof(taskViewModel));

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

            _solvers = new ReactiveList<SolverViewModel>();
            
            AddSolver = ReactiveCommand.CreateFromTask(AddSelectedSolver);

            var canBuildReport = this.WhenAnyValue(
                    m => m.Step,
                    m => m.Time,
                    m => m.Solvers.Count,
                    m => m.TaskViewModel.EquationSystem)
                .Select(s => s.Item1 > 0 && s.Item2 > 0 && s.Item3 > 0 && s.Item4?.Equations.Count > 0);
            BuildReport = ReactiveCommand.CreateFromTask(() => Task.Run(ExecuteBuildReport), canBuildReport);
        }

        private Task AddSelectedSolver()
        {
            var selectedExtrapolation = ExtrapolationStagesSelect.SelectedItem.Value;
            var solver = selectedExtrapolation != null
                ? new ExtrapolationSolver(SolverSelect.SelectedItem.Value, selectedExtrapolation.Value)
                : SolverSelect.SelectedItem.Value;

            var solverViewModel = new SolverViewModel(solver);
            solverViewModel.OnRemove.Subscribe(s => _solvers.Remove(solverViewModel));
            _solvers.Add(solverViewModel);

            return Task.CompletedTask;
        }

        private async Task ExecuteBuildReport()
        {
            using (BusyViewModel.CreateScope())
            {
                var itemsCount = (int)(Time / Step);

                var equationSystem = TaskViewModel.EquationSystem;
                
                var definition = new ExplicitOrdinaryDifferentialEquationSystem(equationSystem.Equations, _functionFactory);
                var initial = equationSystem.InitialState;

                var solves = new Task<(IDynamicSystemSolver solver, DynamicSystemState[] result, TimeSpan elapsed)>[Solvers.Count];
                var i = 0;
                foreach (var solver in Solvers)
                {
                    solves[i++] = GetSolverTask(solver.Solver, definition, initial, Step, itemsCount);
                }

                var baseline = await GetBaselineTask(definition, initial, Step, itemsCount);
               
                await Task.WhenAll(solves);

                var report = new Report();

                var names = equationSystem.Equations.Select(e => e.LeadingDerivative.Variable.Name).ToList();

                /* System */
                report.AddHeader(1, "System");
                
                var formatter = new ExpressionFormatter();
                foreach (var equation in TaskViewModel.EquationSystem.Equations)
                {
                    var name = equation.LeadingDerivative.Variable.Name;
                    var order = new string('\'', equation.LeadingDerivative.Order);
                    report.AddParagraph($"{name}{order} = {formatter.Format(equation.Function)}");
                }
                
                var variables = new List<string>();
                foreach (var equation in TaskViewModel.EquationSystem.Equations)
                {
                    var name = equation.LeadingDerivative.Variable.Name;
                    var value = TaskViewModel.EquationSystem.InitialState.DependentVariables[name];
                    variables.Add($"{name}[0] = {value}");
                }
                report.AddParagraph(string.Join(", ", variables));

                /* Baseline */
                report.AddHeader(1, "Baseline");
                report.AddParagraph($"Elapsed time: {baseline.elapsed:c}");

                var baselineLines = names.Select(n => new { name = n, line = new LineSeries { Title = n } }).ToArray();
                for (var point = 0; point < itemsCount; point++)
                {
                    var time = baseline.result[point].IndependentVariable;
                    foreach (var line in baselineLines)
                    {
                        line.line.Points.Add(new DataPoint(time, baseline.result[point].DependentVariables[line.name]));
                    }
                }

                var baselinePlot = new PlotModel();
                baselinePlot.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, MaximumPadding = 0.05, MinimumPadding = 0.01, Title = "t" });
                baselinePlot.Axes.Add(new LinearAxis { Position = AxisPosition.Left, MaximumPadding = 0.05, MinimumPadding = 0.01, Title = "Value" });

                foreach (var lineSeries in baselineLines)
                {
                    baselinePlot.Series.Add(lineSeries.line);
                }

                report.AddPlot(baselinePlot, "", 1500, 500);
                
                
                /* Results */
                foreach (var solve in solves.Select(s => s.Result))
                {
                    report.AddHeader(1, solve.solver.Description.Name);
                    report.AddParagraph($"Elapsed time: {solve.elapsed:c}");

                    var lines = names.Select(n => new { name = n, line = new LineSeries { Title = n } }).ToArray();
                    for (var point = 0; point < itemsCount; point++)
                    {
                        var time = solve.result[point].IndependentVariable;
                        foreach (var line in lines)
                        {
                            var error = baseline.result[point].DependentVariables[line.name] - solve.result[point].DependentVariables[line.name];
                            line.line.Points.Add(new DataPoint(time, error));
                        }
                    }

                    var plot = new PlotModel();
                    plot.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, MaximumPadding = 0.05, MinimumPadding = 0.01, Title = "t" });
                    plot.Axes.Add(new LinearAxis { Position = AxisPosition.Left, MaximumPadding = 0.05, MinimumPadding = 0.01, Title = "Error" });

                    foreach (var lineSeries in lines)
                    {
                        plot.Series.Add(lineSeries.line);
                    }

                    report.AddPlot(plot, "", 1500, 500);
                }

                var path = Path.GetTempFileName() + ".html";
                using (var fileStream = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite))
                using (var reportWriter = new HtmlReportWriter(fileStream))
                {
                    reportWriter.WriteReport(report, new ReportStyle()
                    {
                        Margins = new OxyThickness(10, 5, 10, 5)
                    });
                }

                Process.Start(path);
            }
        }

        private static async Task<(IDynamicSystemSolver solver, DynamicSystemState[] result, TimeSpan elapsed)> GetSolverTask(IDynamicSystemSolver solver, IExplicitOrdinaryDifferentialEquationSystem system, DynamicSystemState initial, double step, int itemsCount)
        {
            return await Task.Run(() =>
            {
                var parameters = new ModellingTaskParameters(step);

                var result = new DynamicSystemState[itemsCount];
                result[0] = initial;

                var sw = new Stopwatch();
                sw.Start();

                var solve = solver.Solve(system, initial, parameters);

                var i = 1;
                foreach (var state in solve.Take(itemsCount - 1))
                {
                    result[i++] = state;
                }

                sw.Stop();
                return (solver, result, sw.Elapsed);
            });
        }
        
        private static async Task<(DynamicSystemState[] result, TimeSpan elapsed)> GetBaselineTask(IExplicitOrdinaryDifferentialEquationSystem system, DynamicSystemState initial, double step, int itemsCount)
        {
            return await Task.Run(() =>
            {
                var solver = new DormandPrince8DynamicSystemSolver();
                var parameters = new ModellingTaskParameters(step / 10);

                var result = new DynamicSystemState[itemsCount];
                result[0] = initial;

                var sw = new Stopwatch();
                sw.Start();

                var baselineSolve = solver.Solve(system, initial, parameters);

                var i = 1;
                foreach (var state in baselineSolve.Skipping(9, 9).Take(itemsCount - 1))
                {
                    result[i++] = state;
                }

                sw.Stop();
                return (result, sw.Elapsed);
            });
        }
    }
}