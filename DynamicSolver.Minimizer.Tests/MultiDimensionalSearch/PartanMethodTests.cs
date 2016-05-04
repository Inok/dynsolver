using DynamicSolver.LinearAlgebra.Derivative;
using DynamicSolver.Minimizer.DirectedSearch;
using DynamicSolver.Minimizer.MinimizationInterval;
using DynamicSolver.Minimizer.MultiDimensionalSearch;
using NUnit.Framework;

namespace DynamicSolver.Minimizer.Tests.MultiDimensionalSearch
{
    [TestFixture]
    public class PartanMethodTests : MultiDimensionalMethodTests<PartanMethodTests>
    {
        public override void Setup()
        {
            var settings = MultiDimensionalSearchSettings.Default;
            var numericalDerivativeCalculator = new NumericalDerivativeCalculator(DerivativeCalculationSettings.Default);

            SearchStrategy = new PartanMethod(
                new GoldenRatioMethod(
                    new DerivativeSwannMethod(DerivativeSwannMethodSettings.Default, numericalDerivativeCalculator),
                    DirectedSearchSettings.Default),
                numericalDerivativeCalculator,
                settings);
            ExpectedAccuracy = settings.Accuracy * 100;
        }
    }
}