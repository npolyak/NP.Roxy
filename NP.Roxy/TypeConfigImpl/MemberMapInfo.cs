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

        internal abstract bool IsAbstract { get; }

        internal virtual bool IsNonPublic => false;

        internal abstract void SetAllowNonPublic(bool allowNonPublic);

        internal abstract string GetEventHandlerAssignmentStr(bool addOrRemoveHandler);

        internal abstract void AddAssignPluginProp(string assignmentStr, RoslynCodeBuilder roslynCodeBuilder);

        internal abstract void AddPropAssignmentStr(bool setOrUnset, RoslynCodeBuilder roslynCodeBuilder);

        internal abstract void AddPluginPropGetterLine(IPropertySymbol wrapperSymbol, RoslynCodeBuilder roslynCodeBuilder);

        internal abstract void AddPluginMethodLine(IMethodSymbol wrapperSymbol, RoslynCodeBuilder roslynCodeBuilder);
    }


    internal class ExpressionMemberMapInfo : MemberMapInfoBase
    {
        internal override bool IsAbstract => false;

        public Expression TheExpression { get; private set; }

        public ExpressionMemberMapInfo
        (
            ISymbol wrapperMemberSymbol, 
            string pluginPropName, 
            Expression expression) 
            : 
            base(wrapperMemberSymbol, pluginPropName)
        {
            this.TheExpression = expression;
        }

        internal override void AddAssignPluginProp(string assignmentStr, RoslynCodeBuilder roslynCodeBuilder)
        {
            
        }

        internal override void AddPropAssignmentStr(bool setOrUnset, RoslynCodeBuilder roslynCodeBuilder)
        {
            
        }

        internal override void AddPluginMethodLine(IMethodSymbol wrapperSymbol, RoslynCodeBuilder roslynCodeBuilder)
        {
            ReplaceArgsExprStringBuilder exprStrBuilder =
                new ReplaceArgsExprStringBuilder(this.PluginPropName.ToCollection().Union(wrapperSymbol.Parameters.Select(param => param.Name)).ToArray());

            exprStrBuilder.Visit(this.TheExpression);

            roslynCodeBuilder.AddText(exprStrBuilder.ToStr() + ";");
        }

        internal override void AddPluginPropGetterLine(IPropertySymbol wrapperSymbol, RoslynCodeBuilder roslynCodeBuilder)
        {
            ReplaceArgsExprStringBuilder exprStrBuilder = 
                new ReplaceArgsExprStringBuilder(this.PluginPropName);

            exprStrBuilder.Visit(this.TheExpression);

            roslynCodeBuilder.AddReturnVar(exprStrBuilder.ToStr());
        }

        internal override string GetEventHandlerAssignmentStr(bool addOrRemoveHandler)
        {
            throw new Exception("Roxy Usage Error: there can be no ExpressionMemberMaps for events");
        }

        internal override void SetAllowNonPublic(bool allowNonPublic)
        {
            throw new Exception("Roxy Usage Error: Non Public Flag cannot be modified for ExpressionMemberMapInfo");
        }
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
            if (this.ThePluginPropSymbol?.HasSetter() != true)
                return;

            string assignmentStr =
                setOrUnset ?
                    WrapperAssignPropName : $"default({(PluginMemberSymbol as IPropertySymbol).Type.AsNamed().GetFullTypeString()})";

            AddAssignPluginProp(assignmentStr, roslynCodeBuilder);
        }

        internal override void AddPluginPropGetterLine(IPropertySymbol wrapperSymbol, RoslynCodeBuilder roslynCodeBuilder)
        {

            if (this.IsNonPublic)
            {
                if (this.AllowNonPublic)
                {
                    string returnType = (wrapperSymbol.Type as INamedTypeSymbol).GetFullTypeString();
                    roslynCodeBuilder.AddLine($"return ({returnType}) {this.PluginPropName}.GetPropValue(\"{this.PluginMemberName}\", true)", true);
                }
            }
            else
            {
                string pluginMemberStr = this.PluginClassMemberFullName;

                roslynCodeBuilder.AddReturnVar(pluginMemberStr);
            }
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
