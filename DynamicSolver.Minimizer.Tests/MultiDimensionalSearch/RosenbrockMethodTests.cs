using DynamicSolver.LinearAlgebra.Derivative;
using DynamicSolver.Minimizer.DirectedSearch;
using DynamicSolver.Minimizer.MinimizationInterval;
using DynamicSolver.Minimizer.MultiDimensionalSearch;
using NUnit.Framework;

namespace DynamicSolver.Minimizer.Tests.MultiDimensionalSearch
{
    [TestFixture]
    public class RosenbrockMethodTests : MultiDimensionalMethodTests<RosenbrockMethodTests>
    {
        public override void Setup()
        {
            var settings = MultiDimensionalSearchSettings.Default;
            var numericalDerivativeCalculator = new NumericalDerivativeCalculator(DerivativeCalculationSettings.Default);

            SearchStrategy = new RosenbrockMethod(
                new GoldenRatioMethod(
                    new DerivativeSwannMethod(DerivativeSwannMethodSettings.Default, numericalDerivativeCalculator),
                    DirectedSearchSettings.Default),
                settings);
            ExpectedAccuracy = settings.Accuracy * 100;
        }
    }
}