// (c) Nick Polyak 2018 - http://awebpros.com/
// License: Apache License 2.0 (http://www.apache.org/licenses/LICENSE-2.0.html)
//
// short overview of copyright rules:
// 1. you can use this framework in any commercial or non-commercial 
//    product as long as you retain this copyright message
// 2. Do not blame the author(s) of this software if something goes wrong. 
// 
// Also, please, mention this software in any documentation for the 
// products that use it.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using NP.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NP.Roxy
{
    internal static class RoslynCompilationHelper
    {
        public static string GENERATED_ASSEMBLY_NAME { get; } = "NP.Generated";

        public static CSharpCompilationOptions TheAssemblyCompilationsOptions =>
            new CSharpCompilationOptions
                (
                    OutputKind.DynamicallyLinkedLibrary
                );

        public static CSharpCompilationOptions TheNetModuleCompilationOptions =>
            TheAssemblyCompilationsOptions.WithOutputKind(OutputKind.NetModule);

        public static CompilationResult CompileSyntaxTree
        (
            this SyntaxTree syntaxTree,
            string assemblyName,
            bool isModule = true,
            IEnumerable<MetadataReference> references = null
        )
        {
            IEnumerable<SyntaxTree> syntaxTrees = null;

            if (syntaxTree != null)
            {
                syntaxTrees = new[] { syntaxTree };
            }

            CSharpCompilationOptions compilationOptions =
                isModule ? TheNetModuleCompilationOptions : TheAssemblyCompilationsOptions;

            CSharpCompilation compilation = CSharpCompilation.Create
            (
                assemblyName,
                syntaxTrees: syntaxTrees,
                references: references,
                options: compilationOptions
            );

            return compilation.GetCompilationResult();
        }

        public static CompilationResult CompileCode(string code, string assemblyName)
        {
            SyntaxTree syntaxTree =
                CSharpSyntaxTree.ParseText(code);

            return CompileSyntaxTree(syntaxTree, assemblyName);
        }

        public static CompilationResult CompileCode
        (
            string code,
            string assemblyName,
            bool isModule = true,
            IEnumerable<MetadataReference> references = null
        )
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);

            return CompileSyntaxTree
            (
                syntaxTree,
                assemblyName,
                isModule,
                references
            );
        }

        public static CompilationResult GetEmptyCompilationWithModuleRefs
        (
            params CompilationResult[] moduleResults
        )
        {
            return CompileCode
            (
                string.Empty,
                GENERATED_ASSEMBLY_NAME,
                false,
                moduleResults
                    .Where(moduleResult => moduleResult.Success)
                    .Select(moduleResult => ModuleMetadata.CreateFromImage(moduleResult.TheResult).GetReference())
            );
        }


        public static CompilationResult CompileDocument(this Document doc)
        {
            if (doc == null)
                return null;

            SyntaxTree syntaxTree =
                doc.GetSyntaxTreeAsync().Result;

            CompilationResult result = syntaxTree.CompileSyntaxTree(doc.Name);

            return result;
        }

        public static CompilationResult
            GetCompilationResult(this Compilation compilation)
        {
            byte[] byteResult = null;
            IEnumerable<Diagnostic> errors = null;

            OutputKind outputKind =
                compilation.Options.OutputKind;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                //StopWatch.ResetStatic();
                EmitResult result = compilation.Emit(memoryStream);
                //StopWatch.PrintDifference("Emit Time: ");

                if (!result.Success)
                {
                    errors =
                        result.Diagnostics.Where(msg => msg.Severity == DiagnosticSeverity.Error);
                }

                byteResult = memoryStream.ToArray();
            }

            string name = null;

            if (outputKind == OutputKind.NetModule)
            {
                name = compilation.SourceModule.Name;
            }
            else
            {
                name = compilation.AssemblyName;
            }

            return new CompilationResult
            (
                name, 
                byteResult, 
                errors?.Select(error => error.GetMessage() + error.Location.ToString()).ToList()
            );
        }
    }
}
