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
using System.Text;
using System.Threading.Tasks;

namespace NP.Utilities
{
    class SeparatorInfo
    {
        internal PropertyKind ThePropertyKind { get; private set; }
        internal string BeginSeparator { get; private set; }
        internal string EndSepartor { get; private set; }

        public SeparatorInfo (PropertyKind propKind, string beginSeparator, string endSeparator)
	    {
            ThePropertyKind = propKind;
            BeginSeparator = beginSeparator;
            EndSepartor = endSeparator;
	    }
    }

    public static class StrUtils
    {
        public const string UNDERSCORE = "_";
        public const string PERIOD = ".";

        public const string COMMA_SEPARATOR = ", ";
        public const string PLAIN_PATH_LINK_SEPARATOR = ".";
        public const string ATTACHED_PATH_LINK_SEPARATPOR_BEGIN = "(";
        public const string ATTACHED_PATH_LINK_SEPARATPOR_END = ")";
        public const string APROP_PATH_LINK_SEPARATPOR_BEGIN = "*";
        public const string APROP_PATH_LINK_SEPARATPOR_END = "*";
        public const string MAP_PROP_PATH_LINK_SEPARATOR_BEGIN = "-";
        public const string MAP_PROP_PATH_LINK_SEPARATOR_END = "-";

                
        static readonly SeparatorInfo[] Separators = 
            new SeparatorInfo[]
            {
                new SeparatorInfo(PropertyKind.Attached, ATTACHED_PATH_LINK_SEPARATPOR_BEGIN, ATTACHED_PATH_LINK_SEPARATPOR_END),
                new SeparatorInfo(PropertyKind.AProperty, APROP_PATH_LINK_SEPARATPOR_BEGIN, APROP_PATH_LINK_SEPARATPOR_END),
                new SeparatorInfo(PropertyKind.Map, MAP_PROP_PATH_LINK_SEPARATOR_BEGIN, MAP_PROP_PATH_LINK_SEPARATOR_END)
            };

        static SeparatorInfo SeparatorInfoByPropertyKind(this PropertyKind propertyKind)
        {
            SeparatorInfo result = Separators.Where((sepInfo) => sepInfo.ThePropertyKind == propertyKind).FirstOrDefault();

            return result;
        }

        static SeparatorInfo SeparatorInfoByBeginSeparator(string str)
        {
            if (str == null)
                return null;

            SeparatorInfo result = Separators.Where( (sepInfo) => str.StartsWith(sepInfo.BeginSeparator) ).FirstOrDefault();

            return result;
        }

        public static bool StartsWith(this string str, IEnumerable<char> charsToFind, int idx = 0)
        {
            if (charsToFind == null)
                return false;

            int i = idx;
            foreach (char c in charsToFind)
            {
                if (c != str[i])
                    return false;

                i++;
            }

            return true;
        }

        public static string Wrap(this PropertyKind propertyKind,  string stringToWrap)
        {
            SeparatorInfo separatorInfo = propertyKind.SeparatorInfoByPropertyKind();

            if (separatorInfo == null)
                return stringToWrap;

            string result = separatorInfo.BeginSeparator + stringToWrap + separatorInfo.EndSepartor;

            return result;
        }

        public static string SubstrFromTo
        (
            this string str,
            string start,
            string end,
            bool firstOrLast = true // first by default
        )
        {
            if (str == null)
                return string.Empty;

            int startIdx = 0;
            int endIdx = str.Length;

            if (!string.IsNullOrEmpty(start))
            {
                startIdx = firstOrLast ? str.IndexOf(start) : str.LastIndexOf(start);

                startIdx += start.Length;
            }

            if (!string.IsNullOrEmpty(end))
            {
                int endIndex = firstOrLast ? str.IndexOf(end) : str.LastIndexOf(end);

                if (endIndex >= 0)
                {
                    endIdx = endIndex;
                }
            }

            if (endIdx <= startIdx)
                return string.Empty;

            return str.Substring(startIdx, endIdx - startIdx);
        }


        public static string StrConcat<T>
        (
            this IEnumerable<T> items,
            Func<T, string> toStr = null,
            string separator = COMMA_SEPARATOR
        )
        {
            if (toStr == null)
            {
                toStr = (item) => item.ToString();
            }

            string result = string.Empty;

            bool firstIteration = true;
            foreach (T item in items)
            {
                if (firstIteration)
                {
                    firstIteration = false;
                }
                else
                {
                    result += separator;
                }

                result += toStr(item);
            }

            return result;
        }

        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        public static (string, string) BreakStrAtSeparator(this string strToBreak, string separator, bool firstOrLast = true)
        {
            int idx = 
                firstOrLast ? 
                    strToBreak.IndexOf(separator) : 
                    strToBreak.LastIndexOf(separator);

            if (idx < 0)
            {
                return (strToBreak, null);
            }

            return (strToBreak.Substring(0, idx), strToBreak.Substring(idx + separator.Length));
        }
    }
}
