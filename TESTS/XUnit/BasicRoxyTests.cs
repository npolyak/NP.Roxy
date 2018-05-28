using NP.Roxy;
using NP.Roxy.TypeConfigImpl;
using TestProj;
using Xunit;

namespace NP.XUnitRoxyTests
{
    public interface WrapperInterface
    {
        MyClass TheClass { get; }
    }

    public class BasicRoxyTests
    {
        [Fact]
        public void Test1()
        {
            ITypeConfig typeConfig =
                Core.FindOrCreateTypeConfig<IMyInterface, WrapperInterface>("MyGeneratedClass");


            typeConfig.SetMemberMap(nameof(WrapperInterface.TheClass), nameof(MyClass.MyInt), nameof(IMyInterface.TheInt));
            typeConfig.SetMemberMap(nameof(WrapperInterface.TheClass), nameof(MyClass.MyStr), nameof(IMyInterface.TheStr));

            typeConfig.ConfigurationCompleted();

            IMyInterface myInterfaceObj = Core.GetInstanceOfGeneratedType<IMyInterface>(typeConfig.ClassName);

            myInterfaceObj.TheInt = 1234;
            myInterfaceObj.TheStr = "Hello";
            string str = myInterfaceObj.GetResultingStr("blabla", 123);

            Assert.Equal("The resulting string is: blabla_1234", str);
        }
    }
}
