using System;
using System.Collections.Generic;
using DynamicSolver.Common.Extensions;
using NUnit.Framework;

namespace DynamicSolver.Common.Tests.Extensions
{
    [TestFixture]
    public class EnumerableExtensionsTests
    {
        [Test]
        public void Throttle_InvalidArguments_Throws()
        {
            Assert.That(() => EnumerableExtensions.Throttle<int>(null, 1), Throws.ArgumentNullException);
            Assert.That(() => new int[0].Throttle(-1), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => new int[0].Throttle(1, -1), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => new int[0].Throttle(1, -2), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [TestCase(new[] {1, 2, 3}, 0, new[] {1, 2, 3})]
        [TestCase(new[] {1, 2, 3}, 1, new[] {1, 3})]
        [TestCase(new[] {1, 2, 3, 4}, 1, new[] {1, 3})]
        [TestCase(new[] {1, 2, 3, 4, 5}, 1, new[] {1, 3, 5})]
        [TestCase(new[] {1, 2, 3, 4}, 3, new[] {1})]
        [TestCase(new[] {1, 2, 3, 4}, 10, new[] {1})]
        public void Throttle_WhenNoOffset_Throttles(IEnumerable<int> input, int step, IEnumerable<int> expected)
        {
            Assert.That(input.Throttle(step), Is.EqualTo(expected));
        }

        [TestCase(new[] {1, 2, 3}, 0, 0, new[] {1, 2, 3})]
        [TestCase(new[] {1, 2, 3, 4}, 1, 0, new[] {1, 3})]
        [TestCase(new[] {1, 2, 3}, 1, 1, new[] {2})]
        [TestCase(new[] {1, 2, 3, 4}, 1, 1, new[] {2, 4})]
        [TestCase(new[] {1, 2, 3, 4, 5}, 0, 4, new[] {5})]
        [TestCase(new[] {1, 2, 3, 4, 5}, 0, 5, new int[0])]
        [TestCase(new[] {1, 2, 3, 4, 5, 6}, 2, 2, new [] {3, 6})]
        public void Throttle_WithInitialOffset_Throttles(IEnumerable<int> input, int step, int offset, IEnumerable<int> expected)
        {
            Assert.That(input.Throttle(step, offset), Is.EqualTo(expected));
        }
    }
}