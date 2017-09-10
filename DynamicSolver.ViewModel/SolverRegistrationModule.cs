using System;
using DynamicSolver.Core.Execution;
using DynamicSolver.Core.Execution.Compiler;
using DynamicSolver.Core.Execution.Interpreter;
using DynamicSolver.Modelling.Solvers;
using DynamicSolver.Modelling.Solvers.Explicit;
using DynamicSolver.Modelling.Solvers.SemiImplicit;
using Ninject.Modules;

namespace DynamicSolver.ViewModel
{
    public class SolverRegistrationModule : NinjectModule
    {
        public enum FunctionFactoryType
        {
            Compiled,
            Interpreted
        }

        private readonly FunctionFactoryType _factoryType;

        public SolverRegistrationModule(FunctionFactoryType factoryType)
        {
            _factoryType = factoryType;
        }

        public override void Load()
        {
            switch (_factoryType)
            {
                case FunctionFactoryType.Compiled:
                    Bind<IExecutableFunctionFactory>().To<CompiledFunctionFactory>();
                    break;
                case FunctionFactoryType.Interpreted:
                    Bind<IExecutableFunctionFactory>().To<InterpretedFunctionFactory>();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Bind<IDynamicSystemSolver>().ToMethod(c => new ExplicitEulerSolver()).InSingletonScope();
            Bind<IDynamicSystemSolver>().ToMethod(c => new EulerCromerSolver()).InSingletonScope();

            Bind<IDynamicSystemSolver>().ToMethod(c => new KDNewtonBasedDynamicSystemSolver()).InSingletonScope();
            Bind<IDynamicSystemSolver>().ToMethod(c => new KDFastImplicitDynamicSystemSolver()).InSingletonScope();

            Bind<IDynamicSystemSolver>().ToMethod(c => new KDFastDynamicSystemSolver(2)).InSingletonScope();
            Bind<IDynamicSystemSolver>().ToMethod(c => new KDFastDynamicSystemSolver(4)).InSingletonScope();
            Bind<IDynamicSystemSolver>().ToMethod(c => new KDFastDynamicSystemSolver(6)).InSingletonScope();
            Bind<IDynamicSystemSolver>().ToMethod(c => new KDFastDynamicSystemSolver(8)).InSingletonScope();

            Bind<IDynamicSystemSolver>().ToMethod(c => new ExplicitMiddlePointDynamicSystemSolver()).InSingletonScope();
            Bind<IDynamicSystemSolver>().ToMethod(c => new SymmetricExplicitMiddlePointDynamicSystemSolver()).InSingletonScope();
            
            Bind<IDynamicSystemSolver>().ToMethod(c => new RungeKutta4DynamicSystemSolver()).InSingletonScope();
            Bind<IDynamicSystemSolver>().ToMethod(c => new DormandPrince5DynamicSystemSolver()).InSingletonScope();
            Bind<IDynamicSystemSolver>().ToMethod(c => new DormandPrince7DynamicSystemSolver()).InSingletonScope();
            Bind<IDynamicSystemSolver>().ToMethod(c => new DormandPrince8DynamicSystemSolver()).InSingletonScope();
        }
    }
}