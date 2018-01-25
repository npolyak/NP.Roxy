using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NP.Roxy.Demos.PropChangedInterfaceImpl
{
    public interface IPerson : INotifyPropertyChanged
    {
        string FirstName { get; set; }

        string LastName { get; set; }

        int Age { get; set; }

        string Profession { get; set; }
    }
}
