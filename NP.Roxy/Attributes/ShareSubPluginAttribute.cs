using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NP.Roxy.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public class ShareSubPluginAttribute : Attribute
    {
        public string SharedPluginName { get; }

        string _mapsToExternalName;
        public string MapsToExternalName
        {
            get => _mapsToExternalName ?? SharedPluginName;
            private set
            {
                if (_mapsToExternalName == value)
                    return;

                _mapsToExternalName = value;
            }
        }

        public ShareSubPluginAttribute(string sharedPluginName, string mapsToName = null)
        {
            SharedPluginName = sharedPluginName;
            MapsToExternalName = mapsToName;
        }
    }
}
