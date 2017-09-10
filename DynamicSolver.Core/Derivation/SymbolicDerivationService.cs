using System;
using System.Collections.Generic;
using System.Linq;
using DynamicSolver.Core.Syntax;
using DynamicSolver.Core.Syntax.Model;
using JetBrains.Annotations;

namespace DynamicSolver.Core.Derivation
{
    public class SymbolicDerivationService
    {
        [NotNull]
        public ISyntaxExpression GetDerivative([NotNull] ISyntaxExpression expression, [NotNull] string respectToVariableName)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            if (string.IsNullOrWhiteSpace(respectToVariableName))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(respectToVariableName));
            }

            if (expression is IPrimitive)
            {
                if ((expression as VariablePrimitive)?.Name == respectToVariableName)
                {
                    return new NumericPrimitive("1");
                }
                if(expression is ConstantPrimitive || expression is NumericPrimitive || expression is VariablePrimitive)
                {
                    return new NumericPrimitive("0");
                }

                throw new NotImplementedException();
            }

            var unaryOperator = expression as IUnaryOperator;
            if (unaryOperator != null)
            {
                if(unaryOperator is UnaryMinusOperator)
                {
                    return new UnaryMinusOperator(GetDerivative(unaryOperator.Operand, respectToVariableName));
                }

                throw new NotImplementedException();
            }

            var binary = expression as IBinaryOperator;
            if (binary != null)
            {
                var leftDerivative = GetDerivative(binary.LeftOperand, respectToVariableName);
                var rightDerivative = GetDerivative(binary.RightOperand, respectToVariableName);

                if (binary is AddBinaryOperator)
                {
                    return new AddBinaryOperator(leftDerivative, rightDerivative);
                }

                if (binary is SubtractBinaryOperator)
                {
                    return new SubtractBinaryOperator(leftDerivative, rightDerivative);
                }

                if (binary is MultiplyBinaryOperator)
                {
                    return new AddBinaryOperator(
                        new MultiplyBinaryOperator(leftDerivative, binary.RightOperand),
                        new MultiplyBinaryOperator(binary.LeftOperand, rightDerivative));
                }

                if (binary is DivideBinaryOperator)
                {
                    return new DivideBinaryOperator(
                        new SubtractBinaryOperator(
                            new MultiplyBinaryOperator(leftDerivative, binary.RightOperand),
                            new MultiplyBinaryOperator(binary.LeftOperand, rightDerivative)
                        ),
                        new PowBinaryOperator(binary.RightOperand, new NumericPrimitive("2"))
                    );
                }

                if (binary is PowBinaryOperator)
                {
                    if (!IsDependOnRespected(binary.RightOperand, respectToVariableName))
                    {
                        return new MultiplyBinaryOperator(
                            binary.RightOperand,
                            new PowBinaryOperator(binary.LeftOperand, new SubtractBinaryOperator(binary.RightOperand, new NumericPrimitive("1")))
                        );
                    }

                    if (!IsDependOnRespected(binary.LeftOperand, respectToVariableName))
                    {
                        return new MultiplyBinaryOperator(
                            binary,
                            new FunctionCall("ln", binary.LeftOperand)
                        );
                    }

                    var firstTerm = new MultiplyBinaryOperator(leftDerivative, new DivideBinaryOperator(binary.RightOperand, binary.LeftOperand));
                    var secondTerm = new MultiplyBinaryOperator(rightDerivative, new FunctionCall("ln", binary.LeftOperand));
                    var factor = new AddBinaryOperator(firstTerm, secondTerm);
                    return new MultiplyBinaryOperator(binary, factor);
                }

                throw new NotImplementedException();
            }

            var functionCall = expression as IFunctionCall;
            if (functionCall != null)
            {
                Func<ISyntaxExpression, ISyntaxExpression> derivative;
                if (!FunctionDerivatives.TryGetValue(functionCall.FunctionName, out derivative))
                {
                    throw new NotImplementedException($"Function derivative of {functionCall.FunctionName} is unknown");
                }

                return new MultiplyBinaryOperator(derivative(functionCall.Argument), GetDerivative(functionCall.Argument, respectToVariableName));
            }

            throw new NotImplementedException();
        }

        private bool IsDependOnRespected(ISyntaxExpression expression, string respectedTo)
        {
            if (expression is ConstantPrimitive || expression is NumericPrimitive)
            {
                return false;
            }

            var variablePrimitive = expression as VariablePrimitive;
            if (variablePrimitive != null && variablePrimitive.Name != respectedTo)
            {
                return false;
            }

            return new SyntaxExpressionAnalyzer(expression).Variables.Contains(respectedTo);
        }

        private static readonly Dictionary<string, Func<ISyntaxExpression, ISyntaxExpression>> FunctionDerivatives = new Dictionary<string, Func<ISyntaxExpression, ISyntaxExpression>>()
        {
            ["sin"] = e => new FunctionCall("cos", e),
            ["cos"] = e => new UnaryMinusOperator(new FunctionCall("sin", e)),
            ["tg"] = e => new DivideBinaryOperator(new NumericPrimitive("1"), new PowBinaryOperator(new FunctionCall("cos", e), new NumericPrimitive("2"))),
            ["ctg"] = e => new DivideBinaryOperator(new NumericPrimitive("-1"), new PowBinaryOperator(new FunctionCall("sin", e), new NumericPrimitive("2"))),
            ["ln"] = e => new DivideBinaryOperator(new NumericPrimitive("1"), e),
            ["lg"] = e => new DivideBinaryOperator(new NumericPrimitive("1"), new MultiplyBinaryOperator(e, new FunctionCall("ln", new NumericPrimitive("10")))),
            ["exp"] = e => new FunctionCall("exp", e),
        };
    }
}