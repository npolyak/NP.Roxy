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
using System.Text;
using System.Threading.Tasks;

namespace NP.Roxy
{
    public class Core : CoreBase
    {
        internal static Core TheCore { get; } =
            new Core();

        internal Core()
        {
            StartProj();
        }

        INamedTypeSymbol NoInterfaceSymbol =>
            typeof(NoInterface).GetTypeSymbol(TheCompilation);

        INamedTypeSymbol NoClassSymbol =>
            typeof(NoInterface).GetTypeSymbol(TheCompilation);

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

        protected override void OnRegeneratingAssembly()
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


        protected override void OnAssemblyRegenerated()
        {
            foreach (ITypeConfig typeConfig in this.AllTypesAddedToCompilation)
            {
                typeConfig.SetGeneratedType();
            }
        }

        internal static T GetInstanceOfType<T>(ITypeConfig typeConfig)
        {
            return (T)Activator.CreateInstance(typeConfig.TheGeneratedType);
        }

        internal ITypeConfig FindTypeConfigOfGeneratedType(string className)
        {
            ITypeConfig typeConfig =
                this.AllTypesAddedToCompilation
                    .Where(tConfig => tConfig.ClassName == className).SingleOrDefault();

            return typeConfig;
        }

        public T GetInstOfGeneratedType<T>(string className = null)
        {
            Type type = typeof(T);

            ITypeConfig typeConfig =
                this.AllTypesAddedToCompilation
                    .Where
                    (
                        tConfig =>
                            (tConfig.ImplInterfaceTypeSymbol.Matches(type, this.TheCompilation) ||
                             tConfig.SuperClassTypeSymbol.Matches(type, this.TheCompilation)
                            )
                            &&
                            ((className == null) ||
                              (tConfig.ClassName == className))).FirstOrDefault();

            if (typeConfig.TheGeneratedType == null)
            {
                this.RegenerateAssembly();
            }

            return GetInstanceOfType<T>(typeConfig);
        }

        void CheckAlreadyHasType(string className)
        {
            if (HasCreatedType(className))
            {
                throw new Exception($"Error: TypeConfig for class of the name name '{className}' has already created before.");
            }
        }

        internal ITypeConfig CreateTypeConf
        (
            string className,
            INamedTypeSymbol implInterfaceTypeSymbol,
            INamedTypeSymbol superClassTypeSymbol,
            INamedTypeSymbol wrapInterfaceTypeSymbol
        )
        {
            TypeConfigBySymbols result =
                new TypeConfigBySymbols
                (
                    this,
                    className,
                    implInterfaceTypeSymbol,
                    superClassTypeSymbol,
                    wrapInterfaceTypeSymbol
                );

            CheckAlreadyHasType(result.ClassName);

            this.AllCreatedTypes.Add(result);

            return result;
        }

        internal ITypeConfig<TImplementedInterface, TSuperClass, TWrappedInterface>
            CreateTypeConf<TImplementedInterface, TSuperClass, TWrappedInterface>(string className = null)
        {
            ITypeConfig<TImplementedInterface, TSuperClass, TWrappedInterface> result =
                new TypeConfig<TImplementedInterface, TSuperClass, TWrappedInterface>(this, className);

            CheckAlreadyHasType(result.ClassName);

            this.AllCreatedTypes.Add(result);

            return result;
        }


        internal ITypeConfig CreateOrFindTypeConf
        (
            string className,
            INamedTypeSymbol implInterfaceTypeSymbol,
            INamedTypeSymbol superClassTypeSymbol,
            INamedTypeSymbol wrapInterfaceTypeSymbol
        )
        {
            className = implInterfaceTypeSymbol.GetClassName(className);
            ITypeConfig result =
                 AllCreatedTypes.FirstOrDefault(typeConfig => typeConfig.ClassName == className);

            if (result == null)
                result = CreateTypeConf(className, implInterfaceTypeSymbol, superClassTypeSymbol, wrapInterfaceTypeSymbol);

            return result;
        }


        internal ITypeConfig<TImplementedInterface, TSuperClass, TWrappedInterface>
             CreateOrFindTypeConf<TImplementedInterface, TSuperClass, TWrappedInterface>(string className = null)
        {
            // if null - take the name of the interface without first letter 'I'
            className = className.GetClassName<TImplementedInterface>();

            ITypeConfig<TImplementedInterface, TSuperClass, TWrappedInterface> result =
                (ITypeConfig<TImplementedInterface, TSuperClass, TWrappedInterface>)
                    AllCreatedTypes.FirstOrDefault(typeConfig => typeConfig.ClassName == className);

            if (result == null)
                result = CreateTypeConf<TImplementedInterface, TSuperClass, TWrappedInterface>(className);

            return result;
        }

        internal static ITypeConfig<TImplementedInterface, TSuperClass, TWrappedInterface>
            CreateTypeConfig<TImplementedInterface, TSuperClass, TWrappedInterface>(string className = null, Core core = null)
        {
            core = core.GetCore();

            return core.CreateTypeConf<TImplementedInterface, TSuperClass, TWrappedInterface>(className);
        }

        internal static ITypeConfig<TImplementedInterface, NoClass, TWrappedInterface>
            CreateTypeConfig<TImplementedInterface, TWrappedInterface>(string className = null, Core core = null)
        {
            return CreateTypeConfig<TImplementedInterface, NoClass, TWrappedInterface>(className, core);
        }

        public static ITypeConfig<TImplementedInterface, TSuperClass, TWrappedInterface>
            CreateOrFindTypeConfig<TImplementedInterface, TSuperClass, TWrappedInterface>
            (
                string className = null,
                Core core = null
            )
        {
            core = core.GetCore();

            return core.CreateOrFindTypeConf<TImplementedInterface, TSuperClass, TWrappedInterface>(className);
        }

        public static ITypeConfig<TImplementedInterface, NoClass, TWrappedInterface>
            CreateOrFindTypeConfig<TImplementedInterface, TWrappedInterface>
            (
                string className = null,
                Core core = null
            )
        {
            core = core.GetCore();

            return core.CreateOrFindTypeConf<TImplementedInterface, NoClass, TWrappedInterface>(className);
        }

        public static T GetInstanceOfGeneratedType<T>(string className = null)
        {
            return TheCore.GetInstOfGeneratedType<T>(className);
        }

        public ITypeConfig GetOrCreateClassConcretizationTypeConf(INamedTypeSymbol classTypeSymbol)
        {
            ITypeConfig typeConfig =
                CreateOrFindTypeConf(classTypeSymbol.Name.GetConcretizationName(), NoInterfaceSymbol, classTypeSymbol, NoInterfaceSymbol);

            if (typeConfig.TheGeneratedCode == null)
            {
                typeConfig.TheBuilderSetter =
                    DefaultConcretizationMemberBuilderSetter.TheDefaultConcretizationBuilderSetter;

                typeConfig.ConfigurationCompleted();
            }

            return typeConfig;
        }

        public ITypeConfig GetOrCreateClassConcretizationTypeConf<TClass>()
        {
            AddAssembliesToReference((new[] { typeof(TClass), typeof(NoInterface)}).GetAllReferencedAssemblies());

            INamedTypeSymbol classSymbol = typeof(TClass).GetTypeSymbol(this.TheCompilation);

            return GetOrCreateClassConcretizationTypeConf(classSymbol);
        }

        public static ITypeConfig GetClassConcretizationTypeConfig(INamedTypeSymbol classTypeSymbol)
        {
            return TheCore.GetOrCreateClassConcretizationTypeConf(classTypeSymbol);
        }

        public static ITypeConfig GetClassConcretizationTypeConfig<TClass>()
        {
            return TheCore.GetOrCreateClassConcretizationTypeConf<TClass>();
        }

        internal static INamedTypeSymbol GetConcreteTypeSymbol(INamedTypeSymbol classTypeSymbol)
        {
            ITypeConfig typeConfig = GetClassConcretizationTypeConfig(classTypeSymbol);

            return typeConfig.TheSelfTypeSymbol;
        }

        internal static INamedTypeSymbol GetConcreteTypeSymbol<TClass>()
        {
            ITypeConfig typeConfig = GetClassConcretizationTypeConfig<TClass>();

            return typeConfig.TheSelfTypeSymbol;
        }

        public TClass GetClassConcr<TClass>()
        {
            ITypeConfig typeConfig = this.GetOrCreateClassConcretizationTypeConf<TClass>();

            if (typeConfig.TheGeneratedType == null)
            {
                this.RegenerateAssembly();
            }

            return GetInstanceOfType<TClass>(typeConfig);
        }


        public TClass FindOrCreateClassObj<TClass>(string className)
        {
            ITypeConfig typeConfig = FindTypeConfigOfGeneratedType(className);

            if (typeConfig == null)
                return Activator.CreateInstance<TClass>();

            if (typeConfig.TheGeneratedType == null)
            {
                RegenerateAssembly();
            }

            return GetInstanceOfType<TClass>(typeConfig);
        }


        public static TClass GetClassConcretization<TClass>()
        {
            return TheCore.GetClassConcr<TClass>();
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
