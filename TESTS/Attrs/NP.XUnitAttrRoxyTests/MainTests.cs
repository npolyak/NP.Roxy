using NP.Concepts.Behaviors;
using NP.Roxy;
using NP.Roxy.TypeConfigImpl;
using TestProj;
using Xunit;
using static NP.XUnitAttrRoxyTests.MainTests;

namespace NP.XUnitAttrRoxyTests
{
    public static class MyDataUtils
    {
        private static string GetFullName(this StaticMethodsTests.MyData myData)
        {
            return myData.FirstName + " " + myData.LastName;
        }
    }

    public static class MainTests
    {
        [Collection("Sequential")]
        public static class InterfaceWrapperImplementation
        {
            public interface WrapperInterface
            {
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

                IMyInterface myInterfaceObj = Core.GetInstanceOfGeneratedType<IMyInterface>(typeConfig.ClassName);

                myInterfaceObj.TheInt = 1234;
                myInterfaceObj.TheStr = "Hello";
                string str = myInterfaceObj.GetResultingStr("blabla", 123);

                Assert.Equal("The resulting string is: blabla_1234", str);
            }
        }

        [Collection("Sequential")]
        public static class SelectableDataTest
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

            public interface SelectableWrapperInterface
            {
                SelectableItem<ISelectableData> TheSelectableItem { get; }
            }

            [Fact]
            public static void RunTest()
            {
                Core.SetSaveOnErrorPath("GeneratedCode");

                ITypeConfig typeConfig =
                    Core.FindOrCreateTypeConfig<ISelectableData, MyData, SelectableWrapperInterface>();

                typeConfig.SetEventArgThisIdx(nameof(ISelectableData.IsSelectedChanged), 0);

                typeConfig.ConfigurationCompleted();

                ISelectableData myInterfaceObj =
                    Core.GetInstanceOfGeneratedType<ISelectableData>(typeConfig.ClassName) as ISelectableData;

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
        public static class ComplexTypeImplTest
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
                MyDataImplementorClass TheClass { get; }
            }

            [Fact]
            public static void RunTest()
            {
                ITypeConfig typeConfig =
                    Core.FindOrCreateTypeConfig<MyDataImplementorClass, NoType, NoType>("MyGeneratedClass2");

                typeConfig.SetPropBuilder
                (
                    DelegatePropBuilder.TheDelegatePropBuilder,
                    nameof(MyDataImplementorClass.FirstName),
                    nameof(MyDataImplementorClass.LastName),
                    nameof(MyDataImplementorClass.Age)
                 );

                typeConfig.SetMethodBuilder(DelegateMethodBuilder.TheDelegateMethodBuilder, nameof(MyDataImplementorClass.DoSmth));

                typeConfig.ConfigurationCompleted();

                MyDataImplementorClass dataImplementor = Core.GetInstanceOfGeneratedType<MyDataImplementorClass>();
                Core.Save("GeneratedCode");
            }
        }

        [Collection("Sequential")]
        public static class EmbeddingConcretizationTest
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

            public interface WrapperInterface
            {
                MyDataImplementorClass TheClass { get; }
            }

            public interface WrapperInterface1
            {
                MyData MyData { get; }
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
                ITypeConfig<IMyData, WrapperInterface> typeConfig =
                    Core.FindOrCreateTypeConfig<IMyData, MyData, WrapperInterface>("MyType7");

                typeConfig.ConfigurationCompleted();

                MyData myData = Core.GetInstanceOfGeneratedType<MyData>("MyType7");
                myData.FirstName = "Joe";
                myData.LastName = "Doe";

                Assert.Equal("Doe, Joe", myData.GetFullName());

                #endregion WRAPPED CLASS CONCRETIZATION WITH INHERITANCE FROM ABSTRACT CLASS

                ITypeConfig typeConfig1 =
                    Core.FindOrCreateTypeConfig<IMyData, WrapperInterface1>("MyType9");

                typeConfig1.ConfigurationCompleted();

                IMyData myData1 = Core.GetInstanceOfGeneratedType<IMyData>("MyType9");

                myData1.FirstName = "Joe";
                myData1.LastName = "Doe";

                Core.Save("GeneratedCode");

                Assert.Equal("Doe, Joe", myData1.GetFullName());

                ITypeConfig typeConfig2 =
                    Core.FindOrCreateTypeConfig<IMyData, MyData, WrapperInterface>("MyType2");

                typeConfig2.ConfigurationCompleted();

                IMyData myData2 = Core.GetInstanceOfGeneratedType<IMyData>("MyType2");

                myData2.FirstName = "Joe";
                myData2.LastName = "Doe";

                Assert.Equal("Doe, Joe", myData2.GetFullName());
            }
        }

        [Collection("Sequential")]
        public static class NonPublicMembersTest
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
            public interface WrapperInterface
            {
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
                    Core.FindOrCreateTypeConfig<IMyData, NoType, WrapperInterface>("MyType10");

                typeConfig.SetMemberMap
                (
                    nameof(WrapperInterface.TheClass),
                    "TheLastName",
                    nameof(IMyData.LastName),
                    true
                );

                typeConfig.SetMemberMapAllowNonPublic(nameof(WrapperInterface.TheClass), nameof(IMyData.GetFullName));

                typeConfig.ConfigurationCompleted();

                IMyData myData = Core.GetInstanceOfGeneratedType<IMyData>("MyType10");
                myData.FirstName = "Joe";
                myData.LastName = "Doe";

                Assert.Equal("Joe Doe", myData.GetFullName());
                #endregion WRAPPED CLASS CONCRETIZATION WITH INHERITANCE FROM ABSTRACT CLASS
            }
        }

        [Collection("Sequential")]
        public static class OverridingVirtualsTest
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

            public interface WrapperInterface
            {
                OverridingVirtuals_MyDataImplementorClass TheClass { get; }
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
                    Core.FindOrCreateTypeConfig<OverridingVirtualsTest.IMyData, OverridingVirtualsTest.MyData, OverridingVirtualsTest.WrapperInterface>("MyType3");

                typeConfig.SetOverrideVirtual(nameof(MyData.GetFullName), false);

                typeConfig.ConfigurationCompleted();

                OverridingVirtualsTest.MyData myData =
                    Core.GetInstanceOfGeneratedType<OverridingVirtualsTest.MyData>("MyType3");
                myData.FirstName = "Joe";
                myData.LastName = "Doe";

                Assert.Equal("Doe, Joe", myData.GetFullName());
                #endregion WRAPPED CLASS CONCRETIZATION WITH INHERITANCE FROM ABSTRACT CLASS
            }


        }

        [Collection("Sequential")]
        public static class StaticMethodsTests
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
                    nameof(StaticMethodsTests.WrapperInterface.TheClass),
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

                IMyData myData = Core.GetInstanceOfGeneratedType<IMyData>("MyType5");
                myData.FirstName = "Joe";
                myData.LastName = "Doe";

                Assert.Equal("Joe Doe", myData.GetFullName());
                #endregion WRAPPED CLASS CONCRETIZATION WITH INHERITANCE FROM ABSTRACT CLASS
            }
        }


        [Collection("Sequential")]
        public static class TypeImplTest
        {
            public interface WrapperInterface
            {
                MyClass TheClass { get; set; }
            }

            [Fact]
            public static void RunTest()
            {
                ITypeConfig typeConfig =
                    Core.FindOrCreateTypeConfig<IMyInterface, WrapperInterface>("MyGeneratedClass3");
                typeConfig.SetMemberMap(nameof(WrapperInterface.TheClass), nameof(MyClass.MyInt), nameof(IMyInterface.TheInt));

                //typeConfig.SetPropBuilder(DelegatePropBuilder.TheDelegatePropBuilder, nameof(IMyInterface.TheStr));

                typeConfig.ConfigurationCompleted();

                IMyInterface myObj = Core.GetInstanceOfGeneratedType<IMyInterface>("MyGeneratedClass3");

                myObj.TheInt = 123;

                Assert.Equal("The resulting string is: blabla_123", myObj.GetResultingStr("blabla", 123));
            }
        }
    }
}
