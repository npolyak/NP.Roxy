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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NP.Roxy.TypeConfigImpl
{
    public interface ITypeConfig
    {
        string ClassName { get; }

        INamedTypeSymbol TypeToImplementSymbol { get; }

        INamedTypeSymbol ImplSuperClassTypeSymbol { get; }

        INamedTypeSymbol ImplementorTypeSymbol { get; }

        INamedTypeSymbol TheSelfTypeSymbol { get; }

        string TheGeneratedCode { get; }

        void SetEventArgThisIdx(string eventName, int idx);

        void SetEventBuilder(IMemberCodeBuilder<IEventSymbol> eventBuilder, params string[] propNames);

        void SetPropBuilder(IMemberCodeBuilder<IPropertySymbol> propBuilder, params string[] propNames);

        void SetMethodBuilder(IMemberCodeBuilder<IMethodSymbol> methodBuilder, params string[] propNames);

        void SetOverrideVirtual(string memberName, bool includeBase = false);

        void AddStaticUtilsClass
        (
            string wrappedObjPropName, 
            Type staticMethodsContainerClass
        );

        void SetAllowNonPublicForAllMembers();

        void SetAllowNonPublicForAllMembers(string wrappedObjPropName);

        void SetThisMemberMap
        (
            string wrappedObjPropName,
            string wrappedMemberName,
            bool? allowNonPublic = null
        );

        void SetMemberMap
        (
            string wrappedObjPropName, 
            string wrappedMemberName, 
            string wrapperMemberName,
            bool? allowNonPublic = null);

        void SetMemberMapAllowNonPublic
        (
            string wrappedObjPropName,
            params string[] wrapperMemberNames // we pass wrapper member names (not wrapped)
        );

        // signals that the type configuration is completed
        // adds the type to the compilation. 
        void ConfigurationCompleted();

        void SetGeneratedType();

        Type TheGeneratedType { get; }

        // specifies the policy of creating the 
        // member builders
        IMemberBuilderSetter TheBuilderSetter { get; set; }

        bool ConfigurationHasBeenCompleted { get; }

        void SetInit(string propName, INamedTypeSymbol typeSymbol);

        void SetInit<TInit>(string propName);
    }

    public interface ITypeConfig<TToImplement, TImplementor> : ITypeConfig
    {
        void SetPropGetter<TProp>
        (
            Expression<Func<TToImplement, TProp>> propNameGetter,
            Expression<Func<TToImplement, TProp>> propGetter
        );

        void SetWrappedPropGetter<TWrappedObj, TProp>
        (
            string propName, 
            Expression<Func<TImplementor, TWrappedObj>> wrappedObjChooser,
            Expression<Func<TWrappedObj, TProp>> propGetter
        );

        void SetWrappedPropGetter<TWrappedObj, TProp>
        (
            Expression<Func<TToImplement, TProp>> propNameGetter,
            Expression<Func<TImplementor, TWrappedObj>> wrappedObjChooser,
            Expression<Func<TWrappedObj, TProp>> propGetter
        );


        void SetPropMemberMap<TWrapper, TWrappedObj, TWrapperProp>
        (
            Expression<Func<TImplementor, TWrappedObj>> wrappedObjChooser,
            Expression<Func<TWrappedObj, TWrapperProp>> wrappedPropChooser,
            Expression<Func<TWrapper, TWrapperProp>> wrapperPropChooser);

        void SetReturningMethodMap<TWrappedObj, TOut>
        (
             Expression<Func<TToImplement, TOut>> methodNameGetter,
             Expression<Func<TImplementor, TWrappedObj>> wrappedObjChooser,
             Expression<Func<TWrappedObj, TOut>> wrappedMethod
        );

        void SetReturningMethodMap<TWrappedObj, TIn1, TOut>
        (
             Expression<Func<TToImplement, TIn1, TOut>> methodNameGetter,
             Expression<Func<TImplementor, TWrappedObj>> wrappedObjChooser,
             Expression<Func<TWrappedObj, TIn1, TOut>> wrappedMethod
        );

        void SetReturningMethodMap<TWrappedObj, TIn1, TIn2, TOut>
        (
             Expression<Func<TToImplement, TIn1, TIn2, TOut>> methodNameGetter,
             Expression<Func<TImplementor, TWrappedObj>> wrappedObjChooser,
             Expression<Func<TWrappedObj, TIn1, TIn2, TOut>> wrappedMethod
        );

        void SetReturningMethodMap<TWrappedObj, TIn1, TIn2, TIn3, TOut>
         (
              Expression<Func<TToImplement, TIn1, TIn2, TIn3, TOut>> methodNameGetter,
              Expression<Func<TImplementor, TWrappedObj>> wrappedObjChooser,
              Expression<Func<TWrappedObj, TIn1, TIn2, TIn3, TOut>> wrappedMethod
         );

        void SetReturningMethodMap<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TOut>
        (
             Expression<Func<TToImplement, TIn1, TIn2, TIn3, TIn4, TOut>> methodNameGetter,
             Expression<Func<TImplementor, TWrappedObj>> wrappedObjChooser,
             Expression<Func<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TOut>> wrappedMethod
        );


        void SetReturningMethodMap<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5, TOut>
        (
             Expression<Func<TToImplement, TIn1, TIn2, TIn3, TIn4, TIn5, TOut>> methodNameGetter,
             Expression<Func<TImplementor, TWrappedObj>> wrappedObjChooser,
             Expression<Func<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5, TOut>> wrappedMethod
        );

        void SetReturningMethodMap<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TOut>
        (
             Expression<Func<TToImplement, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TOut>> methodNameGetter,
             Expression<Func<TImplementor, TWrappedObj>> wrappedObjChooser,
             Expression<Func<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TOut>> wrappedMethod
        );

        void SetReturningMethodMap<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TOut>
        (
             Expression<Func<TToImplement, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TOut>> methodNameGetter,
             Expression<Func<TImplementor, TWrappedObj>> wrappedObjChooser,
             Expression<Func<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TOut>> wrappedMethod
        );

        void SetReturningMethodMap<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TOut>
        (
             Expression<Func<TToImplement, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TOut>> methodNameGetter,
             Expression<Func<TImplementor, TWrappedObj>> wrappedObjChooser,
             Expression<Func<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TOut>> wrappedMethod
        );

        void SetReturningMethodMap<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TOut>
        (
             Expression<Func<TToImplement, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TOut>> methodNameGetter,
             Expression<Func<TImplementor, TWrappedObj>> wrappedObjChooser,
             Expression<Func<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TOut>> wrappedMethod
        );


        void SetVoidMethodMap<TWrappedObj, TIn1>
        (
             Expression<Func<TToImplement, TIn1>> methodNameGetter,
             Expression<Func<TImplementor, TWrappedObj>> wrappedObjChooser,
             Expression<Func<TWrappedObj, TIn1>> wrappedMethod
        );

        void SetVoidMethodMap<TWrappedObj, TIn1, TIn2>
        (
             Expression<Func<TToImplement, TIn1, TIn2>> methodNameGetter,
             Expression<Func<TImplementor, TWrappedObj>> wrappedObjChooser,
             Expression<Func<TWrappedObj, TIn1, TIn2>> wrappedMethod
        );

        void SetVoidMethodMap<TWrappedObj, TIn1, TIn2, TIn3>
         (
              Expression<Func<TToImplement, TIn1, TIn2, TIn3>> methodNameGetter,
              Expression<Func<TImplementor, TWrappedObj>> wrappedObjChooser,
              Expression<Func<TWrappedObj, TIn1, TIn2, TIn3>> wrappedMethod
         );

        void SetVoidMethodMap<TWrappedObj, TIn1, TIn2, TIn3, TIn4>
        (
             Expression<Func<TToImplement, TIn1, TIn2, TIn3, TIn4>> methodNameGetter,
             Expression<Func<TImplementor, TWrappedObj>> wrappedObjChooser,
             Expression<Func<TWrappedObj, TIn1, TIn2, TIn3, TIn4>> wrappedMethod
        );


        void SetVoidMethodMap<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5>
        (
             Expression<Func<TToImplement, TIn1, TIn2, TIn3, TIn4, TIn5>> methodNameGetter,
             Expression<Func<TImplementor, TWrappedObj>> wrappedObjChooser,
             Expression<Func<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5>> wrappedMethod
        );

        void SetVoidMethodMap<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6>
        (
             Expression<Func<TToImplement, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6>> methodNameGetter,
             Expression<Func<TImplementor, TWrappedObj>> wrappedObjChooser,
             Expression<Func<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6>> wrappedMethod
        );

        void SetVoidMethodMap<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7>
        (
             Expression<Func<TToImplement, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7>> methodNameGetter,
             Expression<Func<TImplementor, TWrappedObj>> wrappedObjChooser,
             Expression<Func<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7>> wrappedMethod
        );

        void SetVoidMethodMap<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8>
        (
             Expression<Func<TToImplement, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8>> methodNameGetter,
             Expression<Func<TImplementor, TWrappedObj>> wrappedObjChooser,
             Expression<Func<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8>> wrappedMethod
        );

        void SetVoidMethodMap<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9>
        (
             Expression<Func<TToImplement, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9>> methodNameGetter,
             Expression<Func<TImplementor, TWrappedObj>> wrappedObjChooser,
             Expression<Func<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9>> wrappedMethod
        );
    }


    public interface ITypeConfig<TToImplement, TImplemenetationSuperClass, TImplementor> : 
        ITypeConfig<TToImplement, TImplementor>
    {
    }

    public class TypeConfigBase
    {
        internal const string INIT_METHOD_NAME = "__Init";
    }


    public class TypeConfigBySymbols : 
        TypeConfigBase,
        ITypeConfig,
        ICompilationContainer
    {
        public INamedTypeSymbol TypeToImplementSymbol { get; private set; }

        public INamedTypeSymbol ImplSuperClassTypeSymbol { get; private set; }

        public INamedTypeSymbol ImplementorTypeSymbol { get; protected set; }

        // all types that can be used to reference the object
        protected IEnumerable<INamedTypeSymbol> AllReferenceTypesSymbols =>
            new[] { TypeToImplementSymbol, ImplSuperClassTypeSymbol };

        // all implemented types 
        protected IEnumerable<INamedTypeSymbol> AllImplementedTypesSymbols =>
            AllReferenceTypesSymbols.Union(ImplementorTypeSymbol.ToCollection());

        public bool HasTypeImplement =>
            TypeToImplementSymbol.Name != nameof(NoType);

        public bool HasImplSuperClassType =>
            ImplSuperClassTypeSymbol.Name != nameof(NoType);

        protected void ThrowErrorIfCompleted()
        {
            if (this.ConfigurationHasBeenCompleted)
            {
                throw new Exception($"Error: one should NOT modify the TypeConfig for class '{ClassName}' once the configuration had been completed.");
            }
        }

        public void SetEventBuilder(IMemberCodeBuilder<IEventSymbol> propBuilder, params string[] eventNames)
        {
            ThrowErrorIfCompleted();

            foreach (string eventName in eventNames)
            {
                EventWrapperMemberBuilderInfo eventBuilderInfo =
                    this.EventBuilderInfos.Single(builderInfo => builderInfo.WrapperSymbolName == eventName);

                eventBuilderInfo.TheCodeBuilder = propBuilder;
            }
        }

        internal PropertyWrapperMemberBuilderInfo GetPropWrapperMemberBuilderInfo(string propName)
        {
            PropertyWrapperMemberBuilderInfo propBuilderInfo =
                this.PropBuilderInfos.Single(builderInfo => builderInfo.WrapperSymbolName == propName);

            return propBuilderInfo;
        }

        public void SetPropBuilder(IMemberCodeBuilder<IPropertySymbol> propBuilder, params string[] propNames)
        {
            ThrowErrorIfCompleted();

            foreach (string propName in propNames)
            {
                PropertyWrapperMemberBuilderInfo propBuilderInfo =
                    GetPropWrapperMemberBuilderInfo(propName);

                propBuilderInfo.TheCodeBuilder = propBuilder;
            }
        }

        public void SetMethodBuilder(IMemberCodeBuilder<IMethodSymbol> methodBuilder, params string[] methodNames)
        {
            ThrowErrorIfCompleted();

            foreach (string methodName in methodNames)
            {
                MethodWrapperMemberBuilderInfo methodBuilderInfo =
                this.MethodBuilderInfos.Single(builderInfo => builderInfo.WrapperSymbolName == methodName);

                methodBuilderInfo.TheCodeBuilder = methodBuilder;
            }
        }

        internal List<WrappedObjInfo> _wrappedObjInfos = new List<WrappedObjInfo>();

        [XmlAttribute]
        public string ClassName { get; set; }

        [XmlIgnore]
        public Core TheCore { get; }

        protected virtual void SetImplementorSymbol(INamedTypeSymbol implementorSymbol)
        {
            this.ImplementorTypeSymbol = implementorSymbol;
        }

        protected virtual void SetFromSymbols
        (
            INamedTypeSymbol typeToImplSymbol,
            INamedTypeSymbol implementationSuperClassTypeSymbol,
            INamedTypeSymbol implementorSymbol = null
        )
        {
            this.TypeToImplementSymbol = typeToImplSymbol;
            this.ImplSuperClassTypeSymbol = implementationSuperClassTypeSymbol;

            if (this.ImplSuperClassTypeSymbol == null)
            {
                this.ImplSuperClassTypeSymbol = this.GetTypeSymbol(RoslynAnalysisAndGenerationUtils.NoTypeType);
            }

            SetImplementorSymbol(implementorSymbol);

            this.ClassName = TypeToImplementSymbol.GetClassName(this.ClassName);

            if ((ImplSuperClassTypeSymbol != null) &&(ImplSuperClassTypeSymbol.TypeKind != TypeKind.Class) && (ImplSuperClassTypeSymbol.Name != nameof(NoType)))
            {
                throw new Exception($"Error: Class to extend type '{ImplSuperClassTypeSymbol.Name}' is not a class.");
            }

            if (ImplementorTypeSymbol == null)
            {
                ImplementorTypeSymbol = this.GetTypeSymbol(RoslynAnalysisAndGenerationUtils.NoTypeType);
            }

            //if ((WrapInterfaceTypeSymbol.TypeKind != TypeKind.Interface))
            //    throw new Exception($"Error: WrappedInterface type '{WrapInterfaceTypeSymbol.Name}' is not interface.");

            if ((!HasTypeImplement) && (!HasImplSuperClassType))
            {
                throw new Exception($"Error: there is neither interface to implement, no class to extend - no public members.");
            }

            IEnumerable<ISymbol> props =
                ImplementorTypeSymbol
                    .GetAllMembers()
                    .Where(symbol => symbol is IPropertySymbol);

            foreach (ISymbol prop in props)
            {
                WrappedObjInfo wrappedObjInfo =
                    new WrappedObjInfo(this.TheCore, prop.Name);

                _wrappedObjInfos.Add(wrappedObjInfo);
            }

            SetMembersFromCompilation();
        }

        public TypeConfigBySymbols
        (
            Core core,
            string className = null,
            INamedTypeSymbol typeToImplementSymbol = null,
            INamedTypeSymbol implementationSuperClassTypeSymbol = null,
            INamedTypeSymbol implementorTypeSymbol = null
        )
        {
            TheCore = core;
            this.ClassName = className;

            if (typeToImplementSymbol == null)
            {
                return;
            }

            SetFromSymbols
            (
                typeToImplementSymbol,
                implementationSuperClassTypeSymbol,
                implementorTypeSymbol
            );
        }

        internal WrappedObjInfo GetWrappedObjInfo(string wrappedObjPropName)
        {
            WrappedObjInfo wrappedObjInfo =
                this._wrappedObjInfos.FirstOrDefault(wrObjInfo => wrObjInfo.WrappedObjPropName == wrappedObjPropName);

            if (wrappedObjInfo == null)
            {
                throw new Exception($"Error no wrapped object property name '{wrappedObjPropName}' found.");
            }

            return wrappedObjInfo;
        }

        internal WrappedObjInfo GetWrappedObjInfo<TWrappedObj, TProp>(Expression<Func<TWrappedObj, TProp>> expr)
        {
            string wrappedObjName = expr.GetMemberName();

            return GetWrappedObjInfo(wrappedObjName);
        }

        public void SetAllowNonPublicForAllMembers(string wrappedObjPropName)
        {
            WrappedObjInfo wrappedObjInfo = GetWrappedObjInfo(wrappedObjPropName);

            wrappedObjInfo.AllowNonPublicForAllMemberMaps = true;
        }

        public void SetAllowNonPublicForAllMembers()
        {
            foreach (WrappedObjInfo wrappedObjInfo in this._wrappedObjInfos)
            {
                wrappedObjInfo.AllowNonPublicForAllMemberMaps = true;
            }
        }

        public ISymbol GetWrapperMemberSymbolByName(string wrapperMemberName)
        {
            IEnumerable<ISymbol> wrapperMemberSymbols = ImplementableSymbols.Where(symbol => symbol.Name == wrapperMemberName).ToList();

            if (wrapperMemberSymbols.IsNullOrEmpty())
            {
                throw new Exception($"Roxy Usage Error: no implementable symbol for member name {wrapperMemberName}");
            }
            else if (wrapperMemberSymbols.Count() > 1)
            {
                throw new Exception($"Roxy Usage Error: there is more than one implementable member corresponding to member name {wrapperMemberName}. Cannot resolve the member by name.");
            }

            ISymbol wrapperMemberSymbol = wrapperMemberSymbols.Single();

            return wrapperMemberSymbol;
        }

        public void SetMemberMap
        (
            string wrappedObjPropName,
            string wrappedMemberName,
            string wrapperMemberName,
            bool? allowNonPublic = null
        )
        {
            ThrowErrorIfCompleted();

            WrappedObjInfo wrappedObjInfo = GetWrappedObjInfo(wrappedObjPropName);

            ISymbol wrapperMemberSymbol = null;

            if (wrapperMemberName != RoslynAnalysisAndGenerationUtils.THIS)
            {
                wrapperMemberSymbol = GetWrapperMemberSymbolByName(wrapperMemberName);
            }

            wrappedObjInfo.SetMap(wrappedMemberName, wrapperMemberSymbol, allowNonPublic);
        }

        public void SetThisMemberMap
        (
            string wrappedObjPropName,
            string wrappedMemberName,
            bool? allowNonPublic = null
        )
        {
            SetMemberMap(wrappedObjPropName, wrappedMemberName, RoslynAnalysisAndGenerationUtils.THIS, allowNonPublic);
        }

        public void SetMemberMapAllowNonPublic
        (
            string wrappedObjPropName,
            params string[] wrapperMemberNames // we pass wrapper member names (not wrapped)
        )
        {
            ThrowErrorIfCompleted();

            WrappedObjInfo wrappedObjInfo = GetWrappedObjInfo(wrappedObjPropName);

            wrapperMemberNames
                .DoForEach(wrapperMemberName => SetMemberMap(wrappedObjPropName, null, wrapperMemberName, true));
        }

        public Compilation TheCompilation => TheCore.TheCompilation;

        public void AddStaticUtilsClass(string wrappedObjPropName, Type staticMethodsContainerClass)
        {
            WrappedObjInfo wrappedObjInfo =
                _wrappedObjInfos.Single(wrappedObj => wrappedObj.WrappedObjPropName == wrappedObjPropName);

            wrappedObjInfo.AddStaticMethodsContainerType(staticMethodsContainerClass);
        }

        private void SetMembersFromCompilation()
        {
            foreach (WrappedObjInfo wrappedObjInfo in this._wrappedObjInfos)
            {
                wrappedObjInfo.SetFromParentSymbol(ImplementorTypeSymbol);
            }

            SetMemberSymbols();
        }

        public void SetEventArgThisIdx(string eventName, int idx)
        {
            ThrowErrorIfCompleted();

            EventWrapperMemberBuilderInfo eventMemberInfo =
                this.EventBuilderInfos?.FirstOrDefault(evt => evt.WrapperSymbolName == eventName);

            if (eventMemberInfo != null)
                eventMemberInfo.IndexInputParamToReplaceByThis = idx;
        }


        // at this point I assume there are no overrides
        // so each name uniquely maps into the corresponding
        // property or method or event
        // I also assume that there is no name mismatch between
        // the interface and implementation class
        // i.e. if name is a property in interface, 
        // it can only be a property within the class
        public IEnumerable<ISymbol> ImplementableSymbols
        {
            get
            {
                if (this.TypeToImplementSymbol.TypeKind == TypeKind.Interface)
                {
                    return ((this.TypeToImplementSymbol?.GetAllMembers()).NullToEmpty<ISymbol>())
                                .Except
                                (
                                    this.ImplSuperClassTypeSymbol.GetAllPublicMembers(),
                                    RoslynAnalysisAndGenerationUtils.TheSymbolByNameAndSignatureComparer
                                ).Union(ImplSuperClassTypeSymbol.GetMembers().Where(member => member.IsOverridable()));
                }
                else
                {
                    return TypeToImplementSymbol.GetMembers().Where(member => member.IsOverridable());
                }
            }
        }

        void SetMemberSymbols()
        {
            this.EventBuilderInfos =
                this.ImplementableSymbols.GetSymbolsOfType<IEventSymbol>()
                    .Select(symbol => new EventWrapperMemberBuilderInfo(symbol, this.TheCore)).ToList();

            this.PropBuilderInfos =
                 this.ImplementableSymbols.GetSymbolsOfType<IPropertySymbol>()
                     .Select(symbol => new PropertyWrapperMemberBuilderInfo(symbol, this.TheCore)).ToList();

            this.MethodBuilderInfos =
                this.ImplementableSymbols.GetSymbolsOfType<IMethodSymbol>()
                    .Where(symbol => symbol.AssociatedSymbol == null)
                    .Select(symbol => new MethodWrapperMemberBuilderInfo(symbol, this.TheCore)).ToList();
        }

        void SetMissingMaps()
        {
            this._wrappedObjInfos
                .DoForEach(wrappedObjInfo => wrappedObjInfo.AddMissingMaps(ImplementableSymbols.Distinct()));
        }


        // sets wrapped member for every builder info
        void SetWrappedMembers()
        {
            this.EventBuilderInfos.Cast<WrapperMemberBuilderInfoBase>()
                .Union(PropBuilderInfos.Where(propBuilderInfo => propBuilderInfo.MustImplement))
                .Union(MethodBuilderInfos.Where(propBuilderInfo => propBuilderInfo.MustImplement))
                .DoForEach(builderInfo => builderInfo.SetWrappedMembers(GetWrappedMemberInfos(builderInfo.WrapperSymbolBase)));
        }

        public void SetOverrideVirtual(string memberName, bool includeBase = false)
        {
            WrapperMemberBuilderInfoBase builderInfoToSet =
                PropBuilderInfos.Cast<WrapperMemberBuilderInfoBase>()
                                .Union(MethodBuilderInfos).First(builderInfo => builderInfo.WrapperSymbolName == memberName);

            builderInfoToSet.SetOverrideVirtual(includeBase);
        }

        public void SetOverrideVirtualMembers(bool includeBase, params string[] memberNames)
        {
            memberNames.DoForEach(memberName => SetOverrideVirtual(memberName, includeBase));
        }

        string SuperClassName => ImplSuperClassTypeSymbol?.GetFullTypeString();

        IEnumerable<INamedTypeSymbol> AllImplementedInterfaces =>
            TypeToImplementSymbol.ToCollection();

        IEnumerable<EventWrapperMemberBuilderInfo> EventBuilderInfos { get; set; }

        IEnumerable<PropertyWrapperMemberBuilderInfo> PropBuilderInfos { get; set; }

        IEnumerable<MethodWrapperMemberBuilderInfo> MethodBuilderInfos { get; set; }

        IEnumerable<WrapperMemberBuilderInfoBase> AllWrapperMemberInfos =>
            EventBuilderInfos.NullToEmpty().Cast<WrapperMemberBuilderInfoBase>().Union(PropBuilderInfos.NullToEmpty()).Union(MethodBuilderInfos.NullToEmpty());

        void OpenClassDeclaration(RoslynCodeBuilder roslynCodeBuilder)
        {
            roslynCodeBuilder.AddClass
            (
                ClassName,
                null,
                ImplSuperClassTypeSymbol.GetNullForNoType(TheCompilation),
                true,
                AllImplementedInterfaces.ToArray()
            );
        }

        private IEnumerable<MemberMapInfoBase>
            GetWrappedMemberInfos(ISymbol wrapperSymbol)
        {
            return
                _wrappedObjInfos
                    .SelectMany(wrappedObj => wrappedObj.GetWrappedMemberInfo(wrapperSymbol).ToCollection().ToList()).ToList();
        }

        private void ImplementEvents(RoslynCodeBuilder roslynCodeBuilder)
        {
            if (this.EventBuilderInfos.IsNullOrEmpty())
                return;

            roslynCodeBuilder.PushRegion("The Wrapped Events Definitions");

            foreach (var eventBuilder in this.EventBuilderInfos)
            {
                eventBuilder.Build(roslynCodeBuilder);
            }

            roslynCodeBuilder.PopRegion();
        }

        private void AddWrappedClasses(RoslynCodeBuilder roslynCodeBuilder)
        {
            if (this._wrappedObjInfos.IsNullOrEmpty())
                return;

            roslynCodeBuilder.PushRegion("Wrapped Classes");

            foreach (WrappedObjInfo wrappedClassInfo in this._wrappedObjInfos)
            {
                wrappedClassInfo.AddWrappedClass(roslynCodeBuilder);
            }

            roslynCodeBuilder.PopRegion();
        }

        private void AddPropWraps(RoslynCodeBuilder roslynCodeBuilder)
        {
            if (PropBuilderInfos.IsNullOrEmpty())
                return;

            roslynCodeBuilder.PushRegion("Generated Properties");

            foreach (WrapperMemberBuilderInfo<IPropertySymbol> propBuilderInfo in this.PropBuilderInfos)
            {
                propBuilderInfo.Build(roslynCodeBuilder);
            }

            roslynCodeBuilder.PopRegion();
        }

        private void AddMethodWraps(RoslynCodeBuilder roslynCodeBuilder)
        {
            if (MethodBuilderInfos.IsNullOrEmpty())
                return;

            roslynCodeBuilder.PushRegion("Wrapped Methods");

            foreach (WrapperMemberBuilderInfo<IMethodSymbol> methodBuilderInfo in this.MethodBuilderInfos)
            {
                methodBuilderInfo.Build(roslynCodeBuilder);
            }

            roslynCodeBuilder.PopRegion();
        }

        private void AddInit(RoslynCodeBuilder roslynCodeBuilder)
        {
            roslynCodeBuilder.PushRegion("Init Method");

            roslynCodeBuilder.AddLine($"void {INIT_METHOD_NAME}()");
            roslynCodeBuilder.Push();

            this.PropBuilderInfos.DoForEach(propBuilderInfo => propBuilderInfo.AddInit(roslynCodeBuilder));

            roslynCodeBuilder.Pop();

            roslynCodeBuilder.PopRegion();
        }

        private void AddConstructor(RoslynCodeBuilder roslynCodeBuilder)
        {
            roslynCodeBuilder.PushRegion("Constructor");

           var wrappedObjInfosInitializedThroughConstructor = 
                _wrappedObjInfos.Where(wrappedObj => wrappedObj.InitializedThroughConstructor).ToList();


            //roslynCodeBuilder
            //    .AddConstructorOpening
            //    (
            //        this.ClassName,
            //        Accessibility.Public
            //    );

            roslynCodeBuilder
                .AddConstructorOpening
                (
                    this.ClassName,
                    Accessibility.Public,
                    wrappedObjInfosInitializedThroughConstructor.Select(wrappedObj => wrappedObj.WrappedObjPropSymbol).ToArray()
                );

            foreach(WrappedObjInfo wrappedObjInitializedThroughConstructor in wrappedObjInfosInitializedThroughConstructor)
            {
                string propName = wrappedObjInitializedThroughConstructor.WrappedObjPropName;
                string assignName = propName.FirstCharToLowerCase();
                roslynCodeBuilder.AddAssignmentLine(propName, assignName);
            }

            foreach (WrappedObjInfo wrappedObj in _wrappedObjInfos)
            {
                wrappedObj.AddWrappedObjDefaultConstructorInitialization(roslynCodeBuilder);
            }

            roslynCodeBuilder.AddLine($"{INIT_METHOD_NAME}()", true);

            roslynCodeBuilder.Pop();

            roslynCodeBuilder.PopRegion();
        }

        private string GetWrappedObjConstructorParamStr()
        {
            return _wrappedObjInfos.StrConcat((wrappedObjInfo) => $"{wrappedObjInfo.ConcreteWrappedObjNamedTypeSymbol.GetFullTypeString()} {wrappedObjInfo.WrappedObjClassName.FirstCharToLowerCase(true)}");
        }

        void AddWrappedObjsConstructor(RoslynCodeBuilder roslynCodeBuilder)
        {
            string paramsLine = GetWrappedObjConstructorParamStr();

            if (paramsLine.IsNullOrEmpty())
                return;

            roslynCodeBuilder.PushRegion("Wrappers Constructor");

            roslynCodeBuilder.AddLine($"public {this.ClassName}({paramsLine})");
            roslynCodeBuilder.Push();

            foreach (WrappedObjInfo wrapObjInfo in _wrappedObjInfos)
            {
                string assignmentLine =
                    $"{wrapObjInfo.WrappedObjPropName} = {wrapObjInfo.WrappedObjClassName.FirstCharToLowerCase(true)}";

                roslynCodeBuilder.AddLine(assignmentLine, true);
            }

            roslynCodeBuilder.AddLine($"{INIT_METHOD_NAME}()", true);

            roslynCodeBuilder.Pop(true);

            roslynCodeBuilder.PopRegion();
        }

        public const string STATIC_CORE_MEMBER_NAME = "TheCore";

        protected void AddStaticCoreReference(RoslynCodeBuilder roslynCodeBuilder)
        {
            roslynCodeBuilder.AddLine($"public static Core {STATIC_CORE_MEMBER_NAME} {{ get; set; }}");
        }

        protected virtual string GenerateCode()
        {
            Compilation compilation = this.TheCompilation;

            SetMissingMaps();

            SetWrappedMembers();

            RoslynCodeBuilder roslynCodeBuilder = new RoslynCodeBuilder();
            RoslynAnalysisAndGenerationUtils.TheNamespaces =
                roslynCodeBuilder.AllNamespaces;

            RoslynAnalysisAndGenerationUtils.TheNamespaces.Add("NP.Utilities");
            RoslynAnalysisAndGenerationUtils.TheNamespaces.Add("NP.Roxy");
            RoslynAnalysisAndGenerationUtils.TheNamespaces.Add("NP.Roxy.Attributes");

            RoslynAnalysisAndGenerationUtils.InitTypeNamespace<Action<int>>();

            roslynCodeBuilder.AddNamespace(AssemblerNames.GENERATED_NAMESPACE_NAME);

            OpenClassDeclaration(roslynCodeBuilder);

            AddStaticCoreReference(roslynCodeBuilder);

            ImplementEvents(roslynCodeBuilder);

            AddWrappedClasses(roslynCodeBuilder);

            AddInit(roslynCodeBuilder);

            AddConstructor(roslynCodeBuilder);

            //AddWrappedObjsConstructor(roslynCodeBuilder);

            AddPropWraps(roslynCodeBuilder);

            AddMethodWraps(roslynCodeBuilder);

            roslynCodeBuilder.PopAll();

            TheGeneratedCode = roslynCodeBuilder.ToStr();

            return TheGeneratedCode;
        }

        // adds the class to compilation.
        public void ConfigurationCompleted()
        {
            if (TheSelfTypeSymbol != null)
                return;

            this.GenerateCode();

            TheCore.AddClass(this);

            TheSelfTypeSymbol =
                TheCore.TheCompilation.GetTypeByMetadataName(FullTypeName);
        }

        public bool ConfigurationHasBeenCompleted =>
            TheSelfTypeSymbol != null;

        protected string FullTypeName => this.ClassName.GetFullTypeName();

        public INamedTypeSymbol TheSelfTypeSymbol { get; private set; }

        public string TheGeneratedCode { get; protected set; }


        Type _generatedType = null;
        public Type TheGeneratedType
        {
            get => _generatedType;

            private set
            {
                if (_generatedType == value)
                    return;

                _generatedType = value;

                PostTypeSet();
            }
        }

        protected virtual void PostTypeSet()
        {
            TheGeneratedType.SetStaticPropValue(STATIC_CORE_MEMBER_NAME, TheCore);
        }

        public void SetGeneratedType()
        {
            //if (TheGeneratedType != null)
            //    return;

            this.TheGeneratedType =
                this.TheCore.TheGeneratedAssembly.GetType(this.FullTypeName);
        }

        IMemberBuilderSetter _builderSetter;
        public IMemberBuilderSetter TheBuilderSetter
        {
            get => _builderSetter;

            set
            {
                if (ReferenceEquals(_builderSetter, value))
                    return;

                _builderSetter = value;

                foreach (PropertyWrapperMemberBuilderInfo propBuilderInfo in this.PropBuilderInfos)
                {
                    propBuilderInfo.TheCodeBuilder =
                        _builderSetter.ChoosePropertyCodeBuilder(propBuilderInfo.WrapperSymbol);
                }

                foreach (MethodWrapperMemberBuilderInfo methodBuilderInfo in this.MethodBuilderInfos)
                {
                    methodBuilderInfo.TheCodeBuilder =
                        _builderSetter.ChooseMethodCodeBuilder(methodBuilderInfo.WrapperSymbol);
                }
            }
        }

        void TestObjPartTypes<TObj, TPart>()
        {
            INamedTypeSymbol objSymbol = typeof(TObj).GetTypeSymbol(TheCompilation);

            //if (this.WrapInterfaceTypeSymbol)

            //INamedTypeSymbol partSymbol = typeof(TPart).GetTypeSymbol(TheCompilation);
        }

        public void SetInit
        (
            string propName,
            INamedTypeSymbol typeSymbol
        )
        {
            PropertyWrapperMemberBuilderInfo propertyWrapperMemberBuilder =
                GetPropWrapperMemberBuilderInfo(propName);

            propertyWrapperMemberBuilder.SetInit(typeSymbol);
        }

        public void SetInit<TInit>(string propName)
        {
            INamedTypeSymbol typeSymbol = typeof(TInit).GetTypeSymbol(this.TheCompilation);

            SetInit(propName, typeSymbol);
        }
    }

    public class TypeConfigBySymbols<TToImplement, TImplementor> : 
        TypeConfigBySymbols, 
        ITypeConfig<TToImplement, TImplementor>, 
        ICompilationContainer
    {
        public TypeConfigBySymbols
        (
            Core core, 
            string className = null, 
            INamedTypeSymbol implInterfaceTypeSymbol = null, 
            INamedTypeSymbol superClassTypeSymbol = null, 
            INamedTypeSymbol implementorTypeSymbol = null
        ) :     
            base
            (
                core, 
                className, 
                implInterfaceTypeSymbol, 
                superClassTypeSymbol, 
                implementorTypeSymbol
            )
        {

        }

        private PropertyWrapperMemberBuilderInfo GetPropWrapperMemberBuilderInfoByExpr<TImpl, TProp>(Expression<Func<TImpl, TProp>> propNameGetter)
        {
            string propName = propNameGetter.GetMemberName();

            Type implType = typeof(TImpl);

            INamedTypeSymbol implTypeSymbol =
                implType.GetTypeSymbol(this.TheCompilation);

            if (!this.AllReferenceTypesSymbols.Contains(implTypeSymbol))
            {
                string errorMessage =
                    $"Roxy Usage Error: referenced interface {TypeToImplementSymbol.Name} and class {ImplSuperClassTypeSymbol}" +
                    $" do not include property {propName} container {implTypeSymbol.Name}";
                throw new Exception(errorMessage);
            }

            PropertyWrapperMemberBuilderInfo propBuilderInfo =
                this.GetPropWrapperMemberBuilderInfo(propName);

            return propBuilderInfo;
        }

        protected virtual INamedTypeSymbol GetImplementorSymbol(INamedTypeSymbol implementorSymbol)
        {
            if (implementorSymbol.Matches(typeof(NoType), this.TheCompilation))
            {
                return TheCore.GetDefaultWrapper(this.TypeToImplementSymbol);
            }
            else
            {
                return implementorSymbol;
            }
        }

        protected override void SetImplementorSymbol(INamedTypeSymbol implementorSymbol)
        {
            INamedTypeSymbol genericArgImplementorTypeSymbol =
                 typeof(TImplementor).GetTypeSymbol(this.TheCompilation);

            if (implementorSymbol == null)
            {
                implementorSymbol = genericArgImplementorTypeSymbol;
            }

            this.ImplementorTypeSymbol = implementorSymbol;

            TheCore.AddTypeSymbolsToReference(AllImplementedTypesSymbols);

            implementorSymbol = GetImplementorSymbol(implementorSymbol);

            if (!implementorSymbol.IsSelfOrSuperType(genericArgImplementorTypeSymbol))
            {
                throw new Exception($"Roxy usage error: {implementorSymbol.GetFullTypeString()} does not derive from {genericArgImplementorTypeSymbol.GetFullTypeString()}.");
            }

            this.ImplementorTypeSymbol = implementorSymbol;
        }


        public void SetPropGetter<TProp>
        (
            Expression<Func<TToImplement, TProp>> propNameGetter,
            Expression<Func<TToImplement, TProp>> propGetter
        )
        {
            PropertyWrapperMemberBuilderInfo propBuilderInfo =
                GetPropWrapperMemberBuilderInfoByExpr(propNameGetter);

            propBuilderInfo.SetPropGetter(propGetter);
        }


        public void SetWrappedPropGetter<TWrappedObj, TProp>
        (
            string propName,
            Expression<Func<TImplementor, TWrappedObj>> wrappedObjChooser,
            Expression<Func<TWrappedObj, TProp>> propGetter
        )
        {
            ThrowErrorIfCompleted();
            WrappedObjInfo wrappedObjInfo = GetWrappedObjInfo(wrappedObjChooser);

            ISymbol wrapperMemberSymbol = GetWrapperMemberSymbolByName(propName);

            wrappedObjInfo.SetExpressionMemberMap<TWrappedObj>(wrapperMemberSymbol, propGetter);
        }


        public void SetWrappedPropGetter<TWrappedObj, TProp>
        (
            Expression<Func<TToImplement, TProp>> propNameGetter,
            Expression<Func<TImplementor, TWrappedObj>> wrappedObjChooser,
            Expression<Func<TWrappedObj, TProp>> propGetter
        )
        {
            this.SetWrappedPropGetter<TWrappedObj, TProp>
            (
                propNameGetter.GetMemberName(), 
                wrappedObjChooser, 
                propGetter
            );
        }

        public void SetVoidMethod<TWrappedObj>
        (
             Expression<Func<TToImplement>> methodNameGetter,
             Expression<Func<TImplementor, TWrappedObj>> wrappedObjChooser,
             Expression<Action<TWrappedObj>> wrappedMethod
        )
        {

        }

        private void SetReturningMethodMapImpl<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TOut>
        (
             LambdaExpression methodNameGetter,
             Expression<Func<TImplementor, TWrappedObj>> wrappedObjChooser,
             Expression wrappedMethod
        )
        {
            ThrowErrorIfCompleted();
            WrappedObjInfo wrappedObjInfo = GetWrappedObjInfo(wrappedObjChooser);

            IMethodSymbol callingMethodSymbol =
                this.TheCompilation.FindMatchingMethodSymbol<TToImplement, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TOut>(methodNameGetter.GetMemberName());

            wrappedObjInfo.SetExpressionMemberMap<TWrappedObj>(callingMethodSymbol, wrappedMethod);
        }

        public void SetReturningMethodMap<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TOut>
        (
             Expression<Func<TToImplement, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TOut>> methodNameGetter,
             Expression<Func<TImplementor, TWrappedObj>> wrappedObjChooser,
             Expression<Func<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TOut>> wrappedMethod
        )
            => SetReturningMethodMapImpl<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TOut>(methodNameGetter, wrappedObjChooser, wrappedMethod);

        public void SetVoidMethodMap<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9>
        (
             Expression<Func<TToImplement, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9>> methodNameGetter,
             Expression<Func<TImplementor, TWrappedObj>> wrappedObjChooser,
             Expression<Func<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9>> wrappedMethod
        )
            => SetReturningMethodMapImpl<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, NoType>(methodNameGetter, wrappedObjChooser, wrappedMethod);


        public void SetReturningMethodMap<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TOut>
        (
             Expression<Func<TToImplement, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TOut>> methodNameGetter,
             Expression<Func<TImplementor, TWrappedObj>> wrappedObjChooser,
             Expression<Func<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TOut>> wrappedMethod
        )
            => SetReturningMethodMapImpl<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, NoType, TOut>(methodNameGetter, wrappedObjChooser, wrappedMethod);

        public void SetVoidMethodMap<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8>
        (
             Expression<Func<TToImplement, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8>> methodNameGetter,
             Expression<Func<TImplementor, TWrappedObj>> wrappedObjChooser,
             Expression<Func<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8>> wrappedMethod
        )
            => SetReturningMethodMapImpl<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, NoType, NoType>(methodNameGetter, wrappedObjChooser, wrappedMethod);


        public void SetReturningMethodMap<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TOut>
        (
             Expression<Func<TToImplement, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TOut>> methodNameGetter,
             Expression<Func<TImplementor, TWrappedObj>> wrappedObjChooser,
             Expression<Func<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TOut>> wrappedMethod
        )
            => SetReturningMethodMapImpl<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, NoType, NoType, TOut>(methodNameGetter, wrappedObjChooser, wrappedMethod);

        public void SetVoidMethodMap<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7>
        (
             Expression<Func<TToImplement, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7>> methodNameGetter,
             Expression<Func<TImplementor, TWrappedObj>> wrappedObjChooser,
             Expression<Func<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7>> wrappedMethod
        )
            => SetReturningMethodMapImpl<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, NoType, NoType, NoType>(methodNameGetter, wrappedObjChooser, wrappedMethod);

        public void SetReturningMethodMap<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TOut>
        (
             Expression<Func<TToImplement, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TOut>> methodNameGetter,
             Expression<Func<TImplementor, TWrappedObj>> wrappedObjChooser,
             Expression<Func<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TOut>> wrappedMethod
        )
            => SetReturningMethodMapImpl<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, NoType, NoType, NoType, TOut>(methodNameGetter, wrappedObjChooser, wrappedMethod);

        public void SetVoidMethodMap<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6>
        (
             Expression<Func<TToImplement, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6>> methodNameGetter,
             Expression<Func<TImplementor, TWrappedObj>> wrappedObjChooser,
             Expression<Func<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6>> wrappedMethod
        )
            => SetReturningMethodMapImpl<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, NoType, NoType, NoType, NoType>(methodNameGetter, wrappedObjChooser, wrappedMethod);


        public void SetReturningMethodMap<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5, TOut>
        (
             Expression<Func<TToImplement, TIn1, TIn2, TIn3, TIn4, TIn5, TOut>> methodNameGetter,
             Expression<Func<TImplementor, TWrappedObj>> wrappedObjChooser,
             Expression<Func<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5, TOut>> wrappedMethod
        )
            => SetReturningMethodMapImpl<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5, NoType, NoType, NoType, NoType, TOut>(methodNameGetter, wrappedObjChooser, wrappedMethod);

        public void SetVoidMethodMap<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5>
        (
             Expression<Func<TToImplement, TIn1, TIn2, TIn3, TIn4, TIn5>> methodNameGetter,
             Expression<Func<TImplementor, TWrappedObj>> wrappedObjChooser,
             Expression<Func<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5>> wrappedMethod
        )
            => SetReturningMethodMapImpl<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TIn5, NoType, NoType, NoType, NoType, NoType>(methodNameGetter, wrappedObjChooser, wrappedMethod);


        public void SetReturningMethodMap<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TOut>
        (
             Expression<Func<TToImplement, TIn1, TIn2, TIn3, TIn4, TOut>> methodNameGetter,
             Expression<Func<TImplementor, TWrappedObj>> wrappedObjChooser,
             Expression<Func<TWrappedObj, TIn1, TIn2, TIn3, TIn4, TOut>> wrappedMethod
        )
            => SetReturningMethodMapImpl<TWrappedObj, TIn1, TIn2, TIn3, TIn4, NoType, NoType, NoType, NoType, NoType, TOut>(methodNameGetter, wrappedObjChooser, wrappedMethod);

        public void SetVoidMethodMap<TWrappedObj, TIn1, TIn2, TIn3, TIn4>
        (
             Expression<Func<TToImplement, TIn1, TIn2, TIn3, TIn4>> methodNameGetter,
             Expression<Func<TImplementor, TWrappedObj>> wrappedObjChooser,
             Expression<Func<TWrappedObj, TIn1, TIn2, TIn3, TIn4>> wrappedMethod
        )
            => SetReturningMethodMapImpl<TWrappedObj, TIn1, TIn2, TIn3, TIn4, NoType, NoType, NoType, NoType, NoType, NoType>(methodNameGetter, wrappedObjChooser, wrappedMethod);


        public void SetReturningMethodMap<TWrappedObj, TIn1, TIn2, TIn3, TOut>
        (
             Expression<Func<TToImplement, TIn1, TIn2, TIn3, TOut>> methodNameGetter,
             Expression<Func<TImplementor, TWrappedObj>> wrappedObjChooser,
             Expression<Func<TWrappedObj, TIn1, TIn2, TIn3, TOut>> wrappedMethod
        )
            => SetReturningMethodMapImpl<TWrappedObj, TIn1, TIn2, TIn3, NoType, NoType, NoType, NoType, NoType, NoType, TOut>(methodNameGetter, wrappedObjChooser, wrappedMethod);

        public void SetVoidMethodMap<TWrappedObj, TIn1, TIn2, TIn3>
        (
             Expression<Func<TToImplement, TIn1, TIn2, TIn3>> methodNameGetter,
             Expression<Func<TImplementor, TWrappedObj>> wrappedObjChooser,
             Expression<Func<TWrappedObj, TIn1, TIn2, TIn3>> wrappedMethod
        )
            => SetReturningMethodMapImpl<TWrappedObj, TIn1, TIn2, TIn3, NoType, NoType, NoType, NoType, NoType, NoType, NoType>(methodNameGetter, wrappedObjChooser, wrappedMethod);

        public void SetReturningMethodMap<TWrappedObj, TIn1, TIn2, TOut>
        (
             Expression<Func<TToImplement, TIn1, TIn2, TOut>> methodNameGetter,
             Expression<Func<TImplementor, TWrappedObj>> wrappedObjChooser,
             Expression<Func<TWrappedObj, TIn1, TIn2, TOut>> wrappedMethod
        )
            => SetReturningMethodMapImpl<TWrappedObj, TIn1, TIn2, NoType, NoType, NoType, NoType, NoType, NoType, NoType, TOut>(methodNameGetter, wrappedObjChooser, wrappedMethod);

        public void SetVoidMethodMap<TWrappedObj, TIn1, TIn2>
        (
             Expression<Func<TToImplement, TIn1, TIn2>> methodNameGetter,
             Expression<Func<TImplementor, TWrappedObj>> wrappedObjChooser,
             Expression<Func<TWrappedObj, TIn1, TIn2>> wrappedMethod
        )
            => SetReturningMethodMapImpl<TWrappedObj, TIn1, TIn2, NoType, NoType, NoType, NoType, NoType, NoType, NoType, NoType>(methodNameGetter, wrappedObjChooser, wrappedMethod);

        public void SetReturningMethodMap<TWrappedObj, TIn1, TOut>
        (
             Expression<Func<TToImplement, TIn1, TOut>> methodNameGetter,
             Expression<Func<TImplementor, TWrappedObj>> wrappedObjChooser,
             Expression<Func<TWrappedObj, TIn1, TOut>> wrappedMethod
        )
            => SetReturningMethodMapImpl<TWrappedObj, TIn1, NoType, NoType, NoType, NoType, NoType, NoType, NoType, NoType, TOut>(methodNameGetter, wrappedObjChooser, wrappedMethod);


        public void SetVoidMethodMap<TWrappedObj, TIn1>
        (
             Expression<Func<TToImplement, TIn1>> methodNameGetter,
             Expression<Func<TImplementor, TWrappedObj>> wrappedObjChooser,
             Expression<Func<TWrappedObj, TIn1>> wrappedMethod
        )
            => SetReturningMethodMapImpl<TWrappedObj, TIn1, NoType, NoType, NoType, NoType, NoType, NoType, NoType, NoType, NoType>(methodNameGetter, wrappedObjChooser, wrappedMethod);


        public void SetReturningMethodMap<TWrappedObj, TOut>
        (
             Expression<Func<TToImplement, TOut>> methodNameGetter,
             Expression<Func<TImplementor, TWrappedObj>> wrappedObjChooser,
             Expression<Func<TWrappedObj, TOut>> wrappedMethod
        )
            => SetReturningMethodMapImpl<TWrappedObj, NoType, NoType, NoType, NoType, NoType, NoType, NoType, NoType, NoType, TOut>(methodNameGetter, wrappedObjChooser, wrappedMethod);


        public void SetVoidMethodMap<TWrappedObj>
        (
             Expression<Func<TToImplement>> methodNameGetter,
             Expression<Func<TImplementor, TWrappedObj>> wrappedObjChooser,
             Expression<Func<TWrappedObj>> wrappedMethod
        )
            => SetReturningMethodMapImpl<TWrappedObj, NoType, NoType, NoType, NoType, NoType, NoType, NoType, NoType, NoType, NoType>(methodNameGetter, wrappedObjChooser, wrappedMethod);


        public void SetPropMemberMap<TImplementer, TWrappedObj, TWrapperProp>
        (
            Expression<Func<TImplementor, TWrappedObj>> wrappedObjChooser, 
            Expression<Func<TWrappedObj, TWrapperProp>> wrappedPropChooser, 
            Expression<Func<TImplementer, TWrapperProp>> wrapperPropChooser)
        {
            string wrappedObjPropName = wrappedObjChooser.GetMemberName();
            string wrappedMemberName = wrappedPropChooser.GetMemberName();
            string wrapperMemberName = wrapperPropChooser.GetMemberName();
            SetMemberMap(wrappedObjPropName, wrappedMemberName, wrapperMemberName);
        }
    }


    internal class TypeConfig<TToImplement, TImplementor> : 
        TypeConfigBySymbols<TToImplement, TImplementor>        
    {
        public Type TypeToImplement { get; private set; }

        public Type ImplementationSuperClassType { get; private set; }

        public Type WrapInterfaceType { get; private set; }

        public TypeConfig
        (
            Core core,
            string className = null,
            Type implementationSuperClassType = null,
            Type wrapInterfaceType = null
        )
            : base(core, className)
        {
            TypeToImplement = typeof(TToImplement);
            ImplementationSuperClassType = implementationSuperClassType.GetRealRoxyType();
            WrapInterfaceType = wrapInterfaceType.GetRealRoxyType();

            if (TypeToImplement.IsClass && ImplementationSuperClassType.IsClass)
            {
                throw new Exception($"Roxy Usage Error: since type to implement '{TypeToImplement.Name}' is a class, the super class '{ImplementationSuperClassType.Name}' cannot be used");
            }

            this.TheCore.AddTypesToReference(ReferencedTypes);

            this.SetFromSymbols
            (
                TypeToImplement.GetTypeSymbol(TheCompilation),
                ImplementationSuperClassType.GetTypeSymbol(TheCompilation), 
                WrapInterfaceType.GetTypeSymbol(TheCompilation)
            );
        }

        internal Type[] ReferencedTypes =>
            new[] { TypeToImplement, ImplementationSuperClassType, WrapInterfaceType };
    }

    internal class InterfaceMergingTypeConfig<TToImplement> : TypeConfig<TToImplement, NoType>
    {
        protected override void SetFromSymbols
        (
            INamedTypeSymbol typeToImplSymbol, 
            INamedTypeSymbol implementationSuperClassTypeSymbol, 
            INamedTypeSymbol implementorSymbol = null)
        {
            
        }

        public INamedTypeSymbol[] AllInterfaceTypeSymbolsToMerge { get; }

        public InterfaceMergingTypeConfig
        (
            Core core,
            string className,
            IEnumerable<INamedTypeSymbol> allInterfaceTypeSymbolsToMerge
        )
            : base(core, className)
        {
            AllInterfaceTypeSymbolsToMerge = allInterfaceTypeSymbolsToMerge.ToArray();
        }

        protected override INamedTypeSymbol GetImplementorSymbol(INamedTypeSymbol implementorSymbol)
        {
            return implementorSymbol;
        }

        protected override string GenerateCode()
        {
            RoslynCodeBuilder roslynCodeBuilder = new RoslynCodeBuilder();
            RoslynAnalysisAndGenerationUtils.TheNamespaces =
                roslynCodeBuilder.AllNamespaces;

            roslynCodeBuilder.AddNamespace(AssemblerNames.GENERATED_NAMESPACE_NAME);

            roslynCodeBuilder.AddClass(this.ClassName, null, null, false, this.AllInterfaceTypeSymbolsToMerge);

            roslynCodeBuilder.PopAll();

            TheGeneratedCode = roslynCodeBuilder.ToStr();

            return TheGeneratedCode;
        }

        protected override void PostTypeSet()
        {
            
        }
    }


    internal class TypeConfig<TToImplement, TImplementationSuperClass, TImplementor> :
        TypeConfig<TToImplement, TImplementor>,
        ITypeConfig<TToImplement, TImplementationSuperClass, TImplementor>
    {
        public TypeConfig(Core core, string className = null) : 
            base(core, className, typeof(TImplementationSuperClass), typeof(TImplementor))
        {
        }
    }

    public static class TypeConfigExtensions
    {
        public static void SetWrappedPropGetter<TImpl, TWrappedObj, TProp>
        (
            this ITypeConfig<TImpl, SingleWrapperInterface<TWrappedObj>> typeConfig,
            string propName,
            Expression<Func<TWrappedObj, TProp>> propGetter
        )
        {
            typeConfig.SetWrappedPropGetter<TWrappedObj, TProp>
            (
                propName,
                (singleWrapperInt) => singleWrapperInt.TheWrappedType,
                propGetter
            );
        }

        public static void SetWrappedPropGetter<TImpl, TWrappedObj, TProp>
        (
            this ITypeConfig<TImpl, SingleWrapperInterface<TWrappedObj>> typeConfig,
            Expression<Func<TImpl, TProp>> propNameGetter,
            Expression<Func<TWrappedObj, TProp>> propGetter
        )
        {
            typeConfig.SetWrappedPropGetter<TWrappedObj, TProp>
            (
                propNameGetter.GetMemberName(),
                (singleWrapperInt) => singleWrapperInt.TheWrappedType,
                propGetter
            );
        }
    }
}
