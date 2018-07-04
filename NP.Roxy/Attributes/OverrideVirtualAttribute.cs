using NP.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NP.Roxy.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = false)]
    public class OverrideVirtualAttribute : Attribute
    {
        public string[] MemberNames { get; }

        public bool IncludeBase { get; }

        public OverrideVirtualAttribute(bool includeBase, params string[] memberNames)
        {
            if (memberNames.IsNullOrEmpty())
                throw new Exception("Roxy Usage Error: No member names are passed over to OverrideVirtualAttribute constructor");

            IncludeBase = includeBase;
            MemberNames = memberNames;
        }
    }
}
