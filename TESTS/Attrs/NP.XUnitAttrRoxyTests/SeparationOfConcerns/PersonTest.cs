using NP.Roxy;
using NP.Roxy.Attributes;
using System;
using Xunit;
using Xunit.Abstractions;

namespace NP.XUnitAttrRoxyTests.PersonTest
{
    public interface IPerson
    {
        string Name { get; set; }

        void Walk();
    }

    public class Person
    {
        public string Name { get; set; }

        public void Walk()
        {
            Console.WriteLine($"The Person {Name} is walking");
        }
    }

    [Collection("Sequential")]
    public class Test : XUnitTestBase
    {
        public Test(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {

        }

        [ImplementationClassName("PersonImplementation")]
        public interface IPersonWrapper
        {
             Person ThePerson { get; }
        }

        [Fact]
        public void RunPersonTest()
        {
            Core.SetSaveOnErrorPath("GeneratedCode");

            IPerson personImplementation =
                Core.CreateImplementedInstance<IPerson, IPersonWrapper>();

            personImplementation.Name = "Bruce";

            personImplementation.Walk();

            Core.Save("GeneratedCode");
        }
    }
}
