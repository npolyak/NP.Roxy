using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NP.Roxy.StrongTypeMethodTest
{
    public interface IMyData 
    {
        string FirstName { get; set; }

        string LastName { get; set; }

        string GetGreeting(string greetingMessage);
    }
}
