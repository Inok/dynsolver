using System;
using DynamicSolver.Abstractions.Expression;
using DynamicSolver.Expressions.Expression;

namespace DynamicSolver.Expressions.Parser
{
    public class ExpressionParser : IExpressionParser
    {
        public IStatement Parse(string inputExpression)
        {
            if (string.IsNullOrWhiteSpace(inputExpression)) throw new ArgumentException("Argument is null or empty", nameof(inputExpression));

            var lexer = new Lexer(inputExpression);
            var expression = ParseAssignment(lexer);

            lexer.SkipLeadingWhitespaces();
            if (!lexer.IsEmpty)
            {
                throw new FormatException($"End of expression expected, but was {lexer.Input}");
            }

            return new Statement(expression);
        }

        private static IExpression ParseAssignment(Lexer lexer)
        {
            var expr = ParseAddSubtract(lexer);

            while (lexer.AdvanceToken('='))
            {
                var right = ParseAddSubtract(lexer);
                expr = new AssignmentBinaryOperator(expr, right);
            }

            return expr;
        }

        private static IExpression ParseAddSubtract(Lexer lexer)
        {
            var expr = ParseMulDiv(lexer);

            while (true)
            {
                if (lexer.AdvanceToken('+'))
                {
                    var right = ParseMulDiv(lexer);
                    expr = new AddBinaryOperator(expr, right);
                    continue;
                }

                if (lexer.AdvanceToken('-'))
                {
                    var right = ParseMulDiv(lexer);
                    expr = new SubtractBinaryOperator(expr, right);
                    continue;
                }
                break;
            }

            return expr;
        }

        private static IExpression ParseMulDiv(Lexer lexer)
        {
            var expr = ParsePow(lexer);

            while (true)
            {
                if (lexer.AdvanceToken('*'))
                {
                    var right = ParsePow(lexer);
                    expr = new MultiplyBinaryOperator(expr, right);
                    continue;
                }

                if (lexer.AdvanceToken('/'))
                {
                    var right = ParsePow(lexer);
                    expr = new DivideBinaryOperator(expr, right);
                    continue;
                }
                break;
            }

            return expr;
        }

        private static IExpression ParsePow(Lexer lexer)
        {
            var expr = ParseFactor(lexer);

            while (lexer.AdvanceToken('^'))
            {
                var right = ParseFactor(lexer);
                expr = new PowBinaryOperator(expr, right);
            }

            return expr;
        }

        private static IExpression ParseFactor(Lexer lexer)
        {
            lexer.SkipLeadingWhitespaces();
            if (lexer.IsEmpty)
            {
                throw new FormatException("Primitive expected, but was <empty>");
            }

            if (lexer.AdvanceToken('('))
            {
                lexer.SkipLeadingWhitespaces();

                var expr = ParseAssignment(lexer);

                lexer.SkipLeadingWhitespaces();
                if (!lexer.AdvanceToken(')'))
                {
                    throw new FormatException($"Closing parenthesis expected, but was {lexer.Input}");
                }
                return expr;
            }

            if (lexer.AdvanceToken('-'))
            {
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

            if (lexer.AdvanceToken('(', false))
            {
                var childExpr = ParseAssignment(lexer);

                lexer.SkipLeadingWhitespaces();
                if (!lexer.AdvanceToken(')'))
                {
                    throw new FormatException($"Closing parenthesis expected, but was {lexer.Input}");
                }

                return new FunctionCall(identifier, childExpr);
            }

            IExpression expr = new VariablePrimitive(identifier);

            while (lexer.AdvanceToken('\'', false))
            {
                expr = new DeriveUnaryOperator(expr);
            }

            return expr;
        }
    }
}