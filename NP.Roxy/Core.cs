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
using NP.Utilities;
using NP.Roxy.TypeConfigImpl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NP.Concepts.Attributes;
using NP.Roxy.Attributes;

namespace NP.Roxy
{
    public interface SingleWrapperInterface<WrappedType>
    {
        [ConstructorInit]
        WrappedType TheWrappedType { get; }
    }

    public partial class Core
    {

        public static Core TheCore { get; } =
            new Core();

        internal Core()
        {
            CoreInit();
            StartProj();
            this.AddTypesToReference(new[] { RoslynAnalysisAndGenerationUtils.NoTypeType });
        }

        INamedTypeSymbol NoTypeSymbol =>
            typeof(NoType).GetTypeSymbol(TheCompilation);

        internal List<ITypeConfig> AllCreatedTypes { get; } = new List<ITypeConfig>();

        internal List<ITypeConfig> AllTypesAddedToCompilation { get; } = new List<ITypeConfig>();

        internal bool HasCreatedType(string className)
        {
            return AllCreatedTypes.FirstOrDefault(t => t.ClassName == className) != null;
        }

        internal bool HasCompiledType(string className)
        {
            return AllTypesAddedToCompilation.FirstOrDefault(t => t.ClassName == className) != null;
        }

        bool HasCompiledType(ITypeConfig typeConfig)
        {
            return HasCompiledType(typeConfig.ClassName);
        }

        internal void AddClass(ITypeConfig typeConfig)
        {
            this.UpdateCompilation(typeConfig.ClassName, typeConfig.TheGeneratedCode);

            AllTypesAddedToCompilation.Add(typeConfig);

            GeneratedAssemblyUpToDate = false;
        }

        protected void OnRegeneratingAssembly()
        {
            IEnumerable<ITypeConfig> uncompiledTypes =
                AllCreatedTypes.Except(AllTypesAddedToCompilation).ToList();

            if (uncompiledTypes.Count() > 0)
            {
                string errorMessage = "Usage Error: some of the created TypeConfigs have not been compiled. Either do not create them or compile them all.\n";
                errorMessage += $"Here are the classNames that have not been compiled: {uncompiledTypes.Select(t => t.ClassName).StrConcat()}";

                throw new Exception(errorMessage);
            }
        }


        protected void OnAssemblyRegenerated()
        {
            foreach (ITypeConfig typeConfig in this.AllTypesAddedToCompilation)
            {
                typeConfig.SetGeneratedType();
            }
        }


        public ITypeConfig FindTypeConfig(string className)
        {
            if (className == null)
                return null;

            return this.AllTypesAddedToCompilation.FirstOrDefault(tConfig => tConfig.ClassName == className);
        }


        internal ITypeConfig CreateTypeConf
        (
            string className,
            INamedTypeSymbol typeToImplementSymbol,
            INamedTypeSymbol implementorSymbol
        )
        {
            if (className == null)
            {
                className = typeToImplementSymbol.CreateClassName(implementorSymbol);
            }

            TypeConfig result =
                new TypeConfig
                (
                    this,
                    className,
                    typeToImplementSymbol,
                    implementorSymbol
                );

            CheckAlreadyHasType(result.ClassName);

            this.AllCreatedTypes.Add(result);

            return result;
        }

        public ITypeConfig FindOrCreateTypeConf
        (
            string className,
            INamedTypeSymbol typeToImplementSymbol,
            INamedTypeSymbol implementorSymbol
        )
        {
            implementorSymbol = implementorSymbol.GetNoTypeForNull(TheCompilation);

            if (className == null)
            {
                className = typeToImplementSymbol.CreateClassName(implementorSymbol);
            }

            ITypeConfig typeConfig = FindTypeConfig(className);

            if (typeConfig != null)
            {
                typeConfig.CheckMatches(typeToImplementSymbol, implementorSymbol);

                return typeConfig;
            }

            return CreateTypeConf(className, typeToImplementSymbol, implementorSymbol);
        }

        public ITypeConfig FindOrCreateTypeConf<TToImplement, TImplementor>(string className = null)
        {
            Type typeToImplement = typeof(TToImplement);
            Type implementorType = typeof(TImplementor);

            AddTypesToReference(new[] { typeToImplement, implementorType });

            return FindOrCreateTypeConf
            (
                className,
                typeToImplement.GetGenericTypeSymbol(TheCompilation),
                implementorType.GetGenericTypeSymbol(TheCompilation));
        }

        void CheckAlreadyHasType(string className)
        {
            if (HasCreatedType(className))
            {
                throw new Exception($"Error: TypeConfig for class of the name name '{className}' has already created before.");
            }
        }

        public ITypeConfig FindOrCreateTypeConfigUsingImplementorWithAttrs
        (
            INamedTypeSymbol typeToImplementSymbol, 
            INamedTypeSymbol implementorTypeSymbol)
        {
            ImplementationClassNameAttribute implementationClassNameAttribute =
                implementorTypeSymbol.GetAttrObject<ImplementationClassNameAttribute>();

            string className = implementationClassNameAttribute.ClassName;

            ITypeConfig typeConfig =
                FindOrCreateTypeConf(className, typeToImplementSymbol, implementorTypeSymbol);

            if (implementorTypeSymbol.IsNoTypeOrNull())
            {
                throw new Exception("Roxy Usage Error: CreateWrapper should have a non-trivial Wrapper argument passed to it.");
            }

            if (!typeConfig.ConfigurationHasBeenCompleted)
            {
                typeConfig.ConfigurationCompleted();
            }

            return typeConfig;
        }

        public ITypeConfig FindOrCreateTypeConfigUsingImplementorWithAttrs
        (
            Type typeToImplement,
            Type implementorType)
        {
            this.AddTypesToReference(new[] { typeToImplement, implementorType });

            return FindOrCreateTypeConfigUsingImplementorWithAttrs
                (
                    typeToImplement.GetTypeSymbol(this.TheCompilation),
                    implementorType.GetTypeSymbol(this.TheCompilation)
                );
        }

        public TToImplement CreateImplInstance<TToImplement, TImplementor>
        (
            params object[] args)
        {
            ITypeConfig typeConfig =
                FindOrCreateTypeConfigUsingImplementorWithAttrs
                (
                    typeof(TToImplement),
                    typeof(TImplementor)
                );

            return typeConfig.CreateInstanceOfType<TToImplement>(args);
        }

        public ITypeConfig FindOrCreateConcretizationTypeConf
        (
            INamedTypeSymbol typeToConcretizeSymbol,
            string concreteClassName = null)
        {
            concreteClassName = concreteClassName ?? typeToConcretizeSymbol.Name.GetConcretizationName();

            ITypeConfig typeConfig =
                this.FindOrCreateTypeConf(concreteClassName, typeToConcretizeSymbol, null);

            if (typeConfig.TheGeneratedCode == null)
            {
                typeConfig.TheBuilderSetter =
                    DefaultConcretizationMemberBuilderSetter.TheDefaultConcretizationBuilderSetter;

                typeConfig.ConfigurationCompleted();
            }

            return typeConfig;
        }

        public ITypeConfig FindOrCreateConcretizationTypeConf<T>(string concreteClassName = null)
        {
            AddTypesToReference((new[] { typeof(T), typeof(NoType) }));

            INamedTypeSymbol typeSymbol = typeof(T).GetTypeSymbol(this.TheCompilation);

            return FindOrCreateConcretizationTypeConf(typeSymbol, concreteClassName);
        }

        public static ITypeConfig FindOrCreateConcretizationTypeConfig(INamedTypeSymbol classTypeSymbol)
        {
            return TheCore.FindOrCreateConcretizationTypeConf(classTypeSymbol);
        }

        public static ITypeConfig FindOrCreateConcretizationTypeConfig<T>()
        {
            return TheCore.FindOrCreateConcretizationTypeConf<T>();
        }

        internal static INamedTypeSymbol GetConcreteTypeSymbol<T>()
        {
            ITypeConfig typeConfig = FindOrCreateConcretizationTypeConfig<T>();

            return typeConfig.TheSelfTypeSymbol;
        }

        public T ConcretizeType<T>()
        {
            ITypeConfig typeConfig = this.FindOrCreateConcretizationTypeConf<T>();

            if (typeConfig.TheGeneratedType == null)
            {
                this.RegenerateAssembly();
            }

            return typeConfig.CreateInstanceOfType<T>();
        }

        public static ITypeConfig FindOrCreateTypeConfig<TToImplement, TImplementor>(string className = null)
        {
            return TheCore.FindOrCreateTypeConf<TToImplement, TImplementor>(className);
        }

        public static T Concretize<T>()
        {
            return TheCore.ConcretizeType<T>();
        }

        public static void Save(string path)
        {
            TheCore.SaveToPath(path);
        }

        public static void SetSaveOnErrorPath(string savePath)
        {
            TheCore.SaveToPathOnCompilationError(savePath);
        }


        public static TToImplement CreateImplementedInstance<TToImplement, TWrapper>(params object[] args)
        {
            return TheCore.CreateImplInstance<TToImplement, TWrapper>(args);
        }


        public TClass CreateClassObj<TClass>(string className = null, params object[] args)
        {
            ITypeConfig typeConfig = FindTypeConfig(className);

            if (typeConfig == null)
                return (TClass)Activator.CreateInstance(typeof(TClass), args);

            if (typeConfig.TheGeneratedType == null)
            {
                RegenerateAssembly();
            }

            return typeConfig.CreateInstanceOfType<TClass>(args);
        }
    }

    internal static class CoreUtils
    {
        internal static Core GetCore(this Core core)
        {
            if (core == null)
                return Core.TheCore;

            return core;
        }
    }
}
