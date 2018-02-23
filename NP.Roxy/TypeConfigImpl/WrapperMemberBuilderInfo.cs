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
using NP.Utilities.Attributes;
using NP.Utilities.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NP.Roxy.TypeConfigImpl
{
    internal abstract class WrapperMemberBuilderInfoBase
    {
        public ISymbol WrapperSymbolBase { get; }

        public abstract string WrapperSymbolName { get; }

        protected List<MemberMapInfoBase> WrappedMembers { get; } = new List<MemberMapInfoBase>();

        public void SetWrappedMembers(IEnumerable<MemberMapInfoBase> wrappedMembers)
        {
            WrappedMembers.Clear();

            WrappedMembers.AddRange(wrappedMembers);
        }

        public abstract void SetOverrideVirtual(bool includeBase);

        public WrapperMemberBuilderInfoBase(ISymbol wrapperSymbolBase)
        {
            WrapperSymbolBase = wrapperSymbolBase;
        }
    }

    internal abstract class WrapperMemberBuilderInfo<TSymbol> : WrapperMemberBuilderInfoBase
        where TSymbol : ISymbol
    {
        public Compilation TheCompilation { get; private set; }

        public TSymbol WrapperSymbol { get; }

        public bool MustImplement => WrapperSymbol.IsAbstract || OverrideVirtual;

        public override string WrapperSymbolName =>
            WrapperSymbol?.Name;

        public ClassMemberType WrapperMemberType =>
            WrapperSymbol.GetMemberType();

        public bool OverrideVirtual { get; private set; } = false;
        public bool IncludeBaseVirtualInOverride { get; private set; } = false;

        public bool NoWrappers { get; protected set; } = false;

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
            TSymbol wrapperSymbol,
            Compilation compilation
        ) 
            : base(wrapperSymbol)
        {
            this.WrapperSymbol = wrapperSymbol;
            this.TheCompilation = compilation;
        }

        internal IMemberCodeBuilder<TSymbol> TheCodeBuilder { get; set; } = null;

        internal IMemberCodeBuilder<TSymbol> DefaultCodeBuilder { get; set; } = null;

        protected abstract void BuildImpl
        (
            TSymbol wrapperSymbol, 
            RoslynCodeBuilder roslynCodeBuilder
        );

        protected virtual void BuildIfNoWrappers(RoslynCodeBuilder roslynCodeBuilder)
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
        }

        internal void Build(RoslynCodeBuilder roslynCodeBuilder)
        {
            if (NoWrappers || this.WrappedMembers.IsNullOrEmpty())
            {
                BuildIfNoWrappers(roslynCodeBuilder);

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
            IEventSymbol wrapperSymbol,
            Compilation compilation
        ) : base(wrapperSymbol, compilation)
        {
            AttributeData attrData = WrapperSymbol.GetAttrSymbol(typeof(EventThisIdxAttribute));

            if (attrData != null)
            {
                int idx = (int) attrData.ConstructorArguments.First().Value;

                this.IndexInputParamToReplaceByThis = idx;
            }

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
        public INamedTypeSymbol WrapperTypeSymbol =>
            WrapperSymbol.Type as INamedTypeSymbol;

        public bool ForceCreateSetter { get; private set; } = false;

        public INamedTypeSymbol TheInitTypeSymbol { get; private set; } = null;

        public bool HasInit => TheInitTypeSymbol != null;

        public bool AddBackingField { get; private set; } = false;

        public void AddInit(RoslynCodeBuilder roslynCodeBuilder)
        {
            if (!HasInit)
                return;

            roslynCodeBuilder.AddAssignCoreObj(WrapperSymbolName, TheInitTypeSymbol);
        }

        public PropertyWrapperMemberBuilderInfo
        (
            IPropertySymbol wrapperSymbol,
            Compilation compilation
        ) : base(wrapperSymbol, compilation)
        {
            this.DefaultCodeBuilder = AutoPropBuilder.TheAutoPropBuilder;
        }

        ReplaceArgsExprStringBuilder _expressionBuilder;
        public string ExpressionStr { get; private set; }

        protected override void BuildIfNoWrappers(RoslynCodeBuilder roslynCodeBuilder)
        {
            if (ExpressionStr == null)
            {
                base.BuildIfNoWrappers(roslynCodeBuilder);
            }
            else
            {
                roslynCodeBuilder.AddGetterProp(this.WrapperSymbol, ExpressionStr);
            }
        }

        public void SetPropGetter<TImpl, TProp>(Expression<Func<TImpl, TProp>> propGetterExpression)
        {
            this.NoWrappers = true;

            Type propType = typeof(TProp);

            if (!this.WrapperTypeSymbol.Matches(propType, this.TheCompilation))
                throw new Exception($"Roxy Usage Error: prop type {propType.Name} does not match the required type {this.WrapperTypeSymbol.Name}.");

            if (this.WrapperSymbol.HasSetter())
                throw new Exception($"Roxy Usage Error: Cannot set expression for property {this.WrapperSymbol.Name} since it has a setter");

            _expressionBuilder = new ReplaceArgsExprStringBuilder(RoslynAnalysisAndGenerationUtils.THIS);

            _expressionBuilder.Visit(propGetterExpression);

            ExpressionStr = _expressionBuilder.ToStr();
        }

        public void SetInit
        (
            INamedTypeSymbol initTypeSymbol
        )
        {
            ForceCreateSetter = true;

            if (!TheCompilation.CanBeConvertedImplicitly(initTypeSymbol, this.WrapperTypeSymbol))
            {
                throw new Exception($"Roxy Usage Error: Cannot initialize property '{WrapperSymbol.Name}' to type '{initTypeSymbol.GetFullTypeString()}'");
            }

            TheInitTypeSymbol = initTypeSymbol;

            this.AddBackingField = true;
        }

        protected override void BuildImpl(IPropertySymbol propertyWrapperSymbol, RoslynCodeBuilder roslynCodeBuilder)
        {
            string backingFieldName = propertyWrapperSymbol.Name.PropToFieldName();
            if (this.AddBackingField)
            {
                roslynCodeBuilder.AddPropBackingField(propertyWrapperSymbol);
            }

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

                if (AddBackingField)
                {
                    roslynCodeBuilder.AddReturnVar(backingFieldName);
                }
                else
                {
                    MemberMapInfoBase firstMemberMap =
                        this.WrappedMembers
                            .FirstOrDefault(member => (!member.IsNonPublic));

                    if (firstMemberMap == null)
                    {
                        firstMemberMap = this.WrappedMembers.First();
                    }

                    firstMemberMap.AddWrappedPropGetterLine(propertyWrapperSymbol, roslynCodeBuilder);
                }
                roslynCodeBuilder.Pop();
            }

            if ((propertyWrapperSymbol.SetMethod != null) || ForceCreateSetter || AddBackingField)
            {
                Accessibility setterAccessibility = Accessibility.Private;

                if (propertyWrapperSymbol.SetMethod != null)
                {
                    setterAccessibility =
                        propertyWrapperSymbol.SetMethod.DeclaredAccessibility;
                }

                if (setterAccessibility == propAccessibility)
                {
                    setterAccessibility = Accessibility.NotApplicable;
                }

                roslynCodeBuilder.OpenPropSetter(setterAccessibility);

                if (AddBackingField)
                {
                    roslynCodeBuilder.AddSettingValue(backingFieldName);
                }

                if (this.IncludeBaseVirtualInOverride)
                {
                    roslynCodeBuilder.AddSettingValue($"base.{WrapperSymbolName}");
                }

                foreach (MemberMapInfo memberMap in this.WrappedMembers)
                {
                    memberMap.AddAssignWrappedProp("value", roslynCodeBuilder);
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
            IMethodSymbol wrapperSymbol,
            Compilation compilation
        ) : base(wrapperSymbol, compilation)
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
            foreach (MemberMapInfoBase memberMap in this.WrappedMembers)
            {
                roslynCodeBuilder.AddEmptyLine();
                if (isFirst)
                {
                    roslynCodeBuilder.AddText(firstLineInsert);
                    isFirst = false;
                }

                memberMap.AddWrappedMethodLine(this.WrapperSymbol, roslynCodeBuilder);
            }

            if (!methodWrapperSymbol.ReturnsVoid)
            {
                roslynCodeBuilder.AddReturnVar(returnVarName);
            }

            roslynCodeBuilder.Pop(true);
        } 
    }
}
