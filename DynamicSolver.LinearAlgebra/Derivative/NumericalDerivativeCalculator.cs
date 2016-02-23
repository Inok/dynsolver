using System;
using System.Linq;
using DynamicSolver.Abstractions;
using JetBrains.Annotations;

namespace DynamicSolver.LinearAlgebra.Derivative
{
    public class NumericalDerivativeCalculator : IDerivativeCalculationStrategy
    {
        [NotNull]
        private readonly DerivativeCalculationSettings _settings;

        public NumericalDerivativeCalculator([NotNull] DerivativeCalculationSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            _settings = settings;
        }

        public Vector Derivative(IExecutableFunction function, Point point)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));
            if (point == null) throw new ArgumentNullException(nameof(point));
            if(function.OrderedArguments.Count != point.Dimension)
                throw new ArgumentException($"Function has {function.OrderedArguments.Count} arguments, but point has dimension of {point.Dimension}");

            double[] result = new double[point.Dimension];

            var tmp = point.ToArray();

            switch (_settings.Mode)
            {
                case DerivativeMode.Auto:
                case DerivativeMode.Center:
                    for (var i = 0; i < point.Dimension; i++)
                    {
                        tmp[i] = point[i] - _settings.Increment/2;
                        var left = function.Execute(tmp);
                        tmp[i] = point[i] + _settings.Increment/2;
                        var right = function.Execute(tmp);
                        result[i] = (right - left)/_settings.Increment;

                        tmp[i] = point[i];
                    }
                    break;
                case DerivativeMode.Left:
                    for (var i = 0; i < point.Dimension; i++)
                    {
                        var left = function.Execute(tmp);
                        tmp[i] = point[i] + _settings.Increment;
                        var right = function.Execute(tmp);
                        result[i] = (right - left) / _settings.Increment;

                        tmp[i] = point[i];
                    }
                    break;
                case DerivativeMode.Right:
                    for (var i = 0; i < point.Dimension; i++)
                    {
                        var right = function.Execute(tmp);
                        tmp[i] = point[i] - _settings.Increment;
                        var left = function.Execute(tmp);
                        result[i] = (right - left) / _settings.Increment;

                        tmp[i] = point[i];
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return new Vector(result);
        }

        public double DerivativeByDirection(IExecutableFunction function, Point point, Vector direction)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));
            if (point == null) throw new ArgumentNullException(nameof(point));
            if (direction == null) throw new ArgumentNullException(nameof(direction));
            if (function.OrderedArguments.Count != point.Dimension)
                throw new ArgumentException($"Function has {function.OrderedArguments.Count} arguments, but point has dimension of {point.Dimension}");
            if (function.OrderedArguments.Count != direction.Dimension)
                throw new ArgumentException($"Function has {function.OrderedArguments.Count} arguments, but direction has dimension of {direction.Dimension}");

            Point left = point, right = point;

            switch (_settings.Mode)
            {
                case DerivativeMode.Auto:
                case DerivativeMode.Center:
                    left = point.Move(direction, -_settings.Increment / 2);
                    right = point.Move(direction, _settings.Increment / 2);
                    break;
                case DerivativeMode.Left:
                    right = point.Move(direction, _settings.Increment);
                    break;
                case DerivativeMode.Right:
                    left = point.Move(direction, -_settings.Increment);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return (function.Execute(right.ToArray()) - function.Execute(left.ToArray()))/_settings.Increment;
        }
    }
}