using System;
using System.Text;
using DynamicSolver.CoreMath.Collections;
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

            private readonly IndexedSet<Variable, string> _variables = new IndexedSet<Variable, string>();
            private int _latestGeneratedVariableIndex = 0;
            
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
                if (_variables.TryGetValueByItem1(variable, out string name))
                {
                    _builder.Append(name);
                    return;
                }

                var explicitName = variable.ExplicitName;
                if (explicitName != null)
                {
                    if (_variables.ContainsItem2(explicitName))
                    {
                        throw new InvalidOperationException(
                            $"Variable with explicit name '{explicitName}' conflicts with different variable with the same explicit or generated name.");
                    }
                    _variables.Add(variable, explicitName);
                    _builder.Append(explicitName);
                    return;
                }

                var generatedName = $"_gen${++_latestGeneratedVariableIndex}";
                if (_variables.ContainsItem2(generatedName))
                {
                    throw new InvalidOperationException(
                        $"Generated name '{generatedName}' conflicts with different variable with the same explicit name. Do not use explicit variable names like _gen${{number}}.");
                }

                _variables.Add(variable, generatedName);
                _builder.Append(generatedName);
            }

            protected override void Visit(MinusOperation minusOperation)
            {
                var shouldAddBracketsAroundOperand = (minusOperation.Operand as Constant)?.Value < 0
                                                     || (minusOperation.Operand is MinusOperation);

                _builder.Append("-");

                if (shouldAddBracketsAroundOperand)
                {
                    _builder.Append("(");
                }
                
                minusOperation.Operand.Accept(this);
                
                if (shouldAddBracketsAroundOperand)
                {
                    _builder.Append(")");
                }
            }

            protected override void Visit(AddOperation addOperation)
            {
                _builder.Append("(");
                addOperation.Left.Accept(this);
                _builder.Append(" + ");
                addOperation.Right.Accept(this);
                _builder.Append(")");
            }

            protected override void Visit(SubtractOperation subtractOperation)
            {
                _builder.Append("(");
                subtractOperation.Left.Accept(this);
                _builder.Append(" - ");
                subtractOperation.Right.Accept(this);
                _builder.Append(")");
            }

            protected override void Visit(MultiplyOperation multiplyOperation)
            {
                _builder.Append("(");
                multiplyOperation.Left.Accept(this);
                _builder.Append(" * ");
                multiplyOperation.Right.Accept(this);
                _builder.Append(")");
            }

            protected override void Visit(DivideOperation divideOperation)
            {
                _builder.Append("(");
                divideOperation.Left.Accept(this);
                _builder.Append(" / ");
                divideOperation.Right.Accept(this);
                _builder.Append(")");
            }

            protected override void Visit(PowOperation powOperation)
            {
                _builder.Append("(");
                powOperation.Value.Accept(this);
                _builder.Append(" ^ ");
                powOperation.Power.Accept(this);
                _builder.Append(")");
            }

            protected override void Visit(FunctionCallOperation functionCallOperation)
            {
                switch (functionCallOperation.Function)
                {
                    case Function.Sin:
                        _builder.Append("sin");
                        break;
                    case Function.Cos:
                        _builder.Append("cos");
                        break;
                    case Function.Tg:
                        _builder.Append("tg");
                        break;
                    case Function.Ctg:
                        _builder.Append("ctg");
                        break;
                    case Function.Ln:
                        _builder.Append("ln");
                        break;
                    case Function.Lg:
                        _builder.Append("lg");
                        break;
                    case Function.Exp:
                        _builder.Append("exp");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                _builder.Append("(");
                _builder.Append(functionCallOperation.Argument.Accept(this));
                _builder.Append(")");
            }

            protected override void Visit(AssignStatement assignStatement)
            {
                assignStatement.Target.Accept(this);
                _builder.Append(" := ");
                assignStatement.Source.Accept(this);
            }
        }
    }
}