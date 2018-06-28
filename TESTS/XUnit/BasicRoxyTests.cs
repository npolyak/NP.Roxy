using NP.Concepts.Behaviors;
using NP.Roxy;
using NP.Roxy.Attributes;
using NP.Roxy.TypeConfigImpl;
using System;
using TestProj;
using Xunit;

namespace NP.XUnitRoxyTests.Basic
{
    [Collection("Sequential")]
    public static class A_InterfaceWrapperImplementation
    {
        public interface WrapperInterface
        {
            [Plugin]
            MyClass TheClass { get; }
        }

        [Fact]
        public static void RunTest()
        {

            ITypeConfig typeConfig =
                Core.FindOrCreateTypeConfig<IMyInterface, WrapperInterface>("MyGeneratedClass1");
            typeConfig.SetMemberMap(nameof(WrapperInterface.TheClass), nameof(MyClass.MyInt), nameof(IMyInterface.TheInt));
            typeConfig.SetMemberMap(nameof(WrapperInterface.TheClass), nameof(MyClass.MyStr), nameof(IMyInterface.TheStr));

            typeConfig.ConfigurationCompleted();

            IMyInterface myInterfaceObj = typeConfig.CreateInstanceOfType<IMyInterface>();

            myInterfaceObj.TheInt = 1234;
            myInterfaceObj.TheStr = "Hello";
            string str = myInterfaceObj.GetResultingStr("blabla", 123);

            Assert.Equal("The resulting string is: blabla_1234", str);
        }
    }

    [Collection("Sequential")]
    public static class B_SelectableDataTest
    {
        public interface IMyData
        {
            string FirstName { get; set; }

            string LastName { get; set; }
        }

        public class MyData
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }
        }

        public interface ISelectableData : IMyData, ISelectableItem<ISelectableData>
        {

        }

        public class SelectableWrapperInterface : MyData
        {
            [Plugin]
            protected SelectableItem<ISelectableData> TheSelectableItem { get; }
        }

        [Fact]
        public static void RunTest()
        {
            Core.SetSaveOnErrorPath("GeneratedCode");

            ITypeConfig typeConfig =
                Core.FindOrCreateTypeConfig<ISelectableData, SelectableWrapperInterface>();

            typeConfig.SetEventArgThisIdx(nameof(ISelectableData.IsSelectedChanged), 0);

            typeConfig.ConfigurationCompleted();

            ISelectableData myInterfaceObj =
                typeConfig.CreateInstanceOfType<ISelectableData>();

            myInterfaceObj.FirstName = "Nick";
            myInterfaceObj.LastName = "Polyak";

            bool isSelectedChanged = false;
            myInterfaceObj.IsSelectedChanged += (selectableItem) =>
                {
                    isSelectedChanged = true;
                };

            myInterfaceObj.IsSelected = true;

            Assert.Equal("Nick", myInterfaceObj.FirstName);
            Assert.Equal("Polyak", myInterfaceObj.LastName);

            Assert.True(isSelectedChanged);
        }
    }

    [Collection("Sequential")]
    public static class C_ComplexTypeImplTest
    {
        public abstract class MyDataImplementorClass
        {
            public abstract string FirstName { get; }

            public abstract string LastName { get; }

            public abstract int Age { get; }

            public abstract int DoSmth(int i, string str);
        }

        public interface WrapperInterface
        {
            [Plugin]
            MyDataImplementorClass TheClass { get; }
        }

        [Fact]
        public static void RunTest()
        {
            ITypeConfig typeConfig =
                Core.FindOrCreateTypeConfig<MyDataImplementorClass, NoType>("MyGeneratedClass2");

            typeConfig.SetPropBuilder
            (
                DelegatePropBuilder.TheDelegatePropBuilder,
                nameof(MyDataImplementorClass.FirstName),
                nameof(MyDataImplementorClass.LastName),
                nameof(MyDataImplementorClass.Age)
             );

            typeConfig.SetMethodBuilder(DelegateMethodBuilder.TheDelegateMethodBuilder, nameof(MyDataImplementorClass.DoSmth));

            typeConfig.ConfigurationCompleted();

            MyDataImplementorClass dataImplementor = typeConfig.CreateInstanceOfType<MyDataImplementorClass>();
            Core.Save("GeneratedCode");
        }
    }

    [Collection("Sequential")]
    public static class D_EmbeddingConcretizationTest
    {
        public interface IMyData
        {
            string LastName { get; set; }

            string FirstName { get; set; }

            int Age { get; set; }

            string GetFullName();
        }

        public abstract class MyData : IMyData
        {
            public string LastName { get; set; }
            public string FirstName { get; set; }

            public int Age { get; set; }

            public abstract string GetFullName();
        }

        public abstract class MyDataImplementorClass
        {
            public abstract string FirstName { get; }

            public abstract string LastName { get; }

            public string GetFullName()
            {
                return $"{LastName}, {FirstName}";
            }
        }

        public abstract class ImplementorClass : MyData
        {
            [Plugin]
            protected MyDataImplementorClass TheClass { get; }
        }

        public interface WrapperInterface1
        {
            [Plugin]
            MyData MyData { get; }

            [Plugin]
            MyDataImplementorClass TheClass { get; }
        }


        // default and other implementations test
        // for events - easy
        // for get/set props - default impl Auto props
        // for get (or set) only abstract implementation
        // for methods - abstract impl

        // besides we have PropertyChanged implementation for get;set props
        // also lambda indirection implementations for props and methods
        // and throw not implemented exception for the methods.
        [Fact]
        public static void RunTest()
        {
            Core.SetSaveOnErrorPath("GeneratedCode");

            #region CONCRETIZATION SAMPLE
            //MyDataImplementorClass classConcretization =
            //    Core.Concretize<MyDataImplementorClass>();
            #endregion CONCRETIZATION SAMPLE

            #region WRAPPED CLASS CONCRETIZATION WITH INHERITANCE FROM ABSTRACT CLASS
            ITypeConfig typeConfig =
                Core.FindOrCreateTypeConfig<IMyData, ImplementorClass>("MyType7");

            typeConfig.ConfigurationCompleted();

            MyData myData = typeConfig.CreateInstanceOfType<MyData>();
            myData.FirstName = "Joe";
            myData.LastName = "Doe";

            Assert.Equal("Doe, Joe", myData.GetFullName());

            #endregion WRAPPED CLASS CONCRETIZATION WITH INHERITANCE FROM ABSTRACT CLASS

            ITypeConfig typeConfig1 =
                Core.FindOrCreateTypeConfig<IMyData, WrapperInterface1>("MyType9");

            typeConfig1.ConfigurationCompleted();

            IMyData myData1 = typeConfig1.CreateInstanceOfType<IMyData>();

            myData1.FirstName = "Joe";
            myData1.LastName = "Doe";

            Core.Save("GeneratedCode");

            Assert.Equal("Doe, Joe", myData1.GetFullName());

            ITypeConfig typeConfig2 =
                Core.FindOrCreateTypeConfig<IMyData, ImplementorClass>();

            typeConfig2.ConfigurationCompleted();

            IMyData myData2 = typeConfig2.CreateInstanceOfType<IMyData>();

            myData2.FirstName = "Joe";
            myData2.LastName = "Doe";

            Assert.Equal("Doe, Joe", myData2.GetFullName());
        }
    }

    [Collection("Sequential")]
    public static class E_NonPublicMembersTest
    {
        public interface IMyData
        {
            string LastName { get; set; }

            string FirstName { get; set; }

            int Age { get; set; }

            string GetFullName();
        }

        public class MyData
        {
            internal string TheLastName { get; set; }
            public string FirstName { get; set; }

            public int Age { get; set; }

            internal string GetFullName()
            {
                return $"{FirstName} {TheLastName}";
            }
        }
        public class Implementor
        {
            [Plugin]
            public MyData TheClass { get; }
        }

        // default and other implementations test
        // for events - easy
        // for get/set props - default impl Auto props
        // for get (or set) only abstract implementation
        // for methods - abstract impl

        // besides we have PropertyChanged implementation for get;set props
        // also lambda indirection implementations for props and methods
        // and throw not implemented exception for the methods.
        [Fact]
        public static void RunTest()
        {
            #region WRAPPED CLASS CONCRETIZATION WITH INHERITANCE FROM ABSTRACT CLASS
            Core.SetSaveOnErrorPath("GeneratedCode");
            ITypeConfig typeConfig =
                Core.FindOrCreateTypeConfig<IMyData, Implementor>("MyType10");

            typeConfig.SetMemberMap
            (
                nameof(Implementor.TheClass),
                "TheLastName",
                nameof(IMyData.LastName),
                true
            );

            typeConfig.SetMemberMapAllowNonPublic(nameof(Implementor.TheClass), nameof(IMyData.GetFullName));

            typeConfig.ConfigurationCompleted();

            IMyData myData = typeConfig.CreateInstanceOfType<IMyData>();
            myData.FirstName = "Joe";
            myData.LastName = "Doe";

            Assert.Equal("Joe Doe", myData.GetFullName());
            Core.Save("GeneratedCode");
            #endregion WRAPPED CLASS CONCRETIZATION WITH INHERITANCE FROM ABSTRACT CLASS
        }
    }

    [Collection("Sequential")]
    public static class F_OverridingVirtualsTest
    {
        public interface IMyData
        {
            string LastName { get; set; }

            string FirstName { get; set; }

            int Age { get; set; }

            string GetFullName();
        }

        public abstract class MyData : IMyData
        {
            public string LastName { get; set; }
            public string FirstName { get; set; }

            public int Age { get; set; }

            public virtual string GetFullName()
            {
                return $"{FirstName} {LastName}";
            }
        }

        public abstract class OverridingVirtuals_MyDataImplementorClass
        {
            public abstract string FirstName { get; }

            public abstract string LastName { get; }

            public string GetFullName()
            {
                return $"{LastName}, {FirstName}";
            }
        }

        public class WrapperInterface : MyData
        {
            [Plugin]
            public OverridingVirtuals_MyDataImplementorClass TheClass { get; }
        }

        // default and other implementations test
        // for events - easy
        // for get/set props - default impl Auto props
        // for get (or set) only abstract implementation
        // for methods - abstract impl

        // besides we have PropertyChanged implementation for get;set props
        // also lambda indirection implementations for props and methods
        // and throw not implemented exception for the methods.
        [Fact]
        public static void RunTest()
        {
            #region WRAPPED CLASS CONCRETIZATION WITH INHERITANCE FROM ABSTRACT CLASS
            ITypeConfig typeConfig =
                Core.FindOrCreateTypeConfig<F_OverridingVirtualsTest.IMyData, F_OverridingVirtualsTest.WrapperInterface>("MyType3");

            typeConfig.SetOverrideVirtual(nameof(MyData.GetFullName), false);

            typeConfig.ConfigurationCompleted();

            F_OverridingVirtualsTest.MyData myData =
                typeConfig.CreateInstanceOfType<F_OverridingVirtualsTest.MyData>();
            myData.FirstName = "Joe";
            myData.LastName = "Doe";

            Assert.Equal("Doe, Joe", myData.GetFullName());
            #endregion WRAPPED CLASS CONCRETIZATION WITH INHERITANCE FROM ABSTRACT CLASS
        }


    }


    public static class MyDataUtils
    {
        private static string GetFullName(this G_StaticMethodsTests.MyData myData)
        {
            return myData.FirstName + " " + myData.LastName;
        }
    }

    [Collection("Sequential")]
    public static class G_StaticMethodsTests
    {
        public interface IMyData
        {
            string LastName { get; set; }

            string FirstName { get; set; }

            int Age { get; set; }

            string GetFullName();
        }

        public class MyData
        {
            public string LastName { get; set; }
            public string FirstName { get; set; }

            public int Age { get; set; }
        }
        public interface WrapperInterface
        {
            [Plugin]
            MyData TheClass { get; }
        }

        // default and other implementations test
        // for events - easy
        // for get/set props - default impl Auto props
        // for get (or set) only abstract implementation
        // for methods - abstract impl

        // besides we have PropertyChanged implementation for get;set props
        // also lambda indirection implementations for props and methods
        // and throw not implemented exception for the methods.
        [Fact]
        public static void RunTest()
        {
            #region WRAPPED CLASS CONCRETIZATION WITH INHERITANCE FROM ABSTRACT CLASS
            ITypeConfig typeConfig =
                Core.FindOrCreateTypeConfig<IMyData, WrapperInterface>("MyType5");

            typeConfig.AddStaticUtilsClass
            (
                nameof(G_StaticMethodsTests.WrapperInterface.TheClass),
                typeof(MyDataUtils)
            );

            typeConfig.SetAllowNonPublicForAllMembers(nameof(WrapperInterface.TheClass));

            //typeConfig.SetPropMap
            //(
            //    nameof(WrapperInterface.TheClass), 
            //    "GetTheFullName", 
            //    nameof(IMyData.GetFullName), 
            //    true);

            typeConfig.ConfigurationCompleted();

            IMyData myData = typeConfig.CreateInstanceOfType<IMyData>();
            myData.FirstName = "Joe";
            myData.LastName = "Doe";

            Assert.Equal("Joe Doe", myData.GetFullName());
            #endregion WRAPPED CLASS CONCRETIZATION WITH INHERITANCE FROM ABSTRACT CLASS
        }
    }


    [Collection("Sequential")]
    public static class H_TypeImplTest
    {
        public interface WrapperInterface
        {
            [Plugin]
            MyClass TheClass { get; set; }
        }

        [Fact]
        public static void RunTest()
        {
            Core.SetSaveOnErrorPath("GeneratedCode");
            ITypeConfig typeConfig =
                Core.FindOrCreateTypeConfig<IMyInterface, WrapperInterface>("MyGeneratedClass3");
            typeConfig.SetMemberMap(nameof(WrapperInterface.TheClass), nameof(MyClass.MyInt), nameof(IMyInterface.TheInt));

            //typeConfig.SetPropBuilder(DelegatePropBuilder.TheDelegatePropBuilder, nameof(IMyInterface.TheStr));

            typeConfig.ConfigurationCompleted();

            IMyInterface myObj = typeConfig.CreateInstanceOfType<IMyInterface>();

            myObj.TheInt = 123;

            Core.Save("GeneratedCode");

            Assert.Equal("The resulting string is: blabla_123", myObj.GetResultingStr("blabla", 123));
        }
    }

    [Collection("Sequential")]
    public static class I_BasicClassTest
    {
        public interface IMyData
        {
            string LastName { get; set; }

            string FirstName { get; set; }

            int Age { get; set; }

            string TheFullName { get; }
        }

        public class MyData
        {
            protected string TheLastName { get; set; }
            public string FirstName { get; set; }

            public int Age { get; set; }

            internal string GetFullName()
            {
                return $"{FirstName} {TheLastName}";
            }
        }

        public class Implementor : MyData
        {
            public string LastName
            {
                get => TheLastName;
                set => TheLastName = value;
            }

            public string TheFullName =>
                base.GetFullName();
        }


        [Fact]
        public static void RunTest()
        {
            ITypeConfig typeConfig = Core.FindOrCreateTypeConfig<IMyData, Implementor>("MyType11");

            typeConfig.ConfigurationCompleted();

            IMyData myData = typeConfig.CreateInstanceOfType<IMyData>();

            myData.FirstName = "Nick";
            myData.LastName = "Polyak";
            myData.Age = 120;

            Assert.Equal($"{myData.FirstName} {myData.LastName}", myData.TheFullName);
        }
    }
}
