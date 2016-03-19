using System;
using DynamicSolver.Abstractions.Expression;
using DynamicSolver.ExpressionParser.Expression;
using JetBrains.Annotations;

namespace DynamicSolver.ExpressionParser.Tools
{
    public class ExpressionVisitor
    {
        [NotNull]
        private readonly IExpression _expression;

        public ExpressionVisitor([NotNull] IExpression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            _expression = expression;
        }

        public void Visit()
        {
            Visit(_expression);
        }

        private void Visit([NotNull] IExpression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            TryRaise(VisitAnyNode, expression);

            if (TryRaise(VisitBinaryOperator, expression as IBinaryOperator))
            {
                TryRaise(VisitAssignmentBinaryOperator, expression as AssignmentBinaryOperator);
                TryRaise(VisitAddBinaryOperator, expression as AddBinaryOperator);
                TryRaise(VisitSubtractBinaryOperator, expression as SubtractBinaryOperator);
                TryRaise(VisitMultiplyBinaryOperator, expression as MultiplyBinaryOperator);
                TryRaise(VisitDivideBinaryOperator, expression as DivideBinaryOperator);
                TryRaise(VisitPowBinaryOperator, expression as PowBinaryOperator);

                var binary = (IBinaryOperator) expression;
                Visit(binary.LeftOperand);
                Visit(binary.RightOperand);
                return;
            }

            if (TryRaise(VisitUnaryOperator, expression as IUnaryOperator))
            {
                TryRaise(VisitUnaryMinusOperator, expression as UnaryMinusOperator);
                TryRaise(VisitDeriveUnaryOperator, expression as DeriveUnaryOperator);

                var unary = (IUnaryOperator)expression;
                Visit(unary.Operand);
                return;
            }


            if (TryRaise(VisitPrimitive, expression as IPrimitive))
            {
                TryRaise(VisitConstantPrimitive, expression as ConstantPrimitive);
                TryRaise(VisitVariablePrimitive, expression as VariablePrimitive);
                TryRaise(VisitNumericPrimitive, expression as NumericPrimitive);
                return;
            }

            if (TryRaise(VisitFunctionCall, expression as IFunctionCall))
            {
                Visit(((IFunctionCall)expression).Argument);
            }
        }

        [ContractAnnotation("arg:null => false; arg:notnull => true")]
        private bool TryRaise<T>([CanBeNull] EventHandler<T> handler, T arg)
        {
            if (arg == null)
            {
                return false;
            }

            handler?.Invoke(this, arg);
            return true;
        }

        public event EventHandler<IExpression> VisitAnyNode;

        public event EventHandler<IBinaryOperator> VisitBinaryOperator;
        public event EventHandler<AssignmentBinaryOperator> VisitAssignmentBinaryOperator;
        public event EventHandler<AddBinaryOperator> VisitAddBinaryOperator;
        public event EventHandler<SubtractBinaryOperator> VisitSubtractBinaryOperator;
        public event EventHandler<MultiplyBinaryOperator> VisitMultiplyBinaryOperator;
        public event EventHandler<DivideBinaryOperator> VisitDivideBinaryOperator;
        public event EventHandler<PowBinaryOperator> VisitPowBinaryOperator;
        
        public event EventHandler<IUnaryOperator> VisitUnaryOperator;
        public event EventHandler<UnaryMinusOperator> VisitUnaryMinusOperator;
        public event EventHandler<DeriveUnaryOperator> VisitDeriveUnaryOperator;

        public event EventHandler<IPrimitive> VisitPrimitive;
        public event EventHandler<VariablePrimitive> VisitVariablePrimitive;
        public event EventHandler<ConstantPrimitive> VisitConstantPrimitive;
        public event EventHandler<NumericPrimitive> VisitNumericPrimitive;

        public event EventHandler<IFunctionCall> VisitFunctionCall;
    }
}