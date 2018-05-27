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

using NP.Roxy.TypeConfigImpl;
using System;

namespace NP.Roxy.MethodOverloadingTest
{


    public interface IWrapper
    {
        MyDataImpl TheDataImpl { get; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Core.SetSaveOnErrorPath("GeneratedCode");

            ITypeConfig<IMyData, IWrapper> typeConfig = Core.FindOrCreateTypeConfig<IMyData, IWrapper>();

            typeConfig.ConfigurationCompleted();

            IMyData myData = Core.GetInstanceOfGeneratedType<IMyData>();

            myData.FirstName = "Joe";
            myData.LastName = "Doe";

            string greetingStr1 = myData.GetGreeting();
            Console.WriteLine(greetingStr1);

            string greetingStr2 = myData.GetGreeting("Hello");

            Console.WriteLine(greetingStr2);

            Core.Save("GeneratedCode");
        }
    }
}
