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
using System.Reflection;
using NP.Utilities.Attributes;

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

        INamedTypeSymbol NoTypeSymbol =>
            typeof(NoType).GetTypeSymbol(TheCompilation);

        internal List<ITypeConfig> AllCreatedTypes { get; } = new List<ITypeConfig>();

        internal List<ITypeConfig> AllTypesAddedToCompilation { get; } = new List<ITypeConfig>();

        Dictionary<INamedTypeSymbol, INamedTypeSymbol> _wrapperDictionary = 
            new Dictionary<INamedTypeSymbol, INamedTypeSymbol>(TypeSymbolComparer.TheTypeSymbolComparer);

        Dictionary<IEventSymbol, int> _eventDictionary = 
            new Dictionary<IEventSymbol, int>(SymbolComparer.TheSymbolComparer);

        private static IEventSymbol GetEventSymbol(INamedTypeSymbol typeSymbol, string eventName)
        {
            IEventSymbol eventSymbol = typeSymbol.GetMemberByName<IEventSymbol>(eventName);

            if (eventSymbol == null)
            {
                throw new Exception($"Roxy Usage Error: event {eventName} is not found within type {typeSymbol.GetUniqueTypeStr()}");
            }

            return eventSymbol;
        }

        public void AddEventInfo(INamedTypeSymbol typeSymbol, string eventName, int idx = 0)
        {
            IEventSymbol eventSymbol = GetEventSymbol(typeSymbol, eventName);

            _eventDictionary[eventSymbol] = idx;
        }

        public void AddEventInfo(Type eventContainerType, string eventName, int idx = 0)
        {
            INamedTypeSymbol typeSymbol = this.GetTypeSymbol(eventContainerType);

            AddEventInfo(typeSymbol, eventName, idx);
        }

        public void AddEventInfo<TEventContainer>(string eventName, int idx = 0)
        {
            AddEventInfo(typeof(TEventContainer), eventName, idx);
        }

        public static void AddEventIdxInfo<TEventContainer>(string eventName, int idx = 0)
        {
            TheCore.AddEventInfo<TEventContainer>(eventName, idx);
        }

        public int GetEventThisIdx(IEventSymbol eventSymbol)
        {
            if (_eventDictionary.TryGetValue(eventSymbol, out int idx))
            {
                return idx;
            }
            return -1;
        }

        public int GetEventThisIdx(Type eventContainerType, string eventName)
        {
            INamedTypeSymbol typeSymbol = this.GetTypeSymbol(eventContainerType);

            IEventSymbol eventSymbol = GetEventSymbol(typeSymbol, eventName);

            return GetEventThisIdx(eventSymbol);
        }

        public void SetTWrapper
        (
            INamedTypeSymbol typeToImplementSymbol, 
            INamedTypeSymbol wrapperTypeSymbol, 
            bool forceOverride = false)
        {
            if (!forceOverride)
            {
                if (_wrapperDictionary.TryGetValue(typeToImplementSymbol, out INamedTypeSymbol existingWrapperType))
                {
                    throw new Exception($"Roxy Usage Error: Type {typeToImplementSymbol.Name} already has an implementation wrapper type {existingWrapperType.Name}");
                }
            }

            AddTypeSymbolsToReference(new[] { typeToImplementSymbol, wrapperTypeSymbol });

            _wrapperDictionary[typeToImplementSymbol] = wrapperTypeSymbol;
        }

        public void SetTWrapper(Type typeToImplement, Type wrapperType, bool forceOverride = false)
        {
            this.AddTypesToReference(new[] { typeToImplement, wrapperType });

            INamedTypeSymbol typeToImplementSymbol = this.GetUnboundGenericTypeSymbol(typeToImplement);
            INamedTypeSymbol wrapperTypeSymbol = this.GetUnboundGenericTypeSymbol(wrapperType);

            SetTWrapper(typeToImplementSymbol, wrapperTypeSymbol, forceOverride);
        }

        public void SetTWrapper<TToImplement, TWrapper>(bool forceOverride = false)
        {
            Type typeToImplement = typeof(TToImplement);
            Type wrapperType = typeof(TWrapper);

            SetTWrapper(typeToImplement, wrapperType, forceOverride);
        }

        public INamedTypeSymbol GetTWrapper(INamedTypeSymbol typeToImplementSymbol)
        {
            if (_wrapperDictionary.TryGetValue(typeToImplementSymbol, out INamedTypeSymbol existingWrapperType))
            {
                return existingWrapperType;
            }

            return null;
        }

        public INamedTypeSymbol GetTWrapper(Type typeToImplement)
        {
            return GetTWrapper(this.GetTypeSymbol(typeToImplement));
        }

        public INamedTypeSymbol GetTWrapper<TToImplement>() =>
            GetTWrapper(typeof(TToImplement));


        IEnumerable<Assembly> _addedAssemblies = new List<Assembly>();
        public void AddAssembly(Assembly assembly)
        {
            if (_addedAssemblies.Contains(assembly))
                return;

            Type[] assemblyTypes = assembly.GetTypes();

            foreach(Type assemblyType in assemblyTypes)
            {
                IEnumerable<WrapperInterfaceAttribute> wrapperAttrs =
                    assemblyType.GetCustomAttributes<WrapperInterfaceAttribute>();

                foreach(WrapperInterfaceAttribute wrapperAttr in wrapperAttrs)
                {
                    Type typeToImplement = wrapperAttr.TypeToImplement;

                    this.SetTWrapper(typeToImplement, assemblyType);
                }
            }
        }

        public void AddTypeAssembly<T>()
        {
            AddAssembly(typeof(T).Assembly);
        }

        public static void AddAssemblyStatic(Assembly assembly)
        {
            TheCore.AddAssembly(assembly);
        }

        public static void AddTypeAssemblyStatic<T>()
        {
            TheCore.AddTypeAssembly<T>();
        }

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


        public ITypeConfig<TToImplement, TImplementationClass, TWrapper> FindTypeConfig<TToImplement, TImplementationClass, TWrapper>(string className = null)
        {
            ITypeConfig<TToImplement, TImplementationClass, TWrapper> typeConfig = null;

            if (!className.IsNullOrEmpty())
            {
                typeConfig =
                    (ITypeConfig<TToImplement, TImplementationClass, TWrapper>) this.AllTypesAddedToCompilation.FirstOrDefault(tConfig => tConfig.ClassName == className);

                return typeConfig;
            }

            INamedTypeSymbol typeToImplementSymbol = this.GetRealTypeSymbol<TToImplement>();
            INamedTypeSymbol implementationSuperClassTypeSymbol = this.GetRealTypeSymbol<TImplementationClass>();
            INamedTypeSymbol wrapperTypeSymbol = this.GetRealTypeSymbol<TWrapper>();

            if (className.IsNullOrEmpty())
            {
                typeConfig = this.AllTypesAddedToCompilation
                    .FirstOrDefault
                    (
                        tConfig => 
                            tConfig.TypeToImplementSymbol.TypesStrictlyMatch(typeToImplementSymbol) &&
                            (tConfig.ImplSuperClassTypeSymbol?.TypesStrictlyMatch(implementationSuperClassTypeSymbol) != false) &&
                            (tConfig.WrapInterfaceTypeSymbol?.TypesStrictlyMatch(wrapperTypeSymbol) != false)
                    ) as ITypeConfig<TToImplement, TImplementationClass, TWrapper>;
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
                            (tConfig.TypeToImplementSymbol.Matches(type, this.TheCompilation) ||
                             tConfig.ImplSuperClassTypeSymbol.Matches(type, this.TheCompilation)
                            )
                            &&
                            (
                                (wrapperType == typeof(NoType)) ||
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
            => FindTypeConfig<T, NoType>(className);

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
            GetInstOfGeneratedType<T, NoType>(className, args);

        public T CreateInstOfGeneratedType<T>(string className = null, params object[] args)
        {
            ITypeConfig typeConfig = FindOrCreateTypeConfByTypeToImpl<T>(className);

            if (!typeConfig.ConfigurationHasBeenCompleted)
            {
                typeConfig.ConfigurationCompleted();
            }

            if (typeConfig.TheGeneratedType == null)
            {
                this.RegenerateAssembly();
            }

            return GetInstanceOfType<T>(typeConfig, args);
        }

        void CheckAlreadyHasType(string className)
        {
            if (HasCreatedType(className))
            {
                throw new Exception($"Error: TypeConfig for class of the name name '{className}' has already created before.");
            }
        }

        internal ITypeConfig FindOrCreateWrapperMergerTypeConfig<TToImplement>(string className, IEnumerable<INamedTypeSymbol> interfacesToMerge)
        {
            InterfaceMergingTypeConfig<TToImplement> interfaceMergingTypeConfig = 
                this.FindTypeConfig<NoType>(className) as InterfaceMergingTypeConfig<TToImplement>;

            if (interfaceMergingTypeConfig == null)
            {
                interfaceMergingTypeConfig =
                    new InterfaceMergingTypeConfig<TToImplement>(this, className, interfacesToMerge);

                CheckAlreadyHasType(interfaceMergingTypeConfig.ClassName);

                this.AllCreatedTypes.Add(interfaceMergingTypeConfig);

                interfaceMergingTypeConfig.ConfigurationCompleted();
            }

            return interfaceMergingTypeConfig;
        }

        internal ITypeConfig<TToImplement, TWrapperInterface> CreateTypeConf<TToImplement, TWrapperInterface>
        (
            string className,
            INamedTypeSymbol implInterfaceTypeSymbol,
            INamedTypeSymbol superClassTypeSymbol
        )
        {
            TypeConfigBySymbols<TToImplement, TWrapperInterface> result =
                new TypeConfigBySymbols<TToImplement, TWrapperInterface>
                (
                    this,
                    className,
                    implInterfaceTypeSymbol,
                    superClassTypeSymbol
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
            return CreateTypeConf<TypeToImplement, NoType, WrappedInterface>(className);
        }


        internal ITypeConfig<TToImplement, TWrapperInterface> FindOrCreateTypeConf<TToImplement, TWrapperInterface>
        (
            string className,
            INamedTypeSymbol implInterfaceTypeSymbol,
            INamedTypeSymbol superClassTypeSymbol
        )
        {   
            className = implInterfaceTypeSymbol.GetClassName(className);

            ITypeConfig<TToImplement, TWrapperInterface> result =
                 AllCreatedTypes.FirstOrDefault(typeConfig => (typeConfig.ClassName == className) &&    
                                                              (typeConfig.WrapInterfaceTypeSymbol.Matches(typeof(TWrapperInterface), this.TheCompilation)))
                                                              as ITypeConfig<TToImplement, TWrapperInterface>;

            if (result == null)
                result = CreateTypeConf<TToImplement, TWrapperInterface>(className, implInterfaceTypeSymbol, superClassTypeSymbol);

            return result;
        }


        public ITypeConfig<TToImplement, TWrapperInterface> FindOrCreateTypeConfByTypeToImpl<TToImplement, TWrapperInterface>
        (
            string className,
            INamedTypeSymbol typeToImplSymbol // can be either an interface or a class
        )
        {
            ITypeConfig<TToImplement, TWrapperInterface> typeConfig =
                this.FindOrCreateTypeConf<TToImplement, TWrapperInterface>(className, typeToImplSymbol, NoTypeSymbol);

            return typeConfig;
        }

        public ITypeConfig<TToImplement, TWrapperInterface>
             FindOrCreateTypeConf<TToImplement, TImplementationSuperClass, TWrapperInterface>(string className = null)
        {
            ITypeConfig<TToImplement, TWrapperInterface> result = 
                FindTypeConfig<TToImplement, TImplementationSuperClass, TWrapperInterface>(className);

            if (result == null)
            {
                className = className.GetClassName<TToImplement, TImplementationSuperClass>();
                result = CreateTypeConf<TToImplement, TImplementationSuperClass, TWrapperInterface>(className);
            }

            return result;
        }

        public ITypeConfig<TypeToImpl, TWrapperInterface>
            FindOrCreateTypeConfByTypeToImpl<TypeToImpl, TWrapperInterface>(string className = null)
        {
            return this.FindOrCreateTypeConf<TypeToImpl, NoType, TWrapperInterface>(className);
        }

        public ITypeConfig FindOrCreateTypeConfByTypeToImpl<TypeToImpl>(string className = null)
        {
            return this.FindOrCreateTypeConfByTypeToImpl<TypeToImpl, NoType>(className);
        }

        private List<INamedTypeSymbol> GetAllWrapperSymbols(INamedTypeSymbol typeToImpl)
        {
            List<INamedTypeSymbol> implementationTypes =
                new List<INamedTypeSymbol>();

            foreach(INamedTypeSymbol baseTypeOrInterface in typeToImpl.GetBaseTypeAndInterfaces())
            {
                INamedTypeSymbol unboundBaseTypeOrInterface =
                    baseTypeOrInterface.IsGenericType ?
                        baseTypeOrInterface.ConstructUnboundGenericType() :
                        baseTypeOrInterface;

                INamedTypeSymbol namedWrapperTypeSymbol = 
                    this.GetTWrapper(unboundBaseTypeOrInterface);

                if (namedWrapperTypeSymbol == null)
                {
                    implementationTypes.AddRangeIfNotThere(GetAllWrapperSymbols(baseTypeOrInterface));
                }
                else
                {
                    if (namedWrapperTypeSymbol.IsUnboundGenericType)
                    {
                        namedWrapperTypeSymbol = namedWrapperTypeSymbol.ConstructedFrom;
                    }

                    INamedTypeSymbol resultSymbol;
                    if (namedWrapperTypeSymbol.IsGenericType)
                    {
                        resultSymbol = namedWrapperTypeSymbol.Construct(baseTypeOrInterface.TypeArguments.ToArray());
                    }
                    else
                    {
                        resultSymbol = namedWrapperTypeSymbol;
                    }
                   
                    implementationTypes.AddIfNotThere(resultSymbol);
                }
            }

            return implementationTypes;
        }

        public ITypeConfig GetDefaultWrapperTypeConf(INamedTypeSymbol typeToImpl)
        {
            IEnumerable<INamedTypeSymbol> implementationTypes =
                GetAllWrapperSymbols(typeToImpl).Union(new[] { typeof(NoType).GetTypeSymbol(this.TheCompilation) }).Distinct(TypeSymbolComparer.TheTypeSymbolComparer).ToList();

            string defaultWrapperName =
                typeToImpl.GetDefaultWrapperName();

            ITypeConfig typeConfig =
                this.FindOrCreateWrapperMergerTypeConfig<NoType>(defaultWrapperName, implementationTypes);

            return typeConfig;
        }

        // assembles the wrapper interface 
        public INamedTypeSymbol GetDefaultWrapper(INamedTypeSymbol typeToImpl)
        {
            ITypeConfig defaultWrapperTypeConfig =
                GetDefaultWrapperTypeConf(typeToImpl);

            return defaultWrapperTypeConfig.TheSelfTypeSymbol;
        }

        public static ITypeConfig FindOrCreateTypeConfigByTypeToImpl<TypeToImpl>(string className = null)
        {
            return TheCore.FindOrCreateTypeConfByTypeToImpl<TypeToImpl>(className);
        }

        public static ITypeConfig<TToImplement, TWrapperInterface>
            FindOrCreateTypeConfig<TToImplement, TSuperClass, TWrapperInterface>
            (
                string className = null,
                Core core = null
            )
        {
            core = core.GetCore();

            return core.FindOrCreateTypeConf<TToImplement, TSuperClass, TWrapperInterface>(className);
        }


        public static ITypeConfig<TToImplement, TWrapperInterface>
            FindOrCreateTypeConfig<TToImplement, TWrapperInterface>
            (
                string className = null,
                Core core = null
            )
        {
            core = core.GetCore();

            return core.FindOrCreateTypeConf<TToImplement, NoType, TWrapperInterface>(className);
        }

        public static ITypeConfig<TToImplement, SingleWrapperInterface<TWrapped>> 
            FindOrCreateSingleWrapperTypeConfig<TToImplement, TWrapped>(string className = null, Core core = null) 
            => FindOrCreateTypeConfig<TToImplement, SingleWrapperInterface<TWrapped>>(className, core);

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
                TheCore.FindOrCreateTypeConfByTypeToImpl<NoType, NoType>(concreteClassName, typeToConcretizeSymbol);

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
            AddTypesToReference((new[] { typeof(T), typeof(NoType)}));

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

        public ITypeConfig<T, SingleWrapperInterface<EnumType>> FindOrCreateEnumWrapperTypeConfig<T, EnumType>(Type staticEnumExtensionsType, bool allowNonPublic)
        {
            string className = typeof(EnumType).GetTypeAdapterClassName(typeof(T));

            ITypeConfig<T, SingleWrapperInterface<EnumType>> enumWrapperTypeConfig =
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

        public static void SetWrapperType
        (
            Type typeToImplement, 
            Type wrapperType, 
            bool forceOverride = false
        )
        {
            TheCore.SetTWrapper(typeToImplement, wrapperType, forceOverride);
        }

        public static void SetWrapperType<TToImplement, TWrapper>(bool forceOverride = false)
        {
            TheCore.SetTWrapper<TToImplement, TWrapper>(forceOverride);
        }


        public static INamedTypeSymbol GetWrapperType(INamedTypeSymbol typeToImplementSymbol) =>
            TheCore.GetTWrapper(typeToImplementSymbol);

        public static INamedTypeSymbol GetWrapperType(Type typeToImplement) =>
            TheCore.GetTWrapper(typeToImplement);

        public static INamedTypeSymbol GetWrapperType<TToImplement>()
            => TheCore.GetTWrapper<TToImplement>();

        public static ITypeConfig GetDefaultWrapperTypeConfig<TToImplement>()
        {
            Type typeToImpl = typeof(TToImplement);

            TheCore.AddTypesToReference(new[]{ typeToImpl});

            INamedTypeSymbol typeToImplSymbol = 
                TheCore.GetTypeSymbol(typeToImpl);

            return TheCore.GetDefaultWrapperTypeConf(typeToImplSymbol);
        }

        public static T CreateInstanceOfGeneratedType<T>(string className = null, params object[] args)
        {
            return TheCore.CreateInstOfGeneratedType<T>(className, args);
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
