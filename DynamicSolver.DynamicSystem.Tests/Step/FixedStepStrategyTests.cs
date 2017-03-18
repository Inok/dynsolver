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
            var factory = new FixedStepStrategy(0.1);

            var stepper = factory.Create(startValue);

            Assert.That(stepper.CurrentStep, Is.Not.Null);
            Assert.That(stepper.CurrentStep.AbsoluteValue, Is.EqualTo(startValue));
            Assert.That(stepper.CurrentStep.Delta, Is.Zero);
        }

        [Test]
        public void MoveNext_MakesFixedStepAndReturnsNewCurrentValue(
            [Range(-3d, 3d, 0.5d)] double startValue,
            [Range(0.1d, 1.5d, 0.3d)] double stepSize
        )
        {
            var factory = new FixedStepStrategy(stepSize);

            var stepper = factory.Create(startValue);

            var newValue = stepper.MoveNext();

            Assert.That(newValue, Is.EqualTo(stepper.CurrentStep));

            Assert.That(stepper.CurrentStep, Is.Not.Null);
            Assert.That(stepper.CurrentStep.AbsoluteValue, Is.EqualTo(startValue + stepSize));
            Assert.That(stepper.CurrentStep.Delta, Is.EqualTo(stepSize));
        }

        [Test]
        public void MoveNext_WhenCalledManyTimes_CreatesCorrectVariableValues()
        {
            const double stepSize = 0.2;
            const double startValue = 1;

            var factory = new FixedStepStrategy(stepSize);
            var stepper = factory.Create(startValue);

            var expectedValue = startValue;
            for (var i = 1; i <= 10; i++)
            {
                expectedValue += stepSize;

                stepper.MoveNext();
                Assert.That(stepper.CurrentStep.Delta, Is.EqualTo(stepSize));
                Assert.That(stepper.CurrentStep.AbsoluteValue, Is.EqualTo(expectedValue));
            }
        }
    }
}