using System;
using System.Collections.Generic;
using System.Linq;
using DynamicSolver.CoreMath.Analysis;
using DynamicSolver.CoreMath.Expression;
using JetBrains.Annotations;

namespace DynamicSolver.DynamicSystem
{
    public class DerivativeAnalyzer
    {
        [NotNull] private readonly IExpression _expression;

        private VariableDerivative _derivative;
        private bool? _isVariableDerivative;
        private bool? _hasVariableDerivativesOnly;

        public bool IsVariableDerivative
        {
            get
            {
                if (_isVariableDerivative != null)
                {
                    return _isVariableDerivative.Value;
                }

                _derivative = GetVariableDerivative(_expression);

                return (_isVariableDerivative = _derivative != null).Value;
            }
        }

        public bool HasVariableDerivativesOnly
        {
            get
            {
                if (_hasVariableDerivativesOnly.HasValue)
                {
                    return _hasVariableDerivativesOnly.Value;
                }

                var success = true;
                var visitor = new ExpressionVisitor(_expression);
                visitor.VisitDeriveUnaryOperator += (_, op) => success = success && (op.Operand is DeriveUnaryOperator || op.Operand is VariablePrimitive);
                visitor.Visit();

                return (_hasVariableDerivativesOnly = success).Value;
            }
        }

        public DerivativeAnalyzer([NotNull] IExpression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            _expression = expression;
        }

        public VariableDerivative AsVariableDerivative()
        {
            return IsVariableDerivative ? _derivative : null;
        }

        public IReadOnlyCollection<VariableDerivative> AllVariableDerivatives()
        {
            if (!HasVariableDerivativesOnly)
            {
                throw new NotImplementedException();
            }

            return EnumerateVariableDerivatives(_expression).Distinct().ToList();
        }


        private static IEnumerable<VariableDerivative> EnumerateVariableDerivatives(IExpression expression)
        {
            var derivative = GetVariableDerivative(expression);
            if (derivative != null)
            {
                yield return derivative;
                yield break;
            }

            var binary = expression as IBinaryOperator;
            if (binary != null)
            {
                foreach (var d in EnumerateVariableDerivatives(binary.LeftOperand).Concat(EnumerateVariableDerivatives(binary.RightOperand)))
                {
                    yield return d;

                }
            }

            var unary = expression as IUnaryOperator;
            if (unary != null)
            {
                foreach (var d in EnumerateVariableDerivatives(unary.Operand))
                {
                    yield return d;

                }
            }

            var functionCall = expression as IFunctionCall;
            if (functionCall != null)
            {
                foreach (var d in EnumerateVariableDerivatives(functionCall.Argument))
                {
                    yield return d;
                }
            }
        }


        private static VariableDerivative GetVariableDerivative(IExpression expression)
        {
            int order = 0;

            while (expression is DeriveUnaryOperator)
            {
                expression = (expression as DeriveUnaryOperator).Operand;
                order++;
            }

            var variable = expression as VariablePrimitive;

            if (order == 0 || variable == null)
            {
                return null;
            }

            return new VariableDerivative(variable, order);
        }
    }
}