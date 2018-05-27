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

            typeConfig.SetMemberMap(nameof(WrapperInterface.TheClass), nameof(MyClass.MyInt), nameof(IMyInterface.TheInt));
            typeConfig.SetMemberMap(nameof(WrapperInterface.TheClass), nameof(MyClass.MyStr), nameof(IMyInterface.TheStr));

            typeConfig.ConfigurationCompleted();

            IMyInterface myInterfaceObj = Core.GetInstanceOfGeneratedType<IMyInterface>(typeConfig.ClassName);

            myInterfaceObj.TheInt = 1234;
            myInterfaceObj.TheStr = "Hello";
            myInterfaceObj.MyMethod("blabla", 123);
        }
    }
}
