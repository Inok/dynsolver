using System;
using System.Collections.Generic;
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
        private readonly IDictionary<string, string> _systemFunctions = new Dictionary<string, string>()
        {
            {"pi", "double pi => Math.PI;"},
            {"sin", "double sin(double arg) => Math.Sin(arg);"},
            {"cos", "double cos(double arg) => Math.Cos(arg);"}
        };


        public IFunction Compile(string expression, string[] allowedArguments)
        {
            if (allowedArguments.Distinct().Count() != allowedArguments.Length)
                throw new ArgumentException("Array has multiple arguments with same value.");
            
            if(allowedArguments.Any(arg => _systemFunctions.ContainsKey(arg)))
                throw new ArgumentException("Argument has name equal to system function.");

            var functions = string.Join(Environment.NewLine, _systemFunctions.Values);
            var arguments = string.Join(Environment.NewLine, allowedArguments.Select((arg, i) => $@"double {arg} = args[i];"));

            var sourceTemplate = CSharpSyntaxTree.ParseText(
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
                assemblyName, new[] {sourceTemplate}, references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (var ms = new MemoryStream())
            {
                var result = compilation.Emit(ms);

                if (!result.Success)
                {
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