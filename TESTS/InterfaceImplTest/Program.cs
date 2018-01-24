// (c) Nick Polyak 2018 - http://awebpros.com/
// License: Apache License 2.0 (http://www.apache.org/licenses/LICENSE-2.0.html)
//
// short overview of copyright rules:
// 1. you can use this framework in any commercial or non-commercial 
//    product as long as you retain this copyright message
// 2. Do not blame the author of this software if something goes wrong. 
// 
// Also, please, mention this software in any documentation for the 
// products that use it.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;
using NP.Roxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TestProj;


namespace InterfaceImplTest
{
    class Program
    {
        static void Main(string[] args)
        {
            MSBuildWorkspace workspace = MSBuildWorkspace.Create();

            Project proj = workspace.OpenProjectAsync("../../InterfaceImplTest.csproj").Result;
            //Project proj = workspace.OpenProjectAsync("../../../TestProj/TestProj.csproj").Result;

            //Assembly testProjAssembly = Assembly.ReflectionOnlyLoadFrom("TestProj.dll");

            //AssemblyMetadata assemblyMetadata =
            //    Microsoft.CodeAnalysis.AssemblyMetadata.CreateFromFile("TestProj.dll");

            //AssemblyIdentity assemblyIdentity = AssemblyIdentity.FromAssemblyDefinition(testProjAssembly);

            Compilation compilation = proj.GetCompilationAsync().Result;

            INamedTypeSymbol myInterface =
                compilation.GetTypeByMetadataName($"InterfaceImplTest.{nameof(MyInterfaceToTestCircularReference)}");
            //INamedTypeSymbol myInterface =
            //    compilation.GetTypeByMetadataName($"TestProj.{nameof(IMyInterface)}");

            IAssemblySymbol assembly = myInterface.ContainingAssembly;

            ISourceAssemblySymbol sourceAssemblySymbol = assembly as ISourceAssemblySymbol;

            IAssemblySymbol assemblySymbol = assembly as IAssemblySymbol;

            IEnumerable<MetadataReference> references =
                sourceAssemblySymbol
                    .Compilation
                    .References
                    .Concat
                    (
                        new[] 
                        {
                            sourceAssemblySymbol
                                .Compilation
                                .ToMetadataReference()
                        }).ToArray();

            IPropertySymbol intProp = 
                myInterface
                    .GetMemberByName<IPropertySymbol>(nameof(IMyInterface.TheInt));
            IPropertySymbol strProp = 
                myInterface
                    .GetMemberByName<IPropertySymbol>(nameof(IMyInterface.TheStr));

            RoslynCodeBuilder roslynCodeBuilder = new RoslynCodeBuilder();

            RoslynAnalysisAndGenerationUtils.TheNamespaces = roslynCodeBuilder.AllNamespaces;
            RoslynAnalysisAndGenerationUtils.TheAssemblies = roslynCodeBuilder.AllAssemblies;

            roslynCodeBuilder.AddNamespace("NP.Generated");
            roslynCodeBuilder.AddClass("MyClass1", null, null, myInterface);

            roslynCodeBuilder.AddAutoProp(intProp.Name, intProp.Type as INamedTypeSymbol);
            roslynCodeBuilder.AddAutoProp(strProp.Name, strProp.Type as INamedTypeSymbol);

            roslynCodeBuilder.PopAll();

            string classStr = roslynCodeBuilder.ToString();
        }
    }
}
