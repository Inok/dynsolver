using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using DynamicSolver.Core.Collections;
using DynamicSolver.Core.Semantic.Model;
using DynamicSolver.Core.Semantic.Model.Array;
using DynamicSolver.Core.Semantic.Model.Operation;
using DynamicSolver.Core.Semantic.Model.Statement;
using DynamicSolver.Core.Semantic.Model.Struct;
using DynamicSolver.Core.Semantic.Model.Value;
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
            private readonly UniqueKeyValueSet<StructDefinition, IReadOnlyDictionary<StructElementDefinition, string>> _structElementNames = new UniqueKeyValueSet<StructDefinition, IReadOnlyDictionary<StructElementDefinition, string>>();
            
            private int _lastGeneratedNameIndex = 0;
            private int _deep = 0;

            public string GetSemanticString()
            {
                return _builder.ToString();
            }

            protected override void Visit(IntegerConstant constant)
            {
                _builder.Append(constant.Value.ToString("D"));
            }

            protected override void Visit(RealConstant constant)
            {
                _builder.Append(constant.Value.ToString("G"));
            }

            protected override void Visit(Variable variable)
            {
                var variableName = GetDeclarationName(variable);
                _builder.Append(variableName);
            }

            protected override void Visit(MinusOperation minusOperation)
            {
                var shouldAddBracketsAroundOperand = minusOperation.Operand is MinusOperation
                                                     || (minusOperation.Operand as IntegerConstant)?.Value < BigInteger.Zero
                                                     || (minusOperation.Operand as RealConstant)?.Value < 0;

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

            protected override void Visit(MathFunctionCallOperation mathFunctionCallOperation)
            {
                switch (mathFunctionCallOperation.MathFunction)
                {
                    case MathFunction.Sin:
                        _builder.Append("sin");
                        break;
                    case MathFunction.Cos:
                        _builder.Append("cos");
                        break;
                    case MathFunction.Tg:
                        _builder.Append("tg");
                        break;
                    case MathFunction.Ctg:
                        _builder.Append("ctg");
                        break;
                    case MathFunction.Ln:
                        _builder.Append("ln");
                        break;
                    case MathFunction.Lg:
                        _builder.Append("lg");
                        break;
                    case MathFunction.Exp:
                        _builder.Append("exp");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                _builder.Append("(");
                _builder.Append(mathFunctionCallOperation.Argument.Accept(this));
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
                var arrayName = GetDeclarationName(arrayAccessOperation.Array);
                _builder.Append(arrayName).Append("[").Append(arrayAccessOperation.Index).Append("]");
            }
            
            protected override void Visit(StructElementAccessOperation structElementAccessOperation)
            {
                var structName = GetDeclarationName(structElementAccessOperation.StructDeclaration);
                var elementName = GetStructElementName(structElementAccessOperation);
                _builder.Append(structName).Append(".").Append(elementName);
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
            
            protected override void Visit(ReturnStatement returnStatement)
            {
                _builder.Append("return ");
                returnStatement.Source.Accept(this);

            }
            
            protected override void Visit(YieldReturnStatement yieldReturnStatement)
            {
                _builder.Append("yield return ");
                yieldReturnStatement.Source.Accept(this);
            }

            private string GetDeclarationName(IDeclaration declaration)
            {
                if (_declarations.TryGetValueByItem1(declaration, out var name))
                {
                    return name;
                }

                var explicitName = declaration.ExplicitName;

                if (explicitName != null)
                {
                    if (_declarations.ContainsItem2(explicitName))
                    {
                        throw new InvalidOperationException($"Declaration with explicit name '{explicitName}' conflicts with different node with the same explicit or generated name.");
                    }

                    _declarations.Add(declaration, explicitName);
                    return explicitName;
                }

                var generatedName = GenerateName();
                _declarations.Add(declaration, generatedName);
                
                return generatedName;
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

            private string GetStructElementName(StructElementAccessOperation operation)
            {
                var structDefinition = operation.StructDeclaration.Definition;

                if (_structElementNames.TryGetValueByItem1(structDefinition, out var elementNames))
                {
                    return elementNames[operation.Element];
                }

                var names = new Dictionary<StructElementDefinition, string>();

                var genIndex = 0;
                foreach (var element in structDefinition.Elements)
                {
                    names.Add(element, element.ExplicitName ?? $"_gen${++genIndex}");
                }

                _structElementNames.Add(structDefinition, names);
                
                return names[operation.Element];
            }

            private void MoveToNewLine()
            {
                _builder.Append(Environment.NewLine);
                _builder.Append(INDENTATION_CHARACTER, _deep * INDENTATION_CHARACTERS_PER_LEVEL);
            }
        }
    }
}