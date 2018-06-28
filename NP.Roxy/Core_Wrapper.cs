using Microsoft.CodeAnalysis;
using NP.Concepts.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NP.Roxy
{
    public partial class Core
    {
        #region DEFAULT WRAPPERS REGION
        Dictionary<INamedTypeSymbol, INamedTypeSymbol> _wrapperDictionary =
            new Dictionary<INamedTypeSymbol, INamedTypeSymbol>(TypeSymbolComparer.TheTypeSymbolComparer);

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

        public static INamedTypeSymbol GetWrapperType(INamedTypeSymbol typeToImplementSymbol) =>
            TheCore.GetTWrapper(typeToImplementSymbol);

        public static INamedTypeSymbol GetWrapperType(Type typeToImplement) =>
            TheCore.GetTWrapper(typeToImplement);

        public static INamedTypeSymbol GetWrapperType<TToImplement>()
            => TheCore.GetTWrapper<TToImplement>();

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



        IEnumerable<Assembly> _addedAssemblies = new List<Assembly>();
        public void AddAssembly(Assembly assembly)
        {
            if (_addedAssemblies.Contains(assembly))
                return;

            Type[] assemblyTypes = assembly.GetTypes();

            foreach (Type assemblyType in assemblyTypes)
            {
                IEnumerable<WrapperInterfaceAttribute> wrapperAttrs =
                    assemblyType.GetCustomAttributes<WrapperInterfaceAttribute>();

                foreach (WrapperInterfaceAttribute wrapperAttr in wrapperAttrs)
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

        #endregion DEFAULT WRAPPERS REGION
    }
}
