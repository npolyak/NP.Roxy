using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NP.Roxy.Tests.StaticMethods
{
    public class MyData
    {
        public string LastName { get; set; }
        public string FirstName { get; set; }

        public int Age { get; set; }
    }

    public static class MyDataUtils
    {
        private static string GetFullName(this MyData myData)
        {
            return myData.FirstName + " " + myData.LastName;
        }
    }
}
