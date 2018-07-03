using Microsoft.CodeAnalysis;
using NP.Roxy;
using NP.Roxy.Attributes;
using NP.Roxy.TypeConfigImpl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NP.XUnitRoxyTests.RoslynTests
{
    [Collection("Sequential")]
    public static class RoslynAttributeTests
    {
        public interface IPerson
        {
            string FirstName { get; }
        }

        public class Person
        {
            public string FirstName { get; set; }
        }

        public class MyImplementor
        {
            [Plugin(typeof(Person), 23, ClassMemberType.Method)]
            public IPerson Person1 { get; set; }

            [Plugin(ImplementorType = typeof(Person))]
            public IPerson Person2 { get; set; }
        }

        [Fact]
        public static void RunTest()
        {
            ITypeConfig typeConfig = Core.FindOrCreateTypeConfig<IPerson, MyImplementor>();

            Compilation compilation =
                Core.TheCore.TheCompilation;

            INamedTypeSymbol namedTypeSymbol = 
                typeConfig.ImplementorTypeSymbol;

            IPropertySymbol person1Symb = 
                namedTypeSymbol.GetMemberByName<IPropertySymbol>(nameof(MyImplementor.Person1));

            AttributeData attrData = person1Symb.GetAttrSymbol(typeof(PluginAttribute));

            PluginAttribute attrObj = attrData.GetAttrObject<PluginAttribute>();
        }
    }
}
