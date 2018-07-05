using System;

namespace NP.Roxy.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class PluginAttribute : Attribute
    {
        public Type ImplementorType { get; set; }

        public bool IsShared { get; set; }

        public PluginAttribute(Type implementorType = null, bool isShared = false)
        {
            ImplementorType = implementorType;
            IsShared = isShared;
        }
    }
}
