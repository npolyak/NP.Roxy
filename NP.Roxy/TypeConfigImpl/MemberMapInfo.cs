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
using System.Xml.Serialization;

namespace NP.Roxy.TypeConfigImpl
{
    internal abstract class MemberMapInfoBase
    {
        // gets this value only from the parent object
        [XmlIgnore]
        public string WrappedObjPropName { get; private set; }

        [XmlAttribute]
        public string WrapperMemberName { get; set; }
        [XmlIgnore]

        public ISymbol TheWrappedSymbol { get; protected set; }

        public MemberMapInfoBase
        (
            string wrapperMemberName, 
            string wrappedObjPropName)
        {
            this.WrapperMemberName = wrapperMemberName;
            this.WrappedObjPropName = wrappedObjPropName;

            TheWrappedSymbol = null;
        }

        internal virtual bool IsNonPublic => false;

        internal abstract void SetFromContainingType
        (
            Compilation compilation,
            INamedTypeSymbol containingType,
            IEnumerable<INamedTypeSymbol> staticMethodContainers);

        internal abstract void SetAllowNonPublic(bool allowNonPublic);

        internal abstract string GetEventHandlerAssignmentStr(bool addOrRemoveHandler);

        internal abstract void AddAssignWrappedProp(string assignmentStr, RoslynCodeBuilder roslynCodeBuilder);

        internal abstract void AddPropAssignmentStr(bool setOrUnset, RoslynCodeBuilder roslynCodeBuilder);

        internal abstract void AddWrappedPropGetterLine(IPropertySymbol wrapperSymbol, RoslynCodeBuilder roslynCodeBuilder);

        internal abstract void AddWrappedMethodLine(IMethodSymbol wrapperSymbol, RoslynCodeBuilder roslynCodeBuilder);
    }

    internal class ExpressionMemberMapInfo : MemberMapInfoBase
    {
        public ExpressionMemberMapInfo(string wrapperMemberName, string wrappedObjPropName) 
            : 
            base(wrapperMemberName, wrappedObjPropName)
        {
        }

        internal override void AddAssignWrappedProp(string assignmentStr, RoslynCodeBuilder roslynCodeBuilder)
        {
            
        }

        internal override void AddPropAssignmentStr(bool setOrUnset, RoslynCodeBuilder roslynCodeBuilder)
        {
            
        }

        internal override void AddWrappedMethodLine(IMethodSymbol wrapperSymbol, RoslynCodeBuilder roslynCodeBuilder)
        {
            
        }

        internal override void AddWrappedPropGetterLine(IPropertySymbol wrapperSymbol, RoslynCodeBuilder roslynCodeBuilder)
        {
           
        }

        internal override string GetEventHandlerAssignmentStr(bool addOrRemoveHandler)
        {
            throw new NotImplementedException();
        }

        internal override void SetAllowNonPublic(bool allowNonPublic)
        {
           
        }

        internal override void SetFromContainingType(Compilation compilation, INamedTypeSymbol containingType, IEnumerable<INamedTypeSymbol> staticMethodContainers)
        {
            
        }
    }

    internal class MemberMapInfo : MemberMapInfoBase
    {

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
            string wrapperMemberName, 
            string wrappedObjPropName
        ) :
            base(wrapperMemberName, wrappedObjPropName)
        {
            this.WrappedMemberName = wrappedMemberName;
        }

        internal override void SetFromContainingType
        (
            Compilation compilation,
            INamedTypeSymbol containingType, 
            IEnumerable<INamedTypeSymbol> staticMethodContainers)
        {
            if (this.WrappedMemberName == null)
                return;

            this.TheWrappedSymbol = 
                containingType?.GetMemberByName<ISymbol>
                (
                    this.WrappedMemberName, 
                    this.AllowNonPublic
                );

            if (this.TheWrappedSymbol != null)
                return;

            foreach(INamedTypeSymbol typeSymbol in staticMethodContainers)
            {
                this.TheWrappedSymbol =
                    compilation.GetStaticMethodWithFirstArgThatCanAcceptType
                    (
                        typeSymbol,
                        containingType,
                        this.WrappedMemberName,
                        this.AllowNonPublic);

                if (this.TheWrappedSymbol != null)
                    break;
            }
        }

        // if private - WrappedSymbol will be null
        internal override bool IsNonPublic =>
            this.TheWrappedSymbol.DeclaredAccessibility != Accessibility.Public;

        protected string GetWrappedClassMemberFullName(bool hasNullCheck)
        {
            string divider = hasNullCheck ? "?." : ".";

            if (this.TheWrappedSymbol.IsStatic)
            {
                IMethodSymbol methodSymbol =
                    this.TheWrappedSymbol as IMethodSymbol;

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

        IPropertySymbol TheWrappedPropSymbol => TheWrappedSymbol as IPropertySymbol;

        internal override void AddPropAssignmentStr(bool setOrUnset, RoslynCodeBuilder roslynCodeBuilder)
        {
            if (this.TheWrappedPropSymbol?.HasSetter() != true)
                return;

            string assignmentStr =
                setOrUnset ?
                    WrapperMemberName : $"default({(TheWrappedSymbol as IPropertySymbol).Type.AsNamed().GetFullTypeString()})";

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

        internal override void AddWrappedMethodLine(IMethodSymbol wrapperSymbol, RoslynCodeBuilder roslynCodeBuilder)
        {
            IMethodSymbol wrappedMethodSymbol = this.TheWrappedSymbol as IMethodSymbol;

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
                if (this.TheWrappedSymbol.IsStatic)
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
}
