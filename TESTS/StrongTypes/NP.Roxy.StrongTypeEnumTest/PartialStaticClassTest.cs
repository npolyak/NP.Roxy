using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NP.Roxy.OverloadingTest
{
    public static partial class PartialStaticClassTest
    {
        public static string GetFullName(string firstName, string lastName)
        {
            return firstName + " " + lastName;
        }
    }

    public static partial class PartialStaticClassTest
    {

    }
}
