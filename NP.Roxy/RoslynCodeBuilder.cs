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

namespace NP.Roxy
{
    public class RoslynCodeBuilder : CodeBuilder
    {
        public SortedSet<string> AllNamespaces { get; } = 
            new SortedSet<string>();

        public HashSet<IAssemblySymbol> AllAssemblies { get; } =
            new HashSet<IAssemblySymbol>();

        void AddWhereStatements(IEnumerable<ITypeParameterSymbol> typeParameters)
        {
            foreach (ITypeParameterSymbol typeParameterSymbol in typeParameters)
            {
                string whereStatement = typeParameterSymbol.GetWhereStatement();

                if (whereStatement.IsNullOrEmpty())
                    continue;

                AddLine("  " + whereStatement);
            }
        }

        public void AddClass
        (
            string className,
            string superClassName,
            INamedTypeSymbol superClassTypeSymbol,
            params INamedTypeSymbol[] baseTypes
        )
        {
            string extensionString = string.Empty;

            baseTypes = baseTypes.NullToEmpty().ToArray();

            string baseTypesStr = "";

            if (superClassTypeSymbol != null)
            {
                baseTypesStr = superClassTypeSymbol.GetFullTypeString(superClassName).Trim();
            }

            string extensionStr = baseTypes.StrConcat(baseType => baseType.GetFullTypeString()).Trim();

            if ( (!baseTypesStr.IsNullOrEmpty()) && 
                 (!extensionStr.IsNullOrEmpty()) )
            {
                baseTypesStr += ", ";
            }

            baseTypesStr += extensionStr;

            if (!baseTypesStr.IsNullOrEmpty())
            {
                extensionString = $" : {baseTypesStr}";
            }

            AddLine($"public class {className}{extensionString}");

            baseTypes.DoForEach(baseType => AddWhereStatements(baseType.TypeParameters));

            this.Push();
        }


        public void AddMethodOpening
        (
            IMethodSymbol methodSymbol,
            bool shouldOverride = false,
            string newMethodName = null, // if null - same name
            Accessibility newAccessibility = Accessibility.NotApplicable, // means the same
            bool? isVirtual = null,
            bool isAbstract = false
        )
        {
            this.AddLine
            (
                methodSymbol.GetMethodSignature
                (
                    shouldOverride, 
                    newMethodName,
                    newAccessibility,
                    isVirtual,
                    isAbstract
                )
            );

            AddWhereStatements(methodSymbol.TypeParameters);

            this.Push();
        }

        public void AddMethodOpeningFromSymbolOnly(IMethodSymbol methodSymbol)
        {
            this.AddMethodOpening
            (
                methodSymbol,
                methodSymbol.ShouldOverride(),
                methodSymbol.Name,
                methodSymbol.DeclaredAccessibility
            );
        }

        public void AddDefaultConstructorOpening(string className, Accessibility accessibility = Accessibility.Public)
        {
            this.AddLine($"{accessibility.ConvertAccessabilityToString()} {className} ()");
            Push();
        }

        public void AddTypedName
        (
            string name, 
            INamedTypeSymbol fieldType,
            Accessibility accessibility,
            bool shouldOverride = false
        )
        {
            string fullTypeName = fieldType.GetFullTypeString();

            string overrideInsert = shouldOverride ? "override " : "";

            this.AddLine(accessibility.ConvertAccessabilityToString() + " " + overrideInsert + fullTypeName + " " + name);
        }

        public void AddField(string fieldName, INamedTypeSymbol fieldType)
        {
            AddTypedName(fieldName, fieldType, Accessibility.Private);
            this.CloseStatement();
        }

        public void AddPropOpening
        (
            string propName, 
            INamedTypeSymbol propType,
            Accessibility accessibility = Accessibility.Public,
            bool shouldOverride = false)
        {
            AddTypedName(propName, propType, accessibility, shouldOverride);

            this.Push();
        }

        public void OpenPropGetterOrSetter(Accessibility accessibility, bool getterOrSetter)
        {
            string getterAccessibilityStr = 
                accessibility.ConvertAccessabilityToString();

            AddEmptyLine();

            if (!getterAccessibilityStr.IsNullOrEmpty())
            {
                AddText(getterAccessibilityStr + " ");
            }

            string textToAdd = getterOrSetter ? "get" : "set";

            AddText(textToAdd);

            Push();
        }

        public void OpenPropGetter(Accessibility getterAccessibility) =>
            OpenPropGetterOrSetter(getterAccessibility, true);

        public void OpenPropSetter(Accessibility setterAccessibility) =>
            OpenPropGetterOrSetter(setterAccessibility, false);

        public void AddPropAndGetOpening
        (
            string propName,
            INamedTypeSymbol propType,
            Accessibility accessibility = Accessibility.Public
        )
        {
            AddPropOpening(propName, propType, accessibility);

            OpenPropGetter();
        }

        public void AddPropGetter(Accessibility accessibility)
        {
            AddLine(accessibility.GetAccessabilityStr());
            AddText(GETTER);
        }


        public void AddPropSetter(Accessibility accessibility)
        {
            AddLine(accessibility.GetAccessabilityStr());
            AddText(SETTER);
        }

        public void AddAutoGetter(Accessibility accessibility)
        {
            AddPropGetter(accessibility);
            CloseStatement();
        }

        public void AddAutoSetter(Accessibility accessibility)
        {
            AddPropSetter(accessibility);
            CloseStatement();
        }

        public void AddPropOpening(IPropertySymbol propSymbol)
        {
            AddPropOpening
            (
                propSymbol.Name, 
                propSymbol.Type.AsNamed(), 
                propSymbol.DeclaredAccessibility, 
                propSymbol.ShouldOverride());
        }

        public void AddAutoProp
        (
            string propName,
            INamedTypeSymbol propType,
            Accessibility propAccessibility = Accessibility.Public,
            Accessibility getterAccessibility = Accessibility.NotApplicable,
            Accessibility setterAccessibility = Accessibility.NotApplicable,
            bool shouldOverride = false
        )
        {
            getterAccessibility = getterAccessibility.GetPropPartAccessibility(propAccessibility);

            AddPropOpening(propName, propType, propAccessibility, shouldOverride);

            AddAutoGetter(getterAccessibility);

            setterAccessibility = setterAccessibility.GetPropPartAccessibility(propAccessibility);

            AddAutoSetter(setterAccessibility);

            Pop();
        }

        public void AddAutoProp(IPropertySymbol propertySymbol)
        {
            INamedTypeSymbol containingType = propertySymbol.ContainingType;

            if (containingType.TypeKind == TypeKind.Class)
            {
                if ( (propertySymbol.GetMethod == null) || 
                     (propertySymbol.SetMethod == null) )
                { 
                    // leave the property abstract 
                    // since it does not make sense to implement
                    // getter or setter only
                    return;
                }
            }

            bool shouldOverride = propertySymbol.ShouldOverride();

            AddAutoProp
            (
                propertySymbol.Name,
                propertySymbol.Type as INamedTypeSymbol,
                propertySymbol.DeclaredAccessibility,
                propertySymbol.GetMethod?.DeclaredAccessibility ?? Accessibility.NotApplicable,
                propertySymbol.SetMethod?.DeclaredAccessibility ?? Accessibility.NotApplicable,
                shouldOverride
            );
        }

        void AddConcretizationDelegateAttribute(Type concrDelegateType, string memberName)
        {
            string attrName = concrDelegateType.Name.SubstrFromTo(null, "Attribute", false);
            AddLine($"[{attrName}(\"{memberName}\")]");
        }

        public void AddDelegatesCallingProp(IPropertySymbol propertySymbol)
        {
            string propName = propertySymbol.Name;
            PushRegion($"Delegate Calling Property {propName}");
            if (propertySymbol.HasGetter())
            {
                string propGetterDelegateStr = propertySymbol.GetPropGetterDelegate();

                AddConcretizationDelegateAttribute(typeof(PropGetterConcretizationDelegateAttribute), propName);
                AddLine(propGetterDelegateStr);
            }

            if (propertySymbol.HasSetter())
            {
                string propSetterDelegateStr = propertySymbol.GetPropSetterDelegate();

                AddConcretizationDelegateAttribute(typeof(PropSetterConcretizationDelegateAttribute), propName);
                AddLine(propSetterDelegateStr);
            }

            AddEmptyLine();

            AddPropOpening(propertySymbol);

            Accessibility propAccessibility = propertySymbol.DeclaredAccessibility;

            if (propertySymbol.HasGetter())
            {
                string returnType = propertySymbol.Type.AsNamed().GetFullTypeString();
                Accessibility getterAccessibility = 
                    propertySymbol.GetMethod
                                  .DeclaredAccessibility
                                  .GetPropPartAccessibility(propAccessibility);

                OpenPropGetter(getterAccessibility);
                AddLine($"return {propertySymbol.GetPropGetterDelegateName()}()", true);

                Pop();
            }

            if (propertySymbol.HasSetter())
            {
                Accessibility setterAccessibility =
                   propertySymbol.SetMethod
                                 .DeclaredAccessibility
                                 .GetPropPartAccessibility(propAccessibility);

                OpenPropSetter(setterAccessibility);

                AddLine($"{propertySymbol.GetPropSetterDelegateName()}(value)", true);

                Pop();
            }

            Pop();

            PopRegion();
        }

        public void AddDelegateCallingMethod(IMethodSymbol methodSymbol)
        {
            string methodName = methodSymbol.Name;
            PushRegion($"Delegate Calling Method {methodName}");

            AddConcretizationDelegateAttribute(typeof(MethodConcretizationDelegateAttribute), methodName);
            AddLine(methodSymbol.GetMethodDelegate());
            AddEmptyLine();
            AddMethodOpeningFromSymbolOnly(methodSymbol);

            string delegateName = methodSymbol.GetMethodDelegateName();

            bool returns = !methodSymbol.ReturnsVoid;

            string returnInsert = returns ? "return " : "";

            string methodParamsInsert = methodSymbol.Parameters.GetMethodParams(false);

            AddLine($"{returnInsert}{delegateName}{methodParamsInsert}", true);

            Pop();

            PopRegion();
        }

        public void AddNonPublicMethodCall
        (
            IMethodSymbol methodSymbol,
            string objName,
            string methodName,
            IMethodSymbol wrappedMethodSymbol = null // only needed for static calls
        )
        {
            bool isStatic = (wrappedMethodSymbol?.IsStatic == true);

            string returnTypeConversionInsert = "";
            if (!methodSymbol.ReturnsVoid)
            {
                INamedTypeSymbol returnType = methodSymbol.ReturnType as INamedTypeSymbol;
                returnTypeConversionInsert = $"({returnType.GetFullTypeString()})";
            }

            string paramsStr = methodSymbol.Parameters.GetMethodParams(false, -1, false).Trim();


            if (!paramsStr.IsNullOrEmpty())
            {
                paramsStr = ", " + paramsStr;
            }

            string prefix;
            if (isStatic)
            {
                paramsStr = $", {objName}{paramsStr}";
                prefix = $"typeof({wrappedMethodSymbol.ContainingType.GetFullTypeString()})";
            }
            else
            {
                prefix = objName;
            }

            this.AddText($"{returnTypeConversionInsert} {prefix}.CallMethod(\"{methodName}\", true, {isStatic.ToStr().ToLower()}{paramsStr})");
            this.CloseStatement();
        }

        internal void AddStaticMethodCall
        (
            IMethodSymbol methodSymbol, 
            string wrappedObjPropName, 
            string methodFullName)
        {
            this.AddText(methodFullName);
            this.AddText(methodSymbol.GetTypeArgsStr(methodSymbol.TypeArguments));

            string paramsWithoutParentheses = 
                methodSymbol.Parameters.GetMethodParams(false, -1, false);

            string allParams = paramsWithoutParentheses.Trim();

            if (!allParams.IsNullOrEmpty())
            {
                allParams = ", " + allParams;
            }

            if (wrappedObjPropName != null)
            {
                allParams = $"{wrappedObjPropName}{allParams}";
            }

            this.AddText($"({allParams})");

            this.CloseStatement();
        }


        public void AddMethodCall
        (
            IMethodSymbol methodSymbol, 
            string methodFullName
        )
        {
            this.AddText(methodFullName);
            this.AddText(methodSymbol.GetTypeArgsStr(methodSymbol.TypeArguments));
            this.AddText(methodSymbol.Parameters.GetMethodParams(false));

            this.CloseStatement();
        }

        public void AddReturnVar(string varToReturn)
        {
            AddLine($"return {varToReturn}", true); 
        }

        public void AddPropEqualityCheck(string varToCheck)
        {
            AddLine($"if ({varToCheck} == value)");
            Push();
            AddLine("return", true);
            Pop();
        }

        public void AddAssignmentLine(string varToAssign, string assignedValue)
        {
            AddLine($"{varToAssign} = {assignedValue}", true);
        }

        public void AddSettingValue(string varToSet)
        {
            AddAssignmentLine(varToSet, "value");
        }

        public void AddPropGetter
        (
            string fullMemberName,
            INamedTypeSymbol propType,
            Accessibility getterAccessibility = Accessibility.NotApplicable
        )
        {
            OpenPropGetter(getterAccessibility);

            AddReturnVar(fullMemberName);

            Pop();
        }

        public void AddPropSetter
        (
            string fullMemberName,
            INamedTypeSymbol propType,
            Accessibility setterAccessibility = Accessibility.NotApplicable,
            string addBeforeSetter = null,
            string addAfterSetter = null
        )
        {
            OpenPropSetter(setterAccessibility);

            AddPropEqualityCheck(fullMemberName);

            if (addBeforeSetter != null)
            {
                AddLines(addBeforeSetter);
                AddEmptyLine();
            }

            AddSettingValue(fullMemberName);

            if (addAfterSetter != null)
            {
                AddEmptyLine();
                AddLines(addAfterSetter);
            }

            Pop();
        }

        public void AddPropMemberWrapper
        (
            string fullMemberName,
            string propName,
            INamedTypeSymbol propType,
            Accessibility accessibility = Accessibility.Public,
            string addBeforeSetter = null,
            string addAfterSetter = null,
            Accessibility setterAccessibility = Accessibility.NotApplicable
        )
        {
            AddPropOpening(propName, propType, accessibility);

            AddPropGetter(fullMemberName, propType, Accessibility.NotApplicable);

            setterAccessibility = setterAccessibility.GetPropPartAccessibility(accessibility);

            AddPropSetter
            (
                fullMemberName, 
                propType,
                setterAccessibility,
                addBeforeSetter,
                addAfterSetter
            );

            Pop();
        }

        public void AddPropWithBackingStore
        (
            string propName,
            string backingStoreFieldName,
            INamedTypeSymbol propType,
            Accessibility accessibility = Accessibility.Public,
            string addBeforeSetter = null,
            string addAfterSetter = null,
            Accessibility setterAccessibility = Accessibility.NotApplicable
        )
        {
            AddField(backingStoreFieldName, propType);

            AddPropMemberWrapper
            (
                backingStoreFieldName, 
                propName, 
                propType, 
                accessibility,
                addBeforeSetter,
                addAfterSetter,
                setterAccessibility
            );
        }

        public void AddPropWithBackingStore
        (
            string propName,
            INamedTypeSymbol propType,
            Accessibility accessibility = Accessibility.Public
        )
        {
            string fieldName = propName.PropToFieldName();

            AddPropWithBackingStore(propName, fieldName, propType, accessibility);
        }

        public void AddMethodWrapper
        (
            string methodOriginName,
            IMethodSymbol methodSymbol,
            string newName = null,
            bool shouldOverride = false,
            Accessibility newAccessibility = Accessibility.NotApplicable,
            bool? isVirtualFlag = null,
            bool isAbstract = false,
            bool useNullProtection = false,
            int indexParamToReplaceByThis = -1
        )
        {
            AddMethodOpening
            (
                methodSymbol, 
                shouldOverride, 
                newName, 
                newAccessibility, 
                isVirtualFlag,
                isAbstract
            );

            string returnInsert = "";
            if (!methodSymbol.ReturnsVoid)
            {
                returnInsert = "return ";
            }

            string divider = useNullProtection ? "?." : ".";

            AddLine(returnInsert + methodOriginName + divider + methodSymbol.Name);

            AddText(methodSymbol.GetTypeArgsStr(methodSymbol.TypeArguments));

            AddText(methodSymbol.Parameters.GetMethodParams(false, indexParamToReplaceByThis));

            CloseStatement();

            Pop();
        }

        public void AddEventInvokationWrapper
        (
            string eventName, 
            IEventSymbol eventSymbol,
            int idxParamToReplaceByThis = -1
        )
        {
            IMethodSymbol delegateInvokeMethodSymbol =
                (eventSymbol.Type as INamedTypeSymbol).DelegateInvokeMethod;

            this.AddMethodWrapper
            (
                eventName,
                delegateInvokeMethodSymbol,
                eventName.GetEventInvokationWrapperName(),
                false,
                Accessibility.Private,
                false,
                false,
                true,
                idxParamToReplaceByThis
            );
        }

        public void AddEventDefinitionAndEventInvocationMethod
        (
            IEventSymbol eventSymbol,
            int idxParamToReplaceByThis = -1,
            string newName = null
        )
        {
            Accessibility accessibility = eventSymbol.DeclaredAccessibility;

            string eventName = 
                newName ?? eventSymbol.Name;

            string result =
                accessibility.ConvertAccessabilityToString() + 
                " event " + 
                (eventSymbol.Type as INamedTypeSymbol).GetFullTypeString() + 
                " " + 
                eventName;

            AddLine(result, true);

            AddEmptyLine();

            this.AddEventInvokationWrapper(eventName, eventSymbol, idxParamToReplaceByThis);

            AddEmptyLine();
        }

        public override string ToString()
        {
            string result = "";

            foreach(string namespaceName in this.AllNamespaces)
            {
                result += namespaceName.GetUsingLine();
            }

            result += "\n" + base.ToString();

            return result;
        }
    }
}
