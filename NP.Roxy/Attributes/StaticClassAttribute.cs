using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NP.Roxy.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class StaticClassAttribute : Attribute
    {
        public Type StaticClassType { get; set; }

        public StaticClassAttribute(Type staticClassType)
        {
            StaticClassType = staticClassType;
        }
    }
}
