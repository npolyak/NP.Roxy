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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NP.Utilities
{
    public static class TypeUtils
    {
        // removes the 'apostrophy' and the number 
        // of generic args for a generic type
        public static string GetTypeName(this Type type)
        {
            return type.Name.SubstrFromTo(null, "`");
        }

        public static string Box(this string typeStr)
        {
            switch (typeStr)
            {
                case "Int32":
                    return "int";
                case "Int64":
                    return "long";
                case "String":
                    return "string";
                case "Double":
                    return "double";
                case "Decimal":
                    return "decimal";
                case "Boolean":
                    return "bool";
                case "Byte":
                    return "byte";
                case "Void":
                    return "void";
            }

            return typeStr;
        }

        // include the generic params
        public static string GetFullTypeName(this Type type)
        {
            string result = type.GetTypeName();

            if (type.IsGenericType)
            {
                result += "<";

                bool firstIteration = true;
                foreach(Type typeParam in type.GetGenericArguments())
                {
                    if (!firstIteration)
                    {
                        result += ", ";
                    }
                    else
                    {
                        firstIteration = false;
                    }

                    result += typeParam.GetFullTypeName();
                }
                result += ">";
            }

            return result;
        }

        public static List<string> 
            GetNamespaces(this IEnumerable<Type> types)
        {
            return types
                .NullToEmpty()
                .Select(type => type.Namespace).Distinct().ToList();
        }

        public static List<string> GetLocations(this IEnumerable<Type> types)
        {
            return types
                .NullToEmpty()
                .Select(type => type.Assembly.Location)
                .Distinct()
                .ToList();
        }

        public static bool IsNullable(this Type type)
        {
            if (!type.IsValueType)
                return true;

            if (Nullable.GetUnderlyingType(type) != null)
                return true;

            return false;
        }

        public static object StrToType(this Type type, string str)
        {
            if (type == typeof(double))
            {
                return Convert.ToDouble(str);
            }
            if (type == typeof(bool))
            {
                return Convert.ToBoolean(str);
            }
            if (type == typeof(decimal))
            {
                return Convert.ToDecimal(str);
            }
            if (type == typeof(int))
            {
                return Convert.ToInt32(str);
            }
            if (type == typeof(DateTime))
            {
                return Convert.ToDateTime(str);
            }
            if (type == typeof(long))
            {
                return Convert.ToInt64(str);
            }
            if (type == typeof(float))
            {
                return Convert.ToSingle(str);
            }
            if (type == typeof(byte))
            {
                return Convert.ToByte(str);
            }
            if (type == typeof(char))
            {
                return Convert.ToChar(str);
            }
            if (type == typeof(uint))
            {
                return Convert.ToUInt32(str);
            }
            if (type == typeof(ulong))
            {
                return Convert.ToUInt64(str);
            }
            if (type == typeof(ushort))
            {
                return Convert.ToUInt16(str);
            }
            if (type.IsEnum)
            {
                return Enum.Parse(type, str);
            }

            return str;
        }

        public static Type GetInnermostGenericTypeParam(this Type type)
        {
            if (type == null)
                return null;

            if (type.IsGenericParameter)
                return type;

            if (type.IsGenericType)
            {
                Type genericArg = type.GetGenericArguments().FirstOrDefault();

                if (genericArg == null)
                    return type;

                return genericArg.GetInnermostGenericTypeParam();
            }

            return type;
        }

        public static bool IsCollection(this Type type)
        {
            return typeof(IEnumerable).IsAssignableFrom(type);
        }

        public static Type GetChildType(this Type type, string propName)
        {
            PropertyInfo propInfo = type.GetProperty(propName);

            return propInfo.PropertyType;
        }

        public static Type GetItemType(this Type type)
        {
            if (!type.IsCollection())
                return type;

            Type argType =
                type.GenericTypeArguments.First();

            return argType;
        }
    }
}
