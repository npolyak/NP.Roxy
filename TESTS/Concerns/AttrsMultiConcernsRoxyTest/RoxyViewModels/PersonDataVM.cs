using NP.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttrsMultiConcernsRoxyTest.RoxyViewModels
{
    public interface IPersonDataVM
    {
        string FirstName { get; set; }

        string LastName { get; set; }

        string FullName { get; }
    }

    public class PersonDataVM : IPersonDataVM
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string FullName => FirstName + " " + LastName;
    }
}
