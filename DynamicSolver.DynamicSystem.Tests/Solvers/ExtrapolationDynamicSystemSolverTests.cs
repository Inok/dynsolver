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

    [TestFixture(typeof(KDFirstExplicitDynamicSystemSolver), 1, 2)]
    [TestFixture(typeof(KDFirstExplicitDynamicSystemSolver), 2, 4, 90)]
    [TestFixture(typeof(KDFirstExplicitDynamicSystemSolver), 3, 6, 90)]
    [TestFixture(typeof(KDFirstExplicitDynamicSystemSolver), 4, 8, 85)]

    [TestFixture(typeof(KDFirstImplicitDynamicSystemSolver), 1, 2)]
    [TestFixture(typeof(KDFirstImplicitDynamicSystemSolver), 2, 4, 95)]
    [TestFixture(typeof(KDFirstImplicitDynamicSystemSolver), 3, 6, 90)]
    [TestFixture(typeof(KDFirstImplicitDynamicSystemSolver), 4, 8, 85)]

    [TestFixture(typeof(ExplicitMiddlePointDynamicSystemSolver), 1, 2)]
    [TestFixture(typeof(ExplicitMiddlePointDynamicSystemSolver), 2, 3)]

    [TestFixture(typeof(SymmetricExplicitMiddlePointDynamicSystemSolver), 1, 2)]
    [TestFixture(typeof(SymmetricExplicitMiddlePointDynamicSystemSolver), 2, 4)]
    [TestFixture(typeof(SymmetricExplicitMiddlePointDynamicSystemSolver), 3, 6)]
    public class ExtrapolationDynamicSystemSolverTests : DynamicSystemSolverTests
    {
        public ExtrapolationDynamicSystemSolverTests(Type baseSolverType, int stageCount, int methodAccuracy)
            : this(baseSolverType, stageCount, methodAccuracy, 100)
        {
        }

        public ExtrapolationDynamicSystemSolverTests(Type baseSolverType, int stageCount, int methodAccuracy,
            int tolerancePercent)
            : base(
                new ExtrapolationSolver((IDynamicSystemSolver) Activator.CreateInstance(baseSolverType), stageCount, false),
                methodAccuracy,
                null,
                tolerancePercent / 100f
            )
        {
        }
    }
}