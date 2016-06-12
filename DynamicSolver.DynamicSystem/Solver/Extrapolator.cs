using System;
using System.Collections.Generic;
using System.Linq;

namespace DynamicSolver.DynamicSystem.Solver
{
    public class Extrapolator
    {
        private readonly int _extrapolationStageCount;
        private readonly int[] _extrapolationCoefficients;

        private readonly double[,] _buffer;
        private readonly IReadOnlyDictionary<string, double>[] _solvesBuffer;
        private readonly int _baseMethodOrder;

        public Extrapolator(int extrapolationStageCount, int baseMethodOrder)
        {
            if (extrapolationStageCount <= 0) throw new ArgumentOutOfRangeException(nameof(extrapolationStageCount));
            if (baseMethodOrder <= 0) throw new ArgumentOutOfRangeException(nameof(baseMethodOrder));

            _extrapolationStageCount = extrapolationStageCount;
            _baseMethodOrder = baseMethodOrder;
            _extrapolationCoefficients = Enumerable.Range(1, extrapolationStageCount).ToArray();
            _buffer = new double[extrapolationStageCount,extrapolationStageCount];
            _solvesBuffer = new IReadOnlyDictionary<string, double>[extrapolationStageCount];
        }

        public IReadOnlyDictionary<string, double> Extrapolate(IReadOnlyDictionary<string, double> variables, double step, Func<IReadOnlyDictionary<string, double>, double, IReadOnlyDictionary<string, double>> getNextValues)
        {
            for (var i = 0; i < _solvesBuffer.Length; i++)
            {
                var newVariables = variables;
                for(int k = 0; k < _extrapolationCoefficients[i]; k++)
                {
                    newVariables = getNextValues(newVariables, step/_extrapolationCoefficients[i]);
                }
                _solvesBuffer[i] = newVariables;
            }

            var newValues = new Dictionary<string, double>();

            foreach (var variable in variables.Keys)
            {
                for (var j = 0; j < _extrapolationStageCount; j++)
                {
                    _buffer[j, 0] = _solvesBuffer[j][variable];

                    for (var k = 0; k < j; k++)
                    {
                        _buffer[j, k+1] = _buffer[j, k] + (_buffer[j, k] - _buffer[j - 1, k]) / (Math.Pow((double)_extrapolationCoefficients[j] /_extrapolationCoefficients[j - k - 1], _baseMethodOrder) - 1);
                    }
                }

                newValues[variable] = _buffer[_extrapolationStageCount - 1, _extrapolationStageCount - 1];
            }

            return newValues;
        }
    }
}