using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NP.Roxy.TypeImplTest
{
    public abstract class MyData : IMyData
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int Age { get; set; }

        public string Profession { get; set; }

        public virtual string FullName =>
            FirstName + " " + LastName;

        public abstract string FirstNameAndAge { get; }
    }
}
