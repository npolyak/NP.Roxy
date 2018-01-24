using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NP.Roxy.TypeImplTest
{
    public class MyDataImplementorClass_Concretization : MyDataImplementorClass
    {
        public Func<string> GetFirstNameDelegate { get; set; }
        public override string FirstName => throw new NotImplementedException();

        public override string LastName => throw new NotImplementedException();

        public override int Age => throw new NotImplementedException();

        public Func<int, string, int> DoSmthImpl { get; set; }
        public int DoSomth(int i, string str)
        {
            return DoSmthImpl(i, str);
        }
    }
}
