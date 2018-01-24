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
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NP.Utilities
{
    public static class ObjUtils
    {
        public static bool ReferenceEq(this object obj1, object obj2)
        {
            return ReferenceEquals(obj1, obj2);
        }

        public static bool ObjEquals(this object obj1, object obj2)
        {
            if (obj1 == obj2)
                return true;

            if ( (obj1 != null) && (obj1.Equals(obj2)))
                return true;

            return false;
        }

        public static int GetHashCodeExtension(this object obj)
        {
            if (obj == null)
                return 0;

            return obj.GetHashCode();
        }


        public static T[] ObjToCollection<T>(this T obj)
        {
            if (obj == null)
            {
                return new T[0];
            }

            return new T[] { obj };
        }



        public static string ToStr(this object obj)
        {
            if (obj == null)
                return string.Empty;

            return obj.ToString();
        }

        public static object TypeConvert(this object sourceValue, Type resultType)
        {
            if ( (sourceValue == null) || resultType.IsAssignableFrom(sourceValue.GetType()))
            {
                return sourceValue;
            }
            else
            {
                TypeConverter typeConverter = TypeDescriptor.GetConverter(resultType);

                return typeConverter?.ConvertFrom(sourceValue);
            }
        }

        public static TTarget TypeConvert<TTarget>(this object source)
        {
            object resultObj = source.TypeConvert(typeof(TTarget));

            return resultObj.ObjToType<TTarget>();
        }

        public static TTarget ObjToType<TTarget>(this object source)
        {
            if (source == null)
                return default(TTarget);

            return (TTarget)source;
        }
    }
}
