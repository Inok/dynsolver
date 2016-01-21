using System;
using DynamicSolver.Abstractions.Expression;
using DynamicSolver.ExpressionParser.Expression;

namespace DynamicSolver.ExpressionParser.Parser
{
    public class ExpressionParser : IExpressionParser
    {
        public IStatement Parse(string inputExpression)
        {
            if (string.IsNullOrWhiteSpace(inputExpression)) throw new ArgumentException("Argument is null or empty", nameof(inputExpression));

            var lexer = new Lexer(inputExpression);
            var expression = ParseExpr(lexer);

            lexer.SkipLeadingWhitespaces();
            if (!lexer.IsEmpty)
            {
                throw new FormatException($"End of expression expected, but was {lexer.Input}");
            }

            return new Statement(expression);
        }

        private static IExpression ParseExpr(Lexer lexer)
        {
            return ParseMulDiv(lexer);
        }

        private static IExpression ParseMulDiv(Lexer lexer)
        {
            return ParsePow(lexer);
        }

        private static IExpression ParsePow(Lexer lexer)
        {
            return ParseFactor(lexer);
        }

        private static IExpression ParseFactor(Lexer lexer)
        {
            lexer.SkipLeadingWhitespaces();
            if (lexer.IsEmpty)
            {
                throw new FormatException("Primitive expected, but was <empty>");
            }

            if (lexer.Input[0] == '(')
            {
                lexer.Advance();
                lexer.SkipLeadingWhitespaces();

                var expr = ParseExpr(lexer);

                lexer.SkipLeadingWhitespaces();
                if (lexer.IsEmpty || lexer.Input[0] != ')')
                    throw new FormatException($"Closing parenthesis expected, but was {lexer.Input}");
                lexer.Advance();
                return expr;
            }

            if (lexer.Input[0] == '-')
            {
                lexer.Advance();

                lexer.SkipLeadingWhitespaces();
                if (!lexer.IsEmpty && lexer.Input[0] == '-')
                {
                    throw new FormatException($"Non-negated factor expected, but was {lexer.Input}");
                }

                var expr = ParseFactor(lexer);

                var numeric = expr as NumericPrimitive;
                if (numeric != null)
                {
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if (numeric.Value == 0)
                    {
                        throw new FormatException("Negated zero are not allowed as numeric value.");
                    }

                    if (numeric.Value > 0)
                    {
                        return new NumericPrimitive("-" + numeric.Token);
                    }
                }

                return new UnaryMinusOperator(expr);
            }
            
            return ParsePrimitive(lexer);
        }

        private static IExpression ParsePrimitive(Lexer lexer)
        {
            lexer.SkipLeadingWhitespaces();

            if (lexer.IsEmpty)
            {
                throw new FormatException("Primitive expected, but was <empty>");
            }

            if (char.IsDigit(lexer.Input[0]))
            {
                return lexer.ReadNumeric();
            }

            if (char.IsLetter(lexer.Input[0]))
            {
                return ParseIdentifier(lexer);
            }

            throw new FormatException($"Primitive cannot be parsed, starting at {lexer.Input}");
        }

        private static IExpression ParseIdentifier(Lexer lexer)
        {
            var identifier = lexer.ReadIdentifier();

            var constant = Constant.TryParse(identifier);
            if (constant != null)
            {
                return new ConstantPrimitive(constant);
            }

            if (!lexer.IsEmpty && lexer.Input[0] == '(')
            {
                lexer.Advance();
                var childExpr = ParseExpr(lexer);
                lexer.SkipLeadingWhitespaces();
                if (lexer.IsEmpty || lexer.Input[0] != ')')
                {
                    throw new FormatException($"Closing parenthesis expected, but was {lexer.Input}");
                }
                lexer.Advance();
                return new FunctionCall(identifier, childExpr);
            }

            return new VariablePrimitive(identifier);
        }
    }
}