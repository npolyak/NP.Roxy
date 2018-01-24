// (c) Nick Polyak 2018 - http://awebpros.com/
// License: Apache License 2.0 (http://www.apache.org/licenses/LICENSE-2.0.html)
//
// short overview of copyright rules:
// 1. you can use this framework in any commercial or non-commercial 
//    product as long as you retain this copyright message
// 2. Do not blame the author(s) of this software if something goes wrong. 
// 
// Also, please, mention this software in any documentation for the 
// products that use it.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NP.Utilities
{
    public static class CodeUtils
    {
        public static string GetClosing(this string opening)
        {
            switch (opening)
            {
                case "{":
                    return "}";
                case "[":
                    return "]";
                default:
                    return null;
            }
        }

        public static string GetTypeNameAndConcretize
        (
            this Type type, 
            Func<Type, Type> genericArgToType = null,
            bool restoreFully = false
        )
        {
            Type resultingType = type;

            if (genericArgToType != null)
                resultingType = genericArgToType(type);

            return restoreFully ?
                resultingType.GetFullTypeName() : resultingType.GetTypeName();
        }

        public static string GetTypeStr
        (
            this Type type,
            Func<Type, Type> genericArgToType = null,
            bool restoreFully = false
        )
        {
            string result = 
                type.GetTypeNameAndConcretize(genericArgToType, restoreFully);

            if (type.Name == "Void")
            {
                return "void";
            }

            if (type.IsGenericType)
            {
                result += "<";

                Type[] genericArgs =
                    type.GetGenericArguments();

                result += genericArgs.StrConcat((t) => t.GetTypeStr(genericArgToType, true));

                result += ">";
            }

            return result;
        }

        public static string GetParamStr
        (
            this ParameterInfo paramInfo,
            Func<Type, Type> genericArgToType = null,
            Func<string, string> paramNameConverter = null
        )
        {
            Type paramType = paramInfo.ParameterType;

            string paramName = paramInfo.Name;

            string result = string.Empty;

            if (paramNameConverter == null)
                paramNameConverter = (str) => str;

            if (paramType.IsByRef)
            {
                result += "ref ";
            }
            else if (paramInfo.IsOut)
            {
                result += "out ";
            }

            result += $"{paramType.GetTypeStr(genericArgToType, true)} {paramNameConverter(paramName)}";

            return result;
        }

        public static string GetEncapsulationStr(this MethodInfo methodInfo)
        {
            if (methodInfo.IsPrivate)
                return "private";

            if (methodInfo.IsPublic)
                return "public";

            if (methodInfo.IsFamily)
                return "protected";

            if (methodInfo.IsAssembly)
            {
                return "internal";
            }

            return null;
        }

        public static string GetMethodDefiners(this MethodInfo methodInfo, bool shouldOverride)
        {
            string result = methodInfo.GetEncapsulationStr() + " ";
            if (shouldOverride)
            {
                result += "override ";
            }
            else if (methodInfo.IsAbstract)
            {
                result += "abstract ";
            }
            else if (methodInfo.IsStatic)
            {
                result += "static ";
            }

            return result;
        }

        public static string GetMethodSignature
        (
            this MethodInfo methodInfo,
            bool shouldOverride = false,
            Func<Type, Type> genericArgToType = null,
            Func<string, string> paramNameConverter = null
        )
        {
            if (methodInfo == null)
                return string.Empty;

            string result = methodInfo.GetMethodDefiners(shouldOverride);


            if (!methodInfo.IsConstructor)
            {
                result += methodInfo.ReturnType.GetTypeStr(genericArgToType, true);
            }

            result += $" {methodInfo.Name}(";

            ParameterInfo[] paramsInfo = methodInfo.GetParameters();

            if (paramNameConverter == null)
                paramNameConverter = (str) => str;

            result += 
                paramsInfo
                    .StrConcat((paramInfo) => paramInfo.GetParamStr(genericArgToType, paramNameConverter));

            result += ")";

            return result;
        }

        // 3 tabs - one for namespace, one for 
        // class and one for method
        public const int MethodBodyShift = 3;

        public static string GetPopAll(int offset)
        {
            CodeBuilder builder = new CodeBuilder(offset);

            builder.PopAll();

            return builder.ToString();
        }

        public static string GetMethodBodyTotalClosing()
        {
            return GetPopAll(MethodBodyShift);
        }

        public static string PropToFieldName(this string propName)
        {
            if (propName.StartsWith(StrUtils.UNDERSCORE))
                throw new Exception("Property Name cannot start with udnerscore");

            return StrUtils.UNDERSCORE +
                   Char.ToLower(propName[0]) +
                   propName.Substring(1);

        }

        public static string GetUsingText(this string namespaceName)
        {
            return $"using {namespaceName}";
        }

        public static string GetUsingLine(this string namespaceName)
        {
            return $"{namespaceName.GetUsingText()};\n";
        }
    }
}
