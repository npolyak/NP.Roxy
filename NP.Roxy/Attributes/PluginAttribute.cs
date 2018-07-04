using System;

namespace NP.Roxy.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class PluginAttribute : Attribute
    {
        public Type ImplementorType { get; set; }

        public PluginAttribute()
        {
        }

        public PluginAttribute(Type implementorType)
        {
            ImplementorType = implementorType;
        }
    }
}
