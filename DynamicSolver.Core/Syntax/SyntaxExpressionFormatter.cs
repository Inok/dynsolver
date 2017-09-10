using System;
using DynamicSolver.Core.Syntax.Model;
using JetBrains.Annotations;

namespace DynamicSolver.Core.Syntax
{
    public class SyntaxExpressionFormatter
    {
        private static readonly BinaryOperatorsPriorityComparer PriorityComparer = new BinaryOperatorsPriorityComparer();

        [NotNull]
        public string Format([NotNull] ISyntaxExpression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            return InternalFormat(expression);
        }

        private static string InternalFormat(ISyntaxExpression expression)
        {
            if (expression is NumericPrimitive numeric)
            {
                return FormatNode(numeric);
            }

            if (expression is ConstantPrimitive constant)
            {
                return FormatNode(constant);
            }

            if (expression is VariablePrimitive variable)
            {
                return FormatNode(variable);
            }

            if (expression is UnaryMinusOperator unaryMinus)
            {
                return FormatNode(unaryMinus);
            }

            if (expression is DeriveUnaryOperator derive)
            {
                return FormatNode(derive);
            }

            if (expression is IFunctionCall functionCall)
            {
                return FormatNode(functionCall);
            }

            if (expression is IBinaryOperator binary)
            {
                return FormatNode(binary);
            }

            throw new InvalidOperationException($"Formatter cannot process expression of type {expression.GetType()}");
        }


        private static string FormatNode(NumericPrimitive numeric) => numeric.Token;
        private static string FormatNode(ConstantPrimitive constant) => constant.Constant.Name;
        private static string FormatNode(VariablePrimitive variable) => variable.Name;

        private static string FormatNode(UnaryMinusOperator op) => op.Operand is IBinaryOperator ? $"-({InternalFormat(op.Operand)})" : $"-{InternalFormat(op.Operand)}";
        private static string FormatNode(DeriveUnaryOperator op) => op.Operand is IBinaryOperator ? $"({InternalFormat(op.Operand)})'" : $"{InternalFormat(op.Operand)}'";
        private static string FormatNode(IFunctionCall fun) => $"{fun.FunctionName}({InternalFormat(fun.Argument)})";

        private static string FormatNode(IBinaryOperator binary)
        {
            return $"{FormatBinary(binary, binary.LeftOperand)} {GetBinaryOperatorToken(binary)} {FormatBinary(binary, binary.RightOperand)}";
        }

        private static string FormatBinary(IBinaryOperator _this, ISyntaxExpression operand)
        {
            var binary = operand as IBinaryOperator;

            if (binary == null)
            {
                return InternalFormat(operand);
            }

            if (binary is PowBinaryOperator)
            {
                return $"({InternalFormat(operand)})";
            }

            var compare = PriorityComparer.Compare(_this, binary);
            if (compare > 0)
            {
                return $"({InternalFormat(operand)})";
            }

            if (compare == 0)
            {
                if (binary is DivideBinaryOperator)
                {
                    return $"({InternalFormat(operand)})";
                }

                if (binary is MultiplyBinaryOperator && !(_this is MultiplyBinaryOperator))
                {
                    return $"({InternalFormat(operand)})";
                }

                if (_this is SubtractBinaryOperator)
                {
                    return $"({InternalFormat(operand)})";
                }
            }

            return InternalFormat(operand);
        }

        private static string GetBinaryOperatorToken(IBinaryOperator binaryOperator)
        {
            switch (binaryOperator)
            {
                case AddBinaryOperator _: return "+";
                case SubtractBinaryOperator _: return "-";
                case MultiplyBinaryOperator _: return "*";
                case DivideBinaryOperator _: return "/";
                case PowBinaryOperator _: return "^";
                case AssignmentBinaryOperator _: return "=";
                default: throw new ArgumentOutOfRangeException(nameof(binaryOperator), binaryOperator.GetType().FullName, "OperatorToken is not found.");
            }
        }
    }
}