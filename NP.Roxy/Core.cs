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
    public interface SingleWrapperInterface<WrappedType>
    {
        WrappedType TheWrappedType { get; }
    }

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
            typeof(NoClass).GetTypeSymbol(TheCompilation);

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

        internal static T GetInstanceOfType<T>(ITypeConfig typeConfig, params object[] args)
        {
            return (T)Activator.CreateInstance(typeConfig.TheGeneratedType, args);
        }

        internal ITypeConfig FindTypeConfigOfGeneratedType(string className)
        {
            ITypeConfig typeConfig =
                this.AllTypesAddedToCompilation
                    .Where(tConfig => tConfig.ClassName == className).SingleOrDefault();

            return typeConfig;
        }


        public ITypeConfig FindTypeConfig<TInterface, TClass, TWrapper>(string className = null)
        {
            Type interfaceType = typeof(TInterface);
            Type baseClassType = typeof(TClass);
            Type wrapperType = typeof(TWrapper);

            ITypeConfig typeConfig = null;

            if (className.IsNullOrEmpty())
            {
                typeConfig = this.AllTypesAddedToCompilation
                    .FirstOrDefault
                    (
                        tConfig =>
                            ( (interfaceType == typeof(NoInterface)) || 
                               tConfig.ImplInterfaceTypeSymbol.Matches(interfaceType, this.TheCompilation))
                               &&
                            ( (baseClassType == typeof(NoClass)) || tConfig.SuperClassTypeSymbol.Matches(baseClassType, this.TheCompilation))
                            &&
                            (
                                (wrapperType == typeof(NoInterface)) ||
                                tConfig.WrapInterfaceTypeSymbol.Matches(wrapperType, this.TheCompilation)
                            ));
            }
            else
            {
                typeConfig = this.AllTypesAddedToCompilation.FirstOrDefault(tConfig => tConfig.ClassName == className);
            }

            return typeConfig;
        }
        public ITypeConfig FindTypeConfig<T, TWrapper>(string className = null)
        {
            Type type = typeof(T);
            Type wrapperType = typeof(TWrapper);

            ITypeConfig typeConfig = null;

            if (className.IsNullOrEmpty())
            {
                typeConfig = this.AllTypesAddedToCompilation
                    .FirstOrDefault
                    (
                        tConfig =>
                            (tConfig.ImplInterfaceTypeSymbol.Matches(type, this.TheCompilation) ||
                             tConfig.SuperClassTypeSymbol.Matches(type, this.TheCompilation)
                            )
                            &&
                            (
                                (wrapperType == typeof(NoInterface)) ||
                                tConfig.WrapInterfaceTypeSymbol.Matches(wrapperType, this.TheCompilation)
                            ));
            }
            else
            {
                typeConfig = this.AllTypesAddedToCompilation.FirstOrDefault(tConfig => tConfig.ClassName == className);
            }

            return typeConfig;
        }

        public ITypeConfig FindTypeConfig<T>(string className = null)
            => FindTypeConfig<T, NoInterface>(className);

        public T GetInstOfGeneratedType<T, TWrapper>(string className = null, params object[] args)
        {
            ITypeConfig typeConfig = FindTypeConfig<T, TWrapper>(className);

            if (typeConfig.TheGeneratedType == null)
            {
                this.RegenerateAssembly();
            }

            return GetInstanceOfType<T>(typeConfig, args);
        }

        public T GetInstOfGeneratedType<T>(string className = null, params object[] args) =>
            GetInstOfGeneratedType<T, NoInterface>(className, args);

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

        public ITypeConfig
            CreateTypeConf<TypeToImplement, WrappedInterface>(string className = null)
        {
            Type typeToImplement = typeof(TypeToImplement);
            if (typeToImplement.IsInterface)
                return CreateTypeConf<TypeToImplement, NoClass, WrappedInterface>(className);
            else if (typeToImplement.IsClass)
            {
                return CreateTypeConf<NoInterface, TypeToImplement, WrappedInterface>(className);
            }

            throw new Exception($"Roxy Usage Error: type '{typeToImplement.Name}' is neither class no interface");
        }


        internal ITypeConfig FindOrCreateTypeConf
        (
            string className,
            INamedTypeSymbol implInterfaceTypeSymbol,
            INamedTypeSymbol superClassTypeSymbol,
            INamedTypeSymbol wrapInterfaceTypeSymbol
        )
        {   
            // if null - take the name of the interface without first letter 'I'
            className = implInterfaceTypeSymbol.GetClassName(className);

            ITypeConfig result =
                 AllCreatedTypes.FirstOrDefault(typeConfig => typeConfig.ClassName == className);

            if (result == null)
                result = CreateTypeConf(className, implInterfaceTypeSymbol, superClassTypeSymbol, wrapInterfaceTypeSymbol);

            return result;
        }


        public ITypeConfig FindOrCreateTypeConfByTypeToImpl
        (
            string className,
            INamedTypeSymbol typeToImplSymbol, // can be either an interface or a class
            INamedTypeSymbol wrappedInterfaceSymbol = null
        )
        {
            INamedTypeSymbol implInterfaceSymbol, superClassSymbol;
            if (typeToImplSymbol.TypeKind == TypeKind.Interface)
            {
                implInterfaceSymbol = typeToImplSymbol;
                superClassSymbol = NoClassSymbol;
            }
            else if (typeToImplSymbol.TypeKind == TypeKind.Class)
            {
                implInterfaceSymbol = NoInterfaceSymbol;
                superClassSymbol = typeToImplSymbol;
            }
            else
            {
                throw new Exception($"Roxy Usage Error: Symbol '{typeToImplSymbol.Name}' is neither a class nor an interface");
            }

            if (wrappedInterfaceSymbol == null)
                wrappedInterfaceSymbol = NoInterfaceSymbol;

            ITypeConfig typeConfig =
                this.FindOrCreateTypeConf(className, implInterfaceSymbol, superClassSymbol, wrappedInterfaceSymbol);

            return typeConfig;
        }

        public ITypeConfig
             FindOrCreateTypeConf<TImplementedInterface, TSuperClass, TWrappedInterface>(string className = null)
        {
            ITypeConfig result = FindTypeConfig<TImplementedInterface, TSuperClass, TWrappedInterface>(className);

            if (result == null)
            {
                className = className.GetClassName<TImplementedInterface, TSuperClass>();
                result = CreateTypeConf<TImplementedInterface, TSuperClass, TWrappedInterface>(className);
            }

            return result;
        }

        public ITypeConfig FindOrCreateTypeConfByTypeToImpl<TypeToImpl, TWrapperInterface>(string className)
        {
            Type typeToImpl = typeof(TypeToImpl);

            if (typeToImpl.IsInterface)
            {
                return this.FindOrCreateTypeConf<TypeToImpl, NoClass, TWrapperInterface>(className);
            }
            else if (typeToImpl.IsClass)
            {
                return this.FindOrCreateTypeConf<NoInterface, TypeToImpl, TWrapperInterface>(className);
            }

            throw new Exception($"Type '{typeToImpl.Name}' is neither a class, no an interface");
        }

        public ITypeConfig FindOrCreateTypeConfByTypeToImpl<TypeToImpl>(string className)
        {
            return this.FindOrCreateTypeConfByTypeToImpl<TypeToImpl, NoInterface>(className);
        }

        public static ITypeConfig FindOrCreateTypeConfigByTypeToImpl<TypeToImpl>(string className)
        {
            return TheCore.FindOrCreateTypeConfByTypeToImpl<TypeToImpl>(className);
        }

        public static ITypeConfig
            FindOrCreateTypeConfig<TImplementedInterface, TSuperClass, TWrappedInterface>
            (
                string className = null,
                Core core = null
            )
        {
            core = core.GetCore();

            return core.FindOrCreateTypeConf<TImplementedInterface, TSuperClass, TWrappedInterface>(className);
        }

        public static ITypeConfig
            FindOrCreateTypeConfig<TImplementedInterface, TWrappedInterface>
            (
                string className = null,
                Core core = null
            )
        {
            core = core.GetCore();

            return core.FindOrCreateTypeConf<TImplementedInterface, NoClass, TWrappedInterface>(className);
        }

        public static T GetInstanceOfGeneratedType<T>(string className = null, params object[] args)
        {
            return TheCore.GetInstOfGeneratedType<T>(className, args);
        }

        public ITypeConfig FindOrCreateConcretizationTypeConf
        (
            INamedTypeSymbol typeToConcretizeSymbol, 
            string concreteClassName = null)
        {
            concreteClassName = concreteClassName ?? typeToConcretizeSymbol.Name.GetConcretizationName();

            ITypeConfig typeConfig =
                TheCore.FindOrCreateTypeConfByTypeToImpl(concreteClassName, typeToConcretizeSymbol);

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
            AddTypesToReference((new[] { typeof(T), typeof(NoInterface)}));

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

        internal static INamedTypeSymbol GetConcreteTypeSymbol(INamedTypeSymbol classTypeSymbol)
        {
            ITypeConfig typeConfig = FindOrCreateConcretizationTypeConfig(classTypeSymbol);

            return typeConfig.TheSelfTypeSymbol;
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

            return GetInstanceOfType<T>(typeConfig);
        }

        public TypeToImplement WrapWithNonPublicMembers<TypeToImplement, TWrapper>(string className)
        {
            ITypeConfig typeConfig = FindTypeConfig<TypeToImplement, TWrapper>(className);

            if (typeConfig == null)
            {
                typeConfig = this.CreateTypeConf<TypeToImplement, TWrapper>(className);
            }

            typeConfig.SetAllowNonPublicForAllMembers();

            typeConfig.ConfigurationCompleted();

            if (typeConfig.TheGeneratedType == null)
            {
                RegenerateAssembly();
            }

            return GetInstanceOfType<TypeToImplement>(typeConfig);
        }

        public static TypeToImplement CreateWrapperWithNonPublicMembers<TypeToImplement, TWrapper>(string className)
        {
            return TheCore.WrapWithNonPublicMembers<TypeToImplement, TWrapper>(className);
        }

        public TClass CreateClassObj<TClass>(string className = null, params object[] args)
        {
            ITypeConfig typeConfig = FindTypeConfig<TClass>(className);

            if (typeConfig == null)
                return (TClass) Activator.CreateInstance(typeof(TClass), args);

            if (typeConfig.TheGeneratedType == null)
            {
                RegenerateAssembly();
            }

            return GetInstanceOfType<TClass>(typeConfig, args);
        }

        public ITypeConfig FindOrCreateEnumWrapperTypeConfig<T, EnumType>(Type staticEnumExtensionsType, bool allowNonPublic)
        {
            string className = typeof(EnumType).GetTypeAdapterClassName(typeof(T));

            ITypeConfig enumWrapperTypeConfig =
                this.FindOrCreateTypeConfByTypeToImpl<T, SingleWrapperInterface<EnumType>>(className);

            if (!enumWrapperTypeConfig.ConfigurationHasBeenCompleted)
            {
                enumWrapperTypeConfig.AddStaticUtilsClass
                (
                    nameof(SingleWrapperInterface<EnumType>.TheWrappedType),
                    staticEnumExtensionsType
                );

                if (allowNonPublic)
                {
                    enumWrapperTypeConfig.SetAllowNonPublicForAllMembers();
                }
            }

            return enumWrapperTypeConfig;
        }

        public void CreateEnumAdaptor<T, EnumType>(Type staticEnumExtensionsType, bool allowNonPublic = true)
        {
            ITypeConfig adapterTypeConfig = 
                FindOrCreateEnumWrapperTypeConfig<T, EnumType>(staticEnumExtensionsType, allowNonPublic);

            adapterTypeConfig.ConfigurationCompleted();
        }

        public static void CreateEnumerationAdapter<T, EnumType>(Type staticEnumExtensionsType, bool allowNonPublic = true)
        {
            TheCore.CreateEnumAdaptor<T, EnumType>(staticEnumExtensionsType, allowNonPublic);
        }

        public T BuildEnumWrapper<T, EnumType>(EnumType enumVal)
            where EnumType : struct
        {
            string className = typeof(EnumType).GetTypeAdapterClassName(typeof(T));
            ITypeConfig enumWrapperTypeConfig = FindTypeConfig<T, SingleWrapperInterface<EnumType>>(className);

            if (enumWrapperTypeConfig == null)
                throw new Exception($"Roxy Usage Error: Adapter {className} has not been created. You should call CreateEnumAdaptor method first.");

            return GetInstOfGeneratedType<T>(className, enumVal);
        }

        public static T CreateEnumWrapper<T, EnumType>(EnumType enumVal, bool allowNonPublic = true)
            where EnumType : struct
        {
            return TheCore.BuildEnumWrapper<T, EnumType>(enumVal);
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
