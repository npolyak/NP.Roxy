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

namespace NP.Roxy.InterfaceImplTest
{
    public interface WrapperInterface
    {
        MyClass TheClass { get; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            ITypeConfig typeConfig =
                Core.FindOrCreateTypeConfig<IMyInterface, WrapperInterface>("MyGeneratedClass");

            typeConfig.SetPropMap(nameof(WrapperInterface.TheClass), nameof(MyClass.MyInt), nameof(IMyInterface.TheInt));
            typeConfig.SetPropMap(nameof(WrapperInterface.TheClass), nameof(MyClass.MyStr), nameof(IMyInterface.TheStr));

            typeConfig.ConfigurationCompleted();

            IMyInterface myInterfaceObj = Core.GetInstanceOfGeneratedType<IMyInterface>(typeConfig.ClassName);

            myInterfaceObj.TheInt = 1234;
            myInterfaceObj.TheStr = "Hello";
            myInterfaceObj.MyMethod("blabla", 123);
        }
    }
}
