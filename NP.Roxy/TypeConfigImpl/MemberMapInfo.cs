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
using NP.Utilities.Expressions;
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
        public string WrappedObjPropName { get; private set; }

        public string WrapperMemberName => WrapperMemberSymbol.Name;

        public virtual ClassMemberType TheMemberType =>
            WrapperMemberSymbol.GetMemberType();

        public virtual string WrapperAssignPropName => WrapperMemberName;

        public MemberMapInfoBase
        (
            ISymbol wrapperMemberSymbol, 
            string wrappedObjPropName)
        {
            this.WrapperMemberSymbol = wrapperMemberSymbol;
            this.WrappedObjPropName = wrappedObjPropName;
        }

        internal abstract bool IsAbstract { get; }

        internal virtual bool IsNonPublic => false;

        internal abstract void SetAllowNonPublic(bool allowNonPublic);

        internal abstract string GetEventHandlerAssignmentStr(bool addOrRemoveHandler);

        internal abstract void AddAssignWrappedProp(string assignmentStr, RoslynCodeBuilder roslynCodeBuilder);

        internal abstract void AddPropAssignmentStr(bool setOrUnset, RoslynCodeBuilder roslynCodeBuilder);

        internal abstract void AddWrappedPropGetterLine(IPropertySymbol wrapperSymbol, RoslynCodeBuilder roslynCodeBuilder);

        internal abstract void AddWrappedMethodLine(IMethodSymbol wrapperSymbol, RoslynCodeBuilder roslynCodeBuilder);
    }


    internal class ExpressionMemberMapInfo : MemberMapInfoBase
    {
        internal override bool IsAbstract => false;

        public Expression TheExpression { get; private set; }

        public ExpressionMemberMapInfo
        (
            ISymbol wrapperMemberSymbol, 
            string wrappedObjPropName, 
            Expression expression) 
            : 
            base(wrapperMemberSymbol, wrappedObjPropName)
        {
            this.TheExpression = expression;
        }

        internal override void AddAssignWrappedProp(string assignmentStr, RoslynCodeBuilder roslynCodeBuilder)
        {
            
        }

        internal override void AddPropAssignmentStr(bool setOrUnset, RoslynCodeBuilder roslynCodeBuilder)
        {
            
        }

        internal override void AddWrappedMethodLine(IMethodSymbol wrapperSymbol, RoslynCodeBuilder roslynCodeBuilder)
        {
            ReplaceFirstArgExprStringBuilder exprStrBuilder =
                new ReplaceFirstArgExprStringBuilder(this.WrappedObjPropName);

            exprStrBuilder.Visit(this.TheExpression);

            roslynCodeBuilder.AddText(exprStrBuilder.ToStr() + ";");
        }

        internal override void AddWrappedPropGetterLine(IPropertySymbol wrapperSymbol, RoslynCodeBuilder roslynCodeBuilder)
        {
            ReplaceFirstArgExprStringBuilder exprStrBuilder = 
                new ReplaceFirstArgExprStringBuilder(this.WrappedObjPropName);

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
            WrappedMemberSymbol?.IsAbstract == true;

        public ISymbol WrappedMemberSymbol { get; protected set; }

        [XmlAttribute]
        public string WrappedMemberName { get; set; }

        [XmlAttribute]
        public bool AllowNonPublic { get; private set; } = false;

        internal override void SetAllowNonPublic(bool allowNonPublic)
        {
            this.AllowNonPublic = allowNonPublic;
        }

        public MemberMapInfo
        (
            string wrappedMemberName, 
            ISymbol wrapperMemberSymbol, 
            string wrappedObjPropName
        ) :
            base(wrapperMemberSymbol, wrappedObjPropName)
        {
            this.WrappedMemberName = wrappedMemberName;
        }

        internal void SetFromContainingType
        (
            Compilation compilation,
            INamedTypeSymbol containingType, 
            IEnumerable<INamedTypeSymbol> staticMethodContainers)
        {
            if (this.WrappedMemberName == null)
                return;

            this.WrappedMemberSymbol =
                compilation.FindMatchingSymbol(this.WrapperMemberSymbol, containingType, this.WrappedMemberName, this.AllowNonPublic);

            if (this.WrappedMemberSymbol != null)
                return;

            foreach(INamedTypeSymbol typeSymbol in staticMethodContainers)
            {
                this.WrappedMemberSymbol =
                    compilation.GetStaticMethodWithFirstArgThatCanAcceptType
                    (
                        typeSymbol,
                        containingType,
                        this.WrappedMemberName,
                        this.AllowNonPublic);

                if (this.WrappedMemberSymbol != null)
                    break;
            }
        }

        // if private - WrappedSymbol will be null
        internal override bool IsNonPublic =>
            this.WrappedMemberSymbol.DeclaredAccessibility != Accessibility.Public;

        protected string GetWrappedClassMemberFullName(bool hasNullCheck)
        {
            string divider = hasNullCheck ? "?." : ".";

            if (this.WrappedMemberSymbol.IsStatic)
            {
                IMethodSymbol methodSymbol =
                    this.WrappedMemberSymbol as IMethodSymbol;

                string containingTypeStr = methodSymbol.ContainingType.GetFullTypeString();

                return $"{containingTypeStr}{divider}{WrappedMemberName}";
            }
            else
            {
                return $"{this.WrappedObjPropName}{divider}{WrappedMemberName}";
            }
        }

        private string WrappedClassMemberFullName =>
            GetWrappedClassMemberFullName(false);

        internal override string GetEventHandlerAssignmentStr(bool addOrRemoveHandler)
        {
            string addOrRemoveStr = addOrRemoveHandler ? "+" : "-";

            string result =
                $"{WrappedClassMemberFullName} {addOrRemoveStr}= {WrapperMemberName.GetEventInvocationWrapperName()}";

            return result;
        }

        internal override void AddAssignWrappedProp(string assignmentStr, RoslynCodeBuilder roslynCodeBuilder)
        {
            if (this.IsNonPublic)
            {
                if (this.AllowNonPublic)
                {
                    roslynCodeBuilder.AddLine($"{this.WrappedObjPropName}.SetPropValue(\"{this.WrappedMemberName}\", {assignmentStr}, true)", true);
                }
                else
                {
                    return; // do not add anything
                }
            }
            else
            {
                roslynCodeBuilder.AddAssignmentLine(WrappedClassMemberFullName, assignmentStr);
            }
        }

        IPropertySymbol TheWrappedPropSymbol => WrappedMemberSymbol as IPropertySymbol;

        internal override void AddPropAssignmentStr(bool setOrUnset, RoslynCodeBuilder roslynCodeBuilder)
        {
            if (this.TheWrappedPropSymbol?.HasSetter() != true)
                return;

            string assignmentStr =
                setOrUnset ?
                    WrapperAssignPropName : $"default({(WrappedMemberSymbol as IPropertySymbol).Type.AsNamed().GetFullTypeString()})";

            AddAssignWrappedProp(assignmentStr, roslynCodeBuilder);
        }

        internal override void AddWrappedPropGetterLine(IPropertySymbol wrapperSymbol, RoslynCodeBuilder roslynCodeBuilder)
        {

            if (this.IsNonPublic)
            {
                if (this.AllowNonPublic)
                {
                    string returnType = (wrapperSymbol.Type as INamedTypeSymbol).GetFullTypeString();
                    roslynCodeBuilder.AddLine($"return ({returnType}) {this.WrappedObjPropName}.GetPropValue(\"{this.WrappedMemberName}\", true)", true);
                }
            }
            else
            {
                string wrappedMemberStr = this.WrappedClassMemberFullName;

                roslynCodeBuilder.AddReturnVar(wrappedMemberStr);
            }
        }

        internal override void AddWrappedMethodLine
        (
            IMethodSymbol wrapperSymbol, 
            RoslynCodeBuilder roslynCodeBuilder
        )
        {
            IMethodSymbol wrappedMethodSymbol = this.WrappedMemberSymbol as IMethodSymbol;

            if (this.IsNonPublic)
            {
                if (this.AllowNonPublic)
                {
                    roslynCodeBuilder
                        .AddNonPublicMethodCall
                        (
                            wrapperSymbol,
                            this.WrappedObjPropName,
                            this.WrappedMemberName,
                            wrappedMethodSymbol
                        );
                }
            }
            else
            {
                if (this.WrappedMemberSymbol.IsStatic)
                {
                    roslynCodeBuilder.AddStaticMethodCall
                    (
                        wrapperSymbol,
                        this.WrappedObjPropName,
                        this.WrappedClassMemberFullName);
                }
                else
                {
                    roslynCodeBuilder.AddMethodCall
                    (
                        wrapperSymbol,
                        this.WrappedClassMemberFullName);
                }
            }
        }
    }


    internal class ThisMemberMapInfo : MemberMapInfo
    {
        public ThisMemberMapInfo(string wrappedObjPropName, string wrappedMemberName) : 
            base(wrappedMemberName, null, wrappedObjPropName)
        {

        }

        public override string WrapperAssignPropName => RoslynAnalysisAndGenerationUtils.THIS;

        public override ClassMemberType TheMemberType => ClassMemberType.Property;
    }
}
