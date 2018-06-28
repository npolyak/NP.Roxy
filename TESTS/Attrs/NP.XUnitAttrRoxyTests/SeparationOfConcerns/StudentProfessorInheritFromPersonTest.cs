using NP.Concepts.Attributes;
using NP.Roxy;
using NP.Roxy.Attributes;
using System;
using Xunit;
using Xunit.Abstractions;

namespace NP.XUnitAttrRoxyTests.StudentProfessorInheritFromPersonTest
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
            Console.WriteLine($"Person {Name} is walking");
        }
    }

    public interface ILearner
    {
        void Learn();
    }

    public interface ITeacher
    {
        void Teach();
    }

    public interface IStudent : IPerson, ILearner
    {

    }

    public interface IProfessor : IPerson, ITeacher
    {

    }

    // learning concern implementation
    public class Student : Person, IStudent
    {
        public void Learn()
        {
            Console.WriteLine($"Student {Name} is learning");
        }
    }

    // teaching concern implementation
    public class Professor : Person, IProfessor
    {
        public void Teach()
        {
            Console.WriteLine($"Professor {Name} is teaching");
        }
    }


    public interface IStudentAndProfessor : IStudent, IProfessor
    {

    }

    [Collection("Sequential")]
    public class Test : XUnitTestBase
    {

        public Test(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [ImplementationClassName("StudentAndProfessorImplementation")]
        public interface IStudentAndProfessorWrapper
        {
            [Plugin]
            Student TheStudent { get; }

            [SuppressWrapping(nameof(IPerson.Walk))]
            [Plugin]
            Professor TheProfessor { get; }
        }


        [Fact]
        public void RunStudentAndProfessorTest()
        {
            Core.SetSaveOnErrorPath("GeneratedCode");

            // combinding Person, learning and teaching concerns. 
            IStudentAndProfessor studentAndProfessorImplementation =
                Core.CreateImplementedInstance<IStudentAndProfessor, IStudentAndProfessorWrapper>();

            studentAndProfessorImplementation.Name = "Bruce";

            studentAndProfessorImplementation.Walk();
            studentAndProfessorImplementation.Learn();
            studentAndProfessorImplementation.Teach();

            Core.Save("GeneratedCode");
        }
    }
}
