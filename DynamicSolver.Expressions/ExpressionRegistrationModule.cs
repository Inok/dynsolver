using System;
using DynamicSolver.Expressions.Execution;
using DynamicSolver.Expressions.Execution.Compiler;
using DynamicSolver.Expressions.Execution.Interpreter;
using DynamicSolver.Expressions.Parser;
using Ninject.Modules;

namespace DynamicSolver.Expressions
{
    public class ExpressionRegistrationModule : NinjectModule
    {
        public enum FunctionFactoryType
        {
            Compiled,
            Interpreted
        }

        private readonly FunctionFactoryType _factoryType;

        public ExpressionRegistrationModule(FunctionFactoryType factoryType)
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
        }
    }
}