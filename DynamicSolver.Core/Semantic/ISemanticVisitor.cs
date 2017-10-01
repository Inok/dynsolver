using DynamicSolver.Core.Semantic.Model;

namespace DynamicSolver.Core.Semantic
{
    public interface ISemanticVisitor<out T>
    {
        T Visit(Constant constant);
        T Visit(Variable variable);
        T Visit(MinusOperation minusOperation);
        T Visit(AddOperation addOperation);
        T Visit(SubtractOperation subtractOperation);
        T Visit(MultiplyOperation multiplyOperation);
        T Visit(DivideOperation divideOperation);
        T Visit(PowOperation powOperation);
        T Visit(FunctionCallOperation functionCallOperation);
        T Visit(ArrayAccessOperation arrayAccessOperation);
        T Visit(StructElementAccessOperation functionCallOperation);

        T Visit(AssignStatement assignStatement);
        T Visit(BlockStatement blockStatement);
        T Visit(RepeatStatement functionCallOperation);
        T Visit(ReturnStatement returnStatement);
        T Visit(YieldReturnStatement yieldReturnStatement);
    }
}