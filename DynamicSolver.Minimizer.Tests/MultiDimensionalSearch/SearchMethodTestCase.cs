using DynamicSolver.Abstractions;
using DynamicSolver.LinearAlgebra;

namespace DynamicSolver.Minimizer.Tests.MultiDimensionalSearch
{
    public class SearchMethodTestCase
    {
        public IExecutableFunction Function { get; set; }
        public Point StartPoint { get; set; }
        public Point ExpectedResultPoint { get; set; }

        public override string ToString()
        {
            return $"Function: {Function}, StartPoint: {StartPoint}, ExpectedResultPoint: {ExpectedResultPoint}";
        }
    }
}