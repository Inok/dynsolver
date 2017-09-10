using DynamicSolver.App.DynamicSystem;
using DynamicSolver.App.ViewModel.DynamicSystem;
using Ninject.Modules;
using ReactiveUI;

namespace DynamicSolver.App
{
    public class ViewsRegistrationModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IViewFor<SystemSolverViewModel>>().To<SystemSolverView>();
        }
    }
}