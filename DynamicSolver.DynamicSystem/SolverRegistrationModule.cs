﻿using System;
using DynamicSolver.CoreMath.Execution;
using DynamicSolver.CoreMath.Execution.Compiler;
using DynamicSolver.CoreMath.Execution.Interpreter;
using DynamicSolver.DynamicSystem.Solvers;
using DynamicSolver.DynamicSystem.Solvers.Explicit;
using DynamicSolver.DynamicSystem.Solvers.SemiImplicit;
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

            Bind<IDynamicSystemSolver>().ToMethod(c => new ExplicitMiddlePointDynamicSystemSolver()).InSingletonScope();
            Bind<IDynamicSystemSolver>().ToMethod(c => new SymmetricExplicitMiddlePointDynamicSystemSolver()).InSingletonScope();
            
            Bind<IDynamicSystemSolver>().ToMethod(c => new RungeKutta4DynamicSystemSolver()).InSingletonScope();
            Bind<IDynamicSystemSolver>().ToMethod(c => new DormandPrince5DynamicSystemSolver()).InSingletonScope();
            Bind<IDynamicSystemSolver>().ToMethod(c => new DormandPrince7DynamicSystemSolver()).InSingletonScope();
            Bind<IDynamicSystemSolver>().ToMethod(c => new DormandPrince8DynamicSystemSolver()).InSingletonScope();
        }
    }
}