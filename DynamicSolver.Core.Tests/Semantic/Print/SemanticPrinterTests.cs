using System;
using System.Globalization;
using DynamicSolver.Core.Semantic.Model;
using DynamicSolver.Core.Semantic.Print;
using NUnit.Framework;

namespace DynamicSolver.Core.Tests.Semantic.Print
{
    [TestFixture]
    public class SemanticPrinterTests
    {
        private readonly SemanticPrinter _semanticPrinter = new SemanticPrinter();

        [SetUp]
        public void Setup()
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        }

        [TestCase(0d, "0")]
        [TestCase(1d, "1")]
        [TestCase(-42d, "-42")]
        [TestCase(1.23456789d, "1.23456789")]
        [TestCase(1.23456789123456789d, "1.23456789123457")]
        [TestCase(-987.654E-42, "-9.87654E-40")]
        [TestCase(1000E+20, "1E+23")]
        [TestCase(double.Epsilon, "4.94065645841247E-324")]
        [TestCase(double.MinValue, "-1.79769313486232E+308")]
        [TestCase(double.MaxValue, "1.79769313486232E+308")]
        [TestCase(Math.PI, "3.14159265358979")]
        [TestCase(Math.E, "2.71828182845905")]
        public void PrintElement_Constant_PrintsValue(double value, string expected)
        {
            var actual = _semanticPrinter.PrintElement(new Constant(value));

            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("x")]
        [TestCase("y")]
        [TestCase("t1")]
        public void PrintElement_Variable_WithExplicitName_PrintsName(string name)
        {
            var actual = _semanticPrinter.PrintElement(new Variable(name));

            Assert.That(actual, Is.EqualTo(name));
        }

        [Test]
        public void PrintElement_Variable_DifferentVariablesWithSameExplicitName_ThrowsInvalidOperationException()
        {
            var element = new AddOperation(new Variable("x"), new Variable("x"));
            Assert.That(() => _semanticPrinter.PrintElement(element), Throws.InvalidOperationException);
        }

        [Test]
        public void PrintElement_Variable_WithNoName_PrintsGeneratedName()
        {
            var actual = _semanticPrinter.PrintElement(new Variable());
            Assert.That(actual, Is.EqualTo("_gen$1"));
        }

        [Test]
        public void PrintElement_ManyVariables_WithNoName_PrintsDifferentNames()
        {
            var actual = _semanticPrinter.PrintElement(new AddOperation(new AddOperation(new Variable(), new Variable()), new Variable()));
            Assert.That(actual, Is.EqualTo("((_gen$1 + _gen$2) + _gen$3)"));
        }
        
        [Test]
        public void PrintElement_Variable_MultipleWithNoName_PrintsSameName()
        {
            var variable = new Variable();
            var actual = _semanticPrinter.PrintElement(new AddOperation(variable, variable));
            Assert.That(actual, Is.EqualTo("(_gen$1 + _gen$1)"));
        }

        [Test]
        public void PrintElement_MinusOperation_PrintsMinusOperator()
        {
            Assert.That(_semanticPrinter.PrintElement(new MinusOperation(new Constant(1))), Is.EqualTo("-1"));
            Assert.That(_semanticPrinter.PrintElement(new MinusOperation(new Constant(2))), Is.EqualTo("-2"));
            Assert.That(_semanticPrinter.PrintElement(new MinusOperation(new Constant(-1))), Is.EqualTo("-(-1)"));
            Assert.That(_semanticPrinter.PrintElement(new MinusOperation(new Variable("x"))), Is.EqualTo("-x"));
            Assert.That(_semanticPrinter.PrintElement(new MinusOperation(new MinusOperation(new Variable("x")))), Is.EqualTo("-(-x)"));
            Assert.That(_semanticPrinter.PrintElement(new MinusOperation(new AddOperation(new Variable("x"), new Variable("y")))), Is.EqualTo("-(x + y)"));
            Assert.That(_semanticPrinter.PrintElement(new SubtractOperation(new MinusOperation(new Variable("x")), new MinusOperation(new Variable("y")))), Is.EqualTo("(-x - -y)"));
        }

        [Test]
        public void PrintElement_AddOperation_PrintsAddOperator()
        {
            var actual = _semanticPrinter.PrintElement(new AddOperation(new Constant(1), new Constant(2)));
            Assert.That(actual, Is.EqualTo("(1 + 2)"));
        }

        [Test]
        public void PrintElement_AddOperation_Multiple_PrintsScoped()
        {
            var actual = _semanticPrinter.PrintElement(new AddOperation(
                new AddOperation(new Constant(1), new Constant(2)),
                new AddOperation(new Constant(3), new Constant(4))));
            Assert.That(actual, Is.EqualTo("((1 + 2) + (3 + 4))"));
        }
        
        [Test]
        public void PrintElement_SubtractOperation_PrintsSubtractOperator()
        {
            var actual = _semanticPrinter.PrintElement(new SubtractOperation(new Constant(1), new Constant(2)));
            Assert.That(actual, Is.EqualTo("(1 - 2)"));
        }

        [Test]
        public void PrintElement_SubtractOperation_Multiple_PrintsScoped()
        {
            var actual = _semanticPrinter.PrintElement(new SubtractOperation(
                new SubtractOperation(new Constant(1), new Constant(2)),
                new SubtractOperation(new Constant(3), new Constant(4))));
            Assert.That(actual, Is.EqualTo("((1 - 2) - (3 - 4))"));
        }
        
        [Test]
        public void PrintElement_MultiplyOperation_PrintsMultiplyOperator()
        {
            var actual = _semanticPrinter.PrintElement(new MultiplyOperation(new Constant(1), new Constant(2)));
            Assert.That(actual, Is.EqualTo("(1 * 2)"));
        }

        [Test]
        public void PrintElement_MultiplyOperation_Multiple_PrintsScoped()
        {
            var actual = _semanticPrinter.PrintElement(new MultiplyOperation(
                new MultiplyOperation(new Constant(1), new Constant(2)),
                new MultiplyOperation(new Constant(3), new Constant(4))));
            Assert.That(actual, Is.EqualTo("((1 * 2) * (3 * 4))"));
        }
        
        [Test]
        public void PrintElement_DivideOperation_PrintsDivideOperator()
        {
            var actual = _semanticPrinter.PrintElement(new DivideOperation(new Constant(1), new Constant(2)));
            Assert.That(actual, Is.EqualTo("(1 / 2)"));
        }

        [Test]
        public void PrintElement_DivideOperation_Multiple_PrintsScoped()
        {
            var actual = _semanticPrinter.PrintElement(new DivideOperation(
                new DivideOperation(new Constant(1), new Constant(2)),
                new DivideOperation(new Constant(3), new Constant(4))));
            Assert.That(actual, Is.EqualTo("((1 / 2) / (3 / 4))"));
        }
        
        [Test]
        public void PrintElement_PowOperation_PrintsPowOperator()
        {
            var actual = _semanticPrinter.PrintElement(new PowOperation(new Constant(1), new Constant(2)));
            Assert.That(actual, Is.EqualTo("(1 ^ 2)"));
        }

        [Test]
        public void PrintElement_PowOperation_Multiple_PrintsScoped()
        {
            var actual = _semanticPrinter.PrintElement(new PowOperation(
                new PowOperation(new Constant(1), new Constant(2)),
                new PowOperation(new Constant(3), new Constant(4))));
            Assert.That(actual, Is.EqualTo("((1 ^ 2) ^ (3 ^ 4))"));
        }

        [Test]
        public void PrintElement_FunctionCallOperation_PrintsFunctionWithCorrectName([Values] Function function)
        {
            var actual = _semanticPrinter.PrintElement(new FunctionCallOperation(function, new Constant(1)));
            Assert.That(actual, Is.EqualTo($"{function.ToString("G").ToLower()}(1)"));
        }
        
        [Test]
        public void PrintElement_AssignStatement_PrintsAssignStatement()
        {
            var actual = _semanticPrinter.PrintElement(new AssignStatement(new Variable("x"), new Constant(1)));
            Assert.That(actual, Is.EqualTo("x := 1"));
        }

        [Test]
        public void PrintElement_AssignStatement_WithComplexArgument_PrintsAssignStatement()
        {
            var actual = _semanticPrinter.PrintElement(new AssignStatement(
                new Variable("x"),
                new AddOperation(new Variable("y"), new Constant(1))
            ));
            Assert.That(actual, Is.EqualTo("x := (y + 1)"));
        }
        
        [Test]
        public void PrintElement_AssignStatement_WithSameSourceAndTarget_PrintsAssignStatement()
        {
            var variable = new Variable("x");
            var actual = _semanticPrinter.PrintElement(new AssignStatement(variable, variable));
            Assert.That(actual, Is.EqualTo("x := x"));
        }
        
        [Test]
        public void PrintElement_AssignStatement_WithTargetInsideSource_PrintsAssignStatement()
        {
            var variable = new Variable("x");
            var actual = _semanticPrinter.PrintElement(new AssignStatement(
                variable,
                new MultiplyOperation(
                    new AddOperation(variable, new Constant(1)),
                    variable)
            ));
            Assert.That(actual, Is.EqualTo("x := ((x + 1) * x)"));
        }

        [Test]
        public void PrintElement_BlockStatement_PrintsInnerStatementsByLines()
        {
            var var1 = new Variable();
            var var2 = new Variable();
            var statement = new BlockStatement(new[]
            {
                new AssignStatement(new Variable("b"), new Variable("a")),
                new AssignStatement(var1, new Variable("m")),
                new AssignStatement(new Variable("y"), new Variable("x")),
                new AssignStatement(var2, var1)
            });

            var actual = _semanticPrinter.PrintElement(statement);

            Assert.That(actual, Is.EqualTo($"b := a{Environment.NewLine}_gen$1 := m{Environment.NewLine}y := x{Environment.NewLine}_gen$2 := _gen$1"));
        }
        
        [TestCase("x")]
        [TestCase("y")]
        [TestCase("t1")]
        public void PrintElement_ArrayAccessOperation_WithExplicitName_PrintsName(string name)
        {
            var actual = _semanticPrinter.PrintElement(new ArrayAccessOperation(new ArrayDeclaration(name, 5), 2));

            Assert.That(actual, Is.EqualTo($"{name}[2]"));
        }

        [Test]
        public void PrintElement_ArrayAccessOperation_DifferentVariablesWithSameExplicitName_ThrowsInvalidOperationException()
        {
            var element = new AddOperation(new ArrayAccessOperation(new ArrayDeclaration("x", 5), 1), new ArrayAccessOperation(new ArrayDeclaration("x", 3), 0));
            Assert.That(() => _semanticPrinter.PrintElement(element), Throws.InvalidOperationException);
        }

        [Test]
        public void PrintElement_ArrayAccessOperation_WithNoName_PrintsGeneratedName()
        {
            var actual = _semanticPrinter.PrintElement(new ArrayAccessOperation(new ArrayDeclaration(5), 2));
            Assert.That(actual, Is.EqualTo("_gen$1[2]"));
        }

        [Test]
        public void PrintElement_ArrayAccessOperation_WithNoName_PrintsDifferentNames()
        {
            var arr1 = new ArrayAccessOperation(new ArrayDeclaration(2), 1);
            var arr2 = new ArrayAccessOperation(new ArrayDeclaration(1), 0);
            var arr3 = new ArrayAccessOperation(new ArrayDeclaration(3), 2);
            
            var actual = _semanticPrinter.PrintElement(new AddOperation(new AddOperation(arr1, arr2), arr3));
            Assert.That(actual, Is.EqualTo("((_gen$1[1] + _gen$2[0]) + _gen$3[2])"));
        }
        
        [Test]
        public void PrintElement_ArrayAccessOperation_MultipleWithExplicitName_PrintsSameName()
        {
            var arrayDeclaration = new ArrayDeclaration("x", 2);
            var arrAccess1 = new ArrayAccessOperation(arrayDeclaration, 0);
            var arrAccess2 = new ArrayAccessOperation(arrayDeclaration, 1);
            
            var actual = _semanticPrinter.PrintElement(new AddOperation(arrAccess1, arrAccess2));
            Assert.That(actual, Is.EqualTo("(x[0] + x[1])"));
        }
        
        [Test]
        public void PrintElement_ArrayAccessOperation_MultipleWithNoName_PrintsSameName()
        {
            var arrayDeclaration = new ArrayDeclaration(2);
            var arrAccess1 = new ArrayAccessOperation(arrayDeclaration, 0);
            var arrAccess2 = new ArrayAccessOperation(arrayDeclaration, 1);
            
            var actual = _semanticPrinter.PrintElement(new AddOperation(arrAccess1, arrAccess2));
            Assert.That(actual, Is.EqualTo("(_gen$1[0] + _gen$1[1])"));
        }
        
        [Test]
        public void PrintElement_MixedVariablesAndArrayAccessOperations_WithNoName_GeneratesValidNames()
        {
            var var1 = new Variable();
            var var2 = new Variable();
            var arr1 = new ArrayAccessOperation(new ArrayDeclaration(2), 0);
            var arr2 = new ArrayAccessOperation(new ArrayDeclaration(2), 1);

            var actual = _semanticPrinter.PrintElement(new MultiplyOperation(new AddOperation(arr1, var2), new SubtractOperation(var1, arr2)));
            Assert.That(actual, Is.EqualTo("((_gen$1[0] + _gen$2) * (_gen$3 - _gen$4[1]))"));
        }

        [Test]
        public void PrintElement_MixedVariablesAndArrayAccessOperations_WithSameExplicitName_Throws()
        {
            var var1 = new Variable("x");
            var arr1 = new ArrayAccessOperation(new ArrayDeclaration("x", 2), 0);

            var element = new AddOperation(var1, arr1);
            Assert.That(() => _semanticPrinter.PrintElement(element), Throws.InvalidOperationException);
        }
        
        [Test]
        public void PrintElement_RepeatStatement_Prints()
        {
            var varX = new Variable("x");
            var s1 = new AssignStatement(varX, new AddOperation(new Constant(1), new Constant(2)));
            var s2 = new AssignStatement(new Variable("y"), new MinusOperation(varX));
            var block = new BlockStatement(new[] {s1, s2});
            
            var repeatStatement = new RepeatStatement(3, block);
            
            var actual = _semanticPrinter.PrintElement(repeatStatement);
            Assert.That(actual, Is.EqualTo($"repeat '3' times {{{Environment.NewLine}  x := (1 + 2){Environment.NewLine}  y := -x{Environment.NewLine}}}"));
        }
        
        [Test]
        public void PrintElement_RepeatStatement_Nested_Prints()
        {
            var statement = new AssignStatement(new Variable("x"), new Constant(1));
            
            var innerRepeat = new RepeatStatement(3, statement);
            var rootRepeat = new RepeatStatement(5, innerRepeat);
            
            var actual = _semanticPrinter.PrintElement(rootRepeat);

            Assert.That(actual,
                Is.EqualTo(
                    $@"repeat '5' times {{{Environment.NewLine
                        }  repeat '3' times {{{Environment.NewLine
                        }    x := 1{Environment.NewLine
                        }  }}{Environment.NewLine
                        }}}"));
        }

        [Test]
        public void PrintElement_ReturnStatement_Prints()
        {
            var statement = new ReturnStatement(new AddOperation(new Variable("x"), new Constant(1)));

            var actual = _semanticPrinter.PrintElement(statement);

            Assert.That(actual, Is.EqualTo("return (x + 1)"));
        }

        [Test]
        public void PrintElement_YieldReturnStatement_Prints()
        {
            var statement = new YieldReturnStatement(new AddOperation(new Variable("x"), new Constant(1)));

            var actual = _semanticPrinter.PrintElement(statement);

            Assert.That(actual, Is.EqualTo("yield return (x + 1)"));
        }
    }
}