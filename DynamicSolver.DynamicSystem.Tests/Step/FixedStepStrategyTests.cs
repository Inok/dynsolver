using DynamicSolver.DynamicSystem.Step;
using NUnit.Framework;

namespace DynamicSolver.DynamicSystem.Tests.Step
{
    [TestFixture]
    public class FixedStepStrategyTests
    {
        [Test]
        public void Factory_CreatesStepStrategyWithCorrectCurrentValue([Range(-3d, 3d, 0.5d)] double startValue)
        {
            var factory = new FixedStepStrategyFactory(0.1);

            var stepStrategy = factory.Create(startValue);

            Assert.That(stepStrategy.Current, Is.Not.Null);
            Assert.That(stepStrategy.Current.AbsoluteValue, Is.EqualTo(startValue));
            Assert.That(stepStrategy.Current.Delta, Is.Zero);
        }

        [Test]
        public void MoveNext_MakesFixedStepAndReturnsNewCurrentValue(
            [Range(-3d, 3d, 0.5d)] double startValue,
            [Range(0.1d, 1.5d, 0.3d)] double stepSize
        )
        {
            var factory = new FixedStepStrategyFactory(stepSize);

            var stepStrategy = factory.Create(startValue);

            var newValue = stepStrategy.MoveNext();

            Assert.That(newValue, Is.EqualTo(stepStrategy.Current));

            Assert.That(stepStrategy.Current, Is.Not.Null);
            Assert.That(stepStrategy.Current.AbsoluteValue, Is.EqualTo(startValue + stepSize));
            Assert.That(stepStrategy.Current.Delta, Is.EqualTo(stepSize));
        }

        [Test]
        public void MoveNext_WhenCalledManyTimes_CreatesCorrectVariableValues()
        {
            const double stepSize = 0.2;
            const double startValue = 1;

            var factory = new FixedStepStrategyFactory(stepSize);
            var stepStrategy = factory.Create(startValue);

            var expectedValue = startValue;
            for (var i = 1; i <= 10; i++)
            {
                expectedValue += stepSize;

                stepStrategy.MoveNext();
                Assert.That(stepStrategy.Current.Delta, Is.EqualTo(stepSize));
                Assert.That(stepStrategy.Current.AbsoluteValue, Is.EqualTo(expectedValue));
            }
        }
    }
}