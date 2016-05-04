using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using DynamicSolver.Expressions.Execution.Interpreter;
using DynamicSolver.Expressions.Parser;
using DynamicSolver.LinearAlgebra;
using DynamicSolver.LinearAlgebra.Derivative;
using DynamicSolver.Minimizer;
using DynamicSolver.Minimizer.DirectedSearch;
using DynamicSolver.Minimizer.MinimizationInterval;
using DynamicSolver.Minimizer.MultiDimensionalSearch;
using DynamicSolver.ViewModel.Properties;
using ReactiveUI;

namespace DynamicSolver.ViewModel.Minimization
{
    public class ExpressionMinimizerViewModel : ReactiveObject
    {
        private static readonly NumericalDerivativeCalculator NumericalDerivativeCalculator = new NumericalDerivativeCalculator(DerivativeCalculationSettings.Default);

        private static readonly GoldenRatioMethod DirectedSearchStrategy = new GoldenRatioMethod(
            new DerivativeSwannMethod(DerivativeSwannMethodSettings.Default, NumericalDerivativeCalculator),
            DirectedSearchSettings.Default);

        private static Dictionary<IMultiDimensionalSearchStrategy, Func<MinimizationTaskInput, bool>> _methods
            = new Dictionary<IMultiDimensionalSearchStrategy, Func<MinimizationTaskInput, bool>>()
            {
                [new HookeJeevesMethod(HookeJeevesSearchSettings.Default)] = (i) => true,
                [new NelderMeadMethod(NelderMeadSearchSettings.Default)] = (i) => i.Statement.Analyzer.Variables.Count > 1,
                [new RosenbrockMethod(DirectedSearchStrategy, MultiDimensionalSearchSettings.Default)] = (i) => true,
                [new PartanMethod(DirectedSearchStrategy, NumericalDerivativeCalculator, MultiDimensionalSearchSettings.Default)] = (i) => true
            };


        private ICollection<MinimizationResultViewModel> _resultViewModel;

        public ExpressionInputViewModel InputViewModel { get; }

        public ICollection<MinimizationResultViewModel> ResultViewModel
        {
            get { return _resultViewModel; }
            set { this.RaiseAndSetIfChanged(ref _resultViewModel, value); }
        }

        public bool ShowCalculateButton => !Settings.Default.Minimization_Features_AutoCalculation;

        public IReactiveCommand Calculate { get; }

        public ExpressionMinimizerViewModel()
        {
            InputViewModel = new ExpressionInputViewModel(new ExpressionParser());
            _resultViewModel = new List<MinimizationResultViewModel>();

            var inputObservable = this.WhenAnyValue(m => m.InputViewModel.MinimizationInput);
            Calculate = ReactiveCommand.CreateAsyncTask(inputObservable.Select(input => input != null), CalculateAsync);

            if(Settings.Default.Minimization_Features_AutoCalculation)
            {
                inputObservable.InvokeCommand(this, m => m.Calculate);
            }
        }

        private async Task CalculateAsync(object obj, CancellationToken token = default(CancellationToken))
        {
            var input = InputViewModel.MinimizationInput;
            if (input == null) return;

            var tasks = new List<Task>();
            var result = new List<MinimizationResultViewModel>();
            foreach (var pair in _methods.Where(m => m.Value(input)))
            {
                var methodResultViewModel = new MinimizationResultViewModel(pair.Key.GetType().Name);

                methodResultViewModel.StartProgress();
                result.Add(methodResultViewModel);

                var task = Task.Run(() =>
                {
                    try
                    {
                        var r = ProcessCalculations(pair.Key, input, token);
                        methodResultViewModel.ApplyResult(r);
                    }
                    catch (InvalidOperationException ex)
                    {
                        methodResultViewModel.ApplyFail(ex.Message);
                    }
                }, token);

                tasks.Add(task);
            }

            ResultViewModel = result;

            await Task.WhenAll(tasks);
        }

        private ICollection<VariableValue> ProcessCalculations([JetBrains.Annotations.NotNull] IMultiDimensionalSearchStrategy strategy, [NotNull] MinimizationTaskInput input, CancellationToken token)
        {
            if (strategy == null) throw new ArgumentNullException(nameof(strategy));
            if (input == null) throw new ArgumentNullException(nameof(input));

            var function = new InterpretedFunction(input.Statement);
            var args = function.OrderedArguments;

            var result = strategy.Search(function, new Point(args.Select(s => input.Variables.First(v => v.VariableName == s).Value.Value).ToArray()), token);

            return result.Zip(args, (d, s) => new VariableValue(s) { Value = d }).ToList();
        }
    }
}