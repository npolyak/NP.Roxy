using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NP.Roxy.MethodOverloadingTest
{
    public abstract class MyDataImpl
    {
        public abstract string FirstName { get; }

        public abstract string LastName { get; }

        public string GetGreeting() => "Hello World!";

        public string GetGreeting(string greetingMessage)
        {
            return $"{greetingMessage} {FirstName} {LastName}!";
        }
    }
}
