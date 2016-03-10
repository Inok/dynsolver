using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DynamicSolver.Abstractions;
using DynamicSolver.LinearAlgebra;
using NUnit.Framework;

namespace DynamicSolver.Minimizer.Tests.MultiDimensionalSearch
{
    public abstract class MultiDimensionalMethodTests<T>
    {
        private class MockFunction : IExecutableFunction
        {
            private readonly Expression<Func<double[], double>> _func;
            public IReadOnlyCollection<string> OrderedArguments { get; }

            public double Execute(double[] arguments)
            {
                return _func.Compile()(arguments);
            }

            public MockFunction(Expression<Func<double[], double>> func, IReadOnlyCollection<string> orderedArguments)
            {
                _func = func;
                OrderedArguments = orderedArguments;
            }

            public double Execute(IReadOnlyDictionary<string, double> arguments) => Execute(OrderedArguments.Select(a => arguments[a]).ToArray());

            public override string ToString()
            {
                return _func.ToString();
            }
        }

        // ReSharper disable once StaticMemberInGenericType
        protected static ICollection<SearchMethodTestCase> TestCases { get; } = new List<SearchMethodTestCase>();

        protected IMultiDimensionalSearchStrategy SearchStrategy { get; set; }
        protected double Accuracy { get; set; }

        [SetUp]
        public abstract void Setup();

        static MultiDimensionalMethodTests()
        {
            TestCases.Add(new SearchMethodTestCase
            {
                Function = new MockFunction(x => x[0]*x[0] + x[1]*x[1] - x[0]*x[1] + x[1], new[] {"x1", "x2"}),
                StartPoint = new Point(5, 0),
                ExpectedResultPoint = new Point(-1.0/3, -2.0/3)
            });

            TestCases.Add(new SearchMethodTestCase
            {
                Function = new MockFunction(x => x[0]*x[0] + 3*x[1]*x[1] + 3*x[0]*x[1] + x[0], new[] {"x1", "x2"}),
                StartPoint = new Point(1, -2),
                ExpectedResultPoint = new Point(-2, 1)
            });

            TestCases.Add(new SearchMethodTestCase
            {
                Function = new MockFunction(x => Math.Pow(x[1] - x[0], 2) + Math.Pow(1 - x[0], 2), new[] {"x1", "x2"}),
                StartPoint = new Point(-7, 3),
                ExpectedResultPoint = new Point(1, 1)
            });

            TestCases.Add(new SearchMethodTestCase
            {
                Function = new MockFunction(x => 2 * x[0] * x[0] + 2 * x[1] * x[1] - 2 * x[0] * x[1] - 6 * x[0] - 6, new[] {"x1", "x2"}),
                StartPoint = new Point(0, 2),
                ExpectedResultPoint = new Point(2, 1)
            });

            TestCases.Add(new SearchMethodTestCase
            {
                Function = new MockFunction(x => Math.Pow(1 - x[0], 2) + 100 * Math.Pow(x[1] - x[0] * x[0], 2), new[] {"x1", "x2"}),
                StartPoint = new Point(0, 0),
                ExpectedResultPoint = new Point(1, 1)
            });
        }
        
        [TestCaseSource(nameof(TestCases))]
        public void Search_FoundsMinimumWithinAccuracy(SearchMethodTestCase testCase)
        {
            var actual = SearchStrategy.Search(testCase.Function, testCase.StartPoint);

            // Допустимая точность повышается на два порядка по причине того, что часть методов производят оценку точности 
            // по производным или очередному шагу, и требуемая точность фактически может быть так и не достигнута.
            Assert.That(new Vector(actual, testCase.ExpectedResultPoint), Has.Property(nameof(Vector.Length)).LessThanOrEqualTo(Accuracy * 100));
        }

        [TestCaseSource(nameof(TestCases))]
        public void Search_WhenStartsExactlyFromMinimumPoint_FoundsMinimumWithinAccuracy(SearchMethodTestCase testCase)
        {
            var actual = SearchStrategy.Search(testCase.Function, testCase.ExpectedResultPoint);
            Assert.That(new Vector(actual, testCase.ExpectedResultPoint), Has.Property(nameof(Vector.Length)).LessThanOrEqualTo(Accuracy));
        }
    }
}