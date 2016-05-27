﻿using System;
using DynamicSolver.Abstractions.Expression;
using DynamicSolver.Expressions.Expression;
using NUnit.Framework;

namespace DynamicSolver.Expressions.Tests.Expression
{
    [TestFixture]
    public class ExpressionPrintingTests
    {
        [Test]
        public void ComplexStatement_ToString()
        {
            IStatement statement = new Statement(
                new AssignmentBinaryOperator(
                    new VariablePrimitive("y"),
                    new DivideBinaryOperator(
                        new FunctionCall("cos",
                            new MultiplyBinaryOperator(
                                new UnaryMinusOperator(new VariablePrimitive("x")),
                                new ConstantPrimitive(Constant.Pi))),
                        new UnaryMinusOperator(
                            new PowBinaryOperator(
                                new SubtractBinaryOperator(
                                    new NumericPrimitive("-1.5"),
                                    new FunctionCall("ln", new VariablePrimitive("x"))),
                                new NumericPrimitive("2")))
                        )));

            Assert.That(statement.ToString(), Is.EqualTo("y = cos(-x * pi) / -((-1.5 - ln(x)) ^ 2)"));
        }

        [TestCase( typeof(AssignmentBinaryOperator),    typeof(AddBinaryOperator),       false)]
        [TestCase( typeof(AddBinaryOperator),      typeof(AddBinaryOperator),       false)]
        [TestCase( typeof(SubtractBinaryOperator), typeof(AddBinaryOperator),       false)]
        [TestCase( typeof(MultiplyBinaryOperator), typeof(AddBinaryOperator),       true )]
        [TestCase( typeof(DivideBinaryOperator),   typeof(AddBinaryOperator),       true )]
        [TestCase( typeof(PowBinaryOperator),      typeof(AddBinaryOperator),       true )]
                   
        [TestCase( typeof(AssignmentBinaryOperator),    typeof(SubtractBinaryOperator),  false)]
        [TestCase( typeof(AddBinaryOperator),      typeof(SubtractBinaryOperator),  false)]
        [TestCase( typeof(SubtractBinaryOperator), typeof(SubtractBinaryOperator),  false)]
        [TestCase( typeof(MultiplyBinaryOperator), typeof(SubtractBinaryOperator),  true )]
        [TestCase( typeof(DivideBinaryOperator),   typeof(SubtractBinaryOperator),  true )]
        [TestCase( typeof(PowBinaryOperator),      typeof(SubtractBinaryOperator),  true )]
                   
        [TestCase( typeof(AssignmentBinaryOperator),    typeof(MultiplyBinaryOperator),  false)]
        [TestCase( typeof(AddBinaryOperator),      typeof(MultiplyBinaryOperator),  false)]
        [TestCase( typeof(SubtractBinaryOperator), typeof(MultiplyBinaryOperator),  false)]
        [TestCase( typeof(MultiplyBinaryOperator), typeof(MultiplyBinaryOperator),  false)]
        [TestCase( typeof(DivideBinaryOperator),   typeof(MultiplyBinaryOperator),  true )]
        [TestCase( typeof(PowBinaryOperator),      typeof(MultiplyBinaryOperator),  true )]
                   
        [TestCase( typeof(AssignmentBinaryOperator),    typeof(DivideBinaryOperator),    false)]
        [TestCase( typeof(AddBinaryOperator),      typeof(DivideBinaryOperator),    false)]
        [TestCase( typeof(SubtractBinaryOperator), typeof(DivideBinaryOperator),    false)]
        [TestCase( typeof(MultiplyBinaryOperator), typeof(DivideBinaryOperator),    true )]
        [TestCase( typeof(DivideBinaryOperator),   typeof(DivideBinaryOperator),    true )]
        [TestCase( typeof(PowBinaryOperator),      typeof(DivideBinaryOperator),    true )]
                   
        [TestCase( typeof(AssignmentBinaryOperator),    typeof(PowBinaryOperator),       true)]
        [TestCase( typeof(AddBinaryOperator),      typeof(PowBinaryOperator),       true)]
        [TestCase( typeof(SubtractBinaryOperator), typeof(PowBinaryOperator),       true)]
        [TestCase( typeof(MultiplyBinaryOperator), typeof(PowBinaryOperator),       true )]
        [TestCase( typeof(DivideBinaryOperator),   typeof(PowBinaryOperator),       true )]
        [TestCase( typeof(PowBinaryOperator),      typeof(PowBinaryOperator),       true )]

        public void BinaryOperators_NotWrapsToParenthesesWhenHasNoParent(Type parentType, Type childType, bool addParentheses)
        {
            var child = (IBinaryOperator) Activator.CreateInstance(childType, new NumericPrimitive("1"), new NumericPrimitive("2"));
            var parent = (IBinaryOperator) Activator.CreateInstance(parentType, child, new NumericPrimitive("3"));
            var statement = new Statement(parent);

            var expected = string.Format(addParentheses ? "(1 {0} 2) {1} 3" : "1 {0} 2 {1} 3", child.OperatorToken, parent.OperatorToken);

            Assert.That(statement.ToString(), Is.EqualTo(expected));
            
            //trace
            Console.WriteLine("Expected: " + expected);
        }
    }
}