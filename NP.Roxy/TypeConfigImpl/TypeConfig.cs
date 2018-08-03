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
using NP.Concepts.Attributes;
using NP.Roxy.Attributes;
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

        IPropertySymbol GetPlugin(string pluginPropName);

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
        void AddCallToPluginObjConstructor
        (
            RoslynCodeBuilder roslynCodeBuilder,
            Dictionary<string, string> sharedPluginsNameMap
        );

        void RecompileAssembly();
    }

    public class TypeConfig : 
        ITypeConfig,
        ICompilationContainer
    {
        internal const string INIT_METHOD_NAME = "__Init";

        [XmlAttribute]
        public string ClassName { get; set; }

        [XmlIgnore]
        public Core TheCore { get; private set; }

        public INamedTypeSymbol TypeToImplementSymbol { get; private set; }

        public INamedTypeSymbol ImplementorTypeSymbol { get; protected set; }

        // all types that can be used to reference the object
        protected IEnumerable<INamedTypeSymbol> AllReferenceTypesSymbols =>
            new[] { TypeToImplementSymbol };

        // all implemented types 
        protected IEnumerable<INamedTypeSymbol> AllImplementedTypesSymbols =>
            AllReferenceTypesSymbols.Union(ImplementorTypeSymbol.ToCollection());

        public bool HasTypeImplement =>
            TypeToImplementSymbol.Name != nameof(NoType);

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

        public void RecompileAssembly()
        {
            TheCore.RegenerateAssembly();
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

        private List<PluginInfo> _sharedPluginInfos = new List<PluginInfo>();
        private List<PluginInfo> _nonSharedPluginInfos = new List<PluginInfo>();

        private IEnumerable<PluginInfo> PluginInfos => 
            _sharedPluginInfos.Union(_nonSharedPluginInfos);

        void SetImplementableSymbols()
        {
            // get all interface (abstract class) members
            // to be implemented
            var stage1 = this.TypeToImplementSymbol
                    ?.GetAllMembers();

            // eliminate the duplicates
            // so that the least abstract will stay in the 
            // resulting collection. 
            var stage2 = stage1
                    ?.EliminateDups();

            // get all members that can be overridden.
            // (abstract or virtual or interface members)
            var overridableState = stage2?.Where(member => member.IsOverridable());

            // remove members that were implemented
            // by Implementor
            var stageImplemetableSymbols = 
                overridableState
                        .NullToEmpty<ISymbol>()
                        .Except
                        (
                            this.ImplementorTypeSymbol.GetAllMembers(),
                            RoslynAnalysisAndGenerationUtils.TheSymbolByNameAndSignatureComparer
                        );

            // add implementor members that can be overriden
            // I do not quite understand - why do we need to get virtual members of 
            // Implementor - I do not think we'd override them anyways?
            ImplementableSymbols =
                stageImplemetableSymbols
                    .Union(ImplementorTypeSymbol.GetAllMembers().EliminateDups().Where(member => member.IsOverridable())).ToList();

            //var implSymbs = stageImplemetableSymbols
            //        .Union(ImplementorTypeSymbol.GetAllSuperMembers().EliminateDups().Where(member => member.IsOverridable())).ToList();
        }

        public IPropertySymbol GetPlugin(string pluginPropName)
        {
            return PluginInfos
                    .FirstOrDefault
                    (
                        pluginInfo => pluginInfo.PluginPropName == pluginPropName)?.PluginPropSymbol;
        }

        IEnumerable<ISymbol> GetAllPluginUnmappedAbstractPropsAndMethods()
        {
            return PluginInfos.NullToEmpty().SelectMany(plugin => plugin.GetUnmappedAbstractMethodsAndProps()).ToList();
        }

        protected void SetFromSymbols
        (
            INamedTypeSymbol typeToImplSymbol,
            INamedTypeSymbol implementorSymbol
        )
        {
            this.TypeToImplementSymbol = typeToImplSymbol;
            this.ImplementorTypeSymbol = implementorSymbol.GetNoTypeForNull(TheCompilation);

            if ( TypeToImplementSymbol.IsClass() && 
                 (ImplementorTypeSymbol?.IsClass() == true) )
            {
                throw new Exception($"Roxy Usage Error: Both TypeToImplement '{TypeToImplementSymbol.Name}' and Implementor '{implementorSymbol.Name}' cannot be classes.");
            }

            this.ClassName = TypeToImplementSymbol.GetClassName(this.ClassName);

            IEnumerable<OverrideVirtualAttribute> overrideVirtualAttrs =
                ImplementorTypeSymbol.GetAttrObjects<OverrideVirtualAttribute>();

            foreach(OverrideVirtualAttribute overrideVirtualAttr in overrideVirtualAttrs)
            {
                SetOverrideVirtualMembers(overrideVirtualAttr.IncludeBase, overrideVirtualAttr.MemberNames);
            }

            IEnumerable<IPropertySymbol> pluginProps =
                ImplementorTypeSymbol
                    .GetAllMembers()
                    .GetItemsOfType<ISymbol, IPropertySymbol>();


            // at this point there is a rule that shared plugins should
            // precede those that share them within the implementor interface/class
            foreach (IPropertySymbol prop in pluginProps)
            {
                PluginAttribute pluginAttr = prop.GetAttrObject<PluginAttribute>();

                if (pluginAttr == null)
                    continue;

                PluginInfo pluginInfo =
                    new PluginInfo
                    (
                        this.TheCore, 
                        prop,  
                        pluginAttr.ImplementorType?.GetTypeSymbol(this.TheCompilation),
                        pluginAttr.InitType?.GetTypeSymbol(this.TheCompilation),
                        _sharedPluginInfos);

                if (pluginAttr.IsShared)
                {
                    _sharedPluginInfos.Add(pluginInfo);
                }
                else
                {
                    _nonSharedPluginInfos.Add(pluginInfo);
                }
            }

            SetImplementableSymbols();

            PluginInfos?.DoForEach(pluginInfo => pluginInfo.SetMaps(ImplementableSymbols));

            //remove plugins from ImplementableSymbols
            ImplementableSymbols = ImplementableSymbols.Except
                        (
                            PluginInfos.Select(pI => pI.PluginPropSymbol),
                            RoslynAnalysisAndGenerationUtils.TheSymbolByNameAndSignatureComparer
                        );

            SetMemberSymbols();
        }

        private void Init
         (
            Core core,
            string className,
            INamedTypeSymbol typeToImplementSymbol,
            INamedTypeSymbol implementorTypeSymbol = null)
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
                implementorTypeSymbol
            );
        }

        public TypeConfig
        (
            Core core,
            string className,
            INamedTypeSymbol typeToImplementSymbol,
            INamedTypeSymbol implementorTypeSymbol = null
        )
        {
            Init(core, className, typeToImplementSymbol, implementorTypeSymbol);
        }

        internal PluginInfo GetWrappedObjInfo(string wrappedObjPropName)
        {
            PluginInfo wrappedObjInfo =
                this.PluginInfos.FirstOrDefault(wrObjInfo => wrObjInfo.PluginPropName == wrappedObjPropName);

            if (wrappedObjInfo == null)
            {
                throw new Exception($"Error no wrapped object property name '{wrappedObjPropName}' found.");
            }

            return wrappedObjInfo;
        }

        internal PluginInfo GetWrappedObjInfo<TWrappedObj, TProp>(Expression<Func<TWrappedObj, TProp>> expr)
        {
            string wrappedObjName = expr.GetMemberName();

            return GetWrappedObjInfo(wrappedObjName);
        }

        public void SetAllowNonPublicForAllMembers(string wrappedObjPropName)
        {
            PluginInfo wrappedObjInfo = GetWrappedObjInfo(wrappedObjPropName);

            wrappedObjInfo.AllowNonPublicForAllMemberMaps = true;
        }

        public void SetAllowNonPublicForAllMembers()
        {
            foreach (PluginInfo wrappedObjInfo in this.PluginInfos)
            {
                wrappedObjInfo.AllowNonPublicForAllMemberMaps = true;
            }
        }

        public ISymbol GetWrapperMemberSymbolByName(string wrapperMemberName)
        {
            return ImplementableSymbols.GetImplementableSymbolByName(wrapperMemberName);
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

            PluginInfo wrappedObjInfo = GetWrappedObjInfo(wrappedObjPropName);

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

            PluginInfo wrappedObjInfo = GetWrappedObjInfo(wrappedObjPropName);

            wrapperMemberNames
                .DoForEach(wrapperMemberName => SetMemberMap(wrappedObjPropName, null, wrapperMemberName, true));
        }

        public Compilation TheCompilation => TheCore.TheCompilation;

        public void AddStaticUtilsClass(string wrappedObjPropName, Type staticMethodsContainerClass)
        {
            PluginInfo wrappedObjInfo =
                PluginInfos.Single(wrappedObj => wrappedObj.PluginPropName == wrappedObjPropName);

            wrappedObjInfo.AddStaticMethodsContainerType(staticMethodsContainerClass);
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
            get;
            private set;
        }

        void SetMemberSymbols()
        {
            this.EventBuilderInfos =
                this.ImplementableSymbols.GetSymbolsOfType<IEventSymbol>()
                    .Select(symbol => new EventWrapperMemberBuilderInfo(symbol, this.TheCore)).ToList();

            // these are the props and methods without implementations
            // defined within abstract plugins which do not have
            // a publice API defined by the interfaces. 
            // they should be implemented by other plugins
            var unmappedAbstractPluginPropsAndMethods =
                GetAllPluginUnmappedAbstractPropsAndMethods().Except(this.ImplementableSymbols, RoslynAnalysisAndGenerationUtils.TheSymbolByNameAndSignatureComparer);

            this.PropBuilderInfos =
                 this.ImplementableSymbols.GetSymbolsOfType<IPropertySymbol>()
                     .Select(symbol => new PropertyWrapperMemberBuilderInfo(symbol, this.TheCore))
                     .Union
                     (
                        unmappedAbstractPluginPropsAndMethods.GetSymbolsOfType<IPropertySymbol>()
                                                            .Select(symbol => new PropertyWrapperMemberBuilderInfo(symbol, this.TheCore, true))).ToList();

            this.MethodBuilderInfos =
                this.ImplementableSymbols
                    .GetRealMethods()
                    .Select(symbol => new MethodWrapperMemberBuilderInfo(symbol, this.TheCore))
                    .Union
                    (
                        unmappedAbstractPluginPropsAndMethods.GetSymbolsOfType<IMethodSymbol>()
                                        .Select(symbol => new MethodWrapperMemberBuilderInfo(symbol, this.TheCore, true))
                    ).ToList();

            this.ImplementableSymbols = ImplementableSymbols.Union(unmappedAbstractPluginPropsAndMethods);
        }

        void SetMissingMaps()
        {
            this.PluginInfos
                .DoForEach(wrappedObjInfo => wrappedObjInfo.AddMissingMaps(ImplementableSymbols.Distinct()));
        }


        // sets wrapped member for every builder info
        void SetWrappedMembers()
        {
            this.EventBuilderInfos.Cast<WrapperMemberBuilderInfoBase>()
                .Union(PropBuilderInfos.Where(propBuilderInfo => propBuilderInfo.MustImplement))
                .Union(MethodBuilderInfos.Where(methodBuilderInfo => methodBuilderInfo.MustImplement))
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

        string SuperClassName => ImplementorTypeSymbol?.GetFullTypeString();

        IEnumerable<EventWrapperMemberBuilderInfo> EventBuilderInfos { get; set; }

        IEnumerable<PropertyWrapperMemberBuilderInfo> PropBuilderInfos { get; set; }

        IEnumerable<MethodWrapperMemberBuilderInfo> MethodBuilderInfos { get; set; }

        void OpenClassDeclaration(RoslynCodeBuilder roslynCodeBuilder)
        {
            INamedTypeSymbol[] allSymbols = { ImplementorTypeSymbol, TypeToImplementSymbol };

            INamedTypeSymbol firstSymbol = allSymbols.FirstOrDefault(symb => symb.IsClass())?? ImplementorTypeSymbol;

            INamedTypeSymbol[] otherSymbols = allSymbols.Except(firstSymbol.ToCollection()).ToArray();

            roslynCodeBuilder.AddClass
            (
                ClassName,
                null,
                firstSymbol.GetNullForNoType(TheCompilation),
                true,
                otherSymbols
            );
        }

        private IEnumerable<MemberMapInfo>
            GetWrappedMemberInfos(ISymbol wrapperSymbol)
        {
            return
                PluginInfos
                    .SelectMany(plugin => plugin.GetPluginMemberInfo(wrapperSymbol).ToCollection().ToList()).ToList();
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

        private void AddPlugins(RoslynCodeBuilder roslynCodeBuilder)
        {
            if (this.PluginInfos.IsNullOrEmpty())
                return;

            roslynCodeBuilder.PushRegion("Plugins");

            foreach (PluginInfo wrappedClassInfo in this.PluginInfos)
            {
                wrappedClassInfo.AddPluginClass(roslynCodeBuilder);
            }

            roslynCodeBuilder.PopRegion();
        }

        private void AddPropWraps(RoslynCodeBuilder roslynCodeBuilder)
        {
            if (PropBuilderInfos.IsNullOrEmpty())
                return;

            roslynCodeBuilder.PushRegion("Generated Properties");

            foreach (PropertyWrapperMemberBuilderInfo propBuilderInfo in this.PropBuilderInfos)
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
            roslynCodeBuilder.PushRegion("Constructor");

            roslynCodeBuilder
                .AddConstructorOpening
                (
                    this.ClassName,
                    Accessibility.Public
                );

            foreach (PluginInfo wrappedObj in PluginInfos)
            {
                wrappedObj.AddPluginDefaultConstructorInitialization(roslynCodeBuilder);
            }

            roslynCodeBuilder.AddLine($"{INIT_METHOD_NAME}()", true);

            roslynCodeBuilder.Pop();

            roslynCodeBuilder.PopRegion();
        }

        private string GetWrappedObjConstructorParamStr()
        {
            return PluginInfos.StrConcat((pluginInfo) => $"{pluginInfo.ConcretePluginNamedTypeSymbol.GetFullTypeString()} {pluginInfo.PluginImplementationClassName.FirstCharToLowerCase(true)}");
        }

        void AddPluginObjsConstructor(RoslynCodeBuilder roslynCodeBuilder)
        {
            string paramsLine = GetWrappedObjConstructorParamStr();

            if (paramsLine.IsNullOrEmpty())
                return;

            roslynCodeBuilder.PushRegion("Plugins Constructor");

            roslynCodeBuilder.AddLine($"public {this.ClassName}({paramsLine})");
            roslynCodeBuilder.Push();

            foreach (PluginInfo pluginInfo in PluginInfos)
            {
                pluginInfo.AddPluginInitInConstructorCode(roslynCodeBuilder);
                roslynCodeBuilder.AddLine();
            }

            roslynCodeBuilder.AddLine($"{INIT_METHOD_NAME}()", true);

            roslynCodeBuilder.Pop(true);

            roslynCodeBuilder.PopRegion();
        }

        public void AddCallToPluginObjConstructor
        (
            RoslynCodeBuilder roslynCodeBuilder, 
            Dictionary<string, string> sharedPluginsNameMap
        )
        {
            roslynCodeBuilder.AddText($" new {this.ClassName}(");

            bool notFirst = false;
            foreach (PluginInfo pluginInfo in PluginInfos)
            {
                if (notFirst)
                {
                    roslynCodeBuilder.AddText(", ");
                }
                else
                {
                    notFirst = true;
                }

                string pluginPropName = pluginInfo.PluginPropName;

                if (!sharedPluginsNameMap.TryGetValue(pluginPropName, out string valToAdd))
                {
                    valToAdd = "null";
                }

                roslynCodeBuilder.AddText(valToAdd);
            }

            roslynCodeBuilder.AddText(");");

            roslynCodeBuilder.AddLine();
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

            AddPlugins(roslynCodeBuilder);

            //AddSharedProps(roslynCodeBuilder);

            AddInit(roslynCodeBuilder);

            AddDefaultConstructor(roslynCodeBuilder);

            AddPluginObjsConstructor(roslynCodeBuilder);

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

    public static class TypeConfigUtils
    {
        public static void CheckTypeToImplementMatches
        (
            this ITypeConfig typeConfig, 
            INamedTypeSymbol typeToImplementSymbol
        )
        {
            if (!typeConfig.TypeToImplementSymbol.TypesStrictlyMatch(typeToImplementSymbol))
            {
                throw new Exception($"Roxy Usage Error: Type to implement '{typeConfig.TypeToImplementSymbol.Name}' does not match '{typeToImplementSymbol.Name}'");
            }
        }

        public static void CheckImplementorMatches
        (
            this ITypeConfig typeConfig,
            INamedTypeSymbol implementorTypeSymbol
        )
        {
            if (!typeConfig.ImplementorTypeSymbol.TypesStrictlyMatch(implementorTypeSymbol))
            {
                throw new Exception($"Roxy Usage Error: Implementor type does not match '{typeConfig.ImplementorTypeSymbol.Name}' does not match '{implementorTypeSymbol.Name}'");
            }
        }

        public static void CheckMatches
        (
            this ITypeConfig typeConfig, 
            INamedTypeSymbol typeToImplementSymbol, 
            INamedTypeSymbol implementorSymbol)
        {
            typeConfig.CheckTypeToImplementMatches(typeToImplementSymbol);
            typeConfig?.CheckImplementorMatches(implementorSymbol);
        }


        public static T CreateInstanceOfType<T>(this ITypeConfig typeConfig, params object[] args)
        {
            if (typeConfig.TheGeneratedType == null)
            {
                typeConfig.RecompileAssembly();
            }

            return (T)Activator.CreateInstance(typeConfig.TheGeneratedType, args);
        }
    }
}
