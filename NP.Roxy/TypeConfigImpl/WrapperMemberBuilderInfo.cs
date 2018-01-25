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
using System.Text;
using System.Threading.Tasks;

namespace NP.Roxy.TypeConfigImpl
{
    internal abstract class WrapperMemberBuilderInfoBase
    {
        public abstract string WrapperSymbolName { get; }

        protected List<MemberMapInfo> WrappedMembers { get; } = new List<MemberMapInfo>();

        public void SetWrappedMembers(IEnumerable<MemberMapInfo> wrappedMembers)
        {
            WrappedMembers.Clear();

            WrappedMembers.AddRange(wrappedMembers);
        }

        public abstract void SetOverrideVirtual(bool includeBase);
    }

    internal abstract class WrapperMemberBuilderInfo<TSymbol> : WrapperMemberBuilderInfoBase
        where TSymbol : ISymbol
    {
        public TSymbol WrapperSymbol { get; }

        public bool MustImplement => WrapperSymbol.IsAbstract || OverrideVirtual;

        public override string WrapperSymbolName =>
            WrapperSymbol?.Name;

        public ClassMemberType WrapperMemberType =>
            WrapperSymbol.GetMemberType();

        public bool OverrideVirtual { get; private set; } = false;
        public bool IncludeBaseVirtualInOverride { get; private set; } = false;

        public override void SetOverrideVirtual(bool includeBase)
        {
            if (!WrapperSymbol.IsVirtual)
            {
                throw new Exception($"Roxy Usage Error: Cannot set override on non-virtual member {WrapperSymbolName}");
            }

            OverrideVirtual = true;

            IncludeBaseVirtualInOverride = includeBase;
        }

        public WrapperMemberBuilderInfo
        (
            TSymbol wrapperSymbol
        )
        {
            this.WrapperSymbol = wrapperSymbol;
        }

        internal IMemberCodeBuilder<TSymbol> TheCodeBuilder { get; set; } = null;

        internal IMemberCodeBuilder<TSymbol> DefaultCodeBuilder { get; set; } = null;

        protected abstract void BuildImpl
        (
            TSymbol wrapperSymbol, 
            RoslynCodeBuilder roslynCodeBuilder
        );


        internal void Build(RoslynCodeBuilder roslynCodeBuilder)
        {
            if (this.WrappedMembers.IsNullOrEmpty())
            {
                if (TheCodeBuilder != null)
                {
                    TheCodeBuilder.Build(this.WrapperSymbol, roslynCodeBuilder);
                    return;
                }

                if (DefaultCodeBuilder != null)
                {
                    DefaultCodeBuilder.Build(this.WrapperSymbol, roslynCodeBuilder);
                }

                return;
            }

            BuildImpl(this.WrapperSymbol, roslynCodeBuilder);
        }
    }

    internal class EventWrapperMemberBuilderInfo : WrapperMemberBuilderInfo<IEventSymbol>
    {
        public int IndexInputParamToReplaceByThis { get; set; } = -1;

        public override void SetOverrideVirtual(bool includeBase)
        {
            throw new Exception($"Usage Error: Events cannot be Virtual. Cannot set override virtual on event {WrapperSymbol.Name}");
        }

        public EventWrapperMemberBuilderInfo
        (
            IEventSymbol wrapperSymbol
        ) : base(wrapperSymbol)
        {
            this.DefaultCodeBuilder = new SimpleEventBuilder();
        }

        protected override void BuildImpl(IEventSymbol wrapperSymbol, RoslynCodeBuilder roslynCodeBuilder)
        {
            roslynCodeBuilder.AddEventDefinitionAndEventInvocationMethod
            (
                this.WrapperSymbol,
                this.IndexInputParamToReplaceByThis
            );
        }
    }

    internal class PropertyWrapperMemberBuilderInfo : WrapperMemberBuilderInfo<IPropertySymbol>
    {
        public PropertyWrapperMemberBuilderInfo
        (
            IPropertySymbol wrapperSymbol
        ) : base(wrapperSymbol)
        {
            this.DefaultCodeBuilder = AutoPropBuilder.TheAutoPropBuilder;
        }

        protected override void BuildImpl(IPropertySymbol propertyWrapperSymbol, RoslynCodeBuilder roslynCodeBuilder)
        {
            Accessibility propAccessibility = propertyWrapperSymbol.DeclaredAccessibility;

            roslynCodeBuilder.AddPropOpening
            (
                propertyWrapperSymbol.Name,
                propertyWrapperSymbol.Type as INamedTypeSymbol,
                propertyWrapperSymbol.DeclaredAccessibility,
                propertyWrapperSymbol.ShouldOverride()
            );

            if (propertyWrapperSymbol.GetMethod != null)
            {
                Accessibility getterAccessibility = propertyWrapperSymbol.GetMethod.DeclaredAccessibility;

                if (getterAccessibility == propAccessibility)
                    getterAccessibility = Accessibility.NotApplicable;

                roslynCodeBuilder.OpenPropGetter(getterAccessibility);

                MemberMapInfo firstMemberMap = 
                    this.WrappedMembers
                        .FirstOrDefault(member => (!member.IsNonPublic) );

                if (firstMemberMap == null)
                {
                    firstMemberMap = this.WrappedMembers.First();
                }

                if (firstMemberMap.IsNonPublic)
                {
                    if (firstMemberMap.AllowNonPublic)
                    {
                        string returnType = (propertyWrapperSymbol.Type as INamedTypeSymbol).GetFullTypeString();
                        roslynCodeBuilder.AddLine($"return ({returnType}) {firstMemberMap.WrappedObjPropName}.GetPropValue(\"{firstMemberMap.WrappedMemberName}\", true)", true);
                    }
                }
                else
                {
                    string wrappedMemberStr = firstMemberMap.WrappedClassMemberFullName;

                    roslynCodeBuilder.AddReturnVar(wrappedMemberStr);
                }
                roslynCodeBuilder.Pop();
            }

            if (propertyWrapperSymbol.SetMethod != null)
            {
                Accessibility setterAccessibility = propertyWrapperSymbol.SetMethod.DeclaredAccessibility;

                if (setterAccessibility == propAccessibility)
                {
                    setterAccessibility = Accessibility.NotApplicable;
                }

                roslynCodeBuilder.OpenPropSetter(setterAccessibility);

                if (this.IncludeBaseVirtualInOverride)
                {
                    roslynCodeBuilder.AddSettingValue($"base.{WrapperSymbolName}");
                }

                foreach (MemberMapInfo memberMap in this.WrappedMembers)
                {
                    if (memberMap.IsNonPublic)
                    {
                        if (memberMap.AllowNonPublic)
                        {
                            roslynCodeBuilder.AddLine($"{memberMap.WrappedObjPropName}.SetPropValue(\"{memberMap.WrappedMemberName}\", value, true)", true);
                        }
                    }
                    else
                    {
                        string wrappedMemberStr = memberMap.WrappedClassMemberFullName;
                        roslynCodeBuilder.AddSettingValue(wrappedMemberStr);
                    }
                }

                roslynCodeBuilder.Pop();
            }

            roslynCodeBuilder.Pop();
        }
    }

    internal class MethodWrapperMemberBuilderInfo : WrapperMemberBuilderInfo<IMethodSymbol>
    {
        public MethodWrapperMemberBuilderInfo
        (
            IMethodSymbol wrapperSymbol
        ) : base(wrapperSymbol)
        {
        }

        protected override void BuildImpl(IMethodSymbol methodWrapperSymbol, RoslynCodeBuilder roslynCodeBuilder)
        {
            roslynCodeBuilder.AddMethodOpeningFromSymbolOnly(methodWrapperSymbol);

            string returnVarName = "result";
            string firstLineInsert = "";
            if (!methodWrapperSymbol.ReturnsVoid)
            {
                firstLineInsert =
                    (methodWrapperSymbol.ReturnType as INamedTypeSymbol).GetFullTypeString() + $" {returnVarName} = ";
            }

            if (this.IncludeBaseVirtualInOverride)
            {
                roslynCodeBuilder.AddEmptyLine();
                roslynCodeBuilder.AddMethodCall(WrapperSymbol, $"base.{WrapperSymbolName}");
            }

            bool isFirst = true;
            foreach (MemberMapInfo memberMap in this.WrappedMembers)
            {
                roslynCodeBuilder.AddEmptyLine();
                if (isFirst)
                {
                    roslynCodeBuilder.AddText(firstLineInsert);
                    isFirst = false;
                }

                IMethodSymbol wrappedMethodSymbol = memberMap.TheWrappedSymbol as IMethodSymbol;

                if (memberMap.IsNonPublic)
                {
                    if (memberMap.AllowNonPublic)
                    {
                        roslynCodeBuilder
                            .AddNonPublicMethodCall
                            (
                                WrapperSymbol, 
                                memberMap.WrappedObjPropName, 
                                memberMap.WrappedMemberName,
                                wrappedMethodSymbol
                            );
                    }
                }
                else
                {
                    if (memberMap.TheWrappedSymbol.IsStatic)
                    {
                        roslynCodeBuilder.AddStaticMethodCall
                        (
                            WrapperSymbol,
                            memberMap.WrappedObjPropName,
                            memberMap.WrappedClassMemberFullName);
                    }
                    else
                    {
                        roslynCodeBuilder.AddMethodCall
                        (
                            WrapperSymbol,
                            memberMap.WrappedClassMemberFullName);
                    }
                }
            }

            if (!methodWrapperSymbol.ReturnsVoid)
            {
                roslynCodeBuilder.AddReturnVar(returnVarName);
            }

            roslynCodeBuilder.Pop(true);
        } 
    }
}
