using NP.Concepts.Attributes;
using NP.Roxy;
using NP.Roxy.Attributes;
using System;
using Xunit;
using Xunit.Abstractions;

namespace NP.XUnitAttrRoxyTests.StudentProfessorSharedPersonTest
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
    public class Learner : ILearner
    {
        public string Name { get; set; }

        public void Learn()
        {
            Console.WriteLine($"Student {Name} is learning");
        }
    }

    // teaching concern implementation
    public class Teacher : ITeacher
    {
        public string Name { get; set; }

        public void Teach()
        {
            Console.WriteLine($"Professor {Name} is teaching");
        }
    }

    public interface IStudentAndProfessor : IStudent, IProfessor
    {

    }


    public class Test : XUnitTestBase
    {
        [ImplementationClassName("Student1")]
        public interface IStudentImplementor
        {
            Person ThePerson { get; }

            Learner TheLearner { get; }
        }

        [ImplementationClassName("Professor1")]
        public interface IProfessorImplementor
        {
            Person ThePerson { get; }

            Teacher TheTeacher { get; }
        }

        [ImplementationClassName("StudentAndProfessorSharedImplementor")]
        public interface IStudentAndProfessorSharedImplementor
        {
            [SharedProperty]
            Person ThePerson { get; }

            [Implementor(typeof(IStudentImplementor))]
            IStudent TheStudent { get; }

            [Implementor(typeof(IProfessorImplementor))]
            IProfessor TheProfessor { get; }
        }

        public Test(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }


        [Fact]
        public void RunStudentAndProfessorSharedPersonTest()
        {
            Core.SetSaveOnErrorPath("GeneratedCode");

            // combinding Person, learning and teaching concerns. 
            IStudentAndProfessor studentAndProfessorImplementation =
                Core.CreateImplementedInstance<IStudentAndProfessor, IStudentAndProfessorSharedImplementor>();

            studentAndProfessorImplementation.Name = "Bruce";

            studentAndProfessorImplementation.Walk();
            studentAndProfessorImplementation.Learn();
            studentAndProfessorImplementation.Teach();

            Core.Save("GeneratedCode");
        }
    }
}
