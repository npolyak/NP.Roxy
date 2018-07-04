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
            [Plugin(ImplementorType = typeof(Person))]
            public IPerson ThePerson { get; set; }
        }

        [Fact]
        public static void RunTest()
        {
            Core.SetSaveOnErrorPath("GeneratedCode");
            //ITypeConfig typeConfig = Core.FindOrCreateTypeConfig<IPerson, MyImplementor>();
            //typeConfig.ConfigurationCompleted();
            //IPerson person = 
            //    typeConfig.CreateInstanceOfType<IPerson>();

            Core.Save("GeneratedCode");
        }
    }
}
