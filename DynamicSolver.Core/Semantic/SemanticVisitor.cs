using DynamicSolver.Core.Semantic.Model.Array;
using DynamicSolver.Core.Semantic.Model.Operation;
using DynamicSolver.Core.Semantic.Model.Statement;
using DynamicSolver.Core.Semantic.Model.Struct;
using DynamicSolver.Core.Semantic.Model.Value;

namespace DynamicSolver.Core.Semantic
{
    public abstract class SemanticVisitor : ISemanticVisitor<object>
    {
        protected abstract void Visit(IntegerConstant constant);
        protected abstract void Visit(RealConstant constant);
        protected abstract void Visit(Variable variable);
        protected abstract void Visit(MinusOperation minusOperation);
        protected abstract void Visit(AddOperation addOperation);
        protected abstract void Visit(SubtractOperation subtractOperation);
        protected abstract void Visit(MultiplyOperation multiplyOperation);
        protected abstract void Visit(DivideOperation divideOperation);
        protected abstract void Visit(PowOperation powOperation);
        protected abstract void Visit(MathFunctionCallOperation mathFunctionCallOperation);
        protected abstract void Visit(StructElementAccessOperation structElementAccessOperation);
        protected abstract void Visit(ArrayAccessOperation arrayAccessOperation);
        protected abstract void Visit(AssignStatement assignStatement);
        protected abstract void Visit(BlockStatement blockStatement);
        protected abstract void Visit(RepeatStatement repeatStatement);
        protected abstract void Visit(ReturnStatement returnStatement);
        protected abstract void Visit(YieldReturnStatement yieldReturnStatement);

        object ISemanticVisitor<object>.Visit(IntegerConstant constant)
        {
            Visit(constant);
            return null;
        }
        
        object ISemanticVisitor<object>.Visit(RealConstant constant)
        {
            Visit(constant);
            return null;
        }

        object ISemanticVisitor<object>.Visit(Variable variable)
        {
            Visit(variable);
            return null;
        }

        object ISemanticVisitor<object>.Visit(MinusOperation minusOperation)
        {
            Visit(minusOperation);
            return null;
        }

        object ISemanticVisitor<object>.Visit(AddOperation addOperation)
        {
            Visit(addOperation);
            return null;
        }

        object ISemanticVisitor<object>.Visit(SubtractOperation subtractOperation)
        {
            Visit(subtractOperation);
            return null;
        }

        object ISemanticVisitor<object>.Visit(MultiplyOperation multiplyOperation)
        {
            Visit(multiplyOperation);
            return null;
        }

        object ISemanticVisitor<object>.Visit(DivideOperation divideOperation)
        {
            Visit(divideOperation);
            return null;
        }

        object ISemanticVisitor<object>.Visit(PowOperation powOperation)
        {
            Visit(powOperation);
            return null;
        }

        object ISemanticVisitor<object>.Visit(MathFunctionCallOperation mathFunctionCallOperation)
        {
            Visit(mathFunctionCallOperation);
            return null;
        }

        object ISemanticVisitor<object>.Visit(ArrayAccessOperation arrayAccessOperation)
        {
            Visit(arrayAccessOperation);
            return null;
        }
        
        object ISemanticVisitor<object>.Visit(StructElementAccessOperation structElementAccessOperation)
        {
            Visit(structElementAccessOperation);
            return null;
        }

        object ISemanticVisitor<object>.Visit(AssignStatement assignStatement)
        {
            Visit(assignStatement);
            return null;
        }

        object ISemanticVisitor<object>.Visit(BlockStatement block)
        {
            Visit(block);
            return null;
        }

        object ISemanticVisitor<object>.Visit(RepeatStatement repeatStatement)
        {
            Visit(repeatStatement);
            return null;
        }

        object ISemanticVisitor<object>.Visit(ReturnStatement returnStatement)
        {
            Visit(returnStatement);
            return null;
        }

        object ISemanticVisitor<object>.Visit(YieldReturnStatement yieldReturnStatement)
        {
            Visit(yieldReturnStatement);
            return null;
        }
    }
}