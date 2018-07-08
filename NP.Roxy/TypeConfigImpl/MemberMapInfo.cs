﻿// (c) Nick Polyak 2018 - http://awebpros.com/
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
using NP.Concepts.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NP.Roxy.TypeConfigImpl
{
    internal abstract class MemberMapInfoBase
    {
        public ISymbol WrapperMemberSymbol { get; }

        // gets this value only from the parent object
        public string PluginPropName { get; private set; }

        public string WrapperMemberName => WrapperMemberSymbol.Name;

        public virtual ClassMemberType TheMemberType =>
            WrapperMemberSymbol.GetMemberType();

        public virtual string WrapperAssignPropName => WrapperMemberName;

        public MemberMapInfoBase
        (
            ISymbol wrapperMemberSymbol, 
            string pluginPropName)
        {
            this.WrapperMemberSymbol = wrapperMemberSymbol;
            this.PluginPropName = pluginPropName;
        }

        public void AddCheckForSharedLine(RoslynCodeBuilder roslynCodeBuilder)
        {
            roslynCodeBuilder.AddLine($"if (!{this.PluginPropName.GetIsSharedFromExternalSourceFieldName()})");
        }

        internal abstract bool IsAbstract { get; }

        internal virtual bool IsNonPublic => false;

        internal abstract void SetAllowNonPublic(bool allowNonPublic);

        internal abstract string GetEventHandlerAssignmentStr(bool addOrRemoveHandler);

        internal abstract void AddAssignPluginProp(string assignmentStr, RoslynCodeBuilder roslynCodeBuilder);

        internal abstract void AddPropAssignmentStr(bool setOrUnset, RoslynCodeBuilder roslynCodeBuilder);

        internal abstract void AddPluginPropGetterLine(IPropertySymbol wrapperSymbol, RoslynCodeBuilder roslynCodeBuilder);

        internal abstract void AddPluginMethodLine(IMethodSymbol wrapperSymbol, RoslynCodeBuilder roslynCodeBuilder);
    }


    internal class MemberMapInfo : MemberMapInfoBase
    {
        internal override bool IsAbstract =>
            PluginMemberSymbol?.IsAbstract == true;

        public ISymbol PluginMemberSymbol { get; protected set; }

        [XmlAttribute]
        public string PluginMemberName { get; set; }

        [XmlAttribute]
        public bool AllowNonPublic { get; private set; } = false;

        internal override void SetAllowNonPublic(bool allowNonPublic)
        {
            this.AllowNonPublic = allowNonPublic;
        }

        public bool HasGetter =>
            (PluginMemberSymbol is IPropertySymbol propSymbol) &&
            propSymbol.HasGetter();

        public bool HasSetter =>
            (PluginMemberSymbol is IPropertySymbol propSymbol) &&
            propSymbol.HasSetter();

        public MemberMapInfo
        (
            string pluginMemberName, 
            ISymbol wrapperMemberSymbol, 
            string pluginPropName
        ) :
            base(wrapperMemberSymbol, pluginPropName)
        {
            this.PluginMemberName = pluginMemberName;
        }

        internal void SetFromContainingType
        (
            Compilation compilation,
            INamedTypeSymbol containingType, 
            IEnumerable<INamedTypeSymbol> staticMethodContainers)
        {
            if (this.PluginMemberName == null)
                return;

            this.PluginMemberSymbol =
                compilation.FindMatchingSymbol(this.WrapperMemberSymbol, containingType, this.PluginMemberName, this.AllowNonPublic);

            if (this.PluginMemberSymbol != null)
                return;

            foreach(INamedTypeSymbol typeSymbol in staticMethodContainers)
            {
                this.PluginMemberSymbol =
                    compilation.GetStaticMethodWithFirstArgThatCanAcceptType
                    (
                        typeSymbol,
                        containingType,
                        this.PluginMemberName,
                        this.AllowNonPublic);

                if (this.PluginMemberSymbol != null)
                    break;
            }
        }

        // if private - WrappedSymbol will be null
        internal override bool IsNonPublic =>
            this.PluginMemberSymbol.DeclaredAccessibility != Accessibility.Public;

        protected string GetPluginClassMemberFullName(bool hasNullCheck)
        {
            string divider = hasNullCheck ? "?." : ".";

            if (this.PluginMemberSymbol.IsStatic)
            {
                IMethodSymbol methodSymbol =
                    this.PluginMemberSymbol as IMethodSymbol;

                string containingTypeStr = methodSymbol.ContainingType.GetFullTypeString();

                return $"{containingTypeStr}{divider}{PluginMemberName}";
            }
            else
            {
                return $"{this.PluginPropName}{divider}{PluginMemberName}";
            }
        }

        private string PluginClassMemberFullName =>
            GetPluginClassMemberFullName(false);

        internal override string GetEventHandlerAssignmentStr(bool addOrRemoveHandler)
        {
            string addOrRemoveStr = addOrRemoveHandler ? "+" : "-";

            string result =
                $"{PluginClassMemberFullName} {addOrRemoveStr}= {WrapperMemberName.GetEventInvocationWrapperName()}";

            return result;
        }

        internal override void AddAssignPluginProp(string assignmentStr, RoslynCodeBuilder roslynCodeBuilder)
        {
            if (!HasSetter)
                return;

            if (this.IsNonPublic)
            {
                if (this.AllowNonPublic)
                {
                    roslynCodeBuilder.AddLine($"{this.PluginPropName}.SetPropValue(\"{this.PluginMemberName}\", {assignmentStr}, true)", true);
                }
                else
                {
                    return; // do not add anything
                }
            }
            else
            {
                roslynCodeBuilder.AddAssignmentLine(PluginClassMemberFullName, assignmentStr);
            }
        }

        IPropertySymbol ThePluginPropSymbol => PluginMemberSymbol as IPropertySymbol;

        internal override void AddPropAssignmentStr(bool setOrUnset, RoslynCodeBuilder roslynCodeBuilder)
        {
            if (!this.HasSetter)
                return;

            string assignmentStr =
                setOrUnset ?
                    WrapperAssignPropName : $"default({(PluginMemberSymbol as IPropertySymbol).Type.AsNamed().GetFullTypeString()})";

            AddAssignPluginProp(assignmentStr, roslynCodeBuilder);
        }

        internal override void AddPluginPropGetterLine(IPropertySymbol wrapperSymbol, RoslynCodeBuilder roslynCodeBuilder)
        {
            if (!HasGetter)
                return;

            string returnType = (wrapperSymbol.Type as INamedTypeSymbol).GetFullTypeString();
            this.AddCheckForSharedLine(roslynCodeBuilder);
            roslynCodeBuilder.Push();
            if (this.IsNonPublic)
            {
                if (this.AllowNonPublic)
                {
                    roslynCodeBuilder
                        .AddLine($"return ({returnType}) {this.PluginPropName}.GetPropValue(\"{this.PluginMemberName}\", true)", true);
                }
            }
            else
            {
                string pluginMemberStr = this.PluginClassMemberFullName;

                roslynCodeBuilder.AddReturnVar(pluginMemberStr);
            }
            roslynCodeBuilder.Pop();
            roslynCodeBuilder.AddLine("else");
            roslynCodeBuilder.Push();
            roslynCodeBuilder.AddReturnVar($"default({returnType})");
            roslynCodeBuilder.Pop();
        }

        internal override void AddPluginMethodLine
        (
            IMethodSymbol wrapperSymbol, 
            RoslynCodeBuilder roslynCodeBuilder
        )
        {
            IMethodSymbol pluginMethodSymbol = this.PluginMemberSymbol as IMethodSymbol;

            if (this.IsNonPublic)
            {
                if (this.AllowNonPublic)
                {
                    roslynCodeBuilder
                        .AddNonPublicMethodCall
                        (
                            wrapperSymbol,
                            this.PluginPropName,
                            this.PluginMemberName,
                            pluginMethodSymbol
                        );
                }
            }
            else
            {
                if (this.PluginMemberSymbol.IsStatic)
                {
                    roslynCodeBuilder.AddStaticMethodCall
                    (
                        wrapperSymbol,
                        this.PluginPropName,
                        this.PluginClassMemberFullName);
                }
                else
                {
                    roslynCodeBuilder.AddMethodCall
                    (
                        wrapperSymbol,
                        this.PluginClassMemberFullName);
                }
            }
        }
    }


    internal class ThisMemberMapInfo : MemberMapInfo
    {
        public ThisMemberMapInfo(string pluginPropName, string pluginMemberName) : 
            base(pluginMemberName, null, pluginPropName)
        {

        }

        public override string WrapperAssignPropName => RoslynAnalysisAndGenerationUtils.THIS;

        public override ClassMemberType TheMemberType => ClassMemberType.Property;
    }
}
