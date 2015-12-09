using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using DynamicSolver.Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DynamicSolver.ExpressionCompiler
{
    public class ExpressionCompiler : IExpressionCompiler
    {
        private static readonly IReadOnlyDictionary<string, string> SystemFunctions = new Dictionary<string, string>()
        {
            {"e", "double e => Math.E;"},
            {"pi", "double pi => Math.PI;"},
            {"sin", "double sin(double arg) => Math.Sin(arg);"},
            {"cos", "double cos(double arg) => Math.Cos(arg);"},
            {"tg", "double tg(double arg) => Math.Tan(arg);"},
            {"ctg", "double ctg(double arg) => 1.0/Math.Tan(arg);"},
            {"exp", "double exp(double arg) => Math.Exp(arg);"},
            {"pow", "double pow(double arg, double pow) => Math.Pow(arg, pow);"}
        };
        
        public IFunction Compile(string expression, string[] allowedArguments)
        {
            if (string.IsNullOrWhiteSpace(expression))
                throw new ArgumentException("Expression is null or empty.");

            if (allowedArguments.Any(string.IsNullOrWhiteSpace))
                throw new ArgumentException("Array has arguments with null or whitespace value.");

            if (allowedArguments.Distinct().Count() != allowedArguments.Length)
                throw new ArgumentException("Array has multiple arguments with same value.");
            
            if(allowedArguments.Any(arg => SystemFunctions.ContainsKey(arg)))
                throw new ArgumentException("Argument has name equal to system function.");

            var functions = string.Join(Environment.NewLine, SystemFunctions.Values);
            var arguments = string.Join(Environment.NewLine, allowedArguments.Select((arg, i) => $@"double {arg} = args[{i}];"));

            var sourceSyntaxTree = CSharpSyntaxTree.ParseText(
$@"using System;
namespace DynamicSolver.DynamicFunctionCompilation {{
public class FunctionContainer : {typeof (IFunction).FullName} {{
{functions}
public double Execute(double[] args) {{
{arguments}
return {expression};
}}
}}
}}");
            var assemblyName = Path.GetRandomFileName();
            var references = new MetadataReference[]
            {
                MetadataReference.CreateFromFile(typeof (object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof (IFunction).Assembly.Location)
            };

            var compilation = CSharpCompilation.Create(
                assemblyName, new[] {sourceSyntaxTree}, references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (var ms = new MemoryStream())
            {
                var result = compilation.Emit(ms);

                if (!result.Success)
                {
                    Console.Error.WriteLine(sourceSyntaxTree.GetText().ToString());
                    var failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);

                    var diagnostics = failures.Select(f => $"{f.Id}: {f.GetMessage()}").ToList();

                    foreach (var diagnostic in diagnostics)
                    {
                        Console.Error.WriteLine(diagnostic);
                    }

                    throw new InvalidOperationException($"Compilation failed: {Environment.NewLine}" + string.Join(Environment.NewLine, diagnostics));
                }

                ms.Seek(0, SeekOrigin.Begin);
                var assembly = Assembly.Load(ms.ToArray());

                var obj = (IFunction) Activator.CreateInstance(assembly.GetType("DynamicSolver.DynamicFunctionCompilation.FunctionContainer"));
                
                return obj;
            }


        }
    }
}