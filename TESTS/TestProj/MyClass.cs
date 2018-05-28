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

namespace TestProj
{
    public class MyClass
    {
        //public event MyDelegate TheEvent = null;

        public event Action<int, string> TheEvent = null;

        public int MyInt { get; set; }

        public string MyStr { get; set; }

        public string GetResultingStr(string str, int i)
        {
            return $"The resulting string is: {str}_{MyInt}";
        }

        public void MyMethod(string str, int i)
        {
            Console.WriteLine();
        }

        public int MethodWithReturn(string str)
        {
            return str?.Length ?? 0;
        }
    }
}
