using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DynamicSolver.Abstractions;
using DynamicSolver.LinearAlgebra;

namespace DynamicSolver.Minimizer
{
    public class MinimizationMethodExecutor
    {
        private readonly IEnumerable<IMultiDimensionalSearchStrategy> _methods;

        public MinimizationMethodExecutor(IEnumerable<IMultiDimensionalSearchStrategy> methods)
        {
            _methods = methods;
        }

        public async Task<MinimizationResultSet> MinimizeAsync(IExecutableFunction function, Point startPoint, CancellationToken token = default(CancellationToken))
        {
            var tasks = _methods.Select(m => Task.Run(() => WrapMinimizationErrors(m, function, startPoint, token), token)).ToArray();

            var results = await Task.WhenAll(tasks);

            return new MinimizationResultSet(results);
        }

        private MinimizationResult WrapMinimizationErrors(IMultiDimensionalSearchStrategy searchStrategy, IExecutableFunction function, Point startPoint, CancellationToken token = default(CancellationToken))
        {
            try
            {
                var result = searchStrategy.Search(function, startPoint);
                return new MinimizationResult(searchStrategy, true, result, function.Execute(result.ToArray()));
            }
            catch (InvalidOperationException)
            {
                return new MinimizationResult(searchStrategy, false, null, 0);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Minimization failed by some reason. See inner exception for details.", ex);
            }
        }
    }
}