using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading;
using DynamicSolver.Abstractions;
using DynamicSolver.LinearAlgebra;
using JetBrains.Annotations;

namespace DynamicSolver.Minimizer.MultiDimensionalSearch
{
    public class GeneticMethod : IMultiDimensionalSearchStrategy
    {
        [NotNull] private readonly GeneticSearchSettings _settings;
        [NotNull] private readonly Random _random;

        public GeneticMethod([NotNull] GeneticSearchSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            _settings = settings;
            _random = new Random(Guid.NewGuid().GetHashCode());
        }

        public Point Search(IExecutableFunction function, Point startPoint, CancellationToken token = default(CancellationToken))
        {
            if (function == null) throw new ArgumentNullException(nameof(function));
            if (startPoint == null) throw new ArgumentNullException(nameof(startPoint));
            if (function.OrderedArguments.Count != startPoint.Dimension)
            {
                throw new ArgumentException("Argument count and point dimension mismatch.");
            }

            var population = CreatePopulation(startPoint);
            
            var limiter = new IterationLimiter(new IterationLimitSettings(_settings.IterationCount, false));
            do
            {
                token.ThrowIfCancellationRequested();
                limiter.NextIteration();

                SortPopulationAsc(function, population);

                if (limiter.ShouldInterrupt)
                {
                    return population[0];
                }

                var parentPool = Selection(population);

                var crossedPool = Cross(parentPool);

                Inverse(crossedPool);

                population = crossedPool;
            }
            while (true);
        }

        private void SortPopulationAsc(IExecutableFunction function, List<Point> population)
        {
            population.Sort((p1, p2) => function.Execute(p1.ToArray()).CompareTo(function.Execute(p2.ToArray())));
        }

        private List<Point> CreatePopulation(Point startPoint)
        {
            var populationSize = _settings.PopulationSize;
            var result = new List<Point>(populationSize);

            for (var i = 0; i < populationSize; i++)
            {
                result.Add(new Point(startPoint.Select(v => v + (-0.5d + _random.NextDouble())*Math.Max(Math.Abs(v), 1)*100).ToArray()));
            }

            return result;
        }

        private List<Point> Selection(List<Point> population)
        {
            var populationSize = _settings.PopulationSize;
            var result = new List<Point>(populationSize);

            for (var i = 0; i < populationSize; i++)
            {
                int t = int.MaxValue;
                for (var ti = 0; ti < _settings.TournamentSelectionSize; ti++)
                {
                    t = Math.Min(t, _random.Next(0, population.Count - 1));
                }
                result.Add(population[t]);
            }

            return result;
        }

        private List<Point> Cross(List<Point> parentPool)
        {
            var populationSize = _settings.PopulationSize;
            var dimension = parentPool[0].Dimension;

            var result = new List<Point>(populationSize);

            while (result.Count < populationSize)
            {
                var parent1 = parentPool[_random.Next(0, parentPool.Count - 1)];
                var parent2 = parentPool[_random.Next(0, parentPool.Count - 1)];

                var child1 = new double[dimension];
                var child2 = new double[dimension];

                if (_random.NextDouble() <= _settings.CrossingChance)
                {
                    const double lambda = 0.4;
                    for (var i = 0; i < dimension; i++)
                    {
                        child1[i] = lambda*parent1[i] + (1 - lambda)*parent2[i];
                        child2[i] = lambda*parent2[i] + (1 - lambda)*parent1[i];

                        if (_random.NextDouble() <= _settings.MutationChance)
                        {
                            child1[i] = child1[i] + (-0.5 + _random.NextDouble());
                        }

                        if (_random.NextDouble() <= _settings.MutationChance)
                        {
                            child2[i] = child2[i] + (-0.5 + _random.NextDouble());
                        }
                    }
                }

                if(result.Count < populationSize)
                {
                    result.Add(new Point(child1));
                }

                if(result.Count < populationSize)
                {
                    result.Add(new Point(child2));
                }
            }

            return result;
        }

        private void Inverse(List<Point> pool)
        {
            for (int i = 0; i < pool.Count; i++)
            {
                if (_random.NextDouble() <= _settings.InversionChance)
                {
                    var point = pool[i].ToArray();
                    var gene = _random.Next(0, point.Length - 1);
                    double inverted = 0;
                    do
                    {
                        var bits = BitConverter.DoubleToInt64Bits(point[gene]);
                        bits ^= 1 << _random.Next(0, sizeof(long)*8 - 1);
                        inverted = BitConverter.Int64BitsToDouble(bits);
                    } while (double.IsInfinity(inverted) || double.IsNaN(inverted));
                    point[gene] = inverted;
                    pool[i] = new Point(point);
                }
            }
        }
    }
}