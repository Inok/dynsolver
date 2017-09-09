using System;

namespace DynamicSolver.CoreMath.Syntax.Parser
{
    public class ExpressionParser : IExpressionParser
    {
        public ISyntaxExpression Parse(string inputExpression)
        {
            if (string.IsNullOrWhiteSpace(inputExpression)) throw new ArgumentException("Argument is null or empty", nameof(inputExpression));

            var lexer = new Lexer(inputExpression);
            var expression = ParseAssignment(lexer);

            lexer.SkipLeadingWhitespaces();
            if (!lexer.IsEmpty)
            {
                throw new FormatException($"End of expression expected, but was {lexer.Input}");
            }

            return expression;
        }

        private static ISyntaxExpression ParseAssignment(Lexer lexer)
        {
            var expr = ParseAddSubtract(lexer);

            while (lexer.AdvanceToken('='))
            {
                var right = ParseAddSubtract(lexer);
                expr = new AssignmentBinaryOperator(expr, right);
            }

            return expr;
        }

        private static ISyntaxExpression ParseAddSubtract(Lexer lexer)
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

        private static ISyntaxExpression ParseMulDiv(Lexer lexer)
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

        private static ISyntaxExpression ParsePow(Lexer lexer)
        {
            var expr = ParseFactor(lexer);

            while (lexer.AdvanceToken('^'))
            {
                var right = ParseFactor(lexer);
                expr = new PowBinaryOperator(expr, right);
            }

            return expr;
        }

        private static ISyntaxExpression ParseFactor(Lexer lexer)
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

        private static ISyntaxExpression ParsePrimitive(Lexer lexer)
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

        private static ISyntaxExpression ParseIdentifier(Lexer lexer)
        {
            var identifier = lexer.ReadIdentifier();

            var constant = Constant.TryParse(identifier);
            if (constant != null)
            {
                return new ConstantPrimitive(constant);
            }

            if (lexer.AdvanceToken('(', false))
            {
                var childExpr = ParseAddSubtract(lexer);

                lexer.SkipLeadingWhitespaces();
                if (!lexer.AdvanceToken(')'))
                {
                    throw new FormatException($"Closing parenthesis expected, but was {lexer.Input}");
                }

                return new FunctionCall(identifier, childExpr);
            }

            ISyntaxExpression expr = new VariablePrimitive(identifier);

            while (lexer.AdvanceToken('\'', false))
            {
                expr = new DeriveUnaryOperator(expr);
            }

            return expr;
        }
    }
}