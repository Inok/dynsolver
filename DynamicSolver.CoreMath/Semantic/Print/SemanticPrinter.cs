using System;
using System.Text;
using DynamicSolver.CoreMath.Semantic.Model;
using JetBrains.Annotations;

namespace DynamicSolver.CoreMath.Semantic.Print
{
    public class SemanticPrinter
    {
        public string PrintElement([NotNull] ISemanticElement semanticElement)
        {
            if (semanticElement == null) throw new ArgumentNullException(nameof(semanticElement));

            var printVisitor = new PrintSemanticVisitor();

            semanticElement.Accept(printVisitor);

            return printVisitor.GetSemanticString();
        }

        private class PrintSemanticVisitor : SemanticVisitor
        {
            private readonly StringBuilder _builder = new StringBuilder();

            public string GetSemanticString()
            {
                return _builder.ToString();
            }

            protected override void Visit(Constant constant)
            {
                _builder.Append(constant.Value.ToString("G"));
            }

            protected override void Visit(Variable variable)
            {
                throw new NotImplementedException();
            }

            protected override void Visit(MinusOperation minusOperation)
            {
                throw new NotImplementedException();
            }

            protected override void Visit(AddOperation addOperation)
            {
                throw new NotImplementedException();
            }

            protected override void Visit(SubtractOperation subtractOperation)
            {
                throw new NotImplementedException();
            }

            protected override void Visit(MultiplyOperation multiplyOperation)
            {
                throw new NotImplementedException();
            }

            protected override void Visit(DivideOperation divideOperation)
            {
                throw new NotImplementedException();
            }

            protected override void Visit(PowOperation powOperation)
            {
                throw new NotImplementedException();
            }

            protected override void Visit(FunctionCallOperation functionCallOperation)
            {
                throw new NotImplementedException();
            }

            protected override void Visit(AssignStatement assignStatement)
            {
                throw new NotImplementedException();
            }
        }
    }
}