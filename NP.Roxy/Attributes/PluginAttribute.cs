using System;

namespace NP.Roxy.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class PluginAttribute : Attribute
    {
        Type _implementorType;
        public Type ImplementorType
        {
            get => _implementorType;
            set
            {
                _implementorType = value;

                ThrowIfBothTypesSet();
            }
        }

        // should not be set when ImplementorType is set 
        // and vice versa
        Type _initType;
        public Type InitType
        {
            get => _initType;
            set
            {
                _initType = value;
                ThrowIfBothTypesSet();
            }
        }

        void ThrowIfBothTypesSet()
        {
            if ( (InitType != null) && (ImplementorType != null))
            {
                throw new Exception("InitType and ImplementType should never be non-null together.");
            }
        }

        public bool IsShared { get; set; }

        public PluginAttribute(Type implementorType = null, bool isShared = false, Type initType = null)
        {
            ImplementorType = implementorType;
            IsShared = isShared;
            InitType = initType;
        }
    }
}
