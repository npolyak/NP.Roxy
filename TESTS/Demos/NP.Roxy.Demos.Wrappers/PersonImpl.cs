using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NP.Roxy.Demos.Wrappers
{
    public class PersonImpl
    {
        public string FirstName { get; set; }

        private string LastName { get; set; }

        private int Age { get; set; }

        private string TheProfession { get; set; }

        private string GetFullNameAndProfession()
        {
            return $"{FirstName} {LastName} - {TheProfession}";
        }
    }
}
