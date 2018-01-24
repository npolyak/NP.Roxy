// (c) Nick Polyak 2018 - http://awebpros.com/
// License: Apache License 2.0 (http://www.apache.org/licenses/LICENSE-2.0.html)
//
// short overview of copyright rules:
// 1. you can use this framework in any commercial or non-commercial 
//    product as long as you retain this copyright message
// 2. Do not blame the author of this software if something goes wrong. 
// 
// Also, please, mention this software in any documentation for the 
// products that use it.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NP.Roxy.TypeImplTest
{
    public class MyDataImplementorClass_Concretization : MyDataImplementorClass
    {
        public Func<string> FirstName_Delegate { get; set; }
        public override string FirstName => FirstName_Delegate();

        public Func<string> LastName_Delegate { get; set; }
        public override string LastName => LastName_Delegate();

        Func<int> Age_Delegate { get; set; }
        public override int Age => Age_Delegate();

        public Func<int, string, int> DoSmth_Delegate { get; set; }
        public override int DoSmth(int i, string str)
        {
            return DoSmth_Delegate(i, str);
        }
    }
}
