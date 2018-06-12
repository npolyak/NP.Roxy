using Microsoft.CodeAnalysis;
using NP.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NP.Roxy.Attributes
{
    // pulls the member out of a wrapped (plugin) class into the
    // wrapper
    [AttributeUsage(AttributeTargets.Property, AllowMultiple=true, Inherited=true)]
    public class PullMemberAttribute : Attribute
    {
        public string WrapperMemberName { get; set; }

        string _wrappedMemberName = null;
        public string WrappedMemberName
        {
            get => _wrappedMemberName ?? WrapperMemberName;
            set
            {
                if (_wrappedMemberName.ObjEquals(value))
                    return;

                _wrappedMemberName = value;
            }
        }

        // allows pulling non-public members of the wrapped class
        public bool AllowNonPublic { get; set; }

        public bool OverrideVirtual { get; set; }

        // TBD - regulating wrapper accessibility 
        // for now the wrapper accessibility will be always public
        public Accessibility WrapperAccessibility { get; set; }

        // TBD - if true, will allow to convert a 
        // parametereless method that returns a single value to a property
        public bool MethodToProp { get; set; }

        public bool MappedToThis => WrapperMemberName == null;

        public PullMemberAttribute()
        {

        }

        public PullMemberAttribute
        (
            string wrapperMemberName, 
            string wrappedMemberName = null, 
            bool allowNonPublic = false, 
            bool overrideVirtual = false,
            Accessibility wrapperAccessibility = Accessibility.Public, 
            bool methodToProp = false)
        {
            WrapperMemberName = wrapperMemberName;
            WrappedMemberName = wrappedMemberName;
            AllowNonPublic = allowNonPublic;
            OverrideVirtual = overrideVirtual;
            WrapperAccessibility = wrapperAccessibility;
            MethodToProp = methodToProp;
        }
    }
}
