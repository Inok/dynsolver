using ReactiveUI;

namespace DynamicSolver.ViewModel.Minimization
{
    public class ExpressionMinimizerViewModel : ReactiveObject
    {
        public ExpressionInputViewModel InputViewModel { get; }

        public ExpressionMinimizerViewModel()
        {
            InputViewModel = new ExpressionInputViewModel(new ExpressionParser.Parser.ExpressionParser());
        }
    }
}