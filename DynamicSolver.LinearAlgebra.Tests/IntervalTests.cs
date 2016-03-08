using System.Linq;
using NUnit.Framework;

namespace DynamicSolver.LinearAlgebra.Tests
{
    [TestFixture]
    public class IntervalTests
    {
        private const double ACCURACY = 10e-8;

        [Test]
        public void Center_HasCorrectValue()
        {
            Assert.That(new Interval(new Point(1,1), new Point(3,3)).Center.ToArray(), Is.EqualTo(new Point(2,2).ToArray()).Within(ACCURACY));
            Assert.That(new Interval(new Point(-1,0), new Point(1,2)).Center.ToArray(), Is.EqualTo(new Point(0,1).ToArray()).Within(ACCURACY));
            Assert.That(new Interval(new Point(1,1), new Point(1,2)).Center.ToArray(), Is.EqualTo(new Point(1,1.5).ToArray()).Within(ACCURACY));
        }
    }
}