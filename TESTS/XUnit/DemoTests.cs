using NP.Roxy;
using NP.Roxy.TypeConfigImpl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace XUnitRoxyTests
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

        [Fact]
        public static void RunTest()
        {
            Core.SetSaveOnErrorPath("GeneratedCode");

            // we create an adaptor adapting ProductKind enumeration
            // to IProduct interface using extension methods from the static 
            // ProductKindExtensions class
            Core.CreateEnumerationAdapter<IProduct, ProductKind>(typeof(ProductKindExtensions));

            // enumeration value ProductKind.FinancialInstrument is converted into
            // IProduct interface
            IProduct product =
                Core.CreateEnumWrapper<IProduct, ProductKind>(ProductKind.FinancialInstrument);

            Assert.Equal(ProductKind.FinancialInstrument.GetDisplayName(), product.GetDisplayName());
            Assert.Equal("Products you can buy on a stock exchange", product.GetDescription());
        }
    }


    [Collection("Sequential")]
    public static class InterfaceImplTest
    {
        public interface IPerson
        {
            string FirstName { get; set; }

            string LastName { get; set; }

            int Age { get; set; }

            string Profession { get; set; }
        }

        [Fact]
        public static void RunTest()
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

    [Collection("Sequential")]
    public static class PropChangedInterfaceImplTest
    {
        public interface IPerson : INotifyPropertyChanged
        {
            string FirstName { get; set; }

            string LastName { get; set; }

            int Age { get; set; }

            string Profession { get; set; }
        }

        [Fact]
        public static void RunTest()
        {
            ITypeConfig typeConfig = Core.FindOrCreateTypeConfigByTypeToImpl<IPerson>("Person");

            typeConfig.SetEventBuilder(PropertyChangedEventBuilder.ThePropertyChangedEventBuilder, "PropertyChanged");

            typeConfig.SetPropBuilder
            (
                PropertyChangedPropBuilder.ThePropertyChangedPropBuilder,
                nameof(IPerson.Age),
                nameof(IPerson.Profession)
            );

            typeConfig.ConfigurationCompleted();

            IPerson person = Core.GetInstanceOfGeneratedType<IPerson>("Person");

            person.FirstName = "Joe";

            person.LastName = "Doe";

            person.Age = 35;

            person.Profession = "Astronaut";

            Console.WriteLine($"Name='{person.FirstName} {person.LastName}'; Age='{person.Age}'; Profession='{person.Profession}'");

            Core.Save("GeneratedCode");
        }
    }

    [Collection("Sequential")]
    public static class WrappersTest
    {
        public interface IPerson
        {
            string FirstName { get; set; }

            string LastName { get; set; }

            int Age { get; set; }

            string Profession { get; set; }

            string GetFullNameAndProfession();
        }

        public class PersonImpl
        {
            public string FirstName { get; set; }

            private string LastName { get; set; }

            private int Age { get; set; }

            private string TheProfession { get; set; }

            private string GetFullNameAndProfession()
            {
                return $"{FirstName} {LastName} - {TheProfession}";
            }
        }

        public interface PersonImplementationWrapperInterface
        {
            PersonImpl ThePersonImplementation { get; }
        }

        [Fact]
        public static void RunTest()
        {
            #region create the generated type configuration object
            // get the type configuration object. The class that it is going to generate
            // will be called "MyPersonImplementation"
            ITypeConfig typeConfig =
                Core.FindOrCreateTypeConfig<IPerson, PersonImplementationWrapperInterface>("MyPersonImplementation");

            // allow access to non-public members of 
            // PersonImplementationWrapperInterface.ThePersonImplementation object.
            typeConfig.SetAllowNonPublicForAllMembers
            (
                nameof(PersonImplementationWrapperInterface.ThePersonImplementation)
            );

            // map TheProfession property of the wrapped object
            // into Profession property of the IPerson interface.
            typeConfig.SetMemberMap
            (
                nameof(PersonImplementationWrapperInterface.ThePersonImplementation),
                "TheProfession",
                nameof(IPerson.Profession)
            );

            // Signal that the configuration is completed, 
            // after ConfigurationCompleted() method is called
            // TypeConfig object for this class cannot be modified.
            typeConfig.ConfigurationCompleted();
            #endregion create the generated type configuration object

            // get the instance of the generated type "MyPersonImplementation"
            IPerson person =
                Core.GetInstanceOfGeneratedType<IPerson>("MyPersonImplementation");

            //IPerson person = Core.CreateWrapperWithNonPublicMembers<IPerson, PersonImplementationWrapperInterface>("MyPersonImplementation");

            // set the properties
            person.FirstName = "Joe";

            person.LastName = "Doe";

            person.Age = 35;

            person.Profession = "Astronaut";

            // test that the wrapped properties and the method work
            Console.WriteLine($"Name/Profession='{person.GetFullNameAndProfession()}'; Age='{person.Age}'");
        }
    }


}