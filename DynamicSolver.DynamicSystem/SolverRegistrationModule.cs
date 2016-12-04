using System;
using DynamicSolver.DynamicSystem.Solver;
using DynamicSolver.Expressions.Execution;
using DynamicSolver.Expressions.Execution.Compiler;
using DynamicSolver.Expressions.Execution.Interpreter;
using DynamicSolver.Expressions.Parser;
using Ninject.Modules;
using Ninject.Parameters;

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

            Bind<IDynamicSystemSolver>().To<EulerDynamicSystemSolver>();
            Bind<IDynamicSystemSolver>().To<ExtrapolationEulerDynamicSystemSolver>().WithParameter(new ConstructorArgument("extrapolationStageCount", 3));
            Bind<IDynamicSystemSolver>().To<ExtrapolationEulerDynamicSystemSolver>().WithParameter(new ConstructorArgument("extrapolationStageCount", 4));
            Bind<IDynamicSystemSolver>().To<ExplicitMiddlePointDynamicSystemSolver>();
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