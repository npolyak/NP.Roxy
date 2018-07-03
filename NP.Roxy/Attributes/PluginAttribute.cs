using System;

namespace NP.Roxy.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class PluginAttribute : Attribute
    {
        public Type ImplementorType { get; set; }
        public int I { get; set; }
        public ClassMemberType TheClassMemberType { get; set; }

        public PluginAttribute()
        {
        }

        public PluginAttribute(Type implementorType, int i, ClassMemberType classMemberType)
        {
            ImplementorType = implementorType;

            I = i;

            TheClassMemberType = classMemberType;
        }
    }
}
