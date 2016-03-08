using DynamicSolver.Minimizer.MultiDimensionalSearch;
using NUnit.Framework;

namespace DynamicSolver.Minimizer.Tests.MultiDimensionalSearch
{
    [TestFixture]
    public class HookeJeevesMethodTests : MultiDimensionalMethodTests<HookeJeevesMethodTests>
    {
        public override void Setup()
        {
            var settings = HookeJeevesSearchSettings.Default;
            SearchStrategy = new HookeJeevesMethod(settings);
            Accuracy = settings.Accuracy;
        }
    }
}