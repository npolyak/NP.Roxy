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
        public string PersonName { get; set; }

        public void Walk()
        {
            Console.WriteLine($"&&&&The Person {PersonName} is walking");
        }
    }

    [Collection("Sequential")]
    public class Test : XUnitTestBase
    {
        public Test(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {

        }

        [ImplementationClassName("PersonImplementation")]
        public abstract class PersonImplementor
        {
            [PullMember(WrappedMemberName = "PersonName", WrapperMemberName = "Name")]
            [Plugin]
            protected abstract Person ThePerson { get; set; }

            public void Walk()
            {
                Console.WriteLine($"Overridden The Person {ThePerson.PersonName} is walking");
            }
        }

        [Fact]
        public void RunPersonTest()
        {
            Core.SetSaveOnErrorPath("GeneratedCode");

            IPerson personImplementation =
                Core.CreateImplementedInstance<IPerson, PersonImplementor>();

            Core.Save("GeneratedCode");

            personImplementation.Name = "Bruce";

            personImplementation.Walk();
        }
    }
}
