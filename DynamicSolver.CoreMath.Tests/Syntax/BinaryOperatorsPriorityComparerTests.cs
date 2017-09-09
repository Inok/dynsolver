using System;
using System.Collections.Generic;
using DynamicSolver.CoreMath.Syntax;
using DynamicSolver.CoreMath.Syntax.Model;
using Moq;
using NUnit.Framework;

namespace DynamicSolver.CoreMath.Tests.Syntax
{
    [TestFixture]
    public class BinaryOperatorsPriorityComparerTests
    {
        private static T Create<T>() where T: IBinaryOperator
        {
            return (T) Activator.CreateInstance(typeof(T), Mock.Of<ISyntaxExpression>(), Mock.Of<ISyntaxExpression>());
        }

        private static IEnumerable<object[]> CompareEqualCases()
        {
            yield return new object[] { Create<AddBinaryOperator>(), Create<AddBinaryOperator>()};
            yield return new object[] { Create<SubtractBinaryOperator>(), Create<SubtractBinaryOperator>()};
            yield return new object[] { Create<MultiplyBinaryOperator>(), Create<MultiplyBinaryOperator>()};
            yield return new object[] { Create<DivideBinaryOperator>(), Create<DivideBinaryOperator>()};
            yield return new object[] { Create<PowBinaryOperator>(), Create<PowBinaryOperator>()};
        }

        [TestCaseSource(nameof(CompareEqualCases))]
        public void Compare_PriorityIsEqual_ReturnsZero(IBinaryOperator x, IBinaryOperator y)
        {
            var comparer = new BinaryOperatorsPriorityComparer();
            Assert.That(comparer.Compare(x, y), Is.EqualTo(0));
        }

        private static IEnumerable<object[]> CompareNotEqualCases()
        {
            yield return new object[] { Create<MultiplyBinaryOperator>(), Create<AddBinaryOperator>() };
            yield return new object[] { Create<MultiplyBinaryOperator>(), Create<SubtractBinaryOperator>() };
            yield return new object[] { Create<DivideBinaryOperator>(), Create<AddBinaryOperator>() };
            yield return new object[] { Create<DivideBinaryOperator>(), Create<SubtractBinaryOperator>() };
            yield return new object[] { Create<PowBinaryOperator>(), Create<AddBinaryOperator>() };
            yield return new object[] { Create<PowBinaryOperator>(), Create<SubtractBinaryOperator>() };
            yield return new object[] { Create<PowBinaryOperator>(), Create<MultiplyBinaryOperator>() };
            yield return new object[] { Create<PowBinaryOperator>(), Create<DivideBinaryOperator>() };
        }

        [TestCaseSource(nameof(CompareNotEqualCases))]
        public void Compare_PriorityOfFirstIsGreater_ReturnsPositive(IBinaryOperator x, IBinaryOperator y)
        {
            var comparer = new BinaryOperatorsPriorityComparer();
            Assert.That(comparer.Compare(x, y), Is.EqualTo(1));
        }

        [TestCaseSource(nameof(CompareNotEqualCases))]
        public void Compare_PriorityOfFirstIsLess_ReturnsNegative(IBinaryOperator x, IBinaryOperator y)
        {
            var comparer = new BinaryOperatorsPriorityComparer();
            Assert.That(comparer.Compare(y, x), Is.EqualTo(-1));
        }

    }
}