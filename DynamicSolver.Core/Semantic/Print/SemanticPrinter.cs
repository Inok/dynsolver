using System;
using System.Text;
using DynamicSolver.Core.Collections;
using DynamicSolver.Core.Semantic.Model;
using JetBrains.Annotations;

namespace DynamicSolver.Core.Semantic.Print
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
            private const int INDENTATION_CHARACTERS_PER_LEVEL = 2;
            private const char INDENTATION_CHARACTER = ' ';

            private readonly StringBuilder _builder = new StringBuilder();

            private readonly UniqueKeyValueSet<IDeclaration, string> _declarations = new UniqueKeyValueSet<IDeclaration, string>();
            private int _lastGeneratedNameIndex = 0;
            private int _deep = 0;

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
                if (_declarations.TryGetValueByItem1(variable, out var name))
                {
                    _builder.Append(name);
                    return;
                }

                var explicitName = variable.ExplicitName;
                if (explicitName != null)
                {
                    if (_declarations.ContainsItem2(explicitName))
                    {
                        throw new InvalidOperationException(
                            $"Variable with explicit name '{explicitName}' conflicts with different node with the same explicit or generated name.");
                    }
                    _declarations.Add(variable, explicitName);
                    _builder.Append(explicitName);
                    return;
                }

                var generatedName = GenerateName();
                _declarations.Add(variable, generatedName);
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

            protected override void Visit(BlockStatement blockStatement)
            {
                var i = 0;
                foreach (var innerStatement in blockStatement.Statements)
                {
                    if (i++ != 0)
                    {
                        MoveToNewLine();
                    }

                    innerStatement.Accept(this);
                }
            }

            protected override void Visit(ArrayAccessOperation arrayAccessOperation)
            {
                void AppendArrayAccess(string arrName)
                {
                    _builder.Append(arrName).Append("[").Append(arrayAccessOperation.Index).Append("]");
                }

                var arrayDeclaration = arrayAccessOperation.Array;
                
                if (_declarations.TryGetValueByItem1(arrayDeclaration, out var name))
                {
                    AppendArrayAccess(name);
                    return;
                }

                var explicitName = arrayDeclaration.ExplicitName;

                if (explicitName != null)
                {
                    if (_declarations.ContainsItem2(explicitName))
                    {
                        throw new InvalidOperationException(
                            $"Array with explicit name '{explicitName}' conflicts with different node with the same explicit or generated name.");
                    }

                    _declarations.Add(arrayDeclaration, explicitName);
                    AppendArrayAccess(explicitName);
                    return;
                }

                var generatedName = GenerateName();
                _declarations.Add(arrayDeclaration, generatedName);
                AppendArrayAccess(generatedName);
            }

            protected override void Visit(RepeatStatement repeatStatement)
            {
                _builder.Append("repeat '").Append(repeatStatement.RepeatsCount).Append("' times {");

                _deep++;
                MoveToNewLine();
                repeatStatement.Body.Accept(this);
                _deep--;

                MoveToNewLine();
                _builder.Append("}");
            }

            private string GenerateName()
            {
                var index = ++_lastGeneratedNameIndex;
                
                var name = $"_gen${index}";
                
                if (_declarations.ContainsItem2(name))
                {
                    throw new InvalidOperationException(
                        $"Generated name '{name}' conflicts with different node with the same explicit name. Do not use explicit variable names like _gen${index}.");
                }

                return name;
            }

            private void MoveToNewLine()
            {
                _builder.Append(Environment.NewLine);
                _builder.Append(INDENTATION_CHARACTER, _deep * INDENTATION_CHARACTERS_PER_LEVEL);
            }
        }
    }
}