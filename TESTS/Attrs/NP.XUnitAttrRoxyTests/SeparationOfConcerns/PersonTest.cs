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

        // generate class called PersonImplementation
        [ImplementationClassName("PersonImplementation1")]
        public abstract class PersonImplementor
        {
            //[PullMember(WrappedMemberName = "PersonName", WrapperMemberName = "Name")]
            [Plugin]
            protected abstract Person ThePersonPlugin { get; set; }

            //public void Walk()
            //{
            //    Console.WriteLine($"Overridden The Person {ThePersonPlugin.Name} is walking");
            //}
        }

        [Fact]
        public void RunPersonTest()
        {
            // make Roxy save the generated code under
            // <Executable>/GeneratedCode folder
            // in case of Roxy error
            Core.SetSaveOnErrorPath("GeneratedCode");

            // make roxy generate IPerson implementation
            // based on PersonImplementor
            IPerson person =
                Core.CreateImplementedInstance<IPerson, PersonImplementor>();

            // set name to Bruce on the object
            // of the Roxy generated class
            person.Name = "Bruce";

            // call method work on the object
            // of the Roxy generated class
            person.Walk();

            // make Roxy save the generated code under
            // <Executable>/GeneratedCode folder
            // in case of successful completion
            Core.Save("GeneratedCode");
        }
    }
}
