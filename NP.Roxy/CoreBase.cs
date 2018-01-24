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
using Microsoft.CodeAnalysis.Host.Mef;
using NP.Utilities;
using System;
using System.Collections.Generic;
using System.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using NP.Roxy.TypeConfigImpl;

namespace NP.Roxy
{
    internal static class AssemblerNames
    {
        public const string GENERATED_NAMESPACE_NAME = "NP.Generated";

        public static string GetFullTypeName(this string type)
        {
            string fullGeneratedTypeName =
                StrUtils.StrConcat
                (
                    new[] { GENERATED_NAMESPACE_NAME, type },
                    null,
                    "."
                );

            return fullGeneratedTypeName;
        }
    }

    public abstract class CoreBase
    {
        MefHostServices _host = null;

        internal RoslynWorkspaceBase TheWorkspace { get; }

        ProjectId TheProjId { get; }

        Project TheProj { get; set; }

        internal const string ProjName = "TheGeneratedProj";

        internal Assembly TheGeneratedAssembly
        {
            get;
            private set;
        }

        internal Compilation TheCompilation { get; private set; }

        internal ParseOptions TheParseOptions { get; } = new CSharpParseOptions();

        internal static CSharpCompilationOptions TheAssemblyCompilationsOptions { get; }

        static CoreBase()
        {
            TheAssemblyCompilationsOptions = new CSharpCompilationOptions
            (
                OutputKind.DynamicallyLinkedLibrary
            );

            // this is a hack to allow the compilation to contain the non-public members
            TheAssemblyCompilationsOptions = 
                (CSharpCompilationOptions) TheAssemblyCompilationsOptions.CallMethod("WithMetadataImportOptions", true, false, (byte) 2);
        }

        protected CoreBase()
        {
            CompositionHost compositionContext =
                new ContainerConfiguration()
                .WithAssemblies(MefHostServices.DefaultAssemblies)
                .CreateContainer();

            _host = MefHostServices.Create(compositionContext);

            TheWorkspace = new RoslynWorkspaceBase(_host, "Generated");
            TheProjId = ProjectId.CreateNewId(ProjName);


            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        internal void CreateProject()
        {
            TheWorkspace.ClearSolution();

            ProjectInfo projInfo =
                ProjectInfo.Create
                (
                    TheProjId,
                    VersionStamp.Create(),
                    ProjName,
                    AssemblerNames.GENERATED_NAMESPACE_NAME,
                    LanguageNames.CSharp,
                    metadataReferences: AllMetadataReferences,
                    parseOptions: TheParseOptions,
                    compilationOptions: TheAssemblyCompilationsOptions
                );

            TheProj = TheWorkspace.AddProject(projInfo);

            TheCompilation =
                TheWorkspace.CurrentProj.GetCompilationAsync().Result;
        }

        internal HashSet<Assembly> AllReferencedAssemblies { get; } =
            new HashSet<Assembly>();

        internal IEnumerable<MetadataReference> AllMetadataReferences =>
            AllReferencedAssemblies.Select(assmbly => assmbly.ToRef());

        protected void UpdateCompilation(string docName, string code)
        {
            Document newDoc =
                TheWorkspace.AddDocument(docName, code);

            SyntaxTree syntaxTree =
                newDoc.GetSyntaxTreeAsync().Result;

            TheCompilation = TheCompilation.AddSyntaxTrees(syntaxTree);
        }

        protected void StartProj()
        {
            CreateProject();
        }

        internal bool GeneratedAssemblyUpToDate { get; set; } = false;

        protected abstract void OnAssemblyRegenerated();

        protected abstract void OnRegeneratingAssembly();

        protected void RegenerateAssembly()
        {
            OnRegeneratingAssembly();

            if (GeneratedAssemblyUpToDate)
                return;

            CompilationResult compilationResult =
                TheCompilation.GetCompilationResult();

            if (!compilationResult.Success)
                throw new Exception($"Compilation Error: {compilationResult.TheErrors.StrConcat(null, "\n\n")}");

            TheGeneratedAssembly = Assembly.Load(compilationResult.TheResult);

            OnAssemblyRegenerated();

            GeneratedAssemblyUpToDate = true;
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return TheGeneratedAssembly;
        }


        internal void AddAssembliesToReference(IEnumerable<Assembly> assemblies)
        {
            IEnumerable<Assembly> newAssemblies =
                assemblies.Except(this.AllReferencedAssemblies);

            if (newAssemblies.IsNullOrEmpty())
                return;

            newAssemblies.DoForEach(assembly => AllReferencedAssemblies.Add(assembly));

            MetadataReference[] newReferences = 
                newAssemblies.Select(assmbly => assmbly.ToRef()).ToArray();

            MetadataReference[] allReferences =
                AllReferencedAssemblies.Select(assmbly => assmbly.ToRef()).ToArray();

            TheProj = TheProj.WithMetadataReferences(allReferences);

            TheCompilation = TheCompilation.WithReferences(allReferences);
        }

    }
}
