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

        INamedTypeSymbol ImplInterfaceTypeSymbol { get; }

        INamedTypeSymbol SuperClassTypeSymbol { get; }

        INamedTypeSymbol TheSelfTypeSymbol { get; }

        IEnumerable<Assembly> ReferencedAssemblies { get; }

        string TheGeneratedCode { get; }

        void SetEventArgThisIdx(string eventName, int idx);

        void SetPropBuilder(string propName, IMemberCodeBuilder<IPropertySymbol> propBuilder);

        void SetMethodBuilder(string propName, IMemberCodeBuilder<IMethodSymbol> propBuilder);

        void SetOverrideVirtual(string memberName, bool includeBase = false);

        void AddStaticUtilsClass
        (
            string wrappedObjPropName, 
            Type staticMethodsContainerClass
        );

        void SetAllowNonPublicForAllMembers(string wrappedObjPropName);

        void SetPropMap
        (
            string wrappedObjPropName, 
            string wrappedMemberName, 
            string wrapperMemberName, 
            bool? allowNonPublic = null);

        void SetPropMapAllowNonPublic
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
    }

    public interface ITypeConfig<TImplementedInterface, TSuperClass, TWrappedInterface> : ITypeConfig
    {
        Action<TImplementedInterface, TSuperClass, TWrappedInterface> UnInitAction { get; set; }

        Action<TImplementedInterface, TSuperClass, TWrappedInterface> InitAction { get; set; }
    }

    public class TypeConfigBase
    {
        internal const string STATIC_UNINIT_LAMBDA_NAME = "___StaticUnInit";

        internal const string STATIC_INIT_LAMBDA_NAME = "___StaticInit";

        internal const string CALL_STATIC_UNINIT_METHOD = "___Call__StaticUnInit";
        internal const string CALL_STATIC_INIT_METHOD = "___Call__StaticInit";
    }

    public class TypeConfigBySymbols : TypeConfigBase, ITypeConfig
    {
        public INamedTypeSymbol ImplInterfaceTypeSymbol { get; private set; }

        public INamedTypeSymbol SuperClassTypeSymbol { get; private set; }

        public INamedTypeSymbol WrapInterfaceTypeSymbol { get; private set; }

        public bool HasInterfaceToImplement =>
            ImplInterfaceTypeSymbol.Name != nameof(NoInterface);

        public bool HasClassToExtend =>
            SuperClassTypeSymbol.Name != nameof(NoClass);

        void ThrowErrorIfCompleted()
        {
            if (this.ConfigurationHasBeenCompleted)
            {
                throw new Exception($"Error: one should NOT modify the TypeConfig for class '{ClassName}' once the configuration had been completed.");
            }
        }

        public void SetPropBuilder(string propName, IMemberCodeBuilder<IPropertySymbol> propBuilder)
        {
            ThrowErrorIfCompleted();

            PropertyWrapperMemberBuilderInfo propBuilderInfo =
                this.PropBuilderInfos.Single(builderInfo => builderInfo.WrapperSymbolName == propName);

            propBuilderInfo.TheCodeBuilder = propBuilder;
        }

        public void SetMethodBuilder(string methodName, IMemberCodeBuilder<IMethodSymbol> methodBuilder)
        {
            ThrowErrorIfCompleted();

            MethodWrapperMemberBuilderInfo methodBuilderInfo =
                this.MethodBuilderInfos.Single(builderInfo => builderInfo.WrapperSymbolName == methodName);

            methodBuilderInfo.TheCodeBuilder = methodBuilder;
        }

        internal List<WrappedObjInfo> _wrappedObjInfos = new List<WrappedObjInfo>();

        [XmlAttribute]
        public string ClassName { get; set; }

        [XmlIgnore]
        public Core TheCore { get; }

        protected void SetFromSymbols
        (
            INamedTypeSymbol implInterfaceTypeSymbol,
            INamedTypeSymbol superClassTypeSymbol,
            INamedTypeSymbol wrapInterfaceTypeSymbol
        )
        {
            this.ImplInterfaceTypeSymbol = implInterfaceTypeSymbol;
            this.SuperClassTypeSymbol = superClassTypeSymbol;
            this.WrapInterfaceTypeSymbol = wrapInterfaceTypeSymbol;

            this.ClassName = ImplInterfaceTypeSymbol.GetClassName(this.ClassName);

            if (ImplInterfaceTypeSymbol.TypeKind != TypeKind.Interface)
                throw new Exception($"Error: ImplementedInterface type '{ImplInterfaceTypeSymbol.Name}' is not interface");

            if (SuperClassTypeSymbol.TypeKind != TypeKind.Class)
                throw new Exception($"Error: Class to extend type '{SuperClassTypeSymbol.Name}' is not a class");

            if (WrapInterfaceTypeSymbol.TypeKind != TypeKind.Interface)
                throw new Exception($"Error: WrappedInterface type '{WrapInterfaceTypeSymbol.Name}' is not interface");

            if ((!HasInterfaceToImplement) && (!HasClassToExtend))
            {
                throw new Exception($"Error: there is neither interface to implement, no class to extend - no public members");
            }

            IEnumerable<ISymbol> props =
                WrapInterfaceTypeSymbol
                    .GetMembers()
                    .Where(symbol => symbol is IPropertySymbol);

            foreach (ISymbol prop in props)
            {
                WrappedObjInfo wrappedObjInfo =
                    new WrappedObjInfo { WrappedObjPropName = prop.Name, TheCore = this.TheCore };

                _wrappedObjInfos.Add(wrappedObjInfo);
            }

            SetMembersFromCompilation();
        }

        public TypeConfigBySymbols
        (
            Core core,
            string className = null,
            INamedTypeSymbol implInterfaceTypeSymbol = null,
            INamedTypeSymbol superClassTypeSymbol = null,
            INamedTypeSymbol wrapInterfaceTypeSymbol = null
        )
        {
            TheCore = core;
            this.ClassName = className;

            if ( (implInterfaceTypeSymbol == null) && 
                 (superClassTypeSymbol == null)  &&
                 (wrapInterfaceTypeSymbol == null) )
            {
                return;
            }

            SetFromSymbols
            (
                implInterfaceTypeSymbol, 
                superClassTypeSymbol, 
                wrapInterfaceTypeSymbol
            );
        }

        WrappedObjInfo GetWrappedObjInfo(string wrappedObjPropName)
        {
            WrappedObjInfo wrappedObjInfo =
                this._wrappedObjInfos.FirstOrDefault(wrObjInfo => wrObjInfo.WrappedObjPropName == wrappedObjPropName);

            if (wrappedObjInfo == null)
            {
                throw new Exception($"Error no wrapped object property name '{wrappedObjPropName}' found.");
            }

            return wrappedObjInfo;
        }

        public void SetAllowNonPublicForAllMembers(string wrappedObjPropName)
        {
            WrappedObjInfo wrappedObjInfo = GetWrappedObjInfo(wrappedObjPropName);

            wrappedObjInfo.AllowNonPublicForAllMemberMaps = true;
        }


        public void SetPropMap
        (
            string wrappedObjPropName, 
            string wrappedMemberName, 
            string wrapperMemberName,
            bool? allowNonPublic = null
        )
        {
            ThrowErrorIfCompleted();

            WrappedObjInfo wrappedObjInfo = GetWrappedObjInfo(wrappedObjPropName);

            wrappedObjInfo.SetMap(wrappedMemberName, wrapperMemberName, allowNonPublic);
        }


        public void SetPropMapAllowNonPublic
        (
            string wrappedObjPropName, 
            params string[] wrapperMemberNames // we pass wrapper member names (not wrapped)
        )
        {
            ThrowErrorIfCompleted();

            WrappedObjInfo wrappedObjInfo = GetWrappedObjInfo(wrappedObjPropName);

            wrapperMemberNames
                .DoForEach(wrapperMemberName => SetPropMap(wrappedObjPropName, null, wrapperMemberName, true));
        }

        protected Compilation TheCompilation => TheCore.TheCompilation;

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
                wrappedObjInfo.SetFromParentSymbol(WrapInterfaceTypeSymbol);
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
                return ((this.ImplInterfaceTypeSymbol?.GetAllMembers()).NullToEmpty<ISymbol>())
                            .Except
                            (
                                this.SuperClassTypeSymbol.GetAllPublicMembers(),
                                RoslynAnalysisAndGenerationUtils.TheSymbolByNameComparer
                            ).Union(SuperClassTypeSymbol.GetMembers().Where(member => member.IsOverridable()));
            }
        }


        void SetMemberSymbols()
        {
            this.EventBuilderInfos =
                this.ImplementableSymbols.GetSymbolsOfType<IEventSymbol>()
                    .Select(symbol => new EventWrapperMemberBuilderInfo(symbol)).ToList();

            this.PropBuilderInfos =
                 this.ImplementableSymbols.GetSymbolsOfType<IPropertySymbol>()
                     .Select(symbol => new PropertyWrapperMemberBuilderInfo(symbol)).ToList();

            this.MethodBuilderInfos =
                this.ImplementableSymbols.GetSymbolsOfType<IMethodSymbol>()
                    .Where(symbol => symbol.AssociatedSymbol == null)
                    .Select(symbol => new MethodWrapperMemberBuilderInfo(symbol)).ToList();
        }

        void SetMissingMaps()
        {
            this._wrappedObjInfos
                .DoForEach(wrappedObjInfo => wrappedObjInfo.AddMissingMaps(ImplementableSymbols.Select(symb => symb.Name).Distinct()));
        }

        void SetWrappedMembers()
        {
            this.EventBuilderInfos.Cast<WrapperMemberBuilderInfoBase>()
                .Union(PropBuilderInfos.Where(propBuilderInfo => propBuilderInfo.MustImplement))
                .Union(MethodBuilderInfos.Where(propBuilderInfo => propBuilderInfo.MustImplement))
                .DoForEach(builderInfo => builderInfo.SetWrappedMembers(GetWrappedMemberInfos(builderInfo.WrapperSymbolName)));
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

        string SuperClassName => SuperClassTypeSymbol?.GetFullTypeString();

        IEnumerable<INamedTypeSymbol> AllImplementedInterfaces =>
            ImplInterfaceTypeSymbol.ToCollection().Union(WrapInterfaceTypeSymbol.ToCollection());

        IEnumerable<EventWrapperMemberBuilderInfo> EventBuilderInfos { get; set; }

        IEnumerable<PropertyWrapperMemberBuilderInfo> PropBuilderInfos { get; set; }

        IEnumerable<MethodWrapperMemberBuilderInfo> MethodBuilderInfos { get; set; }

        void OpenClassDeclaration(RoslynCodeBuilder roslynCodeBuilder)
        {
            roslynCodeBuilder.AddClass
            (
                ClassName,
                null,
                SuperClassTypeSymbol,
                AllImplementedInterfaces.ToArray()
            );
        }

        private IEnumerable<MemberMapInfo>
            GetWrappedMemberInfos(string wrapperMemberName)
        {
            return 
                _wrappedObjInfos
                    .SelectMany(wrappedObj => wrappedObj.GetWrappedMemberInfo(wrapperMemberName).ToCollection().ToList()).ToList();
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

        private void AddDefaultConstructor(RoslynCodeBuilder roslynCodeBuilder)
        {
            roslynCodeBuilder.PushRegion("Constructor");

            roslynCodeBuilder.AddDefaultConstructorOpening(this.ClassName);

            foreach (WrappedObjInfo wrappedObj in _wrappedObjInfos)
            {
                wrappedObj.AddDefaultConstructor(roslynCodeBuilder);
            }

            roslynCodeBuilder.Pop();

            roslynCodeBuilder.PopRegion();
        }


        protected virtual void AddStaticInitLambda(RoslynCodeBuilder roslynCodeBuilder)
        {

        }

        public const string STATIC_CORE_MEMBER_NAME = "TheCore";

        void AddStaticCoreReference(RoslynCodeBuilder roslynCodeBuilder)
        {
            roslynCodeBuilder.AddLine($"public static Core TheCore {{ get; set; }}");
        }

        internal string GenerateCode()
        {
            Compilation compilation = this.TheCompilation;

            SetMissingMaps();

            SetWrappedMembers();

            RoslynCodeBuilder roslynCodeBuilder = new RoslynCodeBuilder();
            RoslynAnalysisAndGenerationUtils.TheNamespaces =
                roslynCodeBuilder.AllNamespaces;

            RoslynAnalysisAndGenerationUtils.TheNamespaces.Add("NP.Paradigms.Extensions");
            RoslynAnalysisAndGenerationUtils.TheNamespaces.Add("NP.Roxy");
            RoslynAnalysisAndGenerationUtils.TheNamespaces.Add("NP.Roxy.Attributes");

            RoslynAnalysisAndGenerationUtils.InitTypeNamespace<Action<int>>();

            roslynCodeBuilder.AddNamespace(AssemblerNames.GENERATED_NAMESPACE_NAME);

            OpenClassDeclaration(roslynCodeBuilder);

            AddStaticCoreReference(roslynCodeBuilder);

            AddStaticInitLambda(roslynCodeBuilder);

            ImplementEvents(roslynCodeBuilder);

            AddWrappedClasses(roslynCodeBuilder);

            AddDefaultConstructor(roslynCodeBuilder);

            AddPropWraps(roslynCodeBuilder);

            AddMethodWraps(roslynCodeBuilder);

            roslynCodeBuilder.PopAll();

            TheGeneratedCode = roslynCodeBuilder.ToStr();

            return TheGeneratedCode;
        }

        // adds the class to compilation.
        public void ConfigurationCompleted()
        {
            this.GenerateCode();

            TheCore.AddAssembliesToReference(this.ReferencedAssemblies);

            this.GenerateCode();

            TheCore.AddClass(this);

            TheSelfTypeSymbol =
                TheCore.TheCompilation.GetTypeByMetadataName(FullTypeName);
        }

        public bool ConfigurationHasBeenCompleted =>
            TheSelfTypeSymbol != null;

        protected string FullTypeName => this.ClassName.GetFullTypeName();

        public INamedTypeSymbol TheSelfTypeSymbol { get; private set; }

        public string TheGeneratedCode { get; private set; }


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

        public virtual IEnumerable<Assembly> ReferencedAssemblies => Enumerable.Empty<Assembly>();
    }

    internal class TypeConfig : TypeConfigBySymbols
    {
        public Type ImplInterfaceType { get; private set; }

        public Type SuperClassType { get; private set; }

        public Type WrapInterfaceType { get; private set; }

        public TypeConfig
        (
            Core core,
            string className = null,
            Type implInterfaceType = null,
            Type superClassType = null,
            Type wrapInterfaceType = null
        )
            : base(core, className)
        {
            ImplInterfaceType = implInterfaceType.GetInterfaceType();
            SuperClassType = superClassType.GetClassType();
            WrapInterfaceType = wrapInterfaceType.GetInterfaceType();

            this.TheCore.AddAssembliesToReference(this.ReferencedAssemblies);

            base.SetFromSymbols
            (
                ImplInterfaceType.GetTypeSymbol(TheCompilation),
                SuperClassType.GetTypeSymbol(TheCompilation),
                WrapInterfaceType.GetTypeSymbol(TheCompilation)
            );
        }

        public override IEnumerable<Assembly> ReferencedAssemblies =>
            new[] { ImplInterfaceType, SuperClassType, WrapInterfaceType }.GetAllReferencedAssemblies();

    }

    internal class TypeConfig<TImplementedInterface, TSuperClass, TWrappedInterface> :
        TypeConfig,
        ITypeConfig<TImplementedInterface, TSuperClass, TWrappedInterface>
    {
        public TypeConfig(Core core, string className = null) : 
            base(core, className, typeof(TImplementedInterface), typeof(TSuperClass), typeof(TWrappedInterface))
        {
        }

        public Action<TImplementedInterface, TSuperClass, TWrappedInterface> UnInitAction { get; set; } = null;
        public Action<TImplementedInterface, TSuperClass, TWrappedInterface> InitAction { get; set; } = null;

        void AddStaticInitLambdaImpl(RoslynCodeBuilder roslynCodeBuilder, string lambdaName, string callLambdaName)
        {
            roslynCodeBuilder.AddLine($"public static Action<{ImplInterfaceTypeSymbol.GetFullTypeStringWithNoInterface()}, {SuperClassTypeSymbol.GetFullTypeStringWithNoClass()}, {WrapInterfaceTypeSymbol.GetFullTypeStringWithNoInterface()}> {lambdaName} {{ get; set; }} = null;");

            roslynCodeBuilder.AddEmptyLine();

            roslynCodeBuilder.AddLine($"private void {callLambdaName}()");
            roslynCodeBuilder.Push();

            roslynCodeBuilder.AddLine($"if ({lambdaName} != null)");
            roslynCodeBuilder.Push();
            roslynCodeBuilder.AddLine($"{lambdaName}(this, this, this)", true);
            roslynCodeBuilder.Pop();

            roslynCodeBuilder.Pop();
        }

        protected override void PostTypeSet()
        {
            base.PostTypeSet();

            if (this.UnInitAction != null)
            {
                TheGeneratedType.SetStaticPropValue(STATIC_UNINIT_LAMBDA_NAME, this.UnInitAction);
            }

            if (this.InitAction != null)
            {
                TheGeneratedType.SetStaticPropValue(STATIC_INIT_LAMBDA_NAME, this.InitAction);
            }
        }

        protected override void AddStaticInitLambda(RoslynCodeBuilder roslynCodeBuilder)
        {
            AddStaticInitLambdaImpl(roslynCodeBuilder, STATIC_INIT_LAMBDA_NAME, CALL_STATIC_INIT_METHOD);

            AddStaticInitLambdaImpl(roslynCodeBuilder, STATIC_UNINIT_LAMBDA_NAME, CALL_STATIC_UNINIT_METHOD);
        }

    }
}
