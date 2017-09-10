using System;
using System.Text.RegularExpressions;
using DynamicSolver.Core.Syntax.Model;

namespace DynamicSolver.Core.Syntax.Parser
{
    public class Lexer
    {
        public string Input { get; private set; }

        public bool IsEmpty => Input.Length == 0;
        
        public Lexer(string input)
        {
            Input = input;
        }

        public void SkipLeadingWhitespaces()
        {
            Input = Input.Trim();
        }

        public void Advance()
        {
            if (IsEmpty)
            {
                return;
            }

            Input = Input.Substring(1);
        }

        public bool AdvanceToken(char token, bool skipLeadingWhitespaces = true)
        {
            if(skipLeadingWhitespaces)
            {
                SkipLeadingWhitespaces();
            }

            if (IsEmpty)
            {
                return false;
            }
            
            if (Input[0] == token)
            {
                Advance();
                return true;
            }

            return false;
        }

        public NumericPrimitive ReadNumeric()
        {
            var number = string.Empty;

            if (!IsEmpty && Input[0] == '-')
            {
                number += Input[0];
                Advance();
            }

            number += ReadInteger();

            if(!IsEmpty && (Input[0] == '.' || Input[0] == ','))
            {
                number += '.';
                Advance();

                number += ReadInteger();
            }

            return new NumericPrimitive(number);
        }

        private string ReadInteger()
        {
            if (IsEmpty)
            {
                throw new FormatException("Number expected, but was <empty>");
            }

            if (!char.IsDigit(Input[0]))
            {
                throw new FormatException($"Number expected, but was '{Input[0]}'");
            }

            var number = string.Empty;
            do
            {
                number += Input[0];
                Advance();
            } while (!IsEmpty && char.IsDigit(Input[0]));

            return number;
        }

        public string ReadIdentifier()
        {
            SkipLeadingWhitespaces();

            if (IsEmpty)
            {
                throw new FormatException("Identifier expected, but was <empty>");
            }

            if (!Regex.IsMatch(Input.Substring(0, 1), "[a-zA-Z]"))
            {
                throw new FormatException($"Letter expected, but was '{Input[0]}'");
            }

            var identifier = string.Empty;
            do
            {
                identifier += Input[0];
                Advance();
            } while (!IsEmpty && Regex.IsMatch(Input.Substring(0, 1), @"[a-zA-Z0-9_]"));

            return identifier;
        }

        public override string ToString()
        {
            return Input;
        }
    }
}