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

namespace NP.Utilities
{
    using static NP.Utilities.StrUtils;

    public static class ReflectionUtils
    {
        public static MemberInfo[] GetMemberInfos
        (
            this Type type,
            string propName,
            bool includeNonPublic
        )
        {
            BindingFlags bindingFlags = BindingFlags.Public;

            if (includeNonPublic)
                bindingFlags |= BindingFlags.NonPublic;

            MemberInfo[] result =
                type.GetMember(propName, bindingFlags);

            return result;
        }

        static MemberInfo GetSingleMemberInfoImpl
        (
            this Type type,
            string memberName,
            bool includeNonPublic
        )
        {
            MemberInfo[] memberInfos = type.GetMemberInfos(memberName, includeNonPublic);

            if (memberInfos.Length == 1)
            {
                return memberInfos.First();
            }

            if (memberInfos.Length > 1)
            {
                throw new Exception
                (
                    $"Error: there is more than one instance of member {memberName} within type {type.GetFullTypeStr()}."
                );
            }

            // if no members found, return null
            return null;
        }

        public static MemberInfo GetSingleMemberInfo(this Type type, string memberName)
        {
            MemberInfo result = type.GetSingleMemberInfoImpl(memberName, false);

            if (result == null)
            {
                // if there are no public members found - try a non-public one
                result = type.GetSingleMemberInfoImpl(memberName, true);

                if (result == null)
                {
                    throw new Exception
                        ($"Error: No member of name {memberName} is found within {type.GetFullTypeStr()} class.");

                }
            }

            return result;
        }

        public static ClassMemberType GetClassMemberType(this Type type, string memberName)
        {
            MemberInfo memberInfo = type.GetSingleMemberInfo(memberName);

            if (memberInfo is PropertyInfo)
                return ClassMemberType.Property;

            if (memberInfo is MethodInfo)
                return ClassMemberType.Method;

            if (memberInfo is EventInfo)
                return ClassMemberType.Event;

            return ClassMemberType.None;
        }

        static BindingFlags GetBindingFlags(bool includeNonPublic, bool isStatic)
        {
            BindingFlags bindingFlags =
                BindingFlags.Public;

            if (includeNonPublic)
                bindingFlags |= BindingFlags.NonPublic;

            if (isStatic)
            {
                bindingFlags |= BindingFlags.Static;
            }
            else
            {
                bindingFlags |= BindingFlags.Instance;
            }

            return bindingFlags;
        }

        public static PropertyInfo GetPropInfoFromType
        (
            this Type type, 
            string propName, 
            bool includeNonPublic = false)
        {
            BindingFlags bindingFlags = GetBindingFlags(includeNonPublic, false);

            PropertyInfo sourcePropInfo = type.GetProperty(propName, bindingFlags);

            return sourcePropInfo;
        }

        public static PropertyInfo GetPropInfo
        (
            this object obj, 
            string propName, 
            bool includeNonPublic = false)
        {
            PropertyInfo sourcePropInfo = obj.GetType().GetPropInfoFromType(propName, includeNonPublic);

            return sourcePropInfo;
        }

        public static object GetPropValue
        (
            this object obj, 
            string propName, 
            bool includeNonPublic = false)
        {
            PropertyInfo propInfo = obj.GetPropInfo(propName, includeNonPublic);

            return propInfo.GetValue(obj, null);
        }

        public static T GetPropValue<T>(this object obj, string propName)
        {
            return (T)obj.GetPropValue(propName);
        }

        public static void SetPropValue(this object obj, string propName, object val, bool includeNonPublic = false)
        {
            PropertyInfo propInfo = GetPropInfo(obj, propName, includeNonPublic);

            propInfo.SetValue(obj, val, null);
        }


        public static object CallMethod(this object obj, string methodName, bool includeNonPublic, bool isStatic, params object[] args)
        {
            BindingFlags bindingFlags = GetBindingFlags(includeNonPublic, isStatic);

            Type type = null;
            if (isStatic)
            {
                type = (Type)obj;
            }
            else
            {
                type = obj.GetType();
            }

            MethodInfo methodInfo = type.GetMethod(methodName, bindingFlags);

            return methodInfo.Invoke(obj, args);
        }


        public static T GetCompoundPropValue<T>
        (
            this object obj, 
            string compoundPropName
        )           
            where T : class
        {
            (string firstLink, string remainder) =
                compoundPropName.BreakStrAtSeparator(PLAIN_PATH_LINK_SEPARATOR);

            object nextObj = obj.GetPropValue<T>(firstLink);

            if (remainder == null)
                return (T) nextObj;

            return nextObj?.GetCompoundPropValue<T>(remainder);
        }

        public static void SetCompoundPropValue(this object obj, string compoundPropName, object val)
        {
            object nextObj = obj;

            (string firstLink, string remainder) =
                compoundPropName.BreakStrAtSeparator(PLAIN_PATH_LINK_SEPARATOR);

            if (remainder == null)
            {
                obj.SetPropValue(firstLink, val);
                return;
            }

            nextObj = obj.GetPropValue(firstLink);

            nextObj?.SetCompoundPropValue(remainder, val);
        }

        public static PropertyInfo GetStaticPropInfo(this Type type, string propName)
        {
            PropertyInfo propInfo = type.GetProperty(propName, BindingFlags.Static | BindingFlags.Public);

            return propInfo;
        }

        public static object GetStaticPropValue(this Type type, string propName)
        {
            PropertyInfo propInfo = type.GetStaticPropInfo(propName);

            object val = propInfo.GetValue(type);

            return val;
        }


        public static FieldInfo GetStaticFieldInfo(this Type type, string propName)
        {
            FieldInfo fieldInfo = type.GetField(propName, BindingFlags.Static | BindingFlags.Public);

            return fieldInfo;
        }

        public static object GetStaticFieldValue(this Type type, string propName)
        {
            FieldInfo fieldInfo = type.GetStaticFieldInfo(propName);

            object val = fieldInfo.GetValue(type);

            return val;
        }

        public static void CopyProp
        (
            this object sourceObj,
            string sourcePropName,
            object targetObj,
            string targetPropName = null,
            IValConverter converter = null
        )
        {
            object sourcePropValue = sourceObj;

            if (sourcePropName != null)
            {
                sourcePropValue = GetPropValue(sourceObj, sourcePropName);
            }

            object targetPropValue = sourcePropValue;

            if (converter != null)
            {
                targetPropValue = converter.Convert(sourcePropValue);
            }

            if (targetPropName == null)
                targetPropName = sourcePropName;

            ReflectionUtils.SetPropValue(targetObj, targetPropName, targetPropValue);
        }

        public static ConstructorInfo GetDefaultConstructor(this Type type)
        {
            ConstructorInfo result = type.GetConstructor(Type.EmptyTypes);

            return result;
        }

        public static bool HasDefaultConstructor(this Type type)
        {
            ConstructorInfo constructorInfo = type.GetConstructor(Type.EmptyTypes);

            return (constructorInfo != null);
        }

        public static Type GetTypeFromTypeName(this string typeName)
        {
            Assembly[] allAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in allAssemblies)
            {
                Type type = assembly.GetType(typeName);

                if (type != null)
                    return type;
            }

            return null;
        }

        public static Assembly FindOrLoadAssembly(this AssemblyName assemblyName)
        {
            Assembly result =
                AppDomain.CurrentDomain
                         .GetAssemblies()
                         .FirstOrDefault(assembly => assembly.FullName == assemblyName.FullName);

            if (result == null)
                result = Assembly.Load(assemblyName);

            return result;
        }

        public static IEnumerable<Assembly> AssemblyNamesToAssembly(this IEnumerable<AssemblyName> assemblyNames)
        {
            var result = assemblyNames.Select(assemblyName => assemblyName.FindOrLoadAssembly()).ToList();

            return result;
        }

        public static IEnumerable<Assembly> GetAssembliesReferencedByAssembly(this Assembly assembly)
        {
            return assembly.GetReferencedAssemblies().AssemblyNamesToAssembly();
        }

        public static IEnumerable<Assembly> GetAssemblyAndReferencedAssemblies(this Assembly assembly)
        {
            return (new[] { assembly }).Union(assembly.GetAssembliesReferencedByAssembly()).ToList();
        }

        public static IEnumerable<Assembly> GetAndLoadAssemblyAndAssembliesItDependsOn(this Assembly assembly)
        {
            IEnumerable<Assembly> result = 
                assembly
                    .GetReferencedAssemblies()
                    .Select(assemblyName => Assembly.Load(assemblyName))
                    .Concat(new[] { assembly });

            return result;
        }

        public static bool HasGetterAndSetter(this PropertyInfo propInfo)
        {
            return (propInfo.GetGetMethod() != null) &&
                           (propInfo.GetSetMethod() != null);
        }


        public static IEnumerable<PropertyInfo> GetTypePropsWithGetterAndSetter(this Type type)
        {
            return
                type
                    .GetProperties()
                    .Where (propInfo => propInfo.HasGetterAndSetter());
        }

        public static IEnumerable<PropertyInfo> GetPropsWithGettersAndSetter(this object obj)
        {
            return obj.GetType().GetTypePropsWithGetterAndSetter();
        }

        public static string GetFullTypeStr(this Type type)
        {
            return type.Namespace + "." + type.Name;
        }

        public static void SetStaticPropValue
        (
            this Type type, 
            string propName, 
            object valueToSet)
        {
            PropertyInfo propInfo = 
                type.GetProperty(propName, BindingFlags.Static | BindingFlags.Public);

            propInfo.SetValue(null, valueToSet);
        }
    }
}
