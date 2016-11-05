using System.Collections.Generic;
using DynamicSolver.DynamicSystem;
using DynamicSolver.ViewModel.DynamicSystem;
using Ninject;
using Ninject.Modules;
using ReactiveUI;
using Splat;

namespace DynamicSolver.ViewModel
{
    public class ApplicationBootstraper : ReactiveObject, IScreen
    {
        public RoutingState Router { get; }

        public ApplicationBootstraper(RoutingState router = null, IEnumerable<INinjectModule> modules = null)
        {
            Router = router ?? new RoutingState();

            var kernel = InitializeKernel(modules);

            Locator.CurrentMutable = new NinjectLocator(kernel);

            Router.Navigate.Execute(kernel.Get<SystemSolverViewModel>());
        }

        private IKernel InitializeKernel(IEnumerable<INinjectModule> modules)
        {
            IKernel kernel = new StandardKernel();

            kernel.Load(modules);
            
            kernel.Load(new SolverRegistrationModule(SolverRegistrationModule.FunctionFactoryType.Compiled));

            kernel.Bind<IScreen>().ToConstant(this);
            kernel.Bind<SystemSolverViewModel>().ToSelf();

            return kernel;
        }
    }
}