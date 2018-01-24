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
    public enum PropertyKind
    {
        Unknown,
        Plain, // plain C# property (in order to detect property change it has to be notifiable)
        Map, // a property that is retrieved from or set on a dictionary object by using string indexer (theObj[propName])
        AProperty, // AProperty
        Attached // WPF attached property
    }

    public static class PropertyKindExtensions
    {
        public static bool IsNameBased(this PropertyKind propKind)
        {
            switch(propKind)
            {
                case PropertyKind.Map:
                case PropertyKind.Plain:
                    return true;
                default:
                    return false;
            }
        }
    }
}
