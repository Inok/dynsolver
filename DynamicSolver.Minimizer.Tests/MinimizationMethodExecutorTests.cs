using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DynamicSolver.Abstractions;
using DynamicSolver.LinearAlgebra;
using Moq;
using NUnit.Framework;

namespace DynamicSolver.Minimizer.Tests
{
    [TestFixture]
    public class MinimizationMethodExecutorTests
    {
        [Test]
        public async Task MinimizeAsync_ReturnsResultForAllSearchers([Values(1, 3, 5)] int searchersCount)
        {
            var searcherMocks = Enumerable.Range(0, searchersCount).Select(i => GetSearchStrategyMock(new Point(i))).ToList();

            var executor = new MinimizationMethodExecutor(searcherMocks.Select(s => s.Object));

            var startPoint = new Point(0);
            var funcMock = new Mock<IExecutableFunction>();
            funcMock.Setup(f => f.Execute(It.IsAny<double[]>())).Returns<double[]>(args => args[0] * -1);

            var resultSet = await executor.MinimizeAsync(funcMock.Object, startPoint);

            Assert.That(resultSet, Is.Not.Null);
            Assert.That(resultSet.Results, Is.Not.Null.And.Not.Contains(null));
            Assert.That(resultSet.Results.Count, Is.EqualTo(searchersCount));
            Assert.That(resultSet.Results.Select(r => r.Minimizer), Is.EqualTo(searcherMocks.Select(m => m.Object)));

            for (var i = 0; i < searcherMocks.Count; i++)
            {
                var res = resultSet.Results.Single(r => r.Minimizer == searcherMocks[i].Object);
                Assert.That(res.Success, Is.True);
                Assert.That(res.Minimum, Is.EqualTo(new Point(i)));
                Assert.That(res.MinimumValue, Is.EqualTo(-1 * i));
            }

            foreach (var searcherMock in searcherMocks)
            {
                searcherMock.Verify(s => s.Search(funcMock.Object, startPoint, default(CancellationToken)));
            }            
        }

        [Test]
        public async Task MinimizeAsync_WhenAnySearchThrowsInvalidOperationException_ReturnsResult()
        {
            var searchStrategyMock = GetFailedSearchStrategyMock<InvalidOperationException>();

            var executor = new MinimizationMethodExecutor(new[] {searchStrategyMock.Object});

            var func = Mock.Of<IExecutableFunction>();
            var startPoint = new Point(0);
            var result = await executor.MinimizeAsync(func, startPoint);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Results, Is.Not.Null.And.Not.Contains(null));
            Assert.That(result.Results.Count, Is.EqualTo(1));

            var actualFirstResult = result.Results.First();
            Assert.That(actualFirstResult.Success, Is.False);
            Assert.That(actualFirstResult.Minimum, Is.Null);
            Assert.That(actualFirstResult.MinimumValue, Is.EqualTo(0));

            searchStrategyMock.Verify(s => s.Search(func, startPoint, default(CancellationToken)));
        }

        [Test]
        public void MinimizeAsync_WhenOneSearchThrowsUnexpectedException_ThrowsInvalidOperationException([Values(1, 3, 5)] int searchersCount)
        {
            var searcherMocks = Enumerable.Range(1, searchersCount).Select(i => GetFailedSearchStrategyMock<ArgumentException>()).ToList();

            var executor = new MinimizationMethodExecutor(searcherMocks.Select(s => s.Object));

            var func = Mock.Of<IExecutableFunction>();
            var startPoint = new Point(0);
            
            Assert.ThrowsAsync<InvalidOperationException>(async () => await executor.MinimizeAsync(func, startPoint));

            foreach (var searcherMock in searcherMocks)
            {
                searcherMock.Verify(s => s.Search(func, startPoint, default(CancellationToken)));
            }
        }

        private static Mock<IMultiDimensionalSearchStrategy> GetSearchStrategyMock(Point p)
        {
            if (p == null) throw new ArgumentNullException(nameof(p));

            var searchStrategyMock = new Mock<IMultiDimensionalSearchStrategy>();

            searchStrategyMock
                .Setup(s => s.Search(It.IsAny<IExecutableFunction>(), It.IsAny<Point>(), It.IsAny<CancellationToken>()))
                .Returns(p);

            return searchStrategyMock;
        }

        private static Mock<IMultiDimensionalSearchStrategy> GetFailedSearchStrategyMock<T>() where T: Exception, new()
        {
            var searchStrategyMock = new Mock<IMultiDimensionalSearchStrategy>();

            searchStrategyMock
                .Setup(s => s.Search(It.IsAny<IExecutableFunction>(), It.IsAny<Point>(), It.IsAny<CancellationToken>()))
                .Throws<T>();

            return searchStrategyMock;
        }
    }
}