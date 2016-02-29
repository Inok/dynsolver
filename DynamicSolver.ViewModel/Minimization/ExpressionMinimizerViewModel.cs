using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using DynamicSolver.ViewModel.Annotations;
using ReactiveUI;

namespace DynamicSolver.ViewModel.Minimization
{
    public class ExpressionMinimizerViewModel : ReactiveObject
    {
        public ExpressionInputViewModel InputViewModel { get; }

        public MinimizationResultViewModel ResultViewModel { get; }

        public IReactiveCommand Calculate { get; }

        public ExpressionMinimizerViewModel()
        {
            InputViewModel = new ExpressionInputViewModel(new ExpressionParser.Parser.ExpressionParser());
            ResultViewModel = new MinimizationResultViewModel();

            var inputObservable = this.WhenAnyValue(m => m.InputViewModel.MinimizationInput);
            Calculate = ReactiveCommand.CreateAsyncTask(inputObservable.Select(input => input != null), CalculateAsync);
            inputObservable.InvokeCommand(this, m => m.Calculate);
        }

        private async Task CalculateAsync(object obj, CancellationToken token = default(CancellationToken))
        {
            await Task.Run(() => ProcessCalculations(InputViewModel.MinimizationInput, token), token);
        }

        private double ProcessCalculations([NotNull] MinimizationTaskInput input, CancellationToken token)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            // emulate calculations
            for (int i = 0; i < 10; i++)
            {
                if(token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                }
                Thread.Sleep(500);
            }

            return 0;
        }
    }
}