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
using DynamicSolver.Minimizer.MinimizationInterval;
using DynamicSolver.Minimizer.MultiDimensionalSearch;
using DynamicSolver.ViewModel.Annotations;
using DynamicSolver.ViewModel.Properties;
using ReactiveUI;

namespace DynamicSolver.ViewModel.Minimization
{
    public class ExpressionMinimizerViewModel : ReactiveObject
    {
        public ExpressionInputViewModel InputViewModel { get; }

        public MinimizationResultViewModel ResultViewModel { get; }

        public bool ShowCalculateButton => !Settings.Default.Minimization_Features_AutoCalculation;

        public IReactiveCommand Calculate { get; }

        public ExpressionMinimizerViewModel()
        {
            InputViewModel = new ExpressionInputViewModel(new ExpressionParser.Parser.ExpressionParser());
            ResultViewModel = new MinimizationResultViewModel();

            var inputObservable = this.WhenAnyValue(m => m.InputViewModel.MinimizationInput);
            Calculate = ReactiveCommand.CreateAsyncTask(inputObservable.Select(input => input != null), CalculateAsync);

            if(Settings.Default.Minimization_Features_AutoCalculation)
            {
                inputObservable.InvokeCommand(this, m => m.Calculate);
            }
        }

        private async Task CalculateAsync(object obj, CancellationToken token = default(CancellationToken))
        {
            ResultViewModel.Clear();

            var input = InputViewModel.MinimizationInput;
            if (input == null) return;

            ResultViewModel.StartProgress();
            try
            {
                var result = await Task.Run(() => ProcessCalculations(input, token), token);
                ResultViewModel.ApplyResult(result);
            }
            catch (InvalidOperationException ex)
            {
                ResultViewModel.ApplyFail(ex.Message);
            }
        }

        private ICollection<VariableValue> ProcessCalculations([NotNull] MinimizationTaskInput input, CancellationToken token)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            IMultiDimensionalSearchStrategy strategy = new HookeJeevesMethod(HookeJeevesSearchSettings.Default);
            
            var function = new InterpretedFunction(input.Statement.Expression);
            var args = function.OrderedArguments;

            var result = strategy.Search(function, new Point(args.Select(s => input.Variables.First(v => v.VariableName == s).Value.Value).ToArray()));

            return result.Zip(args, (d, s) => new VariableValue(s) { Value = d }).ToList();
        }
    }
}