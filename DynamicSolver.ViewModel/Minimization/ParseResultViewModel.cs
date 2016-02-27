using DynamicSolver.Abstractions.Expression;

namespace DynamicSolver.ViewModel.Minimization
{
    public class ParseResultViewModel
    {
        public IStatement Statement { get; }
        public bool Valid { get; }
        public string ErrorMessage { get; }

        public ParseResultViewModel(IStatement statement, bool valid, string errorMessage)
        {
            Statement = statement;
            Valid = valid;
            ErrorMessage = errorMessage;
        }
    }
}