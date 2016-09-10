using DynamicSolver.DynamicSystem.Solver;
using Ninject.Modules;
using Ninject.Parameters;

namespace DynamicSolver.DynamicSystem
{
    public class SolverRegistrationModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IDynamicSystemSolver>().To<EulerDynamicSystemSolver>();
            Bind<IDynamicSystemSolver>().To<ExtrapolationEulerDynamicSystemSolver>().WithParameter(new ConstructorArgument("extrapolationStageCount", 3));
            Bind<IDynamicSystemSolver>().To<ExtrapolationEulerDynamicSystemSolver>().WithParameter(new ConstructorArgument("extrapolationStageCount", 4));

            Bind<IDynamicSystemSolver>().To<ImplicitEulerDynamicSystemSolver>();
            Bind<IDynamicSystemSolver>().To<ExtrapolationImplicitEulerDynamicSystemSolver>().WithParameter(new ConstructorArgument("extrapolationStageCount", 3));
            Bind<IDynamicSystemSolver>().To<ExtrapolationImplicitEulerDynamicSystemSolver>().WithParameter(new ConstructorArgument("extrapolationStageCount", 4));

            Bind<IDynamicSystemSolver>().To<RungeKutta4DynamicSystemSolver>();
            Bind<IDynamicSystemSolver>().To<KDDynamicSystemSolver>();
            Bind<IDynamicSystemSolver>().To<ExtrapolationKDDynamicSystemSolver>().WithParameter(new ConstructorArgument("extrapolationStageCount", 2));
            Bind<IDynamicSystemSolver>().To<ExtrapolationKDDynamicSystemSolver>().WithParameter(new ConstructorArgument("extrapolationStageCount", 3));
            Bind<IDynamicSystemSolver>().To<ExtrapolationKDDynamicSystemSolver>().WithParameter(new ConstructorArgument("extrapolationStageCount", 4));
            Bind<IDynamicSystemSolver>().To<DormandPrince5DynamicSystemSolver>();
            Bind<IDynamicSystemSolver>().To<DormandPrince7DynamicSystemSolver>();
            Bind<IDynamicSystemSolver>().To<DormandPrince8DynamicSystemSolver>();
        }
    }
}