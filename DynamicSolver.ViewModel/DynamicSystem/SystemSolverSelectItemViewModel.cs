using DynamicSolver.DynamicSystem.Solver;
using ReactiveUI;

namespace DynamicSolver.ViewModel.DynamicSystem
{
    public class SystemSolverSelectItemViewModel : ReactiveObject
    {
        private string _name;
        private IDynamicSystemSolver _solver;
        
        public SystemSolverSelectItemViewModel(string name, IDynamicSystemSolver solver)
        {
            _name = name;
            _solver = solver;
        }

        public string Name
        {
            get { return _name; }
            set { this.RaiseAndSetIfChanged(ref _name, value); }
        }

        public IDynamicSystemSolver Solver
        {
            get { return _solver; }
            set { this.RaiseAndSetIfChanged(ref _solver, value); }
        }
    }
}