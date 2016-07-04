using DynamicSolver.GUI.DynamicSystem;
using DynamicSolver.ViewModel.DynamicSystem;
using Ninject.Modules;
using ReactiveUI;

namespace DynamicSolver.GUI
{
    public class ViewsRegistrationModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IViewFor<SystemSolverViewModel>>().To<SystemSolverView>();
        }
    }
}