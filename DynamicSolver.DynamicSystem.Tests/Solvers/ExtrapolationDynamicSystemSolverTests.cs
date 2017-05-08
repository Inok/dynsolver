using System;
using DynamicSolver.DynamicSystem.Solvers;
using DynamicSolver.DynamicSystem.Solvers.Explicit;
using DynamicSolver.DynamicSystem.Solvers.Extrapolation;
using DynamicSolver.DynamicSystem.Solvers.SemiImplicit;
using NUnit.Framework;


namespace DynamicSolver.DynamicSystem.Tests.Solvers
{
    [TestFixture(typeof(ExplicitEulerSolver), 1, 1)]
    [TestFixture(typeof(ExplicitEulerSolver), 2, 2)]
    [TestFixture(typeof(ExplicitEulerSolver), 4, 4)]
    [TestFixture(typeof(ExplicitEulerSolver), 8, 8)]
    [TestFixture(typeof(ExplicitEulerSolver), 8, 8)]

    [TestFixture(typeof(KDDynamicSystemSolver), 1, 2)]
    [TestFixture(typeof(KDDynamicSystemSolver), 2, 4, 90)]
    [TestFixture(typeof(KDDynamicSystemSolver), 3, 6, 90)]
    [TestFixture(typeof(KDDynamicSystemSolver), 4, 8, 85)]


    [TestFixture(typeof(ExplicitMiddlePointDynamicSystemSolver), 1, 2)]
    [TestFixture(typeof(ExplicitMiddlePointDynamicSystemSolver), 2, 3)]

    [TestFixture(typeof(SymmetricExplicitMiddlePointExtrapolationSolver), 1, 2)]
    [TestFixture(typeof(SymmetricExplicitMiddlePointExtrapolationSolver), 2, 4)]
    [TestFixture(typeof(SymmetricExplicitMiddlePointExtrapolationSolver), 3, 6)]
    public class ExtrapolationDynamicSystemSolverTests : DynamicSystemSolverTests
    {
        public ExtrapolationDynamicSystemSolverTests(Type baseSolverType, int stageCount, int methodAccuracy)
            : this(baseSolverType, stageCount, methodAccuracy, 100)
        {
        }

        public ExtrapolationDynamicSystemSolverTests(Type baseSolverType, int stageCount, int methodAccuracy, int tolerancePercent)
            : base(
                baseSolverType.IsSubclassOf(typeof(ExtrapolationSolver))
                  ? (IDynamicSystemSolver)Activator.CreateInstance(baseSolverType, stageCount)
                  : new ExtrapolationSolver((IDynamicSystemSolver)Activator.CreateInstance(baseSolverType), stageCount),
                methodAccuracy,
                tolerancePercent / 100f
            )
        {
        }
    }
}