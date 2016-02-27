using ReactiveUI;

namespace DynamicSolver.ViewModel.Minimization
{
    public class VariableViewModel : ReactiveObject
    {
        private double? _value;

        public string VariableName { get; }

        public double? Value
        {
            get { return _value; }
            set { this.RaiseAndSetIfChanged(ref _value, value); }
        }

        public VariableViewModel(string variableName)
        {
            VariableName = variableName;
        }
    }
}