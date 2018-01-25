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

namespace NP.Roxy.TypeConfigImpl
{
    internal class WrappedObjInfo
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

                this.WrappedMemberNameMaps
                    .ForEach(memberMap => memberMap.AllowNonPublic = _allowNonPublicForAllMemberMaps);
            }
        }

        [XmlIgnore]
        public IPropertySymbol WrappedObjPropSymbol
        {
            get;
            protected set;
        }

        // INamedTypeSymbol do not seem to carry information about
        // non-public members, so I comment it out for now
        // and will use the Reflection types. (the static method containers are not generated anyways).
        // I will have to reconsider it when working on 
        public IEnumerable<INamedTypeSymbol> StaticMethodContainers { get; } =
            new List<INamedTypeSymbol>();


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
        public INamedTypeSymbol WrappedObjNamedTypeSymbol =>
            WrappedObjPropSymbol?.Type as INamedTypeSymbol;

        [XmlIgnore]
        public string WrappedObjClassName =>
            WrappedObjNamedTypeSymbol.Name;


        INamedTypeSymbol _concreteWrappedObjNamedTypeSymbol = null;
        [XmlIgnore]
        public INamedTypeSymbol ConcreteWrappedObjNamedTypeSymbol
        {
            get => _concreteWrappedObjNamedTypeSymbol ?? WrappedObjNamedTypeSymbol;

            protected set
            {
                if (ReferenceEquals(_concreteWrappedObjNamedTypeSymbol, value))
                    return;

                if (ReferenceEquals(WrappedObjNamedTypeSymbol, value))
                {
                    _concreteWrappedObjNamedTypeSymbol = null;

                    return;
                }

                _concreteWrappedObjNamedTypeSymbol = value;
            }
        }

        [XmlIgnore]
        public string ConcreteWrappedObjClassName =>
            this.ConcreteWrappedObjNamedTypeSymbol.Name;

        [XmlAttribute]
        public string WrappedObjPropName { get; internal set; }

        [XmlIgnore]
        public string WrapperObjConcretizedPropName =>
            WrappedObjPropName + RoslynAnalysisAndGenerationUtils.CONCRETIZATION_SUFFIX;

        [XmlIgnore]
        public string WrappedObjBackingFieldName =>
            WrappedObjPropName?.PropToFieldName();

        [XmlIgnore]
        public Core TheCore { get; internal set; }

        [XmlIgnore]
        public Compilation TheCompilation =>
            TheCore.TheCompilation;

        List<MemberMapInfo> WrappedMemberNameMaps { get; } =
            new List<MemberMapInfo>();

        public IEnumerable<MemberMapInfo> EventWrappedMemberNameMaps =>
            WrappedMemberNameMaps.Where(memberMap => memberMap.TheWrappedSymbol is IEventSymbol);


        public void SetFromParentSymbol(INamedTypeSymbol parentTypeSymbol)
        {
            this.WrappedObjPropSymbol = 
                parentTypeSymbol.GetMemberByName<IPropertySymbol>(WrappedObjPropName);
        }
        MemberMapInfo FindMapImpl(string name, Func<MemberMapInfo, string> findMethod)
        {
            return WrappedMemberNameMaps.FirstOrDefault(strMap => findMethod(strMap) == name);
        }

        MemberMapInfo FindMap(string wrappedMemberName)
        {
            return FindMapImpl(wrappedMemberName, strMap => strMap.WrappedMemberName);
        }

        MemberMapInfo FindMapByWrapperMemberName(string wrapperMemberName)
        {
            return FindMapImpl(wrapperMemberName, strMap => strMap.WrapperMemberName);
        }

        public string GetWrapperMemberName(string wrappedMemberName)
        {
            return FindMap(wrappedMemberName)?.WrapperMemberName ?? wrappedMemberName;
        }

        public string GetWrappedMemberName(string wrapperMemberName)
        {
            return FindMapByWrapperMemberName(wrapperMemberName).WrappedMemberName ?? wrapperMemberName;
        }

        public void SetMap(string wrappedMemberName, string wrapperMemberName, bool? allowNonPublic = null)
        {
            MemberMapInfo map = FindMapByWrapperMemberName(wrapperMemberName);

           
            if (map == null)
            {
                if (wrappedMemberName == null)
                    wrappedMemberName = wrapperMemberName;

                map = new MemberMapInfo(wrappedMemberName, wrapperMemberName);
                map.SetWrappedObjPropName(this.WrappedObjPropName);

                WrappedMemberNameMaps.Add(map);
            }
            else // if exists - simply modify it
            {
                map.WrappedMemberName = wrappedMemberName;
            }

            bool resultingAllowNonPublic = allowNonPublic ?? this.AllowNonPublicForAllMemberMaps;

            map.AllowNonPublic = resultingAllowNonPublic;

            map.SetFromContainingType
            (
                this.TheCompilation,
                this.WrappedObjNamedTypeSymbol,
                this.StaticMethodContainers);

            if (map.TheWrappedSymbol == null)
            {
                WrappedMemberNameMaps.Remove(map);
            }
        }

        // if such wrapper name does not exist in 
        // the current wrapper maps, but exists among the
        // public variables of the Wrapped Object, we add 
        // the wrapper map whose wrapper and wrapped names
        // are equal to the passed wrapperMemberName
        public void AddWrapperMapIfMissing(string wrapperMemberName)
        {
            MemberMapInfo map = FindMapByWrapperMemberName(wrapperMemberName);

            if (map != null)
                return;

            this.SetMap(wrapperMemberName, wrapperMemberName);
        }


        public void AddMissingMaps(IEnumerable<string> wrapperMemberNames)
        {
            wrapperMemberNames.DoForEach(wrapperMemberName => AddWrapperMapIfMissing(wrapperMemberName));
        }

        // gets all member infos for the wrapperMemberName 
        // (including those that do not require renaming)
        public MemberMapInfo GetWrappedMemberInfo(string wrapperMemberName)
        {
            MemberMapInfo memberMap = this.FindMapByWrapperMemberName(wrapperMemberName);

            if (memberMap == null)
                return null;

            if (memberMap.TheWrappedSymbol?.IsAbstract == true)
                return null;

            return memberMap;
        }

        void SetOrUnsetConcretizationDelegates(RoslynCodeBuilder wrapperInitBuilder, bool setOrUnset)
        {
            foreach (ISymbol concreteWrappedObjMember in this.ConcreteWrappedObjNamedTypeSymbol.GetAllMembers())
            {
                AttributeData concretizationAttrData =
                    concreteWrappedObjMember.GetAttrSymbol(typeof(ConcretizationDelegateAttribute));

                if (concretizationAttrData == null)
                    continue;

                TypedConstant constrArg =
                    concretizationAttrData.ConstructorArguments[0];

                string wrappedMemberName = constrArg.Value as string;

                string wrapperName = this.GetWrapperMemberName(wrappedMemberName);

                if (wrapperName == null)
                    continue;

                string memberDelegateName = concreteWrappedObjMember.Name;

                wrapperInitBuilder.AddLine($"({this.WrappedObjPropName} as {this.ConcreteWrappedObjClassName}).{memberDelegateName} = ", false, false);

                if (setOrUnset)
                {
                    if (concretizationAttrData.MatchesAttrType(typeof(PropGetterConcretizationDelegateAttribute)))
                    {
                        wrapperInitBuilder.AddText($"{RoslynAnalysisAndGenerationUtils.GetPropGetterDelegateAssigment(wrapperName)}");
                    }
                    else if (concretizationAttrData.MatchesAttrType(typeof(PropSetterConcretizationDelegateAttribute)))
                    {
                        wrapperInitBuilder.AddText($"{RoslynAnalysisAndGenerationUtils.GetPropSetterDelegateAssigment(wrapperName)}");
                    }
                    else //if (concretizationAttrData.MatchesAttrType(typeof(MethodConcretizationDelegateAttribute)))
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

        string BuildWrapperInit(IEnumerable<MemberMapInfo> memberMaps, bool addOrRemove)
        {
            RoslynCodeBuilder wrapperInitBuilder = new RoslynCodeBuilder();

            wrapperInitBuilder.AddEmptyLine();
            wrapperInitBuilder.AddLine
            (
                $"if ({WrappedObjPropName} != null)"
            );

            wrapperInitBuilder.Push();

            if (!addOrRemove)
            {
                SetOrUnsetConcretizationDelegates(wrapperInitBuilder, false);

                wrapperInitBuilder.AddLine($"{TypeConfigBase.CALL_STATIC_UNINIT_METHOD}()", true);
            }

            foreach (MemberMapInfo eventMemberMap in memberMaps)
            {
                wrapperInitBuilder.AddLine(eventMemberMap.GetEventHandlerAssignmentStr(addOrRemove), true);
            }

            if (addOrRemove)
            {
                wrapperInitBuilder.AddLine($"{TypeConfigBase.CALL_STATIC_INIT_METHOD}()", true);

                SetOrUnsetConcretizationDelegates(wrapperInitBuilder, true);
            }

            wrapperInitBuilder.Pop();

            return wrapperInitBuilder.ToStr();
        }

        public void AddWrappedClass(RoslynCodeBuilder roslynCodeBuilder)
        {
            string beforeSetterStr = BuildWrapperInit(EventWrappedMemberNameMaps, false);

            string afterSetterStr = BuildWrapperInit(EventWrappedMemberNameMaps.Reverse(), true);

            Accessibility setterAccessibility = Accessibility.Private;

            if (this.WrappedObjPropSymbol.SetMethod != null)
            {
                setterAccessibility = this.WrappedObjPropSymbol.GetMethod.DeclaredAccessibility;
            }

            if (this.WrappedObjNamedTypeSymbol.IsAbstract)
            {
                // here, the concretization is created
                this.ConcreteWrappedObjNamedTypeSymbol =
                    this.TheCore.FindOrCreateConcretizationTypeConf(this.WrappedObjNamedTypeSymbol).TheSelfTypeSymbol;
            }

            roslynCodeBuilder.AddPropWithBackingStore
            (
                this.WrappedObjPropName,
                this.WrappedObjBackingFieldName,
                this.WrappedObjNamedTypeSymbol,
                Accessibility.Public,
                beforeSetterStr,
                afterSetterStr,
                setterAccessibility
            );

            if (ConcreteWrappedObjClassName != WrappedObjClassName)
            {
                roslynCodeBuilder.AddEmptyLine();
                roslynCodeBuilder.AddPropOpening(WrapperObjConcretizedPropName, ConcreteWrappedObjNamedTypeSymbol);

                roslynCodeBuilder.AddLine($"{RoslynCodeBuilder.GETTER} => ({ConcreteWrappedObjClassName}){WrappedObjPropName}", true);

                roslynCodeBuilder.Pop();
            }
        }

        public void AddDefaultConstructor(RoslynCodeBuilder roslynCodeBuilder)
        {
            roslynCodeBuilder
                .AddAssignmentLine
                (
                    this.WrappedObjPropName,
                    $"TheCore.FindOrCreateClassObj<{WrappedObjNamedTypeSymbol.GetFullTypeString()}>(\"{ConcreteWrappedObjClassName}\")"
                    //$"new {this.WrappedObjNamedTypeSymbol.GetFullTypeString()}()"
                );
        }
    }
}
