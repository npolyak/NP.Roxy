using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NP.Roxy.Demos.InterfaceImpl
{
    public interface IPerson
    {
        string FirstName { get; set; }

        string LastName { get; set; }

        int Age { get; set; }

        string Profession { get; set; }
    }
}
