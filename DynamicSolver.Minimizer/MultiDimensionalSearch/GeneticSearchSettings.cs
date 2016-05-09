using System;
using JetBrains.Annotations;

namespace DynamicSolver.Minimizer.MultiDimensionalSearch
{
    public class GeneticSearchSettings
    {
        [NotNull]
        public static readonly GeneticSearchSettings Default = new GeneticSearchSettings(100, 50, 3, 0.8, 0.03, 0.03);

        public int IterationCount { get; }
        public int PopulationSize { get; }
        public int TournamentSelectionSize { get; }
        public double CrossingChance { get; }
        public double MutationChance { get; }
        public double InversionChance { get; }

        public GeneticSearchSettings(int iterationCount, int populationSize, int tournamentSelectionSize, double crossingChance, double mutationChance, double inversionChance)
        {
            if (iterationCount <= 0) throw new ArgumentOutOfRangeException(nameof(iterationCount));
            if (populationSize <= 0) throw new ArgumentOutOfRangeException(nameof(populationSize));
            if (tournamentSelectionSize <= 0) throw new ArgumentOutOfRangeException(nameof(tournamentSelectionSize));
            if (crossingChance < 0 || crossingChance > 1) throw new ArgumentOutOfRangeException(nameof(crossingChance));
            if (mutationChance < 0 || mutationChance > 1) throw new ArgumentOutOfRangeException(nameof(mutationChance));
            if (inversionChance < 0 | inversionChance > 1) throw new ArgumentOutOfRangeException(nameof(inversionChance));

            IterationCount = iterationCount;
            PopulationSize = populationSize;
            TournamentSelectionSize = tournamentSelectionSize;
            CrossingChance = crossingChance;
            MutationChance = mutationChance;
            InversionChance = inversionChance;
        }
    }
}