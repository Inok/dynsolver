using NUnit.Framework;

namespace DynamicSolver.Minimizer.Tests
{
    [TestFixture]
    public class IterationLimiterTests
    {
        private class LimitSettings : IIterationLimitSettings
        {
            public int MaxIterationCount { get; }
            public bool AbortSearchOnIterationLimit { get; }

            public LimitSettings(int maxIterationCount, bool abortSearchOnIterationLimit)
            {
                MaxIterationCount = maxIterationCount;
                AbortSearchOnIterationLimit = abortSearchOnIterationLimit;
            }
        }

        [Test]
        public void NextIteration_ThrowsOnLimit([Values(1, 3, 5)]int limit, [Values(true, false)]bool abortOnLimit)
        {
            var limiter = new IterationLimiter(new LimitSettings(limit, abortOnLimit));

            for (var i = 0; i < limit; i++)
            {
                Assert.That(limiter.IterationLimitReached, Is.False);
                Assert.That(limiter.ShouldInterrupt, Is.False);
                Assert.That(() => limiter.NextIteration(), Throws.Nothing);
                Assert.That(limiter.Iteration, Is.EqualTo(i + 1));
            }

            Assert.That(limiter.IterationLimitReached);
            Assert.That(limiter.ShouldInterrupt, Is.EqualTo(!abortOnLimit));
            Assert.That(() => limiter.NextIteration(), Throws.InvalidOperationException);
            Assert.That(limiter.Iteration, Is.EqualTo(limit));
        }

        [Test]
        public void TryNextIteration_ThrowsOnLimit([Values(1, 3, 5)]int limit, [Values(true, false)]bool abortOnLimit)
        {
            var limiter = new IterationLimiter(new LimitSettings(limit, abortOnLimit));

            for (var i = 0; i < limit; i++)
            {
                Assert.That(limiter.IterationLimitReached, Is.False);
                Assert.That(limiter.ShouldInterrupt, Is.False);
                Assert.That(limiter.TryNextIteration());
                Assert.That(limiter.Iteration, Is.EqualTo(i + 1));
            }

            Assert.That(limiter.IterationLimitReached);
            Assert.That(limiter.ShouldInterrupt, Is.EqualTo(!abortOnLimit));
            Assert.That(limiter.TryNextIteration(), Is.False);
            Assert.That(limiter.Iteration, Is.EqualTo(limit));
        }

    }
}