using System;
using JetBrains.Annotations;

namespace DynamicSolver.CoreMath.Expression
{
    public class ExpressionFormatter
    {
        private static readonly BinaryOperatorsPriorityComparer PriorityComparer = new BinaryOperatorsPriorityComparer();

        [NotNull]
        public string Format([NotNull] IExpression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            return InternalFormat(expression);
        }

        private static string InternalFormat(IExpression expression)
        {
            var numeric = expression as NumericPrimitive;
            if (numeric != null)
            {
                return FormatNode(numeric);
            }

            var constant = expression as ConstantPrimitive;
            if (constant != null)
            {
                return FormatNode(constant);
            }

            var variable = expression as VariablePrimitive;
            if (variable != null)
            {
                return FormatNode(variable);
            }

            var unaryMinus = expression as UnaryMinusOperator;
            if (unaryMinus != null)
            {
                return FormatNode(unaryMinus);
            }

            var derive = expression as DeriveUnaryOperator;
            if (derive != null)
            {
                return FormatNode(derive);
            }

            var functionCall = expression as IFunctionCall;
            if (functionCall != null)
            {
                return FormatNode(functionCall);
            }

            var binary = expression as IBinaryOperator;
            if (binary != null)
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
            return $"{FormatBinary(binary, binary.LeftOperand)} {binary.OperatorToken} {FormatBinary(binary, binary.RightOperand)}";
        }

        private static string FormatBinary(IBinaryOperator _this, IExpression operand)
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
    }
}