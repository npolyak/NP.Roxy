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
using NP.Roxy.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Linq.Expressions;
using NP.Concepts;
using NP.Concepts.Attributes;

namespace NP.Roxy.TypeConfigImpl
{
    internal class PluginInfo
    {
        bool _allowNonPublicForAllMemberMaps = false;
        [XmlAttribute]
        internal bool AllowNonPublicForAllMemberMaps
        {
            get => _allowNonPublicForAllMemberMaps;
            set
            {
                if (_allowNonPublicForAllMemberMaps == value)
                    return;

                _allowNonPublicForAllMemberMaps = value;

                this.PluginMemberNameMaps
                    .ForEach(memberMap => memberMap.SetAllowNonPublic(_allowNonPublicForAllMemberMaps));
            }
        }

        HashSet<string> _membersNotToWrap = new HashSet<string>();

        IPropertySymbol _pluginPropSymbol;
        [XmlIgnore]
        public IPropertySymbol PluginPropSymbol
        {
            get => _pluginPropSymbol;
            protected set
            {
                if (_pluginPropSymbol.ObjEquals(value))
                    return;

                _pluginPropSymbol = value;

                _pluginPropSymbol?.GetAttrObjects<SuppressWrappingAttribute>()
                                 ?.DoForEach(suppressAttr => _membersNotToWrap.Add(suppressAttr.MemberName));
            }
        }

        // INamedTypeSymbol do not seem to carry information about
        // non-public members, so I comment it out for now
        // and will use the Reflection types. (the static method containers are not generated anyways).
        // I will have to reconsider it when working on 
        public IEnumerable<INamedTypeSymbol> StaticMethodContainers { get; } =
            new List<INamedTypeSymbol>();

        public PluginInfo(Core core, IPropertySymbol pluginPropSymbol, INamedTypeSymbol pluginImplementationType)
        {
            this.PluginPropSymbol = pluginPropSymbol;

            this.TheCore = core;
        }

        public bool InitializedThroughConstructor
        {
            get
            {
                return (this.PluginPropSymbol?.GetAttrSymbol(typeof(ConstructorInitAttribute)) != null);
            }
        }

        public void AddStaticMethodsContainerType(INamedTypeSymbol staticMethodsContainerTypeSymbol)
        {
            (StaticMethodContainers as List<INamedTypeSymbol>).Add(staticMethodsContainerTypeSymbol);
        }

        //public IEnumerable<Type> StaticMethodContainers { get; } =
        //    new List<Type>();
        public void AddStaticMethodsContainerType(Type staticMethodContainerType)
        {
            //(StaticMethodContainers as List<Type>).Add(staticMethodContainerType);

            INamedTypeSymbol staticMethodsContainerTypeSymbol =
                staticMethodContainerType.GetTypeSymbol(this.TheCompilation);

            AddStaticMethodsContainerType(staticMethodsContainerTypeSymbol);
        }

        [XmlIgnore]
        public INamedTypeSymbol PluginNamedTypeSymbol =>
            PluginPropSymbol?.Type as INamedTypeSymbol;

        [XmlIgnore]
        public INamedTypeSymbol PluginImplementationNamedTypeSymbol =>
            PluginPropSymbol?.Type as INamedTypeSymbol;

        [XmlIgnore]
        public string PluginImplementationClassName =>
            PluginImplementationNamedTypeSymbol.Name;


        INamedTypeSymbol _concretePluginNamedTypeSymbol = null;
        [XmlIgnore]
        public INamedTypeSymbol ConcretePluginNamedTypeSymbol
        {
            get => _concretePluginNamedTypeSymbol ?? PluginImplementationNamedTypeSymbol;

            protected set
            {
                if (ReferenceEquals(_concretePluginNamedTypeSymbol, value))
                    return;

                if (ReferenceEquals(PluginImplementationNamedTypeSymbol, value))
                {
                    _concretePluginNamedTypeSymbol = null;

                    return;
                }

                _concretePluginNamedTypeSymbol = value;
            }
        }

        [XmlIgnore]
        public string ConcretePluginClassName =>
            this.ConcretePluginNamedTypeSymbol?.Name;

        [XmlAttribute]
        public string PluginPropName => PluginPropSymbol.Name;

        [XmlIgnore]
        public string WrapperObjConcretizedPropName =>
            PluginPropName + RoslynAnalysisAndGenerationUtils.CONCRETIZATION_SUFFIX;

        [XmlIgnore]
        public string PluginPropBackingFieldName =>
            PluginPropName?.PropToFieldName();

        [XmlIgnore]
        public Core TheCore { get; private set; }

        [XmlIgnore]
        public Compilation TheCompilation =>
            TheCore.TheCompilation;

        List<MemberMapInfoBase> PluginMemberNameMaps { get; } =
            new List<MemberMapInfoBase>();

        public IEnumerable<MemberMapInfoBase> EventPluginMemberNameMaps =>
            PluginMemberNameMaps.Where(memberMap => memberMap.TheMemberType == ClassMemberType.Event);

        public IEnumerable<MemberMapInfoBase> PropPluginMemberNameMaps =>
            PluginMemberNameMaps.Where(memberMap => memberMap.TheMemberType == ClassMemberType.Property);

        public void SetFromParentSymbol(INamedTypeSymbol parentTypeSymbol)
        {
            this.PluginPropSymbol = 
                parentTypeSymbol.GetMemberByName<IPropertySymbol>(PluginPropName);
        }

        MemberMapInfoBase FindMapImpl<T>(T symbol, Func<MemberMapInfoBase, T> findMethod)
            where T : class
        {
            return PluginMemberNameMaps.FirstOrDefault(strMap => findMethod(strMap).ObjEquals(symbol));
        }

        MemberMapInfoBase FindMapByPluginMemberSymbol(string pluginMemberName)
        {
            return FindMapImpl(pluginMemberName, strMap => (strMap as MemberMapInfo)?.PluginMemberName);
        }

        MemberMapInfoBase FindMapByWrapperMemberSymbol(ISymbol wrapperMemberSymbol)
        {
            return FindMapImpl(wrapperMemberSymbol, strMap => strMap.WrapperMemberSymbol);
        }

        internal virtual string GetWrapperMemberSymbol(string pluginMemberName)
        {
            return FindMapByPluginMemberSymbol(pluginMemberName)?.WrapperMemberName ?? pluginMemberName;
        }

        void CheckMapExists(ISymbol wrapperMemberSymbol)
        {
            MemberMapInfoBase map = FindMapByWrapperMemberSymbol(wrapperMemberSymbol);

            if (map != null)
                throw new Exception($"Roxy Usage Error: the member map for member {wrapperMemberSymbol.Name} of {this.PluginPropName} plugin has already been set.");
        }


        // wrapperMemberSymbol == null means that this is 'this' map
        public void SetMap(string pluginMemberName, ISymbol wrapperMemberSymbol, bool? allowNonPublic = null)
        {
            CheckMapExists(wrapperMemberSymbol);

            if ((pluginMemberName == null) && (wrapperMemberSymbol != null))
            {
                pluginMemberName = wrapperMemberSymbol.Name;
            }

            MemberMapInfo map = null;

            if (wrapperMemberSymbol != null)
            {
                map = new MemberMapInfo(pluginMemberName, wrapperMemberSymbol, this.PluginPropName);
            }
            else
            {
                map = new ThisMemberMapInfo(this.PluginPropName, pluginMemberName);
            }

            PluginMemberNameMaps.Add(map);

            bool resultingAllowNonPublic = allowNonPublic ?? this.AllowNonPublicForAllMemberMaps;

            map.SetAllowNonPublic(resultingAllowNonPublic);

            map.SetFromContainingType
            (
                this.TheCompilation,
                this.PluginImplementationNamedTypeSymbol,
                this.StaticMethodContainers);

            if (map.PluginMemberSymbol == null)
            {
                PluginMemberNameMaps.Remove(map);
            }
        }

        // if such wrapper name does not exist in 
        // the current wrapper maps, but exists among the
        // public variables of the Wrapped Object, we add 
        // the wrapper map whose wrapper and wrapped names
        // are equal to the passed wrapperMemberName
        public void AddWrapperMapIfMissing(ISymbol wrapperMemberSymbol)
        {
            MemberMapInfoBase map = FindMapByWrapperMemberSymbol(wrapperMemberSymbol);

            if (map != null)
                return;

            this.SetMap(wrapperMemberSymbol.Name, wrapperMemberSymbol);
        }


        public void AddMissingMaps(IEnumerable<ISymbol> wrapperMembers)
        {
            wrapperMembers.DoForEach(wrapperMember => AddWrapperMapIfMissing(wrapperMember));
        }

        // gets all member infos for the wrapperMemberName 
        // (including those that do not require renaming)
        public MemberMapInfoBase GetPluginMemberInfo(ISymbol wrapperMemberSymbol)
        {
            MemberMapInfoBase memberMap = this.FindMapByWrapperMemberSymbol(wrapperMemberSymbol);

            if (memberMap == null)
                return null;

            if (memberMap.IsAbstract == true)
                return null;

            if (memberMap is MemberMapInfo memberMapInfo)
            {
                if (this._membersNotToWrap.Contains(memberMapInfo.PluginMemberName))
                    return null;
            }

          
            return memberMap;
        }

        void SetOrUnsetConcretizationDelegates(RoslynCodeBuilder wrapperInitBuilder, bool setOrUnset)
        {
            foreach (ISymbol concretePluginMember in this.ConcretePluginNamedTypeSymbol.GetAllMembers())
            {
                ConcretizationDelegateAttribute concretizationDelegateAttribute =
                    concretePluginMember.GetAttrObject<ConcretizationDelegateAttribute>();

                if (concretizationDelegateAttribute == null)
                    continue;

                string pluginMemberName = concretizationDelegateAttribute.ConcretizingMemberName;
                string wrapperName = this.GetWrapperMemberSymbol(pluginMemberName);

                if (wrapperName == null)
                    continue;

                string memberDelegateName = concretePluginMember.Name;

                wrapperInitBuilder.AddLine($"({this.PluginPropName} as {this.ConcretePluginClassName}).{memberDelegateName} = ", false, false);

                if (setOrUnset)
                {
                    if (concretizationDelegateAttribute is PropGetterConcretizationDelegateAttribute)
                    {
                        wrapperInitBuilder.AddText($"{RoslynAnalysisAndGenerationUtils.GetPropGetterDelegateAssigment(wrapperName)}");
                    }
                    else if (concretizationDelegateAttribute is PropSetterConcretizationDelegateAttribute)
                    {
                        wrapperInitBuilder.AddText($"{RoslynAnalysisAndGenerationUtils.GetPropSetterDelegateAssigment(wrapperName)}");
                    }
                    else //if (concretizationAttrData is MethodConcretizationDelegateAttribute)
                    {
                        wrapperInitBuilder.AddText($"{wrapperName}");
                    }
                }
                else
                {
                    wrapperInitBuilder.AddText("null");
                }

                wrapperInitBuilder.AddText(";");
            }
        }

        string BuildWrapperInit
        (
            IEnumerable<MemberMapInfoBase> eventMemberMaps, 
            IEnumerable<MemberMapInfoBase> propMemberMaps,
            bool addOrRemove
        )
        {
            RoslynCodeBuilder wrapperInitBuilder = new RoslynCodeBuilder();

            wrapperInitBuilder.AddEmptyLine();
            wrapperInitBuilder.AddLine
            (
                $"if ({PluginPropName} != null)"
            );

            wrapperInitBuilder.Push();

            if (!addOrRemove)
            {
                SetOrUnsetConcretizationDelegates(wrapperInitBuilder, false);

                foreach(MemberMapInfoBase propMemberMap in propMemberMaps)
                {
                    propMemberMap.AddPropAssignmentStr(addOrRemove, wrapperInitBuilder);
                }
            }

            foreach (MemberMapInfoBase eventMemberMap in eventMemberMaps)
            {
                wrapperInitBuilder.AddLine(eventMemberMap.GetEventHandlerAssignmentStr(addOrRemove), true);
            }

            if (addOrRemove)
            {
                foreach (MemberMapInfoBase propMemberMap in propMemberMaps)
                {
                    propMemberMap.AddPropAssignmentStr(addOrRemove, wrapperInitBuilder);
                }

                SetOrUnsetConcretizationDelegates(wrapperInitBuilder, true);
            }

            wrapperInitBuilder.Pop();

            return wrapperInitBuilder.ToStr();
        }

        public void AddPluginClass(RoslynCodeBuilder roslynCodeBuilder)
        {
            if (this.PluginImplementationNamedTypeSymbol.IsAbstract)
            {
                // here, the concretization is created
                this.ConcretePluginNamedTypeSymbol =
                    this.TheCore.FindOrCreateConcretizationTypeConf(this.PluginImplementationNamedTypeSymbol).TheSelfTypeSymbol;
            }

            string beforeSetterStr = BuildWrapperInit(EventPluginMemberNameMaps, PropPluginMemberNameMaps, false);

            string afterSetterStr = BuildWrapperInit(EventPluginMemberNameMaps.Reverse(), PropPluginMemberNameMaps.Reverse(), true);

            Accessibility setterAccessibility = Accessibility.Private;

            if (this.PluginPropSymbol.SetMethod != null)
            {
                setterAccessibility = this.PluginPropSymbol.GetMethod.DeclaredAccessibility;
            }

            roslynCodeBuilder.AddPropWithBackingStore
            (
                this.PluginPropName,
                this.PluginPropBackingFieldName,
                this.PluginImplementationNamedTypeSymbol,
                Accessibility.Public,
                beforeSetterStr,
                afterSetterStr,
                setterAccessibility
            );

            if ((ConcretePluginClassName != PluginImplementationClassName))
            {
                roslynCodeBuilder.AddEmptyLine();
                roslynCodeBuilder.AddPropOpening(WrapperObjConcretizedPropName, ConcretePluginNamedTypeSymbol);

                roslynCodeBuilder.AddLine($"{RoslynCodeBuilder.GETTER} => ({ConcretePluginClassName}){PluginPropName}", true);

                roslynCodeBuilder.Pop();
            }
        }

        public void AddPluginDefaultConstructorInitialization(RoslynCodeBuilder roslynCodeBuilder)
        {
            //if (!WrappedObjNamedTypeSymbol.HasPublicDefaultConstructor())
            //    return;
            if (InitializedThroughConstructor)
                return;

            if (PluginImplementationNamedTypeSymbol.TypeKind == TypeKind.Enum)
                return;

            roslynCodeBuilder.AddAssignCoreObj
            (
                this.PluginPropName,
                PluginImplementationNamedTypeSymbol,
                ConcretePluginClassName
            );
        }
    }
}
