using DynamicSolver.Core.Semantic.Model.Array;
using DynamicSolver.Core.Semantic.Model.Operation;
using DynamicSolver.Core.Semantic.Model.Statement;
using DynamicSolver.Core.Semantic.Model.Struct;
using DynamicSolver.Core.Semantic.Model.Value;

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
        T Visit(MathFunctionCallOperation mathFunctionCallOperation);
        T Visit(ArrayAccessOperation arrayAccessOperation);
        T Visit(StructElementAccessOperation structElementAccessOperation);

        T Visit(AssignStatement assignStatement);
        T Visit(BlockStatement blockStatement);
        T Visit(RepeatStatement functionCallOperation);
        T Visit(ReturnStatement returnStatement);
        T Visit(YieldReturnStatement yieldReturnStatement);
    }
}