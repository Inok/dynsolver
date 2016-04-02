using System;
using System.Collections.Generic;
using DynamicSolver.Abstractions.Expression;
using DynamicSolver.ExpressionParser.Expression;
using JetBrains.Annotations;

namespace DynamicSolver.ExpressionParser.Derivation
{
    public class SymbolicDerivationService
    {
        [NotNull]
        public IStatement GetDerivative([NotNull] IStatement statement, [NotNull] string respectToVariableName)
        {
            if (statement == null) throw new ArgumentNullException(nameof(statement));
            if (string.IsNullOrWhiteSpace(respectToVariableName))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(respectToVariableName));
            }

            return new Statement(GetDerivative(statement.Expression, respectToVariableName));
        }

        [NotNull]
        private IExpression GetDerivative([NotNull] IExpression expression, [NotNull] string respectToVariableName)
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
                if (binary is AddBinaryOperator)
                {
                    return new AddBinaryOperator(GetDerivative(binary.LeftOperand, respectToVariableName), GetDerivative(binary.RightOperand, respectToVariableName));
                }

                if (binary is SubtractBinaryOperator)
                {
                    return new SubtractBinaryOperator(GetDerivative(binary.LeftOperand, respectToVariableName), GetDerivative(binary.RightOperand, respectToVariableName));
                }

                if (binary is MultiplyBinaryOperator)
                {
                    return new AddBinaryOperator(
                        new MultiplyBinaryOperator(GetDerivative(binary.LeftOperand, respectToVariableName), binary.RightOperand),
                        new MultiplyBinaryOperator(binary.LeftOperand, GetDerivative(binary.RightOperand, respectToVariableName)));
                }

                if (binary is DivideBinaryOperator)
                {
                    return new DivideBinaryOperator(
                        new SubtractBinaryOperator(
                            new MultiplyBinaryOperator(GetDerivative(binary.LeftOperand, respectToVariableName), binary.RightOperand),
                            new MultiplyBinaryOperator(binary.LeftOperand, GetDerivative(binary.RightOperand, respectToVariableName))),
                        new PowBinaryOperator(binary.RightOperand, new NumericPrimitive("2"))
                        );
                }

                if (binary is PowBinaryOperator)
                {
                    var firstTerm = new MultiplyBinaryOperator(GetDerivative(binary.LeftOperand, respectToVariableName), new DivideBinaryOperator(binary.RightOperand, binary.LeftOperand));
                    var secondTerm = new MultiplyBinaryOperator(GetDerivative(binary.RightOperand, respectToVariableName), new FunctionCall("ln", binary.LeftOperand));
                    var factor = new AddBinaryOperator(firstTerm, secondTerm);
                    return new MultiplyBinaryOperator(binary, factor);
                }

                throw new NotImplementedException();
            }

            var functionCall = expression as IFunctionCall;
            if (functionCall != null)
            {
                Func<IExpression, IExpression> derivative;
                if (!FunctionDerivatives.TryGetValue(functionCall.FunctionName, out derivative))
                {
                    throw new NotImplementedException($"Function derivative of {functionCall.FunctionName} is unknown");
                }

                return new MultiplyBinaryOperator(derivative(functionCall.Argument), GetDerivative(functionCall.Argument, respectToVariableName));
            }

            throw new NotImplementedException();
        }

        private static readonly Dictionary<string, Func<IExpression, IExpression>> FunctionDerivatives = new Dictionary<string, Func<IExpression, IExpression>>()
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