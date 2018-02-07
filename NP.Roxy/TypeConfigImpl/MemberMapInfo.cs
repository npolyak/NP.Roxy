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
    internal class MemberMapInfo
    {
        // gets this value only from the parent object
        [XmlIgnore]
        public string WrappedObjPropName { get; private set; }

        [XmlAttribute]
        public string WrappedMemberName { get; set; }

        [XmlAttribute]
        public string WrapperMemberName { get; set; }

        [XmlIgnore]
        public ISymbol TheWrappedSymbol { get; private set; }

        [XmlAttribute]
        public bool AllowNonPublic { get; set; } = false;

        public ClassMemberType TheMemberType =>
            TheWrappedSymbol.GetMemberType();

        internal void SetWrappedObjPropName(string wrappedObjPropName)
        {
            WrappedObjPropName = wrappedObjPropName;
        }

        internal void SetFromContainingType
        (
            Compilation compilation,
            INamedTypeSymbol containingType, 
            IEnumerable<INamedTypeSymbol> staticMethodContainers)
        {
            if (this.WrappedMemberName == null)
                return;

            this.TheWrappedSymbol = 
                containingType?.GetMemberByName<ISymbol>(this.WrappedMemberName, this.AllowNonPublic);

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

            ///if TheWrappedSymbol is null, we simply remove the map. 

            //if (this.TheWrappedSymbol == null)
            //{
            //    throw new Exception($"Error: there is no member '{this.WrappedMemberName}' within class '{containingType.GetFullTypeString()}' and no such static member within Static Method Containers");
            //}
        }

        // if private - WrappedSymbol will be null
        public bool IsNonPublic =>
            this.TheWrappedSymbol.DeclaredAccessibility != Accessibility.Public;

        public MemberMapInfo()
        {

        }

        public MemberMapInfo(string wrappedMemberName, string wrapperMemberName = null)
        {
            this.WrappedMemberName = wrappedMemberName;

            if (wrapperMemberName == null)
                wrapperMemberName = wrappedMemberName;

            this.WrapperMemberName = wrapperMemberName;
        }


        public string GetWrappedClassMemberFullName(bool hasNullCheck)
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

        public string WrappedClassMemberFullName =>
            GetWrappedClassMemberFullName(false);

        public string GetEventHandlerAssignmentStr(bool addOrRemoveHandler)
        {
            string addOrRemoveStr = addOrRemoveHandler ? "+" : "-";

            string result =
                $"{WrappedClassMemberFullName} {addOrRemoveStr}= {WrapperMemberName.GetEventInvocationWrapperName()}";

            return result;
        }

        public void AddAssignWrappedProp(string assignmentStr, RoslynCodeBuilder roslynCodeBuilder)
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

        public void AddPropAssignmentStr(bool setOrUnset, RoslynCodeBuilder roslynCodeBuilder)
        {
            if (this.TheWrappedPropSymbol?.HasSetter() != true)
                return;

            string assignmentStr =
                setOrUnset ?
                    WrapperMemberName : $"default({(TheWrappedSymbol as IPropertySymbol).Type.AsNamed().GetFullTypeString()})";

            AddAssignWrappedProp(assignmentStr, roslynCodeBuilder);
        }
    }
}
