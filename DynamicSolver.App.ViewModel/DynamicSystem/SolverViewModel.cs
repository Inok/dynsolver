using System;
using System.Reactive;
using System.Reactive.Subjects;
using DynamicSolver.Modelling.Solvers;
using JetBrains.Annotations;
using ReactiveUI;

namespace DynamicSolver.App.ViewModel.DynamicSystem
{
    public class SolverViewModel
    {
        public IDynamicSystemSolver Solver { get; }

        public ReactiveCommand Remove { get; }

        private readonly Subject<Unit> _onRemove = new Subject<Unit>();
        public IObservable<Unit> OnRemove => _onRemove;
        
        public SolverViewModel([NotNull] IDynamicSystemSolver solver)
        {
            Solver = solver ?? throw new ArgumentNullException(nameof(solver));
            Remove = ReactiveCommand.Create(() => { _onRemove.OnNext(Unit.Default); });
        }
    }
}