using NP.Roxy;
using NP.Roxy.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace NP.XUnitAttrRoxyTests.PersonWithAbstractMethodTest
{
    public interface ILog
    {
        void Log(string message);
    }

    public class ConsoleLogger : ILog
    {
        public void Log(string message)
        {
            Console.WriteLine($"----------{message}----------");
        }
    }

    public class AnotherConsoleLogger : ILog
    {
        public void Log(string message)
        {
            Console.WriteLine($"**********{message}**********");
        }
    }

    public interface IPerson
    {
        string Name { get; set; }

        void Walk();
    }

    public abstract class Person
    {
        public string Name { get; set; }

        protected abstract void Log(string message);

        public void Walk()
        {
            Log("Entering Walk");
            Console.WriteLine($"The Person {Name} is walking");
            Log("Exiting Walk");
        }
    }

    [Collection("Sequential")]
    public class Test : XUnitTestBase
    {
        public Test(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {

        }

        [ImplementationClassName("PersonImplementation")]
        public interface IPersonImplementor
        {
            [Plugin]
            Person ThePerson { get;  }

            [Plugin(InitType = typeof(AnotherConsoleLogger))]
            //[Plugin(InitType = typeof(ConsoleLogger))]
            ILog TheLog { get; }
        }

        [Fact]
        public void RunPersonTest()
        {
            Core.SetSaveOnErrorPath("GeneratedCode");

            IPerson personImplementation =
                Core.CreateImplementedInstance<IPerson, IPersonImplementor>();

            Core.Save("GeneratedCode");

            personImplementation.Name = "Bruce";

            personImplementation.Walk();
        }
    }
}
