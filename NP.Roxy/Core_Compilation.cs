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

    public partial class Core : ICompilationContainer
    {
        MefHostServices _host = null;

        internal RoslynWorkspaceBase TheWorkspace { get; private set; }

        ProjectId TheProjId { get; set; }

        Project TheProj { get; set; }

        internal const string ProjName = "TheGeneratedProj";

        internal Assembly TheGeneratedAssembly
        {
            get;
            private set;
        }

        public Compilation TheCompilation { get; private set; }

        internal ParseOptions TheParseOptions { get; } = new CSharpParseOptions();

        internal static CSharpCompilationOptions TheAssemblyCompilationsOptions { get; private set; }

        static bool _initialized = false;
        static void StaticInit()
        {
            if (_initialized)
                return;

            TheAssemblyCompilationsOptions = new CSharpCompilationOptions
            (
                OutputKind.DynamicallyLinkedLibrary
            );

            // this is a hack to allow the compilation to contain the non-public members
            TheAssemblyCompilationsOptions = 
                (CSharpCompilationOptions) TheAssemblyCompilationsOptions.CallMethod("WithMetadataImportOptions", true, false, (byte) 2);

            _initialized = true;
        }

        public INamedTypeSymbol GetTypeSymbol(Type type)
        {
            return type.GetTypeSymbol(this.TheCompilation);
        }

        protected void CoreInit()
        {
            StaticInit();

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
                    parseOptions: TheParseOptions,
                    compilationOptions: TheAssemblyCompilationsOptions
                );

            TheProj = TheWorkspace.AddProject(projInfo);

            TheCompilation =
                TheWorkspace.CurrentProj.GetCompilationAsync().Result;
        }

        protected void UpdateCompilation(string docName, string code)
        {
            Document newDoc =
                TheWorkspace.AddDocument(docName, code);

            this.TheProj = TheWorkspace.CurrentProj;

            SyntaxTree syntaxTree =
                newDoc.GetSyntaxTreeAsync().Result;

            TheCompilation = TheCompilation.AddSyntaxTrees(syntaxTree);
        }

        protected void StartProj()
        {
            CreateProject();
        }

        public void SaveToPath(string path = null)
        {
            if (path == null)
                path = _pathToSaveToOnError;

            if (path == null)
            {
                throw new Exception("Roxy Usage Error: No path is given to save the generated classes");
            }

            this.TheProj.SaveProj(path);
        }

        internal bool GeneratedAssemblyUpToDate { get; set; } = false;

        public void RegenerateAssembly()
        {
            OnRegeneratingAssembly();

            if (GeneratedAssemblyUpToDate)
                return;

            CompilationResult compilationResult =
                TheCompilation.GetCompilationResult();

            if (!compilationResult.Success)
            {
                if (!_pathToSaveToOnError.IsNullOrEmpty())
                {
                    this.SaveToPath(_pathToSaveToOnError);
                }

                throw new Exception($"Compilation Error: {compilationResult.TheErrors.StrConcat(null, "\n\n")}");
            }

            TheGeneratedAssembly = Assembly.Load(compilationResult.TheResult);

            OnAssemblyRegenerated();

            GeneratedAssemblyUpToDate = true;
        }

        string _pathToSaveToOnError = null;
        public void SaveToPathOnCompilationError(string pathToSaveToOnError)
        {
            _pathToSaveToOnError = pathToSaveToOnError;
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return TheGeneratedAssembly.FullName == args.Name ? this.TheGeneratedAssembly : null;
        }


        internal void AddAssembliesToReference<T>(IEnumerable<T> assemblyInfos, Func<T, AssemblyIdentity> toId, Func<T, MetadataReference> toMDReference)
        {
            IEnumerable<AssemblyIdentity> newAssemblyIds =  
                assemblyInfos.Select(assemblySymbol => toId(assemblySymbol)).Except(TheCompilation.ReferencedAssemblyNames).ToList();

            IEnumerable<T> newAssemblies =
                assemblyInfos.Where(aSymbol => newAssemblyIds.Contains(toId(aSymbol))).ToList();

            if (newAssemblies.IsNullOrEmpty())
                return;

            IEnumerable<MetadataReference> allMetadataReferences =
                TheCompilation.References.Union(newAssemblies.SelectMany(assemblySymbol => toMDReference(assemblySymbol).ToCollection())).ToList();

            TheProj = TheProj.WithMetadataReferences(allMetadataReferences);

            TheCompilation = TheCompilation.WithReferences(allMetadataReferences);
        }

        internal void AddAssembliesToReference(IEnumerable<Assembly> assemblies)
        {
            AddAssembliesToReference
            (
                assemblies,
                (assembly) => AssemblyIdentity.FromAssemblyDefinition(assembly),
                (assembly) => assembly.ToRef()
            );
        }

        internal void AddTypesToReference(IEnumerable<Type> types)
        {
            AddAssembliesToReference(types.SelectMany(type => type.Assembly.GetAssemblyAndReferencedAssemblies()));
        }

        internal void AddAssemblySymbolsToReference(IEnumerable<IAssemblySymbol> assemblySymbols)
        {
            AddAssembliesToReference
            (
                assemblySymbols,
                (assemblySymbol) => assemblySymbol.Identity,
                (assemblySymbol) => assemblySymbol.ToRef()
            );
        }

        internal void AddTypeSymbolsToReference(IEnumerable<ITypeSymbol> typeSymbols)
        {
            AddAssemblySymbolsToReference(typeSymbols.SelectMany(typeSymbol => typeSymbol.ContainingAssembly.GetAssemblyAndReferencedAssemblies()));
        }
    }
}
