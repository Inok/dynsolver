using DynamicSolver.Minimizer.MultiDimensionalSearch;
using NUnit.Framework;

namespace DynamicSolver.Minimizer.Tests.MultiDimensionalSearch
{
    [TestFixture]
    public class NelderMeadMethodTests : MultiDimensionalMethodTests<NelderMeadMethodTests>
    {
        public override void Setup()
        {
            var settings = NelderMeadSearchSettings.Default;
            SearchStrategy = new NelderMeadMethod(settings);
            ExpectedAccuracy = 10e-4;
        }
    }
}