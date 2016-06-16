using System.Collections.Generic;
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

            Router.Navigate.Execute(new SystemSolverViewModel(this));
        }

        private static IKernel InitializeKernel(IEnumerable<INinjectModule> modules)
        {
            IKernel kernel = new StandardKernel();

            kernel.Load(modules);
            

            return kernel;
        }
    }
}