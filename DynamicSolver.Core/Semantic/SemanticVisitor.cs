using DynamicSolver.Core.Semantic.Model;

namespace DynamicSolver.Core.Semantic
{
    public abstract class SemanticVisitor : ISemanticVisitor<object>
    {
        protected abstract void Visit(Constant constant);
        protected abstract void Visit(Variable variable);
        protected abstract void Visit(MinusOperation minusOperation);
        protected abstract void Visit(AddOperation addOperation);
        protected abstract void Visit(SubtractOperation subtractOperation);
        protected abstract void Visit(MultiplyOperation multiplyOperation);
        protected abstract void Visit(DivideOperation divideOperation);
        protected abstract void Visit(PowOperation powOperation);
        protected abstract void Visit(FunctionCallOperation functionCallOperation);
        protected abstract void Visit(AssignStatement assignStatement);
        protected abstract void Visit(BlockStatement blockStatement);
        protected abstract void Visit(ArrayAccessOperation arrayAccessOperation);
        protected abstract void Visit(RepeatStatement repeatStatement);

        object ISemanticVisitor<object>.Visit(Constant constant)
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

        object ISemanticVisitor<object>.Visit(FunctionCallOperation functionCallOperation)
        {
            Visit(functionCallOperation);
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
        
        object ISemanticVisitor<object>.Visit(ArrayAccessOperation arrayAccessOperation)
        {
            Visit(arrayAccessOperation);
            return null;
        }
        
        object ISemanticVisitor<object>.Visit(RepeatStatement repeatStatement)
        {
            Visit(repeatStatement);
            return null;
        }
    }
}