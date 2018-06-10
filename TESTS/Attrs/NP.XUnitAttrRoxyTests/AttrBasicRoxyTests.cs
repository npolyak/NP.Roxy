using NP.Concepts.Behaviors;
using NP.Roxy;
using NP.Roxy.Attributes;
using NP.Roxy.TypeConfigImpl;
using TestProj;
using Xunit;
using static NP.XUnitAttrRoxyTests.BasicTests.AttrBasicRoxyTests;

namespace NP.XUnitAttrRoxyTests.BasicTests
{
    public static class MyDataUtils
    {
        private static string GetFullName(this StaticMethodsTests.MyData myData)
        {
            return myData.FirstName + " " + myData.LastName;
        }
    }

    public static class AttrBasicRoxyTests
    {
        [Collection("Sequential")]
        public static class InterfaceWrapperImplementation
        {
            [ImplementationClassName("MyImplementationClass")]
            public class WrapperInterface
            {

                [PullMember(nameof(IMyInterface.TheInt), nameof(MyClass.MyInt))]
                [PullMember(nameof(IMyInterface.TheStr), nameof(MyClass.MyStr))]
                public MyClass TheClass { get; }
            }

            [Fact]
            public static void RunTest()
            {

                IMyInterface myInterfaceObj = Core.CreateImplementedInstance<IMyInterface, WrapperInterface>();

                myInterfaceObj.TheInt = 1234;
                myInterfaceObj.TheStr = "Hello";
                string str = myInterfaceObj.GetResultingStr("blabla", 123);

                Core.Save("GeneratedCode");

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

                ISelectableData myInterfaceObj =
                    Core.CreateImplementedInstance<ISelectableData, MyData, SelectableWrapperInterface>();

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

            [ImplementationClassName("TheImplementaion")]
            public interface WrapperInterface
            {
                MyDataImplementorClass TheClass { get; }
            }

            [ImplementationClassName("TheImplementaion1")]
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

                #region WRAPPED CLASS CONCRETIZATION WITH INHERITANCE FROM ABSTRACT CLASS
                IMyData myData = Core.CreateImplementedInstance<IMyData, MyData, WrapperInterface>();
                myData.FirstName = "Joe";
                myData.LastName = "Doe";

                Assert.Equal("Doe, Joe", myData.GetFullName());

                #endregion WRAPPED CLASS CONCRETIZATION WITH INHERITANCE FROM ABSTRACT CLASS

                IMyData myData1 = Core.CreateImplementedInstance<IMyData, WrapperInterface1>(); //Core.GetInstanceOfGeneratedType<IMyData>("MyType9");

                myData1.FirstName = "Joe";
                myData1.LastName = "Doe";

                Core.Save("GeneratedCode");

                Assert.Equal("Doe, Joe", myData1.GetFullName());

                IMyData myData2 = Core.CreateImplementedInstance<IMyData, MyData, WrapperInterface>();

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
                [PullMember(nameof(IMyData.LastName), nameof(MyData.TheLastName), true)]
                [PullMember(nameof(IMyData.GetFullName), nameof(IMyData.GetFullName), true)]
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
                IMyData myData = Core.CreateImplementedInstance<IMyData, WrapperInterface>();
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
                [PullMember(nameof(IMyData.GetFullName), OverrideVirtual = true)]
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
                //ITypeConfig typeConfig =
                //    Core.FindOrCreateTypeConfig<OverridingVirtualsTest.IMyData, OverridingVirtualsTest.MyData, OverridingVirtualsTest.WrapperInterface>("MyType3");

                //typeConfig.SetOverrideVirtual(nameof(MyData.GetFullName), false);

                //typeConfig.ConfigurationCompleted();

                IMyData myData = Core.CreateImplementedInstance<IMyData, MyData, WrapperInterface>();
                    //Core.GetInstanceOfGeneratedType<OverridingVirtualsTest.MyData>("MyType3");
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
                [StaticClass(typeof(MyDataUtils))]
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
            [ImplementationClassName("MyClass123")]
            public interface WrapperInterface
            {
                [PullMember(nameof(IMyInterface.TheInt), nameof(MyClass.MyInt))]
                MyClass TheClass { get; set; }
            }

            [Fact]
            public static void RunTest()
            {
                IMyInterface myObj = Core.CreateImplementedInstance<IMyInterface, WrapperInterface>(); 

                myObj.TheInt = 123;

                Assert.Equal("The resulting string is: blabla_123", myObj.GetResultingStr("blabla", 123));
            }
        }
    }


    public enum ProductKind
    {
        Grocery,
        FinancialInstrument,
        Information
    }

    public static class ProductKindExtensions
    {
        // returns a displayable short name for the ProductKind
        public static string GetDisplayName(this ProductKind productKind)
        {
            switch (productKind)
            {
                case ProductKind.Grocery:
                    return "Grocery";
                case ProductKind.FinancialInstrument:
                    return "Financial Instrument";
                case ProductKind.Information:
                    return "Information";
            }

            return null;
        }

        // returns the full description of the ProductKind
        // note that the method is private
        private static string GetDescription(this ProductKind productKind)
        {
            switch (productKind)
            {
                case ProductKind.Grocery:
                    return "Products you can buy in a grocery store";
                case ProductKind.FinancialInstrument:
                    return "Products you can buy on a stock exchange";
                case ProductKind.Information:
                    return "Products you can get on the Internet";
            }

            return null;
        }
    }


    [Collection("Sequential")]
    public static class EnumToInterfaceTest
    {
        public interface IProduct
        {
            string GetDisplayName();

            string GetDescription();
        }

        internal interface ProductKindWrapper
        {
            [ConstructorInit]
            [PullMember(WrapperMemberName = nameof(IProduct.GetDescription), WrappedMemberName = "GetDescription", AllowNonPublic = true)]
            [StaticClass(typeof(ProductKindExtensions))]
            ProductKind TheProductKind { get; }
        }


        [Fact]
        public static void RunTest()
        {
            Core.SetSaveOnErrorPath("GeneratedCode");

            IProduct product = Core.CreateImplementedInstance<IProduct, ProductKindWrapper>(ProductKind.FinancialInstrument);

            Assert.Equal(ProductKind.FinancialInstrument.GetDisplayName(), product.GetDisplayName());
            Assert.Equal("Products you can buy on a stock exchange", product.GetDescription());
        }
    }


}
