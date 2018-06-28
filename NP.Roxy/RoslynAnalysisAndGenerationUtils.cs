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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using NP.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NP.Roxy
{
    public interface NoType { }

    public static class RoslynAnalysisAndGenerationUtils
    {
        public const string THIS = "this";

        public static bool IsOverridable(this ISymbol symbol)
        {
            return symbol.IsAbstract || symbol.IsVirtual;
        }

        public static Type NoTypeType =>
            typeof(NoType);

        public static Type GetRealRoxyType(this Type type)
        {
            return (type == null) ? NoTypeType : type;
        }

        public static Type GetRealRoxyType<T>() =>
            GetRealRoxyType(typeof(T));

        public static INamedTypeSymbol GetNullForNoType(this INamedTypeSymbol typeSymbol, Compilation compilation) =>
            (typeSymbol?.Matches(NoTypeType, compilation) != false) ? null : typeSymbol;

        public static INamedTypeSymbol GetNoTypeForNull(this INamedTypeSymbol typeSymbol, Compilation compilation)
        {
            return typeSymbol ?? NoTypeType.GetTypeSymbol(compilation);
        }

        public static bool IsClass(this INamedTypeSymbol typeSymbol)
        {
            return typeSymbol.TypeKind == TypeKind.Class;
        }

        public static INamedTypeSymbol GetRealRoxyTypeSymbol
        (
            this Type type,
            Compilation compilation
        ) =>
            GetRealRoxyType(type)?.GetTypeSymbol(compilation);

        public static INamedTypeSymbol GetRealRoxyTypeSymbol<T>(Compilation compilation) =>
            GetRealRoxyType<T>()?.GetTypeSymbol(compilation);

        public static bool IsNoTypeOrNull(this INamedTypeSymbol namedTypeSymbol)
        {
            if (namedTypeSymbol == null)
                return true;

            string typeStr = namedTypeSymbol.GetUniqueTypeStr();

            if (typeStr == NoTypeType.GetFullTypeStr())
                return true;

            return false;
        }

        public static INamedTypeSymbol NoTypeToNull(this INamedTypeSymbol namedTypeSymbol)
        {
            if (namedTypeSymbol.IsNoTypeOrNull())
                return null;

            return namedTypeSymbol;
        }

        public static ICollection<string> TheNamespaces { get; set; }

        public static ICollection<IAssemblySymbol> TheAssemblies { get; set; }

        public static string GetFullNamespace(this ISymbol symbol)
        {
            if ((symbol.ContainingNamespace == null) ||
                 (string.IsNullOrEmpty(symbol.ContainingNamespace.Name)))
            {
                return null;
            }

            // get the rest of the full namespace string
            string restOfResult = symbol.ContainingNamespace.GetFullNamespace();

            string result = symbol.ContainingNamespace.Name;

            if (restOfResult != null)
                // if restOfResult is not null, append it after a period
                result = restOfResult + '.' + result;

            return result;
        }

        public static string GetTypeArgsStr
        (
            this ISymbol symbol,
            IEnumerable<ITypeSymbol> typeArgs
        )
        {
            string result = "";

            if (typeArgs.Count() > 0)
            {
                result += "<";

                bool isFirstIteration = true;
                foreach (ITypeSymbol typeArg in typeArgs)
                {
                    // insert comma if not first iteration
                    if (isFirstIteration)
                    {
                        isFirstIteration = false;
                    }
                    else
                    {
                        result += ", ";
                    }

                    ITypeParameterSymbol typeParameterSymbol =
                        typeArg as ITypeParameterSymbol;

                    string strToAdd = null;
                    if (typeParameterSymbol != null)
                    {
                        // this is a generic argument
                        strToAdd = typeParameterSymbol.Name;
                    }
                    else
                    {
                        // this is a generic argument value. 
                        INamedTypeSymbol namedTypeSymbol =
                            typeArg as INamedTypeSymbol;

                        strToAdd = namedTypeSymbol.GetFullTypeString();
                    }

                    result += strToAdd;
                }

                result += ">";
            }

            return result;
        }

        public static Accessibility GetAccessibility(this Accessibility accessibility, ISymbol symbol)
        {
            if (accessibility == Accessibility.NotApplicable)
                return symbol.DeclaredAccessibility;

            return accessibility;
        }

        public static Accessibility GetPropPartAccessibility
        (
            this Accessibility propPartAccessibility,
            Accessibility propAccessibility)
        {
            if (propPartAccessibility == propAccessibility)
                return Accessibility.NotApplicable;

            return propPartAccessibility;
        }


        public static void InitTypeNamespace<Type>()
        {
            TheNamespaces.Add(typeof(Type).Namespace);
        }

        public static string UnwrapNestedType(this INamedTypeSymbol typeSymbol)
        {
            string result = "";
            if (typeSymbol.ContainingType != null)
            {
                result = typeSymbol.ContainingType.UnwrapNestedType();
            }

            result += "_" + typeSymbol.GetTypeName();

            return result;
        }

        public static string GetFullTypeString(this INamedTypeSymbol type, string typeName = null)
        {
            if (typeName == null)
                typeName = type.Name;

            INamedTypeSymbol containingTypeSymbol = type.ContainingType;

            if (containingTypeSymbol != null)
            {
                typeName = containingTypeSymbol.GetFullTypeString() + "." + typeName;
            }

            string result =
                typeName + type.GetTypeArgsStr(type.TypeArguments);

            if (TheNamespaces != null)
            {
                string fullNamespace = type.GetFullNamespace();

                if (!fullNamespace.IsNullOrEmpty())
                {
                    TheNamespaces.Add(fullNamespace);
                }
            }

            if (TheAssemblies != null)
            {
                TheAssemblies.Add(type.ContainingAssembly);
            }

            result = result.Box();

            return result;
        }

        public static string AddPrefixThe(this string str)
        {
            return $"The{str}";
        }

        public static string ConvertAccessabilityToString(this Accessibility accessability)
        {
            switch (accessability)
            {
                case Accessibility.Internal:
                    return "internal";
                case Accessibility.Private:
                    return "private";
                case Accessibility.Protected:
                    return "protected";
                case Accessibility.Public:
                    return "public";
                case Accessibility.ProtectedAndInternal:
                    return "protected internal";
                case Accessibility.NotApplicable:
                    return string.Empty;
                default:
                    return "private";
            }
        }

        public static string GetAccessabilityStr(this Accessibility accessibility)
        {
            string accessibilityStr = accessibility.ConvertAccessabilityToString();

            if (!accessibilityStr.IsNullOrEmpty())
                accessibilityStr += " ";

            return accessibilityStr;
        }

        public static string GetWhereStatement(this ITypeParameterSymbol typeParameterSymbol)
        {
            string result = "where " + typeParameterSymbol.Name + " : ";

            string constraints = "";

            bool isFirstConstraint = true;

            if (typeParameterSymbol.HasReferenceTypeConstraint)
            {
                constraints += "class";

                isFirstConstraint = false;
            }

            if (typeParameterSymbol.HasValueTypeConstraint)
            {
                constraints += "struct";

                isFirstConstraint = false;
            }

            foreach (INamedTypeSymbol contstraintType in typeParameterSymbol.ConstraintTypes)
            {
                // if not first constraint prepend with comma
                if (!isFirstConstraint)
                {
                    constraints += ", ";
                }
                else
                {
                    isFirstConstraint = false;
                }

                constraints += contstraintType.GetFullTypeString();
            }

            if (typeParameterSymbol.HasConstructorConstraint)
            {
                if (!isFirstConstraint)
                {
                    constraints += ", ";
                }
                constraints += "new()";
            }

            if (string.IsNullOrEmpty(constraints))
                return null;

            result += constraints;

            return result;
        }

        public static string GetMemberDeclarationCode
        (
            string memberName,
            INamedTypeSymbol memberType, // or return type for a method
            Accessibility accessibility,
            bool isStatic = false,
            bool shouldOverride = false,
            bool isAbstract = false,
            bool isVirtual = false,
            bool isAsync = false
        )
        {
            string result = accessibility.ConvertAccessabilityToString();

            if (shouldOverride)
                isVirtual = false;

            if (isAsync)
                result += " async";

            if (isAbstract)
                result += " abstract";

            if (isVirtual)
            {
                result += " virtual";
            }

            if (isStatic)
            {
                result += " static";
            }

            //if (methodSymbol.IsOverride)
            if (shouldOverride)
            {
                result += " override";
            }

            result += " " + memberType.GetFullTypeString() + " " + memberName;

            return result;
        }

        public static List<TSymbol>
            GetMembersByName<TSymbol>
            (
                this ITypeSymbol typeSymbol,
                string name,
                bool allowNonPublic = true
            )
            where TSymbol : class, ISymbol
        {
            IEnumerable<TSymbol> allResults =
                typeSymbol.GetAllMembers().Where(symb => (symb.Name == name) && (symb is TSymbol)).Cast<TSymbol>();

            if (!allowNonPublic)
            {
                allResults = allResults.Where(symb => symb.DeclaredAccessibility == Accessibility.Public);
            }

            return allResults.ToList();
        }


        public static void ThrowOverloadedException
        (
            string className,
            string memberName
        )
        {
            throw new Exception
            (
                $"Error: Member {className}.{memberName} is Overloaded. Overloaded members are not allowed to be wrapped at this point. Perhaps this functionality will be implemented in the future."
            );
        }

        public static ReturnType ThrowOverloadedException<ReturnType>
        (
            string className,
            string memberName
        )
        {
            ThrowOverloadedException(className, memberName);

            return default(ReturnType);
        }

        public static void ThrowNoSuchMemberException
        (
            string className,
            string memberName
        )
        {
            throw new Exception
            (
                $"Error: Class '{className}' does not contain member named '{memberName}'"
            );
        }

        public static ReturnType ThrowNoSuchMemberException<ReturnType>
        (
            string className,
            string memberName
        )
        {
            ThrowNoSuchMemberException(className, memberName);

            return default(ReturnType);
        }

        public static TSymbol
            GetMemberByNameWithOverloadingTesting<TSymbol>
            (
                this ITypeSymbol typeSymbol,
                string memberName,
                bool allowNonPublic = true
            )
            where TSymbol : class, ISymbol
        {
            List<TSymbol> publicSymbols =
                typeSymbol.GetMembersByName<TSymbol>(memberName, allowNonPublic);

            if (publicSymbols.Count > 1)
            {
                return ThrowOverloadedException<TSymbol>(typeSymbol.Name, memberName);
            }

            return publicSymbols.FirstOrDefault();
        }

        public static string GetMethodParams
        (
            this IEnumerable<IParameterSymbol> methodParams,
            bool includeTypes = true,
            int indexParamToReplaceByThis = -1,
            bool includeParenthesis = true
        )
        {
            string result = includeParenthesis ? "(" : "";
            int paramIdx = -1;

            foreach (IParameterSymbol parameter in methodParams)
            {
                paramIdx++;

                if (paramIdx != 0) // not the first parameter
                {
                    result += ", ";
                }

                if (parameter.RefKind == RefKind.Out)
                {
                    result += "out ";
                }
                else if (parameter.RefKind == RefKind.Ref)
                {
                    result += "ref ";
                }

                if (includeTypes)
                {
                    string parameterTypeString = null;
                    if (parameter.IsParams) // variable num arguments case
                    {
                        result += "params ";

                        INamedTypeSymbol elementType =
                            (parameter.Type as IArrayTypeSymbol).ElementType as INamedTypeSymbol;

                        result += elementType.GetFullTypeString() + "[]";
                    }
                    else
                    {
                        parameterTypeString =
                            (parameter.Type as INamedTypeSymbol).GetFullTypeString();
                    }

                    result += parameterTypeString + " ";

                }

                if (paramIdx == indexParamToReplaceByThis)
                {
                    result += THIS;
                }
                else
                {
                    result += parameter.Name;
                }

                if (includeTypes)
                {
                    if (parameter.HasExplicitDefaultValue)
                    {
                        result += " = " + parameter.ExplicitDefaultValue.ToString();
                    }
                }


                if ((indexParamToReplaceByThis >= 0) && (parameter.RefKind != RefKind.None))
                {
                    throw new Exception($"Error: cannot have a '{parameter.RefKind}' parameter replaced by 'this' : '{result}'");
                }
            }

            result += includeParenthesis ? ")" : "";


            if ((indexParamToReplaceByThis >= 0) && includeTypes)
            {
                throw new Exception($"Error: cannot have a parameter replaced by 'this' within function declaration - only within invokation: '{result}'");
            }

            return result;
        }

        public static string GetMethodSignature
        (
            this IMethodSymbol methodSymbol,
            bool shouldOverride = false,
            string newMethodName = null, // if null - same name
            Accessibility newAccessibility = Accessibility.NotApplicable, // means the same
            bool? isVirtualFlag = false,
            bool isAbstract = false
        )
        {
            Accessibility accessibility =
                newAccessibility.GetAccessibility(methodSymbol);

            string methodName = newMethodName;

            if (methodName == null)
            {
                methodName = methodSymbol.Name;
            }

            bool isVirtual = isVirtualFlag ?? methodSymbol.IsVirtual;

            string result =
                GetMemberDeclarationCode
                (
                    methodName,
                    methodSymbol.ReturnType as INamedTypeSymbol,
                    accessibility,
                    methodSymbol.IsStatic,
                    shouldOverride,
                    isAbstract,
                    isVirtual,
                    methodSymbol.IsAsync
                );

            result +=
                methodSymbol.GetTypeArgsStr(methodSymbol.TypeArguments);

            result += methodSymbol.Parameters.GetMethodParams(true);

            return result;
        }

        public static object GetAttrValueByArgName(this AttributeData attributeData, string argName)
        {
            return attributeData.NamedArguments.FirstOrDefault(kvp => kvp.Key == argName).Value;
        }

        public static object
            GetAttributeConstructorValueByParameterName(this AttributeData attributeData, string argName)
        {

            // Get the parameter
            IParameterSymbol parameterSymbol = attributeData.AttributeConstructor
                .Parameters
                .Where((constructorParam) => constructorParam.Name == argName).FirstOrDefault();

            // get the index of the parameter
            int parameterIdx = attributeData.AttributeConstructor.Parameters.IndexOf(parameterSymbol);

            // get the construct argument corresponding to this parameter
            TypedConstant constructorArg = attributeData.ConstructorArguments[parameterIdx];

            // the case of variable number of arguments
            if (constructorArg.Kind == TypedConstantKind.Array)
            {
                List<object> result = new List<object>();

                foreach (TypedConstant typedConst in constructorArg.Values)
                {
                    result.Add(typedConst.Value);
                }

                return result;
            }

            // return the value passed to the attribute
            return constructorArg.Value;
        }

        public static bool HasGetter(this IPropertySymbol propertySymbol)
        {
            return propertySymbol.GetMethod != null;
        }

        public static bool HasSetter(this IPropertySymbol propertySymbol)
        {
            return propertySymbol.SetMethod != null;
        }

        public static INamedTypeSymbol AsNamed(this ITypeSymbol typeSymbol)
        {
            return typeSymbol as INamedTypeSymbol;
        }

        public static string GetEventInvocationWrapperName(this string eventName)
            => $"__Invoke_{eventName}";

        const string FUNC = "Func";
        const string ACTION = "Action";

        private static string GetDelegateKind(this bool returns)
        {
            return returns ? FUNC : ACTION;
        }

        private static string GetPropDelegate(this INamedTypeSymbol propType, string delegateName, bool isGetter)
        {
            string delegateKind = isGetter.GetDelegateKind();

            return $"internal {delegateKind}<{propType.GetFullTypeString()}> {delegateName} {{ get; set; }}";
        }

        public static string GetPropGetterDelegate(this INamedTypeSymbol propType, string delegateName)
        {
            return propType.GetPropDelegate(delegateName, true);
        }

        public static string GetPropSetterDelegate(this INamedTypeSymbol propType, string delegateName)
        {
            return propType.GetPropDelegate(delegateName, false);
        }

        public const string DelegatePostFix = "_Delegate";


        public static string GetDelegateName(this string name)
        {
            return name + DelegatePostFix;
        }

        public static string GetPropGetterDelegateName(this IPropertySymbol propertySymbol)
        {
            return (propertySymbol.Name + "_Get").GetDelegateName();
        }

        public static string GetPropSetterDelegateName(this IPropertySymbol propertySymbol)
        {
            return (propertySymbol.Name + "_Set").GetDelegateName();
        }

        public static string GetPropGetterDelegate(this IPropertySymbol propertySymbol)
        {
            if (propertySymbol.GetMethod == null)
                return null;

            return (propertySymbol.Type as INamedTypeSymbol).GetPropGetterDelegate(propertySymbol.GetPropGetterDelegateName());
        }

        public static string GetPropGetterDelegateAssigment(string propName) =>
            $"() => this.{propName}";

        public static string GetPropSetterDelegateAssigment(string propName) =>
             $"(val) => this.{propName} = val";


        public static string GetPropSetterDelegate(this IPropertySymbol propertySymbol)
        {
            if (propertySymbol.SetMethod == null)
                return null;

            return (propertySymbol.Type as INamedTypeSymbol).GetPropSetterDelegate(propertySymbol.GetPropSetterDelegateName());
        }

        public static string GetMethodDelegateName(this IMethodSymbol methodSymbol)
        {
            string result = methodSymbol.Name + DelegatePostFix;

            return result;
        }

        public static string GetMethodDelegate(this IMethodSymbol methodSymbol)
        {
            string delegateName = methodSymbol.GetMethodDelegateName();

            bool returns = !methodSymbol.ReturnsVoid;

            INamedTypeSymbol returnType = null;

            if (returns)
                returnType = methodSymbol.ReturnType.AsNamed();

            string delegateKind = returns.GetDelegateKind();

            string argList = "";

            if ((methodSymbol.Parameters.Count() > 0) || returns)
            {
                List<INamedTypeSymbol> allTypes =
                    methodSymbol.Parameters
                                .Select(param => param.Type.AsNamed()).ToList();

                allTypes.AddRange(returnType.ToCollection());

                argList = "<" + allTypes
                                .StrConcat((type) => type.GetFullTypeString()) + ">";
            }

            string result = $"public {delegateKind}{argList} {delegateName} {{ get; set; }}";

            return result;
        }

        public static IEnumerable<Assembly> GetAllReferencedAssemblies(this IEnumerable<Type> types)
        {
            return types.Where(type => type != null)
                        .Distinct()
                        .Select(type => type.Assembly)
                        .Distinct()
                        .SelectMany(assembly => assembly.GetAssemblyAndReferencedAssemblies())
                        .Distinct().ToList();
        }

        public static MetadataReference ToRef(this Assembly assembly)
        {
            return MetadataReference.CreateFromFile(assembly.Location);
        }

        public static IEnumerable<IAssemblySymbol> GetReferencedAssemblies(this IAssemblySymbol assemblySymbol)
        {
            return assemblySymbol.Modules.SelectMany(module => module.ReferencedAssemblySymbols).ToList();
        }

        public static IEnumerable<IAssemblySymbol> GetAssemblyAndReferencedAssemblies(this IAssemblySymbol assemblySymbol)
        {
            return assemblySymbol.ToCollection().Union(assemblySymbol.GetReferencedAssemblies()).ToList();
        }

        public static IEnumerable<IAssemblySymbol> GetAllReferencedAssemblies(this ITypeSymbol type)
        {
            return type.ContainingAssembly.GetAssemblyAndReferencedAssemblies();
        }

        public static IEnumerable<IAssemblySymbol> GetAllReferencedAssemblies(this IEnumerable<ITypeSymbol> types)
        {
            return types.SelectMany(type => type.GetAllReferencedAssemblies()).Distinct().ToList();
        }

        public static bool Matches(this AssemblyIdentity assemblyIdentity, AssemblyName assemblyName)
        {
            return (assemblyIdentity.Name == assemblyName.Name) &&
                   (assemblyIdentity.Version == assemblyName.Version);
        }

        public static MetadataReference ToRef(this IAssemblySymbol assembly)
        {
            AssemblyMetadata metaData = assembly.GetMetadata();

            if (metaData != null)
                return metaData.GetReference();

            AssemblyIdentity id = assembly.Identity;

            Assembly resultAssembly =
                 AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(assmbly => id.Matches(assmbly.GetName()));

            return resultAssembly?.ToRef();
        }

        public static string MemberConcat(params string[] strs) =>
            strs.StrConcat(null, StrUtils.PERIOD);

        public static INamedTypeSymbol GetGenericTypeSymbol(this Type type, Compilation compilation)
        {
            if (type == null)
                return null;

            string basicType = type.GetFullGenericTypeName();//type.GetFullTypeStr();

            INamedTypeSymbol namedTypeSymbol = compilation.GetTypeByMetadataName(basicType);

            return namedTypeSymbol;
        }

        public static INamedTypeSymbol GetUnboundGenericTypeSymbol(this Type type, Compilation compilation)
        {
            INamedTypeSymbol genericSymbol = type.GetGenericTypeSymbol(compilation);

            INamedTypeSymbol result = genericSymbol.IsGenericType ? genericSymbol.ConstructUnboundGenericType() : genericSymbol;

            return result;
        }

        public static INamedTypeSymbol GetTypeSymbol(this Type type, Compilation compilation)
        {
            if (type == null)
                return null;

            INamedTypeSymbol namedTypeSymbol = type.GetGenericTypeSymbol(compilation);

            if (namedTypeSymbol == null)
                return null;

            IEnumerable<Type> genericArgs = type.GetGenericArguments();

            if (genericArgs.IsNullOrEmpty())
                return namedTypeSymbol;

            INamedTypeSymbol[] genericArgsTypeSymbols =
                genericArgs.Select(genArg => genArg.GetTypeSymbol(compilation)).ToArray();

            namedTypeSymbol = namedTypeSymbol.Construct(genericArgsTypeSymbols);

            return namedTypeSymbol;
        }

        public static ClassMemberType GetMemberType(this ISymbol symbol)
        {
            if (symbol is IEventSymbol)
                return ClassMemberType.Event;
            if (symbol is IPropertySymbol)
                return ClassMemberType.Property;
            if (symbol is IMethodSymbol)
                return ClassMemberType.Method;

            return ClassMemberType.None;
        }

        public static bool ShouldOverride(this ISymbol symbol)
        {
            return (symbol.IsAbstract && (symbol.ContainingType.TypeKind != TypeKind.Interface)) ||
                    symbol.IsVirtual;
        }

        public static IEnumerable<INamedTypeSymbol> GetSelfAndAllBaseTypes(this INamedTypeSymbol typeSymbol)
        {
            yield return typeSymbol;

            if (typeSymbol.BaseType == null)
                yield break;

            foreach (INamedTypeSymbol baseTypeSymbol in typeSymbol.BaseType.GetSelfAndAllBaseTypes())
            {
                yield return baseTypeSymbol;
            }
        }

        public static IEnumerable<INamedTypeSymbol> GetBaseTypeAndInterfaces(this INamedTypeSymbol typeSymbol)
        {
            if (typeSymbol == null)
                yield break;

            if (typeSymbol.BaseType != null)
                yield return typeSymbol.BaseType;

            if (typeSymbol.Interfaces == null)
                yield break;

            foreach(INamedTypeSymbol baseInterface in typeSymbol.Interfaces)
            {
                yield return baseInterface;
            }
        }

        public static IEnumerable<INamedTypeSymbol> GetSelfAndAllBaseTypesAndInterfaces
        (
            this INamedTypeSymbol typeSymbol
        )
        {
            return typeSymbol.GetSelfAndAllBaseTypes().Union(typeSymbol.AllInterfaces.NullToEmpty());
        }

        public static bool IsSelfOrSuperClass(this INamedTypeSymbol typeSymbol, string className)
        {
            return typeSymbol.GetSelfAndAllBaseTypes().FirstOrDefault(t => t.Name == className) != null;
        }

        public static bool IsSelfOrSuperType(this INamedTypeSymbol typeSymbol, INamedTypeSymbol superTypeSymbol)
        {
            return 
                typeSymbol.GetSelfAndAllBaseTypesAndInterfaces().FirstOrDefault(t => t.TypesStrictlyMatch(superTypeSymbol)) != null;
        }

        public static IEnumerable<ITypeSymbol> SelfAndSuperInterfaces(this ITypeSymbol typeSymbol)
        {
            yield return typeSymbol;

            if (typeSymbol.Interfaces == null)
                yield break;

            foreach (ITypeSymbol interfaceTypeSymbol in typeSymbol.Interfaces)
            {
                foreach (ITypeSymbol tS in interfaceTypeSymbol.SelfAndSuperInterfaces())
                    yield return tS;
            }
        }

        public static IEnumerable<AttributeData> GetAttrSymbols(this ISymbol symbol, Type attrType)
        {
            return
                symbol.GetAttributes()
                      .Where(attrData => attrData.AttributeClass.IsSelfOrSuperClass(attrType.Name));
        }

        public static AttributeData GetAttrSymbol(this ISymbol symbol, Type attrType)
        {
            return
                symbol.GetAttrSymbols(attrType).FirstOrDefault();
        }

        public static AttributeData GetTypeAttrSymbol(this ITypeSymbol typeSymbol, Type attrType)
        {
            return
                typeSymbol.SelfAndSuperInterfaces().SelectMany(typeSymb => typeSymb.GetAttrSymbols(attrType)).FirstOrDefault();
        }

        public static bool MatchesAttrType(this AttributeData attributeData, Type attrType)
        {
            return attributeData.AttributeClass.Name == attrType.Name;
        }

        public static TSymbol GetMemberByName<TSymbol>
        (
            this ITypeSymbol typeSymbol,
            string name,
            bool allowNonPublic = true
        )
            where TSymbol : class, ISymbol
        {

            IEnumerable<TSymbol> results = typeSymbol.GetMembersByName<TSymbol>(name, allowNonPublic);

            if (results.IsNullOrEmpty())
                return null;

            // prefer non-abstract symbols, but if none - return abstract
            TSymbol result = results.FirstOrDefault(symb => !symb.IsAbstract) ?? results.FirstOrDefault();

            return result;
        }

        private static IEnumerable<ISymbol> GetSelfSymbols(this ITypeSymbol typeSymbol, string name = null)
        {
            IEnumerable<ISymbol> result;
            if (name == null)
            {
                result = typeSymbol.GetMembers();
            }
            else
            {
                result = typeSymbol.GetMembers(name);
            }

            return result.Where(member => (member as IMethodSymbol)?.AssociatedSymbol == null);
        }

        public static IEnumerable<ISymbol> GetAllSuperMembers
        (
            this ITypeSymbol typeSymbol,
            string name = null
        )
        {
            List<ISymbol> result = new List<ISymbol>();

            typeSymbol.Interfaces.DoForEach(intrfc => result.AddRange(intrfc.GetAllMembers(name)));

            if (typeSymbol.BaseType != null)
            {
                result.AddRange(typeSymbol.BaseType.GetAllMembers(name));
            }

            return result;
        }

        public static IEnumerable<ISymbol> EliminateDups(this IEnumerable<ISymbol> symbols)
        {
            var preliminaryResult =
                symbols 
                    .GroupBy(symb => symb, TheSymbolByNameAndSignatureComparer);

            //var findNameResults = preliminaryResult.ToList()[3];

            //var findNameOrdered = findNameResults.OrderBy(symb => symb, TheAbstractionComparer).ToList();

            var result = 
                preliminaryResult
                    .Select(g => g.OrderBy(symb => symb, TheAbstractionComparer).ToList().Last());

            return result;
        }

        public static IEnumerable<ISymbol> GetAllMembers
        (
            this ITypeSymbol typeSymbol,
            string name = null
        )
        {
            List<ISymbol> result = new List<ISymbol>();

            result.AddRange(typeSymbol.GetSelfSymbols(name));

            result.AddRange(typeSymbol.GetAllSuperMembers(name));

            return result;
        }

        public static bool MethodsStrictlyMatch(this IMethodSymbol method1, IMethodSymbol method2)
        {
            if (method1.Name != method2.Name)
                return false;

            IEnumerable<IParameterSymbol> params1 = method1.Parameters;
            IEnumerable<IParameterSymbol> params2 = method2.Parameters;

            if (params1.Count() != params2.Count())
                return false;

            foreach(var pars in params1.Zip(params2, (par1, par2) => new { Par1 = par1, Par2 = par2 }))
            {
                if (!pars.Par1.Type.TypesStrictlyMatch(pars.Par2.Type))
                    return false;
            }

            return true;
        }

        public static IEnumerable<ISymbol> GetAllPublicMembers
        (
            this ITypeSymbol typeSymbol,
            string name = null
        )
        {
            return typeSymbol.GetAllMembers(name).Where(member => member.DeclaredAccessibility == Accessibility.Public);
        }

        public static IEnumerable<ISymbol> GetAllPublicNonAbstractMembers(this INamedTypeSymbol typeSymbol)
        {
            return typeSymbol.GetAllPublicMembers()
                             .Where(member => (!member.IsAbstract));
        }

        public static IEnumerable<TSymbol> GetSymbolsOfType<TSymbol>(this IEnumerable<ISymbol> symbols)
            where TSymbol : class, ISymbol
            => symbols.Where(symb => symb is TSymbol).Cast<TSymbol>();

        public static IEqualityComparer<ISymbol> TheSymbolByNameAndSignatureComparer { get; } =
            new SymbolByNameAndSignatureComparer();

        public static IComparer<ISymbol> TheAbstractionComparer { get; } =
            new AbstractionComparer();

        public static string InterfaceToClassName(this string typeName)
        {
            if (typeName.StartsWith("I"))
            {
                typeName = typeName.Substring(1);
            }

            return typeName;
        }

        public static string ToClassName(this ITypeSymbol symbol)
        {
            return symbol.Name.InterfaceToClassName();
        }

        public const string CONCRETIZATION_SUFFIX = "_Concretization";

        public const string DEFAULT_WRAPPER_SUFFIX = "_DefaultWrapper";

        public static string GetConcretizationName(this string typeName)
        {
            typeName = typeName.InterfaceToClassName();

            return typeName + CONCRETIZATION_SUFFIX;
        }

        public static string GetDefaultWrapperName(this string typeName)
        {
            typeName = "I" + typeName;

            return typeName + DEFAULT_WRAPPER_SUFFIX;
        }

        public static string GetConcretizedTypeName(this ITypeSymbol typeSymbol)
        {
            return typeSymbol.Name.GetConcretizationName();
        }

        public static string GetDefaultWrapperName(this ITypeSymbol typeSymbol)
        {
            return typeSymbol.Name.GetDefaultWrapperName();
        }

        static string GetClassName(string implIntefaceName, string className)
        {
            if (className != null)
                return className;

            if (implIntefaceName != typeof(NoType).Name)
            {
                // we simply remove first 'I' from interface name
                className = implIntefaceName.InterfaceToClassName();
            }

            return className ?? throw new Exception($"Class name cannot be null");
        }

        internal static string GetClassName(this Type implInterfaceType, string className)
        {
            return GetClassName(implInterfaceType.Name, className);
        }

        internal static string GetClassName(this ITypeSymbol typeSymbol, string className)
        {
            return GetClassName(typeSymbol.Name, className);
        }

        public static string GetTypeName(this INamedTypeSymbol namedTypeSymbol)
        {
            return TypeUtils.GetTypeName(namedTypeSymbol.Name);
        }

        internal static string CreateClassName(this INamedTypeSymbol typeToImpl, INamedTypeSymbol implementorType)
        {
            string result = typeToImpl.UnwrapNestedType();

            if (implementorType.IsNoTypeOrNull())
            {
                return result;
            }

            result += "_" + TypeUtils.GetTypeName(implementorType.Name);

            return result;
        }

        internal static string CreateClassName(this Type typeToImpl, Type wrapperType)
        {
            if (typeToImpl == typeof(NoType))
            {
                throw new Exception("Roxy Usage Error: Cannot create a type class without Type to Implement.");
            }

            string result = "";

            result += typeToImpl.NestedTypeToName() + "_";

            if ((wrapperType != typeof(NoType)) && (wrapperType != null))
            {
                result += wrapperType.GetTypeName() + "_";
            }

            result += "Default";

            return result;
        }

        internal static string GetClassName<TToImplement, TWrapper>(this string className)
        {
            if (!className.IsNullOrEmpty())
                return className;

            Type typeToImpl = typeof(TToImplement);

            Type wrapperType = typeof(TWrapper);

            return CreateClassName(typeToImpl, wrapperType);
        }

        internal static bool Matches(this INamedTypeSymbol typeSymbol, Type type, Compilation compilation)
        {
            if (typeSymbol == null)
                return false;

            INamedTypeSymbol typeSymbolToCompare = type.GetTypeSymbol(compilation);

            return typeSymbolToCompare.GetUniqueTypeStr() == typeSymbol.GetUniqueTypeStr();
        }

        public static bool CanBeConvertedImplicitly
        (
            this Compilation compilation,
            ITypeSymbol sourceType,
            ITypeSymbol targetType
        )
        {
            return compilation.ClassifyConversion(sourceType, targetType).IsImplicit;
        }


        public static IMethodSymbol GetStaticMethodWithFirstArgThatCanAcceptType
        (
            this Compilation compilation,
            INamedTypeSymbol staticMethodsContainer,
            INamedTypeSymbol firstArgType,
            string name,
            bool allowNonPublic
        )
        {
            IEnumerable<IMethodSymbol> foundMethods =
                staticMethodsContainer.GetMembersByName<IMethodSymbol>(name, allowNonPublic)
                                      .Where(method => method.IsStatic)
                                      .Where(method => compilation.FirstParamMatches(method, firstArgType));

            if (foundMethods.Count() > 1)
                throw new Exception($"Error: this Roxy version cannot deal with overloaded methods at this point. Static method {staticMethodsContainer.Name}.{name} is overloaded.");

            return foundMethods.FirstOrDefault();
        }

        public static string GetTypeAdapterClassName(this Type fromType, Type toType)
        {
            string className = $"{fromType.Name}_To_{toType.Name}_Adapter";

            return className;
        }

        public static void SaveProj(this Project proj, string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            foreach (Document doc in proj.Documents)
            {
                SourceText docText = doc.GetTextAsync().Result;

                string fileName = doc.Name + ".cs";

                using (TextWriter textWriter = new StreamWriter(path + "/" + fileName))
                {
                    docText.Write(textWriter);
                    textWriter.Close();
                }
            }
        }

        public static IMethodSymbol GetPublicDefaultConstructor(this INamedTypeSymbol typeSymbol)
        {
            IMethodSymbol result =
                typeSymbol.InstanceConstructors
                          .Where(constr => constr.DeclaredAccessibility == Accessibility.Public)
                          .FirstOrDefault(constr => constr.Parameters.Count() == 0);

            return result;
        }

        public static bool HasPublicDefaultConstructor(this INamedTypeSymbol typeSymbol) =>
            typeSymbol.GetPublicDefaultConstructor() != null;

        public static string GetUniqueEventId(this IEventSymbol eventSymbol)
        {
            return eventSymbol.Type.GetUniqueTypeStr() + "#" + eventSymbol.Name;
        }

        public static string GetUniqueTypeStr(this ITypeSymbol type)
        {
            string fullNamespace = type.GetFullNamespace();

            if (type is INamedTypeSymbol namedType1)
            {
                return fullNamespace + "." + namedType1.GetFullTypeString();
            }

            return fullNamespace + "." + type.Name;
        }

        public static bool TypesStrictlyMatch
        (
           this ITypeSymbol type1,
           ITypeSymbol type2
        )
        {
            if (type1 is INamedTypeSymbol namedType1)
            {
                if (type2 is INamedTypeSymbol namedType2)
                {
                    return namedType1.GetUniqueTypeStr() == namedType2.GetUniqueTypeStr();
                }
                else
                {
                    return false;
                }
            }

            return (type1.GetUniqueTypeStr() == type2.GetUniqueTypeStr());
        }

        public static bool ParamTypesMatch
        (
            this Compilation compilation,
            ITypeSymbol callingParamType,
            ITypeSymbol calledParamType,
            bool isInputParam = true)
        {
            ITypeSymbol source, target;

            if (isInputParam)
            {
                source = callingParamType;
                target = calledParamType;
            }
            else
            {
                target = callingParamType;
                source = calledParamType;
            }

            if (target == null)
            {
                if (source != null)
                    return false;
                else
                    return true;
            }
            else
            {
                if (source == null)
                    return false;
            }

            if (source == target)
                return true;

            return compilation.ClassifyConversion(source, target).IsImplicit;
        }

        public static bool MethodArgsMatch
        (
            this Compilation compilation,
            IMethodSymbol methodSymbol,
            ITypeSymbol returnType, // null for void
            IEnumerable<ITypeSymbol> inputTypes
        )
        {
            if (!compilation.ParamTypesMatch(returnType, methodSymbol.ReturnType, false))
                return false;

            if (methodSymbol.Parameters.Count() != inputTypes.Count())
                return false;

            foreach (var sourceTargetParams in inputTypes.Zip(methodSymbol.Parameters, (sourceType, targetParam) => new { SourceType = sourceType, TargetType = targetParam.Type }))
            {
                if (!compilation.ParamTypesMatch(sourceTargetParams.SourceType, sourceTargetParams.TargetType))
                    return false;
            }

            return true;
        }

        public static bool MethodArgsMatch(this Compilation compilation, IMethodSymbol callingMethod, IMethodSymbol calledMethod)
        {
            return compilation.MethodArgsMatch(calledMethod, callingMethod.ReturnType, callingMethod.Parameters.Select(param => param.Type));
        }

        public static IMethodSymbol FindMatchingMethodSymbol
        (
            this Compilation compilation,
            INamedTypeSymbol calledMethodContainerTypeSymbol,
            string methodName,
            ITypeSymbol returnTypeSymbol,
            IEnumerable<ITypeSymbol> inputTypeSymbols,
            bool allowNonPublic = false
        )
        {
            IEnumerable<IMethodSymbol> methodsMatchedByName =
                calledMethodContainerTypeSymbol.GetMembersByName<IMethodSymbol>(methodName, allowNonPublic);

            IEnumerable<IMethodSymbol> results =
                methodsMatchedByName
                    .Where(methodSymbol => compilation.MethodArgsMatch(methodSymbol, returnTypeSymbol, inputTypeSymbols));

            IMethodSymbol result = results.FirstOrDefault(methodSymbol => !methodSymbol.IsAbstract);

            if (result == null)
            {
                result = results.FirstOrDefault();
            }

            return result;
        }

        public static IMethodSymbol FindMatchingMethodSymbol
        (
            this Compilation compilation,
            IMethodSymbol callingMethod,
            INamedTypeSymbol calledMethodContainerTypeSymbol,
            string nameToMatch = null,
            bool allowNonPublic = false
        )
        {
            if (nameToMatch == null)
            {
                nameToMatch = callingMethod.Name;
            }

            return compilation
                    .FindMatchingMethodSymbol
                    (
                        calledMethodContainerTypeSymbol, 
                        nameToMatch, 
                        callingMethod.ReturnType, 
                        callingMethod.Parameters.Select(param => param.Type),
                        allowNonPublic
                    );
        }

        public static ISymbol FindMatchingSymbol
        (
            this Compilation compilation,
            ISymbol callingSymbol,
            INamedTypeSymbol calledContainerTypeSymbol,
            string nameToMatch = null,
            bool allowNonPublic = false
        )
        {
            if (nameToMatch == null)
            {
                nameToMatch = callingSymbol.Name;
            }

            IMethodSymbol callingMethodSymbol = callingSymbol as IMethodSymbol;

            if (callingMethodSymbol != null)
            {
                return
                    compilation.FindMatchingMethodSymbol
                    (
                        callingMethodSymbol,
                        calledContainerTypeSymbol,
                        nameToMatch,
                        allowNonPublic);
            }

            return calledContainerTypeSymbol?.GetMemberByName<ISymbol>(nameToMatch, allowNonPublic);
        }


        public static bool FirstParamMatches(this Compilation compilation, IMethodSymbol methodSymbol, INamedTypeSymbol typeToMatch)
        {
            if (methodSymbol.Parameters.Length == 0)
                return false;

            return compilation.ParamTypesMatch(typeToMatch, methodSymbol.Parameters.First().Type);
        }

        public static IMethodSymbol FindMatchingMethodSymbol
        (
            this Compilation compilation,
            INamedTypeSymbol methodContainerTypeSymbol,
            string methodName,
            Type returnType, // null for void
            params Type[] inputTypes
        )
        {
            return compilation.FindMatchingMethodSymbol
            (
                methodContainerTypeSymbol,
                methodName,
                returnType.GetTypeSymbol(compilation),
                inputTypes.Select(inputType => inputType.GetTypeSymbol(compilation)));
        }

        public static IMethodSymbol FindMatchingMethodSymbol<ContainerType>
        (
            this Compilation compilation,
            string methodName,
            Type returnType, // null for void
            params Type[] inputTypes
        )
        {
            INamedTypeSymbol methodContainerTypeSymbol = typeof(ContainerType).GetTypeSymbol(compilation);

            return compilation.FindMatchingMethodSymbol
            (
                methodContainerTypeSymbol,
                methodName,
                returnType.GetTypeSymbol(compilation),
                inputTypes.Select(inputType => inputType.GetTypeSymbol(compilation)));
        }
    }

    public class SymbolComparer : IEqualityComparer<ISymbol>
    {
        public static SymbolComparer TheSymbolComparer { get; } =
            new SymbolComparer();

        private string GetDefiningStr(ISymbol symbol)
        {
            INamedTypeSymbol containingSymbol =
                symbol.ContainingSymbol as INamedTypeSymbol;

            string name = symbol?.Name;

            return (containingSymbol?.GetUniqueTypeStr()).NullToEmpty() + " " + name.NullToEmpty();
        }

        public bool Equals(ISymbol symbol1, ISymbol symbol2)
        {
            return GetDefiningStr(symbol1).ObjEquals(GetDefiningStr(symbol2));
        }

        public int GetHashCode(ISymbol symbol)
        {
            return GetDefiningStr(symbol).GetHashCodeExtension();
        }
    }

    public class TypeSymbolComparer : IEqualityComparer<INamedTypeSymbol>
    {
        public static TypeSymbolComparer TheTypeSymbolComparer { get; } =
            new TypeSymbolComparer();

        public bool Equals(INamedTypeSymbol type1, INamedTypeSymbol type2)
        {
            return type1.TypesStrictlyMatch(type2);
        }

        public int GetHashCode(INamedTypeSymbol type)
        {
            return type.GetUniqueTypeStr().GetHashCodeExtension();
        }
    }

    public class SymbolByNameAndSignatureComparer : IEqualityComparer<ISymbol>
    {
        public bool Equals(ISymbol x, ISymbol y)
        {
            if (x is IMethodSymbol xMethod)
            {
                if (y is IMethodSymbol yMethod)
                {
                    return xMethod.MethodsStrictlyMatch(yMethod);
                }
                else
                {
                    return false;
                }
            }

            return x.Name == y.Name;
        }

        public int GetHashCode(ISymbol obj)
        {
            return obj.Name.GetHashCodeExtension();
        }
    }

    public class AbstractionComparer : IComparer<ISymbol>
    {
        int ToInt(ISymbol symbol)
        {
            if (symbol.ContainingType?.TypeKind == TypeKind.Interface)
                return 0;

            if (symbol.IsAbstract)
                return 1;

            if (symbol.IsOverridable())
                return 2;

            return 3;
        }

        public int Compare(ISymbol x, ISymbol y)
        {
            return (ToInt(x)).CompareTo(ToInt(y));
        }
    }
}
