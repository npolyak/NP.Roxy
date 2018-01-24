using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NP.Roxy.TypeImplTest
{
    public abstract class MyDataImplementorClass
    {
        public abstract string FirstName { get; }

        public abstract string LastName { get; }

        public abstract int Age { get; }

        public string FullName =>
                LastName + ", " + FirstName;

        public string FirstNameAndAge =>
            FirstName + " " + Age;
    }
}
