using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NP.Roxy.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
    public class ImplementationClassNameAttribute : Attribute
    {
        public string ClassName { get; }

        public ImplementationClassNameAttribute(string className)
        {
            ClassName = className;
        }
    }
}
