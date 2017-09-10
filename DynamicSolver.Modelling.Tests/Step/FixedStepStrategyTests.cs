using DynamicSolver.Modelling.Step;
using NUnit.Framework;

namespace DynamicSolver.Modelling.Tests.Step
{
    [TestFixture]
    public class FixedStepStrategyTests
    {
        [Test]
        public void Factory_CreatesStepStrategyWithCorrectCurrentValue([Range(-3d, 3d, 0.5d)] double startValue)
        {
            var stepper = new FixedStepStepper(0.1, startValue);

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
            var stepper = new FixedStepStepper(stepSize, startValue);

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

            var stepper = new FixedStepStepper(stepSize, startValue);

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