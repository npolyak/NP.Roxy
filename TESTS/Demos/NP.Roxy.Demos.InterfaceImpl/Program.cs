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

using Microsoft.CodeAnalysis;
using NP.Roxy;
using NP.Roxy.TypeConfigImpl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TestProj;

namespace NP.Roxy.Demos.InterfaceImpl
{
    class Program
    {
        static void Main(string[] args)
        {            
            // if there is a compiler error
            // all the generated code will be dumped
            // into "GeneratedCode" folder located within 
            // the directory containing the executable
            Core.SetSaveOnErrorPath("GeneratedCode");

            // get default implementation of IPerson
            // interface containing only propertys
            // the default property implementatio
            // is the auto property
            IPerson person = Core.Concretize<IPerson>();

            person.FirstName = "Joe";

            person.LastName = "Doe";

            person.Age = 35;

            person.Profession = "Astronaut";

            // test that the properties have indeed been assigned. 
            Console.WriteLine($"Name='{person.FirstName} {person.LastName}'; Age='{person.Age}'; Profession='{person.Profession}'");

            // dump all the generated code into 
            // "GeneratedCode" folder located within 
            // the directory containing the executable
            Core.Save("GeneratedCode");
        }
    }
}
