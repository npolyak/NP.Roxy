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

        INamedTypeSymbol WrapInterfaceTypeSymbol { get; }

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

    public interface ITypeConfig<TWrapperInterface> : ITypeConfig
    {

    }


    public interface ITypeConfig<TImplementedInterface, TSuperClass, TWrapperInterface> : ITypeConfig<TWrapperInterface>
    {
    }

    public class TypeConfigBase
    {
        internal const string INIT_METHOD_NAME = "__Init";
    }

    public class TypeConfigBySymbols<TWrapperInterface> : TypeConfigBase, ITypeConfig<TWrapperInterface>
    {
        public INamedTypeSymbol ImplInterfaceTypeSymbol { get; private set; }

        public INamedTypeSymbol SuperClassTypeSymbol { get; private set; }

        public INamedTypeSymbol WrapInterfaceTypeSymbol { get; private set; }

        protected IEnumerable<INamedTypeSymbol> AllImplementedTypesSymbols =>
            new[] { ImplInterfaceTypeSymbol, SuperClassTypeSymbol, WrapInterfaceTypeSymbol };

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

            foreach(string propName in propNames)
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

        protected void SetFromSymbols
        (
            INamedTypeSymbol implInterfaceTypeSymbol,
            INamedTypeSymbol superClassTypeSymbol
        )
        {
            this.ImplInterfaceTypeSymbol = implInterfaceTypeSymbol;
            this.SuperClassTypeSymbol = superClassTypeSymbol;
            this.WrapInterfaceTypeSymbol = typeof(TWrapperInterface).GetTypeSymbol(this.TheCompilation);

            TheCore.AddTypeSymbolsToReference(AllImplementedTypesSymbols);

            this.ClassName = ImplInterfaceTypeSymbol.GetClassName(this.ClassName);

            if (ImplInterfaceTypeSymbol.TypeKind != TypeKind.Interface)
                throw new Exception($"Error: ImplementedInterface type '{ImplInterfaceTypeSymbol.Name}' is not interface.");

            if ( (SuperClassTypeSymbol.TypeKind != TypeKind.Class) ) 
                throw new Exception($"Error: Class to extend type '{SuperClassTypeSymbol.Name}' is not a class.");

            if (WrapInterfaceTypeSymbol.TypeKind != TypeKind.Interface)
                throw new Exception($"Error: WrappedInterface type '{WrapInterfaceTypeSymbol.Name}' is not interface.");

            if ((!HasInterfaceToImplement) && (!HasClassToExtend))
            {
                throw new Exception($"Error: there is neither interface to implement, no class to extend - no public members.");
            }

            IEnumerable<ISymbol> props =
                WrapInterfaceTypeSymbol
                    .GetAllMembers()
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
            INamedTypeSymbol superClassTypeSymbol = null
        )
        {
            TheCore = core;
            this.ClassName = className;

            if ( (implInterfaceTypeSymbol == null) && 
                 (superClassTypeSymbol == null) )
            {
                return;
            }

            SetFromSymbols
            (
                implInterfaceTypeSymbol, 
                superClassTypeSymbol
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

        public void SetAllowNonPublicForAllMembers()
        {
            foreach(WrappedObjInfo wrappedObjInfo in this._wrappedObjInfos)
            {
                wrappedObjInfo.AllowNonPublicForAllMemberMaps = true;
            }
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

            wrappedObjInfo.SetMap(wrappedMemberName, wrapperMemberName, allowNonPublic);
        }

        public void SetThisMemberMap
        (
            string wrappedObjPropName,
            string wrappedMemberName,
            bool? allowNonPublic = null
        )
        {
            SetMemberMap(wrappedObjPropName, wrappedMemberName, "this", allowNonPublic);
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


        // sets wrapped member for every builder info
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

        private void AddInit(RoslynCodeBuilder roslynCodeBuilder)
        {
            roslynCodeBuilder.PushRegion("Init Method");

            roslynCodeBuilder.AddLine($"void {INIT_METHOD_NAME}()");
            roslynCodeBuilder.Push();

            this.PropBuilderInfos.DoForEach(propBuilderInfo => propBuilderInfo.AddInit(roslynCodeBuilder));

            roslynCodeBuilder.Pop();

            roslynCodeBuilder.PopRegion();
        }

        private void AddDefaultConstructor(RoslynCodeBuilder roslynCodeBuilder)
        {
            roslynCodeBuilder.PushRegion("Default Constructor");

            roslynCodeBuilder.AddDefaultConstructorOpening(this.ClassName);

            foreach (WrappedObjInfo wrappedObj in _wrappedObjInfos)
            {
                wrappedObj.AddDefaultConstructor(roslynCodeBuilder);
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

            RoslynAnalysisAndGenerationUtils.TheNamespaces.Add("NP.Utilities");
            RoslynAnalysisAndGenerationUtils.TheNamespaces.Add("NP.Roxy");
            RoslynAnalysisAndGenerationUtils.TheNamespaces.Add("NP.Roxy.Attributes");

            RoslynAnalysisAndGenerationUtils.InitTypeNamespace<Action<int>>();

            roslynCodeBuilder.AddNamespace(AssemblerNames.GENERATED_NAMESPACE_NAME);

            OpenClassDeclaration(roslynCodeBuilder);

            AddStaticCoreReference(roslynCodeBuilder);

            //AddStaticInitLambda(roslynCodeBuilder);

            ImplementEvents(roslynCodeBuilder);

            AddWrappedClasses(roslynCodeBuilder);

            AddInit(roslynCodeBuilder);

            AddDefaultConstructor(roslynCodeBuilder);

            AddWrappedObjsConstructor(roslynCodeBuilder);

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

            propertyWrapperMemberBuilder.SetInit(typeSymbol, this.TheCompilation);
        }

        public void SetInit<TInit>(string propName)
        {
            INamedTypeSymbol typeSymbol = typeof(TInit).GetTypeSymbol(this.TheCompilation);

            SetInit(propName, typeSymbol);
        }
    }

    internal class TypeConfig<TWrapperInterface>: TypeConfigBySymbols<TWrapperInterface>
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

            this.TheCore.AddTypesToReference(ReferencedTypes);

            base.SetFromSymbols
            (
                ImplInterfaceType.GetTypeSymbol(TheCompilation),
                SuperClassType.GetTypeSymbol(TheCompilation)
            );
        }

        internal Type[] ReferencedTypes =>
            new[] { ImplInterfaceType, SuperClassType, WrapInterfaceType };
    }

    internal class TypeConfig<TImplementedInterface, TSuperClass, TWrapperInterface> :
        TypeConfig<TWrapperInterface>,
        ITypeConfig<TImplementedInterface, TSuperClass, TWrapperInterface>
    {
        public TypeConfig(Core core, string className = null) : 
            base(core, className, typeof(TImplementedInterface), typeof(TSuperClass), typeof(TWrapperInterface))
        {
        }
    }
}
