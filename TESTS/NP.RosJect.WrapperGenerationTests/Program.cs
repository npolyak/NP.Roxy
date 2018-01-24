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
using System.Text;
using System.Threading.Tasks;
using TestProj;

namespace NP.RosJect.WrapperGenerationTests
{
    class Program
    {
        MyClass myClass = new MyClass();

        static void Main(string[] args)
        {
            MSBuildWorkspace workspace = MSBuildWorkspace.Create();

            Project proj = workspace.OpenProjectAsync("../../../TestProj/TestProj.csproj").Result;

            Compilation compilation = proj.GetCompilationAsync().Result;

            INamedTypeSymbol myClass = compilation.GetTypeByMetadataName("TestProj.MyClass");
            INamedTypeSymbol myInterface = compilation.GetTypeByMetadataName("TestProj.IMyInterface");

            string fullNamespace = myClass.GetFullNamespace();

            IEnumerable<ISymbol> allMembers = myClass.GetMembers();

            IMethodSymbol myMethod = myClass.GetMemberByName<IMethodSymbol>(nameof(MyClass.MyMethod));

            IPropertySymbol myProp = myClass.GetMemberByName<IPropertySymbol>(nameof(MyClass.MyInt));

            string methodSignature = myMethod.GetMethodSignature();

            IPropertySymbol intProp = myInterface.GetMemberByName<IPropertySymbol>(nameof(IMyInterface.TheInt));
            IPropertySymbol strProp = myInterface.GetMemberByName<IPropertySymbol>(nameof(IMyInterface.TheStr));

            RoslynCodeBuilder roslynCodeBuilder = new RoslynCodeBuilder();

            RoslynAnalysisAndGenerationUtils.TheNamespaces = roslynCodeBuilder.AllNamespaces;
            RoslynAnalysisAndGenerationUtils.TheAssemblies = roslynCodeBuilder.AllAssemblies;

            roslynCodeBuilder.AddNamespace("NP.Generated");
            roslynCodeBuilder.AddClass("MyClass1", null, null, myInterface);

            roslynCodeBuilder.AddAutoProp(intProp.Name, intProp.Type as INamedTypeSymbol);
            roslynCodeBuilder.AddAutoProp(strProp.Name, strProp.Type as INamedTypeSymbol);

            //roslynCodeBuilder.AddField("_myField", myClass);

            //roslynCodeBuilder.AddPropWithBackingStore("MyProp", myClass);

            //roslynCodeBuilder.AddPropWrapper("_propField", myProp);

            //roslynCodeBuilder.AddPropOpening("MyField", myClass);
            //roslynCodeBuilder.Pop();

            roslynCodeBuilder.AddMethodWrapper("_propField", myMethod);


            roslynCodeBuilder.PopAll();

            Console.WriteLine(roslynCodeBuilder.ToString());
        }
    }
}
