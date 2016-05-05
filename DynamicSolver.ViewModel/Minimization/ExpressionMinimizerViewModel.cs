using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using DynamicSolver.ExpressionCompiler.Interpreter;
using DynamicSolver.LinearAlgebra;
using DynamicSolver.LinearAlgebra.Derivative;
using DynamicSolver.Minimizer;
using DynamicSolver.Minimizer.DirectedSearch;
using DynamicSolver.Minimizer.MinimizationInterval;
using DynamicSolver.Minimizer.MultiDimensionalSearch;
using DynamicSolver.ViewModel.Annotations;
using DynamicSolver.ViewModel.Properties;
using ReactiveUI;

namespace DynamicSolver.ViewModel.Minimization
{
    public class ExpressionMinimizerViewModel : ReactiveObject
    {
        private class MethodInfo
        {
            [NotNull] public string Name { get; }
            [NotNull] public IMultiDimensionalSearchStrategy Strategy { get; }
            [NotNull] public Predicate<MinimizationTaskInput> AvailablePredicate { get; }

            public MethodInfo([NotNull] IMultiDimensionalSearchStrategy strategy) 
                : this(strategy, i => true)
            {
                
            }

            public MethodInfo([NotNull] IMultiDimensionalSearchStrategy strategy, [NotNull] Predicate<MinimizationTaskInput> availablePredicate)
                : this(strategy.GetType().Name, strategy, availablePredicate)
            {                
            }

            public MethodInfo(
                [NotNull] string name, 
                [NotNull] IMultiDimensionalSearchStrategy strategy) : this(name, strategy, i => true)
            {
            }

            public MethodInfo(
                [NotNull] string name, 
                [NotNull] IMultiDimensionalSearchStrategy strategy,
                [NotNull] Predicate<MinimizationTaskInput> availablePredicate)
            {
                if (name == null) throw new ArgumentNullException(nameof(name));
                if (strategy == null) throw new ArgumentNullException(nameof(strategy));
                if (availablePredicate == null) throw new ArgumentNullException(nameof(availablePredicate));

                Name = name;
                Strategy = strategy;
                AvailablePredicate = availablePredicate;
            }
        }

        private static readonly NumericalDerivativeCalculator NumericalDerivativeCalculator = new NumericalDerivativeCalculator(DerivativeCalculationSettings.Default);

        private static readonly GoldenRatioMethod DirectedSearchStrategy = new GoldenRatioMethod(
            new DerivativeSwannMethod(DerivativeSwannMethodSettings.Default, NumericalDerivativeCalculator),
            DirectedSearchSettings.Default);

        private static readonly IEnumerable<MethodInfo> Methods = new[]
        {
            new MethodInfo(new HookeJeevesMethod(HookeJeevesSearchSettings.Default)),
            new MethodInfo(new NelderMeadMethod(NelderMeadSearchSettings.Default), i => i.Statement.Analyzer.Variables.Count > 1),
            new MethodInfo(new RosenbrockMethod(DirectedSearchStrategy, MultiDimensionalSearchSettings.Default)),
            new MethodInfo(new PartanMethod(DirectedSearchStrategy, NumericalDerivativeCalculator, MultiDimensionalSearchSettings.Default)),
            new MethodInfo("Genetic i100, p50, t3, c0.8, m0.03, i0.03", new GeneticMethod(new GeneticSearchSettings(100, 50, 3, 0.8, 0.03, 0.03))),
            new MethodInfo("Genetic i100, p100, t2, c0.7, m0.1, i0.05", new GeneticMethod(new GeneticSearchSettings(100, 100, 2, 0.7, 0.1, 0.05)))
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
            InputViewModel = new ExpressionInputViewModel(new ExpressionParser.Parser.ExpressionParser());
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
            foreach (var methodInfo in Methods.Where(m => m.AvailablePredicate(input)))
            {
                var methodResultViewModel = new MinimizationResultViewModel(methodInfo.Name);

                methodResultViewModel.StartProgress();
                result.Add(methodResultViewModel);

                var task = Task.Run(() =>
                {
                    try
                    {
                        var r = ProcessCalculations(methodInfo.Strategy, input, token);
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

            var function = new InterpretedFunction(input.Statement.Expression);
            var args = function.OrderedArguments;

            var result = strategy.Search(function, new Point(args.Select(s => input.Variables.First(v => v.VariableName == s).Value.Value).ToArray()), token);

            return result.Zip(args, (d, s) => new VariableValue(s) { Value = d }).ToList();
        }
    }
}