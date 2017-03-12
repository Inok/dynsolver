using System;
using DynamicSolver.DynamicSystem.Solvers;
using DynamicSolver.DynamicSystem.Solvers.Explicit;
using DynamicSolver.DynamicSystem.Solvers.Extrapolation;
using DynamicSolver.DynamicSystem.Solvers.SemiImplicit;
using DynamicSolver.Expressions.Execution;
using DynamicSolver.Expressions.Execution.Compiler;
using DynamicSolver.Expressions.Execution.Interpreter;
using DynamicSolver.Expressions.Parser;
using Ninject.Modules;

namespace DynamicSolver.DynamicSystem
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
            Bind<IExpressionParser>().To<ExpressionParser>();
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

            Bind<IDynamicSystemSolver>().ToMethod(c => new ExplicitEulerSolver());

            Bind<IDynamicSystemSolver>().ToMethod(c => new ExtrapolationSolver(new ExplicitEulerSolver(), 2));
            Bind<IDynamicSystemSolver>().ToMethod(c => new ExtrapolationSolver(new ExplicitEulerSolver(), 3));
            Bind<IDynamicSystemSolver>().ToMethod(c => new ExtrapolationSolver(new ExplicitEulerSolver(), 4));
            Bind<IDynamicSystemSolver>().ToMethod(c => new ExtrapolationSolver(new ExplicitEulerSolver(), 8));

            Bind<IDynamicSystemSolver>().ToMethod(c => new KDDynamicSystemSolver());
            Bind<IDynamicSystemSolver>().ToMethod(c => new ExtrapolationSolver(new KDDynamicSystemSolver(), 2));
            Bind<IDynamicSystemSolver>().ToMethod(c => new ExtrapolationSolver(new KDDynamicSystemSolver(), 3));
            Bind<IDynamicSystemSolver>().ToMethod(c => new ExtrapolationSolver(new KDDynamicSystemSolver(), 4));

            Bind<IDynamicSystemSolver>().ToMethod(c => new ExplicitMiddlePointDynamicSystemSolver());
            Bind<IDynamicSystemSolver>().ToMethod(c => new RungeKutta4DynamicSystemSolver());
            Bind<IDynamicSystemSolver>().ToMethod(c => new DormandPrince5DynamicSystemSolver());
            Bind<IDynamicSystemSolver>().ToMethod(c => new DormandPrince7DynamicSystemSolver());
            Bind<IDynamicSystemSolver>().ToMethod(c => new DormandPrince8DynamicSystemSolver());
        }
    }
}