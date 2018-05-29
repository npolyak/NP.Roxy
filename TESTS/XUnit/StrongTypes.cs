using NP.Roxy;
using NP.Roxy.TypeConfigImpl;
using Xunit;

namespace XUnitRoxyTests.StrongTypes
{
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
        public static string GetDescription(this ProductKind productKind)
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
    public static class CompositeGettersTest
    {
        public interface IMyData
        {
            string LastName { get; set; }

            string FirstName { get; set; }

            int Age { get; set; }

            string FullName { get; }
        }

        public abstract class MyDataImplementorClass
        {
            public abstract string FirstName { get; }

            public abstract string LastName { get; }

            public string FullName1
            {
                get => $"{LastName}, {FirstName}";
            }
        }

        public interface IMyDataImplWrapper
        {
            MyDataImplementorClass DataImplementor { get; }
        }

        [Fact]
        public static void RunTest()
        {
            #region Prop Getter 
            ITypeConfig<IMyData, NoType> typeConfig =
                Core.FindOrCreateTypeConfig<IMyData, NoType>();

            typeConfig.SetPropGetter<string>
            (
                (data) => data.FullName,
                (data) => data.LastName + ", " + data.FirstName
            );

            typeConfig.ConfigurationCompleted();

            IMyData myData = Core.GetInstanceOfGeneratedType<IMyData>();

            myData.FirstName = "Joe";
            myData.LastName = "Doe";

            Assert.Equal("Doe, Joe", myData.FullName);

            #endregion Prop Getter 
        }
    }

    [Collection("Sequential")]
    public static class MethodOverloadingTest
    {
        public interface IMyData
        {
            string FirstName { get; set; }

            string LastName { get; set; }

            // demonstrates method overloading
            string GetGreeting();

            // demonstrates method overloading
            string GetGreeting(string greetingMessage);
        }

        public abstract class MethodOverloadingMyDataImpl
        {
            public abstract string FirstName { get; }

            public abstract string LastName { get; }

            // demonstrates method overloading
            public string GetGreeting() => "Hello World!";

            // demonstrates method overloading
            public string GetGreeting(string greetingMessage)
            {
                return $"{greetingMessage} {FirstName} {LastName}!";
            }
        }

        public interface IWrapper
        {
            MethodOverloadingMyDataImpl TheDataImpl { get; }
        }

        [Fact]
        public static void RunTest()
        {
            Core.SetSaveOnErrorPath("GeneratedCode");

            ITypeConfig<IMyData, IWrapper> typeConfig = Core.FindOrCreateTypeConfig<IMyData, IWrapper>("MyData1");

            typeConfig.ConfigurationCompleted();

            IMyData myData = Core.GetInstanceOfGeneratedType<IMyData>("MyData1");

            myData.FirstName = "Joe";
            myData.LastName = "Doe";

            string greetingStr1 = myData.GetGreeting();

            Assert.Equal("Hello World!", greetingStr1);

            string greetingStr2 = myData.GetGreeting("Hello");

            Assert.Equal("Hello Joe Doe!", greetingStr2);
        }
    }

    [Collection("Sequential")]
    public static class StrongTimeEnumTest
    {
        public interface IProduct
        {
            string DisplayName { get; }

            string Description { get; }
        }

        [Fact]
        public static void RunTest()
        {
            ITypeConfig<IProduct, SingleWrapperInterface<ProductKind>> adapterTypeConfig =
                Core.FindOrCreateSingleWrapperTypeConfig<IProduct, ProductKind>();

            adapterTypeConfig.SetWrappedPropGetter<IProduct, ProductKind, string>
            (
                prod => prod.DisplayName,
                prodKind => prodKind.GetDisplayName()
            );

            adapterTypeConfig.SetWrappedPropGetter<IProduct, ProductKind, string>
            (
                prod => prod.Description,
                prodKind => prodKind.GetDescription()
            );

            adapterTypeConfig.ConfigurationCompleted();

            IProduct product = Core.GetInstanceOfGeneratedType<IProduct>(null, ProductKind.Information);

            Assert.Equal(ProductKind.Information.GetDescription(), product.Description);
            Assert.Equal(ProductKind.Information.GetDisplayName(), product.DisplayName);
        }
    }

    [Collection("Sequential")]
    public static class StrongTypeMethodTest
    {
        public interface IMyData
        {
            string FirstName { get; set; }

            string LastName { get; set; }

            string GetGreeting(string greetingMessage);
        }

        public abstract class StrongMethodTestMyDataImpl
        {
            public abstract string FirstName { get; }

            public abstract string LastName { get; }

            public string GetGreeting() => "Hello World!";

            public string GetGreetingImpl(string greetingMessage)
            {
                return $"{greetingMessage} {FirstName} {LastName}!";
            }
        }


        public interface IWrapper
        {
            StrongMethodTestMyDataImpl TheDataImpl { get; }
        }

        [Fact]
        public static void RunTest()
        {

            ITypeConfig<IMyData, IWrapper> typeConfig = 
                Core.FindOrCreateTypeConfig<IMyData, IWrapper>("MyData2");

            typeConfig.SetReturningMethodMap<StrongMethodTestMyDataImpl, string, string>
            (
                (data, inputStr) => data.GetGreeting(inputStr),
                (wrapper) => wrapper.TheDataImpl,
                (dataImpl, inputStr) => dataImpl.GetGreetingImpl(inputStr)
            );

            typeConfig.ConfigurationCompleted();

            IMyData myData = Core.GetInstanceOfGeneratedType<IMyData>("MyData2");

            myData.FirstName = "Joe";
            myData.LastName = "Doe";

            string greetingStr = myData.GetGreeting("Hello");

            Assert.Equal("Hello Joe Doe!", greetingStr);
        }
    }
}
