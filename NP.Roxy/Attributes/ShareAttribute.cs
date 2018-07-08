using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NP.Roxy.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public class ShareAttribute : Attribute
    {
        public string SharedPluginName { get; }

        string _mapsToName;
        public string MapsToName
        {
            get => _mapsToName ?? SharedPluginName;
            private set
            {
                if (_mapsToName == value)
                    return;

                _mapsToName = value;
            }
        }

        public ShareAttribute(string sharedPluginName, string mapsToName = null)
        {
            SharedPluginName = sharedPluginName;
            MapsToName = mapsToName;
        }
    }
}
