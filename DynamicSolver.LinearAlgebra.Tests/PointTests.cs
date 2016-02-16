using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DynamicSolver.Abstractions.Tools;
using NUnit.Framework;

namespace DynamicSolver.LinearAlgebra.Tests
{
    [TestFixture]
    public class PointTests
    {
        [Test]
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        public void Point_InvalidArguments_Throws()
        {
            Assert.That(() => new Point(null), Throws.ArgumentNullException);
            Assert.That(() => new Point(new double[0]), Throws.ArgumentException);
        }

        [Test]
        public void Dimension_HasCorrectValue([Range(1, 5)]int expectedDimension)
        {
            var point = new Point(new double[expectedDimension]);

            Assert.That(point.Dimension, Is.EqualTo(expectedDimension));
        }

        [Test]
        public void Count_HasCorrectValue([Range(1, 5)]int expectedCount)
        {
            var point = new Point(new double[expectedCount]);

            Assert.That(((IReadOnlyCollection<double>)point).Count, Is.EqualTo(expectedCount));
        }

        [Test]
        public void Indexer_ReturnsCorrectItems()
        {
            var expected = new double[] {1, 2, 4, 0, 3};
            var point = new Point(expected);

            for (var i = 0; i < expected.Length; i++)
            {
                Assert.That(point[i], Is.EqualTo(expected[i]));
            }
            
            Assert.That(() => point[-1], Throws.TypeOf<IndexOutOfRangeException>());
            Assert.That(() => point[expected.Length], Throws.TypeOf<IndexOutOfRangeException>());
        }

        [Test]
        public void Enumerate_ReturnsAllItemsInCorrectOrder()
        {
            var expected = new double[] {1, 2, 4, 0, 3};
            var point = new Point(expected);

            Assert.That(point.AsEnumerable(), Is.EqualTo(expected));
        }

        [Test]
        public void Enumerate_Typed_ReturnsAllItemsInCorrectOrder()
        {
            var expected = new double[] {1, 2, 4, 0, 3};
            var point = new Point(expected);

            var list = point.ToList();

            Assert.That(list, Is.EqualTo(expected));
        }


        [Test]
        public void Equals_WhenEqual_ReturnsTrue()
        {
            var values = new double[] {1, 2, 4, 0, 3};

            var first = new Point(values);
            var second = new Point(values);

            // ReSharper disable once EqualExpressionComparison
            Assert.That(first.Equals(first));
            Assert.That(first.Equals(first.Clone()));
            Assert.That(first.Equals(second));
            Assert.That(second.Equals(first));
            Assert.That(((IEquatable<Point>)first).Equals(second));
            Assert.That(((IEquatable<Point>)first).Equals(first));
        }

        [Test]
        public void Equals_WhenNotEqual_ReturnsFalse()
        {
            var first = new Point(new double[] {1, 2, 4, 0, 3});

            Assert.That(first.Equals(new Point(new double[] { 0, 2, 4, 0, 3 })), Is.False);
            Assert.That(first.Equals(new Point(new double[] { 1, 2, 4, 0, 6 })), Is.False);
            Assert.That(first.Equals(new Point(new double[] { 1, 3, 4, 0, 3 })), Is.False);
            Assert.That(first.Equals(new Point(new double[] { 1, 3, 4, 3 })), Is.False);
            
            Assert.That(first.Equals((object)null), Is.False);
            Assert.That(first.Equals((Point)null), Is.False);
            Assert.That(first.Equals(new object()), Is.False);
            Assert.That(first.Equals(new double[] { 1, 2, 4, 0, 3 }), Is.False);

            var second = (IEquatable<Point>)new Point(new double[] { 1, 2, 4, 0, 3 });

            Assert.That(second.Equals(new Point(new double[] { 0, 2, 4, 0, 3 })), Is.False);
            Assert.That(second.Equals(new Point(new double[] { 1, 2, 4, 0, 6 })), Is.False);
            Assert.That(second.Equals(new Point(new double[] { 1, 3, 4, 0, 3 })), Is.False);
            Assert.That(first.Equals(new Point(new double[] { 1, 3, 4, 3 })), Is.False);

            Assert.That(second.Equals((object)null), Is.False);
            Assert.That(second.Equals((Point)null), Is.False);
            Assert.That(second.Equals(new object()), Is.False);
            Assert.That(second.Equals(new double[] { 1, 2, 4, 0, 3 }), Is.False);

        }

        [Test]
        public void Clone_CreatesCorrectClone()
        {
            var original = new Point(new double[] {1, 2, 4, 0, 3});
            Assert.That(original.Clone(), Is.Not.SameAs(original).And.EqualTo(original));
        }

        [Test]
        public void ToString_FormatsPointCorrectly()
        {
            var values = new double[] {1, 2.5, 4.0, 0.474, 3};
            var expected = values.DumpInline();
            var point = new Point(values);
            Assert.That(point.ToString(), Is.EqualTo(expected));
        }
    }
}
