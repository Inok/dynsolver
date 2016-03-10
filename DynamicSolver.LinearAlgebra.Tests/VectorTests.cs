using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DynamicSolver.Abstractions.Tools;
using NUnit.Framework;

namespace DynamicSolver.LinearAlgebra.Tests
{
    [TestFixture]
    public class VectorTests
    {
        [Test]
        public void Vector_ValidArguments_CreatesCorrectInstance()
        {
            Assert.That(new Vector(new double[] {1}.ToArray()), Is.EqualTo(new double[] {1}));
            Assert.That(new Vector(new double[] {4, 7}.ToArray()), Is.EqualTo(new double[] {4, 7}));

            Assert.That(new Vector(new Point(new double[] {4, 7})).ToArray(), Is.EqualTo(new double[] {4, 7}));
            Assert.That(new Vector(new Point(new double[] {-1, 5})).ToArray(), Is.EqualTo(new double[] {-1, 5}));

            Assert.That(new Vector(new Point(new[] {0d, 0d}), new Point(new[] {1d, 2d})).ToArray(), Is.EqualTo(new double[] {1, 2}));
            Assert.That(new Vector(new Point(new[] {1d, 2d}), new Point(new[] {0d, 0d})).ToArray(), Is.EqualTo(new double[] {-1, -2}));
            Assert.That(new Vector(new Point(new[] {1d, 2d}), new Point(new[] {1d, 2d})).ToArray(), Is.EqualTo(new double[] {0, 0}));
        }

        [Test]
        public void Vector_FromArray_ModifyOriginalArray_VectorNotChanges()
        {
            var original = new double[] {1, 2, 3};
            var expected = original.ToArray();

            var vector = new Vector(original);
            original[0] = 3;
            original[1] = -5;
            original[2] = 4;

            Assert.That(vector, Is.EqualTo(expected));
        }

        [Test]
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        public void Vector_InvalidArguments_Throws()
        {
            Assert.That(() => new Vector((Point) null), Throws.ArgumentNullException);
            Assert.That(() => new Vector((double[]) null), Throws.ArgumentNullException);
            Assert.That(() => new Vector((Point) null, (Point) null), Throws.ArgumentNullException);
            Assert.That(() => new Vector((Point) null, new Point(new[] {1.0, 2.0})), Throws.ArgumentNullException);
            Assert.That(() => new Vector(new Point(new[] {1.0, 2.0}), (Point) null), Throws.ArgumentNullException);

            Assert.That(() => new Vector(), Throws.ArgumentException);
            Assert.That(() => new Vector(new Point(new[] {1.0, 2.0, 3.0}), new Point(new[] {1.0, 2.0})), Throws.ArgumentException);
        }

        [TestCase(new double[] {0, 0, 0}, 0)]
        [TestCase(new double[] {0, 1, 0}, 1)]
        [TestCase(new double[] {2, 2, 1}, 3)]
        [TestCase(new double[] {-1, 2, 2}, 3)]
        [TestCase(new double[] {10}, 10)]
        public void Length_HasCorrectValue(double[] args, double expectedLength)
        {
            var vector = new Vector(args);

            Assert.That(vector.Length, Is.EqualTo(expectedLength));
        }


        [TestCase(new double[] {1}, new double[] {1})]
        [TestCase(new double[] {0, 1, 0}, new double[] {0, 1, 0})]
        [TestCase(new double[] {2, 2, 1}, new double[] {2.0/3, 2.0/3, 1.0/3})]
        [TestCase(new double[] {8, 0}, new double[] {1, 0})]
        public void Normalize_HasCorrectValue(double[] original, double[] expected)
        {
            Console.WriteLine(original.Dump());
            Console.WriteLine(expected.Dump());

            var vector = new Vector(original);

            var actual = vector.Normalize();
            Assert.That(actual, Is.Not.SameAs(vector));
            Assert.That(actual.ToArray(), Is.EqualTo(expected));
        }

        [Test]
        public void Normalize_WhenZeroLength_Throws()
        {
            var vector = new Vector(new double[] {0, 0, 0});
            Assert.That(() => vector.Normalize(), Throws.InvalidOperationException);
        }

        [TestCase(new double[] {1, 2}, new double[] {-2, 5}, new double[] {-1, 7})]
        [TestCase(new double[] {0, 0}, new double[] {1, 1}, new double[] {1, 1})]
        public void PlusOperator_AddsCorrectly_KeepsTermsUnchanged(double[] first, double[] second, double[] expected)
        {
            var firstVector = new Vector(first);
            var secondVector = new Vector(second);
            var actual = firstVector + secondVector;

            Assert.That(actual.ToArray(), Is.EqualTo(expected));

            Assert.That(firstVector.ToArray(), Is.EqualTo(first));
            Assert.That(secondVector.ToArray(), Is.EqualTo(second));
        }

        [TestCase(new double[] {-1, 7}, new double[] {1, 2}, new double[] {-2, 5})]
        [TestCase(new double[] {1, 1}, new double[] {0, 0}, new double[] {1, 1})]
        public void MinusOperator_SubtractsCorrectly_KeepsTermsUnchanged(double[] first, double[] second, double[] expected)
        {
            var firstVector = new Vector(first);
            var secondVector = new Vector(second);
            var actual = firstVector - secondVector;

            Assert.That(actual.ToArray(), Is.EqualTo(expected));

            Assert.That(firstVector.ToArray(), Is.EqualTo(first));
            Assert.That(secondVector.ToArray(), Is.EqualTo(second));
        }


        [TestCase(new double[] {-1, 7}, new double[] {1, 2}, 13)]
        [TestCase(new double[] {1, 1}, new double[] {0, 0}, 0)]
        public void MultiplyOperator_OnVectors_MultipliesCorrectly_KeepsTermsUnchanged(double[] first, double[] second, double expected)
        {
            var firstVector = new Vector(first);
            var secondVector = new Vector(second);
            var actual = firstVector * secondVector;

            Assert.That(actual, Is.EqualTo(expected));

            Assert.That(firstVector.ToArray(), Is.EqualTo(first));
            Assert.That(secondVector.ToArray(), Is.EqualTo(second));
        }

        [TestCase(new double[] { 1, 2 }, 3, new double[] {3, 6})]
        [TestCase(new double[] {-1, 1}, 0, new double[] {0, 0})]
        [TestCase(new double[] {-1, 1}, -2, new double[] {2, -2})]
        public void MultiplyOperator_WithNumber_MultipliesCorrectly_KeepsTermsUnchanged(double[] first, double second, double[] expected)
        {
            var firstVector = new Vector(first);

            Assert.That((firstVector * second).ToArray(), Is.EqualTo(expected));
            Assert.That((second * firstVector).ToArray(), Is.EqualTo(expected));

            Assert.That(firstVector.ToArray(), Is.EqualTo(first));
        }

        [Test]
        public void Operators_DifferentDimensions_Throws()
        {
            var first = new Vector(1, 2);
            var second = new Vector(1, 2, 3);
            Assert.That(() => first + second, Throws.ArgumentException);
            Assert.That(() => first - second, Throws.ArgumentException);
            Assert.That(() => first * second, Throws.ArgumentException);
        }

        [Test]
        public void Dimension_HasCorrectValue([Range(1, 5)] int expectedDimension)
        {
            var vector = new Vector(new double[expectedDimension]);

            Assert.That(vector.Dimension, Is.EqualTo(expectedDimension));
        }

        [Test]
        public void Count_HasCorrectValue([Range(1, 5)] int expectedCount)
        {
            var vector = new Vector(new double[expectedCount]);

            Assert.That(((IReadOnlyCollection<double>) vector).Count, Is.EqualTo(expectedCount));
        }

        [Test]
        public void Indexer_ReturnsCorrectItems()
        {
            var expected = new double[] {1, 2, 4, 0, 3};
            var vector = new Vector(expected);

            for (var i = 0; i < expected.Length; i++)
            {
                Assert.That(vector[i], Is.EqualTo(expected[i]));
            }

            Assert.That(() => vector[-1], Throws.TypeOf<IndexOutOfRangeException>());
            Assert.That(() => vector[expected.Length], Throws.TypeOf<IndexOutOfRangeException>());
        }

        [Test]
        public void Enumerate_ReturnsAllItemsInCorrectOrder()
        {
            var expected = new double[] {1, 2, 4, 0, 3};
            var vector = new Vector(expected);

            Assert.That(vector.AsEnumerable(), Is.EqualTo(expected));
        }

        [Test]
        public void Enumerate_Typed_ReturnsAllItemsInCorrectOrder()
        {
            var expected = new double[] {1, 2, 4, 0, 3};
            var vector = new Vector(expected);

            var list = vector.ToList();

            Assert.That(list, Is.EqualTo(expected));
        }

        [Test]
        public void Clone_CreatesCorrectClone()
        {
            var original = new Vector(1, 2, 4, 0, 3);
            Assert.That(original.Clone(), Is.Not.SameAs(original).And.EqualTo(original));
            Assert.That(((ICloneable) original).Clone(), Is.Not.SameAs(original).And.EqualTo(original));
        }

        [Test]
        public void ToString_FormatsPointCorrectly()
        {
            var values = new double[] {1, 2.5, 4.0, 0.474, 3};
            var vector = new Vector(values);
            Assert.That(vector.ToString(), Is.EqualTo($"({string.Join(";", values)})"));
        }
    }
}