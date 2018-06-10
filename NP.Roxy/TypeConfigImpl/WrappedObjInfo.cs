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
                    .ForEach(memberMap => memberMap.SetAllowNonPublic(_allowNonPublicForAllMemberMaps));
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

        public WrappedObjInfo(Core core, string wrappedObjPropName)
        {
            this.WrappedObjPropName = wrappedObjPropName;
            this.TheCore = core;
        }

        public bool InitializedThroughConstructor
        {
            get
            {
                return (this.WrappedObjPropSymbol?.GetAttrSymbol(typeof(ConstructorInitAttribute)) != null);
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
        public string WrappedObjPropName { get; private set; }

        [XmlIgnore]
        public string WrapperObjConcretizedPropName =>
            WrappedObjPropName + RoslynAnalysisAndGenerationUtils.CONCRETIZATION_SUFFIX;

        [XmlIgnore]
        public string WrappedObjBackingFieldName =>
            WrappedObjPropName?.PropToFieldName();

        [XmlIgnore]
        public Core TheCore { get; private set; }

        [XmlIgnore]
        public Compilation TheCompilation =>
            TheCore.TheCompilation;

        List<MemberMapInfoBase> WrappedMemberNameMaps { get; } =
            new List<MemberMapInfoBase>();

        public IEnumerable<MemberMapInfoBase> EventWrappedMemberNameMaps =>
            WrappedMemberNameMaps.Where(memberMap => memberMap.TheMemberType == ClassMemberType.Event);

        public IEnumerable<MemberMapInfoBase> PropWrappedMemberNameMaps =>
            WrappedMemberNameMaps.Where(memberMap => memberMap.TheMemberType == ClassMemberType.Property);

        public void SetFromParentSymbol(INamedTypeSymbol parentTypeSymbol)
        {
            this.WrappedObjPropSymbol = 
                parentTypeSymbol.GetMemberByName<IPropertySymbol>(WrappedObjPropName);
        }

        MemberMapInfoBase FindMapImpl<T>(T symbol, Func<MemberMapInfoBase, T> findMethod)
            where T : class
        {
            return WrappedMemberNameMaps.FirstOrDefault(strMap => findMethod(strMap).ObjEquals(symbol));
        }

        MemberMapInfoBase FindMapByWrappedMemberSymbol(string wrappedMemberName)
        {
            return FindMapImpl(wrappedMemberName, strMap => (strMap as MemberMapInfo)?.WrappedMemberName);
        }

        MemberMapInfoBase FindMapByWrapperMemberSymbol(ISymbol wrapperMemberSymbol)
        {
            return FindMapImpl(wrapperMemberSymbol, strMap => strMap.WrapperMemberSymbol);
        }

        internal virtual string GetWrapperMemberSymbol(string wrappedMemberName)
        {
            return FindMapByWrappedMemberSymbol(wrappedMemberName)?.WrapperMemberName ?? wrappedMemberName;
        }

        void CheckMapExists(ISymbol wrapperMemberSymbol)
        {
            MemberMapInfoBase map = FindMapByWrapperMemberSymbol(wrapperMemberSymbol);

            if (map != null)
                throw new Exception($"Roxy Usage Error: the member map for member {wrapperMemberSymbol.Name} of {this.WrappedObjPropName} wrapped obj has already been set.");
        }

        public void SetExpressionMemberMap<TWrappedObj>
        (
            ISymbol wrapperMemberSymbol, 
            Expression expression
        )
        {
            CheckMapExists(wrapperMemberSymbol);

            Type wrappedObjType = typeof(TWrappedObj);

            if (!this.WrappedObjNamedTypeSymbol.Matches(wrappedObjType, this.TheCompilation))
            {
                throw new Exception($"Roxy Usage Error: the type {wrappedObjType.Name} does not match the type {this.WrappedObjNamedTypeSymbol.Name} of the wrapped obj {this.WrappedObjPropName}.");
            }

            ExpressionMemberMapInfo expressionMap = 
                new ExpressionMemberMapInfo(wrapperMemberSymbol, this.WrappedObjPropName, expression);

            WrappedMemberNameMaps.Add(expressionMap);
        }


        // wrapperMemberSymbol == null means that this is 'this' map
        public void SetMap(string wrappedMemberName, ISymbol wrapperMemberSymbol, bool? allowNonPublic = null)
        {
            CheckMapExists(wrapperMemberSymbol);

            if ((wrappedMemberName == null) && (wrapperMemberSymbol != null))
            {
                wrappedMemberName = wrapperMemberSymbol.Name;
            }

            MemberMapInfo map = null;

            if (wrapperMemberSymbol != null)
            {
                map = new MemberMapInfo(wrappedMemberName, wrapperMemberSymbol, this.WrappedObjPropName);
            }
            else
            {
                map = new ThisMemberMapInfo(this.WrappedObjPropName, wrappedMemberName);
            }

            WrappedMemberNameMaps.Add(map);

            bool resultingAllowNonPublic = allowNonPublic ?? this.AllowNonPublicForAllMemberMaps;

            map.SetAllowNonPublic(resultingAllowNonPublic);

            map.SetFromContainingType
            (
                this.TheCompilation,
                this.WrappedObjNamedTypeSymbol,
                this.StaticMethodContainers);

            if (map.WrappedMemberSymbol == null)
            {
                WrappedMemberNameMaps.Remove(map);
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
        public MemberMapInfoBase GetWrappedMemberInfo(ISymbol wrapperMemberSymbol)
        {
            MemberMapInfoBase memberMap = this.FindMapByWrapperMemberSymbol(wrapperMemberSymbol);

            if (memberMap == null)
                return null;

            if (memberMap.IsAbstract == true)
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

                string wrapperName = this.GetWrapperMemberSymbol(wrappedMemberName);

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
                $"if ({WrappedObjPropName} != null)"
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

        public void AddWrappedClass(RoslynCodeBuilder roslynCodeBuilder)
        {
            if (this.WrappedObjNamedTypeSymbol.IsAbstract)
            {
                // here, the concretization is created
                this.ConcreteWrappedObjNamedTypeSymbol =
                    this.TheCore.FindOrCreateConcretizationTypeConf(this.WrappedObjNamedTypeSymbol).TheSelfTypeSymbol;
            }

            string beforeSetterStr = BuildWrapperInit(EventWrappedMemberNameMaps, PropWrappedMemberNameMaps, false);

            string afterSetterStr = BuildWrapperInit(EventWrappedMemberNameMaps.Reverse(), PropWrappedMemberNameMaps.Reverse(), true);

            Accessibility setterAccessibility = Accessibility.Private;

            if (this.WrappedObjPropSymbol.SetMethod != null)
            {
                setterAccessibility = this.WrappedObjPropSymbol.GetMethod.DeclaredAccessibility;
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

        public void AddWrappedObjDefaultConstructorInitialization(RoslynCodeBuilder roslynCodeBuilder)
        {
            //if (!WrappedObjNamedTypeSymbol.HasPublicDefaultConstructor())
            //    return;
            if (InitializedThroughConstructor)
                return;

            if (WrappedObjNamedTypeSymbol.TypeKind == TypeKind.Enum)
                return;

            roslynCodeBuilder.AddAssignCoreObj
            (
                this.WrappedObjPropName, 
                WrappedObjNamedTypeSymbol, 
                ConcreteWrappedObjClassName
            );
        }
    }
}
